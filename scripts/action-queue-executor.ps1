param(
    [string]$BridgeRoot = 'C:\Users\gue\AppData\LocalLow\Chronocle\He Is Coming\MCPBridge',
    [string]$WindowTitle = 'He is coming',
    [string]$WindowProcessName = 'He is coming',
    [int]$PollMilliseconds = 200
)

$ErrorActionPreference = 'Stop'

$queuePath = Join-Path $BridgeRoot 'action-queue.jsonl'
$logPath = 'C:\Users\GuE\hic-mcp\action-queue-executor.log'
$mutex = [System.Threading.Mutex]::new($false, 'Global\Copilot.HeIsComing.ActionQueueExecutor')
$script:wshShell = $null

if (-not $mutex.WaitOne(0, $false)) {
    exit 0
}

Add-Type @"
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

public static class QueueExecutorNative
{
    public delegate bool EnumWindowsProc(IntPtr handle, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    public const uint INPUT_KEYBOARD = 1;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    public const uint KEYEVENTF_SCANCODE = 0x0008;

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

    public static int SendScanCode(ushort scanCode, int holdMilliseconds)
    {
        var inputs = new INPUT[1];
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].U.ki.wScan = scanCode;
        inputs[0].U.ki.dwFlags = KEYEVENTF_SCANCODE;

        if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) != 1)
        {
            return Marshal.GetLastWin32Error();
        }

        Thread.Sleep(holdMilliseconds);

        inputs[0].U.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
        if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) != 1)
        {
            return Marshal.GetLastWin32Error();
        }

        return 0;
    }
}
"@

function Write-Log {
    param([string]$Message)

    $timestamp = (Get-Date).ToString('s')
    Add-Content -Path $logPath -Value "$timestamp $Message"
}

function Get-WindowTitle {
    param([IntPtr]$Handle)

    if ($Handle -eq [IntPtr]::Zero) {
        return ''
    }

    $builder = New-Object System.Text.StringBuilder 512
    [void][QueueExecutorNative]::GetWindowText($Handle, $builder, $builder.Capacity)
    return $builder.ToString()
}

function Find-WindowHandle {
    param(
        [int]$ExpectedProcessId,
        [string]$ExpectedTitleFragment
    )

    $matchingHandle = [IntPtr]::Zero
    $callback = [QueueExecutorNative+EnumWindowsProc]{
        param([IntPtr]$Handle, [IntPtr]$LParam)

        if (-not [QueueExecutorNative]::IsWindowVisible($Handle)) {
            return $true
        }

        $windowProcessId = 0
        [void][QueueExecutorNative]::GetWindowThreadProcessId($Handle, [ref]$windowProcessId)
        if ($ExpectedProcessId -gt 0 -and $windowProcessId -eq $ExpectedProcessId) {
            $script:matchingHandle = $Handle
            return $false
        }

        $title = Get-WindowTitle -Handle $Handle
        if (-not [string]::IsNullOrWhiteSpace($ExpectedTitleFragment) -and $title.IndexOf($ExpectedTitleFragment, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $script:matchingHandle = $Handle
            return $false
        }

        return $true
    }

    [void][QueueExecutorNative]::EnumWindows($callback, [IntPtr]::Zero)
    return $script:matchingHandle
}

function Send-KeyBinding {
    param(
        $Binding,
        [int]$TargetProcessId
    )

    $errorCode = [QueueExecutorNative]::SendScanCode([uint16]$Binding.ScanCode, [int]$Binding.HoldMilliseconds)
    if ($errorCode -eq 0) {
        return $true
    }

    Write-Log "sendinput-failed code=$errorCode"

    try {
        if ($null -eq $script:wshShell) {
            $script:wshShell = New-Object -ComObject WScript.Shell
        }

        [void]$script:wshShell.AppActivate($TargetProcessId)
        Start-Sleep -Milliseconds 100
        $script:wshShell.SendKeys($Binding.SendKeys)
        Write-Log "sendkeys-fallback keys=$($Binding.SendKeys)"
        return $true
    }
    catch {
        Write-Log "sendkeys-failed $_"
        return $false
    }
}

function Invoke-QueuedAction {
    param([string]$ActionId)

    $keyMap = @{
        confirm = @{ ScanCode = 0x1C; HoldMilliseconds = 80; SendKeys = '{ENTER}' }
        cancel = @{ ScanCode = 0x01; HoldMilliseconds = 80; SendKeys = '{ESC}' }
        move_up = @{ ScanCode = 0x11; HoldMilliseconds = 150; SendKeys = 'w' }
        move_down = @{ ScanCode = 0x1F; HoldMilliseconds = 150; SendKeys = 's' }
        move_left = @{ ScanCode = 0x1E; HoldMilliseconds = 150; SendKeys = 'a' }
        move_right = @{ ScanCode = 0x20; HoldMilliseconds = 150; SendKeys = 'd' }
        attack = @{ ScanCode = 0x39; HoldMilliseconds = 80; SendKeys = ' ' }
        open_map = @{ ScanCode = 0x32; HoldMilliseconds = 80; SendKeys = 'm' }
        close_map = @{ ScanCode = 0x01; HoldMilliseconds = 80; SendKeys = '{ESC}' }
        end_turn = @{ ScanCode = 0x12; HoldMilliseconds = 80; SendKeys = 'e' }
        reroll_shop = @{ ScanCode = 0x13; HoldMilliseconds = 80; SendKeys = 'r' }
        buy_selected = @{ ScanCode = 0x30; HoldMilliseconds = 80; SendKeys = 'b' }
    }

    $binding = $keyMap[$ActionId]
    if (-not $binding) {
        Write-Log "skip unsupported action=$ActionId"
        return
    }

    $targetProcess = Get-Process -Name $WindowProcessName -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -eq $targetProcess) {
        Write-Log "skip process-not-found action=$ActionId process=$WindowProcessName"
        return
    }

    $handle = Find-WindowHandle -ExpectedProcessId $targetProcess.Id -ExpectedTitleFragment $WindowTitle
    if ($handle -ne [IntPtr]::Zero) {
        [void][QueueExecutorNative]::SetForegroundWindow($handle)
        Start-Sleep -Milliseconds 100
    }

    $foregroundHandle = [QueueExecutorNative]::GetForegroundWindow()
    $foregroundProcessId = 0
    if ($foregroundHandle -ne [IntPtr]::Zero) {
        [void][QueueExecutorNative]::GetWindowThreadProcessId($foregroundHandle, [ref]$foregroundProcessId)
    }

    if ($foregroundProcessId -ne $targetProcess.Id) {
        $foregroundTitle = Get-WindowTitle -Handle $foregroundHandle
        Write-Log "skip foreground-mismatch action=$ActionId targetPid=$($targetProcess.Id) foregroundPid=$foregroundProcessId foregroundTitle=$foregroundTitle"
        return
    }

    if (-not (Send-KeyBinding -Binding $binding -TargetProcessId $targetProcess.Id)) {
        Write-Log "skip input-failed action=$ActionId"
        return
    }

    Write-Log "executed action=$ActionId pid=$($targetProcess.Id) foregroundPid=$foregroundProcessId"
}

New-Item -ItemType Directory -Path $BridgeRoot -Force | Out-Null
if (-not (Test-Path $queuePath)) {
    New-Item -ItemType File -Path $queuePath -Force | Out-Null
}

$position = (Get-Item $queuePath).Length
Write-Log "started bridgeRoot=$BridgeRoot windowTitle=$WindowTitle initialPosition=$position"

try {
    while ($true) {
        if (-not (Test-Path $queuePath)) {
            Start-Sleep -Milliseconds $PollMilliseconds
            continue
        }

        $length = (Get-Item $queuePath).Length
        if ($length -lt $position) {
            $position = 0
            Write-Log 'queue truncated; resetting offset'
        }

        $stream = [System.IO.File]::Open($queuePath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
        try {
            [void]$stream.Seek($position, [System.IO.SeekOrigin]::Begin)
            $reader = New-Object System.IO.StreamReader($stream)
            while (-not $reader.EndOfStream) {
                $line = $reader.ReadLine()
                $position = $stream.Position
                if ([string]::IsNullOrWhiteSpace($line)) {
                    continue
                }

                try {
                    $request = $line | ConvertFrom-Json
                    Invoke-QueuedAction -ActionId $request.actionId
                }
                catch {
                    Write-Log "parse-error $_"
                }
            }
        }
        finally {
            $reader.Dispose()
            $stream.Dispose()
        }

        Start-Sleep -Milliseconds $PollMilliseconds
    }
}
finally {
    $mutex.ReleaseMutex()
    $mutex.Dispose()
}

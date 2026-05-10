using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Bridge;

public sealed class WindowsGameInputDispatcher : IActionDispatcher
{
	private delegate bool EnumWindowsProc(nint handle, nint lParam);

	private const uint KeyUpFlag = 2u;

	private static readonly IReadOnlyDictionary<string, byte> ActionKeyMap = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase)
	{
		["confirm"] = 13,
		["cancel"] = 27,
		["move_up"] = 87,
		["move_down"] = 83,
		["move_left"] = 65,
		["move_right"] = 68,
		["attack"] = 32,
		["open_map"] = 77,
		["close_map"] = 27,
		["end_turn"] = 69,
		["reroll_shop"] = 82,
		["buy_selected"] = 66
	};

	private readonly string _windowTitle;

	private readonly bool _isEnabled;

	public WindowsGameInputDispatcher(string windowTitle, bool isEnabled)
	{
		_windowTitle = windowTitle;
		_isEnabled = isEnabled;
	}

	public Task<ActionExecutionResult> ExecuteAsync(ActionExecutionRequest request, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!_isEnabled)
		{
			return Task.FromResult(new ActionExecutionResult(request.ActionId, "queued", queued: true, executed: false, "Input execution is disabled; action remains queued only."));
		}
		if (!ActionKeyMap.TryGetValue(request.ActionId, out var value))
		{
			return Task.FromResult(new ActionExecutionResult(request.ActionId, "unsupported", queued: false, executed: false, "No keyboard mapping is defined for this action."));
		}
		nint num = FindWindowContaining(_windowTitle);
		if (num == IntPtr.Zero)
		{
			return Task.FromResult(new ActionExecutionResult(request.ActionId, "window-not-found", queued: true, executed: false, "Unable to find a visible window containing '" + _windowTitle + "'."));
		}
		SetForegroundWindow(num);
		keybd_event(value, 0, 0u, UIntPtr.Zero);
		keybd_event(value, 0, 2u, UIntPtr.Zero);
		return Task.FromResult(new ActionExecutionResult(request.ActionId, "executed", queued: true, executed: true, "Queued action dispatched to window '" + _windowTitle + "'."));
	}

	private static nint FindWindowContaining(string expectedTitleFragment)
	{
		if (string.IsNullOrWhiteSpace(expectedTitleFragment))
		{
			return IntPtr.Zero;
		}
		nint matchingHandle = IntPtr.Zero;
		EnumWindows(delegate(nint handle, nint _)
		{
			if (!IsWindowVisible(handle))
			{
				return true;
			}
			StringBuilder stringBuilder = new StringBuilder(256);
			_ = GetWindowText(handle, stringBuilder, stringBuilder.Capacity);
			if (stringBuilder.ToString().Contains(expectedTitleFragment, StringComparison.OrdinalIgnoreCase))
			{
				matchingHandle = handle;
				return false;
			}
			return true;
		}, IntPtr.Zero);
		return matchingHandle;
	}

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(nint hWnd);

	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(nint hWnd);

	[DllImport("user32.dll")]
	private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, nuint dwExtraInfo);
}

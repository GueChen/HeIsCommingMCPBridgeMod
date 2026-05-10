param(
    [string]$BridgeRoot = (Join-Path $PSScriptRoot ".bridge"),
    [string]$SaveDirectory = (Join-Path $env:USERPROFILE "AppData\LocalLow\Chronocle\He Is Coming"),
    [string]$GameDirectory = "E:\SteamLibrary\steamapps\common\He is coming",
    [string]$WindowTitle = "He is coming",
    [switch]$ExecuteInput
)

$projectPath = Join-Path $PSScriptRoot "src\MCPBridgeMod.Server\MCPBridgeMod.Server.csproj"

$arguments = @(
    "run",
    "--project", $projectPath,
    "--",
    "--bridge-root", $BridgeRoot,
    "--save-directory", $SaveDirectory,
    "--game-directory", $GameDirectory,
    "--window-title", $WindowTitle
)

if ($ExecuteInput) {
    $arguments += "--execute-input"
}

dotnet @arguments

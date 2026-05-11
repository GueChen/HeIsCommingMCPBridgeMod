# Copilot instructions for MCPBridgeMod

## Build, run, and validation commands

- Build the full solution: `dotnet build .\MCPBridgeMod.slnx`
- Build the plugin only: `dotnet build .\src\MCPBridgeMod.Plugin\MCPBridgeMod.Plugin.csproj`
- Build the server only: `dotnet build .\src\MCPBridgeMod.Server\MCPBridgeMod.Server.csproj`
- Build the shared contracts only: `dotnet build .\src\MCPBridgeMod.Contracts\MCPBridgeMod.Contracts.csproj`
- Build the interop bootstrapper: `dotnet build .\tools\InteropBootstrapper\InteropBootstrapper.csproj`
- Run the MCP stdio server with repo-local defaults: `powershell -File .\start-mcp-server.ps1`
- Run the MCP stdio server against live plugin artifacts: `powershell -File .\start-mcp-server.ps1 -BridgeRoot "$env:USERPROFILE\AppData\LocalLow\Chronocle\He Is Coming\MCPBridge"`
- Run the server directly with custom paths: `dotnet run --project .\src\MCPBridgeMod.Server\MCPBridgeMod.Server.csproj -- --bridge-root <path> --save-directory <path> --game-directory <path> --window-title "He is coming" [--execute-input]`
- Run the interactive queue consumer in the logged-in desktop session: `powershell -File .\scripts\action-queue-executor.ps1 -BridgeRoot 'C:\Users\gue\AppData\LocalLow\Chronocle\He Is Coming\MCPBridge'`

There are currently no committed automated test projects or lint targets in this repo. The narrowest available validation is to build the specific project you changed and then run the affected bridge component against live artifacts or a live game session.

## High-level architecture

- `src\MCPBridgeMod.Contracts` is the shared boundary between the in-game plugin and the MCP server. It defines tool names, action ids, and the JSON schema for `handshake.json`, `snapshot.json`, `catalog.json`, and `action-queue.jsonl`.
- `src\MCPBridgeMod.Plugin` is the BepInEx IL2CPP mod that runs inside the game process. `MCPBridgePlugin` writes bootstrap artifacts, registers `BridgeCaptureBehaviour`, and then relies on `LiveCatalogCapture` to publish structured runtime state to the artifact directory from inside the Unity scene.
- `BridgeCaptureBehaviour` is the plugin’s runtime loop. It pumps queued actions every frame through `BridgeActionQueueProcessor` and performs passive recapture every 5 seconds.
- `LiveCatalogCapture` is the authoritative source for live state. It resolves Unity/IL2CPP managers with `Object.FindObjectOfType`, infers the current screen, builds the agent-facing map neighborhood view plus discovered semantic points of interest, inventory, encounter, and catalog payloads, and writes them through `BridgeArtifactWriter`.
- `BridgeScaffold` and `BridgeActionQueueProcessor` are the action layer on the plugin side. `BridgeScaffold` exposes the action catalog that agents see, while `BridgeActionQueueProcessor` executes the queued actions in-process and triggers a fresh capture after successful execution.
- `src\MCPBridgeMod.Bridge` is the server-side adapter layer. `BridgeCoordinator` fronts the artifacts, enforces action enablement against the current snapshot, writes actions to `action-queue.jsonl` through `FileActionDispatcher`, and optionally tries direct Windows input through `WindowsGameInputDispatcher`.
- `ArtifactBackedSnapshotSource` prefers the plugin-authored `snapshot.json`. `HeIsComingFileSnapshotSource` is the fallback path when the live plugin is unavailable; it infers coarse state from LocalLow save/settings/log files.
- `src\MCPBridgeMod.Server` is the MCP stdio endpoint. `Program.cs` wires the bridge services and `McpStdioServer` handles JSON-RPC `initialize`, `tools/list`, and `tools/call`.
- `scripts\action-queue-executor.ps1` is the companion process for interactive sessions. It tails `action-queue.jsonl`, brings the game window to the foreground, and sends scan codes / `SendKeys` fallback. This matters because the MCP server often runs from a non-interactive context where direct input injection is unreliable.
- `vendor\interop-generated` plus the vendored BepInEx IL2CPP binaries are part of the plugin build contract. The plugin project references those DLLs directly.

## Key conventions

- Treat plugin-authored artifacts as the source of truth for live state. If you need richer game semantics, extend `LiveCatalogCapture` first rather than teaching the server to guess game state.
- Keep screen detection, action exposure, and execution semantics aligned. New state work usually requires coordinated updates in `LiveCatalogCapture.DetermineScreenState`, `BridgeScaffold.CreateActionsForScreen`, and `BridgeActionQueueProcessor`.
- The action catalog is intentionally stateful. Actions can be present but disabled, and `BridgeCoordinator` enforces `IsEnabled` before dispatching. Do not add always-on fallback actions that bypass current screen/state checks.
- Schema changes are lockstep changes. If you add or rename fields in `MCPBridgeMod.Contracts`, update both plugin writers and server readers in the same change, and deploy `MCPBridgeMod.Plugin.dll` and `MCPBridgeMod.Contracts.dll` together.
- `snapshot.map.nodes` / `snapshot.map.edges` are now a filtered local view, not a full overworld dump. If you need broader planning context, prefer `availableMoves` plus `knownPointsOfInterest`, and treat any future raw/full graph exposure as an explicit debug surface rather than the default agent contract.
- The action path is queue-first. Even when direct input execution is enabled, the server still appends to `action-queue.jsonl`. New actions should work correctly through the queue consumer, not only through `WindowsGameInputDispatcher`.
- Be careful with artifact roots. The plugin default artifact root is `%LocalLow%\Chronocle\He Is Coming\MCPBridge`, but `start-mcp-server.ps1` defaults `-BridgeRoot` to `.\.bridge`. Override `-BridgeRoot` when you want the server to read the live plugin output instead of a repo-local staging folder.
- The repo assumes Windows paths and a fixed default install/window target (`E:\SteamLibrary\steamapps\common\He is coming`, window title `He is coming`). Keep scripts and examples Windows-native.
- Passive recapture is slow compared with action execution. If you need immediate post-action state, rely on explicit capture triggered by the action processor instead of assuming the 5-second probe loop is enough.
- Artifact IO is plain JSON file IO (`File.WriteAllText`, `File.OpenRead`, `File.AppendAllTextAsync`). Changes that touch artifact access need to account for concurrent plugin/server/queue-consumer access rather than assuming exclusive file ownership.

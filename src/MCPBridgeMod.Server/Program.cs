using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MCPBridgeMod.Bridge;
using MCPBridgeMod.Server;

[CompilerGenerated]
internal class Program
{
	private static async Task _003CMain_003E_0024(string[] args)
	{
		CancellationTokenSource cancellationSource = new CancellationTokenSource();
		Console.CancelKeyPress += delegate(object? _, ConsoleCancelEventArgs eventArgs)
		{
			eventArgs.Cancel = true;
			cancellationSource.Cancel();
		};
		ServerCommandLineOptions options = ServerCommandLineOptions.Parse(args, Directory.GetCurrentDirectory());
		BridgeServiceOptions bridgeOptions = options.ToBridgeServiceOptions();
		BridgeArtifacts artifacts = new BridgeArtifacts(bridgeOptions.BridgeRoot);
		JsonArtifactStore artifactStore = new JsonArtifactStore(artifacts);
		ArtifactBackedSnapshotSource snapshotSource = new ArtifactBackedSnapshotSource(artifacts, new HeIsComingFileSnapshotSource(bridgeOptions));
		BridgeCoordinator coordinator = new BridgeCoordinator(snapshotSource, artifactStore, new FileActionDispatcher(artifacts), new WindowsGameInputDispatcher(bridgeOptions.WindowTitle, bridgeOptions.EnableInputExecution));
		await coordinator.GetHandshakeAsync(cancellationSource.Token);
		McpStdioServer server = new McpStdioServer(coordinator);
		await server.RunAsync(cancellationSource.Token);
	}
}

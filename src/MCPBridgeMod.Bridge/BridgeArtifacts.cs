using System.IO;

namespace MCPBridgeMod.Bridge;

public sealed class BridgeArtifacts
{
	public string BridgeRoot { get; }

	public string HandshakePath => Path.Combine(BridgeRoot, "handshake.json");

	public string SnapshotPath => Path.Combine(BridgeRoot, "snapshot.json");

	public string CatalogPath => Path.Combine(BridgeRoot, "catalog.json");

	public string ActionQueuePath => Path.Combine(BridgeRoot, "action-queue.jsonl");

	public BridgeArtifacts(string bridgeRoot)
	{
		Directory.CreateDirectory(bridgeRoot);
		BridgeRoot = bridgeRoot;
	}
}

using System.IO;
using System.Text.Json;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Plugin;

public sealed class BridgeArtifactWriter
{
	private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly BridgeRuntimeOptions _runtimeOptions;

	public string HandshakePath => Path.Combine(_runtimeOptions.ArtifactRoot, "handshake.json");

	public string SnapshotPath => Path.Combine(_runtimeOptions.ArtifactRoot, "snapshot.json");

	public string CatalogPath => Path.Combine(_runtimeOptions.ArtifactRoot, "catalog.json");

	public string ActionQueuePath => Path.Combine(_runtimeOptions.ArtifactRoot, "action-queue.jsonl");

	public BridgeArtifactWriter(BridgeRuntimeOptions runtimeOptions)
	{
		_runtimeOptions = runtimeOptions;
		Directory.CreateDirectory(_runtimeOptions.ArtifactRoot);
	}

	public void WriteHandshake(BridgeHandshake handshake)
	{
		string contents = JsonSerializer.Serialize(handshake, SerializerOptions);
		File.WriteAllText(HandshakePath, contents);
	}

	public void WriteSnapshot(GameSnapshot snapshot)
	{
		string contents = JsonSerializer.Serialize(snapshot, SerializerOptions);
		File.WriteAllText(SnapshotPath, contents);
	}

	public void WriteCatalog(GameCatalog catalog)
	{
		string contents = JsonSerializer.Serialize(catalog, SerializerOptions);
		File.WriteAllText(CatalogPath, contents);
	}
}

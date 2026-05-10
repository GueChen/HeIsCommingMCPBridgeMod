using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Bridge;

public sealed class JsonArtifactStore
{
	private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly BridgeArtifacts _artifacts;

	public JsonArtifactStore(BridgeArtifacts artifacts)
	{
		_artifacts = artifacts;
	}

	public async Task WriteHandshakeAsync(BridgeHandshake handshake, CancellationToken cancellationToken)
	{
		await File.WriteAllTextAsync(contents: JsonSerializer.Serialize(handshake, SerializerOptions), path: _artifacts.HandshakePath, cancellationToken: cancellationToken);
	}

	public async Task WriteSnapshotAsync(GameSnapshot snapshot, CancellationToken cancellationToken)
	{
		await File.WriteAllTextAsync(contents: JsonSerializer.Serialize(snapshot, SerializerOptions), path: _artifacts.SnapshotPath, cancellationToken: cancellationToken);
	}

	public async Task WriteCatalogAsync(GameCatalog catalog, CancellationToken cancellationToken)
	{
		await File.WriteAllTextAsync(contents: JsonSerializer.Serialize(catalog, SerializerOptions), path: _artifacts.CatalogPath, cancellationToken: cancellationToken);
	}

	public async Task<GameCatalog?> ReadCatalogAsync(CancellationToken cancellationToken)
	{
		if (!File.Exists(_artifacts.CatalogPath))
		{
			return null;
		}
		GameCatalog result;
		await using (FileStream stream = File.OpenRead(_artifacts.CatalogPath))
		{
			result = await JsonSerializer.DeserializeAsync<GameCatalog>((Stream)stream, SerializerOptions, cancellationToken);
		}
		return result;
	}
}

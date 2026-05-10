using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Bridge;

public sealed class ArtifactBackedSnapshotSource : IGameSnapshotSource
{
	private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	private readonly BridgeArtifacts _artifacts;

	private readonly IGameSnapshotSource _fallback;

	public ArtifactBackedSnapshotSource(BridgeArtifacts artifacts, IGameSnapshotSource fallback)
	{
		_artifacts = artifacts;
		_fallback = fallback;
	}

	public async Task<GameSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
	{
		if (File.Exists(_artifacts.SnapshotPath))
		{
			await using FileStream stream = File.OpenRead(_artifacts.SnapshotPath);
			GameSnapshot snapshot = await JsonSerializer.DeserializeAsync<GameSnapshot>((Stream)stream, SerializerOptions, cancellationToken);
			if (snapshot != null)
			{
				return snapshot;
			}
		}
		return await _fallback.GetSnapshotAsync(cancellationToken);
	}
}

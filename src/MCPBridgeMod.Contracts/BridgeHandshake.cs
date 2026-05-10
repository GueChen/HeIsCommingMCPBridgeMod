using System.Collections.Generic;

namespace MCPBridgeMod.Contracts;

public sealed class BridgeHandshake
{
	public string ModId { get; }

	public string ModVersion { get; }

	public string TargetGame { get; }

	public string Loader { get; }

	public IReadOnlyList<string> Tools { get; }

	public BridgeHandshake(string modId, string modVersion, string targetGame, string loader, IReadOnlyList<string> tools)
	{
		ModId = modId;
		ModVersion = modVersion;
		TargetGame = targetGame;
		Loader = loader;
		Tools = tools;
	}
}

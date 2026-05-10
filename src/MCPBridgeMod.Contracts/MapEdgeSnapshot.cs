namespace MCPBridgeMod.Contracts;

public sealed class MapEdgeSnapshot
{
	public string FromNodeId { get; }

	public string ToNodeId { get; }

	public string Direction { get; }

	public MapEdgeSnapshot(string fromNodeId, string toNodeId, string direction)
	{
		FromNodeId = fromNodeId;
		ToNodeId = toNodeId;
		Direction = direction;
	}
}

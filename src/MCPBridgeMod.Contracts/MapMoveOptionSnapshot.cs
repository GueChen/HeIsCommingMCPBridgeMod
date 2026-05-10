namespace MCPBridgeMod.Contracts;

public sealed class MapMoveOptionSnapshot
{
	public string Direction { get; }

	public string NodeId { get; }

	public int X { get; }

	public int Y { get; }

	public string OccupantCategory { get; }

	public string OccupantId { get; }

	public string OccupantName { get; }

	public MapMoveOptionSnapshot(string direction, string nodeId, int x, int y, string occupantCategory, string occupantId, string occupantName)
	{
		Direction = direction;
		NodeId = nodeId;
		X = x;
		Y = y;
		OccupantCategory = occupantCategory;
		OccupantId = occupantId;
		OccupantName = occupantName;
	}
}

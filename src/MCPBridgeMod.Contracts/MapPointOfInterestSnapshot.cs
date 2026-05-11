namespace MCPBridgeMod.Contracts;

public sealed class MapPointOfInterestSnapshot
{
	public string NodeId { get; }

	public int X { get; }

	public int Y { get; }

	public string OccupantCategory { get; }

	public string OccupantId { get; }

	public string OccupantName { get; }

	public bool IsCurrentNode { get; }

	public bool IsVisible { get; }

	public bool IsExplored { get; }

	public MapPointOfInterestSnapshot(string nodeId, int x, int y, string occupantCategory, string occupantId, string occupantName, bool isCurrentNode, bool isVisible, bool isExplored)
	{
		NodeId = nodeId;
		X = x;
		Y = y;
		OccupantCategory = occupantCategory;
		OccupantId = occupantId;
		OccupantName = occupantName;
		IsCurrentNode = isCurrentNode;
		IsVisible = isVisible;
		IsExplored = isExplored;
	}
}

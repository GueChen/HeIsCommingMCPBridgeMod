namespace MCPBridgeMod.Contracts;

public sealed class MapNodeSnapshot
{
	public string NodeId { get; }

	public int X { get; }

	public int Y { get; }

	public bool CanTraverse { get; }

	public bool IsExplored { get; }

	public bool HasFog { get; }

	public bool HasEnemy { get; }

	public bool HasEvent { get; }

	public string Environment { get; }

	public string OccupantCategory { get; }

	public string OccupantId { get; }

	public string OccupantName { get; }

	public MapNodeSnapshot(string nodeId, int x, int y, bool canTraverse, bool isExplored, bool hasFog, bool hasEnemy, bool hasEvent, string environment, string occupantCategory, string occupantId, string occupantName)
	{
		NodeId = nodeId;
		X = x;
		Y = y;
		CanTraverse = canTraverse;
		IsExplored = isExplored;
		HasFog = hasFog;
		HasEnemy = hasEnemy;
		HasEvent = hasEvent;
		Environment = environment;
		OccupantCategory = occupantCategory;
		OccupantId = occupantId;
		OccupantName = occupantName;
	}
}

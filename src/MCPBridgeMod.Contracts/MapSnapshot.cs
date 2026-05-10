using System.Collections.Generic;

namespace MCPBridgeMod.Contracts;

public sealed class MapSnapshot
{
	public string NodeId { get; }

	public string BiomeName { get; }

	public int DistanceToBoss { get; }

	public string? CurrentNodeId { get; }

	public IReadOnlyList<string> AvailableMoveNodeIds { get; }

	public IReadOnlyList<MapMoveOptionSnapshot> AvailableMoves { get; }

	public IReadOnlyList<MapNodeSnapshot> Nodes { get; }

	public IReadOnlyList<MapEdgeSnapshot> Edges { get; }

	public MapSnapshot(string nodeId, string biomeName, int distanceToBoss, string? currentNodeId, IReadOnlyList<string> availableMoveNodeIds, IReadOnlyList<MapMoveOptionSnapshot> availableMoves, IReadOnlyList<MapNodeSnapshot> nodes, IReadOnlyList<MapEdgeSnapshot> edges)
	{
		NodeId = nodeId;
		BiomeName = biomeName;
		DistanceToBoss = distanceToBoss;
		CurrentNodeId = currentNodeId;
		AvailableMoveNodeIds = availableMoveNodeIds;
		AvailableMoves = availableMoves;
		Nodes = nodes;
		Edges = edges;
	}
}

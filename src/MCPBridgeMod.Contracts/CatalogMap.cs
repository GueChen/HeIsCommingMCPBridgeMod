namespace MCPBridgeMod.Contracts;

public sealed class CatalogMap
{
	public string MapId { get; }

	public string AreaName { get; }

	public int WorldDimensions { get; }

	public int AreaCount { get; }

	public int ExploredCells { get; }

	public int EnemyTileCount { get; }

	public int CurrentBiomeCode { get; }

	public CatalogMap(string mapId, string areaName, int worldDimensions, int areaCount, int exploredCells, int enemyTileCount, int currentBiomeCode)
	{
		MapId = mapId;
		AreaName = areaName;
		WorldDimensions = worldDimensions;
		AreaCount = areaCount;
		ExploredCells = exploredCells;
		EnemyTileCount = enemyTileCount;
		CurrentBiomeCode = currentBiomeCode;
	}
}

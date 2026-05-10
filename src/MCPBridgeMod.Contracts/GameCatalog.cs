using System;
using System.Collections.Generic;

namespace MCPBridgeMod.Contracts;

public sealed class GameCatalog
{
	public DateTimeOffset CapturedAt { get; }

	public CharacterProfile? Character { get; }

	public IReadOnlyList<CatalogItem> Items { get; }

	public IReadOnlyList<CatalogMonster> Monsters { get; }

	public IReadOnlyList<CatalogMap> Maps { get; }

	public SnapshotDiagnostics Diagnostics { get; }

	public GameCatalog(DateTimeOffset capturedAt, CharacterProfile? character, IReadOnlyList<CatalogItem> items, IReadOnlyList<CatalogMonster> monsters, IReadOnlyList<CatalogMap> maps, SnapshotDiagnostics diagnostics)
	{
		CapturedAt = capturedAt;
		Character = character;
		Items = items;
		Monsters = monsters;
		Maps = maps;
		Diagnostics = diagnostics;
	}
}

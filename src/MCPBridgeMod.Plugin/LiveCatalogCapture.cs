using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem.Collections.Generic;
using MCPBridgeMod.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MCPBridgeMod.Plugin;

public sealed class LiveCatalogCapture
{
	private sealed class OccupantInfo
	{
		public static OccupantInfo None { get; } = new OccupantInfo("none", "none", "none");

		public string Category { get; }

		public string Id { get; }

		public string Name { get; }

		public OccupantInfo(string category, string id, string name)
		{
			Category = category;
			Id = id;
			Name = name;
		}
	}

	private sealed class ScreenState
	{
		public string Screen { get; }

		public bool IsWorldSelection { get; }

		public bool IsDifficultySelection { get; }

		public bool IsWaitingRoom { get; }

		public bool IsContinueWorld { get; }

		public ScreenState(string screen, bool isWorldSelection, bool isDifficultySelection, bool isWaitingRoom, bool isContinueWorld)
		{
			Screen = screen;
			IsWorldSelection = isWorldSelection;
			IsDifficultySelection = isDifficultySelection;
			IsWaitingRoom = isWaitingRoom;
			IsContinueWorld = isContinueWorld;
		}
	}

	private readonly BridgeRuntimeOptions _runtimeOptions;

	private readonly BridgeScaffold _scaffold;

	private readonly BridgeArtifactWriter _artifactWriter;

	private readonly ManualLogSource _log;

	public LiveCatalogCapture(BridgeRuntimeOptions runtimeOptions, BridgeScaffold scaffold, BridgeArtifactWriter artifactWriter, ManualLogSource log)
	{
		_runtimeOptions = runtimeOptions;
		_scaffold = scaffold;
		_artifactWriter = artifactWriter;
		_log = log;
	}

	public void Capture(string reason)
	{
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Expected O, but got Unknown
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Expected O, but got Unknown
		bool flag = default(bool);
		try
		{
			Scene activeScene = SceneManager.GetActiveScene();
			OverworldUIManager val = Object.FindObjectOfType<OverworldUIManager>();
			ItemManager val2 = ((val != null) ? val.itemManager : null) ?? Object.FindObjectOfType<ItemManager>();
			StatsManager val3 = Object.FindObjectOfType<StatsManager>();
			MapManager val4 = Object.FindObjectOfType<MapManager>();
			PlayerControlsManager controlsManager = Object.FindObjectOfType<PlayerControlsManager>();
			WorldsMenu val5 = Object.FindObjectOfType<WorldsMenu>();
			DifficultyToggle difficultyToggle = Object.FindObjectOfType<DifficultyToggle>();
			WaitingRoomDisplayer val6 = Object.FindObjectOfType<WaitingRoomDisplayer>();
			ShowContinueWorld val7 = Object.FindObjectOfType<ShowContinueWorld>();
			PlayerController val8 = ((val3 != null) ? val3.GetPlayerController() : null) ?? Object.FindObjectOfType<PlayerController>();
			ScreenState screenState = DetermineScreenState(controlsManager, val5, difficultyToggle, val6, val7, val3, val4);
			IReadOnlyList<CatalogItem> readOnlyList = BuildItems(val2);
			IReadOnlyList<CatalogMonster> readOnlyList2 = BuildMonsters(val3);
			IReadOnlyList<CatalogMap> readOnlyList3 = BuildMaps(val4);
			CharacterProfile characterProfile = BuildCharacter(val, val3);
			MapSnapshot mapSnapshot = BuildMapSnapshot(readOnlyList3, val4, val8, screenState);
			Dictionary<string, string> obj = new Dictionary<string, string>
			{
				["reason"] = reason,
				["scene"] = ((Scene)(ref activeScene)).name
			};
			flag = val2 != null;
			obj["itemManager"] = flag.ToString();
			flag = val3 != null;
			obj["statsManager"] = flag.ToString();
			flag = val4 != null;
			obj["mapManager"] = flag.ToString();
			flag = val != null;
			obj["overworldUiManager"] = flag.ToString();
			flag = controlsManager != null;
			obj["controlsManager"] = flag.ToString();
			flag = val5 != null;
			obj["worldsMenu"] = flag.ToString();
			flag = difficultyToggle != null;
			obj["difficultyToggle"] = flag.ToString();
			flag = val6 != null;
			obj["waitingRoom"] = flag.ToString();
			flag = val7 != null;
			obj["showContinueWorld"] = flag.ToString();
			obj["currentStoryWorld"] = SafeCall<string>(delegate
			{
				//IL_001f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				PlayerControlsManager obj2 = controlsManager;
				int num;
				if (obj2 != null)
				{
					num = (int)obj2.currentlyPlayingArea;
				}
				else
				{
					DifficultyToggle obj3 = difficultyToggle;
					num = ((obj3 == null) ? 99 : ((int)obj3.storyWorld));
				}
				return ((object)(StoryWorld)num/*cast due to constrained. prefix*/).ToString();
			}, "unknown");
			obj["gameDifficulty"] = SafeCall<string>(delegate
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				PlayerControlsManager obj2 = controlsManager;
				return ((obj2 != null) ? ((object)obj2.gameDifficulty/*cast due to constrained. prefix*/).ToString() : null) ?? "unknown";
			}, "unknown");
			obj["gameMode"] = SafeCall<string>(delegate
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				PlayerControlsManager obj2 = controlsManager;
				return ((obj2 != null) ? ((object)obj2.gameMode/*cast due to constrained. prefix*/).ToString() : null) ?? "unknown";
			}, "unknown");
			flag = screenState.IsWorldSelection;
			obj["menuWorldSelection"] = flag.ToString();
			flag = screenState.IsDifficultySelection;
			obj["menuDifficultySelection"] = flag.ToString();
			flag = screenState.IsWaitingRoom;
			obj["menuWaitingRoom"] = flag.ToString();
			flag = screenState.IsContinueWorld;
			obj["menuContinueWorld"] = flag.ToString();
			obj["mapGraphNodes"] = (mapSnapshot?.Nodes.Count ?? 0).ToString(CultureInfo.InvariantCulture);
			obj["mapGraphEdges"] = (mapSnapshot?.Edges.Count ?? 0).ToString(CultureInfo.InvariantCulture);
			obj["mapCurrentNodeId"] = mapSnapshot?.CurrentNodeId;
			obj["mapAvailableMoveCount"] = (mapSnapshot?.AvailableMoveNodeIds.Count ?? 0).ToString(CultureInfo.InvariantCulture);
			flag = val8 != null;
			obj["playerController"] = flag.ToString();
			flag = _runtimeOptions.VerboseLogging;
			obj["verboseLogging"] = flag.ToString();
			SnapshotDiagnostics diagnostics = new SnapshotDiagnostics("live-plugin-capture", "Structured runtime metadata captured from the active IL2CPP scene.", obj);
			GameCatalog catalog = new GameCatalog(DateTimeOffset.UtcNow, characterProfile, readOnlyList, readOnlyList2, readOnlyList3, diagnostics);
			_artifactWriter.WriteCatalog(catalog);
			_artifactWriter.WriteSnapshot(BuildSnapshot(catalog, val, val3, mapSnapshot, screenState));
			ManualLogSource log = _log;
			BepInExInfoLogInterpolatedStringHandler val9 = new BepInExInfoLogInterpolatedStringHandler(72, 6, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val9).AppendLiteral("Captured catalog from scene '");
				((BepInExLogInterpolatedStringHandler)val9).AppendFormatted<string>(((Scene)(ref activeScene)).name);
				((BepInExLogInterpolatedStringHandler)val9).AppendLiteral("' (");
				((BepInExLogInterpolatedStringHandler)val9).AppendFormatted<string>(reason);
				((BepInExLogInterpolatedStringHandler)val9).AppendLiteral("): items=");
				((BepInExLogInterpolatedStringHandler)val9).AppendFormatted<int>(readOnlyList.Count);
				((BepInExLogInterpolatedStringHandler)val9).AppendLiteral(", monsters=");
				((BepInExLogInterpolatedStringHandler)val9).AppendFormatted<int>(readOnlyList2.Count);
				((BepInExLogInterpolatedStringHandler)val9).AppendLiteral(", maps=");
				((BepInExLogInterpolatedStringHandler)val9).AppendFormatted<int>(readOnlyList3.Count);
				((BepInExLogInterpolatedStringHandler)val9).AppendLiteral(", character=");
				((BepInExLogInterpolatedStringHandler)val9).AppendFormatted<string>((characterProfile == null) ? "none" : "present");
				((BepInExLogInterpolatedStringHandler)val9).AppendLiteral(".");
			}
			log.LogInfo(val9);
		}
		catch (Exception ex)
		{
			ManualLogSource log2 = _log;
			BepInExErrorLogInterpolatedStringHandler val10 = new BepInExErrorLogInterpolatedStringHandler(35, 2, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val10).AppendLiteral("Failed to capture live catalog (");
				((BepInExLogInterpolatedStringHandler)val10).AppendFormatted<string>(reason);
				((BepInExLogInterpolatedStringHandler)val10).AppendLiteral("): ");
				((BepInExLogInterpolatedStringHandler)val10).AppendFormatted<Exception>(ex);
			}
			log2.LogError(val10);
		}
	}

	private IReadOnlyList<CatalogItem> BuildItems(ItemManager itemManager)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		if (((itemManager != null) ? itemManager.itemPrefabs : null) == null)
		{
			return Array.Empty<CatalogItem>();
		}
		List<CatalogItem> list = new List<CatalogItem>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Enumerator<InventoryItem> enumerator = itemManager.itemPrefabs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			InventoryItem current = enumerator.Current;
			if (current != null)
			{
				string text = NormalizeId(((EffectBase)current).nameTag, ((Object)current).name);
				if (hashSet.Add(text))
				{
					list.Add(new CatalogItem(text, FirstValue(((EffectBase)current).effectName, ((EffectBase)current).nameTag, ((Object)current).name), FirstValue(((EffectBase)current).effectDesc, ((EffectBase)current).nameTag), ((object)current.itemType/*cast due to constrained. prefix*/).ToString(), ((object)current.itemRarity/*cast due to constrained. prefix*/).ToString(), ((EffectBase)current).attack, ((EffectBase)current).armor, ((EffectBase)current).speed, ((EffectBase)current).maxHealth, current.spawnWeight, CollectTags(current.itemTags)));
				}
			}
		}
		return list.OrderBy<CatalogItem, string>((CatalogItem item) => item.DisplayName, StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private IReadOnlyList<CatalogMonster> BuildMonsters(StatsManager statsManager)
	{
		List<CatalogMonster> list = new List<CatalogMonster>();
		HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (EnemyTile item in Object.FindObjectsOfType<EnemyTile>())
		{
			if (item != null)
			{
				EnemyStats enemyStats = item.GetEnemyStats();
				if (enemyStats != null)
				{
					AddMonster(list, seen, new CatalogMonster(NormalizeId(enemyStats.nameTag, enemyStats.enemyName, ((Tile)item).nameTag, ((Object)item).name), FirstValue(enemyStats.enemyName, ((Tile)item).nameTag, ((Object)item).name), FirstValue(enemyStats.enemyDesc, item.GetAdditionalEnemyItem()), enemyStats.level, enemyStats.health, enemyStats.maxHealth, enemyStats.attack, enemyStats.armor, enemyStats.speed, 0, 0, NullIfBlank(item.GetAdditionalEnemyItem()), "active-enemy-tile"));
				}
			}
		}
		if (statsManager != null)
		{
			Enumerator<EnemyBase> enumerator2 = statsManager.GetAllBosses().GetEnumerator();
			while (enumerator2.MoveNext())
			{
				EnemyBase current2 = enumerator2.Current;
				if (current2 != null)
				{
					AddMonster(list, seen, new CatalogMonster(NormalizeId(((EffectBase)current2).nameTag, ((EffectBase)current2).effectName, ((Object)current2).name), FirstValue(((EffectBase)current2).effectName, ((EffectBase)current2).nameTag, ((Object)current2).name), FirstValue(((EffectBase)current2).effectDesc, current2.additionalItemNameTag), current2.level, current2.health, ((EffectBase)current2).maxHealth, ((EffectBase)current2).attack, ((EffectBase)current2).armor, ((EffectBase)current2).speed, current2.goldAmount, current2.boneAmount, NullIfBlank(current2.additionalItemNameTag), "boss-pool"));
				}
			}
		}
		return list.OrderBy<CatalogMonster, string>((CatalogMonster monster) => monster.DisplayName, StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private static void AddMonster(ICollection<CatalogMonster> results, ISet<string> seen, CatalogMonster monster)
	{
		if (seen.Add(monster.MonsterId))
		{
			results.Add(monster);
		}
	}

	private IReadOnlyList<CatalogMap> BuildMaps(MapManager mapManager)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (mapManager == null)
		{
			return Array.Empty<CatalogMap>();
		}
		MapArea val = ((mapManager.mapAreas != null && mapManager.mapAreas.Count > 0) ? mapManager.mapAreas[0] : null);
		string text = "biome-" + mapManager.monoWorldEnvironType.ToString(CultureInfo.InvariantCulture);
		int worldDimensions = ((val != null) ? val.dimensions : 0);
		int areaCount = mapManager.mapAreas?.Count ?? 0;
		int exploredCells = mapManager.playerExploredCoords?.Count ?? 0;
		int length = Object.FindObjectsOfType<EnemyTile>().Length;
		CatalogMap[] array = new CatalogMap[1];
		string[] obj = new string[2] { text, null };
		Scene activeScene = SceneManager.GetActiveScene();
		obj[1] = ((Scene)(ref activeScene)).name;
		array[0] = new CatalogMap(NormalizeId(obj), text, worldDimensions, areaCount, exploredCells, length, mapManager.monoWorldEnvironType);
		return array;
	}

	private CharacterProfile BuildCharacter(OverworldUIManager overworldUiManager, StatsManager statsManager)
	{
		if (overworldUiManager == null || statsManager == null)
		{
			return null;
		}
		EnemyBase currentBoss = statsManager.GetCurrentBoss();
		return new CharacterProfile("Player", SafeCall(() => statsManager.GetPlayerHealth(), ParseStatText(overworldUiManager.healthNumberText)), ParseStatText(overworldUiManager.attackNumberText), ParseStatText(overworldUiManager.armorNumberText), ParseStatText(overworldUiManager.speedNumberText), SafeCall(() => statsManager.GetGold(), ParseStatText(overworldUiManager.goldNumberText)), SafeCall<string>(() => ((object)statsManager.GetPlayerPosition()/*cast due to constrained. prefix*/).ToString(), "unknown"), SafeCall(() => overworldUiManager.GetNrOfInventoryItems(), 0), SafeCall(() => overworldUiManager.GetNrOfBackpackItems(), 0), SafeCall(() => overworldUiManager.GetNrOfOpenInventorySlots(), 0), (currentBoss == null) ? null : FirstValue(((EffectBase)currentBoss).effectName, ((EffectBase)currentBoss).nameTag, ((Object)currentBoss).name), SafeCall(() => statsManager.GetCurrentBossNumber(), 0));
	}

	private GameSnapshot BuildSnapshot(GameCatalog catalog, OverworldUIManager overworldUiManager, StatsManager statsManager, MapSnapshot mapSnapshot, ScreenState screenState)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		CharacterProfile character = catalog.Character;
		IReadOnlyList<InventoryItemSnapshot> inventory = BuildInventory(overworldUiManager);
		EncounterSnapshot encounter = ((!string.Equals(screenState.Screen, "live-battle", StringComparison.OrdinalIgnoreCase)) ? null : BuildEncounter(statsManager));
		string screen = screenState.Screen;
		Scene activeScene = SceneManager.GetActiveScene();
		return new GameSnapshot(screen, ((Scene)(ref activeScene)).name, catalog.CapturedAt, new PlayerSnapshot(character?.Health ?? 0, character?.Health ?? 0, character?.Armor ?? 0, character?.Gold ?? 0, character?.CurrentBossNumber ?? 0), inventory, _scaffold.CreateActionsForScreen(screenState.Screen), encounter, mapSnapshot, catalog.Diagnostics);
	}

	private static MapSnapshot BuildMapSnapshot(IReadOnlyList<CatalogMap> maps, MapManager mapManager, PlayerController playerController, ScreenState screenState)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		if (screenState.Screen.StartsWith("menu-", StringComparison.OrdinalIgnoreCase) || mapManager == null || maps.Count == 0)
		{
			return null;
		}
		Vector2Int currentGridPosition = (Vector2Int)((playerController == null) ? new Vector2Int(0, 0) : SafeCall((Func<Vector2Int>)delegate
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			Vector2 playerGridPosition = playerController.GetPlayerGridPosition();
			return new Vector2Int(Mathf.RoundToInt(playerGridPosition.x), Mathf.RoundToInt(playerGridPosition.y));
		}, new Vector2Int(0, 0)));
		MapArea val = SafeCall(() => mapManager.GetAreaContaining(currentGridPosition), null) ?? ((mapManager.mapAreas != null && mapManager.mapAreas.Count > 0) ? mapManager.mapAreas[0] : null);
		if (((val != null) ? val.grid : null) == null)
		{
			return new MapSnapshot(maps[0].MapId, maps[0].AreaName, maps[0].EnemyTileCount, null, Array.Empty<string>(), Array.Empty<MapMoveOptionSnapshot>(), Array.Empty<MapNodeSnapshot>(), Array.Empty<MapEdgeSnapshot>());
		}
		Dictionary<Vector2Int, MapNodeSnapshot> nodes = new Dictionary<Vector2Int, MapNodeSnapshot>();
		Enumerator<List<CellData>> enumerator = val.grid.GetEnumerator();
		while (enumerator.MoveNext())
		{
			List<CellData> current = enumerator.Current;
			if (current == null)
			{
				continue;
			}
			Enumerator<CellData> enumerator2 = current.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				CellData cell = enumerator2.Current;
				if (cell != null)
				{
					Vector2Int coordinate = new Vector2Int(cell.xCoord, cell.yCoord);
					OccupantInfo occupantInfo = DescribeOccupant(mapManager, coordinate);
					nodes[coordinate] = new MapNodeSnapshot(BuildMapNodeId(coordinate), ((Vector2Int)(ref coordinate)).x, ((Vector2Int)(ref coordinate)).y, SafeCall(() => cell.CanTraverse(), fallback: false), SafeCall(() => mapManager.playerExploredCoords.Contains(coordinate), fallback: false), SafeCall(() => cell.hasFog, fallback: false), SafeCall(() => mapManager.enemyTiles.ContainsKey(coordinate), fallback: false), SafeCall(() => mapManager.eventTiles.ContainsKey(coordinate), fallback: false), SafeCall<string>(() => ((object)(CellEnvironmentType)cell.cellEnvironmentType/*cast due to constrained. prefix*/).ToString(), cell.cellEnvironmentType.ToString(CultureInfo.InvariantCulture)), occupantInfo.Category, occupantInfo.Id, occupantInfo.Name);
				}
			}
		}
		List<MapEdgeSnapshot> list = new List<MapEdgeSnapshot>();
		KeyValuePair<Vector2Int, string>[] array = new KeyValuePair<Vector2Int, string>[4]
		{
			new KeyValuePair<Vector2Int, string>(new Vector2Int(0, 1), "up"),
			new KeyValuePair<Vector2Int, string>(new Vector2Int(1, 0), "right"),
			new KeyValuePair<Vector2Int, string>(new Vector2Int(0, -1), "down"),
			new KeyValuePair<Vector2Int, string>(new Vector2Int(-1, 0), "left")
		};
		foreach (KeyValuePair<Vector2Int, MapNodeSnapshot> item in nodes)
		{
			KeyValuePair<Vector2Int, string>[] array2 = array;
			for (int num = 0; num < array2.Length; num++)
			{
				KeyValuePair<Vector2Int, string> keyValuePair = array2[num];
				Vector2Int val2 = item.Key + keyValuePair.Key;
				if (nodes.TryGetValue(val2, out var value) && CanMoveBetween(playerController, item.Key, val2, item.Value, value))
				{
					list.Add(new MapEdgeSnapshot(item.Value.NodeId, value.NodeId, keyValuePair.Value));
				}
			}
		}
		MapNodeSnapshot value2;
		string currentNodeId = (nodes.TryGetValue(currentGridPosition, out value2) ? value2.NodeId : null);
		string[] availableMoveNodeIds = (from edge in list
			where string.Equals(edge.FromNodeId, currentNodeId, StringComparison.Ordinal)
			select edge.ToNodeId).Distinct<string>(StringComparer.Ordinal).OrderBy<string, string>((string nodeId) => nodeId, StringComparer.Ordinal).ToArray();
		MapMoveOptionSnapshot[] availableMoves = list.Where((MapEdgeSnapshot edge) => string.Equals(edge.FromNodeId, currentNodeId, StringComparison.Ordinal)).Select(delegate(MapEdgeSnapshot edge)
		{
			MapNodeSnapshot mapNodeSnapshot = nodes.Values.First((MapNodeSnapshot node) => string.Equals(node.NodeId, edge.ToNodeId, StringComparison.Ordinal));
			return new MapMoveOptionSnapshot(edge.Direction, mapNodeSnapshot.NodeId, mapNodeSnapshot.X, mapNodeSnapshot.Y, mapNodeSnapshot.OccupantCategory, mapNodeSnapshot.OccupantId, mapNodeSnapshot.OccupantName);
		}).OrderBy<MapMoveOptionSnapshot, string>((MapMoveOptionSnapshot move) => move.Direction, StringComparer.Ordinal)
			.ToArray();
		return new MapSnapshot(maps[0].MapId, maps[0].AreaName, maps[0].EnemyTileCount, currentNodeId, availableMoveNodeIds, availableMoves, (from node in nodes.Values
			orderby node.X, node.Y
			select node).ToArray(), list);
	}

	private static OccupantInfo DescribeOccupant(MapManager mapManager, Vector2Int coordinate)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		Tile enemyTileBase = default(Tile);
		if (((mapManager != null) ? mapManager.enemyTiles : null) != null && mapManager.enemyTiles.TryGetValue(coordinate, ref enemyTileBase))
		{
			EnemyTile enemyTile = SafeCall(() => ((Il2CppObjectBase)enemyTileBase).TryCast<EnemyTile>(), null);
			if (enemyTile != null)
			{
				EnemyStats val = SafeCall(() => enemyTile.GetEnemyStats(), null);
				return new OccupantInfo("monster", NormalizeId((val != null) ? val.nameTag : null, (val != null) ? val.enemyName : null, ((Tile)enemyTile).nameTag, ((Object)enemyTile).name), FirstValue((val != null) ? val.enemyName : null, ((Tile)enemyTile).nameTag, ((Object)enemyTile).name));
			}
		}
		Tile eventTileBase = default(Tile);
		if (((mapManager != null) ? mapManager.eventTiles : null) != null && mapManager.eventTiles.TryGetValue(coordinate, ref eventTileBase))
		{
			EventTile val2 = SafeCall(() => ((Il2CppObjectBase)eventTileBase).TryCast<EventTile>(), null);
			if (val2 == null)
			{
				return new OccupantInfo("event", NormalizeId(eventTileBase.nameTag, ((Object)eventTileBase).name), FirstValue(eventTileBase.nameTag, ((Object)eventTileBase).name));
			}
			string text = ResolveEventTypeName((Tile)(object)val2);
			return new OccupantInfo(NormalizeOccupantCategory(text, (Tile)(object)val2), NormalizeId(text, ((Tile)val2).nameTag, ((Object)val2).name), FirstValue(((Tile)val2).nameTag, text, ((Object)val2).name));
		}
		return OccupantInfo.None;
	}

	private static string ResolveEventTypeName(Tile eventTile)
	{
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<ChestTile>(), null) != null)
		{
			return "ChestTile";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<BlacksmithTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<GoldVendorTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<WanderingSalesmanTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<TagMerchantTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<JunksmithTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<RuneCarverTile>(), null) != null)
		{
			return "ShopTile";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<FortuneTellerTile>(), null) != null)
		{
			return "FortuneTellerTile";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<WaypointTile>(), null) != null)
		{
			return "WaypointTile";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<CampfireTile>(), null) != null)
		{
			return "CampfireTile";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<FerrypointTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<NightGateTile>(), null) != null)
		{
			return "TravelTile";
		}
		return SafeCall<string>(() => ((object)eventTile).GetType().Name, "EventTile");
	}

	private static string NormalizeOccupantCategory(string typeName, Tile eventTile)
	{
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<ChestTile>(), null) != null)
		{
			return "chest";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<BlacksmithTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<GoldVendorTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<WanderingSalesmanTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<TagMerchantTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<JunksmithTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<RuneCarverTile>(), null) != null)
		{
			return "shop";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<FortuneTellerTile>(), null) != null)
		{
			return "fortune_teller";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<WaypointTile>(), null) != null)
		{
			return "waypoint";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<CampfireTile>(), null) != null)
		{
			return "campfire";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<FerrypointTile>(), null) != null || SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<NightGateTile>(), null) != null)
		{
			return "travel";
		}
		if (!string.IsNullOrWhiteSpace(typeName))
		{
			string text = typeName.Replace("Tile", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(" ", string.Empty).ToLowerInvariant();
			if (text.Contains("smith", StringComparison.Ordinal) || text.Contains("vendor", StringComparison.Ordinal) || text.Contains("merchant", StringComparison.Ordinal) || text.Contains("salesman", StringComparison.Ordinal) || text.Contains("carver", StringComparison.Ordinal))
			{
				return "shop";
			}
			if (text.Contains("chest", StringComparison.Ordinal))
			{
				return "chest";
			}
			if (text.Contains("waypoint", StringComparison.Ordinal))
			{
				return "waypoint";
			}
			if (text.Contains("fortune", StringComparison.Ordinal))
			{
				return "fortune_teller";
			}
			if (text.Contains("campfire", StringComparison.Ordinal))
			{
				return "campfire";
			}
			if (text.Contains("ferry", StringComparison.Ordinal) || text.Contains("gate", StringComparison.Ordinal))
			{
				return "travel";
			}
		}
		return "event";
	}

	private static bool CanMoveBetween(PlayerController playerController, Vector2Int from, Vector2Int to, MapNodeSnapshot fromNode, MapNodeSnapshot toNode)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!fromNode.CanTraverse || !toNode.CanTraverse)
		{
			return false;
		}
		bool flag = Mathf.Abs(((Vector2Int)(ref from)).x - ((Vector2Int)(ref to)).x) + Mathf.Abs(((Vector2Int)(ref from)).y - ((Vector2Int)(ref to)).y) == 1;
		if (playerController == null)
		{
			return flag;
		}
		return SafeCall(() => playerController.IsMoveAllowed(new Vector2((float)((Vector2Int)(ref from)).x, (float)((Vector2Int)(ref from)).y), new Vector2((float)((Vector2Int)(ref to)).x, (float)((Vector2Int)(ref to)).y)), flag);
	}

	private static string BuildMapNodeId(Vector2Int coordinate)
	{
		return ((Vector2Int)(ref coordinate)).x.ToString(CultureInfo.InvariantCulture) + "," + ((Vector2Int)(ref coordinate)).y.ToString(CultureInfo.InvariantCulture);
	}

	private static ScreenState DetermineScreenState(PlayerControlsManager controlsManager, WorldsMenu worldsMenu, DifficultyToggle difficultyToggle, WaitingRoomDisplayer waitingRoom, ShowContinueWorld showContinueWorld, StatsManager statsManager, MapManager mapManager)
	{
		bool flag = IsActive((waitingRoom != null) ? ((Component)waitingRoom).gameObject : null) || IsActive((controlsManager != null) ? controlsManager.waitingRoomMenu : null);
		bool flag2 = IsActive((difficultyToggle != null) ? difficultyToggle.difficultySelectionHolder : null) || IsActive((difficultyToggle != null) ? ((Component)difficultyToggle).gameObject : null);
		bool flag3 = IsActive((worldsMenu != null) ? worldsMenu.worldsParent : null) || IsActive((controlsManager != null) ? controlsManager.worldsParent : null) || IsActive((worldsMenu != null) ? ((Component)worldsMenu).gameObject : null);
		bool flag4 = IsActive((showContinueWorld != null) ? showContinueWorld.worldHolder : null) || IsActive((showContinueWorld != null) ? ((Component)showContinueWorld).gameObject : null);
		bool flag5 = IsActive((controlsManager != null) ? controlsManager.mainMenuParent : null);
		object gameObject;
		if (statsManager == null)
		{
			gameObject = null;
		}
		else
		{
			BattleManager battleManager = statsManager.battleManager;
			gameObject = ((battleManager != null) ? battleManager.battleCanvas : null);
		}
		bool flag6 = IsActive((GameObject)gameObject);
		string screen = "live-plugin";
		if (flag)
		{
			screen = "menu-waiting-room";
		}
		else if (flag2)
		{
			screen = "menu-difficulty-selection";
		}
		else if (flag3)
		{
			screen = "menu-world-selection";
		}
		else if (flag4)
		{
			screen = "menu-continue-world";
		}
		else if (flag5)
		{
			screen = "menu-start";
		}
		else if (flag6)
		{
			screen = "live-battle";
		}
		else if (mapManager != null)
		{
			screen = "live-overworld";
		}
		return new ScreenState(screen, flag3, flag2, flag, flag4);
	}

	private static bool IsActive(GameObject gameObject)
	{
		return gameObject != null && gameObject.activeInHierarchy;
	}

	private static IReadOnlyList<InventoryItemSnapshot> BuildInventory(OverworldUIManager overworldUiManager)
	{
		if (overworldUiManager == null)
		{
			return Array.Empty<InventoryItemSnapshot>();
		}
		List<InventoryItemSnapshot> list = new List<InventoryItemSnapshot>();
		int num = SafeCall(() => overworldUiManager.GetNrOfInventorySlots(), 0);
		for (int num2 = 0; num2 < num; num2++)
		{
			InventoryItem val = null;
			try
			{
				val = overworldUiManager.GetItemInSlot(num2);
			}
			catch
			{
			}
			if (val != null)
			{
				list.Add(new InventoryItemSnapshot(NormalizeId(((EffectBase)val).nameTag, ((Object)val).name), FirstValue(((EffectBase)val).effectName, ((EffectBase)val).nameTag, ((Object)val).name), 1));
			}
		}
		return list;
	}

	private static EncounterSnapshot BuildEncounter(StatsManager statsManager)
	{
		if (statsManager == null)
		{
			return null;
		}
		EnemyBase currentBoss = statsManager.GetCurrentBoss();
		if (currentBoss == null)
		{
			return null;
		}
		return new EncounterSnapshot(NormalizeId(((EffectBase)currentBoss).nameTag, ((Object)currentBoss).name), FirstValue(((EffectBase)currentBoss).effectName, ((EffectBase)currentBoss).nameTag, ((Object)currentBoss).name), statsManager.GetCurrentBossNumber());
	}

	private static IReadOnlyList<string> CollectTags(List<ItemTag> tags)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		Enumerator<ItemTag> enumerator = tags.GetEnumerator();
		while (enumerator.MoveNext())
		{
			list.Add(((object)enumerator.Current/*cast due to constrained. prefix*/).ToString());
		}
		return list;
	}

	private static string FirstValue(params string[] values)
	{
		foreach (string text in values)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}
		return "unknown";
	}

	private static string NormalizeId(params string[] values)
	{
		string text = FirstValue(values).Trim().Replace(' ', '_').Replace(':', '_')
			.Replace('/', '_')
			.Replace('\\', '_');
		return text.ToLowerInvariant();
	}

	private static string NullIfBlank(string value)
	{
		return string.IsNullOrWhiteSpace(value) ? null : value;
	}

	private static int ParseStatText(TextMeshProUGUI label)
	{
		if (label == null)
		{
			return 0;
		}
		string s = new string((((TMP_Text)label).text ?? string.Empty).Where(char.IsDigit).ToArray());
		int result;
		return int.TryParse(s, out result) ? result : 0;
	}

	private static T SafeCall<T>(Func<T> callback, T fallback)
	{
		try
		{
			return callback();
		}
		catch
		{
			return fallback;
		}
	}
}

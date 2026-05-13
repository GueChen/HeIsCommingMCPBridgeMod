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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SystemCollections = System.Collections.Generic;
using Object = UnityEngine.Object;

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
			EventPopup val9 = Object.FindObjectOfType<EventPopup>();
			EventTile currentEventTile = GetCurrentEventTile(val4, val8);
			EventChooseEntry[] choiceEntries = GetChoiceEntries(val9, currentEventTile);
			GameObject[] activeInventoryChoiceObjects = GetActiveInventoryChoiceObjects();
			ScreenState screenState = DetermineScreenState(controlsManager, val5, difficultyToggle, val6, val7, val3, val4, val8, val9);
			SystemCollections.IReadOnlyList<CatalogItem> readOnlyList = BuildItems(val2);
			SystemCollections.IReadOnlyList<CatalogMonster> readOnlyList2 = BuildMonsters(val3);
			SystemCollections.IReadOnlyList<CatalogMap> readOnlyList3 = BuildMaps(val4);
			CharacterProfile characterProfile = BuildCharacter(val, val3);
			MapSnapshot mapSnapshot = BuildMapSnapshot(readOnlyList3, val4, val8, screenState);
			SystemCollections.Dictionary<string, string> obj = new SystemCollections.Dictionary<string, string>
			{
				["reason"] = reason,
				["scene"] = activeScene.name
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
			flag = IsActive((val9 != null) ? ((Component)val9).gameObject : null);
			obj["eventPopup"] = flag.ToString();
			flag = SafeCall(() => controlsManager != null && controlsManager.isViewingEventPopup, false);
			obj["controlsViewingEventPopup"] = flag.ToString();
			obj["currentEventTileType"] = currentEventTile?.GetType().Name ?? "none";
			obj["currentEventTileName"] = SafeCall(() => (currentEventTile != null) ? ((Object)currentEventTile).name : null, null) ?? "none";
			obj["eventChoiceCount"] = choiceEntries.Length.ToString(CultureInfo.InvariantCulture);
			obj["inventoryChoiceCount"] = activeInventoryChoiceObjects.Length.ToString(CultureInfo.InvariantCulture);
			obj["currentInventoryChoiceIndex"] = GetCurrentInventoryChoiceIndex(activeInventoryChoiceObjects).ToString(CultureInfo.InvariantCulture);
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
			obj["mapViewScope"] = mapSnapshot?.ViewScope ?? "none";
			obj["mapGraphNodes"] = (mapSnapshot?.Nodes.Count ?? 0).ToString(CultureInfo.InvariantCulture);
			obj["mapGraphEdges"] = (mapSnapshot?.Edges.Count ?? 0).ToString(CultureInfo.InvariantCulture);
			obj["mapCurrentNodeId"] = mapSnapshot?.CurrentNodeId;
			obj["mapAvailableMoveCount"] = (mapSnapshot?.AvailableMoveNodeIds.Count ?? 0).ToString(CultureInfo.InvariantCulture);
			obj["mapKnownPointOfInterestCount"] = (mapSnapshot?.KnownPointsOfInterest.Count ?? 0).ToString(CultureInfo.InvariantCulture);
			flag = val8 != null;
			obj["playerController"] = flag.ToString();
			flag = _runtimeOptions.VerboseLogging;
			obj["verboseLogging"] = flag.ToString();
			SnapshotDiagnostics diagnostics = new SnapshotDiagnostics("live-plugin-capture", "Structured runtime metadata captured from the active IL2CPP scene.", obj);
			GameCatalog catalog = new GameCatalog(DateTimeOffset.UtcNow, characterProfile, readOnlyList, readOnlyList2, readOnlyList3, diagnostics);
			_artifactWriter.WriteCatalog(catalog);
			_artifactWriter.WriteSnapshot(BuildSnapshot(catalog, val, val3, val4, val8, mapSnapshot, screenState, val9));
			ManualLogSource log = _log;
			BepInExInfoLogInterpolatedStringHandler val10 = new BepInExInfoLogInterpolatedStringHandler(72, 6, out flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val10).AppendLiteral("Captured catalog from scene '");
				((BepInExLogInterpolatedStringHandler)val10).AppendFormatted<string>(activeScene.name);
				((BepInExLogInterpolatedStringHandler)val10).AppendLiteral("' (");
				((BepInExLogInterpolatedStringHandler)val10).AppendFormatted<string>(reason);
				((BepInExLogInterpolatedStringHandler)val10).AppendLiteral("): items=");
				((BepInExLogInterpolatedStringHandler)val10).AppendFormatted<int>(readOnlyList.Count);
				((BepInExLogInterpolatedStringHandler)val10).AppendLiteral(", monsters=");
				((BepInExLogInterpolatedStringHandler)val10).AppendFormatted<int>(readOnlyList2.Count);
				((BepInExLogInterpolatedStringHandler)val10).AppendLiteral(", maps=");
				((BepInExLogInterpolatedStringHandler)val10).AppendFormatted<int>(readOnlyList3.Count);
				((BepInExLogInterpolatedStringHandler)val10).AppendLiteral(", character=");
				((BepInExLogInterpolatedStringHandler)val10).AppendFormatted<string>((characterProfile == null) ? "none" : "present");
				((BepInExLogInterpolatedStringHandler)val10).AppendLiteral(".");
			}
			log.LogInfo(val10);
		}
		catch (Exception ex)
		{
			ManualLogSource log2 = _log;
			BepInExErrorLogInterpolatedStringHandler val10 = new BepInExErrorLogInterpolatedStringHandler(35, 2, out flag);
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

	private SystemCollections.IReadOnlyList<CatalogItem> BuildItems(ItemManager itemManager)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		if (((itemManager != null) ? itemManager.itemPrefabs : null) == null)
		{
			return Array.Empty<CatalogItem>();
		}
		SystemCollections.List<CatalogItem> list = new SystemCollections.List<CatalogItem>();
		SystemCollections.HashSet<string> hashSet = new SystemCollections.HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var enumerator = itemManager.itemPrefabs.GetEnumerator();
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

	private SystemCollections.IReadOnlyList<CatalogMonster> BuildMonsters(StatsManager statsManager)
	{
		SystemCollections.List<CatalogMonster> list = new SystemCollections.List<CatalogMonster>();
		SystemCollections.HashSet<string> seen = new SystemCollections.HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
			var enumerator2 = statsManager.GetAllBosses().GetEnumerator();
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

	private static void AddMonster(SystemCollections.ICollection<CatalogMonster> results, SystemCollections.ISet<string> seen, CatalogMonster monster)
	{
		if (seen.Add(monster.MonsterId))
		{
			results.Add(monster);
		}
	}

	private SystemCollections.IReadOnlyList<CatalogMap> BuildMaps(MapManager mapManager)
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
		obj[1] = activeScene.name;
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

	private GameSnapshot BuildSnapshot(GameCatalog catalog, OverworldUIManager overworldUiManager, StatsManager statsManager, MapManager mapManager, PlayerController playerController, MapSnapshot mapSnapshot, ScreenState screenState, EventPopup eventPopup)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		CharacterProfile character = catalog.Character;
		SystemCollections.IReadOnlyList<InventoryItemSnapshot> inventory = BuildInventory(overworldUiManager);
		GameEventSnapshot eventContext = BuildEventContext(eventPopup, mapSnapshot, overworldUiManager, inventory, mapManager, playerController, Object.FindObjectOfType<PlayerControlsManager>(), string.Equals(screenState.Screen, "live-event", StringComparison.OrdinalIgnoreCase));
		EncounterSnapshot encounter = ((!string.Equals(screenState.Screen, "live-battle", StringComparison.OrdinalIgnoreCase)) ? null : BuildEncounter(statsManager));
		string screen = screenState.Screen;
		Scene activeScene = SceneManager.GetActiveScene();
		return new GameSnapshot(screen, activeScene.name, catalog.CapturedAt, new PlayerSnapshot(character?.Health ?? 0, character?.Health ?? 0, character?.Armor ?? 0, character?.Gold ?? 0, character?.CurrentBossNumber ?? 0), inventory, _scaffold.CreateActionsForScreen(screenState.Screen, mapSnapshot), encounter, mapSnapshot, catalog.Diagnostics, eventContext);
	}

	private static MapSnapshot BuildMapSnapshot(SystemCollections.IReadOnlyList<CatalogMap> maps, MapManager mapManager, PlayerController playerController, ScreenState screenState)
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
			return new MapSnapshot(maps[0].MapId, maps[0].AreaName, maps[0].EnemyTileCount, "local-neighborhood", null, Array.Empty<string>(), Array.Empty<MapMoveOptionSnapshot>(), Array.Empty<MapPointOfInterestSnapshot>(), Array.Empty<MapNodeSnapshot>(), Array.Empty<MapEdgeSnapshot>());
		}
		SystemCollections.Dictionary<Vector2Int, MapNodeSnapshot> nodes = new SystemCollections.Dictionary<Vector2Int, MapNodeSnapshot>();
		var enumerator = val.grid.GetEnumerator();
		while (enumerator.MoveNext())
		{
			var current = enumerator.Current;
			if (current == null)
			{
				continue;
			}
			var enumerator2 = current.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				CellData cell = enumerator2.Current;
				if (cell != null)
				{
					Vector2Int coordinate = new Vector2Int(cell.xCoord, cell.yCoord);
					OccupantInfo occupantInfo = DescribeOccupant(mapManager, coordinate);
					nodes[coordinate] = new MapNodeSnapshot(BuildMapNodeId(coordinate), coordinate.x, coordinate.y, SafeCall(() => cell.CanTraverse(), fallback: false), SafeCall(() => mapManager.playerExploredCoords.Contains(coordinate), fallback: false), SafeCall(() => cell.hasFog, fallback: false), SafeCall(() => mapManager.enemyTiles.ContainsKey(coordinate), fallback: false), SafeCall(() => mapManager.eventTiles.ContainsKey(coordinate), fallback: false), SafeCall<string>(() => ((object)(CellEnvironmentType)cell.cellEnvironmentType/*cast due to constrained. prefix*/).ToString(), cell.cellEnvironmentType.ToString(CultureInfo.InvariantCulture)), occupantInfo.Category, occupantInfo.Id, occupantInfo.Name);
				}
			}
		}
		SystemCollections.List<MapEdgeSnapshot> list = new SystemCollections.List<MapEdgeSnapshot>();
		SystemCollections.KeyValuePair<Vector2Int, string>[] array = new SystemCollections.KeyValuePair<Vector2Int, string>[4]
		{
			new SystemCollections.KeyValuePair<Vector2Int, string>(new Vector2Int(0, 1), "up"),
			new SystemCollections.KeyValuePair<Vector2Int, string>(new Vector2Int(1, 0), "right"),
			new SystemCollections.KeyValuePair<Vector2Int, string>(new Vector2Int(0, -1), "down"),
			new SystemCollections.KeyValuePair<Vector2Int, string>(new Vector2Int(-1, 0), "left")
		};
		foreach (SystemCollections.KeyValuePair<Vector2Int, MapNodeSnapshot> item in nodes)
		{
			SystemCollections.KeyValuePair<Vector2Int, string>[] array2 = array;
			for (int num = 0; num < array2.Length; num++)
			{
				SystemCollections.KeyValuePair<Vector2Int, string> keyValuePair = array2[num];
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
		SystemCollections.HashSet<string> localNodeIds = BuildLocalViewNodeIds(currentGridPosition, currentNodeId, nodes.Values, availableMoveNodeIds);
		MapPointOfInterestSnapshot[] knownPointsOfInterest = (from node in nodes.Values
			where IsDiscoveredPointOfInterest(node, localNodeIds)
			orderby string.Equals(node.NodeId, currentNodeId, StringComparison.Ordinal) descending, Mathf.Abs(node.X - currentGridPosition.x) + Mathf.Abs(node.Y - currentGridPosition.y), node.X, node.Y
			select new MapPointOfInterestSnapshot(node.NodeId, node.X, node.Y, node.OccupantCategory, node.OccupantId, node.OccupantName, string.Equals(node.NodeId, currentNodeId, StringComparison.Ordinal), !node.HasFog, node.IsExplored)).ToArray();
		MapNodeSnapshot[] array3 = (from node in nodes.Values
			where localNodeIds.Contains(node.NodeId)
			orderby node.X, node.Y
			select node).ToArray();
		MapEdgeSnapshot[] edges = list.Where((MapEdgeSnapshot edge) => localNodeIds.Contains(edge.FromNodeId) && localNodeIds.Contains(edge.ToNodeId)).OrderBy((MapEdgeSnapshot edge) => edge.FromNodeId, StringComparer.Ordinal).ThenBy((MapEdgeSnapshot edge) => edge.ToNodeId, StringComparer.Ordinal).ThenBy((MapEdgeSnapshot edge) => edge.Direction, StringComparer.Ordinal).ToArray();
		return new MapSnapshot(maps[0].MapId, maps[0].AreaName, maps[0].EnemyTileCount, "local-neighborhood", currentNodeId, availableMoveNodeIds, availableMoves, knownPointsOfInterest, array3, edges);
	}

	private static SystemCollections.HashSet<string> BuildLocalViewNodeIds(Vector2Int currentGridPosition, string currentNodeId, SystemCollections.IEnumerable<MapNodeSnapshot> nodes, SystemCollections.IReadOnlyCollection<string> availableMoveNodeIds)
	{
		SystemCollections.HashSet<string> hashSet = new SystemCollections.HashSet<string>(availableMoveNodeIds ?? Array.Empty<string>(), StringComparer.Ordinal);
		if (!string.IsNullOrWhiteSpace(currentNodeId))
		{
			hashSet.Add(currentNodeId);
		}
		foreach (MapNodeSnapshot node in nodes)
		{
			if (node != null && IsNodeInLocalView(currentGridPosition, node, hashSet))
			{
				hashSet.Add(node.NodeId);
			}
		}
		return hashSet;
	}

	private static bool IsNodeInLocalView(Vector2Int currentGridPosition, MapNodeSnapshot node, SystemCollections.ISet<string> localNodeIds)
	{
		if (localNodeIds.Contains(node.NodeId))
		{
			return true;
		}
		int num = Mathf.Abs(node.X - currentGridPosition.x) + Mathf.Abs(node.Y - currentGridPosition.y);
		return num <= 1 && (node.IsExplored || !node.HasFog || node.HasEnemy || node.HasEvent);
	}

	private static bool IsDiscoveredPointOfInterest(MapNodeSnapshot node, SystemCollections.ISet<string> localNodeIds)
	{
		if (node == null || string.Equals(node.OccupantCategory, "none", StringComparison.Ordinal))
		{
			return false;
		}
		return localNodeIds.Contains(node.NodeId) || node.IsExplored || !node.HasFog;
	}

	private static OccupantInfo DescribeOccupant(MapManager mapManager, Vector2Int coordinate)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		Tile enemyTileBase = default(Tile);
		if (((mapManager != null) ? mapManager.enemyTiles : null) != null && mapManager.enemyTiles.TryGetValue(coordinate, out enemyTileBase))
		{
			EnemyTile enemyTile = SafeCall(() => ((Il2CppObjectBase)enemyTileBase).TryCast<EnemyTile>(), null);
			if (enemyTile != null)
			{
				EnemyStats val = SafeCall(() => enemyTile.GetEnemyStats(), null);
				return new OccupantInfo("monster", NormalizeId((val != null) ? val.nameTag : null, (val != null) ? val.enemyName : null, ((Tile)enemyTile).nameTag, ((Object)enemyTile).name), FirstValue((val != null) ? val.enemyName : null, ((Tile)enemyTile).nameTag, ((Object)enemyTile).name));
			}
		}
		Tile eventTileBase = default(Tile);
		if (((mapManager != null) ? mapManager.eventTiles : null) != null && mapManager.eventTiles.TryGetValue(coordinate, out eventTileBase))
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
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<HomeHouseTile>(), null) != null)
		{
			return "HomeHouseTile";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<LargeBonfireTile>(), null) != null)
		{
			return "LargeBonfireTile";
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
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<HomeHouseTile>(), null) != null)
		{
			return "home";
		}
		if (SafeCall(() => ((Il2CppObjectBase)eventTile).TryCast<LargeBonfireTile>(), null) != null)
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
			if (text.Contains("homehouse", StringComparison.Ordinal) || text.Contains("home", StringComparison.Ordinal) || text.Contains("house", StringComparison.Ordinal))
			{
				return "home";
			}
			if (text.Contains("bonfire", StringComparison.Ordinal))
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
		bool flag = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y) == 1;
		if (playerController == null)
		{
			return flag;
		}
		return SafeCall(() => playerController.IsMoveAllowed(new Vector2((float)from.x, (float)from.y), new Vector2((float)to.x, (float)to.y)), flag);
	}

	private static string BuildMapNodeId(Vector2Int coordinate)
	{
		return coordinate.x.ToString(CultureInfo.InvariantCulture) + "," + coordinate.y.ToString(CultureInfo.InvariantCulture);
	}

	private static ScreenState DetermineScreenState(PlayerControlsManager controlsManager, WorldsMenu worldsMenu, DifficultyToggle difficultyToggle, WaitingRoomDisplayer waitingRoom, ShowContinueWorld showContinueWorld, StatsManager statsManager, MapManager mapManager, PlayerController playerController, EventPopup eventPopup)
	{
		bool flag = IsActive((waitingRoom != null) ? ((Component)waitingRoom).gameObject : null) || IsActive((controlsManager != null) ? controlsManager.waitingRoomMenu : null);
		bool flag2 = IsActive((difficultyToggle != null) ? difficultyToggle.difficultySelectionHolder : null) || IsActive((difficultyToggle != null) ? ((Component)difficultyToggle).gameObject : null);
		bool flag3 = IsActive((worldsMenu != null) ? worldsMenu.worldsParent : null) || IsActive((controlsManager != null) ? controlsManager.worldsParent : null) || IsActive((worldsMenu != null) ? ((Component)worldsMenu).gameObject : null);
		bool flag4 = IsActive((showContinueWorld != null) ? showContinueWorld.worldHolder : null) || IsActive((showContinueWorld != null) ? ((Component)showContinueWorld).gameObject : null);
		bool flag5 = IsActive((controlsManager != null) ? controlsManager.mainMenuParent : null);
		bool flag7 = HasLiveEventContext(eventPopup, mapManager, playerController, controlsManager);
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
		else if (flag7)
		{
			screen = "live-event";
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

	private static SystemCollections.IReadOnlyList<InventoryItemSnapshot> BuildInventory(OverworldUIManager overworldUiManager)
	{
		if (overworldUiManager == null)
		{
			return Array.Empty<InventoryItemSnapshot>();
		}
		SystemCollections.List<InventoryItemSnapshot> list = new SystemCollections.List<InventoryItemSnapshot>();
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
				list.Add(BuildInventoryItemSnapshot(val, "inventory", num2));
			}
		}
		return list;
	}

	private static GameEventSnapshot BuildEventContext(EventPopup eventPopup, MapSnapshot mapSnapshot, OverworldUIManager overworldUiManager, SystemCollections.IReadOnlyList<InventoryItemSnapshot> inventoryItems, MapManager mapManager, PlayerController playerController, PlayerControlsManager controlsManager, bool forceForLiveEventScreen)
	{
		EventTile currentEventTile = GetCurrentEventTile(mapManager, playerController);
		EventChooseEntry[] choiceEntries = GetChoiceEntries(eventPopup, currentEventTile);
		if (!forceForLiveEventScreen && !HasLiveEventContext(eventPopup, currentEventTile, choiceEntries, controlsManager))
		{
			return null;
		}

		InventoryStateSnapshot inventoryState = BuildInventoryState(overworldUiManager);
		SystemCollections.List<EventChoiceSnapshot> list = new SystemCollections.List<EventChoiceSnapshot>();
		MapNodeSnapshot currentNode = mapSnapshot?.Nodes?.FirstOrDefault((MapNodeSnapshot node) => string.Equals(node.NodeId, mapSnapshot.CurrentNodeId, StringComparison.Ordinal));
		if (ShouldUseChestItemChoices(currentEventTile, choiceEntries) && TryBuildChestChoices(list, currentEventTile, inventoryItems, inventoryState))
		{
		}
		else if (ShouldUseInventorySelectionChoices(choiceEntries) && TryBuildInventorySelectionChoices(list, inventoryItems, inventoryState))
		{
		}
		else
		{
			for (int i = 0; i < choiceEntries.Length; i++)
			{
				EventChoiceSnapshot eventChoiceSnapshot = BuildEventChoice(choiceEntries[i], i, inventoryItems, inventoryState);
				if (eventChoiceSnapshot != null)
				{
					list.Add(eventChoiceSnapshot);
				}
			}
		}

		int? selectedOptionIndex = list.Where((EventChoiceSnapshot option) => option.IsSelected).Select((EventChoiceSnapshot option) => (int?)option.Index).FirstOrDefault();
		if (!selectedOptionIndex.HasValue && BridgeActionQueueProcessor.SelectedEventOptionIndexOverride.HasValue && list.Count > 0)
		{
			selectedOptionIndex = Math.Max(0, Math.Min(list.Count - 1, BridgeActionQueueProcessor.SelectedEventOptionIndexOverride.Value));
		}
		if (!selectedOptionIndex.HasValue && list.Count > 0)
		{
			selectedOptionIndex = 0;
		}
		string text = GetText((eventPopup != null) ? ((TMP_Text)eventPopup.titleEntry) : null);
		if (string.IsNullOrWhiteSpace(text))
		{
			text = currentNode?.OccupantName ?? currentNode?.OccupantCategory ?? "event";
		}
		string text2 = GetText((eventPopup != null) ? ((TMP_Text)eventPopup.explainerEntry) : null);
		return new GameEventSnapshot(currentNode?.OccupantCategory ?? "event", text, text2, selectedOptionIndex, list, inventoryState);
	}

	private static bool ShouldUseInventorySelectionChoices(EventChooseEntry[] choiceEntries)
	{
		if (HasItemChoiceEntry(choiceEntries))
		{
			return false;
		}

		GameObject[] activeInventoryChoiceObjects = GetActiveInventoryChoiceObjects();
		return activeInventoryChoiceObjects.Length > 0;
	}

	private static bool ShouldUseChestItemChoices(EventTile currentEventTile, EventChooseEntry[] choiceEntries)
	{
		ChestTile chestTile = (currentEventTile == null) ? null : SafeCall(() => ((Il2CppObjectBase)currentEventTile).TryCast<ChestTile>(), null);
		if (chestTile?.items == null || chestTile.items.Count == 0)
		{
			return false;
		}

		if (choiceEntries == null || choiceEntries.Length == 0)
		{
			return true;
		}

		for (int i = 0; i < choiceEntries.Length; i++)
		{
			if (IsItemChoiceEntry(choiceEntries[i]))
			{
				return true;
			}
		}

		return false;
	}

	private static bool TryBuildChestChoices(SystemCollections.ICollection<EventChoiceSnapshot> choices, EventTile currentEventTile, SystemCollections.IReadOnlyList<InventoryItemSnapshot> inventoryItems, InventoryStateSnapshot inventoryState)
	{
		ChestTile chestTile = (currentEventTile == null) ? null : SafeCall(() => ((Il2CppObjectBase)currentEventTile).TryCast<ChestTile>(), null);
		if (chestTile?.items == null || chestTile.items.Count == 0)
		{
			return false;
		}

		for (int i = 0; i < chestTile.items.Count; i++)
		{
			EffectBase effectBase = chestTile.items[i];
			InventoryItem inventoryItem = (effectBase == null) ? null : SafeCall(() => ((Il2CppObjectBase)effectBase).TryCast<InventoryItem>(), null);
			CatalogItem catalogItem = BuildCatalogItem(inventoryItem);
			if (catalogItem != null)
			{
				choices.Add(new EventChoiceSnapshot("option-" + i.ToString(CultureInfo.InvariantCulture), i, catalogItem.DisplayName, catalogItem.Description, isEnabled: true, isSelected: false, catalogItem, BuildItemComparison(catalogItem, inventoryItems, inventoryState)));
			}
		}

		return choices.Count > 0;
	}

	private static bool TryBuildInventorySelectionChoices(SystemCollections.ICollection<EventChoiceSnapshot> choices, SystemCollections.IReadOnlyList<InventoryItemSnapshot> inventoryItems, InventoryStateSnapshot inventoryState)
	{
		GameObject[] activeInventoryChoiceObjects = GetActiveInventoryChoiceObjects();
		GameObject currentSelectedGameObject = EventSystem.current?.currentSelectedGameObject;
		for (int i = 0; i < activeInventoryChoiceObjects.Length; i++)
		{
			GameObject gameObject = activeInventoryChoiceObjects[i];
			InventoryItem inventoryItem = GetInventoryItemFromSlotObject(gameObject);
			if (inventoryItem == null)
			{
				continue;
			}

			CatalogItem catalogItem = BuildCatalogItem(inventoryItem);
			if (catalogItem == null)
			{
				continue;
			}

			Button inventoryChoiceButton = GetInventoryChoiceButton(gameObject);
			GameObject gameObject2 = ((inventoryChoiceButton != null) ? ((Component)inventoryChoiceButton).gameObject : gameObject);
			bool isSelected = IsSelectionTarget(currentSelectedGameObject, gameObject2) || IsSelectionTarget(currentSelectedGameObject, gameObject);
			choices.Add(new EventChoiceSnapshot("inventory-option-" + i.ToString(CultureInfo.InvariantCulture), i, catalogItem.DisplayName, catalogItem.Description, isEnabled: true, isSelected, catalogItem, BuildItemComparison(catalogItem, inventoryItems, inventoryState)));
		}

		return choices.Count > 0;
	}

	private static EventChoiceSnapshot BuildEventChoice(EventChooseEntry entry, int index, SystemCollections.IReadOnlyList<InventoryItemSnapshot> inventoryItems, InventoryStateSnapshot inventoryState)
	{
		if (entry == null)
		{
			return null;
		}

		CatalogItem catalogItem = BuildCatalogItemFromChoice(entry);
		string label = catalogItem?.DisplayName ?? GetEntryLabel(entry) ?? ("option-" + index.ToString(CultureInfo.InvariantCulture));
		string description = catalogItem?.Description ?? string.Empty;
		bool isSelected = IsSelectedChoice(entry);
		EventItemComparisonSnapshot itemComparison = BuildItemComparison(catalogItem, inventoryItems, inventoryState);
		return new EventChoiceSnapshot("option-" + index.ToString(CultureInfo.InvariantCulture), index, label, description, entry.enableInteraction, isSelected, catalogItem, itemComparison);
	}

	private static InventoryStateSnapshot BuildInventoryState(OverworldUIManager overworldUiManager)
	{
		if (overworldUiManager == null)
		{
			return new InventoryStateSnapshot(0, 0, 0, 0, 0, 0, Array.Empty<InventoryItemSnapshot>());
		}

		SystemCollections.IReadOnlyList<InventoryItemSnapshot> backpackItems = BuildBackpackItems(overworldUiManager);
		int inventorySlotCount = SafeCall(() => overworldUiManager.GetNrOfInventorySlots(), 0);
		int inventoryItemCount = SafeCall(() => overworldUiManager.GetNrOfInventoryItems(), 0);
		int openInventorySlots = SafeCall(() => overworldUiManager.GetNrOfOpenInventorySlots(), Math.Max(0, inventorySlotCount - inventoryItemCount));
		int backpackItemCount = SafeCall(() => overworldUiManager.GetNrOfBackpackItems(), backpackItems.Count);
		int backpackSlotCount = SafeCall(() => overworldUiManager.backpackSlots?.Count ?? 0, backpackItems.Count);
		int openBackpackSlots = Math.Max(0, backpackSlotCount - backpackItemCount);
		return new InventoryStateSnapshot(inventoryItemCount, inventorySlotCount, openInventorySlots, backpackItemCount, backpackSlotCount, openBackpackSlots, backpackItems);
	}

	private static SystemCollections.IReadOnlyList<InventoryItemSnapshot> BuildBackpackItems(OverworldUIManager overworldUiManager)
	{
		if (overworldUiManager?.backpackSlots == null)
		{
			return Array.Empty<InventoryItemSnapshot>();
		}

		SystemCollections.List<InventoryItemSnapshot> list = new SystemCollections.List<InventoryItemSnapshot>();
		for (int i = 0; i < overworldUiManager.backpackSlots.Count; i++)
		{
			GameObject gameObject = overworldUiManager.backpackSlots[i];
			InventoryItem inventoryItem = GetInventoryItemFromSlotObject(gameObject);
			if (inventoryItem != null)
			{
				list.Add(BuildInventoryItemSnapshot(inventoryItem, "backpack", i));
			}
		}

		return list;
	}

	private static InventoryItem GetInventoryItemFromSlotObject(GameObject slotObject)
	{
		if (slotObject == null)
		{
			return null;
		}

		InventorySlot component = slotObject.GetComponent<InventorySlot>();
		InventoryDisplayItem inventoryDisplayItem = component?.inventoryDisplayItem;
		if (inventoryDisplayItem == null)
		{
			return null;
		}

		return SafeCall(() => inventoryDisplayItem.GetInventoryItem(), null);
	}

	private static InventoryItemSnapshot BuildInventoryItemSnapshot(InventoryItem item, string container, int slotIndex)
	{
		if (item == null)
		{
			return null;
		}

		return new InventoryItemSnapshot(NormalizeId(((EffectBase)item).nameTag, ((Object)item).name), FirstValue(((EffectBase)item).effectName, ((EffectBase)item).nameTag, ((Object)item).name), 1, container, slotIndex, FirstValue(((EffectBase)item).effectDesc, ((EffectBase)item).nameTag), ((object)item.itemType/*cast due to constrained. prefix*/).ToString(), ((object)item.itemRarity/*cast due to constrained. prefix*/).ToString(), ((EffectBase)item).attack, ((EffectBase)item).armor, ((EffectBase)item).speed, ((EffectBase)item).maxHealth, CollectTags(item.itemTags));
	}

	private static CatalogItem BuildCatalogItem(InventoryItem item)
	{
		if (item == null)
		{
			return null;
		}

		return new CatalogItem(NormalizeId(((EffectBase)item).nameTag, ((Object)item).name), FirstValue(((EffectBase)item).effectName, ((EffectBase)item).nameTag, ((Object)item).name), FirstValue(((EffectBase)item).effectDesc, ((EffectBase)item).nameTag), ((object)item.itemType/*cast due to constrained. prefix*/).ToString(), ((object)item.itemRarity/*cast due to constrained. prefix*/).ToString(), ((EffectBase)item).attack, ((EffectBase)item).armor, ((EffectBase)item).speed, ((EffectBase)item).maxHealth, item.spawnWeight, CollectTags(item.itemTags));
	}

	private static CatalogItem BuildCatalogItemFromChoice(EventChooseEntry entry)
	{
		ItemChooseEntry itemChooseEntry = SafeCall(() => ((Il2CppObjectBase)entry).TryCast<ItemChooseEntry>(), null);
		if (itemChooseEntry == null)
		{
			return null;
		}

		EffectBase effectBase = SafeCall(() => itemChooseEntry.GetEffectBase(), null);
		InventoryItem inventoryItem = (effectBase == null) ? null : SafeCall(() => ((Il2CppObjectBase)effectBase).TryCast<InventoryItem>(), null);
		return BuildCatalogItem(inventoryItem);
	}

	private static bool IsItemChoiceEntry(EventChooseEntry entry)
	{
		return entry != null && SafeCall(() => ((Il2CppObjectBase)entry).TryCast<ItemChooseEntry>(), null) != null;
	}

	private static bool HasItemChoiceEntry(EventChooseEntry[] choiceEntries)
	{
		if (choiceEntries == null)
		{
			return false;
		}

		for (int i = 0; i < choiceEntries.Length; i++)
		{
			if (IsItemChoiceEntry(choiceEntries[i]))
			{
				return true;
			}
		}

		return false;
	}

	private static EventItemComparisonSnapshot BuildItemComparison(CatalogItem catalogItem, SystemCollections.IReadOnlyList<InventoryItemSnapshot> inventoryItems, InventoryStateSnapshot inventoryState)
	{
		if (catalogItem == null)
		{
			return null;
		}

		int matchingInventoryCount = inventoryItems.Count((InventoryItemSnapshot item) => string.Equals(item.ItemId, catalogItem.ItemId, StringComparison.OrdinalIgnoreCase));
		int matchingBackpackCount = inventoryState.BackpackItems.Count((InventoryItemSnapshot item) => string.Equals(item.ItemId, catalogItem.ItemId, StringComparison.OrdinalIgnoreCase));
		int matchingExactInventoryCount = inventoryItems.Count((InventoryItemSnapshot item) => IsExactInventoryMatch(item, catalogItem));
		int matchingExactBackpackCount = inventoryState.BackpackItems.Count((InventoryItemSnapshot item) => IsExactInventoryMatch(item, catalogItem));
		int matchingExactTotalCount = matchingExactInventoryCount + matchingExactBackpackCount;
		bool hasFreeInventorySlot = inventoryState.OpenInventorySlots > 0;
		bool hasFreeBackpackSlot = inventoryState.OpenBackpackSlots > 0;
		return new EventItemComparisonSnapshot(matchingInventoryCount > 0, matchingBackpackCount > 0, matchingInventoryCount, matchingBackpackCount, matchingExactInventoryCount, matchingExactBackpackCount, matchingExactTotalCount, matchingExactTotalCount >= 2, hasFreeInventorySlot, hasFreeBackpackSlot, hasFreeInventorySlot || hasFreeBackpackSlot);
	}

	private static bool IsExactInventoryMatch(InventoryItemSnapshot inventoryItem, CatalogItem catalogItem)
	{
		if (inventoryItem == null || catalogItem == null)
		{
			return false;
		}

		return string.Equals(inventoryItem.ItemId, catalogItem.ItemId, StringComparison.OrdinalIgnoreCase) && string.Equals(inventoryItem.DisplayName, catalogItem.DisplayName, StringComparison.OrdinalIgnoreCase) && string.Equals(inventoryItem.Description, catalogItem.Description, StringComparison.OrdinalIgnoreCase) && string.Equals(inventoryItem.ItemType, catalogItem.ItemType, StringComparison.OrdinalIgnoreCase) && string.Equals(inventoryItem.Rarity, catalogItem.Rarity, StringComparison.OrdinalIgnoreCase) && inventoryItem.Attack == catalogItem.Attack && inventoryItem.Armor == catalogItem.Armor && inventoryItem.Speed == catalogItem.Speed && inventoryItem.MaxHealth == catalogItem.MaxHealth && HaveEquivalentTags(inventoryItem.Tags, catalogItem.Tags);
	}

	private static bool HaveEquivalentTags(SystemCollections.IReadOnlyList<string> left, SystemCollections.IReadOnlyList<string> right)
	{
		left ??= Array.Empty<string>();
		right ??= Array.Empty<string>();
		if (left.Count != right.Count)
		{
			return false;
		}

		return left.Select((string tag) => tag ?? string.Empty).OrderBy((string tag) => tag, StringComparer.OrdinalIgnoreCase).SequenceEqual(right.Select((string tag) => tag ?? string.Empty).OrderBy((string tag) => tag, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
	}

	private static EventChooseEntry[] GetChoiceEntries(EventPopup eventPopup, EventTile currentEventTile)
	{
		SystemCollections.List<EventChooseEntry> list = new SystemCollections.List<EventChooseEntry>();
		AddChoiceEntry(list, SafeCall(() => eventPopup.GetEventChooseEntry(), null));
		AddChoiceEntry(list, SafeCall(() => eventPopup.GetEventChooseEntryAdditional(), null));
		AddChoiceEntry(list, SafeCall(() => currentEventTile.GetEventChooseEntry(), null));
		AddChoiceEntry(list, SafeCall(() => currentEventTile.GetEventChooseEntryAdditional(), null));
		foreach (EventChooseEntry activeSceneEventChoice in GetActiveSceneEventChoices())
		{
			AddChoiceEntry(list, activeSceneEventChoice);
		}
		return list.ToArray();
	}

	private static EventChooseEntry[] GetActiveSceneEventChoices()
	{
		return Object.FindObjectsOfType<EventChooseEntry>().Where((EventChooseEntry entry) => entry != null && SafeCall(() => ((Component)entry).gameObject.activeInHierarchy, false)).ToArray();
	}

	private static GameObject[] GetActiveInventoryChoiceObjects()
	{
		SystemCollections.List<GameObject> list = new SystemCollections.List<GameObject>();
		foreach (InventorySlot item in Object.FindObjectsOfType<InventorySlot>())
		{
			GameObject gameObject = ((item != null) ? ((Component)item).gameObject : null);
			if (gameObject != null && gameObject.activeInHierarchy && GetInventoryItemFromSlotObject(gameObject) != null)
			{
				list.Add(gameObject);
			}
		}

		return list.OrderByDescending((GameObject gameObject) => gameObject.transform.position.y).ThenBy((GameObject gameObject) => gameObject.transform.position.x).ToArray();
	}

	private static Button GetInventoryChoiceButton(GameObject slotObject)
	{
		if (slotObject == null)
		{
			return null;
		}

		Button component = slotObject.GetComponent<Button>();
		return component ?? slotObject.GetComponentInChildren<Button>(includeInactive: true);
	}

	private static int GetCurrentInventoryChoiceIndex(GameObject[] activeInventoryChoiceObjects)
	{
		GameObject currentSelectedGameObject = EventSystem.current?.currentSelectedGameObject;
		for (int i = 0; i < activeInventoryChoiceObjects.Length; i++)
		{
			GameObject gameObject = activeInventoryChoiceObjects[i];
			Button inventoryChoiceButton = GetInventoryChoiceButton(gameObject);
			if (IsSelectionTarget(currentSelectedGameObject, gameObject) || IsSelectionTarget(currentSelectedGameObject, (inventoryChoiceButton != null) ? ((Component)inventoryChoiceButton).gameObject : null))
			{
				return i;
			}
		}

		return -1;
	}

	private static bool IsSelectionTarget(GameObject selectedObject, GameObject candidateObject)
	{
		if (selectedObject == null || candidateObject == null)
		{
			return false;
		}

		Transform transform = selectedObject.transform;
		Transform transform2 = candidateObject.transform;
		return transform == transform2 || transform.IsChildOf(transform2) || transform2.IsChildOf(transform);
	}

	private static void AddChoiceEntry(SystemCollections.ICollection<EventChooseEntry> entries, EventChooseEntry entry)
	{
		if (entry == null)
		{
			return;
		}

		foreach (EventChooseEntry existing in entries)
		{
			if (((Il2CppObjectBase)existing).Pointer == ((Il2CppObjectBase)entry).Pointer)
			{
				return;
			}
		}

		entries.Add(entry);
	}

	private static string GetEntryLabel(EventChooseEntry entry)
	{
		Button button = SafeCall(() => entry.GetButton(), null);
		if (button == null)
		{
			return null;
		}

		TextMeshProUGUI[] componentsInChildren = ((Component)button).GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		foreach (TextMeshProUGUI val in componentsInChildren)
		{
			string text = GetText((TMP_Text)val);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}

		return null;
	}

	private static bool IsSelectedChoice(EventChooseEntry entry)
	{
		Button button = SafeCall(() => entry.GetButton(), null);
		GameObject currentSelectedGameObject = EventSystem.current?.currentSelectedGameObject;
		if (button == null || currentSelectedGameObject == null)
		{
			return false;
		}

		Transform transform = ((Component)button).transform;
		Transform transform2 = currentSelectedGameObject.transform;
		return transform2 == transform || transform2.IsChildOf(transform);
	}

	private static string GetText(TMP_Text label)
	{
		return string.IsNullOrWhiteSpace(label?.text) ? string.Empty : label.text.Trim();
	}

	private static EventTile GetCurrentEventTile(MapManager mapManager, PlayerController playerController)
	{
		if (mapManager == null || playerController == null || mapManager.eventTiles == null)
		{
			return null;
		}

		Vector2 playerGridPosition = playerController.GetPlayerGridPosition();
		Vector2Int coordinate = new Vector2Int(Mathf.RoundToInt(playerGridPosition.x), Mathf.RoundToInt(playerGridPosition.y));
		Tile eventTileBase = default(Tile);
		if (!mapManager.eventTiles.TryGetValue(coordinate, out eventTileBase))
		{
			return null;
		}

		return SafeCall(() => ((Il2CppObjectBase)eventTileBase).TryCast<EventTile>(), null);
	}

	private static bool HasLiveEventContext(EventPopup eventPopup, MapManager mapManager, PlayerController playerController, PlayerControlsManager controlsManager)
	{
		EventTile currentEventTile = GetCurrentEventTile(mapManager, playerController);
		EventChooseEntry[] choiceEntries = GetChoiceEntries(eventPopup, currentEventTile);
		return HasLiveEventContext(eventPopup, currentEventTile, choiceEntries, controlsManager);
	}

	private static bool HasLiveEventContext(EventPopup eventPopup, EventTile currentEventTile, EventChooseEntry[] choiceEntries, PlayerControlsManager controlsManager)
	{
		if (IsActive((eventPopup != null) ? ((Component)eventPopup).gameObject : null))
		{
			return true;
		}

		if (!SafeCall(() => controlsManager != null && controlsManager.isViewingEventPopup, false))
		{
			return false;
		}

		if (currentEventTile == null)
		{
			return false;
		}

		if (choiceEntries.Length > 0)
		{
			return true;
		}

		return ShouldUseInventorySelectionChoices(choiceEntries);
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
		BattleManager battleManager = statsManager.battleManager;
		BattleSystem battleSystem = ((battleManager != null) ? battleManager.battleSystem : null);
		EnemyStats enemyStats = ((battleSystem != null) ? battleSystem._enemyStats : null);
		int turnNumber = SafeCall(() => (battleSystem != null) ? battleSystem.GetTurnCounter() : 0, 0);
		string currentTurn = SafeCall(() => (battleSystem != null) ? ((object)battleSystem.GetBattleTurn()).ToString() : null, null);
		string battlePhase = SafeCall(() => (battleSystem != null) ? ((object)battleSystem._battlePhase).ToString() : null, null);
		bool? isPaused = SafeCall(() => (battleSystem != null) ? new bool?(battleSystem.isPaused) : null, (bool?)null);
		int? playerHealth = SafeCall(() => (statsManager != null) ? new int?(statsManager.GetPlayerHealth()) : null, (int?)null);
		int? playerStartHealth = SafeCall(() => (battleSystem != null) ? new int?(battleSystem.GetPlayerStartHealth()) : null, (int?)null);
		int? enemyHealth = SafeCall(() => (enemyStats != null) ? new int?(enemyStats.health) : null, (int?)null);
		int? enemyMaxHealth = SafeCall(() => (enemyStats != null) ? new int?(enemyStats.maxHealth) : null, (int?)null);
		return new EncounterSnapshot(NormalizeId(((EffectBase)currentBoss).nameTag, ((Object)currentBoss).name), FirstValue(((EffectBase)currentBoss).effectName, ((EffectBase)currentBoss).nameTag, ((Object)currentBoss).name), turnNumber, statsManager.GetCurrentBossNumber(), currentTurn, battlePhase, isPaused, playerHealth, playerStartHealth, enemyHealth, enemyMaxHealth);
	}

	private static SystemCollections.IReadOnlyList<string> CollectTags(Il2CppSystem.Collections.Generic.List<ItemTag> tags)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		SystemCollections.List<string> list = new SystemCollections.List<string>();
		var enumerator = tags.GetEnumerator();
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

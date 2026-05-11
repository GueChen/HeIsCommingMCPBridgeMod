using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Il2CppInterop.Runtime.InteropTypes;
using MCPBridgeMod.Contracts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MCPBridgeMod.Plugin;

public sealed class BridgeActionQueueProcessor
{
	private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	private readonly BridgeArtifactWriter _artifactWriter;

	private readonly LiveCatalogCapture _capture;

	private readonly ManualLogSource _log;

	private readonly Queue<ActionExecutionRequest> _pendingActions = new Queue<ActionExecutionRequest>();

	internal static int? SelectedEventOptionIndexOverride { get; private set; }

	private int _processedLineCount;

	private float _nextActionTime;

	public BridgeActionQueueProcessor(BridgeArtifactWriter artifactWriter, LiveCatalogCapture capture, ManualLogSource log)
	{
		_artifactWriter = artifactWriter;
		_capture = capture;
		_log = log;
		Directory.CreateDirectory(Path.GetDirectoryName(_artifactWriter.ActionQueuePath) ?? _artifactWriter.ActionQueuePath);
		if (!File.Exists(_artifactWriter.ActionQueuePath))
		{
			File.WriteAllText(_artifactWriter.ActionQueuePath, string.Empty);
		}
		_processedLineCount = File.ReadAllLines(_artifactWriter.ActionQueuePath).Length;
	}

	public void Pump()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		EnqueuePendingRequests();
		if (_pendingActions.Count == 0 || Time.unscaledTime < _nextActionTime)
		{
			return;
		}
		ActionExecutionRequest actionExecutionRequest = _pendingActions.Dequeue();
		try
		{
			bool flag = Execute(actionExecutionRequest);
			_nextActionTime = Time.unscaledTime + 0.15f;
			if (flag)
			{
				_capture.Capture("action-" + actionExecutionRequest.ActionId);
			}
		}
		catch (Exception ex)
		{
			ManualLogSource log = _log;
			bool flag2 = default(bool);
			BepInExErrorLogInterpolatedStringHandler val = new BepInExErrorLogInterpolatedStringHandler(43, 2, out flag2);
			if (flag2)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Failed to execute queued bridge action '");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionExecutionRequest.ActionId);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("': ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<Exception>(ex);
			}
			log.LogError(val);
		}
	}

	private void EnqueuePendingRequests()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		string[] array;
		try
		{
			array = File.ReadAllLines(_artifactWriter.ActionQueuePath);
		}
		catch (IOException)
		{
			return;
		}
		if (array.Length < _processedLineCount)
		{
			_processedLineCount = 0;
		}
		bool flag = default(bool);
		for (int i = _processedLineCount; i < array.Length; i++)
		{
			string text = array[i];
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			try
			{
				ActionExecutionRequest actionExecutionRequest = JsonSerializer.Deserialize<ActionExecutionRequest>(text, SerializerOptions);
				if (actionExecutionRequest != null)
				{
					_pendingActions.Enqueue(actionExecutionRequest);
				}
			}
			catch (Exception ex2)
			{
				ManualLogSource log = _log;
				BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(53, 2, out flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Ignoring invalid bridge action queue entry at line ");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>(i + 1);
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral(": ");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(ex2.Message);
				}
				log.LogWarning(val);
			}
		}
		_processedLineCount = array.Length;
	}

	private bool Execute(ActionExecutionRequest request)
	{
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Expected O, but got Unknown
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		string text = request.ActionId?.Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		StatsManager val = UnityEngine.Object.FindObjectOfType<StatsManager>();
		PlayerController playerController = ((val != null) ? val.GetPlayerController() : null) ?? UnityEngine.Object.FindObjectOfType<PlayerController>();
		PlayerControlsManager val2 = UnityEngine.Object.FindObjectOfType<PlayerControlsManager>();
		MapManager val3 = UnityEngine.Object.FindObjectOfType<MapManager>();
		object obj;
		if (val == null)
		{
			obj = null;
		}
		else
		{
			BattleManager battleManager = val.battleManager;
			obj = ((battleManager != null) ? battleManager.battleSystem : null);
		}
		BattleSystem battleSystem = (BattleSystem)obj;
		switch (text)
		{
		case "move_up":
			return TryMove(playerController, Vector2.up, text);
		case "move_down":
			return TryMove(playerController, Vector2.down, text);
		case "move_left":
			return TryMove(playerController, Vector2.left, text);
		case "move_right":
			return TryMove(playerController, Vector2.right, text);
		case "confirm":
			return TryConfirm(val2, playerController, text);
		case "cancel":
			return TryInvoke(val2, delegate(PlayerControlsManager controls)
			{
				controls.PressedEastButton();
			}, text, "PressedEastButton");
		case "open_map":
			return TryInvoke(val2, delegate(PlayerControlsManager controls)
			{
				controls.ToggleMapView(true);
			}, text, "ToggleMapView(true)");
		case "close_map":
			return TryInvoke(val2, delegate(PlayerControlsManager controls)
			{
				controls.ToggleMapView(false);
			}, text, "ToggleMapView(false)");
		case "attack":
		case "end_turn":
			return TryBattleAdvance(battleSystem, val2, text);
		case "interact":
			return TryInteract(val2, playerController, val3, text);
		case "refresh_state":
			_log.LogInfo((object)"Refreshing bridge state from queued request.");
			return true;
		default:
		{
			ManualLogSource log = _log;
			bool flag = default(bool);
			BepInExInfoLogInterpolatedStringHandler val4 = new BepInExInfoLogInterpolatedStringHandler(54, 1, out flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val4).AppendLiteral("Queued bridge action '");
				((BepInExLogInterpolatedStringHandler)val4).AppendFormatted<string>(text);
				((BepInExLogInterpolatedStringHandler)val4).AppendLiteral("' is not handled in-process yet.");
			}
			log.LogInfo(val4);
			return false;
		}
		}
	}

	private bool TryInteract(PlayerControlsManager controlsManager, PlayerController playerController, MapManager mapManager, string actionId)
	{
		if (playerController != null && (playerController.isMapMoving || playerController.isPlayerMoving))
		{
			_log.LogInfo((object)("Skipping '" + actionId + "' because the player or map is already moving."));
			return false;
		}

		EventTile currentEventTile = GetCurrentEventTile(mapManager, playerController);
		EventPopup activeEventPopup = GetActiveEventPopup(currentEventTile) ?? GetActiveGlobalEventPopup();
		if (currentEventTile == null)
		{
			Vector2 vector = ((playerController != null) ? playerController.GetPlayerGridPosition() : default(Vector2));
			_log.LogInfo((object)("Could not resolve current EventTile for '" + actionId + "' at player grid (" + vector.x.ToString("0.##", CultureInfo.InvariantCulture) + ", " + vector.y.ToString("0.##", CultureInfo.InvariantCulture) + ")."));
		}
		else
		{
			_log.LogInfo((object)("Resolved current EventTile for '" + actionId + "' with tilePopupActive=" + HasActiveEventPopup(currentEventTile) + " globalPopupActive=" + (activeEventPopup != null) + "."));
		}

		EventChooseEntry[] visibleEventChoices = GetVisibleEventChoices(currentEventTile, activeEventPopup);
		if (TryResolveCurrentInventoryChoice(controlsManager, visibleEventChoices, actionId))
		{
			return true;
		}

		if (activeEventPopup != null && TryResolveCurrentEventChoice(currentEventTile, activeEventPopup, actionId))
		{
			return true;
		}

		if (TryTriggerCurrentEventTile(currentEventTile, actionId))
		{
			return true;
		}

		return TryInvoke(controlsManager, delegate(PlayerControlsManager controls)
		{
			controls.PressedSouthButton();
		}, actionId, "PressedSouthButton");
	}

	private bool TryTriggerCurrentEventTile(EventTile currentEventTile, string actionId)
	{
		if (currentEventTile == null)
		{
			return false;
		}

		bool result = currentEventTile.TriggerTile();
		if (!result)
		{
			_log.LogInfo((object)("EventTile.TriggerTile() returned false for '" + actionId + "'."));
			return false;
		}

		_log.LogInfo((object)("Executed '" + actionId + "' via EventTile.TriggerTile()."));
		return true;
	}

	private bool TryResolveCurrentEventChoice(EventTile currentEventTile, EventPopup activeEventPopup, string actionId)
	{
		if (currentEventTile == null && activeEventPopup == null)
		{
			return false;
		}

		EventChooseEntry[] visibleEventChoices = GetVisibleEventChoices(currentEventTile, activeEventPopup);
		int currentEventChoiceIndex = GetCurrentEventChoiceIndex(currentEventTile, activeEventPopup, visibleEventChoices);
		EventChooseEntry primary = ResolveEventChoiceAt(visibleEventChoices, currentEventChoiceIndex) ?? GetPrimaryEventChoice(currentEventTile, activeEventPopup);
		if (primary == null || !primary.enableInteraction)
		{
			return false;
		}

		ChestTile chestTile = (currentEventTile == null) ? null : SafeCall(() => ((Il2CppObjectBase)currentEventTile).TryCast<ChestTile>(), null);
		if (ShouldUseChestItemChoices(chestTile, visibleEventChoices) && currentEventChoiceIndex >= 0 && currentEventChoiceIndex < chestTile.items.Count)
		{
			chestTile.ChooseItem(currentEventChoiceIndex);
			SelectedEventOptionIndexOverride = null;
			_log.LogInfo((object)("Executed '" + actionId + "' via ChestTile.ChooseItem(" + currentEventChoiceIndex.ToString(CultureInfo.InvariantCulture) + ")."));
			return true;
		}

		primary.RunAssignment();
		SelectedEventOptionIndexOverride = null;
		_log.LogInfo((object)("Executed '" + actionId + "' via EventChooseEntry.RunAssignment()."));
		return true;
	}

	private static EventChooseEntry GetPrimaryEventChoice(EventTile currentEventTile, EventPopup activeEventPopup)
	{
		if (activeEventPopup == null)
		{
			return null;
		}

		EventChooseEntry eventChooseEntry = SafeCall(() => activeEventPopup.GetEventChooseEntry(), null);
		if (eventChooseEntry != null)
		{
			return eventChooseEntry;
		}

		eventChooseEntry = SafeCall(() => currentEventTile.GetEventChooseEntry(), null);
		if (eventChooseEntry != null)
		{
			return eventChooseEntry;
		}

		return SafeCall(() => activeEventPopup.GetEventChooseEntryAdditional(), null);
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

		return SafeCall(() => ((Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)eventTileBase).TryCast<EventTile>(), null);
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

	private static int GetCurrentEventChoiceIndex(EventTile currentEventTile, EventPopup activeEventPopup, EventChooseEntry[] visibleEventChoices)
	{
		if (SelectedEventOptionIndexOverride.HasValue)
		{
			int eventChoiceCount = GetEventChoiceCount(currentEventTile, activeEventPopup, visibleEventChoices);
			if (eventChoiceCount > 0)
			{
				return Math.Max(0, Math.Min(eventChoiceCount - 1, SelectedEventOptionIndexOverride.Value));
			}
		}

		int selectedEventChoiceIndex = GetSelectedEventChoiceIndex(visibleEventChoices);
		if (selectedEventChoiceIndex >= 0)
		{
			return selectedEventChoiceIndex;
		}

		return 0;
	}

	private static int GetSelectedEventChoiceIndex(EventChooseEntry[] visibleEventChoices)
	{
		for (int i = 0; i < visibleEventChoices.Length; i++)
		{
			Button button = SafeCall(() => visibleEventChoices[i].GetButton(), null);
			GameObject currentSelectedGameObject = EventSystem.current?.currentSelectedGameObject;
			if (button != null && currentSelectedGameObject != null)
			{
				Transform transform = ((Component)button).transform;
				Transform transform2 = currentSelectedGameObject.transform;
				if (transform2 == transform || transform2.IsChildOf(transform))
				{
					return i;
				}
			}
		}

		return -1;
	}

	private static int GetEventChoiceCount(EventTile currentEventTile, EventPopup activeEventPopup, EventChooseEntry[] visibleEventChoices)
	{
		ChestTile chestTile = (currentEventTile == null) ? null : SafeCall(() => ((Il2CppObjectBase)currentEventTile).TryCast<ChestTile>(), null);
		if (ShouldUseChestItemChoices(chestTile, visibleEventChoices))
		{
			return chestTile.items.Count;
		}

		return visibleEventChoices.Length;
	}

	private bool TryResolveCurrentInventoryChoice(PlayerControlsManager controlsManager, EventChooseEntry[] visibleEventChoices, string actionId)
	{
		if (!ShouldUseInventorySelectionChoices(visibleEventChoices))
		{
			return false;
		}

		GameObject[] activeInventoryChoiceObjects = GetActiveInventoryChoiceObjects();
		if (activeInventoryChoiceObjects.Length == 0)
		{
			return false;
		}

		int currentInventoryChoiceIndex = GetCurrentInventoryChoiceIndex(activeInventoryChoiceObjects);
		if (currentInventoryChoiceIndex < 0)
		{
			currentInventoryChoiceIndex = 0;
		}

		GameObject gameObject = activeInventoryChoiceObjects[currentInventoryChoiceIndex];
		SelectInventoryChoice(gameObject);
		Button inventoryChoiceButton = GetInventoryChoiceButton(gameObject);
		if (inventoryChoiceButton != null)
		{
			inventoryChoiceButton.onClick.Invoke();
			SelectedEventOptionIndexOverride = null;
			_log.LogInfo((object)("Executed '" + actionId + "' via inventory slot button click at index " + currentInventoryChoiceIndex.ToString(CultureInfo.InvariantCulture) + "."));
			return true;
		}

		return TryInvoke(controlsManager, delegate(PlayerControlsManager controls)
		{
			controls.PressedSouthButton();
		}, actionId, "PressedSouthButton after inventory slot selection");
	}

	private bool TryNavigateInventoryChoice(EventChooseEntry[] visibleEventChoices, Vector2 direction, string actionId)
	{
		if (!ShouldUseInventorySelectionChoices(visibleEventChoices))
		{
			return false;
		}

		GameObject[] activeInventoryChoiceObjects = GetActiveInventoryChoiceObjects();
		if (activeInventoryChoiceObjects.Length <= 1)
		{
			return false;
		}

		int currentInventoryChoiceIndex = GetCurrentInventoryChoiceIndex(activeInventoryChoiceObjects);
		if (currentInventoryChoiceIndex < 0)
		{
			currentInventoryChoiceIndex = 0;
		}

		int num = (((direction.x > 0f || direction.y > 0f) ? 1 : (-1)) + currentInventoryChoiceIndex + activeInventoryChoiceObjects.Length) % activeInventoryChoiceObjects.Length;
		SelectedEventOptionIndexOverride = num;
		SelectInventoryChoice(activeInventoryChoiceObjects[num]);
		_log.LogInfo((object)("Executed '" + actionId + "' by selecting inventory event option index " + num.ToString(CultureInfo.InvariantCulture) + "."));
		return true;
	}

	private static bool ShouldUseChestItemChoices(ChestTile chestTile, EventChooseEntry[] visibleEventChoices)
	{
		if (chestTile?.items == null || chestTile.items.Count <= 0)
		{
			return false;
		}

		if (visibleEventChoices == null || visibleEventChoices.Length == 0)
		{
			return true;
		}

		for (int i = 0; i < visibleEventChoices.Length; i++)
		{
			if (IsItemChoiceEntry(visibleEventChoices[i]))
			{
				return true;
			}
		}

		return false;
	}

	private static bool IsItemChoiceEntry(EventChooseEntry entry)
	{
		return entry != null && SafeCall(() => ((Il2CppObjectBase)entry).TryCast<ItemChooseEntry>(), null) != null;
	}

	private static bool ShouldUseInventorySelectionChoices(EventChooseEntry[] visibleEventChoices)
	{
		if (HasItemChoiceEntry(visibleEventChoices))
		{
			return false;
		}

		GameObject[] activeInventoryChoiceObjects = GetActiveInventoryChoiceObjects();
		return activeInventoryChoiceObjects.Length > 0 && visibleEventChoices.Length <= 1;
	}

	private static bool HasItemChoiceEntry(EventChooseEntry[] visibleEventChoices)
	{
		for (int i = 0; i < visibleEventChoices.Length; i++)
		{
			if (IsItemChoiceEntry(visibleEventChoices[i]))
			{
				return true;
			}
		}

		return false;
	}

	private static GameObject[] GetActiveInventoryChoiceObjects()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (InventorySlot item in UnityEngine.Object.FindObjectsOfType<InventorySlot>())
		{
			GameObject gameObject = ((item != null) ? ((Component)item).gameObject : null);
			if (gameObject != null && gameObject.activeInHierarchy && GetInventoryItemFromSlotObject(gameObject) != null)
			{
				list.Add(gameObject);
			}
		}

		list.Sort(delegate(GameObject left, GameObject right)
		{
			int num = right.transform.position.y.CompareTo(left.transform.position.y);
			return (num != 0) ? num : left.transform.position.x.CompareTo(right.transform.position.x);
		});
		return list.ToArray();
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
		if (SelectedEventOptionIndexOverride.HasValue && activeInventoryChoiceObjects.Length > 0)
		{
			return Math.Max(0, Math.Min(activeInventoryChoiceObjects.Length - 1, SelectedEventOptionIndexOverride.Value));
		}

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

	private static void SelectInventoryChoice(GameObject slotObject)
	{
		if (slotObject == null)
		{
			return;
		}

		Button inventoryChoiceButton = GetInventoryChoiceButton(slotObject);
		GameObject gameObject = ((inventoryChoiceButton != null) ? ((Component)inventoryChoiceButton).gameObject : slotObject);
		if (inventoryChoiceButton != null)
		{
			SafeCall(delegate
			{
				inventoryChoiceButton.Select();
				return true;
			}, fallback: false);
		}

		if (EventSystem.current != null && gameObject != null)
		{
			EventSystem.current.SetSelectedGameObject(gameObject);
		}
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

	private static EventChooseEntry[] GetVisibleEventChoices(EventTile currentEventTile, EventPopup activeEventPopup)
	{
		if (activeEventPopup == null && currentEventTile == null)
		{
			return Array.Empty<EventChooseEntry>();
		}

		List<EventChooseEntry> list = new List<EventChooseEntry>();
		AddVisibleEventChoice(list, SafeCall(() => activeEventPopup.GetEventChooseEntry(), null));
		AddVisibleEventChoice(list, SafeCall(() => activeEventPopup.GetEventChooseEntryAdditional(), null));
		AddVisibleEventChoice(list, SafeCall(() => currentEventTile.GetEventChooseEntry(), null));
		AddVisibleEventChoice(list, SafeCall(() => currentEventTile.GetEventChooseEntryAdditional(), null));
		return list.ToArray();
	}

	private static void AddVisibleEventChoice(List<EventChooseEntry> results, EventChooseEntry entry)
	{
		if (entry == null)
		{
			return;
		}

		foreach (EventChooseEntry result in results)
		{
			if (((Il2CppObjectBase)result).Pointer == ((Il2CppObjectBase)entry).Pointer)
			{
				return;
			}
		}

		results.Add(entry);
	}

	private static EventChooseEntry ResolveEventChoiceAt(EventChooseEntry[] visibleEventChoices, int index)
	{
		if (index < 0 || index >= visibleEventChoices.Length)
		{
			return null;
		}

		return visibleEventChoices[index];
	}

	private static bool HasActiveEventPopup(EventTile currentEventTile)
	{
		return GetActiveEventPopup(currentEventTile) != null;
	}

	private static EventPopup GetActiveEventPopup(EventTile currentEventTile)
	{
		if (currentEventTile == null)
		{
			return null;
		}

		EventPopup eventPopup = SafeCall(() => currentEventTile.GetEventPopup(), null);
		if (eventPopup == null)
		{
			return null;
		}

		GameObject gameObject = SafeCall(() => ((Component)eventPopup).gameObject, null);
		if (gameObject == null || !SafeCall(() => gameObject.activeInHierarchy, false))
		{
			return null;
		}

		return eventPopup;
	}

	private static EventPopup GetActiveGlobalEventPopup()
	{
		EventPopup eventPopup = SafeCall(() => UnityEngine.Object.FindObjectOfType<EventPopup>(), null);
		if (eventPopup == null)
		{
			return null;
		}

		GameObject gameObject = SafeCall(() => ((Component)eventPopup).gameObject, null);
		if (gameObject == null || !SafeCall(() => gameObject.activeInHierarchy, false))
		{
			return null;
		}

		return eventPopup;
	}

	private bool TryMove(PlayerController playerController, Vector2 direction, string actionId)
	{
		MapManager mapManager = UnityEngine.Object.FindObjectOfType<MapManager>();
		EventTile currentEventTile = GetCurrentEventTile(mapManager, playerController);
		EventPopup activeEventPopup = GetActiveEventPopup(currentEventTile) ?? GetActiveGlobalEventPopup();
		EventChooseEntry[] visibleEventChoices = GetVisibleEventChoices(currentEventTile, activeEventPopup);
		if (TryNavigateInventoryChoice(visibleEventChoices, direction, actionId))
		{
			return true;
		}

		if (activeEventPopup != null && TryNavigateEventChoice(currentEventTile, activeEventPopup, direction, actionId))
		{
			return true;
		}

		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = default(bool);
		BepInExInfoLogInterpolatedStringHandler val;
		if (playerController == null)
		{
			ManualLogSource log = _log;
			val = new BepInExInfoLogInterpolatedStringHandler(52, 1, out flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Skipping '");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionId);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("' because PlayerController is unavailable.");
			}
			log.LogInfo(val);
			return false;
		}
		if (playerController.isMapMoving || playerController.isPlayerMoving)
		{
			ManualLogSource log2 = _log;
			val = new BepInExInfoLogInterpolatedStringHandler(56, 1, out flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Skipping '");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionId);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("' because the player or map is already moving.");
			}
			log2.LogInfo(val);
			return false;
		}
		Vector2 playerGridPosition = playerController.GetPlayerGridPosition();
		Vector2 val2 = new Vector2(Mathf.Round(playerGridPosition.x), Mathf.Round(playerGridPosition.y));
		Vector2 val3 = new Vector2(Mathf.Round(direction.x), Mathf.Round(direction.y));
		Vector2 val4 = val2 + val3;
		if (!playerController.IsMoveAllowed(val2, val4))
		{
			ManualLogSource log3 = _log;
			val = new BepInExInfoLogInterpolatedStringHandler(50, 3, out flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Skipping '");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionId);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("' because move from ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<Vector2>(val2);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" to ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<Vector2>(val4);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" is not allowed.");
			}
			log3.LogInfo(val);
			return false;
		}
		playerController.MoveMapMovepoint(val3);
		playerController.MovePlayerMovepoint(val3);
		ManualLogSource log4 = _log;
		val = new BepInExInfoLogInterpolatedStringHandler(84, 4, out flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Executed '");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionId);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("' via PlayerController.MoveMapMovepoint(");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<Vector2>(val3);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral(") + MovePlayerMovepoint(");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<Vector2>(val3);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral(") toward ");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<Vector2>(val4);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral(".");
		}
		log4.LogInfo(val);
		return true;
	}

	private bool TryConfirm(PlayerControlsManager controlsManager, PlayerController playerController, string actionId)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		MapManager mapManager = UnityEngine.Object.FindObjectOfType<MapManager>();
		EventTile currentEventTile = GetCurrentEventTile(mapManager, playerController);
		EventPopup activeEventPopup = GetActiveEventPopup(currentEventTile) ?? GetActiveGlobalEventPopup();
		EventChooseEntry[] visibleEventChoices = GetVisibleEventChoices(currentEventTile, activeEventPopup);
		if (TryResolveCurrentInventoryChoice(controlsManager, visibleEventChoices, actionId))
		{
			return true;
		}

		if (activeEventPopup != null && TryResolveCurrentEventChoice(currentEventTile, activeEventPopup, actionId))
		{
			return true;
		}

		if (TryStartRunFromMenus(controlsManager, actionId))
		{
			return true;
		}
		ManualLogSource log = _log;
		bool flag = default(bool);
		BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(72, 1, out flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Skipping '");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionId);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("' because confirm is only intended for start/menu progression.");
		}
		log.LogInfo(val);
		return false;
	}

	private bool TryNavigateEventChoice(EventTile currentEventTile, EventPopup activeEventPopup, Vector2 direction, string actionId)
	{
		EventChooseEntry[] visibleEventChoices = GetVisibleEventChoices(currentEventTile, activeEventPopup);
		int eventChoiceCount = GetEventChoiceCount(currentEventTile, activeEventPopup, visibleEventChoices);
		if (eventChoiceCount <= 1)
		{
			return false;
		}

		int currentEventChoiceIndex = GetCurrentEventChoiceIndex(currentEventTile, activeEventPopup, visibleEventChoices);
		int num = (((direction.x > 0f || direction.y > 0f) ? 1 : (-1)) + currentEventChoiceIndex + eventChoiceCount) % eventChoiceCount;
		SelectedEventOptionIndexOverride = num;
		EventChooseEntry eventChooseEntry = ResolveEventChoiceAt(visibleEventChoices, num);
		SelectEventChoice(eventChooseEntry);
		_log.LogInfo((object)("Executed '" + actionId + "' by selecting event option index " + num.ToString(CultureInfo.InvariantCulture) + "."));
		return true;
	}

	private static void SelectEventChoice(EventChooseEntry entry)
	{
		if (entry == null)
		{
			return;
		}

		SafeCall(delegate
		{
			entry.OnItemSelect();
			return true;
		}, fallback: false);
		Button button = SafeCall(() => entry.GetButton(), null);
		if (button == null)
		{
			return;
		}

		SafeCall(delegate
		{
			button.Select();
			return true;
		}, fallback: false);
		GameObject gameObject = ((Component)button).gameObject;
		if (EventSystem.current != null && gameObject != null)
		{
			EventSystem.current.SetSelectedGameObject(gameObject);
		}
	}

	private bool TryStartRunFromMenus(PlayerControlsManager controlsManager, string actionId)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Expected O, but got Unknown
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		DifficultyToggle difficultyToggle = UnityEngine.Object.FindObjectOfType<DifficultyToggle>();
		if (TrySelectNormalDifficulty(difficultyToggle, actionId))
		{
			return true;
		}
		WaitingRoomDisplayer val = UnityEngine.Object.FindObjectOfType<WaitingRoomDisplayer>();
		bool flag = default(bool);
		if (val != null && ((Component)val).gameObject != null && ((Component)val).gameObject.activeInHierarchy)
		{
			val.GenerateNewgameClicked();
			val.OnStartGameClicked();
			ManualLogSource log = _log;
			BepInExInfoLogInterpolatedStringHandler val2 = new BepInExInfoLogInterpolatedStringHandler(85, 1, out flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Executed '");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(actionId);
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("' via WaitingRoomDisplayer.GenerateNewgameClicked() + OnStartGameClicked().");
			}
			log.LogInfo(val2);
			return true;
		}
		WorldsMenu val3 = UnityEngine.Object.FindObjectOfType<WorldsMenu>();
		if (val3 != null && ((Component)val3).gameObject != null && ((Component)val3).gameObject.activeInHierarchy)
		{
			val3.PushPlay(1);
			BepInExInfoLogInterpolatedStringHandler val2;
			if (TrySelectNormalDifficulty(val3.difficultyToggle, actionId))
			{
				ManualLogSource log2 = _log;
				val2 = new BepInExInfoLogInterpolatedStringHandler(87, 1, out flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Executed '");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(actionId);
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("' via WorldsMenu.PushPlay(WOODLAND) + DifficultyToggle.SetNormalDifficulty().");
				}
				log2.LogInfo(val2);
				return true;
			}
			ManualLogSource log3 = _log;
			val2 = new BepInExInfoLogInterpolatedStringHandler(46, 1, out flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Executed '");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(actionId);
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("' via WorldsMenu.PushPlay(WOODLAND).");
			}
			log3.LogInfo(val2);
			return true;
		}
		if (controlsManager != null)
		{
			controlsManager.OpenWithStartMenu();
			controlsManager.SetCurrentlyPlayingAreaWoodland();
			controlsManager.SetWorldsMenu(true);
			controlsManager.PressedSouthButton();
			ManualLogSource log4 = _log;
			BepInExInfoLogInterpolatedStringHandler val2 = new BepInExInfoLogInterpolatedStringHandler(61, 1, out flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Executed '");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(actionId);
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("' via PlayerControlsManager forced start-menu flow.");
			}
			log4.LogInfo(val2);
			return true;
		}
		return false;
	}

	private bool TrySelectNormalDifficulty(DifficultyToggle difficultyToggle, string actionId)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		if (difficultyToggle == null)
		{
			return false;
		}
		difficultyToggle.SetNormalDifficulty();
		ManualLogSource log = _log;
		bool flag = default(bool);
		BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(55, 1, out flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Executed '");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionId);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("' via DifficultyToggle.SetNormalDifficulty().");
		}
		log.LogInfo(val);
		return true;
	}

	private bool TryBattleAdvance(BattleSystem battleSystem, PlayerControlsManager controlsManager, string actionId)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		if (battleSystem != null)
		{
			battleSystem.RunTurn();
			ManualLogSource log = _log;
			bool flag = default(bool);
			BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(39, 1, out flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Executed '");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionId);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("' via BattleSystem.RunTurn().");
			}
			log.LogInfo(val);
			return true;
		}
		return TryInvoke(controlsManager, delegate(PlayerControlsManager controls)
		{
			controls.PressedSouthButton();
		}, actionId, "PressedSouthButton");
	}

	private bool TryInvoke<T>(T target, Action<T> callback, string actionId, string pathway) where T : class
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		bool flag = default(bool);
		BepInExInfoLogInterpolatedStringHandler val;
		if (target == null)
		{
			ManualLogSource log = _log;
			val = new BepInExInfoLogInterpolatedStringHandler(36, 2, out flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Skipping '");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionId);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("' because ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(typeof(T).Name);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" is unavailable.");
			}
			log.LogInfo(val);
			return false;
		}
		callback(target);
		ManualLogSource log2 = _log;
		val = new BepInExInfoLogInterpolatedStringHandler(18, 3, out flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Executed '");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionId);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("' via ");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(typeof(T).Name);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral(".");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(pathway);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral(".");
		}
		log2.LogInfo(val);
		return true;
	}
}

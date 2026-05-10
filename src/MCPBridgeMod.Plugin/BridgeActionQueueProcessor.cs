using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using MCPBridgeMod.Contracts;
using UnityEngine;

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
			BepInExErrorLogInterpolatedStringHandler val = new BepInExErrorLogInterpolatedStringHandler(43, 2, ref flag2);
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
				BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(53, 2, ref flag);
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
		StatsManager val = Object.FindObjectOfType<StatsManager>();
		PlayerController playerController = ((val != null) ? val.GetPlayerController() : null) ?? Object.FindObjectOfType<PlayerController>();
		PlayerControlsManager val2 = Object.FindObjectOfType<PlayerControlsManager>();
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
		case "refresh_state":
			_log.LogInfo((object)"Refreshing bridge state from queued request.");
			return true;
		default:
		{
			ManualLogSource log = _log;
			bool flag = default(bool);
			BepInExInfoLogInterpolatedStringHandler val3 = new BepInExInfoLogInterpolatedStringHandler(54, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val3).AppendLiteral("Queued bridge action '");
				((BepInExLogInterpolatedStringHandler)val3).AppendFormatted<string>(text);
				((BepInExLogInterpolatedStringHandler)val3).AppendLiteral("' is not handled in-process yet.");
			}
			log.LogInfo(val3);
			return false;
		}
		}
	}

	private bool TryMove(PlayerController playerController, Vector2 direction, string actionId)
	{
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
			val = new BepInExInfoLogInterpolatedStringHandler(52, 1, ref flag);
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
			val = new BepInExInfoLogInterpolatedStringHandler(56, 1, ref flag);
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
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(Mathf.Round(playerGridPosition.x), Mathf.Round(playerGridPosition.y));
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(Mathf.Round(direction.x), Mathf.Round(direction.y));
		Vector2 val4 = val2 + val3;
		if (!playerController.IsMoveAllowed(val2, val4))
		{
			ManualLogSource log3 = _log;
			val = new BepInExInfoLogInterpolatedStringHandler(50, 3, ref flag);
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
		val = new BepInExInfoLogInterpolatedStringHandler(84, 4, ref flag);
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
		if (TryStartRunFromMenus(controlsManager, actionId))
		{
			return true;
		}
		ManualLogSource log = _log;
		bool flag = default(bool);
		BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(72, 1, ref flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Skipping '");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(actionId);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("' because confirm is only intended for start/menu progression.");
		}
		log.LogInfo(val);
		return false;
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
		DifficultyToggle difficultyToggle = Object.FindObjectOfType<DifficultyToggle>();
		if (TrySelectNormalDifficulty(difficultyToggle, actionId))
		{
			return true;
		}
		WaitingRoomDisplayer val = Object.FindObjectOfType<WaitingRoomDisplayer>();
		bool flag = default(bool);
		if (val != null && ((Component)val).gameObject != null && ((Component)val).gameObject.activeInHierarchy)
		{
			val.GenerateNewgameClicked();
			val.OnStartGameClicked();
			ManualLogSource log = _log;
			BepInExInfoLogInterpolatedStringHandler val2 = new BepInExInfoLogInterpolatedStringHandler(85, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Executed '");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(actionId);
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("' via WaitingRoomDisplayer.GenerateNewgameClicked() + OnStartGameClicked().");
			}
			log.LogInfo(val2);
			return true;
		}
		WorldsMenu val3 = Object.FindObjectOfType<WorldsMenu>();
		if (val3 != null && ((Component)val3).gameObject != null && ((Component)val3).gameObject.activeInHierarchy)
		{
			val3.PushPlay(1);
			BepInExInfoLogInterpolatedStringHandler val2;
			if (TrySelectNormalDifficulty(val3.difficultyToggle, actionId))
			{
				ManualLogSource log2 = _log;
				val2 = new BepInExInfoLogInterpolatedStringHandler(87, 1, ref flag);
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
			val2 = new BepInExInfoLogInterpolatedStringHandler(46, 1, ref flag);
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
			BepInExInfoLogInterpolatedStringHandler val2 = new BepInExInfoLogInterpolatedStringHandler(61, 1, ref flag);
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
		BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(55, 1, ref flag);
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
			BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(39, 1, ref flag);
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
			val = new BepInExInfoLogInterpolatedStringHandler(36, 2, ref flag);
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
		val = new BepInExInfoLogInterpolatedStringHandler(18, 3, ref flag);
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

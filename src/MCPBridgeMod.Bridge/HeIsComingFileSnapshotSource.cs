using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Bridge;

public sealed class HeIsComingFileSnapshotSource : IGameSnapshotSource
{
	private readonly BridgeServiceOptions _options;

	public HeIsComingFileSnapshotSource(BridgeServiceOptions options)
	{
		_options = options;
	}

	public async Task<GameSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
	{
		string saveDataPath = Path.Combine(_options.SaveDirectory, "SaveData.txt");
		string settingsPath = Path.Combine(_options.SaveDirectory, "SettingsData.json");
		string playerLogPath = Path.Combine(_options.SaveDirectory, "Player.log");
		Dictionary<string, string?> metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			["saveDirectory"] = _options.SaveDirectory,
			["gameDirectory"] = _options.GameDirectory,
			["windowTitle"] = _options.WindowTitle,
			["inputExecutionEnabled"] = _options.EnableInputExecution.ToString()
		};
		IReadOnlyDictionary<string, string?> settingsData = await ReadSettingsAsync(settingsPath, cancellationToken);
		if (settingsData != null)
		{
			foreach (KeyValuePair<string, string> pair in settingsData)
			{
				metadata[pair.Key] = pair.Value;
			}
		}
		FileInfo saveInfo = new FileInfo(saveDataPath);
		if (saveInfo.Exists)
		{
			metadata["saveDataPresent"] = "true";
			metadata["saveDataSizeBytes"] = saveInfo.Length.ToString();
			metadata["saveDataLastWriteUtc"] = saveInfo.LastWriteTimeUtc.ToString("O");
		}
		else
		{
			metadata["saveDataPresent"] = "false";
		}
		string logTail = await ReadLastLinesAsync(playerLogPath, 80, cancellationToken);
		metadata["playerLogPresent"] = (!string.IsNullOrWhiteSpace(logTail)).ToString();
		metadata["lastLogEvent"] = FindLastInterestingLogLine(logTail);
		return new GameSnapshot(InferScreen(logTail, saveInfo.Exists), saveInfo.Exists ? $"save-{saveInfo.LastWriteTimeUtc:yyyyMMddHHmmss}" : "save-missing", diagnostics: new SnapshotDiagnostics("file-monitor", "Monitoring LocalLow save/settings/log files. Live IL2CPP hooks are not attached yet.", metadata), capturedAt: DateTimeOffset.UtcNow, player: new PlayerSnapshot(0, 0, 0, 0, 0), inventory: Array.Empty<InventoryItemSnapshot>(), availableActions: BridgeActionCatalog.CreateDefaultActions(), encounter: null, map: null);
	}

	private static async Task<IReadOnlyDictionary<string, string?>?> ReadSettingsAsync(string settingsPath, CancellationToken cancellationToken)
	{
		if (!File.Exists(settingsPath))
		{
			return null;
		}
		IReadOnlyDictionary<string, string?> result;
		await using (FileStream stream = File.OpenRead(settingsPath))
		{
			using JsonDocument document = await JsonDocument.ParseAsync(stream, default(JsonDocumentOptions), cancellationToken);
			Dictionary<string, string?> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (!document.RootElement.TryGetProperty("settingsManager", out var settingsManager))
			{
				result = values;
			}
			else
			{
				foreach (JsonProperty property in settingsManager.EnumerateObject())
				{
					values["settings." + property.Name] = property.Value.ToString();
				}
				result = values;
			}
		}
		return result;
	}

	private static async Task<string> ReadLastLinesAsync(string path, int maxLines, CancellationToken cancellationToken)
	{
		if (!File.Exists(path))
		{
			return string.Empty;
		}
		return string.Join(values: (await File.ReadAllLinesAsync(path, cancellationToken)).TakeLast(maxLines), separator: Environment.NewLine);
	}

	private static string InferScreen(string logTail, bool hasSaveData)
	{
		if (string.IsNullOrWhiteSpace(logTail))
		{
			return hasSaveData ? "menu-or-run" : "unavailable";
		}
		if (logTail.Contains("BattleManager:WinKingmaker", StringComparison.OrdinalIgnoreCase))
		{
			return "post-battle";
		}
		if (logTail.Contains("<CO_EndBattle>", StringComparison.OrdinalIgnoreCase))
		{
			return "battle-resolution";
		}
		if (logTail.Contains("GameEvents:OnSaveInitiated", StringComparison.OrdinalIgnoreCase))
		{
			return "save-transition";
		}
		return hasSaveData ? "menu-or-run" : "log-only";
	}

	private static string FindLastInterestingLogLine(string logTail)
	{
		if (string.IsNullOrWhiteSpace(logTail))
		{
			return "No Player.log found yet.";
		}
		foreach (string item in logTail.Split(Environment.NewLine).Reverse())
		{
			string text = item.Trim();
			if (text.Contains(':', StringComparison.Ordinal))
			{
				return (text.Length <= 220) ? text : text.Substring(0, 220);
			}
		}
		string text2 = logTail.Replace(Environment.NewLine, " ").Trim();
		return (text2.Length <= 220) ? text2 : text2.Substring(0, 220);
	}
}

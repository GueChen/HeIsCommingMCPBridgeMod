using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Bridge;

public sealed class BridgeCoordinator
{
	private readonly IGameSnapshotSource _snapshotSource;

	private readonly JsonArtifactStore _artifactStore;

	private readonly IActionDispatcher _queueDispatcher;

	private readonly IActionDispatcher _inputDispatcher;

	public BridgeCoordinator(IGameSnapshotSource snapshotSource, JsonArtifactStore artifactStore, IActionDispatcher queueDispatcher, IActionDispatcher inputDispatcher)
	{
		_snapshotSource = snapshotSource;
		_artifactStore = artifactStore;
		_queueDispatcher = queueDispatcher;
		_inputDispatcher = inputDispatcher;
	}

	public async Task<BridgeHandshake> GetHandshakeAsync(CancellationToken cancellationToken)
	{
		BridgeHandshake handshake = new BridgeHandshake("gue.heiscomming.mcpbridge", "0.1.0", "He is coming", "BepInEx 6", new string[5] { "bridge_get_handshake", "bridge_get_snapshot", "bridge_get_catalog", "bridge_list_actions", "bridge_execute_action" });
		await _artifactStore.WriteHandshakeAsync(handshake, cancellationToken);
		return handshake;
	}

	public async Task<GameSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
	{
		GameSnapshot snapshot = await _snapshotSource.GetSnapshotAsync(cancellationToken);
		await _artifactStore.WriteSnapshotAsync(snapshot, cancellationToken);
		return snapshot;
	}

	public async Task<GameCatalog> GetCatalogAsync(CancellationToken cancellationToken)
	{
		GameCatalog catalog = await _artifactStore.ReadCatalogAsync(cancellationToken);
		if (catalog != null)
		{
			return catalog;
		}
		return new GameCatalog(DateTimeOffset.UtcNow, null, Array.Empty<CatalogItem>(), Array.Empty<CatalogMonster>(), Array.Empty<CatalogMap>(), new SnapshotDiagnostics("catalog-missing", "catalog.json has not been produced by the live plugin yet.", new Dictionary<string, string> { ["artifact"] = "catalog.json" }));
	}

	public async Task<IReadOnlyList<ActionDescriptor>> ListActionsAsync(CancellationToken cancellationToken)
	{
		return (await GetSnapshotAsync(cancellationToken)).AvailableActions;
	}

	public async Task<ActionExecutionResult> ExecuteActionAsync(string actionId, IReadOnlyDictionary<string, string?> parameters, CancellationToken cancellationToken)
	{
		if (string.Equals(actionId, "refresh_state", StringComparison.OrdinalIgnoreCase))
		{
			await GetSnapshotAsync(cancellationToken);
			return new ActionExecutionResult(actionId, "executed", queued: false, executed: true, "Snapshot refreshed and persisted to snapshot.json.");
		}
		ActionDescriptor matchedAction = (await ListActionsAsync(cancellationToken)).FirstOrDefault((ActionDescriptor action) => string.Equals(action.ActionId, actionId, StringComparison.OrdinalIgnoreCase));
		if (matchedAction == null)
		{
			return new ActionExecutionResult(actionId, "unknown-action", queued: false, executed: false, "Action is not part of the current bridge action catalog.");
		}
		if (!matchedAction.IsEnabled)
		{
			return new ActionExecutionResult(actionId, "disabled-action", queued: false, executed: false, matchedAction.DisabledReason ?? "Action is not enabled in the current game state.");
		}
		ActionExecutionRequest request = new ActionExecutionRequest(actionId, DateTimeOffset.UtcNow, parameters);
		ActionExecutionResult queuedResult = await _queueDispatcher.ExecuteAsync(request, cancellationToken);
		ActionExecutionResult executionResult = await _inputDispatcher.ExecuteAsync(request, cancellationToken);
		if (executionResult.Executed)
		{
			return executionResult;
		}
		if (queuedResult.Queued)
		{
			return queuedResult;
		}
		return new ActionExecutionResult(actionId, queuedResult.Status, queuedResult.Queued, executionResult.Executed, (queuedResult.Message + " " + executionResult.Message).Trim());
	}
}

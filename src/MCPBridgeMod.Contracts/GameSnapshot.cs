using System;
using System.Collections.Generic;

namespace MCPBridgeMod.Contracts;

public sealed class GameSnapshot
{
	public string Screen { get; }

	public string RunId { get; }

	public DateTimeOffset CapturedAt { get; }

	public PlayerSnapshot Player { get; }

	public IReadOnlyList<InventoryItemSnapshot> Inventory { get; }

	public IReadOnlyList<ActionDescriptor> AvailableActions { get; }

	public EncounterSnapshot? Encounter { get; }

	public MapSnapshot? Map { get; }

	public SnapshotDiagnostics Diagnostics { get; }

	public GameSnapshot(string screen, string runId, DateTimeOffset capturedAt, PlayerSnapshot player, IReadOnlyList<InventoryItemSnapshot> inventory, IReadOnlyList<ActionDescriptor> availableActions, EncounterSnapshot? encounter, MapSnapshot? map, SnapshotDiagnostics diagnostics)
	{
		Screen = screen;
		RunId = runId;
		CapturedAt = capturedAt;
		Player = player;
		Inventory = inventory;
		AvailableActions = availableActions;
		Encounter = encounter;
		Map = map;
		Diagnostics = diagnostics;
	}
}

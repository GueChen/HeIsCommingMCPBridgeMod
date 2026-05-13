using System;
using System.Collections.Generic;
using System.Linq;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Plugin;

public sealed class BridgeScaffold
{
	public BridgeHandshake CreateHandshake()
	{
		return new BridgeHandshake("gue.heiscomming.mcpbridge", "0.1.0", "He is coming", "BepInEx 6", new string[5] { "bridge_get_handshake", "bridge_get_snapshot", "bridge_get_catalog", "bridge_list_actions", "bridge_execute_action" });
	}

	public IReadOnlyList<ActionDescriptor> CreateDefaultActions()
	{
		return CreateActionsForScreen("bootstrap", null);
	}

	public IReadOnlyList<ActionDescriptor> CreateActionsForScreen(string screen, MapSnapshot mapSnapshot)
	{
		bool flag = !string.IsNullOrWhiteSpace(screen) && screen.StartsWith("menu-", StringComparison.OrdinalIgnoreCase);
		bool flag2 = string.Equals(screen, "live-overworld", StringComparison.OrdinalIgnoreCase);
		bool flag3 = string.Equals(screen, "live-event", StringComparison.OrdinalIgnoreCase);
		bool enabled = string.Equals(screen, "live-battle", StringComparison.OrdinalIgnoreCase);
		MapNodeSnapshot currentNode = GetCurrentNode(mapSnapshot);
		bool canInteract = (flag2 && IsInteractableNode(currentNode)) || flag3;
		return new ActionDescriptor[14]
		{
			Create("confirm", flag3 ? "Choose selected event option" : "Advance start/menu", flag || flag3, flag3 ? "Only available while resolving an event popup." : "Only used for start and menu progression."),
			Create("cancel", flag3 ? "Back out of current event" : "Cancel / back", flag || flag3, flag3 ? "Only available while resolving an event popup." : "Only used for menu navigation."),
			Create("move_up", flag2 ? "Walk up" : (flag3 ? "Select previous event option" : "Move selection up"), flag || flag2 || flag3, "Only used for menu navigation, event selection, or overworld movement."),
			Create("move_down", flag2 ? "Walk down" : (flag3 ? "Select next event option" : "Move selection down"), flag || flag2 || flag3, "Only used for menu navigation, event selection, or overworld movement."),
			Create("move_left", flag2 ? "Walk left" : (flag3 ? "Select previous event option" : "Move selection left"), flag || flag2 || flag3, "Only used for menu navigation, event selection, or overworld movement."),
			Create("move_right", flag2 ? "Walk right" : (flag3 ? "Select next event option" : "Move selection right"), flag || flag2 || flag3, "Only used for menu navigation, event selection, or overworld movement."),
			Create("attack", "Perform default attack / continue battle", enabled, "Only available during battle."),
			Create("interact", BuildInteractLabel(currentNode, screen), canInteract, flag3 ? "Only available while resolving an event popup." : (flag2 ? "Only available while standing on an interactable overworld node." : "Only available during overworld exploration.")),
			Create("open_map", "Open map", flag2, "Only available during overworld exploration."),
			Create("close_map", "Close map", flag2, "Only available during overworld exploration."),
			Create("end_turn", "End current turn", enabled, "Only available during battle."),
			Create("reroll_shop", "Reroll shop", enabled: false, "Shop actions are not wired yet."),
			Create("buy_selected", "Buy selected item", enabled: false, "Shop actions are not wired yet."),
			new ActionDescriptor("refresh_state", "Refresh captured state", isEnabled: true, null)
		};
	}

	private static MapNodeSnapshot GetCurrentNode(MapSnapshot mapSnapshot)
	{
		if (mapSnapshot == null || string.IsNullOrWhiteSpace(mapSnapshot.CurrentNodeId))
		{
			return null;
		}

		return mapSnapshot.Nodes.FirstOrDefault(node => string.Equals(node.NodeId, mapSnapshot.CurrentNodeId, StringComparison.Ordinal));
	}

	private static bool IsInteractableNode(MapNodeSnapshot node)
	{
		if (node == null)
		{
			return false;
		}

		return !string.Equals(node.OccupantCategory, "none", StringComparison.OrdinalIgnoreCase)
			&& !string.Equals(node.OccupantCategory, "monster", StringComparison.OrdinalIgnoreCase);
	}

	private static string BuildInteractLabel(MapNodeSnapshot node, string screen)
	{
		if (string.Equals(screen, "live-event", StringComparison.OrdinalIgnoreCase))
		{
			return "Choose primary event option";
		}

		if (node == null)
		{
			return "Interact with current node";
		}

		return node.OccupantCategory switch
		{
			"chest" => "Open current chest",
			"shop" => "Enter current shop",
			"campfire" => "Use current campfire",
			"home" => "Use current home",
			"fortune_teller" => "Use current fortune teller",
			"waypoint" => "Use current waypoint",
			"travel" => "Use current travel node",
			"event" => "Interact with current event",
			_ => "Interact with current node"
		};
	}

	public GameSnapshot CreateBootstrapSnapshot(BridgeRuntimeOptions runtimeOptions)
	{
		return new GameSnapshot("bootstrap", "unattached", DateTimeOffset.UtcNow, new PlayerSnapshot(0, 0, 0, 0, 0), Array.Empty<InventoryItemSnapshot>(), CreateDefaultActions(), null, null, new SnapshotDiagnostics("bootstrap", "Plugin is loaded and waiting for the live scene to expose structured runtime data.", new Dictionary<string, string>
		{
			["artifactRoot"] = runtimeOptions.ArtifactRoot,
			["verboseLogging"] = runtimeOptions.VerboseLogging.ToString()
		}));
	}

	public GameCatalog CreateBootstrapCatalog(BridgeRuntimeOptions runtimeOptions)
	{
		return new GameCatalog(DateTimeOffset.UtcNow, null, Array.Empty<CatalogItem>(), Array.Empty<CatalogMonster>(), Array.Empty<CatalogMap>(), new SnapshotDiagnostics("bootstrap", "Plugin is loaded but no live catalog has been captured yet.", new Dictionary<string, string>
		{
			["artifactRoot"] = runtimeOptions.ArtifactRoot,
			["verboseLogging"] = runtimeOptions.VerboseLogging.ToString()
		}));
	}

	private static ActionDescriptor Create(string actionId, string label, bool enabled, string disabledReason)
	{
		return new ActionDescriptor(actionId, label, enabled, enabled ? null : disabledReason);
	}
}

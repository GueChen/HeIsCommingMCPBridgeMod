using System;
using System.Collections.Generic;
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
		return CreateActionsForScreen("bootstrap");
	}

	public IReadOnlyList<ActionDescriptor> CreateActionsForScreen(string screen)
	{
		bool flag = !string.IsNullOrWhiteSpace(screen) && screen.StartsWith("menu-", StringComparison.OrdinalIgnoreCase);
		bool flag2 = string.Equals(screen, "live-overworld", StringComparison.OrdinalIgnoreCase);
		bool enabled = string.Equals(screen, "live-battle", StringComparison.OrdinalIgnoreCase);
		return new ActionDescriptor[13]
		{
			Create("confirm", "Advance start/menu", flag, "Only used for start and menu progression."),
			Create("cancel", "Cancel / back", flag, "Only used for menu navigation."),
			Create("move_up", flag2 ? "Walk up" : "Move selection up", flag || flag2, "Only used for menu navigation or overworld movement."),
			Create("move_down", flag2 ? "Walk down" : "Move selection down", flag || flag2, "Only used for menu navigation or overworld movement."),
			Create("move_left", flag2 ? "Walk left" : "Move selection left", flag || flag2, "Only used for menu navigation or overworld movement."),
			Create("move_right", flag2 ? "Walk right" : "Move selection right", flag || flag2, "Only used for menu navigation or overworld movement."),
			Create("attack", "Perform default attack / continue battle", enabled, "Only available during battle."),
			Create("open_map", "Open map", flag2, "Only available during overworld exploration."),
			Create("close_map", "Close map", flag2, "Only available during overworld exploration."),
			Create("end_turn", "End current turn", enabled, "Only available during battle."),
			Create("reroll_shop", "Reroll shop", enabled: false, "Shop actions are not wired yet."),
			Create("buy_selected", "Buy selected item", enabled: false, "Shop actions are not wired yet."),
			new ActionDescriptor("refresh_state", "Refresh captured state", isEnabled: true, null)
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

using System.Collections.Generic;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Bridge;

public static class BridgeActionCatalog
{
	public static IReadOnlyList<ActionDescriptor> CreateDefaultActions()
	{
		return new ActionDescriptor[12]
		{
			Create("confirm", "Advance start/menu"),
			Create("cancel", "Cancel / back"),
			Create("move_up", "Move selection up"),
			Create("move_down", "Move selection down"),
			Create("move_left", "Move selection left"),
			Create("move_right", "Move selection right"),
			Create("attack", "Perform default attack / continue battle"),
			Create("open_map", "Open map"),
			Create("close_map", "Close map"),
			Create("end_turn", "End current turn"),
			Create("reroll_shop", "Reroll shop"),
			Create("buy_selected", "Buy selected item")
		};
	}

	private static ActionDescriptor Create(string actionId, string label)
	{
		return new ActionDescriptor(actionId, label, isEnabled: true, null);
	}
}

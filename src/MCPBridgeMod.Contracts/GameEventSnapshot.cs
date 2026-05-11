using System.Collections.Generic;

namespace MCPBridgeMod.Contracts;

public sealed class GameEventSnapshot
{
	public string Category { get; }

	public string Title { get; }

	public string Description { get; }

	public int? SelectedOptionIndex { get; }

	public IReadOnlyList<EventChoiceSnapshot> Options { get; }

	public InventoryStateSnapshot InventoryState { get; }

	public GameEventSnapshot(string category, string title, string description, int? selectedOptionIndex, IReadOnlyList<EventChoiceSnapshot> options, InventoryStateSnapshot inventoryState)
	{
		Category = category;
		Title = title;
		Description = description;
		SelectedOptionIndex = selectedOptionIndex;
		Options = options;
		InventoryState = inventoryState;
	}
}

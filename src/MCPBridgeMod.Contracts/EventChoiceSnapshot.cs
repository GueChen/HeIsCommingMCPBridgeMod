namespace MCPBridgeMod.Contracts;

public sealed class EventChoiceSnapshot
{
	public string OptionId { get; }

	public int Index { get; }

	public string Label { get; }

	public string Description { get; }

	public bool IsEnabled { get; }

	public bool IsSelected { get; }

	public CatalogItem? Item { get; }

	public EventItemComparisonSnapshot? ItemComparison { get; }

	public EventChoiceSnapshot(string optionId, int index, string label, string description, bool isEnabled, bool isSelected, CatalogItem? item, EventItemComparisonSnapshot? itemComparison)
	{
		OptionId = optionId;
		Index = index;
		Label = label;
		Description = description;
		IsEnabled = isEnabled;
		IsSelected = isSelected;
		Item = item;
		ItemComparison = itemComparison;
	}
}

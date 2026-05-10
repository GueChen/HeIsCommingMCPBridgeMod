namespace MCPBridgeMod.Contracts;

public sealed class InventoryItemSnapshot
{
	public string ItemId { get; }

	public string DisplayName { get; }

	public int Quantity { get; }

	public InventoryItemSnapshot(string itemId, string displayName, int quantity)
	{
		ItemId = itemId;
		DisplayName = displayName;
		Quantity = quantity;
	}
}

namespace MCPBridgeMod.Contracts;

public sealed class EventItemComparisonSnapshot
{
	public bool HasSameItemInInventory { get; }

	public bool HasSameItemInBackpack { get; }

	public int MatchingInventoryCount { get; }

	public int MatchingBackpackCount { get; }

	public bool HasFreeInventorySlot { get; }

	public bool HasFreeBackpackSlot { get; }

	public bool HasAnyFreeSlot { get; }

	public EventItemComparisonSnapshot(bool hasSameItemInInventory, bool hasSameItemInBackpack, int matchingInventoryCount, int matchingBackpackCount, bool hasFreeInventorySlot, bool hasFreeBackpackSlot, bool hasAnyFreeSlot)
	{
		HasSameItemInInventory = hasSameItemInInventory;
		HasSameItemInBackpack = hasSameItemInBackpack;
		MatchingInventoryCount = matchingInventoryCount;
		MatchingBackpackCount = matchingBackpackCount;
		HasFreeInventorySlot = hasFreeInventorySlot;
		HasFreeBackpackSlot = hasFreeBackpackSlot;
		HasAnyFreeSlot = hasAnyFreeSlot;
	}
}

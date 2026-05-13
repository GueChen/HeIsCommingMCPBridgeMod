namespace MCPBridgeMod.Contracts;

public sealed class EventItemComparisonSnapshot
{
	public bool HasSameItemInInventory { get; }

	public bool HasSameItemInBackpack { get; }

	public int MatchingInventoryCount { get; }

	public int MatchingBackpackCount { get; }

	public int MatchingExactInventoryCount { get; }

	public int MatchingExactBackpackCount { get; }

	public int MatchingExactTotalCount { get; }

	public bool HasUpgradeablePair { get; }

	public bool HasFreeInventorySlot { get; }

	public bool HasFreeBackpackSlot { get; }

	public bool HasAnyFreeSlot { get; }

	public EventItemComparisonSnapshot(bool hasSameItemInInventory, bool hasSameItemInBackpack, int matchingInventoryCount, int matchingBackpackCount, int matchingExactInventoryCount, int matchingExactBackpackCount, int matchingExactTotalCount, bool hasUpgradeablePair, bool hasFreeInventorySlot, bool hasFreeBackpackSlot, bool hasAnyFreeSlot)
	{
		HasSameItemInInventory = hasSameItemInInventory;
		HasSameItemInBackpack = hasSameItemInBackpack;
		MatchingInventoryCount = matchingInventoryCount;
		MatchingBackpackCount = matchingBackpackCount;
		MatchingExactInventoryCount = matchingExactInventoryCount;
		MatchingExactBackpackCount = matchingExactBackpackCount;
		MatchingExactTotalCount = matchingExactTotalCount;
		HasUpgradeablePair = hasUpgradeablePair;
		HasFreeInventorySlot = hasFreeInventorySlot;
		HasFreeBackpackSlot = hasFreeBackpackSlot;
		HasAnyFreeSlot = hasAnyFreeSlot;
	}
}

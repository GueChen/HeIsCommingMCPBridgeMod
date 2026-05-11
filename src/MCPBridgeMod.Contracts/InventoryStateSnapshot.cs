using System.Collections.Generic;

namespace MCPBridgeMod.Contracts;

public sealed class InventoryStateSnapshot
{
	public int InventoryItemCount { get; }

	public int InventorySlotCount { get; }

	public int OpenInventorySlots { get; }

	public int BackpackItemCount { get; }

	public int BackpackSlotCount { get; }

	public int OpenBackpackSlots { get; }

	public IReadOnlyList<InventoryItemSnapshot> BackpackItems { get; }

	public InventoryStateSnapshot(int inventoryItemCount, int inventorySlotCount, int openInventorySlots, int backpackItemCount, int backpackSlotCount, int openBackpackSlots, IReadOnlyList<InventoryItemSnapshot> backpackItems)
	{
		InventoryItemCount = inventoryItemCount;
		InventorySlotCount = inventorySlotCount;
		OpenInventorySlots = openInventorySlots;
		BackpackItemCount = backpackItemCount;
		BackpackSlotCount = backpackSlotCount;
		OpenBackpackSlots = openBackpackSlots;
		BackpackItems = backpackItems;
	}
}

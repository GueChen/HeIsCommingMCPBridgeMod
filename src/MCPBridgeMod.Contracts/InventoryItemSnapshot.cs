using System;
using System.Collections.Generic;

namespace MCPBridgeMod.Contracts;

public sealed class InventoryItemSnapshot
{
	public string ItemId { get; }

	public string DisplayName { get; }

	public int Quantity { get; }

	public string Container { get; }

	public int SlotIndex { get; }

	public string Description { get; }

	public string ItemType { get; }

	public string Rarity { get; }

	public int Attack { get; }

	public int Armor { get; }

	public int Speed { get; }

	public int MaxHealth { get; }

	public IReadOnlyList<string> Tags { get; }

	public InventoryItemSnapshot(string itemId, string displayName, int quantity, string container = "inventory", int slotIndex = -1, string description = "", string itemType = "unknown", string rarity = "unknown", int attack = 0, int armor = 0, int speed = 0, int maxHealth = 0, IReadOnlyList<string>? tags = null)
	{
		ItemId = itemId;
		DisplayName = displayName;
		Quantity = quantity;
		Container = container;
		SlotIndex = slotIndex;
		Description = description ?? string.Empty;
		ItemType = itemType ?? "unknown";
		Rarity = rarity ?? "unknown";
		Attack = attack;
		Armor = armor;
		Speed = speed;
		MaxHealth = maxHealth;
		Tags = tags ?? Array.Empty<string>();
	}
}

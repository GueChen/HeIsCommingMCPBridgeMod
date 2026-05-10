using System.Collections.Generic;

namespace MCPBridgeMod.Contracts;

public sealed class CatalogItem
{
	public string ItemId { get; }

	public string DisplayName { get; }

	public string Description { get; }

	public string ItemType { get; }

	public string Rarity { get; }

	public int Attack { get; }

	public int Armor { get; }

	public int Speed { get; }

	public int MaxHealth { get; }

	public int SpawnWeight { get; }

	public IReadOnlyList<string> Tags { get; }

	public CatalogItem(string itemId, string displayName, string description, string itemType, string rarity, int attack, int armor, int speed, int maxHealth, int spawnWeight, IReadOnlyList<string> tags)
	{
		ItemId = itemId;
		DisplayName = displayName;
		Description = description;
		ItemType = itemType;
		Rarity = rarity;
		Attack = attack;
		Armor = armor;
		Speed = speed;
		MaxHealth = maxHealth;
		SpawnWeight = spawnWeight;
		Tags = tags;
	}
}

namespace MCPBridgeMod.Contracts;

public sealed class CatalogMonster
{
	public string MonsterId { get; }

	public string DisplayName { get; }

	public string Description { get; }

	public int Level { get; }

	public int Health { get; }

	public int MaxHealth { get; }

	public int Attack { get; }

	public int Armor { get; }

	public int Speed { get; }

	public int Gold { get; }

	public int Bones { get; }

	public string? AdditionalItemId { get; }

	public string Source { get; }

	public CatalogMonster(string monsterId, string displayName, string description, int level, int health, int maxHealth, int attack, int armor, int speed, int gold, int bones, string? additionalItemId, string source)
	{
		MonsterId = monsterId;
		DisplayName = displayName;
		Description = description;
		Level = level;
		Health = health;
		MaxHealth = maxHealth;
		Attack = attack;
		Armor = armor;
		Speed = speed;
		Gold = gold;
		Bones = bones;
		AdditionalItemId = additionalItemId;
		Source = source;
	}
}

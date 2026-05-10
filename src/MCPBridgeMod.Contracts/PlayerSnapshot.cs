namespace MCPBridgeMod.Contracts;

public sealed class PlayerSnapshot
{
	public int Health { get; }

	public int MaxHealth { get; }

	public int Armor { get; }

	public int Gold { get; }

	public int Level { get; }

	public PlayerSnapshot(int health, int maxHealth, int armor, int gold, int level)
	{
		Health = health;
		MaxHealth = maxHealth;
		Armor = armor;
		Gold = gold;
		Level = level;
	}
}

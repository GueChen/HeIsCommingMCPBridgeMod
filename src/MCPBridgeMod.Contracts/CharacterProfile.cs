namespace MCPBridgeMod.Contracts;

public sealed class CharacterProfile
{
	public string DisplayName { get; }

	public int Health { get; }

	public int Attack { get; }

	public int Armor { get; }

	public int Speed { get; }

	public int Gold { get; }

	public string Position { get; }

	public int InventoryCount { get; }

	public int BackpackCount { get; }

	public int OpenInventorySlots { get; }

	public string? CurrentBoss { get; }

	public int CurrentBossNumber { get; }

	public CharacterProfile(string displayName, int health, int attack, int armor, int speed, int gold, string position, int inventoryCount, int backpackCount, int openInventorySlots, string? currentBoss, int currentBossNumber)
	{
		DisplayName = displayName;
		Health = health;
		Attack = attack;
		Armor = armor;
		Speed = speed;
		Gold = gold;
		Position = position;
		InventoryCount = inventoryCount;
		BackpackCount = backpackCount;
		OpenInventorySlots = openInventorySlots;
		CurrentBoss = currentBoss;
		CurrentBossNumber = currentBossNumber;
	}
}

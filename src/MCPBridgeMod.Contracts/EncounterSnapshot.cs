namespace MCPBridgeMod.Contracts;

public sealed class EncounterSnapshot
{
	public string EncounterId { get; }

	public string Title { get; }

	public int TurnNumber { get; }

	public int BossNumber { get; }

	public string CurrentTurn { get; }

	public string BattlePhase { get; }

	public bool? IsPaused { get; }

	public int? PlayerHealth { get; }

	public int? PlayerStartHealth { get; }

	public int? EnemyHealth { get; }

	public int? EnemyMaxHealth { get; }

	public EncounterSnapshot(string encounterId, string title, int turnNumber, int bossNumber, string currentTurn, string battlePhase, bool? isPaused, int? playerHealth, int? playerStartHealth, int? enemyHealth, int? enemyMaxHealth)
	{
		EncounterId = encounterId;
		Title = title;
		TurnNumber = turnNumber;
		BossNumber = bossNumber;
		CurrentTurn = currentTurn;
		BattlePhase = battlePhase;
		IsPaused = isPaused;
		PlayerHealth = playerHealth;
		PlayerStartHealth = playerStartHealth;
		EnemyHealth = enemyHealth;
		EnemyMaxHealth = enemyMaxHealth;
	}
}

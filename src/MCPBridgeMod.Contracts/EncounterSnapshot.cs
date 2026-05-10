namespace MCPBridgeMod.Contracts;

public sealed class EncounterSnapshot
{
	public string EncounterId { get; }

	public string Title { get; }

	public int TurnNumber { get; }

	public EncounterSnapshot(string encounterId, string title, int turnNumber)
	{
		EncounterId = encounterId;
		Title = title;
		TurnNumber = turnNumber;
	}
}

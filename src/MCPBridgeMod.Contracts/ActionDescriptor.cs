namespace MCPBridgeMod.Contracts;

public sealed class ActionDescriptor
{
	public string ActionId { get; }

	public string Label { get; }

	public bool IsEnabled { get; }

	public string? DisabledReason { get; }

	public ActionDescriptor(string actionId, string label, bool isEnabled, string? disabledReason)
	{
		ActionId = actionId;
		Label = label;
		IsEnabled = isEnabled;
		DisabledReason = disabledReason;
	}
}

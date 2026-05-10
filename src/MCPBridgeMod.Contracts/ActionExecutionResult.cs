namespace MCPBridgeMod.Contracts;

public sealed class ActionExecutionResult
{
	public string ActionId { get; }

	public string Status { get; }

	public bool Queued { get; }

	public bool Executed { get; }

	public string Message { get; }

	public ActionExecutionResult(string actionId, string status, bool queued, bool executed, string message)
	{
		ActionId = actionId;
		Status = status;
		Queued = queued;
		Executed = executed;
		Message = message;
	}
}

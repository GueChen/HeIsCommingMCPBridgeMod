using System;
using System.Collections.Generic;

namespace MCPBridgeMod.Contracts;

public sealed class ActionExecutionRequest
{
	public string ActionId { get; }

	public DateTimeOffset RequestedAt { get; }

	public IReadOnlyDictionary<string, string?> Parameters { get; }

	public ActionExecutionRequest(string actionId, DateTimeOffset requestedAt, IReadOnlyDictionary<string, string?> parameters)
	{
		ActionId = actionId;
		RequestedAt = requestedAt;
		Parameters = parameters;
	}
}

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Bridge;

public sealed class FileActionDispatcher : IActionDispatcher
{
	private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly BridgeArtifacts _artifacts;

	private readonly SemaphoreSlim _queueLock = new SemaphoreSlim(1, 1);

	public FileActionDispatcher(BridgeArtifacts artifacts)
	{
		_artifacts = artifacts;
	}

	public async Task<ActionExecutionResult> ExecuteAsync(ActionExecutionRequest request, CancellationToken cancellationToken)
	{
		string payload = JsonSerializer.Serialize(request, SerializerOptions);
		await _queueLock.WaitAsync(cancellationToken);
		try
		{
			await File.AppendAllTextAsync(_artifacts.ActionQueuePath, payload + Environment.NewLine, cancellationToken);
		}
		finally
		{
			_queueLock.Release();
		}
		return new ActionExecutionResult(request.ActionId, "queued", queued: true, executed: false, "Action queued to action-queue.jsonl for the mod or companion executor.");
	}
}

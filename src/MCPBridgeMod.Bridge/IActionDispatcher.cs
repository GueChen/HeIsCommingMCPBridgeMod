using System.Threading;
using System.Threading.Tasks;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Bridge;

public interface IActionDispatcher
{
	Task<ActionExecutionResult> ExecuteAsync(ActionExecutionRequest request, CancellationToken cancellationToken);
}

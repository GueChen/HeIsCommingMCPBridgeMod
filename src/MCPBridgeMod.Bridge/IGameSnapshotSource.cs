using System.Threading;
using System.Threading.Tasks;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Bridge;

public interface IGameSnapshotSource
{
	Task<GameSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}

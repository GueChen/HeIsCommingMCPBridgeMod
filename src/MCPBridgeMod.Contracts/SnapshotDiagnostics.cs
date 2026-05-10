using System.Collections.Generic;

namespace MCPBridgeMod.Contracts;

public sealed class SnapshotDiagnostics
{
	public string SourceMode { get; }

	public string Summary { get; }

	public IReadOnlyDictionary<string, string?> Metadata { get; }

	public SnapshotDiagnostics(string sourceMode, string summary, IReadOnlyDictionary<string, string?> metadata)
	{
		SourceMode = sourceMode;
		Summary = summary;
		Metadata = metadata;
	}
}

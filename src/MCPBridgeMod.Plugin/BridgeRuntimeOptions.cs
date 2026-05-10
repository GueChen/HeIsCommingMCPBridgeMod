using System;
using System.IO;

namespace MCPBridgeMod.Plugin;

public sealed class BridgeRuntimeOptions
{
	public string ArtifactRoot { get; }

	public bool VerboseLogging { get; }

	public BridgeRuntimeOptions(string artifactRoot, bool verboseLogging)
	{
		ArtifactRoot = artifactRoot;
		VerboseLogging = verboseLogging;
	}

	public static BridgeRuntimeOptions CreateDefault()
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		string fullPath = Path.GetFullPath(Path.Combine(new string[6] { folderPath, "..", "LocalLow", "Chronocle", "He Is Coming", "MCPBridge" }));
		return new BridgeRuntimeOptions(fullPath, verboseLogging: false);
	}
}

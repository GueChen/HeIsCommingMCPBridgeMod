using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace MCPBridgeMod.Bridge;

public sealed class BridgeServiceOptions
{
	public string BridgeRoot { get; }

	public string SaveDirectory { get; }

	public string GameDirectory { get; }

	public string WindowTitle { get; }

	public bool EnableInputExecution { get; }

	public BridgeServiceOptions(string bridgeRoot, string saveDirectory, string gameDirectory, string windowTitle, bool enableInputExecution)
	{
		BridgeRoot = bridgeRoot;
		SaveDirectory = saveDirectory;
		GameDirectory = gameDirectory;
		WindowTitle = windowTitle;
		EnableInputExecution = enableInputExecution;
	}

	public static BridgeServiceOptions CreateDefault(string workingDirectory)
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		InlineArray5<string> buffer = default(InlineArray5<string>);
		buffer[0] = folderPath;
		buffer[1] = "..";
		buffer[2] = "LocalLow";
		buffer[3] = "Chronocle";
		buffer[4] = "He Is Coming";
		string fullPath = Path.GetFullPath(Path.Combine(buffer));
		return new BridgeServiceOptions(Path.Combine(workingDirectory, ".bridge"), fullPath, "E:\\SteamLibrary\\steamapps\\common\\He is coming", "He is coming", enableInputExecution: false);
	}
}

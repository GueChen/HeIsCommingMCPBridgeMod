using System;
using MCPBridgeMod.Bridge;

namespace MCPBridgeMod.Server;

public sealed class ServerCommandLineOptions
{
	public string WorkingDirectory { get; }

	public string? BridgeRoot { get; }

	public string? SaveDirectory { get; }

	public string? GameDirectory { get; }

	public string? WindowTitle { get; }

	public bool EnableInputExecution { get; }

	private ServerCommandLineOptions(string workingDirectory, string? bridgeRoot, string? saveDirectory, string? gameDirectory, string? windowTitle, bool enableInputExecution)
	{
		WorkingDirectory = workingDirectory;
		BridgeRoot = bridgeRoot;
		SaveDirectory = saveDirectory;
		GameDirectory = gameDirectory;
		WindowTitle = windowTitle;
		EnableInputExecution = enableInputExecution;
	}

	public static ServerCommandLineOptions Parse(string[] args, string workingDirectory)
	{
		string bridgeRoot = null;
		string saveDirectory = null;
		string gameDirectory = null;
		string windowTitle = null;
		bool enableInputExecution = false;
		for (int i = 0; i < args.Length; i++)
		{
			switch (args[i])
			{
			case "--bridge-root":
				bridgeRoot = RequireValue(args, ++i, "--bridge-root");
				break;
			case "--save-directory":
				saveDirectory = RequireValue(args, ++i, "--save-directory");
				break;
			case "--game-directory":
				gameDirectory = RequireValue(args, ++i, "--game-directory");
				break;
			case "--window-title":
				windowTitle = RequireValue(args, ++i, "--window-title");
				break;
			case "--execute-input":
				enableInputExecution = true;
				break;
			default:
				throw new ArgumentException("Unknown argument '" + args[i] + "'.");
			}
		}
		return new ServerCommandLineOptions(workingDirectory, bridgeRoot, saveDirectory, gameDirectory, windowTitle, enableInputExecution);
	}

	public BridgeServiceOptions ToBridgeServiceOptions()
	{
		BridgeServiceOptions bridgeServiceOptions = BridgeServiceOptions.CreateDefault(WorkingDirectory);
		return new BridgeServiceOptions(BridgeRoot ?? bridgeServiceOptions.BridgeRoot, SaveDirectory ?? bridgeServiceOptions.SaveDirectory, GameDirectory ?? bridgeServiceOptions.GameDirectory, WindowTitle ?? bridgeServiceOptions.WindowTitle, EnableInputExecution || bridgeServiceOptions.EnableInputExecution);
	}

	private static string RequireValue(string[] args, int index, string optionName)
	{
		if (index >= args.Length)
		{
			throw new ArgumentException("Missing value for " + optionName + ".");
		}
		return args[index];
	}
}

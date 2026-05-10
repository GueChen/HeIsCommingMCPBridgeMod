using System;
using AssetRipper.Primitives;

internal sealed class BootstrapperOptions
{
	public string GameAssemblyPath { get; }

	public string GlobalMetadataPath { get; }

	public UnityVersion UnityVersion { get; }

	public string UnityLibrariesDirectory { get; }

	public string OutputDirectory { get; }

	public string? DeobfuscationRegex { get; }

	private BootstrapperOptions(string gameAssemblyPath, string globalMetadataPath, UnityVersion unityVersion, string unityLibrariesDirectory, string outputDirectory, string? deobfuscationRegex)
	{
		GameAssemblyPath = gameAssemblyPath;
		GlobalMetadataPath = globalMetadataPath;
		UnityVersion = unityVersion;
		UnityLibrariesDirectory = unityLibrariesDirectory;
		OutputDirectory = outputDirectory;
		DeobfuscationRegex = deobfuscationRegex;
	}

	public static BootstrapperOptions Parse(string[] args)
	{
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		string deobfuscationRegex = null;
		int num;
		for (num = 0; num < args.Length; num++)
		{
			switch (args[num])
			{
			case "--game-assembly":
				text = RequireValue(args, ++num, "--game-assembly");
				break;
			case "--metadata":
				text2 = RequireValue(args, ++num, "--metadata");
				break;
			case "--unity-version":
				text3 = RequireValue(args, ++num, "--unity-version");
				break;
			case "--unity-libs":
				text4 = RequireValue(args, ++num, "--unity-libs");
				break;
			case "--output":
				text5 = RequireValue(args, ++num, "--output");
				break;
			case "--deobf-regex":
				deobfuscationRegex = RequireValue(args, ++num, "--deobf-regex");
				break;
			default:
				throw new ArgumentException("Unknown argument '" + args[num] + "'.");
			}
		}
		if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(text2) || string.IsNullOrWhiteSpace(text3) || string.IsNullOrWhiteSpace(text4) || string.IsNullOrWhiteSpace(text5))
		{
			throw new ArgumentException("Required arguments: --game-assembly <path> --metadata <path> --unity-version <version> --unity-libs <dir> --output <dir>");
		}
		return new BootstrapperOptions(text, text2, UnityVersion.Parse(text3), text4, text5, deobfuscationRegex);
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

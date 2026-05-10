using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AsmResolver.DotNet;
using Cpp2IL.Core;
using Cpp2IL.Core.Api;
using Cpp2IL.Core.InstructionSets;
using Cpp2IL.Core.OutputFormats;
using Cpp2IL.Core.ProcessingLayers;
using Il2CppInterop.Generator;
using Il2CppInterop.Generator.Runners;
using LibCpp2IL;

[CompilerGenerated]
internal class Program
{
	private static void _003CMain_003E_0024(string[] args)
	{
		BootstrapperOptions bootstrapperOptions = BootstrapperOptions.Parse(args);
		InstructionSetRegistry.RegisterInstructionSet<X86InstructionSet>(DefaultInstructionSets.X86_32);
		InstructionSetRegistry.RegisterInstructionSet<X86InstructionSet>(DefaultInstructionSets.X86_64);
		LibCpp2IlBinaryRegistry.RegisterBuiltInBinarySupport();
		Directory.CreateDirectory(bootstrapperOptions.OutputDirectory);
		Directory.CreateDirectory(bootstrapperOptions.UnityLibrariesDirectory);
		Cpp2IlApi.InitializeLibCpp2Il(bootstrapperOptions.GameAssemblyPath, bootstrapperOptions.GlobalMetadataPath, bootstrapperOptions.UnityVersion);
		List<Cpp2IlProcessingLayer> list = new List<Cpp2IlProcessingLayer>
		{
			new AttributeInjectorProcessingLayer()
		};
		foreach (Cpp2IlProcessingLayer item in list)
		{
			item.PreProcess(Cpp2IlApi.CurrentAppContext, list);
		}
		foreach (Cpp2IlProcessingLayer item2 in list)
		{
			item2.Process(Cpp2IlApi.CurrentAppContext);
		}
		List<AssemblyDefinition> source = new AsmResolverDllOutputFormatDefault().BuildAssemblies(Cpp2IlApi.CurrentAppContext);
		GeneratorOptions options = new GeneratorOptions
		{
			GameAssemblyPath = bootstrapperOptions.GameAssemblyPath,
			Source = source,
			OutputDir = bootstrapperOptions.OutputDirectory,
			UnityBaseLibsDir = bootstrapperOptions.UnityLibrariesDirectory,
			ObfuscatedNamesRegex = (string.IsNullOrWhiteSpace(bootstrapperOptions.DeobfuscationRegex) ? null : new Regex(bootstrapperOptions.DeobfuscationRegex))
		};
		Il2CppInteropGenerator.Create(options).AddInteropAssemblyGenerator().Run();
		Console.WriteLine("Generated interop assemblies to " + bootstrapperOptions.OutputDirectory);
	}
}

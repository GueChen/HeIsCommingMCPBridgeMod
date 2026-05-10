using BepInEx;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;

namespace MCPBridgeMod.Plugin;

[BepInPlugin("gue.heiscomming.mcpbridge", "He Is Coming MCP Bridge", "0.1.0")]
public sealed class MCPBridgePlugin : BasePlugin
{
	private LiveCatalogCapture _liveCatalogCapture;

	private BridgeActionQueueProcessor _actionQueueProcessor;

	private BridgeCaptureBehaviour _captureBehaviour;

	public override void Load()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		BridgeRuntimeOptions bridgeRuntimeOptions = BridgeRuntimeOptions.CreateDefault();
		BridgeScaffold bridgeScaffold = new BridgeScaffold();
		BridgeArtifactWriter bridgeArtifactWriter = new BridgeArtifactWriter(bridgeRuntimeOptions);
		bridgeArtifactWriter.WriteHandshake(bridgeScaffold.CreateHandshake());
		bridgeArtifactWriter.WriteSnapshot(bridgeScaffold.CreateBootstrapSnapshot(bridgeRuntimeOptions));
		bridgeArtifactWriter.WriteCatalog(bridgeScaffold.CreateBootstrapCatalog(bridgeRuntimeOptions));
		_liveCatalogCapture = new LiveCatalogCapture(bridgeRuntimeOptions, bridgeScaffold, bridgeArtifactWriter, ((BasePlugin)this).Log);
		_actionQueueProcessor = new BridgeActionQueueProcessor(bridgeArtifactWriter, _liveCatalogCapture, ((BasePlugin)this).Log);
		BridgeCaptureBehaviour.SharedCapture = _liveCatalogCapture;
		BridgeCaptureBehaviour.SharedActionQueueProcessor = _actionQueueProcessor;
		ClassInjector.RegisterTypeInIl2Cpp<BridgeCaptureBehaviour>();
		_captureBehaviour = ((BasePlugin)this).AddComponent<BridgeCaptureBehaviour>();
		_liveCatalogCapture.Capture("plugin-load");
		ManualLogSource log = ((BasePlugin)this).Log;
		bool flag = default(bool);
		BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(43, 2, ref flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>("He Is Coming MCP Bridge");
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" loaded. Bootstrap artifacts written to '");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(bridgeRuntimeOptions.ArtifactRoot);
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("'.");
		}
		log.LogInfo(val);
	}
}

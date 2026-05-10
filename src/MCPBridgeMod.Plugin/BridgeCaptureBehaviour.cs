using System;
using UnityEngine;

namespace MCPBridgeMod.Plugin;

public sealed class BridgeCaptureBehaviour : MonoBehaviour
{
	private float _nextCaptureTime;

	public static LiveCatalogCapture SharedCapture { get; set; }

	public static BridgeActionQueueProcessor SharedActionQueueProcessor { get; set; }

	public BridgeCaptureBehaviour(IntPtr pointer)
		: base(pointer)
	{
	}

	private void Update()
	{
		SharedActionQueueProcessor?.Pump();
		if (SharedCapture != null && !(Time.unscaledTime < _nextCaptureTime))
		{
			_nextCaptureTime = Time.unscaledTime + 5f;
			SharedCapture.Capture("probe-update");
		}
	}
}

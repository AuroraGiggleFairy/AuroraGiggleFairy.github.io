using System;
using UnityEngine;

public class SpeedTreeMotionVectorHelper : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Renderer renderer;

	public void Init(Renderer renderer)
	{
		this.renderer = renderer;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnBecameVisible()
	{
		if (!SpeedTreeWindHistoryBufferManager.Instance.TryRegisterActiveRenderer(renderer))
		{
			Debug.LogError("Failed to register tree renderer.");
			base.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnBecameInvisible()
	{
		SpeedTreeWindHistoryBufferManager.Instance.DeregisterActiveRenderer(renderer);
	}
}

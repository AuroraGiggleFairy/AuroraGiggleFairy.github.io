using System;
using UnityEngine;

public class UpdateLightOnPlayers : UpdateLight
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUpdateTime = 0.05f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isForceUpdate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int forceUpdateFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastFPV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float updateDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null || !GameManager.Instance.gameStateManager.IsGameStarted() || GameManager.IsDedicatedServer)
		{
			return;
		}
		if (!entity)
		{
			Transform transform = RootTransformRefEntity.FindEntityUpwards(base.transform);
			if ((bool)transform)
			{
				entity = transform.GetComponent<Entity>();
			}
		}
		else if (entity.emodel.IsFPV != lastFPV)
		{
			lastFPV = entity.emodel.IsFPV;
			forceUpdateFrame = Time.frameCount + 5;
			isForceUpdate = true;
		}
		if (isForceUpdate)
		{
			appliedLit = -1f;
			updateDelay = 0f;
			if (Time.frameCount >= forceUpdateFrame)
			{
				isForceUpdate = false;
			}
		}
		updateDelay -= Time.deltaTime;
		if (!(updateDelay > 0f))
		{
			updateDelay = 0.05f;
			UpdateLighting(0.15f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationFocus(bool focusStatus)
	{
		forceUpdateFrame = Time.frameCount + 3;
		isForceUpdate = true;
	}

	public void ForceUpdate()
	{
		updateDelay = 0f;
	}
}

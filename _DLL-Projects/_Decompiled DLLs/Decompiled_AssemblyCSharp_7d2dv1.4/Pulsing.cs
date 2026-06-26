using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Pulsing : LightState
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light lightComp;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		LODThreshold = 0.75f;
		lightComp = lightLOD.GetLight();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!GameManager.Instance.IsPaused())
		{
			if (!lightComp.enabled)
			{
				lightLOD.SwitchLightByState(_isOn: true);
			}
			float distSqrRatio = GetDistSqrRatio();
			if (distSqrRatio >= LODThreshold || !lightLOD.bSwitchedOn)
			{
				base.enabled = false;
			}
			float num = (Mathf.Sin(MathF.PI * 2f * lightLOD.StateRate * Time.time) + 1f) / 2f;
			lightComp.intensity = Utils.FastClamp((1f - distSqrRatio) * lightLOD.MaxIntensity, 0f, lightLOD.MaxIntensity) * num;
			UpdateEmissive(num, lightLOD.EmissiveFromLightColorOn);
		}
	}

	public override void Kill()
	{
		base.Kill();
		lightLOD.lightStateEnabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		lightLOD.lightStateEnabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		lightLOD.lightStateEnabled = false;
	}
}

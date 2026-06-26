using System;
using UnityEngine;

public class pulsingLightEmissive : LightState
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light lightP;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		lightP = lightLOD.GetLight();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!GameManager.Instance.IsPaused())
		{
			float num = (Mathf.Sin(MathF.PI * 2f * lightLOD.StateRate * Time.time) + 1f) / 2f;
			lightP.intensity = Utils.FastClamp(lightLOD.MaxIntensity, 0f, lightLOD.MaxIntensity) * num;
			UpdateEmissive(num, lightLOD.EmissiveFromLightColorOn);
		}
	}
}

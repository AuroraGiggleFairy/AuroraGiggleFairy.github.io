using System;
using UnityEngine;

public class pulsingLightEmissive : LightState
{
	public override float Intensity => (Mathf.Sin(MathF.PI * 2f * lightLOD.StateRate * Time.time) + 1f) / 2f;

	public override float Emissive => Intensity;
}

using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Pulsing : LightState
{
	public override float LODThreshold => 0.75f;

	public override float Intensity => (Mathf.Sin(MathF.PI * 2f * lightLOD.StateRate * Time.time) + 1f) / 2f;

	public override float Emissive => Intensity;
}

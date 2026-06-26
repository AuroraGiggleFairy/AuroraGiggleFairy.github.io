using System;
using UnityEngine;

public abstract class LightState : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public LightLOD lightLOD;

	public float LODThreshold;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		lightLOD = base.gameObject.GetComponent<LightLOD>();
	}

	public virtual void Kill()
	{
		UnityEngine.Object.Destroy(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float GetDistSqrRatio()
	{
		Vector3 vector = GameLightManager.Instance.CameraPos();
		float num = (base.transform.position - vector).sqrMagnitude * lightLOD.DistanceScale;
		float num2 = ((LightLOD.DebugViewDistance > 0f) ? LightLOD.DebugViewDistance : lightLOD.MaxDistance);
		num2 *= num2;
		return num / num2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateEmissive(float newV, bool useLightColor)
	{
		Color rgbColor = (useLightColor ? lightLOD.GetLight().color : lightLOD.EmissiveColor);
		Color.RGBToHSV(rgbColor, out var H, out var S, out var _);
		rgbColor = Color.HSVToRGB(H, S, newV);
		lightLOD.SetEmissiveColorCurrent(rgbColor);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public LightState()
	{
	}
}

using System;
using UnityEngine;

public abstract class LightState : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public LightLOD lightLOD;

	public virtual float LODThreshold => 1f;

	public virtual bool CanBeOn => true;

	public virtual float Intensity => 1f;

	public virtual float Emissive => 1f;

	public virtual float AudioFrequency
	{
		get
		{
			if (!lightLOD)
			{
				return -1f;
			}
			return lightLOD.StateRate;
		}
	}

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
	public LightState()
	{
	}
}

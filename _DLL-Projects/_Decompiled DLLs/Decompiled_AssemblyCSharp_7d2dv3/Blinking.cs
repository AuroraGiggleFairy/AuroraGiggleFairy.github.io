using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Blinking : LightState
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool switchedOn = true;

	public override float LODThreshold => 0.75f;

	public override bool CanBeOn => switchedOn;

	public override float Emissive
	{
		get
		{
			if (!switchedOn)
			{
				return 0f;
			}
			return 1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator blink()
	{
		while (true)
		{
			switchedOn = !switchedOn;
			yield return new WaitForSeconds(1f / lightLOD.StateRate);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		StartCoroutine(blink());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		StopAllCoroutines();
	}
}

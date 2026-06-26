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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		LODThreshold = 0.75f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator blink()
	{
		while (true)
		{
			switchedOn = !switchedOn;
			lightLOD.SwitchLightByState(switchedOn);
			UpdateEmissive(switchedOn ? 1f : 0f, lightLOD.EmissiveFromLightColorOn);
			yield return new WaitForSeconds(1f / lightLOD.StateRate);
		}
	}

	public override void Kill()
	{
		base.Kill();
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

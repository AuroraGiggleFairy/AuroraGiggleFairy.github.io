using System;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
	public float fadeSpeed = 2f;

	public float highIntensity = 2f;

	public float lowIntensity = 0.5f;

	public float changeMargin = 0.2f;

	public bool alarmOn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float targetIntensity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		GetComponent<Light>().intensity = 0f;
		targetIntensity = highIntensity;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (alarmOn)
		{
			GetComponent<Light>().intensity = Mathf.Lerp(GetComponent<Light>().intensity, targetIntensity, fadeSpeed * Time.deltaTime);
			CheckTargetIntensity();
		}
		else
		{
			GetComponent<Light>().intensity = Mathf.Lerp(GetComponent<Light>().intensity, 0f, fadeSpeed * Time.deltaTime);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckTargetIntensity()
	{
		if (Mathf.Abs(targetIntensity - GetComponent<Light>().intensity) < changeMargin)
		{
			if (targetIntensity == highIntensity)
			{
				targetIntensity = lowIntensity;
			}
			else
			{
				targetIntensity = highIntensity;
			}
		}
	}
}

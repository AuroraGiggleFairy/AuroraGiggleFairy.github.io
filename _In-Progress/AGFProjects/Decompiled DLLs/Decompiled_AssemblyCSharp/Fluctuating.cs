using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Fluctuating : LightState
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float unityLightIntensityMax = 8f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float hiRange = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float loRange = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float flutterVariance = 0.0625f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float increment;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float startIntensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fixedFrameRate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentIntensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentEmissive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool canSwitchProcess = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int process;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int numOfFrames;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int currentFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float t;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float preSlideIntenisty;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float slideTo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool up;

	public override float LODThreshold => 0.2f;

	public override float Intensity => currentIntensity;

	public override float Emissive => currentEmissive;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		startIntensity = 1f;
		fixedFrameRate = 1f / Time.fixedDeltaTime;
		currentIntensity = startIntensity;
		currentEmissive = startIntensity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.Instance.IsPaused())
		{
			return;
		}
		if (canSwitchProcess)
		{
			ChangeProcess();
			currentFrame = 0;
			int num = (int)(lightLOD.FluxDelay * fixedFrameRate);
			if (process == 0)
			{
				numOfFrames = UnityEngine.Random.Range(num / 2, num);
				t = 0f;
				preSlideIntenisty = currentIntensity;
				up = UnityEngine.Random.Range(0f, 1f) > slideProbability(preSlideIntenisty);
				if (up)
				{
					slideTo = UnityEngine.Random.Range(preSlideIntenisty, 1f);
				}
				else
				{
					slideTo = UnityEngine.Random.Range(0.2f, preSlideIntenisty);
				}
				increment = (slideTo - preSlideIntenisty) / (float)numOfFrames;
			}
			else if (process == 1)
			{
				numOfFrames = UnityEngine.Random.Range(90, 181);
			}
			else
			{
				numOfFrames = UnityEngine.Random.Range(num / 2, num);
			}
		}
		if (process == 0)
		{
			Slide();
		}
		else if (process == 1)
		{
			Flutter();
		}
		else if (canSwitchProcess)
		{
			currentIntensity = startIntensity;
			currentEmissive = startIntensity / 1f;
		}
		canSwitchProcess = ++currentFrame >= numOfFrames;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Slide()
	{
		if (up)
		{
			currentIntensity = Mathf.Lerp(preSlideIntenisty, slideTo, t);
			currentEmissive = currentIntensity / 1f;
			t += increment;
		}
		else
		{
			currentIntensity = Mathf.Lerp(slideTo, preSlideIntenisty, t);
			currentEmissive = currentIntensity / 1f;
			t -= increment;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Flutter()
	{
		switch (UnityEngine.Random.Range(0, 3))
		{
		case 0:
			currentIntensity = Mathf.Clamp(currentIntensity + 0.0625f, 0.2f, 1f);
			currentEmissive = currentIntensity / 1f;
			break;
		case 1:
			currentIntensity = Mathf.Clamp(currentIntensity - 0.0625f, 0.2f, 1f);
			currentEmissive = currentIntensity / 1f;
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float slideProbability(float intensity)
	{
		return (intensity - 0.2f) / 0.8f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChangeProcess()
	{
		int num = UnityEngine.Random.Range(1, 3);
		if (num != process)
		{
			process = num;
		}
		else if (process > 0)
		{
			process = (process + 1) % 3;
		}
		else
		{
			process++;
		}
	}
}

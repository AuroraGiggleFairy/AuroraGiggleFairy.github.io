using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Fluctuating : LightState
{
	public float hiRange;

	public float loRange = 4f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float increment;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light lightComp;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float startIntensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fixedFrameRate;

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

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool didSkip;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		LODThreshold = 0.2f;
		lightComp = lightLOD.GetLight();
		startIntensity = lightComp.intensity;
		fixedFrameRate = 1f / Time.fixedDeltaTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.Instance.IsPaused())
		{
			return;
		}
		if (GetDistSqrRatio() >= LODThreshold)
		{
			base.enabled = false;
		}
		if (canSwitchProcess)
		{
			lightLOD.SwitchLightByState(_isOn: true);
			ChangeProcess();
			currentFrame = 0;
			int num = (int)(lightLOD.FluxDelay * fixedFrameRate);
			if (process == 0)
			{
				numOfFrames = UnityEngine.Random.Range(num / 2, num);
				t = 0f;
				preSlideIntenisty = lightComp.intensity;
				up = UnityEngine.Random.Range(0f, 1f) > slideProbability(preSlideIntenisty);
				if (up)
				{
					slideTo = UnityEngine.Random.Range(preSlideIntenisty, hiRange);
				}
				else
				{
					slideTo = UnityEngine.Random.Range(loRange, preSlideIntenisty);
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
			lightComp.intensity = startIntensity;
			UpdateEmissive(startIntensity / hiRange, lightLOD.EmissiveFromLightColorOn);
		}
		canSwitchProcess = ++currentFrame >= numOfFrames;
	}

	public override void Kill()
	{
		base.Kill();
		lightLOD.lightStateEnabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Slide()
	{
		if (up)
		{
			lightComp.intensity = Mathf.Lerp(preSlideIntenisty, slideTo, t);
			UpdateEmissive(lightComp.intensity / hiRange, lightLOD.EmissiveFromLightColorOn);
			t += increment;
		}
		else
		{
			lightComp.intensity = Mathf.Lerp(slideTo, preSlideIntenisty, t);
			UpdateEmissive(lightComp.intensity / hiRange, lightLOD.EmissiveFromLightColorOn);
			t -= increment;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Flutter()
	{
		switch (UnityEngine.Random.Range(0, 3))
		{
		case 0:
			lightComp.intensity = Mathf.Clamp(lightComp.intensity + 0.25f, loRange, hiRange);
			UpdateEmissive(lightComp.intensity / hiRange, lightLOD.EmissiveFromLightColorOn);
			break;
		case 1:
			lightComp.intensity = Mathf.Clamp(lightComp.intensity - 0.25f, loRange, hiRange);
			UpdateEmissive(lightComp.intensity / hiRange, lightLOD.EmissiveFromLightColorOn);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Skip()
	{
		if (didSkip)
		{
			lightLOD.SwitchLightByState(_isOn: true);
			didSkip = false;
		}
		if (currentFrame == UnityEngine.Random.Range(0, numOfFrames))
		{
			lightLOD.SwitchLightByState(!lightLOD.bSwitchedOn);
			didSkip = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float slideProbability(float intensity)
	{
		return 1f / (hiRange - loRange) * (intensity - loRange);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		LightLOD obj = lightLOD;
		obj.MaxIntensityChanged = (LightLOD.MaxIntensityEvent)Delegate.Combine(obj.MaxIntensityChanged, new LightLOD.MaxIntensityEvent(ChangeMaxIntensity));
		hiRange = lightLOD.MaxIntensity;
		loRange = 0.2f * lightLOD.MaxIntensity;
		lightLOD.lightStateEnabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		LightLOD obj = lightLOD;
		obj.MaxIntensityChanged = (LightLOD.MaxIntensityEvent)Delegate.Remove(obj.MaxIntensityChanged, new LightLOD.MaxIntensityEvent(ChangeMaxIntensity));
		lightLOD.MaxIntensity = hiRange;
		lightLOD.lightStateEnabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChangeMaxIntensity()
	{
		loRange = 0.2f * lightLOD.MaxIntensity;
		hiRange = lightLOD.MaxIntensity;
		lightComp.intensity = (startIntensity = lightLOD.MaxIntensity);
		currentFrame = numOfFrames;
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

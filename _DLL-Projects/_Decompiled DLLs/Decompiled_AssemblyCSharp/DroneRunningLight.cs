using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneRunningLight : MonoBehaviour
{
	public float MinLightIntensity;

	public float MaxLightIntensity;

	public float LightBlinkInterval;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light runningLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float startIntensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleSystem particles;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lightBlinkTimer;

	public Color LightColor;

	public List<Light> connectedLights;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool lightsActive;

	public bool dayTimeVisibility;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float transitionTime = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float transitionTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		runningLight = GetComponent<Light>();
		particles = base.transform.GetComponentInChildren<ParticleSystem>();
		_initLights();
		setLightsActive(value: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _initLights()
	{
		lightBlinkTimer = LightBlinkInterval;
		startIntensity = MinLightIntensity;
		runningLight.intensity = startIntensity;
		setLightColor(LightColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLightsActive(bool value)
	{
		runningLight.enabled = value;
		if (connectedLights != null)
		{
			for (int i = 0; i < connectedLights.Count; i++)
			{
				connectedLights[i].enabled = value;
			}
		}
		if (!dayTimeVisibility)
		{
			particles.gameObject.SetActive(value);
		}
		lightsActive = value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLightColor(Color color)
	{
		runningLight.color = color;
		ParticleSystem.MainModule main = particles.main;
		main.startColor = color;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		float num = GameUtils.WorldTimeToHours(world.worldTime);
		if (num > 4f && num < 22f)
		{
			if (runningLight.intensity > startIntensity)
			{
				_initLights();
			}
			if (lightsActive)
			{
				setLightsActive(!lightsActive);
			}
			return;
		}
		if (!lightsActive)
		{
			setLightsActive(!lightsActive);
		}
		if (runningLight.color != LightColor || particles.main.startColor.color != LightColor)
		{
			setLightColor(LightColor);
		}
		if (startIntensity != MinLightIntensity)
		{
			startIntensity = MinLightIntensity;
		}
		if (lightBlinkTimer > 0f)
		{
			lightBlinkTimer -= Time.deltaTime;
			if (lightBlinkTimer < 0.2f && lightBlinkTimer > 0.15f && particles.gameObject.activeSelf)
			{
				particles.gameObject.SetActive(value: false);
			}
			if (lightBlinkTimer < 0.15f && !particles.gameObject.activeSelf)
			{
				particles.gameObject.SetActive(value: true);
			}
			if (lightBlinkTimer <= 0f)
			{
				StartCoroutine(blink());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator blink()
	{
		transitionTimer = transitionTime;
		while (runningLight.intensity < MaxLightIntensity)
		{
			transitionTimer += Time.deltaTime;
			runningLight.intensity = Mathf.Lerp(startIntensity, MaxLightIntensity, transitionTimer / transitionTime);
			yield return null;
		}
		transitionTimer = transitionTime;
		while (runningLight.intensity > startIntensity)
		{
			transitionTimer += Time.deltaTime;
			runningLight.intensity = Mathf.Lerp(MaxLightIntensity, startIntensity, transitionTimer / transitionTime);
			yield return null;
		}
		runningLight.intensity = startIntensity;
		lightBlinkTimer = LightBlinkInterval;
		yield return null;
	}
}

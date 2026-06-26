using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemSpawnRateLimiter : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<ParticleSystem, float> originalSpawnRates = new Dictionary<ParticleSystem, float>();

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			originalSpawnRates[particleSystem] = particleSystem.emission.rateOverTime.constant;
			ParticleSystem.EmissionModule emission = particleSystem.emission;
			ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
			rateOverTime.constant = originalSpawnRates[particleSystem] * num;
			emission.rateOverTime = rateOverTime;
		}
		GameOptionsManager.OnGameOptionsApplied += OnGameOptionsApplied;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.OnGameOptionsApplied -= OnGameOptionsApplied;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameOptionsApplied()
	{
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			if (originalSpawnRates.ContainsKey(particleSystem))
			{
				ParticleSystem.EmissionModule emission = particleSystem.emission;
				ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
				rateOverTime.constant = originalSpawnRates[particleSystem] * num;
				emission.rateOverTime = rateOverTime;
			}
		}
	}
}

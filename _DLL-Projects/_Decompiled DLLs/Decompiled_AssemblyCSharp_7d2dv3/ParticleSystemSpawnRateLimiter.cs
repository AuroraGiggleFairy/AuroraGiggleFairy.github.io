using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemSpawnRateLimiter : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class EmissionRateData
	{
		public ParticleSystemCurveMode mode;

		public float constant;

		public AnimationCurve curve;

		public float curveMultiplier;

		public float constantMin;

		public float constantMax;

		public AnimationCurve curveMin;

		public AnimationCurve curveMax;

		public ParticleSystem.Burst[] originalBursts;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cScaleMin = 0.15f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<ParticleSystem, EmissionRateData> originalRates = new Dictionary<ParticleSystem, EmissionRateData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		float v = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		v = Utils.FastMax(0.15f, v);
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			ParticleSystem.EmissionModule emission = particleSystem.emission;
			ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
			EmissionRateData emissionRateData = new EmissionRateData
			{
				mode = rateOverTime.mode,
				curveMultiplier = rateOverTime.curveMultiplier,
				originalBursts = new ParticleSystem.Burst[emission.burstCount]
			};
			emission.GetBursts(emissionRateData.originalBursts);
			switch (rateOverTime.mode)
			{
			case ParticleSystemCurveMode.Constant:
				emissionRateData.constant = rateOverTime.constant;
				break;
			case ParticleSystemCurveMode.Curve:
				emissionRateData.curve = new AnimationCurve(rateOverTime.curve.keys);
				break;
			case ParticleSystemCurveMode.TwoConstants:
				emissionRateData.constantMin = rateOverTime.constantMin;
				emissionRateData.constantMax = rateOverTime.constantMax;
				break;
			case ParticleSystemCurveMode.TwoCurves:
				emissionRateData.curveMin = new AnimationCurve(rateOverTime.curveMin.keys);
				emissionRateData.curveMax = new AnimationCurve(rateOverTime.curveMax.keys);
				break;
			}
			originalRates[particleSystem] = emissionRateData;
			ScaleEmissionRate(particleSystem, v);
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
		float v = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		v = Utils.FastMax(0.15f, v);
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			if (originalRates.ContainsKey(particleSystem))
			{
				ScaleEmissionRate(particleSystem, v);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ScaleEmissionRate(ParticleSystem ps, float scale)
	{
		if (!originalRates.ContainsKey(ps))
		{
			return;
		}
		ParticleSystem.EmissionModule emission = ps.emission;
		EmissionRateData emissionRateData = originalRates[ps];
		ParticleSystem.MinMaxCurve rateOverTime = default(ParticleSystem.MinMaxCurve);
		switch (emissionRateData.mode)
		{
		case ParticleSystemCurveMode.Constant:
			rateOverTime = new ParticleSystem.MinMaxCurve(emissionRateData.constant * scale);
			break;
		case ParticleSystemCurveMode.Curve:
			rateOverTime = new ParticleSystem.MinMaxCurve(emissionRateData.curveMultiplier * scale, emissionRateData.curve);
			break;
		case ParticleSystemCurveMode.TwoConstants:
			rateOverTime = new ParticleSystem.MinMaxCurve(emissionRateData.constantMin * scale, emissionRateData.constantMax * scale);
			break;
		case ParticleSystemCurveMode.TwoCurves:
			rateOverTime = new ParticleSystem.MinMaxCurve(emissionRateData.curveMultiplier * scale, emissionRateData.curveMin, emissionRateData.curveMax);
			break;
		}
		emission.rateOverTime = rateOverTime;
		if (emissionRateData.originalBursts != null && emissionRateData.originalBursts.Length != 0)
		{
			ParticleSystem.Burst[] array = new ParticleSystem.Burst[emissionRateData.originalBursts.Length];
			for (int i = 0; i < emissionRateData.originalBursts.Length; i++)
			{
				ParticleSystem.Burst burst = emissionRateData.originalBursts[i];
				array[i] = new ParticleSystem.Burst(burst.time, ScaleBurstCurve(burst.count, scale), burst.cycleCount, burst.repeatInterval);
			}
			emission.SetBursts(array);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ParticleSystem.MinMaxCurve ScaleBurstCurve(ParticleSystem.MinMaxCurve original, float scale)
	{
		ParticleSystem.MinMaxCurve result = new ParticleSystem.MinMaxCurve
		{
			mode = original.mode
		};
		switch (original.mode)
		{
		case ParticleSystemCurveMode.Constant:
			result.constant = ScaleBurstConstant(original.constant, scale);
			break;
		case ParticleSystemCurveMode.Curve:
			result.curveMultiplier = original.curveMultiplier * scale;
			result.curve = original.curve;
			break;
		case ParticleSystemCurveMode.TwoConstants:
			result.constantMin = ScaleBurstConstant(original.constantMin, scale);
			result.constantMax = ScaleBurstConstant(original.constantMax, scale);
			break;
		case ParticleSystemCurveMode.TwoCurves:
			result.curveMultiplier = original.curveMultiplier * scale;
			result.curveMin = original.curveMin;
			result.curveMax = original.curveMax;
			break;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float ScaleBurstConstant(float constant, float scale)
	{
		if (Utils.FastAbs(constant) <= float.Epsilon)
		{
			return 0f;
		}
		return Utils.FastMax(Mathf.Round(constant * scale), 1f) + float.Epsilon;
	}
}

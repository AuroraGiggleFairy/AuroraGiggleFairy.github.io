using System.Collections.Generic;
using UnityEngine;

public class BiomeAtmosphereEffects
{
	public BiomeDefinition[] nearBiomes = new BiomeDefinition[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeIntensity currentBiomeIntensity = BiomeIntensity.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public AtmosphereEffect[] worldColorSpectrums;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i playerPosition;

	public bool ForceDefault;

	public void Init(World _world)
	{
		world = _world;
		worldColorSpectrums = new AtmosphereEffect[255];
		worldColorSpectrums[0] = AtmosphereEffect.Load("default", null);
		foreach (KeyValuePair<uint, BiomeDefinition> item in _world.Biomes.GetBiomeMap())
		{
			worldColorSpectrums[item.Value.m_Id] = AtmosphereEffect.Load(item.Value.m_SpectrumName, worldColorSpectrums[0]);
		}
		ForceDefault = false;
	}

	public void Reload()
	{
		Init(world);
		Update();
	}

	public virtual void Update()
	{
		EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		if (ForceDefault)
		{
			currentBiomeIntensity = BiomeIntensity.Default;
			return;
		}
		Vector3i blockPosition = primaryPlayer.GetBlockPosition();
		if (blockPosition.Equals(playerPosition) || !world.GetBiomeIntensity(blockPosition, out var _biomeIntensity))
		{
			return;
		}
		playerPosition = blockPosition;
		if (!currentBiomeIntensity.Equals(_biomeIntensity))
		{
			WorldBiomes biomes = GameManager.Instance.World.Biomes;
			BiomeDefinition biome = biomes.GetBiome(_biomeIntensity.biomeId0);
			if (biome != null)
			{
				biome.currentPlayerIntensity = _biomeIntensity.intensity0;
			}
			nearBiomes[0] = biome;
			biome = biomes.GetBiome(_biomeIntensity.biomeId1);
			if (biome != null)
			{
				biome.currentPlayerIntensity = _biomeIntensity.intensity1;
			}
			nearBiomes[1] = biome;
			biome = biomes.GetBiome(_biomeIntensity.biomeId2);
			if (biome != null)
			{
				biome.currentPlayerIntensity = _biomeIntensity.intensity1;
			}
			nearBiomes[2] = biome;
			biome = biomes.GetBiome(_biomeIntensity.biomeId3);
			if (biome != null)
			{
				biome.currentPlayerIntensity = _biomeIntensity.intensity2;
			}
			nearBiomes[3] = biome;
		}
		currentBiomeIntensity = _biomeIntensity;
	}

	public virtual Color GetSkyColorSpectrum(float _v)
	{
		return getColorFromSpectrum(currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.Sky);
	}

	public virtual Color GetAmbientColorSpectrum(float _v)
	{
		return getColorFromSpectrum(currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.Ambient);
	}

	public virtual Color GetSunColorSpectrum(float _v)
	{
		return getColorFromSpectrum(currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.Sun);
	}

	public virtual Color GetMoonColorSpectrum(float _v)
	{
		return getColorFromSpectrum(currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.Moon);
	}

	public virtual Color GetFogColorSpectrum(float _v)
	{
		return getColorFromSpectrum(currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.Fog);
	}

	public virtual Color GetFogFadeColorSpectrum(float _v)
	{
		return getColorFromSpectrum(currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.FogFade);
	}

	public virtual Color GetCloudsColor(float _v)
	{
		return Color.white;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color getColorFromSpectrum(BiomeIntensity _bi, float _v, AtmosphereEffect.ESpecIdx _spectrumIdx)
	{
		float intensity = _bi.intensity0;
		float intensity2 = _bi.intensity1;
		float intensity3 = _bi.intensity2;
		float intensity4 = _bi.intensity3;
		intensity4 = 0f;
		return (worldColorSpectrums[_bi.biomeId0].spectrums[(int)_spectrumIdx].GetValue(_v) * intensity + worldColorSpectrums[_bi.biomeId1].spectrums[(int)_spectrumIdx].GetValue(_v) * intensity2 + worldColorSpectrums[_bi.biomeId2].spectrums[(int)_spectrumIdx].GetValue(_v) * intensity3 + worldColorSpectrums[_bi.biomeId3].spectrums[(int)_spectrumIdx].GetValue(_v) * intensity4) / (intensity + intensity2 + intensity3 + intensity4);
	}
}

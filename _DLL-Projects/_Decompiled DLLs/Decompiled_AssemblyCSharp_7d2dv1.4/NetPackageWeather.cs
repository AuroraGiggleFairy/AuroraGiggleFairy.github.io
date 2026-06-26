using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWeather : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public WeatherPackage[] weatherPackages;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lightningPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lightningTime;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageWeather Setup(WeatherPackage[] _packages, ulong _lightningTime, Vector3 _lightningPos)
	{
		weatherPackages = _packages;
		lightningTime = _lightningTime;
		lightningPos = _lightningPos;
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitPackages()
	{
		if (!(GameManager.Instance != null) || GameManager.Instance.World == null || GameManager.Instance.World.Biomes == null)
		{
			return;
		}
		Dictionary<uint, BiomeDefinition> biomeMap = GameManager.Instance.World.Biomes.GetBiomeMap();
		if (biomeMap != null)
		{
			weatherPackages = new WeatherPackage[biomeMap.Count];
			for (int i = 0; i < biomeMap.Count; i++)
			{
				weatherPackages[i] = new WeatherPackage();
			}
		}
	}

	public override void read(PooledBinaryReader _br)
	{
		if (weatherPackages == null)
		{
			InitPackages();
			if (weatherPackages == null)
			{
				return;
			}
		}
		for (int i = 0; i < weatherPackages.Length; i++)
		{
			WeatherPackage weatherPackage = weatherPackages[i];
			for (int j = 0; j < weatherPackage.param.Length; j++)
			{
				weatherPackage.param[j] = _br.ReadSingle();
			}
			weatherPackage.particleRain = _br.ReadSingle();
			weatherPackage.particleSnow = _br.ReadSingle();
			weatherPackage.surfaceWet = _br.ReadSingle();
			weatherPackage.surfaceSnow = _br.ReadSingle();
			weatherPackage.biomeID = _br.ReadByte();
			weatherPackage.weatherSpectrum = _br.ReadInt16();
		}
		lightningTime = _br.ReadUInt64();
		lightningPos.x = _br.ReadSingle();
		lightningPos.y = _br.ReadSingle();
		lightningPos.z = _br.ReadSingle();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		for (int i = 0; i < weatherPackages.Length; i++)
		{
			WeatherPackage weatherPackage = weatherPackages[i];
			for (int j = 0; j < weatherPackage.param.Length; j++)
			{
				_bw.Write(weatherPackage.param[j]);
			}
			_bw.Write(weatherPackage.particleRain);
			_bw.Write(weatherPackage.particleSnow);
			_bw.Write(weatherPackage.surfaceWet);
			_bw.Write(weatherPackage.surfaceSnow);
			_bw.Write(weatherPackage.biomeID);
			_bw.Write(weatherPackage.weatherSpectrum);
		}
		_bw.Write(lightningTime);
		_bw.Write(lightningPos.x);
		_bw.Write(lightningPos.y);
		_bw.Write(lightningPos.z);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && (bool)WeatherManager.Instance)
		{
			if (lightningTime != 0)
			{
				WeatherManager.Instance.TriggerThunder(lightningTime, lightningPos);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient && weatherPackages != null)
			{
				WeatherManager.Instance.ClientProcessPackages(weatherPackages);
			}
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}

using UnityEngine.Scripting;

[Preserve]
public class NetPackageWeather : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public WeatherPackage[] weatherPackages;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageWeather Setup(WeatherPackage[] _packages)
	{
		weatherPackages = _packages;
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitPackages()
	{
		if ((bool)WeatherManager.Instance)
		{
			int count = WeatherManager.Instance.biomeWeather.Count;
			weatherPackages = new WeatherPackage[count];
			for (int i = 0; i < count; i++)
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
			weatherPackage.biomeId = _br.ReadByte();
			weatherPackage.groupIndex = _br.ReadByte();
			weatherPackage.remainingSeconds = _br.ReadByte();
			for (int j = 0; j < weatherPackage.param.Length; j++)
			{
				weatherPackage.param[j] = _br.ReadSingle();
			}
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		for (int i = 0; i < weatherPackages.Length; i++)
		{
			WeatherPackage weatherPackage = weatherPackages[i];
			_bw.Write(weatherPackage.biomeId);
			_bw.Write(weatherPackage.groupIndex);
			_bw.Write(weatherPackage.remainingSeconds);
			for (int j = 0; j < weatherPackage.param.Length; j++)
			{
				_bw.Write(weatherPackage.param[j]);
			}
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient && weatherPackages != null && (bool)WeatherManager.Instance)
		{
			WeatherManager.Instance.ClientProcessPackages(weatherPackages);
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}

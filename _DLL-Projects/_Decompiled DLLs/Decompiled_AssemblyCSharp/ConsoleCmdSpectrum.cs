using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSpectrum : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "spectrum" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Force a particular lighting spectrum.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "spectrum <Auto, Biome, BloodMoon, Foggy, Rainy, Stormy, Snowy>\n";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (_params.Count == 0)
		{
			if (WeatherManager.forcedSpectrum != SpectrumWeatherType.None)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("forced " + WeatherManager.forcedSpectrum);
			}
			else if (!(WeatherManager.Instance == null))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(WeatherManager.Instance.GetSpectrumInfo());
			}
		}
		else if (_params.Count == 1)
		{
			if (_params[0].EqualsCaseInsensitive("Snowy"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.Snowy);
			}
			else if (_params[0].EqualsCaseInsensitive("Rainy"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.Rainy);
			}
			else if (_params[0].EqualsCaseInsensitive("Stormy"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.Stormy);
			}
			else if (_params[0].EqualsCaseInsensitive("Foggy"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.Foggy);
			}
			else if (_params[0].EqualsCaseInsensitive("BloodMoon"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.BloodMoon);
			}
			else if (_params[0].EqualsCaseInsensitive("Biome"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.Biome);
			}
			else if (_params[0].EqualsCaseInsensitive("Auto"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.None);
			}
		}
	}
}

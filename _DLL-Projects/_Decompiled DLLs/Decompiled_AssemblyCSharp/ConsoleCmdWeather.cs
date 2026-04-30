using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdWeather : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "weather" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Control weather settings";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Clouds [0 to 1 (-1 defaults)]\nFog [density (-1 defaults)] [start] [end]\nFogColor [red (-1 defaults)] [green] [blue]\nRain [0 to 1 (-1 defaults)]\nSnowFall [0 to 1 (-1 defaults)]\nTemp [-99 to 101 (< -99 defaults)]\nWind [0 to 200 (-1 defaults)]\nSimRand [0 to 1 (-1 defaults)]\nStorm <duration hours> <biome name>\nDefaults or d\n";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			WeatherManager.BiomeWeather currentWeather = WeatherManager.currentWeather;
			bool flag = currentWeather != null;
			if ((bool)GameManager.Instance && GameManager.Instance.World != null)
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if ((bool)primaryPlayer)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Player biome {0}", primaryPlayer.biomeStandingOn);
				}
			}
			if ((bool)WeatherManager.Instance)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("WeatherManager " + WeatherManager.Instance.ToString());
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Clouds " + (WeatherManager.GetCloudThickness() * 0.01f).ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Fog density {0}, start {1}, end {2}", SkyManager.GetFogDensity().ToCultureInvariantString(), SkyManager.GetFogStart().ToCultureInvariantString(), SkyManager.GetFogEnd().ToCultureInvariantString());
			Color fogColor = SkyManager.GetFogColor();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("FogColor {0} {1} {2}", fogColor.r.ToCultureInvariantString(), fogColor.g.ToCultureInvariantString(), fogColor.b.ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Rain " + ((WeatherManager.forceRain >= 0f) ? WeatherManager.forceRain : (flag ? currentWeather.rainParam.value : 0f)).ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Snowfall " + ((WeatherManager.forceSnowfall >= 0f) ? WeatherManager.forceSnowfall : (flag ? currentWeather.snowFallParam.value : 0f)).ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Temperature " + WeatherManager.GetTemperature().ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wind " + WeatherManager.GetWindSpeed().ToCultureInvariantString());
			return;
		}
		WeatherManager.needToReUpdateWeatherSpectrums = true;
		float num = 1f;
		if (_params.Count > 1)
		{
			num = StringParsers.ParseFloat(_params[1]);
		}
		if (_params[0].EqualsCaseInsensitive("Defaults") || _params[0].EqualsCaseInsensitive("d"))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Using defaults");
			WeatherManager.forceClouds = -1f;
			WeatherManager.forceRain = -1f;
			WeatherManager.forceSnowfall = -1f;
			WeatherManager.forceTemperature = -100f;
			WeatherManager.forceWind = -1f;
			WeatherManager.SetSimRandom(-1f);
			SkyManager.SetFogDebug();
			SkyManager.SetFogDebugColor();
		}
		else if (_params[0].EqualsCaseInsensitive("IndoorFogOff"))
		{
			SkyManager.indoorFogOn = false;
		}
		else if (_params[0].EqualsCaseInsensitive("IndoorFogOn"))
		{
			SkyManager.indoorFogOn = true;
		}
		else if (_params[0].EqualsCaseInsensitive("clouds"))
		{
			WeatherManager.forceClouds = Mathf.Clamp(num, -1f, 1f);
			if ((bool)WeatherManager.Instance)
			{
				WeatherManager.Instance.CloudsFrameUpdateNow();
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cloud thickness set to " + WeatherManager.forceClouds.ToCultureInvariantString() + ".");
		}
		else if (_params[0].EqualsCaseInsensitive("rain"))
		{
			WeatherManager.forceRain = Mathf.Clamp(num, -1f, 1f);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Rain set to " + WeatherManager.forceRain.ToCultureInvariantString() + ".");
		}
		else if (_params[0].EqualsCaseInsensitive("snowfall"))
		{
			WeatherManager.forceSnowfall = Mathf.Clamp(num, -1f, 1f);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Snowfall set to " + WeatherManager.forceSnowfall.ToCultureInvariantString() + ".");
		}
		else if (_params[0].EqualsCaseInsensitive("temp"))
		{
			WeatherManager.forceTemperature = num;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Temperature set to " + WeatherManager.forceTemperature.ToCultureInvariantString() + ".");
		}
		else if (_params[0].EqualsCaseInsensitive("fog"))
		{
			float start = float.MinValue;
			float end = float.MinValue;
			if (_params.Count >= 3)
			{
				start = StringParsers.ParseFloat(_params[2]);
			}
			if (_params.Count >= 4)
			{
				end = StringParsers.ParseFloat(_params[3]);
			}
			SkyManager.SetFogDebug(num, start, end);
		}
		else if (_params[0].EqualsCaseInsensitive("fogcolor"))
		{
			if (num < 0f)
			{
				SkyManager.SetFogDebugColor();
				return;
			}
			float g = num;
			float b = num;
			if (_params.Count >= 3)
			{
				g = StringParsers.ParseFloat(_params[2]);
			}
			if (_params.Count >= 4)
			{
				b = StringParsers.ParseFloat(_params[3]);
			}
			SkyManager.SetFogDebugColor(new Color(num, g, b));
		}
		else if (_params[0].EqualsCaseInsensitive("thunder"))
		{
			if (GameManager.Instance != null && GameManager.Instance.World != null)
			{
				EntityPlayerLocal primaryPlayer2 = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer2 != null)
				{
					WeatherManager.Instance.TriggerThunder((int)GameManager.Instance.World.GetWorldTime() + (int)num, primaryPlayer2.position);
				}
			}
		}
		else if (_params[0].EqualsCaseInsensitive("wind"))
		{
			WeatherManager.forceWind = Mathf.Clamp(num, -1f, 200f);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wind set to " + WeatherManager.forceWind.ToCultureInvariantString() + ".");
		}
		else if (_params[0].EqualsCaseInsensitive("simrand"))
		{
			WeatherManager.SetSimRandom(num);
		}
		else if (_params[0].EqualsCaseInsensitive("storm"))
		{
			if ((bool)WeatherManager.Instance)
			{
				string biomeName = null;
				if (_params.Count >= 3)
				{
					biomeName = _params[2];
				}
				WeatherManager.Instance.SetStorm(biomeName, (int)(num * 1000f));
				WeatherManager.Instance.TriggerUpdate();
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command not recognized!");
		}
	}
}

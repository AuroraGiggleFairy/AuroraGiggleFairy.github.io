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
		return "Clouds [0 to 1 (-1 defaults)]\nFog [density (-1 defaults)] [start] [end]\nFogColor [red (-1 defaults)] [green] [blue]\nRain [0 to 1 (-1 defaults)]\nWet [0 to 1 (-1 defaults)]\nSnow [0 to 1 (-1 defaults)]\nSnowFall [0 to 1 (-1 defaults)]\nTemp [-99 to 101 (< -99 defaults)]\nWind [0 to 200 (-1 defaults)]\nSimRand [0 to 1 (-1 defaults)]\nDefaults or d\n";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("<Begin> Processing weather command...");
		if (_params.Count == 0)
		{
			WeatherManager.BiomeWeather currentWeather = WeatherManager.currentWeather;
			bool flag = currentWeather != null;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Clouds " + (WeatherManager.GetCloudThickness() * 0.01f).ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Fog density {0}, start {1}, end {2}", SkyManager.GetFogDensity().ToCultureInvariantString(), SkyManager.GetFogStart().ToCultureInvariantString(), SkyManager.GetFogEnd().ToCultureInvariantString());
			Color fogColor = SkyManager.GetFogColor();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("FogColor {0} {1} {2}", fogColor.r.ToCultureInvariantString(), fogColor.g.ToCultureInvariantString(), fogColor.b.ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Rain " + ((WeatherManager.forceRain >= 0f) ? WeatherManager.forceRain : (flag ? currentWeather.rainParam.value : 0f)).ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wet " + ((WeatherManager.forceWet >= 0f) ? WeatherManager.forceWet : (flag ? currentWeather.wetParam.value : 0f)).ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Snow " + ((WeatherManager.forceSnow >= 0f) ? WeatherManager.forceSnow : (flag ? currentWeather.snowCoverParam.value : 0f)).ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Snowfall " + ((WeatherManager.forceSnowfall >= 0f) ? WeatherManager.forceSnowfall : (flag ? currentWeather.snowFallParam.value : 0f)).ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Temperature " + WeatherManager.GetTemperature().ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wind " + WeatherManager.GetWindSpeed().ToCultureInvariantString());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("<end/>");
			return;
		}
		WeatherManager.needToReUpdateWeatherSpectrums = true;
		if (_params.Count == 1)
		{
			if (_params[0].EqualsCaseInsensitive("Defaults") || _params[0].EqualsCaseInsensitive("d"))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Using defaults. <end/>");
				WeatherManager.forceClouds = -1f;
				WeatherManager.forceRain = -1f;
				WeatherManager.forceWet = -1f;
				WeatherManager.forceSnow = -1f;
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
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command not recognized. <end/>");
			}
			return;
		}
		float num = 1f;
		if (_params.Count > 1)
		{
			num = StringParsers.ParseFloat(_params[1]);
		}
		if (_params[0].EqualsCaseInsensitive("clouds"))
		{
			WeatherManager.forceClouds = Mathf.Clamp(num, -1f, 1f);
			if ((bool)WeatherManager.Instance)
			{
				WeatherManager.Instance.CloudsFrameUpdateNow();
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cloud thickness set to " + WeatherManager.forceClouds.ToCultureInvariantString() + ". <end/>");
		}
		else if (_params[0].EqualsCaseInsensitive("rain"))
		{
			WeatherManager.forceRain = Mathf.Clamp(num, -1f, 1f);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Rain set to " + WeatherManager.forceRain.ToCultureInvariantString() + ". <end/>");
		}
		else if (_params[0].EqualsCaseInsensitive("snow"))
		{
			WeatherManager.forceSnow = Mathf.Clamp(num, -1f, 1f);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Snow set to " + WeatherManager.forceSnow.ToCultureInvariantString() + ". <end/>");
		}
		else if (_params[0].EqualsCaseInsensitive("snowfall"))
		{
			WeatherManager.forceSnowfall = Mathf.Clamp(num, -1f, 1f);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Snowfall set to " + WeatherManager.forceSnowfall.ToCultureInvariantString() + ". <end/>");
		}
		else if (_params[0].EqualsCaseInsensitive("wet"))
		{
			WeatherManager.forceWet = Mathf.Clamp(num, -1f, 1f);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wetness set to " + WeatherManager.forceWet.ToCultureInvariantString() + ". <end/>");
		}
		else if (_params[0].EqualsCaseInsensitive("temp"))
		{
			WeatherManager.forceTemperature = num;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Temperature set to " + WeatherManager.forceTemperature.ToCultureInvariantString() + ". <end/>");
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
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer != null)
				{
					WeatherManager.Instance.TriggerThunder(GameManager.Instance.World.GetWorldTime() + (ulong)num, primaryPlayer.position);
				}
			}
		}
		else if (_params[0].EqualsCaseInsensitive("globaltemp"))
		{
			WeatherManager.globalTemperatureOffset = num;
		}
		else if (_params[0].EqualsCaseInsensitive("snowacc"))
		{
			WeatherManager.SetSnowAccumulationSpeed(num);
		}
		else if (_params[0].EqualsCaseInsensitive("wind"))
		{
			WeatherManager.forceWind = Mathf.Clamp(num, -1f, 200f);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wind set to " + WeatherManager.forceWind.ToCultureInvariantString() + ". <end/>");
		}
		else if (_params[0].EqualsCaseInsensitive("simrand"))
		{
			WeatherManager.SetSimRandom(num);
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command not recognized. <end/>");
		}
	}
}

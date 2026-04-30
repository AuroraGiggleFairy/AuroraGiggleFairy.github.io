using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdWeatherSurvival : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "weathersurvival" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Enables/disables weather survival";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 0)
		{
			if (_params.Count > 1 || (_params[0] != "on" && _params[0] != "off"))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: weathersurvival [on/off]");
				return;
			}
			EntityStats.WeatherSurvivalEnabled = _params[0] == "on";
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Weather survival is " + (EntityStats.WeatherSurvivalEnabled ? "on" : "off"));
	}
}

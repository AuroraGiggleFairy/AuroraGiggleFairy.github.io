using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdNewWeatherSurvival : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "newweathersurvival" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Enables/disables new weather survival";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 0)
		{
			if (_params.Count > 1 || (_params[0] != "on" && _params[0] != "off"))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: newweathersurvival [on/off]");
				return;
			}
			EntityStats.NewWeatherSurvivalEnabled = _params[0] == "on";
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("New Weather survival is " + (EntityStats.NewWeatherSurvivalEnabled ? "on" : "off"));
	}
}

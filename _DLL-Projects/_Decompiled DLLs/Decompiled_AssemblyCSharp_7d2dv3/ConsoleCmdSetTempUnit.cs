using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSetTempUnit : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	public override int DefaultPermissionLevel => 1000;

	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "settempunit", "stu" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Set the current temperature units.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Set the current temperature units.\nUsage:\n  1. settempunit F\n  2. settempunit C\n1. sets the temperature unit to Fahrenheit.\n2. sets the temperature unit to Celsius.\n";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 1)
		{
			if (_params[0].EqualsCaseInsensitive("f"))
			{
				GamePrefs.SetObject(EnumGamePrefs.OptionsTempCelsius, false);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Set temperature units to Fahrenheit.");
			}
			else
			{
				if (!_params[0].EqualsCaseInsensitive("c"))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid value for single argument variant: \"" + _params[0] + "\"");
					return;
				}
				GamePrefs.SetObject(EnumGamePrefs.OptionsTempCelsius, true);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Set temperature units to Celsius.");
			}
			GamePrefs.Instance.Save();
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1, found " + _params.Count + ".");
		}
	}
}

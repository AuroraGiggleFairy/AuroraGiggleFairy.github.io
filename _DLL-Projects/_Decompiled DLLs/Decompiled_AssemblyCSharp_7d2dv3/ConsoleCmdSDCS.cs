using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSDCS : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum cTypes
	{
		Sex,
		Race,
		Variant
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum rTypes
	{
		White,
		Black,
		Asian,
		Native
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum sTypes
	{
		Male,
		Female
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Change a player's sex, race, and variant:\n  Usage:\n    sdcs                            : Show current archetype values\n    sdcs <sex|race|variant> <value> : Set the specified value\n  Examples :\n    sdcs sex <male|female>\n    sdcs race <white|black|asian|native>\n    sdcs variant <1|2|3|4>";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "sdcs" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Control entity sex, race, and variant";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.Instance.World.GetLocalPlayers().Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No local players found");
			return;
		}
		EModelSDCS component = GameManager.Instance.World.GetLocalPlayers()[0].GetComponent<EModelSDCS>();
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Current Archetype Values:\n  Sex: " + component.Archetype.Sex + "\n  Race: " + component.Archetype.Race + "\n  Variant: " + component.Archetype.Variant);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		if (!Enum.TryParse<cTypes>(_params[0], ignoreCase: true, out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid control type");
			return;
		}
		_ = _params[1];
		if (component == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No SDCS model found");
			return;
		}
		switch (result)
		{
		case cTypes.Sex:
		{
			if (!Enum.TryParse<sTypes>(_params[1], ignoreCase: true, out var result3))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sex '" + _params[1] + "'");
				break;
			}
			bool sex = false;
			if (result3 == sTypes.Male)
			{
				sex = true;
			}
			component.SetSex(sex);
			break;
		}
		case cTypes.Race:
		{
			if (!Enum.TryParse<rTypes>(_params[1], ignoreCase: true, out var result2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid race '" + _params[1] + "'");
			}
			else
			{
				component.SetRace(result2.ToString());
			}
			break;
		}
		case cTypes.Variant:
		{
			if (!StringParsers.TryParseSInt32(_params[1], out var _result))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid variant number " + _params[1]);
			}
			else if (_result < 1 || _result > 4)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Invalid variant number {_result}");
			}
			else
			{
				component.SetVariant(_result);
			}
			break;
		}
		}
	}
}

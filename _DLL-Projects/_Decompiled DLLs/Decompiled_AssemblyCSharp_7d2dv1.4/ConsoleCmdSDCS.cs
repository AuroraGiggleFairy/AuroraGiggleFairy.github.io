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
		Hispanic,
		MiddleEastern
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum sTypes
	{
		male,
		female
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Change a player's sex, race, and variant for SDCS testing\nUsage:\n   sdcs sex male\n   sdcs race white\n   sdcs variant 4\n";
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
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("sdcs requires a control type (sex, race, variant) and a value");
			return;
		}
		if (GameManager.Instance.World.GetLocalPlayers().Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No local players found");
			return;
		}
		EntityPlayer entityPlayer = GameManager.Instance.World.GetLocalPlayers()[0];
		if (!Enum.TryParse<cTypes>(_params[0], ignoreCase: true, out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid control type");
			return;
		}
		_ = _params[1];
		EModelSDCS component = entityPlayer.GetComponent<EModelSDCS>();
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
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid race '" + _params[1] + "'");
				break;
			}
			bool sex = false;
			if (result3 == sTypes.male)
			{
				sex = true;
			}
			component.SetSex(sex);
			component.SetRace("white");
			component.SetVariant(1);
			break;
		}
		case cTypes.Race:
		{
			if (!Enum.TryParse<rTypes>(_params[1], ignoreCase: true, out var _))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid race '" + _params[1] + "'");
				break;
			}
			component.SetRace(_params[1]);
			component.SetVariant(1);
			break;
		}
		case cTypes.Variant:
		{
			if (!StringParsers.TryParseSInt32(_params[1], out var _result))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid variant number " + _params[1]);
			}
			if (_result > 4)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Invalid variant number {_result}");
			}
			component.SetVariant(_result);
			break;
		}
		}
	}
}

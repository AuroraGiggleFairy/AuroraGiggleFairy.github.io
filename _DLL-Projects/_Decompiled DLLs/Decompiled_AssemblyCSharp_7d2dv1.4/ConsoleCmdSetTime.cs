using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSetTime : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "settime", "st" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Set the current game time";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Set the current game time.\nUsage:\n  1. settime day\n  2. settime night\n  3. settime <time>\n  4. settime <day> <hour> <minute>\n1. sets the time to day 1, 12:00 pm.\n2. sets the time to day 2, 12:00 am.\n3. sets the time to the given value. 1000 is one hour.\n4. sets the time to the given day/hour/minute values.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		ulong result = 0uL;
		if (_params.Count == 1)
		{
			if (_params[0].EqualsCaseInsensitive("day"))
			{
				result = GameUtils.DayTimeToWorldTime(1, 12, 0);
			}
			else if (_params[0].EqualsCaseInsensitive("night"))
			{
				result = GameUtils.DayTimeToWorldTime(2, 0, 0);
			}
			else if (!ulong.TryParse(_params[0], out result))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid value for single argument variant: \"" + _params[0] + "\"");
				return;
			}
		}
		else
		{
			if (_params.Count != 3)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1 or 3, found " + _params.Count + ".");
				return;
			}
			if (!int.TryParse(_params[0], out var result2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[0] + "\" is not a valid integer.");
				return;
			}
			if (!int.TryParse(_params[1], out var result3))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid integer.");
				return;
			}
			if (!int.TryParse(_params[2], out var result4))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[2] + "\" is not a valid integer.");
				return;
			}
			if (result2 < 1)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Day must be >= 1");
				return;
			}
			if (result3 > 23)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Hour must be <= 23");
				return;
			}
			if (result4 > 59)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Minute must be <= 59");
				return;
			}
			result = GameUtils.DayTimeToWorldTime(result2, result3, result4);
		}
		GameManager.Instance.World.SetTimeJump(result);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Set time to " + result);
	}
}

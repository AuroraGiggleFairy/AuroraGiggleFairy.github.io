using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSetTargetFps : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "settargetfps" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Set the target FPS the game should run at (upper limit)";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Set the target FPS the game should run at (upper limit).\nUsage:\n  1. settargetfps\n  2. settargetfps <fps>\n1. gets the current target FPS.\n2. sets the target FPS to the given integer value, 0 disables the FPS limiter.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			int targetFPS = GameManager.Instance.waitForTargetFPS.TargetFPS;
			if (targetFPS > 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Current FPS limit is " + targetFPS);
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("FPS limiter is currently disabled");
			}
		}
		else if (_params.Count == 1)
		{
			if (!int.TryParse(_params[0], out var result))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[0] + "\" is not a valid integer.");
				return;
			}
			if (result < 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("FPS must be >= 0");
				return;
			}
			GameManager.Instance.waitForTargetFPS.TargetFPS = result;
			if (result > 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Set FPS limit to " + result);
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Disabled target FPS limiter");
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 0 or 1, found " + _params.Count + ".");
		}
	}
}

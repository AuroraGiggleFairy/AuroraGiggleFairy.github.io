using System.Collections.Generic;
using UnityEngine.Scripting;

namespace MapRendering.Commands;

[Preserve]
public class EnableRendering : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Disable live map rendering";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\n\t\t\t|Usage:\n\t\t\t|  1. enablerendering\n\t\t\t|  2. enablerendering <0/1>\n\t\t\t|1. Show current state of renderer\n\t\t\t|2. Disable/enable renderer\n\t\t\t|NOTE: This command can only turn the renderer off, it can not turn it on if it is not enabled in the serverconfig!\n\t\t\t".Unindent();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "enablerendering" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Current state: {0}{1}", MapRenderer.Enabled && MapRenderer.renderingEnabled, (!MapRenderer.Enabled) ? " (disabled by serverconfig!)" : ""));
			return;
		}
		MapRenderer.renderingEnabled = _params[0].Equals("1");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Set live map rendering to {0}", _params[0].Equals("1")));
	}
}

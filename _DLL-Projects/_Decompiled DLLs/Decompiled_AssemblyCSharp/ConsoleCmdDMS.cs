using System.Collections.Generic;
using DynamicMusic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDMS : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "dms" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 0)
		{
			if (_params[0].ToLower() == "state")
			{
				Conductor dmsConductor = GameManager.Instance.World.dmsConductor;
				if (dmsConductor != null)
				{
					Log.Out($"dms exists with current state ${dmsConductor.CurrentSectionType}");
				}
				else
				{
					Log.Out("dms does not currently exist");
				}
			}
			else
			{
				Log.Out($"{_params[0]} is not a known parameter for 'dms'");
			}
		}
		else
		{
			Log.Out("a parameter is required to run a dms command. Call 'help dms' to see the list of available parameters.");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Gives control over Dynamic Music functionality.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "No commands available for dms at the moment.";
	}
}

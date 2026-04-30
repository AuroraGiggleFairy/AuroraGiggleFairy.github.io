using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDynamicProperties : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "dynamicproperties", "dprop" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params[0] == "block")
		{
			if (_params.Count == 1)
			{
				Debug.LogError("Needs sub-command - cachestats");
			}
			else if (_params[1] == "cachestats")
			{
				Block.CacheStats();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Dynamic Properties debugging";
	}
}

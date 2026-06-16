using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdLogFellThroughWorldDebugInfo : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "ftw", "fellthroughworld" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Log the fell through world debug information for testing purposes.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameManager.Instance.World.GetPrimaryPlayer().LogFellThroughWorldDebugInfo();
	}
}

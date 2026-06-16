using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdNewAvatarTest : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Test new HD stuff.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. na\n";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "na" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		Log.Warning("No New Avatar!");
	}
}

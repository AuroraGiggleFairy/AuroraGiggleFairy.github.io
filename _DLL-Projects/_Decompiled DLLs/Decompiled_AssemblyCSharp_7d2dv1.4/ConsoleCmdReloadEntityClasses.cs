using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdReloadEntityClasses : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "reloadentityclasses", "rec" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "reloads entityclasses xml data.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		WorldStaticData.Reset("entityclasses");
	}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGetLogfilePath : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "getlogpath", "glp" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Get the path of the logfile the game currently writes to";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(Application.consoleLogPath);
	}
}

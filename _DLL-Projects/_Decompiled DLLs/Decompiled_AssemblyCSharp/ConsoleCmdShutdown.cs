using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShutdown : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "shutdown" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Shutting server down...");
		Application.Quit();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "shuts down the game";
	}
}

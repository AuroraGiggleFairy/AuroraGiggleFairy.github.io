using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdListGameObjects : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "lgo", "listgameobjects" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "List all active game objects";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		int num = Object.FindObjectsOfType<Object>().Length;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"GOs: {num}, took {microStopwatch.ElapsedMilliseconds} ms");
	}
}

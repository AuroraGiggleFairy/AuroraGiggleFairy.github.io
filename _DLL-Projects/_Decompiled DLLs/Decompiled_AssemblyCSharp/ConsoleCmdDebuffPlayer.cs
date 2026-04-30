using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDebuffPlayer : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Remove a buff from a player";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n   debuffplayer <player name / steam id / entity id> <buff name>\nRemove the given buff from the player given by the player name or entity id (as given by e.g. \"lpi\").";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "debuffplayer" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid arguments, requires a target player and a buff name");
			ConsoleCmdBuff.PrintAvailableBuffNames();
			return;
		}
		string text = _params[1];
		ClientInfo clientInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
		if (clientInfo != null)
		{
			clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("debuff " + text, _bExecute: true));
		}
		else if (_senderInfo.IsLocalGame)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Use the \"debuff\" command for the local player.");
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Playername or entity ID not found.");
		}
	}
}

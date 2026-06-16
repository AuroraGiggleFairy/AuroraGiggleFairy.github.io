using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPPList : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "pplist" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(persistentPlayers.Players.Count + " Persistent Player(s)");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in persistentPlayers.Players)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   " + player.Key?.ToString() + " -> " + player.Value.EntityId);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Lists all PersistentPlayer data";
	}
}

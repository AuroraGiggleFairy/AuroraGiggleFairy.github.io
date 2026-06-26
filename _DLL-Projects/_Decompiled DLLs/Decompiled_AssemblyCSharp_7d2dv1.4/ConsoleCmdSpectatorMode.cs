using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSpectatorMode : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "enables/disables spectator mode";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[3] { "spectator", "spectatormode", "sm" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cannot execute spectatormode.");
		}
		if (_params.Count != 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid arguments");
			return;
		}
		EntityPlayer player = XUiM_Player.GetPlayer();
		if (player != null)
		{
			player.IsSpectator = !player.IsSpectator;
			player.bPlayerStatsChanged = true;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Spectator Mode: " + player.IsSpectator);
		}
	}
}

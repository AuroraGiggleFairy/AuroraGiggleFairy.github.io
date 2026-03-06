using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTeleportPoi : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "tppoi" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Open POI Teleporter window";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!_senderInfo.IsLocalGame)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on game clients");
		}
		else if (_params.Count == 0)
		{
			GameManager.Instance.SetConsoleWindowVisible(_b: false);
			LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.Open(XUiC_PoiTeleportMenu.ID, _bModal: true);
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not implemented yet");
		}
	}
}

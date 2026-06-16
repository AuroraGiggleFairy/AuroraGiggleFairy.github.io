using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSwitchView : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "switchview", "sv" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Switch between fpv and tpv";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!(GameManager.Instance.World.GetPrimaryPlayer() == null))
		{
			GameManager.Instance.World.GetPrimaryPlayer().SwitchFirstPersonViewFromInput();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Switched to " + (GameManager.Instance.World.GetPrimaryPlayer().bFirstPersonView ? "FPV" : "TPV"));
		}
	}
}

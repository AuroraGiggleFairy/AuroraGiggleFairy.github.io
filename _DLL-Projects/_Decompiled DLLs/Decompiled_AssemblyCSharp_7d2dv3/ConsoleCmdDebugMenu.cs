using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDebugMenu : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "debugmenu", "dm" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GamePrefs.Set(EnumGamePrefs.DebugMenuEnabled, !GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("debugmenu " + (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) ? "on" : "off"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "enables/disables the debugmenu ";
	}
}

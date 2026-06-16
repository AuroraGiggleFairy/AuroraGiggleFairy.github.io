using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCensor : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "testCensor", "tcc" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Censorship testing toggle.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && _params.Count == 0)
		{
			GameManager.DebugCensorship = !GameManager.DebugCensorship;
			Log.Out("Censor testing enabled: " + GameManager.DebugCensorship);
		}
	}
}

using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdResetAchievementStats : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "resetallstats" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Resets all achievement stats (and achievements when parameter is true)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("cannot execute resetallstats on dedicated server, please execute as a client");
		}
		else
		{
			PlatformManager.NativePlatform.AchievementManager?.ResetStats(_params.Count > 0 && ConsoleHelper.ParseParamBool(_params[0], _invalidStringsAsFalse: true));
		}
	}
}

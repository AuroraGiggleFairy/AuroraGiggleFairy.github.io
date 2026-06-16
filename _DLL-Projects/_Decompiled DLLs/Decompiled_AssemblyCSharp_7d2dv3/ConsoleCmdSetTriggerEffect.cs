using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSetTriggerEffect : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "sette" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Sets the UseTriggerEffects flag, if true controller trigger effects are to be used";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Client only setting, please execute as a client");
			return;
		}
		if (_params.Count > 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: sette <on/true/y/1/off/false/n/0>");
			return;
		}
		bool flag = false;
		if (_params.Count != 0)
		{
			flag = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerTriggerEffects);
		}
		if (_params.Count == 1)
		{
			try
			{
				flag = ConsoleHelper.ParseParamBool(_params[0]);
			}
			catch
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: sette <on/true/y/1/off/false/n/0>");
				return;
			}
		}
		GamePrefs.Set(EnumGamePrefs.OptionsControllerTriggerEffects, flag);
		GamePrefs.Instance.Save();
		foreach (EntityPlayerLocal localPlayer in GameManager.Instance.World.GetLocalPlayers())
		{
			_ = localPlayer;
			GameManager.Instance.triggerEffectManager.PollSetting();
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"UseTriggerEffects now set to: {flag}");
	}
}

using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSelfExp : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "giveselfxp" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Give yourself experience\nUsage:\n   giveselfxp <number> [1 (use xp bonuses)]";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("cannot execute giveselfxp on dedicated server, please execute as a client");
		}
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("giveselfxp requires xp amount");
			return;
		}
		if (!float.TryParse(_params[0], out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("xp amount must be a number.");
			return;
		}
		if (result < 0f)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("xp amount must be positive.");
			return;
		}
		result = Mathf.Clamp(result, 0f, 1.0737418E+09f);
		bool useBonus = _params.Count >= 2;
		GameManager.Instance.World.GetPrimaryPlayer().Progression.AddLevelExp((int)result, "_xpOther", Progression.XPTypes.Debug, useBonus);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "usage: giveselfxp 10000";
	}
}

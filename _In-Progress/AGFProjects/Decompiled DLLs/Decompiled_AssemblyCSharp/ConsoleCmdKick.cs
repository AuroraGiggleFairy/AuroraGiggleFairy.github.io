using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdKick : ConsoleCmdAbstract
{
	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "kick" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Kicks user with optional reason. \"kick playername reason\"";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		PlatformUserIdentifierAbs _id;
		ClientInfo _cInfo;
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected at least 1, found " + _params.Count + ".");
		}
		else if (ConsoleHelper.ParseParamPartialNameOrId(_params[0], out _id, out _cInfo) == 1 && _cInfo != null)
		{
			string text = string.Empty;
			if (_params.Count > 1)
			{
				text = _params[1];
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Kicking Player " + _cInfo.playerName + ": " + text);
			ClientInfo cInfo = _cInfo;
			string customReason = text;
			GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, 0, default(DateTime), customReason));
		}
	}
}

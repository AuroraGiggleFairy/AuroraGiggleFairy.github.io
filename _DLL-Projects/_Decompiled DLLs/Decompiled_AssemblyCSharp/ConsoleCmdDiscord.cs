using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDiscord : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "discord", "dc" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Toggle Discord debug window";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		LocalPlayerUI.primaryUI.windowManager.SwitchVisible(XUiC_DiscordWindow.ID, _bIsNotEscClosable: true, _modal: false);
	}
}

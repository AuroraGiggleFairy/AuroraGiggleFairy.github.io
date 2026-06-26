using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdVersionUi : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "versionui" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Toggle version number display";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		NGUIWindowManager nguiWindowManager = LocalPlayerUI.primaryUI.nguiWindowManager;
		nguiWindowManager.AlwaysShowVersionUi = !nguiWindowManager.AlwaysShowVersionUi;
	}
}

using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCreativeMenu : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "creativemenu", "cm" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GamePrefs.Set(EnumGamePrefs.CreativeMenuEnabled, !GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled));
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("creativemenu " + (GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled) ? "on" : "off"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "enables/disables the creativemenu";
	}
}

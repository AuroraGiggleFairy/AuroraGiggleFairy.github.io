using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSaveWorld : ConsoleCmdAbstract
{
	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "saveworld", "sa" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.SaveLocalPlayerData();
			GameManager.Instance.SaveWorld();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("World saved");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Saves the world manually.";
	}
}

using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdListPlayerIds : ConsoleCmdAbstract
{
	public override int DefaultPermissionLevel => 1000;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "listplayerids", "lpi" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Lists all players with their IDs for ingame commands";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		World world = GameManager.Instance.World;
		int num = 0;
		foreach (KeyValuePair<int, EntityPlayer> item in world.Players.dict)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{++num}. id={item.Value.entityId}, {item.Value.EntityName}");
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Total of " + world.Players.list.Count + " in the game");
	}
}

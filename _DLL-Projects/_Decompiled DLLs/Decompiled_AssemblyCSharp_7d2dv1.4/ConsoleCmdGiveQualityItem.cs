using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGiveQualityItem : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "giveself" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cannot execute giveself on dedicated server, please execute as a client");
			return;
		}
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("giveself requires an itemname as parameter");
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		ItemValue item = ItemClass.GetItem(_params[0], _caseInsensitive: true);
		if (item.type == ItemValue.None.type)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown itemname given");
			return;
		}
		int result = 6;
		if (_params.Count > 1 && !int.TryParse(_params[1], out result))
		{
			result = 6;
		}
		int result2 = 1;
		if (_params.Count > 2 && !int.TryParse(_params[2], out result2))
		{
			result2 = 1;
		}
		bool _result = false;
		if (_params.Count > 3 && !StringParsers.TryParseBool(_params[3], out _result))
		{
			_result = false;
		}
		bool _result2 = true;
		if (_params.Count > 4 && !StringParsers.TryParseBool(_params[4], out _result2))
		{
			_result2 = true;
		}
		for (int i = 0; i < result2; i++)
		{
			ItemStack itemStack = new ItemStack(new ItemValue(item.type, result, result, _result2), 1);
			if (!_result)
			{
				GameManager.Instance.ItemDropServer(itemStack, primaryPlayer.position, Vector3.zero);
			}
			else
			{
				primaryPlayer.bag.AddItem(itemStack);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "usage: giveself itemName [qualityLevel=" + (ushort)6 + "] [count=1] [putInInventory=false] [spawnWithMods=true]";
	}
}

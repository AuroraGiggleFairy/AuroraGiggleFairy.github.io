using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCommandUnlockInventories : ConsoleCmdAbstract
{
	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override bool IsExecuteOnClient => false;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Force unlock inventories for everyone or a specific player.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  unlock\n  unlock <player id>\n";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "unlock" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		int result = -1;
		if (_params.Count == 2 && !int.TryParse(_params[1], out result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to parse int for player id.");
		}
		else if (result == -1)
		{
			LockManager.Instance.ForceUnlockAll();
		}
		else
		{
			LockManager.Instance.ForceUnlockByPlayer(result);
		}
	}
}

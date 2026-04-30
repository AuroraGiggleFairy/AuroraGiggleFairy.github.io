using System.Collections.Generic;
using Platform;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTwitchAdminCommand : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "twitchadmin" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!TwitchManager.Current.IsReady)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Twitch must be active to use this command!");
		}
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(getHelp());
		}
		else if (_params.Count == 1)
		{
			switch (_params[0])
			{
			case "cleanup":
				TwitchManager.Current.ViewerData.Cleanup();
				break;
			case "export":
				TwitchManager.Current.SaveExportViewerData();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Exporting Viewer Data");
				break;
			case "import":
				TwitchManager.Current.LoadExportViewerData();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Importing Viewer Data");
				break;
			case "pointstotal":
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Twitch Viewer Data Point Totals:\n" + TwitchManager.Current.ViewerData.GetPointTotals());
				break;
			}
		}
		else if (_params.Count == 3 && _params[0] == "reset" && _params[1] == "all")
		{
			switch (_params[2])
			{
			case "both":
				TwitchManager.Current.ViewerData.ResetAllPoints();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Resetting all Twitch PP and SP");
				break;
			case "sp":
				TwitchManager.Current.ViewerData.ResetAllSpecialPoints();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Resetting all Twitch SP");
				break;
			case "pp":
				TwitchManager.Current.ViewerData.ResetAllStandardPoints();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Resetting all Twitch PP");
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Twitch Admin Commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. twitchadmin cleanup\n  2. twitchadmin export\n  3. twitchadmin import\n  4. twitchadmin pointtotals\n  5. twitchadmin reset all both\n  6. twitchadmin reset all pp\n  7. twitchadmin reset all sp\n1. Cleans up duplicate Viewer Data.\n2. Exports all the users to a comma delimited twitchusers.xml\n3. Imports all the users to a comma delimited twitchusers.xml\n4. Prints out the totals for PP, SP, and BitCredit\n5. Clears all PP and SP for all users\n6. Clears all PP for all users\n7. Clears all SP for all users";
	}
}

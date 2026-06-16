using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdVersion : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "version" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Get the currently running version of the game and loaded mods";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Game version: " + Constants.cVersionInformation.LongString + " Compatibility Version: " + Constants.cVersionInformation.LongStringNoBuild);
		for (int i = 0; i < ModManager.GetLoadedMods().Count; i++)
		{
			Mod mod = ModManager.GetLoadedMods()[i];
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Mod " + mod.Name + ": " + (mod.VersionString ?? "<unknown version>"));
		}
	}
}

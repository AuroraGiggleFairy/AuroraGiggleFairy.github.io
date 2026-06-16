using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdFallingBlocks : ConsoleCmdAbstract
{
	public static bool ClientVerification = true;

	public static bool AllowSendToPeer = true;

	public static bool AllowReceiveFromPeer = true;

	public static bool DisablePeerAction = false;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "fallingblocks", "fb" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "FallingBlocks WIP Settings";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params[0].ToLowerInvariant() == "enable")
		{
			EntityFallingBlocks.Enabled = !EntityFallingBlocks.Enabled;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"EntityFallingBlocks Feature Enabled: {EntityFallingBlocks.Enabled}");
		}
	}
}

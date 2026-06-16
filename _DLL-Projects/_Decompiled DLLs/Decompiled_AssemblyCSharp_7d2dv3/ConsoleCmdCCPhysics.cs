using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCCPhysics : ConsoleCmdAbstract
{
	public static bool ClientVerification = true;

	public static bool AllowSendToPeer = true;

	public static bool AllowReceiveFromPeer = true;

	public static bool DisablePeerAction = false;

	public static bool EnableCCPhysicsChanges = true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "ccphysics" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Enables or disables changes to CCPhysics layer interactions. Reloading the game session may be necessary to fully apply if changed.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			EnableCCPhysicsChanges = !EnableCCPhysicsChanges;
		}
		else
		{
			string text = _params[0].ToLowerInvariant();
			if (!(text == "on"))
			{
				if (!(text == "off"))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unrecognised subcommand. Use 'on' or 'off' subcommand, or none to toggle.");
					return;
				}
				EnableCCPhysicsChanges = false;
			}
			else
			{
				EnableCCPhysicsChanges = true;
			}
		}
		Physics.IgnoreLayerCollision(20, 15, EnableCCPhysicsChanges);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Physics layer interaction changes " + (EnableCCPhysicsChanges ? "enabled" : "disabled") + ". Reloading the game session may be necessary to fully apply.");
	}
}

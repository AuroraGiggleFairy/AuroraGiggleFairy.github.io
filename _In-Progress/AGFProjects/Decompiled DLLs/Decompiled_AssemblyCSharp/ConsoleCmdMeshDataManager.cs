using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdMeshDataManager : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	public override bool AllowedInMainMenu => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "meshdatamanager", "mdm" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Toggle the MeshDataManager";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "mdm";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		MeshDataManager.Enabled = !MeshDataManager.Enabled;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("MeshDataManager " + (MeshDataManager.Enabled ? "enabled" : "disabled") + ".");
	}
}

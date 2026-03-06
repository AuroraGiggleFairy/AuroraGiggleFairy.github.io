using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdListDLC : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "listdlc", "dlcs" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "List the available DLC and their current entitlement status.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "List DLCs and their entitlement states.\nUsage:\n   listdlc\n   dlcs\n";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		ExecuteList();
	}

	public static void ExecuteList()
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("DLC List:");
		foreach (EntitlementSetEnum value in Enum.GetValues(typeof(EntitlementSetEnum)))
		{
			if (value != EntitlementSetEnum.None)
			{
				(bool hasOverride, bool overrideValue) tuple = EntitlementManager.Instance.CheckOverride(value);
				bool item = tuple.hasOverride;
				bool item2 = tuple.overrideValue;
				bool flag = EntitlementManager.Instance.HasEntitlement(value);
				bool flag2 = EntitlementManager.Instance.IsAvailableOnPlatform(value);
				bool flag3 = EntitlementManager.Instance.IsEntitlementPurchasable(value);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("  {0}: {1} => Entitlement State: {2}, Available on Platform: {3}, Purchasable: {4}, Override State: {5}", (int)value, value, flag, flag2, flag3, item ? item2.ToString() : "Not Overridden"));
			}
		}
	}
}

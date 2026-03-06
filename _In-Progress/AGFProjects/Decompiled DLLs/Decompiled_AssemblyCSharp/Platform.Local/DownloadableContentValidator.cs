using System;

namespace Platform.Local;

public class DownloadableContentValidator : IEntitlementValidator
{
	public void Init(IPlatform _owner)
	{
	}

	public bool IsAvailableOnPlatform(EntitlementSetEnum _dlcSet)
	{
		return false;
	}

	public bool HasEntitlement(EntitlementSetEnum _dlcSet)
	{
		return false;
	}

	public bool IsEntitlementPurchasable(EntitlementSetEnum _dlcSet)
	{
		return false;
	}

	public bool OpenStore(EntitlementSetEnum _dlcSet, Action<EntitlementSetEnum> _onDlcPurchased)
	{
		return false;
	}
}

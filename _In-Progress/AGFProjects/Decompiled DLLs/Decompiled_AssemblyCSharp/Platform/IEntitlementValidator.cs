using System;

namespace Platform;

public interface IEntitlementValidator
{
	void Init(IPlatform _owner);

	bool HasEntitlement(EntitlementSetEnum _entitlementSet);

	bool IsAvailableOnPlatform(EntitlementSetEnum _entitlementSet);

	bool IsEntitlementPurchasable(EntitlementSetEnum _entitlementSet);

	bool OpenStore(EntitlementSetEnum _entitlementSet, Action<EntitlementSetEnum> _onPurchased);
}

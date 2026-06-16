using System;
using System.Collections.Generic;

namespace Twitch;

[Serializable]
public class EntitlementListWrapper
{
	public List<Entitlement> entitlements = new List<Entitlement>();
}

using System;

namespace Twitch;

[Serializable]
public class Entitlement
{
	public string id;

	public string benefit_id;

	public string fulfillment_status;
}

using System;

namespace Twitch;

[Serializable]
public class ExtensionBitAction : ExtensionAction
{
	public string txn_id;

	public long time_created;

	public int cost;
}

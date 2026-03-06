using System.Collections.Generic;

namespace Twitch;

public class PubSubStatusRequestData
{
	public string broadcaster_id;

	public List<string> target;

	public string message;
}

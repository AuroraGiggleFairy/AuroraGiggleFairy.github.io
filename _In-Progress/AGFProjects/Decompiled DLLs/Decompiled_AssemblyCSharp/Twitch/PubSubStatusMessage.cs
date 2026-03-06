using System;
using System.Collections.Generic;

namespace Twitch;

[Serializable]
public class PubSubStatusMessage
{
	public string opaqueId;

	public string displayName;

	public string status;

	public List<string> party;

	public string commands;

	public string language;

	public List<string> actionTypes;
}

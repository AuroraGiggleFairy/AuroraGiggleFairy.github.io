using System;

namespace Twitch;

[Serializable]
public class SetConfigRequestData
{
	public string extension_id;

	public string broadcaster_id;

	public string segment;

	public string version;

	public string content;
}

namespace Twitch;

public class TwitchMessageEntry
{
	public string Message = "";

	public string Sound = "";

	public TwitchMessageEntry(string msg, string sound)
	{
		Message = msg;
		Sound = sound;
	}
}

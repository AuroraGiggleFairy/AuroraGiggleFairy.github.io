namespace Twitch;

public class TwitchLeaderboardEntry
{
	public string UserName;

	public string UserColor;

	public int Kills;

	public TwitchLeaderboardEntry(string username, string usercolor, int kills)
	{
		UserName = username;
		UserColor = ((usercolor == null) ? "FFFFFF" : usercolor);
		Kills = kills;
	}
}

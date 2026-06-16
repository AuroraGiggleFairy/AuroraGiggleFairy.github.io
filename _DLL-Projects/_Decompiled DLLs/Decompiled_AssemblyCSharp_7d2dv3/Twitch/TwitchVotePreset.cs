namespace Twitch;

public class TwitchVotePreset
{
	public string Name;

	public bool IsDefault;

	public bool IsEmpty;

	public string Title;

	public string Description;

	public TwitchVotingManager.BossVoteSettings BossVoteSetting = TwitchVotingManager.BossVoteSettings.Standard;
}

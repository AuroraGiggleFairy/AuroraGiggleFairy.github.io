namespace Discord;

internal static class GuildExtensions
{
	public static bool GetWelcomeMessagesEnabled(this IGuild guild)
	{
		return !guild.SystemChannelFlags.HasFlag(SystemChannelMessageDeny.WelcomeMessage);
	}

	public static bool GetGuildBoostMessagesEnabled(this IGuild guild)
	{
		return !guild.SystemChannelFlags.HasFlag(SystemChannelMessageDeny.GuildBoost);
	}

	public static bool GetGuildSetupTipMessagesEnabled(this IGuild guild)
	{
		return !guild.SystemChannelFlags.HasFlag(SystemChannelMessageDeny.GuildSetupTip);
	}

	public static bool GetGuildWelcomeMessageReplyEnabled(this IGuild guild)
	{
		return !guild.SystemChannelFlags.HasFlag(SystemChannelMessageDeny.WelcomeMessageReply);
	}
}

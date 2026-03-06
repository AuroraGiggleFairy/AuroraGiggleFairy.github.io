namespace Discord.Interactions;

internal class DefaultMentionableConverter<T> : DefaultEntityTypeConverter<T> where T : class, IMentionable
{
	public override ApplicationCommandOptionType GetDiscordType()
	{
		return ApplicationCommandOptionType.Mentionable;
	}
}

namespace Discord.Interactions;

internal class DefaultUserConverter<T> : DefaultEntityTypeConverter<T> where T : class, IUser
{
	public override ApplicationCommandOptionType GetDiscordType()
	{
		return ApplicationCommandOptionType.User;
	}
}

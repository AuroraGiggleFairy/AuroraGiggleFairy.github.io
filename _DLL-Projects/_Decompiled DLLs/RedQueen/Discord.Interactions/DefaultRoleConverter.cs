namespace Discord.Interactions;

internal class DefaultRoleConverter<T> : DefaultEntityTypeConverter<T> where T : class, IRole
{
	public override ApplicationCommandOptionType GetDiscordType()
	{
		return ApplicationCommandOptionType.Role;
	}
}

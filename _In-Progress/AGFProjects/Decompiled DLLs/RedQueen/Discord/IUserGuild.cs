namespace Discord;

internal interface IUserGuild : IDeletable, ISnowflakeEntity, IEntity<ulong>
{
	string Name { get; }

	string IconUrl { get; }

	bool IsOwner { get; }

	GuildPermissions Permissions { get; }
}

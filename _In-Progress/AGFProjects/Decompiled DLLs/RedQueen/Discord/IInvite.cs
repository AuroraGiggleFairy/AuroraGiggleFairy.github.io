namespace Discord;

internal interface IInvite : IEntity<string>, IDeletable
{
	string Code { get; }

	string Url { get; }

	IUser Inviter { get; }

	IChannel Channel { get; }

	ChannelType ChannelType { get; }

	ulong ChannelId { get; }

	string ChannelName { get; }

	IGuild Guild { get; }

	ulong? GuildId { get; }

	string GuildName { get; }

	int? PresenceCount { get; }

	int? MemberCount { get; }

	IUser TargetUser { get; }

	TargetUserType TargetUserType { get; }
}

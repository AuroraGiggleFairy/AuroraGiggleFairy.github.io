using System;
using Discord.API;

namespace Discord.Rest;

internal class RestInviteMetadata : RestInvite, IInviteMetadata, IInvite, IEntity<string>, IDeletable
{
	private long _createdAtTicks;

	public bool IsTemporary { get; private set; }

	public int? MaxAge { get; private set; }

	public int? MaxUses { get; private set; }

	public int? Uses { get; private set; }

	public DateTimeOffset? CreatedAt => DateTimeUtils.FromTicks(_createdAtTicks);

	internal RestInviteMetadata(BaseDiscordClient discord, IGuild guild, IChannel channel, string id)
		: base(discord, guild, channel, id)
	{
	}

	internal static RestInviteMetadata Create(BaseDiscordClient discord, IGuild guild, IChannel channel, InviteMetadata model)
	{
		RestInviteMetadata restInviteMetadata = new RestInviteMetadata(discord, guild, channel, model.Code);
		restInviteMetadata.Update(model);
		return restInviteMetadata;
	}

	internal void Update(InviteMetadata model)
	{
		Update((Invite)model);
		IsTemporary = model.Temporary;
		MaxAge = model.MaxAge;
		MaxUses = model.MaxUses;
		Uses = model.Uses;
		_createdAtTicks = model.CreatedAt.UtcTicks;
	}
}

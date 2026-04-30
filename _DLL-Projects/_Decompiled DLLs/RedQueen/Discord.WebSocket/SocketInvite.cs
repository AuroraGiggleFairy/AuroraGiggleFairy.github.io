using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API.Gateway;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketInvite : SocketEntity<string>, IInviteMetadata, IInvite, IEntity<string>, IDeletable
{
	private long _createdAtTicks;

	public ulong ChannelId { get; private set; }

	public SocketGuildChannel Channel { get; private set; }

	public ulong? GuildId { get; private set; }

	public SocketGuild Guild { get; private set; }

	ChannelType IInvite.ChannelType
	{
		get
		{
			SocketGuildChannel channel = Channel;
			if (!(channel is IVoiceChannel))
			{
				if (!(channel is ICategoryChannel))
				{
					if (!(channel is IDMChannel))
					{
						if (!(channel is IGroupChannel))
						{
							if (!(channel is INewsChannel))
							{
								if (channel is ITextChannel)
								{
									return ChannelType.Text;
								}
								throw new InvalidOperationException("Invalid channel type.");
							}
							return ChannelType.News;
						}
						return ChannelType.Group;
					}
					return ChannelType.DM;
				}
				return ChannelType.Category;
			}
			return ChannelType.Voice;
		}
	}

	string IInvite.ChannelName => Channel.Name;

	string IInvite.GuildName => Guild.Name;

	int? IInvite.PresenceCount
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	int? IInvite.MemberCount
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsTemporary { get; private set; }

	int? IInviteMetadata.MaxAge => MaxAge;

	int? IInviteMetadata.MaxUses => MaxUses;

	int? IInviteMetadata.Uses => Uses;

	public int MaxAge { get; private set; }

	public int MaxUses { get; private set; }

	public int Uses { get; private set; }

	public SocketGuildUser Inviter { get; private set; }

	DateTimeOffset? IInviteMetadata.CreatedAt => DateTimeUtils.FromTicks(_createdAtTicks);

	public DateTimeOffset CreatedAt => DateTimeUtils.FromTicks(_createdAtTicks);

	public SocketUser TargetUser { get; private set; }

	public TargetUserType TargetUserType { get; private set; }

	public string Code => base.Id;

	public string Url => "https://discord.gg/" + Code;

	private string DebuggerDisplay => Url + " (" + Guild?.Name + " / " + Channel.Name + ")";

	IGuild IInvite.Guild => Guild;

	IChannel IInvite.Channel => Channel;

	IUser IInvite.Inviter => Inviter;

	IUser IInvite.TargetUser => TargetUser;

	internal SocketInvite(DiscordSocketClient discord, SocketGuild guild, SocketGuildChannel channel, SocketGuildUser inviter, SocketUser target, string id)
		: base(discord, id)
	{
		Guild = guild;
		Channel = channel;
		Inviter = inviter;
		TargetUser = target;
	}

	internal static SocketInvite Create(DiscordSocketClient discord, SocketGuild guild, SocketGuildChannel channel, SocketGuildUser inviter, SocketUser target, InviteCreateEvent model)
	{
		SocketInvite socketInvite = new SocketInvite(discord, guild, channel, inviter, target, model.Code);
		socketInvite.Update(model);
		return socketInvite;
	}

	internal void Update(InviteCreateEvent model)
	{
		ChannelId = model.ChannelId;
		GuildId = (model.GuildId.IsSpecified ? model.GuildId.Value : Guild.Id);
		IsTemporary = model.Temporary;
		MaxAge = model.MaxAge;
		MaxUses = model.MaxUses;
		Uses = model.Uses;
		_createdAtTicks = model.CreatedAt.UtcTicks;
		TargetUserType = (model.TargetUserType.IsSpecified ? model.TargetUserType.Value : TargetUserType.Undefined);
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return InviteHelper.DeleteAsync(this, base.Discord, options);
	}

	public override string ToString()
	{
		return Url;
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.WebSocket;

internal class SocketThreadUser : SocketUser, IThreadUser, IMentionable, IGuildUser, IUser, ISnowflakeEntity, IEntity<ulong>, IPresence, IVoiceState
{
	public SocketThreadChannel Thread { get; private set; }

	public DateTimeOffset ThreadJoinedAt { get; private set; }

	public SocketGuild Guild { get; private set; }

	public DateTimeOffset? JoinedAt => GuildUser.JoinedAt;

	public string DisplayName => GuildUser.Nickname ?? GuildUser.Username;

	public string Nickname => GuildUser.Nickname;

	public DateTimeOffset? PremiumSince => GuildUser.PremiumSince;

	public DateTimeOffset? TimedOutUntil => GuildUser.TimedOutUntil;

	public bool? IsPending => GuildUser.IsPending;

	public int Hierarchy => GuildUser.Hierarchy;

	public override string AvatarId
	{
		get
		{
			return GuildUser.AvatarId;
		}
		internal set
		{
			GuildUser.AvatarId = value;
		}
	}

	public string DisplayAvatarId => GuildAvatarId ?? AvatarId;

	public string GuildAvatarId => GuildUser.GuildAvatarId;

	public override ushort DiscriminatorValue
	{
		get
		{
			return GuildUser.DiscriminatorValue;
		}
		internal set
		{
			GuildUser.DiscriminatorValue = value;
		}
	}

	public override bool IsBot
	{
		get
		{
			return GuildUser.IsBot;
		}
		internal set
		{
			GuildUser.IsBot = value;
		}
	}

	public override bool IsWebhook => GuildUser.IsWebhook;

	public override string Username
	{
		get
		{
			return GuildUser.Username;
		}
		internal set
		{
			GuildUser.Username = value;
		}
	}

	public bool IsDeafened => GuildUser.IsDeafened;

	public bool IsMuted => GuildUser.IsMuted;

	public bool IsSelfDeafened => GuildUser.IsSelfDeafened;

	public bool IsSelfMuted => GuildUser.IsSelfMuted;

	public bool IsSuppressed => GuildUser.IsSuppressed;

	public IVoiceChannel VoiceChannel => GuildUser.VoiceChannel;

	public string VoiceSessionId => GuildUser.VoiceSessionId;

	public bool IsStreaming => GuildUser.IsStreaming;

	public bool IsVideoing => GuildUser.IsVideoing;

	public DateTimeOffset? RequestToSpeakTimestamp => GuildUser.RequestToSpeakTimestamp;

	private SocketGuildUser GuildUser { get; set; }

	IThreadChannel IThreadUser.Thread => Thread;

	IGuild IThreadUser.Guild => Guild;

	IGuild IGuildUser.Guild => Guild;

	ulong IGuildUser.GuildId => Guild.Id;

	GuildPermissions IGuildUser.GuildPermissions => GuildUser.GuildPermissions;

	IReadOnlyCollection<ulong> IGuildUser.RoleIds => GuildUser.Roles.Select((SocketRole x) => x.Id).ToImmutableArray();

	internal override SocketGlobalUser GlobalUser
	{
		get
		{
			return GuildUser.GlobalUser;
		}
		set
		{
			GuildUser.GlobalUser = value;
		}
	}

	internal override SocketPresence Presence
	{
		get
		{
			return GuildUser.Presence;
		}
		set
		{
			GuildUser.Presence = value;
		}
	}

	internal SocketThreadUser(SocketGuild guild, SocketThreadChannel thread, SocketGuildUser member, ulong userId)
		: base(guild.Discord, userId)
	{
		Thread = thread;
		Guild = guild;
		GuildUser = member;
	}

	internal static SocketThreadUser Create(SocketGuild guild, SocketThreadChannel thread, ThreadMember model, SocketGuildUser member)
	{
		SocketThreadUser socketThreadUser = new SocketThreadUser(guild, thread, member, model.UserId.Value);
		socketThreadUser.Update(model);
		return socketThreadUser;
	}

	internal static SocketThreadUser Create(SocketGuild guild, SocketThreadChannel thread, SocketGuildUser owner)
	{
		SocketThreadUser socketThreadUser = new SocketThreadUser(guild, thread, owner, owner.Id);
		socketThreadUser.Update(new ThreadMember
		{
			JoinTimestamp = thread.CreatedAt
		});
		return socketThreadUser;
	}

	internal void Update(ThreadMember model)
	{
		ThreadJoinedAt = model.JoinTimestamp;
	}

	public ChannelPermissions GetPermissions(IGuildChannel channel)
	{
		return GuildUser.GetPermissions(channel);
	}

	public Task KickAsync(string reason = null, RequestOptions options = null)
	{
		return GuildUser.KickAsync(reason, options);
	}

	public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null)
	{
		return GuildUser.ModifyAsync(func, options);
	}

	public Task AddRoleAsync(ulong roleId, RequestOptions options = null)
	{
		return GuildUser.AddRoleAsync(roleId, options);
	}

	public Task AddRoleAsync(IRole role, RequestOptions options = null)
	{
		return GuildUser.AddRoleAsync(role, options);
	}

	public Task AddRolesAsync(IEnumerable<ulong> roleIds, RequestOptions options = null)
	{
		return GuildUser.AddRolesAsync(roleIds, options);
	}

	public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
	{
		return GuildUser.AddRolesAsync(roles, options);
	}

	public Task RemoveRoleAsync(ulong roleId, RequestOptions options = null)
	{
		return GuildUser.RemoveRoleAsync(roleId, options);
	}

	public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
	{
		return GuildUser.RemoveRoleAsync(role, options);
	}

	public Task RemoveRolesAsync(IEnumerable<ulong> roleIds, RequestOptions options = null)
	{
		return GuildUser.RemoveRolesAsync(roleIds, options);
	}

	public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
	{
		return GuildUser.RemoveRolesAsync(roles, options);
	}

	public Task SetTimeOutAsync(TimeSpan span, RequestOptions options = null)
	{
		return GuildUser.SetTimeOutAsync(span, options);
	}

	public Task RemoveTimeOutAsync(RequestOptions options = null)
	{
		return GuildUser.RemoveTimeOutAsync(options);
	}

	string IGuildUser.GetDisplayAvatarUrl(ImageFormat format, ushort size)
	{
		return GuildUser.GetDisplayAvatarUrl(format, size);
	}

	string IGuildUser.GetGuildAvatarUrl(ImageFormat format, ushort size)
	{
		return GuildUser.GetGuildAvatarUrl(format, size);
	}

	public static explicit operator SocketGuildUser(SocketThreadUser user)
	{
		return user.GuildUser;
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestGuildUser : RestUser, IGuildUser, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IVoiceState
{
	private long? _premiumSinceTicks;

	private long? _timedOutTicks;

	private long? _joinedAtTicks;

	private System.Collections.Immutable.ImmutableArray<ulong> _roleIds;

	public string DisplayName => Nickname ?? base.Username;

	public string Nickname { get; private set; }

	public string DisplayAvatarId => GuildAvatarId ?? base.AvatarId;

	public string GuildAvatarId { get; private set; }

	internal IGuild Guild { get; private set; }

	public bool IsDeafened { get; private set; }

	public bool IsMuted { get; private set; }

	public DateTimeOffset? PremiumSince => DateTimeUtils.FromTicks(_premiumSinceTicks);

	public ulong GuildId { get; }

	public bool? IsPending { get; private set; }

	public int Hierarchy
	{
		get
		{
			if (Guild.OwnerId == base.Id)
			{
				return int.MaxValue;
			}
			return (from x in Guild.Roles
				orderby x.Position descending
				where RoleIds.Contains(x.Id)
				select x).Max((IRole x) => x.Position);
		}
	}

	public DateTimeOffset? TimedOutUntil
	{
		get
		{
			if (!_timedOutTicks.HasValue || _timedOutTicks.Value < 0)
			{
				return null;
			}
			return DateTimeUtils.FromTicks(_timedOutTicks);
		}
	}

	public GuildPermissions GuildPermissions
	{
		get
		{
			if (!Guild.Available)
			{
				throw new InvalidOperationException("Resolving permissions requires the parent guild to be downloaded.");
			}
			return new GuildPermissions(Permissions.ResolveGuild(Guild, this));
		}
	}

	public IReadOnlyCollection<ulong> RoleIds => _roleIds;

	public DateTimeOffset? JoinedAt => DateTimeUtils.FromTicks(_joinedAtTicks);

	IGuild IGuildUser.Guild
	{
		get
		{
			if (Guild != null)
			{
				return Guild;
			}
			throw new InvalidOperationException("Unable to return this entity's parent unless it was fetched through that object.");
		}
	}

	bool IVoiceState.IsSelfDeafened => false;

	bool IVoiceState.IsSelfMuted => false;

	bool IVoiceState.IsSuppressed => false;

	IVoiceChannel IVoiceState.VoiceChannel => null;

	string IVoiceState.VoiceSessionId => null;

	bool IVoiceState.IsStreaming => false;

	bool IVoiceState.IsVideoing => false;

	DateTimeOffset? IVoiceState.RequestToSpeakTimestamp => null;

	internal RestGuildUser(BaseDiscordClient discord, IGuild guild, ulong id, ulong? guildId = null)
		: base(discord, id)
	{
		if (guild != null)
		{
			Guild = guild;
		}
		GuildId = guildId ?? Guild.Id;
	}

	internal static RestGuildUser Create(BaseDiscordClient discord, IGuild guild, GuildMember model, ulong? guildId = null)
	{
		RestGuildUser restGuildUser = new RestGuildUser(discord, guild, model.User.Id, guildId);
		restGuildUser.Update(model);
		return restGuildUser;
	}

	internal void Update(GuildMember model)
	{
		base.Update(model.User);
		if (model.JoinedAt.IsSpecified)
		{
			_joinedAtTicks = model.JoinedAt.Value.UtcTicks;
		}
		if (model.Nick.IsSpecified)
		{
			Nickname = model.Nick.Value;
		}
		if (model.Avatar.IsSpecified)
		{
			GuildAvatarId = model.Avatar.Value;
		}
		if (model.Deaf.IsSpecified)
		{
			IsDeafened = model.Deaf.Value;
		}
		if (model.Mute.IsSpecified)
		{
			IsMuted = model.Mute.Value;
		}
		if (model.Roles.IsSpecified)
		{
			UpdateRoles(model.Roles.Value);
		}
		if (model.PremiumSince.IsSpecified)
		{
			_premiumSinceTicks = model.PremiumSince.Value?.UtcTicks;
		}
		if (model.TimedOutUntil.IsSpecified)
		{
			_timedOutTicks = model.TimedOutUntil.Value?.UtcTicks;
		}
		if (model.Pending.IsSpecified)
		{
			IsPending = model.Pending.Value;
		}
	}

	private void UpdateRoles(ulong[] roleIds)
	{
		System.Collections.Immutable.ImmutableArray<ulong>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<ulong>(roleIds.Length + 1);
		builder.Add(GuildId);
		for (int i = 0; i < roleIds.Length; i++)
		{
			builder.Add(roleIds[i]);
		}
		_roleIds = builder.ToImmutable();
	}

	public override async Task UpdateAsync(RequestOptions options = null)
	{
		Update(await base.Discord.ApiClient.GetGuildMemberAsync(GuildId, base.Id, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public async Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null)
	{
		GuildUserProperties guildUserProperties = await UserHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guildUserProperties.Deaf.IsSpecified)
		{
			IsDeafened = guildUserProperties.Deaf.Value;
		}
		if (guildUserProperties.Mute.IsSpecified)
		{
			IsMuted = guildUserProperties.Mute.Value;
		}
		if (guildUserProperties.Nickname.IsSpecified)
		{
			Nickname = guildUserProperties.Nickname.Value;
		}
		if (guildUserProperties.Roles.IsSpecified)
		{
			UpdateRoles(guildUserProperties.Roles.Value.Select((IRole x) => x.Id).ToArray());
		}
		else if (guildUserProperties.RoleIds.IsSpecified)
		{
			UpdateRoles(guildUserProperties.RoleIds.Value.ToArray());
		}
	}

	public Task KickAsync(string reason = null, RequestOptions options = null)
	{
		return UserHelper.KickAsync(this, base.Discord, reason, options);
	}

	public Task AddRoleAsync(ulong roleId, RequestOptions options = null)
	{
		return AddRolesAsync(new ulong[1] { roleId }, options);
	}

	public Task AddRoleAsync(IRole role, RequestOptions options = null)
	{
		return AddRoleAsync(role.Id, options);
	}

	public Task AddRolesAsync(IEnumerable<ulong> roleIds, RequestOptions options = null)
	{
		return UserHelper.AddRolesAsync(this, base.Discord, roleIds, options);
	}

	public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
	{
		return AddRolesAsync(roles.Select((IRole x) => x.Id), options);
	}

	public Task RemoveRoleAsync(ulong roleId, RequestOptions options = null)
	{
		return RemoveRolesAsync(new ulong[1] { roleId }, options);
	}

	public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
	{
		return RemoveRoleAsync(role.Id, options);
	}

	public Task RemoveRolesAsync(IEnumerable<ulong> roleIds, RequestOptions options = null)
	{
		return UserHelper.RemoveRolesAsync(this, base.Discord, roleIds, options);
	}

	public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
	{
		return RemoveRolesAsync(roles.Select((IRole x) => x.Id));
	}

	public Task SetTimeOutAsync(TimeSpan span, RequestOptions options = null)
	{
		return UserHelper.SetTimeoutAsync(this, base.Discord, span, options);
	}

	public Task RemoveTimeOutAsync(RequestOptions options = null)
	{
		return UserHelper.RemoveTimeOutAsync(this, base.Discord, options);
	}

	public ChannelPermissions GetPermissions(IGuildChannel channel)
	{
		GuildPermissions guildPermissions = GuildPermissions;
		return new ChannelPermissions(Permissions.ResolveChannel(Guild, this, channel, guildPermissions.RawValue));
	}

	public string GetDisplayAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		if (GuildAvatarId == null)
		{
			return GetAvatarUrl(format, size);
		}
		return GetGuildAvatarUrl(format, size);
	}

	public string GetGuildAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		return CDN.GetGuildUserAvatarUrl(base.Id, GuildId, GuildAvatarId, size, format);
	}
}

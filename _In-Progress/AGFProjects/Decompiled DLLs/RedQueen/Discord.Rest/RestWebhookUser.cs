using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestWebhookUser : RestUser, IWebhookUser, IGuildUser, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IVoiceState
{
	public ulong WebhookId { get; }

	internal IGuild Guild { get; }

	public DateTimeOffset? PremiumSince { get; private set; }

	public override bool IsWebhook => true;

	public ulong GuildId => Guild.Id;

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

	IReadOnlyCollection<ulong> IGuildUser.RoleIds => System.Collections.Immutable.ImmutableArray.Create<ulong>();

	DateTimeOffset? IGuildUser.JoinedAt => null;

	string IGuildUser.DisplayName => null;

	string IGuildUser.Nickname => null;

	string IGuildUser.DisplayAvatarId => null;

	string IGuildUser.GuildAvatarId => null;

	bool? IGuildUser.IsPending => null;

	int IGuildUser.Hierarchy => 0;

	DateTimeOffset? IGuildUser.TimedOutUntil => null;

	GuildPermissions IGuildUser.GuildPermissions => GuildPermissions.Webhook;

	bool IVoiceState.IsDeafened => false;

	bool IVoiceState.IsMuted => false;

	bool IVoiceState.IsSelfDeafened => false;

	bool IVoiceState.IsSelfMuted => false;

	bool IVoiceState.IsSuppressed => false;

	IVoiceChannel IVoiceState.VoiceChannel => null;

	string IVoiceState.VoiceSessionId => null;

	bool IVoiceState.IsStreaming => false;

	bool IVoiceState.IsVideoing => false;

	DateTimeOffset? IVoiceState.RequestToSpeakTimestamp => null;

	internal RestWebhookUser(BaseDiscordClient discord, IGuild guild, ulong id, ulong webhookId)
		: base(discord, id)
	{
		Guild = guild;
		WebhookId = webhookId;
	}

	internal static RestWebhookUser Create(BaseDiscordClient discord, IGuild guild, User model, ulong webhookId)
	{
		RestWebhookUser restWebhookUser = new RestWebhookUser(discord, guild, model.Id, webhookId);
		restWebhookUser.Update(model);
		return restWebhookUser;
	}

	string IGuildUser.GetDisplayAvatarUrl(ImageFormat format, ushort size)
	{
		return null;
	}

	string IGuildUser.GetGuildAvatarUrl(ImageFormat format, ushort size)
	{
		return null;
	}

	ChannelPermissions IGuildUser.GetPermissions(IGuildChannel channel)
	{
		return Permissions.ToChannelPerms(channel, GuildPermissions.Webhook.RawValue);
	}

	Task IGuildUser.KickAsync(string reason, RequestOptions options)
	{
		throw new NotSupportedException("Webhook users cannot be kicked.");
	}

	Task IGuildUser.ModifyAsync(Action<GuildUserProperties> func, RequestOptions options)
	{
		throw new NotSupportedException("Webhook users cannot be modified.");
	}

	Task IGuildUser.AddRoleAsync(ulong role, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.AddRoleAsync(IRole role, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.AddRolesAsync(IEnumerable<ulong> roles, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.RemoveRoleAsync(ulong role, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.RemoveRoleAsync(IRole role, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.RemoveRolesAsync(IEnumerable<ulong> roles, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.SetTimeOutAsync(TimeSpan span, RequestOptions options)
	{
		throw new NotSupportedException("Timeouts are not supported on webhook users.");
	}

	Task IGuildUser.RemoveTimeOutAsync(RequestOptions options)
	{
		throw new NotSupportedException("Timeouts are not supported on webhook users.");
	}
}

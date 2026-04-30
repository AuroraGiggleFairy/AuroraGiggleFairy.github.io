using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

internal interface IGuildUser : IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IVoiceState
{
	DateTimeOffset? JoinedAt { get; }

	string DisplayName { get; }

	string Nickname { get; }

	string DisplayAvatarId { get; }

	string GuildAvatarId { get; }

	GuildPermissions GuildPermissions { get; }

	IGuild Guild { get; }

	ulong GuildId { get; }

	DateTimeOffset? PremiumSince { get; }

	IReadOnlyCollection<ulong> RoleIds { get; }

	bool? IsPending { get; }

	int Hierarchy { get; }

	DateTimeOffset? TimedOutUntil { get; }

	ChannelPermissions GetPermissions(IGuildChannel channel);

	string GetGuildAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128);

	string GetDisplayAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128);

	Task KickAsync(string reason = null, RequestOptions options = null);

	Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null);

	Task AddRoleAsync(ulong roleId, RequestOptions options = null);

	Task AddRoleAsync(IRole role, RequestOptions options = null);

	Task AddRolesAsync(IEnumerable<ulong> roleIds, RequestOptions options = null);

	Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null);

	Task RemoveRoleAsync(ulong roleId, RequestOptions options = null);

	Task RemoveRoleAsync(IRole role, RequestOptions options = null);

	Task RemoveRolesAsync(IEnumerable<ulong> roleIds, RequestOptions options = null);

	Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null);

	Task SetTimeOutAsync(TimeSpan span, RequestOptions options = null);

	Task RemoveTimeOutAsync(RequestOptions options = null);
}

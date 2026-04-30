using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

internal interface IGuildChannel : IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	int Position { get; }

	IGuild Guild { get; }

	ulong GuildId { get; }

	IReadOnlyCollection<Overwrite> PermissionOverwrites { get; }

	Task ModifyAsync(Action<GuildChannelProperties> func, RequestOptions options = null);

	OverwritePermissions? GetPermissionOverwrite(IRole role);

	OverwritePermissions? GetPermissionOverwrite(IUser user);

	Task RemovePermissionOverwriteAsync(IRole role, RequestOptions options = null);

	Task RemovePermissionOverwriteAsync(IUser user, RequestOptions options = null);

	Task AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions, RequestOptions options = null);

	Task AddPermissionOverwriteAsync(IUser user, OverwritePermissions permissions, RequestOptions options = null);

	new IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	new Task<IGuildUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);
}

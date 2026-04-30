using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

internal interface IChannel : ISnowflakeEntity, IEntity<ulong>
{
	string Name { get; }

	IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);
}

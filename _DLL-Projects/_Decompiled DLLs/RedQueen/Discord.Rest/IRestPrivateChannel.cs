using System.Collections.Generic;

namespace Discord.Rest;

internal interface IRestPrivateChannel : IPrivateChannel, IChannel, ISnowflakeEntity, IEntity<ulong>
{
	new IReadOnlyCollection<RestUser> Recipients { get; }
}

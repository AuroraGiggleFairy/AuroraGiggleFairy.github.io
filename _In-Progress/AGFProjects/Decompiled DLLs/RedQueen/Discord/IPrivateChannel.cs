using System.Collections.Generic;

namespace Discord;

internal interface IPrivateChannel : IChannel, ISnowflakeEntity, IEntity<ulong>
{
	IReadOnlyCollection<IUser> Recipients { get; }
}

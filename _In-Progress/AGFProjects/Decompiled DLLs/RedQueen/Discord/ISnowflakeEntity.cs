using System;

namespace Discord;

internal interface ISnowflakeEntity : IEntity<ulong>
{
	DateTimeOffset CreatedAt { get; }
}

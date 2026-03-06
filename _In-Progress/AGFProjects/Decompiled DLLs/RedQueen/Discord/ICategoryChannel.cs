namespace Discord;

internal interface ICategoryChannel : IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
}

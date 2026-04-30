namespace Discord;

internal interface ISystemMessage : IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
}

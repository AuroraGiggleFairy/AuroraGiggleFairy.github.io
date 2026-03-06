namespace Discord;

internal interface IGroupUser : IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IVoiceState
{
}

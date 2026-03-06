using System.Threading.Tasks;

namespace Discord;

internal interface IGroupChannel : IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IPrivateChannel, IAudioChannel
{
	Task LeaveAsync(RequestOptions options = null);
}

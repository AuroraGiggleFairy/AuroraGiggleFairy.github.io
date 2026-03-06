using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

internal interface IVoiceChannel : IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, INestedChannel, IGuildChannel, IDeletable, IAudioChannel, IMentionable
{
	int Bitrate { get; }

	int? UserLimit { get; }

	Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null);

	Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null);

	Task ModifyAsync(Action<VoiceChannelProperties> func, RequestOptions options = null);
}

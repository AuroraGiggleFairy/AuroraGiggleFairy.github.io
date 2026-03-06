using System;
using System.Threading.Tasks;
using Discord.Audio;

namespace Discord;

internal interface IAudioChannel : IChannel, ISnowflakeEntity, IEntity<ulong>
{
	string RTCRegion { get; }

	Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false);

	Task DisconnectAsync();

	Task ModifyAsync(Action<AudioChannelProperties> func, RequestOptions options = null);
}

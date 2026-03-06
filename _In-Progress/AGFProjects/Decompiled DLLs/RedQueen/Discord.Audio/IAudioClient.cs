using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Audio;

internal interface IAudioClient : IDisposable
{
	ConnectionState ConnectionState { get; }

	int Latency { get; }

	int UdpLatency { get; }

	event Func<Task> Connected;

	event Func<Exception, Task> Disconnected;

	event Func<int, int, Task> LatencyUpdated;

	event Func<int, int, Task> UdpLatencyUpdated;

	event Func<ulong, AudioInStream, Task> StreamCreated;

	event Func<ulong, Task> StreamDestroyed;

	event Func<ulong, bool, Task> SpeakingUpdated;

	IReadOnlyDictionary<ulong, AudioInStream> GetStreams();

	Task StopAsync();

	Task SetSpeakingAsync(bool value);

	AudioOutStream CreateOpusStream(int bufferMillis = 1000);

	AudioOutStream CreateDirectOpusStream();

	AudioOutStream CreatePCMStream(AudioApplication application, int? bitrate = null, int bufferMillis = 1000, int packetLoss = 30);

	AudioOutStream CreateDirectPCMStream(AudioApplication application, int? bitrate = null, int packetLoss = 30);
}

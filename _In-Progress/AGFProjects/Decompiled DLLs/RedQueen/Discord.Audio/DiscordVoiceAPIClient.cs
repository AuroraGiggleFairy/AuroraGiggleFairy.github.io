using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Voice;
using Discord.Net.Converters;
using Discord.Net.Udp;
using Discord.Net.WebSockets;
using Newtonsoft.Json;

namespace Discord.Audio;

internal class DiscordVoiceAPIClient : IDisposable
{
	public const int MaxBitrate = 131072;

	public const string Mode = "xsalsa20_poly1305";

	private readonly AsyncEvent<Func<string, string, double, Task>> _sentRequestEvent = new AsyncEvent<Func<string, string, double, Task>>();

	private readonly AsyncEvent<Func<VoiceOpCode, Task>> _sentGatewayMessageEvent = new AsyncEvent<Func<VoiceOpCode, Task>>();

	private readonly AsyncEvent<Func<Task>> _sentDiscoveryEvent = new AsyncEvent<Func<Task>>();

	private readonly AsyncEvent<Func<int, Task>> _sentDataEvent = new AsyncEvent<Func<int, Task>>();

	private readonly AsyncEvent<Func<VoiceOpCode, object, Task>> _receivedEvent = new AsyncEvent<Func<VoiceOpCode, object, Task>>();

	private readonly AsyncEvent<Func<byte[], Task>> _receivedPacketEvent = new AsyncEvent<Func<byte[], Task>>();

	private readonly AsyncEvent<Func<Exception, Task>> _disconnectedEvent = new AsyncEvent<Func<Exception, Task>>();

	private readonly JsonSerializer _serializer;

	private readonly SemaphoreSlim _connectionLock;

	private readonly IUdpSocket _udp;

	private CancellationTokenSource _connectCancelToken;

	private bool _isDisposed;

	private ulong _nextKeepalive;

	public ulong GuildId { get; }

	internal IWebSocketClient WebSocketClient { get; }

	public ConnectionState ConnectionState { get; private set; }

	public ushort UdpPort => _udp.Port;

	public event Func<string, string, double, Task> SentRequest
	{
		add
		{
			_sentRequestEvent.Add(value);
		}
		remove
		{
			_sentRequestEvent.Remove(value);
		}
	}

	public event Func<VoiceOpCode, Task> SentGatewayMessage
	{
		add
		{
			_sentGatewayMessageEvent.Add(value);
		}
		remove
		{
			_sentGatewayMessageEvent.Remove(value);
		}
	}

	public event Func<Task> SentDiscovery
	{
		add
		{
			_sentDiscoveryEvent.Add(value);
		}
		remove
		{
			_sentDiscoveryEvent.Remove(value);
		}
	}

	public event Func<int, Task> SentData
	{
		add
		{
			_sentDataEvent.Add(value);
		}
		remove
		{
			_sentDataEvent.Remove(value);
		}
	}

	public event Func<VoiceOpCode, object, Task> ReceivedEvent
	{
		add
		{
			_receivedEvent.Add(value);
		}
		remove
		{
			_receivedEvent.Remove(value);
		}
	}

	public event Func<byte[], Task> ReceivedPacket
	{
		add
		{
			_receivedPacketEvent.Add(value);
		}
		remove
		{
			_receivedPacketEvent.Remove(value);
		}
	}

	public event Func<Exception, Task> Disconnected
	{
		add
		{
			_disconnectedEvent.Add(value);
		}
		remove
		{
			_disconnectedEvent.Remove(value);
		}
	}

	internal DiscordVoiceAPIClient(ulong guildId, WebSocketProvider webSocketProvider, UdpSocketProvider udpSocketProvider, JsonSerializer serializer = null)
	{
		GuildId = guildId;
		_connectionLock = new SemaphoreSlim(1, 1);
		_udp = udpSocketProvider();
		_udp.ReceivedDatagram += async delegate(byte[] data, int index, int count)
		{
			if (index != 0 || count != data.Length)
			{
				byte[] array = new byte[count];
				Buffer.BlockCopy(data, index, array, 0, count);
				data = array;
			}
			await _receivedPacketEvent.InvokeAsync(data).ConfigureAwait(continueOnCapturedContext: false);
		};
		WebSocketClient = webSocketProvider();
		WebSocketClient.BinaryMessage += async delegate(byte[] data, int index, int count)
		{
			using MemoryStream compressed = new MemoryStream(data, index + 2, count - 2);
			using MemoryStream decompressed = new MemoryStream();
			using (DeflateStream deflateStream = new DeflateStream(compressed, CompressionMode.Decompress))
			{
				deflateStream.CopyTo(decompressed);
			}
			decompressed.Position = 0L;
			using StreamReader reader = new StreamReader(decompressed);
			SocketFrame socketFrame = JsonConvert.DeserializeObject<SocketFrame>(reader.ReadToEnd());
			await _receivedEvent.InvokeAsync((VoiceOpCode)socketFrame.Operation, socketFrame.Payload).ConfigureAwait(continueOnCapturedContext: false);
		};
		WebSocketClient.TextMessage += async delegate(string text)
		{
			SocketFrame socketFrame = JsonConvert.DeserializeObject<SocketFrame>(text);
			await _receivedEvent.InvokeAsync((VoiceOpCode)socketFrame.Operation, socketFrame.Payload).ConfigureAwait(continueOnCapturedContext: false);
		};
		WebSocketClient.Closed += async delegate(Exception ex)
		{
			await DisconnectAsync().ConfigureAwait(continueOnCapturedContext: false);
			await _disconnectedEvent.InvokeAsync(ex).ConfigureAwait(continueOnCapturedContext: false);
		};
		_serializer = serializer ?? new JsonSerializer
		{
			ContractResolver = new DiscordContractResolver()
		};
	}

	private void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_connectCancelToken?.Dispose();
				_udp?.Dispose();
				WebSocketClient?.Dispose();
				_connectionLock?.Dispose();
			}
			_isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public async Task SendAsync(VoiceOpCode opCode, object payload, RequestOptions options = null)
	{
		byte[] array = null;
		payload = new SocketFrame
		{
			Operation = (int)opCode,
			Payload = payload
		};
		if (payload != null)
		{
			array = Encoding.UTF8.GetBytes(SerializeJson(payload));
		}
		await WebSocketClient.SendAsync(array, 0, array.Length, isText: true).ConfigureAwait(continueOnCapturedContext: false);
		await _sentGatewayMessageEvent.InvokeAsync(opCode).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendAsync(byte[] data, int offset, int bytes)
	{
		await _udp.SendAsync(data, offset, bytes).ConfigureAwait(continueOnCapturedContext: false);
		await _sentDataEvent.InvokeAsync(bytes).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendHeartbeatAsync(RequestOptions options = null)
	{
		await SendAsync(VoiceOpCode.Heartbeat, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendIdentityAsync(ulong userId, string sessionId, string token)
	{
		await SendAsync(VoiceOpCode.Identify, new IdentifyParams
		{
			GuildId = GuildId,
			UserId = userId,
			SessionId = sessionId,
			Token = token
		}).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendSelectProtocol(string externalIp, int externalPort)
	{
		await SendAsync(VoiceOpCode.SelectProtocol, new SelectProtocolParams
		{
			Protocol = "udp",
			Data = new UdpProtocolInfo
			{
				Address = externalIp,
				Port = externalPort,
				Mode = "xsalsa20_poly1305"
			}
		}).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendSetSpeaking(bool value)
	{
		await SendAsync(VoiceOpCode.Speaking, new SpeakingParams
		{
			IsSpeaking = value,
			Delay = 0
		}).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task ConnectAsync(string url)
	{
		await _connectionLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await ConnectInternalAsync(url).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_connectionLock.Release();
		}
	}

	private async Task ConnectInternalAsync(string url)
	{
		ConnectionState = ConnectionState.Connecting;
		try
		{
			_connectCancelToken?.Dispose();
			_connectCancelToken = new CancellationTokenSource();
			CancellationToken cancelToken = _connectCancelToken.Token;
			WebSocketClient.SetCancelToken(cancelToken);
			await WebSocketClient.ConnectAsync(url).ConfigureAwait(continueOnCapturedContext: false);
			_udp.SetCancelToken(cancelToken);
			await _udp.StartAsync().ConfigureAwait(continueOnCapturedContext: false);
			ConnectionState = ConnectionState.Connected;
		}
		catch
		{
			await DisconnectInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
			throw;
		}
	}

	public async Task DisconnectAsync()
	{
		await _connectionLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await DisconnectInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_connectionLock.Release();
		}
	}

	private async Task DisconnectInternalAsync()
	{
		if (ConnectionState != ConnectionState.Disconnected)
		{
			ConnectionState = ConnectionState.Disconnecting;
			try
			{
				_connectCancelToken?.Cancel(throwOnFirstException: false);
			}
			catch
			{
			}
			await _udp.StopAsync().ConfigureAwait(continueOnCapturedContext: false);
			await WebSocketClient.DisconnectAsync().ConfigureAwait(continueOnCapturedContext: false);
			ConnectionState = ConnectionState.Disconnected;
		}
	}

	public async Task SendDiscoveryAsync(uint ssrc)
	{
		byte[] array = new byte[70];
		array[0] = (byte)(ssrc >> 24);
		array[1] = (byte)(ssrc >> 16);
		array[2] = (byte)(ssrc >> 8);
		array[3] = (byte)ssrc;
		await SendAsync(array, 0, 70).ConfigureAwait(continueOnCapturedContext: false);
		await _sentDiscoveryEvent.InvokeAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ulong> SendKeepaliveAsync()
	{
		ulong value = _nextKeepalive++;
		await SendAsync(new byte[8]
		{
			(byte)value,
			(byte)(value >> 8),
			(byte)(value >> 16),
			(byte)(value >> 24),
			(byte)(value >> 32),
			(byte)(value >> 40),
			(byte)(value >> 48),
			(byte)(value >> 56)
		}, 0, 8).ConfigureAwait(continueOnCapturedContext: false);
		return value;
	}

	public void SetUdpEndpoint(string ip, int port)
	{
		_udp.SetDestination(ip, port);
	}

	private static double ToMilliseconds(Stopwatch stopwatch)
	{
		return Math.Round((double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0, 2);
	}

	private string SerializeJson(object value)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		using (TextWriter textWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
		{
			using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
			_serializer.Serialize(jsonWriter, value);
		}
		return stringBuilder.ToString();
	}

	private T DeserializeJson<T>(Stream jsonStream)
	{
		using TextReader reader = new StreamReader(jsonStream);
		using JsonReader reader2 = new JsonTextReader(reader);
		return _serializer.Deserialize<T>(reader2);
	}
}

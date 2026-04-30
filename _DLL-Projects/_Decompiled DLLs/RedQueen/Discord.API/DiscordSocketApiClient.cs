using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.API.Gateway;
using Discord.Net.Queue;
using Discord.Net.Rest;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Discord.API;

internal class DiscordSocketApiClient : DiscordRestApiClient
{
	private readonly AsyncEvent<Func<GatewayOpCode, Task>> _sentGatewayMessageEvent = new AsyncEvent<Func<GatewayOpCode, Task>>();

	private readonly AsyncEvent<Func<GatewayOpCode, int?, string, object, Task>> _receivedGatewayEvent = new AsyncEvent<Func<GatewayOpCode, int?, string, object, Task>>();

	private readonly AsyncEvent<Func<Exception, Task>> _disconnectedEvent = new AsyncEvent<Func<Exception, Task>>();

	private readonly bool _isExplicitUrl;

	private CancellationTokenSource _connectCancelToken;

	private string _gatewayUrl;

	private string _resumeGatewayUrl;

	private MemoryStream _compressed;

	private DeflateStream _decompressor;

	internal IWebSocketClient WebSocketClient { get; }

	public ConnectionState ConnectionState { get; private set; }

	public string GatewayUrl
	{
		set
		{
			if (!_isExplicitUrl)
			{
				_gatewayUrl = FormatGatewayUrl(value);
			}
		}
	}

	public string ResumeGatewayUrl
	{
		set
		{
			_resumeGatewayUrl = FormatGatewayUrl(value);
		}
	}

	public event Func<GatewayOpCode, Task> SentGatewayMessage
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

	public event Func<GatewayOpCode, int?, string, object, Task> ReceivedGatewayEvent
	{
		add
		{
			_receivedGatewayEvent.Add(value);
		}
		remove
		{
			_receivedGatewayEvent.Remove(value);
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

	public DiscordSocketApiClient(RestClientProvider restClientProvider, WebSocketProvider webSocketProvider, string userAgent, string url = null, RetryMode defaultRetryMode = RetryMode.AlwaysRetry, JsonSerializer serializer = null, bool useSystemClock = true, Func<IRateLimitInfo, Task> defaultRatelimitCallback = null)
		: base(restClientProvider, userAgent, defaultRetryMode, serializer, useSystemClock, defaultRatelimitCallback)
	{
		_gatewayUrl = url;
		if (url != null)
		{
			_isExplicitUrl = true;
		}
		WebSocketClient = webSocketProvider();
		WebSocketClient.BinaryMessage += async delegate(byte[] data, int index, int count)
		{
			using MemoryStream decompressed = new MemoryStream();
			if (data[0] == 120)
			{
				_compressed.Write(data, index + 2, count - 2);
				_compressed.SetLength(count - 2);
			}
			else
			{
				_compressed.Write(data, index, count);
				_compressed.SetLength(count);
			}
			_compressed.Position = 0L;
			_decompressor.CopyTo(decompressed);
			_compressed.Position = 0L;
			decompressed.Position = 0L;
			using StreamReader reader = new StreamReader(decompressed);
			using JsonTextReader jsonReader = new JsonTextReader(reader);
			SocketFrame socketFrame = _serializer.Deserialize<SocketFrame>(jsonReader);
			if (socketFrame != null)
			{
				await _receivedGatewayEvent.InvokeAsync((GatewayOpCode)socketFrame.Operation, socketFrame.Sequence, socketFrame.Type, socketFrame.Payload).ConfigureAwait(continueOnCapturedContext: false);
			}
		};
		WebSocketClient.TextMessage += async delegate(string text)
		{
			using StringReader reader = new StringReader(text);
			using JsonTextReader jsonReader = new JsonTextReader(reader);
			SocketFrame socketFrame = _serializer.Deserialize<SocketFrame>(jsonReader);
			if (socketFrame != null)
			{
				await _receivedGatewayEvent.InvokeAsync((GatewayOpCode)socketFrame.Operation, socketFrame.Sequence, socketFrame.Type, socketFrame.Payload).ConfigureAwait(continueOnCapturedContext: false);
			}
		};
		WebSocketClient.Closed += async delegate(Exception ex)
		{
			await DisconnectAsync().ConfigureAwait(continueOnCapturedContext: false);
			await _disconnectedEvent.InvokeAsync(ex).ConfigureAwait(continueOnCapturedContext: false);
		};
	}

	internal override void Dispose(bool disposing)
	{
		if (!_isDisposed && disposing)
		{
			_connectCancelToken?.Dispose();
			WebSocketClient?.Dispose();
			_decompressor?.Dispose();
			_compressed?.Dispose();
		}
		base.Dispose(disposing);
	}

	internal override ValueTask DisposeAsync(bool disposing)
	{
		if (!_isDisposed && disposing)
		{
			_connectCancelToken?.Dispose();
			WebSocketClient?.Dispose();
			_decompressor?.Dispose();
		}
		return base.DisposeAsync(disposing);
	}

	private static string FormatGatewayUrl(string gatewayUrl)
	{
		if (gatewayUrl == null)
		{
			return null;
		}
		return string.Format("{0}?v={1}&encoding={2}&compress=zlib-stream", gatewayUrl, 10, "json");
	}

	public async Task ConnectAsync()
	{
		await _stateLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await ConnectInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_stateLock.Release();
		}
	}

	internal override async Task ConnectInternalAsync()
	{
		if (base.LoginState != LoginState.LoggedIn)
		{
			throw new InvalidOperationException("The client must be logged in before connecting.");
		}
		if (WebSocketClient == null)
		{
			throw new NotSupportedException("This client is not configured with WebSocket support.");
		}
		base.RequestQueue.ClearGatewayBuckets();
		_compressed?.Dispose();
		_decompressor?.Dispose();
		_compressed = new MemoryStream();
		_decompressor = new DeflateStream(_compressed, CompressionMode.Decompress);
		ConnectionState = ConnectionState.Connecting;
		try
		{
			_connectCancelToken?.Dispose();
			_connectCancelToken = new CancellationTokenSource();
			if (WebSocketClient != null)
			{
				WebSocketClient.SetCancelToken(_connectCancelToken.Token);
			}
			string host;
			if (_resumeGatewayUrl == null)
			{
				if (!_isExplicitUrl && _gatewayUrl == null)
				{
					_gatewayUrl = FormatGatewayUrl((await GetBotGatewayAsync().ConfigureAwait(continueOnCapturedContext: false)).Url);
				}
				host = _gatewayUrl;
			}
			else
			{
				host = _resumeGatewayUrl;
			}
			await WebSocketClient.ConnectAsync(host).ConfigureAwait(continueOnCapturedContext: false);
			ConnectionState = ConnectionState.Connected;
		}
		catch
		{
			await DisconnectInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
			throw;
		}
	}

	public async Task DisconnectAsync(Exception ex = null)
	{
		await _stateLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await DisconnectInternalAsync(ex).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_stateLock.Release();
		}
	}

	internal override async Task DisconnectInternalAsync(Exception ex = null)
	{
		if (WebSocketClient == null)
		{
			throw new NotSupportedException("This client is not configured with WebSocket support.");
		}
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
			if (!(ex is GatewayReconnectException))
			{
				await WebSocketClient.DisconnectAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await WebSocketClient.DisconnectAsync(4000).ConfigureAwait(continueOnCapturedContext: false);
			}
			ConnectionState = ConnectionState.Disconnected;
		}
	}

	public Task SendGatewayAsync(GatewayOpCode opCode, object payload, RequestOptions options = null)
	{
		return SendGatewayInternalAsync(opCode, payload, options);
	}

	private async Task SendGatewayInternalAsync(GatewayOpCode opCode, object payload, RequestOptions options)
	{
		CheckState();
		byte[] data = null;
		payload = new SocketFrame
		{
			Operation = (int)opCode,
			Payload = payload
		};
		if (payload != null)
		{
			data = Encoding.UTF8.GetBytes(SerializeJson(payload));
		}
		options.IsGatewayBucket = true;
		if (options.BucketId == null)
		{
			options.BucketId = GatewayBucket.Get(GatewayBucketType.Unbucketed).Id;
		}
		await base.RequestQueue.SendAsync(new WebSocketRequest(WebSocketClient, data, isText: true, opCode == GatewayOpCode.Heartbeat, options)).ConfigureAwait(continueOnCapturedContext: false);
		await _sentGatewayMessageEvent.InvokeAsync(opCode).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendIdentifyAsync(int largeThreshold = 100, int shardID = 0, int totalShards = 1, GatewayIntents gatewayIntents = GatewayIntents.AllUnprivileged, (UserStatus, bool, long?, Game)? presence = null, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		Dictionary<string, string> properties = new Dictionary<string, string>
		{
			["$device"] = "Discord.Net",
			["$os"] = Environment.OSVersion.Platform.ToString(),
			["$browser"] = "Discord.Net"
		};
		IdentifyParams identifyParams = new IdentifyParams
		{
			Token = base.AuthToken,
			Properties = properties,
			LargeThreshold = largeThreshold
		};
		if (totalShards > 1)
		{
			identifyParams.ShardingParams = new int[2] { shardID, totalShards };
		}
		options.BucketId = GatewayBucket.Get(GatewayBucketType.Identify).Id;
		identifyParams.Intents = (int)gatewayIntents;
		if (presence.HasValue)
		{
			identifyParams.Presence = new PresenceUpdateParams
			{
				Status = presence.Value.Item1,
				IsAFK = presence.Value.Item2,
				IdleSince = presence.Value.Item3,
				Activities = new object[1] { presence.Value.Item4 }
			};
		}
		await SendGatewayAsync(GatewayOpCode.Identify, identifyParams, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendResumeAsync(string sessionId, int lastSeq, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		ResumeParams payload = new ResumeParams
		{
			Token = base.AuthToken,
			SessionId = sessionId,
			Sequence = lastSeq
		};
		await SendGatewayAsync(GatewayOpCode.Resume, payload, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendHeartbeatAsync(int lastSeq, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		await SendGatewayAsync(GatewayOpCode.Heartbeat, lastSeq, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendPresenceUpdateAsync(UserStatus status, bool isAFK, long? since, Game game, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		PresenceUpdateParams presenceUpdateParams = new PresenceUpdateParams();
		presenceUpdateParams.Status = status;
		presenceUpdateParams.IdleSince = since;
		presenceUpdateParams.IsAFK = isAFK;
		presenceUpdateParams.Activities = new object[1] { game };
		PresenceUpdateParams payload = presenceUpdateParams;
		options.BucketId = GatewayBucket.Get(GatewayBucketType.PresenceUpdate).Id;
		await SendGatewayAsync(GatewayOpCode.PresenceUpdate, payload, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendRequestMembersAsync(IEnumerable<ulong> guildIds, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		await SendGatewayAsync(GatewayOpCode.RequestGuildMembers, new RequestMembersParams
		{
			GuildIds = guildIds,
			Query = "",
			Limit = 0
		}, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendVoiceStateUpdateAsync(ulong guildId, ulong? channelId, bool selfDeaf, bool selfMute, RequestOptions options = null)
	{
		VoiceStateUpdateParams payload = new VoiceStateUpdateParams
		{
			GuildId = guildId,
			ChannelId = channelId,
			SelfDeaf = selfDeaf,
			SelfMute = selfMute
		};
		options = RequestOptions.CreateOrClone(options);
		await SendGatewayAsync(GatewayOpCode.VoiceStateUpdate, payload, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendVoiceStateUpdateAsync(VoiceStateUpdateParams payload, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		await SendGatewayAsync(GatewayOpCode.VoiceStateUpdate, payload, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task SendGuildSyncAsync(IEnumerable<ulong> guildIds, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		await SendGatewayAsync(GatewayOpCode.GuildSync, guildIds, options).ConfigureAwait(continueOnCapturedContext: false);
	}
}

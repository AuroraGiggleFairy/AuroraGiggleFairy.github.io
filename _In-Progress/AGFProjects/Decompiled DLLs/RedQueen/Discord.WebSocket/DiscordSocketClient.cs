using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Gateway;
using Discord.Logging;
using Discord.Net.Converters;
using Discord.Net.Udp;
using Discord.Net.WebSockets;
using Discord.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Discord.WebSocket;

internal class DiscordSocketClient : BaseSocketClient, IDiscordClient, IDisposable, IAsyncDisposable
{
	private readonly ConcurrentQueue<ulong> _largeGuilds;

	internal readonly JsonSerializer _serializer;

	private readonly DiscordShardedClient _shardedClient;

	private readonly DiscordSocketClient _parentClient;

	private readonly ConcurrentQueue<long> _heartbeatTimes;

	private readonly ConnectionManager _connection;

	private readonly Logger _gatewayLogger;

	private readonly SemaphoreSlim _stateLock;

	private string _sessionId;

	private int _lastSeq;

	private ImmutableDictionary<string, RestVoiceRegion> _voiceRegions;

	private Task _heartbeatTask;

	private Task _guildDownloadTask;

	private int _unavailableGuildCount;

	private long _lastGuildAvailableTime;

	private long _lastMessageTime;

	private int _nextAudioId;

	private DateTimeOffset? _statusSince;

	private RestApplication _applicationInfo;

	private bool _isDisposed;

	private GatewayIntents _gatewayIntents;

	private System.Collections.Immutable.ImmutableArray<StickerPack<SocketSticker>> _defaultStickers;

	private SocketSelfUser _previousSessionUser;

	private UserStatus? _status;

	private Optional<IActivity> _activity;

	private readonly AsyncEvent<Func<Task>> _connectedEvent = new AsyncEvent<Func<Task>>();

	private readonly AsyncEvent<Func<Exception, Task>> _disconnectedEvent = new AsyncEvent<Func<Exception, Task>>();

	private readonly AsyncEvent<Func<Task>> _readyEvent = new AsyncEvent<Func<Task>>();

	private readonly AsyncEvent<Func<int, int, Task>> _latencyUpdatedEvent = new AsyncEvent<Func<int, int, Task>>();

	public override DiscordSocketRestClient Rest { get; }

	public int ShardId { get; }

	public ConnectionState ConnectionState => _connection.State;

	public override int Latency { get; protected set; }

	public override UserStatus Status
	{
		get
		{
			return _status ?? UserStatus.Online;
		}
		protected set
		{
			_status = value;
		}
	}

	public override IActivity Activity
	{
		get
		{
			return _activity.GetValueOrDefault();
		}
		protected set
		{
			_activity = Optional.Create(value);
		}
	}

	internal int TotalShards { get; private set; }

	internal int MessageCacheSize { get; private set; }

	internal int LargeThreshold { get; private set; }

	internal ClientState State { get; private set; }

	internal UdpSocketProvider UdpSocketProvider { get; private set; }

	internal WebSocketProvider WebSocketProvider { get; private set; }

	internal bool AlwaysDownloadUsers { get; private set; }

	internal int? HandlerTimeout { get; private set; }

	internal bool AlwaysDownloadDefaultStickers { get; private set; }

	internal bool AlwaysResolveStickers { get; private set; }

	internal bool LogGatewayIntentWarnings { get; private set; }

	internal bool SuppressUnknownDispatchWarnings { get; private set; }

	internal new DiscordSocketApiClient ApiClient => base.ApiClient;

	public override IReadOnlyCollection<SocketGuild> Guilds => State.Guilds;

	public override IReadOnlyCollection<StickerPack<SocketSticker>> DefaultStickerPacks
	{
		get
		{
			if (_shardedClient != null)
			{
				return _shardedClient.DefaultStickerPacks;
			}
			return _defaultStickers.ToReadOnlyCollection();
		}
	}

	public override IReadOnlyCollection<ISocketPrivateChannel> PrivateChannels => State.PrivateChannels;

	public IReadOnlyCollection<SocketDMChannel> DMChannels => State.PrivateChannels.OfType<SocketDMChannel>().ToImmutableArray();

	public IReadOnlyCollection<SocketGroupChannel> GroupChannels => State.PrivateChannels.OfType<SocketGroupChannel>().ToImmutableArray();

	public event Func<Task> Connected
	{
		add
		{
			_connectedEvent.Add(value);
		}
		remove
		{
			_connectedEvent.Remove(value);
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

	public event Func<Task> Ready
	{
		add
		{
			_readyEvent.Add(value);
		}
		remove
		{
			_readyEvent.Remove(value);
		}
	}

	public event Func<int, int, Task> LatencyUpdated
	{
		add
		{
			_latencyUpdatedEvent.Add(value);
		}
		remove
		{
			_latencyUpdatedEvent.Remove(value);
		}
	}

	public DiscordSocketClient()
		: this(new DiscordSocketConfig())
	{
	}

	public DiscordSocketClient(DiscordSocketConfig config)
		: this(config, CreateApiClient(config), null, null)
	{
	}

	internal DiscordSocketClient(DiscordSocketConfig config, DiscordShardedClient shardedClient, DiscordSocketClient parentClient)
		: this(config, CreateApiClient(config), shardedClient, parentClient)
	{
	}

	private DiscordSocketClient(DiscordSocketConfig config, DiscordSocketApiClient client, DiscordShardedClient shardedClient, DiscordSocketClient parentClient)
		: base(config, client)
	{
		ShardId = config.ShardId.GetValueOrDefault();
		TotalShards = config.TotalShards ?? 1;
		MessageCacheSize = config.MessageCacheSize;
		LargeThreshold = config.LargeThreshold;
		UdpSocketProvider = config.UdpSocketProvider;
		WebSocketProvider = config.WebSocketProvider;
		AlwaysDownloadUsers = config.AlwaysDownloadUsers;
		AlwaysDownloadDefaultStickers = config.AlwaysDownloadDefaultStickers;
		AlwaysResolveStickers = config.AlwaysResolveStickers;
		LogGatewayIntentWarnings = config.LogGatewayIntentWarnings;
		SuppressUnknownDispatchWarnings = config.SuppressUnknownDispatchWarnings;
		HandlerTimeout = config.HandlerTimeout;
		State = new ClientState(0, 0);
		Rest = new DiscordSocketRestClient(config, ApiClient);
		_heartbeatTimes = new ConcurrentQueue<long>();
		_gatewayIntents = config.GatewayIntents;
		_defaultStickers = System.Collections.Immutable.ImmutableArray.Create<StickerPack<SocketSticker>>();
		_stateLock = new SemaphoreSlim(1, 1);
		_gatewayLogger = base.LogManager.CreateLogger((ShardId == 0 && TotalShards == 1) ? "Gateway" : $"Shard #{ShardId}");
		_connection = new ConnectionManager(_stateLock, _gatewayLogger, config.ConnectionTimeout, OnConnectingAsync, OnDisconnectingAsync, delegate(Func<Exception, Task> x)
		{
			ApiClient.Disconnected += x;
		});
		_connection.Connected += () => TimedInvokeAsync(_connectedEvent, "Connected");
		_connection.Disconnected += (Exception ex, bool recon) => TimedInvokeAsync(_disconnectedEvent, "Disconnected", ex);
		_nextAudioId = 1;
		_shardedClient = shardedClient;
		_parentClient = parentClient;
		_serializer = new JsonSerializer
		{
			ContractResolver = new DiscordContractResolver()
		};
		_serializer.Error += (object s, [_003C310dd647_002Dcb79_002D4ede_002Daa5a_002D43bf82c1bf46_003ENullable(1)] Newtonsoft.Json.Serialization.ErrorEventArgs e) =>
		{
			_gatewayLogger.WarningAsync("Serializer Error", e.ErrorContext.Error).GetAwaiter().GetResult();
			e.ErrorContext.Handled = true;
		};
		ApiClient.SentGatewayMessage += async delegate(GatewayOpCode opCode)
		{
			await _gatewayLogger.DebugAsync($"Sent {opCode}").ConfigureAwait(continueOnCapturedContext: false);
		};
		ApiClient.ReceivedGatewayEvent += ProcessMessageAsync;
		base.LeftGuild += async delegate(SocketGuild g)
		{
			await _gatewayLogger.InfoAsync("Left " + g.Name).ConfigureAwait(continueOnCapturedContext: false);
		};
		base.JoinedGuild += async delegate(SocketGuild g)
		{
			await _gatewayLogger.InfoAsync("Joined " + g.Name).ConfigureAwait(continueOnCapturedContext: false);
		};
		base.GuildAvailable += async delegate(SocketGuild g)
		{
			await _gatewayLogger.VerboseAsync("Connected to " + g.Name).ConfigureAwait(continueOnCapturedContext: false);
		};
		base.GuildUnavailable += async delegate(SocketGuild g)
		{
			await _gatewayLogger.VerboseAsync("Disconnected from " + g.Name).ConfigureAwait(continueOnCapturedContext: false);
		};
		LatencyUpdated += async delegate(int old, int val)
		{
			await _gatewayLogger.DebugAsync($"Latency = {val} ms").ConfigureAwait(continueOnCapturedContext: false);
		};
		base.GuildAvailable += delegate(SocketGuild g)
		{
			Task guildDownloadTask = _guildDownloadTask;
			if (guildDownloadTask != null && guildDownloadTask.IsCompleted && ConnectionState == ConnectionState.Connected && AlwaysDownloadUsers && !g.HasAllMembers)
			{
				g.DownloadUsersAsync();
			}
			return Task.Delay(0);
		};
		_largeGuilds = new ConcurrentQueue<ulong>();
	}

	private static DiscordSocketApiClient CreateApiClient(DiscordSocketConfig config)
	{
		return new DiscordSocketApiClient(config.RestClientProvider, config.WebSocketProvider, DiscordConfig.UserAgent, config.GatewayHost, RetryMode.AlwaysRetry, null, config.UseSystemClock, config.DefaultRatelimitCallback);
	}

	internal override void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				StopAsync().GetAwaiter().GetResult();
				ApiClient?.Dispose();
				_stateLock?.Dispose();
			}
			_isDisposed = true;
		}
		base.Dispose(disposing);
	}

	internal override async ValueTask DisposeAsync(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				await StopAsync().ConfigureAwait(continueOnCapturedContext: false);
				if (ApiClient != null)
				{
					await ApiClient.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				}
				_stateLock?.Dispose();
			}
			_isDisposed = true;
		}
		await base.DisposeAsync(disposing).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal override async Task OnLoginAsync(TokenType tokenType, string token)
	{
		if (_shardedClient != null || _defaultStickers.Length != 0 || !AlwaysDownloadDefaultStickers)
		{
			return;
		}
		NitroStickerPacks obj = await ApiClient.ListNitroStickerPacksAsync().ConfigureAwait(continueOnCapturedContext: false);
		System.Collections.Immutable.ImmutableArray<StickerPack<SocketSticker>>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<StickerPack<SocketSticker>>();
		foreach (StickerPack stickerPack in obj.StickerPacks)
		{
			IEnumerable<SocketSticker> stickers = stickerPack.Stickers.Select((Discord.API.Sticker x) => SocketSticker.Create(this, x));
			StickerPack<SocketSticker> item = new StickerPack<SocketSticker>(stickerPack.Name, stickerPack.Id, stickerPack.SkuId, stickerPack.CoverStickerId.ToNullable(), stickerPack.Description, stickerPack.BannerAssetId, stickers);
			builder.Add(item);
		}
		_defaultStickers = builder.ToImmutable();
	}

	internal override async Task OnLogoutAsync()
	{
		await StopAsync().ConfigureAwait(continueOnCapturedContext: false);
		_applicationInfo = null;
		_voiceRegions = null;
		await Rest.OnLogoutAsync();
	}

	public override async Task StartAsync()
	{
		await _connection.StartAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task StopAsync()
	{
		await _connection.StopAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task OnConnectingAsync()
	{
		bool locked = false;
		if (_shardedClient != null && _sessionId == null)
		{
			await _shardedClient.AcquireIdentifyLockAsync(ShardId, _connection.CancelToken).ConfigureAwait(continueOnCapturedContext: false);
			locked = true;
		}
		try
		{
			await _gatewayLogger.DebugAsync("Connecting ApiClient").ConfigureAwait(continueOnCapturedContext: false);
			await ApiClient.ConnectAsync().ConfigureAwait(continueOnCapturedContext: false);
			if (_sessionId == null)
			{
				await _gatewayLogger.DebugAsync("Identifying").ConfigureAwait(continueOnCapturedContext: false);
				await ApiClient.SendIdentifyAsync(100, ShardId, TotalShards, _gatewayIntents, BuildCurrentStatus()).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await _gatewayLogger.DebugAsync("Resuming").ConfigureAwait(continueOnCapturedContext: false);
				await ApiClient.SendResumeAsync(_sessionId, _lastSeq).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			if (locked)
			{
				_shardedClient.ReleaseIdentifyLock();
			}
		}
		await _connection.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (LogGatewayIntentWarnings)
		{
			await LogGatewayIntentsWarning().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task OnDisconnectingAsync(Exception ex)
	{
		await _gatewayLogger.DebugAsync("Disconnecting ApiClient").ConfigureAwait(continueOnCapturedContext: false);
		await ApiClient.DisconnectAsync(ex).ConfigureAwait(continueOnCapturedContext: false);
		await _gatewayLogger.DebugAsync("Waiting for heartbeater").ConfigureAwait(continueOnCapturedContext: false);
		Task heartbeatTask = _heartbeatTask;
		if (heartbeatTask != null)
		{
			await heartbeatTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		_heartbeatTask = null;
		long result;
		while (_heartbeatTimes.TryDequeue(out result))
		{
		}
		_lastMessageTime = 0L;
		await _gatewayLogger.DebugAsync("Waiting for guild downloader").ConfigureAwait(continueOnCapturedContext: false);
		Task guildDownloadTask = _guildDownloadTask;
		if (guildDownloadTask != null)
		{
			await guildDownloadTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		_guildDownloadTask = null;
		await _gatewayLogger.DebugAsync("Clearing large guild queue").ConfigureAwait(continueOnCapturedContext: false);
		ulong result2;
		while (_largeGuilds.TryDequeue(out result2))
		{
		}
		await _gatewayLogger.DebugAsync("Raising virtual GuildUnavailables").ConfigureAwait(continueOnCapturedContext: false);
		foreach (SocketGuild guild in State.Guilds)
		{
			if (guild.IsAvailable)
			{
				await GuildUnavailableAsync(guild).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		_sessionId = null;
		_lastSeq = 0;
		ApiClient.ResumeGatewayUrl = null;
	}

	public override async Task<RestApplication> GetApplicationInfoAsync(RequestOptions options = null)
	{
		RestApplication restApplication = _applicationInfo;
		if (restApplication == null)
		{
			restApplication = (_applicationInfo = await ClientHelper.GetApplicationInfoAsync(this, options ?? RequestOptions.Default).ConfigureAwait(continueOnCapturedContext: false));
		}
		return restApplication;
	}

	public override SocketGuild GetGuild(ulong id)
	{
		return State.GetGuild(id);
	}

	public override SocketChannel GetChannel(ulong id)
	{
		return State.GetChannel(id);
	}

	public async ValueTask<IChannel> GetChannelAsync(ulong id, RequestOptions options = null)
	{
		IChannel channel = GetChannel(id);
		if (channel == null)
		{
			channel = await ClientHelper.GetChannelAsync(this, id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return channel;
	}

	public async ValueTask<IUser> GetUserAsync(ulong id, RequestOptions options = null)
	{
		return await ((IDiscordClient)this).GetUserAsync(id, CacheMode.AllowDownload, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public void PurgeChannelCache()
	{
		State.PurgeAllChannels();
	}

	public void PurgeDMChannelCache()
	{
		RemoveDMChannels();
	}

	public override SocketUser GetUser(ulong id)
	{
		return State.GetUser(id);
	}

	public override SocketUser GetUser(string username, string discriminator)
	{
		return State.Users.FirstOrDefault((SocketGlobalUser x) => x.Discriminator == discriminator && x.Username == username);
	}

	public async ValueTask<SocketApplicationCommand> GetGlobalApplicationCommandAsync(ulong id, RequestOptions options = null)
	{
		SocketApplicationCommand command = State.GetCommand(id);
		if (command != null)
		{
			return command;
		}
		ApplicationCommand applicationCommand = await ApiClient.GetGlobalApplicationCommandAsync(id, options);
		if (applicationCommand == null)
		{
			return null;
		}
		command = SocketApplicationCommand.Create(this, applicationCommand);
		State.AddCommand(command);
		return command;
	}

	public async Task<IReadOnlyCollection<SocketApplicationCommand>> GetGlobalApplicationCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		IEnumerable<SocketApplicationCommand> enumerable = (await ApiClient.GetGlobalApplicationCommandsAsync(withLocalizations, locale, options)).Select((ApplicationCommand x) => SocketApplicationCommand.Create(this, x));
		foreach (SocketApplicationCommand item in enumerable)
		{
			State.AddCommand(item);
		}
		return enumerable.ToImmutableArray();
	}

	public async Task<SocketApplicationCommand> CreateGlobalApplicationCommandAsync(ApplicationCommandProperties properties, RequestOptions options = null)
	{
		ApplicationCommand model = await InteractionHelper.CreateGlobalCommandAsync(this, properties, options).ConfigureAwait(continueOnCapturedContext: false);
		SocketApplicationCommand orAddCommand = State.GetOrAddCommand(model.Id, (ulong id) => SocketApplicationCommand.Create(this, model));
		orAddCommand.Update(model);
		return orAddCommand;
	}

	public async Task<IReadOnlyCollection<SocketApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(ApplicationCommandProperties[] properties, RequestOptions options = null)
	{
		IEnumerable<SocketApplicationCommand> enumerable = (await InteractionHelper.BulkOverwriteGlobalCommandsAsync(this, properties, options)).Select((ApplicationCommand x) => SocketApplicationCommand.Create(this, x));
		State.PurgeCommands((SocketApplicationCommand x) => x.IsGlobalCommand);
		foreach (SocketApplicationCommand item in enumerable)
		{
			State.AddCommand(item);
		}
		return enumerable.ToImmutableArray();
	}

	public void PurgeUserCache()
	{
		State.PurgeUsers();
	}

	internal SocketGlobalUser GetOrCreateUser(ClientState state, User model)
	{
		return state.GetOrAddUser(model.Id, (ulong x) => SocketGlobalUser.Create(this, state, model));
	}

	internal SocketUser GetOrCreateTemporaryUser(ClientState state, User model)
	{
		return (SocketUser)(((object)state.GetUser(model.Id)) ?? ((object)SocketUnknownUser.Create(this, state, model)));
	}

	internal SocketGlobalUser GetOrCreateSelfUser(ClientState state, User model)
	{
		return state.GetOrAddUser(model.Id, delegate
		{
			SocketGlobalUser socketGlobalUser = SocketGlobalUser.Create(this, state, model);
			socketGlobalUser.GlobalUser.AddRef();
			socketGlobalUser.Presence = new SocketPresence(UserStatus.Online, null, null);
			return socketGlobalUser;
		});
	}

	internal void RemoveUser(ulong id)
	{
		State.RemoveUser(id);
	}

	public override async Task<SocketSticker> GetStickerAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
	{
		SocketSticker socketSticker = _defaultStickers.FirstOrDefault((StickerPack<SocketSticker> x) => x.Stickers.Any((SocketSticker y) => y.Id == id))?.Stickers.FirstOrDefault((SocketSticker x) => x.Id == id);
		if (socketSticker != null)
		{
			return socketSticker;
		}
		foreach (SocketGuild guild2 in Guilds)
		{
			socketSticker = await guild2.GetStickerAsync(id, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false);
			if (socketSticker != null)
			{
				return socketSticker;
			}
		}
		if (mode == CacheMode.CacheOnly)
		{
			return null;
		}
		Discord.API.Sticker sticker = await ApiClient.GetStickerAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (sticker == null)
		{
			return null;
		}
		if (sticker.GuildId.IsSpecified)
		{
			SocketGuild guild = State.GetGuild(sticker.GuildId.Value);
			return (guild == null) ? SocketSticker.Create(this, sticker) : guild.AddOrUpdateSticker(sticker);
		}
		return SocketSticker.Create(this, sticker);
	}

	public SocketSticker GetSticker(ulong id)
	{
		return GetStickerAsync(id, CacheMode.CacheOnly).GetAwaiter().GetResult();
	}

	public override async ValueTask<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
	{
		if (_parentClient == null)
		{
			if (_voiceRegions == null)
			{
				options = RequestOptions.CreateOrClone(options);
				options.IgnoreState = true;
				_voiceRegions = (await ApiClient.GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((VoiceRegion x) => RestVoiceRegion.Create(this, x)).ToImmutableDictionary((RestVoiceRegion x) => x.Id);
			}
			return _voiceRegions.ToReadOnlyCollection();
		}
		return await _parentClient.GetVoiceRegionsAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async ValueTask<RestVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null)
	{
		if (_parentClient == null)
		{
			if (_voiceRegions == null)
			{
				await GetVoiceRegionsAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			if (_voiceRegions.TryGetValue(id, out var value))
			{
				return value;
			}
			return null;
		}
		return await _parentClient.GetVoiceRegionAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task DownloadUsersAsync(IEnumerable<IGuild> guilds)
	{
		if (ConnectionState == ConnectionState.Connected)
		{
			EnsureGatewayIntent(GatewayIntents.GuildMembers);
			await ProcessUserDownloadsAsync(from x in guilds
				select GetGuild(x.Id) into x
				where x != null
				select x).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task ProcessUserDownloadsAsync(IEnumerable<SocketGuild> guilds)
	{
		System.Collections.Immutable.ImmutableArray<SocketGuild> cachedGuilds = guilds.ToImmutableArray();
		ulong[] batchIds = new ulong[Math.Min(1, cachedGuilds.Length)];
		Task[] batchTasks = new Task[batchIds.Length];
		int batchCount = cachedGuilds.Length / 1;
		int i = 0;
		int k = 0;
		for (; i < batchCount; i++)
		{
			bool isLast = i == batchCount - 1;
			int count = ((!isLast) ? 1 : (cachedGuilds.Length - (batchCount - 1)));
			int num = 0;
			while (num < count)
			{
				SocketGuild socketGuild = cachedGuilds[k];
				batchIds[num] = socketGuild.Id;
				batchTasks[num] = socketGuild.DownloaderPromise;
				num++;
				k++;
			}
			await ApiClient.SendRequestMembersAsync(batchIds).ConfigureAwait(continueOnCapturedContext: false);
			if (!isLast || batchCount <= 1)
			{
				await Task.WhenAll(batchTasks).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await Task.WhenAll(batchTasks.Take(count)).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public override async Task SetStatusAsync(UserStatus status)
	{
		Status = status;
		if (status == UserStatus.AFK)
		{
			_statusSince = DateTimeOffset.UtcNow;
		}
		else
		{
			_statusSince = null;
		}
		await SendStatusAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task SetGameAsync(string name, string streamUrl = null, ActivityType type = ActivityType.Playing)
	{
		if (!string.IsNullOrEmpty(streamUrl))
		{
			Activity = new StreamingGame(name, streamUrl);
		}
		else if (!string.IsNullOrEmpty(name))
		{
			Activity = new Game(name, type);
		}
		else
		{
			Activity = null;
		}
		await SendStatusAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task SetActivityAsync(IActivity activity)
	{
		Activity = activity;
		await SendStatusAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task SendStatusAsync()
	{
		if (CurrentUser != null)
		{
			ImmutableList<IActivity> activities = (_activity.IsSpecified ? ImmutableList.Create(_activity.Value) : null);
			CurrentUser.Presence = new SocketPresence(Status, null, activities);
			(UserStatus, bool, long?, Discord.API.Game) tuple = BuildCurrentStatus() ?? (UserStatus.Online, false, null, null);
			await ApiClient.SendPresenceUpdateAsync(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private (UserStatus, bool, long?, Discord.API.Game)? BuildCurrentStatus()
	{
		UserStatus? status = _status;
		DateTimeOffset? statusSince = _statusSince;
		Optional<IActivity> activity = _activity;
		if (!status.HasValue && !activity.IsSpecified)
		{
			return null;
		}
		Discord.API.Game item = null;
		if (activity.GetValueOrDefault() != null)
		{
			Discord.API.Game game = new Discord.API.Game();
			if (activity.Value is RichGame)
			{
				throw new NotSupportedException("Outgoing Rich Presences are not supported via WebSocket.");
			}
			game.Name = Activity.Name;
			game.Type = Activity.Type;
			if (Activity is StreamingGame streamingGame)
			{
				game.StreamUrl = streamingGame.Url;
			}
			item = game;
		}
		else if (activity.IsSpecified)
		{
			item = null;
		}
		return (status ?? UserStatus.Online, status == UserStatus.AFK, statusSince.HasValue ? new long?(_statusSince.Value.ToUnixTimeMilliseconds()) : ((long?)null), item);
	}

	private async Task LogGatewayIntentsWarning()
	{
		if (_gatewayIntents.HasFlag(GatewayIntents.GuildPresences) && ((_shardedClient == null && !_presenceUpdated.HasSubscribers) || (_shardedClient != null && !_shardedClient._presenceUpdated.HasSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using the GuildPresences intent without listening to the PresenceUpdate event, consider removing the intent from your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
		if (!_gatewayIntents.HasFlag(GatewayIntents.GuildPresences) && ((_shardedClient == null && _presenceUpdated.HasSubscribers) || (_shardedClient != null && _shardedClient._presenceUpdated.HasSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using the PresenceUpdate event without specifying the GuildPresences intent. Discord wont send this event to your client without the intent set in your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
		bool hasGuildScheduledEventsSubscribers = _guildScheduledEventCancelled.HasSubscribers || _guildScheduledEventUserRemove.HasSubscribers || _guildScheduledEventCompleted.HasSubscribers || _guildScheduledEventCreated.HasSubscribers || _guildScheduledEventStarted.HasSubscribers || _guildScheduledEventUpdated.HasSubscribers || _guildScheduledEventUserAdd.HasSubscribers;
		bool shardedClientHasGuildScheduledEventsSubscribers = _shardedClient != null && (_shardedClient._guildScheduledEventCancelled.HasSubscribers || _shardedClient._guildScheduledEventUserRemove.HasSubscribers || _shardedClient._guildScheduledEventCompleted.HasSubscribers || _shardedClient._guildScheduledEventCreated.HasSubscribers || _shardedClient._guildScheduledEventStarted.HasSubscribers || _shardedClient._guildScheduledEventUpdated.HasSubscribers || _shardedClient._guildScheduledEventUserAdd.HasSubscribers);
		if (_gatewayIntents.HasFlag(GatewayIntents.GuildScheduledEvents) && ((_shardedClient == null && !hasGuildScheduledEventsSubscribers) || (_shardedClient != null && !shardedClientHasGuildScheduledEventsSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using the GuildScheduledEvents gateway intent without listening to any events related to that intent, consider removing the intent from your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
		if (!_gatewayIntents.HasFlag(GatewayIntents.GuildScheduledEvents) && ((_shardedClient == null && hasGuildScheduledEventsSubscribers) || (_shardedClient != null && shardedClientHasGuildScheduledEventsSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using events related to the GuildScheduledEvents gateway intent without specifying the intent. Discord wont send this event to your client without the intent set in your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
		bool hasInviteEventSubscribers = _inviteCreatedEvent.HasSubscribers || _inviteDeletedEvent.HasSubscribers;
		bool shardedClientHasInviteEventSubscribers = _shardedClient != null && (_shardedClient._inviteCreatedEvent.HasSubscribers || _shardedClient._inviteDeletedEvent.HasSubscribers);
		if (_gatewayIntents.HasFlag(GatewayIntents.GuildInvites) && ((_shardedClient == null && !hasInviteEventSubscribers) || (_shardedClient != null && !shardedClientHasInviteEventSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using the GuildInvites gateway intent without listening to any events related to that intent, consider removing the intent from your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
		if (!_gatewayIntents.HasFlag(GatewayIntents.GuildInvites) && ((_shardedClient == null && hasInviteEventSubscribers) || (_shardedClient != null && shardedClientHasInviteEventSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using events related to the GuildInvites gateway intent without specifying the intent. Discord wont send this event to your client without the intent set in your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task ProcessMessageAsync(GatewayOpCode opCode, int? seq, string type, object payload)
	{
		if (seq.HasValue)
		{
			_lastSeq = seq.Value;
		}
		_lastMessageTime = Environment.TickCount;
		try
		{
			switch (opCode)
			{
			case GatewayOpCode.Hello:
			{
				await _gatewayLogger.DebugAsync("Received Hello").ConfigureAwait(continueOnCapturedContext: false);
				HelloEvent helloEvent = (payload as JToken).ToObject<HelloEvent>(_serializer);
				_heartbeatTask = RunHeartbeatAsync(helloEvent.HeartbeatInterval, _connection.CancelToken);
				break;
			}
			case GatewayOpCode.Heartbeat:
				await _gatewayLogger.DebugAsync("Received Heartbeat").ConfigureAwait(continueOnCapturedContext: false);
				await ApiClient.SendHeartbeatAsync(_lastSeq).ConfigureAwait(continueOnCapturedContext: false);
				break;
			case GatewayOpCode.HeartbeatAck:
			{
				await _gatewayLogger.DebugAsync("Received HeartbeatAck").ConfigureAwait(continueOnCapturedContext: false);
				if (_heartbeatTimes.TryDequeue(out var result))
				{
					int num4 = (int)(Environment.TickCount - result);
					int latency = Latency;
					Latency = num4;
					await TimedInvokeAsync(_latencyUpdatedEvent, "LatencyUpdated", latency, num4).ConfigureAwait(continueOnCapturedContext: false);
				}
				break;
			}
			case GatewayOpCode.InvalidSession:
				await _gatewayLogger.DebugAsync("Received InvalidSession").ConfigureAwait(continueOnCapturedContext: false);
				await _gatewayLogger.WarningAsync("Failed to resume previous session").ConfigureAwait(continueOnCapturedContext: false);
				_sessionId = null;
				_lastSeq = 0;
				ApiClient.ResumeGatewayUrl = null;
				if (_shardedClient != null)
				{
					await _shardedClient.AcquireIdentifyLockAsync(ShardId, _connection.CancelToken).ConfigureAwait(continueOnCapturedContext: false);
					try
					{
						await ApiClient.SendIdentifyAsync(100, ShardId, TotalShards, _gatewayIntents, BuildCurrentStatus()).ConfigureAwait(continueOnCapturedContext: false);
					}
					finally
					{
						_shardedClient.ReleaseIdentifyLock();
					}
				}
				else
				{
					await ApiClient.SendIdentifyAsync(100, ShardId, TotalShards, _gatewayIntents, BuildCurrentStatus()).ConfigureAwait(continueOnCapturedContext: false);
				}
				break;
			case GatewayOpCode.Reconnect:
				await _gatewayLogger.DebugAsync("Received Reconnect").ConfigureAwait(continueOnCapturedContext: false);
				_connection.Error(new GatewayReconnectException("Server requested a reconnect"));
				break;
			case GatewayOpCode.Dispatch:
				switch (type)
				{
				case "READY":
					try
					{
						await _gatewayLogger.DebugAsync("Received Dispatch (READY)").ConfigureAwait(continueOnCapturedContext: false);
						ReadyEvent data19 = (payload as JToken).ToObject<ReadyEvent>(_serializer);
						ClientState state = new ClientState(data19.Guilds.Length, data19.PrivateChannels.Length);
						SocketSelfUser currentUser = SocketSelfUser.Create(this, state, data19.User);
						Rest.CreateRestSelfUser(data19.User);
						ImmutableList<IActivity> activities = (_activity.IsSpecified ? ImmutableList.Create(_activity.Value) : null);
						currentUser.Presence = new SocketPresence(Status, null, activities);
						ApiClient.CurrentUserId = currentUser.Id;
						ApiClient.CurrentApplicationId = data19.Application.Id;
						Rest.CurrentUser = RestSelfUser.Create(this, data19.User);
						int unavailableGuilds = 0;
						for (int i = 0; i < data19.Guilds.Length; i++)
						{
							ExtendedGuild model2 = data19.Guilds[i];
							SocketGuild socketGuild8 = AddGuild(model2, state);
							if (!socketGuild8.IsAvailable)
							{
								unavailableGuilds++;
							}
							else
							{
								await GuildAvailableAsync(socketGuild8).ConfigureAwait(continueOnCapturedContext: false);
							}
						}
						for (int num3 = 0; num3 < data19.PrivateChannels.Length; num3++)
						{
							AddPrivateChannel(data19.PrivateChannels[num3], state);
						}
						_sessionId = data19.SessionId;
						ApiClient.ResumeGatewayUrl = data19.ResumeGatewayUrl;
						_unavailableGuildCount = unavailableGuilds;
						CurrentUser = currentUser;
						_previousSessionUser = CurrentUser;
						State = state;
					}
					catch (Exception innerException)
					{
						_connection.CriticalError(new Exception("Processing READY failed", innerException));
						return;
					}
					_lastGuildAvailableTime = Environment.TickCount;
					_guildDownloadTask = WaitForGuildsAsync(_connection.CancelToken, _gatewayLogger).ContinueWith((Func<Task, Task>)async delegate(Task x)
					{
						if (x.IsFaulted)
						{
							_connection.Error(x.Exception);
						}
						else if (!_connection.CancelToken.IsCancellationRequested)
						{
							if (BaseConfig.AlwaysDownloadUsers)
							{
								DownloadUsersAsync(Guilds.Where((SocketGuild socketGuild9) => socketGuild9.IsAvailable && !socketGuild9.HasAllMembers));
							}
							await TimedInvokeAsync(_readyEvent, "Ready").ConfigureAwait(continueOnCapturedContext: false);
							await _gatewayLogger.InfoAsync("Ready").ConfigureAwait(continueOnCapturedContext: false);
						}
					});
					_connection.CompleteAsync();
					break;
				case "RESUMED":
					await _gatewayLogger.DebugAsync("Received Dispatch (RESUMED)").ConfigureAwait(continueOnCapturedContext: false);
					_connection.CompleteAsync();
					foreach (SocketGuild guild35 in State.Guilds)
					{
						if (guild35.IsAvailable)
						{
							await GuildAvailableAsync(guild35).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					CurrentUser = _previousSessionUser;
					await _gatewayLogger.InfoAsync("Resumed previous session").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "GUILD_CREATE":
				{
					ExtendedGuild data = (payload as JToken).ToObject<ExtendedGuild>(_serializer);
					if (data.Unavailable == false)
					{
						type = "GUILD_AVAILABLE";
						_lastGuildAvailableTime = Environment.TickCount;
						await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_AVAILABLE)").ConfigureAwait(continueOnCapturedContext: false);
						SocketGuild guild = State.GetGuild(data.Id);
						if (guild == null)
						{
							await UnknownGuildAsync(type, data.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						guild.Update(State, data);
						if (_unavailableGuildCount != 0)
						{
							_unavailableGuildCount--;
						}
						await GuildAvailableAsync(guild).ConfigureAwait(continueOnCapturedContext: false);
						if (guild.DownloadedMemberCount >= guild.MemberCount && !guild.DownloaderPromise.IsCompleted)
						{
							guild.CompleteDownloadUsers();
							await TimedInvokeAsync(_guildMembersDownloadedEvent, "GuildMembersDownloaded", guild).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					else
					{
						await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
						SocketGuild guild = AddGuild(data, State);
						if (guild == null)
						{
							await UnknownGuildAsync(type, data.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_joinedGuildEvent, "JoinedGuild", guild).ConfigureAwait(continueOnCapturedContext: false);
						await GuildAvailableAsync(guild).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "GUILD_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Guild guild26 = (payload as JToken).ToObject<Guild>(_serializer);
					SocketGuild guild27 = State.GetGuild(guild26.Id);
					if (guild27 != null)
					{
						SocketGuild arg30 = guild27.Clone();
						guild27.Update(State, guild26);
						await TimedInvokeAsync(_guildUpdatedEvent, "GuildUpdated", arg30, guild27).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guild26.Id).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_EMOJIS_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_EMOJIS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildEmojiUpdateEvent guildEmojiUpdateEvent = (payload as JToken).ToObject<GuildEmojiUpdateEvent>(_serializer);
					SocketGuild guild4 = State.GetGuild(guildEmojiUpdateEvent.GuildId);
					if (guild4 != null)
					{
						SocketGuild arg2 = guild4.Clone();
						guild4.Update(State, guildEmojiUpdateEvent);
						await TimedInvokeAsync(_guildUpdatedEvent, "GuildUpdated", arg2, guild4).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guildEmojiUpdateEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_SYNC":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (GUILD_SYNC)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "GUILD_DELETE":
				{
					ExtendedGuild data = (payload as JToken).ToObject<ExtendedGuild>(_serializer);
					if (data.Unavailable == true)
					{
						type = "GUILD_UNAVAILABLE";
						await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_UNAVAILABLE)").ConfigureAwait(continueOnCapturedContext: false);
						SocketGuild guild28 = State.GetGuild(data.Id);
						if (guild28 == null)
						{
							await UnknownGuildAsync(type, data.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await GuildUnavailableAsync(guild28).ConfigureAwait(continueOnCapturedContext: false);
						_unavailableGuildCount++;
					}
					else
					{
						await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
						SocketGuild guild = RemoveGuild(data.Id);
						if (guild == null)
						{
							await UnknownGuildAsync(type, data.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await GuildUnavailableAsync(guild).ConfigureAwait(continueOnCapturedContext: false);
						await TimedInvokeAsync(_leftGuildEvent, "LeftGuild", guild).ConfigureAwait(continueOnCapturedContext: false);
						((IDisposable)guild).Dispose();
					}
					break;
				}
				case "GUILD_STICKERS_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_STICKERS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildStickerUpdateEvent data17 = (payload as JToken).ToObject<GuildStickerUpdateEvent>(_serializer);
					SocketGuild guild19 = State.GetGuild(data17.GuildId);
					if (guild19 == null)
					{
						await UnknownGuildAsync(type, data17.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					IEnumerable<Discord.API.Sticker> enumerable = data17.Stickers.Where((Discord.API.Sticker x) => !guild19.Stickers.Any((SocketCustomSticker y) => y.Id == x.Id));
					IEnumerable<SocketCustomSticker> deletedStickers = guild19.Stickers.Where((SocketCustomSticker x) => !data17.Stickers.Any((Discord.API.Sticker y) => y.Id == x.Id));
					(SocketCustomSticker Entity, Discord.API.Sticker Model)[] updatedStickers = (from x in data17.Stickers.Select(delegate(Discord.API.Sticker x)
						{
							SocketCustomSticker socketCustomSticker = guild19.Stickers.FirstOrDefault((SocketCustomSticker y) => y.Id == x.Id);
							if (socketCustomSticker == null)
							{
								return ((SocketCustomSticker Entity, Discord.API.Sticker Model)?)null;
							}
							return (!socketCustomSticker.Equals(x)) ? new(SocketCustomSticker, Discord.API.Sticker)?((socketCustomSticker, x)) : (((SocketCustomSticker, Discord.API.Sticker)?)null);
						})
						where x.HasValue
						select x.Value).ToArray();
					foreach (Discord.API.Sticker item2 in enumerable)
					{
						SocketCustomSticker arg24 = guild19.AddSticker(item2);
						await TimedInvokeAsync(_guildStickerCreated, "GuildStickerCreated", arg24);
					}
					foreach (SocketCustomSticker item3 in deletedStickers)
					{
						SocketCustomSticker arg25 = guild19.RemoveSticker(item3.Id);
						await TimedInvokeAsync(_guildStickerDeleted, "GuildStickerDeleted", arg25);
					}
					(SocketCustomSticker Entity, Discord.API.Sticker Model)[] array = updatedStickers;
					for (int unavailableGuilds = 0; unavailableGuilds < array.Length; unavailableGuilds++)
					{
						(SocketCustomSticker, Discord.API.Sticker) tuple = array[unavailableGuilds];
						SocketCustomSticker arg26 = tuple.Item1.Clone();
						tuple.Item1.Update(tuple.Item2);
						await TimedInvokeAsync(_guildStickerUpdated, "GuildStickerUpdated", arg26, tuple.Item1);
					}
					break;
				}
				case "CHANNEL_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (CHANNEL_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel channel8 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketChannel socketChannel3;
					if (channel8.GuildId.IsSpecified)
					{
						SocketGuild guild23 = State.GetGuild(channel8.GuildId.Value);
						if (guild23 == null)
						{
							await UnknownGuildAsync(type, channel8.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						socketChannel3 = guild23.AddChannel(State, channel8);
						if (!guild23.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild23.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
					}
					else
					{
						socketChannel3 = State.GetChannel(channel8.Id);
						if (socketChannel3 != null)
						{
							return;
						}
						socketChannel3 = AddPrivateChannel(channel8, State) as SocketChannel;
					}
					if (socketChannel3 != null)
					{
						await TimedInvokeAsync(_channelCreatedEvent, "ChannelCreated", socketChannel3).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "CHANNEL_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (CHANNEL_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel channel5 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketChannel channel6 = State.GetChannel(channel5.Id);
					if (channel6 != null)
					{
						SocketChannel arg18 = channel6.Clone();
						channel6.Update(State, channel5);
						SocketGuild socketGuild6 = (channel6 as SocketGuildChannel)?.Guild;
						if (!(socketGuild6?.IsSynced ?? true))
						{
							await UnsyncedGuildAsync(type, socketGuild6.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_channelUpdatedEvent, "ChannelUpdated", arg18, channel6).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownChannelAsync(type, channel5.Id).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "CHANNEL_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (CHANNEL_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel channel4 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketChannel socketChannel2;
					if (channel4.GuildId.IsSpecified)
					{
						SocketGuild guild15 = State.GetGuild(channel4.GuildId.Value);
						if (guild15 == null)
						{
							await UnknownGuildAsync(type, channel4.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						socketChannel2 = guild15.RemoveChannel(State, channel4.Id);
						if (!guild15.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild15.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
					}
					else
					{
						socketChannel2 = RemovePrivateChannel(channel4.Id) as SocketChannel;
					}
					if (socketChannel2 == null)
					{
						await UnknownChannelAsync(type, channel4.Id, channel4.GuildId.GetValueOrDefault(0uL)).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					await TimedInvokeAsync(_channelDestroyedEvent, "ChannelDestroyed", socketChannel2).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "GUILD_MEMBER_ADD":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_MEMBER_ADD)").ConfigureAwait(continueOnCapturedContext: false);
					GuildMemberAddEvent guildMemberAddEvent = (payload as JToken).ToObject<GuildMemberAddEvent>(_serializer);
					SocketGuild guild13 = State.GetGuild(guildMemberAddEvent.GuildId);
					if (guild13 != null)
					{
						SocketGuildUser arg13 = guild13.AddOrUpdateUser(guildMemberAddEvent);
						guild13.MemberCount++;
						if (!guild13.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild13.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_userJoinedEvent, "UserJoined", arg13).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guildMemberAddEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_MEMBER_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_MEMBER_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildMemberUpdateEvent data8 = (payload as JToken).ToObject<GuildMemberUpdateEvent>(_serializer);
					SocketGuild guild12 = State.GetGuild(data8.GuildId);
					if (guild12 != null)
					{
						SocketGuildUser user3 = guild12.GetUser(data8.User.Id);
						if (!guild12.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild12.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (user3 != null)
						{
							SocketGuildUser before = user3.Clone();
							if (user3.GlobalUser.Update(State, data8.User))
							{
								await TimedInvokeAsync(_userUpdatedEvent, "UserUpdated", before.GlobalUser, user3).ConfigureAwait(continueOnCapturedContext: false);
							}
							user3.Update(State, data8);
							await TimedInvokeAsync<Cacheable<SocketGuildUser, ulong>, SocketGuildUser>(arg1: new Cacheable<SocketGuildUser, ulong>(before, user3.Id, hasValue: true, () => Task.FromResult<SocketGuildUser>(null)), eventHandler: _guildMemberUpdatedEvent, name: "GuildMemberUpdated", arg2: user3).ConfigureAwait(continueOnCapturedContext: false);
						}
						else
						{
							user3 = guild12.AddOrUpdateUser(data8);
							await TimedInvokeAsync<Cacheable<SocketGuildUser, ulong>, SocketGuildUser>(arg1: new Cacheable<SocketGuildUser, ulong>(null, user3.Id, hasValue: false, () => Task.FromResult<SocketGuildUser>(null)), eventHandler: _guildMemberUpdatedEvent, name: "GuildMemberUpdated", arg2: user3).ConfigureAwait(continueOnCapturedContext: false);
						}
						break;
					}
					await UnknownGuildAsync(type, data8.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_MEMBER_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_MEMBER_REMOVE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildMemberRemoveEvent data21 = (payload as JToken).ToObject<GuildMemberRemoveEvent>(_serializer);
					SocketGuild guild32 = State.GetGuild(data21.GuildId);
					if (guild32 != null)
					{
						SocketUser socketUser7 = guild32.RemoveUser(data21.User.Id);
						guild32.MemberCount--;
						if (!guild32.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild32.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (socketUser7 == null)
						{
							socketUser7 = State.GetUser(data21.User.Id);
						}
						if (socketUser7 != null)
						{
							socketUser7.Update(State, data21.User);
						}
						else
						{
							socketUser7 = State.GetOrAddUser(data21.User.Id, (ulong x) => SocketGlobalUser.Create(this, State, data21.User));
						}
						await TimedInvokeAsync(_userLeftEvent, "UserLeft", guild32, socketUser7).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, data21.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_MEMBERS_CHUNK":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_MEMBERS_CHUNK)").ConfigureAwait(continueOnCapturedContext: false);
					GuildMembersChunkEvent guildMembersChunkEvent = (payload as JToken).ToObject<GuildMembersChunkEvent>(_serializer);
					SocketGuild guild16 = State.GetGuild(guildMembersChunkEvent.GuildId);
					if (guild16 != null)
					{
						GuildMember[] members = guildMembersChunkEvent.Members;
						foreach (GuildMember model in members)
						{
							guild16.AddOrUpdateUser(model);
						}
						if (guild16.DownloadedMemberCount >= guild16.MemberCount && !guild16.DownloaderPromise.IsCompleted)
						{
							guild16.CompleteDownloadUsers();
							await TimedInvokeAsync(_guildMembersDownloadedEvent, "GuildMembersDownloaded", guild16).ConfigureAwait(continueOnCapturedContext: false);
						}
						break;
					}
					await UnknownGuildAsync(type, guildMembersChunkEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_JOIN_REQUEST_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_JOIN_REQUEST_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildJoinRequestDeleteEvent guildJoinRequestDeleteEvent = (payload as JToken).ToObject<GuildJoinRequestDeleteEvent>(_serializer);
					SocketGuild guild5 = State.GetGuild(guildJoinRequestDeleteEvent.GuildId);
					if (guild5 == null)
					{
						await UnknownGuildAsync(type, guildJoinRequestDeleteEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketGuildUser socketGuildUser = guild5.RemoveUser(guildJoinRequestDeleteEvent.UserId);
					guild5.MemberCount--;
					await TimedInvokeAsync<Cacheable<SocketGuildUser, ulong>, SocketGuild>(arg1: new Cacheable<SocketGuildUser, ulong>(socketGuildUser, guildJoinRequestDeleteEvent.UserId, socketGuildUser != null, () => Task.FromResult<SocketGuildUser>(null)), eventHandler: _guildJoinRequestDeletedEvent, name: "GuildJoinRequestDeleted", arg2: guild5).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "CHANNEL_RECIPIENT_ADD":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (CHANNEL_RECIPIENT_ADD)").ConfigureAwait(continueOnCapturedContext: false);
					RecipientEvent recipientEvent = (payload as JToken).ToObject<RecipientEvent>(_serializer);
					if (State.GetChannel(recipientEvent.ChannelId) is SocketGroupChannel socketGroupChannel)
					{
						SocketGroupUser orAddUser = socketGroupChannel.GetOrAddUser(recipientEvent.User);
						await TimedInvokeAsync(_recipientAddedEvent, "RecipientAdded", orAddUser).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownChannelAsync(type, recipientEvent.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "CHANNEL_RECIPIENT_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (CHANNEL_RECIPIENT_REMOVE)").ConfigureAwait(continueOnCapturedContext: false);
					RecipientEvent recipientEvent2 = (payload as JToken).ToObject<RecipientEvent>(_serializer);
					if (State.GetChannel(recipientEvent2.ChannelId) is SocketGroupChannel socketGroupChannel3)
					{
						SocketGroupUser socketGroupUser = socketGroupChannel3.RemoveUser(recipientEvent2.User.Id);
						if (socketGroupUser == null)
						{
							await UnknownChannelUserAsync(type, recipientEvent2.User.Id, recipientEvent2.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_recipientRemovedEvent, "RecipientRemoved", socketGroupUser).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownChannelAsync(type, recipientEvent2.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_ROLE_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_ROLE_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildRoleCreateEvent guildRoleCreateEvent = (payload as JToken).ToObject<GuildRoleCreateEvent>(_serializer);
					SocketGuild guild17 = State.GetGuild(guildRoleCreateEvent.GuildId);
					if (guild17 != null)
					{
						SocketRole arg20 = guild17.AddRole(guildRoleCreateEvent.Role);
						if (!guild17.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild17.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_roleCreatedEvent, "RoleCreated", arg20).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guildRoleCreateEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_ROLE_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_ROLE_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildRoleUpdateEvent guildRoleUpdateEvent = (payload as JToken).ToObject<GuildRoleUpdateEvent>(_serializer);
					SocketGuild guild8 = State.GetGuild(guildRoleUpdateEvent.GuildId);
					if (guild8 != null)
					{
						SocketRole role = guild8.GetRole(guildRoleUpdateEvent.Role.Id);
						if (role != null)
						{
							SocketRole arg10 = role.Clone();
							role.Update(State, guildRoleUpdateEvent.Role);
							if (!guild8.IsSynced)
							{
								await UnsyncedGuildAsync(type, guild8.Id).ConfigureAwait(continueOnCapturedContext: false);
								return;
							}
							await TimedInvokeAsync(_roleUpdatedEvent, "RoleUpdated", arg10, role).ConfigureAwait(continueOnCapturedContext: false);
							break;
						}
						await UnknownRoleAsync(type, guildRoleUpdateEvent.Role.Id, guild8.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					await UnknownGuildAsync(type, guildRoleUpdateEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_ROLE_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_ROLE_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildRoleDeleteEvent guildRoleDeleteEvent = (payload as JToken).ToObject<GuildRoleDeleteEvent>(_serializer);
					SocketGuild guild25 = State.GetGuild(guildRoleDeleteEvent.GuildId);
					if (guild25 != null)
					{
						SocketRole socketRole = guild25.RemoveRole(guildRoleDeleteEvent.RoleId);
						if (socketRole != null)
						{
							if (!guild25.IsSynced)
							{
								await UnsyncedGuildAsync(type, guild25.Id).ConfigureAwait(continueOnCapturedContext: false);
								return;
							}
							await TimedInvokeAsync(_roleDeletedEvent, "RoleDeleted", socketRole).ConfigureAwait(continueOnCapturedContext: false);
							break;
						}
						await UnknownRoleAsync(type, guildRoleDeleteEvent.RoleId, guild25.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					await UnknownGuildAsync(type, guildRoleDeleteEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_BAN_ADD":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_BAN_ADD)").ConfigureAwait(continueOnCapturedContext: false);
					GuildBanEvent guildBanEvent = (payload as JToken).ToObject<GuildBanEvent>(_serializer);
					SocketGuild guild11 = State.GetGuild(guildBanEvent.GuildId);
					if (guild11 != null)
					{
						if (!guild11.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild11.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						SocketUser socketUser3 = guild11.GetUser(guildBanEvent.User.Id);
						if (socketUser3 == null)
						{
							socketUser3 = SocketUnknownUser.Create(this, State, guildBanEvent.User);
						}
						await TimedInvokeAsync(_userBannedEvent, "UserBanned", socketUser3, guild11).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guildBanEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_BAN_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_BAN_REMOVE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildBanEvent guildBanEvent2 = (payload as JToken).ToObject<GuildBanEvent>(_serializer);
					SocketGuild guild29 = State.GetGuild(guildBanEvent2.GuildId);
					if (guild29 != null)
					{
						if (!guild29.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild29.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						SocketUser socketUser6 = State.GetUser(guildBanEvent2.User.Id);
						if (socketUser6 == null)
						{
							socketUser6 = SocketUnknownUser.Create(this, State, guildBanEvent2.User);
						}
						await TimedInvokeAsync(_userUnbannedEvent, "UserUnbanned", socketUser6, guild29).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guildBanEvent2.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "MESSAGE_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Message message = (payload as JToken).ToObject<Message>(_serializer);
					ISocketMessageChannel socketMessageChannel4 = GetChannel(message.ChannelId) as ISocketMessageChannel;
					SocketGuild socketGuild4 = (socketMessageChannel4 as SocketGuildChannel)?.Guild;
					if (socketGuild4 != null && !socketGuild4.IsSynced)
					{
						await UnsyncedGuildAsync(type, socketGuild4.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					if (socketMessageChannel4 == null)
					{
						if (message.GuildId.IsSpecified)
						{
							await UnknownChannelAsync(type, message.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						socketMessageChannel4 = CreateDMChannel(message.ChannelId, message.Author.Value, State);
					}
					SocketUser socketUser4 = ((socketGuild4 == null) ? (socketMessageChannel4 as SocketChannel).GetUser(message.Author.Value.Id) : ((!message.WebhookId.IsSpecified) ? ((SocketUser)socketGuild4.GetUser(message.Author.Value.Id)) : ((SocketUser)SocketWebhookUser.Create(socketGuild4, State, message.Author.Value, message.WebhookId.Value))));
					if (socketUser4 == null)
					{
						if (socketGuild4 != null)
						{
							if (message.Member.IsSpecified)
							{
								message.Member.Value.User = message.Author.Value;
								socketUser4 = socketGuild4.AddOrUpdateUser(message.Member.Value);
							}
							else
							{
								socketUser4 = socketGuild4.AddOrUpdateUser(message.Author.Value);
							}
						}
						else
						{
							if (!(socketMessageChannel4 is SocketGroupChannel socketGroupChannel2))
							{
								await UnknownChannelUserAsync(type, message.Author.Value.Id, socketMessageChannel4.Id).ConfigureAwait(continueOnCapturedContext: false);
								return;
							}
							socketUser4 = socketGroupChannel2.GetOrAddUser(message.Author.Value);
						}
					}
					SocketMessage socketMessage3 = SocketMessage.Create(this, State, socketUser4, socketMessageChannel4, message);
					SocketChannelHelper.AddMessage(socketMessageChannel4, this, socketMessage3);
					await TimedInvokeAsync(_messageReceivedEvent, "MessageReceived", socketMessage3).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Message data15 = (payload as JToken).ToObject<Message>(_serializer);
					ISocketMessageChannel channel7 = GetChannel(data15.ChannelId) as ISocketMessageChannel;
					SocketGuild socketGuild7 = (channel7 as SocketGuildChannel)?.Guild;
					if (socketGuild7 != null && !socketGuild7.IsSynced)
					{
						await UnsyncedGuildAsync(type, socketGuild7.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketMessage value = null;
					SocketMessage socketMessage4 = channel7?.GetCachedMessage(data15.Id);
					bool flag2 = socketMessage4 != null;
					SocketMessage arg21;
					if (flag2)
					{
						value = socketMessage4.Clone();
						socketMessage4.Update(State, data15);
						arg21 = socketMessage4;
					}
					else
					{
						SocketUser socketUser5;
						if (data15.Author.IsSpecified)
						{
							socketUser5 = ((socketGuild7 == null) ? (channel7 as SocketChannel)?.GetUser(data15.Author.Value.Id) : ((!data15.WebhookId.IsSpecified) ? ((SocketUser)socketGuild7.GetUser(data15.Author.Value.Id)) : ((SocketUser)SocketWebhookUser.Create(socketGuild7, State, data15.Author.Value, data15.WebhookId.Value))));
							if (socketUser5 == null)
							{
								if (socketGuild7 != null)
								{
									if (data15.Member.IsSpecified)
									{
										data15.Member.Value.User = data15.Author.Value;
										socketUser5 = socketGuild7.AddOrUpdateUser(data15.Member.Value);
									}
									else
									{
										socketUser5 = socketGuild7.AddOrUpdateUser(data15.Author.Value);
									}
								}
								else if (channel7 is SocketGroupChannel socketGroupChannel5)
								{
									socketUser5 = socketGroupChannel5.GetOrAddUser(data15.Author.Value);
								}
							}
						}
						else
						{
							socketUser5 = new SocketUnknownUser(this, 0uL);
						}
						if (channel7 == null)
						{
							if (data15.GuildId.IsSpecified)
							{
								await UnknownChannelAsync(type, data15.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
								return;
							}
							if (data15.Author.IsSpecified)
							{
								socketUser5 = ((SocketDMChannel)(channel7 = CreateDMChannel(data15.ChannelId, data15.Author.Value, State))).Recipient;
							}
							else
							{
								channel7 = CreateDMChannel(data15.ChannelId, socketUser5, State);
							}
						}
						arg21 = SocketMessage.Create(this, State, socketUser5, channel7, data15);
					}
					await TimedInvokeAsync<Cacheable<IMessage, ulong>, SocketMessage, ISocketMessageChannel>(arg1: new Cacheable<IMessage, ulong>(value, data15.Id, flag2, async () => await channel7.GetMessageAsync(data15.Id).ConfigureAwait(continueOnCapturedContext: false)), eventHandler: _messageUpdatedEvent, name: "MessageUpdated", arg2: arg21, arg3: channel7).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					Message data7 = (payload as JToken).ToObject<Message>(_serializer);
					ISocketMessageChannel socketMessageChannel3 = GetChannel(data7.ChannelId) as ISocketMessageChannel;
					SocketGuild socketGuild3 = (socketMessageChannel3 as SocketGuildChannel)?.Guild;
					if (!(socketGuild3?.IsSynced ?? true))
					{
						await UnsyncedGuildAsync(type, socketGuild3.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketMessage socketMessage2 = null;
					if (socketMessageChannel3 != null)
					{
						socketMessage2 = SocketChannelHelper.RemoveMessage(socketMessageChannel3, this, data7.Id);
					}
					await TimedInvokeAsync(arg1: new Cacheable<IMessage, ulong>(socketMessage2, data7.Id, socketMessage2 != null, () => Task.FromResult<IMessage>(null)), arg2: new Cacheable<IMessageChannel, ulong>(socketMessageChannel3, data7.ChannelId, socketMessageChannel3 != null, async () => (await GetChannelAsync(data7.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel), eventHandler: _messageDeletedEvent, name: "MessageDeleted").ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_REACTION_ADD":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_REACTION_ADD)").ConfigureAwait(continueOnCapturedContext: false);
					Discord.API.Gateway.Reaction data10 = (payload as JToken).ToObject<Discord.API.Gateway.Reaction>(_serializer);
					ISocketMessageChannel channel3 = GetChannel(data10.ChannelId) as ISocketMessageChannel;
					SocketUserMessage cachedMsg = channel3?.GetCachedMessage(data10.MessageId) as SocketUserMessage;
					bool isMsgCached = cachedMsg != null;
					IUser user4 = null;
					if (channel3 != null)
					{
						user4 = await channel3.GetUserAsync(data10.UserId, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false);
					}
					Optional<SocketUserMessage> message2 = ((!isMsgCached) ? Optional.Create<SocketUserMessage>() : Optional.Create(cachedMsg));
					if (data10.Member.IsSpecified)
					{
						SocketGuild socketGuild5 = (channel3 as SocketGuildChannel)?.Guild;
						if (socketGuild5 != null)
						{
							user4 = socketGuild5.AddOrUpdateUser(data10.Member.Value);
						}
					}
					else
					{
						user4 = GetUser(data10.UserId);
					}
					Optional<IUser> user5 = ((user4 == null) ? Optional.Create<IUser>() : Optional.Create(user4));
					Cacheable<IMessageChannel, ulong> cacheableChannel = new Cacheable<IMessageChannel, ulong>(channel3, data10.ChannelId, channel3 != null, async () => (await GetChannelAsync(data10.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					Cacheable<IUserMessage, ulong> arg15 = new Cacheable<IUserMessage, ulong>(cachedMsg, data10.MessageId, isMsgCached, async () => (await (await cacheableChannel.GetOrDownloadAsync().ConfigureAwait(continueOnCapturedContext: false)).GetMessageAsync(data10.MessageId).ConfigureAwait(continueOnCapturedContext: false)) as IUserMessage);
					SocketReaction socketReaction = SocketReaction.Create(data10, channel3, message2, user5);
					cachedMsg?.AddReaction(socketReaction);
					await TimedInvokeAsync(_reactionAddedEvent, "ReactionAdded", arg15, cacheableChannel, socketReaction).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_REACTION_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_REACTION_REMOVE)").ConfigureAwait(continueOnCapturedContext: false);
					Discord.API.Gateway.Reaction data20 = (payload as JToken).ToObject<Discord.API.Gateway.Reaction>(_serializer);
					ISocketMessageChannel channel3 = GetChannel(data20.ChannelId) as ISocketMessageChannel;
					SocketUserMessage cachedMsg = channel3?.GetCachedMessage(data20.MessageId) as SocketUserMessage;
					bool isMsgCached = cachedMsg != null;
					IUser user9 = null;
					if (channel3 != null)
					{
						user9 = await channel3.GetUserAsync(data20.UserId, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false);
					}
					else if (!data20.GuildId.IsSpecified)
					{
						user9 = GetUser(data20.UserId);
					}
					Optional<SocketUserMessage> message3 = ((!isMsgCached) ? Optional.Create<SocketUserMessage>() : Optional.Create(cachedMsg));
					Optional<IUser> user10 = ((user9 == null) ? Optional.Create<IUser>() : Optional.Create(user9));
					Cacheable<IMessageChannel, ulong> cacheableChannel4 = new Cacheable<IMessageChannel, ulong>(channel3, data20.ChannelId, channel3 != null, async () => (await GetChannelAsync(data20.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					Cacheable<IUserMessage, ulong> arg31 = new Cacheable<IUserMessage, ulong>(cachedMsg, data20.MessageId, isMsgCached, async () => (await (await cacheableChannel4.GetOrDownloadAsync().ConfigureAwait(continueOnCapturedContext: false)).GetMessageAsync(data20.MessageId).ConfigureAwait(continueOnCapturedContext: false)) as IUserMessage);
					SocketReaction socketReaction2 = SocketReaction.Create(data20, channel3, message3, user10);
					cachedMsg?.RemoveReaction(socketReaction2);
					await TimedInvokeAsync(_reactionRemovedEvent, "ReactionRemoved", arg31, cacheableChannel4, socketReaction2).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_REACTION_REMOVE_ALL":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_REACTION_REMOVE_ALL)").ConfigureAwait(continueOnCapturedContext: false);
					RemoveAllReactionsEvent data14 = (payload as JToken).ToObject<RemoveAllReactionsEvent>(_serializer);
					ISocketMessageChannel socketMessageChannel6 = GetChannel(data14.ChannelId) as ISocketMessageChannel;
					Cacheable<IMessageChannel, ulong> cacheableChannel3 = new Cacheable<IMessageChannel, ulong>(socketMessageChannel6, data14.ChannelId, socketMessageChannel6 != null, async () => (await GetChannelAsync(data14.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					SocketUserMessage socketUserMessage2 = socketMessageChannel6?.GetCachedMessage(data14.MessageId) as SocketUserMessage;
					bool hasValue2 = socketUserMessage2 != null;
					Cacheable<IUserMessage, ulong> arg19 = new Cacheable<IUserMessage, ulong>(socketUserMessage2, data14.MessageId, hasValue2, async () => (await (await cacheableChannel3.GetOrDownloadAsync().ConfigureAwait(continueOnCapturedContext: false)).GetMessageAsync(data14.MessageId).ConfigureAwait(continueOnCapturedContext: false)) as IUserMessage);
					socketUserMessage2?.ClearReactions();
					await TimedInvokeAsync(_reactionsClearedEvent, "ReactionsCleared", arg19, cacheableChannel3).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_REACTION_REMOVE_EMOJI":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_REACTION_REMOVE_EMOJI)").ConfigureAwait(continueOnCapturedContext: false);
					RemoveAllReactionsForEmoteEvent data13 = (payload as JToken).ToObject<RemoveAllReactionsForEmoteEvent>(_serializer);
					ISocketMessageChannel socketMessageChannel5 = GetChannel(data13.ChannelId) as ISocketMessageChannel;
					SocketUserMessage socketUserMessage = socketMessageChannel5?.GetCachedMessage(data13.MessageId) as SocketUserMessage;
					bool flag = socketUserMessage != null;
					if (flag)
					{
						Optional.Create(socketUserMessage);
					}
					else
					{
						Optional.Create<SocketUserMessage>();
					}
					Cacheable<IMessageChannel, ulong> cacheableChannel2 = new Cacheable<IMessageChannel, ulong>(socketMessageChannel5, data13.ChannelId, socketMessageChannel5 != null, async () => (await GetChannelAsync(data13.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					Cacheable<IUserMessage, ulong> arg17 = new Cacheable<IUserMessage, ulong>(socketUserMessage, data13.MessageId, flag, async () => (await (await cacheableChannel2.GetOrDownloadAsync().ConfigureAwait(continueOnCapturedContext: false)).GetMessageAsync(data13.MessageId).ConfigureAwait(continueOnCapturedContext: false)) as IUserMessage);
					IEmote emote = data13.Emoji.ToIEmote();
					socketUserMessage?.RemoveReactionsForEmote(emote);
					await TimedInvokeAsync(_reactionsRemovedForEmoteEvent, "ReactionsRemovedForEmote", arg17, cacheableChannel2, emote).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_DELETE_BULK":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_DELETE_BULK)").ConfigureAwait(continueOnCapturedContext: false);
					MessageDeleteBulkEvent data4 = (payload as JToken).ToObject<MessageDeleteBulkEvent>(_serializer);
					ISocketMessageChannel socketMessageChannel = GetChannel(data4.ChannelId) as ISocketMessageChannel;
					SocketGuild socketGuild = (socketMessageChannel as SocketGuildChannel)?.Guild;
					if (!(socketGuild?.IsSynced ?? true))
					{
						await UnsyncedGuildAsync(type, socketGuild.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					Cacheable<IMessageChannel, ulong> arg3 = new Cacheable<IMessageChannel, ulong>(socketMessageChannel, data4.ChannelId, socketMessageChannel != null, async () => (await GetChannelAsync(data4.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					List<Cacheable<IMessage, ulong>> list = new List<Cacheable<IMessage, ulong>>(data4.Ids.Length);
					ulong[] ids = data4.Ids;
					foreach (ulong id in ids)
					{
						SocketMessage socketMessage = null;
						if (socketMessageChannel != null)
						{
							socketMessage = SocketChannelHelper.RemoveMessage(socketMessageChannel, this, id);
						}
						bool hasValue = socketMessage != null;
						Cacheable<IMessage, ulong> item = new Cacheable<IMessage, ulong>(socketMessage, id, hasValue, () => Task.FromResult<IMessage>(null));
						list.Add(item);
					}
					await TimedInvokeAsync(_messagesBulkDeletedEvent, "MessagesBulkDeleted", list, arg3).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "PRESENCE_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (PRESENCE_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Presence data16 = (payload as JToken).ToObject<Presence>(_serializer);
					SocketUser user6;
					if (data16.GuildId.IsSpecified)
					{
						SocketGuild guild18 = State.GetGuild(data16.GuildId.Value);
						if (guild18 == null)
						{
							await UnknownGuildAsync(type, data16.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (!guild18.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild18.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						user6 = guild18.GetUser(data16.User.Id);
						if (user6 == null)
						{
							if (data16.Status == UserStatus.Offline)
							{
								return;
							}
							user6 = guild18.AddOrUpdateUser(data16);
						}
						else
						{
							SocketGlobalUser arg22 = user6.GlobalUser.Clone();
							if (user6.GlobalUser.Update(State, data16.User))
							{
								await TimedInvokeAsync(_userUpdatedEvent, "UserUpdated", arg22, user6).ConfigureAwait(continueOnCapturedContext: false);
							}
						}
					}
					else
					{
						user6 = State.GetUser(data16.User.Id);
						if (user6 == null)
						{
							await UnknownGlobalUserAsync(type, data16.User.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
					}
					SocketPresence arg23 = user6.Presence?.Clone();
					user6.Update(State, data16.User);
					user6.Update(data16);
					await TimedInvokeAsync(_presenceUpdated, "PresenceUpdated", user6, arg23, user6.Presence).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "TYPING_START":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (TYPING_START)").ConfigureAwait(continueOnCapturedContext: false);
					TypingStartEvent data6 = (payload as JToken).ToObject<TypingStartEvent>(_serializer);
					ISocketMessageChannel socketMessageChannel2 = GetChannel(data6.ChannelId) as ISocketMessageChannel;
					SocketGuild socketGuild2 = (socketMessageChannel2 as SocketGuildChannel)?.Guild;
					if (!(socketGuild2?.IsSynced ?? true))
					{
						await UnsyncedGuildAsync(type, socketGuild2.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					Cacheable<IMessageChannel, ulong> arg11 = new Cacheable<IMessageChannel, ulong>(socketMessageChannel2, data6.ChannelId, socketMessageChannel2 != null, async () => (await GetChannelAsync(data6.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					SocketUser socketUser2 = (socketMessageChannel2 as SocketChannel)?.GetUser(data6.UserId);
					if (socketUser2 == null && socketGuild2 != null)
					{
						socketUser2 = socketGuild2.AddOrUpdateUser(data6.Member);
					}
					await TimedInvokeAsync<Cacheable<IUser, ulong>, Cacheable<IMessageChannel, ulong>>(arg1: new Cacheable<IUser, ulong>(socketUser2, data6.UserId, socketUser2 != null, async () => await GetUserAsync(data6.UserId).ConfigureAwait(continueOnCapturedContext: false)), eventHandler: _userIsTypingEvent, name: "UserIsTyping", arg2: arg11).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "INTEGRATION_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INTEGRATION_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Integration integration2 = (payload as JToken).ToObject<Integration>(_serializer);
					if (!integration2.GuildId.IsSpecified)
					{
						return;
					}
					SocketGuild guild34 = State.GetGuild(integration2.GuildId.Value);
					if (guild34 != null)
					{
						if (!guild34.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild34.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_integrationCreated, "IntegrationCreated", RestIntegration.Create(this, guild34, integration2)).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, integration2.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "INTEGRATION_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INTEGRATION_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Integration integration = (payload as JToken).ToObject<Integration>(_serializer);
					if (!integration.GuildId.IsSpecified)
					{
						return;
					}
					SocketGuild guild31 = State.GetGuild(integration.GuildId.Value);
					if (guild31 != null)
					{
						if (!guild31.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild31.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_integrationUpdated, "IntegrationUpdated", RestIntegration.Create(this, guild31, integration)).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, integration.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "INTEGRATION_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INTEGRATION_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					IntegrationDeletedEvent integrationDeletedEvent = (payload as JToken).ToObject<IntegrationDeletedEvent>(_serializer);
					SocketGuild guild21 = State.GetGuild(integrationDeletedEvent.GuildId);
					if (guild21 != null)
					{
						if (!guild21.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild21.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_integrationDeleted, "IntegrationDeleted", guild21, integrationDeletedEvent.Id, integrationDeletedEvent.ApplicationID).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, integrationDeletedEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "USER_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (USER_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					User user7 = (payload as JToken).ToObject<User>(_serializer);
					if (user7.Id == CurrentUser.Id)
					{
						SocketSelfUser arg29 = CurrentUser.Clone();
						CurrentUser.Update(State, user7);
						await TimedInvokeAsync(_selfUpdatedEvent, "CurrentUserUpdated", arg29, CurrentUser).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await _gatewayLogger.WarningAsync("Received USER_UPDATE for wrong user.").ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "VOICE_STATE_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (VOICE_STATE_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					VoiceState data12 = (payload as JToken).ToObject<VoiceState>(_serializer);
					SocketVoiceState before2;
					SocketVoiceState after;
					SocketUser user6;
					if (data12.GuildId.HasValue)
					{
						SocketGuild guild = State.GetGuild(data12.GuildId.Value);
						if (guild == null)
						{
							await UnknownGuildAsync(type, data12.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (!guild.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (data12.ChannelId.HasValue)
						{
							before2 = guild.GetVoiceState(data12.UserId)?.Clone() ?? SocketVoiceState.Default;
							after = await guild.AddOrUpdateVoiceStateAsync(State, data12).ConfigureAwait(continueOnCapturedContext: false);
						}
						else
						{
							before2 = (await guild.RemoveVoiceStateAsync(data12.UserId).ConfigureAwait(continueOnCapturedContext: false)) ?? SocketVoiceState.Default;
							after = SocketVoiceState.Create(null, data12);
						}
						user6 = guild.GetUser(data12.UserId) ?? (data12.Member.IsSpecified ? guild.AddOrUpdateUser(data12.Member.Value) : null);
						if (user6 == null)
						{
							await UnknownGuildUserAsync(type, data12.UserId, guild.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
					}
					else
					{
						if (!(GetChannel(data12.ChannelId.Value) is SocketGroupChannel socketGroupChannel4))
						{
							await UnknownChannelAsync(type, data12.ChannelId.Value).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (data12.ChannelId.HasValue)
						{
							before2 = socketGroupChannel4.GetVoiceState(data12.UserId)?.Clone() ?? SocketVoiceState.Default;
							after = socketGroupChannel4.AddOrUpdateVoiceState(State, data12);
						}
						else
						{
							before2 = socketGroupChannel4.RemoveVoiceState(data12.UserId) ?? SocketVoiceState.Default;
							after = SocketVoiceState.Create(null, data12);
						}
						user6 = socketGroupChannel4.GetUser(data12.UserId);
						if (user6 == null)
						{
							await UnknownChannelUserAsync(type, data12.UserId, socketGroupChannel4.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
					}
					if (user6 is SocketGuildUser socketGuildUser2 && data12.ChannelId.HasValue)
					{
						SocketStageChannel stageChannel = socketGuildUser2.Guild.GetStageChannel(data12.ChannelId.Value);
						if (stageChannel != null && before2.VoiceChannel != null && after.VoiceChannel != null)
						{
							if (!before2.RequestToSpeakTimestamp.HasValue && after.RequestToSpeakTimestamp.HasValue)
							{
								await TimedInvokeAsync(_requestToSpeak, "RequestToSpeak", stageChannel, socketGuildUser2);
								return;
							}
							if (before2.IsSuppressed && !after.IsSuppressed)
							{
								await TimedInvokeAsync(_speakerAdded, "SpeakerAdded", stageChannel, socketGuildUser2);
								return;
							}
							if (!before2.IsSuppressed && after.IsSuppressed)
							{
								await TimedInvokeAsync(_speakerRemoved, "SpeakerRemoved", stageChannel, socketGuildUser2);
							}
						}
					}
					await TimedInvokeAsync(_userVoiceStateUpdatedEvent, "UserVoiceStateUpdated", user6, before2, after).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "VOICE_SERVER_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (VOICE_SERVER_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					VoiceServerUpdateEvent data11 = (payload as JToken).ToObject<VoiceServerUpdateEvent>(_serializer);
					SocketGuild guild = State.GetGuild(data11.GuildId);
					bool isMsgCached = guild != null;
					SocketVoiceServer arg16 = new SocketVoiceServer(new Cacheable<IGuild, ulong>(guild, data11.GuildId, isMsgCached, () => Task.FromResult((IGuild)State.GetGuild(data11.GuildId))), data11.Endpoint, data11.Token);
					await TimedInvokeAsync(_voiceServerUpdatedEvent, "UserVoiceStateUpdated", arg16).ConfigureAwait(continueOnCapturedContext: false);
					if (isMsgCached)
					{
						string text2 = data11.Endpoint;
						int num2 = text2.LastIndexOf(':');
						if (num2 > 0)
						{
							text2 = text2.Substring(0, num2);
						}
						guild.FinishConnectAudio(text2, data11.Token).ConfigureAwait(continueOnCapturedContext: false);
					}
					else
					{
						await UnknownGuildAsync(type, data11.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "INVITE_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INVITE_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					InviteCreateEvent inviteCreateEvent = (payload as JToken).ToObject<InviteCreateEvent>(_serializer);
					if (State.GetChannel(inviteCreateEvent.ChannelId) is SocketGuildChannel { Guild: var guild33 } socketGuildChannel2)
					{
						if (!guild33.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild33.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						SocketGuildUser inviter = (inviteCreateEvent.Inviter.IsSpecified ? (guild33.GetUser(inviteCreateEvent.Inviter.Value.Id) ?? guild33.AddOrUpdateUser(inviteCreateEvent.Inviter.Value)) : null);
						SocketUser target = (SocketUser)(inviteCreateEvent.TargetUser.IsSpecified ? (((object)guild33.GetUser(inviteCreateEvent.TargetUser.Value.Id)) ?? ((object)SocketUnknownUser.Create(this, State, inviteCreateEvent.TargetUser.Value))) : null);
						SocketInvite arg32 = SocketInvite.Create(this, guild33, socketGuildChannel2, inviter, target, inviteCreateEvent);
						await TimedInvokeAsync(_inviteCreatedEvent, "InviteCreated", arg32).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownChannelAsync(type, inviteCreateEvent.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "INVITE_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INVITE_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					InviteDeleteEvent inviteDeleteEvent = (payload as JToken).ToObject<InviteDeleteEvent>(_serializer);
					if (State.GetChannel(inviteDeleteEvent.ChannelId) is SocketGuildChannel { Guild: var guild30 } socketGuildChannel)
					{
						if (!guild30.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild30.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_inviteDeletedEvent, "InviteDeleted", socketGuildChannel, inviteDeleteEvent.Code).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownChannelAsync(type, inviteDeleteEvent.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "INTERACTION_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INTERACTION_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Interaction data5 = (payload as JToken).ToObject<Interaction>(_serializer);
					SocketGuild guild = (data5.GuildId.IsSpecified ? GetGuild(data5.GuildId.Value) : null);
					if (guild != null && !guild.IsSynced)
					{
						await UnsyncedGuildAsync(type, guild.Id).ConfigureAwait(continueOnCapturedContext: false);
					}
					SocketUser user = (data5.User.IsSpecified ? State.GetOrAddUser(data5.User.Value.Id, (ulong _) => SocketGlobalUser.Create(this, State, data5.User.Value)) : ((guild != null) ? ((SocketUser)guild.AddOrUpdateUser(data5.Member.Value)) : ((SocketUser)State.GetOrAddUser(data5.Member.Value.User.Id, (ulong _) => SocketGlobalUser.Create(this, State, data5.Member.Value.User)))));
					SocketChannel socketChannel = null;
					if (data5.ChannelId.IsSpecified)
					{
						socketChannel = State.GetChannel(data5.ChannelId.Value);
						if (socketChannel == null && !data5.GuildId.IsSpecified)
						{
							socketChannel = CreateDMChannel(data5.ChannelId.Value, user, State);
						}
					}
					else if (data5.User.IsSpecified)
					{
						socketChannel = State.GetDMChannel(data5.User.Value.Id);
					}
					SocketInteraction interaction = SocketInteraction.Create(this, data5, socketChannel as ISocketMessageChannel, user);
					await TimedInvokeAsync(_interactionCreatedEvent, "InteractionCreated", interaction).ConfigureAwait(continueOnCapturedContext: false);
					if (!(interaction is SocketSlashCommand arg4))
					{
						if (!(interaction is SocketMessageComponent messageComponent))
						{
							if (!(interaction is SocketUserCommand arg5))
							{
								if (!(interaction is SocketMessageCommand arg6))
								{
									if (!(interaction is SocketAutocompleteInteraction arg7))
									{
										if (interaction is SocketModal arg8)
										{
											await TimedInvokeAsync(_modalSubmitted, "ModalSubmitted", arg8).ConfigureAwait(continueOnCapturedContext: false);
										}
									}
									else
									{
										await TimedInvokeAsync(_autocompleteExecuted, "AutocompleteExecuted", arg7).ConfigureAwait(continueOnCapturedContext: false);
									}
								}
								else
								{
									await TimedInvokeAsync(_messageCommandExecuted, "MessageCommandExecuted", arg6).ConfigureAwait(continueOnCapturedContext: false);
								}
							}
							else
							{
								await TimedInvokeAsync(_userCommandExecuted, "UserCommandExecuted", arg5).ConfigureAwait(continueOnCapturedContext: false);
							}
						}
						else
						{
							if (messageComponent.Data.Type == ComponentType.SelectMenu)
							{
								await TimedInvokeAsync(_selectMenuExecuted, "SelectMenuExecuted", messageComponent).ConfigureAwait(continueOnCapturedContext: false);
							}
							if (messageComponent.Data.Type == ComponentType.Button)
							{
								await TimedInvokeAsync(_buttonExecuted, "ButtonExecuted", messageComponent).ConfigureAwait(continueOnCapturedContext: false);
							}
						}
					}
					else
					{
						await TimedInvokeAsync(_slashCommandExecuted, "SlashCommandExecuted", arg4).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "APPLICATION_COMMAND_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (APPLICATION_COMMAND_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					ApplicationCommandCreatedUpdatedEvent applicationCommandCreatedUpdatedEvent = (payload as JToken).ToObject<ApplicationCommandCreatedUpdatedEvent>(_serializer);
					if (applicationCommandCreatedUpdatedEvent.GuildId.IsSpecified && State.GetGuild(applicationCommandCreatedUpdatedEvent.GuildId.Value) == null)
					{
						await UnknownGuildAsync(type, applicationCommandCreatedUpdatedEvent.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketApplicationCommand socketApplicationCommand = SocketApplicationCommand.Create(this, applicationCommandCreatedUpdatedEvent);
					State.AddCommand(socketApplicationCommand);
					await TimedInvokeAsync(_applicationCommandCreated, "ApplicationCommandCreated", socketApplicationCommand).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "APPLICATION_COMMAND_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (APPLICATION_COMMAND_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					ApplicationCommandCreatedUpdatedEvent applicationCommandCreatedUpdatedEvent2 = (payload as JToken).ToObject<ApplicationCommandCreatedUpdatedEvent>(_serializer);
					if (applicationCommandCreatedUpdatedEvent2.GuildId.IsSpecified && State.GetGuild(applicationCommandCreatedUpdatedEvent2.GuildId.Value) == null)
					{
						await UnknownGuildAsync(type, applicationCommandCreatedUpdatedEvent2.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketApplicationCommand socketApplicationCommand2 = SocketApplicationCommand.Create(this, applicationCommandCreatedUpdatedEvent2);
					State.AddCommand(socketApplicationCommand2);
					await TimedInvokeAsync(_applicationCommandUpdated, "ApplicationCommandUpdated", socketApplicationCommand2).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "APPLICATION_COMMAND_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (APPLICATION_COMMAND_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					ApplicationCommandCreatedUpdatedEvent applicationCommandCreatedUpdatedEvent3 = (payload as JToken).ToObject<ApplicationCommandCreatedUpdatedEvent>(_serializer);
					if (applicationCommandCreatedUpdatedEvent3.GuildId.IsSpecified && State.GetGuild(applicationCommandCreatedUpdatedEvent3.GuildId.Value) == null)
					{
						await UnknownGuildAsync(type, applicationCommandCreatedUpdatedEvent3.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketApplicationCommand socketApplicationCommand3 = SocketApplicationCommand.Create(this, applicationCommandCreatedUpdatedEvent3);
					State.RemoveCommand(socketApplicationCommand3.Id);
					await TimedInvokeAsync(_applicationCommandDeleted, "ApplicationCommandDeleted", socketApplicationCommand3).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "THREAD_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel data18 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketGuild guild24 = State.GetGuild(data18.GuildId.Value);
					if (guild24 == null)
					{
						await UnknownGuildAsync(type, data18.GuildId.Value);
						return;
					}
					SocketThreadChannel socketThreadChannel4;
					if ((socketThreadChannel4 = guild24.ThreadChannels.FirstOrDefault((SocketThreadChannel x) => x.Id == data18.Id)) != null)
					{
						socketThreadChannel4.Update(State, data18);
						if (data18.ThreadMember.IsSpecified)
						{
							socketThreadChannel4.AddOrUpdateThreadMember(data18.ThreadMember.Value, guild24.CurrentUser);
						}
					}
					else
					{
						socketThreadChannel4 = (SocketThreadChannel)guild24.AddChannel(State, data18);
						if (data18.ThreadMember.IsSpecified)
						{
							socketThreadChannel4.AddOrUpdateThreadMember(data18.ThreadMember.Value, guild24.CurrentUser);
						}
					}
					await TimedInvokeAsync(_threadCreated, "ThreadCreated", socketThreadChannel4).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "THREAD_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel data9 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketGuild guild14 = State.GetGuild(data9.GuildId.Value);
					if (guild14 == null)
					{
						await UnknownGuildAsync(type, data9.GuildId.Value);
						return;
					}
					SocketThreadChannel socketThreadChannel3 = guild14.ThreadChannels.FirstOrDefault((SocketThreadChannel x) => x.Id == data9.Id);
					Cacheable<SocketThreadChannel, ulong> arg14 = ((socketThreadChannel3 != null) ? new Cacheable<SocketThreadChannel, ulong>(socketThreadChannel3.Clone(), data9.Id, hasValue: true, () => Task.FromResult<SocketThreadChannel>(null)) : new Cacheable<SocketThreadChannel, ulong>(null, data9.Id, hasValue: false, () => Task.FromResult<SocketThreadChannel>(null)));
					if (socketThreadChannel3 != null)
					{
						socketThreadChannel3.Update(State, data9);
						if (data9.ThreadMember.IsSpecified)
						{
							socketThreadChannel3.AddOrUpdateThreadMember(data9.ThreadMember.Value, guild14.CurrentUser);
						}
					}
					else
					{
						socketThreadChannel3 = (SocketThreadChannel)guild14.AddChannel(State, data9);
						if (data9.ThreadMember.IsSpecified)
						{
							socketThreadChannel3.AddOrUpdateThreadMember(data9.ThreadMember.Value, guild14.CurrentUser);
						}
					}
					if (!(guild14?.IsSynced ?? true))
					{
						await UnsyncedGuildAsync(type, guild14.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					await TimedInvokeAsync(_threadUpdated, "ThreadUpdated", arg14, socketThreadChannel3).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "THREAD_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel channel2 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketGuild guild9 = State.GetGuild(channel2.GuildId.Value);
					if (guild9 == null)
					{
						await UnknownGuildAsync(type, channel2.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketThreadChannel socketThreadChannel2 = (SocketThreadChannel)guild9.GetChannel(channel2.Id);
					await TimedInvokeAsync(arg: new Cacheable<SocketThreadChannel, ulong>(socketThreadChannel2, channel2.Id, socketThreadChannel2 != null, null), eventHandler: _threadDeleted, name: "ThreadDeleted").ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "THREAD_LIST_SYNC":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_LIST_SYNC)").ConfigureAwait(continueOnCapturedContext: false);
					ThreadListSyncEvent threadListSyncEvent = (payload as JToken).ToObject<ThreadListSyncEvent>(_serializer);
					SocketGuild guild6 = State.GetGuild(threadListSyncEvent.GuildId);
					if (guild6 == null)
					{
						await UnknownGuildAsync(type, threadListSyncEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					Channel[] threads = threadListSyncEvent.Threads;
					foreach (Channel thread in threads)
					{
						SocketThreadChannel entity = guild6.ThreadChannels.FirstOrDefault((SocketThreadChannel x) => x.Id == thread.Id);
						if (entity == null)
						{
							entity = (SocketThreadChannel)guild6.AddChannel(State, thread);
						}
						else
						{
							entity.Update(State, thread);
						}
						foreach (ThreadMember item4 in threadListSyncEvent.Members.Where((ThreadMember x) => x.Id.Value == entity.Id))
						{
							SocketGuildUser user2 = guild6.GetUser(item4.Id.Value);
							entity.AddOrUpdateThreadMember(item4, user2);
						}
					}
					break;
				}
				case "THREAD_MEMBER_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_MEMBER_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					ThreadMember threadMember = (payload as JToken).ToObject<ThreadMember>(_serializer);
					SocketThreadChannel socketThreadChannel = (SocketThreadChannel)State.GetChannel(threadMember.Id.Value);
					if (socketThreadChannel == null)
					{
						await UnknownChannelAsync(type, threadMember.Id.Value);
						return;
					}
					socketThreadChannel.AddOrUpdateThreadMember(threadMember, socketThreadChannel.Guild.CurrentUser);
					break;
				}
				case "THREAD_MEMBERS_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_MEMBERS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					ThreadMembersUpdated threadMembersUpdated = (payload as JToken).ToObject<ThreadMembersUpdated>(_serializer);
					SocketGuild guild = State.GetGuild(threadMembersUpdated.GuildId);
					if (guild == null)
					{
						await UnknownGuildAsync(type, threadMembersUpdated.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketThreadChannel thread2 = (SocketThreadChannel)guild.GetChannel(threadMembersUpdated.Id);
					if (thread2 == null)
					{
						await UnknownChannelAsync(type, threadMembersUpdated.Id);
						return;
					}
					IReadOnlyCollection<SocketThreadUser> leftUsers = null;
					IReadOnlyCollection<SocketThreadUser> joinUsers = null;
					if (threadMembersUpdated.RemovedMemberIds.IsSpecified)
					{
						leftUsers = thread2.RemoveUsers(threadMembersUpdated.RemovedMemberIds.Value);
					}
					if (threadMembersUpdated.AddedMembers.IsSpecified)
					{
						List<SocketThreadUser> newThreadMembers = new List<SocketThreadUser>();
						ThreadMember[] value2 = threadMembersUpdated.AddedMembers.Value;
						foreach (ThreadMember threadMember2 in value2)
						{
							SocketGuildUser user8 = guild.GetUser(threadMember2.UserId.Value);
							if (user8 == null)
							{
								await UnknownGuildUserAsync("THREAD_MEMBERS_UPDATE", threadMember2.UserId.Value, guild.Id);
							}
							else
							{
								newThreadMembers.Add(thread2.AddOrUpdateThreadMember(threadMember2, user8));
							}
						}
						if (newThreadMembers.Any())
						{
							joinUsers = newThreadMembers.ToImmutableArray();
						}
					}
					if (leftUsers != null)
					{
						foreach (SocketThreadUser item5 in leftUsers)
						{
							await TimedInvokeAsync(_threadMemberLeft, "ThreadMemberLeft", item5).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					if (joinUsers == null)
					{
						break;
					}
					foreach (SocketThreadUser item6 in joinUsers)
					{
						await TimedInvokeAsync(_threadMemberJoined, "ThreadMemberJoined", item6).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "STAGE_INSTANCE_CREATE":
				case "STAGE_INSTANCE_UPDATE":
				case "STAGE_INSTANCE_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					StageInstance stageInstance = (payload as JToken).ToObject<StageInstance>(_serializer);
					SocketGuild guild22 = State.GetGuild(stageInstance.GuildId);
					if (guild22 == null)
					{
						await UnknownGuildAsync(type, stageInstance.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketStageChannel stageChannel2 = guild22.GetStageChannel(stageInstance.ChannelId);
					if (stageChannel2 == null)
					{
						await UnknownChannelAsync(type, stageInstance.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketStageChannel arg28 = ((type == "STAGE_INSTANCE_UPDATE") ? stageChannel2.Clone() : null);
					stageChannel2.Update(stageInstance, type == "STAGE_INSTANCE_CREATE");
					switch (type)
					{
					case "STAGE_INSTANCE_CREATE":
						await TimedInvokeAsync(_stageStarted, "StageStarted", stageChannel2).ConfigureAwait(continueOnCapturedContext: false);
						return;
					case "STAGE_INSTANCE_DELETE":
						await TimedInvokeAsync(_stageEnded, "StageEnded", stageChannel2).ConfigureAwait(continueOnCapturedContext: false);
						return;
					case "STAGE_INSTANCE_UPDATE":
						await TimedInvokeAsync(_stageUpdated, "StageUpdated", arg28, stageChannel2).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					break;
				}
				case "GUILD_SCHEDULED_EVENT_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					GuildScheduledEvent guildScheduledEvent3 = (payload as JToken).ToObject<GuildScheduledEvent>(_serializer);
					SocketGuild guild20 = State.GetGuild(guildScheduledEvent3.GuildId);
					if (guild20 == null)
					{
						await UnknownGuildAsync(type, guildScheduledEvent3.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketGuildEvent arg27 = guild20.AddOrUpdateEvent(guildScheduledEvent3);
					await TimedInvokeAsync(_guildScheduledEventCreated, "GuildScheduledEventCreated", arg27).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "GUILD_SCHEDULED_EVENT_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					GuildScheduledEvent guildScheduledEvent2 = (payload as JToken).ToObject<GuildScheduledEvent>(_serializer);
					SocketGuild guild10 = State.GetGuild(guildScheduledEvent2.GuildId);
					if (guild10 == null)
					{
						await UnknownGuildAsync(type, guildScheduledEvent2.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketGuildEvent socketGuildEvent2 = guild10.GetEvent(guildScheduledEvent2.Id)?.Clone();
					Cacheable<SocketGuildEvent, ulong> arg12 = new Cacheable<SocketGuildEvent, ulong>(socketGuildEvent2, guildScheduledEvent2.Id, socketGuildEvent2 != null, () => Task.FromResult<SocketGuildEvent>(null));
					SocketGuildEvent socketGuildEvent3 = guild10.AddOrUpdateEvent(guildScheduledEvent2);
					if ((socketGuildEvent2 == null || socketGuildEvent2.Status != GuildScheduledEventStatus.Completed) && guildScheduledEvent2.Status == GuildScheduledEventStatus.Completed)
					{
						await TimedInvokeAsync(_guildScheduledEventCompleted, "GuildScheduledEventCompleted", socketGuildEvent3).ConfigureAwait(continueOnCapturedContext: false);
					}
					else if (socketGuildEvent2 != null && socketGuildEvent2.Status != GuildScheduledEventStatus.Active && guildScheduledEvent2.Status == GuildScheduledEventStatus.Active)
					{
						await TimedInvokeAsync(_guildScheduledEventStarted, "GuildScheduledEventStarted", socketGuildEvent3).ConfigureAwait(continueOnCapturedContext: false);
					}
					else
					{
						await TimedInvokeAsync(_guildScheduledEventUpdated, "GuildScheduledEventUpdated", arg12, socketGuildEvent3).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "GUILD_SCHEDULED_EVENT_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					GuildScheduledEvent guildScheduledEvent = (payload as JToken).ToObject<GuildScheduledEvent>(_serializer);
					SocketGuild guild7 = State.GetGuild(guildScheduledEvent.GuildId);
					if (guild7 == null)
					{
						await UnknownGuildAsync(type, guildScheduledEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketGuildEvent arg9 = guild7.RemoveEvent(guildScheduledEvent.Id) ?? SocketGuildEvent.Create(this, guild7, guildScheduledEvent);
					await TimedInvokeAsync(_guildScheduledEventCancelled, "GuildScheduledEventCancelled", arg9).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "GUILD_SCHEDULED_EVENT_USER_ADD":
				case "GUILD_SCHEDULED_EVENT_USER_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					GuildScheduledEventUserAddRemoveEvent data3 = (payload as JToken).ToObject<GuildScheduledEventUserAddRemoveEvent>(_serializer);
					SocketGuild guild3 = State.GetGuild(data3.GuildId);
					if (guild3 == null)
					{
						await UnknownGuildAsync(type, data3.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketGuildEvent socketGuildEvent = guild3.GetEvent(data3.EventId);
					if (socketGuildEvent == null)
					{
						await UnknownGuildEventAsync(type, data3.EventId, data3.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketUser socketUser = (SocketUser)(((object)guild3.GetUser(data3.UserId)) ?? ((object)State.GetUser(data3.UserId)));
					Cacheable<SocketUser, RestUser, IUser, ulong> arg = new Cacheable<SocketUser, RestUser, IUser, ulong>(socketUser, data3.UserId, socketUser != null, () => Rest.GetUserAsync(data3.UserId));
					string text = type;
					if (!(text == "GUILD_SCHEDULED_EVENT_USER_ADD"))
					{
						if (text == "GUILD_SCHEDULED_EVENT_USER_REMOVE")
						{
							await TimedInvokeAsync(_guildScheduledEventUserRemove, "GuildScheduledEventUserRemove", arg, socketGuildEvent).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					else
					{
						await TimedInvokeAsync(_guildScheduledEventUserAdd, "GuildScheduledEventUserAdd", arg, socketGuildEvent).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "WEBHOOKS_UPDATE":
				{
					WebhooksUpdatedEvent data2 = (payload as JToken).ToObject<WebhooksUpdatedEvent>(_serializer);
					type = "WEBHOOKS_UPDATE";
					await _gatewayLogger.DebugAsync("Received Dispatch (WEBHOOKS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					SocketGuild guild2 = State.GetGuild(data2.GuildId);
					SocketChannel channel = State.GetChannel(data2.ChannelId);
					await TimedInvokeAsync(_webhooksUpdated, "WebhooksUpdated", guild2, channel);
					break;
				}
				case "CHANNEL_PINS_ACK":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (CHANNEL_PINS_ACK)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "CHANNEL_PINS_UPDATE":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (CHANNEL_PINS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "GUILD_INTEGRATIONS_UPDATE":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (GUILD_INTEGRATIONS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "MESSAGE_ACK":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (MESSAGE_ACK)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "PRESENCES_REPLACE":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (PRESENCES_REPLACE)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "USER_SETTINGS_UPDATE":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (USER_SETTINGS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				default:
					if (!SuppressUnknownDispatchWarnings)
					{
						await _gatewayLogger.WarningAsync("Unknown Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				break;
			default:
				await _gatewayLogger.WarningAsync($"Unknown OpCode ({opCode})").ConfigureAwait(continueOnCapturedContext: false);
				break;
			}
		}
		catch (Exception exception)
		{
			await _gatewayLogger.ErrorAsync(string.Format("Error handling {0}{1}", opCode, (type != null) ? (" (" + type + ")") : ""), exception).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task RunHeartbeatAsync(int intervalMillis, CancellationToken cancelToken)
	{
		try
		{
			await _gatewayLogger.DebugAsync("Heartbeat Started").ConfigureAwait(continueOnCapturedContext: false);
			while (!cancelToken.IsCancellationRequested)
			{
				int tickCount = Environment.TickCount;
				if (_heartbeatTimes.Count != 0 && tickCount - _lastMessageTime > intervalMillis && ConnectionState == ConnectionState.Connected && (_guildDownloadTask?.IsCompleted ?? true))
				{
					_connection.Error(new GatewayReconnectException("Server missed last heartbeat"));
					return;
				}
				_heartbeatTimes.Enqueue(tickCount);
				try
				{
					await ApiClient.SendHeartbeatAsync(_lastSeq).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (Exception exception)
				{
					await _gatewayLogger.WarningAsync("Heartbeat Errored", exception).ConfigureAwait(continueOnCapturedContext: false);
				}
				await Task.Delay(intervalMillis, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			await _gatewayLogger.DebugAsync("Heartbeat Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			await _gatewayLogger.DebugAsync("Heartbeat Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception2)
		{
			await _gatewayLogger.ErrorAsync("Heartbeat Errored", exception2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task WaitForGuildsAsync(CancellationToken cancelToken, Logger logger)
	{
		try
		{
			await logger.DebugAsync("GuildDownloader Started").ConfigureAwait(continueOnCapturedContext: false);
			while (_unavailableGuildCount != 0 && Environment.TickCount - _lastGuildAvailableTime < BaseConfig.MaxWaitBetweenGuildAvailablesBeforeReady)
			{
				await Task.Delay(500, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			await logger.DebugAsync("GuildDownloader Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			await logger.DebugAsync("GuildDownloader Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception)
		{
			await logger.ErrorAsync("GuildDownloader Errored", exception).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task SyncGuildsAsync()
	{
		System.Collections.Immutable.ImmutableArray<ulong> immutableArray = (from x in Guilds
			where !x.IsSynced
			select x.Id).ToImmutableArray();
		if (immutableArray.Length > 0)
		{
			await ApiClient.SendGuildSyncAsync(immutableArray).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal SocketGuild AddGuild(ExtendedGuild model, ClientState state)
	{
		SocketGuild socketGuild = SocketGuild.Create(this, state, model);
		state.AddGuild(socketGuild);
		if (model.Large)
		{
			_largeGuilds.Enqueue(model.Id);
		}
		return socketGuild;
	}

	internal SocketGuild RemoveGuild(ulong id)
	{
		return State.RemoveGuild(id);
	}

	internal ISocketPrivateChannel AddPrivateChannel(Channel model, ClientState state)
	{
		ISocketPrivateChannel socketPrivateChannel = SocketChannel.CreatePrivate(this, state, model);
		state.AddChannel(socketPrivateChannel as SocketChannel);
		return socketPrivateChannel;
	}

	internal SocketDMChannel CreateDMChannel(ulong channelId, User model, ClientState state)
	{
		return SocketDMChannel.Create(this, state, channelId, model);
	}

	internal SocketDMChannel CreateDMChannel(ulong channelId, SocketUser user, ClientState state)
	{
		return new SocketDMChannel(this, channelId, user);
	}

	internal ISocketPrivateChannel RemovePrivateChannel(ulong id)
	{
		ISocketPrivateChannel socketPrivateChannel = State.RemoveChannel(id) as ISocketPrivateChannel;
		if (socketPrivateChannel != null)
		{
			foreach (SocketUser recipient in socketPrivateChannel.Recipients)
			{
				recipient.GlobalUser.RemoveRef(this);
			}
		}
		return socketPrivateChannel;
	}

	internal void RemoveDMChannels()
	{
		IReadOnlyCollection<SocketDMChannel> dMChannels = State.DMChannels;
		State.PurgeDMChannels();
		foreach (SocketDMChannel item in dMChannels)
		{
			item.Recipient.GlobalUser.RemoveRef(this);
		}
	}

	internal void EnsureGatewayIntent(GatewayIntents intents)
	{
		if (!_gatewayIntents.HasFlag(intents))
		{
			IEnumerable<GatewayIntents> source = from GatewayIntents x in Enum.GetValues(typeof(GatewayIntents))
				where intents.HasFlag(x) && !_gatewayIntents.HasFlag(x)
				select x;
			throw new InvalidOperationException("Missing required gateway intent" + ((source.Count() > 1) ? "s" : "") + " " + string.Join(", ", source.Select((GatewayIntents x) => x.ToString())) + " in order to execute this operation.");
		}
	}

	internal bool HasGatewayIntent(GatewayIntents intents)
	{
		return _gatewayIntents.HasFlag(intents);
	}

	private async Task GuildAvailableAsync(SocketGuild guild)
	{
		if (!guild.IsConnected)
		{
			guild.IsConnected = true;
			await TimedInvokeAsync(_guildAvailableEvent, "GuildAvailable", guild).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task GuildUnavailableAsync(SocketGuild guild)
	{
		if (guild.IsConnected)
		{
			guild.IsConnected = false;
			await TimedInvokeAsync(_guildUnavailableEvent, "GuildUnavailable", guild).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task TimedInvokeAsync(AsyncEvent<Func<Task>> eventHandler, string name)
	{
		if (eventHandler.HasSubscribers)
		{
			if (HandlerTimeout.HasValue)
			{
				await TimeoutWrap(name, eventHandler.InvokeAsync).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await eventHandler.InvokeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	private async Task TimedInvokeAsync<T>(AsyncEvent<Func<T, Task>> eventHandler, string name, T arg)
	{
		if (!eventHandler.HasSubscribers)
		{
			return;
		}
		if (HandlerTimeout.HasValue)
		{
			await TimeoutWrap(name, () => eventHandler.InvokeAsync(arg)).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await eventHandler.InvokeAsync(arg).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task TimedInvokeAsync<T1, T2>(AsyncEvent<Func<T1, T2, Task>> eventHandler, string name, T1 arg1, T2 arg2)
	{
		if (!eventHandler.HasSubscribers)
		{
			return;
		}
		if (HandlerTimeout.HasValue)
		{
			await TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2)).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await eventHandler.InvokeAsync(arg1, arg2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task TimedInvokeAsync<T1, T2, T3>(AsyncEvent<Func<T1, T2, T3, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3)
	{
		if (!eventHandler.HasSubscribers)
		{
			return;
		}
		if (HandlerTimeout.HasValue)
		{
			await TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3)).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await eventHandler.InvokeAsync(arg1, arg2, arg3).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task TimedInvokeAsync<T1, T2, T3, T4>(AsyncEvent<Func<T1, T2, T3, T4, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (!eventHandler.HasSubscribers)
		{
			return;
		}
		if (HandlerTimeout.HasValue)
		{
			await TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3, arg4)).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await eventHandler.InvokeAsync(arg1, arg2, arg3, arg4).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task TimedInvokeAsync<T1, T2, T3, T4, T5>(AsyncEvent<System.Func<T1, T2, T3, T4, T5, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (!eventHandler.HasSubscribers)
		{
			return;
		}
		if (HandlerTimeout.HasValue)
		{
			await TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3, arg4, arg5)).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await eventHandler.InvokeAsync(arg1, arg2, arg3, arg4, arg5).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task TimeoutWrap(string name, Func<Task> action)
	{
		try
		{
			Task timeoutTask = Task.Delay(HandlerTimeout.Value);
			Task handlersTask = action();
			if (await Task.WhenAny(timeoutTask, handlersTask).ConfigureAwait(continueOnCapturedContext: false) == timeoutTask)
			{
				await _gatewayLogger.WarningAsync("A " + name + " handler is blocking the gateway task.").ConfigureAwait(continueOnCapturedContext: false);
			}
			await handlersTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception)
		{
			await _gatewayLogger.WarningAsync("A " + name + " handler has thrown an unhandled exception.", exception).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task UnknownGlobalUserAsync(string evnt, ulong userId)
	{
		string text = $"{evnt} User={userId}";
		await _gatewayLogger.WarningAsync("Unknown User (" + text + ").").ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task UnknownChannelUserAsync(string evnt, ulong userId, ulong channelId)
	{
		string text = $"{evnt} User={userId} Channel={channelId}";
		await _gatewayLogger.WarningAsync("Unknown User (" + text + ").").ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task UnknownGuildUserAsync(string evnt, ulong userId, ulong guildId)
	{
		string text = $"{evnt} User={userId} Guild={guildId}";
		await _gatewayLogger.WarningAsync("Unknown User (" + text + ").").ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task IncompleteGuildUserAsync(string evnt, ulong userId, ulong guildId)
	{
		string text = $"{evnt} User={userId} Guild={guildId}";
		await _gatewayLogger.DebugAsync("User has not been downloaded (" + text + ").").ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task UnknownChannelAsync(string evnt, ulong channelId)
	{
		string text = $"{evnt} Channel={channelId}";
		await _gatewayLogger.WarningAsync("Unknown Channel (" + text + ").").ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task UnknownChannelAsync(string evnt, ulong channelId, ulong guildId)
	{
		if (guildId == 0L)
		{
			await UnknownChannelAsync(evnt, channelId).ConfigureAwait(continueOnCapturedContext: false);
			return;
		}
		string text = $"{evnt} Channel={channelId} Guild={guildId}";
		await _gatewayLogger.WarningAsync("Unknown Channel (" + text + ").").ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task UnknownRoleAsync(string evnt, ulong roleId, ulong guildId)
	{
		string text = $"{evnt} Role={roleId} Guild={guildId}";
		await _gatewayLogger.WarningAsync("Unknown Role (" + text + ").").ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task UnknownGuildAsync(string evnt, ulong guildId)
	{
		string text = $"{evnt} Guild={guildId}";
		await _gatewayLogger.WarningAsync("Unknown Guild (" + text + ").").ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task UnknownGuildEventAsync(string evnt, ulong eventId, ulong guildId)
	{
		string text = $"{evnt} Event={eventId} Guild={guildId}";
		await _gatewayLogger.WarningAsync("Unknown Guild Event (" + text + ").").ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task UnsyncedGuildAsync(string evnt, ulong guildId)
	{
		string text = $"{evnt} Guild={guildId}";
		await _gatewayLogger.DebugAsync("Unsynced Guild (" + text + ").").ConfigureAwait(continueOnCapturedContext: false);
	}

	internal int GetAudioId()
	{
		return _nextAudioId++;
	}

	async Task<IApplication> IDiscordClient.GetApplicationInfoAsync(RequestOptions options)
	{
		return await GetApplicationInfoAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IChannel> IDiscordClient.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return (mode != CacheMode.AllowDownload) ? GetChannel(id) : (await GetChannelAsync(id, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	Task<IReadOnlyCollection<IPrivateChannel>> IDiscordClient.GetPrivateChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IPrivateChannel>)PrivateChannels);
	}

	Task<IReadOnlyCollection<IDMChannel>> IDiscordClient.GetDMChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IDMChannel>)DMChannels);
	}

	Task<IReadOnlyCollection<IGroupChannel>> IDiscordClient.GetGroupChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IGroupChannel>)GroupChannels);
	}

	async Task<IReadOnlyCollection<IConnection>> IDiscordClient.GetConnectionsAsync(RequestOptions options)
	{
		return await GetConnectionsAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IInvite> IDiscordClient.GetInviteAsync(string inviteId, RequestOptions options)
	{
		return await GetInviteAsync(inviteId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<IGuild> IDiscordClient.GetGuildAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IGuild)GetGuild(id));
	}

	Task<IReadOnlyCollection<IGuild>> IDiscordClient.GetGuildsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IGuild>)Guilds);
	}

	async Task<IGuild> IDiscordClient.CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
	{
		return await CreateGuildAsync(name, region, jpegIcon).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUser> IDiscordClient.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		SocketUser user = GetUser(id);
		if (user != null || mode == CacheMode.CacheOnly)
		{
			return user;
		}
		return await Rest.GetUserAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<IUser> IDiscordClient.GetUserAsync(string username, string discriminator, RequestOptions options)
	{
		return Task.FromResult((IUser)GetUser(username, discriminator));
	}

	async Task<IReadOnlyCollection<IVoiceRegion>> IDiscordClient.GetVoiceRegionsAsync(RequestOptions options)
	{
		return await GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IVoiceRegion> IDiscordClient.GetVoiceRegionAsync(string id, RequestOptions options)
	{
		return await GetVoiceRegionAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IApplicationCommand> IDiscordClient.GetGlobalApplicationCommandAsync(ulong id, RequestOptions options)
	{
		return await GetGlobalApplicationCommandAsync(id, options);
	}

	async Task<IReadOnlyCollection<IApplicationCommand>> IDiscordClient.GetGlobalApplicationCommandsAsync(bool withLocalizations, string locale, RequestOptions options)
	{
		return await GetGlobalApplicationCommandsAsync(withLocalizations, locale, options);
	}

	async Task IDiscordClient.StartAsync()
	{
		await StartAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task IDiscordClient.StopAsync()
	{
		await StopAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	internal DiscordSocketClient(DiscordSocketConfig config, DiscordRestApiClient client)
		: base(config, client)
	{
	}
}

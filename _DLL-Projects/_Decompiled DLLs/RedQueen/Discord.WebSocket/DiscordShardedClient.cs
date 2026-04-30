using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

internal class DiscordShardedClient : BaseSocketClient, IDiscordClient, IDisposable, IAsyncDisposable
{
	private readonly DiscordSocketConfig _baseConfig;

	private readonly Dictionary<int, int> _shardIdsToIndex;

	private readonly bool _automaticShards;

	private int[] _shardIds;

	private DiscordSocketClient[] _shards;

	private System.Collections.Immutable.ImmutableArray<StickerPack<SocketSticker>> _defaultStickers;

	private int _totalShards;

	private SemaphoreSlim[] _identifySemaphores;

	private object _semaphoreResetLock;

	private Task _semaphoreResetTask;

	private bool _isDisposed;

	private readonly AsyncEvent<Func<DiscordSocketClient, Task>> _shardConnectedEvent = new AsyncEvent<Func<DiscordSocketClient, Task>>();

	private readonly AsyncEvent<Func<Exception, DiscordSocketClient, Task>> _shardDisconnectedEvent = new AsyncEvent<Func<Exception, DiscordSocketClient, Task>>();

	private readonly AsyncEvent<Func<DiscordSocketClient, Task>> _shardReadyEvent = new AsyncEvent<Func<DiscordSocketClient, Task>>();

	private readonly AsyncEvent<Func<int, int, DiscordSocketClient, Task>> _shardLatencyUpdatedEvent = new AsyncEvent<Func<int, int, DiscordSocketClient, Task>>();

	public override int Latency
	{
		get
		{
			return GetLatency();
		}
		protected set
		{
		}
	}

	public override UserStatus Status
	{
		get
		{
			return _shards[0].Status;
		}
		protected set
		{
		}
	}

	public override IActivity Activity
	{
		get
		{
			return _shards[0].Activity;
		}
		protected set
		{
		}
	}

	internal new DiscordSocketApiClient ApiClient
	{
		get
		{
			if (!base.ApiClient.CurrentUserId.HasValue)
			{
				base.ApiClient.CurrentUserId = CurrentUser?.Id;
			}
			return base.ApiClient;
		}
	}

	public override IReadOnlyCollection<StickerPack<SocketSticker>> DefaultStickerPacks => _defaultStickers.ToReadOnlyCollection();

	public override IReadOnlyCollection<SocketGuild> Guilds => GetGuilds().ToReadOnlyCollection(GetGuildCount);

	public override IReadOnlyCollection<ISocketPrivateChannel> PrivateChannels => GetPrivateChannels().ToReadOnlyCollection(GetPrivateChannelCount);

	public IReadOnlyCollection<DiscordSocketClient> Shards => _shards;

	public override DiscordSocketRestClient Rest
	{
		get
		{
			DiscordSocketClient[] shards = _shards;
			if (shards == null)
			{
				return null;
			}
			return shards[0].Rest;
		}
	}

	public override SocketSelfUser CurrentUser
	{
		get
		{
			return _shards?.FirstOrDefault((DiscordSocketClient x) => x.CurrentUser != null)?.CurrentUser;
		}
		protected set
		{
			throw new InvalidOperationException();
		}
	}

	ISelfUser IDiscordClient.CurrentUser => CurrentUser;

	public event Func<DiscordSocketClient, Task> ShardConnected
	{
		add
		{
			_shardConnectedEvent.Add(value);
		}
		remove
		{
			_shardConnectedEvent.Remove(value);
		}
	}

	public event Func<Exception, DiscordSocketClient, Task> ShardDisconnected
	{
		add
		{
			_shardDisconnectedEvent.Add(value);
		}
		remove
		{
			_shardDisconnectedEvent.Remove(value);
		}
	}

	public event Func<DiscordSocketClient, Task> ShardReady
	{
		add
		{
			_shardReadyEvent.Add(value);
		}
		remove
		{
			_shardReadyEvent.Remove(value);
		}
	}

	public event Func<int, int, DiscordSocketClient, Task> ShardLatencyUpdated
	{
		add
		{
			_shardLatencyUpdatedEvent.Add(value);
		}
		remove
		{
			_shardLatencyUpdatedEvent.Remove(value);
		}
	}

	public DiscordShardedClient()
		: this(null, new DiscordSocketConfig())
	{
	}

	public DiscordShardedClient(DiscordSocketConfig config)
		: this(null, config, CreateApiClient(config))
	{
	}

	public DiscordShardedClient(int[] ids)
		: this(ids, new DiscordSocketConfig())
	{
	}

	public DiscordShardedClient(int[] ids, DiscordSocketConfig config)
		: this(ids, config, CreateApiClient(config))
	{
	}

	private DiscordShardedClient(int[] ids, DiscordSocketConfig config, DiscordSocketApiClient client)
		: base(config, client)
	{
		if (config.ShardId.HasValue)
		{
			throw new ArgumentException("ShardId must not be set.");
		}
		if (ids != null && !config.TotalShards.HasValue)
		{
			throw new ArgumentException("Custom ids are not supported when TotalShards is not specified.");
		}
		_semaphoreResetLock = new object();
		_shardIdsToIndex = new Dictionary<int, int>();
		config.DisplayInitialLog = false;
		_baseConfig = config;
		_defaultStickers = System.Collections.Immutable.ImmutableArray.Create<StickerPack<SocketSticker>>();
		if (!config.TotalShards.HasValue)
		{
			_automaticShards = true;
			return;
		}
		_totalShards = config.TotalShards.Value;
		_shardIds = ids ?? Enumerable.Range(0, _totalShards).ToArray();
		_shards = new DiscordSocketClient[_shardIds.Length];
		_identifySemaphores = new SemaphoreSlim[config.IdentifyMaxConcurrency];
		for (int i = 0; i < config.IdentifyMaxConcurrency; i++)
		{
			_identifySemaphores[i] = new SemaphoreSlim(1, 1);
		}
		for (int j = 0; j < _shardIds.Length; j++)
		{
			_shardIdsToIndex.Add(_shardIds[j], j);
			DiscordSocketConfig discordSocketConfig = config.Clone();
			discordSocketConfig.ShardId = _shardIds[j];
			_shards[j] = new DiscordSocketClient(discordSocketConfig, this, (j != 0) ? _shards[0] : null);
			RegisterEvents(_shards[j], j == 0);
		}
	}

	private static DiscordSocketApiClient CreateApiClient(DiscordSocketConfig config)
	{
		return new DiscordSocketApiClient(config.RestClientProvider, config.WebSocketProvider, DiscordConfig.UserAgent, config.GatewayHost, RetryMode.AlwaysRetry, null, config.UseSystemClock, config.DefaultRatelimitCallback);
	}

	internal async Task AcquireIdentifyLockAsync(int shardId, CancellationToken token)
	{
		int num = shardId % _baseConfig.IdentifyMaxConcurrency;
		await _identifySemaphores[num].WaitAsync(token).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal void ReleaseIdentifyLock()
	{
		lock (_semaphoreResetLock)
		{
			if (_semaphoreResetTask == null)
			{
				_semaphoreResetTask = ResetSemaphoresAsync();
			}
		}
	}

	private async Task ResetSemaphoresAsync()
	{
		await Task.Delay(5000).ConfigureAwait(continueOnCapturedContext: false);
		lock (_semaphoreResetLock)
		{
			SemaphoreSlim[] identifySemaphores = _identifySemaphores;
			foreach (SemaphoreSlim semaphoreSlim in identifySemaphores)
			{
				if (semaphoreSlim.CurrentCount == 0)
				{
					semaphoreSlim.Release();
				}
			}
			_semaphoreResetTask = null;
		}
	}

	internal override async Task OnLoginAsync(TokenType tokenType, string token)
	{
		BotGateway botGateway = await GetBotGatewayAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (_automaticShards)
		{
			_shardIds = Enumerable.Range(0, botGateway.Shards).ToArray();
			_totalShards = _shardIds.Length;
			_shards = new DiscordSocketClient[_shardIds.Length];
			int maxConcurrency = botGateway.SessionStartLimit.MaxConcurrency;
			_baseConfig.IdentifyMaxConcurrency = maxConcurrency;
			_identifySemaphores = new SemaphoreSlim[maxConcurrency];
			for (int i = 0; i < maxConcurrency; i++)
			{
				_identifySemaphores[i] = new SemaphoreSlim(1, 1);
			}
			for (int j = 0; j < _shardIds.Length; j++)
			{
				_shardIdsToIndex.Add(_shardIds[j], j);
				DiscordSocketConfig discordSocketConfig = _baseConfig.Clone();
				discordSocketConfig.ShardId = _shardIds[j];
				discordSocketConfig.TotalShards = _totalShards;
				_shards[j] = new DiscordSocketClient(discordSocketConfig, this, (j != 0) ? _shards[0] : null);
				RegisterEvents(_shards[j], j == 0);
			}
		}
		for (int k = 0; k < _shards.Length; k++)
		{
			_shards[k].ApiClient.GatewayUrl = botGateway.Url;
			await _shards[k].LoginAsync(tokenType, token);
		}
		if (_defaultStickers.Length == 0 && _baseConfig.AlwaysDownloadDefaultStickers)
		{
			await DownloadDefaultStickersAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal override async Task OnLogoutAsync()
	{
		if (_shards != null)
		{
			for (int i = 0; i < _shards.Length; i++)
			{
				_shards[i].ApiClient.GatewayUrl = null;
				await _shards[i].LogoutAsync();
			}
		}
		if (_automaticShards)
		{
			_shardIds = new int[0];
			_shardIdsToIndex.Clear();
			_totalShards = 0;
			_shards = null;
		}
	}

	public override async Task StartAsync()
	{
		await Task.WhenAll(_shards.Select((DiscordSocketClient x) => x.StartAsync())).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task StopAsync()
	{
		await Task.WhenAll(_shards.Select((DiscordSocketClient x) => x.StopAsync())).ConfigureAwait(continueOnCapturedContext: false);
	}

	public DiscordSocketClient GetShard(int id)
	{
		if (_shardIdsToIndex.TryGetValue(id, out id))
		{
			return _shards[id];
		}
		return null;
	}

	private int GetShardIdFor(ulong guildId)
	{
		return (int)((guildId >> 22) % (uint)_totalShards);
	}

	public int GetShardIdFor(IGuild guild)
	{
		return GetShardIdFor(guild?.Id ?? 0);
	}

	private DiscordSocketClient GetShardFor(ulong guildId)
	{
		return GetShard(GetShardIdFor(guildId));
	}

	public DiscordSocketClient GetShardFor(IGuild guild)
	{
		return GetShardFor(guild?.Id ?? 0);
	}

	public override async Task<RestApplication> GetApplicationInfoAsync(RequestOptions options = null)
	{
		return await _shards[0].GetApplicationInfoAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override SocketGuild GetGuild(ulong id)
	{
		return GetShardFor(id).GetGuild(id);
	}

	public override SocketChannel GetChannel(ulong id)
	{
		for (int i = 0; i < _shards.Length; i++)
		{
			SocketChannel channel = _shards[i].GetChannel(id);
			if (channel != null)
			{
				return channel;
			}
		}
		return null;
	}

	private IEnumerable<ISocketPrivateChannel> GetPrivateChannels()
	{
		for (int i = 0; i < _shards.Length; i++)
		{
			foreach (ISocketPrivateChannel privateChannel in _shards[i].PrivateChannels)
			{
				yield return privateChannel;
			}
		}
	}

	private int GetPrivateChannelCount()
	{
		int num = 0;
		for (int i = 0; i < _shards.Length; i++)
		{
			num += _shards[i].PrivateChannels.Count;
		}
		return num;
	}

	private IEnumerable<SocketGuild> GetGuilds()
	{
		for (int i = 0; i < _shards.Length; i++)
		{
			foreach (SocketGuild guild in _shards[i].Guilds)
			{
				yield return guild;
			}
		}
	}

	private int GetGuildCount()
	{
		int num = 0;
		for (int i = 0; i < _shards.Length; i++)
		{
			num += _shards[i].Guilds.Count;
		}
		return num;
	}

	public override async Task<SocketSticker> GetStickerAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
	{
		SocketSticker socketSticker = _defaultStickers.FirstOrDefault((StickerPack<SocketSticker> x) => x.Stickers.Any((SocketSticker y) => y.Id == id))?.Stickers.FirstOrDefault((SocketSticker x) => x.Id == id);
		if (socketSticker != null)
		{
			return socketSticker;
		}
		foreach (SocketGuild guild in Guilds)
		{
			socketSticker = await guild.GetStickerAsync(id, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false);
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
			return GetGuild(sticker.GuildId.Value).AddOrUpdateSticker(sticker);
		}
		return SocketSticker.Create(_shards[0], sticker);
	}

	private async Task DownloadDefaultStickersAsync()
	{
		NitroStickerPacks obj = await ApiClient.ListNitroStickerPacksAsync().ConfigureAwait(continueOnCapturedContext: false);
		System.Collections.Immutable.ImmutableArray<StickerPack<SocketSticker>>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<StickerPack<SocketSticker>>();
		foreach (StickerPack stickerPack in obj.StickerPacks)
		{
			IEnumerable<SocketSticker> stickers = stickerPack.Stickers.Select((Discord.API.Sticker x) => SocketSticker.Create(_shards[0], x));
			StickerPack<SocketSticker> item = new StickerPack<SocketSticker>(stickerPack.Name, stickerPack.Id, stickerPack.SkuId, stickerPack.CoverStickerId.ToNullable(), stickerPack.Description, stickerPack.BannerAssetId, stickers);
			builder.Add(item);
		}
		_defaultStickers = builder.ToImmutable();
	}

	public override SocketUser GetUser(ulong id)
	{
		for (int i = 0; i < _shards.Length; i++)
		{
			SocketUser user = _shards[i].GetUser(id);
			if (user != null)
			{
				return user;
			}
		}
		return null;
	}

	public override SocketUser GetUser(string username, string discriminator)
	{
		for (int i = 0; i < _shards.Length; i++)
		{
			SocketUser user = _shards[i].GetUser(username, discriminator);
			if (user != null)
			{
				return user;
			}
		}
		return null;
	}

	public override async ValueTask<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
	{
		return await _shards[0].GetVoiceRegionsAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async ValueTask<RestVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null)
	{
		return await _shards[0].GetVoiceRegionAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task DownloadUsersAsync(IEnumerable<IGuild> guilds)
	{
		if (guilds == null)
		{
			throw new ArgumentNullException("guilds");
		}
		for (int i = 0; i < _shards.Length; i++)
		{
			int id = _shardIds[i];
			IGuild[] array = guilds.Where((IGuild x) => GetShardIdFor(x) == id).ToArray();
			if (array.Length != 0)
			{
				await _shards[i].DownloadUsersAsync(array).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	private int GetLatency()
	{
		int num = 0;
		for (int i = 0; i < _shards.Length; i++)
		{
			num += _shards[i].Latency;
		}
		return (int)Math.Round((double)num / (double)_shards.Length);
	}

	public override async Task SetStatusAsync(UserStatus status)
	{
		for (int i = 0; i < _shards.Length; i++)
		{
			await _shards[i].SetStatusAsync(status).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public override async Task SetGameAsync(string name, string streamUrl = null, ActivityType type = ActivityType.Playing)
	{
		IActivity activityAsync = null;
		if (!string.IsNullOrEmpty(streamUrl))
		{
			activityAsync = new StreamingGame(name, streamUrl);
		}
		else if (!string.IsNullOrEmpty(name))
		{
			activityAsync = new Game(name, type);
		}
		await SetActivityAsync(activityAsync).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task SetActivityAsync(IActivity activity)
	{
		for (int i = 0; i < _shards.Length; i++)
		{
			await _shards[i].SetActivityAsync(activity).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private void RegisterEvents(DiscordSocketClient client, bool isPrimary)
	{
		client.Log += (LogMessage msg) => _logEvent.InvokeAsync(msg);
		client.LoggedOut += delegate
		{
			LoginState loginState = base.LoginState;
			if (loginState == LoginState.LoggedIn || loginState == LoginState.LoggingIn)
			{
				LogoutAsync();
			}
			return Task.Delay(0);
		};
		client.Connected += () => _shardConnectedEvent.InvokeAsync(client);
		client.Disconnected += (Exception exception) => _shardDisconnectedEvent.InvokeAsync(exception, client);
		client.Ready += () => _shardReadyEvent.InvokeAsync(client);
		client.LatencyUpdated += (int oldLatency, int newLatency) => _shardLatencyUpdatedEvent.InvokeAsync(oldLatency, newLatency, client);
		client.ChannelCreated += (SocketChannel channel) => _channelCreatedEvent.InvokeAsync(channel);
		client.ChannelDestroyed += (SocketChannel channel) => _channelDestroyedEvent.InvokeAsync(channel);
		client.ChannelUpdated += (SocketChannel oldChannel, SocketChannel newChannel) => _channelUpdatedEvent.InvokeAsync(oldChannel, newChannel);
		client.MessageReceived += (SocketMessage msg) => _messageReceivedEvent.InvokeAsync(msg);
		client.MessageDeleted += (Cacheable<IMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel) => _messageDeletedEvent.InvokeAsync(cache, channel);
		client.MessagesBulkDeleted += (IReadOnlyCollection<Cacheable<IMessage, ulong>> cache, Cacheable<IMessageChannel, ulong> channel) => _messagesBulkDeletedEvent.InvokeAsync(cache, channel);
		client.MessageUpdated += (Cacheable<IMessage, ulong> oldMsg, SocketMessage newMsg, ISocketMessageChannel channel) => _messageUpdatedEvent.InvokeAsync(oldMsg, newMsg, channel);
		client.ReactionAdded += (Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) => _reactionAddedEvent.InvokeAsync(cache, channel, reaction);
		client.ReactionRemoved += (Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) => _reactionRemovedEvent.InvokeAsync(cache, channel, reaction);
		client.ReactionsCleared += (Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel) => _reactionsClearedEvent.InvokeAsync(cache, channel);
		client.ReactionsRemovedForEmote += (Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, IEmote emote) => _reactionsRemovedForEmoteEvent.InvokeAsync(cache, channel, emote);
		client.RoleCreated += (SocketRole role) => _roleCreatedEvent.InvokeAsync(role);
		client.RoleDeleted += (SocketRole role) => _roleDeletedEvent.InvokeAsync(role);
		client.RoleUpdated += (SocketRole oldRole, SocketRole newRole) => _roleUpdatedEvent.InvokeAsync(oldRole, newRole);
		client.JoinedGuild += (SocketGuild guild) => _joinedGuildEvent.InvokeAsync(guild);
		client.LeftGuild += (SocketGuild guild) => _leftGuildEvent.InvokeAsync(guild);
		client.GuildAvailable += (SocketGuild guild) => _guildAvailableEvent.InvokeAsync(guild);
		client.GuildUnavailable += (SocketGuild guild) => _guildUnavailableEvent.InvokeAsync(guild);
		client.GuildMembersDownloaded += (SocketGuild guild) => _guildMembersDownloadedEvent.InvokeAsync(guild);
		client.GuildUpdated += (SocketGuild oldGuild, SocketGuild newGuild) => _guildUpdatedEvent.InvokeAsync(oldGuild, newGuild);
		client.UserJoined += (SocketGuildUser user) => _userJoinedEvent.InvokeAsync(user);
		client.UserLeft += (SocketGuild guild, SocketUser user) => _userLeftEvent.InvokeAsync(guild, user);
		client.UserBanned += (SocketUser user, SocketGuild guild) => _userBannedEvent.InvokeAsync(user, guild);
		client.UserUnbanned += (SocketUser user, SocketGuild guild) => _userUnbannedEvent.InvokeAsync(user, guild);
		client.UserUpdated += (SocketUser oldUser, SocketUser newUser) => _userUpdatedEvent.InvokeAsync(oldUser, newUser);
		client.PresenceUpdated += (SocketUser user, SocketPresence oldPresence, SocketPresence newPresence) => _presenceUpdated.InvokeAsync(user, oldPresence, newPresence);
		client.GuildMemberUpdated += (Cacheable<SocketGuildUser, ulong> oldUser, SocketGuildUser newUser) => _guildMemberUpdatedEvent.InvokeAsync(oldUser, newUser);
		client.UserVoiceStateUpdated += (SocketUser user, SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState) => _userVoiceStateUpdatedEvent.InvokeAsync(user, oldVoiceState, newVoiceState);
		client.VoiceServerUpdated += (SocketVoiceServer server) => _voiceServerUpdatedEvent.InvokeAsync(server);
		client.CurrentUserUpdated += (SocketSelfUser oldUser, SocketSelfUser newUser) => _selfUpdatedEvent.InvokeAsync(oldUser, newUser);
		client.UserIsTyping += (Cacheable<IUser, ulong> oldUser, Cacheable<IMessageChannel, ulong> newUser) => _userIsTypingEvent.InvokeAsync(oldUser, newUser);
		client.RecipientAdded += (SocketGroupUser user) => _recipientAddedEvent.InvokeAsync(user);
		client.RecipientRemoved += (SocketGroupUser user) => _recipientRemovedEvent.InvokeAsync(user);
		client.InviteCreated += (SocketInvite invite) => _inviteCreatedEvent.InvokeAsync(invite);
		client.InviteDeleted += (SocketGuildChannel channel, string invite) => _inviteDeletedEvent.InvokeAsync(channel, invite);
		client.InteractionCreated += (SocketInteraction interaction) => _interactionCreatedEvent.InvokeAsync(interaction);
		client.ButtonExecuted += (SocketMessageComponent arg) => _buttonExecuted.InvokeAsync(arg);
		client.SelectMenuExecuted += (SocketMessageComponent arg) => _selectMenuExecuted.InvokeAsync(arg);
		client.SlashCommandExecuted += (SocketSlashCommand arg) => _slashCommandExecuted.InvokeAsync(arg);
		client.UserCommandExecuted += (SocketUserCommand arg) => _userCommandExecuted.InvokeAsync(arg);
		client.MessageCommandExecuted += (SocketMessageCommand arg) => _messageCommandExecuted.InvokeAsync(arg);
		client.AutocompleteExecuted += (SocketAutocompleteInteraction arg) => _autocompleteExecuted.InvokeAsync(arg);
		client.ModalSubmitted += (SocketModal arg) => _modalSubmitted.InvokeAsync(arg);
		client.ThreadUpdated += (Cacheable<SocketThreadChannel, ulong> thread1, SocketThreadChannel thread2) => _threadUpdated.InvokeAsync(thread1, thread2);
		client.ThreadCreated += (SocketThreadChannel thread) => _threadCreated.InvokeAsync(thread);
		client.ThreadDeleted += (Cacheable<SocketThreadChannel, ulong> thread) => _threadDeleted.InvokeAsync(thread);
		client.ThreadMemberJoined += (SocketThreadUser user) => _threadMemberJoined.InvokeAsync(user);
		client.ThreadMemberLeft += (SocketThreadUser user) => _threadMemberLeft.InvokeAsync(user);
		client.StageEnded += (SocketStageChannel stage) => _stageEnded.InvokeAsync(stage);
		client.StageStarted += (SocketStageChannel stage) => _stageStarted.InvokeAsync(stage);
		client.StageUpdated += (SocketStageChannel stage1, SocketStageChannel stage2) => _stageUpdated.InvokeAsync(stage1, stage2);
		client.RequestToSpeak += (SocketStageChannel stage, SocketGuildUser user) => _requestToSpeak.InvokeAsync(stage, user);
		client.SpeakerAdded += (SocketStageChannel stage, SocketGuildUser user) => _speakerAdded.InvokeAsync(stage, user);
		client.SpeakerRemoved += (SocketStageChannel stage, SocketGuildUser user) => _speakerRemoved.InvokeAsync(stage, user);
		client.GuildStickerCreated += (SocketCustomSticker sticker) => _guildStickerCreated.InvokeAsync(sticker);
		client.GuildStickerDeleted += (SocketCustomSticker sticker) => _guildStickerDeleted.InvokeAsync(sticker);
		client.GuildStickerUpdated += (SocketCustomSticker before, SocketCustomSticker after) => _guildStickerUpdated.InvokeAsync(before, after);
		client.GuildJoinRequestDeleted += (Cacheable<SocketGuildUser, ulong> userId, SocketGuild guildId) => _guildJoinRequestDeletedEvent.InvokeAsync(userId, guildId);
		client.GuildScheduledEventCancelled += (SocketGuildEvent arg) => _guildScheduledEventCancelled.InvokeAsync(arg);
		client.GuildScheduledEventCompleted += (SocketGuildEvent arg) => _guildScheduledEventCompleted.InvokeAsync(arg);
		client.GuildScheduledEventCreated += (SocketGuildEvent arg) => _guildScheduledEventCreated.InvokeAsync(arg);
		client.GuildScheduledEventUpdated += (Cacheable<SocketGuildEvent, ulong> arg1, SocketGuildEvent arg2) => _guildScheduledEventUpdated.InvokeAsync(arg1, arg2);
		client.GuildScheduledEventStarted += (SocketGuildEvent arg) => _guildScheduledEventStarted.InvokeAsync(arg);
		client.GuildScheduledEventUserAdd += (Cacheable<SocketUser, RestUser, IUser, ulong> arg1, SocketGuildEvent arg2) => _guildScheduledEventUserAdd.InvokeAsync(arg1, arg2);
		client.GuildScheduledEventUserRemove += (Cacheable<SocketUser, RestUser, IUser, ulong> arg1, SocketGuildEvent arg2) => _guildScheduledEventUserRemove.InvokeAsync(arg1, arg2);
		client.WebhooksUpdated += (SocketGuild arg1, SocketChannel arg2) => _webhooksUpdated.InvokeAsync(arg1, arg2);
	}

	async Task<IApplication> IDiscordClient.GetApplicationInfoAsync(RequestOptions options)
	{
		return await GetApplicationInfoAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<IChannel> IDiscordClient.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IChannel)GetChannel(id));
	}

	Task<IReadOnlyCollection<IPrivateChannel>> IDiscordClient.GetPrivateChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IPrivateChannel>)PrivateChannels);
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
		return await GetVoiceRegionsAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IVoiceRegion> IDiscordClient.GetVoiceRegionAsync(string id, RequestOptions options)
	{
		return await GetVoiceRegionAsync(id).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal override void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing && _shards != null)
			{
				DiscordSocketClient[] shards = _shards;
				for (int i = 0; i < shards.Length; i++)
				{
					shards[i]?.Dispose();
				}
			}
			_isDisposed = true;
		}
		base.Dispose(disposing);
	}

	internal override ValueTask DisposeAsync(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing && _shards != null)
			{
				DiscordSocketClient[] shards = _shards;
				for (int i = 0; i < shards.Length; i++)
				{
					shards[i]?.Dispose();
				}
			}
			_isDisposed = true;
		}
		return base.DisposeAsync(disposing);
	}
}

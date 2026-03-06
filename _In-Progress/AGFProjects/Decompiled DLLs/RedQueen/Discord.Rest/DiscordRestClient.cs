using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.API;
using Discord.Net.Converters;
using Discord.Net.ED25519;
using Newtonsoft.Json;

namespace Discord.Rest;

internal class DiscordRestClient : BaseDiscordClient, IDiscordClient, IDisposable, IAsyncDisposable
{
	private RestApplication _applicationInfo;

	internal static JsonSerializer Serializer = new JsonSerializer
	{
		ContractResolver = new DiscordContractResolver(),
		NullValueHandling = NullValueHandling.Include
	};

	private readonly bool _apiOnCreation;

	public new RestSelfUser CurrentUser
	{
		get
		{
			return base.CurrentUser as RestSelfUser;
		}
		internal set
		{
			base.CurrentUser = value;
		}
	}

	public DiscordRestClient()
		: this(new DiscordRestConfig())
	{
	}

	public DiscordRestClient(DiscordRestConfig config)
		: base(config, CreateApiClient(config))
	{
		_apiOnCreation = config.APIOnRestInteractionCreation;
	}

	internal DiscordRestClient(DiscordRestConfig config, DiscordRestApiClient api)
		: base(config, api)
	{
		_apiOnCreation = config.APIOnRestInteractionCreation;
	}

	private static DiscordRestApiClient CreateApiClient(DiscordRestConfig config)
	{
		return new DiscordRestApiClient(config.RestClientProvider, DiscordConfig.UserAgent, RetryMode.AlwaysRetry, Serializer, config.UseSystemClock, config.DefaultRatelimitCallback);
	}

	internal override void Dispose(bool disposing)
	{
		if (disposing)
		{
			base.ApiClient.Dispose();
		}
		base.Dispose(disposing);
	}

	internal override async ValueTask DisposeAsync(bool disposing)
	{
		if (disposing)
		{
			await base.ApiClient.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		base.Dispose(disposing);
	}

	internal override async Task OnLoginAsync(TokenType tokenType, string token)
	{
		User user = await base.ApiClient.GetMyUserAsync(new RequestOptions
		{
			RetryMode = RetryMode.AlwaysRetry
		}).ConfigureAwait(continueOnCapturedContext: false);
		base.ApiClient.CurrentUserId = user.Id;
		base.CurrentUser = RestSelfUser.Create(this, user);
		if (tokenType == TokenType.Bot)
		{
			await GetApplicationInfoAsync(new RequestOptions
			{
				RetryMode = RetryMode.AlwaysRetry
			}).ConfigureAwait(continueOnCapturedContext: false);
			base.ApiClient.CurrentApplicationId = _applicationInfo.Id;
		}
	}

	internal void CreateRestSelfUser(User user)
	{
		base.CurrentUser = RestSelfUser.Create(this, user);
	}

	internal override Task OnLogoutAsync()
	{
		_applicationInfo = null;
		return Task.Delay(0);
	}

	public bool IsValidHttpInteraction(string publicKey, string signature, string timestamp, string body)
	{
		return IsValidHttpInteraction(publicKey, signature, timestamp, Encoding.UTF8.GetBytes(body));
	}

	public bool IsValidHttpInteraction(string publicKey, string signature, string timestamp, byte[] body)
	{
		byte[] publicKey2 = HexConverter.HexToByteArray(publicKey);
		byte[] signature2 = HexConverter.HexToByteArray(signature);
		byte[] bytes = Encoding.UTF8.GetBytes(timestamp);
		List<byte> list = new List<byte>();
		list.AddRange(bytes);
		list.AddRange(body);
		return IsValidHttpInteraction(publicKey2, signature2, list.ToArray());
	}

	private bool IsValidHttpInteraction(byte[] publicKey, byte[] signature, byte[] message)
	{
		return Ed25519.Verify(signature, message, publicKey);
	}

	public Task<RestInteraction> ParseHttpInteractionAsync(string publicKey, string signature, string timestamp, string body, Func<InteractionProperties, bool> doApiCallOnCreation = null)
	{
		return ParseHttpInteractionAsync(publicKey, signature, timestamp, Encoding.UTF8.GetBytes(body), doApiCallOnCreation);
	}

	public async Task<RestInteraction> ParseHttpInteractionAsync(string publicKey, string signature, string timestamp, byte[] body, Func<InteractionProperties, bool> doApiCallOnCreation = null)
	{
		if (!IsValidHttpInteraction(publicKey, signature, timestamp, body))
		{
			throw new BadSignatureException();
		}
		using StringReader textReader = new StringReader(Encoding.UTF8.GetString(body));
		using JsonTextReader jsonReader = new JsonTextReader(textReader);
		Interaction model = Serializer.Deserialize<Interaction>(jsonReader);
		return await RestInteraction.CreateAsync(this, model, doApiCallOnCreation?.Invoke(new InteractionProperties(model)) ?? _apiOnCreation);
	}

	public async Task<RestApplication> GetApplicationInfoAsync(RequestOptions options = null)
	{
		RestApplication restApplication = _applicationInfo;
		if (restApplication == null)
		{
			restApplication = (_applicationInfo = await ClientHelper.GetApplicationInfoAsync(this, options).ConfigureAwait(continueOnCapturedContext: false));
		}
		return restApplication;
	}

	public Task<RestChannel> GetChannelAsync(ulong id, RequestOptions options = null)
	{
		return ClientHelper.GetChannelAsync(this, id, options);
	}

	public Task<IReadOnlyCollection<IRestPrivateChannel>> GetPrivateChannelsAsync(RequestOptions options = null)
	{
		return ClientHelper.GetPrivateChannelsAsync(this, options);
	}

	public Task<IReadOnlyCollection<RestDMChannel>> GetDMChannelsAsync(RequestOptions options = null)
	{
		return ClientHelper.GetDMChannelsAsync(this, options);
	}

	public Task<IReadOnlyCollection<RestGroupChannel>> GetGroupChannelsAsync(RequestOptions options = null)
	{
		return ClientHelper.GetGroupChannelsAsync(this, options);
	}

	public Task<IReadOnlyCollection<RestConnection>> GetConnectionsAsync(RequestOptions options = null)
	{
		return ClientHelper.GetConnectionsAsync(this, options);
	}

	public Task<RestInviteMetadata> GetInviteAsync(string inviteId, RequestOptions options = null)
	{
		return ClientHelper.GetInviteAsync(this, inviteId, options);
	}

	public Task<RestGuild> GetGuildAsync(ulong id, RequestOptions options = null)
	{
		return ClientHelper.GetGuildAsync(this, id, withCounts: false, options);
	}

	public Task<RestGuild> GetGuildAsync(ulong id, bool withCounts, RequestOptions options = null)
	{
		return ClientHelper.GetGuildAsync(this, id, withCounts, options);
	}

	public Task<RestGuildWidget?> GetGuildWidgetAsync(ulong id, RequestOptions options = null)
	{
		return ClientHelper.GetGuildWidgetAsync(this, id, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestUserGuild>> GetGuildSummariesAsync(RequestOptions options = null)
	{
		return ClientHelper.GetGuildSummariesAsync(this, null, null, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestUserGuild>> GetGuildSummariesAsync(ulong fromGuildId, int limit, RequestOptions options = null)
	{
		return ClientHelper.GetGuildSummariesAsync(this, fromGuildId, limit, options);
	}

	public Task<IReadOnlyCollection<RestGuild>> GetGuildsAsync(RequestOptions options = null)
	{
		return ClientHelper.GetGuildsAsync(this, withCounts: false, options);
	}

	public Task<IReadOnlyCollection<RestGuild>> GetGuildsAsync(bool withCounts, RequestOptions options = null)
	{
		return ClientHelper.GetGuildsAsync(this, withCounts, options);
	}

	public Task<RestGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null)
	{
		return ClientHelper.CreateGuildAsync(this, name, region, jpegIcon, options);
	}

	public Task<RestUser> GetUserAsync(ulong id, RequestOptions options = null)
	{
		return ClientHelper.GetUserAsync(this, id, options);
	}

	public Task<RestGuildUser> GetGuildUserAsync(ulong guildId, ulong id, RequestOptions options = null)
	{
		return ClientHelper.GetGuildUserAsync(this, guildId, id, options);
	}

	public Task<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
	{
		return ClientHelper.GetVoiceRegionsAsync(this, options);
	}

	public Task<RestVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null)
	{
		return ClientHelper.GetVoiceRegionAsync(this, id, options);
	}

	public Task<RestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
	{
		return ClientHelper.GetWebhookAsync(this, id, options);
	}

	public Task<RestGlobalCommand> CreateGlobalCommand(ApplicationCommandProperties properties, RequestOptions options = null)
	{
		return ClientHelper.CreateGlobalApplicationCommandAsync(this, properties, options);
	}

	public Task<RestGuildCommand> CreateGuildCommand(ApplicationCommandProperties properties, ulong guildId, RequestOptions options = null)
	{
		return ClientHelper.CreateGuildApplicationCommandAsync(this, guildId, properties, options);
	}

	public Task<IReadOnlyCollection<RestGlobalCommand>> GetGlobalApplicationCommands(bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		return ClientHelper.GetGlobalApplicationCommandsAsync(this, withLocalizations, locale, options);
	}

	public Task<IReadOnlyCollection<RestGuildCommand>> GetGuildApplicationCommands(ulong guildId, bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		return ClientHelper.GetGuildApplicationCommandsAsync(this, guildId, withLocalizations, locale, options);
	}

	public Task<IReadOnlyCollection<RestGlobalCommand>> BulkOverwriteGlobalCommands(ApplicationCommandProperties[] commandProperties, RequestOptions options = null)
	{
		return ClientHelper.BulkOverwriteGlobalApplicationCommandAsync(this, commandProperties, options);
	}

	public Task<IReadOnlyCollection<RestGuildCommand>> BulkOverwriteGuildCommands(ApplicationCommandProperties[] commandProperties, ulong guildId, RequestOptions options = null)
	{
		return ClientHelper.BulkOverwriteGuildApplicationCommandAsync(this, guildId, commandProperties, options);
	}

	public Task<IReadOnlyCollection<GuildApplicationCommandPermission>> BatchEditGuildCommandPermissions(ulong guildId, IDictionary<ulong, ApplicationCommandPermission[]> permissions, RequestOptions options = null)
	{
		return InteractionHelper.BatchEditGuildCommandPermissionsAsync(this, guildId, permissions, options);
	}

	public Task DeleteAllGlobalCommandsAsync(RequestOptions options = null)
	{
		return InteractionHelper.DeleteAllGlobalCommandsAsync(this, options);
	}

	public Task AddRoleAsync(ulong guildId, ulong userId, ulong roleId)
	{
		return ClientHelper.AddRoleAsync(this, guildId, userId, roleId);
	}

	public Task RemoveRoleAsync(ulong guildId, ulong userId, ulong roleId)
	{
		return ClientHelper.RemoveRoleAsync(this, guildId, userId, roleId);
	}

	public Task AddReactionAsync(ulong channelId, ulong messageId, IEmote emote, RequestOptions options = null)
	{
		return MessageHelper.AddReactionAsync(channelId, messageId, emote, this, options);
	}

	public Task RemoveReactionAsync(ulong channelId, ulong messageId, ulong userId, IEmote emote, RequestOptions options = null)
	{
		return MessageHelper.RemoveReactionAsync(channelId, messageId, userId, emote, this, options);
	}

	public Task RemoveAllReactionsAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		return MessageHelper.RemoveAllReactionsAsync(channelId, messageId, this, options);
	}

	public Task RemoveAllReactionsForEmoteAsync(ulong channelId, ulong messageId, IEmote emote, RequestOptions options = null)
	{
		return MessageHelper.RemoveAllReactionsForEmoteAsync(channelId, messageId, emote, this, options);
	}

	async Task<IApplication> IDiscordClient.GetApplicationInfoAsync(RequestOptions options)
	{
		return await GetApplicationInfoAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IChannel> IDiscordClient.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetChannelAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IReadOnlyCollection<IPrivateChannel>> IDiscordClient.GetPrivateChannelsAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetPrivateChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return System.Collections.Immutable.ImmutableArray.Create<IPrivateChannel>();
	}

	async Task<IReadOnlyCollection<IDMChannel>> IDiscordClient.GetDMChannelsAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetDMChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return System.Collections.Immutable.ImmutableArray.Create<IDMChannel>();
	}

	async Task<IReadOnlyCollection<IGroupChannel>> IDiscordClient.GetGroupChannelsAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetGroupChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return System.Collections.Immutable.ImmutableArray.Create<IGroupChannel>();
	}

	async Task<IReadOnlyCollection<IConnection>> IDiscordClient.GetConnectionsAsync(RequestOptions options)
	{
		return await GetConnectionsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IInvite> IDiscordClient.GetInviteAsync(string inviteId, RequestOptions options)
	{
		return await GetInviteAsync(inviteId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IGuild> IDiscordClient.GetGuildAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetGuildAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IReadOnlyCollection<IGuild>> IDiscordClient.GetGuildsAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetGuildsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return System.Collections.Immutable.ImmutableArray.Create<IGuild>();
	}

	async Task<IGuild> IDiscordClient.CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
	{
		return await CreateGuildAsync(name, region, jpegIcon, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUser> IDiscordClient.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetUserAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IReadOnlyCollection<IVoiceRegion>> IDiscordClient.GetVoiceRegionsAsync(RequestOptions options)
	{
		return await GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IVoiceRegion> IDiscordClient.GetVoiceRegionAsync(string id, RequestOptions options)
	{
		return await GetVoiceRegionAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IWebhook> IDiscordClient.GetWebhookAsync(ulong id, RequestOptions options)
	{
		return await GetWebhookAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IApplicationCommand>> IDiscordClient.GetGlobalApplicationCommandsAsync(bool withLocalizations, string locale, RequestOptions options)
	{
		return await GetGlobalApplicationCommands(withLocalizations, locale, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IApplicationCommand> IDiscordClient.GetGlobalApplicationCommandAsync(ulong id, RequestOptions options)
	{
		return await ClientHelper.GetGlobalApplicationCommandAsync(this, id, options).ConfigureAwait(continueOnCapturedContext: false);
	}
}

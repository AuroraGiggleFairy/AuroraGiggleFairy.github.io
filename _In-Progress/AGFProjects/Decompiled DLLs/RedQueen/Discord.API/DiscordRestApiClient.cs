using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord.API.Rest;
using Discord.Net;
using Discord.Net.Converters;
using Discord.Net.Queue;
using Discord.Net.Rest;
using Newtonsoft.Json;

namespace Discord.API;

internal class DiscordRestApiClient : IDisposable, IAsyncDisposable
{
	internal class BucketIds
	{
		public ulong GuildId { get; internal set; }

		public ulong ChannelId { get; internal set; }

		public ulong WebhookId { get; internal set; }

		public string HttpMethod { get; internal set; }

		internal BucketIds(ulong guildId = 0uL, ulong channelId = 0uL, ulong webhookId = 0uL)
		{
			GuildId = guildId;
			ChannelId = channelId;
			WebhookId = webhookId;
		}

		internal object[] ToArray()
		{
			return new object[4] { HttpMethod, GuildId, ChannelId, WebhookId };
		}

		internal Dictionary<string, string> ToMajorParametersDictionary()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (GuildId != 0L)
			{
				dictionary["GuildId"] = GuildId.ToString();
			}
			if (ChannelId != 0L)
			{
				dictionary["ChannelId"] = ChannelId.ToString();
			}
			if (WebhookId != 0L)
			{
				dictionary["WebhookId"] = WebhookId.ToString();
			}
			return dictionary;
		}

		internal static int? GetIndex(string name)
		{
			return name switch
			{
				"httpMethod" => 0, 
				"guildId" => 1, 
				"channelId" => 2, 
				"webhookId" => 3, 
				_ => null, 
			};
		}
	}

	private static readonly ConcurrentDictionary<string, Func<BucketIds, BucketId>> _bucketIdGenerators = new ConcurrentDictionary<string, Func<BucketIds, BucketId>>();

	private readonly AsyncEvent<Func<string, string, double, Task>> _sentRequestEvent = new AsyncEvent<Func<string, string, double, Task>>();

	protected readonly JsonSerializer _serializer;

	protected readonly SemaphoreSlim _stateLock;

	private readonly RestClientProvider _restClientProvider;

	protected bool _isDisposed;

	private CancellationTokenSource _loginCancelToken;

	public RetryMode DefaultRetryMode { get; }

	public string UserAgent { get; }

	internal RequestQueue RequestQueue { get; }

	public LoginState LoginState { get; private set; }

	public TokenType AuthTokenType { get; private set; }

	internal string AuthToken { get; private set; }

	internal IRestClient RestClient { get; private set; }

	internal ulong? CurrentUserId { get; set; }

	internal ulong? CurrentApplicationId { get; set; }

	internal bool UseSystemClock { get; set; }

	internal Func<IRateLimitInfo, Task> DefaultRatelimitCallback { get; set; }

	internal JsonSerializer Serializer => _serializer;

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

	public DiscordRestApiClient(RestClientProvider restClientProvider, string userAgent, RetryMode defaultRetryMode = RetryMode.AlwaysRetry, JsonSerializer serializer = null, bool useSystemClock = true, Func<IRateLimitInfo, Task> defaultRatelimitCallback = null)
	{
		_restClientProvider = restClientProvider;
		UserAgent = userAgent;
		DefaultRetryMode = defaultRetryMode;
		_serializer = serializer ?? new JsonSerializer
		{
			ContractResolver = new DiscordContractResolver()
		};
		UseSystemClock = useSystemClock;
		DefaultRatelimitCallback = defaultRatelimitCallback;
		RequestQueue = new RequestQueue();
		_stateLock = new SemaphoreSlim(1, 1);
		SetBaseUrl(DiscordConfig.APIUrl);
	}

	internal void SetBaseUrl(string baseUrl)
	{
		RestClient?.Dispose();
		RestClient = _restClientProvider(baseUrl);
		RestClient.SetHeader("accept", "*/*");
		RestClient.SetHeader("user-agent", UserAgent);
		RestClient.SetHeader("authorization", GetPrefixedToken(AuthTokenType, AuthToken));
	}

	internal static string GetPrefixedToken(TokenType tokenType, string token)
	{
		return tokenType switch
		{
			TokenType.Bot => "Bot " + token, 
			TokenType.Bearer => "Bearer " + token, 
			_ => throw new ArgumentException("Unknown OAuth token type.", "tokenType"), 
		};
	}

	internal virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_loginCancelToken?.Dispose();
				RestClient?.Dispose();
				RequestQueue?.Dispose();
				_stateLock?.Dispose();
			}
			_isDisposed = true;
		}
	}

	internal virtual async ValueTask DisposeAsync(bool disposing)
	{
		if (_isDisposed)
		{
			return;
		}
		if (disposing)
		{
			_loginCancelToken?.Dispose();
			RestClient?.Dispose();
			if (RequestQueue != null)
			{
				await RequestQueue.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			_stateLock?.Dispose();
		}
		_isDisposed = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public ValueTask DisposeAsync()
	{
		return DisposeAsync(disposing: true);
	}

	public async Task LoginAsync(TokenType tokenType, string token, RequestOptions options = null)
	{
		await _stateLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await LoginInternalAsync(tokenType, token, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_stateLock.Release();
		}
	}

	private async Task LoginInternalAsync(TokenType tokenType, string token, RequestOptions options = null)
	{
		if (LoginState != LoginState.LoggedOut)
		{
			await LogoutInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		LoginState = LoginState.LoggingIn;
		try
		{
			_loginCancelToken?.Dispose();
			_loginCancelToken = new CancellationTokenSource();
			AuthToken = null;
			await RequestQueue.SetCancelTokenAsync(_loginCancelToken.Token).ConfigureAwait(continueOnCapturedContext: false);
			RestClient.SetCancelToken(_loginCancelToken.Token);
			AuthTokenType = tokenType;
			AuthToken = token?.TrimEnd();
			if (tokenType != TokenType.Webhook)
			{
				RestClient.SetHeader("authorization", GetPrefixedToken(AuthTokenType, AuthToken));
			}
			LoginState = LoginState.LoggedIn;
		}
		catch
		{
			await LogoutInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
			throw;
		}
	}

	public async Task LogoutAsync()
	{
		await _stateLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await LogoutInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_stateLock.Release();
		}
	}

	private async Task LogoutInternalAsync()
	{
		if (LoginState != LoginState.LoggedOut)
		{
			LoginState = LoginState.LoggingOut;
			try
			{
				_loginCancelToken?.Cancel(throwOnFirstException: false);
			}
			catch
			{
			}
			await DisconnectInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
			await RequestQueue.ClearAsync().ConfigureAwait(continueOnCapturedContext: false);
			await RequestQueue.SetCancelTokenAsync(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
			RestClient.SetCancelToken(CancellationToken.None);
			CurrentUserId = null;
			LoginState = LoginState.LoggedOut;
		}
	}

	internal virtual Task ConnectInternalAsync()
	{
		return Task.Delay(0);
	}

	internal virtual Task DisconnectInternalAsync(Exception ex = null)
	{
		return Task.Delay(0);
	}

	internal Task SendAsync(string method, Expression<Func<string>> endpointExpr, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null)
	{
		return SendAsync(method, GetEndpoint(endpointExpr), GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public async Task SendAsync(string method, string endpoint, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null)
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.HeaderOnly = true;
		options.BucketId = bucketId;
		RestRequest request = new RestRequest(RestClient, method, endpoint, options);
		await SendInternalAsync(method, endpoint, request).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal Task SendJsonAsync(string method, Expression<Func<string>> endpointExpr, object payload, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null)
	{
		return SendJsonAsync(method, GetEndpoint(endpointExpr), payload, GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public async Task SendJsonAsync(string method, string endpoint, object payload, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null)
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.HeaderOnly = true;
		options.BucketId = bucketId;
		string json = ((payload != null) ? SerializeJson(payload) : null);
		JsonRestRequest request = new JsonRestRequest(RestClient, method, endpoint, json, options);
		await SendInternalAsync(method, endpoint, request).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal Task SendMultipartAsync(string method, Expression<Func<string>> endpointExpr, IReadOnlyDictionary<string, object> multipartArgs, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null)
	{
		return SendMultipartAsync(method, GetEndpoint(endpointExpr), multipartArgs, GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public async Task SendMultipartAsync(string method, string endpoint, IReadOnlyDictionary<string, object> multipartArgs, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null)
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.HeaderOnly = true;
		options.BucketId = bucketId;
		MultipartRestRequest request = new MultipartRestRequest(RestClient, method, endpoint, multipartArgs, options);
		await SendInternalAsync(method, endpoint, request).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal Task<TResponse> SendAsync<TResponse>(string method, Expression<Func<string>> endpointExpr, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null) where TResponse : class
	{
		return SendAsync<TResponse>(method, GetEndpoint(endpointExpr), GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public async Task<TResponse> SendAsync<TResponse>(string method, string endpoint, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null) where TResponse : class
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.BucketId = bucketId;
		RestRequest request = new RestRequest(RestClient, method, endpoint, options);
		return DeserializeJson<TResponse>(await SendInternalAsync(method, endpoint, request).ConfigureAwait(continueOnCapturedContext: false));
	}

	internal Task<TResponse> SendJsonAsync<TResponse>(string method, Expression<Func<string>> endpointExpr, object payload, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null) where TResponse : class
	{
		return SendJsonAsync<TResponse>(method, GetEndpoint(endpointExpr), payload, GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public async Task<TResponse> SendJsonAsync<TResponse>(string method, string endpoint, object payload, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null) where TResponse : class
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.BucketId = bucketId;
		string json = ((payload != null) ? SerializeJson(payload) : null);
		JsonRestRequest request = new JsonRestRequest(RestClient, method, endpoint, json, options);
		return DeserializeJson<TResponse>(await SendInternalAsync(method, endpoint, request).ConfigureAwait(continueOnCapturedContext: false));
	}

	internal Task<TResponse> SendMultipartAsync<TResponse>(string method, Expression<Func<string>> endpointExpr, IReadOnlyDictionary<string, object> multipartArgs, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null)
	{
		return SendMultipartAsync<TResponse>(method, GetEndpoint(endpointExpr), multipartArgs, GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public async Task<TResponse> SendMultipartAsync<TResponse>(string method, string endpoint, IReadOnlyDictionary<string, object> multipartArgs, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null)
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.BucketId = bucketId;
		MultipartRestRequest request = new MultipartRestRequest(RestClient, method, endpoint, multipartArgs, options);
		return DeserializeJson<TResponse>(await SendInternalAsync(method, endpoint, request).ConfigureAwait(continueOnCapturedContext: false));
	}

	private async Task<Stream> SendInternalAsync(string method, string endpoint, RestRequest request)
	{
		if (!request.Options.IgnoreState)
		{
			CheckState();
		}
		RequestOptions options = request.Options;
		RetryMode? retryMode = options.RetryMode;
		retryMode.GetValueOrDefault();
		if (!retryMode.HasValue)
		{
			RetryMode defaultRetryMode = DefaultRetryMode;
			options.RetryMode = defaultRetryMode;
		}
		options = request.Options;
		bool? useSystemClock = options.UseSystemClock;
		_ = useSystemClock == true;
		if (!useSystemClock.HasValue)
		{
			bool useSystemClock2 = UseSystemClock;
			options.UseSystemClock = useSystemClock2;
		}
		options = request.Options;
		if (options.RatelimitCallback == null)
		{
			options.RatelimitCallback = DefaultRatelimitCallback;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		Stream responseStream = await RequestQueue.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
		stopwatch.Stop();
		double arg = ToMilliseconds(stopwatch);
		await _sentRequestEvent.InvokeAsync(method, endpoint, arg).ConfigureAwait(continueOnCapturedContext: false);
		return responseStream;
	}

	public async Task ValidateTokenAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		await SendAsync("GET", () => "auth/login", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ValidateTokenAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GetGatewayResponse> GetGatewayAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<GetGatewayResponse>("GET", () => "gateway", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGatewayAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GetBotGatewayResponse> GetBotGatewayAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<GetBotGatewayResponse>("GET", () => "gateway/bot", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetBotGatewayAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Channel> GetChannelAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(0uL, channelId, 0uL);
			return await SendAsync<Channel>("GET", () => $"channels/{channelId}", ids, ClientBucketType.Unbucketed, options, "GetChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<Channel> GetChannelAsync(ulong guildId, ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(0uL, channelId, 0uL);
			Channel channel = await SendAsync<Channel>("GET", () => $"channels/{channelId}", ids, ClientBucketType.Unbucketed, options, "GetChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
			if (!channel.GuildId.IsSpecified || channel.GuildId.Value != guildId)
			{
				return null;
			}
			return channel;
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<IReadOnlyCollection<Channel>> GetGuildChannelsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<IReadOnlyCollection<Channel>>("GET", () => $"guilds/{guildId}/channels", ids, ClientBucketType.Unbucketed, options, "GetGuildChannelsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Channel> CreateGuildChannelAsync(ulong guildId, CreateGuildChannelParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.Bitrate, 0, "Bitrate");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		Preconditions.AtMost(args.Name.Length, 100, "Name");
		if (args.Topic.IsSpecified)
		{
			Preconditions.AtMost(args.Topic.Value.Length, 1024, "Name");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<Channel>("POST", () => $"guilds/{guildId}/channels", args, ids, ClientBucketType.Unbucketed, options, "CreateGuildChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Channel> DeleteChannelAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendAsync<Channel>("DELETE", () => $"channels/{channelId}", ids, ClientBucketType.Unbucketed, options, "DeleteChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Channel> ModifyGuildChannelAsync(ulong channelId, ModifyGuildChannelParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Position, 0, "Position");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		if (args.Name.IsSpecified)
		{
			Preconditions.AtMost(args.Name.Value.Length, 100, "Name");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Channel>("PATCH", () => $"channels/{channelId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Channel> ModifyGuildChannelAsync(ulong channelId, ModifyTextChannelParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Position, 0, "Position");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		if (args.Name.IsSpecified)
		{
			Preconditions.AtMost(args.Name.Value.Length, 100, "Name");
		}
		if (args.Topic.IsSpecified)
		{
			Preconditions.AtMost(args.Topic.Value.Length, 1024, "Name");
		}
		Preconditions.AtLeast(args.SlowModeInterval, 0, "SlowModeInterval");
		Preconditions.AtMost(args.SlowModeInterval, 21600, "SlowModeInterval");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Channel>("PATCH", () => $"channels/{channelId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Channel> ModifyGuildChannelAsync(ulong channelId, ModifyVoiceChannelParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Bitrate, 8000, "Bitrate");
		Preconditions.AtLeast(args.UserLimit, 0, "UserLimit");
		Preconditions.AtLeast(args.Position, 0, "Position");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Channel>("PATCH", () => $"channels/{channelId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task ModifyGuildChannelsAsync(ulong guildId, IEnumerable<ModifyGuildChannelsParams> args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		ModifyGuildChannelsParams[] array = args.ToArray();
		switch (array.Length)
		{
		case 0:
			return;
		case 1:
			await ModifyGuildChannelAsync(array[0].Id, new ModifyGuildChannelParams
			{
				Position = array[0].Position
			}).ConfigureAwait(continueOnCapturedContext: false);
			return;
		}
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		await SendJsonAsync("PATCH", () => $"guilds/{guildId}/channels", array, ids, ClientBucketType.Unbucketed, options, "ModifyGuildChannelsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Channel> CreatePostAsync(ulong channelId, CreatePostParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Channel>("POST", () => $"channels/{channelId}/threads", args, ids, ClientBucketType.Unbucketed, options, "CreatePostAsync");
	}

	public async Task<Channel> CreatePostAsync(ulong channelId, CreateMultipartPostAsync args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendMultipartAsync<Channel>("POST", () => $"channels/{channelId}/threads", args.ToDictionary(), ids, ClientBucketType.Unbucketed, options, "CreatePostAsync");
	}

	public async Task<Channel> ModifyThreadAsync(ulong channelId, ModifyThreadParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Channel>("PATCH", () => $"channels/{channelId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyThreadAsync");
	}

	public async Task<Channel> StartThreadAsync(ulong channelId, ulong messageId, StartThreadParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Channel>("POST", () => $"channels/{channelId}/messages/{messageId}/threads", args, ids, ClientBucketType.Unbucketed, options, "StartThreadAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Channel> StartThreadAsync(ulong channelId, StartThreadParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Channel>("POST", () => $"channels/{channelId}/threads", args, ids, ClientBucketType.Unbucketed, options, "StartThreadAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task JoinThreadAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("PUT", () => $"channels/{channelId}/thread-members/@me", ids, ClientBucketType.Unbucketed, options, "JoinThreadAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task AddThreadMemberAsync(ulong channelId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(userId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("PUT", () => $"channels/{channelId}/thread-members/{userId}", ids, ClientBucketType.Unbucketed, options, "AddThreadMemberAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task LeaveThreadAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("DELETE", () => $"channels/{channelId}/thread-members/@me", ids, ClientBucketType.Unbucketed, options, "LeaveThreadAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task RemoveThreadMemberAsync(ulong channelId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(userId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("DELETE", () => $"channels/{channelId}/thread-members/{userId}", ids, ClientBucketType.Unbucketed, options, "RemoveThreadMemberAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ThreadMember[]> ListThreadMembersAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendAsync<ThreadMember[]>("GET", () => $"channels/{channelId}/thread-members", ids, ClientBucketType.Unbucketed, options, "ListThreadMembersAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ThreadMember> GetThreadMemberAsync(ulong channelId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendAsync<ThreadMember>("GET", () => $"channels/{channelId}/thread-members/{userId}", ids, ClientBucketType.Unbucketed, options, "GetThreadMemberAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ChannelThreads> GetActiveThreadsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<ChannelThreads>("GET", () => $"guilds/{guildId}/threads/active", ids, ClientBucketType.Unbucketed, options, "GetActiveThreadsAsync");
	}

	public async Task<ChannelThreads> GetPublicArchivedThreadsAsync(ulong channelId, DateTimeOffset? before = null, int? limit = null, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string query = "";
		if (limit.HasValue)
		{
			query = string.Format("?before={0}&limit={1}", before.GetValueOrDefault(DateTimeOffset.UtcNow).ToString("O"), limit.Value);
		}
		else if (before.HasValue)
		{
			query = "?before=" + before.Value.ToString("O");
		}
		return await SendAsync<ChannelThreads>("GET", () => $"channels/{channelId}/threads/archived/public{query}", ids, ClientBucketType.Unbucketed, options, "GetPublicArchivedThreadsAsync");
	}

	public async Task<ChannelThreads> GetPrivateArchivedThreadsAsync(ulong channelId, DateTimeOffset? before = null, int? limit = null, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string query = "";
		if (limit.HasValue)
		{
			query = string.Format("?before={0}&limit={1}", before.GetValueOrDefault(DateTimeOffset.UtcNow).ToString("O"), limit.Value);
		}
		else if (before.HasValue)
		{
			query = "?before=" + before.Value.ToString("O");
		}
		return await SendAsync<ChannelThreads>("GET", () => $"channels/{channelId}/threads/archived/private{query}", ids, ClientBucketType.Unbucketed, options, "GetPrivateArchivedThreadsAsync");
	}

	public async Task<ChannelThreads> GetJoinedPrivateArchivedThreadsAsync(ulong channelId, DateTimeOffset? before = null, int? limit = null, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string query = "";
		if (limit.HasValue)
		{
			query = $"?before={SnowflakeUtils.ToSnowflake(before.GetValueOrDefault(DateTimeOffset.UtcNow))}&limit={limit.Value}";
		}
		else if (before.HasValue)
		{
			query = "?before=" + before.Value.ToString("O");
		}
		return await SendAsync<ChannelThreads>("GET", () => $"channels/{channelId}/users/@me/threads/archived/private{query}", ids, ClientBucketType.Unbucketed, options, "GetJoinedPrivateArchivedThreadsAsync");
	}

	public async Task<StageInstance> CreateStageInstanceAsync(CreateStageInstanceParams args, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return await SendJsonAsync<StageInstance>("POST", () => "stage-instances", args, ids, ClientBucketType.Unbucketed, options, "CreateStageInstanceAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<StageInstance> ModifyStageInstanceAsync(ulong channelId, ModifyStageInstanceParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<StageInstance>("PATCH", () => $"stage-instances/{channelId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyStageInstanceAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteStageInstanceAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		try
		{
			await SendAsync("DELETE", () => $"stage-instances/{channelId}", ids, ClientBucketType.Unbucketed, options, "DeleteStageInstanceAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
		}
	}

	public async Task<StageInstance> GetStageInstanceAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		try
		{
			return await SendAsync<StageInstance>("POST", () => $"stage-instances/{channelId}", ids, ClientBucketType.Unbucketed, options, "GetStageInstanceAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task ModifyMyVoiceState(ulong guildId, ModifyVoiceStateParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		await SendJsonAsync("PATCH", () => $"guilds/{guildId}/voice-states/@me", args, ids, ClientBucketType.Unbucketed, options, "ModifyMyVoiceState").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task ModifyUserVoiceState(ulong guildId, ulong userId, ModifyVoiceStateParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		await SendJsonAsync("PATCH", () => $"guilds/{guildId}/voice-states/{userId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyUserVoiceState").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task AddRoleAsync(ulong guildId, ulong userId, ulong roleId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotEqual(roleId, 0uL, "roleId");
		Preconditions.NotEqual(roleId, guildId, "roleId", "The Everyone role cannot be added to a user.");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		await SendAsync("PUT", () => $"guilds/{guildId}/members/{userId}/roles/{roleId}", ids, ClientBucketType.Unbucketed, options, "AddRoleAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task RemoveRoleAsync(ulong guildId, ulong userId, ulong roleId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotEqual(roleId, 0uL, "roleId");
		Preconditions.NotEqual(roleId, guildId, "roleId", "The Everyone role cannot be removed from a user.");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		await SendAsync("DELETE", () => $"guilds/{guildId}/members/{userId}/roles/{roleId}", ids, ClientBucketType.Unbucketed, options, "RemoveRoleAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> GetChannelMessageAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(0uL, channelId, 0uL);
			return await SendAsync<Message>("GET", () => $"channels/{channelId}/messages/{messageId}", ids, ClientBucketType.Unbucketed, options, "GetChannelMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<IReadOnlyCollection<Message>> GetChannelMessagesAsync(ulong channelId, GetChannelMessagesParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 100, "Limit");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(100);
		ulong? relativeId = (args.RelativeMessageId.IsSpecified ? new ulong?(args.RelativeMessageId.Value) : ((ulong?)null));
		string relativeDir = args.RelativeDirection.GetValueOrDefault(Direction.Before) switch
		{
			Direction.After => "after", 
			Direction.Around => "around", 
			_ => "before", 
		};
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		Expression<Func<string>> endpointExpr = ((!relativeId.HasValue) ? ((Expression<Func<string>>)(() => $"channels/{channelId}/messages?limit={limit}")) : ((Expression<Func<string>>)(() => $"channels/{channelId}/messages?limit={limit}&{relativeDir}={relativeId}")));
		return await SendAsync<IReadOnlyCollection<Message>>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetChannelMessagesAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> CreateMessageAsync(ulong channelId, CreateMessageParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		if ((!args.Embeds.IsSpecified || args.Embeds.Value == null || args.Embeds.Value.Length == 0) && (!args.Stickers.IsSpecified || args.Stickers.Value == null || args.Stickers.Value.Length == 0))
		{
			Preconditions.NotNullOrEmpty(args.Content, "Content");
		}
		string content = args.Content;
		if (content != null && content.Length > 2000)
		{
			throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Message>("POST", () => $"channels/{channelId}/messages", args, ids, ClientBucketType.SendEdit, options, "CreateMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> CreateWebhookMessageAsync(ulong webhookId, CreateWebhookMessageParams args, RequestOptions options = null, ulong? threadId = null)
	{
		if (AuthTokenType != TokenType.Webhook)
		{
			throw new InvalidOperationException("This operation may only be called with a Webhook token.");
		}
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		if (!args.Embeds.IsSpecified || args.Embeds.Value == null || args.Embeds.Value.Length == 0)
		{
			Preconditions.NotNullOrEmpty(args.Content, "Content");
		}
		if (args.Content.IsSpecified)
		{
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, webhookId);
		return await SendJsonAsync<Message>("POST", () => $"webhooks/{webhookId}/{AuthToken}?{WebhookQuery(wait: true, threadId)}", args, ids, ClientBucketType.SendEdit, options, "CreateWebhookMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task ModifyWebhookMessageAsync(ulong webhookId, ulong messageId, ModifyWebhookMessageParams args, RequestOptions options = null, ulong? threadId = null)
	{
		if (AuthTokenType != TokenType.Webhook)
		{
			throw new InvalidOperationException("This operation may only be called with a Webhook token.");
		}
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		if (args.Embeds.IsSpecified)
		{
			Preconditions.AtMost(args.Embeds.Value.Length, 10, "Embeds", "A max of 10 Embeds are allowed.");
		}
		if (args.Content.IsSpecified && args.Content.Value.Length > 2000)
		{
			throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, webhookId);
		await SendJsonAsync<Message>("PATCH", () => $"webhooks/{webhookId}/{AuthToken}/messages/{messageId}?{WebhookQuery(wait: false, threadId)}", args, ids, ClientBucketType.SendEdit, options, "ModifyWebhookMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteWebhookMessageAsync(ulong webhookId, ulong messageId, RequestOptions options = null, ulong? threadId = null)
	{
		if (AuthTokenType != TokenType.Webhook)
		{
			throw new InvalidOperationException("This operation may only be called with a Webhook token.");
		}
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, webhookId);
		await SendAsync("DELETE", () => $"webhooks/{webhookId}/{AuthToken}/messages/{messageId}?{WebhookQuery(wait: false, threadId)}", ids, ClientBucketType.Unbucketed, options, "DeleteWebhookMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> UploadFileAsync(ulong channelId, UploadFileParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		if (args.Content.GetValueOrDefault(null) == null)
		{
			args.Content = "";
		}
		else if (args.Content.IsSpecified)
		{
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentOutOfRangeException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendMultipartAsync<Message>("POST", () => $"channels/{channelId}/messages", args.ToDictionary(), ids, ClientBucketType.SendEdit, options, "UploadFileAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> UploadWebhookFileAsync(ulong webhookId, UploadWebhookFileParams args, RequestOptions options = null, ulong? threadId = null)
	{
		if (AuthTokenType != TokenType.Webhook)
		{
			throw new InvalidOperationException("This operation may only be called with a Webhook token.");
		}
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		options = RequestOptions.CreateOrClone(options);
		if (args.Content.GetValueOrDefault(null) == null)
		{
			args.Content = "";
		}
		else if (args.Content.IsSpecified)
		{
			if (args.Content.Value == null)
			{
				args.Content = "";
			}
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentOutOfRangeException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		BucketIds ids = new BucketIds(0uL, 0uL, webhookId);
		return await SendMultipartAsync<Message>("POST", () => $"webhooks/{webhookId}/{AuthToken}?{WebhookQuery(wait: true, threadId)}", args.ToDictionary(), ids, ClientBucketType.SendEdit, options, "UploadWebhookFileAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteMessageAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("DELETE", () => $"channels/{channelId}/messages/{messageId}", ids, ClientBucketType.Unbucketed, options, "DeleteMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteMessagesAsync(ulong channelId, DeleteMessagesParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotNull(args.MessageIds, "MessageIds");
		Preconditions.AtMost(args.MessageIds.Length, 100, "Length");
		Preconditions.YoungerThanTwoWeeks(args.MessageIds, "MessageIds");
		options = RequestOptions.CreateOrClone(options);
		switch (args.MessageIds.Length)
		{
		case 0:
			return;
		case 1:
			await DeleteMessageAsync(channelId, args.MessageIds[0]).ConfigureAwait(continueOnCapturedContext: false);
			return;
		}
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendJsonAsync("POST", () => $"channels/{channelId}/messages/bulk-delete", args, ids, ClientBucketType.Unbucketed, options, "DeleteMessagesAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> ModifyMessageAsync(ulong channelId, ulong messageId, ModifyMessageParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNull(args, "args");
		if (args.Content.IsSpecified)
		{
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentOutOfRangeException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Message>("PATCH", () => $"channels/{channelId}/messages/{messageId}", args, ids, ClientBucketType.SendEdit, options, "ModifyMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> ModifyMessageAsync(ulong channelId, ulong messageId, UploadFileParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNull(args, "args");
		if (args.Content.IsSpecified)
		{
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentOutOfRangeException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendMultipartAsync<Message>("PATCH", () => $"channels/{channelId}/messages/{messageId}", args.ToDictionary(), ids, ClientBucketType.SendEdit, options, "ModifyMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Sticker> GetStickerAsync(ulong id, RequestOptions options = null)
	{
		Preconditions.NotEqual(id, 0uL, "id");
		options = RequestOptions.CreateOrClone(options);
		return await NullifyNotFound(SendAsync<Sticker>("GET", () => $"stickers/{id}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetStickerAsync")).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Sticker> GetGuildStickerAsync(ulong guildId, ulong id, RequestOptions options = null)
	{
		Preconditions.NotEqual(id, 0uL, "id");
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return await NullifyNotFound(SendAsync<Sticker>("GET", () => $"guilds/{guildId}/stickers/{id}", new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGuildStickerAsync")).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Sticker[]> ListGuildStickersAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<Sticker[]>("GET", () => $"guilds/{guildId}/stickers", new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ListGuildStickersAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<NitroStickerPacks> ListNitroStickerPacksAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<NitroStickerPacks>("GET", () => "sticker-packs", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ListNitroStickerPacksAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Sticker> CreateGuildStickerAsync(CreateStickerParams args, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return await SendMultipartAsync<Sticker>("POST", () => $"guilds/{guildId}/stickers", args.ToDictionary(), new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateGuildStickerAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Sticker> ModifyStickerAsync(ModifyStickerParams args, ulong guildId, ulong stickerId, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(stickerId, 0uL, "stickerId");
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<Sticker>("PATCH", () => $"guilds/{guildId}/stickers/{stickerId}", args, new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyStickerAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteStickerAsync(ulong guildId, ulong stickerId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(stickerId, 0uL, "stickerId");
		options = RequestOptions.CreateOrClone(options);
		await SendAsync("DELETE", () => $"guilds/{guildId}/stickers/{stickerId}", new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteStickerAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task AddReactionAsync(ulong channelId, ulong messageId, string emoji, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNullOrWhitespace(emoji, "emoji");
		options = RequestOptions.CreateOrClone(options);
		options.IsReactionBucket = true;
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string me = "@me";
		await SendAsync("PUT", () => $"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{me}", ids, ClientBucketType.Unbucketed, options, "AddReactionAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task RemoveReactionAsync(ulong channelId, ulong messageId, ulong userId, string emoji, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNullOrWhitespace(emoji, "emoji");
		options = RequestOptions.CreateOrClone(options);
		options.IsReactionBucket = true;
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string user = ((!CurrentUserId.HasValue) ? userId.ToString() : ((userId == CurrentUserId.Value) ? "@me" : userId.ToString()));
		await SendAsync("DELETE", () => $"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{user}", ids, ClientBucketType.Unbucketed, options, "RemoveReactionAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task RemoveAllReactionsAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("DELETE", () => $"channels/{channelId}/messages/{messageId}/reactions", ids, ClientBucketType.Unbucketed, options, "RemoveAllReactionsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task RemoveAllReactionsForEmoteAsync(ulong channelId, ulong messageId, string emoji, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNullOrWhitespace(emoji, "emoji");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("DELETE", () => $"channels/{channelId}/messages/{messageId}/reactions/{emoji}", ids, ClientBucketType.Unbucketed, options, "RemoveAllReactionsForEmoteAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<User>> GetReactionUsersAsync(ulong channelId, ulong messageId, string emoji, GetReactionUsersParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNullOrWhitespace(emoji, "emoji");
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 100, "Limit");
		Preconditions.GreaterThan(args.AfterUserId, 0uL, "AfterUserId");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(100);
		ulong afterUserId = args.AfterUserId.GetValueOrDefault(0uL);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		Expression<Func<string>> endpointExpr = () => $"channels/{channelId}/messages/{messageId}/reactions/{emoji}?limit={limit}&after={afterUserId}";
		return await SendAsync<IReadOnlyCollection<User>>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetReactionUsersAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task AckMessageAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("POST", () => $"channels/{channelId}/messages/{messageId}/ack", ids, ClientBucketType.Unbucketed, options, "AckMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task TriggerTypingIndicatorAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("POST", () => $"channels/{channelId}/typing", ids, ClientBucketType.Unbucketed, options, "TriggerTypingIndicatorAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task CrosspostAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("POST", () => $"channels/{channelId}/messages/{messageId}/crosspost", ids, ClientBucketType.Unbucketed, options, "CrosspostAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task ModifyChannelPermissionsAsync(ulong channelId, ulong targetId, ModifyChannelPermissionsParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(targetId, 0uL, "targetId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendJsonAsync("PUT", () => $"channels/{channelId}/permissions/{targetId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyChannelPermissionsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteChannelPermissionAsync(ulong channelId, ulong targetId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(targetId, 0uL, "targetId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("DELETE", () => $"channels/{channelId}/permissions/{targetId}", ids, ClientBucketType.Unbucketed, options, "DeleteChannelPermissionAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task AddPinAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.GreaterThan(channelId, 0uL, "channelId");
		Preconditions.GreaterThan(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("PUT", () => $"channels/{channelId}/pins/{messageId}", ids, ClientBucketType.Unbucketed, options, "AddPinAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task RemovePinAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("DELETE", () => $"channels/{channelId}/pins/{messageId}", ids, ClientBucketType.Unbucketed, options, "RemovePinAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<Message>> GetPinsAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendAsync<IReadOnlyCollection<Message>>("GET", () => $"channels/{channelId}/pins", ids, ClientBucketType.Unbucketed, options, "GetPinsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task AddGroupRecipientAsync(ulong channelId, ulong userId, RequestOptions options = null)
	{
		Preconditions.GreaterThan(channelId, 0uL, "channelId");
		Preconditions.GreaterThan(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("PUT", () => $"channels/{channelId}/recipients/{userId}", ids, ClientBucketType.Unbucketed, options, "AddGroupRecipientAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task RemoveGroupRecipientAsync(ulong channelId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		await SendAsync("DELETE", () => $"channels/{channelId}/recipients/{userId}", ids, ClientBucketType.Unbucketed, options, "RemoveGroupRecipientAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ApplicationCommand[]> GetGlobalApplicationCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		if (locale != null)
		{
			if (!Regex.IsMatch(locale, "^\\w{2}(?:-\\w{2})?$"))
			{
				throw new ArgumentException(locale + " is not a valid locale.", "locale");
			}
			options.RequestHeaders["X-Discord-Locale"] = new string[1] { locale };
		}
		string query = (withLocalizations ? "?with_localizations=true" : string.Empty);
		return await SendAsync<ApplicationCommand[]>("GET", () => $"applications/{CurrentApplicationId}/commands{query}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGlobalApplicationCommandsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ApplicationCommand> GetGlobalApplicationCommandAsync(ulong id, RequestOptions options = null)
	{
		Preconditions.NotEqual(id, 0uL, "id");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			return await SendAsync<ApplicationCommand>("GET", () => $"applications/{CurrentApplicationId}/commands/{id}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGlobalApplicationCommandAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<ApplicationCommand> CreateGlobalApplicationCommandAsync(CreateApplicationCommandParams command, RequestOptions options = null)
	{
		Preconditions.NotNull(command, "command");
		Preconditions.AtMost(command.Name.Length, 32, "Name");
		Preconditions.AtLeast(command.Name.Length, 1, "Name");
		if (command.Type == ApplicationCommandType.Slash)
		{
			Preconditions.NotNullOrEmpty(command.Description, "Description");
			Preconditions.AtMost(command.Description.Length, 100, "Description");
			Preconditions.AtLeast(command.Description.Length, 1, "Description");
		}
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<ApplicationCommand>("POST", () => $"applications/{CurrentApplicationId}/commands", command, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateGlobalApplicationCommandAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ApplicationCommand> ModifyGlobalApplicationCommandAsync(ModifyApplicationCommandParams command, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<ApplicationCommand>("PATCH", () => $"applications/{CurrentApplicationId}/commands/{commandId}", command, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyGlobalApplicationCommandAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ApplicationCommand> ModifyGlobalApplicationUserCommandAsync(ModifyApplicationCommandParams command, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<ApplicationCommand>("PATCH", () => $"applications/{CurrentApplicationId}/commands/{commandId}", command, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyGlobalApplicationUserCommandAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ApplicationCommand> ModifyGlobalApplicationMessageCommandAsync(ModifyApplicationCommandParams command, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<ApplicationCommand>("PATCH", () => $"applications/{CurrentApplicationId}/commands/{commandId}", command, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyGlobalApplicationMessageCommandAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteGlobalApplicationCommandAsync(ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		await SendAsync("DELETE", () => $"applications/{CurrentApplicationId}/commands/{commandId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteGlobalApplicationCommandAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ApplicationCommand[]> BulkOverwriteGlobalApplicationCommandsAsync(CreateApplicationCommandParams[] commands, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<ApplicationCommand[]>("PUT", () => $"applications/{CurrentApplicationId}/commands", commands, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "BulkOverwriteGlobalApplicationCommandsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ApplicationCommand[]> GetGuildApplicationCommandsAsync(ulong guildId, bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		if (locale != null)
		{
			if (!Regex.IsMatch(locale, "^\\w{2}(?:-\\w{2})?$"))
			{
				throw new ArgumentException(locale + " is not a valid locale.", "locale");
			}
			options.RequestHeaders["X-Discord-Locale"] = new string[1] { locale };
		}
		string query = (withLocalizations ? "?with_localizations=true" : string.Empty);
		return await SendAsync<ApplicationCommand[]>("GET", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands{query}", ids, ClientBucketType.Unbucketed, options, "GetGuildApplicationCommandsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ApplicationCommand> GetGuildApplicationCommandAsync(ulong guildId, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		try
		{
			return await SendAsync<ApplicationCommand>("GET", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/{commandId}", ids, ClientBucketType.Unbucketed, options, "GetGuildApplicationCommandAsync");
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<ApplicationCommand> CreateGuildApplicationCommandAsync(CreateApplicationCommandParams command, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotNull(command, "command");
		Preconditions.AtMost(command.Name.Length, 32, "Name");
		Preconditions.AtLeast(command.Name.Length, 1, "Name");
		if (command.Type == ApplicationCommandType.Slash)
		{
			Preconditions.NotNullOrEmpty(command.Description, "Description");
			Preconditions.AtMost(command.Description.Length, 100, "Description");
			Preconditions.AtLeast(command.Description.Length, 1, "Description");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<ApplicationCommand>("POST", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands", command, ids, ClientBucketType.Unbucketed, options, "CreateGuildApplicationCommandAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ApplicationCommand> ModifyGuildApplicationCommandAsync(ModifyApplicationCommandParams command, ulong guildId, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<ApplicationCommand>("PATCH", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/{commandId}", command, ids, ClientBucketType.Unbucketed, options, "ModifyGuildApplicationCommandAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteGuildApplicationCommandAsync(ulong guildId, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		await SendAsync<ApplicationCommand>("DELETE", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/{commandId}", ids, ClientBucketType.Unbucketed, options, "DeleteGuildApplicationCommandAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ApplicationCommand[]> BulkOverwriteGuildApplicationCommandsAsync(ulong guildId, CreateApplicationCommandParams[] commands, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<ApplicationCommand[]>("PUT", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands", commands, ids, ClientBucketType.Unbucketed, options, "BulkOverwriteGuildApplicationCommandsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task CreateInteractionResponseAsync(InteractionResponse response, ulong interactionId, string interactionToken, RequestOptions options = null)
	{
		if (response.Data.IsSpecified && response.Data.Value.Content.IsSpecified)
		{
			Preconditions.AtMost(response.Data.Value.Content.Value?.Length ?? 0, 2000, "Content");
		}
		options = RequestOptions.CreateOrClone(options);
		await SendJsonAsync("POST", () => $"interactions/{interactionId}/{interactionToken}/callback", response, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateInteractionResponseAsync");
	}

	public async Task CreateInteractionResponseAsync(UploadInteractionFileParams response, ulong interactionId, string interactionToken, RequestOptions options = null)
	{
		if ((!response.Embeds.IsSpecified || response.Embeds.Value == null || response.Embeds.Value.Length == 0) && !response.Files.Any())
		{
			Preconditions.NotNullOrEmpty(response.Content, "Content");
		}
		if (response.Content.IsSpecified && response.Content.Value.Length > 2000)
		{
			throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		await SendMultipartAsync("POST", () => $"interactions/{interactionId}/{interactionToken}/callback", response.ToDictionary(), ids, ClientBucketType.SendEdit, options, "CreateInteractionResponseAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> GetInteractionResponseAsync(string interactionToken, RequestOptions options = null)
	{
		Preconditions.NotNullOrEmpty(interactionToken, "interactionToken");
		options = RequestOptions.CreateOrClone(options);
		return await NullifyNotFound(SendAsync<Message>("GET", () => $"webhooks/{CurrentApplicationId}/{interactionToken}/messages/@original", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetInteractionResponseAsync")).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> ModifyInteractionResponseAsync(ModifyInteractionResponseParams args, string interactionToken, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<Message>("PATCH", () => $"webhooks/{CurrentApplicationId}/{interactionToken}/messages/@original", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyInteractionResponseAsync");
	}

	public async Task<Message> ModifyInteractionResponseAsync(UploadWebhookFileParams args, string interactionToken, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendMultipartAsync<Message>("PATCH", () => $"webhooks/{CurrentApplicationId}/{interactionToken}/messages/@original", args.ToDictionary(), new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyInteractionResponseAsync");
	}

	public async Task DeleteInteractionResponseAsync(string interactionToken, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		await SendAsync("DELETE", () => $"webhooks/{CurrentApplicationId}/{interactionToken}/messages/@original", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteInteractionResponseAsync");
	}

	public async Task<Message> CreateInteractionFollowupMessageAsync(CreateWebhookMessageParams args, string token, RequestOptions options = null)
	{
		if ((!args.Embeds.IsSpecified || args.Embeds.Value == null || args.Embeds.Value.Length == 0) && !args.File.IsSpecified)
		{
			Preconditions.NotNullOrEmpty(args.Content, "Content");
		}
		if (args.Content.IsSpecified)
		{
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		if (!args.File.IsSpecified)
		{
			return await SendJsonAsync<Message>("POST", () => $"webhooks/{CurrentApplicationId}/{token}?wait=true", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateInteractionFollowupMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		return await SendMultipartAsync<Message>("POST", () => $"webhooks/{CurrentApplicationId}/{token}?wait=true", args.ToDictionary(), new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateInteractionFollowupMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> CreateInteractionFollowupMessageAsync(UploadWebhookFileParams args, string token, RequestOptions options = null)
	{
		if ((!args.Embeds.IsSpecified || args.Embeds.Value == null || args.Embeds.Value.Length == 0) && !args.Files.Any())
		{
			Preconditions.NotNullOrEmpty(args.Content, "Content");
		}
		if (args.Content.IsSpecified)
		{
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return await SendMultipartAsync<Message>("POST", () => $"webhooks/{CurrentApplicationId}/{token}?wait=true", args.ToDictionary(), ids, ClientBucketType.SendEdit, options, "CreateInteractionFollowupMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Message> ModifyInteractionFollowupMessageAsync(ModifyInteractionResponseParams args, ulong id, string token, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(id, 0uL, "id");
		if (args.Content.IsSpecified)
		{
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<Message>("PATCH", () => $"webhooks/{CurrentApplicationId}/{token}/messages/{id}", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyInteractionFollowupMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteInteractionFollowupMessageAsync(ulong id, string token, RequestOptions options = null)
	{
		Preconditions.NotEqual(id, 0uL, "id");
		options = RequestOptions.CreateOrClone(options);
		await SendAsync("DELETE", () => $"webhooks/{CurrentApplicationId}/{token}/messages/{id}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteInteractionFollowupMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildApplicationCommandPermission[]> GetGuildApplicationCommandPermissionsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<GuildApplicationCommandPermission[]>("GET", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/permissions", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGuildApplicationCommandPermissionsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildApplicationCommandPermission> GetGuildApplicationCommandPermissionAsync(ulong guildId, ulong commandId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(commandId, 0uL, "commandId");
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<GuildApplicationCommandPermission>("GET", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/{commandId}/permissions", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGuildApplicationCommandPermissionAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildApplicationCommandPermission> ModifyApplicationCommandPermissionsAsync(ModifyGuildApplicationCommandPermissionsParams permissions, ulong guildId, ulong commandId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(commandId, 0uL, "commandId");
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<GuildApplicationCommandPermission>("PUT", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/{commandId}/permissions", permissions, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyApplicationCommandPermissionsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<GuildApplicationCommandPermission>> BatchModifyApplicationCommandPermissionsAsync(ModifyGuildApplicationCommandPermissions[] permissions, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(permissions, "permissions");
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<GuildApplicationCommandPermission[]>("PUT", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/permissions", permissions, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "BatchModifyApplicationCommandPermissionsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Guild> GetGuildAsync(ulong guildId, bool withCounts, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			return await SendAsync<Guild>("GET", () => string.Format("guilds/{0}?with_counts={1}", guildId, withCounts ? "true" : "false"), ids, ClientBucketType.Unbucketed, options, "GetGuildAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<Guild> CreateGuildAsync(CreateGuildParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		Preconditions.NotNullOrWhitespace(args.RegionId, "RegionId");
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<Guild>("POST", () => "guilds", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateGuildAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Guild> DeleteGuildAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<Guild>("DELETE", () => $"guilds/{guildId}", ids, ClientBucketType.Unbucketed, options, "DeleteGuildAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Guild> LeaveGuildAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<Guild>("DELETE", () => $"users/@me/guilds/{guildId}", ids, ClientBucketType.Unbucketed, options, "LeaveGuildAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Guild> ModifyGuildAsync(ulong guildId, ModifyGuildParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(args.AfkChannelId, 0uL, "AfkChannelId");
		Preconditions.AtLeast(args.AfkTimeout, 0, "AfkTimeout");
		Preconditions.NotNullOrEmpty(args.Name, "Name");
		Preconditions.GreaterThan(args.OwnerId, 0uL, "OwnerId");
		Preconditions.NotNull(args.RegionId, "RegionId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<Guild>("PATCH", () => $"guilds/{guildId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GetGuildPruneCountResponse> BeginGuildPruneAsync(ulong guildId, GuildPruneParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Days, 1, "Days");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<GetGuildPruneCountResponse>("POST", () => $"guilds/{guildId}/prune", args, ids, ClientBucketType.Unbucketed, options, "BeginGuildPruneAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GetGuildPruneCountResponse> GetGuildPruneCountAsync(ulong guildId, GuildPruneParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Days, 1, "Days");
		ulong[] includeRoleIds = args.IncludeRoleIds;
		string endpointRoleIds = ((includeRoleIds != null && includeRoleIds.Length != 0) ? ("&include_roles=" + string.Join(",", args.IncludeRoleIds)) : "");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<GetGuildPruneCountResponse>("GET", () => $"guilds/{guildId}/prune?days={args.Days}{endpointRoleIds}", ids, ClientBucketType.Unbucketed, options, "GetGuildPruneCountAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<Ban>> GetGuildBansAsync(ulong guildId, GetGuildBansParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 1000, "Limit");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(1000);
		ulong? relativeId = (args.RelativeUserId.IsSpecified ? new ulong?(args.RelativeUserId.Value) : ((ulong?)null));
		string relativeDir = args.RelativeDirection.GetValueOrDefault(Direction.Before) switch
		{
			Direction.After => "after", 
			Direction.Around => "around", 
			_ => "before", 
		};
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		Expression<Func<string>> endpointExpr = ((!relativeId.HasValue) ? ((Expression<Func<string>>)(() => $"guilds/{guildId}/bans?limit={limit}")) : ((Expression<Func<string>>)(() => $"guilds/{guildId}/bans?limit={limit}&{relativeDir}={relativeId}")));
		return await SendAsync<IReadOnlyCollection<Ban>>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetGuildBansAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Ban> GetGuildBanAsync(ulong guildId, ulong userId, RequestOptions options)
	{
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			return await SendAsync<Ban>("GET", () => $"guilds/{guildId}/bans/{userId}", ids, ClientBucketType.Unbucketed, options, "GetGuildBanAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task CreateGuildBanAsync(ulong guildId, ulong userId, CreateGuildBanParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.DeleteMessageDays, 0, "DeleteMessageDays", "Prune length must be within [0, 7]");
		Preconditions.AtMost(args.DeleteMessageDays, 7, "DeleteMessageDays", "Prune length must be within [0, 7]");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		string reason = (string.IsNullOrWhiteSpace(args.Reason) ? "" : ("&reason=" + Uri.EscapeDataString(args.Reason)));
		await SendAsync("PUT", () => $"guilds/{guildId}/bans/{userId}?delete_message_days={args.DeleteMessageDays}{reason}", ids, ClientBucketType.Unbucketed, options, "CreateGuildBanAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task RemoveGuildBanAsync(ulong guildId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		await SendAsync("DELETE", () => $"guilds/{guildId}/bans/{userId}", ids, ClientBucketType.Unbucketed, options, "RemoveGuildBanAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildWidget> GetGuildWidgetAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			return await SendAsync<GuildWidget>("GET", () => $"guilds/{guildId}/widget", ids, ClientBucketType.Unbucketed, options, "GetGuildWidgetAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<GuildWidget> ModifyGuildWidgetAsync(ulong guildId, ModifyGuildWidgetParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<GuildWidget>("PATCH", () => $"guilds/{guildId}/widget", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildWidgetAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<Integration>> GetIntegrationsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<IReadOnlyCollection<Integration>>("GET", () => $"guilds/{guildId}/integrations", ids, ClientBucketType.Unbucketed, options, "GetIntegrationsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteIntegrationAsync(ulong guildId, ulong integrationId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(integrationId, 0uL, "integrationId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		await SendAsync("DELETE", () => $"guilds/{guildId}/integrations/{integrationId}", ids, ClientBucketType.Unbucketed, options, "DeleteIntegrationAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<InviteMetadata> GetInviteAsync(string inviteId, RequestOptions options = null)
	{
		Preconditions.NotNullOrEmpty(inviteId, "inviteId");
		options = RequestOptions.CreateOrClone(options);
		if (inviteId[inviteId.Length - 1] == '/')
		{
			inviteId = inviteId.Substring(0, inviteId.Length - 1);
		}
		int num = inviteId.LastIndexOf('/');
		if (num >= 0)
		{
			inviteId = inviteId.Substring(num + 1);
		}
		try
		{
			return await SendAsync<InviteMetadata>("GET", () => $"invites/{inviteId}?with_counts=true", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetInviteAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<InviteVanity> GetVanityInviteAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<InviteVanity>("GET", () => $"guilds/{guildId}/vanity-url", ids, ClientBucketType.Unbucketed, options, "GetVanityInviteAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<InviteMetadata>> GetGuildInvitesAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<IReadOnlyCollection<InviteMetadata>>("GET", () => $"guilds/{guildId}/invites", ids, ClientBucketType.Unbucketed, options, "GetGuildInvitesAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<InviteMetadata>> GetChannelInvitesAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendAsync<IReadOnlyCollection<InviteMetadata>>("GET", () => $"channels/{channelId}/invites", ids, ClientBucketType.Unbucketed, options, "GetChannelInvitesAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<InviteMetadata> CreateChannelInviteAsync(ulong channelId, CreateChannelInviteParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.MaxAge, 0, "MaxAge");
		Preconditions.AtLeast(args.MaxUses, 0, "MaxUses");
		Preconditions.AtMost(args.MaxAge, 86400, "MaxAge", "The maximum age of an invite must be less than or equal to a day (86400 seconds).");
		if (args.TargetType.IsSpecified)
		{
			Preconditions.NotEqual((int)args.TargetType.Value, 0, "TargetType");
			if (args.TargetType.Value == TargetUserType.Stream)
			{
				Preconditions.GreaterThan(args.TargetUserId, 0uL, "TargetUserId");
			}
			if (args.TargetType.Value == TargetUserType.EmbeddedApplication)
			{
				Preconditions.GreaterThan(args.TargetApplicationId, 0uL, "TargetApplicationId");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<InviteMetadata>("POST", () => $"channels/{channelId}/invites", args, ids, ClientBucketType.Unbucketed, options, "CreateChannelInviteAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Invite> DeleteInviteAsync(string inviteId, RequestOptions options = null)
	{
		Preconditions.NotNullOrEmpty(inviteId, "inviteId");
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<Invite>("DELETE", () => $"invites/{inviteId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteInviteAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildMember> AddGuildMemberAsync(ulong guildId, ulong userId, AddGuildMemberParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrWhitespace(args.AccessToken, "AccessToken");
		if (args.RoleIds.IsSpecified)
		{
			ulong[] value = args.RoleIds.Value;
			for (int i = 0; i < value.Length; i++)
			{
				Preconditions.NotEqual(value[i], 0uL, "roleId");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<GuildMember>("PUT", () => $"guilds/{guildId}/members/{userId}", args, ids, ClientBucketType.Unbucketed, options, "AddGuildMemberAsync");
	}

	public async Task<GuildMember> GetGuildMemberAsync(ulong guildId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			return await SendAsync<GuildMember>("GET", () => $"guilds/{guildId}/members/{userId}", ids, ClientBucketType.Unbucketed, options, "GetGuildMemberAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<IReadOnlyCollection<GuildMember>> GetGuildMembersAsync(ulong guildId, GetGuildMembersParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 1000, "Limit");
		Preconditions.GreaterThan(args.AfterUserId, 0uL, "AfterUserId");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(int.MaxValue);
		ulong afterUserId = args.AfterUserId.GetValueOrDefault(0uL);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		Expression<Func<string>> endpointExpr = () => $"guilds/{guildId}/members?limit={limit}&after={afterUserId}";
		return await SendAsync<IReadOnlyCollection<GuildMember>>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetGuildMembersAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task RemoveGuildMemberAsync(ulong guildId, ulong userId, string reason, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		reason = (string.IsNullOrWhiteSpace(reason) ? "" : ("?reason=" + Uri.EscapeDataString(reason)));
		await SendAsync("DELETE", () => $"guilds/{guildId}/members/{userId}{reason}", ids, ClientBucketType.Unbucketed, options, "RemoveGuildMemberAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task ModifyGuildMemberAsync(ulong guildId, ulong userId, ModifyGuildMemberParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		bool isCurrentUser = userId == CurrentUserId;
		if (args.RoleIds.IsSpecified)
		{
			Preconditions.NotEveryoneRole(args.RoleIds.Value, guildId, "RoleIds");
		}
		if (isCurrentUser && args.Nickname.IsSpecified)
		{
			ModifyCurrentUserNickParams args2 = new ModifyCurrentUserNickParams(args.Nickname.Value ?? "");
			await ModifyMyNickAsync(guildId, args2).ConfigureAwait(continueOnCapturedContext: false);
			args.Nickname = Optional.Create<string>();
		}
		if (!isCurrentUser || args.Deaf.IsSpecified || args.Mute.IsSpecified || args.RoleIds.IsSpecified)
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			await SendJsonAsync("PATCH", () => $"guilds/{guildId}/members/{userId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildMemberAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public async Task<IReadOnlyCollection<GuildMember>> SearchGuildMembersAsync(ulong guildId, SearchGuildMembersParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 1000, "Limit");
		Preconditions.NotNullOrEmpty(args.Query, "Query");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(1000);
		string query = args.Query;
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		Expression<Func<string>> endpointExpr = () => $"guilds/{guildId}/members/search?limit={limit}&query={query}";
		return await SendAsync<IReadOnlyCollection<GuildMember>>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "SearchGuildMembersAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<Role>> GetGuildRolesAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<IReadOnlyCollection<Role>>("GET", () => $"guilds/{guildId}/roles", ids, ClientBucketType.Unbucketed, options, "GetGuildRolesAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Role> CreateGuildRoleAsync(ulong guildId, ModifyGuildRoleParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<Role>("POST", () => $"guilds/{guildId}/roles", args, ids, ClientBucketType.Unbucketed, options, "CreateGuildRoleAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteGuildRoleAsync(ulong guildId, ulong roleId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(roleId, 0uL, "roleId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		await SendAsync("DELETE", () => $"guilds/{guildId}/roles/{roleId}", ids, ClientBucketType.Unbucketed, options, "DeleteGuildRoleAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Role> ModifyGuildRoleAsync(ulong guildId, ulong roleId, ModifyGuildRoleParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(roleId, 0uL, "roleId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Color, 0u, "Color");
		Preconditions.NotNullOrEmpty(args.Name, "Name");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<Role>("PATCH", () => $"guilds/{guildId}/roles/{roleId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildRoleAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<Role>> ModifyGuildRolesAsync(ulong guildId, IEnumerable<ModifyGuildRolesParams> args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<IReadOnlyCollection<Role>>("PATCH", () => $"guilds/{guildId}/roles", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildRolesAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<Emoji>> GetGuildEmotesAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<IReadOnlyCollection<Emoji>>("GET", () => $"guilds/{guildId}/emojis", ids, ClientBucketType.Unbucketed, options, "GetGuildEmotesAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Emoji> GetGuildEmoteAsync(ulong guildId, ulong emoteId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(emoteId, 0uL, "emoteId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<Emoji>("GET", () => $"guilds/{guildId}/emojis/{emoteId}", ids, ClientBucketType.Unbucketed, options, "GetGuildEmoteAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Emoji> CreateGuildEmoteAsync(ulong guildId, CreateGuildEmoteParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		Preconditions.NotNull(args.Image.Stream, "Image");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<Emoji>("POST", () => $"guilds/{guildId}/emojis", args, ids, ClientBucketType.Unbucketed, options, "CreateGuildEmoteAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Emoji> ModifyGuildEmoteAsync(ulong guildId, ulong emoteId, ModifyGuildEmoteParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(emoteId, 0uL, "emoteId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<Emoji>("PATCH", () => $"guilds/{guildId}/emojis/{emoteId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildEmoteAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteGuildEmoteAsync(ulong guildId, ulong emoteId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(emoteId, 0uL, "emoteId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		await SendAsync("DELETE", () => $"guilds/{guildId}/emojis/{emoteId}", ids, ClientBucketType.Unbucketed, options, "DeleteGuildEmoteAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildScheduledEvent[]> ListGuildScheduledEventsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<GuildScheduledEvent[]>("GET", () => $"guilds/{guildId}/scheduled-events?with_user_count=true", ids, ClientBucketType.Unbucketed, options, "ListGuildScheduledEventsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildScheduledEvent> GetGuildScheduledEventAsync(ulong eventId, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(eventId, 0uL, "eventId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await NullifyNotFound(SendAsync<GuildScheduledEvent>("GET", () => $"guilds/{guildId}/scheduled-events/{eventId}?with_user_count=true", ids, ClientBucketType.Unbucketed, options, "GetGuildScheduledEventAsync")).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildScheduledEvent> CreateGuildScheduledEventAsync(CreateGuildScheduledEventParams args, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<GuildScheduledEvent>("POST", () => $"guilds/{guildId}/scheduled-events", args, ids, ClientBucketType.Unbucketed, options, "CreateGuildScheduledEventAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildScheduledEvent> ModifyGuildScheduledEventAsync(ModifyGuildScheduledEventParams args, ulong eventId, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(eventId, 0uL, "eventId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<GuildScheduledEvent>("PATCH", () => $"guilds/{guildId}/scheduled-events/{eventId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildScheduledEventAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteGuildScheduledEventAsync(ulong eventId, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(eventId, 0uL, "eventId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		await SendAsync("DELETE", () => $"guilds/{guildId}/scheduled-events/{eventId}", ids, ClientBucketType.Unbucketed, options, "DeleteGuildScheduledEventAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildScheduledEventUser[]> GetGuildScheduledEventUsersAsync(ulong eventId, ulong guildId, int limit = 100, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(eventId, 0uL, "eventId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<GuildScheduledEventUser[]>("GET", () => $"guilds/{guildId}/scheduled-events/{eventId}/users?limit={limit}&with_member=true", ids, ClientBucketType.Unbucketed, options, "GetGuildScheduledEventUsersAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<GuildScheduledEventUser[]> GetGuildScheduledEventUsersAsync(ulong eventId, ulong guildId, GetEventUsersParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(eventId, 0uL, "eventId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 100, "Limit");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(100);
		ulong? relativeId = (args.RelativeUserId.IsSpecified ? new ulong?(args.RelativeUserId.Value) : ((ulong?)null));
		string relativeDir = args.RelativeDirection.GetValueOrDefault(Direction.Before) switch
		{
			Direction.After => "after", 
			Direction.Around => "around", 
			_ => "before", 
		};
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		Expression<Func<string>> endpointExpr = ((!relativeId.HasValue) ? ((Expression<Func<string>>)(() => $"guilds/{guildId}/scheduled-events/{eventId}/users?with_member=true&limit={limit}")) : ((Expression<Func<string>>)(() => $"guilds/{guildId}/scheduled-events/{eventId}/users?with_member=true&limit={limit}&{relativeDir}={relativeId}")));
		return await SendAsync<GuildScheduledEventUser[]>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetGuildScheduledEventUsersAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<User> GetUserAsync(ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			return await SendAsync<User>("GET", () => $"users/{userId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetUserAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<User> GetMyUserAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<User>("GET", () => "users/@me", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetMyUserAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<Connection>> GetMyConnectionsAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<IReadOnlyCollection<Connection>>("GET", () => "users/@me/connections", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetMyConnectionsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<Channel>> GetMyPrivateChannelsAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<IReadOnlyCollection<Channel>>("GET", () => "users/@me/channels", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetMyPrivateChannelsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<UserGuild>> GetMyGuildsAsync(GetGuildSummariesParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 100, "Limit");
		Preconditions.GreaterThan(args.AfterGuildId, 0uL, "AfterGuildId");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(int.MaxValue);
		ulong afterGuildId = args.AfterGuildId.GetValueOrDefault(0uL);
		return await SendAsync<IReadOnlyCollection<UserGuild>>("GET", () => $"users/@me/guilds?limit={limit}&after={afterGuildId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetMyGuildsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Application> GetMyApplicationAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<Application>("GET", () => "oauth2/applications/@me", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetMyApplicationAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<User> ModifySelfAsync(ModifyCurrentUserParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrEmpty(args.Username, "Username");
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<User>("PATCH", () => "users/@me", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifySelfAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task ModifyMyNickAsync(ulong guildId, ModifyCurrentUserNickParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotNull(args.Nickname, "Nickname");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		await SendJsonAsync("PATCH", () => $"guilds/{guildId}/members/@me/nick", args, ids, ClientBucketType.Unbucketed, options, "ModifyMyNickAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Channel> CreateDMChannelAsync(CreateDMChannelParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.RecipientId, 0uL, "RecipientId");
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<Channel>("POST", () => "users/@me/channels", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateDMChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<VoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return await SendAsync<IReadOnlyCollection<VoiceRegion>>("GET", () => "voice/regions", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetVoiceRegionsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<VoiceRegion>> GetGuildVoiceRegionsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<IReadOnlyCollection<VoiceRegion>>("GET", () => $"guilds/{guildId}/regions", ids, ClientBucketType.Unbucketed, options, "GetGuildVoiceRegionsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<AuditLog> GetAuditLogsAsync(ulong guildId, GetAuditLogsParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(int.MaxValue);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		StringBuilder queryArgs = new StringBuilder();
		if (args.BeforeEntryId.IsSpecified)
		{
			queryArgs.Append("&before=").Append(args.BeforeEntryId);
		}
		if (args.UserId.IsSpecified)
		{
			queryArgs.Append("&user_id=").Append(args.UserId.Value);
		}
		if (args.ActionType.IsSpecified)
		{
			queryArgs.Append("&action_type=").Append(args.ActionType.Value);
		}
		Expression<Func<string>> endpointExpr = () => $"guilds/{guildId}/audit-logs?limit={limit}{queryArgs.ToString()}";
		return await SendAsync<AuditLog>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetAuditLogsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Webhook> CreateWebhookAsync(ulong channelId, CreateWebhookParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotNull(args.Name, "Name");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Webhook>("POST", () => $"channels/{channelId}/webhooks", args, ids, ClientBucketType.Unbucketed, options, "CreateWebhookAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Webhook> GetWebhookAsync(ulong webhookId, RequestOptions options = null)
	{
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			if (AuthTokenType == TokenType.Webhook)
			{
				return await SendAsync<Webhook>("GET", () => $"webhooks/{webhookId}/{AuthToken}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetWebhookAsync").ConfigureAwait(continueOnCapturedContext: false);
			}
			return await SendAsync<Webhook>("GET", () => $"webhooks/{webhookId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetWebhookAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<Webhook> ModifyWebhookAsync(ulong webhookId, ModifyWebhookParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrEmpty(args.Name, "Name");
		options = RequestOptions.CreateOrClone(options);
		if (AuthTokenType == TokenType.Webhook)
		{
			return await SendJsonAsync<Webhook>("PATCH", () => $"webhooks/{webhookId}/{AuthToken}", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyWebhookAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		return await SendJsonAsync<Webhook>("PATCH", () => $"webhooks/{webhookId}", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyWebhookAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteWebhookAsync(ulong webhookId, RequestOptions options = null)
	{
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		options = RequestOptions.CreateOrClone(options);
		if (AuthTokenType == TokenType.Webhook)
		{
			await SendAsync("DELETE", () => $"webhooks/{webhookId}/{AuthToken}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteWebhookAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await SendAsync("DELETE", () => $"webhooks/{webhookId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteWebhookAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public async Task<IReadOnlyCollection<Webhook>> GetGuildWebhooksAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendAsync<IReadOnlyCollection<Webhook>>("GET", () => $"guilds/{guildId}/webhooks", ids, ClientBucketType.Unbucketed, options, "GetGuildWebhooksAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IReadOnlyCollection<Webhook>> GetChannelWebhooksAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendAsync<IReadOnlyCollection<Webhook>>("GET", () => $"channels/{channelId}/webhooks", ids, ClientBucketType.Unbucketed, options, "GetChannelWebhooksAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	protected void CheckState()
	{
		if (LoginState != LoginState.LoggedIn)
		{
			throw new InvalidOperationException("Client is not logged in.");
		}
	}

	protected static double ToMilliseconds(Stopwatch stopwatch)
	{
		return Math.Round((double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0, 2);
	}

	protected string SerializeJson(object value)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		using (TextWriter textWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
		{
			using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
			_serializer.Serialize(jsonWriter, value);
		}
		return stringBuilder.ToString();
	}

	protected T DeserializeJson<T>(Stream jsonStream)
	{
		using TextReader reader = new StreamReader(jsonStream);
		using JsonReader reader2 = new JsonTextReader(reader);
		return _serializer.Deserialize<T>(reader2);
	}

	protected async Task<T> NullifyNotFound<T>(Task<T> sendTask) where T : class
	{
		try
		{
			T result = await sendTask.ConfigureAwait(continueOnCapturedContext: false);
			if (sendTask.Exception != null)
			{
				if (sendTask.Exception.InnerException is HttpException { HttpCode: HttpStatusCode.NotFound })
				{
					return null;
				}
				throw sendTask.Exception;
			}
			return result;
		}
		catch (HttpException ex2) when (ex2.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	private static string GetEndpoint(Expression<Func<string>> endpointExpr)
	{
		return endpointExpr.Compile()();
	}

	private static BucketId GetBucketId(string httpMethod, BucketIds ids, Expression<Func<string>> endpointExpr, string callingMethod)
	{
		if (ids.HttpMethod == null)
		{
			string text = (ids.HttpMethod = httpMethod);
		}
		return _bucketIdGenerators.GetOrAdd(callingMethod, (string x) => CreateBucketId(endpointExpr))(ids);
	}

	private static Func<BucketIds, BucketId> CreateBucketId(Expression<Func<string>> endpoint)
	{
		try
		{
			if (endpoint.Body.NodeType == ExpressionType.Constant)
			{
				return (BucketIds x) => BucketId.Create(x.HttpMethod, (endpoint.Body as ConstantExpression).Value.ToString(), x.ToMajorParametersDictionary());
			}
			StringBuilder stringBuilder = new StringBuilder();
			Expression[] array = (endpoint.Body as MethodCallExpression).Arguments.ToArray();
			string format = (array[0] as ConstantExpression).Value as string;
			if (array.Length > 1 && array[1].NodeType == ExpressionType.NewArrayInit)
			{
				Expression[] array2 = (array[1] as NewArrayExpression).Expressions.ToArray();
				Array.Resize(ref array, array2.Length + 1);
				Array.Copy(array2, 0, array, 1, array2.Length);
			}
			int num = format.IndexOf('?');
			if (num == -1)
			{
				num = format.Length;
			}
			int num2 = 0;
			while (true)
			{
				int num3 = format.IndexOf("{", num2);
				if (num3 == -1 || num3 > num)
				{
					break;
				}
				stringBuilder.Append(format, num2, num3 - num2);
				int num4 = format.IndexOf("}", num3);
				int num5 = int.Parse(format.Substring(num3 + 1, num4 - num3 - 1), NumberStyles.None, CultureInfo.InvariantCulture);
				int? index = BucketIds.GetIndex(GetFieldName(array[num5 + 1]));
				if (!index.HasValue && num4 != num && format.Length > num4 + 1 && format[num4 + 1] == '/')
				{
					num4++;
				}
				if (index.HasValue)
				{
					stringBuilder.Append($"{{{index.Value}}}");
				}
				num2 = num4 + 1;
			}
			stringBuilder.Append(format, num2, num - num2);
			if (stringBuilder[stringBuilder.Length - 1] == '/')
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			format = stringBuilder.ToString();
			return (BucketIds x) => BucketId.Create(x.HttpMethod, string.Format(format, x.ToArray()), x.ToMajorParametersDictionary());
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("Failed to generate the bucket id for this operation.", innerException);
		}
	}

	private static string GetFieldName(Expression expr)
	{
		if (expr.NodeType == ExpressionType.Convert)
		{
			expr = (expr as UnaryExpression).Operand;
		}
		if (expr.NodeType != ExpressionType.MemberAccess)
		{
			throw new InvalidOperationException("Unsupported expression");
		}
		return (expr as MemberExpression).Member.Name;
	}

	private static string WebhookQuery(bool wait = false, ulong? threadId = null)
	{
		List<string> list = new List<string>();
		if (wait)
		{
			list.Add("wait=true");
		}
		if (threadId.HasValue)
		{
			list.Add($"thread_id={threadId}");
		}
		return string.Join("&", list) ?? "";
	}
}

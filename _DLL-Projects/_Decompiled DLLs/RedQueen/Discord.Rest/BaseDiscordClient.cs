using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.Logging;
using Discord.Net;

namespace Discord.Rest;

internal abstract class BaseDiscordClient : IDiscordClient, IDisposable, IAsyncDisposable
{
	internal readonly AsyncEvent<Func<LogMessage, Task>> _logEvent = new AsyncEvent<Func<LogMessage, Task>>();

	private readonly AsyncEvent<Func<Task>> _loggedInEvent = new AsyncEvent<Func<Task>>();

	private readonly AsyncEvent<Func<Task>> _loggedOutEvent = new AsyncEvent<Func<Task>>();

	internal readonly Logger _restLogger;

	private readonly SemaphoreSlim _stateLock;

	private bool _isFirstLogin;

	private bool _isDisposed;

	internal DiscordRestApiClient ApiClient { get; }

	internal LogManager LogManager { get; }

	public LoginState LoginState { get; private set; }

	public ISelfUser CurrentUser { get; protected set; }

	public TokenType TokenType => ApiClient.AuthTokenType;

	internal bool UseInteractionSnowflakeDate { get; private set; }

	internal bool FormatUsersInBidirectionalUnicode { get; private set; }

	ConnectionState IDiscordClient.ConnectionState => ConnectionState.Disconnected;

	ISelfUser IDiscordClient.CurrentUser => CurrentUser;

	public event Func<LogMessage, Task> Log
	{
		add
		{
			_logEvent.Add(value);
		}
		remove
		{
			_logEvent.Remove(value);
		}
	}

	public event Func<Task> LoggedIn
	{
		add
		{
			_loggedInEvent.Add(value);
		}
		remove
		{
			_loggedInEvent.Remove(value);
		}
	}

	public event Func<Task> LoggedOut
	{
		add
		{
			_loggedOutEvent.Add(value);
		}
		remove
		{
			_loggedOutEvent.Remove(value);
		}
	}

	internal BaseDiscordClient(DiscordRestConfig config, DiscordRestApiClient client)
	{
		ApiClient = client;
		LogManager = new LogManager(config.LogLevel);
		LogManager.Message += async delegate(LogMessage msg)
		{
			await _logEvent.InvokeAsync(msg).ConfigureAwait(continueOnCapturedContext: false);
		};
		_stateLock = new SemaphoreSlim(1, 1);
		_restLogger = LogManager.CreateLogger("Rest");
		_isFirstLogin = config.DisplayInitialLog;
		UseInteractionSnowflakeDate = config.UseInteractionSnowflakeDate;
		FormatUsersInBidirectionalUnicode = config.FormatUsersInBidirectionalUnicode;
		ApiClient.RequestQueue.RateLimitTriggered += async delegate(BucketId id, RateLimitInfo? info, string endpoint)
		{
			if (!info.HasValue)
			{
				await _restLogger.VerboseAsync("Preemptive Rate limit triggered: " + endpoint + " " + (id.IsHashBucket ? ("(Bucket: " + id.BucketHash + ")") : "")).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await _restLogger.WarningAsync(string.Format("Rate limit triggered: {0} Remaining: {1}s {2}", endpoint, info.Value.RetryAfter, id.IsHashBucket ? ("(Bucket: " + id.BucketHash + ")") : "")).ConfigureAwait(continueOnCapturedContext: false);
			}
		};
		ApiClient.SentRequest += async delegate(string method, string endpoint, double millis)
		{
			await _restLogger.VerboseAsync($"{method} {endpoint}: {millis} ms").ConfigureAwait(continueOnCapturedContext: false);
		};
	}

	public async Task LoginAsync(TokenType tokenType, string token, bool validateToken = true)
	{
		await _stateLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await LoginInternalAsync(tokenType, token, validateToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_stateLock.Release();
		}
	}

	internal virtual async Task LoginInternalAsync(TokenType tokenType, string token, bool validateToken)
	{
		if (_isFirstLogin)
		{
			_isFirstLogin = false;
			await LogManager.WriteInitialLog().ConfigureAwait(continueOnCapturedContext: false);
		}
		if (LoginState != LoginState.LoggedOut)
		{
			await LogoutInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		LoginState = LoginState.LoggingIn;
		try
		{
			if (validateToken)
			{
				try
				{
					TokenUtils.ValidateToken(tokenType, token);
				}
				catch (ArgumentException ex)
				{
					await LogManager.WarningAsync("Discord", "A supplied token was invalid.", ex).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			await ApiClient.LoginAsync(tokenType, token).ConfigureAwait(continueOnCapturedContext: false);
			await OnLoginAsync(tokenType, token).ConfigureAwait(continueOnCapturedContext: false);
			LoginState = LoginState.LoggedIn;
		}
		catch
		{
			await LogoutInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
			throw;
		}
		await _loggedInEvent.InvokeAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	internal virtual Task OnLoginAsync(TokenType tokenType, string token)
	{
		return Task.Delay(0);
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

	internal virtual async Task LogoutInternalAsync()
	{
		if (LoginState != LoginState.LoggedOut)
		{
			LoginState = LoginState.LoggingOut;
			await ApiClient.LogoutAsync().ConfigureAwait(continueOnCapturedContext: false);
			await OnLogoutAsync().ConfigureAwait(continueOnCapturedContext: false);
			CurrentUser = null;
			LoginState = LoginState.LoggedOut;
			await _loggedOutEvent.InvokeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal virtual Task OnLogoutAsync()
	{
		return Task.Delay(0);
	}

	internal virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			ApiClient.Dispose();
			_stateLock?.Dispose();
			_isDisposed = true;
		}
	}

	internal virtual async ValueTask DisposeAsync(bool disposing)
	{
		if (!_isDisposed)
		{
			await ApiClient.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			_stateLock?.Dispose();
			_isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public ValueTask DisposeAsync()
	{
		return DisposeAsync(disposing: true);
	}

	public Task<int> GetRecommendedShardCountAsync(RequestOptions options = null)
	{
		return ClientHelper.GetRecommendShardCountAsync(this, options);
	}

	public Task<BotGateway> GetBotGatewayAsync(RequestOptions options = null)
	{
		return ClientHelper.GetBotGatewayAsync(this, options);
	}

	Task<IApplication> IDiscordClient.GetApplicationInfoAsync(RequestOptions options)
	{
		throw new NotSupportedException();
	}

	Task<IChannel> IDiscordClient.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IChannel>(null);
	}

	Task<IReadOnlyCollection<IPrivateChannel>> IDiscordClient.GetPrivateChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IPrivateChannel>)System.Collections.Immutable.ImmutableArray.Create<IPrivateChannel>());
	}

	Task<IReadOnlyCollection<IDMChannel>> IDiscordClient.GetDMChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IDMChannel>)System.Collections.Immutable.ImmutableArray.Create<IDMChannel>());
	}

	Task<IReadOnlyCollection<IGroupChannel>> IDiscordClient.GetGroupChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IGroupChannel>)System.Collections.Immutable.ImmutableArray.Create<IGroupChannel>());
	}

	Task<IReadOnlyCollection<IConnection>> IDiscordClient.GetConnectionsAsync(RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IConnection>)System.Collections.Immutable.ImmutableArray.Create<IConnection>());
	}

	Task<IInvite> IDiscordClient.GetInviteAsync(string inviteId, RequestOptions options)
	{
		return Task.FromResult<IInvite>(null);
	}

	Task<IGuild> IDiscordClient.GetGuildAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IGuild>(null);
	}

	Task<IReadOnlyCollection<IGuild>> IDiscordClient.GetGuildsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IGuild>)System.Collections.Immutable.ImmutableArray.Create<IGuild>());
	}

	Task<IGuild> IDiscordClient.CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
	{
		throw new NotSupportedException();
	}

	Task<IUser> IDiscordClient.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IUser>(null);
	}

	Task<IUser> IDiscordClient.GetUserAsync(string username, string discriminator, RequestOptions options)
	{
		return Task.FromResult<IUser>(null);
	}

	Task<IReadOnlyCollection<IVoiceRegion>> IDiscordClient.GetVoiceRegionsAsync(RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IVoiceRegion>)System.Collections.Immutable.ImmutableArray.Create<IVoiceRegion>());
	}

	Task<IVoiceRegion> IDiscordClient.GetVoiceRegionAsync(string id, RequestOptions options)
	{
		return Task.FromResult<IVoiceRegion>(null);
	}

	Task<IWebhook> IDiscordClient.GetWebhookAsync(ulong id, RequestOptions options)
	{
		return Task.FromResult<IWebhook>(null);
	}

	Task<IApplicationCommand> IDiscordClient.GetGlobalApplicationCommandAsync(ulong id, RequestOptions options)
	{
		return Task.FromResult<IApplicationCommand>(null);
	}

	Task<IReadOnlyCollection<IApplicationCommand>> IDiscordClient.GetGlobalApplicationCommandsAsync(bool withLocalizations, string locale, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IApplicationCommand>)System.Collections.Immutable.ImmutableArray.Create<IApplicationCommand>());
	}

	Task<IApplicationCommand> IDiscordClient.CreateGlobalApplicationCommand(ApplicationCommandProperties properties, RequestOptions options)
	{
		return Task.FromResult<IApplicationCommand>(null);
	}

	Task<IReadOnlyCollection<IApplicationCommand>> IDiscordClient.BulkOverwriteGlobalApplicationCommand(ApplicationCommandProperties[] properties, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IApplicationCommand>)System.Collections.Immutable.ImmutableArray.Create<IApplicationCommand>());
	}

	Task IDiscordClient.StartAsync()
	{
		return Task.Delay(0);
	}

	Task IDiscordClient.StopAsync()
	{
		return Task.Delay(0);
	}
}

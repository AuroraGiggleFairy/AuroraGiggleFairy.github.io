using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.Net.Rest;
using Discord.Rest;
using Newtonsoft.Json;

namespace Discord.Net.Queue;

internal class RequestBucket
{
	private const int MinimumSleepTimeMs = 750;

	private readonly object _lock;

	private readonly RequestQueue _queue;

	private int _semaphore;

	private DateTimeOffset? _resetTick;

	private RequestBucket _redirectBucket;

	private static int nextId;

	public BucketId Id { get; private set; }

	public int WindowCount { get; private set; }

	public DateTimeOffset LastAttemptAt { get; private set; }

	public RequestBucket(RequestQueue queue, IRequest request, BucketId id)
	{
		_queue = queue;
		Id = id;
		_lock = new object();
		if (request.Options.IsClientBucket)
		{
			WindowCount = ClientBucket.Get(request.Options.BucketId).WindowCount;
		}
		else if (request.Options.IsGatewayBucket)
		{
			WindowCount = GatewayBucket.Get(request.Options.BucketId).WindowCount;
		}
		else
		{
			WindowCount = 1;
		}
		_semaphore = WindowCount;
		_resetTick = null;
		LastAttemptAt = DateTimeOffset.UtcNow;
	}

	public async Task<Stream> SendAsync(RestRequest request)
	{
		int id = Interlocked.Increment(ref nextId);
		LastAttemptAt = DateTimeOffset.UtcNow;
		while (true)
		{
			await _queue.EnterGlobalAsync(id, request).ConfigureAwait(continueOnCapturedContext: false);
			await EnterAsync(id, request).ConfigureAwait(continueOnCapturedContext: false);
			if (_redirectBucket != null)
			{
				break;
			}
			RateLimitInfo info = default(RateLimitInfo);
			try
			{
				RestResponse restResponse = await request.SendAsync().ConfigureAwait(continueOnCapturedContext: false);
				info = new RateLimitInfo(restResponse.Headers, request.Endpoint);
				request.Options.ExecuteRatelimitCallback(info);
				if (restResponse.StatusCode < HttpStatusCode.OK || restResponse.StatusCode >= HttpStatusCode.MultipleChoices)
				{
					switch (restResponse.StatusCode)
					{
					case (HttpStatusCode)429:
						if (info.IsGlobal)
						{
							_queue.PauseGlobal(info);
						}
						else
						{
							UpdateRateLimit(id, request, info, is429: true, redirected: false, restResponse.Stream);
						}
						await _queue.RaiseRateLimitTriggered(Id, info, request.Method + " " + request.Endpoint).ConfigureAwait(continueOnCapturedContext: false);
						continue;
					case HttpStatusCode.BadGateway:
						if (((uint?)request.Options.RetryMode & 8u) == 0)
						{
							throw new HttpException(HttpStatusCode.BadGateway, request);
						}
						continue;
					}
					Discord.API.DiscordError discordError = null;
					if (restResponse.Stream != null)
					{
						try
						{
							using StreamReader reader = new StreamReader(restResponse.Stream);
							using JsonTextReader reader2 = new JsonTextReader(reader);
							discordError = DiscordRestClient.Serializer.Deserialize<Discord.API.DiscordError>(reader2);
						}
						catch
						{
						}
					}
					throw new HttpException(restResponse.StatusCode, request, discordError?.Code, discordError?.Message, (discordError != null && discordError.Errors.IsSpecified) ? discordError.Errors.Value.Select((ErrorDetails x) => new DiscordJsonError(x.Name.GetValueOrDefault("root"), x.Errors.Select((Discord.API.Error y) => new DiscordError(y.Code, y.Message)).ToArray())).ToArray() : null);
				}
				return restResponse.Stream;
			}
			catch (TimeoutException)
			{
				if (((uint?)request.Options.RetryMode & 1u) == 0)
				{
					throw;
				}
				await Task.Delay(500).ConfigureAwait(continueOnCapturedContext: false);
			}
			finally
			{
				UpdateRateLimit(id, request, info, is429: false);
			}
		}
		return await _redirectBucket.SendAsync(request);
	}

	public async Task SendAsync(WebSocketRequest request)
	{
		int id = Interlocked.Increment(ref nextId);
		LastAttemptAt = DateTimeOffset.UtcNow;
		while (true)
		{
			await _queue.EnterGlobalAsync(id, request).ConfigureAwait(continueOnCapturedContext: false);
			await EnterAsync(id, request).ConfigureAwait(continueOnCapturedContext: false);
			try
			{
				await request.SendAsync().ConfigureAwait(continueOnCapturedContext: false);
				break;
			}
			catch (TimeoutException)
			{
				if (((uint?)request.Options.RetryMode & 1u) == 0)
				{
					throw;
				}
				await Task.Delay(500).ConfigureAwait(continueOnCapturedContext: false);
			}
			finally
			{
				UpdateRateLimit(id, request, default(RateLimitInfo), is429: false);
			}
		}
	}

	internal async Task TriggerAsync(int id, IRequest request)
	{
		await EnterAsync(id, request).ConfigureAwait(continueOnCapturedContext: false);
		UpdateRateLimit(id, request, default(RateLimitInfo), is429: false);
	}

	private async Task EnterAsync(int id, IRequest request)
	{
		bool isRateLimited = false;
		while (_redirectBucket == null)
		{
			DateTimeOffset utcNow = DateTimeOffset.UtcNow;
			DateTimeOffset? timeoutAt = request.TimeoutAt;
			if (utcNow > timeoutAt || request.Options.CancelToken.IsCancellationRequested)
			{
				if (!isRateLimited)
				{
					throw new TimeoutException();
				}
				ThrowRetryLimit(request);
			}
			int windowCount;
			DateTimeOffset? resetAt;
			lock (_lock)
			{
				windowCount = WindowCount;
				resetAt = _resetTick;
			}
			DateTimeOffset? timeoutAt2 = request.TimeoutAt;
			int num = Interlocked.Decrement(ref _semaphore);
			if (windowCount <= 0 || num >= 0)
			{
				break;
			}
			if (!isRateLimited)
			{
				bool ignoreRatelimit = false;
				isRateLimited = true;
				if (request is RestRequest restRequest)
				{
					await _queue.RaiseRateLimitTriggered(Id, null, restRequest.Method + " " + restRequest.Endpoint).ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					if (!(request is WebSocketRequest webSocketRequest))
					{
						throw new InvalidOperationException("Unknown request type");
					}
					if (!webSocketRequest.IgnoreLimit)
					{
						await _queue.RaiseRateLimitTriggered(Id, null, Id.Endpoint).ConfigureAwait(continueOnCapturedContext: false);
					}
					else
					{
						ignoreRatelimit = true;
					}
				}
				if (ignoreRatelimit)
				{
					break;
				}
			}
			ThrowRetryLimit(request);
			if (resetAt.HasValue && resetAt > DateTimeOffset.UtcNow)
			{
				if (resetAt > timeoutAt2)
				{
					ThrowRetryLimit(request);
				}
				int num2 = (int)Math.Ceiling((resetAt.Value - DateTimeOffset.UtcNow).TotalMilliseconds);
				if (num2 > 0)
				{
					await Task.Delay(num2, request.Options.CancelToken).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			else
			{
				if ((timeoutAt2.Value - DateTimeOffset.UtcNow).TotalMilliseconds < 750.0)
				{
					ThrowRetryLimit(request);
				}
				await Task.Delay(750, request.Options.CancelToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	private void UpdateRateLimit(int id, IRequest request, RateLimitInfo info, bool is429, bool redirected = false, Stream body = null)
	{
		if (WindowCount == 0)
		{
			return;
		}
		lock (_lock)
		{
			if (redirected)
			{
				Interlocked.Decrement(ref _semaphore);
			}
			bool hasValue = _resetTick.HasValue;
			if (info.Bucket != null && !redirected)
			{
				(RequestBucket, BucketId) tuple = _queue.UpdateBucketHash(Id, info.Bucket);
				if (tuple.Item1 != null && tuple.Item2 != null)
				{
					if (tuple.Item1 != this)
					{
						(_redirectBucket, _) = tuple;
						_redirectBucket.UpdateRateLimit(id, request, info, is429, redirected: true);
						return;
					}
					Id = tuple.Item2;
				}
			}
			if (info.Limit.HasValue && WindowCount != info.Limit.Value)
			{
				WindowCount = info.Limit.Value;
				_semaphore = ((!is429) ? info.Remaining.Value : 0);
			}
			DateTimeOffset? dateTimeOffset = null;
			if (is429)
			{
				Ratelimit ratelimit = info.ReadRatelimitPayload(body);
				dateTimeOffset = DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(ratelimit?.RetryAfter ?? info.ResetAfter?.TotalSeconds ?? 0.0));
			}
			else if (info.RetryAfter.HasValue)
			{
				dateTimeOffset = DateTimeOffset.UtcNow.AddSeconds(info.RetryAfter.Value);
			}
			else if (info.ResetAfter.HasValue && request.Options.UseSystemClock.HasValue && !request.Options.UseSystemClock.Value)
			{
				dateTimeOffset = DateTimeOffset.UtcNow.Add(info.ResetAfter.Value);
			}
			else if (info.Reset.HasValue)
			{
				dateTimeOffset = info.Reset.Value.AddSeconds(info.Lag?.TotalSeconds ?? 1.0);
				_ = (dateTimeOffset.Value - DateTimeOffset.UtcNow).TotalMilliseconds;
			}
			else if (request.Options.IsClientBucket && Id != null)
			{
				dateTimeOffset = DateTimeOffset.UtcNow.AddSeconds(ClientBucket.Get(Id).WindowSeconds);
			}
			else if (request.Options.IsGatewayBucket && request.Options.BucketId != null)
			{
				dateTimeOffset = DateTimeOffset.UtcNow.AddSeconds(GatewayBucket.Get(request.Options.BucketId).WindowSeconds);
				if (!hasValue)
				{
					_resetTick = dateTimeOffset;
					LastAttemptAt = dateTimeOffset.Value;
					QueueReset(id, (int)Math.Ceiling((_resetTick.Value - DateTimeOffset.UtcNow).TotalMilliseconds), request);
				}
				return;
			}
			if (!dateTimeOffset.HasValue)
			{
				WindowCount = 0;
			}
			else if (!hasValue || dateTimeOffset > _resetTick)
			{
				_resetTick = dateTimeOffset;
				LastAttemptAt = dateTimeOffset.Value;
				if (!hasValue)
				{
					QueueReset(id, (int)Math.Ceiling((_resetTick.Value - DateTimeOffset.UtcNow).TotalMilliseconds), request);
				}
			}
		}
	}

	private async Task QueueReset(int id, int millis, IRequest request)
	{
		while (true)
		{
			if (millis > 0)
			{
				await Task.Delay(millis).ConfigureAwait(continueOnCapturedContext: false);
			}
			lock (_lock)
			{
				millis = (int)Math.Ceiling((_resetTick.Value - DateTimeOffset.UtcNow).TotalMilliseconds);
				if (millis <= 0)
				{
					_semaphore = WindowCount;
					_resetTick = null;
					break;
				}
			}
		}
	}

	private void ThrowRetryLimit(IRequest request)
	{
		if (((uint?)request.Options.RetryMode & 4u) == 0)
		{
			throw new RateLimitedException(request);
		}
	}
}

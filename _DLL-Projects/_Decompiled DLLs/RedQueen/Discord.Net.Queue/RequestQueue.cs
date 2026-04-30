using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Net.Queue;

internal class RequestQueue : IDisposable, IAsyncDisposable
{
	private readonly ConcurrentDictionary<BucketId, object> _buckets;

	private readonly SemaphoreSlim _tokenLock;

	private readonly CancellationTokenSource _cancelTokenSource;

	private CancellationTokenSource _clearToken;

	private CancellationToken _parentToken;

	private CancellationTokenSource _requestCancelTokenSource;

	private CancellationToken _requestCancelToken;

	private DateTimeOffset _waitUntil;

	private Task _cleanupTask;

	public event Func<BucketId, RateLimitInfo?, string, Task> RateLimitTriggered;

	public RequestQueue()
	{
		_tokenLock = new SemaphoreSlim(1, 1);
		_clearToken = new CancellationTokenSource();
		_cancelTokenSource = new CancellationTokenSource();
		_requestCancelToken = CancellationToken.None;
		_parentToken = CancellationToken.None;
		_buckets = new ConcurrentDictionary<BucketId, object>();
		_cleanupTask = RunCleanup();
	}

	public async Task SetCancelTokenAsync(CancellationToken cancelToken)
	{
		await _tokenLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			_parentToken = cancelToken;
			_requestCancelTokenSource?.Dispose();
			_requestCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, _clearToken.Token);
			_requestCancelToken = _requestCancelTokenSource.Token;
		}
		finally
		{
			_tokenLock.Release();
		}
	}

	public async Task ClearAsync()
	{
		await _tokenLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			_clearToken?.Cancel();
			_clearToken?.Dispose();
			_clearToken = new CancellationTokenSource();
			_requestCancelTokenSource?.Dispose();
			_requestCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_clearToken.Token, _parentToken);
			_requestCancelToken = _requestCancelTokenSource.Token;
		}
		finally
		{
			_tokenLock.Release();
		}
	}

	public async Task<Stream> SendAsync(RestRequest request)
	{
		CancellationTokenSource createdTokenSource = null;
		if (request.Options.CancelToken.CanBeCanceled)
		{
			createdTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_requestCancelToken, request.Options.CancelToken);
			request.Options.CancelToken = createdTokenSource.Token;
		}
		else
		{
			request.Options.CancelToken = _requestCancelToken;
		}
		Stream result = await GetOrCreateBucket(request.Options, request).SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
		createdTokenSource?.Dispose();
		return result;
	}

	public async Task SendAsync(WebSocketRequest request)
	{
		CancellationTokenSource createdTokenSource = null;
		if (request.Options.CancelToken.CanBeCanceled)
		{
			createdTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_requestCancelToken, request.Options.CancelToken);
			request.Options.CancelToken = createdTokenSource.Token;
		}
		else
		{
			request.Options.CancelToken = _requestCancelToken;
		}
		await GetOrCreateBucket(request.Options, request).SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
		createdTokenSource?.Dispose();
	}

	internal async Task EnterGlobalAsync(int id, RestRequest request)
	{
		int num = (int)Math.Ceiling((_waitUntil - DateTimeOffset.UtcNow).TotalMilliseconds);
		if (num > 0)
		{
			await Task.Delay(num).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal void PauseGlobal(RateLimitInfo info)
	{
		_waitUntil = DateTimeOffset.UtcNow.AddMilliseconds((double)info.RetryAfter.Value + (info.Lag?.TotalMilliseconds ?? 0.0));
	}

	internal async Task EnterGlobalAsync(int id, WebSocketRequest request)
	{
		if (GatewayBucket.Get(request.Options.BucketId).Type != GatewayBucketType.Unbucketed)
		{
			GatewayBucket gatewayBucket = GatewayBucket.Get(GatewayBucketType.Unbucketed);
			RequestOptions requestOptions = RequestOptions.CreateOrClone(request.Options);
			requestOptions.BucketId = gatewayBucket.Id;
			WebSocketRequest request2 = new WebSocketRequest(null, null, isText: false, ignoreLimit: false, requestOptions);
			await GetOrCreateBucket(requestOptions, request2).TriggerAsync(id, request2);
		}
	}

	private RequestBucket GetOrCreateBucket(RequestOptions options, IRequest request)
	{
		BucketId bucketId = options.BucketId;
		object orAdd = _buckets.GetOrAdd(bucketId, (BucketId x) => new RequestBucket(this, request, x));
		if (orAdd is BucketId bucketId2)
		{
			options.BucketId = bucketId2;
			return (RequestBucket)_buckets.GetOrAdd(bucketId2, (BucketId x) => new RequestBucket(this, request, x));
		}
		return (RequestBucket)orAdd;
	}

	internal async Task RaiseRateLimitTriggered(BucketId bucketId, RateLimitInfo? info, string endpoint)
	{
		await this.RateLimitTriggered(bucketId, info, endpoint).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal (RequestBucket, BucketId) UpdateBucketHash(BucketId id, string discordHash)
	{
		if (!id.IsHashBucket)
		{
			BucketId bucket = BucketId.Create(discordHash, id);
			RequestBucket item = (RequestBucket)_buckets.GetOrAdd(bucket, _buckets[id]);
			_buckets.AddOrUpdate(id, bucket, (BucketId oldBucket, object oldObj) => bucket);
			return (item, bucket);
		}
		return (null, null);
	}

	public void ClearGatewayBuckets()
	{
		GatewayBucketType[] array = (GatewayBucketType[])Enum.GetValues(typeof(GatewayBucketType));
		foreach (GatewayBucketType type in array)
		{
			_buckets.TryRemove(GatewayBucket.Get(type).Id, out var _);
		}
	}

	private async Task RunCleanup()
	{
		try
		{
			while (!_cancelTokenSource.IsCancellationRequested)
			{
				DateTimeOffset utcNow = DateTimeOffset.UtcNow;
				foreach (RequestBucket bucket in from x in _buckets
					where x.Value is RequestBucket
					select (RequestBucket)x.Value)
				{
					if (!((utcNow - bucket.LastAttemptAt).TotalMinutes > 1.0))
					{
						continue;
					}
					object value;
					if (bucket.Id.IsHashBucket)
					{
						foreach (BucketId item in from x in _buckets
							where x.Value == bucket.Id
							select (BucketId)x.Value)
						{
							_buckets.TryRemove(item, out value);
						}
					}
					_buckets.TryRemove(bucket.Id, out value);
				}
				await Task.Delay(60000, _cancelTokenSource.Token).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch (TaskCanceledException)
		{
		}
		catch (ObjectDisposedException)
		{
		}
	}

	public void Dispose()
	{
		if (_cancelTokenSource != null)
		{
			_cancelTokenSource.Cancel();
			_cancelTokenSource.Dispose();
			_cleanupTask.GetAwaiter().GetResult();
		}
		_tokenLock?.Dispose();
		_clearToken?.Dispose();
		_requestCancelTokenSource?.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		if (_cancelTokenSource != null)
		{
			_cancelTokenSource.Cancel();
			_cancelTokenSource.Dispose();
			await _cleanupTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		_tokenLock?.Dispose();
		_clearToken?.Dispose();
		_requestCancelTokenSource?.Dispose();
	}
}

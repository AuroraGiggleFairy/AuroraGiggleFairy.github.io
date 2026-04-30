using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Logging;
using Discord.Net;

namespace Discord;

internal class ConnectionManager : IDisposable
{
	private readonly AsyncEvent<Func<Task>> _connectedEvent = new AsyncEvent<Func<Task>>();

	private readonly AsyncEvent<Func<Exception, bool, Task>> _disconnectedEvent = new AsyncEvent<Func<Exception, bool, Task>>();

	private readonly SemaphoreSlim _stateLock;

	private readonly Logger _logger;

	private readonly int _connectionTimeout;

	private readonly Func<Task> _onConnecting;

	private readonly Func<Exception, Task> _onDisconnecting;

	private TaskCompletionSource<bool> _connectionPromise;

	private TaskCompletionSource<bool> _readyPromise;

	private CancellationTokenSource _combinedCancelToken;

	private CancellationTokenSource _reconnectCancelToken;

	private CancellationTokenSource _connectionCancelToken;

	private Task _task;

	private bool _isDisposed;

	public ConnectionState State { get; private set; }

	public CancellationToken CancelToken { get; private set; }

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

	public event Func<Exception, bool, Task> Disconnected
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

	internal ConnectionManager(SemaphoreSlim stateLock, Logger logger, int connectionTimeout, Func<Task> onConnecting, Func<Exception, Task> onDisconnecting, Action<Func<Exception, Task>> clientDisconnectHandler)
	{
		_stateLock = stateLock;
		_logger = logger;
		_connectionTimeout = connectionTimeout;
		_onConnecting = onConnecting;
		_onDisconnecting = onDisconnecting;
		clientDisconnectHandler(delegate(Exception ex)
		{
			if (ex != null)
			{
				WebSocketClosedException ex2 = ex as WebSocketClosedException;
				if (ex2 != null && ex2.CloseCode == 4006)
				{
					CriticalError(new Exception("WebSocket session expired", ex));
				}
				else if (ex2 != null && ex2.CloseCode == 4014)
				{
					CriticalError(new Exception("WebSocket connection was closed", ex));
				}
				else
				{
					Error(new Exception("WebSocket connection was closed", ex));
				}
			}
			else
			{
				Error(new Exception("WebSocket connection was closed"));
			}
			return Task.Delay(0);
		});
	}

	public virtual async Task StartAsync()
	{
		if (State != ConnectionState.Disconnected)
		{
			throw new InvalidOperationException("Cannot start an already running client.");
		}
		await AcquireConnectionLock().ConfigureAwait(continueOnCapturedContext: false);
		CancellationTokenSource reconnectCancelToken = new CancellationTokenSource();
		_reconnectCancelToken?.Dispose();
		_reconnectCancelToken = reconnectCancelToken;
		_task = Task.Run(async delegate
		{
			_ = 7;
			try
			{
				Random jitter = new Random();
				int nextReconnectDelay = 1000;
				while (!reconnectCancelToken.IsCancellationRequested)
				{
					try
					{
						await ConnectAsync(reconnectCancelToken).ConfigureAwait(continueOnCapturedContext: false);
						nextReconnectDelay = 1000;
						await _connectionPromise.Task.ConfigureAwait(continueOnCapturedContext: false);
					}
					catch (OperationCanceledException ex)
					{
						Cancel();
						await DisconnectAsync(ex, !reconnectCancelToken.IsCancellationRequested).ConfigureAwait(continueOnCapturedContext: false);
					}
					catch (Exception ex2)
					{
						Error(ex2);
						if (reconnectCancelToken.IsCancellationRequested)
						{
							await _logger.ErrorAsync(ex2).ConfigureAwait(continueOnCapturedContext: false);
							await DisconnectAsync(ex2, isReconnecting: false).ConfigureAwait(continueOnCapturedContext: false);
						}
						else
						{
							await _logger.WarningAsync(ex2).ConfigureAwait(continueOnCapturedContext: false);
							await DisconnectAsync(ex2, isReconnecting: true).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					if (!reconnectCancelToken.IsCancellationRequested)
					{
						await Task.Delay(nextReconnectDelay, reconnectCancelToken.Token).ConfigureAwait(continueOnCapturedContext: false);
						nextReconnectDelay = nextReconnectDelay * 2 + jitter.Next(-250, 250);
						if (nextReconnectDelay > 60000)
						{
							nextReconnectDelay = 60000;
						}
					}
				}
			}
			finally
			{
				_stateLock.Release();
			}
		});
	}

	public virtual Task StopAsync()
	{
		Cancel();
		return Task.CompletedTask;
	}

	private async Task ConnectAsync(CancellationTokenSource reconnectCancelToken)
	{
		_connectionCancelToken?.Dispose();
		_combinedCancelToken?.Dispose();
		_connectionCancelToken = new CancellationTokenSource();
		_combinedCancelToken = CancellationTokenSource.CreateLinkedTokenSource(_connectionCancelToken.Token, reconnectCancelToken.Token);
		CancelToken = _combinedCancelToken.Token;
		_connectionPromise = new TaskCompletionSource<bool>();
		State = ConnectionState.Connecting;
		await _logger.InfoAsync("Connecting").ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			TaskCompletionSource<bool> readyPromise = new TaskCompletionSource<bool>();
			_readyPromise = readyPromise;
			CancellationToken cancelToken = CancelToken;
			Task.Run(async delegate
			{
				try
				{
					await Task.Delay(_connectionTimeout, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
					readyPromise.TrySetException(new TimeoutException());
				}
				catch (OperationCanceledException)
				{
				}
			});
			await _onConnecting().ConfigureAwait(continueOnCapturedContext: false);
			await _logger.InfoAsync("Connected").ConfigureAwait(continueOnCapturedContext: false);
			State = ConnectionState.Connected;
			await _logger.DebugAsync("Raising Event").ConfigureAwait(continueOnCapturedContext: false);
			await _connectedEvent.InvokeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex)
		{
			Error(ex);
			throw;
		}
	}

	private async Task DisconnectAsync(Exception ex, bool isReconnecting)
	{
		if (State != ConnectionState.Disconnected)
		{
			State = ConnectionState.Disconnecting;
			await _logger.InfoAsync("Disconnecting").ConfigureAwait(continueOnCapturedContext: false);
			await _onDisconnecting(ex).ConfigureAwait(continueOnCapturedContext: false);
			await _disconnectedEvent.InvokeAsync(ex, isReconnecting).ConfigureAwait(continueOnCapturedContext: false);
			State = ConnectionState.Disconnected;
			await _logger.InfoAsync("Disconnected").ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public async Task CompleteAsync()
	{
		await _readyPromise.TrySetResultAsync(result: true).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task WaitAsync()
	{
		await _readyPromise.Task.ConfigureAwait(continueOnCapturedContext: false);
	}

	public void Cancel()
	{
		_readyPromise?.TrySetCanceled();
		_connectionPromise?.TrySetCanceled();
		_reconnectCancelToken?.Cancel();
		_connectionCancelToken?.Cancel();
	}

	public void Error(Exception ex)
	{
		_readyPromise.TrySetException(ex);
		_connectionPromise.TrySetException(ex);
		_connectionCancelToken?.Cancel();
	}

	public void CriticalError(Exception ex)
	{
		_reconnectCancelToken?.Cancel();
		Error(ex);
	}

	public void Reconnect()
	{
		_readyPromise.TrySetCanceled();
		_connectionPromise.TrySetCanceled();
		_connectionCancelToken?.Cancel();
	}

	private async Task AcquireConnectionLock()
	{
		do
		{
			await StopAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		while (!(await _stateLock.WaitAsync(0).ConfigureAwait(continueOnCapturedContext: false)));
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_combinedCancelToken?.Dispose();
				_reconnectCancelToken?.Dispose();
				_connectionCancelToken?.Dispose();
			}
			_isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}

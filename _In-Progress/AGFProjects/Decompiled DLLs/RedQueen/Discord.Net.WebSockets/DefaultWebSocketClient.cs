using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Net.WebSockets;

internal class DefaultWebSocketClient : IWebSocketClient, IDisposable
{
	public const int ReceiveChunkSize = 16384;

	public const int SendChunkSize = 4096;

	private const int HR_TIMEOUT = -2147012894;

	private readonly SemaphoreSlim _lock;

	private readonly Dictionary<string, string> _headers;

	private readonly IWebProxy _proxy;

	private ClientWebSocket _client;

	private Task _task;

	private CancellationTokenSource _disconnectTokenSource;

	private CancellationTokenSource _cancelTokenSource;

	private CancellationToken _cancelToken;

	private CancellationToken _parentToken;

	private bool _isDisposed;

	private bool _isDisconnecting;

	public event Func<byte[], int, int, Task> BinaryMessage;

	public event Func<string, Task> TextMessage;

	public event Func<Exception, Task> Closed;

	public DefaultWebSocketClient(IWebProxy proxy = null)
	{
		_lock = new SemaphoreSlim(1, 1);
		_disconnectTokenSource = new CancellationTokenSource();
		_cancelToken = CancellationToken.None;
		_parentToken = CancellationToken.None;
		_headers = new Dictionary<string, string>();
		_proxy = proxy;
	}

	private void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				DisconnectInternalAsync(1000, isDisposing: true).GetAwaiter().GetResult();
				_disconnectTokenSource?.Dispose();
				_cancelTokenSource?.Dispose();
				_lock?.Dispose();
			}
			_isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public async Task ConnectAsync(string host)
	{
		await _lock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await ConnectInternalAsync(host).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task ConnectInternalAsync(string host)
	{
		await DisconnectInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		_disconnectTokenSource?.Dispose();
		_cancelTokenSource?.Dispose();
		_disconnectTokenSource = new CancellationTokenSource();
		_cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_parentToken, _disconnectTokenSource.Token);
		_cancelToken = _cancelTokenSource.Token;
		_client?.Dispose();
		_client = new ClientWebSocket();
		_client.Options.Proxy = _proxy;
		_client.Options.KeepAliveInterval = TimeSpan.Zero;
		foreach (KeyValuePair<string, string> header in _headers)
		{
			if (header.Value != null)
			{
				_client.Options.SetRequestHeader(header.Key, header.Value);
			}
		}
		await _client.ConnectAsync(new Uri(host), _cancelToken).ConfigureAwait(continueOnCapturedContext: false);
		_task = RunAsync(_cancelToken);
	}

	public async Task DisconnectAsync(int closeCode = 1000)
	{
		await _lock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await DisconnectInternalAsync(closeCode).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task DisconnectInternalAsync(int closeCode = 1000, bool isDisposing = false)
	{
		_isDisconnecting = true;
		try
		{
			_disconnectTokenSource.Cancel(throwOnFirstException: false);
		}
		catch
		{
		}
		if (_client != null)
		{
			if (!isDisposing)
			{
				try
				{
					await _client.CloseOutputAsync((WebSocketCloseStatus)closeCode, "", default(CancellationToken));
				}
				catch
				{
				}
			}
			try
			{
				_client.Dispose();
			}
			catch
			{
			}
			_client = null;
		}
		try
		{
			await (_task ?? Task.Delay(0)).ConfigureAwait(continueOnCapturedContext: false);
			_task = null;
		}
		finally
		{
			_isDisconnecting = false;
		}
	}

	private async Task OnClosed(Exception ex)
	{
		if (!_isDisconnecting)
		{
			await _lock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
			try
			{
				await DisconnectInternalAsync();
			}
			finally
			{
				_lock.Release();
			}
			await this.Closed(ex);
		}
	}

	public void SetHeader(string key, string value)
	{
		_headers[key] = value;
	}

	public void SetCancelToken(CancellationToken cancelToken)
	{
		_cancelTokenSource?.Dispose();
		_parentToken = cancelToken;
		_cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_parentToken, _disconnectTokenSource.Token);
		_cancelToken = _cancelTokenSource.Token;
	}

	public async Task SendAsync(byte[] data, int index, int count, bool isText)
	{
		try
		{
			await _lock.WaitAsync(_cancelToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		try
		{
			if (_client != null)
			{
				int frameCount = (int)Math.Ceiling((double)count / 4096.0);
				int i = 0;
				while (i < frameCount)
				{
					bool endOfMessage = i == frameCount - 1;
					WebSocketMessageType messageType = ((!isText) ? WebSocketMessageType.Binary : WebSocketMessageType.Text);
					await _client.SendAsync(new ArraySegment<byte>(data, index, count), messageType, endOfMessage, _cancelToken).ConfigureAwait(continueOnCapturedContext: false);
					i++;
					index += 4096;
				}
			}
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task RunAsync(CancellationToken cancelToken)
	{
		ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[16384]);
		try
		{
			while (!cancelToken.IsCancellationRequested)
			{
				WebSocketReceiveResult webSocketReceiveResult = await _client.ReceiveAsync(buffer, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
				if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
				{
					throw new WebSocketClosedException((int)webSocketReceiveResult.CloseStatus.Value, webSocketReceiveResult.CloseStatusDescription);
				}
				int num;
				byte[] array;
				if (!webSocketReceiveResult.EndOfMessage)
				{
					using MemoryStream stream = new MemoryStream();
					stream.Write(buffer.Array, 0, webSocketReceiveResult.Count);
					do
					{
						if (cancelToken.IsCancellationRequested)
						{
							return;
						}
						webSocketReceiveResult = await _client.ReceiveAsync(buffer, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
						stream.Write(buffer.Array, 0, webSocketReceiveResult.Count);
					}
					while (webSocketReceiveResult == null || !webSocketReceiveResult.EndOfMessage);
					num = (int)stream.Length;
					array = (stream.TryGetBuffer(out var buffer2) ? buffer2.Array : stream.ToArray());
				}
				else
				{
					num = webSocketReceiveResult.Count;
					array = buffer.Array;
				}
				if (webSocketReceiveResult.MessageType == WebSocketMessageType.Text)
				{
					string arg = Encoding.UTF8.GetString(array, 0, num);
					await this.TextMessage(arg).ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					await this.BinaryMessage(array, 0, num).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
		catch (Win32Exception ex) when (ex.HResult == -2147012894)
		{
			OnClosed(new Exception("Connection timed out.", ex));
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex3)
		{
			OnClosed(ex3);
		}
	}
}

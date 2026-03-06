using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Net.Udp;

internal class DefaultUdpSocket : IUdpSocket, IDisposable
{
	private readonly SemaphoreSlim _lock;

	private UdpClient _udp;

	private IPEndPoint _destination;

	private CancellationTokenSource _stopCancelTokenSource;

	private CancellationTokenSource _cancelTokenSource;

	private CancellationToken _cancelToken;

	private CancellationToken _parentToken;

	private Task _task;

	private bool _isDisposed;

	public ushort Port => (ushort)((_udp?.Client.LocalEndPoint as IPEndPoint)?.Port ?? 0);

	public event Func<byte[], int, int, Task> ReceivedDatagram;

	public DefaultUdpSocket()
	{
		_lock = new SemaphoreSlim(1, 1);
		_stopCancelTokenSource = new CancellationTokenSource();
	}

	private void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				StopInternalAsync(isDisposing: true).GetAwaiter().GetResult();
				_stopCancelTokenSource?.Dispose();
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

	public async Task StartAsync()
	{
		await _lock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await StartInternalAsync(_cancelToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_lock.Release();
		}
	}

	public async Task StartInternalAsync(CancellationToken cancelToken)
	{
		await StopInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		_stopCancelTokenSource?.Dispose();
		_cancelTokenSource?.Dispose();
		_stopCancelTokenSource = new CancellationTokenSource();
		_cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_parentToken, _stopCancelTokenSource.Token);
		_cancelToken = _cancelTokenSource.Token;
		_udp?.Dispose();
		_udp = new UdpClient(0);
		_task = RunAsync(_cancelToken);
	}

	public async Task StopAsync()
	{
		await _lock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await StopInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_lock.Release();
		}
	}

	public async Task StopInternalAsync(bool isDisposing = false)
	{
		try
		{
			_stopCancelTokenSource.Cancel(throwOnFirstException: false);
		}
		catch
		{
		}
		if (!isDisposing)
		{
			await (_task ?? Task.Delay(0)).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (_udp != null)
		{
			try
			{
				_udp.Dispose();
			}
			catch
			{
			}
			_udp = null;
		}
	}

	public void SetDestination(string ip, int port)
	{
		_destination = new IPEndPoint(IPAddress.Parse(ip), port);
	}

	public void SetCancelToken(CancellationToken cancelToken)
	{
		_cancelTokenSource?.Dispose();
		_parentToken = cancelToken;
		_cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_parentToken, _stopCancelTokenSource.Token);
		_cancelToken = _cancelTokenSource.Token;
	}

	public async Task SendAsync(byte[] data, int index, int count)
	{
		if (index != 0)
		{
			byte[] array = new byte[count];
			Buffer.BlockCopy(data, index, array, 0, count);
			data = array;
		}
		await _udp.SendAsync(data, count, _destination).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task RunAsync(CancellationToken cancelToken)
	{
		Task closeTask = Task.Delay(-1, cancelToken);
		while (!cancelToken.IsCancellationRequested)
		{
			Task<UdpReceiveResult> receiveTask = _udp.ReceiveAsync();
			receiveTask.ContinueWith(delegate(Task<UdpReceiveResult> receiveResult)
			{
				_ = receiveResult.Exception;
			}, TaskContinuationOptions.OnlyOnFaulted);
			if (await Task.WhenAny(closeTask, receiveTask).ConfigureAwait(continueOnCapturedContext: false) != closeTask)
			{
				UdpReceiveResult result = receiveTask.Result;
				await this.ReceivedDatagram(result.Buffer, 0, result.Buffer.Length).ConfigureAwait(continueOnCapturedContext: false);
				continue;
			}
			break;
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord.Logging;

namespace Discord.Audio.Streams;

internal class BufferedWriteStream : AudioOutStream
{
	private struct Frame(byte[] buffer, int bytes)
	{
		public readonly byte[] Buffer = buffer;

		public readonly int Bytes = bytes;
	}

	private const int MaxSilenceFrames = 10;

	private static readonly byte[] _silenceFrame = new byte[0];

	private readonly AudioClient _client;

	private readonly AudioStream _next;

	private readonly CancellationTokenSource _disposeTokenSource;

	private readonly CancellationTokenSource _cancelTokenSource;

	private readonly CancellationToken _cancelToken;

	private readonly Task _task;

	private readonly ConcurrentQueue<Frame> _queuedFrames;

	private readonly ConcurrentQueue<byte[]> _bufferPool;

	private readonly SemaphoreSlim _queueLock;

	private readonly Logger _logger;

	private readonly int _ticksPerFrame;

	private readonly int _queueLength;

	private bool _isPreloaded;

	private int _silenceFrames;

	public BufferedWriteStream(AudioStream next, IAudioClient client, int bufferMillis, CancellationToken cancelToken, int maxFrameSize = 1500)
		: this(next, client as AudioClient, bufferMillis, cancelToken, null, maxFrameSize)
	{
	}

	internal BufferedWriteStream(AudioStream next, AudioClient client, int bufferMillis, CancellationToken cancelToken, Logger logger, int maxFrameSize = 1500)
	{
		_next = next;
		_client = client;
		_ticksPerFrame = 20;
		_logger = logger;
		_queueLength = (bufferMillis + (_ticksPerFrame - 1)) / _ticksPerFrame;
		_disposeTokenSource = new CancellationTokenSource();
		_cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_disposeTokenSource.Token, cancelToken);
		_cancelToken = _cancelTokenSource.Token;
		_queuedFrames = new ConcurrentQueue<Frame>();
		_bufferPool = new ConcurrentQueue<byte[]>();
		for (int i = 0; i < _queueLength; i++)
		{
			_bufferPool.Enqueue(new byte[maxFrameSize]);
		}
		_queueLock = new SemaphoreSlim(_queueLength, _queueLength);
		_silenceFrames = 10;
		_task = Run();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposeTokenSource?.Cancel();
			_disposeTokenSource?.Dispose();
			_cancelTokenSource?.Cancel();
			_cancelTokenSource?.Dispose();
			_queueLock?.Dispose();
			_next.Dispose();
		}
		base.Dispose(disposing);
	}

	private Task Run()
	{
		return Task.Run(async delegate
		{
			_ = 5;
			try
			{
				while (!_isPreloaded && !_cancelToken.IsCancellationRequested)
				{
					await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				}
				long nextTick = Environment.TickCount;
				ushort seq = 0;
				uint timestamp = 0u;
				while (!_cancelToken.IsCancellationRequested)
				{
					long tick = Environment.TickCount;
					long num = nextTick - tick;
					if (num <= 0)
					{
						if (_queuedFrames.TryDequeue(out var frame))
						{
							await _client.SetSpeakingAsync(value: true).ConfigureAwait(continueOnCapturedContext: false);
							_next.WriteHeader(seq, timestamp, missed: false);
							await _next.WriteAsync(frame.Buffer, 0, frame.Bytes).ConfigureAwait(continueOnCapturedContext: false);
							_bufferPool.Enqueue(frame.Buffer);
							_queueLock.Release();
							nextTick += _ticksPerFrame;
							seq++;
							timestamp += 960;
							_silenceFrames = 0;
						}
						else
						{
							while (nextTick - tick <= 0)
							{
								if (_silenceFrames++ < 10)
								{
									_next.WriteHeader(seq, timestamp, missed: false);
									await _next.WriteAsync(_silenceFrame, 0, _silenceFrame.Length).ConfigureAwait(continueOnCapturedContext: false);
								}
								else
								{
									await _client.SetSpeakingAsync(value: false).ConfigureAwait(continueOnCapturedContext: false);
								}
								nextTick += _ticksPerFrame;
								seq++;
								timestamp += 960;
							}
						}
						frame = default(Frame);
					}
					else
					{
						await Task.Delay((int)num).ConfigureAwait(continueOnCapturedContext: false);
					}
				}
			}
			catch (OperationCanceledException)
			{
			}
		});
	}

	public override void WriteHeader(ushort seq, uint timestamp, bool missed)
	{
	}

	public override async Task WriteAsync(byte[] data, int offset, int count, CancellationToken cancelToken)
	{
		CancellationTokenSource writeCancelToken = null;
		if (cancelToken.CanBeCanceled)
		{
			writeCancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, _cancelToken);
			cancelToken = writeCancelToken.Token;
		}
		else
		{
			cancelToken = _cancelToken;
		}
		await _queueLock.WaitAsync(-1, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
		if (!_bufferPool.TryDequeue(out var result))
		{
			writeCancelToken?.Dispose();
			return;
		}
		Buffer.BlockCopy(data, offset, result, 0, count);
		_queuedFrames.Enqueue(new Frame(result, count));
		if (!_isPreloaded && _queuedFrames.Count == _queueLength)
		{
			_isPreloaded = true;
		}
		writeCancelToken?.Dispose();
	}

	public override async Task FlushAsync(CancellationToken cancelToken)
	{
		while (true)
		{
			cancelToken.ThrowIfCancellationRequested();
			if (_queuedFrames.Count == 0)
			{
				break;
			}
			await Task.Delay(250, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public override Task ClearAsync(CancellationToken cancelToken)
	{
		Frame result;
		do
		{
			cancelToken.ThrowIfCancellationRequested();
		}
		while (_queuedFrames.TryDequeue(out result));
		return Task.Delay(0);
	}
}

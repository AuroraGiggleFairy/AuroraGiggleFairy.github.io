using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Audio.Streams;

internal class InputStream : AudioInStream
{
	private const int MaxFrames = 100;

	private ConcurrentQueue<RTPFrame> _frames;

	private SemaphoreSlim _signal;

	private ushort _nextSeq;

	private uint _nextTimestamp;

	private bool _nextMissed;

	private bool _hasHeader;

	private bool _isDisposed;

	public override bool CanRead => !_isDisposed;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override int AvailableFrames => _signal.CurrentCount;

	public InputStream()
	{
		_frames = new ConcurrentQueue<RTPFrame>();
		_signal = new SemaphoreSlim(0, 100);
	}

	public override bool TryReadFrame(CancellationToken cancelToken, out RTPFrame frame)
	{
		cancelToken.ThrowIfCancellationRequested();
		if (_signal.Wait(0))
		{
			_frames.TryDequeue(out frame);
			return true;
		}
		frame = default(RTPFrame);
		return false;
	}

	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancelToken)
	{
		cancelToken.ThrowIfCancellationRequested();
		RTPFrame rTPFrame = await ReadFrameAsync(cancelToken).ConfigureAwait(continueOnCapturedContext: false);
		if (count < rTPFrame.Payload.Length)
		{
			throw new InvalidOperationException("Buffer is too small.");
		}
		Buffer.BlockCopy(rTPFrame.Payload, 0, buffer, offset, rTPFrame.Payload.Length);
		return rTPFrame.Payload.Length;
	}

	public override async Task<RTPFrame> ReadFrameAsync(CancellationToken cancelToken)
	{
		cancelToken.ThrowIfCancellationRequested();
		await _signal.WaitAsync(cancelToken).ConfigureAwait(continueOnCapturedContext: false);
		_frames.TryDequeue(out var result);
		return result;
	}

	public override void WriteHeader(ushort seq, uint timestamp, bool missed)
	{
		if (_hasHeader)
		{
			throw new InvalidOperationException("Header received with no payload");
		}
		_hasHeader = true;
		_nextSeq = seq;
		_nextTimestamp = timestamp;
		_nextMissed = missed;
	}

	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancelToken)
	{
		cancelToken.ThrowIfCancellationRequested();
		if (_signal.CurrentCount >= 100)
		{
			_hasHeader = false;
			return Task.Delay(0);
		}
		if (!_hasHeader)
		{
			throw new InvalidOperationException("Received payload without an RTP header");
		}
		_hasHeader = false;
		byte[] array = new byte[count];
		Buffer.BlockCopy(buffer, offset, array, 0, count);
		ConcurrentQueue<RTPFrame> frames = _frames;
		ushort nextSeq = _nextSeq;
		uint nextTimestamp = _nextTimestamp;
		bool nextMissed = _nextMissed;
		frames.Enqueue(new RTPFrame(nextSeq, nextTimestamp, array, nextMissed));
		_signal.Release();
		return Task.Delay(0);
	}

	protected override void Dispose(bool isDisposing)
	{
		if (!_isDisposed)
		{
			if (isDisposing)
			{
				_signal?.Dispose();
			}
			_isDisposed = true;
		}
		base.Dispose(isDisposing);
	}
}

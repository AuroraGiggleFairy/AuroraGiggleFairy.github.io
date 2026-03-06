using System.Threading;
using System.Threading.Tasks;

namespace Discord.Audio.Streams;

internal class RTPReadStream : AudioOutStream
{
	private readonly AudioStream _next;

	private readonly byte[] _buffer;

	private readonly byte[] _nonce;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

	public RTPReadStream(AudioStream next, int bufferSize = 4000)
	{
		_next = next;
		_buffer = new byte[bufferSize];
		_nonce = new byte[24];
	}

	public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancelToken)
	{
		cancelToken.ThrowIfCancellationRequested();
		int headerSize = GetHeaderSize(buffer, offset);
		ushort seq = (ushort)((buffer[offset + 2] << 8) | buffer[offset + 3]);
		uint timestamp = (uint)((buffer[offset + 4] << 24) | (buffer[offset + 5] << 16) | (buffer[offset + 6] << 8) | buffer[offset + 7]);
		_next.WriteHeader(seq, timestamp, missed: false);
		await _next.WriteAsync(buffer, offset + headerSize, count - headerSize, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static bool TryReadSsrc(byte[] buffer, int offset, out uint ssrc)
	{
		ssrc = 0u;
		if (buffer.Length - offset < 12)
		{
			return false;
		}
		if ((buffer[offset] & 0xC0) >> 6 != 2)
		{
			return false;
		}
		if ((buffer[offset + 1] & 0xFF) != 120)
		{
			return false;
		}
		ssrc = (uint)((buffer[offset + 8] << 24) | (buffer[offset + 9] << 16) | (buffer[offset + 10] << 8) | buffer[offset + 11]);
		return true;
	}

	public static int GetHeaderSize(byte[] buffer, int offset)
	{
		byte num = buffer[offset];
		bool flag = (num & 0x10) != 0;
		int num2 = (num & 0xF) >> 4;
		if (!flag)
		{
			return 12 + num2 * 4;
		}
		int num3 = offset + 12 + num2 * 4;
		int num4 = (buffer[num3 + 2] << 8) | buffer[num3 + 3];
		return num3 + 4 + num4 * 4;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_next.Dispose();
		}
		base.Dispose(disposing);
	}
}

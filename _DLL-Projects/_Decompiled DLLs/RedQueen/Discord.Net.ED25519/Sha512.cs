using System;

namespace Discord.Net.ED25519;

internal class Sha512
{
	private Array8<ulong> _state;

	private readonly byte[] _buffer;

	private ulong _totalBytes;

	public const int BlockSize = 128;

	private static readonly byte[] _padding = new byte[1] { 128 };

	public Sha512()
	{
		_buffer = new byte[128];
		Init();
	}

	public void Init()
	{
		Sha512Internal.Sha512Init(out _state);
		_totalBytes = 0uL;
	}

	public void Update(ArraySegment<byte> data)
	{
		Update(data.Array, data.Offset, data.Count);
	}

	public void Update(byte[] data, int index, int length)
	{
		int num = (int)_totalBytes & 0x7F;
		_totalBytes += (uint)length;
		if (_totalBytes >= 2305843009213693951L)
		{
			throw new InvalidOperationException("Too much data");
		}
		Array16<ulong> output;
		if (num != 0)
		{
			int num2 = Math.Min(128 - num, length);
			Buffer.BlockCopy(data, index, _buffer, num, num2);
			index += num2;
			length -= num2;
			num += num2;
			if (num == 128)
			{
				ByteIntegerConverter.Array16LoadBigEndian64(out output, _buffer, 0);
				Sha512Internal.Core(out _state, ref _state, ref output);
				CryptoBytes.InternalWipe(_buffer, 0, _buffer.Length);
				num = 0;
			}
		}
		while (length >= 128)
		{
			ByteIntegerConverter.Array16LoadBigEndian64(out output, data, index);
			Sha512Internal.Core(out _state, ref _state, ref output);
			index += 128;
			length -= 128;
		}
		if (length > 0)
		{
			Buffer.BlockCopy(data, index, _buffer, num, length);
		}
	}

	public void Finalize(ArraySegment<byte> output)
	{
		Preconditions.NotNull(output.Array, "output");
		if (output.Count != 64)
		{
			throw new ArgumentException("Output should be 64 in length");
		}
		Update(_padding, 0, _padding.Length);
		ByteIntegerConverter.Array16LoadBigEndian64(out var output2, _buffer, 0);
		CryptoBytes.InternalWipe(_buffer, 0, _buffer.Length);
		if (((int)_totalBytes & 0x7F) > 112)
		{
			Sha512Internal.Core(out _state, ref _state, ref output2);
			output2 = default(Array16<ulong>);
		}
		output2.x15 = (_totalBytes - 1) * 8;
		Sha512Internal.Core(out _state, ref _state, ref output2);
		ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset, _state.x0);
		ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 8, _state.x1);
		ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 16, _state.x2);
		ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 24, _state.x3);
		ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 32, _state.x4);
		ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 40, _state.x5);
		ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 48, _state.x6);
		ByteIntegerConverter.StoreBigEndian64(output.Array, output.Offset + 56, _state.x7);
		_state = default(Array8<ulong>);
	}

	public byte[] Finalize()
	{
		byte[] array = new byte[64];
		Finalize(new ArraySegment<byte>(array));
		return array;
	}

	public static byte[] Hash(byte[] data)
	{
		return Hash(data, 0, data.Length);
	}

	public static byte[] Hash(byte[] data, int index, int length)
	{
		Sha512 sha = new Sha512();
		sha.Update(data, index, length);
		return sha.Finalize();
	}
}

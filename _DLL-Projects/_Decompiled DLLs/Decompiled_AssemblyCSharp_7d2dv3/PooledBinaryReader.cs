using System;
using System.IO;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

public class PooledBinaryReader : BinaryReader, IBinaryReaderOrWriter, IMemoryPoolableObject, IDisposable
{
	public readonly struct StreamReadSizeMarker(long _position, uint _expectedSize)
	{
		public readonly long Position = _position;

		public readonly uint ExpectedSize = _expectedSize;
	}

	public static int INSTANCES_LIVE;

	public static int INSTANCES_MAX;

	public static int INSTANCES_CREATED;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int BYTE_BUFFER_SIZE = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CHAR_BUFFER_SIZE = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] buffer = new byte[128];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly char[] charBuffer = new char[128];

	[PublicizedFrom(EAccessModifier.Private)]
	public Decoder decoder;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder stringBuilder = new StringBuilder(128);

	[PublicizedFrom(EAccessModifier.Private)]
	public Stream baseStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public Encoding encoding;

	public override Stream BaseStream => baseStream;

	public Encoding Encoding
	{
		get
		{
			return encoding;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			encoding = value;
			decoder = null;
		}
	}

	public PooledBinaryReader()
		: base(Stream.Null)
	{
		Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
		Interlocked.Increment(ref INSTANCES_CREATED);
		Interlocked.Increment(ref INSTANCES_LIVE);
		if (INSTANCES_LIVE > INSTANCES_MAX)
		{
			Interlocked.Exchange(ref INSTANCES_MAX, INSTANCES_LIVE);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~PooledBinaryReader()
	{
		Interlocked.Decrement(ref INSTANCES_LIVE);
	}

	public void SetBaseStream(Stream _stream)
	{
		if (_stream != null && !_stream.CanRead)
		{
			throw new ArgumentException("The stream doesn't support reading.");
		}
		baseStream = _stream;
		decoder = null;
	}

	public override int PeekChar()
	{
		if (baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		if (!baseStream.CanSeek)
		{
			return -1;
		}
		int _bytesRead;
		int num = ReadCharBytes(charBuffer, 0, 1, out _bytesRead);
		baseStream.Position -= _bytesRead;
		if (num == 0)
		{
			return -1;
		}
		return charBuffer[0];
	}

	public override int Read()
	{
		if (Read(charBuffer, 0, 1) == 0)
		{
			return -1;
		}
		return charBuffer[0];
	}

	public override bool ReadBoolean()
	{
		return ReadByte() != 0;
	}

	public override byte ReadByte()
	{
		if (baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		int num = baseStream.ReadByte();
		if (num == -1)
		{
			throw new EndOfStreamException();
		}
		return (byte)num;
	}

	public override sbyte ReadSByte()
	{
		return (sbyte)ReadByte();
	}

	public override char ReadChar()
	{
		int num = Read();
		if (num == -1)
		{
			throw new EndOfStreamException();
		}
		return (char)num;
	}

	public override short ReadInt16()
	{
		FillBuffer(2);
		return (short)(buffer[0] | (buffer[1] << 8));
	}

	public override ushort ReadUInt16()
	{
		FillBuffer(2);
		return (ushort)(buffer[0] | (buffer[1] << 8));
	}

	public override int ReadInt32()
	{
		FillBuffer(4);
		return buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
	}

	public override uint ReadUInt32()
	{
		FillBuffer(4);
		return (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
	}

	public override long ReadInt64()
	{
		FillBuffer(8);
		uint num = (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
		return (long)(((ulong)(uint)(buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24)) << 32) | num);
	}

	public override ulong ReadUInt64()
	{
		FillBuffer(8);
		uint num = (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
		return ((ulong)(uint)(buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24)) << 32) | num;
	}

	public override float ReadSingle()
	{
		FillBuffer(4);
		return BitConverterLE.ToSingle(buffer, 0);
	}

	public override double ReadDouble()
	{
		FillBuffer(8);
		return BitConverterLE.ToDouble(buffer, 0);
	}

	public unsafe override decimal ReadDecimal()
	{
		FillBuffer(16);
		decimal result = default(decimal);
		byte* ptr = (byte*)(&result);
		if (BitConverter.IsLittleEndian)
		{
			for (int i = 0; i < 16; i++)
			{
				if (i < 4)
				{
					ptr[i + 8] = buffer[i];
				}
				else if (i < 8)
				{
					ptr[i + 8] = buffer[i];
				}
				else if (i < 12)
				{
					ptr[i - 4] = buffer[i];
				}
				else if (i < 16)
				{
					ptr[i - 12] = buffer[i];
				}
			}
		}
		else
		{
			for (int j = 0; j < 16; j++)
			{
				if (j < 4)
				{
					ptr[11 - j] = buffer[j];
				}
				else if (j < 8)
				{
					ptr[19 - j] = buffer[j];
				}
				else if (j < 12)
				{
					ptr[15 - j] = buffer[j];
				}
				else if (j < 16)
				{
					ptr[15 - j] = buffer[j];
				}
			}
		}
		return result;
	}

	public override string ReadString()
	{
		int num = Read7BitEncodedInt();
		if (num < 0)
		{
			throw new IOException("Invalid binary file (string len < 0)");
		}
		if (num == 0)
		{
			return string.Empty;
		}
		stringBuilder.Length = 0;
		stringBuilder.EnsureCapacity(num);
		do
		{
			int num2 = ((num <= 128) ? num : 128);
			FillBuffer(num2);
			if (decoder == null)
			{
				decoder = encoding.GetDecoder();
			}
			int chars = decoder.GetChars(buffer, 0, num2, charBuffer, 0);
			stringBuilder.Append(charBuffer, 0, chars);
			num -= num2;
		}
		while (num > 0);
		return stringBuilder.ToString();
	}

	public override int Read(char[] _buffer, int _index, int _count)
	{
		if (baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		if (_buffer == null)
		{
			throw new ArgumentNullException("_buffer", "_buffer is null");
		}
		if (_index < 0)
		{
			throw new ArgumentOutOfRangeException("_index", "_index is less than 0");
		}
		if (_count < 0)
		{
			throw new ArgumentOutOfRangeException("_count", "_count is less than 0");
		}
		if (_buffer.Length - _index < _count)
		{
			throw new ArgumentException("buffer is too small");
		}
		int _bytesRead;
		return ReadCharBytes(_buffer, _index, _count, out _bytesRead);
	}

	[Obsolete("char[] ReadChars (int) allocates memory. Try using int Read (char[], int, int) instead.")]
	public override char[] ReadChars(int _count)
	{
		if (_count < 0)
		{
			throw new ArgumentOutOfRangeException("_count", "_count is less than 0");
		}
		if (_count == 0)
		{
			return new char[0];
		}
		char[] array = new char[_count];
		int num = Read(array, 0, _count);
		if (num == 0)
		{
			throw new EndOfStreamException();
		}
		if (num != array.Length)
		{
			char[] array2 = new char[num];
			Array.Copy(array, 0, array2, 0, num);
			return array2;
		}
		return array;
	}

	public override int Read(Span<char> _buffer)
	{
		if (baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		int _bytesRead;
		return ReadCharBytes(_buffer, out _bytesRead);
	}

	public override int Read(Span<byte> _buffer)
	{
		if (baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		return baseStream.Read(_buffer);
	}

	public override int Read(byte[] _buffer, int _index, int _count)
	{
		if (baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		if (_buffer == null)
		{
			throw new ArgumentNullException("_buffer", "_buffer is null");
		}
		if (_index < 0)
		{
			throw new ArgumentOutOfRangeException("_index", "_index is less than 0");
		}
		if (_count < 0)
		{
			throw new ArgumentOutOfRangeException("_count", "_count is less than 0");
		}
		if (_buffer.Length - _index < _count)
		{
			throw new ArgumentException("buffer is too small");
		}
		return baseStream.Read(_buffer, _index, _count);
	}

	[Obsolete("byte[] ReadBytes (int) allocates memory. Try using int Read (byte[], int, int) instead.")]
	public override byte[] ReadBytes(int _count)
	{
		if (baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		if (_count < 0)
		{
			throw new ArgumentOutOfRangeException("_count", "_count is less than 0");
		}
		byte[] array = new byte[_count];
		int i;
		int num;
		for (i = 0; i < _count; i += num)
		{
			num = baseStream.Read(array, i, _count - i);
			if (num == 0)
			{
				break;
			}
		}
		if (i != _count)
		{
			byte[] array2 = new byte[i];
			Buffer.BlockCopy(array, 0, array2, 0, i);
			return array2;
		}
		return array;
	}

	public int Read7BitEncodedSignedInt()
	{
		int num = 0;
		int num2 = 0;
		byte b = ReadByte();
		num |= b & 0x3F;
		num2 += 6;
		bool flag = (b & 0x40) != 0;
		if ((b & 0x80) == 0)
		{
			if (!flag)
			{
				return num;
			}
			return -num;
		}
		while (num2 < 32)
		{
			b = ReadByte();
			num |= (b & 0x7F) << num2;
			num2 += 7;
			if ((b & 0x80) == 0)
			{
				if (!flag)
				{
					return num;
				}
				return -num;
			}
		}
		throw new FormatException("Illegal encoding for 7 bit encoded int");
	}

	public new int Read7BitEncodedInt()
	{
		int num = 0;
		int num2 = 0;
		while (num2 < 35)
		{
			byte b = ReadByte();
			num |= (b & 0x7F) << num2;
			num2 += 7;
			if ((b & 0x80) == 0)
			{
				return num;
			}
		}
		throw new FormatException("Illegal encoding for 7 bit encoded int");
	}

	[MustUseReturnValue]
	public StreamReadSizeMarker ReadSizeMarker(PooledBinaryWriter.EMarkerSize _markerSize)
	{
		long position = baseStream.Position;
		return new StreamReadSizeMarker(position, _markerSize switch
		{
			PooledBinaryWriter.EMarkerSize.UInt8 => ReadByte(), 
			PooledBinaryWriter.EMarkerSize.UInt16 => ReadUInt16(), 
			PooledBinaryWriter.EMarkerSize.UInt32 => ReadUInt32(), 
			_ => throw new ArgumentOutOfRangeException("_markerSize"), 
		});
	}

	public bool ValidateSizeMarker(ref StreamReadSizeMarker _sizeMarker, out uint _bytesReceived, bool _fixPosition = true)
	{
		long num = baseStream.Position - _sizeMarker.Position;
		_bytesReceived = (uint)num;
		if (num == _sizeMarker.ExpectedSize)
		{
			return true;
		}
		if (_fixPosition)
		{
			baseStream.Position = _sizeMarker.Position + _sizeMarker.ExpectedSize;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ReadCharBytes(char[] _targetBuffer, int _targetIndex, int _count, out int _bytesRead)
	{
		return ReadCharBytes(_targetBuffer.AsSpan(_targetIndex, _count), out _bytesRead);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ReadCharBytes(Span<char> _targetBuffer, out int _bytesRead)
	{
		int i = 0;
		_bytesRead = 0;
		for (; i < _targetBuffer.Length; i++)
		{
			int length = 0;
			int chars;
			do
			{
				int num = baseStream.ReadByte();
				if (num == -1)
				{
					return i;
				}
				buffer[length++] = (byte)num;
				_bytesRead++;
				chars = encoding.GetChars(buffer.AsSpan(0, length), _targetBuffer.Slice(i));
			}
			while (chars <= 0);
		}
		return i;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FillBuffer(int _numBytes)
	{
		if (baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		int num;
		for (int i = 0; i < _numBytes; i += num)
		{
			num = baseStream.Read(buffer, i, _numBytes - i);
			if (num == 0)
			{
				throw new EndOfStreamException();
			}
		}
	}

	public void Flush()
	{
		if (baseStream != null)
		{
			baseStream.Flush();
		}
	}

	public override void Close()
	{
	}

	public void Reset()
	{
		stringBuilder.Length = 0;
		baseStream = null;
	}

	public void Cleanup()
	{
		Reset();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Dispose(bool _disposing)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	void IDisposable.Dispose()
	{
		MemoryPools.poolBinaryReader.FreeSync(this);
	}

	public bool ReadWrite(bool _value)
	{
		return ReadBoolean();
	}

	public byte ReadWrite(byte _value)
	{
		return ReadByte();
	}

	public sbyte ReadWrite(sbyte _value)
	{
		return ReadSByte();
	}

	public char ReadWrite(char _value)
	{
		return ReadChar();
	}

	public short ReadWrite(short _value)
	{
		return ReadInt16();
	}

	public ushort ReadWrite(ushort _value)
	{
		return ReadUInt16();
	}

	public int ReadWrite(int _value)
	{
		return ReadInt32();
	}

	public uint ReadWrite(uint _value)
	{
		return ReadUInt32();
	}

	public long ReadWrite(long _value)
	{
		return ReadInt64();
	}

	public ulong ReadWrite(ulong _value)
	{
		return ReadUInt64();
	}

	public float ReadWrite(float _value)
	{
		return ReadSingle();
	}

	public double ReadWrite(double _value)
	{
		return ReadDouble();
	}

	public decimal ReadWrite(decimal _value)
	{
		return ReadDecimal();
	}

	public string ReadWrite(string _value)
	{
		return ReadString();
	}

	public void ReadWrite(byte[] _buffer, int _index, int _count)
	{
		Read(_buffer, _index, _count);
	}

	public Vector3 ReadWrite(Vector3 _value)
	{
		Vector3 result = _value;
		result.x = ReadSingle();
		result.y = ReadSingle();
		result.z = ReadSingle();
		return result;
	}
}

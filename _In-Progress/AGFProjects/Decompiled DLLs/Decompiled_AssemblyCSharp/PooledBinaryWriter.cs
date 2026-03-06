using System;
using System.IO;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

public class PooledBinaryWriter : BinaryWriter, IBinaryReaderOrWriter, IMemoryPoolableObject, IDisposable
{
	public enum EMarkerSize
	{
		UInt8 = 1,
		UInt16 = 2,
		UInt32 = 4
	}

	public readonly struct StreamWriteSizeMarker(long _position, EMarkerSize _markerSize)
	{
		public readonly long Position = _position;

		public readonly EMarkerSize MarkerSize = _markerSize;
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
	public int maxBytesPerChar;

	[PublicizedFrom(EAccessModifier.Private)]
	public Encoder encoder;

	[PublicizedFrom(EAccessModifier.Private)]
	public Encoding encoding;

	public Encoding Encoding
	{
		get
		{
			return encoding;
		}
		set
		{
			encoding = value ?? throw new ArgumentNullException("value");
			encoder = null;
			maxBytesPerChar = encoding.GetMaxByteCount(1);
		}
	}

	public override Stream BaseStream => OutStream;

	public PooledBinaryWriter()
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
	~PooledBinaryWriter()
	{
		Interlocked.Decrement(ref INSTANCES_LIVE);
	}

	public void SetBaseStream(Stream _stream)
	{
		if (_stream != null && !_stream.CanWrite)
		{
			throw new ArgumentException("Stream does not support writing or already closed.");
		}
		OutStream = _stream;
		encoder = null;
	}

	public override void Flush()
	{
		OutStream.Flush();
	}

	public override long Seek(int _offset, SeekOrigin _origin)
	{
		return OutStream.Seek(_offset, _origin);
	}

	public override void Write(bool _value)
	{
		buffer[0] = (byte)(_value ? 1u : 0u);
		OutStream.Write(buffer, 0, 1);
	}

	public override void Write(byte _value)
	{
		OutStream.WriteByte(_value);
	}

	public override void Write(byte[] _buffer)
	{
		if (_buffer == null)
		{
			throw new ArgumentNullException("_buffer");
		}
		OutStream.Write(_buffer, 0, _buffer.Length);
	}

	public override void Write(byte[] _buffer, int _index, int _count)
	{
		if (_buffer == null)
		{
			throw new ArgumentNullException("_buffer");
		}
		OutStream.Write(_buffer, _index, _count);
	}

	// ...existing code...

	public override void Write(char _ch)
	{
		charBuffer[0] = _ch;
		int bytes = encoding.GetBytes(charBuffer, 0, 1, buffer, 0);
		OutStream.Write(buffer, 0, bytes);
	}

	public override void Write(char[] _chars)
	{
		Write(_chars, 0, _chars.Length);
	}

	public override void Write(char[] _chars, int _index, int _count)
	{
		if (_chars == null)
		{
			throw new ArgumentNullException("_chars");
		}
		int num;
		for (int i = _index; i < _index + _count; i += num)
		{
			num = Math.Min(128 / maxBytesPerChar, (_index + _count) - i);
			int bytes = encoding.GetBytes(_chars, i, num, buffer, 0);
			OutStream.Write(buffer, 0, bytes);
		}
	}

	// ...existing code...

	public unsafe override void Write(decimal _value)
	{
		byte* ptr = (byte*)(&_value);
		if (BitConverter.IsLittleEndian)
		{
			for (int i = 0; i < 16; i++)
			{
				if (i < 4)
				{
					buffer[i + 12] = ptr[i];
				}
				else if (i < 8)
				{
					buffer[i + 4] = ptr[i];
				}
				else if (i < 12)
				{
					buffer[i - 8] = ptr[i];
				}
				else
				{
					buffer[i - 8] = ptr[i];
				}
			}
		}
		else
		{
			for (int j = 0; j < 16; j++)
			{
				if (j < 4)
				{
					buffer[15 - j] = ptr[j];
				}
				else if (j < 8)
				{
					buffer[15 - j] = ptr[j];
				}
				else if (j < 12)
				{
					buffer[11 - j] = ptr[j];
				}
				else
				{
					buffer[19 - j] = ptr[j];
				}
			}
		}
		OutStream.Write(buffer, 0, 16);
	}

	public override void Write(double _value)
	{
		BitConverterLE.GetBytes(_value, buffer);
		OutStream.Write(buffer, 0, 8);
	}

	public override void Write(short _value)
	{
		buffer[0] = (byte)_value;
		buffer[1] = (byte)(_value >> 8);
		OutStream.Write(buffer, 0, 2);
	}

	public override void Write(int _value)
	{
		buffer[0] = (byte)_value;
		buffer[1] = (byte)(_value >> 8);
		buffer[2] = (byte)(_value >> 16);
		buffer[3] = (byte)(_value >> 24);
		OutStream.Write(buffer, 0, 4);
	}

	public override void Write(long _value)
	{
		int num = 0;
		int num2 = 0;
		while (num < 8)
		{
			buffer[num] = (byte)(_value >> num2);
			num++;
			num2 += 8;
		}
		OutStream.Write(buffer, 0, 8);
	}

	public override void Write(sbyte _value)
	{
		buffer[0] = (byte)_value;
		OutStream.Write(buffer, 0, 1);
	}

	public override void Write(float _value)
	{
		BitConverterLE.GetBytes(_value, buffer);
		OutStream.Write(buffer, 0, 4);
	}

	public unsafe override void Write(string _value)
	{
		if (_value == null)
		{
			throw new ArgumentNullException("_value");
		}
		if (encoder == null)
		{
			encoder = encoding.GetEncoder();
		}
		int byteCount;
		fixed (char* chars = _value)
		{
			byteCount = encoder.GetByteCount(chars, _value.Length, flush: true);
		}
		Write7BitEncodedInt(byteCount);
		int num = 128 / maxBytesPerChar;
		int num2 = 0;
		int num3 = _value.Length;
		while (num3 > 0)
		{
			int num4 = ((num3 <= num) ? num3 : num);
			int bytes2;
			fixed (char* ptr = _value)
			{
				fixed (byte* bytes = buffer)
				{
					bytes2 = encoder.GetBytes((char*)(void*)((UIntPtr)ptr + num2 * 2), num4, bytes, 128, num4 == num3);
				}
			}
			OutStream.Write(buffer, 0, bytes2);
			num2 += num4;
			num3 -= num4;
		}
	}

	public override void Write(ushort _value)
	{
		buffer[0] = (byte)_value;
		buffer[1] = (byte)(_value >> 8);
		OutStream.Write(buffer, 0, 2);
	}

	public override void Write(uint _value)
	{
		buffer[0] = (byte)_value;
		buffer[1] = (byte)(_value >> 8);
		buffer[2] = (byte)(_value >> 16);
		buffer[3] = (byte)(_value >> 24);
		OutStream.Write(buffer, 0, 4);
	}

	public override void Write(ulong _value)
	{
		int num = 0;
		int num2 = 0;
		while (num < 8)
		{
			buffer[num] = (byte)(_value >> num2);
			num++;
			num2 += 8;
		}
		OutStream.Write(buffer, 0, 8);
	}

	public void Write7BitEncodedSignedInt(int _value)
	{
		long num = _value;
		bool num2 = num < 0;
		num = Math.Abs(num);
		long num3 = (num >> 6) & 0x1FFFFFF;
		byte b = (byte)(num & 0x3F);
		if (num3 != 0L)
		{
			b |= 0x80;
		}
		if (num2)
		{
			b |= 0x40;
		}
		Write(b);
		for (num = num3; num != 0L; num = num3)
		{
			num3 = (num >> 7) & 0xFFFFFF;
			b = (byte)(num & 0x7F);
			if (num3 != 0L)
			{
				b |= 0x80;
			}
			Write(b);
		}
	}

	public new void Write7BitEncodedInt(int _value)
	{
		do
		{
			int num = (_value >> 7) & 0xFFFFFF;
			byte b = (byte)(_value & 0x7F);
			if (num != 0)
			{
				b |= 0x80;
			}
			Write(b);
			_value = num;
		}
		while (_value != 0);
	}

	[MustUseReturnValue]
	public StreamWriteSizeMarker ReserveSizeMarker(EMarkerSize _markerSize)
	{
		long position = OutStream.Position;
		Array.Clear(buffer, 0, (int)_markerSize);
		OutStream.Write(buffer, 0, (int)_markerSize);
		return new StreamWriteSizeMarker(position, _markerSize);
	}

	public void FinalizeSizeMarker(ref StreamWriteSizeMarker _sizeMarker)
	{
		long position = OutStream.Position;
		long num = position - _sizeMarker.Position;
		if (num < 0)
		{
			throw new Exception($"FinalizeMarker position ({position}) before Reserved position ({_sizeMarker.Position})");
		}
		long num2 = (uint)(_sizeMarker.MarkerSize switch
		{
			EMarkerSize.UInt8 => 255, 
			EMarkerSize.UInt16 => 65535, 
			EMarkerSize.UInt32 => -1, 
			_ => throw new ArgumentOutOfRangeException("MarkerSize"), 
		});
		if (num > num2)
		{
			throw new Exception($"Marked size ({num}) exceeding marker type ({_sizeMarker.MarkerSize.ToStringCached()} maximum ({num2})");
		}
		OutStream.Position = _sizeMarker.Position;
		switch (_sizeMarker.MarkerSize)
		{
		case EMarkerSize.UInt8:
			Write((byte)num);
			break;
		case EMarkerSize.UInt16:
			Write((ushort)num);
			break;
		case EMarkerSize.UInt32:
			Write((uint)num);
			break;
		}
		OutStream.Position = position;
	}

	public override void Close()
	{
	}

	public void Reset()
	{
		SetBaseStream(null);
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
		MemoryPools.poolBinaryWriter.FreeSync(this);
	}

	public bool ReadWrite(bool _value)
	{
		Write(_value);
		return _value;
	}

	public byte ReadWrite(byte _value)
	{
		Write(_value);
		return _value;
	}

	public sbyte ReadWrite(sbyte _value)
	{
		Write(_value);
		return _value;
	}

	public char ReadWrite(char _value)
	{
		Write(_value);
		return _value;
	}

	public short ReadWrite(short _value)
	{
		Write(_value);
		return _value;
	}

	public ushort ReadWrite(ushort _value)
	{
		Write(_value);
		return _value;
	}

	public int ReadWrite(int _value)
	{
		Write(_value);
		return _value;
	}

	public uint ReadWrite(uint _value)
	{
		Write(_value);
		return _value;
	}

	public long ReadWrite(long _value)
	{
		Write(_value);
		return _value;
	}

	public ulong ReadWrite(ulong _value)
	{
		Write(_value);
		return _value;
	}

	public float ReadWrite(float _value)
	{
		Write(_value);
		return _value;
	}

	public double ReadWrite(double _value)
	{
		Write(_value);
		return _value;
	}

	public decimal ReadWrite(decimal _value)
	{
		Write(_value);
		return _value;
	}

	public string ReadWrite(string _value)
	{
		Write(_value);
		return _value;
	}

	public void ReadWrite(byte[] _buffer, int _index, int _count)
	{
		Write(_buffer, _index, _count);
	}

	public Vector3 ReadWrite(Vector3 _value)
	{
		Write(_value.x);
		Write(_value.y);
		Write(_value.z);
		return _value;
	}
}

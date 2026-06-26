using System;
using System.IO;
using UnityEngine;

public class StreamUtils
{
	public static long ReadInt64(Stream clientStream)
	{
		return (clientStream.ReadByte() & 0xFF) | ((long)(clientStream.ReadByte() & 0xFF) << 8) | ((long)(clientStream.ReadByte() & 0xFF) << 16) | ((long)(clientStream.ReadByte() & 0xFF) << 24) | ((long)(clientStream.ReadByte() & 0xFF) << 32) | ((long)(clientStream.ReadByte() & 0xFF) << 40) | ((long)(clientStream.ReadByte() & 0xFF) << 48) | ((long)(clientStream.ReadByte() & 0xFF) << 56);
	}

	public static void Write(Stream clientStream, long v)
	{
		clientStream.WriteByte((byte)(v & 0xFF));
		clientStream.WriteByte((byte)((v >> 8) & 0xFF));
		clientStream.WriteByte((byte)((v >> 16) & 0xFF));
		clientStream.WriteByte((byte)((v >> 24) & 0xFF));
		clientStream.WriteByte((byte)((v >> 32) & 0xFF));
		clientStream.WriteByte((byte)((v >> 40) & 0xFF));
		clientStream.WriteByte((byte)((v >> 48) & 0xFF));
		clientStream.WriteByte((byte)((v >> 56) & 0xFF));
	}

	public static int ReadInt32(Stream clientStream)
	{
		return 0 | (clientStream.ReadByte() & 0xFF) | ((clientStream.ReadByte() & 0xFF) << 8) | ((clientStream.ReadByte() & 0xFF) << 16) | ((clientStream.ReadByte() & 0xFF) << 24);
	}

	public static int ReadInt32(byte[] buffer, ref int offset)
	{
		return 0 | buffer[offset++] | (buffer[offset++] << 8) | (buffer[offset++] << 16) | (buffer[offset++] << 24);
	}

	public static void Write(Stream clientStream, int v)
	{
		clientStream.WriteByte((byte)(v & 0xFF));
		clientStream.WriteByte((byte)((v >> 8) & 0xFF));
		clientStream.WriteByte((byte)((v >> 16) & 0xFF));
		clientStream.WriteByte((byte)((v >> 24) & 0xFF));
	}

	public static ushort ReadUInt16(Stream clientStream)
	{
		return (ushort)((ushort)(0 | (ushort)(clientStream.ReadByte() & 0xFF)) | (ushort)((clientStream.ReadByte() & 0xFF) << 8));
	}

	public static ushort ReadUInt16(byte[] buffer, ref int offset)
	{
		return (ushort)((ushort)(0 | buffer[offset++]) | (ushort)(buffer[offset++] << 8));
	}

	public static void Write(Stream clientStream, ushort v)
	{
		clientStream.WriteByte((byte)(v & 0xFF));
		clientStream.WriteByte((byte)((v >> 8) & 0xFF));
	}

	public static short ReadInt16(Stream clientStream)
	{
		return (short)((short)(0 | (short)(clientStream.ReadByte() & 0xFF)) | (short)((clientStream.ReadByte() & 0xFF) << 8));
	}

	public static short ReadInt16(byte[] buffer, ref int offset)
	{
		return (short)((short)(0 | buffer[offset++]) | (short)(buffer[offset++] << 8));
	}

	public static void Write(Stream clientStream, short v)
	{
		clientStream.WriteByte((byte)(v & 0xFF));
		clientStream.WriteByte((byte)((v >> 8) & 0xFF));
	}

	public static byte ReadByte(Stream clientStream)
	{
		return (byte)clientStream.ReadByte();
	}

	public static byte ReadByte(byte[] buffer, ref int offset)
	{
		return buffer[offset++];
	}

	public static void Write(Stream clientStream, byte _b)
	{
		clientStream.WriteByte(_b);
	}

	public static void Write(BinaryWriter _bw, Vector3 _v)
	{
		_bw.Write(_v.x);
		_bw.Write(_v.y);
		_bw.Write(_v.z);
	}

	public static Vector3 ReadVector3(BinaryReader _br)
	{
		return new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
	}

	public static void Write(BinaryWriter _bw, Vector3i _v)
	{
		_bw.Write(_v.x);
		_bw.Write(_v.y);
		_bw.Write(_v.z);
	}

	public static Vector3i ReadVector3i(BinaryReader _br)
	{
		return new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());
	}

	public static void Write(BinaryWriter _bw, Vector2 _v)
	{
		_bw.Write(_v.x);
		_bw.Write(_v.y);
	}

	public static Vector2 ReadVector2(BinaryReader _br)
	{
		return new Vector2(_br.ReadSingle(), _br.ReadSingle());
	}

	public static void Write(BinaryWriter _bw, Vector2i _v)
	{
		_bw.Write(_v.x);
		_bw.Write(_v.y);
	}

	public static Vector2i ReadVector2i(BinaryReader _br)
	{
		return new Vector2i(_br.ReadInt32(), _br.ReadInt32());
	}

	public static void Write(BinaryWriter _bw, Quaternion _q)
	{
		_bw.Write(_q.x);
		_bw.Write(_q.y);
		_bw.Write(_q.z);
		_bw.Write(_q.w);
	}

	public static Quaternion ReadQuaterion(BinaryReader _br)
	{
		return new Quaternion(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
	}

	public static Color ReadColor(BinaryReader _br)
	{
		return new Color(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
	}

	public static void Write(BinaryWriter _bw, Color _c)
	{
		_bw.Write(_c.r);
		_bw.Write(_c.g);
		_bw.Write(_c.b);
		_bw.Write(_c.a);
	}

	public static Color ReadColor32(BinaryReader _br)
	{
		uint num = _br.ReadUInt32();
		return new Color((float)((num >> 24) & 0xFF) / 255f, (float)((num >> 16) & 0xFF) / 255f, (float)((num >> 8) & 0xFF) / 255f, (float)(num & 0xFF) / 255f);
	}

	public static void WriteColor32(BinaryWriter _bw, Color _c)
	{
		uint value = ((uint)(_c.r * 255f) << 24) | ((uint)(_c.g * 255f) << 16) | ((uint)(_c.b * 255f) << 8) | (uint)(_c.a * 255f);
		_bw.Write(value);
	}

	public static void StreamCopy(Stream _source, Stream _destination, byte[] _tempBuf = null, bool _bFlush = true)
	{
		byte[] array = _tempBuf;
		if (_tempBuf == null)
		{
			array = MemoryPools.poolByte.Alloc(4096);
		}
		bool flag = true;
		while (flag)
		{
			int num = _source.Read(array, 0, array.Length);
			if (num > 0)
			{
				_destination.Write(array, 0, num);
				continue;
			}
			if (_bFlush)
			{
				_destination.Flush();
			}
			flag = false;
		}
		if (_tempBuf == null)
		{
			MemoryPools.poolByte.Free(array);
		}
	}

	public static void StreamCopy(Stream _source, Stream _destination, int _length, byte[] _tempBuf = null, bool _bFlush = true)
	{
		byte[] array = _tempBuf;
		if (_tempBuf == null)
		{
			array = MemoryPools.poolByte.Alloc(4096);
		}
		bool flag = true;
		while (flag)
		{
			int num = _source.Read(array, 0, Math.Min(_length, array.Length));
			if (num > 0)
			{
				_destination.Write(array, 0, num);
				_length -= num;
				continue;
			}
			if (_bFlush)
			{
				_destination.Flush();
			}
			flag = false;
		}
		if (_tempBuf == null)
		{
			MemoryPools.poolByte.Free(array);
		}
	}

	public static void WriteStreamToFile(Stream _source, string _fileName)
	{
		using Stream destination = SdFile.Create(_fileName);
		StreamCopy(_source, destination);
	}

	public static void WriteStreamToFile(Stream _source, string _fileName, int _length)
	{
		using Stream destination = SdFile.Create(_fileName);
		StreamCopy(_source, destination, _length);
	}
}

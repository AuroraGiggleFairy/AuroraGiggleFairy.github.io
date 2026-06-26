using System;
using System.IO;
using System.Text;

namespace SharpEXR;

public class EXRReader : IDisposable, IEXRReader
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BinaryReader reader;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool disposed;

	public int Position
	{
		get
		{
			return (int)reader.BaseStream.Position;
		}
		set
		{
			reader.BaseStream.Seek(value, SeekOrigin.Begin);
		}
	}

	public EXRReader(Stream stream, bool leaveOpen = false)
		: this(new BinaryReader(stream, Encoding.ASCII, leaveOpen))
	{
	}

	public EXRReader(BinaryReader reader)
	{
		this.reader = reader;
	}

	public byte ReadByte()
	{
		return reader.ReadByte();
	}

	public int ReadInt32()
	{
		return reader.ReadInt32();
	}

	public uint ReadUInt32()
	{
		return reader.ReadUInt32();
	}

	public Half ReadHalf()
	{
		return Half.ToHalf(reader.ReadUInt16());
	}

	public float ReadSingle()
	{
		return reader.ReadSingle();
	}

	public double ReadDouble()
	{
		return reader.ReadDouble();
	}

	public string ReadNullTerminatedString(int maxLength)
	{
		long position = reader.BaseStream.Position;
		StringBuilder stringBuilder = new StringBuilder();
		byte value;
		while ((value = reader.ReadByte()) != 0)
		{
			if (reader.BaseStream.Position - position > maxLength)
			{
				throw new EXRFormatException("Null terminated string exceeded maximum length of " + maxLength + " bytes.");
			}
			stringBuilder.Append((char)value);
		}
		return stringBuilder.ToString();
	}

	public string ReadString()
	{
		int length = ReadInt32();
		return ReadString(length);
	}

	public string ReadString(int length)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < length; i++)
		{
			stringBuilder.Append((char)reader.ReadByte());
		}
		return stringBuilder.ToString();
	}

	public byte[] ReadBytes(int count)
	{
		return reader.ReadBytes(count);
	}

	public void CopyBytes(byte[] dest, int offset, int count)
	{
		if (reader.BaseStream.Read(dest, offset, count) != count)
		{
			throw new Exception("Less bytes read than expected");
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}
		if (disposing)
		{
			try
			{
				reader.Dispose();
			}
			catch
			{
			}
		}
		disposed = true;
	}
}

using System;
using System.IO;

public class PrefabInsideDataFile
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSaveVersion = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	public PrefabInsideDataFile()
	{
	}

	public PrefabInsideDataFile(PrefabInsideDataFile _other)
	{
		size = _other.size;
		data = _other.data;
	}

	public void Init(Vector3i _size)
	{
		size = _size;
		data = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Alloc()
	{
		data = new byte[size.x * size.y * size.z + 7 >> 3];
	}

	public void Add(int _offset)
	{
		if (data == null)
		{
			Alloc();
		}
		data[_offset >> 3] |= (byte)(1 << (_offset & 7));
	}

	public void Add(int x, int y, int z)
	{
		int offset = x + y * size.x + z * size.x * size.y;
		Add(offset);
	}

	public bool Contains(int x, int y, int z)
	{
		if (data == null)
		{
			return false;
		}
		int num = x + y * size.x + z * size.x * size.y;
		return (data[num >> 3] & (1 << (num & 7))) > 0;
	}

	public PrefabInsideDataFile Clone()
	{
		return new PrefabInsideDataFile(this);
	}

	public void Load(string _filename, Vector3i _size)
	{
		Init(_size);
		if (!SdFile.Exists(_filename))
		{
			return;
		}
		try
		{
			using Stream baseStream = SdFile.OpenRead(_filename);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			try
			{
				Read(pooledBinaryReader);
			}
			catch (Exception e)
			{
				Log.Error("PrefabInsideDataFile Load {0}, expected data len {1}. Probably outdated ins file, please re-save to fix. Read error:", _filename, data.Length);
				Log.Exception(e);
			}
		}
		catch (Exception e2)
		{
			Log.Exception(e2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Read(BinaryReader _br)
	{
		byte num = _br.ReadByte();
		int num2 = _br.ReadInt32();
		if (num <= 1)
		{
			for (int i = 0; i < num2; i++)
			{
				int x = _br.ReadByte();
				int y = _br.ReadByte();
				int z = _br.ReadByte();
				Add(x, y, z);
			}
		}
		else if (num2 > 0)
		{
			Alloc();
			_br.Read(data, 0, num2);
		}
	}

	public void Save(string _filename)
	{
		try
		{
			using Stream baseStream = SdFile.Open(_filename, FileMode.Create);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			Write(pooledBinaryWriter);
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)2);
		int num = ((data != null) ? data.Length : 0);
		_bw.Write(num);
		if (num > 0)
		{
			_bw.Write(data, 0, num);
		}
	}
}

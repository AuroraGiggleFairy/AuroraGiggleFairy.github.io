using System.IO;

public class ExtensionChunkDatabase : DatabaseWithFixedDS<long, byte[]>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int SizeOfDataSet;

	public ExtensionChunkDatabase(int _magicBytes, int AraSizeX, int AraSizeY, int ChunkSize)
		: base(_magicBytes, 4, AraSizeX * AraSizeY / ChunkSize / ChunkSize, ChunkSize * ChunkSize, -1L, -1)
	{
		SizeOfDataSet = ChunkSize * ChunkSize;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override long readKey(BinaryReader _br)
	{
		return _br.ReadInt64();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void writeKey(BinaryWriter _bw, long _key)
	{
		_bw.Write(_key);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void copyFromRead(byte[] _dataRead, byte[] _data)
	{
		for (int i = 0; i < _data.Length; i++)
		{
			_data[i] = _dataRead[i];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void copyToWrite(byte[] _data, byte[] _dataWrite)
	{
		for (int i = 0; i < _data.Length; i++)
		{
			_dataWrite[i] = _data[i];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override byte[] allocateDataStorage()
	{
		return new byte[SizeOfDataSet];
	}
}

using System;
using System.IO;

public class WaveReader
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string filename;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int bufferSize = 8192;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] buffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int dataStartPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int position;

	public int Position
	{
		set
		{
			position = dataStartPos + value;
		}
	}

	public WaveReader(string _fileName)
	{
		filename = _fileName;
		buffer = MemoryPools.poolByte.Alloc(8192);
		using BinaryReader binaryReader = new BinaryReader(SdFile.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
		string text = new string(binaryReader.ReadChars(88));
		position = (dataStartPos = text.IndexOf("data") + 8);
	}

	public void Read(float[] data, int count)
	{
		using BinaryReader binaryReader = new BinaryReader(SdFile.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
		binaryReader.BaseStream.Position = position;
		binaryReader.Read(buffer, 0, 8192);
		for (int i = 0; i < data.Length; i++)
		{
			short num = BitConverter.ToInt16(buffer, 2 * i);
			data[i] = (float)num / 32767f;
		}
	}

	public void Cleanup()
	{
		MemoryPools.poolByte.Free(buffer);
		buffer = null;
	}
}

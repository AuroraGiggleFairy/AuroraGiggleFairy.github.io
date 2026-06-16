using System.Collections.Generic;
using System.IO;

[PublicizedFrom(EAccessModifier.Internal)]
public class SimpleBitStream
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<byte> data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curBitIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curByteIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte curByteData;

	public SimpleBitStream(int _initialCapacity = 1000)
	{
		data = new List<byte>(_initialCapacity);
		Reset();
	}

	public void Reset()
	{
		data.Clear();
		curBitIdx = 0;
		curByteIdx = 0;
		curByteData = 0;
	}

	public void Add(bool _b)
	{
		if (_b)
		{
			curByteData = (byte)(curByteData | (1 << curBitIdx));
		}
		curBitIdx++;
		if (curBitIdx > 7)
		{
			data.Add(curByteData);
			curBitIdx = 0;
			curByteIdx++;
			curByteData = 0;
		}
	}

	public bool GetNext()
	{
		if (curBitIdx > 7)
		{
			curByteData = data[++curByteIdx];
			curBitIdx = 0;
		}
		bool result = (curByteData & 1) != 0;
		curByteData >>= 1;
		curBitIdx++;
		return result;
	}

	public int GetNextOffset()
	{
		bool flag = false;
		do
		{
			if (curBitIdx > 7)
			{
				curByteIdx++;
				if (curByteIdx >= data.Count)
				{
					return -1;
				}
				curByteData = data[curByteIdx];
				curBitIdx = 0;
			}
			if (curByteData == 0)
			{
				curBitIdx = 8;
				continue;
			}
			flag = (curByteData & 1) == 1;
			curByteData >>= 1;
			curBitIdx++;
		}
		while (!flag);
		return curByteIdx * 8 + curBitIdx - 1;
	}

	public void Write(BinaryWriter _bw)
	{
		if (curBitIdx > 0)
		{
			data.Add(curByteData);
		}
		_bw.Write(data.Count);
		for (int i = 0; i < data.Count; i++)
		{
			_bw.Write(data[i]);
		}
	}

	public void Read(BinaryReader _br)
	{
		int num = _br.ReadInt32();
		byte[] collection = _br.ReadBytes(num);
		data.Clear();
		data.AddRange(collection);
		if (num > 0)
		{
			curByteData = data[0];
		}
	}
}

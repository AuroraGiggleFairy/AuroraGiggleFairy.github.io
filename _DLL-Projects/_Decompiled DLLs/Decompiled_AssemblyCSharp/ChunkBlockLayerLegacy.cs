using System;
using System.IO;

public class ChunkBlockLayerLegacy : IMemoryPoolableObject
{
	public ushort[] m_Lower16Bits;

	public ushort[] m_Upper16Bits;

	[PublicizedFrom(EAccessModifier.Private)]
	public SmartArray m_Stability;

	[PublicizedFrom(EAccessModifier.Private)]
	public int wPow;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hPow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bOnlyTerrain;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bOnlyBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockRefCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tickRefCount;

	public static int InstanceCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cLayerHeight = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cArrSize = 256;

	public ChunkBlockLayerLegacy()
	{
		wPow = 4;
		hPow = 4;
		m_Lower16Bits = new ushort[256];
		m_Stability = new SmartArray(wPow, 0, hPow);
	}

	public BlockValue GetAt(int _x, int _y, int _z)
	{
		return new BlockValue((uint)((m_Upper16Bits != null) ? ((m_Upper16Bits[_x + (_z << wPow)] << 16) | m_Lower16Bits[_x + (_z << wPow)]) : m_Lower16Bits[_x + (_z << wPow)]));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue getAt(int _offs)
	{
		return new BlockValue((uint)((m_Upper16Bits != null) ? ((m_Upper16Bits[_offs] << 16) | m_Lower16Bits[_offs]) : m_Lower16Bits[_offs]));
	}

	public void SetAt(int _x, int _y, int _z, uint _fullBlock)
	{
		int num = _x + (_z << wPow);
		uint typeMasked = BlockValue.GetTypeMasked(m_Lower16Bits[num]);
		m_Lower16Bits[num] = (ushort)(_fullBlock & 0xFFFF);
		if ((_fullBlock & 0xFFFF0000u) != 0)
		{
			if (m_Upper16Bits == null)
			{
				m_Upper16Bits = new ushort[256];
			}
			m_Upper16Bits[num] = (ushort)((_fullBlock >> 16) & 0xFFFF);
		}
		else if (m_Upper16Bits != null)
		{
			m_Upper16Bits[num] = 0;
		}
		if (!Block.BlocksLoaded)
		{
			return;
		}
		uint typeMasked2 = BlockValue.GetTypeMasked(_fullBlock);
		Block block = Block.list[typeMasked];
		Block block2 = Block.list[typeMasked2];
		if (typeMasked == 0 && typeMasked2 != 0)
		{
			blockRefCount++;
			if (block2 != null && block2.IsRandomlyTick)
			{
				tickRefCount++;
			}
		}
		else if (typeMasked != 0 && typeMasked2 == 0)
		{
			blockRefCount--;
			if (block != null && block.IsRandomlyTick)
			{
				tickRefCount--;
			}
		}
		else if (block != null && block.IsRandomlyTick && block2 != null && !block2.IsRandomlyTick)
		{
			tickRefCount--;
		}
		else if (block != null && !block.IsRandomlyTick && block2 != null && block2.IsRandomlyTick)
		{
			tickRefCount++;
		}
		if (bOnlyTerrain && !block2.shape.IsTerrain())
		{
			bOnlyTerrain = false;
		}
	}

	public byte GetStabilityAt(int _x, int _y)
	{
		return m_Stability.get(_x, 0, _y);
	}

	public void SetStabilityAt(int _x, int _y, byte _v)
	{
		m_Stability.set(_x, 0, _y, _v);
	}

	public void Reset()
	{
		Array.Clear(m_Lower16Bits, 0, m_Lower16Bits.Length);
		m_Upper16Bits = null;
		if (m_Stability != null)
		{
			m_Stability.clear();
		}
		blockRefCount = 0;
		tickRefCount = 0;
	}

	public void Cleanup()
	{
	}

	public static int CalcOffset(int _x, int _z)
	{
		return (_x & 0xF) + ((_z & 0xF) << 4);
	}

	public static int OffsetX(int _offset)
	{
		return _offset & 0xF;
	}

	public static int OffsetY(int _offset)
	{
		return _offset >> 4;
	}

	public void UpdateRefCounts()
	{
		blockRefCount = 0;
		tickRefCount = 0;
		for (int num = m_Lower16Bits.Length - 1; num >= 0; num--)
		{
			int type = getAt(num).type;
			if (type > 0)
			{
				blockRefCount++;
				if (Block.list[type].IsRandomlyTick)
				{
					tickRefCount++;
				}
			}
		}
	}

	public int GetTickRefCount()
	{
		return tickRefCount;
	}

	public bool IsOnlyTerrain()
	{
		return bOnlyTerrain;
	}

	public void Read(BinaryReader stream, uint _version, bool _bNetworkRead, byte[] _tempReadBuf)
	{
		switch (_version)
		{
		default:
		{
			stream.Read(_tempReadBuf, 0, 512);
			for (int j = 0; j < m_Lower16Bits.Length; j++)
			{
				ushort num2 = (ushort)(_tempReadBuf[j * 2] | (_tempReadBuf[j * 2 + 1] << 8));
				m_Lower16Bits[j] = num2;
			}
			if (stream.ReadBoolean())
			{
				if (m_Upper16Bits == null)
				{
					m_Upper16Bits = new ushort[256];
				}
				stream.Read(_tempReadBuf, 0, 512);
				for (int k = 0; k < m_Upper16Bits.Length; k++)
				{
					ushort num3 = (ushort)(_tempReadBuf[k * 2] | (_tempReadBuf[k * 2 + 1] << 8));
					m_Upper16Bits[k] = num3;
				}
			}
			else
			{
				m_Upper16Bits = null;
			}
			break;
		}
		case 5u:
		case 6u:
		case 7u:
		case 8u:
		case 9u:
		case 10u:
		case 11u:
		case 12u:
		case 13u:
		case 14u:
		case 15u:
		case 16u:
		case 17u:
		case 18u:
		{
			if (_version >= 19)
			{
				break;
			}
			stream.Read(_tempReadBuf, 0, 1024);
			for (int i = 0; i < m_Lower16Bits.Length; i++)
			{
				uint num = (uint)(_tempReadBuf[i * 4] | (_tempReadBuf[i * 4 + 1] << 8) | (_tempReadBuf[i * 4 + 2] << 16) | (_tempReadBuf[i * 4 + 3] << 24));
				m_Lower16Bits[i] = (ushort)(num & 0xFFFF);
				if ((num & 0xFFFF0000u) != 0)
				{
					if (m_Upper16Bits == null)
					{
						m_Upper16Bits = new ushort[256];
					}
					m_Upper16Bits[i] = (ushort)((num >> 16) & 0xFFFF);
				}
				else if (m_Upper16Bits != null)
				{
					m_Upper16Bits[i] = 0;
				}
			}
			break;
		}
		case 0u:
		case 1u:
		case 2u:
		case 3u:
		case 4u:
			break;
		}
		if (_version > 8 && _version < 18 && !_bNetworkRead)
		{
			byte[] array = new byte[256];
			stream.Read(array, 0, array.Length);
			for (int l = 0; l < 16; l++)
			{
				for (int m = 0; m < 16; m++)
				{
					m_Stability.set(l, 0, m, array[l + m * 16]);
				}
			}
		}
		if (_version >= 18 && _version < 28 && !_bNetworkRead)
		{
			m_Stability.read(stream);
		}
		CheckOnlyTerrain();
	}

	public void Write(BinaryWriter _bw, bool _bNetworkWrite, byte[] _tempSaveBuf)
	{
		for (int i = 0; i < m_Lower16Bits.Length; i++)
		{
			uint num = m_Lower16Bits[i];
			_tempSaveBuf[i * 2] = (byte)(num & 0xFF);
			_tempSaveBuf[i * 2 + 1] = (byte)((num >> 8) & 0xFF);
		}
		_bw.Write(_tempSaveBuf, 0, m_Lower16Bits.Length * 2);
		_bw.Write(m_Upper16Bits != null);
		if (m_Upper16Bits != null)
		{
			for (int j = 0; j < m_Upper16Bits.Length; j++)
			{
				uint num2 = m_Upper16Bits[j];
				_tempSaveBuf[j * 2] = (byte)(num2 & 0xFF);
				_tempSaveBuf[j * 2 + 1] = (byte)((num2 >> 8) & 0xFF);
			}
			_bw.Write(_tempSaveBuf, 0, m_Upper16Bits.Length * 2);
		}
	}

	public void CheckOnlyTerrain()
	{
		bOnlyTerrain = true;
		uint typeMasked = BlockValue.GetTypeMasked(m_Lower16Bits[0]);
		for (int i = 0; i < m_Lower16Bits.Length; i++)
		{
			typeMasked = BlockValue.GetTypeMasked(m_Lower16Bits[i]);
			if (typeMasked != 0 && !Block.list[typeMasked].shape.IsTerrain())
			{
				bOnlyTerrain = false;
				break;
			}
		}
	}

	public int GetUsedMem()
	{
		return m_Lower16Bits.Length * 2 + ((m_Upper16Bits != null) ? (m_Upper16Bits.Length * 2) : 0) + 20 + 2;
	}
}

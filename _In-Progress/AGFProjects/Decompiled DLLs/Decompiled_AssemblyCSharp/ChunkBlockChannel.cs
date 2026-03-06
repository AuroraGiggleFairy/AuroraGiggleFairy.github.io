using System.IO;
using System.Runtime.CompilerServices;

public class ChunkBlockChannel
{
	public const int cElementsPerLayer = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] sameValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CBCLayer[] layers;

	[PublicizedFrom(EAccessModifier.Private)]
	public long defaultValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bytesPerVal;

	public ChunkBlockChannel(long _defaultValue, int _bytesPerVal = 1)
	{
		defaultValue = _defaultValue;
		bytesPerVal = _bytesPerVal;
		sameValue = new byte[64 * bytesPerVal];
		fillSameValue(-1L);
		layers = new CBCLayer[64 * bytesPerVal];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public CBCLayer allocLayer()
	{
		return MemoryPools.poolCBC.AllocSync(_bReset: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void freeLayer(int _idx)
	{
		if (layers[_idx] != null)
		{
			MemoryPools.poolCBC.FreeSync(layers[_idx]);
			layers[_idx] = null;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int calcOffset(int _x, int _y, int _z)
	{
		return _x + _z * 16 + (_y & 3) * 256;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void fillSameValue(long _value = -1L)
	{
		long num = ((_value == -1) ? defaultValue : _value);
		for (int i = 0; i < bytesPerVal; i++)
		{
			byte b = (byte)(num >> i * 8);
			for (int num2 = 63; num2 >= 0; num2--)
			{
				sameValue[num2 * bytesPerVal + i] = b;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long getSameValue(int _idx)
	{
		long num = 0L;
		for (int i = 0; i < bytesPerVal; i++)
		{
			num |= (long)((ulong)sameValue[_idx + i] << i * 8);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSameValue(int _idx, long _value)
	{
		for (int i = 0; i < bytesPerVal; i++)
		{
			sameValue[_idx + i] = (byte)_value;
			_value >>= 8;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long getData(int _idx, int _offs)
	{
		long num = 0L;
		for (int i = 0; i < bytesPerVal; i++)
		{
			CBCLayer cBCLayer = layers[_idx + i];
			if (cBCLayer == null)
			{
				break;
			}
			num |= (long)((ulong)cBCLayer.data[_offs] << i * 8);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long getSetData(int _idx, int _offs, long _value)
	{
		long num = 0L;
		for (int i = 0; i < bytesPerVal; i++)
		{
			CBCLayer cBCLayer = layers[_idx + i];
			if (cBCLayer == null)
			{
				break;
			}
			num |= (long)((ulong)cBCLayer.data[_offs] << i * 8);
			cBCLayer.data[_offs] = (byte)_value;
			_value >>= 8;
		}
		return num;
	}

	public long GetSet(int _x, int _y, int _z, long _value)
	{
		int num = (_y >> 2) * bytesPerVal;
		CBCLayer cBCLayer = layers[num];
		if (cBCLayer == null)
		{
			long num2 = getSameValue(num);
			if (num2 == _value)
			{
				return _value;
			}
			for (int i = 0; i < bytesPerVal; i++)
			{
				cBCLayer = allocLayer();
				layers[num + i] = cBCLayer;
				byte b = (byte)(num2 >> i * 8);
				for (int num3 = 1023; num3 >= 0; num3--)
				{
					cBCLayer.data[num3] = b;
				}
			}
		}
		int offs = calcOffset(_x, _y, _z);
		return getSetData(num, offs, _value);
	}

	public void Set(int _x, int _y, int _z, long _value)
	{
		int num = (_y >> 2) * bytesPerVal;
		CBCLayer cBCLayer = layers[num];
		if (cBCLayer == null)
		{
			long num2 = getSameValue(num);
			if (num2 == _value)
			{
				return;
			}
			for (int i = 0; i < bytesPerVal; i++)
			{
				cBCLayer = allocLayer();
				layers[num + i] = cBCLayer;
				byte b = (byte)(num2 >> i * 8);
				for (int num3 = 1023; num3 >= 0; num3--)
				{
					cBCLayer.data[num3] = b;
				}
			}
		}
		int num4 = calcOffset(_x, _y, _z);
		for (int j = 0; j < bytesPerVal; j++)
		{
			cBCLayer = layers[num + j];
			if (cBCLayer != null)
			{
				cBCLayer.data[num4] = (byte)_value;
				_value >>= 8;
				continue;
			}
			break;
		}
	}

	public long Get(int _x, int _y, int _z)
	{
		int num = (_y >> 2) * bytesPerVal;
		if (num < 0)
		{
			return 0L;
		}
		CBCLayer cBCLayer = layers[num];
		if (cBCLayer == null)
		{
			return getSameValue(num);
		}
		int num2 = calcOffset(_x, _y, _z);
		if (bytesPerVal == 1)
		{
			return cBCLayer.data[num2];
		}
		return getData(num, num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetByte(int _x, int _y, int _z)
	{
		int num = _y >> 2;
		if ((uint)num >= 64u)
		{
			return 0;
		}
		CBCLayer cBCLayer = layers[num];
		if (cBCLayer == null)
		{
			return sameValue[num];
		}
		int num2 = calcOffset(_x, _y, _z);
		return cBCLayer.data[num2];
	}

	public void Read(BinaryReader _br, uint _version, bool _bNetworkRead)
	{
		if (_version > 34)
		{
			for (int i = 0; i < 64; i++)
			{
				int num = i * bytesPerVal;
				bool flag = _br.ReadByte() == 1;
				for (int j = 0; j < bytesPerVal; j++)
				{
					int num2 = num + j;
					if (!flag)
					{
						if (layers[num2] == null)
						{
							layers[num2] = allocLayer();
						}
						_br.Read(layers[num2].data, 0, 1024);
					}
					else
					{
						sameValue[num2] = _br.ReadByte();
						freeLayer(num2);
					}
				}
				onLayerRead(num);
			}
			return;
		}
		for (int k = 0; k < 64; k++)
		{
			int num3 = k * bytesPerVal;
			bool flag2 = _br.ReadBoolean();
			for (int l = 0; l < bytesPerVal; l++)
			{
				if (!flag2)
				{
					if (layers[num3 + l] == null)
					{
						layers[num3 + l] = allocLayer();
					}
					_br.Read(layers[num3 + l].data, 0, 1024);
				}
				else
				{
					sameValue[num3 + l] = _br.ReadByte();
					freeLayer(num3 + l);
				}
			}
			onLayerRead(num3);
		}
	}

	public void Write(BinaryWriter _bw, bool _bNetworkWrite, byte[] temp)
	{
		int num = 0;
		for (int i = 0; i < 64; i++)
		{
			int num2 = i * bytesPerVal;
			bool flag = layers[num2] == null;
			temp[num++] = (byte)(flag ? 1u : 0u);
			if (num == temp.Length)
			{
				_bw.Write(temp, 0, num);
				num = 0;
			}
			for (int j = 0; j < bytesPerVal; j++)
			{
				if (!flag)
				{
					if (num > 0)
					{
						_bw.Write(temp, 0, num);
						num = 0;
					}
					_bw.Write(layers[num2 + j].data, 0, 1024);
				}
				else
				{
					temp[num++] = sameValue[num2 + j];
					if (num == temp.Length)
					{
						_bw.Write(temp, 0, num);
						num = 0;
					}
				}
			}
		}
		if (num > 0)
		{
			_bw.Write(temp, 0, num);
			num = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onLayerRead(int _idx)
	{
		if (layers[_idx] != null)
		{
			checkSameValue(_idx);
		}
	}

	public void CheckSameValue()
	{
		for (int num = 63; num >= 0; num--)
		{
			checkSameValue(num * bytesPerVal);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkSameValue(int _idx)
	{
		if (layers[_idx] == null)
		{
			return;
		}
		long data = getData(_idx, 0);
		for (int i = 1; i < 1024; i++)
		{
			if (data != getData(_idx, i))
			{
				return;
			}
		}
		setSameValue(_idx, data);
		for (int j = 0; j < bytesPerVal; j++)
		{
			freeLayer(_idx + j);
		}
	}

	public bool HasSameValue(int _y)
	{
		int num = (_y >> 2) * bytesPerVal;
		return layers[num] == null;
	}

	public long GetSameValue(int _y)
	{
		int idx = (_y >> 2) * bytesPerVal;
		return getSameValue(idx);
	}

	public bool IsDefault()
	{
		for (int num = 63; num >= 0; num--)
		{
			if (!IsDefaultLayer(num))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsDefault(int _y)
	{
		int blockLayer = _y >> 2;
		return IsDefaultLayer(blockLayer);
	}

	public bool IsDefaultLayer(int _blockLayer)
	{
		return isDefault(_blockLayer * bytesPerVal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDefault(int _idx)
	{
		checkSameValue(_idx);
		if (layers[_idx] == null)
		{
			return getSameValue(_idx) == defaultValue;
		}
		return false;
	}

	public int GetUsedMem()
	{
		int num = 0;
		for (int num2 = layers.Length - 1; num2 >= 0; num2--)
		{
			if (layers[num2] != null)
			{
				num += 1024;
			}
		}
		num += sameValue.Length;
		return num + layers.Length * 4;
	}

	public void FreeLayers()
	{
		MemoryPools.poolCBC.FreeSync(layers);
		fillSameValue(-1L);
	}

	public void Clear(long _defaultValue = -1L)
	{
		for (int i = 0; i < layers.Length; i++)
		{
			freeLayer(i);
		}
		fillSameValue(_defaultValue);
	}

	public void ClearHalf(bool _bClearUpperHalf)
	{
		byte b = (byte)(_bClearUpperHalf ? 15u : 240u);
		for (int i = 0; i < 64; i++)
		{
			CBCLayer cBCLayer = layers[i];
			if (cBCLayer != null)
			{
				for (int j = 0; j < 1024; j++)
				{
					cBCLayer.data[j] &= b;
				}
			}
			else
			{
				sameValue[i] &= b;
			}
		}
	}

	public void SetHalf(bool _bSetUpperHalf, byte _v)
	{
		byte b = (byte)(_bSetUpperHalf ? 15u : 240u);
		for (int i = 0; i < 64; i++)
		{
			CBCLayer cBCLayer = layers[i];
			if (cBCLayer != null)
			{
				for (int j = 0; j < 1024; j++)
				{
					cBCLayer.data[j] &= b;
					cBCLayer.data[j] |= _v;
				}
			}
			else
			{
				sameValue[i] &= b;
				sameValue[i] |= _v;
			}
		}
	}

	public void CopyFrom(ChunkBlockChannel _other)
	{
		for (int i = 0; i < _other.layers.Length; i++)
		{
			if (_other.layers[i] != null)
			{
				if (layers[i] == null)
				{
					layers[i] = allocLayer();
				}
				layers[i].CopyFrom(_other.layers[i]);
			}
			else
			{
				freeLayer(i);
			}
		}
		for (int j = 0; j < _other.sameValue.Length; j++)
		{
			sameValue[j] = _other.sameValue[j];
		}
	}

	public void Convert(SmartArray _sa, int _shiftBits)
	{
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					byte b = _sa.get(j, i, k);
					byte b2 = (byte)Get(j, i, k);
					b2 |= (byte)(b << _shiftBits);
					Set(j, i, k, b2);
				}
			}
		}
		CheckSameValue();
	}

	public void Convert(ChunkBlockLayerLegacy[] m_BlockLayers)
	{
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					byte b = (byte)((m_BlockLayers[i] != null) ? m_BlockLayers[i].GetStabilityAt(j, k) : 0);
					Set(j, i, k, b);
				}
			}
		}
		CheckSameValue();
	}
}

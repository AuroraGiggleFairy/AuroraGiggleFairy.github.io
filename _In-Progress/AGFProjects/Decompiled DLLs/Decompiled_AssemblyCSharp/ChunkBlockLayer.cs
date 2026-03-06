using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public class ChunkBlockLayer : IMemoryPoolableObject
{
	public delegate void LoopBlocksDelegate(int x, int y, int z, BlockValue bv);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWPow = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cLayerHeight = 4;

	public const int cArrSize = 1024;

	public static int InstanceCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte lower8BitSameValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_Lower8Bits;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_Upper24Bits;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bOnlyTerrain;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockRefCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tickRefCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public object lockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> notifyLoadUnloadCallbackBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool[] saved = new bool[Block.MAX_BLOCKS];

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] allocArray8Bit(bool _bClear, byte _val)
	{
		lock (MemoryPools.poolCBLLower8BitArrCache)
		{
			byte[] array = null;
			if (MemoryPools.poolCBLLower8BitArrCache.Count == 0)
			{
				array = new byte[1024];
			}
			else
			{
				array = MemoryPools.poolCBLLower8BitArrCache[MemoryPools.poolCBLLower8BitArrCache.Count - 1];
				MemoryPools.poolCBLLower8BitArrCache.RemoveAt(MemoryPools.poolCBLLower8BitArrCache.Count - 1);
			}
			if (_bClear)
			{
				Utils.Memset(array, _val, array.Length);
			}
			return array;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void freeArray8Bit(byte[] _array)
	{
		lock (MemoryPools.poolCBLLower8BitArrCache)
		{
			if (_array != null && MemoryPools.poolCBLLower8BitArrCache.Count < 10000)
			{
				MemoryPools.poolCBLLower8BitArrCache.Add(_array);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] allocArray24Bit(bool _bClear)
	{
		lock (MemoryPools.poolCBLUpper24BitArrCache)
		{
			byte[] array = null;
			if (MemoryPools.poolCBLUpper24BitArrCache.Count == 0)
			{
				array = new byte[3072];
			}
			else
			{
				array = MemoryPools.poolCBLUpper24BitArrCache[MemoryPools.poolCBLUpper24BitArrCache.Count - 1];
				MemoryPools.poolCBLUpper24BitArrCache.RemoveAt(MemoryPools.poolCBLUpper24BitArrCache.Count - 1);
			}
			if (_bClear)
			{
				Utils.Memset(array, 0, array.Length);
			}
			return array;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void freeArray24Bit(byte[] _array)
	{
		lock (MemoryPools.poolCBLUpper24BitArrCache)
		{
			if (_array != null && MemoryPools.poolCBLUpper24BitArrCache.Count < 10000)
			{
				MemoryPools.poolCBLUpper24BitArrCache.Add(_array);
			}
		}
	}

	public static int GetTempBufSize()
	{
		return 3072;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockValue GetAt(int _x, int _y, int _z)
	{
		int offs = _x + (_z << 4) + (_y & 3) * 256;
		return GetAt(offs);
	}

	public BlockValue GetAt(int offs)
	{
		uint num = lower8BitSameValue;
		if (m_Lower8Bits != null)
		{
			num = m_Lower8Bits[offs];
		}
		if (m_Upper24Bits != null)
		{
			int num2 = offs * 3;
			num |= (uint)((m_Upper24Bits[num2] << 8) | (m_Upper24Bits[num2 + 1] << 16) | (m_Upper24Bits[num2 + 2] << 24));
		}
		BlockValue result = new BlockValue(num);
		if (result.type >= Block.list.Length || result.Block == null)
		{
			return BlockValue.Air;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetIdAt(int _x, int _y, int _z)
	{
		int offs = _x + (_z << 4) + (_y & 3) * 16 * 16;
		return GetIdAt(offs);
	}

	public int GetIdAt(int offs)
	{
		uint num = lower8BitSameValue;
		if (m_Lower8Bits != null)
		{
			num = m_Lower8Bits[offs];
		}
		if (m_Upper24Bits != null)
		{
			int num2 = offs * 3;
			num |= (uint)(m_Upper24Bits[num2] << 8);
			num &= 0xFFFF;
		}
		return (int)num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalcOffset(int _x, int _y, int _z)
	{
		return _x + (_z << 4) + (_y & 3) * 256;
	}

	public void SetAt(int _x, int _y, int _z, uint _fullBlock)
	{
		int offs = _x + (_z << 4) + (_y & 3) * 256;
		SetAt(offs, _fullBlock);
	}

	public void SetAt(int offs, uint _fullBlock)
	{
		uint num = ((m_Lower8Bits != null) ? m_Lower8Bits[offs] : lower8BitSameValue);
		if (m_Upper24Bits != null)
		{
			num |= (uint)(m_Upper24Bits[offs * 3] << 8);
			num &= 0xFFFF;
		}
		byte b = (byte)_fullBlock;
		if (m_Lower8Bits == null && lower8BitSameValue != b)
		{
			m_Lower8Bits = allocArray8Bit(_bClear: true, lower8BitSameValue);
		}
		if (m_Lower8Bits != null)
		{
			m_Lower8Bits[offs] = b;
		}
		if ((_fullBlock & 0xFFFFFF00u) != 0)
		{
			if (m_Upper24Bits == null)
			{
				m_Upper24Bits = allocArray24Bit(_bClear: true);
			}
			m_Upper24Bits[offs * 3] = (byte)(_fullBlock >> 8);
			m_Upper24Bits[offs * 3 + 1] = (byte)(_fullBlock >> 16);
			m_Upper24Bits[offs * 3 + 2] = (byte)(_fullBlock >> 24);
		}
		else if (m_Upper24Bits != null)
		{
			m_Upper24Bits[offs * 3] = 0;
			m_Upper24Bits[offs * 3 + 1] = 0;
			m_Upper24Bits[offs * 3 + 2] = 0;
		}
		if (!Block.BlocksLoaded)
		{
			return;
		}
		uint num2 = _fullBlock & 0xFFFF;
		Block block = Block.list[num];
		Block block2 = Block.list[num2];
		if (num == 0 && num2 != 0)
		{
			blockRefCount++;
			if (block2 != null && block2.IsRandomlyTick)
			{
				tickRefCount++;
			}
		}
		else if (num != 0 && num2 == 0)
		{
			blockRefCount--;
			if (block != null && block.IsRandomlyTick)
			{
				tickRefCount--;
			}
		}
		else if (block != null)
		{
			if (block.IsRandomlyTick && block2 != null && !block2.IsRandomlyTick)
			{
				tickRefCount--;
			}
			else if (!block.IsRandomlyTick && block2 != null && block2.IsRandomlyTick)
			{
				tickRefCount++;
			}
		}
		if (block2 != null && block2.IsNotifyOnLoadUnload)
		{
			lock (lockObj)
			{
				if (notifyLoadUnloadCallbackBlocks == null)
				{
					notifyLoadUnloadCallbackBlocks = new HashSet<int>();
				}
				if (!notifyLoadUnloadCallbackBlocks.Contains(offs))
				{
					notifyLoadUnloadCallbackBlocks.Add(offs);
				}
			}
		}
		else if (block != null && block.IsNotifyOnLoadUnload && notifyLoadUnloadCallbackBlocks != null)
		{
			lock (lockObj)
			{
				notifyLoadUnloadCallbackBlocks.Remove(offs);
			}
		}
		if (bOnlyTerrain && block2 != null && !block2.shape.IsTerrain())
		{
			bOnlyTerrain = false;
		}
	}

	public void Fill(uint _fullBlock)
	{
		uint num = _fullBlock & 0xFFFF;
		Block block = Block.list[num];
		freeArray8Bit(m_Lower8Bits);
		m_Lower8Bits = null;
		lower8BitSameValue = (byte)_fullBlock;
		if ((_fullBlock & 0xFFFFFF00u) != 0)
		{
			if (m_Upper24Bits == null)
			{
				m_Upper24Bits = allocArray24Bit(_bClear: true);
			}
			byte b = (byte)(_fullBlock >> 8);
			byte b2 = (byte)(_fullBlock >> 16);
			byte b3 = (byte)(_fullBlock >> 24);
			for (int i = 0; i < m_Upper24Bits.Length; i += 3)
			{
				m_Upper24Bits[i] = b;
				m_Upper24Bits[i + 1] = b2;
				m_Upper24Bits[i + 2] = b3;
			}
		}
		else
		{
			freeArray24Bit(m_Upper24Bits);
			m_Upper24Bits = null;
		}
		bOnlyTerrain = block.shape.IsTerrain();
		lock (lockObj)
		{
			if (notifyLoadUnloadCallbackBlocks != null)
			{
				notifyLoadUnloadCallbackBlocks.Clear();
			}
			else if (block.IsNotifyOnLoadUnload)
			{
				notifyLoadUnloadCallbackBlocks = new HashSet<int>();
			}
			if (block.IsNotifyOnLoadUnload)
			{
				for (int j = 0; j < 1024; j++)
				{
					notifyLoadUnloadCallbackBlocks.Add(j);
				}
			}
		}
	}

	public void Reset()
	{
		freeArray8Bit(m_Lower8Bits);
		m_Lower8Bits = null;
		lower8BitSameValue = 0;
		freeArray24Bit(m_Upper24Bits);
		m_Upper24Bits = null;
		blockRefCount = 0;
		tickRefCount = 0;
		lock (lockObj)
		{
			if (notifyLoadUnloadCallbackBlocks != null)
			{
				notifyLoadUnloadCallbackBlocks.Clear();
			}
		}
	}

	public void Cleanup()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRefCounts()
	{
		blockRefCount = 0;
		tickRefCount = 0;
		for (int num = 1023; num >= 0; num--)
		{
			int idAt = GetIdAt(num);
			if (idAt > 0 && idAt < Block.list.Length)
			{
				Block block = Block.list[idAt];
				if (block == null)
				{
					SetAt(num, 0u);
				}
				else
				{
					blockRefCount++;
					if (block.IsRandomlyTick)
					{
						tickRefCount++;
					}
					if (block.IsNotifyOnLoadUnload)
					{
						lock (lockObj)
						{
							if (notifyLoadUnloadCallbackBlocks == null)
							{
								notifyLoadUnloadCallbackBlocks = new HashSet<int>();
							}
							notifyLoadUnloadCallbackBlocks.Add(num);
						}
					}
				}
			}
		}
	}

	public void OnLoad(WorldBase _world, int _clrIdx, int _x, int _y, int _z)
	{
		if (notifyLoadUnloadCallbackBlocks == null)
		{
			return;
		}
		lock (lockObj)
		{
			foreach (int notifyLoadUnloadCallbackBlock in notifyLoadUnloadCallbackBlocks)
			{
				BlockValue at = GetAt(notifyLoadUnloadCallbackBlock);
				int y = notifyLoadUnloadCallbackBlock / 256 + _y;
				int num = notifyLoadUnloadCallbackBlock % 256;
				int x = num % 16 + _x;
				int z = num / 16 + _z;
				at.Block.OnBlockLoaded(_world, _clrIdx, new Vector3i(x, y, z), at);
			}
		}
	}

	public void OnUnload(WorldBase _world, int _clrIdx, int _x, int _y, int _z)
	{
		if (notifyLoadUnloadCallbackBlocks == null)
		{
			return;
		}
		lock (lockObj)
		{
			foreach (int notifyLoadUnloadCallbackBlock in notifyLoadUnloadCallbackBlocks)
			{
				BlockValue at = GetAt(notifyLoadUnloadCallbackBlock);
				int y = notifyLoadUnloadCallbackBlock / 256 + _y;
				int num = notifyLoadUnloadCallbackBlock % 256;
				int x = num % 16 + _x;
				int z = num / 16 + _z;
				at.Block.OnBlockUnloaded(_world, _clrIdx, new Vector3i(x, y, z), at);
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

	public void AddIndexedBlocks(int _curLayerIdx, DictionarySave<string, List<Vector3i>> _indexedBlocksDict)
	{
		for (int i = 0; i < 1024; i++)
		{
			int idAt = GetIdAt(i);
			Block block = Block.list[idAt];
			if (block == null || block.IndexName == null)
			{
				continue;
			}
			BlockValue at = GetAt(i);
			if (block.FilterIndexType(at))
			{
				if (!_indexedBlocksDict.ContainsKey(block.IndexName))
				{
					_indexedBlocksDict[block.IndexName] = new List<Vector3i>();
				}
				int y = (_curLayerIdx << 2) + i / 256;
				int x = i % 256 % 16;
				int z = i % 256 / 16;
				_indexedBlocksDict[block.IndexName].Add(new Vector3i(x, y, z));
			}
		}
	}

	public void Read(BinaryReader stream, uint _version, bool _bNetworkRead)
	{
		if (_version < 30)
		{
			throw new Exception("Chunk version " + _version + " not supported any more!");
		}
		if (stream.ReadBoolean())
		{
			if (m_Lower8Bits == null)
			{
				m_Lower8Bits = allocArray8Bit(_bClear: false, 0);
			}
			stream.Read(m_Lower8Bits, 0, 1024);
		}
		else
		{
			if (m_Lower8Bits != null)
			{
				freeArray8Bit(m_Lower8Bits);
				m_Lower8Bits = null;
			}
			lower8BitSameValue = stream.ReadByte();
		}
		if (stream.ReadBoolean())
		{
			if (m_Upper24Bits == null)
			{
				m_Upper24Bits = allocArray24Bit(_bClear: false);
			}
			stream.Read(m_Upper24Bits, 0, 3072);
		}
		else if (m_Upper24Bits != null)
		{
			freeArray24Bit(m_Upper24Bits);
			m_Upper24Bits = null;
		}
		updateRefCounts();
		CheckOnlyTerrain();
	}

	public void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		_bw.Write(m_Lower8Bits != null);
		if (m_Lower8Bits != null)
		{
			_bw.Write(m_Lower8Bits, 0, 1024);
		}
		else
		{
			_bw.Write(lower8BitSameValue);
		}
		_bw.Write(m_Upper24Bits != null);
		if (m_Upper24Bits != null)
		{
			_bw.Write(m_Upper24Bits, 0, 3072);
		}
	}

	public void CopyFrom(ChunkBlockLayer _other)
	{
		if (_other.m_Lower8Bits != null)
		{
			if (m_Lower8Bits == null)
			{
				m_Lower8Bits = allocArray8Bit(_bClear: true, 0);
			}
			Array.Copy(_other.m_Lower8Bits, m_Lower8Bits, m_Lower8Bits.Length);
		}
		else if (m_Lower8Bits != null)
		{
			freeArray8Bit(m_Lower8Bits);
		}
		if (_other.m_Upper24Bits != null)
		{
			if (m_Upper24Bits == null)
			{
				m_Upper24Bits = allocArray24Bit(_bClear: true);
			}
			Array.Copy(_other.m_Upper24Bits, m_Upper24Bits, m_Upper24Bits.Length);
		}
		else if (m_Upper24Bits != null)
		{
			freeArray24Bit(m_Upper24Bits);
		}
		bOnlyTerrain = _other.bOnlyTerrain;
		blockRefCount = _other.blockRefCount;
		tickRefCount = _other.tickRefCount;
	}

	public void CheckOnlyTerrain()
	{
		if (m_Upper24Bits != null)
		{
			bool flag = m_Upper24Bits[0] == 0;
			int num = 1;
			while (flag && num < m_Upper24Bits.Length)
			{
				flag &= m_Upper24Bits[num] == 0;
				num++;
			}
			if (flag)
			{
				freeArray24Bit(m_Upper24Bits);
				m_Upper24Bits = null;
			}
		}
		bOnlyTerrain = m_Upper24Bits == null;
		if (m_Lower8Bits == null)
		{
			bOnlyTerrain &= lower8BitSameValue > 0 && lower8BitSameValue <= 128;
			return;
		}
		if (bOnlyTerrain)
		{
			for (int i = 0; i < m_Lower8Bits.Length; i++)
			{
				uint num2 = m_Lower8Bits[i];
				if (num2 > 128 || num2 == 0)
				{
					bOnlyTerrain = false;
					break;
				}
			}
		}
		bool flag2 = true;
		lower8BitSameValue = m_Lower8Bits[0];
		for (int j = 1; j < m_Lower8Bits.Length; j++)
		{
			if (lower8BitSameValue != m_Lower8Bits[j])
			{
				flag2 = false;
				lower8BitSameValue = 0;
				break;
			}
		}
		if (flag2)
		{
			freeArray8Bit(m_Lower8Bits);
			m_Lower8Bits = null;
			bOnlyTerrain &= lower8BitSameValue != 0;
		}
	}

	public void LoopOverAllBlocks(Chunk _c, int _yPos, LoopBlocksDelegate _delegate, bool _bIncludeChilds = false, bool _bIncludeAirBlocks = false)
	{
		for (int i = 0; i < 1024; i++)
		{
			BlockValue at = GetAt(i);
			if ((_bIncludeAirBlocks || !at.isair) && (_bIncludeChilds || !at.ischild))
			{
				int y = i / 256 + _yPos;
				int num = i % 256;
				int x = num % 16;
				int z = num / 16;
				at.damage = _c.GetDamage(x, y, z);
				_delegate(x, y, z, at);
			}
		}
	}

	public int GetUsedMem()
	{
		return ((m_Lower8Bits == null) ? 1 : m_Lower8Bits.Length) + ((m_Upper24Bits != null) ? m_Upper24Bits.Length : 0) + 20 + 2;
	}

	public void SaveBlockMappings(NameIdMapping _mappings)
	{
		if (m_Lower8Bits == null && m_Upper24Bits == null)
		{
			Block block = GetAt(0).Block;
			_mappings.AddMapping(block.blockID, block.GetBlockName());
			return;
		}
		Array.Clear(saved, 0, Block.MAX_BLOCKS);
		bool flag = m_Lower8Bits != null;
		bool flag2 = m_Upper24Bits != null;
		for (int i = 0; i < 1024; i++)
		{
			int num = (flag ? m_Lower8Bits[i] : lower8BitSameValue);
			num |= (flag2 ? ((m_Upper24Bits[i * 3] << 8) & 0xFF00) : 0);
			num &= 0xFFFF;
			if (!saved[num])
			{
				Block block2 = Block.list[num];
				if (block2 != null)
				{
					_mappings.AddMapping(num, block2.GetBlockName());
				}
				saved[num] = true;
			}
		}
	}
}

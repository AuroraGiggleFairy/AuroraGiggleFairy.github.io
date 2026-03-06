using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

public class WorldChunkCache
{
	public const long InvalidChunk = long.MaxValue;

	public Vector2i ChunkMinPos = Vector2i.zero;

	public Vector2i ChunkMaxPos = Vector2i.zero;

	public DictionaryLinkedList<long, Chunk> chunks = new DictionaryLinkedList<long, Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IChunkCallback> chunkCallbacks = new List<IChunkCallback>();

	public HashSetLong chunkKeys = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isChunkKeysDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunkKeysCopy = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isChunkArrayDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> chunkArrayCopy = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReaderWriterLockSlim sync = new ReaderWriterLockSlim();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long MakeChunkKey(int x, int y)
	{
		return (long)((((ulong)y & 0xFFFFFFuL) << 24) | ((ulong)x & 0xFFFFFFuL));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long MakeChunkKey(int x, int y, int clrIdx)
	{
		return (long)((((ulong)clrIdx & 0xFFuL) << 56) | (((ulong)y & 0xFFFFFFuL) << 24) | ((ulong)x & 0xFFFFFFuL));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long MakeChunkKey(long key, int clrIdx)
	{
		return (long)(((ulong)clrIdx & 0xFFuL) << 56) | (key & 0xFFFFFFFFFFFFFFL);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int extractX(long key)
	{
		return (int)(key << 8) >> 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int extractZ(long key)
	{
		return (int)(key >> 16) >> 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i extractXZ(long key)
	{
		Vector2i result = default(Vector2i);
		result.x = extractX(key);
		result.y = extractZ(key);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int extractClrIdx(long key)
	{
		return (int)((key >> 56) & 0xFF);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Chunk GetChunkSync(int _x, int _y)
	{
		return GetChunkSync(MakeChunkKey(_x, _y));
	}

	public virtual Chunk GetChunkSync(long _key)
	{
		sync.EnterReadLock();
		chunks.dict.TryGetValue(_key & 0xFFFFFFFFFFFFFFL, out var value);
		sync.ExitReadLock();
		return value;
	}

	public virtual void RemoveChunkSync(long _key)
	{
		sync.EnterWriteLock();
		if (chunks.dict.TryGetValue(_key & 0xFFFFFFFFFFFFFFL, out var value))
		{
			chunks.Remove(_key & 0xFFFFFFFFFFFFFFL);
			isChunkArrayDirty = true;
		}
		chunkKeys.Remove(_key);
		isChunkKeysDirty = true;
		sync.ExitWriteLock();
		if (value != null)
		{
			for (int i = 0; i < chunkCallbacks.Count; i++)
			{
				chunkCallbacks[i].OnChunkBeforeRemove(value);
			}
		}
	}

	public void NotifyOnChunkBeforeSave(Chunk _c)
	{
		for (int i = 0; i < chunkCallbacks.Count; i++)
		{
			chunkCallbacks[i].OnChunkBeforeSave(_c);
		}
	}

	public virtual bool AddChunkSync(Chunk _chunk, bool _bOmitCallbacks = false)
	{
		ChunkMinPos.x = Utils.FastMin(ChunkMinPos.x, _chunk.X);
		ChunkMinPos.y = Utils.FastMin(ChunkMinPos.y, _chunk.Z);
		ChunkMaxPos.x = Utils.FastMax(ChunkMaxPos.x, _chunk.X);
		ChunkMaxPos.y = Utils.FastMax(ChunkMaxPos.y, _chunk.Z);
		sync.EnterWriteLock();
		long key = _chunk.Key;
		if (chunkKeys.Contains(key))
		{
			sync.ExitWriteLock();
			return false;
		}
		chunks.Add(key & 0xFFFFFFFFFFFFFFL, _chunk);
		chunkKeys.Add(key);
		isChunkArrayDirty = true;
		isChunkKeysDirty = true;
		sync.ExitWriteLock();
		int num = 0;
		while (!_bOmitCallbacks && num < chunkCallbacks.Count)
		{
			chunkCallbacks[num].OnChunkAdded(_chunk);
			num++;
		}
		return true;
	}

	public List<Chunk> GetChunkArrayCopySync()
	{
		sync.EnterReadLock();
		if (isChunkArrayDirty)
		{
			chunkArrayCopy = new List<Chunk>(chunks.list.Count);
			chunkArrayCopy.AddRange(chunks.list);
			isChunkArrayDirty = false;
		}
		sync.ExitReadLock();
		return chunkArrayCopy;
	}

	public LinkedList<Chunk> GetChunkArray()
	{
		return chunks.list;
	}

	public HashSetLong GetChunkKeysCopySync()
	{
		if (isChunkKeysDirty)
		{
			sync.EnterReadLock();
			chunkKeysCopy.Clear();
			foreach (long chunkKey in chunkKeys)
			{
				chunkKeysCopy.Add(chunkKey);
			}
			sync.ExitReadLock();
			isChunkKeysDirty = false;
		}
		return chunkKeysCopy;
	}

	public int Count()
	{
		return chunks.list.Count;
	}

	public ReaderWriterLockSlim GetSyncRoot()
	{
		return sync;
	}

	public virtual bool ContainsChunkSync(long key)
	{
		sync.EnterReadLock();
		bool result = chunks.dict.ContainsKey(key);
		sync.ExitReadLock();
		return result;
	}

	public void AddChunkCallback(IChunkCallback _callback)
	{
		chunkCallbacks.Add(_callback);
	}

	public void RemoveChunkCallback(IChunkCallback _callback)
	{
		chunkCallbacks.Remove(_callback);
	}

	public virtual void Update()
	{
	}

	public virtual void Clear()
	{
		sync.EnterWriteLock();
		List<Chunk> list = new List<Chunk>();
		list.AddRange(GetChunkArray());
		chunks.Clear();
		chunkKeys.Clear();
		isChunkArrayDirty = true;
		isChunkKeysDirty = true;
		sync.ExitWriteLock();
		MemoryPools.PoolChunks.FreeSync(list);
	}

	public bool GetNeighborChunks(Chunk _chunk, Chunk[] neighbours)
	{
		sync.EnterReadLock();
		int x = _chunk.X;
		int z = _chunk.Z;
		Chunk chunk = GetChunk(x + 1, z);
		if (chunk != null)
		{
			neighbours[0] = chunk;
			chunk = GetChunk(x - 1, z);
			if (chunk != null)
			{
				neighbours[1] = chunk;
				chunk = GetChunk(x, z + 1);
				if (chunk != null)
				{
					neighbours[2] = chunk;
					chunk = GetChunk(x, z - 1);
					if (chunk != null)
					{
						neighbours[3] = chunk;
						chunk = GetChunk(x + 1, z + 1);
						if (chunk != null)
						{
							neighbours[4] = chunk;
							chunk = GetChunk(x - 1, z - 1);
							if (chunk != null)
							{
								neighbours[5] = chunk;
								chunk = GetChunk(x - 1, z + 1);
								if (chunk != null)
								{
									neighbours[6] = chunk;
									chunk = GetChunk(x + 1, z - 1);
									if (chunk != null)
									{
										neighbours[7] = chunk;
										sync.ExitReadLock();
										return true;
									}
								}
							}
						}
					}
				}
			}
		}
		sync.ExitReadLock();
		return false;
	}

	public bool HasNeighborChunks(Chunk _chunk)
	{
		sync.EnterReadLock();
		int x = _chunk.X;
		int z = _chunk.Z;
		if (GetChunk(x + 1, z) != null && GetChunk(x - 1, z) != null && GetChunk(x, z + 1) != null && GetChunk(x, z - 1) != null && GetChunk(x + 1, z + 1) != null && GetChunk(x - 1, z - 1) != null && GetChunk(x - 1, z + 1) != null && GetChunk(x + 1, z - 1) != null)
		{
			sync.ExitReadLock();
			return true;
		}
		sync.ExitReadLock();
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk GetChunk(int _x, int _y)
	{
		long key = MakeChunkKey(_x, _y);
		chunks.dict.TryGetValue(key, out var value);
		return value;
	}
}

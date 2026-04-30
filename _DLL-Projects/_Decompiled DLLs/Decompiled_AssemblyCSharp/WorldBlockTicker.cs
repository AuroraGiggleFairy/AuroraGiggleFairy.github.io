using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class WorldBlockTicker
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class EntryComparer : IComparer
	{
		public int Compare(object _o1, object _o2)
		{
			WorldBlockTickerEntry worldBlockTickerEntry = (WorldBlockTickerEntry)_o1;
			WorldBlockTickerEntry worldBlockTickerEntry2 = (WorldBlockTickerEntry)_o2;
			if (worldBlockTickerEntry.scheduledTime < worldBlockTickerEntry2.scheduledTime)
			{
				return -1;
			}
			if (worldBlockTickerEntry.scheduledTime > worldBlockTickerEntry2.scheduledTime)
			{
				return 1;
			}
			if (worldBlockTickerEntry.tickEntryID < worldBlockTickerEntry2.tickEntryID)
			{
				return -1;
			}
			if (worldBlockTickerEntry.tickEntryID > worldBlockTickerEntry2.tickEntryID)
			{
				return 1;
			}
			return 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object lockObject = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SortedList scheduledTicksSorted = new SortedList(new EntryComparer());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, WorldBlockTickerEntry> scheduledTicksDict = new Dictionary<int, WorldBlockTickerEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DictionarySave<long, HashSet<WorldBlockTickerEntry>> chunkToScheduledTicks = new DictionarySave<long, HashSet<WorldBlockTickerEntry>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public int randomTickIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int randomTickCountPerFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<long> randomTickChunkKeys = new List<long>();

	public WorldBlockTicker(World _world)
	{
		world = _world;
	}

	public void Cleanup()
	{
		lock (lockObject)
		{
			scheduledTicksSorted.Clear();
			scheduledTicksDict.Clear();
			chunkToScheduledTicks.Clear();
		}
	}

	public void Tick(ArraySegment<long> _activeChunks, EntityPlayer _thePlayer, GameRandom _rnd)
	{
		if (GameManager.bTickingActive)
		{
			if (!world.IsRemote())
			{
				tickScheduled(_rnd);
			}
			if (!world.IsRemote())
			{
				tickRandom(_activeChunks, _rnd);
			}
		}
	}

	public void AddScheduledBlockUpdate(int _clrIdx, Vector3i _pos, int _blockId, ulong _ticks)
	{
		if (_blockId == 0)
		{
			return;
		}
		WorldBlockTickerEntry worldBlockTickerEntry = new WorldBlockTickerEntry(_clrIdx, _pos, _blockId, _ticks + GameTimer.Instance.ticks);
		lock (lockObject)
		{
			if (scheduledTicksDict.ContainsKey(worldBlockTickerEntry.GetHashCode()))
			{
				remove(_clrIdx, _pos, _blockId);
			}
			add(worldBlockTickerEntry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void add(WorldBlockTickerEntry _wbte)
	{
		scheduledTicksDict.Add(_wbte.GetHashCode(), _wbte);
		scheduledTicksSorted.Add(_wbte, null);
		HashSet<WorldBlockTickerEntry> hashSet = chunkToScheduledTicks[_wbte.GetChunkKey()];
		if (hashSet == null)
		{
			hashSet = new HashSet<WorldBlockTickerEntry>();
			chunkToScheduledTicks[_wbte.GetChunkKey()] = hashSet;
		}
		hashSet.Add(_wbte);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void execute(WorldBlockTickerEntry _wbte, GameRandom _rnd, ulong _ticksIfLoaded)
	{
		BlockValue block = world.GetBlock(_wbte.clrIdx, _wbte.worldPos);
		if (block.type == _wbte.blockID)
		{
			block.Block.UpdateTick(world, _wbte.clrIdx, _wbte.worldPos, block, _bRandomTick: false, _ticksIfLoaded, _rnd);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tickScheduled(GameRandom _rnd)
	{
		int num;
		lock (lockObject)
		{
			num = scheduledTicksSorted.Count;
			if (num != scheduledTicksDict.Count)
			{
				throw new Exception("WBT: Invalid dict state");
			}
		}
		if (num > 100)
		{
			num = 100;
		}
		for (int num2 = 0; num2 < num; num2++)
		{
			WorldBlockTickerEntry worldBlockTickerEntry;
			lock (lockObject)
			{
				if (scheduledTicksSorted.Count == 0)
				{
					Log.Warning("WorldBlockTicker tickScheduled count 0");
					break;
				}
				worldBlockTickerEntry = (WorldBlockTickerEntry)scheduledTicksSorted.GetKey(0);
				if (worldBlockTickerEntry.scheduledTime > GameTimer.Instance.ticks)
				{
					break;
				}
				scheduledTicksSorted.Remove(worldBlockTickerEntry);
				scheduledTicksDict.Remove(worldBlockTickerEntry.GetHashCode());
				chunkToScheduledTicks[worldBlockTickerEntry.GetChunkKey()]?.Remove(worldBlockTickerEntry);
				goto IL_00f5;
			}
			IL_00f5:
			if (!world.IsChunkAreaLoaded(worldBlockTickerEntry.worldPos.x, worldBlockTickerEntry.worldPos.y, worldBlockTickerEntry.worldPos.z))
			{
				int chunkX = World.toChunkXZ(worldBlockTickerEntry.worldPos.x);
				int chunkZ = World.toChunkXZ(worldBlockTickerEntry.worldPos.z);
				if (world.GetChunkSync(chunkX, chunkZ) != null)
				{
					AddScheduledBlockUpdate(worldBlockTickerEntry.clrIdx, worldBlockTickerEntry.worldPos, worldBlockTickerEntry.blockID, (uint)(30 + _rnd.RandomRange(0, 15)));
				}
			}
			else
			{
				execute(worldBlockTickerEntry, _rnd, 0uL);
			}
		}
		return scheduledTicksSorted.Count != 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void tickRandom(ArraySegment<long> _activeChunkSet, GameRandom _rnd)
	{
		if (randomTickIndex >= randomTickChunkKeys.Count)
		{
			randomTickChunkKeys.Clear();
			if (_activeChunkSet.Count > randomTickChunkKeys.Capacity)
			{
				randomTickChunkKeys.Capacity = _activeChunkSet.Count * 2;
			}
			int num = _activeChunkSet.Offset + _activeChunkSet.Count;
			for (int i = _activeChunkSet.Offset; i < num; i++)
			{
				randomTickChunkKeys.Add(_activeChunkSet.Array[i]);
			}
			randomTickCountPerFrame = Math.Max(_activeChunkSet.Count / 100, 1);
			randomTickIndex = 0;
		}
		int num2 = 0;
		while (randomTickIndex < randomTickChunkKeys.Count && num2 < randomTickCountPerFrame)
		{
			long key = randomTickChunkKeys[randomTickIndex];
			Chunk chunkSync = world.ChunkCache.GetChunkSync(key);
			tickChunkRandom(chunkSync, _rnd);
			randomTickIndex++;
			num2++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void tickChunkRandom(Chunk chunk, GameRandom _rnd)
	{
		if (chunk == null || chunk.NeedsLightCalculation || GameTimer.Instance.ticks - chunk.LastTimeRandomTicked < 1200)
		{
			return;
		}
		ulong ticksIfLoaded = GameTimer.Instance.ticks - chunk.LastTimeRandomTicked;
		chunk.LastTimeRandomTicked = GameTimer.Instance.ticks;
		DictionaryKeyList<Vector3i, int> tickedBlocks = chunk.GetTickedBlocks();
		lock (tickedBlocks)
		{
			for (int num = tickedBlocks.list.Count - 1; num >= 0; num--)
			{
				Vector3i vector3i = tickedBlocks.list[num];
				BlockValue block = chunk.GetBlock(World.toBlockXZ(vector3i.x), vector3i.y, World.toBlockXZ(vector3i.z));
				if (scheduledTicksDict.Count == 0 || !scheduledTicksDict.ContainsKey(WorldBlockTickerEntry.ToHashCode(chunk.ClrIdx, vector3i, block.type)))
				{
					block.Block.UpdateTick(world, chunk.ClrIdx, vector3i, block, _bRandomTick: true, ticksIfLoaded, _rnd);
				}
			}
		}
	}

	public void OnChunkAdded(WorldBase _world, Chunk _c, GameRandom _rnd)
	{
		if (!_c.ChunkCustomData.dict.TryGetValue("wbt.sch", out var value) || value == null)
		{
			return;
		}
		_c.ChunkCustomData.Remove("wbt.sch");
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		pooledBinaryReader.SetBaseStream(new MemoryStream(value.data));
		int num = pooledBinaryReader.ReadUInt16();
		int version = pooledBinaryReader.ReadByte();
		for (int i = 0; i < num; i++)
		{
			WorldBlockTickerEntry worldBlockTickerEntry = WorldBlockTickerEntry.Read(pooledBinaryReader, _c.X, _c.Z, version);
			lock (lockObject)
			{
				if (!scheduledTicksDict.ContainsKey(worldBlockTickerEntry.GetHashCode()))
				{
					if (worldBlockTickerEntry.scheduledTime > GameTimer.Instance.ticks)
					{
						add(worldBlockTickerEntry);
					}
					else
					{
						execute(worldBlockTickerEntry, _rnd, GameTimer.Instance.ticks - worldBlockTickerEntry.scheduledTime);
					}
				}
			}
		}
	}

	public void OnChunkRemoved(Chunk _c)
	{
		addScheduleInformationToChunk(_c, _bChunkIsRemoved: true);
	}

	public void OnChunkBeforeSave(Chunk _c)
	{
		addScheduleInformationToChunk(_c, _bChunkIsRemoved: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addScheduleInformationToChunk(Chunk _c, bool _bChunkIsRemoved)
	{
		HashSet<WorldBlockTickerEntry> hashSet = chunkToScheduledTicks[_c.Key];
		lock (lockObject)
		{
			if (hashSet == null)
			{
				return;
			}
			if (_bChunkIsRemoved)
			{
				chunkToScheduledTicks.Remove(_c.Key);
			}
		}
		if (hashSet.Count == 0)
		{
			return;
		}
		ChunkCustomData chunkCustomData = new ChunkCustomData("wbt.sch", ulong.MaxValue, _isSavedToNetwork: false);
		using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true))
		{
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				pooledBinaryWriter.Write((ushort)hashSet.Count);
				pooledBinaryWriter.Write((byte)1);
				foreach (WorldBlockTickerEntry item in hashSet)
				{
					item.Write(pooledBinaryWriter);
					if (_bChunkIsRemoved)
					{
						lock (lockObject)
						{
							scheduledTicksSorted.Remove(item);
							scheduledTicksDict.Remove(item.GetHashCode());
						}
					}
				}
			}
			chunkCustomData.data = pooledExpandableMemoryStream.ToArray();
		}
		_c.ChunkCustomData.Set("wbt.sch", chunkCustomData);
		_c.isModified = true;
	}

	public int GetCount()
	{
		lock (lockObject)
		{
			return scheduledTicksDict.Count;
		}
	}

	public void InvalidateScheduledBlockUpdate(int _clrIdx, Vector3i _pos, int _blockID)
	{
		if (_blockID != 0)
		{
			remove(_clrIdx, _pos, _blockID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void remove(int _clrIdx, Vector3i _pos, int _blockID)
	{
		lock (lockObject)
		{
			int key = WorldBlockTickerEntry.ToHashCode(_clrIdx, _pos, _blockID);
			WorldBlockTickerEntry worldBlockTickerEntry = null;
			if (scheduledTicksDict.ContainsKey(key))
			{
				worldBlockTickerEntry = scheduledTicksDict[key];
				scheduledTicksDict.Remove(key);
				if (worldBlockTickerEntry != null)
				{
					scheduledTicksSorted.Remove(worldBlockTickerEntry);
				}
			}
			long v = WorldChunkCache.MakeChunkKey(World.toChunkXZ(_pos.x), World.toChunkXZ(_pos.z), _clrIdx);
			chunkToScheduledTicks[v]?.Remove(worldBlockTickerEntry);
		}
	}
}

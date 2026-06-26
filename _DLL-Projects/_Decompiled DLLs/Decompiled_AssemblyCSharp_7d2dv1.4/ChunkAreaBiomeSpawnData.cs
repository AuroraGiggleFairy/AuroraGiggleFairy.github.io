using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ChunkAreaBiomeSpawnData
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct CountsAndTime(int _count, int _maxCount, ulong _delayWorldTime)
	{
		public int count = _count;

		public int maxCount = _maxCount;

		public ulong delayWorldTime = _delayWorldTime;

		public override string ToString()
		{
			return $"cnt {count}, maxCnt {maxCount}, wtime {delayWorldTime}";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCurrentSaveVersion = 2;

	public byte biomeId;

	public Rect area;

	public Chunk chunk;

	public bool checkedPOITags;

	public FastTags<TagGroup.Poi> poiTags;

	public int groupsEnabledFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCustomData ccd;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, CountsAndTime> entitesSpawned = new Dictionary<int, CountsAndTime>();

	public ChunkAreaBiomeSpawnData(Chunk _chunk, byte _biomeId, ChunkCustomData _ccd)
	{
		biomeId = _biomeId;
		area = new Rect(_chunk.X * 16, _chunk.Z * 16, 80f, 80f);
		chunk = _chunk;
		ccd = _ccd;
		ccd.TriggerWriteDataDelegate = BeforeWrite;
		if (ccd.data != null)
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
			{
				pooledBinaryReader.SetBaseStream(new MemoryStream(ccd.data));
				read(pooledBinaryReader);
			}
		}
	}

	public bool IsSpawnNeeded(WorldBiomes _worldBiomes, ulong _worldTime)
	{
		BiomeDefinition biome = _worldBiomes.GetBiome(biomeId);
		if (biome == null)
		{
			return false;
		}
		BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName];
		if (biomeSpawnEntityGroupList == null)
		{
			return false;
		}
		for (int i = 0; i < biomeSpawnEntityGroupList.list.Count; i++)
		{
			BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.list[i];
			if (!entitesSpawned.TryGetValue(biomeSpawnEntityGroupData.idHash, out var value))
			{
				return true;
			}
			if (value.count < value.maxCount || _worldTime > value.delayWorldTime)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanSpawn(int _idHash)
	{
		if (entitesSpawned.TryGetValue(_idHash, out var value))
		{
			return value.count < value.maxCount;
		}
		return false;
	}

	public void SetCounts(int _idHash, int _count, int _maxCount)
	{
		entitesSpawned.TryGetValue(_idHash, out var value);
		value.count = _count;
		value.maxCount = _maxCount;
		entitesSpawned[_idHash] = value;
	}

	public void IncCount(int _idHash)
	{
		if (!entitesSpawned.TryGetValue(_idHash, out var value))
		{
			value.count = 1;
		}
		else
		{
			value.count++;
		}
		entitesSpawned[_idHash] = value;
		chunk.isModified = true;
	}

	public void DecCount(int _idHash, bool _killed)
	{
		if (entitesSpawned.TryGetValue(_idHash, out var value))
		{
			value.count = Utils.FastMax(value.count - 1, 0);
			if (_killed)
			{
				value.maxCount = Utils.FastMax(0, value.maxCount - 1);
			}
			entitesSpawned[_idHash] = value;
			chunk.isModified = true;
		}
	}

	public void DecMaxCount(int _idHash)
	{
		if (entitesSpawned.TryGetValue(_idHash, out var value))
		{
			value.maxCount = Utils.FastMax(0, value.maxCount - 1);
			entitesSpawned[_idHash] = value;
			chunk.isModified = true;
		}
	}

	public ulong GetDelayWorldTime(int _idHash)
	{
		entitesSpawned.TryGetValue(_idHash, out var value);
		return value.delayWorldTime;
	}

	public void ResetRespawn(int _idHash, World _world, int _maxCount)
	{
		BiomeDefinition biome = _world.Biomes.GetBiome(biomeId);
		if (biome == null)
		{
			return;
		}
		BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName];
		if (biomeSpawnEntityGroupList != null)
		{
			BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.Find(_idHash);
			if (biomeSpawnEntityGroupData != null)
			{
				entitesSpawned.TryGetValue(_idHash, out var value);
				value.delayWorldTime = _world.worldTime + (ulong)((float)biomeSpawnEntityGroupData.respawnDelayInWorldTime * _world.RandomRange(0.9f, 1.1f));
				value.maxCount = _maxCount;
				entitesSpawned[_idHash] = value;
				chunk.isModified = true;
			}
		}
	}

	public bool DelayAllEnemySpawningUntil(ulong _worldTime, WorldBiomes _worldBiomes)
	{
		bool result = false;
		BiomeDefinition biome = _worldBiomes.GetBiome(biomeId);
		if (biome == null)
		{
			return false;
		}
		BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName];
		if (biomeSpawnEntityGroupList == null)
		{
			return false;
		}
		Dictionary<int, CountsAndTime> dictionary = new Dictionary<int, CountsAndTime>();
		foreach (KeyValuePair<int, CountsAndTime> item in entitesSpawned)
		{
			BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.Find(item.Key);
			if (biomeSpawnEntityGroupData != null && EntityGroups.IsEnemyGroup(biomeSpawnEntityGroupData.entityGroupName))
			{
				CountsAndTime value = item.Value;
				bool flag = false;
				if (value.delayWorldTime < _worldTime)
				{
					value.delayWorldTime = _worldTime;
					flag = true;
				}
				if (value.maxCount > 0)
				{
					value.maxCount = 0;
					flag = true;
				}
				if (flag)
				{
					dictionary[item.Key] = value;
					result = true;
				}
			}
		}
		foreach (KeyValuePair<int, CountsAndTime> item2 in dictionary)
		{
			entitesSpawned[item2.Key] = item2.Value;
		}
		for (int i = 0; i < biomeSpawnEntityGroupList.list.Count; i++)
		{
			BiomeSpawnEntityGroupData biomeSpawnEntityGroupData2 = biomeSpawnEntityGroupList.list[i];
			if (EntityGroups.IsEnemyGroup(biomeSpawnEntityGroupData2.entityGroupName) && !entitesSpawned.ContainsKey(biomeSpawnEntityGroupData2.idHash))
			{
				entitesSpawned[biomeSpawnEntityGroupData2.idHash] = new CountsAndTime(0, 0, _worldTime);
				result = true;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(BinaryReader _br)
	{
		int num = _br.ReadByte();
		entitesSpawned.Clear();
		int num2 = _br.ReadByte();
		CountsAndTime value = default(CountsAndTime);
		for (int i = 0; i < num2; i++)
		{
			if (num <= 1)
			{
				_br.ReadString();
				_br.ReadUInt16();
				_br.ReadUInt64();
				continue;
			}
			int key = _br.ReadInt32();
			int num3 = _br.ReadUInt16();
			value.count = num3 & 0xFF;
			value.maxCount = num3 >> 8;
			value.delayWorldTime = _br.ReadUInt64();
			entitesSpawned[key] = value;
		}
	}

	public void BeforeWrite()
	{
		using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
			write(pooledBinaryWriter);
		}
		ccd.data = pooledExpandableMemoryStream.ToArray();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(BinaryWriter _bw)
	{
		_bw.Write((byte)2);
		int num = 0;
		int num2 = Utils.FastMin(entitesSpawned.Count, 255);
		_bw.Write((byte)num2);
		foreach (KeyValuePair<int, CountsAndTime> item in entitesSpawned)
		{
			_bw.Write(item.Key);
			_bw.Write((ushort)((item.Value.maxCount << 8) | item.Value.count));
			_bw.Write(item.Value.delayWorldTime);
			if (++num >= num2)
			{
				break;
			}
		}
	}

	public override string ToString()
	{
		World world = GameManager.Instance.World;
		ulong worldTime = world.worldTime;
		BiomeDefinition biome = world.Biomes.GetBiome(biomeId);
		if (biome == null)
		{
			return "biome? " + biomeId;
		}
		BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName];
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<int, CountsAndTime> item in entitesSpawned)
		{
			string text = "?";
			if (biomeSpawnEntityGroupList != null)
			{
				BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.Find(item.Key);
				if (biomeSpawnEntityGroupData != null)
				{
					text = biomeSpawnEntityGroupData.entityGroupName + " " + biomeSpawnEntityGroupData.daytime;
				}
			}
			ulong num = item.Value.delayWorldTime - worldTime;
			if ((long)num < 0L)
			{
				num = 0uL;
			}
			stringBuilder.Append($"{text} #{item.Value.count}/{item.Value.maxCount} {GameUtils.WorldTimeDeltaToString(num)}, ");
		}
		return string.Format("biomeId {0}, XZ {1} {2}: {3}", biomeId, area.x.ToCultureInvariantString("0"), area.y.ToCultureInvariantString("0"), stringBuilder.ToString());
	}
}

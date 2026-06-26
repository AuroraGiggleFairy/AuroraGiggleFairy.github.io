using System.Collections.Generic;
using UnityEngine;

public class SpawnManagerBiomes : SpawnManagerAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cAnimalMinDistance = 48;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cAnimalMaxDistance = 70;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cEnemyMinDistance = 28;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cEnemyMaxDistance = 54;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> spawnNearList = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> spawnPIs = new List<PrefabInstance>();

	public SpawnManagerBiomes(World _world)
		: base(_world)
	{
		_world.EntityUnloadedDelegates += OnEntityUnloaded;
	}

	public void Cleanup()
	{
		world.EntityUnloadedDelegates -= OnEntityUnloaded;
		world = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEntityUnloaded(Entity entity, EnumRemoveEntityReason _reason)
	{
		if (_reason == EnumRemoveEntityReason.Undef || _reason == EnumRemoveEntityReason.Unloaded || entity.GetSpawnerSource() != EnumSpawnerSource.Biome)
		{
			return;
		}
		Chunk chunk = (Chunk)world.GetChunkSync(entity.GetSpawnerSourceChunkKey());
		if (chunk == null)
		{
			return;
		}
		ChunkAreaBiomeSpawnData chunkBiomeSpawnData = chunk.GetChunkBiomeSpawnData();
		if (chunkBiomeSpawnData == null)
		{
			return;
		}
		int spawnerSourceBiomeIdHash = entity.GetSpawnerSourceBiomeIdHash();
		switch (_reason)
		{
		case EnumRemoveEntityReason.Despawned:
			chunkBiomeSpawnData.DecCount(spawnerSourceBiomeIdHash, _killed: false);
			break;
		case EnumRemoveEntityReason.Killed:
		{
			EntityHuman entityHuman = entity as EntityHuman;
			if ((bool)entityHuman && world.worldTime >= entityHuman.timeToDie)
			{
				chunkBiomeSpawnData.DecCount(spawnerSourceBiomeIdHash, _killed: false);
			}
			else
			{
				chunkBiomeSpawnData.DecCount(spawnerSourceBiomeIdHash, _killed: true);
			}
			break;
		}
		}
	}

	public override void Update(string _spawnerName, bool _bSpawnEnemyEntities, object _userData)
	{
		if (!GameUtils.IsPlaytesting())
		{
			SpawnUpdate(_spawnerName, _bSpawnEnemyEntities, _userData as ChunkAreaBiomeSpawnData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnUpdate(string _spawnerName, bool _isSpawnEnemy, ChunkAreaBiomeSpawnData _spawnData)
	{
		if (_spawnData == null)
		{
			return;
		}
		if (_isSpawnEnemy)
		{
			if (!AIDirector.CanSpawn())
			{
				_isSpawnEnemy = false;
			}
			else if (world.aiDirector.BloodMoonComponent.BloodMoonActive)
			{
				_isSpawnEnemy = false;
			}
		}
		if (!_isSpawnEnemy && GameStats.GetInt(EnumGameStats.AnimalCount) >= GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedAnimals))
		{
			return;
		}
		bool flag = false;
		List<EntityPlayer> players = world.GetPlayers();
		for (int i = 0; i < players.Count; i++)
		{
			EntityPlayer entityPlayer = players[i];
			if (entityPlayer.Spawned && new Rect(entityPlayer.position.x - 40f, entityPlayer.position.z - 40f, 80f, 80f).Overlaps(_spawnData.area))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		int minDistance = (_isSpawnEnemy ? 28 : 48);
		int uNUSED_maxDistance = (_isSpawnEnemy ? 54 : 70);
		if (!world.GetRandomSpawnPositionInAreaMinMaxToPlayers(_spawnData.area, minDistance, uNUSED_maxDistance, _checkBedrolls: true, out var _position))
		{
			return;
		}
		BiomeDefinition biome = world.Biomes.GetBiome(_spawnData.biomeId);
		if (biome == null)
		{
			return;
		}
		BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName];
		if (biomeSpawnEntityGroupList == null)
		{
			return;
		}
		EDaytime eDaytime = (world.IsDaytime() ? EDaytime.Day : EDaytime.Night);
		GameRandom gameRandom = world.GetGameRandom();
		if (!_spawnData.checkedPOITags)
		{
			_spawnData.checkedPOITags = true;
			FastTags<TagGroup.Poi> none = FastTags<TagGroup.Poi>.none;
			Vector3i worldPos = _spawnData.chunk.GetWorldPos();
			world.GetPOIsAtXZ(worldPos.x + 16, worldPos.x + 80 - 16, worldPos.z + 16, worldPos.z + 80 - 16, spawnPIs);
			for (int j = 0; j < spawnPIs.Count; j++)
			{
				PrefabInstance prefabInstance = spawnPIs[j];
				none |= prefabInstance.prefab.Tags;
			}
			_spawnData.poiTags = none;
			bool isEmpty = none.IsEmpty;
			for (int k = 0; k < biomeSpawnEntityGroupList.list.Count; k++)
			{
				BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.list[k];
				if ((biomeSpawnEntityGroupData.POITags.IsEmpty || biomeSpawnEntityGroupData.POITags.Test_AnySet(none)) && (isEmpty || biomeSpawnEntityGroupData.noPOITags.IsEmpty || !biomeSpawnEntityGroupData.noPOITags.Test_AnySet(none)))
				{
					_spawnData.groupsEnabledFlags |= 1 << k;
				}
			}
		}
		int num = 0;
		int num2 = -1;
		int num3 = gameRandom.RandomRange(biomeSpawnEntityGroupList.list.Count);
		int num4 = Utils.FastMin(5, biomeSpawnEntityGroupList.list.Count);
		int num5 = 0;
		while (num5 < num4)
		{
			BiomeSpawnEntityGroupData biomeSpawnEntityGroupData2 = biomeSpawnEntityGroupList.list[num3];
			if ((_spawnData.groupsEnabledFlags & (1 << num3)) != 0 && (biomeSpawnEntityGroupData2.daytime == EDaytime.Any || biomeSpawnEntityGroupData2.daytime == eDaytime))
			{
				bool flag2 = EntityGroups.IsEnemyGroup(biomeSpawnEntityGroupData2.entityGroupName);
				if (!flag2 || _isSpawnEnemy)
				{
					num = biomeSpawnEntityGroupData2.idHash;
					ulong delayWorldTime = _spawnData.GetDelayWorldTime(num);
					if (world.worldTime > delayWorldTime)
					{
						int num6 = biomeSpawnEntityGroupData2.maxCount;
						if (flag2)
						{
							num6 = EntitySpawner.ModifySpawnCountByGameDifficulty(num6);
						}
						_spawnData.ResetRespawn(num, world, num6);
					}
					if (_spawnData.CanSpawn(num))
					{
						num2 = num3;
						break;
					}
				}
			}
			num5++;
			num3 = (num3 + 1) % biomeSpawnEntityGroupList.list.Count;
		}
		if (num2 < 0)
		{
			return;
		}
		Bounds bb = new Bounds(_position, new Vector3(4f, 2.5f, 4f));
		world.GetEntitiesInBounds(typeof(Entity), bb, spawnNearList);
		int count = spawnNearList.Count;
		spawnNearList.Clear();
		if (count <= 0)
		{
			int randomFromGroup = EntityGroups.GetRandomFromGroup(biomeSpawnEntityGroupList.list[num2].entityGroupName, ref lastClassId);
			if (randomFromGroup == 0)
			{
				_spawnData.DecMaxCount(num);
				return;
			}
			_spawnData.IncCount(num);
			Entity entity = EntityFactory.CreateEntity(randomFromGroup, _position);
			entity.SetSpawnerSource(EnumSpawnerSource.Biome, _spawnData.chunk.Key, num);
			world.SpawnEntityInWorld(entity);
			world.DebugAddSpawnedEntity(entity);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogSpawn(Entity _entity, string format, params object[] args)
	{
		format = string.Format("{0} SpawnManagerBiomes {1}, {2}", GameManager.frameCount, _entity ? _entity.ToString() : "", format);
		Log.Warning(format, args);
	}
}

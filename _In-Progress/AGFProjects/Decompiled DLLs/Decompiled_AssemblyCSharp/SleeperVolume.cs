using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class SleeperVolume
{
	public enum ETriggerType
	{
		Active,
		Passive,
		Attack,
		Trigger,
		Wander
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct GroupCount
	{
		public string groupName;

		public int count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct RespawnData
	{
		public string className;

		public int spawnPointIndex;
	}

	public struct SpawnPoint(Vector3i _pos, float _rot, int _blockType)
	{
		public readonly Vector3i pos = _pos;

		public readonly float rot = _rot;

		public readonly int blockType = _blockType;

		public BlockSleeper GetBlock()
		{
			BlockSleeper blockSleeper = Block.list[blockType] as BlockSleeper;
			if (blockSleeper == null)
			{
				blockSleeper = (BlockSleeper)Block.GetBlockByName("sleeperSit");
			}
			return blockSleeper;
		}

		public static SpawnPoint Read(BinaryReader _br, int _version)
		{
			Vector3i vector3i = new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());
			if (_version >= 7 && _version < 20)
			{
				_br.ReadSingle();
				_br.ReadSingle();
				_br.ReadSingle();
			}
			float num = _br.ReadSingle();
			if (_version < 20)
			{
				_br.ReadByte();
			}
			int num2 = 0;
			if (_version > 14)
			{
				string text = _br.ReadString();
				Block blockByName = Block.GetBlockByName(text);
				if (blockByName != null)
				{
					num2 = blockByName.blockID;
				}
				else
				{
					Log.Warning("SpawnPoint Read missing block {0}", text);
				}
			}
			else if (_version >= 9)
			{
				num2 = _br.ReadUInt16();
			}
			return new SpawnPoint(vector3i, num, num2);
		}

		public void Write(BinaryWriter _bw)
		{
			_bw.Write(pos.x);
			_bw.Write(pos.y);
			_bw.Write(pos.z);
			_bw.Write(rot);
			_bw.Write(GetBlock().GetBlockName());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cVersion = 21;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnDelay = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDespawnDelay = 900;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDespawnPassiveDelay = 200;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBedrollClearTime = 24000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPlayerInsideDelayTime = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayerYOffset = 0.8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAttackPaddingXZ = -0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPassivePaddingXZ = -0.3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPassiveNoisePadding = 0.9f;

	public static Vector3i chunkPadding = new Vector3i(12, 1, 12);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i triggerPaddingMin = new Vector3i(8f, 0.7f, 8f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i triggerPaddingMax = new Vector3i(8f, 0.7f, 8f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i unpadding = new Vector3i(14, 16, 14);

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance prefabInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public short groupId;

	[PublicizedFrom(EAccessModifier.Private)]
	public short spawnCountMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public short spawnCountMax;

	public const int cTriggerFlagsMask = 7;

	public const int cFlagsHasScript = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public int flags;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SpawnPoint> spawnPointList = new List<SpawnPoint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> spawnsAvailable;

	[PublicizedFrom(EAccessModifier.Private)]
	public string groupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GroupCount> groupCountList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, RespawnData> respawnMap = new Dictionary<int, RespawnData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> respawnList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int gameStage;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong respawnTime = ulong.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numSpawned;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSpawned;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSpawning;

	[PublicizedFrom(EAccessModifier.Private)]
	public int spawnDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int ticksUntilDespawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer playerTouchedToUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer playerTouchedTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasPassives;

	[PublicizedFrom(EAccessModifier.Private)]
	public ETriggerType triggerState = ETriggerType.Passive;

	public bool wasCleared;

	public bool isQuestExclude;

	public bool isPriority;

	public Vector3i BoxMin;

	public Vector3i BoxMax;

	public Vector3 Center;

	public List<byte> TriggeredByIndices = new List<byte>();

	[PublicizedFrom(EAccessModifier.Private)]
	public MinScript minScript;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWanderingCountdown = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int wanderingCountdown = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameRandom sleeperRandom;

	public static int TickSpawnCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnPerTickMax = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[] difficultyTierScale = new float[7] { 1f, 1f, 1f, 0.9f, 0.9f, 0.9f, 0.9f };

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[][] isHiddenOffsets = new float[2][]
	{
		new float[12]
		{
			-0.7f, 0.3f, 0f, 0.3f, 0.7f, 0.3f, -0.7f, 0.8f, 0f, 0.8f,
			0.7f, 0.8f
		},
		new float[12]
		{
			-0.4f, 0.5f, 0f, 0.5f, 0.4f, 0.5f, -0.4f, 1.5f, 0f, 1.5f,
			0.4f, 1.5f
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagsQuestExclude = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagsPriority = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagsSpawning = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagsCleared = 8;

	public bool IsTrigger
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return TriggeredByIndices.Count > 0;
		}
	}

	public bool IsTriggerAndNoRespawn
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if ((flags & 7) == 3)
			{
				return respawnMap.Count == 0;
			}
			return false;
		}
	}

	public PrefabInstance PrefabInstance => prefabInstance;

	public static void WorldInit()
	{
		if (GameUtils.IsPlaytesting())
		{
			sleeperRandom = GameRandomManager.Instance.CreateGameRandom((int)Stopwatch.GetTimestamp());
		}
		else
		{
			sleeperRandom = GameRandomManager.Instance.CreateGameRandom();
		}
	}

	public static SleeperVolume Create(Prefab.PrefabSleeperVolume psv, Vector3i _boxMin, Vector3i _boxMax)
	{
		SleeperVolume sleeperVolume = new SleeperVolume();
		sleeperVolume.SetMinMax(_boxMin, _boxMax);
		sleeperVolume.groupId = psv.groupId;
		sleeperVolume.isQuestExclude = psv.isQuestExclude;
		sleeperVolume.isPriority = psv.isPriority;
		sleeperVolume.spawnCountMin = psv.spawnCountMin;
		sleeperVolume.spawnCountMax = psv.spawnCountMax;
		sleeperVolume.flags = psv.flags;
		sleeperVolume.TriggeredByIndices = new List<byte>(psv.triggeredByIndices);
		sleeperVolume.groupName = GameStageGroup.CleanName(psv.groupName);
		sleeperVolume.SetScript(psv.minScript);
		sleeperVolume.AddToPrefabInstance();
		return sleeperVolume;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMinMax(Vector3i _boxMin, Vector3i _boxMax)
	{
		BoxMin = _boxMin;
		BoxMax = _boxMax;
		Center = (BoxMin + BoxMax).ToVector3() * 0.5f;
	}

	public bool Intersects(Bounds bounds)
	{
		return BoundsUtils.Intersects(bounds, BoxMin, BoxMax);
	}

	public void AddToPrefabInstance()
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		prefabInstance = dynamicPrefabDecorator.GetPrefabAtPosition(Center);
		if (prefabInstance != null)
		{
			prefabInstance.AddSleeperVolume(this);
		}
	}

	public void AddSpawnPoint(int _x, int _y, int _z, BlockSleeper _block, BlockValue _blockValue)
	{
		if (spawnPointList.Count < 255)
		{
			spawnPointList.Add(new SpawnPoint(new Vector3i(_x, _y, _z), _block.GetSleeperRotation(_blockValue), _blockValue.type));
		}
	}

	public void SetScript(string _script)
	{
		if (string.IsNullOrEmpty(_script))
		{
			minScript = null;
			return;
		}
		minScript = new MinScript();
		minScript.SetText(_script);
	}

	public void Tick(World _world)
	{
		if (isSpawning)
		{
			if (minScript != null && minScript.IsRunning())
			{
				foreach (KeyValuePair<int, RespawnData> item in respawnMap)
				{
					if (!_world.GetEntity(item.Key))
					{
						respawnMap.Clear();
						respawnList = null;
						groupCountList.Clear();
						numSpawned = 0;
						minScript.Restart();
						break;
					}
				}
				minScript.Tick(this);
			}
			if (TickSpawnCount < 2)
			{
				UpdateSpawn(_world);
			}
		}
		if (isSpawning)
		{
			return;
		}
		if (isSpawned)
		{
			if (respawnMap.Count == 0)
			{
				isSpawned = false;
			}
			foreach (KeyValuePair<int, RespawnData> item2 in respawnMap)
			{
				if (!_world.GetEntity(item2.Key))
				{
					isSpawned = false;
					break;
				}
			}
		}
		if (playerTouchedToUpdate != null)
		{
			UpdatePlayerTouched(_world, playerTouchedToUpdate);
			playerTouchedToUpdate = null;
		}
		else if (--ticksUntilDespawn == 0)
		{
			Despawn(_world);
		}
	}

	public int GetPlayerTouchedToUpdateId()
	{
		int result = -1;
		if (playerTouchedToUpdate != null)
		{
			result = playerTouchedToUpdate.entityId;
		}
		return result;
	}

	public int GetPlayerTouchedTriggerId()
	{
		int result = -1;
		if (playerTouchedTrigger != null)
		{
			result = playerTouchedTrigger.entityId;
		}
		return result;
	}

	public void DespawnAndReset(World _world)
	{
		Despawn(_world);
		Reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Despawn(World _world)
	{
		triggerState = ETriggerType.Passive;
		playerTouchedTrigger = null;
		int num = 0;
		foreach (KeyValuePair<int, RespawnData> item in respawnMap)
		{
			EntityAlive entityAlive = _world.GetEntity(item.Key) as EntityAlive;
			if ((bool)entityAlive && entityAlive.IsSleeping)
			{
				entityAlive.IsDespawned = true;
				entityAlive.MarkToUnload();
				num++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Reset()
	{
		playerTouchedToUpdate = null;
		playerTouchedTrigger = null;
		respawnTime = ulong.MaxValue;
		isSpawning = false;
		isSpawned = false;
		wasCleared = false;
		groupCountList = null;
		numSpawned = 0;
		respawnMap.Clear();
		respawnList = null;
		if (minScript != null)
		{
			minScript.Reset();
		}
	}

	public int GetAliveCount()
	{
		int num = 0;
		if (groupCountList != null)
		{
			for (int i = 0; i < groupCountList.Count; i++)
			{
				num += groupCountList[i].count;
			}
		}
		return num - numSpawned + respawnMap.Count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePlayerTouched(World _world, EntityPlayer _playerTouched)
	{
		if (isSpawned || (_world.worldTime < respawnTime && wasCleared))
		{
			return;
		}
		if (_world.worldTime >= respawnTime)
		{
			Reset();
		}
		isSpawning = true;
		isSpawned = true;
		float num = 1f;
		if (prefabInstance != null)
		{
			num = ((prefabInstance.LastQuestClass == null) ? 1f : prefabInstance.LastQuestClass.SpawnMultiplier);
			byte difficultyTier = prefabInstance.prefab.DifficultyTier;
			num *= ((difficultyTier < difficultyTierScale.Length) ? difficultyTierScale[difficultyTier] : difficultyTierScale[difficultyTierScale.Length - 1]);
			if (prefabInstance.LastRefreshType.Test_AnySet(QuestEventManager.banditTag))
			{
				num = 0.2f;
			}
		}
		if (spawnPointList.Count > 0)
		{
			int num2 = 0;
			gameStage = Mathf.Max(0, GetGameStageAround(_playerTouched) + num2);
			if (respawnMap.Count > 0)
			{
				respawnList = new List<int>(respawnMap.Count);
				foreach (KeyValuePair<int, RespawnData> item in respawnMap)
				{
					respawnList.Add(item.Key);
				}
			}
			ResetSpawnsAvailable();
			if (groupCountList != null)
			{
				groupCountList.Clear();
			}
			if (spawnCountMin < 0 || spawnCountMax < 0)
			{
				spawnCountMin = 5;
				spawnCountMax = 6;
			}
			AddSpawnCount(groupName, (float)spawnCountMin * num, (float)spawnCountMax * num);
			spawnDelay = 0;
		}
		if (minScript != null)
		{
			minScript.Run(this, _playerTouched, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetSpawnsAvailable()
	{
		bool flag = false;
		if (prefabInstance != null && prefabInstance.LastRefreshType.Test_AnySet(QuestEventManager.infestedTag))
		{
			flag = true;
		}
		spawnsAvailable = new List<int>(spawnPointList.Count);
		for (int i = 0; i < spawnPointList.Count; i++)
		{
			if (flag || spawnPointList[i].GetBlock().spawnMode != BlockSleeper.eMode.Infested)
			{
				spawnsAvailable.Add(i);
			}
		}
	}

	public void AddSpawnCount(string _groupName, float _min, float _max)
	{
		if (_max == 0f)
		{
			return;
		}
		GroupCount item = default(GroupCount);
		item.groupName = _groupName;
		float num = sleeperRandom.RandomRange(_min, _max);
		int num2 = (int)num;
		if (sleeperRandom.RandomFloat < num - (float)num2)
		{
			num2++;
		}
		if (_min > 0f && num2 == 0)
		{
			num2 = 1;
		}
		item.count = num2;
		if (num2 > 0)
		{
			if (groupCountList == null)
			{
				groupCountList = new List<GroupCount>();
			}
			groupCountList.Add(item);
		}
	}

	public void CheckTouching(World _world, EntityPlayer _player)
	{
		if (IsTriggerAndNoRespawn || _player.IsSpectator)
		{
			return;
		}
		Vector3 position = _player.position;
		position.y += 0.8f;
		ETriggerType eTriggerType = (ETriggerType)(flags & 7);
		if (hasPassives)
		{
			if (position.x >= (float)BoxMin.x - -0.3f && position.x < (float)BoxMax.x + -0.3f && position.y >= (float)BoxMin.y && position.y < (float)BoxMax.y && position.z >= (float)BoxMin.z - -0.3f && position.z < (float)BoxMax.z + -0.3f && eTriggerType != ETriggerType.Passive)
			{
				TouchGroup(_world, _player, setActive: true);
			}
		}
		else if ((eTriggerType == ETriggerType.Attack || eTriggerType == ETriggerType.Trigger) && triggerState != eTriggerType && position.x >= (float)BoxMin.x - -0.1f && position.x < (float)BoxMax.x + -0.1f && position.y >= (float)BoxMin.y && position.y < (float)BoxMax.y && position.z >= (float)BoxMin.z - -0.1f && position.z < (float)BoxMax.z + -0.1f)
		{
			TouchGroup(_world, _player, setActive: true);
		}
		if (playerTouchedToUpdate == null && CheckTrigger(_world, position))
		{
			TouchGroup(_world, _player, setActive: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TouchGroup(World _world, EntityPlayer _player, bool setActive)
	{
		ETriggerType trigger = (ETriggerType)(flags & 7);
		if (groupId == 0 || prefabInstance == null)
		{
			Touch(_world, _player, setActive, trigger);
			return;
		}
		List<SleeperVolume> sleeperVolumes = prefabInstance.sleeperVolumes;
		for (int i = 0; i < sleeperVolumes.Count; i++)
		{
			SleeperVolume sleeperVolume = sleeperVolumes[i];
			if (sleeperVolume.groupId == groupId && !sleeperVolume.IsTriggerAndNoRespawn)
			{
				sleeperVolume.Touch(_world, _player, setActive, trigger);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Touch(World _world, EntityPlayer _player, bool setActive, ETriggerType trigger)
	{
		if (setActive)
		{
			bool flag = (trigger == ETriggerType.Attack || trigger == ETriggerType.Trigger) && (bool)_player;
			foreach (KeyValuePair<int, RespawnData> item in respawnMap)
			{
				int key = item.Key;
				EntityAlive entityAlive = (EntityAlive)_world.GetEntity(key);
				if ((bool)entityAlive)
				{
					if (flag && _player.Stealth.CanSleeperAttackDetect(entityAlive))
					{
						entityAlive.ConditionalTriggerSleeperWakeUp();
						entityAlive.SetAttackTarget(_player, 400);
					}
					else if (trigger == ETriggerType.Wander)
					{
						entityAlive.ConditionalTriggerSleeperWakeUp();
					}
					else if (--wanderingCountdown <= 0)
					{
						wanderingCountdown = 10;
						entityAlive.ConditionalTriggerSleeperWakeUp();
					}
					else
					{
						entityAlive.SetSleeperActive();
					}
				}
			}
			hasPassives = false;
			triggerState = trigger;
		}
		else
		{
			playerTouchedToUpdate = _player;
			ticksUntilDespawn = 900;
			if (hasPassives)
			{
				ticksUntilDespawn = 200;
			}
			if (wasCleared && _world.worldTime < respawnTime)
			{
				respawnTime = Math.Max(respawnTime, _world.worldTime + 1000);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckTrigger(World _world, Vector3 playerPos)
	{
		if (isSpawned)
		{
			Vector3i vector3i = BoxMin - unpadding;
			Vector3i vector3i2 = BoxMax + unpadding;
			if (playerPos.x >= (float)vector3i.x && playerPos.x < (float)vector3i2.x && playerPos.y >= (float)vector3i.y && playerPos.y < (float)vector3i2.y && playerPos.z >= (float)vector3i.z && playerPos.z < (float)vector3i2.z)
			{
				return true;
			}
			return false;
		}
		Vector3i vector3i3 = BoxMin - triggerPaddingMin;
		Vector3i vector3i4 = BoxMax + triggerPaddingMax;
		if (!(playerPos.x >= (float)vector3i3.x) || !(playerPos.x < (float)vector3i4.x) || !(playerPos.y >= (float)vector3i3.y) || !(playerPos.y < (float)vector3i4.y) || !(playerPos.z >= (float)vector3i3.z) || !(playerPos.z < (float)vector3i4.z))
		{
			return false;
		}
		if (wasCleared)
		{
			if (GameUtils.CheckForAnyPlayerHome(GameManager.Instance.World, BoxMin, BoxMax) != GameUtils.EPlayerHomeType.None)
			{
				respawnTime = Math.Max(respawnTime, _world.worldTime + 24000);
				return false;
			}
			return true;
		}
		if (prefabInstance != null)
		{
			_world.UncullPOI(prefabInstance);
		}
		return true;
	}

	public void CheckNoise(World _world, Vector3 pos)
	{
		if (hasPassives && pos.x >= (float)BoxMin.x - 0.9f && pos.x < (float)BoxMax.x + 0.9f && pos.y >= (float)BoxMin.y - 0.9f && pos.y < (float)BoxMax.y + 0.9f && pos.z >= (float)BoxMin.z - 0.9f && pos.z < (float)BoxMax.z + 0.9f && (flags & 7) != 1)
		{
			TouchGroup(_world, null, setActive: true);
		}
	}

	public void OnTriggered(EntityPlayer _player, World _world, int _triggerIndex)
	{
		triggerState = (ETriggerType)(flags & 7);
		playerTouchedTrigger = _player;
		UpdatePlayerTouched(_world, _player);
	}

	public void EntityDied(EntityAlive entity)
	{
		if (respawnMap.Remove(entity.entityId))
		{
			if (respawnList != null)
			{
				respawnList.Remove(entity.entityId);
			}
			_ = numSpawned;
			_ = respawnMap.Count;
			if (!isSpawning)
			{
				ClearedUpdate(entity.world);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearedUpdate(World _world)
	{
		if (!wasCleared && respawnMap.Count <= 0)
		{
			int num = GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays);
			if (num <= 0)
			{
				num = 30;
			}
			respawnTime = _world.worldTime + (uint)(num * 24000);
			wasCleared = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetGameStageAround(EntityPlayer player)
	{
		return GameStageDefinition.CalcGameStageAround(player);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SpawnPointIsHidden(World _world, int _index)
	{
		SpawnPoint spawnPoint = spawnPointList[_index];
		Vector3 vector = spawnPoint.pos.ToVector3();
		vector.x += 0.5f;
		vector.z += 0.5f;
		int num = 0;
		if (spawnPoint.GetBlock().pose == 5)
		{
			num = 1;
		}
		float[] array = isHiddenOffsets[num];
		for (int i = 0; i < _world.Players.list.Count; i++)
		{
			EntityPlayer entityPlayer = _world.Players.list[i];
			Vector3 headPosition = entityPlayer.getHeadPosition();
			int modelLayer = entityPlayer.GetModelLayer();
			entityPlayer.SetModelLayer(2);
			Ray ray = new Ray(headPosition, Vector3.one);
			Vector3 vector2 = Vector3.Cross((vector - headPosition).normalized, Vector3.up);
			for (int j = 0; j < array.Length; j += 2)
			{
				Vector3 vector3 = vector + vector2 * array[j];
				vector3.y += array[j + 1];
				Vector3 vector4 = (ray.direction = vector3 - headPosition);
				if (!Voxel.Raycast(_world, ray, vector4.magnitude, 71, 0f))
				{
					entityPlayer.SetModelLayer(modelLayer);
					return false;
				}
			}
			entityPlayer.SetModelLayer(modelLayer);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindFathestSpawnFromPlayers(World _world)
	{
		int num = -1;
		float num2 = float.MinValue;
		for (int i = 0; i < spawnsAvailable.Count; i++)
		{
			int index = spawnsAvailable[i];
			Vector3i pos = spawnPointList[index].pos;
			if (!_world.CanSleeperSpawnAtPos(pos, minScript == null))
			{
				continue;
			}
			Vector3 vector = pos.ToVector3();
			vector.x += 0.5f;
			vector.z += 0.5f;
			float num3 = float.MaxValue;
			for (int j = 0; j < _world.Players.list.Count; j++)
			{
				Vector3 position = _world.Players.list[j].position;
				float sqrMagnitude = (vector - position).sqrMagnitude;
				if (sqrMagnitude < num3)
				{
					num3 = sqrMagnitude;
				}
			}
			if (num3 > num2)
			{
				num2 = num3;
				num = i;
			}
		}
		if (num < 0)
		{
			return -1;
		}
		int result = spawnsAvailable[num];
		spawnsAvailable.RemoveAt(num);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveSpawnAvailable(int index)
	{
		for (int i = 0; i < spawnsAvailable.Count; i++)
		{
			if (spawnsAvailable[i] == index)
			{
				spawnsAvailable.RemoveAt(i);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSpawn(World _world)
	{
		if (--spawnDelay > 0)
		{
			return;
		}
		spawnDelay = 2;
		bool flag = AIDirector.CanSpawn(2.1f);
		int num = GameStats.GetInt(EnumGameStats.EnemyCount);
		bool flag2 = false;
		if (minScript != null && minScript.IsRunning())
		{
			flag2 = true;
		}
		if (spawnsAvailable != null)
		{
			string text = Time.time.ToCultureInvariantString();
			if (respawnList != null && respawnList.Count > 0)
			{
				int num2 = respawnList[respawnList.Count - 1];
				respawnList.RemoveAt(respawnList.Count - 1);
				Entity entity = _world.GetEntity(num2);
				if ((bool)entity)
				{
					hasPassives = true;
					flag2 = true;
					Log.Out("{0} SleeperVolume {1}: Still alive '{2}'", text, BoxMin, entity.name);
				}
				else
				{
					int num3 = respawnMap[num2].spawnPointIndex;
					if (num3 >= 0)
					{
						RemoveSpawnAvailable(num3);
					}
					else
					{
						num3 = FindSpawnIndex(_world);
					}
					if (num3 >= 0)
					{
						SpawnPoint spawnPoint = spawnPointList[num3];
						if (!CheckSpawnPos(_world, spawnPoint.pos))
						{
							respawnList.Add(num2);
							spawnsAvailable.Add(num3);
							return;
						}
						string className = respawnMap[num2].className;
						Log.Out("{0} SleeperVolume {1}: Restoring {2} ({3}) '{4}', count {5}", text, BoxMin, spawnPoint.pos, World.toChunkXZ(spawnPoint.pos), className, num);
						int entityClass = EntityClass.FromString(className);
						BlockSleeper block = spawnPoint.GetBlock();
						if ((bool)Spawn(_world, entityClass, num3, block))
						{
							respawnMap.Remove(num2);
						}
						flag2 = true;
					}
				}
			}
			else if (flag)
			{
				GameStageDefinition gameStageDefinition = null;
				if (groupCountList != null)
				{
					int num4 = 0;
					for (int i = 0; i < groupCountList.Count; i++)
					{
						num4 += groupCountList[i].count;
						if (num4 > numSpawned)
						{
							GameStageGroup gameStageGroup = GameStageGroup.TryGet(groupCountList[i].groupName);
							if (gameStageGroup == null)
							{
								string text2 = prefabInstance?.name ?? "null";
								Log.Error("{0} SleeperVolume {1} {2}: Spawning group '{3}' missing", text, text2, BoxMin, groupCountList[i].groupName);
								gameStageGroup = GameStageGroup.TryGet("GroupGenericZombie");
							}
							gameStageDefinition = gameStageGroup.spawner;
							break;
						}
					}
				}
				if (gameStageDefinition != null)
				{
					GameStageDefinition.Stage stage = gameStageDefinition.GetStage(gameStage);
					if (stage != null)
					{
						int num5 = FindSpawnIndex(_world);
						if (num5 >= 0)
						{
							SpawnPoint spawnPoint2 = spawnPointList[num5];
							if (!CheckSpawnPos(_world, spawnPoint2.pos))
							{
								spawnsAvailable.Add(num5);
								return;
							}
							BlockSleeper block2 = spawnPoint2.GetBlock();
							if (block2 == null)
							{
								Log.Error("{0} BlockSleeper {1} null, type {2}", text, spawnPoint2.pos, spawnPoint2.blockType);
							}
							else
							{
								string spawnGroup = block2.spawnGroup;
								if (string.IsNullOrEmpty(spawnGroup))
								{
									spawnGroup = stage.GetSpawnGroup(0).groupName;
								}
								int randomFromGroup = EntityGroups.GetRandomFromGroup(spawnGroup, ref lastClassId, sleeperRandom);
								EntityClass.list.TryGetValue(randomFromGroup, out var _value);
								Log.Out("{0} SleeperVolume {1}: Spawning {2} ({3}), group '{4}', class {5}, count {6}", text, BoxMin, spawnPoint2.pos, World.toChunkXZ(spawnPoint2.pos), spawnGroup, (_value != null) ? _value.entityClassName : "?", num);
								if ((bool)Spawn(_world, randomFromGroup, num5, block2))
								{
									numSpawned++;
								}
								flag2 = true;
							}
						}
					}
				}
			}
		}
		if (flag2)
		{
			return;
		}
		isSpawning = false;
		respawnList = null;
		if (numSpawned == 0)
		{
			if (respawnMap.Count == 0)
			{
				wasCleared = true;
			}
			Log.Out("{0} SleeperVolume {1}: None spawned, canSpawn {2}, respawnCnt {3}", Time.time.ToCultureInvariantString(), BoxMin, flag, respawnMap.Count);
		}
		else
		{
			ClearedUpdate(_world);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindSpawnIndex(World _world)
	{
		if (spawnsAvailable.Count == 0)
		{
			ResetSpawnsAvailable();
		}
		int num = sleeperRandom.RandomRange(0, spawnsAvailable.Count);
		for (int num2 = spawnsAvailable.Count; num2 > 0; num2--)
		{
			int num3 = spawnsAvailable[num];
			Vector3i pos = spawnPointList[num3].pos;
			if (_world.CanSleeperSpawnAtPos(pos, _checkBelow: true) && SpawnPointIsHidden(_world, num3))
			{
				spawnsAvailable.RemoveAt(num);
				return num3;
			}
			if (++num >= spawnsAvailable.Count)
			{
				num = 0;
			}
		}
		return FindFathestSpawnFromPlayers(_world);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckSpawnPos(World _world, Vector3i pos)
	{
		if (GameManager.bRecordNextSession || GameManager.bPlayRecordedSession)
		{
			return true;
		}
		Chunk chunk = (Chunk)_world.GetChunkFromWorldPos(pos);
		if (chunk == null || chunk.IsInternalBlocksCulled || chunk.NeedsCopying || chunk.NeedsRegeneration)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive Spawn(World _world, int entityClass, int spawnIndex, BlockSleeper block)
	{
		SpawnPoint spawnPoint = spawnPointList[spawnIndex];
		Vector3 vector = spawnPoint.pos.ToVector3();
		vector.x += 0.502f;
		vector.z += 0.501f;
		if (!EntityClass.list.TryGetValue(entityClass, out var _value))
		{
			Log.Warning("Spawn class {0} is missing", entityClass);
			entityClass = EntityClass.FromString("zombieArlene");
		}
		else if (block != null && block.ExcludesWalkType(EntityAlive.GetSpawnWalkType(_value)))
		{
			Log.Warning("Spawn {0} can't walk on block {1} with walkType {2}", _value.entityClassName, block, EntityAlive.GetSpawnWalkType(_value));
			return null;
		}
		EntityAlive entityAlive = (EntityAlive)EntityFactory.CreateEntity(entityClass, vector, new Vector3(0f, spawnPoint.rot, 0f));
		if (!entityAlive)
		{
			Log.Error("Spawn class {0} is null", entityClass);
			return null;
		}
		TickSpawnCount++;
		entityAlive.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		entityAlive.IsSleeperPassive = true;
		entityAlive.SleeperSpawnPosition = vector;
		entityAlive.SleeperSpawnLookDir = block.look;
		entityAlive.SetSleeper();
		entityAlive.TriggerSleeperPose(block.pose);
		_world.SpawnEntityInWorld(entityAlive);
		RespawnData value = default(RespawnData);
		value.className = EntityClass.list[entityClass].entityClassName;
		value.spawnPointIndex = spawnIndex;
		respawnMap.Add(entityAlive.entityId, value);
		hasPassives = true;
		SpawnParticle("sleeperSpawn", entityAlive);
		if ((bool)playerTouchedTrigger)
		{
			GameManager.Instance.StartCoroutine(WakeAttackLater(entityAlive, playerTouchedTrigger));
		}
		return entityAlive;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnParticle(string _particleName, EntityAlive _zombie)
	{
		World world = _zombie.world;
		Vector3 position = _zombie.position;
		position.y += 0.5f;
		Vector3i vector3i = World.worldToBlockPos(position);
		vector3i.y++;
		if (!world.GetBlock(vector3i).isair)
		{
			vector3i.y--;
			float lightBrightness = world.GetLightBrightness(vector3i);
			ParticleEffect pe = new ParticleEffect(_particleName, position, lightBrightness, Color.white, null, null, _OLDCreateColliders: false);
			world.GetGameManager().SpawnParticleEffectServer(pe, _zombie.entityId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator WakeAttackLater(EntityAlive _ea, EntityPlayer _playerTouched)
	{
		yield return new WaitForSeconds(1f);
		if ((bool)_ea && (bool)_playerTouched)
		{
			_ea.ConditionalTriggerSleeperWakeUp();
			_ea.SetAttackTarget(_playerTouched, 400);
		}
	}

	public List<SpawnPoint> GetSpawnPoints()
	{
		return spawnPointList;
	}

	public static SleeperVolume Read(BinaryReader _br)
	{
		SleeperVolume sleeperVolume = new SleeperVolume();
		int num = _br.ReadByte();
		string name = _br.ReadString();
		name = GameStageGroup.CleanName(name);
		if (num >= 13)
		{
			if (num >= 16)
			{
				sleeperVolume.groupId = _br.ReadInt16();
			}
			sleeperVolume.spawnCountMin = _br.ReadInt16();
			sleeperVolume.spawnCountMax = _br.ReadInt16();
		}
		sleeperVolume.groupName = name;
		sleeperVolume.SetMinMax(new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()), new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()));
		sleeperVolume.respawnTime = _br.ReadUInt64();
		if (num <= 13)
		{
			_br.ReadUInt64();
		}
		sleeperVolume.numSpawned = _br.ReadInt32();
		if (num > 7)
		{
			_br.ReadInt32();
		}
		sleeperVolume.gameStage = _br.ReadInt32();
		switch (num)
		{
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
			_br.ReadString();
			break;
		case 4:
			_br.ReadInt32();
			break;
		}
		if (num >= 10)
		{
			_br.ReadString();
			_br.ReadInt32();
		}
		if (num > 5)
		{
			sleeperVolume.ticksUntilDespawn = _br.ReadInt32();
		}
		if (num >= 14)
		{
			ushort num2 = _br.ReadUInt16();
			sleeperVolume.isQuestExclude = (num2 & 1) > 0;
			sleeperVolume.isPriority = (num2 & 2) > 0;
			sleeperVolume.isSpawning = (num2 & 4) > 0;
			sleeperVolume.wasCleared = (num2 & 8) > 0;
			if (num >= 18)
			{
				sleeperVolume.flags = _br.ReadInt32();
			}
		}
		else
		{
			sleeperVolume.isSpawning = _br.ReadBoolean();
			sleeperVolume.wasCleared = _br.ReadBoolean();
			if (num >= 12)
			{
				sleeperVolume.isQuestExclude = _br.ReadBoolean();
			}
		}
		int num3 = _br.ReadByte();
		if (num3 > 0)
		{
			for (int i = 0; i < num3; i++)
			{
				sleeperVolume.spawnPointList.Add(SpawnPoint.Read(_br, num));
			}
		}
		if (num > 1)
		{
			num3 = _br.ReadByte();
			if (num3 > 0)
			{
				sleeperVolume.spawnsAvailable = new List<int>(num3);
				for (int j = 0; j < num3; j++)
				{
					sleeperVolume.spawnsAvailable.Add(_br.ReadByte());
				}
			}
		}
		num3 = _br.ReadByte();
		if (num3 > 0)
		{
			for (int k = 0; k < num3; k++)
			{
				_br.ReadInt32();
			}
			sleeperVolume.hasPassives = true;
		}
		if (num >= 8)
		{
			num3 = _br.ReadByte();
			if (num3 > 0)
			{
				RespawnData value = default(RespawnData);
				for (int l = 0; l < num3; l++)
				{
					int key = _br.ReadInt32();
					value.className = _br.ReadString();
					value.spawnPointIndex = ((num >= 17) ? _br.ReadByte() : (-1));
					sleeperVolume.respawnMap.Add(key, value);
				}
			}
		}
		num3 = _br.ReadByte();
		if (num3 > 0)
		{
			sleeperVolume.groupCountList = new List<GroupCount>(num3);
			GroupCount item = default(GroupCount);
			for (int m = 0; m < num3; m++)
			{
				item.groupName = name;
				if (num >= 21)
				{
					item.groupName = _br.ReadString();
				}
				item.count = _br.ReadInt32();
				sleeperVolume.groupCountList.Add(item);
			}
		}
		if (num >= 19)
		{
			num3 = _br.ReadByte();
			sleeperVolume.TriggeredByIndices.Clear();
			if (num3 > 0)
			{
				for (int n = 0; n < num3; n++)
				{
					sleeperVolume.TriggeredByIndices.Add(_br.ReadByte());
				}
			}
		}
		if ((sleeperVolume.flags & 0x10) > 0)
		{
			sleeperVolume.minScript = MinScript.Read(_br);
		}
		return sleeperVolume;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)21);
		_bw.Write(groupName ?? string.Empty);
		_bw.Write(groupId);
		_bw.Write(spawnCountMin);
		_bw.Write(spawnCountMax);
		_bw.Write(BoxMin.x);
		_bw.Write(BoxMin.y);
		_bw.Write(BoxMin.z);
		_bw.Write(BoxMax.x);
		_bw.Write(BoxMax.y);
		_bw.Write(BoxMax.z);
		_bw.Write(respawnTime);
		_bw.Write(numSpawned);
		_bw.Write(0);
		_bw.Write(gameStage);
		_bw.Write(string.Empty);
		_bw.Write(0);
		_bw.Write(ticksUntilDespawn);
		ushort num = 0;
		if (isQuestExclude)
		{
			num |= 1;
		}
		if (isPriority)
		{
			num |= 2;
		}
		if (isSpawning)
		{
			num |= 4;
		}
		if (wasCleared)
		{
			num |= 8;
		}
		_bw.Write(num);
		flags &= -17;
		if (minScript != null && minScript.HasData())
		{
			flags |= 16;
		}
		_bw.Write(flags);
		int count = spawnPointList.Count;
		_bw.Write((byte)count);
		for (int i = 0; i < count; i++)
		{
			spawnPointList[i].Write(_bw);
		}
		int num2 = ((spawnsAvailable != null) ? spawnsAvailable.Count : 0);
		_bw.Write((byte)num2);
		for (int j = 0; j < num2; j++)
		{
			_bw.Write((byte)spawnsAvailable[j]);
		}
		_bw.Write((byte)0);
		_bw.Write((byte)((respawnMap != null) ? ((uint)respawnMap.Count) : 0u));
		if (respawnMap != null)
		{
			foreach (KeyValuePair<int, RespawnData> item in respawnMap)
			{
				_bw.Write(item.Key);
				_bw.Write(item.Value.className);
				_bw.Write((byte)item.Value.spawnPointIndex);
			}
		}
		int num3 = ((groupCountList != null) ? groupCountList.Count : 0);
		if (num3 > 255)
		{
			num3 = 255;
			Log.Error("{0}, groupCountList > 255", this);
		}
		_bw.Write((byte)num3);
		if (groupCountList != null)
		{
			for (int k = 0; k < groupCountList.Count; k++)
			{
				_bw.Write(groupCountList[k].groupName);
				_bw.Write(groupCountList[k].count);
			}
		}
		_bw.Write((byte)TriggeredByIndices.Count);
		for (int l = 0; l < TriggeredByIndices.Count; l++)
		{
			_bw.Write(TriggeredByIndices[l]);
		}
		if ((flags & 0x10) > 0)
		{
			minScript.Write(_bw);
		}
	}

	public override string ToString()
	{
		string text = ((groupCountList != null && groupCountList.Count > 0) ? groupCountList[0].groupName : "");
		return $"{BoxMin} {text} G{groupId} Trig{(IsTrigger ? 1 : 0)} RespawnC{respawnMap.Count}";
	}

	[Conditional("DEBUG_SLEEPERLOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogSleeper(string format, params object[] args)
	{
		format = $"{GameManager.frameTime.ToCultureInvariantString()} {GameManager.frameCount} SleeperVolume {format}";
		Log.Warning(format, args);
	}

	public void Draw(float _duration)
	{
		Vector3 minPos = BoxMin.ToVector3() - Origin.position;
		Vector3 maxPos = BoxMax.ToVector3();
		maxPos -= Origin.position;
		Color color = GetColor();
		Utils.DrawBoxLines(minPos, maxPos, color, _duration);
	}

	public void DrawDebugLines(float _duration)
	{
		string name = $"SleeperVolume{BoxMin},{BoxMax}";
		Color color = GetColor();
		Vector3 cornerPos = BoxMin.ToVector3();
		Vector3 cornerPos2 = BoxMax.ToVector3();
		cornerPos += DebugLines.InsideOffsetV;
		cornerPos2 -= DebugLines.InsideOffsetV;
		DebugLines.Create(name, GameManager.Instance.World.GetPrimaryPlayer().RootTransform, color, color, 0.05f, 0.05f, _duration).AddCube(cornerPos, cornerPos2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color GetColor()
	{
		Color result = (isQuestExclude ? Color.red : Color.green);
		if (respawnMap.Count > 0)
		{
			result.b = 1f;
		}
		if (IsTrigger)
		{
			result = new Color(0.25f, 0.25f, 0.25f);
		}
		if (wasCleared)
		{
			result.r *= 0.4f;
			result.g *= 0.4f;
			result.b *= 0.4f;
			result.a = 0.16f;
		}
		return result;
	}

	public string GetDescription()
	{
		long num = (long)(respawnTime - GameManager.Instance.World.worldTime);
		if (num < 0)
		{
			num = 0L;
		}
		int num2 = -1;
		int num3 = 0;
		if (groupCountList != null)
		{
			num2 = groupCountList.Count;
			for (int i = 0; i > groupCountList.Count; i++)
			{
				num3 += groupCountList[i].count;
			}
		}
		return string.Format("{0}, grpId {1}, {2} ({3}), cntList {4}/{5}, respawnCnt {6}, spawned {7}, clear{8}, plHome {9}, respawnIn {10}, {11}", BoxMin, groupId, (ETriggerType)(flags & 7), triggerState, num2, num3, respawnMap.Count, numSpawned, wasCleared, GameUtils.CheckForAnyPlayerHome(GameManager.Instance.World, BoxMin, BoxMax), DurationToString(num), (prefabInstance != null) ? (prefabInstance.name + ", volumes " + prefabInstance.sleeperVolumes.Count) : "?");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string DurationToString(long duration)
	{
		string text = "";
		int num = (int)((double)duration / 1000.0 / 24.0);
		if (num > 0)
		{
			text += num.ToString("0:");
		}
		int num2 = (int)((double)duration / 1000.0) % 24;
		if (num > 0 || num2 > 0)
		{
			text += num2.ToString("00:");
		}
		int num3 = (int)((double)duration / 1000.0 * 60.0) % 60;
		if (num > 0 || num2 > 0 || num3 > 0)
		{
			text += num3.ToString("00:");
		}
		return text + ((int)((double)duration / 1000.0 * 60.0 * 60.0) % 60).ToString("00");
	}
}

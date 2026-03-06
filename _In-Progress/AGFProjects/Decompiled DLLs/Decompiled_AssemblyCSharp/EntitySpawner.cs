using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EntitySpawner
{
	public delegate bool ES_CheckSpawnPrecondition(EntitySpawner _es, out EntityPlayer _outPlayerToAttack);

	public delegate bool ES_GetSpawnPosition(EntitySpawner _es, EntityPlayer _inPlayerToAttack, out EntityPlayer _outPlayerToAttack, out Vector3 _pos);

	public delegate int ES_ModifySpawnCount(EntitySpawner _es, int spawnCount);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFileVersion = 3;

	public Vector3i position;

	public Vector3i size;

	public int triggerDiameter;

	public string entitySpawnerClassName;

	public bool bCaveSpawn;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int totalSpawnedThisWave;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeDelayToNextWave;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeDelayBetweenSpawns;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ulong worldTimeNextWave;

	[PublicizedFrom(EAccessModifier.Protected)]
	public PList<int> entityIdSpawned;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int currentWave;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int lastDaySpawnCalled;

	public int numberToSpawnThisWave;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float lastTimeSpawnCalled;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	public int CurrentWave => currentWave;

	public static int ModifySpawnCountByGameDifficulty(int count)
	{
		if (count < 1)
		{
			return count;
		}
		return (int)Mathf.Max(Mathf.Floor((GameStats.GetBool(EnumGameStats.EnemySpawnMode) ? 1f : 0f) * (float)count + 0.5f), 1f);
	}

	public EntitySpawner()
	{
		position = Vector3i.zero;
		size = Vector3i.one;
		totalSpawnedThisWave = 0;
		timeDelayToNextWave = 0f;
		timeDelayBetweenSpawns = 0f;
		currentWave = 0;
		numberToSpawnThisWave = 0;
		lastDaySpawnCalled = -1;
		bCaveSpawn = false;
		entityIdSpawned = new PList<int>
		{
			writeElement = [PublicizedFrom(EAccessModifier.Internal)] (BinaryWriter _bw, int _el) =>
			{
				_bw.Write(_el);
			},
			readElement = [PublicizedFrom(EAccessModifier.Internal)] (BinaryReader _br, uint _version) => _br.ReadInt32()
		};
	}

	public EntitySpawner(string _esClassname, Vector3i _position, Vector3i _size, int _triggerDiameter, ICollection<int> _entityIdsAlreadySpawned = null)
		: this()
	{
		position = _position;
		size = _size;
		triggerDiameter = _triggerDiameter;
		entitySpawnerClassName = _esClassname;
		if (_entityIdsAlreadySpawned != null)
		{
			entityIdSpawned.AddRange(_entityIdsAlreadySpawned);
		}
		timeDelayBetweenSpawns = (EntitySpawnerClass.list.ContainsKey(entitySpawnerClassName) ? EntitySpawnerClass.list[entitySpawnerClassName].Day(0).delayBetweenSpawns : 0f);
	}

	public ICollection<int> GetEntityIdsSpaned()
	{
		return entityIdSpawned;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)3);
		_bw.Write(position.x);
		_bw.Write(position.y);
		_bw.Write(position.z);
		_bw.Write((short)size.x);
		_bw.Write((short)size.y);
		_bw.Write((short)size.z);
		_bw.Write((ushort)triggerDiameter);
		_bw.Write(entitySpawnerClassName);
		_bw.Write((short)totalSpawnedThisWave);
		_bw.Write(timeDelayToNextWave);
		_bw.Write(timeDelayBetweenSpawns);
		entityIdSpawned.Write(_bw);
		_bw.Write((short)currentWave);
		_bw.Write(lastDaySpawnCalled);
		_bw.Write(numberToSpawnThisWave);
		_bw.Write(worldTimeNextWave);
		_bw.Write(bCaveSpawn);
	}

	public void Read(BinaryReader _br)
	{
		byte b = _br.ReadByte();
		position = new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());
		size = new Vector3i(_br.ReadInt16(), _br.ReadInt16(), _br.ReadInt16());
		triggerDiameter = _br.ReadUInt16();
		entitySpawnerClassName = _br.ReadString();
		if (!EntitySpawnerClass.list.ContainsKey(entitySpawnerClassName))
		{
			string[] obj = new string[5] { "Entity spawner at pos ", null, null, null, null };
			Vector3i vector3i = position;
			obj[1] = vector3i.ToString();
			obj[2] = " contains invalid spawner class reference '";
			obj[3] = entitySpawnerClassName;
			obj[4] = "'";
			Log.Warning(string.Concat(obj));
			entitySpawnerClassName = EntitySpawnerClass.DefaultClassName.name;
		}
		totalSpawnedThisWave = _br.ReadInt16();
		timeDelayToNextWave = _br.ReadSingle();
		timeDelayBetweenSpawns = _br.ReadSingle();
		entityIdSpawned.Read(_br);
		currentWave = _br.ReadInt16();
		lastDaySpawnCalled = _br.ReadInt32();
		numberToSpawnThisWave = _br.ReadInt32();
		if (b > 1)
		{
			worldTimeNextWave = _br.ReadUInt64();
		}
		if (b > 2)
		{
			bCaveSpawn = _br.ReadBoolean();
		}
	}

	public void Spawn(World _world, int _day, bool _bSpawnEnemies)
	{
		if (!AIDirector.CanSpawn())
		{
			return;
		}
		SpawnManually(_world, _day, _bSpawnEnemies, [PublicizedFrom(EAccessModifier.Internal)] (EntitySpawner _es, out EntityPlayer _outPlayerToAttack) =>
		{
			_outPlayerToAttack = null;
			EntitySpawnerClass entitySpawnerClass = EntitySpawnerClass.list[_es.entitySpawnerClassName].Day(_day);
			if (entitySpawnerClass.bIgnoreTrigger)
			{
				if (entitySpawnerClass.bAttackPlayerImmediately)
				{
					_outPlayerToAttack = _world.GetClosestPlayer(position.x, position.y, position.z, 0, 160.0);
				}
				return true;
			}
			for (int i = 0; i < _world.Players.list.Count; i++)
			{
				if (_outPlayerToAttack == null)
				{
					Vector3 vector = _world.Players.list[i].GetPosition();
					if (Mathf.Abs(vector.x - (float)position.x) <= (float)(_es.triggerDiameter / 2) && Mathf.Abs(vector.y - (float)position.y) <= (float)(_es.triggerDiameter / 2) && Mathf.Abs(vector.z - (float)position.z) <= (float)(_es.triggerDiameter / 2))
					{
						_outPlayerToAttack = _world.Players.list[i];
					}
				}
				for (int j = 0; j < _world.Players.list[i].SpawnPoints.Count; j++)
				{
					Vector3 vector2 = _world.Players.list[i].SpawnPoints[j].ToVector3();
					if (Mathf.Abs(vector2.x - (float)position.x) <= (float)(_es.triggerDiameter / 2) && Mathf.Abs(vector2.y - (float)position.y) <= (float)(_es.triggerDiameter / 2) && Mathf.Abs(vector2.z - (float)position.z) <= (float)(_es.triggerDiameter / 2))
					{
						_outPlayerToAttack = null;
						return false;
					}
				}
			}
			return _outPlayerToAttack != null;
		}, [PublicizedFrom(EAccessModifier.Internal)] (EntitySpawner _es, EntityPlayer _inPlayerToAttack, out EntityPlayer _outPlayerToAttack, out Vector3 _pos) =>
		{
			_outPlayerToAttack = _inPlayerToAttack;
			int x;
			int y;
			int z;
			if (bCaveSpawn)
			{
				if (!_world.FindRandomSpawnPointNearPositionUnderground(_es.position.ToVector3(), 16, out x, out y, out z, _es.size.ToVector3()))
				{
					_pos = Vector3.zero;
					return false;
				}
			}
			else
			{
				bool bSpawnOnGround = EntitySpawnerClass.list[_es.entitySpawnerClassName].Day(_day).bSpawnOnGround;
				if (!_world.FindRandomSpawnPointNearPosition(_es.position.ToVector3(), 16, out x, out y, out z, _es.size.ToVector3(), bSpawnOnGround, _bIgnoreCanMobsSpawnOn: true))
				{
					_pos = Vector3.zero;
					return false;
				}
			}
			_pos = new Vector3(x, y, z);
			return true;
		}, null, null);
	}

	public void ResetSpawner()
	{
		resetRuntimeVariables();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetRuntimeVariables()
	{
		totalSpawnedThisWave = 0;
		timeDelayToNextWave = 0f;
		timeDelayBetweenSpawns = 0f;
		entityIdSpawned.Clear();
		currentWave = 0;
		numberToSpawnThisWave = 0;
	}

	public void SpawnManually(World _world, int _day, bool _bSpawnEnemyEntities, ES_CheckSpawnPrecondition _checkSpawnPrecondition, ES_GetSpawnPosition _getSpawnPosition, ES_ModifySpawnCount _checkSpawnCount, List<Entity> _spawned)
	{
		if (entitySpawnerClassName == null)
		{
			return;
		}
		EntitySpawnerClassForDay entitySpawnerClassForDay = EntitySpawnerClass.list[entitySpawnerClassName];
		if (entitySpawnerClassForDay == null)
		{
			return;
		}
		EntitySpawnerClass entitySpawnerClass = entitySpawnerClassForDay.Day(_day);
		if (lastDaySpawnCalled != -1 && lastDaySpawnCalled != _day && entitySpawnerClass.bPropResetToday)
		{
			resetRuntimeVariables();
		}
		lastDaySpawnCalled = _day;
		if ((entitySpawnerClass.numberOfWaves > 0 && currentWave >= entitySpawnerClass.numberOfWaves) || (entitySpawnerClass.spawnAtTimeOfDay != EDaytime.Any && !(_world.IsDaytime() ? (entitySpawnerClass.spawnAtTimeOfDay == EDaytime.Day) : (entitySpawnerClass.spawnAtTimeOfDay == EDaytime.Night))))
		{
			return;
		}
		float num = Time.time - lastTimeSpawnCalled;
		lastTimeSpawnCalled = Time.time;
		if (timeDelayToNextWave > 0f)
		{
			timeDelayToNextWave -= num;
			return;
		}
		if (timeDelayBetweenSpawns > 0f)
		{
			timeDelayBetweenSpawns -= num;
			if (timeDelayBetweenSpawns > 0f)
			{
				return;
			}
		}
		EntityPlayer _outPlayerToAttack;
		bool flag = _checkSpawnPrecondition(this, out _outPlayerToAttack);
		if (_outPlayerToAttack != null && entitySpawnerClass.daysToRespawnIfPlayerLeft != 0 && worldTimeNextWave != 0L && worldTimeNextWave < _world.worldTime)
		{
			worldTimeNextWave = _world.worldTime + (ulong)((long)entitySpawnerClass.daysToRespawnIfPlayerLeft * 24000L);
		}
		if (worldTimeNextWave != 0L && worldTimeNextWave >= _world.worldTime)
		{
			return;
		}
		worldTimeNextWave = 0uL;
		if (!flag)
		{
			return;
		}
		for (int i = 0; i < entityIdSpawned.Count; i++)
		{
			Entity entity = null;
			if ((entity = _world.GetEntity(entityIdSpawned[i])) == null || entity.IsDead())
			{
				entityIdSpawned.MarkToRemove(entityIdSpawned[i]);
			}
		}
		entityIdSpawned.RemoveAllMarked();
		if (numberToSpawnThisWave == 0)
		{
			numberToSpawnThisWave = _world.GetGameRandom().RandomRange(ModifySpawnCountByGameDifficulty(entitySpawnerClass.totalPerWaveMin), ModifySpawnCountByGameDifficulty(entitySpawnerClass.totalPerWaveMax + 1));
			if (_checkSpawnCount != null)
			{
				numberToSpawnThisWave = _checkSpawnCount(this, numberToSpawnThisWave);
			}
			Log.Out("Spawning this wave: " + numberToSpawnThisWave);
		}
		if (totalSpawnedThisWave >= numberToSpawnThisWave)
		{
			if ((float)entityIdSpawned.Count <= Utils.FastMax((float)numberToSpawnThisWave * 0.2f, 1f))
			{
				timeDelayToNextWave = entitySpawnerClass.delayToNextWave;
				if (entitySpawnerClass.daysToRespawnIfPlayerLeft > 0)
				{
					worldTimeNextWave = _world.worldTime + (ulong)((long)entitySpawnerClass.daysToRespawnIfPlayerLeft * 24000L);
				}
				Log.Out("Start a new wave '" + entitySpawnerClassName + "'. timeout=" + timeDelayToNextWave.ToCultureInvariantString() + "s. worldtime=" + worldTimeNextWave);
				totalSpawnedThisWave = 0;
				numberToSpawnThisWave = 0;
				currentWave++;
			}
		}
		else
		{
			if (entityIdSpawned.Count >= entitySpawnerClass.totalAlive)
			{
				return;
			}
			int num2 = 1;
			if (entitySpawnerClass.delayBetweenSpawns == 0f)
			{
				num2 = Utils.FastMin(numberToSpawnThisWave, entitySpawnerClass.totalAlive - entityIdSpawned.Count);
			}
			for (int j = 0; j < num2; j++)
			{
				if (!_getSpawnPosition(this, _outPlayerToAttack, out _outPlayerToAttack, out var _pos))
				{
					continue;
				}
				int randomFromGroup = EntityGroups.GetRandomFromGroup(entitySpawnerClass.entityGroupName, ref lastClassId);
				if (!_bSpawnEnemyEntities && EntityClass.list[randomFromGroup].bIsEnemyEntity)
				{
					continue;
				}
				Entity entity2 = EntityFactory.CreateEntity(randomFromGroup, _pos, new Vector3(0f, _world.GetGameRandom().RandomFloat * 360f, 0f));
				_world.SpawnEntityInWorld(entity2);
				entityIdSpawned.Add(entity2.entityId);
				_spawned?.Add(entity2);
				if (totalSpawnedThisWave == 0 && entitySpawnerClass.startSound != null && entitySpawnerClass.startSound.Length > 0)
				{
					_world.GetGameManager().PlaySoundAtPositionServer(position.ToVector3(), entitySpawnerClass.startSound, AudioRolloffMode.Custom, 300);
				}
				totalSpawnedThisWave++;
				EntityAlive entityAlive = entity2 as EntityAlive;
				if ((bool)entityAlive)
				{
					if (entitySpawnerClass.bAttackPlayerImmediately && _outPlayerToAttack != null)
					{
						entityAlive.SetRevengeTarget(_outPlayerToAttack);
					}
					if (entitySpawnerClass.bTerritorial)
					{
						entityAlive.setHomeArea(new Vector3i(_pos), entitySpawnerClass.territorialRange);
					}
				}
				entity2.SetSpawnerSource(entitySpawnerClassForDay.bDynamicSpawner ? EnumSpawnerSource.Dynamic : EnumSpawnerSource.StaticSpawner);
				Log.Out("Spawned " + entity2?.ToString() + " at " + _pos.ToCultureInvariantString() + " Day=" + _day + " TotalInWave=" + totalSpawnedThisWave + " CurrentWave=" + (currentWave + 1));
				_world.DebugAddSpawnedEntity(entity2);
				timeDelayBetweenSpawns = entitySpawnerClass.delayBetweenSpawns;
			}
		}
	}
}

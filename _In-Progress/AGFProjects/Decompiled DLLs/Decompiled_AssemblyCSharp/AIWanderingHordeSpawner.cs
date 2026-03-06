using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIWanderingHordeSpawner
{
	public delegate void HordeArrivedDelegate();

	public enum SpawnType
	{
		Bandits,
		Horde
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum ECommand
	{
		PitStop,
		Wander,
		EndPos
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class ZombieCommand
	{
		public ECommand Command;

		public EntityEnemy Enemy;

		public float WanderTime;

		public Vector3 TargetPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cInvestigateTime = 6000;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirector director;

	[PublicizedFrom(EAccessModifier.Private)]
	public HordeArrivedDelegate arrivedCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 pitStopPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 endPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong endTime;

	public SpawnType spawnType;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorGameStagePartySpawner spawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public float spawnDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ZombieCommand> commandList = new List<ZombieCommand>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int bonusLootSpawnCount;

	public AIWanderingHordeSpawner(AIDirector _director, SpawnType _spawnType, HordeArrivedDelegate _arrivedEvent, List<AIDirectorPlayerState> _targets, ulong _endTime, Vector3 _startPos, Vector3 _pitStopPos, Vector3 _endPos)
	{
		director = _director;
		startPos = _startPos;
		pitStopPos = _pitStopPos;
		endPos = _endPos;
		endTime = _endTime;
		arrivedCallback = _arrivedEvent;
		spawnType = _spawnType;
		string gameStageName;
		int mod;
		switch (spawnType)
		{
		case SpawnType.Bandits:
			gameStageName = "WanderingBandits";
			mod = 0;
			break;
		default:
			gameStageName = "WanderingHorde";
			mod = 50;
			break;
		}
		spawner = new AIDirectorGameStagePartySpawner(_director.World, gameStageName);
		for (int i = 0; i < _targets.Count; i++)
		{
			spawner.AddMember(_targets[i].Player);
		}
		spawner.ResetPartyLevel(mod);
		spawner.ClearMembers();
	}

	public bool Update(World world, float _deltaTime)
	{
		if (world.GetPlayers().Count == 0)
		{
			return true;
		}
		if (world.worldTime >= endTime)
		{
			if (arrivedCallback != null)
			{
				arrivedCallback();
			}
			return true;
		}
		bool flag = UpdateSpawn(world, _deltaTime);
		if (flag && commandList.Count == 0)
		{
			if (arrivedCallback != null)
			{
				arrivedCallback();
			}
			return true;
		}
		if (!flag)
		{
			AstarManager.Instance.AddLocationLine(startPos, endPos, 64);
		}
		else
		{
			Vector3 zero = Vector3.zero;
			int num = 0;
			for (int i = 0; i < commandList.Count; i++)
			{
				Entity enemy = commandList[i].Enemy;
				if (!enemy.IsDead())
				{
					zero += enemy.position;
					num++;
				}
			}
			if (num > 0)
			{
				zero *= 1f / (float)num;
				AstarManager.Instance.AddLocation(zero, 64);
			}
		}
		UpdateHorde(_deltaTime);
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UpdateSpawn(World _world, float _deltaTime)
	{
		if (!AIDirector.CanSpawn())
		{
			return true;
		}
		if (!spawner.Tick(_deltaTime))
		{
			return true;
		}
		spawnDelay -= _deltaTime;
		if (spawnDelay >= 0f)
		{
			return false;
		}
		spawnDelay = 1f;
		if (!spawner.canSpawn)
		{
			return false;
		}
		if (!_world.GetMobRandomSpawnPosWithWater(startPos, 1, 6, 15, _checkBedrolls: true, out var _position))
		{
			return false;
		}
		EntityEnemy entityEnemy = (EntityEnemy)EntityFactory.CreateEntity(EntityGroups.GetRandomFromGroup(spawner.spawnGroupName, ref lastClassId), _position);
		_world.SpawnEntityInWorld(entityEnemy);
		entityEnemy.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		entityEnemy.IsHordeZombie = true;
		entityEnemy.bIsChunkObserver = true;
		entityEnemy.IsHordeZombie = true;
		entityEnemy.bIsChunkObserver = true;
		if (++bonusLootSpawnCount >= GameStageDefinition.LootWanderingBonusEvery)
		{
			bonusLootSpawnCount = 0;
			entityEnemy.lootDropProb *= GameStageDefinition.LootWanderingBonusScale;
		}
		ZombieCommand zombieCommand = new ZombieCommand();
		zombieCommand.Enemy = entityEnemy;
		zombieCommand.TargetPos = RandomPos(director, endPos, 6f);
		zombieCommand.Command = ECommand.EndPos;
		commandList.Add(zombieCommand);
		entityEnemy.SetInvestigatePosition(zombieCommand.TargetPos, 6000, isAlert: false);
		AIDirector.LogAI("Spawned wandering horde (group {0}, zombie {1})", spawner.spawnGroupName, entityEnemy);
		spawner.IncSpawnCount();
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHorde(float dt)
	{
		int num = 0;
		while (num < commandList.Count)
		{
			ZombieCommand zombieCommand = commandList[num];
			bool flag = zombieCommand.Enemy.IsDead() || zombieCommand.Enemy.GetAttackTarget() != null;
			if (!flag)
			{
				if (zombieCommand.Command == ECommand.PitStop || zombieCommand.Command == ECommand.EndPos)
				{
					if (zombieCommand.Enemy.HasInvestigatePosition)
					{
						if (zombieCommand.Enemy.InvestigatePosition != zombieCommand.TargetPos)
						{
							flag = true;
							AIDirector.LogAIExtra("Wandering horde zombie '" + zombieCommand.Enemy?.ToString() + "' removed from horde control. Was killed or investigating");
						}
						else
						{
							zombieCommand.Enemy.SetInvestigatePosition(zombieCommand.TargetPos, 6000, isAlert: false);
						}
					}
					else if (zombieCommand.Command == ECommand.PitStop)
					{
						AIDirector.LogAIExtra("Wandering horde zombie '" + zombieCommand.Enemy?.ToString() + "' reached pitstop. Wander around for awhile");
						zombieCommand.WanderTime = 90f + director.random.RandomFloat * 4f;
						zombieCommand.Command = ECommand.Wander;
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					zombieCommand.WanderTime -= dt;
					zombieCommand.Enemy.ResetDespawnTime();
					if (zombieCommand.WanderTime <= 0f && zombieCommand.Enemy.GetAttackTarget() == null)
					{
						AIDirector.LogAIExtra("Wandering horde zombie '" + zombieCommand.Enemy?.ToString() + "' wandered long enough. Going to endstop");
						zombieCommand.Command = ECommand.EndPos;
						zombieCommand.TargetPos = RandomPos(director, endPos, 6f);
						zombieCommand.Enemy.SetInvestigatePosition(zombieCommand.TargetPos, 6000, isAlert: false);
						zombieCommand.Enemy.IsHordeZombie = false;
					}
				}
			}
			if (flag)
			{
				AIDirector.LogAIExtra("Wandering horde zombie '" + zombieCommand.Enemy?.ToString() + "' removed from control");
				zombieCommand.Enemy.IsHordeZombie = false;
				zombieCommand.Enemy.bIsChunkObserver = false;
				commandList.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public void Cleanup()
	{
		for (int i = 0; i < commandList.Count; i++)
		{
			ZombieCommand zombieCommand = commandList[i];
			zombieCommand.Enemy.IsHordeZombie = false;
			zombieCommand.Enemy.bIsChunkObserver = false;
		}
	}

	public static Vector3 RandomPos(AIDirector director, Vector3 target, float radius)
	{
		Vector2 vector = director.random.RandomOnUnitCircle * radius;
		return target + new Vector3(vector.x, 0f, vector.y);
	}
}

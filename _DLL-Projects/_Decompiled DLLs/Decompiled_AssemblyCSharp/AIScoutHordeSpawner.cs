using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIScoutHordeSpawner
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class ZombieCommand
	{
		public EntityEnemy Zombie;

		public ulong WorldExpiryTime;

		public Vector3 TargetPos;

		public bool Wandering;

		public bool Attacking;

		public float AttackDelay;

		public IHorde Horde;
	}

	public interface IHorde
	{
		bool canSpawnMore { get; }

		bool isSpawning { get; }

		void SpawnMore(int size);

		void SetSpawnPos(Vector3 pos);

		void Destroy();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntitySpawner spawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 endPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBloodMoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> spawnedList = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ZombieCommand> hordeList = new List<ZombieCommand>();

	public AIScoutHordeSpawner(EntitySpawner _spawner, Vector3 _startPos, Vector3 _endPos, bool _isBloodMoon)
	{
		spawner = _spawner;
		startPos = _startPos;
		endPos = _endPos;
		isBloodMoon = _isBloodMoon;
	}

	public bool Update(World world, float dt)
	{
		if (world.GetPlayers().Count == 0)
		{
			return true;
		}
		if (SpawnUpdate(world) && hordeList.Count == 0)
		{
			return true;
		}
		UpdateHorde(world, dt);
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SpawnUpdate(World world)
	{
		if (!AIDirector.CanSpawn() || spawner.CurrentWave > 0)
		{
			return true;
		}
		spawner.SpawnManually(world, GameUtils.WorldTimeToDays(world.worldTime), _bSpawnEnemyEntities: true, [PublicizedFrom(EAccessModifier.Internal)] (EntitySpawner _es, out EntityPlayer _outPlayerToAttack) =>
		{
			_outPlayerToAttack = null;
			return true;
		}, [PublicizedFrom(EAccessModifier.Internal)] (EntitySpawner _es, EntityPlayer _inPlayerToAttack, out EntityPlayer _outPlayerToAttack, out Vector3 _pos) =>
		{
			_outPlayerToAttack = null;
			return world.GetMobRandomSpawnPosWithWater(startPos, 0, 8, 10, _checkBedrolls: true, out _pos);
		}, null, spawnedList);
		for (int num = 0; num < spawnedList.Count; num++)
		{
			EntityEnemy entityEnemy = spawnedList[num] as EntityEnemy;
			if (entityEnemy != null)
			{
				entityEnemy.IsHordeZombie = true;
				entityEnemy.IsScoutZombie = true;
				entityEnemy.IsBloodMoon = isBloodMoon;
				entityEnemy.bIsChunkObserver = true;
				ZombieCommand zombieCommand = new ZombieCommand();
				zombieCommand.Zombie = entityEnemy;
				zombieCommand.TargetPos = CalcRandomPos(world.aiDirector, endPos, 6f);
				zombieCommand.Wandering = false;
				zombieCommand.AttackDelay = 2f;
				entityEnemy.SetInvestigatePosition(zombieCommand.TargetPos, 6000);
				hordeList.Add(zombieCommand);
				AIDirector.LogAI("scout horde spawned '" + entityEnemy?.ToString() + "'. Moving to point of interest");
			}
		}
		spawnedList.Clear();
		return spawner.CurrentWave > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHorde(World world, float deltaTime)
	{
		int num = 0;
		while (num < hordeList.Count)
		{
			bool flag = false;
			ZombieCommand zombieCommand = hordeList[num];
			EntityEnemy zombie = zombieCommand.Zombie;
			if (zombie.IsDead())
			{
				flag = true;
			}
			else
			{
				EntityAlive attackTarget = zombie.GetAttackTarget();
				bool flag2 = attackTarget is EntityPlayer;
				if (zombieCommand.Horde != null)
				{
					if ((bool)attackTarget && !attackTarget.IsDead() && flag2)
					{
						zombieCommand.Horde.SetSpawnPos(attackTarget.GetPosition());
					}
					else
					{
						zombieCommand.Horde.SetSpawnPos(zombie.GetPosition());
					}
				}
				if (zombieCommand.Attacking)
				{
					if (!zombieCommand.Zombie.HasInvestigatePosition && (attackTarget == null || attackTarget.IsDead() || !flag2))
					{
						zombieCommand.Wandering = true;
						zombieCommand.WorldExpiryTime = world.worldTime + 2000;
					}
					else
					{
						zombieCommand.AttackDelay -= deltaTime;
						if (zombieCommand.AttackDelay <= 0f && zombie.bodyDamage.CurrentStun == EnumEntityStunType.None)
						{
							if (zombie.HasInvestigatePosition || (flag2 && !attackTarget.IsDead()))
							{
								Vector3 target = (attackTarget ? attackTarget.GetPosition() : zombieCommand.Zombie.InvestigatePosition);
								if (spawnHordeNear(world, zombieCommand, target))
								{
									zombieCommand.AttackDelay = 18f;
								}
								else
								{
									flag = true;
								}
							}
							else
							{
								zombieCommand.Wandering = true;
								zombieCommand.WorldExpiryTime = world.worldTime + 2000;
							}
						}
					}
				}
				else if ((bool)attackTarget)
				{
					if (flag2)
					{
						zombieCommand.Attacking = true;
					}
				}
				else if (zombieCommand.Wandering)
				{
					if (world.worldTime >= zombieCommand.WorldExpiryTime)
					{
						flag = true;
					}
				}
				else if (zombieCommand.Zombie.HasInvestigatePosition)
				{
					if (zombieCommand.Zombie.InvestigatePosition == zombieCommand.TargetPos)
					{
						zombieCommand.Zombie.SetInvestigatePosition(zombieCommand.TargetPos, 6000);
					}
				}
				else
				{
					zombieCommand.Wandering = true;
					zombieCommand.WorldExpiryTime = world.worldTime + 2000;
				}
			}
			if (flag)
			{
				if (zombieCommand.Horde != null)
				{
					zombieCommand.Horde.Destroy();
				}
				AIDirector.LogAIExtra("scout horde '" + zombieCommand.Zombie?.ToString() + "' removed from control");
				zombieCommand.Zombie.IsHordeZombie = false;
				zombieCommand.Zombie.bIsChunkObserver = false;
				hordeList.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public void Cleanup()
	{
		for (int i = 0; i < hordeList.Count; i++)
		{
			ZombieCommand zombieCommand = hordeList[i];
			zombieCommand.Zombie.IsHordeZombie = false;
			zombieCommand.Zombie.bIsChunkObserver = false;
		}
		hordeList.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool spawnHordeNear(World world, ZombieCommand command, Vector3 target)
	{
		AIDirector.LogAI("Scout spawned a zombie horde");
		if (command.Horde == null)
		{
			AIDirectorChunkEventComponent component = world.GetAIDirector().GetComponent<AIDirectorChunkEventComponent>();
			command.Horde = component.CreateHorde(target);
		}
		if (command.Horde.canSpawnMore)
		{
			int num = 5;
			if (world.aiDirector.random.RandomFloat < 0.12f)
			{
				num--;
				if (spawner.CurrentWave > 0)
				{
					Vector3 vector = endPos;
					spawner.ResetSpawner();
					spawner.numberToSpawnThisWave = 1;
					endPos = target;
					SpawnUpdate(world);
					endPos = vector;
				}
				else
				{
					spawner.numberToSpawnThisWave++;
				}
			}
			command.Horde.SpawnMore(num);
			command.Zombie.PlayOneShot(command.Zombie.GetSoundAlert());
		}
		command.Horde.SetSpawnPos(target);
		if (!command.Horde.canSpawnMore)
		{
			return command.Horde.isSpawning;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 CalcRandomPos(AIDirector director, Vector3 target, float radius)
	{
		Vector2 vector = director.random.RandomOnUnitCircle * radius;
		return target + new Vector3(vector.x, 0f, vector.y);
	}
}

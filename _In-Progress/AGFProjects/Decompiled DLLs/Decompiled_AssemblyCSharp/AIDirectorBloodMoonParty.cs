using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorBloodMoonParty
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class ManagedZombie
	{
		public EntityPlayer player;

		public EntityEnemy zombie;

		public float updateDelay;

		public ManagedZombie(EntityEnemy _zombie, EntityPlayer _player)
		{
			zombie = _zombie;
			player = _player;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPartyJoinDistance = 80f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPartyJoinDistanceSq = 6400f;

	public const float cSightDist = 100f;

	public const float cSightDistSq = 10000f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTeleportDist = 150f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTeleportDistSq = 22500f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnPreferredArc = 120;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSpawnAngle = 90f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSpawnDistance = 40f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnMinRandDistance = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnMaxRandDistance = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnMinPlayerDistance = 30;

	public AIDirectorGameStagePartySpawner partySpawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ManagedZombie> zombies = new List<ManagedZombie>();

	[PublicizedFrom(EAccessModifier.Private)]
	public World spawnWorld;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 spawnBasePos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int spawnBaseDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public int enemyActiveMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorBloodMoonComponent controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 spawnDirectionV;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int groupIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bonusLootSpawnCount;

	public bool IsEmpty => partySpawner.partyMembers.Count <= 0;

	public bool BloodmoonZombiesRemain => zombies.Count > 0;

	public AIDirectorBloodMoonParty(EntityPlayer _initialPlayer, AIDirectorBloodMoonComponent _controller, int _bloodMoonCountUNUSED)
	{
		spawnWorld = _initialPlayer.world;
		spawnBasePos = _initialPlayer.position;
		controller = _controller;
		partySpawner = new AIDirectorGameStagePartySpawner(_controller.Director.World, "BloodMoonHorde");
		partySpawner.AddMember(_initialPlayer);
		_initialPlayer.bloodMoonParty = this;
		spawnBaseDir = _controller.Random.RandomRange(0, 360);
		groupIndex = -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcBestDir(Vector3 basePos)
	{
		int[] array = new int[16];
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			float num2 = (float)i * 22.5f;
			spawnDirectionV = Quaternion.AngleAxis(num2, Vector3.up) * Vector3.forward * 40f;
			int num3 = 0;
			for (int j = 0; j < 9; j++)
			{
				if (spawnWorld.GetRandomSpawnPositionMinMaxToPosition(basePos + spawnDirectionV, 0, 10, 30, _checkBedrolls: false, out var _, -1, _checkWater: true, 30))
				{
					num3++;
				}
			}
			if (num3 > 0)
			{
				num3 = (num3 + 2) / 3;
				if (Utils.FastAbs(Mathf.DeltaAngle(num2, spawnBaseDir)) <= 60f)
				{
					num3 *= 3;
				}
			}
			array[i] = num3;
			num = Utils.FastMax(num, num3);
		}
		int num4 = 0;
		for (int k = 0; k < 16; k++)
		{
			if (array[k] == num)
			{
				num4++;
			}
		}
		int num5 = 0;
		int num6 = controller.Random.RandomRange(0, num4);
		for (int l = 0; l < 16; l++)
		{
			if (array[l] >= num && --num6 < 0)
			{
				num5 = l;
				break;
			}
		}
		spawnDirectionV = Quaternion.AngleAxis((float)num5 * 22.5f, Vector3.up) * Vector3.forward * 40f;
	}

	public bool Tick(World _world, double _dt, bool _canSpawn)
	{
		if (partySpawner.partyLevel < 0)
		{
			InitParty();
		}
		for (int num = zombies.Count - 1; num >= 0; num--)
		{
			ManagedZombie managedZombie = zombies[num];
			managedZombie.updateDelay -= (float)_dt;
			if (managedZombie.updateDelay <= 0f)
			{
				managedZombie.updateDelay = 1.8f;
				if (!SeekTarget(managedZombie))
				{
					zombies.RemoveAt(num);
				}
			}
		}
		partySpawner.Tick(_dt);
		bool result = false;
		if (_canSpawn)
		{
			if (!partySpawner.canSpawn || partySpawner.partyMembers.Count == 0)
			{
				return true;
			}
			if (AIDirector.CanSpawn(1.9f))
			{
				int num2 = partySpawner.groupIndex;
				if (num2 != groupIndex)
				{
					groupIndex = num2;
					spawnBaseDir += 120;
					CalcBestDir(spawnBasePos);
				}
				result = true;
				int count = partySpawner.partyMembers.Count;
				int num3 = Utils.FastMin(partySpawner.maxAlive, enemyActiveMax);
				if (zombies.Count < num3)
				{
					for (int num4 = Utils.FastMin(count, 3); num4 > 0; num4--)
					{
						if (nextPlayer >= count)
						{
							nextPlayer = 0;
						}
						EntityPlayer entityPlayer = partySpawner.partyMembers[nextPlayer];
						bool flag = false;
						if (IsPlayerATarget(entityPlayer))
						{
							flag = SpawnZombie(_world, entityPlayer, entityPlayer.position, spawnDirectionV);
						}
						nextPlayer++;
						if (flag)
						{
							break;
						}
					}
				}
			}
		}
		return result;
	}

	public void PlayerLoggedOut(EntityPlayer _player)
	{
		partySpawner.RemoveMember(_player, removeID: false);
		if (nextPlayer >= partySpawner.partyMembers.Count)
		{
			nextPlayer = 0;
		}
	}

	public void KillPartyZombies()
	{
		int count = zombies.Count;
		if (count <= 0)
		{
			return;
		}
		partySpawner.DecSpawnCount(count);
		for (int i = 0; i < count; i++)
		{
			EntityEnemy zombie = zombies[i].zombie;
			if ((bool)zombie && !zombie.IsDead() && !zombie.IsDespawned && (bool)zombie.gameObject)
			{
				zombie.Kill(DamageResponse.New(_fatal: true));
			}
		}
		zombies.Clear();
	}

	public bool IsMemberOfParty(int _entityID)
	{
		return partySpawner.IsMemberOfParty(_entityID);
	}

	public bool TryAddPlayer(EntityPlayer _player)
	{
		for (int i = 0; i < partySpawner.partyMembers.Count; i++)
		{
			if ((partySpawner.partyMembers[i].GetPosition() - _player.GetPosition()).sqrMagnitude <= 6400f)
			{
				AddPlayer(_player);
				return true;
			}
		}
		return false;
	}

	public void AddPlayer(EntityPlayer _player)
	{
		partySpawner.AddMember(_player);
		_player.bloodMoonParty = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitParty()
	{
		int num = partySpawner.CalcPartyLevel();
		int num2 = GameStats.GetInt(EnumGameStats.BloodMoonEnemyCount) * partySpawner.partyMembers.Count;
		enemyActiveMax = Utils.FastMin(30, num2);
		float b = Utils.FastMax(1f, (float)num2 / (float)enemyActiveMax);
		b = Utils.FastLerp(1f, b, (float)num / 60f);
		partySpawner.SetScaling(b);
		partySpawner.SetPartyLevel(num);
		bonusLootSpawnCount = partySpawner.bonusLootEvery / 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SpawnZombie(World _world, EntityPlayer _target, Vector3 _focusPos, Vector3 _radiusV)
	{
		if (!CalcSpawnPos(_world, _focusPos, _radiusV, out var spawnPos))
		{
			return false;
		}
		bool flag = true;
		int et = EntityGroups.GetRandomFromGroup(partySpawner.spawnGroupName, ref lastClassId);
		if ((bool)_target.AttachedToEntity && controller.Random.RandomFloat < 0.5f)
		{
			flag = false;
			et = EntityClass.FromString("animalZombieVultureRadiated");
		}
		EntityEnemy entityEnemy = (EntityEnemy)EntityFactory.CreateEntity(et, spawnPos);
		_world.SpawnEntityInWorld(entityEnemy);
		entityEnemy.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		entityEnemy.IsHordeZombie = true;
		entityEnemy.IsBloodMoon = true;
		entityEnemy.bIsChunkObserver = true;
		entityEnemy.timeStayAfterDeath /= 3;
		if (flag && ++bonusLootSpawnCount >= partySpawner.bonusLootEvery)
		{
			bonusLootSpawnCount = 0;
			entityEnemy.lootDropProb *= GameStageDefinition.LootBonusScale;
		}
		ManagedZombie managedZombie = new ManagedZombie(entityEnemy, _target);
		zombies.Add(managedZombie);
		SeekTarget(managedZombie);
		partySpawner.IncSpawnCount();
		AstarManager.Instance.AddLocation(spawnPos, 40);
		var (num, num2, num3) = GameUtils.WorldTimeToElements(_world.worldTime);
		Log.Out("BloodMoonParty: SpawnZombie grp {0}, cnt {1}, {2}, loot {3}, at player {4}, day/time {5} {6:D2}:{7:D2}", partySpawner.ToString(), zombies.Count, entityEnemy.EntityName, entityEnemy.lootDropProb, _target.entityId, num, num2, num3);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CalcSpawnPos(World _world, Vector3 _focusPos, Vector3 _radiusV, out Vector3 spawnPos)
	{
		_radiusV = Quaternion.AngleAxis((controller.Random.RandomFloat - 0.5f) * 90f, Vector3.up) * _radiusV;
		if (!_world.GetMobRandomSpawnPosWithWater(_focusPos + _radiusV, 0, 10, 30, _checkBedrolls: false, out spawnPos))
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer FindPartyTarget(Vector3 fromPos)
	{
		float num = float.MaxValue;
		EntityPlayer result = null;
		for (int num2 = partySpawner.partyMembers.Count - 1; num2 >= 0; num2--)
		{
			EntityPlayer entityPlayer = partySpawner.partyMembers[num2];
			if (IsPlayerATarget(entityPlayer))
			{
				float sqrMagnitude = (fromPos - entityPlayer.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = entityPlayer;
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SeekTarget(ManagedZombie mz)
	{
		EntityAlive zombie = mz.zombie;
		if (!zombie || zombie.IsDead() || zombie.IsDespawned || !zombie.gameObject)
		{
			return false;
		}
		EntityPlayer entityPlayer = zombie.GetAttackTarget() as EntityPlayer;
		if ((bool)entityPlayer)
		{
			mz.player = entityPlayer;
		}
		if (!mz.player || !IsPlayerATarget(mz.player))
		{
			mz.player = FindPartyTarget(zombie.position);
		}
		if (!mz.player)
		{
			if (!zombie.world.IsPlayerAliveAndNear(zombie.position, 60f))
			{
				zombie.Kill(DamageResponse.New(_fatal: true));
				return false;
			}
			return true;
		}
		Vector3 vector = zombie.position - mz.player.position;
		float sqrMagnitude = vector.sqrMagnitude;
		vector.y = 0f;
		if (vector.sqrMagnitude >= 22500f && CalcSpawnPos(zombie.world, mz.player.position, spawnDirectionV, out var spawnPos) && !zombie.world.IsPlayerAliveAndNear(zombie.position, 70f))
		{
			if (controller.Random.RandomFloat < 0.5f)
			{
				partySpawner.DecSpawnCount(1);
				zombie.lootDropProb = 0f;
				zombie.Kill(DamageResponse.New(_fatal: true));
				return false;
			}
			zombie.SetPosition(spawnPos);
			zombie.moveHelper.Stop();
			Log.Warning("SeekTarget {0}, far, move {1}", zombie.GetDebugName(), spawnPos);
		}
		if (sqrMagnitude <= 10000f || entityPlayer != mz.player)
		{
			zombie.SetAttackTarget(mz.player, 1200);
		}
		else
		{
			if ((bool)entityPlayer)
			{
				zombie.SetAttackTarget(null, 0);
			}
			zombie.SetInvestigatePosition(mz.player.position, 1200);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPlayerATarget(EntityPlayer player)
	{
		if (player.IsDead() || !player.IsSpawned() || player.entityId == -1)
		{
			return false;
		}
		if (player.IsIgnoredByAI())
		{
			return false;
		}
		if (player.Progression.Level <= 1 || player.IsBloodMoonDead)
		{
			return false;
		}
		return true;
	}
}

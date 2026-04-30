using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIHordeSpawner
{
	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorGameStagePartySpawner spawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityEnemy> hordeList = new List<EntityEnemy>();

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public float playerSearchBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInited;

	public int numToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numSpawned;

	public bool isSpawning => spawner.canSpawn;

	public AIHordeSpawner(World _world, string _spawnerDefinition, Vector3 _targetPos, float _playerSearchBounds)
	{
		world = _world;
		spawner = new AIDirectorGameStagePartySpawner(_world, _spawnerDefinition);
		playerSearchBounds = _playerSearchBounds;
		targetPos = _targetPos;
	}

	public bool Tick(double _dt)
	{
		if (world.GetPlayers().Count == 0 || !AIDirector.CanSpawn())
		{
			return true;
		}
		if (!isInited)
		{
			List<Entity> entitiesInBounds = world.GetEntitiesInBounds(typeof(EntityPlayer), BoundsUtils.BoundsForMinMax(targetPos.x - playerSearchBounds, targetPos.y - playerSearchBounds, targetPos.z - playerSearchBounds, targetPos.x + playerSearchBounds, targetPos.y + playerSearchBounds, targetPos.z + playerSearchBounds), new List<Entity>());
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				EntityPlayer entityPlayer = (EntityPlayer)entitiesInBounds[i];
				if (!entityPlayer.IsIgnoredByAI())
				{
					spawner.AddMember(entityPlayer);
				}
			}
			if (spawner.partyMembers.Count == 0)
			{
				return false;
			}
			isInited = true;
			spawner.ResetPartyLevel();
			spawner.ClearMembers();
		}
		if (!spawner.Tick(_dt))
		{
			return true;
		}
		if (!spawner.canSpawn || numSpawned >= numToSpawn)
		{
			return false;
		}
		Vector3 _position;
		if (world.IsDaytime())
		{
			if (!world.GetMobRandomSpawnPosWithWater(targetPos, 45, 55, 45, _checkBedrolls: true, out _position))
			{
				return false;
			}
		}
		else if (!world.GetMobRandomSpawnPosWithWater(targetPos, 55, 70, 55, _checkBedrolls: true, out _position))
		{
			return false;
		}
		EntityEnemy entityEnemy = (EntityEnemy)EntityFactory.CreateEntity(EntityGroups.GetRandomFromGroup(spawner.spawnGroupName, ref lastClassId), _position);
		Log.Out("Screamer spawned {0} from {1}", entityEnemy.EntityName, spawner.spawnGroupName);
		world.SpawnEntityInWorld(entityEnemy);
		entityEnemy.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		entityEnemy.IsHordeZombie = true;
		entityEnemy.bIsChunkObserver = true;
		entityEnemy.SetInvestigatePosition(AIWanderingHordeSpawner.RandomPos(world.aiDirector, targetPos, 3f), 2400);
		hordeList.Add(entityEnemy);
		spawner.IncSpawnCount();
		numSpawned++;
		return false;
	}

	public void Cleanup()
	{
		for (int i = 0; i < hordeList.Count; i++)
		{
			EntityEnemy entityEnemy = hordeList[i];
			entityEnemy.IsHordeZombie = false;
			entityEnemy.bIsChunkObserver = false;
		}
		hordeList.Clear();
	}
}

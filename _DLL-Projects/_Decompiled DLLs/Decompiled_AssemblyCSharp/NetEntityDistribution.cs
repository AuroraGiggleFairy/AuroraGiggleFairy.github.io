using System;
using System.Collections.Generic;
using UnityEngine;

public class NetEntityDistribution
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct SEnts(Type _eType, int _distance, int _update, bool _motion)
	{
		public Type eType = _eType;

		public int distance = _distance;

		public int update = _update;

		public bool motion = _motion;
	}

	public const float cHighPriorityRange = 5f;

	public const float cLowPriorityRange = 18f;

	public const float cLowestPriorityRange = 25f;

	public const int MobsUpdateTicks = 3;

	public const int lowPriorityTick = 6;

	public const int lowestPriorityTick = 10;

	public static float priorityViewAngleLimit = 60f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float priorityViewAngleMinDistance = 128f;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetList<NetEntityDistributionEntry> trackedEntitySet;

	[PublicizedFrom(EAccessModifier.Private)]
	public IntHashMap trackedEntityHashTable;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SEnts> config = new List<SEnts>
	{
		new SEnts(typeof(EntityPlayer), int.MaxValue, 3, _motion: false),
		new SEnts(typeof(EntityVehicle), int.MaxValue, 3, _motion: false),
		new SEnts(typeof(EntityEnemy), 80, 3, _motion: false),
		new SEnts(typeof(EntityNPC), 80, 3, _motion: false),
		new SEnts(typeof(EntityItem), 64, 3, _motion: false),
		new SEnts(typeof(EntityFallingBlock), 120, 3, _motion: false),
		new SEnts(typeof(EntityFallingTree), 120, 1, _motion: false),
		new SEnts(typeof(EntityAnimalStag), 80, 3, _motion: false),
		new SEnts(typeof(EntityAnimalRabbit), 64, 3, _motion: false),
		new SEnts(typeof(EntityCar), 100, 3, _motion: false),
		new SEnts(typeof(EntitySupplyCrate), 1200, 3, _motion: false),
		new SEnts(typeof(EntitySupplyPlane), 1200, 3, _motion: true),
		new SEnts(typeof(EntityTurret), 60, 3, _motion: false),
		new SEnts(typeof(EntityHomerunGoal), 80, 3, _motion: false)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> playerList = new List<EntityPlayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityEnemy> enemyList = new List<EntityEnemy>();

	public NetEntityDistribution(World _world, int _v)
	{
		trackedEntitySet = new HashSetList<NetEntityDistributionEntry>();
		trackedEntityHashTable = new IntHashMap();
		world = _world;
	}

	public void OnUpdateEntities()
	{
		playerList.Clear();
		enemyList.Clear();
		for (int i = 0; i < trackedEntitySet.list.Count; i++)
		{
			NetEntityDistributionEntry netEntityDistributionEntry = trackedEntitySet.list[i];
			if (netEntityDistributionEntry.trackedEntity is EntityEnemy item)
			{
				enemyList.Add(item);
			}
			else if (netEntityDistributionEntry.trackedEntity is EntityPlayer item2)
			{
				playerList.Add(item2);
			}
		}
		foreach (EntityEnemy enemy in enemyList)
		{
			NetEntityDistributionEntry netEntityDistributionEntry2 = (NetEntityDistributionEntry)trackedEntityHashTable.lookup(enemy.entityId);
			bool flag = enemy.IsAirBorne();
			netEntityDistributionEntry2.priorityLevel = 1;
			if (!GameManager.enableNetworkdPrioritization)
			{
				continue;
			}
			float num = float.MaxValue;
			bool flag2 = false;
			Vector3 position = enemy.transform.position;
			position.y = 0f;
			for (int j = 0; j < world.Players.Count; j++)
			{
				Transform transform = world.Players.list[j].transform;
				Vector3 position2 = transform.position;
				position2.y = 0f;
				Vector3 vector = position - position2;
				float num2 = vector.x * vector.x + vector.z + vector.z;
				if (!flag2 && num2 < 16384f && Vector3.Angle(transform.forward, vector.normalized) < priorityViewAngleLimit)
				{
					flag2 = true;
				}
				if (num2 < num)
				{
					num = num2;
				}
			}
			if (num < 25f)
			{
				netEntityDistributionEntry2.priorityLevel = 0;
			}
			else if (!flag2 && !flag)
			{
				if (num > 625f)
				{
					netEntityDistributionEntry2.priorityLevel = 3;
				}
				else if (num > 324f)
				{
					netEntityDistributionEntry2.priorityLevel = 2;
				}
			}
		}
		if (playerList.Count > 1)
		{
			foreach (EntityPlayer player in playerList)
			{
				NetEntityDistributionEntry netEntityDistributionEntry3 = (NetEntityDistributionEntry)trackedEntityHashTable.lookup(player.entityId);
				netEntityDistributionEntry3.priorityLevel = 1;
				if (!GameManager.enableNetworkdPrioritization)
				{
					continue;
				}
				Vector3 position3 = player.transform.position;
				foreach (EntityPlayer player2 in playerList)
				{
					if (!(player2 == player))
					{
						Vector3 vector2 = position3 - player2.transform.position;
						if (vector2.x * vector2.x + vector2.z * vector2.z < 25f)
						{
							netEntityDistributionEntry3.priorityLevel = 0;
							break;
						}
					}
				}
			}
		}
		for (int k = 0; k < trackedEntitySet.list.Count; k++)
		{
			trackedEntitySet.list[k].updatePlayerList(world.Players.list);
		}
		for (int l = 0; l < playerList.Count; l++)
		{
			EntityPlayer entityPlayer = playerList[l];
			for (int m = 0; m < trackedEntitySet.list.Count; m++)
			{
				NetEntityDistributionEntry netEntityDistributionEntry4 = trackedEntitySet.list[m];
				if (netEntityDistributionEntry4.trackedEntity != entityPlayer)
				{
					netEntityDistributionEntry4.updatePlayerEntity(entityPlayer);
				}
			}
		}
	}

	public void SendPacketToTrackedPlayers(int _entityId, int _excludePlayer, NetPackage _package, bool _inRangeOnly = false)
	{
		((NetEntityDistributionEntry)trackedEntityHashTable.lookup(_entityId))?.SendToPlayers(_package, _excludePlayer, _inRangeOnly);
	}

	public void SendPacketToTrackedPlayersAndTrackedEntity(int _entityId, int _excludePlayer, NetPackage _package, bool _inRangeOnly = false)
	{
		((NetEntityDistributionEntry)trackedEntityHashTable.lookup(_entityId))?.sendPacketToTrackedPlayersAndTrackedEntity(_package, _excludePlayer, _inRangeOnly);
	}

	public void Add(Entity _e)
	{
		for (int i = 0; i < config.Count; i++)
		{
			SEnts sEnts = config[i];
			if (sEnts.eType.IsAssignableFrom(_e.GetType()))
			{
				Add(_e, sEnts.distance, sEnts.update, sEnts.motion);
			}
			if (!(_e is EntityPlayer))
			{
				continue;
			}
			EntityPlayer entityPlayer = (EntityPlayer)_e;
			for (int j = 0; j < trackedEntitySet.list.Count; j++)
			{
				NetEntityDistributionEntry netEntityDistributionEntry = trackedEntitySet.list[j];
				if (netEntityDistributionEntry.trackedEntity != entityPlayer)
				{
					netEntityDistributionEntry.updatePlayerEntity(entityPlayer);
				}
			}
		}
	}

	public void Add(Entity _e, int _d, int _t)
	{
		Add(_e, _d, _t, _upd: false);
	}

	public void Add(Entity _e, int _distance, int _t, bool _upd)
	{
		if (!trackedEntityHashTable.containsItem(_e.entityId))
		{
			NetEntityDistributionEntry netEntityDistributionEntry = new NetEntityDistributionEntry(_e, _distance, _t, _upd);
			trackedEntitySet.Add(netEntityDistributionEntry);
			trackedEntityHashTable.addKey(_e.entityId, netEntityDistributionEntry);
			netEntityDistributionEntry.updatePlayerEntities(world.Players.list);
		}
	}

	public void Remove(Entity _e, EnumRemoveEntityReason _reason)
	{
		if (_e is EntityPlayer)
		{
			EntityPlayer e = (EntityPlayer)_e;
			for (int i = 0; i < trackedEntitySet.list.Count; i++)
			{
				trackedEntitySet.list[i].Remove(e);
			}
		}
		NetEntityDistributionEntry netEntityDistributionEntry = (NetEntityDistributionEntry)trackedEntityHashTable.removeObject(_e.entityId);
		if (netEntityDistributionEntry != null)
		{
			trackedEntitySet.Remove(netEntityDistributionEntry);
			if (_reason == EnumRemoveEntityReason.Unloaded)
			{
				netEntityDistributionEntry.SendUnloadEntityToPlayers();
			}
			else
			{
				netEntityDistributionEntry.SendDestroyEntityToPlayers();
			}
		}
	}

	public void SyncEntity(Entity _e, Vector3 _pos, Vector3 _rot)
	{
		NetEntityDistributionEntry netEntityDistributionEntry = (NetEntityDistributionEntry)trackedEntityHashTable.lookup(_e.entityId);
		if (netEntityDistributionEntry != null)
		{
			netEntityDistributionEntry.encodedPos = NetEntityDistributionEntry.EncodePos(_pos);
			netEntityDistributionEntry.encodedRot = NetEntityDistributionEntry.EncodePos(_rot);
		}
	}

	public void SendFullUpdateNextTick(Entity _e)
	{
		((NetEntityDistributionEntry)trackedEntityHashTable.lookup(_e.entityId))?.SendFullUpdateNextTick();
	}

	public void Cleanup()
	{
		trackedEntitySet.Clear();
		trackedEntityHashTable.clearMap();
	}

	public NetEntityDistributionEntry FindEntry(Entity entity)
	{
		return trackedEntityHashTable.lookup(entity.entityId) as NetEntityDistributionEntry;
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class DroneManager
{
	public static bool Debug_LocalControl;

	public static bool DebugLogEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSaveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cChangeSaveDelay = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxDrones = 500;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int serverDroneCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityDrone> dronesActive = new List<EntityDrone>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityCreationData> dronesUnloaded = new List<EntityCreationData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityCreationData> dronesWithoutOwner = new List<EntityCreationData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float saveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DroneManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo saveThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> debugDronePlayerAssignment = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cNameKey = "drones";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cThreadKey = "droneDataSave";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cStringName = "DroneManager";

	public static DroneManager Instance => instance;

	public static void Init()
	{
		instance = new DroneManager();
		instance.Load();
	}

	public void AddTrackedDrone(EntityDrone _drone)
	{
		if (!_drone)
		{
			Log.Error("{0} AddTrackedDrone null", GetType());
			return;
		}
		_drone.OnWakeUp();
		if (!dronesActive.Contains(_drone))
		{
			dronesActive.Add(_drone);
			TriggerSave();
		}
	}

	public void RemoveTrackedDrone(EntityDrone _drone, EnumRemoveEntityReason _reason)
	{
		dronesActive.Remove(_drone);
		if (_reason == EnumRemoveEntityReason.Unloaded)
		{
			EntityAlive owner = _drone.Owner;
			if ((bool)owner)
			{
				OwnedEntityData ownedEntity = owner.GetOwnedEntity(_drone.entityId);
				if (ownedEntity != null)
				{
					ownedEntity.SetLastKnownPosition(_drone.position);
					_ = EntityClass.list[ownedEntity.ClassId];
				}
			}
			dronesUnloaded.Add(new EntityCreationData(_drone));
		}
		TriggerSave();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageVehicleCount>().Setup());
	}

	public void TriggerSave()
	{
		saveTime = Mathf.Min(saveTime, 10f);
	}

	public void Update()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (world == null || world.Players == null || world.Players.Count == 0 || !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		int num = 0;
		for (int num2 = dronesUnloaded.Count - 1; num2 >= 0; num2--)
		{
			EntityCreationData entityCreationData = dronesUnloaded[num2];
			EntityDrone entityDrone = world.GetEntity(entityCreationData.id) as EntityDrone;
			if ((bool)entityDrone)
			{
				Log.Warning("{0} already loaded #{1}, id {2}, {3}, {4}", GetType(), num2, entityCreationData.id, entityDrone, entityDrone.position.ToCultureInvariantString());
				dronesUnloaded.RemoveAt(num2);
			}
			else
			{
				if (!world.IsChunkAreaCollidersLoaded(entityCreationData.pos))
				{
					continue;
				}
				if (!isValidDronePos(entityCreationData.pos))
				{
					bool flag = false;
					IDictionary<PlatformUserIdentifierAbs, PersistentPlayerData> players = GameManager.Instance.GetPersistentPlayerList().Players;
					if (players != null)
					{
						foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> item in players)
						{
							EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(item.Value.EntityId) as EntityPlayer;
							if (!entityPlayer)
							{
								continue;
							}
							OwnedEntityData[] ownedEntities = entityPlayer.GetOwnedEntities();
							for (int i = 0; i < ownedEntities.Length; i++)
							{
								if (entityCreationData.id == ownedEntities[i].Id)
								{
									Log.Warning("recovering {0} owned entity for {1}", entityCreationData.id, entityPlayer.entityId);
									entityCreationData.pos = entityPlayer.getHeadPosition();
									entityCreationData.belongsPlayerId = entityPlayer.entityId;
									flag = true;
									break;
								}
							}
						}
					}
					if (!flag)
					{
						entityCreationData.pos = Vector3.zero;
						if (entityCreationData.belongsPlayerId == -1)
						{
							dronesWithoutOwner.Add(entityCreationData);
							dronesUnloaded.RemoveAt(num2);
						}
						continue;
					}
				}
				entityDrone = CreateDroneEntity(entityCreationData, world);
				if ((bool)entityDrone)
				{
					num++;
				}
				else
				{
					Log.Error("DroneManager load failed #{0}, id {1}, {2}", num2, entityCreationData.id, EntityClass.GetEntityClassName(entityCreationData.entityClass));
				}
				dronesUnloaded.RemoveAt(num2);
			}
		}
		_ = 0;
		saveTime -= Time.deltaTime;
		if (saveTime <= 0f && (saveThread == null || saveThread.HasTerminated()))
		{
			saveTime = 120f;
			Save();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone CreateDroneEntity(EntityCreationData data, World world)
	{
		EntityDrone entityDrone = EntityFactory.CreateEntity(data) as EntityDrone;
		if ((bool)entityDrone)
		{
			dronesActive.Add(entityDrone);
			world.SpawnEntityInWorld(entityDrone);
			entityDrone.SyncOwnerData();
		}
		return entityDrone;
	}

	public EntityDrone LoadDrone(int _entityId, World world)
	{
		EntityDrone result = null;
		foreach (EntityCreationData item in dronesUnloaded)
		{
			if (item.id == _entityId)
			{
				result = CreateDroneEntity(item, world);
				break;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isValidDronePos(Vector3 pos)
	{
		if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y))
		{
			return !float.IsNaN(pos.z);
		}
		return false;
	}

	public void RemoveAllDronesFromMap()
	{
		GameManager gameManager = GameManager.Instance;
		PersistentPlayerList persistentPlayerList = gameManager.GetPersistentPlayerList();
		World world = gameManager.World;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in persistentPlayerList.Players)
		{
			EntityPlayer entityPlayer = world.GetEntity(player.Value.EntityId) as EntityPlayer;
			if (!entityPlayer)
			{
				continue;
			}
			OwnedEntityData[] ownedEntities = entityPlayer.GetOwnedEntities();
			int i = ownedEntities.Length - 1;
			while (i >= 0)
			{
				EntityDrone entityDrone = dronesActive.Find([PublicizedFrom(EAccessModifier.Internal)] (EntityDrone v) => v.entityId == ownedEntities[i].Id);
				if ((bool)entityDrone && entityPlayer.HasOwnedEntity(entityDrone.entityId))
				{
					GameManager.Instance.World.RemoveEntityFromMap(entityDrone, EnumRemoveEntityReason.Unloaded);
				}
				int num = i - 1;
				i = num;
			}
		}
		UpdateWaypointsForAllPlayers();
	}

	public void ClearAllDronesForPlayer(EntityPlayer player)
	{
		ClearAllDronesForPlayer(player.entityId);
	}

	public void ClearAllDronesForPlayer(int entityId)
	{
		ClearUnloadedDrones(entityId);
		ClearActiveDrones(entityId);
		TriggerSave();
		UpdateWaypointsForPlayer(entityId);
	}

	public void ClearUnloadedDrones(EntityPlayer player)
	{
		ClearUnloadedDrones(player.entityId);
	}

	public void ClearUnloadedDrones(int entityId)
	{
		for (int num = dronesUnloaded.Count - 1; num >= 0; num--)
		{
			if (dronesUnloaded[num].belongsPlayerId == entityId)
			{
				dronesUnloaded.RemoveAt(num);
			}
		}
	}

	public void ClearActiveDrones(int entityId)
	{
		for (int num = dronesActive.Count - 1; num >= 0; num--)
		{
			EntityDrone entityDrone = dronesActive[num];
			if (entityDrone.belongsPlayerId == entityId)
			{
				dronesActive.RemoveAt(num);
				GameManager.Instance.World.RemoveEntity(entityDrone.entityId, EnumRemoveEntityReason.Killed);
			}
		}
	}

	public string LogActiveDrones()
	{
		string text = string.Empty;
		for (int i = 0; i < dronesActive.Count; i++)
		{
			EntityDrone entityDrone = dronesActive[i];
			text = text + "owner: " + entityDrone.OwnerID?.ToString() + " pid: " + entityDrone.belongsPlayerId + " id: " + entityDrone.EntityId + Environment.NewLine;
		}
		return text;
	}

	public string LogUnloadedDrones()
	{
		string text = string.Empty;
		for (int i = 0; i < dronesUnloaded.Count; i++)
		{
			EntityCreationData entityCreationData = dronesUnloaded[i];
			text = text + "pid: " + entityCreationData.belongsPlayerId + " id: " + entityCreationData.id + Environment.NewLine;
		}
		return text;
	}

	public bool AssignUnloadedDrone(EntityPlayer player, int entityId)
	{
		for (int i = 0; i < dronesUnloaded.Count; i++)
		{
			EntityCreationData entityCreationData = dronesUnloaded[i];
			if (entityCreationData.id == entityId)
			{
				entityCreationData.pos = player.getHeadPosition();
				entityCreationData.belongsPlayerId = player.entityId;
				Log.Warning(entityCreationData.belongsPlayerId.ToString());
				debugDronePlayerAssignment.Add(entityCreationData.belongsPlayerId);
				return true;
			}
		}
		return false;
	}

	public List<EntityCreationData> GetAllDronesECD()
	{
		List<EntityCreationData> list = new List<EntityCreationData>();
		for (int i = 0; i < dronesActive.Count; i++)
		{
			list.Add(new EntityCreationData(dronesActive[i], _bNetworkWrite: false));
		}
		for (int j = 0; j < dronesUnloaded.Count; j++)
		{
			list.Add(dronesUnloaded[j]);
		}
		return list;
	}

	public void UpdateWaypointsForAllPlayers()
	{
		foreach (EntityPlayer player in GameManager.Instance.World.GetPlayers())
		{
			UpdateWaypointsForPlayer(player.entityId);
		}
	}

	public void UpdateWaypointsForPlayer(int entityId)
	{
		EntityPlayer entityPlayer = (EntityPlayer)GameManager.Instance.World.GetEntity(entityId);
		if (entityPlayer == null)
		{
			return;
		}
		NavObjectManager.Instance.UnRegisterNavObjectByClass("entityJunkDrone");
		List<EntityCreationData> allDronesECD = GetAllDronesECD();
		OwnedEntityData[] ownedEntityClass = entityPlayer.GetOwnedEntityClass("entityJunkDrone");
		List<(int, Vector3)> list = new List<(int, Vector3)>();
		OwnedEntityData[] array = ownedEntityClass;
		foreach (OwnedEntityData ownedEntityData in array)
		{
			foreach (EntityCreationData item in allDronesECD)
			{
				if (item.id == ownedEntityData.Id)
				{
					list.Add((ownedEntityData.Id, item.pos));
				}
			}
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null && entityId == primaryPlayer.entityId)
		{
			primaryPlayer.Waypoints.SetDroneWaypointsFromDroneManager(list);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityWaypointList>().Setup(eWayPointListType.Drone, list), _onlyClientsAttachedToAnEntity: false, entityId);
		}
	}

	public void SpawnFollowingDronesForPLayer(int entityId, World world)
	{
		if ((EntityPlayer)GameManager.Instance.World.GetEntity(entityId) == null)
		{
			return;
		}
		foreach (EntityCreationData item in GetAllDronesECD())
		{
			if (item.belongsPlayerId == entityId && dronesUnloaded.Contains(item) && item.orderState == 0)
			{
				CreateDroneEntity(item, world).TeleportIfFollowing();
			}
		}
		foreach (EntityDrone item2 in dronesActive)
		{
			if (item2.belongsPlayerId == entityId && item2.OrderState == EntityDrone.Orders.Follow)
			{
				item2.TeleportIfFollowing();
			}
		}
	}

	public static void Cleanup()
	{
		if (instance != null)
		{
			instance.SaveAndClear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveAndClear()
	{
		WaitOnSave();
		Save();
		WaitOnSave();
		dronesActive.Clear();
		dronesUnloaded.Clear();
		instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WaitOnSave()
	{
		if (saveThread != null)
		{
			saveThread.WaitForEnd();
			saveThread = null;
		}
	}

	public void Load()
	{
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "drones.dat");
		if (!SdFile.Exists(text))
		{
			return;
		}
		try
		{
			using Stream baseStream = SdFile.OpenRead(text);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			read(pooledBinaryReader);
		}
		catch (Exception)
		{
			text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "drones.dat.bak");
			if (SdFile.Exists(text))
			{
				using Stream baseStream2 = SdFile.OpenRead(text);
				using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader2.SetBaseStream(baseStream2);
				read(pooledBinaryReader2);
			}
		}
		Log.Out("{0} {1}, loaded {2}", GetType(), text, dronesUnloaded.Count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save()
	{
		if (saveThread == null || !ThreadManager.ActiveThreads.ContainsKey("droneDataSave"))
		{
			Log.Out("{0} saving {1} ({2} + {3})", GetType(), dronesActive.Count + dronesUnloaded.Count, dronesActive.Count, dronesUnloaded.Count);
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				write(pooledBinaryWriter);
			}
			saveThread = ThreadManager.StartThread("droneDataSave", null, SaveThread, null, pooledExpandableMemoryStream, null, _useRealThread: false, _isSilent: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SaveThread(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "drones.dat");
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "drones.dat.bak"), overwrite: true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		Log.Out("{0} saved {1} bytes", GetType(), pooledExpandableMemoryStream.Length);
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(PooledBinaryReader _br)
	{
		if (_br.ReadChar() != 'v' || _br.ReadChar() != 'd' || _br.ReadChar() != 'a' || _br.ReadChar() != 0)
		{
			Log.Error("{0} file bad signature", GetType());
			return;
		}
		if (_br.ReadByte() != 1)
		{
			Log.Error("{0} file bad version", GetType());
			return;
		}
		dronesUnloaded.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			EntityCreationData entityCreationData = new EntityCreationData();
			entityCreationData.read(_br, _bNetworkRead: false);
			dronesUnloaded.Add(entityCreationData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(PooledBinaryWriter _bw)
	{
		_bw.Write('v');
		_bw.Write('d');
		_bw.Write('a');
		_bw.Write((byte)0);
		_bw.Write((byte)1);
		List<EntityCreationData> list = new List<EntityCreationData>();
		GetDrones(list);
		_bw.Write(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			EntityCreationData entityCreationData = list[i];
			if (!isValidDronePos(entityCreationData.pos))
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityCreationData.belongsPlayerId) as EntityPlayer;
				if ((bool)entityPlayer)
				{
					Log.Warning("corrupted data using the player position");
					entityCreationData.pos = entityPlayer.getHeadPosition();
				}
				else
				{
					Log.Warning("corrupted data clearing the drone position");
					entityCreationData.pos = Vector3.zero;
				}
			}
			entityCreationData.write(_bw, _bNetworkWrite: false);
		}
	}

	public List<EntityCreationData> GetDronesList()
	{
		List<EntityCreationData> list = new List<EntityCreationData>();
		GetDrones(list);
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetDrones(List<EntityCreationData> _list)
	{
		for (int i = 0; i < dronesActive.Count; i++)
		{
			_list.Add(new EntityCreationData(dronesActive[i]));
		}
		for (int j = 0; j < dronesUnloaded.Count; j++)
		{
			_list.Add(dronesUnloaded[j]);
		}
		for (int k = 0; k < dronesWithoutOwner.Count; k++)
		{
			_list.Add(dronesWithoutOwner[k]);
		}
	}

	public List<(int entityId, Vector3 position)> GetDronePositionsList()
	{
		List<(int, Vector3)> list = new List<(int, Vector3)>();
		for (int i = 0; i < dronesActive.Count; i++)
		{
			list.Add((dronesActive[i].entityId, dronesActive[i].position));
		}
		for (int j = 0; j < dronesUnloaded.Count; j++)
		{
			list.Add((dronesUnloaded[j].id, dronesUnloaded[j].pos));
		}
		for (int k = 0; k < dronesWithoutOwner.Count; k++)
		{
			list.Add((dronesWithoutOwner[k].id, dronesWithoutOwner[k].pos));
		}
		return list;
	}

	public static int GetServerDroneCount()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return Instance.dronesActive.Count + Instance.dronesUnloaded.Count;
		}
		return serverDroneCount;
	}

	public static void SetServerDroneCount(int count)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			serverDroneCount = count;
		}
	}

	public static bool CanAddMoreDrones()
	{
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			return GetServerDroneCount() < 500;
		}
		return true;
	}

	[Conditional("DEBUG_DRONEMAN")]
	public static void VMLog(string _format = "", params object[] _args)
	{
		int frameCount = GameManager.frameCount;
		_format = string.Format("{0} {1} {2}", frameCount, "DroneManager", _format);
		Log.Out(_format, _args);
	}
}

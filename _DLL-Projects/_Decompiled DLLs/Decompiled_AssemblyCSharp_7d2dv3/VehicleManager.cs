#define DEBUG_VEHICLEMAN
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Platform;
using UnityEngine;

public class VehicleManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSaveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cChangeSaveDelay = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxVehicles = 500;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int serverVehicleCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityVehicle> vehiclesActive = new List<EntityVehicle>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityCreationData> vehiclesUnloaded = new List<EntityCreationData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float saveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo saveThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public static VehicleManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityCreationData> vehiclesList;

	public static VehicleManager Instance => instance;

	public VehicleManager()
	{
		vehiclesList = new List<EntityCreationData>();
	}

	public static void Init()
	{
		instance = new VehicleManager();
		instance.Load();
	}

	public void AddTrackedVehicle(EntityVehicle _vehicle)
	{
		if (!_vehicle)
		{
			Log.Error("VehicleManager AddTrackedVehicle null");
		}
		else if (!vehiclesActive.Contains(_vehicle))
		{
			vehiclesActive.Add(_vehicle);
			TriggerSave();
		}
	}

	public void RemoveTrackedVehicle(EntityVehicle _vehicle, EnumRemoveEntityReason _reason)
	{
		VMLog("RemoveTrackedVehicle {0}, {1}", _vehicle, _reason);
		vehiclesActive.Remove(_vehicle);
		if (_reason == EnumRemoveEntityReason.Unloaded)
		{
			vehiclesUnloaded.Add(new EntityCreationData(_vehicle, _bNetworkWrite: false));
		}
		TriggerSave();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageVehicleCount>().Setup());
		UpdateVehicleWaypoints();
	}

	[PublicizedFrom(EAccessModifier.Private)]
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
		for (int num2 = vehiclesUnloaded.Count - 1; num2 >= 0; num2--)
		{
			EntityCreationData entityCreationData = vehiclesUnloaded[num2];
			Entity entity = world.GetEntity(entityCreationData.id);
			if ((bool)entity)
			{
				if (entity is EntityVehicle entityVehicle)
				{
					Log.Warning("VehicleManager already loaded #{0}, id {1}, {2}, {3}, owner {4}", num2, entityCreationData.id, entityVehicle, entityVehicle.position.ToCultureInvariantString(), entityVehicle.vehicle.OwnerId?.CombinedString);
					vehiclesUnloaded.RemoveAt(num2);
					continue;
				}
				Log.Warning("VehicleManager id used #{0}, id {1}, {2}, {3}", num2, entityCreationData.id, entity, entity.position.ToCultureInvariantString());
				entityCreationData.id = -1;
			}
			if (!world.IsChunkAreaCollidersLoaded(entityCreationData.pos))
			{
				continue;
			}
			entityCreationData.pos.y += 0.002f;
			EntityVehicle entityVehicle2 = EntityFactory.CreateEntity(entityCreationData) as EntityVehicle;
			if ((bool)entityVehicle2)
			{
				vehiclesActive.Add(entityVehicle2);
				world.SpawnEntityInWorld(entityVehicle2);
				int belongsPlayerId = -1;
				if (entityVehicle2.vehicle.OwnerId != null)
				{
					PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(entityVehicle2.vehicle.OwnerId);
					if (playerData != null)
					{
						belongsPlayerId = playerData.EntityId;
					}
				}
				entityVehicle2.belongsPlayerId = belongsPlayerId;
				VMLog("loaded #{0}, id {1}, {2}, {3}, chunk {4} ({5}, {6}), owner {7}", num2, entityCreationData.id, entityVehicle2, entityVehicle2.position.ToCultureInvariantString(), World.toChunkXZ(entityVehicle2.position), entityVehicle2.chunkPosAddedEntityTo.x, entityVehicle2.chunkPosAddedEntityTo.z, entityVehicle2.vehicle.OwnerId?.CombinedString);
				num++;
			}
			else
			{
				Log.Error("VehicleManager load failed #{0}, id {1}, {2}", num2, entityCreationData.id, EntityClass.GetEntityClassName(entityCreationData.entityClass));
			}
			vehiclesUnloaded.RemoveAt(num2);
		}
		if (num > 0)
		{
			VMLog("Update loaded {0}", num);
		}
		saveTime -= Time.deltaTime;
		if (saveTime <= 0f && (saveThread == null || saveThread.HasTerminated()))
		{
			saveTime = 120f;
			Save();
		}
	}

	public void PhysicsWakeNear(Vector3 pos)
	{
		for (int i = 0; i < vehiclesActive.Count; i++)
		{
			EntityVehicle entityVehicle = vehiclesActive[i];
			if ((bool)entityVehicle && (entityVehicle.position - pos).sqrMagnitude <= 400f)
			{
				entityVehicle.AddForce(Vector3.zero);
			}
		}
	}

	public void RemoveAllVehiclesFromMap()
	{
		for (int i = 0; i < vehiclesActive.Count; i++)
		{
			GameManager.Instance.World.RemoveEntityFromMap(vehiclesActive[i], EnumRemoveEntityReason.Unloaded);
		}
	}

	public void RemoveUnloadedVehicle(int id)
	{
		EntityCreationData entityCreationData = null;
		foreach (EntityCreationData item in vehiclesUnloaded)
		{
			if (item.id == id)
			{
				entityCreationData = item;
				break;
			}
		}
		if (entityCreationData != null)
		{
			vehiclesUnloaded.Remove(entityCreationData);
			TriggerSave();
			UpdateVehicleWaypoints();
		}
	}

	public void UpdateVehicleWaypoints()
	{
		foreach (EntityPlayer player in GameManager.Instance.World.GetPlayers())
		{
			UpdateVehicleWaypointsForPlayer(player.entityId);
		}
	}

	public void UpdateVehicleWaypointsForPlayer(int entityId)
	{
		GetVehiclesECDList();
		List<(int, Vector3)> list = new List<(int, Vector3)>();
		foreach (EntityCreationData vehicles in vehiclesList)
		{
			if (vehicles.belongsPlayerId == entityId || vehicles.belongsPlayerId == -1)
			{
				list.Add((vehicles.id, vehicles.pos));
			}
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null && entityId == primaryPlayer.entityId)
		{
			primaryPlayer.Waypoints.SetEntityVehicleWaypointFromVehicleManager(list);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityWaypointList>().Setup(eWayPointListType.Vehicle, list), _onlyClientsAttachedToAnEntity: false, entityId);
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
		vehiclesActive.Clear();
		vehiclesUnloaded.Clear();
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
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "vehicles.dat");
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
			text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "vehicles.dat.bak");
			if (SdFile.Exists(text))
			{
				using Stream baseStream2 = SdFile.OpenRead(text);
				using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader2.SetBaseStream(baseStream2);
				read(pooledBinaryReader2);
			}
		}
		Log.Out("VehicleManager {0}, loaded {1}", text, vehiclesUnloaded.Count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save()
	{
		if (saveThread == null || !ThreadManager.ActiveThreads.ContainsKey("vehicleDataSave"))
		{
			Log.Out("VehicleManager saving {0} ({1} + {2})", vehiclesActive.Count + vehiclesUnloaded.Count, vehiclesActive.Count, vehiclesUnloaded.Count);
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				write(pooledBinaryWriter);
			}
			saveThread = ThreadManager.StartThread("vehicleDataSave", null, SaveThread, null, pooledExpandableMemoryStream, null, _useRealThread: false, _isSilent: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SaveThread(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "vehicles.dat");
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "vehicles.dat.bak"), overwrite: true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		Log.Out("VehicleManager saved {0} bytes", pooledExpandableMemoryStream.Length);
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(PooledBinaryReader _br)
	{
		if (_br.ReadChar() != 'v' || _br.ReadChar() != 'd' || _br.ReadChar() != 'a' || _br.ReadChar() != 0)
		{
			Log.Error("Vehicle file bad signature");
			return;
		}
		if (_br.ReadByte() != 1)
		{
			Log.Error("Vehicle file bad version");
			return;
		}
		vehiclesUnloaded.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			EntityCreationData entityCreationData = new EntityCreationData();
			entityCreationData.read(_br, _bNetworkRead: false);
			vehiclesUnloaded.Add(entityCreationData);
			VMLog("read #{0}, id {1}, {2}, {3}, chunk {4}", i, entityCreationData.id, EntityClass.GetEntityClassName(entityCreationData.entityClass), entityCreationData.pos.ToCultureInvariantString(), World.toChunkXZ(entityCreationData.pos));
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
		List<EntityCreationData> vehiclesECDList = GetVehiclesECDList();
		_bw.Write(vehiclesECDList.Count);
		for (int i = 0; i < vehiclesECDList.Count; i++)
		{
			EntityCreationData entityCreationData = vehiclesECDList[i];
			entityCreationData.write(_bw, _bNetworkWrite: false);
			VMLog("write #{0}, id {1}, {2}, {3}, chunk {4}", i, entityCreationData.id, EntityClass.GetEntityClassName(entityCreationData.entityClass), entityCreationData.pos.ToCultureInvariantString(), World.toChunkXZ(entityCreationData.pos));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityCreationData> GetVehiclesECDList()
	{
		vehiclesList.Clear();
		for (int i = 0; i < vehiclesActive.Count; i++)
		{
			vehiclesList.Add(new EntityCreationData(vehiclesActive[i], _bNetworkWrite: false));
		}
		for (int j = 0; j < vehiclesUnloaded.Count; j++)
		{
			vehiclesList.Add(vehiclesUnloaded[j]);
		}
		return vehiclesList;
	}

	public List<(int entityId, Vector3 position)> GetVehiclePositionsList()
	{
		List<(int, Vector3)> list = new List<(int, Vector3)>();
		for (int i = 0; i < vehiclesActive.Count; i++)
		{
			list.Add((vehiclesActive[i].entityId, vehiclesActive[i].position));
		}
		for (int j = 0; j < vehiclesUnloaded.Count; j++)
		{
			list.Add((vehiclesUnloaded[j].id, vehiclesUnloaded[j].pos));
		}
		return list;
	}

	public static int GetServerVehicleCount()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return Instance.vehiclesActive.Count + Instance.vehiclesUnloaded.Count;
		}
		return serverVehicleCount;
	}

	public static void SetServerVehicleCount(int count)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			serverVehicleCount = count;
		}
	}

	public static bool CanAddMoreVehicles()
	{
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			return GetServerVehicleCount() < 500;
		}
		return true;
	}

	[Conditional("DEBUG_VEHICLEMAN")]
	public static void VMLog(string _format = "", params object[] _args)
	{
		int frameCount = GameManager.frameCount;
		_format = $"{frameCount} VehicleManager {_format}";
		Log.Out(_format, _args);
	}
}

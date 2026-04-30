using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageVehicleDataSync : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int senderId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int vehicleId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort syncFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream entityData = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

	public NetPackageVehicleDataSync Setup(EntityVehicle _ev, int _senderId, ushort _syncFlags)
	{
		senderId = _senderId;
		vehicleId = _ev.entityId;
		syncFlags = _syncFlags;
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(entityData);
		_ev.WriteSyncData(pooledBinaryWriter, _syncFlags);
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackageVehicleDataSync()
	{
		MemoryPools.poolMemoryStream.FreeSync(entityData);
	}

	public override void read(PooledBinaryReader _br)
	{
		senderId = _br.ReadInt32();
		vehicleId = _br.ReadInt32();
		syncFlags = _br.ReadUInt16();
		int length = _br.ReadUInt16();
		StreamUtils.StreamCopy(_br.BaseStream, entityData, length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(senderId);
		_bw.Write(vehicleId);
		_bw.Write(syncFlags);
		_bw.Write((ushort)entityData.Length);
		entityData.WriteTo(_bw.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !ValidEntityIdForSender(senderId))
		{
			return;
		}
		EntityVehicle entityVehicle = GameManager.Instance.World.GetEntity(vehicleId) as EntityVehicle;
		if (entityVehicle == null)
		{
			return;
		}
		if (entityData.Length > 0)
		{
			lock (entityData)
			{
				entityData.Position = 0L;
				try
				{
					using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
					pooledBinaryReader.SetBaseStream(entityData);
					entityVehicle.ReadSyncData(pooledBinaryReader, syncFlags, senderId);
				}
				catch (Exception e)
				{
					Log.Exception(e);
					Log.Error("Error syncing data for entity " + entityVehicle?.ToString() + "; Sender id = " + senderId);
				}
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			ushort syncFlagsReplicated = entityVehicle.GetSyncFlagsReplicated(syncFlags);
			if (syncFlagsReplicated != 0)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageVehicleDataSync>().Setup(entityVehicle, senderId, syncFlagsReplicated), _onlyClientsAttachedToAnEntity: false, -1, senderId);
			}
		}
	}

	public override int GetLength()
	{
		return (int)(12 + entityData.Length);
	}
}

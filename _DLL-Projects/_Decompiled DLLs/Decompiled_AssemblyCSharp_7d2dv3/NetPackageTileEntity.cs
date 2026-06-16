using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTileEntity : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte handle;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i teWorldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

	public NetPackageTileEntity Setup(TileEntity _te, TileEntity.StreamModeWrite _eStreamMode)
	{
		return Setup(_te, _eStreamMode, byte.MaxValue);
	}

	public NetPackageTileEntity Setup(TileEntity _te, TileEntity.StreamModeWrite _eStreamMode, byte _handle)
	{
		handle = _handle;
		teWorldPos = _te.ToWorldPos();
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(ms);
		_te.write(pooledBinaryWriter, _eStreamMode);
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackageTileEntity()
	{
		MemoryPools.poolMemoryStream.FreeSync(ms);
	}

	public override void read(PooledBinaryReader _br)
	{
		handle = _br.ReadByte();
		teWorldPos = StreamUtils.ReadVector3i(_br);
		int length = _br.ReadUInt16();
		StreamUtils.StreamCopy(_br.BaseStream, ms, length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(handle);
		StreamUtils.Write(_bw, teWorldPos);
		_bw.Write((ushort)ms.Length);
		ms.WriteTo(_bw.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		TileEntity tileEntity = _world.GetTileEntity(teWorldPos);
		if (tileEntity == null)
		{
			return;
		}
		tileEntity.SetHandle(handle);
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
		{
			lock (ms)
			{
				pooledBinaryReader.SetBaseStream(ms);
				ms.Position = 0L;
				tileEntity.read(pooledBinaryReader, _world.IsRemote() ? TileEntity.StreamModeRead.FromServer : TileEntity.StreamModeRead.FromClient);
			}
		}
		tileEntity.NotifyListeners();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			tileEntity.SetChunkModified();
			Vector3? entitiesInRangeOfWorldPos = tileEntity.ToWorldCenterPos();
			if (entitiesInRangeOfWorldPos.Value == Vector3.zero)
			{
				entitiesInRangeOfWorldPos = null;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(tileEntity, TileEntity.StreamModeWrite.ToClient, handle), _onlyClientsAttachedToAnEntity: true, -1, -1, -1, entitiesInRangeOfWorldPos);
		}
	}

	public override int GetLength()
	{
		return (int)(22 + ms.Length);
	}
}

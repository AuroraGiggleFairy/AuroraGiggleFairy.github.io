using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityStatsBuff : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	public override bool ReliableDelivery => false;

	public NetPackageEntityStatsBuff Setup(EntityAlive entity, byte[] _data = null)
	{
		m_entityId = entity.entityId;
		if (_data == null)
		{
			using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true);
			pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
			entity.Buffs.Write(pooledBinaryWriter, _netSync: true);
			data = pooledExpandableMemoryStream.ToArray();
		}
		else
		{
			data = _data;
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		m_entityId = _reader.ReadInt32();
		data = _reader.ReadBytes(_reader.ReadInt32());
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(m_entityId);
		_writer.Write(data.Length);
		_writer.Write(data);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(m_entityId) as EntityAlive;
		if (!(entityAlive != null))
		{
			return;
		}
		if (entityAlive.isEntityRemote)
		{
			using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			pooledExpandableMemoryStream.Write(data, 0, data.Length);
			pooledExpandableMemoryStream.Position = 0L;
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
			pooledBinaryReader.SetBaseStream(pooledExpandableMemoryStream);
			entityAlive.Buffs.Read(pooledBinaryReader);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityStatsBuff>().Setup(entityAlive, data), _onlyClientsAttachedToAnEntity: false, -1, entityAlive.entityId);
		}
	}

	public override int GetLength()
	{
		return data.Length + 4;
	}
}

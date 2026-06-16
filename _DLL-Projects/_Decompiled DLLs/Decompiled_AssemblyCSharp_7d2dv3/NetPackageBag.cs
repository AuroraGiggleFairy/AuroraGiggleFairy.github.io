using UnityEngine.Scripting;

[Preserve]
public class NetPackageBag : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageBag Setup(int _entityId, Bag _bag)
	{
		entityId = _entityId;
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(ms);
		_bag.Write(pooledBinaryWriter);
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackageBag()
	{
		MemoryPools.poolMemoryStream.FreeSync(ms);
	}

	public override void read(PooledBinaryReader _br)
	{
		entityId = _br.ReadInt32();
		int length = _br.ReadUInt16();
		StreamUtils.StreamCopy(_br.BaseStream, ms, length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(entityId);
		_bw.Write((ushort)ms.Length);
		ms.WriteTo(_bw.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Bag bag = _world.GetEntity(entityId)?.bag;
		if (bag == null)
		{
			Log.Warning("[NetPackageBag] Entity " + entityId + " with bag not found");
			return;
		}
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		lock (ms)
		{
			pooledBinaryReader.SetBaseStream(ms);
			ms.Position = 0L;
			bag.ReadInto(pooledBinaryReader);
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}

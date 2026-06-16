using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityPrimeDetonator : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntityPrimeDetonator Setup(EntityZombieCop entity)
	{
		id = entity.entityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		id = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(id);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityZombieCop entityZombieCop = _world.GetEntity(id) as EntityZombieCop;
			if (entityZombieCop == null)
			{
				Log.Out("Discarding " + GetType().Name);
			}
			else
			{
				entityZombieCop.PrimeDetonator();
			}
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}

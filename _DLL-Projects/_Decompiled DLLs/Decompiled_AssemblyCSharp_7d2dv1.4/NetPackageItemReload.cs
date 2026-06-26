using UnityEngine.Scripting;

[Preserve]
public class NetPackageItemReload : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	public NetPackageItemReload Setup(int _entityId)
	{
		entityId = _entityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		entityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(entityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (!_world.IsRemote())
			{
				_world.GetGameManager().ItemReloadServer(entityId);
			}
			else
			{
				_world.GetGameManager().ItemReloadClient(entityId);
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}

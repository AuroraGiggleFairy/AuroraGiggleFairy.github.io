using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAddExpClient : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int xp;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int xpType;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntityAddExpClient Setup(int _entityId, int _xp, Progression.XPTypes _xpType)
	{
		entityId = _entityId;
		xp = _xp;
		xpType = (int)_xpType;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		xp = _reader.ReadInt32();
		xpType = _reader.ReadInt16();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(xp);
		_writer.Write((short)xpType);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)_world.GetEntity(entityId);
		if (entityAlive != null)
		{
			string cvarXPName = "_xpOther";
			if (xpType == 0)
			{
				cvarXPName = "_xpFromKill";
			}
			entityAlive.Progression.AddLevelExp(xp, cvarXPName, (Progression.XPTypes)xpType, useBonus: true, notifyUI: true, entityId);
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}

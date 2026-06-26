using UnityEngine.Scripting;

[Preserve]
public class NetPackageSharedPartyKill : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityTypeID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int xp;

	[PublicizedFrom(EAccessModifier.Private)]
	public int killerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityID;

	public NetPackageSharedPartyKill Setup(int _entityID, int _killerID)
	{
		entityID = _entityID;
		killerID = _killerID;
		return this;
	}

	public NetPackageSharedPartyKill Setup(int _entityTypeID, int _xp, int _killerID, int _killedEntityID)
	{
		entityTypeID = _entityTypeID;
		xp = _xp;
		killerID = _killerID;
		entityID = _killedEntityID;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		entityTypeID = _br.ReadInt32();
		xp = _br.ReadInt32();
		entityID = _br.ReadInt32();
		killerID = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(entityTypeID);
		_bw.Write(xp);
		_bw.Write(entityID);
		_bw.Write(killerID);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.SharedKillServer(entityID, killerID);
		}
		else
		{
			GameManager.Instance.SharedKillClient(entityTypeID, xp, null, entityID);
		}
	}

	public override int GetLength()
	{
		return 25;
	}
}

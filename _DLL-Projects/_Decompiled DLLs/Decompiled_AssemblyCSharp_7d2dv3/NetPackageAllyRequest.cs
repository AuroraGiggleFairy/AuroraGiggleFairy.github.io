using UnityEngine.Scripting;

[Preserve]
public class NetPackageAllyRequest : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs source;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs target;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool addAlly;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageAllyRequest Setup(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, bool _addAlly)
	{
		source = _source;
		target = _target;
		addAlly = _addAlly;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		source = PlatformUserIdentifierAbs.FromStream(_br);
		target = PlatformUserIdentifierAbs.FromStream(_br);
		addAlly = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		source.ToStream(_bw);
		target.ToStream(_bw);
		_bw.Write(addAlly);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		GameManager.Instance.persistentPlayers.Allies.ProcessAllyRequest(source, target, addAlly);
	}

	public override int GetLength()
	{
		return 0;
	}
}

using UnityEngine.Scripting;

[Preserve]
public class NetPackageAllyResponse : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs source;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs target;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllyStore.AllyStatus newStatus;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllyStore.AllyEvent allyEventSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllyStore.AllyEvent allyEventTarget;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageAllyResponse Setup(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, AllyStore.AllyStatus _newStatus, AllyStore.AllyEvent _allyEventSource, AllyStore.AllyEvent _allyEventTarget)
	{
		source = _source;
		target = _target;
		newStatus = _newStatus;
		allyEventSource = _allyEventSource;
		allyEventTarget = _allyEventTarget;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		source = PlatformUserIdentifierAbs.FromStream(_br);
		target = PlatformUserIdentifierAbs.FromStream(_br);
		newStatus = (AllyStore.AllyStatus)_br.ReadByte();
		allyEventSource = (AllyStore.AllyEvent)_br.ReadByte();
		allyEventTarget = (AllyStore.AllyEvent)_br.ReadByte();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		source.ToStream(_bw);
		target.ToStream(_bw);
		_bw.Write((byte)newStatus);
		_bw.Write((byte)allyEventSource);
		_bw.Write((byte)allyEventTarget);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		GameManager.Instance.persistentPlayers.Allies.AllyUpdateResponse(source, target, newStatus, allyEventSource, allyEventTarget);
	}

	public override int GetLength()
	{
		return 0;
	}
}

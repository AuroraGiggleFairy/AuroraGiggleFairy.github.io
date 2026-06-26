using UnityEngine.Scripting;

[Preserve]
public class NetPackageMinEventFire : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int selfEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int otherEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public MinEventTypes eventType;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageMinEventFire Setup(int _selfEntityID, int _otherEntityID, MinEventTypes _eventType, ItemValue _itemValue)
	{
		selfEntityID = _selfEntityID;
		otherEntityID = _otherEntityID;
		eventType = _eventType;
		itemValue = _itemValue;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		selfEntityID = _br.ReadInt32();
		otherEntityID = _br.ReadInt32();
		eventType = (MinEventTypes)_br.ReadByte();
		itemValue = new ItemValue();
		itemValue.Read(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(selfEntityID);
		_bw.Write(otherEntityID);
		_bw.Write((byte)eventType);
		itemValue.Write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.GetEntity(selfEntityID) is EntityAlive entityAlive)
		{
			entityAlive.MinEventContext.Self = entityAlive;
			entityAlive.MinEventContext.Other = ((otherEntityID == -1) ? null : (_world.GetEntity(otherEntityID) as EntityAlive));
			entityAlive.MinEventContext.ItemValue = itemValue;
			entityAlive.FireEvent(eventType);
		}
	}

	public override int GetLength()
	{
		return 32;
	}
}

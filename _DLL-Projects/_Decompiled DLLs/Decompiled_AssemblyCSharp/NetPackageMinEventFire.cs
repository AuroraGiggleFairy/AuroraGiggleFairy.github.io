using UnityEngine.Scripting;

[Preserve]
public class NetPackageMinEventFire : NetPackage
{
	public enum EventPackageTypes
	{
		ItemEvent,
		BlockEvent
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int selfEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int otherEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public MinEventTypes eventType;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public EventPackageTypes eventPackageType;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageMinEventFire Setup(int _selfEntityID, int _otherEntityID, MinEventTypes _eventType, ItemValue _itemValue)
	{
		selfEntityID = _selfEntityID;
		otherEntityID = _otherEntityID;
		eventType = _eventType;
		itemValue = _itemValue;
		eventPackageType = EventPackageTypes.ItemEvent;
		return this;
	}

	public NetPackageMinEventFire Setup(int _selfEntityID, int _otherEntityID, MinEventTypes _eventType, BlockValue _blockValue)
	{
		selfEntityID = _selfEntityID;
		otherEntityID = _otherEntityID;
		eventType = _eventType;
		blockValue = _blockValue;
		eventPackageType = EventPackageTypes.BlockEvent;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		selfEntityID = _br.ReadInt32();
		otherEntityID = _br.ReadInt32();
		eventType = (MinEventTypes)_br.ReadByte();
		eventPackageType = (EventPackageTypes)_br.ReadByte();
		if (eventPackageType == EventPackageTypes.ItemEvent)
		{
			itemValue = new ItemValue();
			itemValue.Read(_br);
			blockValue = BlockValue.Air;
		}
		else
		{
			blockValue = new BlockValue(_br.ReadUInt32());
			itemValue = ItemValue.None;
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(selfEntityID);
		_bw.Write(otherEntityID);
		_bw.Write((byte)eventType);
		_bw.Write((byte)eventPackageType);
		if (eventPackageType == EventPackageTypes.ItemEvent)
		{
			itemValue.Write(_bw);
		}
		else
		{
			_bw.Write(blockValue.rawData);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.GetEntity(selfEntityID) is EntityAlive entityAlive)
		{
			entityAlive.MinEventContext.Self = entityAlive;
			entityAlive.MinEventContext.Other = ((otherEntityID == -1) ? null : (_world.GetEntity(otherEntityID) as EntityAlive));
			entityAlive.MinEventContext.ItemValue = itemValue;
			entityAlive.MinEventContext.BlockValue = blockValue;
			entityAlive.FireEvent(eventType);
		}
	}

	public override int GetLength()
	{
		return 32;
	}
}

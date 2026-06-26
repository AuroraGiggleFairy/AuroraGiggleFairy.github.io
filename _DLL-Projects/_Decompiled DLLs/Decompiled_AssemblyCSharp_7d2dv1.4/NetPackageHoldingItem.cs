using UnityEngine.Scripting;

[Preserve]
public class NetPackageHoldingItem : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack holdingItemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte holdingItemIndex;

	public NetPackageHoldingItem Setup(EntityAlive _entity)
	{
		entityId = _entity.entityId;
		holdingItemStack = _entity.inventory.holdingItemStack;
		holdingItemIndex = (byte)_entity.inventory.holdingItemIdx;
		Log.Out("SENDING item with meta {0} ", holdingItemStack.itemValue.Meta);
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		holdingItemStack = new ItemStack();
		holdingItemStack.Read(_reader);
		holdingItemIndex = _reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		holdingItemStack.Write(_writer);
		_writer.Write(holdingItemIndex);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !ValidEntityIdForSender(entityId))
		{
			return;
		}
		Log.Out("RECEIVED item with meta {0} ", holdingItemStack.itemValue.Meta);
		EntityAlive entityAlive = _world.GetEntity(entityId) as EntityAlive;
		if ((bool)entityAlive)
		{
			if (!entityAlive.inventory.GetItem(holdingItemIndex).Equals(holdingItemStack))
			{
				entityAlive.inventory.SetItem(holdingItemIndex, holdingItemStack);
			}
			if (entityAlive.inventory.holdingItemIdx != holdingItemIndex)
			{
				entityAlive.inventory.SetHoldingItemIdxNoHolsterTime(holdingItemIndex);
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageHoldingItem>().Setup(entityAlive), _onlyClientsAttachedToAnEntity: false, -1, base.Sender.entityId);
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}

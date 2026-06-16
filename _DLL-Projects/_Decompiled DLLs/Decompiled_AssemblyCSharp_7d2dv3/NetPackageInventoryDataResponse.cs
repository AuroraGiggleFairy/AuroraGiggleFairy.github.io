using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageInventoryDataResponse : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool success;

	[PublicizedFrom(EAccessModifier.Private)]
	public string errorMsg = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public Guid inventoryKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public Guid managerToken;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageInventoryDataResponse Setup(bool _success, string _errorMsg, Guid _inventoryKey, ItemStack[] _items, Guid _managerToken)
	{
		success = _success;
		errorMsg = _errorMsg;
		inventoryKey = _inventoryKey;
		items = _items;
		managerToken = _managerToken;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		success = _br.ReadBoolean();
		errorMsg = _br.ReadString();
		inventoryKey = StreamUtils.ReadGuid(_br);
		items = ItemStack.ReadArray(_br);
		managerToken = StreamUtils.ReadGuid(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(success);
		_bw.Write(errorMsg);
		StreamUtils.Write(_bw, inventoryKey);
		ItemStack.WriteArray(_bw, items);
		StreamUtils.Write(_bw, managerToken);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		TransactionalInventory _inventory;
		if (!success)
		{
			Log.Warning("Error during transaction request: " + errorMsg);
		}
		else if (InventoryManager.Instance.TryGetTransactionalInventory(inventoryKey, out _inventory))
		{
			_inventory.UpdateInventory(items, managerToken);
		}
		else
		{
			Log.Error(string.Format("{0} Could not find inventory with key: {1}", "NetPackageInventoryDataResponse", inventoryKey));
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}

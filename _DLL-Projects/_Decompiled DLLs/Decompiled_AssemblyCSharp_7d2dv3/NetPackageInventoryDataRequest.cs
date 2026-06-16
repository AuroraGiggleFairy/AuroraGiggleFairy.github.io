using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageInventoryDataRequest : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TransactionalInventory.KeyHashPair keyHash;

	[PublicizedFrom(EAccessModifier.Private)]
	public Guid managerToken;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageInventoryDataRequest Setup(TransactionalInventory.KeyHashPair _keyHash, Guid _managerToken)
	{
		keyHash = _keyHash;
		managerToken = _managerToken;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		keyHash = TransactionalInventory.KeyHashPair.Read(_br);
		managerToken = StreamUtils.ReadGuid(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		keyHash.Write(_bw);
		StreamUtils.Write(_bw, managerToken);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (InventoryManager.Instance.TryGetTransactionalInventory(keyHash.Key, out var _inventory))
		{
			if (_inventory.Hash == keyHash.Hash)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageInventoryDataResponse>().Setup(_success: true, string.Empty, keyHash.Key, null, managerToken), _onlyClientsAttachedToAnEntity: false, base.Sender.entityId);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageInventoryDataResponse>().Setup(_success: true, string.Empty, keyHash.Key, _inventory.GetItemsReadonly(), managerToken), _onlyClientsAttachedToAnEntity: false, base.Sender.entityId);
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageInventoryDataResponse>().Setup(_success: false, string.Format("{0} Could not find inventory with key: {1}", "NetPackageInventoryDataRequest", keyHash.Key), keyHash.Key, null, managerToken), _onlyClientsAttachedToAnEntity: false, base.Sender.entityId);
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}

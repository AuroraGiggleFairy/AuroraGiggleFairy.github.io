using System;
using System.Collections.Generic;

public class InventoryManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static InventoryManager _instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Guid, TransactionalInventory> inventories = new Dictionary<Guid, TransactionalInventory>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Guid secretToken = Guid.NewGuid();

	public static InventoryManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new InventoryManager();
			}
			return _instance;
		}
	}

	public void Init()
	{
		if (inventories.Count > 0)
		{
			Log.Warning("[InventoryManager] had locked entities from a previous session!");
			inventories.Clear();
		}
	}

	public void TransactionRequestLocal(InventoryTransaction _tx)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			if (!_tx.Apply(secretToken))
			{
				LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.CloseAllOpenModalWindows();
				throw new InvalidOperationException("[InventoryManager] failed to apply transaction locally.");
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageInventoryTransactionRequest>().Setup(_tx));
		}
		else
		{
			TransactionRequestServer(_tx, GameManager.Instance.World.GetPrimaryPlayerId());
		}
	}

	public void TransactionRequestServer(InventoryTransaction _tx, int _playerId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			throw new InvalidOperationException("[InventoryManager] TransactionRequestServer can only be called on the server.");
		}
		bool flag = _tx.Apply(secretToken);
		if (flag)
		{
			flag = _tx.ValidateFinalHashes();
		}
		if (!flag)
		{
			Log.Error("[InventoryManager] failed to apply transaction on server.");
			LockManager.Instance.ForceUnlockByPlayer(_playerId);
		}
		else if (GameManager.Instance.World.GetPrimaryPlayerId() != _playerId)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageInventoryTransactionResponse>().Setup(flag, null, null), _onlyClientsAttachedToAnEntity: false, _playerId);
		}
	}

	public void TransactionResponse(bool _success, IList<Guid> _keys, IList<ItemStack[]> _inventories)
	{
		throw new NotImplementedException();
	}

	public TransactionalInventory CreateInventoryServer(int _size)
	{
		return CreateInventoryServer(ItemStack.CreateArray(_size));
	}

	public TransactionalInventory CreateInventoryServer(ItemStack[] _stacks)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			throw new InvalidOperationException("[InventoryManager] CreateInventoryServer can only be called on the server.");
		}
		return CreateInventory(Guid.NewGuid(), _stacks);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TransactionalInventory CreateInventory(Guid _key, ItemStack[] _stacks)
	{
		TransactionalInventory transactionalInventory = new TransactionalInventory(_key, _stacks, secretToken);
		inventories.Add(_key, transactionalInventory);
		return transactionalInventory;
	}

	public void RequestInventoryFromServer(TransactionalInventory.KeyHashPair _keyHash)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			throw new InvalidOperationException("[InventoryManager] RequestInventoryFromServer can only be called on a client.");
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageInventoryDataRequest>().Setup(_keyHash, secretToken));
	}

	public bool TryGetTransactionalInventory(Guid _key, out TransactionalInventory _inventory)
	{
		return inventories.TryGetValue(_key, out _inventory);
	}

	public bool ValidateToken(Guid _token)
	{
		bool num = _token.Equals(secretToken);
		if (!num)
		{
			Log.Warning("[InventoryManager] calling client code provided invalid token.");
		}
		return num;
	}

	public void ReadInventory(PooledBinaryReader _br, TileEntity.StreamModeRead _mode, ref TransactionalInventory _inventory)
	{
		switch (_mode)
		{
		case TileEntity.StreamModeRead.Persistency:
		{
			Guid key2 = StreamUtils.ReadGuid(_br);
			ItemStack[] array = ItemStack.ReadArray(_br);
			if (TryGetTransactionalInventory(key2, out _inventory))
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && LockManager.Instance.IsLockedServer(_inventory, 0))
				{
					Log.Error("[InventoryManager] Cannot read from storage while target is locked.");
				}
				else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
				{
					Log.Error("[InventoryManager] Client tried to read inventory data from persistent storage.");
				}
				else
				{
					_inventory.UpdateInventory(array, secretToken);
				}
			}
			else
			{
				_inventory = CreateInventory(key2, array);
			}
			break;
		}
		case TileEntity.StreamModeRead.FromServer:
		{
			Guid key = StreamUtils.ReadGuid(_br);
			int size = _br.ReadInt32();
			if (!TryGetTransactionalInventory(key, out _inventory))
			{
				_inventory = CreateInventory(key, ItemStack.CreateArray(size));
			}
			break;
		}
		case TileEntity.StreamModeRead.FromClient:
			break;
		}
	}

	public void WriteInventory(TransactionalInventory _inventory, PooledBinaryWriter _bw, TileEntity.StreamModeWrite _mode)
	{
		switch (_mode)
		{
		case TileEntity.StreamModeWrite.Persistency:
			StreamUtils.Write(_bw, _inventory.Key);
			ItemStack.WriteArray(_bw, _inventory.GetItemsReadonly());
			break;
		case TileEntity.StreamModeWrite.ToClient:
			StreamUtils.Write(_bw, _inventory.Key);
			_bw.Write(_inventory.Length);
			break;
		case TileEntity.StreamModeWrite.ToServer:
			break;
		}
	}
}

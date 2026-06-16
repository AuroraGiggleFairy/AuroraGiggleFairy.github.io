using System;
using System.IO;

public class TransactionalInventory : ILockTarget
{
	public struct KeyHashPair(Guid _key, int _hash)
	{
		public Guid Key = _key;

		public int Hash = _hash;

		public static KeyHashPair Read(BinaryReader _br)
		{
			Guid key = StreamUtils.ReadGuid(_br);
			int hash = _br.ReadInt32();
			return new KeyHashPair(key, hash);
		}

		public void Write(BinaryWriter _bw)
		{
			StreamUtils.Write(_bw, Key);
			_bw.Write(Hash);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action inventoryRequestComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] buffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hashDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hash;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool needsSaving = true;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool DataReadyLocally
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Guid Key
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int Hash
	{
		get
		{
			if (hashDirty)
			{
				hash = items.GetArrayHashCode();
				hashDirty = false;
			}
			return hash;
		}
	}

	public KeyHashPair KeyHash => new KeyHashPair(Key, Hash);

	public bool NeedsSaving => needsSaving;

	public int Length => items.Length;

	public LockTargetType LockTargetType => LockTargetType.TransactionalInventory;

	public bool IsSharedLock(ushort _channel)
	{
		return false;
	}

	public TransactionalInventory(Guid _key, ItemStack[] _items, Guid _managerToken)
	{
		DataReadyLocally = true;
		items = _items;
		Key = _key;
		InventoryManager.Instance.ValidateToken(_managerToken);
	}

	public ItemStack GetItemReadonly(int _slot)
	{
		if (_slot > items.Length)
		{
			return null;
		}
		return items[_slot].Clone();
	}

	public ItemStack[] GetItemsReadonly()
	{
		return ItemStack.Clone(items);
	}

	public void ResizeInventory(int _size, Guid _managerToken)
	{
		if (InventoryManager.Instance.ValidateToken(_managerToken))
		{
			if (_size < items.Length)
			{
				Log.Warning("[TransactionalInventory] Inventory was resized to a smaller size, items may be lost.");
			}
			Array.Copy(items, 0, items, 0, (_size > items.Length) ? items.Length : _size);
		}
	}

	public void UpdateInventory(ItemStack[] _items, Guid _managerToken)
	{
		if (!InventoryManager.Instance.ValidateToken(_managerToken))
		{
			return;
		}
		if (_items != null)
		{
			if (items == null)
			{
				items = ItemStack.Clone(_items);
			}
			else if (_items.Length != items.Length)
			{
				ResizeInventory(_items.Length, _managerToken);
			}
			else
			{
				Array.Copy(_items, items, items.Length);
			}
		}
		hashDirty = true;
		DataReadyLocally = true;
		inventoryRequestComplete?.Invoke();
		inventoryRequestComplete = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckInventoryReady()
	{
		if (!DataReadyLocally)
		{
			Log.Warning("[TransactionalInventory] Data has not yet been fetched properly.");
			return false;
		}
		if (!LockManager.Instance.IsLockedByLocalPlayer(this, 0))
		{
			Log.Warning("[TransactionalInventory] Local player does not have a lock for this inventory.");
			return false;
		}
		return true;
	}

	public bool IsEmpty()
	{
		if (!CheckInventoryReady())
		{
			return false;
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (!items[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public int HasItem(ItemValue _itemValue)
	{
		if (!CheckInventoryReady())
		{
			return -1;
		}
		if (_itemValue == null)
		{
			return -1;
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (_itemValue.ItemClass.Equals(items[i].itemValue.ItemClass))
			{
				return i;
			}
		}
		return -1;
	}

	public bool TrySetItem(int _idx, ItemStack _stack)
	{
		if (!CheckInventoryReady())
		{
			return false;
		}
		if (_idx < 0 || _idx >= items.Length)
		{
			Log.Warning("[TransactionalInventory] Supplied index was out of bounds.");
			return false;
		}
		InventoryTransaction tx = new InventoryTransaction().SetStackAbsolute(this, _stack, _idx);
		InventoryManager.Instance.TransactionRequestLocal(tx);
		return true;
	}

	public bool TryAddItem(ItemStack _stack)
	{
		if (!CheckInventoryReady())
		{
			return false;
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].IsEmpty())
			{
				InventoryTransaction tx = new InventoryTransaction().SetStackAbsolute(this, _stack, i);
				InventoryManager.Instance.TransactionRequestLocal(tx);
				return true;
			}
		}
		return false;
	}

	public (bool success, int stacked, int total) TryStackItem(int _startIndex, ItemStack _stack)
	{
		if (!CheckInventoryReady())
		{
			return (success: false, stacked: 0, total: _stack.count);
		}
		int count = _stack.count;
		InventoryTransaction inventoryTransaction = new InventoryTransaction();
		for (int i = _startIndex; i < items.Length; i++)
		{
			if (items[i].CanStackPartlyWith(_stack, out var _count))
			{
				ItemStack itemStack = _stack.Clone();
				itemStack.count = _count;
				_stack.count -= _count;
				inventoryTransaction.SetStackRelative(this, itemStack, i);
				if (_stack.count == 0)
				{
					break;
				}
				continue;
			}
			return (success: false, stacked: 0, total: count);
		}
		InventoryManager.Instance.TransactionRequestLocal(inventoryTransaction);
		return (success: true, stacked: count - _stack.count, total: count);
	}

	public bool TryClearItem(int _idx)
	{
		return TrySetItem(_idx, ItemStack.Empty);
	}

	public bool ClearAllItems()
	{
		throw new NotImplementedException();
	}

	public void Rollback()
	{
		throw new NotImplementedException();
	}

	public void StartTransaction(Guid _managerToken)
	{
		if (InventoryManager.Instance.ValidateToken(_managerToken))
		{
			buffer = GetItemsReadonly();
		}
	}

	public void FinalizeTransaction(bool _success, Guid _managerToken)
	{
		if (InventoryManager.Instance.ValidateToken(_managerToken))
		{
			if (_success)
			{
				UpdateInventory(buffer, _managerToken);
				needsSaving = true;
			}
			buffer = null;
		}
	}

	public bool ProcessOperation(InventoryOperation _operation, Guid _managerToken)
	{
		if (!InventoryManager.Instance.ValidateToken(_managerToken))
		{
			return false;
		}
		return _operation.Operation switch
		{
			InventoryOperation.EnumOperation.SetAbsolute => ProcessSetAbsolute(_operation), 
			InventoryOperation.EnumOperation.SetRelative => ProcessSetRelative(_operation), 
			InventoryOperation.EnumOperation.SetAll => ProcessSetAll(_operation), 
			_ => false, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ProcessSetAbsolute(InventoryOperation _op)
	{
		if (buffer == null || _op.Operation != InventoryOperation.EnumOperation.SetAbsolute)
		{
			return false;
		}
		if (_op.Index >= buffer.Length)
		{
			return false;
		}
		if (_op.Stack.count < 0)
		{
			return false;
		}
		buffer[_op.Index] = _op.Stack.Clone();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ProcessSetRelative(InventoryOperation _op)
	{
		if (buffer == null || _op.Operation != InventoryOperation.EnumOperation.SetRelative)
		{
			return false;
		}
		if (_op.Index >= buffer.Length)
		{
			return false;
		}
		ItemStack itemStack = buffer[_op.Index];
		if (_op.Stack.count > 0)
		{
			if (!_op.Stack.CanStackWith(itemStack))
			{
				return false;
			}
			buffer[_op.Index].count = _op.Stack.count + itemStack.count;
		}
		else
		{
			if (!_op.Stack.itemValue.Equals(itemStack.itemValue))
			{
				return false;
			}
			int num = _op.Stack.count + itemStack.count;
			if (num == 0)
			{
				buffer[_op.Index] = ItemStack.Empty;
			}
			else
			{
				if (num <= 0)
				{
					return false;
				}
				buffer[_op.Index].count = _op.Stack.count + itemStack.count;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ProcessSetAll(InventoryOperation _op)
	{
		if (buffer == null || _op.NewStacks == null || _op.Operation != InventoryOperation.EnumOperation.SetAll)
		{
			return false;
		}
		if (_op.NewStacks.Length != buffer.Length)
		{
			return false;
		}
		for (int i = 0; i < buffer.Length; i++)
		{
			if (_op.Stack.count < 0)
			{
				return false;
			}
			buffer[i] = _op.NewStacks[i].Clone();
		}
		return true;
	}

	public bool CanLockOnServer(int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		return true;
	}

	public bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return true;
	}

	public void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
	}

	public void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		throw new NotImplementedException("[TransactionalInventory] This class is designed to be locked with the custom UI action RequestInventoryAndShowUI");
	}

	public void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
	}

	public void RequestInventoryAndShowUI(bool _success, Action _showUi)
	{
		if (_success)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				DataReadyLocally = false;
				inventoryRequestComplete = _showUi;
				InventoryManager.Instance.RequestInventoryFromServer(KeyHash);
			}
			else
			{
				_showUi();
			}
		}
	}
}

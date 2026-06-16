using System;
using System.Collections.Generic;
using System.IO;

public class InventoryTransaction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class OperationData
	{
		public int InitialHash;

		public int FinalHash;

		public List<InventoryOperation> Ops;

		public OperationData(int _initialHash)
		{
			InitialHash = _initialHash;
			FinalHash = 0;
			Ops = new List<InventoryOperation>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<TransactionalInventory, OperationData> InventoryOps = new Dictionary<TransactionalInventory, OperationData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddOperation(TransactionalInventory _inventory, InventoryOperation _operation)
	{
		if (!InventoryOps.ContainsKey(_inventory))
		{
			InventoryOps.Add(_inventory, new OperationData(_inventory.Hash));
		}
		InventoryOps[_inventory].Ops.Add(_operation);
	}

	public InventoryTransaction SetStackAbsolute(TransactionalInventory _inventory, ItemStack _stack, int _index)
	{
		if (_inventory == null)
		{
			Log.Warning("[InventoryTransaction] supplied inventory as null.");
			return this;
		}
		if (_stack.count < 0)
		{
			Log.Warning("[InventoryTransaction] stack count cannot be negative when using SetStackAbsolute.");
			return this;
		}
		AddOperation(_inventory, InventoryOperation.CreateSetAbsolute(_stack, _index));
		return this;
	}

	public InventoryTransaction SetStackRelative(TransactionalInventory _inventory, ItemStack _stack, int _index)
	{
		if (_inventory == null)
		{
			Log.Warning("[InventoryTransaction] supplied inventory as null.");
			return this;
		}
		AddOperation(_inventory, InventoryOperation.CreateSetRelative(_stack, _index));
		return this;
	}

	public InventoryTransaction SetStacksAll(TransactionalInventory _inventory, ItemStack[] _newStacks)
	{
		if (_inventory == null)
		{
			Log.Warning("[InventoryTransaction] supplied inventory as null.");
			return this;
		}
		AddOperation(_inventory, InventoryOperation.CreateSetAll(_newStacks));
		return this;
	}

	public bool Apply(Guid managerToken)
	{
		if (InventoryOps.Count == 0)
		{
			return false;
		}
		bool flag = true;
		foreach (KeyValuePair<TransactionalInventory, OperationData> inventoryOp in InventoryOps)
		{
			TransactionalInventory key = inventoryOp.Key;
			OperationData value = inventoryOp.Value;
			if (key.Hash != value.InitialHash)
			{
				Log.Warning("[InventoryTransaction] transaction failed, an initial hash was out of date.");
				flag = false;
				break;
			}
			key.StartTransaction(managerToken);
		}
		if (flag)
		{
			foreach (KeyValuePair<TransactionalInventory, OperationData> inventoryOp2 in InventoryOps)
			{
				TransactionalInventory key2 = inventoryOp2.Key;
				foreach (InventoryOperation op in inventoryOp2.Value.Ops)
				{
					if (!key2.ProcessOperation(op, managerToken))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
		}
		foreach (KeyValuePair<TransactionalInventory, OperationData> inventoryOp3 in InventoryOps)
		{
			TransactionalInventory key3 = inventoryOp3.Key;
			OperationData value2 = inventoryOp3.Value;
			key3.FinalizeTransaction(flag, managerToken);
			if (flag)
			{
				value2.FinalHash = key3.Hash;
			}
		}
		return flag;
	}

	public bool ValidateFinalHashes()
	{
		bool result = true;
		foreach (KeyValuePair<TransactionalInventory, OperationData> inventoryOp in InventoryOps)
		{
			TransactionalInventory key = inventoryOp.Key;
			OperationData value = inventoryOp.Value;
			if (value.FinalHash != key.Hash)
			{
				result = false;
				Log.Error(string.Format("[{0}] transaction failed, final hash was mismatched. Expected: {1}, Acutal: {2}.", "InventoryTransaction", key.Hash, value.FinalHash));
			}
		}
		return result;
	}

	public static InventoryTransaction Read(BinaryReader _br)
	{
		InventoryTransaction inventoryTransaction = new InventoryTransaction();
		bool flag = false;
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Guid guid = StreamUtils.ReadGuid(_br);
			int initialHash = _br.ReadInt32();
			int finalHash = _br.ReadInt32();
			OperationData operationData = new OperationData(initialHash)
			{
				FinalHash = finalHash
			};
			int num2 = _br.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				operationData.Ops.Add(InventoryOperation.Read(_br));
			}
			if (!InventoryManager.Instance.TryGetTransactionalInventory(guid, out var _inventory))
			{
				Log.Error(string.Format("[{0}] Could not find inventory with key {1}", "InventoryTransaction", guid));
				flag = true;
			}
			else
			{
				inventoryTransaction.InventoryOps[_inventory] = operationData;
			}
		}
		if (flag)
		{
			inventoryTransaction.InventoryOps.Clear();
		}
		return inventoryTransaction;
	}

	public static void Write(BinaryWriter _bw, InventoryTransaction tx)
	{
		if (tx == null || tx.InventoryOps == null || tx.InventoryOps.Count == 0)
		{
			_bw.Write(0);
			return;
		}
		_bw.Write(tx.InventoryOps.Count);
		foreach (KeyValuePair<TransactionalInventory, OperationData> inventoryOp in tx.InventoryOps)
		{
			TransactionalInventory key = inventoryOp.Key;
			OperationData value = inventoryOp.Value;
			StreamUtils.Write(_bw, key.Key);
			_bw.Write(value.InitialHash);
			_bw.Write(value.FinalHash);
			_bw.Write(value.Ops.Count);
			foreach (InventoryOperation op in value.Ops)
			{
				op.Write(_bw);
			}
		}
	}
}

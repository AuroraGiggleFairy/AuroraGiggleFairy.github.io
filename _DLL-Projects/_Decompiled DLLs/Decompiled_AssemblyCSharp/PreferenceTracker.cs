using System;

public class PreferenceTracker
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int PlayerID
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] toolbelt
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] bag
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ItemValue[] equipment
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool AnyPreferences
	{
		get
		{
			if (toolbelt == null && bag == null)
			{
				return equipment != null;
			}
			return true;
		}
	}

	public PreferenceTracker(int playerId)
	{
		PlayerID = playerId;
	}

	public void SetToolbelt(ItemStack[] _itemStacks, Predicate<ItemStack> _includeCondition)
	{
		if (_itemStacks == null || _itemStacks.Length == 0)
		{
			return;
		}
		toolbelt = new ItemStack[_itemStacks.Length];
		for (int i = 0; i < toolbelt.Length; i++)
		{
			if (_includeCondition(_itemStacks[i]))
			{
				toolbelt[i] = _itemStacks[i].Clone();
			}
			else
			{
				toolbelt[i] = new ItemStack();
			}
		}
	}

	public void SetBag(ItemStack[] _itemStacks, Predicate<ItemStack> _includeCondition)
	{
		if (_itemStacks == null || _itemStacks.Length == 0)
		{
			return;
		}
		bag = new ItemStack[_itemStacks.Length];
		for (int i = 0; i < bag.Length; i++)
		{
			if (_includeCondition(_itemStacks[i]))
			{
				bag[i] = _itemStacks[i].Clone();
			}
			else
			{
				bag[i] = new ItemStack();
			}
		}
	}

	public void SetEquipment(ItemValue[] _itemValues, Predicate<ItemValue> _includeCondition)
	{
		if (_itemValues == null && _itemValues.Length != 0)
		{
			return;
		}
		equipment = new ItemValue[_itemValues.Length];
		for (int i = 0; i < equipment.Length; i++)
		{
			if (_includeCondition(_itemValues[i]))
			{
				equipment[i] = _itemValues[i].Clone();
			}
			else
			{
				equipment[i] = new ItemValue();
			}
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write(PlayerID);
		bool flag = toolbelt != null && toolbelt.Length != 0;
		_bw.Write(flag);
		if (flag)
		{
			GameUtils.WriteItemStack(_bw, toolbelt);
		}
		bool flag2 = equipment != null && equipment.Length != 0;
		_bw.Write(flag2);
		if (flag2)
		{
			GameUtils.WriteItemValueArray(_bw, equipment);
		}
		bool flag3 = bag != null && bag.Length != 0;
		_bw.Write(flag3);
		if (flag3)
		{
			GameUtils.WriteItemStack(_bw, bag);
		}
	}

	public void Read(PooledBinaryReader _br)
	{
		PlayerID = _br.ReadInt32();
		if (_br.ReadBoolean())
		{
			toolbelt = GameUtils.ReadItemStack(_br);
		}
		if (_br.ReadBoolean())
		{
			equipment = GameUtils.ReadItemValueArray(_br);
		}
		if (_br.ReadBoolean())
		{
			bag = GameUtils.ReadItemStack(_br);
		}
	}
}

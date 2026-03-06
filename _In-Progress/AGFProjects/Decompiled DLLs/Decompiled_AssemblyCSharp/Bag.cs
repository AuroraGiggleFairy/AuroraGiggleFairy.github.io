using System.Collections.Generic;
using UnityEngine;

public class Bag : IInventory
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;

	public int MaxItemCount = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;

	public int SlotCount => GetSlots().Length;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PackedBoolArray LockedSlots { get; set; }

	public event XUiEvent_BackpackItemsChangedInternal OnBackpackItemsChangedInternal;

	public Bag(EntityAlive _entity)
	{
		entity = _entity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkBagAssigned(int slotCount = 45)
	{
		if (items == null)
		{
			items = ItemStack.CreateArray((int)EffectManager.GetValue(PassiveEffects.BagSize, null, slotCount, entity, null, default(FastTags<TagGroup.Global>), calcEquipment: false, calcHoldingItem: false));
		}
	}

	public void Clear()
	{
		ItemStack[] array = items;
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Clear();
			}
		}
		onBackpackChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onBackpackChanged()
	{
		if (this.OnBackpackItemsChangedInternal != null)
		{
			this.OnBackpackItemsChangedInternal();
		}
	}

	public bool CanStackNoEmpty(ItemStack _itemStack)
	{
		ItemStack[] slots = GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].CanStackPartlyWith(_itemStack))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEmpty()
	{
		ItemStack[] slots = GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (!slots[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public bool CanStack(ItemStack _itemStack)
	{
		ItemStack[] slots = GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].IsEmpty() || slots[i].CanStackWith(_itemStack))
			{
				return true;
			}
		}
		return false;
	}

	public (bool anyMoved, bool allMoved) TryStackItem(int startIndex, ItemStack _itemStack)
	{
		ItemStack[] slots = GetSlots();
		int num = 0;
		bool item = false;
		for (int i = startIndex; i < slots.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == slots[i].itemValue.type && slots[i].CanStackPartly(ref num))
			{
				slots[i].count += num;
				_itemStack.count -= num;
				onBackpackChanged();
				item = true;
				if (_itemStack.count == 0)
				{
					return (anyMoved: true, allMoved: true);
				}
			}
		}
		return (anyMoved: item, allMoved: false);
	}

	public bool CanTakeItem(ItemStack _itemStack)
	{
		ItemStack[] slots = GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].CanStackPartlyWith(_itemStack))
			{
				return true;
			}
			if (slots[i].IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public ItemStack[] GetSlots()
	{
		checkBagAssigned();
		return items;
	}

	public void SetupSlots(ItemStack[] _slots)
	{
		checkBagAssigned(_slots.Length);
		items = _slots;
		onBackpackChanged();
	}

	public void SetSlots(ItemStack[] _slots)
	{
		checkBagAssigned();
		items = _slots;
		onBackpackChanged();
	}

	public void SetSlot(int index, ItemStack _stack, bool callChangedEvent = true)
	{
		if (index < items.Length)
		{
			items[index] = _stack;
			if (callChangedEvent)
			{
				onBackpackChanged();
			}
		}
	}

	public void OnUpdate()
	{
		if (entity is EntityPlayer)
		{
			MaxItemCount = (int)EffectManager.GetValue(PassiveEffects.CarryCapacity, null, 0f, entity);
			entity.Buffs.SetCustomVar("_carrycapacity", MaxItemCount);
			entity.Buffs.SetCustomVar("_encumbrance", Mathf.Max(GetUsedSlotCount() - MaxItemCount, 0f) / (float)(items.Length - MaxItemCount));
			entity.Buffs.SetCustomVar("_encumberedslots", Mathf.Max(GetUsedSlotCount() - MaxItemCount, 0f));
		}
	}

	public bool AddItem(ItemStack _itemStack)
	{
		if (items == null)
		{
			items = ItemStack.CreateArray((int)EffectManager.GetValue(PassiveEffects.BagSize, null, 45f, entity, null, default(FastTags<TagGroup.Global>), calcEquipment: false, calcHoldingItem: false));
		}
		bool num = ItemStack.AddToItemStackArray(items, _itemStack) >= 0;
		if (num)
		{
			onBackpackChanged();
		}
		return num;
	}

	public int DecItem(ItemValue _itemValue, int _count, bool _ignoreModdedItems = false, IList<ItemStack> _removedItems = null)
	{
		int num = _count;
		ItemStack[] slots = GetSlots();
		int num2 = 0;
		while (_count > 0 && num2 < GetSlots().Length)
		{
			if (slots[num2].itemValue.type == _itemValue.type && (!_ignoreModdedItems || !slots[num2].itemValue.HasModSlots || !slots[num2].itemValue.HasMods()))
			{
				if (ItemClass.GetForId(slots[num2].itemValue.type).CanStack())
				{
					int count = slots[num2].count;
					int num3 = ((count >= _count) ? _count : count);
					_removedItems?.Add(new ItemStack(slots[num2].itemValue.Clone(), num3));
					slots[num2].count -= num3;
					_count -= num3;
					if (slots[num2].count <= 0)
					{
						slots[num2].Clear();
					}
				}
				else
				{
					_removedItems?.Add(slots[num2].Clone());
					slots[num2].Clear();
					_count--;
				}
			}
			num2++;
		}
		SetSlots(slots);
		return num - _count;
	}

	public int GetUsedSlotCount()
	{
		ItemStack[] slots = GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if (!slots[i].IsEmpty())
			{
				num++;
			}
		}
		return num;
	}

	public int GetItemCount(ItemValue _itemValue, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		ItemStack[] slots = GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !slots[i].itemValue.HasModSlots || !slots[i].itemValue.HasMods()) && slots[i].itemValue.type == _itemValue.type && (_seed == -1 || _seed == slots[i].itemValue.Seed) && (_meta == -1 || _meta == slots[i].itemValue.Meta))
			{
				num += slots[i].count;
			}
		}
		return num;
	}

	public int GetItemCount(FastTags<TagGroup.Global> itemTags, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		ItemStack[] slots = GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !slots[i].itemValue.HasModSlots || !slots[i].itemValue.HasMods()) && !slots[i].itemValue.IsEmpty() && slots[i].itemValue.ItemClass.ItemTags.Test_AnySet(itemTags) && (_seed == -1 || _seed == slots[i].itemValue.Seed) && (_meta == -1 || _meta == slots[i].itemValue.Meta))
			{
				num += slots[i].count;
			}
		}
		return num;
	}

	public bool HasItem(ItemValue _item)
	{
		return GetItemCount(_item) > 0;
	}

	public ItemStack[] CloneItemStack()
	{
		checkBagAssigned();
		ItemStack[] array = new ItemStack[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			array[i] = items[i].Clone();
		}
		return array;
	}
}

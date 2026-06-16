using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using UnityEngine;

public class XUiM_PlayerInventory : XUiModel, IInventory
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Bag backpack;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Inventory toolbelt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ItemValue currencyItem;

	public Bag Backpack => backpack;

	public Inventory Toolbelt => toolbelt;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int CurrencyAmount
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int QuickSwapSlot => toolbelt.GetBestQuickSwapSlot();

	public event XUiEvent_BackpackItemsChanged OnBackpackItemsChanged;

	public event XUiEvent_ToolbeltItemsChanged OnToolbeltItemsChanged;

	public event XUiEvent_CurrencyChanged OnCurrencyChanged;

	public XUiM_PlayerInventory(XUi _xui, EntityPlayerLocal _player)
	{
		if (!(_player == null))
		{
			xui = _xui;
			localPlayer = _player;
			backpack = localPlayer.bag;
			toolbelt = localPlayer.inventory;
			backpack.OnBackpackItemsChangedInternal += dispatchBackpackItemsChanged;
			toolbelt.OnToolbeltItemsChangedInternal += dispatchToolbeltItemsChanged;
			localPlayer.PlayerUI.OnUIShutdown += HandleUIShutdown;
			currencyItem = ItemClass.GetItem(TraderInfo.CurrencyItem);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dispatchBackpackItemsChanged()
	{
		this.OnBackpackItemsChanged?.Invoke();
		RefreshCurrency();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dispatchToolbeltItemsChanged()
	{
		this.OnToolbeltItemsChanged?.Invoke();
		RefreshCurrency();
	}

	public bool AddItemToPreferredToolbeltSlot(ItemStack _itemStack, int _slot)
	{
		return toolbelt.AddItemAtSlot(_itemStack, _slot);
	}

	public bool AddItemNoPartial(ItemStack _itemStack, bool _playCollectSound = true)
	{
		if (!backpack.CanStack(_itemStack) && !toolbelt.CanStack(_itemStack))
		{
			return false;
		}
		return AddItem(_itemStack, _playCollectSound);
	}

	public bool CanSwapItems(ItemStack _removedStack, ItemStack _addedStack, int _slotNumber = -1)
	{
		List<ItemStack> allItemStacks = GetAllItemStacks();
		int num = _removedStack.count;
		int num2 = _addedStack.count;
		_ = _removedStack.itemValue.ItemClass.Stacknumber.Value;
		int value = _addedStack.itemValue.ItemClass.Stacknumber.Value;
		for (int i = 0; i < allItemStacks.Count - 1; i++)
		{
			if (num > 0 && allItemStacks[i].itemValue.type == _removedStack.itemValue.type && (_slotNumber == -1 || _slotNumber == i))
			{
				int count = allItemStacks[i].count;
				if (count > num)
				{
					num = 0;
				}
				else
				{
					num -= count;
					count = 0;
					num2 -= value;
				}
			}
			else if (num2 > 0 && allItemStacks[i].itemValue.type == _addedStack.itemValue.type)
			{
				int num3 = value - allItemStacks[i].count;
				num2 = ((num3 < num2) ? (num2 - num3) : 0);
			}
			else if (allItemStacks[i].IsEmpty())
			{
				num2 -= value;
			}
			if (num <= 0 && num2 <= 0)
			{
				return true;
			}
		}
		return false;
	}

	public void SortStacks(int _ignoreSlots = 0, PackedBoolArray _ignoredSlots = null)
	{
		if (EffectManager.GetValue(PassiveEffects.ShuffledBackpack, null, 0f, localPlayer) == 0f)
		{
			ItemStack[] slots = StackSortUtil.CombineAndSortStacks(GetBackpackItemStacks(), _ignoreSlots, _ignoredSlots);
			backpack.SetSlots(slots);
		}
	}

	public bool AddItem(ItemStack _itemStack, bool _playCollectSound)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.ToolBelt))
		{
			return false;
		}
		bool flag = false;
		ItemStack itemStack = _itemStack.Clone();
		if (_itemStack.itemValue.ItemClass is ItemClassArmor { AutoEquip: not false } itemClassArmor)
		{
			Equipment equipment = xui.playerUI.entityPlayer.equipment;
			int equipSlot = (int)itemClassArmor.EquipSlot;
			if (itemClassArmor.AllowUnEquip)
			{
				ItemValue slotItem = equipment.GetSlotItem(equipSlot);
				equipment.SetSlotItem(equipSlot, itemStack.itemValue);
				QuestEventManager.Current.WoreItem(itemStack.itemValue);
				itemStack.count = 0;
				itemStack = new ItemStack(slotItem, 1);
			}
			else
			{
				equipment.SetSlotItem(equipSlot, itemStack.itemValue);
				QuestEventManager.Current.WoreItem(itemStack.itemValue);
				flag = true;
				itemStack.count = 0;
			}
			xui.PlayerEquipment.RefreshEquipment();
		}
		if (!flag)
		{
			if (backpack.CanStackNoEmpty(itemStack))
			{
				flag = backpack.TryStackItem(0, itemStack).allMoved;
			}
			else if (toolbelt.CanStackNoEmpty(itemStack))
			{
				flag = toolbelt.TryStackItem(0, itemStack);
			}
		}
		if (!flag)
		{
			if (backpack.CanStack(itemStack))
			{
				flag = backpack.TryStackItem(0, itemStack).allMoved;
			}
			else if (toolbelt.CanStack(itemStack))
			{
				flag = toolbelt.TryStackItem(0, itemStack);
			}
		}
		if (!flag)
		{
			ItemClass itemClass = itemStack.itemValue.ItemClass;
			int num = 1;
			if (itemClass?.Stacknumber != null)
			{
				num = itemClass.Stacknumber.Value;
			}
			if (itemStack.count > num)
			{
				for (int num2 = itemStack.count; num2 > 0; num2 -= num)
				{
					bool flag2 = false;
					int val = Math.Min(num2, num);
					val = Math.Max(0, val);
					ItemStack itemStack2 = itemStack.Clone();
					itemStack2.count = val;
					if (itemStack.itemValue.ItemClass is ItemClassArmor itemClassArmor2 && !xui.PlayerEquipment.IsEquipmentTypeWorn(itemClassArmor2.EquipSlot) && localPlayer.equipment.ReturnItem(itemStack2))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
						xui.PlayerEquipment.RefreshEquipment();
					}
					else if (toolbelt.ReturnItem(itemStack2))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
					}
					else if (backpack.AddItem(itemStack2))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
					}
					else if (toolbelt.AddItem(itemStack2))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
					}
					if (!flag2)
					{
						if (_itemStack.count != itemStack.count)
						{
							xui.CollectedItemList?.AddItemStack(new ItemStack(itemStack.itemValue, _itemStack.count - itemStack.count));
							if (_playCollectSound)
							{
								Manager.PlayInsidePlayerHead("item_pickup");
							}
							_itemStack.count = itemStack.count;
						}
						return false;
					}
				}
			}
			else if (itemStack.itemValue.ItemClass is ItemClassArmor itemClassArmor3 && !xui.PlayerEquipment.IsEquipmentTypeWorn(itemClassArmor3.EquipSlot) && localPlayer.equipment.ReturnItem(itemStack))
			{
				flag = true;
				itemStack = itemStack.Clone();
				itemStack.count = 0;
				xui.PlayerEquipment.RefreshEquipment();
			}
			else if (toolbelt.ReturnItem(itemStack))
			{
				flag = true;
				itemStack = itemStack.Clone();
				itemStack.count = 0;
				dispatchToolbeltItemsChanged();
			}
			else if (backpack.AddItem(itemStack))
			{
				flag = true;
				itemStack = itemStack.Clone();
				itemStack.count = 0;
				dispatchBackpackItemsChanged();
			}
			else if (toolbelt.AddItem(itemStack))
			{
				flag = true;
				itemStack = itemStack.Clone();
				itemStack.count = 0;
				dispatchToolbeltItemsChanged();
			}
		}
		if (itemStack.count != _itemStack.count)
		{
			ItemStack itemStack3 = new ItemStack(itemStack.itemValue, _itemStack.count - itemStack.count);
			QuestEventManager.Current.ItemAdded(itemStack3);
			xui.CollectedItemList?.AddItemStack(itemStack3);
			if (_playCollectSound)
			{
				Manager.PlayInsidePlayerHead("item_pickup");
			}
		}
		if (itemStack.count == 0)
		{
			itemStack = ItemStack.Empty;
		}
		_itemStack.count = itemStack.count;
		return flag;
	}

	public int CountAvailableSpaceForItem(ItemValue _itemValue, bool _limitToOneStack = true)
	{
		ItemStack itemStack = new ItemStack(_itemValue, 1);
		int value = _itemValue.ItemClass.Stacknumber.Value;
		int num = 0;
		ItemStack[] slots = backpack.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].IsEmpty())
			{
				num += value;
			}
			else if (itemStack.CanStackWith(slots[i]))
			{
				num += value - slots[i].count;
			}
			if (_limitToOneStack && num >= value)
			{
				return value;
			}
		}
		ItemStack[] slots2 = toolbelt.GetSlots();
		for (int j = 0; j < toolbelt.PUBLIC_SLOTS; j++)
		{
			if (slots2[j].IsEmpty())
			{
				num += value;
			}
			else if (itemStack.CanStackWith(slots2[j]))
			{
				num += value - slots2[j].count;
			}
			if (_limitToOneStack && num > value)
			{
				return value;
			}
		}
		return num;
	}

	public void DropItem(ItemStack _stack)
	{
		GameManager instance = GameManager.Instance;
		if ((bool)instance)
		{
			instance.ItemDropServer(_stack, localPlayer.GetDropPosition(), Vector3.zero, localPlayer.entityId);
			Manager.BroadcastPlay("itemdropped");
		}
		xui.CollectedItemList?.RemoveItemStack(_stack);
	}

	public bool AddItems(ItemStack[] _itemStacks)
	{
		bool flag = true;
		for (int i = 0; i < _itemStacks.Length; i++)
		{
			flag &= AddItem(_itemStacks[i]);
		}
		return flag;
	}

	public bool AddItemsUsingPreferenceTracker(XUiC_ItemStackGrid _srcGrid, PreferenceTracker _preferences)
	{
		if (!_preferences.AnyPreferences)
		{
			return false;
		}
		if (localPlayer.entityId != _preferences.PlayerID)
		{
			return false;
		}
		bool flag = false;
		XUiC_ItemStack[] itemStackControllers = _srcGrid.GetItemStackControllers();
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < itemStackControllers.Length; i++)
		{
			(bool anyMoved, bool allMoved) tuple = TryStackItem(0, itemStackControllers[i].ItemStack);
			bool item = tuple.anyMoved;
			bool item2 = tuple.allMoved;
			flag = flag || item;
			if (item)
			{
				if (item2)
				{
					itemStackControllers[i].ItemStack = ItemStack.Empty;
				}
				else
				{
					hashSet.Add(itemStackControllers[i].ItemStack.itemValue.type);
				}
				_srcGrid.HandleSlotChangedEvent(i, itemStackControllers[i].ItemStack);
			}
		}
		if (_preferences.toolbelt != null)
		{
			ItemStack[] slots = toolbelt.GetSlots();
			int j;
			for (j = 0; j < _preferences.toolbelt.Length && j < slots.Length; j++)
			{
				if (slots[j].IsEmpty())
				{
					int type = _preferences.toolbelt[j].itemValue.type;
					XUiC_ItemStack xUiC_ItemStack = ((!hashSet.Contains(type)) ? itemStackControllers.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (XUiC_ItemStack _stack) => _stack.ItemStack.Equals(_preferences.toolbelt[j])) : itemStackControllers.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (XUiC_ItemStack _stack) => _stack.ItemStack.itemValue.type == type));
					if (xUiC_ItemStack?.ItemStack != null && !xUiC_ItemStack.ItemStack.IsEmpty())
					{
						toolbelt.SetItem(j, xUiC_ItemStack.ItemStack);
						xUiC_ItemStack.ItemStack = ItemStack.Empty;
						_srcGrid.HandleSlotChangedEvent(xUiC_ItemStack.SlotNumber, ItemStack.Empty);
						flag = true;
					}
				}
			}
		}
		if (_preferences.equipment != null)
		{
			int num = Utils.FastMin(localPlayer.equipment.GetSlotCount(), _preferences.equipment.Length);
			int i2;
			for (i2 = 0; i2 < num; i2++)
			{
				if (localPlayer.equipment.GetSlotItem(i2) != null)
				{
					continue;
				}
				XUiC_ItemStack xUiC_ItemStack2 = itemStackControllers.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (XUiC_ItemStack _stack) => _stack.ItemStack.itemValue.Equals(_preferences.equipment[i2]));
				if (xUiC_ItemStack2?.ItemStack != null && !xUiC_ItemStack2.ItemStack.IsEmpty())
				{
					localPlayer.equipment.SetSlotItem(i2, xUiC_ItemStack2.ItemStack.itemValue);
					xUiC_ItemStack2.ItemStack.count--;
					if (xUiC_ItemStack2.ItemStack.IsEmpty())
					{
						xUiC_ItemStack2.ItemStack = ItemStack.Empty;
						_srcGrid.HandleSlotChangedEvent(xUiC_ItemStack2.SlotNumber, ItemStack.Empty);
					}
					flag = true;
				}
			}
		}
		xui.PlayerEquipment.RefreshEquipment();
		if (_preferences.bag != null)
		{
			ItemStack[] slots2 = backpack.GetSlots();
			int i3;
			for (i3 = 0; i3 < _preferences.bag.Length && i3 < slots2.Length; i3++)
			{
				if (slots2[i3].IsEmpty())
				{
					int type2 = _preferences.bag[i3].itemValue.type;
					XUiC_ItemStack xUiC_ItemStack3 = ((!hashSet.Contains(type2)) ? itemStackControllers.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (XUiC_ItemStack _stack) => _stack.ItemStack.Equals(_preferences.bag[i3])) : itemStackControllers.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (XUiC_ItemStack _stack) => _stack.ItemStack.itemValue.type == type2));
					if (xUiC_ItemStack3?.ItemStack != null && !xUiC_ItemStack3.ItemStack.IsEmpty())
					{
						backpack.SetSlot(i3, xUiC_ItemStack3.ItemStack);
						xUiC_ItemStack3.ItemStack = ItemStack.Empty;
						_srcGrid.HandleSlotChangedEvent(xUiC_ItemStack3.SlotNumber, ItemStack.Empty);
						flag = true;
					}
				}
			}
		}
		return flag;
	}

	public bool AddItemToBackpack(ItemStack _itemStack)
	{
		backpack.TryStackItem(0, _itemStack);
		if (_itemStack.count > 0 && backpack.AddItem(_itemStack))
		{
			return true;
		}
		return false;
	}

	public bool AddItemToToolbelt(ItemStack _itemStack)
	{
		toolbelt.TryStackItem(0, _itemStack);
		if (_itemStack.count > 0 && toolbelt.AddItem(_itemStack))
		{
			return true;
		}
		return false;
	}

	public bool HasItem(ItemStack _itemStack)
	{
		return HasItems(new ItemStack[1] { _itemStack });
	}

	public bool HasItems(IList<ItemStack> _itemStacks, int _multiplier = 1)
	{
		for (int i = 0; i < _itemStacks.Count; i++)
		{
			int num = _itemStacks[i].count * _multiplier;
			num -= backpack.GetItemCount(_itemStacks[i].itemValue);
			if (num > 0)
			{
				num -= toolbelt.GetItemCount(_itemStacks[i].itemValue);
			}
			if (num > 0)
			{
				return false;
			}
		}
		return true;
	}

	public void RemoveItem(ItemStack _itemStack)
	{
		RemoveItems(new ItemStack[1] { _itemStack });
	}

	public void RemoveItems(IList<ItemStack> _itemStacks, int _multiplier = 1, IList<ItemStack> _removedItems = null)
	{
		if (!HasItems(_itemStacks))
		{
			return;
		}
		for (int i = 0; i < _itemStacks.Count; i++)
		{
			int num = _itemStacks[i].count * _multiplier;
			num -= backpack.DecItem(_itemStacks[i].itemValue, num, _ignoreModdedItems: true, _removedItems);
			if (num > 0)
			{
				toolbelt.DecItem(_itemStacks[i].itemValue, num, _ignoreModdedItems: true, _removedItems);
			}
		}
		dispatchBackpackItemsChanged();
		dispatchToolbeltItemsChanged();
	}

	public int GetItemCount(ItemValue _itemValue)
	{
		return 0 + backpack.GetItemCount(_itemValue) + toolbelt.GetItemCount(_itemValue);
	}

	public int GetItemCountWithMods(ItemValue _itemValue)
	{
		return 0 + backpack.GetItemCount(_itemValue, -1, -1, _ignoreModdedItems: false) + toolbelt.GetItemCount(_itemValue, _bConsiderTexture: false, -1, -1, _ignoreModdedItems: false);
	}

	public int GetItemCount(int _itemId)
	{
		ItemValue itemValue = new ItemValue(_itemId);
		return 0 + backpack.GetItemCount(itemValue) + toolbelt.GetItemCount(itemValue);
	}

	public List<ItemStack> GetAllItemStacks()
	{
		List<ItemStack> list = new List<ItemStack>();
		list.AddRange(GetBackpackItemStacks());
		list.AddRange(GetToolbeltItemStacks());
		return list;
	}

	public ItemStack[] GetBackpackItemStacks()
	{
		return backpack.GetSlots();
	}

	public ItemStack[] GetToolbeltItemStacks()
	{
		return toolbelt.GetSlots();
	}

	public void SetBackpackItemStacks(ItemStack[] _itemStacks)
	{
		backpack.SetSlots(_itemStacks);
		dispatchBackpackItemsChanged();
	}

	public void SetToolbeltItemStacks(ItemStack[] _itemStacks)
	{
		toolbelt.SetSlots(_itemStacks, _allowSettingDummySlot: false);
		dispatchToolbeltItemsChanged();
	}

	public void RefreshCurrency()
	{
		int itemCount = GetItemCount(currencyItem);
		if (itemCount != CurrencyAmount)
		{
			CurrencyAmount = itemCount;
			this.OnCurrencyChanged?.Invoke();
		}
	}

	public void HandleUIShutdown()
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(localPlayer);
		if (uIForPlayer != null)
		{
			uIForPlayer.OnUIShutdown -= HandleUIShutdown;
		}
		backpack.OnBackpackItemsChangedInternal -= dispatchBackpackItemsChanged;
		toolbelt.OnToolbeltItemsChangedInternal -= dispatchToolbeltItemsChanged;
	}

	public bool AddItem(ItemStack _itemStack)
	{
		return AddItem(_itemStack, _playCollectSound: true);
	}

	public (bool anyMoved, bool allMoved) TryStackItem(int _startIndex, ItemStack _itemStack)
	{
		int count = _itemStack.count;
		bool flag = toolbelt.TryStackItem(_startIndex, _itemStack);
		if (!flag)
		{
			flag = backpack.TryStackItem(_startIndex, _itemStack).allMoved;
		}
		if (count != _itemStack.count)
		{
			ItemStack itemStack = new ItemStack(_itemStack.itemValue, count - _itemStack.count);
			QuestEventManager.Current.ItemAdded(itemStack);
			xui.CollectedItemList?.AddItemStack(itemStack);
			Manager.PlayInsidePlayerHead("item_pickup");
		}
		return (anyMoved: count != _itemStack.count, allMoved: flag);
	}

	public bool HasItem(ItemValue _item)
	{
		return GetItemCount(_item) > 0;
	}

	public static bool TryStackItem(int _startIndex, ItemStack _itemStack, ItemStack[] _items)
	{
		for (int i = _startIndex; i < _items.Length; i++)
		{
			int _count = _itemStack.count;
			if (_itemStack.itemValue.type == _items[i].itemValue.type && _items[i].CanStackPartly(ref _count))
			{
				_items[i].count += _count;
				_itemStack.count -= _count;
				if (_itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}
}

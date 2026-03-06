using UnityEngine;

public class XUiM_LootContainer : XUiModel
{
	public enum EItemMoveKind
	{
		All,
		FillOnly,
		FillAndCreate,
		FillOnlyFirstCreateSecond
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastStashTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SecondClickMaxDelaySec = 2f;

	public static bool AddItem(ItemStack _itemStack, XUi _xui)
	{
		if (_xui.lootContainer == null)
		{
			return false;
		}
		_xui.lootContainer.TryStackItem(0, _itemStack);
		if (_itemStack.count > 0 && _xui.lootContainer.AddItem(_itemStack))
		{
			return true;
		}
		return false;
	}

	public static bool TakeAll(XUi _xui)
	{
		XUiM_PlayerInventory playerInventory = _xui.PlayerInventory;
		ItemStack[] items = _xui.lootContainer.items;
		bool result = true;
		for (int i = 0; i < items.Length; i++)
		{
			if (!items[i].IsEmpty())
			{
				ItemStack itemStack = items[i].Clone();
				if (!playerInventory.AddItem(itemStack))
				{
					playerInventory.DropItem(itemStack);
					result = false;
				}
				_xui.lootContainer.UpdateSlot(i, ItemStack.Empty.Clone());
			}
		}
		return result;
	}

	public static (bool _allMoved, bool _anyMoved) StashItems(XUiController _srcWindow, XUiC_ItemStackGrid _srcGrid, IInventory _dstInventory, int _ignoreSlots, PackedBoolArray _ignoredSlots, EItemMoveKind _moveKind, bool _startBottomRight)
	{
		if (_srcGrid == null || _dstInventory == null)
		{
			return (_allMoved: false, _anyMoved: false);
		}
		float unscaledTime = Time.unscaledTime;
		if (_moveKind == EItemMoveKind.FillOnlyFirstCreateSecond && unscaledTime - lastStashTime < 2f)
		{
			_moveKind = EItemMoveKind.FillAndCreate;
		}
		bool item = true;
		bool item2 = false;
		PreferenceTracker preferenceTracker = null;
		if (_srcWindow is XUiC_LootWindow xUiC_LootWindow)
		{
			preferenceTracker = xUiC_LootWindow.GetPreferenceTrackerFromTileEntity();
		}
		if (preferenceTracker != null && preferenceTracker.AnyPreferences && _dstInventory is XUiM_PlayerInventory xUiM_PlayerInventory)
		{
			item2 = xUiM_PlayerInventory.AddItemsUsingPreferenceTracker(_srcGrid, preferenceTracker);
		}
		XUiController[] itemStackControllers = _srcGrid.GetItemStackControllers();
		XUiController[] array = itemStackControllers;
		int num = (_startBottomRight ? (array.Length - 1) : 0);
		while (_startBottomRight ? (num >= 0) : (num < array.Length))
		{
			if (!StackSortUtil.IsIgnoredSlot(_ignoreSlots, _ignoredSlots, num))
			{
				XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)array[num];
				if (!xUiC_ItemStack.StackLock)
				{
					ItemStack itemStack = xUiC_ItemStack.ItemStack;
					if (!xUiC_ItemStack.ItemStack.IsEmpty())
					{
						int count = itemStack.count;
						_dstInventory.TryStackItem(0, itemStack);
						if (itemStack.count > 0 && (_moveKind == EItemMoveKind.All || (_moveKind == EItemMoveKind.FillAndCreate && _dstInventory.HasItem(itemStack.itemValue))) && _dstInventory.AddItem(itemStack))
						{
							itemStack = ItemStack.Empty.Clone();
						}
						if (itemStack.count == 0)
						{
							itemStack = ItemStack.Empty.Clone();
						}
						else
						{
							item = false;
						}
						if (count != itemStack.count)
						{
							xUiC_ItemStack.ForceSetItemStack(itemStack);
							item2 = true;
						}
					}
				}
			}
			num = (_startBottomRight ? (num - 1) : (num + 1));
		}
		lastStashTime = unscaledTime;
		return (_allMoved: item, _anyMoved: item2);
	}
}

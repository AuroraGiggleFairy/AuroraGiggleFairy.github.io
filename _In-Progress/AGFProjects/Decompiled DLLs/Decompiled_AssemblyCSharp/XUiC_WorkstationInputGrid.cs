using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationInputGrid : XUiC_WorkstationGrid
{
	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.currentWorkstationInputGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.currentWorkstationInputGrid = null;
	}

	public bool AcceptsMaterial(MaterialBlock material)
	{
		bool result = false;
		if (windowGroup.Controller is XUiC_WorkstationWindowGroup xUiC_WorkstationWindowGroup)
		{
			result = xUiC_WorkstationWindowGroup.WorkstationData.TileEntity.AcceptsMaterial(material);
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		workstationData.SetInputStacks(stackList);
		windowGroup.Controller.SetAllChildrenDirty();
	}

	public bool HasItems(IList<ItemStack> _itemStacks, int _multiplier = 1)
	{
		for (int i = 0; i < _itemStacks.Count; i++)
		{
			if (_itemStacks[i].count * _multiplier - GetItemCount(_itemStacks[i].itemValue) > 0)
			{
				return false;
			}
		}
		return true;
	}

	public void RemoveItems(IList<ItemStack> _itemStacks, int _multiplier = 1, IList<ItemStack> _removedItems = null)
	{
		for (int i = 0; i < _itemStacks.Count; i++)
		{
			int num = _itemStacks[i].count * _multiplier;
			num -= DecItem(_itemStacks[i].itemValue, num, _removedItems);
		}
	}

	public new int AddToItemStackArray(ItemStack _itemStack)
	{
		ItemStack[] slots = GetSlots();
		int num = -1;
		int num2 = 0;
		while (num == -1 && num2 < slots.Length)
		{
			if (slots[num2].CanStackWith(_itemStack))
			{
				slots[num2].count += _itemStack.count;
				_itemStack.count = 0;
				num = num2;
			}
			num2++;
		}
		int num3 = 0;
		while (num == -1 && num3 < slots.Length)
		{
			if (slots[num3].IsEmpty())
			{
				slots[num3] = _itemStack;
				num = num3;
			}
			num3++;
		}
		if (num != -1)
		{
			SetSlots(slots);
			UpdateBackend(slots);
		}
		return num;
	}

	public int DecItem(ItemValue _itemValue, int _count, IList<ItemStack> _removedItems = null)
	{
		int num = _count;
		ItemStack[] slots = GetSlots();
		int num2 = 0;
		while (_count > 0 && num2 < GetSlots().Length)
		{
			if (slots[num2].itemValue.type == _itemValue.type)
			{
				if (slots[num2].itemValue.ItemClass.CanStack())
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
		UpdateBackend(slots);
		return num - _count;
	}

	public int GetItemCount(ItemValue _itemValue)
	{
		ItemStack[] slots = GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].itemValue.type == _itemValue.type)
			{
				num += slots[i].count;
			}
		}
		return num;
	}
}

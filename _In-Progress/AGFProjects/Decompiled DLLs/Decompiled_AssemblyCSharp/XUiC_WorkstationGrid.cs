using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationGrid : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiM_Workstation workstationData;

	public XUiM_Workstation WorkstationData => workstationData;

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public virtual void SetSlots(ItemStack[] stacks)
	{
		base.SetStacks(stacks);
	}

	public virtual bool HasRequirement(Recipe recipe)
	{
		return true;
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		workstationData = ((XUiC_WorkstationWindowGroup)windowGroup.Controller).WorkstationData;
		IsDirty = true;
		IsDormant = false;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		IsDirty = true;
		IsDormant = true;
	}

	public int AddToItemStackArray(ItemStack _itemStack)
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

	public virtual bool AddItem(ItemClass _itemClass, ItemStack _itemStack)
	{
		int startIndex = 0;
		TryStackItem(startIndex, _itemStack);
		if (_itemStack.count > 0 && AddItem(_itemStack))
		{
			return true;
		}
		return false;
	}

	public bool TryStackItem(int startIndex, ItemStack _itemStack)
	{
		int num = 0;
		bool flag = false;
		for (int i = startIndex; i < itemControllers.Length; i++)
		{
			XUiC_ItemStack xUiC_ItemStack = itemControllers[i];
			ItemStack itemStack = xUiC_ItemStack.ItemStack;
			num = _itemStack.count;
			if (itemStack != null && _itemStack.itemValue.type == itemStack.itemValue.type && itemStack.CanStackPartly(ref num))
			{
				xUiC_ItemStack.ItemStack.count += num;
				xUiC_ItemStack.ItemStack = xUiC_ItemStack.ItemStack;
				xUiC_ItemStack.ForceRefreshItemStack();
				_itemStack.count -= num;
				flag = true;
				if (_itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AddItem(ItemStack _item)
	{
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_ItemStack xUiC_ItemStack = itemControllers[i];
			ItemStack itemStack = xUiC_ItemStack.ItemStack;
			if (itemStack == null || itemStack.IsEmpty())
			{
				xUiC_ItemStack.ItemStack = _item;
				return true;
			}
		}
		return false;
	}
}

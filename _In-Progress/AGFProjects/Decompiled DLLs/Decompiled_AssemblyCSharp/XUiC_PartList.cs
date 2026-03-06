using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartList : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	public override void Init()
	{
		base.Init();
		XUiC_ItemStack[] array = itemControllers;
		foreach (XUiC_ItemStack obj in array)
		{
			obj.ViewComponent.IsNavigatable = false;
			obj.StackLocation = XUiC_ItemStack.StackLocationTypes.Part;
		}
	}

	public override ItemStack[] GetSlots()
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
	}

	public void SetSlot(ItemValue part, int index)
	{
		XUiC_ItemStack xUiC_ItemStack = itemControllers[index];
		if (part != null && !part.IsEmpty())
		{
			ItemStack itemStack = new ItemStack(part.Clone(), 1);
			xUiC_ItemStack.ItemStack = itemStack;
			xUiC_ItemStack.GreyedOut = false;
		}
		else
		{
			xUiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
			xUiC_ItemStack.GreyedOut = false;
		}
		itemControllers[index].ViewComponent.EventOnPress = false;
		itemControllers[index].ViewComponent.EventOnHover = false;
	}

	public void SetSlots(ItemValue[] parts, int startIndex = 0)
	{
		for (int i = 0; i < itemControllers.Length - startIndex; i++)
		{
			int num = i + startIndex;
			XUiC_ItemStack xUiC_ItemStack = itemControllers[num];
			if (parts.Length > i && parts[i] != null && !parts[i].IsEmpty())
			{
				ItemStack itemStack = new ItemStack(parts[i].Clone(), 1);
				xUiC_ItemStack.ItemStack = itemStack;
				xUiC_ItemStack.GreyedOut = false;
			}
			else
			{
				xUiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
				xUiC_ItemStack.GreyedOut = false;
			}
			xUiC_ItemStack.ViewComponent.EventOnPress = false;
			xUiC_ItemStack.ViewComponent.EventOnHover = false;
		}
	}

	public void SetAmmoSlot(ItemValue ammo, int count)
	{
		int num = 5;
		int count2 = ((count < 1) ? 1 : count);
		if (ammo != null && !ammo.IsEmpty())
		{
			ItemStack itemStack = new ItemStack(ammo, count2);
			itemControllers[num].ItemStack = itemStack;
			itemControllers[num].GreyedOut = count == 0;
		}
		else
		{
			itemControllers[num].ItemStack = ItemStack.Empty.Clone();
			itemControllers[num].GreyedOut = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetMainItem(ItemStack itemStack)
	{
		itemClass = itemStack.itemValue.ItemClass;
	}
}

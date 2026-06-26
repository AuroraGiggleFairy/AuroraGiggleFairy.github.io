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
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ViewComponent.IsNavigatable = false;
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
		if (part != null && !part.IsEmpty())
		{
			ItemStack itemStack = new ItemStack(part.Clone(), 1);
			itemControllers[index].ItemStack = itemStack;
			itemControllers[index].GreyedOut = false;
		}
		else
		{
			itemControllers[index].ItemStack = ItemStack.Empty.Clone();
			itemControllers[index].GreyedOut = false;
		}
		itemControllers[index].ViewComponent.EventOnPress = false;
		itemControllers[index].ViewComponent.EventOnHover = false;
	}

	public void SetSlots(ItemValue[] parts, int startIndex = 0)
	{
		for (int i = 0; i < itemControllers.Length - startIndex; i++)
		{
			int num = i + startIndex;
			if (parts.Length > i && parts[i] != null && !parts[i].IsEmpty())
			{
				ItemStack itemStack = new ItemStack(parts[i].Clone(), 1);
				itemControllers[num].ItemStack = itemStack;
				itemControllers[num].GreyedOut = false;
			}
			else
			{
				itemControllers[num].ItemStack = ItemStack.Empty.Clone();
				itemControllers[num].GreyedOut = false;
			}
			itemControllers[num].ViewComponent.EventOnPress = false;
			itemControllers[num].ViewComponent.EventOnHover = false;
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

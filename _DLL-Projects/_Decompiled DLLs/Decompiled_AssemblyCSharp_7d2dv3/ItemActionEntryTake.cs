using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryTake : BaseItemActionEntry
{
	public ItemActionEntryTake(XUiController _controller)
		: base(_controller, "lblContextActionTake", "ui_game_symbol_hand", GamepadShortCut.DPadUp)
	{
	}

	public override void RefreshEnabled()
	{
		ItemStack itemStack = getItemStack();
		if (itemStack == null || itemStack.itemValue.IsEmpty())
		{
			base.Enabled = false;
			return;
		}
		int num = base.ItemController.xui.PlayerInventory.CountAvailableSpaceForItem(itemStack.itemValue);
		base.Enabled = num >= 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack getItemStack()
	{
		XUiController itemController = base.ItemController;
		if (!(itemController is XUiC_ItemStack { ItemStack: var itemStack }))
		{
			if (!(itemController is XUiC_EquipmentStack { ItemStack: var itemStack2 }))
			{
				if (!(itemController is XUiC_BasePartStack { ItemStack: var itemStack3 }))
				{
					return null;
				}
				return itemStack3;
			}
			return itemStack2;
		}
		return itemStack;
	}

	public override void OnActivated()
	{
		XUiController itemController = base.ItemController;
		if (!(itemController is XUiC_ItemStack xUiC_ItemStack))
		{
			if (!(itemController is XUiC_EquipmentStack xUiC_EquipmentStack))
			{
				if (itemController is XUiC_BasePartStack xUiC_BasePartStack)
				{
					xUiC_BasePartStack.HandleMoveToPreferredLocation();
				}
			}
			else
			{
				xUiC_EquipmentStack.HandleMoveToPreferredLocation();
			}
		}
		else
		{
			xUiC_ItemStack.HandleMoveToPreferredLocation();
		}
	}
}

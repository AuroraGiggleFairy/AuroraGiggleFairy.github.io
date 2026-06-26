using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryTake : BaseItemActionEntry
{
	public ItemActionEntryTake(XUiController controller)
		: base(controller, "lblContextActionTake", "ui_game_symbol_hand", GamepadShortCut.DPadUp)
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
		int num = base.ItemController.xui.PlayerInventory.CountAvailabileSpaceForItem(itemStack.itemValue);
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
		if (base.ItemController is XUiC_ItemStack xUiC_ItemStack)
		{
			xUiC_ItemStack.HandleMoveToPreferredLocation();
		}
		else if (base.ItemController is XUiC_EquipmentStack xUiC_EquipmentStack)
		{
			xUiC_EquipmentStack.HandleMoveToPreferredLocation();
		}
		else if (base.ItemController is XUiC_BasePartStack xUiC_BasePartStack)
		{
			xUiC_BasePartStack.HandleMoveToPreferredLocation();
		}
	}
}

using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryAssemble : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool recipeUnknown;

	public ItemActionEntryAssemble(XUiController _controller)
		: base(_controller, "lblContextActionModify", "ui_game_symbol_assemble")
	{
		if (base.ItemController is XUiC_ItemStack xUiC_ItemStack)
		{
			if (xUiC_ItemStack.AssembleLock)
			{
				base.ActionName = Localization.Get("lblContextActionComplete");
			}
		}
		else if (base.ItemController is XUiC_EquipmentStack && base.ItemController.xui.AssembleItem.CurrentEquipmentStackController == base.ItemController)
		{
			base.ActionName = Localization.Get("lblContextActionComplete");
		}
	}

	public override void OnDisabledActivate()
	{
		setWindowsDirty();
		if (recipeUnknown)
		{
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiAssembleMustReadBook"));
		}
	}

	public override void OnActivated()
	{
		setWindowsDirty();
		GUIWindowManager windowManager = base.ItemController.xui.playerUI.windowManager;
		XUiController itemController = base.ItemController;
		if (!(itemController is XUiC_ItemStack xUiC_ItemStack))
		{
			if (itemController is XUiC_EquipmentStack xUiC_EquipmentStack)
			{
				ItemStack itemStack = xUiC_EquipmentStack.ItemStack.Clone();
				base.ItemController.xui.AssembleItem.CurrentItem = itemStack;
				base.ItemController.xui.AssembleItem.CurrentItemStackController = null;
				base.ItemController.xui.AssembleItem.CurrentEquipmentStackController = xUiC_EquipmentStack;
				if (!windowManager.IsWindowOpen(XUiC_AssembleWindowGroup.ID))
				{
					windowManager.Open(XUiC_AssembleWindowGroup.ID, _bModal: true);
					xUiC_EquipmentStack.InfoWindow.SetItemStack(xUiC_EquipmentStack, _makeVisible: true);
				}
				else
				{
					XUiC_AssembleWindowGroup.GetWindowGroup(base.ItemController.xui).ItemStack = itemStack;
					xUiC_EquipmentStack.InfoWindow.SetItemStack(xUiC_EquipmentStack, _makeVisible: true);
					windowManager.Close(XUiC_AssembleWindowGroup.ID);
				}
			}
			else
			{
				Log.Error("Modify, neither ItemStack nor EquipmentStack");
			}
		}
		else if (xUiC_ItemStack.AssembleLock)
		{
			base.ItemController.xui.playerUI.windowManager.CloseAllOpenWindows();
		}
		else
		{
			ItemStack itemStack = xUiC_ItemStack.ItemStack.Clone();
			itemStack = ItemActionEntryScrap.HandleRemoveAmmo(itemStack, base.ItemController.xui);
			xUiC_ItemStack.ForceSetItemStack(itemStack);
			base.ItemController.xui.AssembleItem.CurrentItem = itemStack;
			base.ItemController.xui.AssembleItem.CurrentItemStackController = xUiC_ItemStack;
			base.ItemController.xui.AssembleItem.CurrentEquipmentStackController = null;
			if (!windowManager.IsWindowOpen(XUiC_AssembleWindowGroup.ID))
			{
				windowManager.Open(XUiC_AssembleWindowGroup.ID, _bModal: true);
				xUiC_ItemStack.InfoWindow.SetItemStack(xUiC_ItemStack, _makeVisible: true);
			}
			else
			{
				XUiC_AssembleWindowGroup.GetWindowGroup(base.ItemController.xui).ItemStack = itemStack;
				xUiC_ItemStack.InfoWindow.SetItemStack(xUiC_ItemStack, _makeVisible: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setWindowsDirty()
	{
		GUIWindowManager windowManager = base.ItemController.xui.playerUI.windowManager;
		((XUiWindowGroup)windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>().SetAllChildrenDirty();
		((XUiWindowGroup)windowManager.GetWindow("backpack")).Controller.GetChildByType<XUiC_Backpack>().SetAllChildrenDirty();
	}

	public override void RefreshEnabled()
	{
		setWindowsDirty();
		XUiController itemController = base.ItemController;
		ItemStack itemStack;
		if (!(itemController is XUiC_ItemStack xUiC_ItemStack))
		{
			if (!(itemController is XUiC_EquipmentStack xUiC_EquipmentStack))
			{
				return;
			}
			itemStack = xUiC_EquipmentStack.ItemStack;
		}
		else
		{
			itemStack = xUiC_ItemStack.ItemStack;
		}
		if (itemStack.IsEmpty())
		{
			return;
		}
		ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
		if (forId.HasSubItems)
		{
			if (!XUiM_Recipes.GetRecipeIsUnlocked(base.ItemController.xui, forId.Name))
			{
				recipeUnknown = true;
			}
		}
		else if (forId.PartParentId != null)
		{
			for (int i = 0; i < forId.PartParentId.Count; i++)
			{
				if (XUiM_Recipes.GetRecipeIsUnlocked(base.ItemController.xui, ItemClass.GetForId(forId.PartParentId[i]).Name))
				{
					recipeUnknown = false;
					break;
				}
				recipeUnknown = true;
			}
		}
		base.Enabled = base.ItemController.xui.playerUI.entityPlayer.IsAimingGunPossible();
	}
}

using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryAssemble : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string lblMustReadBook;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool recipeUnknown;

	public ItemActionEntryAssemble(XUiController controller)
		: base(controller, "lblContextActionModify", "ui_game_symbol_assemble")
	{
		XUiC_ItemStack xUiC_ItemStack = base.ItemController as XUiC_ItemStack;
		lblMustReadBook = Localization.Get("xuiAssembleMustReadBook");
		if (xUiC_ItemStack != null)
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
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, lblMustReadBook);
		}
	}

	public override void OnActivated()
	{
		setWindowsDirty();
		GUIWindowManager windowManager = base.ItemController.xui.playerUI.windowManager;
		if (base.ItemController is XUiC_ItemStack xUiC_ItemStack)
		{
			if (xUiC_ItemStack.AssembleLock)
			{
				base.ItemController.xui.playerUI.windowManager.CloseAllOpenWindows();
				return;
			}
			ItemStack stack = xUiC_ItemStack.ItemStack.Clone();
			stack = HandleRemoveAmmo(stack);
			xUiC_ItemStack.ForceSetItemStack(stack);
			base.ItemController.xui.AssembleItem.CurrentItem = stack;
			base.ItemController.xui.AssembleItem.CurrentItemStackController = xUiC_ItemStack;
			base.ItemController.xui.AssembleItem.CurrentEquipmentStackController = null;
			if (!windowManager.IsWindowOpen(XUiC_AssembleWindowGroup.ID))
			{
				windowManager.Open(XUiC_AssembleWindowGroup.ID, _bModal: true);
				xUiC_ItemStack.InfoWindow.SetItemStack(xUiC_ItemStack, _makeVisible: true);
			}
			else
			{
				XUiC_AssembleWindowGroup.GetWindowGroup(base.ItemController.xui).ItemStack = stack;
				xUiC_ItemStack.InfoWindow.SetItemStack(xUiC_ItemStack, _makeVisible: true);
			}
		}
		else if (base.ItemController is XUiC_EquipmentStack xUiC_EquipmentStack)
		{
			ItemStack stack = xUiC_EquipmentStack.ItemStack.Clone();
			base.ItemController.xui.AssembleItem.CurrentItem = stack;
			base.ItemController.xui.AssembleItem.CurrentItemStackController = null;
			base.ItemController.xui.AssembleItem.CurrentEquipmentStackController = xUiC_EquipmentStack;
			if (!windowManager.IsWindowOpen(XUiC_AssembleWindowGroup.ID))
			{
				windowManager.Open(XUiC_AssembleWindowGroup.ID, _bModal: true);
				xUiC_EquipmentStack.InfoWindow.SetItemStack(xUiC_EquipmentStack, _makeVisible: true);
			}
			else
			{
				XUiC_AssembleWindowGroup.GetWindowGroup(base.ItemController.xui).ItemStack = stack;
				xUiC_EquipmentStack.InfoWindow.SetItemStack(xUiC_EquipmentStack, _makeVisible: true);
				windowManager.Close(XUiC_AssembleWindowGroup.ID);
			}
		}
		else
		{
			Log.Error("Modify, neither ItemStack nor EquipmentStack");
		}
	}

	public ItemStack HandleRemoveAmmo(ItemStack stack)
	{
		if (stack.itemValue.Meta > 0)
		{
			ItemClass forId = ItemClass.GetForId(stack.itemValue.type);
			for (int i = 0; i < forId.Actions.Length; i++)
			{
				if (!(forId.Actions[i] is ItemActionRanged))
				{
					continue;
				}
				ItemActionRanged itemActionRanged = (ItemActionRanged)forId.Actions[i];
				if (!itemActionRanged.InfiniteAmmo && stack.itemValue.SelectedAmmoTypeIndex < itemActionRanged.MagazineItemNames.Length)
				{
					ItemStack itemStack = new ItemStack(ItemClass.GetItem(itemActionRanged.MagazineItemNames[stack.itemValue.SelectedAmmoTypeIndex]), stack.itemValue.Meta);
					if (!base.ItemController.xui.PlayerInventory.AddItem(itemStack))
					{
						base.ItemController.xui.PlayerInventory.DropItem(itemStack);
					}
					stack.itemValue.Meta = 0;
				}
			}
		}
		return stack;
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
		ItemStack itemStack = ((!(base.ItemController is XUiC_ItemStack xUiC_ItemStack)) ? (base.ItemController as XUiC_EquipmentStack).ItemStack : xUiC_ItemStack.ItemStack);
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
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		base.IconName = "ui_game_symbol_assemble";
		base.Enabled = entityPlayer.IsAimingGunPossible();
	}
}

using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryScrap(XUiController _controller) : BaseItemActionEntry(_controller, "lblContextActionScrap", "ui_game_symbol_scrap", GamepadShortCut.DPadRight)
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public float scrapItemCount = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float numberOfCurrentItemsNeededFor1StackOfOutputItem = -1f;

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		XUiC_ItemStack obj = (XUiC_ItemStack)base.ItemController;
		obj.LockChangedEvent += ItemStackController_LockChangedEvent;
		ItemStack itemStack = obj.ItemStack.Clone();
		Recipe scrapableRecipe = CraftingManager.GetScrapableRecipe(itemStack.itemValue, itemStack.count);
		if (scrapableRecipe == null)
		{
			return;
		}
		this.recipe = scrapableRecipe;
		XUiController xUiController = base.ItemController.xui.FindWindowGroupByName("workstation_workbench");
		if (xUiController == null || !xUiController.WindowGroup.isShowing)
		{
			xUiController = xui.FindWindowGroupByName("crafting");
		}
		XUiC_CraftingWindowGroup childByType = xUiController.GetChildByType<XUiC_CraftingWindowGroup>();
		if (childByType == null)
		{
			return;
		}
		ItemClass forId = ItemClass.GetForId(this.recipe.itemValueType);
		ItemClass forId2 = ItemClass.GetForId(itemStack.itemValue.type);
		int num = forId2.GetWeight() * itemStack.count;
		int weight = forId.GetWeight();
		int num2 = num / weight;
		if (num2 > 0)
		{
			int num3 = (int)((float)(num2 * weight) / (float)forId2.GetWeight() + 0.5f);
			Recipe recipe = new Recipe();
			num2 = (int)((float)num2 * 0.75f);
			if (num2 <= 0)
			{
				num2 = 1;
			}
			recipe.count = num2;
			recipe.craftExpGain = this.recipe.craftExpGain;
			recipe.ingredients.Add(new ItemStack(itemStack.itemValue, num3));
			recipe.itemValueType = forId.Id;
			recipe.craftingTime = ((forId2.ScrapTimeOverride > 0f) ? forId2.ScrapTimeOverride : EffectManager.GetValue(PassiveEffects.ScrappingTime, null, forId.CraftComponentTime * (float)num2, xui.playerUI.entityPlayer));
			recipe.scrapable = true;
			recipe.IsScrap = true;
			if (!childByType.AddItemToQueue(recipe, 1))
			{
				warnQueueFull();
				return;
			}
			itemStack.count -= num3;
			itemStack = HandleRemoveAmmo(itemStack, base.ItemController.xui);
			((XUiC_ItemStack)base.ItemController).ItemStack = ((itemStack.count <= 0) ? ItemStack.Empty.Clone() : itemStack.Clone());
			((XUiC_ItemStack)base.ItemController).WindowGroup.Controller.SetAllChildrenDirty();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void warnQueueFull()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiCraftQueueFull"));
		Manager.PlayInsidePlayerHead("ui_denied");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemStackController_TimeIntervalElapsedEvent(float _timeLeft, XUiC_ItemStack _uiItemStack)
	{
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		XUiM_PlayerInventory playerInventory = base.ItemController.xui.PlayerInventory;
		XUiC_ItemStack obj = (XUiC_ItemStack)base.ItemController;
		ItemStack itemStack = obj.ItemStack.Clone();
		itemStack.count -= (int)numberOfCurrentItemsNeededFor1StackOfOutputItem;
		obj.ItemStack = itemStack;
		ItemStack itemStack2 = new ItemStack(new ItemValue(recipe.itemValueType), (int)scrapItemCount);
		if (!playerInventory.AddItem(itemStack2))
		{
			GameManager.Instance.ItemDropServer(itemStack2, entityPlayer.GetPosition(), new Vector3(0.5f, 0f, 0.5f));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemStackController_LockChangedEvent(XUiC_ItemStack.LockTypes _lockType, XUiC_ItemStack _uiItemStack)
	{
		XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		if (xUiC_ItemStack.ItemStack.count == 0)
		{
			xUiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
		}
		xUiC_ItemStack.TimeIntervalElapsedEvent -= ItemStackController_TimeIntervalElapsedEvent;
		xUiC_ItemStack.UnlockStack();
		xUiC_ItemStack.LockChangedEvent -= ItemStackController_LockChangedEvent;
	}

	public override void RefreshEnabled()
	{
		ItemStack itemStack = ((XUiC_ItemStack)base.ItemController).ItemStack;
		if (itemStack.itemValue.Modifications.Length != 0)
		{
			for (int i = 0; i < itemStack.itemValue.Modifications.Length; i++)
			{
				ItemValue itemValue = itemStack.itemValue.Modifications[i];
				if (itemValue != null && !itemValue.IsEmpty() && ((ItemClassModifier)itemValue.ItemClass).Type == ItemClassModifier.ModifierTypes.Attachment)
				{
					base.Enabled = false;
					return;
				}
			}
		}
		base.Enabled = true;
	}

	public override void OnDisabledActivate()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("ttCannotScrapWithAttachments"));
	}

	public static ItemStack HandleRemoveAmmo(ItemStack _stack, XUi _xui)
	{
		if (_stack.itemValue.Meta <= 0)
		{
			return _stack;
		}
		ItemClass forId = ItemClass.GetForId(_stack.itemValue.type);
		for (int i = 0; i < forId.Actions.Length; i++)
		{
			if (forId.Actions[i] is ItemActionRanged itemActionRanged && !(forId.Actions[i] is ItemActionTextureBlock) && itemActionRanged.MagazineItemNames != null && _stack.itemValue.SelectedAmmoTypeIndex < itemActionRanged.MagazineItemNames.Length)
			{
				ItemStack itemStack = new ItemStack(ItemClass.GetItem(itemActionRanged.MagazineItemNames[_stack.itemValue.SelectedAmmoTypeIndex]), _stack.itemValue.Meta);
				if (!_xui.PlayerInventory.AddItem(itemStack))
				{
					_xui.PlayerInventory.DropItem(itemStack);
				}
				_stack.itemValue.Meta = 0;
			}
		}
		return _stack;
	}
}

using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryRepair : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum StateTypes
	{
		Normal,
		RecipeLocked,
		NotEnoughMaterials
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public StateTypes state;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblReadBook;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblNeedMaterials;

	public ItemActionEntryRepair(XUiController controller)
		: base(controller, "lblContextActionRepair", "ui_game_symbol_wrench", GamepadShortCut.DPadLeft)
	{
		lblReadBook = Localization.Get("xuiRepairMustReadBook");
		lblNeedMaterials = Localization.Get("xuiRepairMissingMats");
		controller.xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
		controller.xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		RefreshEnabled();
		if (base.ParentItem != null)
		{
			base.ParentItem.MarkDirty();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		RefreshEnabled();
		if (base.ParentItem != null)
		{
			base.ParentItem.MarkDirty();
		}
	}

	public override void OnDisabledActivate()
	{
		switch (state)
		{
		case StateTypes.RecipeLocked:
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, lblReadBook);
			break;
		case StateTypes.NotEnoughMaterials:
		{
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, lblNeedMaterials);
			ItemClass forId = ItemClass.GetForId(((XUiC_ItemStack)base.ItemController).ItemStack.itemValue.type);
			if (forId.RepairTools != null && forId.RepairTools.Length > 0)
			{
				ItemClass itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value);
				if (itemClass != null)
				{
					ItemStack itemStack = new ItemStack(new ItemValue(itemClass.Id), 0);
					base.ItemController.xui.playerUI.entityPlayer.AddUIHarvestingItem(itemStack, _bAddOnlyIfNotExisting: true);
				}
			}
			break;
		}
		}
	}

	public override void RefreshEnabled()
	{
		base.RefreshEnabled();
		state = StateTypes.Normal;
		XUi xui = base.ItemController.xui;
		if (((XUiC_ItemStack)base.ItemController).ItemStack.IsEmpty() || ((XUiC_ItemStack)base.ItemController).StackLock)
		{
			return;
		}
		ItemClass forId = ItemClass.GetForId(((XUiC_ItemStack)base.ItemController).ItemStack.itemValue.type);
		base.Enabled = state == StateTypes.Normal;
		if (!base.Enabled)
		{
			base.IconName = "ui_game_symbol_book";
			return;
		}
		ItemValue itemValue = ((XUiC_ItemStack)base.ItemController).ItemStack.itemValue;
		if (forId.RepairTools == null || forId.RepairTools.Length <= 0)
		{
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value);
		if (itemClass != null)
		{
			int b = Convert.ToInt32(Math.Ceiling((float)Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass.RepairAmount.Value));
			if (Mathf.Min(xui.PlayerInventory.GetItemCount(new ItemValue(itemClass.Id)), b) * itemClass.RepairAmount.Value <= 0)
			{
				state = StateTypes.NotEnoughMaterials;
				base.Enabled = state == StateTypes.Normal;
			}
		}
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		((XUiC_ItemStack)base.ItemController).TimeIntervalElapsedEvent += ItemActionEntryRepair_TimeIntervalElapsedEvent;
		XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		ItemValue itemValue = xUiC_ItemStack.ItemStack.itemValue;
		ItemClass forId = ItemClass.GetForId(itemValue.type);
		int sourceToolbeltSlot = ((xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) ? xUiC_ItemStack.SlotNumber : (-1));
		if (forId.RepairTools == null || forId.RepairTools.Length <= 0)
		{
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value);
		if (itemClass == null)
		{
			return;
		}
		int b = Convert.ToInt32(Math.Ceiling((float)Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass.RepairAmount.Value));
		int num = Mathf.Min(playerInventory.GetItemCount(new ItemValue(itemClass.Id)), b);
		int num2 = num * itemClass.RepairAmount.Value;
		XUiC_CraftingWindowGroup childByType = xui.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingWindowGroup>();
		if (childByType != null && num2 > 0)
		{
			Recipe recipe = new Recipe();
			recipe.count = 1;
			recipe.craftExpGain = Mathf.CeilToInt(forId.RepairExpMultiplier * (float)num);
			recipe.ingredients.Add(new ItemStack(new ItemValue(itemClass.Id), num));
			recipe.itemValueType = itemValue.type;
			recipe.craftingTime = itemClass.RepairTime.Value * (float)num;
			num2 = (int)EffectManager.GetValue(PassiveEffects.RepairAmount, null, num2, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()));
			recipe.craftingTime = (int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()));
			ItemClass.GetForId(recipe.itemValueType);
			if (!childByType.AddRepairItemToQueue(recipe.craftingTime, itemValue.Clone(), num2, sourceToolbeltSlot))
			{
				WarnQueueFull();
				return;
			}
			((XUiC_ItemStack)base.ItemController).ItemStack = ItemStack.Empty.Clone();
			playerInventory.RemoveItems(recipe.ingredients);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemActionEntryRepair_TimeIntervalElapsedEvent(float timeLeft, XUiC_ItemStack _uiItemStack)
	{
		if (timeLeft <= 0f)
		{
			ItemStack itemStack = _uiItemStack.ItemStack.Clone();
			itemStack.itemValue.UseTimes = Mathf.Max(0f, itemStack.itemValue.UseTimes - (float)_uiItemStack.RepairAmount);
			_uiItemStack.ItemStack = itemStack;
			_uiItemStack.TimeIntervalElapsedEvent -= ItemActionEntryRepair_TimeIntervalElapsedEvent;
			_uiItemStack.UnlockStack();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WarnQueueFull()
	{
		string text = "No room in queue!";
		if (Localization.Exists("wrnQueueFull"))
		{
			text = Localization.Get("wrnQueueFull");
		}
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, text);
		Manager.PlayInsidePlayerHead("ui_denied");
	}

	public override void DisableEvents()
	{
		base.ItemController.xui.PlayerInventory.OnBackpackItemsChanged -= PlayerInventory_OnBackpackItemsChanged;
		base.ItemController.xui.PlayerInventory.OnToolbeltItemsChanged -= PlayerInventory_OnToolbeltItemsChanged;
	}
}

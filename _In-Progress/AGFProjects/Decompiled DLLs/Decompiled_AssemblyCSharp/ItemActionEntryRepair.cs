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

	public ItemActionEntryRepair(XUiController _controller)
		: base(_controller, "lblContextActionRepair", "ui_game_symbol_wrench", GamepadShortCut.DPadLeft)
	{
		_controller.xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
		_controller.xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		RefreshEnabled();
		base.ParentItem?.MarkDirty();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		RefreshEnabled();
		base.ParentItem?.MarkDirty();
	}

	public override void OnDisabledActivate()
	{
		switch (state)
		{
		case StateTypes.RecipeLocked:
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiRepairMustReadBook"));
			break;
		case StateTypes.NotEnoughMaterials:
		{
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiRepairMissingMats"));
			ItemClass itemClass = null;
			if (base.ItemController is XUiC_EquipmentStack xUiC_EquipmentStack)
			{
				if (xUiC_EquipmentStack.ItemStack.IsEmpty())
				{
					break;
				}
				itemClass = xUiC_EquipmentStack.ItemValue.ItemClass;
			}
			else if (base.ItemController is XUiC_ItemStack xUiC_ItemStack)
			{
				if (xUiC_ItemStack.ItemStack.IsEmpty() || xUiC_ItemStack.StackLock)
				{
					break;
				}
				itemClass = xUiC_ItemStack.ItemStack.itemValue.ItemClass;
			}
			if (itemClass != null && itemClass.RepairTools?.Length > 0)
			{
				ItemClass itemClass2 = ItemClass.GetItemClass(itemClass.RepairTools[0].Value);
				if (itemClass2 != null)
				{
					ItemStack itemStack = new ItemStack(new ItemValue(itemClass2.Id), 0);
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
		ItemClass itemClass = null;
		ItemStack itemStack = ItemStack.Empty.Clone();
		if (base.ItemController is XUiC_EquipmentStack xUiC_EquipmentStack)
		{
			if (xUiC_EquipmentStack.ItemStack.IsEmpty())
			{
				return;
			}
			itemClass = xUiC_EquipmentStack.ItemValue.ItemClass;
			itemStack = xUiC_EquipmentStack.ItemStack;
		}
		else if (base.ItemController is XUiC_ItemStack xUiC_ItemStack)
		{
			if (xUiC_ItemStack.ItemStack.IsEmpty() || xUiC_ItemStack.StackLock)
			{
				return;
			}
			itemClass = xUiC_ItemStack.ItemStack.itemValue.ItemClass;
			itemStack = xUiC_ItemStack.ItemStack;
		}
		base.Enabled = state == StateTypes.Normal;
		if (!base.Enabled)
		{
			base.IconName = "ui_game_symbol_book";
			return;
		}
		ItemValue itemValue = itemStack.itemValue;
		if (itemClass == null || !(itemClass.RepairTools?.Length > 0))
		{
			return;
		}
		ItemClass itemClass2 = ItemClass.GetItemClass(itemClass.RepairTools[0].Value);
		if (itemClass2 != null)
		{
			int b = Convert.ToInt32(Math.Ceiling((float)Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass2.RepairAmount.Value));
			if (Mathf.Min(xui.PlayerInventory.GetItemCount(new ItemValue(itemClass2.Id)), b) * itemClass2.RepairAmount.Value <= 0)
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
		ItemStack itemStack = ItemStack.Empty;
		if (base.ItemController is XUiC_EquipmentStack xUiC_EquipmentStack)
		{
			itemStack = xUiC_EquipmentStack.ItemStack;
		}
		else if (base.ItemController is XUiC_ItemStack xUiC_ItemStack)
		{
			xUiC_ItemStack.TimeIntervalElapsedEvent += ItemActionEntryRepair_TimeIntervalElapsedEvent;
			itemStack = xUiC_ItemStack.ItemStack;
		}
		ItemValue itemValue = itemStack.itemValue;
		ItemClass forId = ItemClass.GetForId(itemValue.type);
		if (forId?.RepairTools == null || forId.RepairTools.Length <= 0)
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
		if (childByType == null || num2 <= 0)
		{
			return;
		}
		Recipe recipe = new Recipe();
		recipe.count = 1;
		recipe.craftExpGain = Mathf.CeilToInt(forId.RepairExpMultiplier * (float)num);
		recipe.ingredients.Add(new ItemStack(new ItemValue(itemClass.Id), num));
		recipe.itemValueType = itemValue.type;
		recipe.craftingTime = itemClass.RepairTime.Value * (float)num;
		num2 = (int)EffectManager.GetValue(PassiveEffects.RepairAmount, null, num2, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()));
		recipe.craftingTime = (int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()));
		if (!childByType.AddRepairItemToQueue(recipe.craftingTime, itemValue.Clone(), num2))
		{
			warnQueueFull();
			return;
		}
		if (base.ItemController is XUiC_EquipmentStack xUiC_EquipmentStack2)
		{
			xUiC_EquipmentStack2.ItemStack = ItemStack.Empty.Clone();
			xui.PlayerEquipment.Equipment.SetPreferredItemSlot(xUiC_EquipmentStack2.SlotNumber, itemValue);
		}
		else if (base.ItemController is XUiC_ItemStack xUiC_ItemStack2)
		{
			xUiC_ItemStack2.ItemStack = ItemStack.Empty.Clone();
		}
		playerInventory.RemoveItems(recipe.ingredients);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemActionEntryRepair_TimeIntervalElapsedEvent(float _timeLeft, XUiC_ItemStack _uiItemStack)
	{
		if (!(_timeLeft > 0f))
		{
			ItemStack itemStack = _uiItemStack.ItemStack.Clone();
			itemStack.itemValue.UseTimes = Mathf.Max(0f, itemStack.itemValue.UseTimes - (float)_uiItemStack.RepairAmount);
			_uiItemStack.ItemStack = itemStack;
			_uiItemStack.TimeIntervalElapsedEvent -= ItemActionEntryRepair_TimeIntervalElapsedEvent;
			_uiItemStack.UnlockStack();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void warnQueueFull()
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

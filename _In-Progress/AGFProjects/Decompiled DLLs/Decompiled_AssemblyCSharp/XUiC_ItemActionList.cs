using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemActionList : XUiController
{
	public enum ItemActionListTypes
	{
		None,
		Buff,
		Crafting,
		Forge,
		Item,
		Loot,
		Equipment,
		Creative,
		Skill,
		Part,
		Trader,
		QuestReward
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ItemActionEntry> entryList = new List<XUiC_ItemActionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BaseItemActionEntry> itemActionEntries = new List<BaseItemActionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionListTypes itemActionListType = ItemActionListTypes.Crafting;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeEntry recipeEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeCraftCount craftCountControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool actionWasRunning;

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is XUiC_ItemActionEntry)
			{
				entryList.Add((XUiC_ItemActionEntry)children[i]);
			}
		}
		craftCountControl = windowGroup.Controller.GetChildByType<XUiC_RecipeCraftCount>();
	}

	public override void Update(float _dt)
	{
		bool flag = base.xui.playerUI.entityPlayer.inventory.GetHoldingPrimary()?.IsActionRunning(base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[0]) ?? false;
		if (!flag && actionWasRunning)
		{
			isDirty = true;
		}
		actionWasRunning = flag;
		if (isDirty)
		{
			SortActionList();
			for (int i = 0; i < entryList.Count; i++)
			{
				XUiC_ItemActionEntry xUiC_ItemActionEntry = entryList[i];
				if (xUiC_ItemActionEntry != null)
				{
					if (i < itemActionEntries.Count)
					{
						xUiC_ItemActionEntry.ItemActionEntry = itemActionEntries[i];
					}
					else
					{
						xUiC_ItemActionEntry.ItemActionEntry = null;
					}
				}
			}
			isDirty = false;
		}
		if (base.ViewComponent.UiTransform.gameObject.activeInHierarchy)
		{
			PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !base.xui.playerUI.windowManager.IsInputActive())
			{
				for (int j = 0; j < entryList.Count; j++)
				{
					if (entryList[j].ItemActionEntry != null && (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || gUIActions.Inspect.IsPressed) && ((gUIActions.DPad_Up.WasPressed && entryList[j].ItemActionEntry.ShortCut == BaseItemActionEntry.GamepadShortCut.DPadUp) || (gUIActions.DPad_Right.WasPressed && entryList[j].ItemActionEntry.ShortCut == BaseItemActionEntry.GamepadShortCut.DPadRight) || (gUIActions.DPad_Down.WasPressed && entryList[j].ItemActionEntry.ShortCut == BaseItemActionEntry.GamepadShortCut.DPadDown) || (gUIActions.DPad_Left.WasPressed && entryList[j].ItemActionEntry.ShortCut == BaseItemActionEntry.GamepadShortCut.DPadLeft)))
					{
						entryList[j].Background.Pressed(-1);
						break;
					}
				}
			}
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortActionList()
	{
		if (itemActionEntries.Count <= 0)
		{
			return;
		}
		List<BaseItemActionEntry> list = new List<BaseItemActionEntry>();
		List<BaseItemActionEntry.GamepadShortCut> list2 = new List<BaseItemActionEntry.GamepadShortCut>();
		List<BaseItemActionEntry> list3 = new List<BaseItemActionEntry>();
		for (int i = 0; i < 5; i++)
		{
			BaseItemActionEntry.GamepadShortCut shortcut = (BaseItemActionEntry.GamepadShortCut)i;
			List<BaseItemActionEntry> list4 = itemActionEntries.FindAll([PublicizedFrom(EAccessModifier.Internal)] (BaseItemActionEntry itemEntry) => itemEntry.ShortCut == shortcut);
			if (shortcut == BaseItemActionEntry.GamepadShortCut.None)
			{
				list3.AddRange(list4);
			}
			else if (list4.Count == 0)
			{
				list2.Add(shortcut);
			}
			else if (list4.Count > 1)
			{
				list3.AddRange(list4.GetRange(1, list4.Count - 1));
				list.Add(list4[0]);
			}
			else
			{
				list.AddRange(list4);
			}
		}
		for (int num = 0; num < list3.Count; num++)
		{
			if (list2.Count == 0)
			{
				list3[num].ShortCut = BaseItemActionEntry.GamepadShortCut.None;
			}
			else
			{
				BaseItemActionEntry.GamepadShortCut shortCut = list2[0];
				list2.RemoveAt(0);
				list3[num].ShortCut = shortCut;
			}
			list.Add(list3[num]);
		}
		list.Sort([PublicizedFrom(EAccessModifier.Internal)] (BaseItemActionEntry x, BaseItemActionEntry y) => x.ShortCut - y.ShortCut);
		itemActionEntries = list;
	}

	public void AddActionListEntry(BaseItemActionEntry actionEntry)
	{
		itemActionEntries.Add(actionEntry);
		actionEntry.ParentActionList = this;
	}

	public void RefreshActionList()
	{
		isDirty = true;
	}

	public void SetCraftingActionList(ItemActionListTypes _actionListType, XUiController itemController)
	{
		for (int i = 0; i < itemActionEntries.Count; i++)
		{
			itemActionEntries[i].DisableEvents();
		}
		itemActionEntries.Clear();
		switch (_actionListType)
		{
		case ItemActionListTypes.Crafting:
		{
			XUiC_RecipeEntry xUiC_RecipeEntry = (XUiC_RecipeEntry)itemController;
			List<XUiC_CraftingWindowGroup> childrenByType = base.xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
			XUiC_CraftingWindowGroup xUiC_CraftingWindowGroup = null;
			for (int j = 0; j < childrenByType.Count; j++)
			{
				if (childrenByType[j].WindowGroup != null && childrenByType[j].WindowGroup.isShowing)
				{
					xUiC_CraftingWindowGroup = childrenByType[j];
					break;
				}
			}
			bool flag = xUiC_CraftingWindowGroup == null || (xUiC_CraftingWindowGroup.Workstation == "" && xUiC_RecipeEntry.Recipe.craftingArea == "");
			int craftingTier = -1;
			if (xUiC_CraftingWindowGroup != null)
			{
				craftingTier = xUiC_CraftingWindowGroup.GetChildByType<XUiC_CraftingInfoWindow>().SelectedCraftingTier;
				Block blockByName = Block.GetBlockByName(xUiC_CraftingWindowGroup.Workstation);
				if (blockByName != null && blockByName.Properties.Values.ContainsKey("Workstation.CraftingAreaRecipes"))
				{
					string text = blockByName.Properties.Values["Workstation.CraftingAreaRecipes"];
					string[] array = new string[1] { text };
					if (text.Contains(","))
					{
						array = text.Replace(", ", ",").Replace(" ,", ",").Replace(" , ", ",")
							.Split(',');
					}
					for (int k = 0; k < array.Length; k++)
					{
						flag = ((!array[k].EqualsCaseInsensitive("player")) ? (flag | array[k].EqualsCaseInsensitive(xUiC_RecipeEntry.Recipe.craftingArea)) : (flag | string.IsNullOrEmpty(xUiC_RecipeEntry.Recipe.craftingArea)));
					}
				}
				else
				{
					flag |= xUiC_CraftingWindowGroup.Workstation.EqualsCaseInsensitive(xUiC_RecipeEntry.Recipe.craftingArea);
				}
			}
			if (flag)
			{
				if (XUiM_Recipes.GetRecipeIsUnlocked(base.xui, xUiC_RecipeEntry.Recipe))
				{
					AddActionListEntry(new ItemActionEntryCraft(itemController, craftCountControl, craftingTier));
					AddActionListEntry(new ItemActionEntryFavorite(itemController, xUiC_RecipeEntry.Recipe));
				}
				else
				{
					HandleUnlockedBy(itemController, xUiC_RecipeEntry);
				}
			}
			else if (!XUiM_Recipes.GetRecipeIsUnlocked(base.xui, xUiC_RecipeEntry.Recipe))
			{
				HandleUnlockedBy(itemController, xUiC_RecipeEntry);
			}
			if (xUiC_RecipeEntry.Recipe.IsTrackable)
			{
				AddActionListEntry(new ItemActionEntryTrackRecipe(itemController, craftingTier));
			}
			break;
		}
		case ItemActionListTypes.Equipment:
		case ItemActionListTypes.Part:
		{
			if (itemController is XUiC_VehiclePartStack xUiC_VehiclePartStack)
			{
				if (xUiC_VehiclePartStack.SlotType != "chassis")
				{
					AddActionListEntry(new ItemActionEntryTake(itemController));
				}
				break;
			}
			if (itemController is XUiC_ItemPartStack { ItemStack: var itemStack2 })
			{
				if (itemStack2 != null && (itemStack2.itemValue.ItemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment)
				{
					AddActionListEntry(new ItemActionEntryTake(itemController));
				}
				break;
			}
			if (itemController is XUiC_ItemCosmeticStack)
			{
				AddActionListEntry(new ItemActionEntryTake(itemController));
				break;
			}
			XUiC_EquipmentStack xUiC_EquipmentStack = (XUiC_EquipmentStack)itemController;
			if (xUiC_EquipmentStack == base.xui.AssembleItem.CurrentEquipmentStackController)
			{
				AddActionListEntry(new ItemActionEntryAssemble(itemController));
				break;
			}
			ItemStack itemStack3 = xUiC_EquipmentStack.ItemStack;
			if (!itemStack3.IsEmpty())
			{
				ItemValue itemValue2 = itemStack3.itemValue;
				ItemClass itemClass2 = itemValue2.ItemClass;
				ItemClassArmor itemClassArmor = itemClass2 as ItemClassArmor;
				if (itemClassArmor == null || itemClassArmor.AllowUnEquip)
				{
					AddActionListEntry(new ItemActionEntryTake(itemController));
				}
				if (itemValue2.Modifications.Length != 0 || itemValue2.CosmeticMods.Length != 0)
				{
					AddActionListEntry(new ItemActionEntryAssemble(itemController));
				}
				if (itemValue2.MaxUseTimes > 0 && itemValue2.UseTimes > 0f && itemClass2.RepairTools != null && itemClass2.RepairTools.Length > 0 && itemClass2.RepairTools[0].Value.Length > 0)
				{
					AddActionListEntry(new ItemActionEntryRepair(itemController));
				}
				if (itemClassArmor != null && itemClassArmor.IsCosmetic)
				{
					AddActionListEntry(new ItemActionEntryShowCosmetics(itemController));
				}
			}
			break;
		}
		case ItemActionListTypes.Item:
		{
			XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)itemController;
			ItemStack itemStack = xUiC_ItemStack.ItemStack;
			ItemValue itemValue = itemStack.itemValue;
			ItemClass itemClass = itemStack.itemValue.ItemClass;
			if (itemClass == null)
			{
				break;
			}
			if (xUiC_ItemStack is XUiC_Creative2Stack)
			{
				AddActionListEntry(new ItemActionEntryTake(itemController));
				bool equipFound = true;
				AddActionActions(itemValue, itemClass, xUiC_ItemStack, ref equipFound);
				if (XUiM_Recipes.FilterRecipesByIngredient(itemStack, XUiM_Recipes.GetRecipes()).Count > 0)
				{
					AddActionListEntry(new ItemActionEntryRecipes(itemController));
				}
				AddActionListEntry(new CreativeActionEntryFavorite(itemController, itemStack.itemValue.type));
			}
			else
			{
				if (itemStack.IsEmpty())
				{
					break;
				}
				if (xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.LootContainer || xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Vehicle)
				{
					AddActionListEntry(new ItemActionEntryTake(itemController));
				}
				if (xUiC_ItemStack is XUiC_RequiredItemStack)
				{
					break;
				}
				if ((xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && xUiC_ItemStack == base.xui.AssembleItem.CurrentItemStackController)
				{
					AddActionListEntry(new ItemActionEntryAssemble(itemController));
					break;
				}
				if (base.xui.Trader.Trader != null)
				{
					if (base.xui.Trader.TraderTileEntity is TileEntityVendingMachine tileEntityVendingMachine2)
					{
						if (base.xui.Trader.Trader.TraderInfo.AllowSell || (!base.xui.Trader.Trader.TraderInfo.AllowSell && tileEntityVendingMachine2.LocalPlayerIsOwner()) || tileEntityVendingMachine2.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
						{
							AddActionListEntry(new ItemActionEntrySell(itemController));
						}
					}
					else if (base.xui.Trader.Trader.TraderInfo.AllowSell)
					{
						AddActionListEntry(new ItemActionEntrySell(itemController));
					}
					break;
				}
				if (itemClass.IsEquipment && base.xui.AssembleItem.CurrentItem == null)
				{
					AddActionListEntry(new ItemActionEntryWear(itemController));
				}
				bool equipFound2 = false;
				if (itemClass is ItemClassBlock && xUiC_ItemStack.StackLocation != XUiC_ItemStack.StackLocationTypes.ToolBelt && !xUiC_ItemStack.AssembleLock)
				{
					ItemActionEntryEquip actionEntry = new ItemActionEntryEquip(itemController);
					AddActionListEntry(actionEntry);
					equipFound2 = true;
				}
				AddActionActions(itemValue, itemClass, xUiC_ItemStack, ref equipFound2);
				if (itemValue.MaxUseTimes > 0 && itemValue.UseTimes > 0f && itemClass.RepairTools != null && itemClass.RepairTools.Length > 0 && itemClass.RepairTools[0].Value.Length > 0)
				{
					AddActionListEntry(new ItemActionEntryRepair(itemController));
				}
				if ((xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && (itemValue.Modifications.Length != 0 || itemValue.CosmeticMods.Length != 0))
				{
					AddActionListEntry(new ItemActionEntryAssemble(itemController));
				}
				Recipe scrapableRecipe = CraftingManager.GetScrapableRecipe(itemStack.itemValue, itemStack.count);
				if (scrapableRecipe != null && scrapableRecipe.CanCraft(new ItemStack[1] { itemStack }))
				{
					AddActionListEntry(new ItemActionEntryScrap(itemController));
				}
				if ((xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && base.xui.AssembleItem.CurrentItemStackController == null && XUiM_Recipes.FilterRecipesByIngredient(itemStack, XUiM_Recipes.GetRecipes()).Count > 0)
				{
					AddActionListEntry(new ItemActionEntryRecipes(itemController));
				}
				AddActionListEntry(new ItemActionEntryDrop(itemController));
			}
			break;
		}
		case ItemActionListTypes.Trader:
			if (base.xui.Trader.Trader.TraderInfo.AllowBuy)
			{
				AddActionListEntry(new ItemActionEntryPurchase(itemController));
			}
			if (base.xui.Trader.TraderTileEntity is TileEntityVendingMachine tileEntityVendingMachine && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				AddActionListEntry(new ItemActionEntryMarkup(itemController));
				AddActionListEntry(new ItemActionEntryMarkdown(itemController));
				AddActionListEntry(new ItemActionEntryResetPrice(itemController));
			}
			break;
		}
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddActionActions(ItemValue itemValue, ItemClass itemClass, XUiC_ItemStack stackController, ref bool equipFound)
	{
		for (int i = 0; i < itemClass.Actions.Length; i++)
		{
			ItemAction itemAction = itemClass.Actions[i];
			if (itemAction is ItemActionEat itemActionEat && (!itemActionEat.UsePrompt || stackController.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || stackController.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt))
			{
				AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Heal));
			}
			if (itemAction is ItemActionLearnRecipe)
			{
				AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Read));
			}
			if (itemAction is ItemActionGainSkill)
			{
				AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Read));
			}
			if ((stackController.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || stackController.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && itemAction is ItemActionQuest)
			{
				AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Quest));
			}
			if (itemAction is ItemActionOpenBundle)
			{
				AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Open));
			}
			if (itemAction is ItemActionOpenLootBundle)
			{
				AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Open));
			}
			if (!equipFound && stackController.StackLocation != XUiC_ItemStack.StackLocationTypes.ToolBelt && !stackController.AssembleLock && (itemAction is ItemActionMelee || itemAction is ItemActionDynamicMelee || itemAction is ItemActionRanged || itemAction is ItemActionLauncher || itemAction is ItemActionPlaceAsBlock || itemAction is ItemActionExchangeBlock || itemAction is ItemActionBailLiquid || itemAction is ItemActionActivate || itemAction is ItemActionConnectPower || itemAction is ItemActionThrowAway))
			{
				ItemValue holdingItemItemValue = base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue;
				ItemActionEntryEquip itemActionEntryEquip = new ItemActionEntryEquip(stackController);
				itemActionEntryEquip.Enabled = itemValue.type != holdingItemItemValue.type || itemValue.Quality != holdingItemItemValue.Quality || !Mathf.Approximately(itemValue.UseTimes, holdingItemItemValue.UseTimes);
				AddActionListEntry(itemActionEntryEquip);
				equipFound = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleUnlockedBy(XUiController itemController, XUiC_RecipeEntry entry)
	{
		ItemClass forId = ItemClass.GetForId(entry.Recipe.itemValueType);
		RecipeUnlockData[] array = (forId.IsBlock() ? forId.GetBlock().UnlockedBy : forId.UnlockedBy);
		RecipeUnlockData recipeUnlockData = null;
		RecipeUnlockData recipeUnlockData2 = null;
		RecipeUnlockData recipeUnlockData3 = null;
		RecipeUnlockData recipeUnlockData4 = null;
		RecipeUnlockData recipeUnlockData5 = null;
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				switch (array[i].UnlockType)
				{
				case RecipeUnlockData.UnlockTypes.Book:
					recipeUnlockData2 = array[i];
					break;
				case RecipeUnlockData.UnlockTypes.Perk:
					recipeUnlockData = array[i];
					break;
				case RecipeUnlockData.UnlockTypes.Skill:
					recipeUnlockData3 = array[i];
					break;
				case RecipeUnlockData.UnlockTypes.Schematic:
					_ = array[i];
					break;
				case RecipeUnlockData.UnlockTypes.ChallengeGroup:
					recipeUnlockData4 = array[i];
					break;
				case RecipeUnlockData.UnlockTypes.Challenge:
					recipeUnlockData5 = array[i];
					break;
				}
			}
		}
		if (recipeUnlockData != null)
		{
			AddActionListEntry(new ItemActionEntryShowPerk(itemController, recipeUnlockData));
		}
		if (recipeUnlockData2 != null)
		{
			AddActionListEntry(new ItemActionEntryShowPerk(itemController, recipeUnlockData2));
		}
		if (recipeUnlockData3 != null)
		{
			AddActionListEntry(new ItemActionEntryShowPerk(itemController, recipeUnlockData3));
		}
		if (recipeUnlockData4 != null)
		{
			AddActionListEntry(new ItemActionEntryShowChallenge(itemController, recipeUnlockData4));
		}
		if (recipeUnlockData5 != null)
		{
			AddActionListEntry(new ItemActionEntryShowChallenge(itemController, recipeUnlockData5));
		}
	}

	public void SetServiceActionList(InGameService service, XUiController itemController)
	{
		itemActionEntries.Clear();
		if (service.ServiceType == InGameService.InGameServiceTypes.VendingRent)
		{
			AddActionListEntry(new ServiceActionEntryRent(itemController, base.xui.Trader.TraderTileEntity as TileEntityVendingMachine));
		}
		isDirty = true;
	}
}

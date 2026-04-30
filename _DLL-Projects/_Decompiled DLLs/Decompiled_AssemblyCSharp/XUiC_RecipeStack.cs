using System.Collections;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeStack : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public int recipeCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int recipeTier;

	[PublicizedFrom(EAccessModifier.Private)]
	public float craftingTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public float totalCraftTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oneItemCraftTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCrafting;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInventoryFull;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue originalItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public int amountToRepair;

	[PublicizedFrom(EAccessModifier.Private)]
	public int outputQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startingEntityId = -1;

	public XUiC_CraftingQueue Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController timer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController count;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController itemIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lockIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController overlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController cancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public string inventoryFullDropping;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue outputItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Coroutine sendCraftedItemsForAchievementsCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastItemCraftedTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int itemsCraftedSinceLastAchievementUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WaitForSeconds sendCraftedItemsForAchievementsInterval;

	public int OutputQuality
	{
		get
		{
			return outputQuality;
		}
		set
		{
			outputQuality = value;
		}
	}

	public int StartingEntityId
	{
		get
		{
			return startingEntityId;
		}
		set
		{
			startingEntityId = value;
		}
	}

	public ItemValue OriginalItem
	{
		get
		{
			return originalItem;
		}
		set
		{
			originalItem = value;
		}
	}

	public int AmountToRepair
	{
		get
		{
			return amountToRepair;
		}
		set
		{
			amountToRepair = value;
		}
	}

	public bool IsCrafting
	{
		get
		{
			return isCrafting;
		}
		set
		{
			isCrafting = value;
		}
	}

	public string LockIconSprite
	{
		get
		{
			if (lockIcon != null)
			{
				return ((XUiV_Sprite)lockIcon.ViewComponent).SpriteName;
			}
			return "";
		}
		set
		{
			if (lockIcon != null)
			{
				((XUiV_Sprite)lockIcon.ViewComponent).SpriteName = value;
			}
		}
	}

	public void CopyTo(XUiC_RecipeStack _recipeStack)
	{
		_recipeStack.recipe = recipe;
		_recipeStack.craftingTimeLeft = craftingTimeLeft;
		_recipeStack.totalCraftTimeLeft = totalCraftTimeLeft;
		_recipeStack.recipeCount = recipeCount;
		_recipeStack.IsCrafting = IsCrafting;
		_recipeStack.originalItem = originalItem;
		_recipeStack.amountToRepair = amountToRepair;
		_recipeStack.LockIconSprite = LockIconSprite;
		_recipeStack.outputQuality = outputQuality;
		_recipeStack.startingEntityId = startingEntityId;
		_recipeStack.outputItemValue = outputItemValue;
		_recipeStack.oneItemCraftTime = oneItemCraftTime;
	}

	public override void Init()
	{
		base.Init();
		background = GetChildById("background");
		overlay = GetChildById("overlay");
		lockIcon = GetChildById("lockIcon");
		itemIcon = GetChildById("itemIcon");
		timer = GetChildById("timer");
		count = GetChildById("count");
		cancel = GetChildById("cancel");
		if (background != null)
		{
			background.OnPress += HandleOnPress;
			background.OnHover += HandleOnHover;
		}
		inventoryFullDropping = Localization.Get("xuiInventoryFullDropping");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnHover(XUiController _sender, bool _isOver)
	{
		isOver = _isOver;
	}

	public void ForceCancel()
	{
		HandleOnPress(null, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnPress(XUiController _sender, int _mouseButton)
	{
		if (recipe == null)
		{
			return;
		}
		XUiC_WorkstationMaterialInputGrid childByType = windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputGrid>();
		XUiC_WorkstationInputGrid childByType2 = windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (childByType != null)
		{
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				childByType.SetWeight(recipe.ingredients[i].itemValue.Clone(), recipe.ingredients[i].count * recipeCount);
			}
		}
		else
		{
			if (originalItem != null && !originalItem.Equals(ItemValue.None))
			{
				ItemStack itemStack = new ItemStack(originalItem.Clone(), 1);
				if (!base.xui.PlayerInventory.AddItem(itemStack))
				{
					GameManager.ShowTooltip(entityPlayer, inventoryFullDropping);
					GameManager.Instance.ItemDropServer(new ItemStack(originalItem.Clone(), 1), entityPlayer.position, Vector3.zero, entityPlayer.entityId, 120f);
				}
				originalItem = ItemValue.None.Clone();
			}
			int[] array = new int[recipe.ingredients.Count];
			for (int j = 0; j < recipe.ingredients.Count; j++)
			{
				array[j] = recipe.ingredients[j].count * recipeCount;
				ItemStack itemStack2 = new ItemStack(recipe.ingredients[j].itemValue.Clone(), array[j]);
				if ((childByType2 == null) ? base.xui.PlayerInventory.AddItem(itemStack2, true) : (childByType2.AddToItemStackArray(itemStack2) != -1))
				{
					array[j] = 0;
				}
				else
				{
					array[j] = itemStack2.count;
				}
			}
			bool flag = false;
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] > 0)
				{
					flag = true;
					GameManager.Instance.ItemDropServer(new ItemStack(recipe.ingredients[k].itemValue.Clone(), array[k]), entityPlayer.position, Vector3.zero, entityPlayer.entityId, 120f);
				}
			}
			if (flag)
			{
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, inventoryFullDropping);
			}
		}
		isCrafting = false;
		ClearRecipe();
		Owner?.RefreshQueue();
		windowGroup.Controller.SetAllChildrenDirty();
	}

	public override void Update(float _dt)
	{
		if (isInventoryFull)
		{
			if (recipe != null && outputItemValue != null)
			{
				XUiC_WorkstationOutputGrid childByType = windowGroup.Controller.GetChildByType<XUiC_WorkstationOutputGrid>();
				bool flag = false;
				ItemStack[] array = new ItemStack[0];
				if (childByType != null)
				{
					array = childByType.GetSlots();
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].CanStackWith(new ItemStack(outputItemValue, recipe.count)))
						{
							array[i].count += recipe.count;
							flag = true;
							break;
						}
						if (array[i].IsEmpty())
						{
							array[i] = new ItemStack(outputItemValue, recipe.count);
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					childByType.SetSlots(array);
					childByType.UpdateData(array);
					childByType.IsDirty = true;
					isInventoryFull = false;
					recipeCount--;
					if (recipeCount <= 0)
					{
						isCrafting = false;
						if (recipe != null || craftingTimeLeft != 0f)
						{
							ClearRecipe();
						}
					}
					else
					{
						craftingTimeLeft += oneItemCraftTime;
					}
					base.Update(_dt);
					return;
				}
				if (!base.xui.dragAndDrop.CurrentStack.IsEmpty() && base.xui.dragAndDrop.CurrentStack.itemValue.ItemClass is ItemClassQuest)
				{
					base.Update(_dt);
					return;
				}
				ItemStack itemStack = new ItemStack(outputItemValue, recipe.count);
				if (!base.xui.PlayerInventory.AddItemNoPartial(itemStack, _playCollectSound: false))
				{
					updateRecipeData();
					if (itemStack.count != recipe.count)
					{
						base.xui.PlayerInventory.DropItem(itemStack);
						QuestEventManager.Current.CraftedItem(itemStack);
						isInventoryFull = false;
						recipeCount--;
						if (recipeCount <= 0)
						{
							isCrafting = false;
							if (recipe != null || craftingTimeLeft != 0f)
							{
								ClearRecipe();
							}
						}
						else
						{
							craftingTimeLeft += oneItemCraftTime;
						}
					}
					base.Update(_dt);
					return;
				}
				QuestEventManager.Current.CraftedItem(new ItemStack(outputItemValue, recipe.count));
				isInventoryFull = false;
				recipeCount--;
				if (recipeCount <= 0)
				{
					isCrafting = false;
					if (recipe != null || craftingTimeLeft != 0f)
					{
						ClearRecipe();
					}
				}
				else
				{
					craftingTimeLeft += oneItemCraftTime;
				}
				base.Update(_dt);
				return;
			}
			isInventoryFull = false;
			isCrafting = false;
		}
		if (recipe == null)
		{
			isCrafting = false;
		}
		if (recipeCount > 0)
		{
			if (isCrafting && craftingTimeLeft <= 0f && recipe != null && outputStack())
			{
				recipeCount--;
				if (recipeCount <= 0)
				{
					isCrafting = false;
					if (recipe != null || craftingTimeLeft != 0f)
					{
						ClearRecipe();
					}
				}
				else
				{
					craftingTimeLeft += oneItemCraftTime;
				}
			}
		}
		else
		{
			isCrafting = false;
			if (recipe != null && (recipe != null || craftingTimeLeft != 0f))
			{
				ClearRecipe();
			}
		}
		if (base.ViewComponent.IsVisible)
		{
			updateRecipeData();
		}
		if (recipeCount > 0 && isCrafting)
		{
			craftingTimeLeft -= _dt;
			totalCraftTimeLeft = oneItemCraftTime * ((float)recipeCount - 1f) + craftingTimeLeft;
		}
		else
		{
			if (craftingTimeLeft < 0f)
			{
				craftingTimeLeft = 0f;
			}
			if (totalCraftTimeLeft < 0f)
			{
				totalCraftTimeLeft = 0f;
			}
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool outputStack()
	{
		if (recipe == null)
		{
			return false;
		}
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (entityPlayer == null)
		{
			return false;
		}
		if (originalItem == null || originalItem.Equals(ItemValue.None))
		{
			outputItemValue = new ItemValue(recipe.itemValueType, outputQuality, outputQuality);
			ItemClass itemClass = outputItemValue.ItemClass;
			if (outputItemValue == null)
			{
				return false;
			}
			if (itemClass == null)
			{
				return false;
			}
			if (entityPlayer.entityId == startingEntityId)
			{
				giveExp(outputItemValue, itemClass);
			}
			else if (windowGroup.Controller is XUiC_WorkstationWindowGroup xUiC_WorkstationWindowGroup)
			{
				xUiC_WorkstationWindowGroup.WorkstationData.TileEntity.AddCraftComplete(startingEntityId, outputItemValue, recipe.GetName(), recipe.IsScrap ? recipe.ingredients[0].itemValue.ItemClass.GetItemName() : "", recipe.craftExpGain, recipe.count);
			}
			if (recipe.GetName().Equals("meleeToolRepairT0StoneAxe"))
			{
				PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.StoneAxeCrafted, 1);
			}
			else if (recipe.GetName().Equals("frameShapes:VariantHelper"))
			{
				PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.WoodFrameCrafted, 1);
			}
		}
		else if (amountToRepair > 0)
		{
			ItemValue itemValue = originalItem.Clone();
			itemValue.UseTimes -= amountToRepair;
			_ = itemValue.ItemClass;
			if (itemValue.UseTimes < 0f)
			{
				itemValue.UseTimes = 0f;
			}
			outputItemValue = itemValue.Clone();
			QuestEventManager.Current.RepairedItem(outputItemValue);
			amountToRepair = 0;
		}
		if (outputItemValue != null)
		{
			GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.CraftedItems, outputItemValue.ItemClass.Name, recipe.count);
		}
		else
		{
			outputItemValue = originalItem;
		}
		XUiC_WorkstationOutputGrid childByType = windowGroup.Controller.GetChildByType<XUiC_WorkstationOutputGrid>();
		if (childByType != null && (originalItem == null || originalItem.Equals(ItemValue.None)))
		{
			ItemStack itemStack = new ItemStack(outputItemValue, recipe.count);
			ItemStack[] slots = childByType.GetSlots();
			bool flag = false;
			for (int i = 0; i < slots.Length; i++)
			{
				if (slots[i].CanStackWith(itemStack))
				{
					slots[i].count += recipe.count;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < slots.Length; j++)
				{
					if (slots[j].IsEmpty())
					{
						slots[j] = itemStack;
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				childByType.SetSlots(slots);
				childByType.UpdateData(slots);
				childByType.IsDirty = true;
				if (recipe.IsScrap)
				{
					QuestEventManager.Current.ScrappedItem(recipe.ingredients[0]);
					entityPlayer.equipment.UnlockCosmeticItem(recipe.ingredients[0].itemValue.ItemClass);
				}
				else
				{
					QuestEventManager.Current.CraftedItem(itemStack);
				}
				if (playSound)
				{
					if (recipe.craftingArea != null)
					{
						WorkstationData workstationData = CraftingManager.GetWorkstationData(recipe.craftingArea);
						if (workstationData != null)
						{
							Manager.PlayInsidePlayerHead(workstationData.CraftCompleteSound);
						}
					}
					else
					{
						Manager.PlayInsidePlayerHead("craft_complete_item");
					}
				}
			}
			else if (!AddItemToInventory())
			{
				isInventoryFull = true;
				string text = "No room in workstation output, crafting has been halted until space is cleared.";
				if (Localization.Exists("wrnWorkstationOutputFull"))
				{
					text = Localization.Get("wrnWorkstationOutputFull");
				}
				GameManager.ShowTooltip(entityPlayer, text);
				Manager.PlayInsidePlayerHead("ui_denied");
				return false;
			}
		}
		else
		{
			if (!base.xui.dragAndDrop.CurrentStack.IsEmpty() && base.xui.dragAndDrop.CurrentStack.itemValue.ItemClass is ItemClassQuest)
			{
				return false;
			}
			ItemStack itemStack2 = new ItemStack(outputItemValue, recipe.count);
			if (!base.xui.PlayerInventory.AddItemNoPartial(itemStack2, _playCollectSound: false))
			{
				if (itemStack2.count != recipe.count)
				{
					base.xui.PlayerInventory.DropItem(itemStack2);
					QuestEventManager.Current.CraftedItem(itemStack2);
					return true;
				}
				isInventoryFull = true;
				string text2 = "No room in inventory, crafting has been halted until space is cleared.";
				if (Localization.Exists("wrnInventoryFull"))
				{
					text2 = Localization.Get("wrnInventoryFull");
				}
				GameManager.ShowTooltip(entityPlayer, text2);
				Manager.PlayInsidePlayerHead("ui_denied");
				return false;
			}
			if (originalItem != null && !originalItem.IsEmpty())
			{
				if (recipe.ingredients.Count > 0)
				{
					QuestEventManager.Current.ScrappedItem(recipe.ingredients[0]);
					entityPlayer.equipment.UnlockCosmeticItem(recipe.ingredients[0].itemValue.ItemClass);
				}
			}
			else
			{
				itemStack2.count = recipe.count - itemStack2.count;
				if (recipe.IsScrap)
				{
					QuestEventManager.Current.ScrappedItem(recipe.ingredients[0]);
					entityPlayer.equipment.UnlockCosmeticItem(recipe.ingredients[0].itemValue.ItemClass);
				}
				else
				{
					QuestEventManager.Current.CraftedItem(itemStack2);
				}
			}
			if (playSound)
			{
				Manager.PlayInsidePlayerHead("craft_complete_item");
			}
		}
		if (!isInventoryFull)
		{
			originalItem = ItemValue.None.Clone();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool AddItemToInventory()
	{
		ItemStack itemStack = new ItemStack(outputItemValue, recipe.count);
		if (!base.xui.PlayerInventory.AddItemNoPartial(itemStack, _playCollectSound: false))
		{
			updateRecipeData();
			return false;
		}
		QuestEventManager.Current.CraftedItem(new ItemStack(outputItemValue, recipe.count));
		isInventoryFull = false;
		if (playSound)
		{
			if (recipe.craftingArea != null)
			{
				WorkstationData workstationData = CraftingManager.GetWorkstationData(recipe.craftingArea);
				if (workstationData != null)
				{
					Manager.PlayInsidePlayerHead(workstationData.CraftCompleteSound);
				}
			}
			else
			{
				Manager.PlayInsidePlayerHead("craft_complete_item");
			}
		}
		if (recipeCount <= 0)
		{
			isCrafting = false;
			if (recipe != null || craftingTimeLeft != 0f)
			{
				ClearRecipe();
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void giveExp(ItemValue _iv, ItemClass _ic)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		int num = (int)base.xui.playerUI.entityPlayer.Buffs.GetCustomVar("_craftCount_" + recipe.GetName());
		base.xui.playerUI.entityPlayer.Buffs.SetCustomVar("_craftCount_" + recipe.GetName(), num + 1);
		base.xui.playerUI.entityPlayer.Progression.AddLevelExp(recipe.craftExpGain / (num + 1), "_xpFromCrafting", Progression.XPTypes.Crafting);
		entityPlayer.totalItemsCrafted++;
		itemCraftedAchievementUpdate();
	}

	public bool HasRecipe()
	{
		return recipe != null;
	}

	public Recipe GetRecipe()
	{
		return recipe;
	}

	public void ClearRecipe()
	{
		SetRecipe(null, 0, 0f, recipeModification: true);
	}

	public int GetRecipeCount()
	{
		return recipeCount;
	}

	public float GetRecipeCraftingTimeLeft()
	{
		return craftingTimeLeft;
	}

	public float GetTotalRecipeCraftingTimeLeft()
	{
		return totalCraftTimeLeft;
	}

	public float GetOneItemCraftTime()
	{
		return oneItemCraftTime;
	}

	public bool SetRepairRecipe(float _repairTimeLeft, ItemValue _itemToRepair, int _amountToRepair)
	{
		if (isCrafting || (originalItem != null && originalItem.type != 0))
		{
			return false;
		}
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		recipeCount = 1;
		craftingTimeLeft = _repairTimeLeft;
		originalItem = _itemToRepair.Clone();
		amountToRepair = _amountToRepair;
		totalCraftTimeLeft = _repairTimeLeft;
		oneItemCraftTime = _repairTimeLeft;
		if (lockIcon != null && _itemToRepair.type != 0)
		{
			((XUiV_Sprite)lockIcon.ViewComponent).SpriteName = "ui_game_symbol_wrench";
		}
		outputQuality = originalItem.Quality;
		StartingEntityId = entityPlayer.entityId;
		recipe = new Recipe();
		recipe.craftingTime = _repairTimeLeft;
		recipe.count = 1;
		recipe.itemValueType = originalItem.type;
		recipe.craftExpGain = Mathf.Clamp(amountToRepair, 0, 200);
		ItemClass itemClass = originalItem.ItemClass;
		if (itemClass.RepairTools != null && itemClass.RepairTools.Length > 0)
		{
			ItemClass itemClass2 = ItemClass.GetItemClass(itemClass.RepairTools[0].Value);
			if (itemClass2 != null)
			{
				int num = Mathf.CeilToInt((float)_amountToRepair / (float)itemClass2.RepairAmount.Value);
				recipe.ingredients.Add(new ItemStack(ItemClass.GetItem(itemClass.RepairTools[0].Value), num));
			}
		}
		updateRecipeData();
		return true;
	}

	public bool SetRecipe(Recipe _recipe, int _count = 1, float craftTime = -1f, bool recipeModification = false, int startingEntityId = -1, int _outputQuality = -1, float _oneItemCraftTime = -1f)
	{
		if ((isCrafting || (recipe != null && _recipe != null)) && !recipeModification)
		{
			return false;
		}
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (startingEntityId == -1)
		{
			startingEntityId = entityPlayer.entityId;
		}
		StartingEntityId = startingEntityId;
		recipe = _recipe;
		recipeCount = _count;
		craftingTimeLeft = ((craftTime != -1f) ? craftTime : (_recipe?.craftingTime ?? 0f));
		if (originalItem != null && !originalItem.Equals(ItemValue.None))
		{
			originalItem = ItemValue.None.Clone();
		}
		amountToRepair = 0;
		oneItemCraftTime = ((_oneItemCraftTime != -1f) ? _oneItemCraftTime : (_recipe?.craftingTime ?? 0f));
		totalCraftTimeLeft = oneItemCraftTime * ((float)_count - 1f) + craftingTimeLeft;
		if (lockIcon != null && recipe != null)
		{
			WorkstationData workstationData = CraftingManager.GetWorkstationData(recipe.craftingArea);
			if (workstationData != null)
			{
				((XUiV_Sprite)lockIcon.ViewComponent).SpriteName = workstationData.CraftIcon;
			}
		}
		if (_outputQuality == -1)
		{
			if (recipe != null)
			{
				outputQuality = recipe.craftingTier;
			}
			else
			{
				outputQuality = 1;
			}
		}
		else
		{
			outputQuality = _outputQuality;
		}
		ClearDisplayFromLastRecipe();
		updateRecipeData();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearDisplayFromLastRecipe()
	{
		if (timer != null)
		{
			((XUiV_Label)timer.ViewComponent).SetTextImmediately("");
			timer.ViewComponent.IsVisible = true;
		}
		if (count != null)
		{
			count.ViewComponent.IsVisible = true;
			((XUiV_Label)count.ViewComponent).SetTextImmediately("");
		}
		if (cancel != null)
		{
			Color color = ((XUiV_Sprite)cancel.ViewComponent).Color;
			((XUiV_Sprite)cancel.ViewComponent).SetColorImmediately(new Color(color.r, color.g, color.b, 0f));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRecipeData()
	{
		if (recipe == null && (originalItem == null || originalItem.type == 0))
		{
			if (lockIcon != null)
			{
				lockIcon.ViewComponent.IsVisible = false;
			}
			if (overlay != null)
			{
				overlay.ViewComponent.IsVisible = false;
			}
			if (itemIcon != null)
			{
				itemIcon.ViewComponent.IsVisible = false;
			}
			if (timer != null)
			{
				timer.ViewComponent.IsVisible = false;
			}
			if (count != null)
			{
				count.ViewComponent.IsVisible = false;
			}
			if (cancel != null)
			{
				cancel.ViewComponent.IsVisible = false;
			}
			return;
		}
		if (lockIcon != null)
		{
			lockIcon.ViewComponent.IsVisible = true;
		}
		if (overlay != null)
		{
			overlay.ViewComponent.IsVisible = true;
		}
		if (itemIcon != null)
		{
			ItemClass itemClass = ((recipe != null) ? ItemClass.GetForId(recipe.itemValueType) : originalItem.ItemClass);
			if (itemClass != null)
			{
				((XUiV_Sprite)itemIcon.ViewComponent).SetSpriteImmediately(itemClass.GetIconName());
				itemIcon.ViewComponent.IsVisible = true;
				((XUiV_Sprite)itemIcon.ViewComponent).Color = itemClass.GetIconTint();
			}
		}
		if (timer != null)
		{
			((XUiV_Label)timer.ViewComponent).SetTextImmediately(craftingTimeToString(totalCraftTimeLeft + 0.5f));
			timer.ViewComponent.IsVisible = true;
		}
		if (count != null)
		{
			count.ViewComponent.IsVisible = true;
			((XUiV_Label)count.ViewComponent).SetTextImmediately((recipeCount * recipe.count).ToString());
		}
		if (cancel != null)
		{
			Color color = ((XUiV_Sprite)cancel.ViewComponent).Color;
			if (isOver && UICamera.hoveredObject == background.ViewComponent.UiTransform.gameObject)
			{
				((XUiV_Sprite)cancel.ViewComponent).Color = new Color(color.r, color.g, color.b, 0.75f);
			}
			else
			{
				((XUiV_Sprite)cancel.ViewComponent).Color = new Color(color.r, color.g, color.b, 0f);
			}
			cancel.ViewComponent.IsVisible = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isInventoryFull = false;
		if (cancel != null)
		{
			isOver = false;
			Color color = ((XUiV_Sprite)cancel.ViewComponent).Color;
			((XUiV_Sprite)cancel.ViewComponent).Color = new Color(color.r, color.g, color.b, 0f);
		}
		playSound = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		playSound = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string craftingTimeToString(float time)
	{
		return string.Format("{0}:{1}", ((int)(time / 60f)).ToString("0").PadLeft(2, '0'), ((int)(time % 60f)).ToString("0").PadLeft(2, '0'));
	}

	public override void Cleanup()
	{
		base.Cleanup();
		stopAchievementUpdateCoroutine();
	}

	public static void HandleCraftXPGained()
	{
		itemCraftedAchievementUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void itemCraftedAchievementUpdate()
	{
		itemsCraftedSinceLastAchievementUpdate++;
		lastItemCraftedTime = Time.unscaledTime;
		startAchievementUpdateCoroutine();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void startAchievementUpdateCoroutine()
	{
		if (sendCraftedItemsForAchievementsCoroutine == null)
		{
			sendCraftedItemsForAchievementsCoroutine = ThreadManager.StartCoroutine(sendCraftedItemsForAchievementsCo());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopAchievementUpdateCoroutine()
	{
		if (sendCraftedItemsForAchievementsCoroutine != null)
		{
			ThreadManager.StopCoroutine(sendCraftedItemsForAchievementsCoroutine);
			sendCraftedItemsForAchievementsCoroutine = null;
			doSendCraftingStatsForAchievements();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator sendCraftedItemsForAchievementsCo()
	{
		if (sendCraftedItemsForAchievementsInterval == null)
		{
			sendCraftedItemsForAchievementsInterval = new WaitForSeconds(30f);
		}
		while (true)
		{
			yield return sendCraftedItemsForAchievementsInterval;
			doSendCraftingStatsForAchievements();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void doSendCraftingStatsForAchievements()
	{
		if (itemsCraftedSinceLastAchievementUpdate != 0 && (itemsCraftedSinceLastAchievementUpdate >= 20 || Time.unscaledTime > lastItemCraftedTime + 15f || sendCraftedItemsForAchievementsCoroutine == null))
		{
			PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.ItemsCrafted, itemsCraftedSinceLastAchievementUpdate);
			itemsCraftedSinceLastAchievementUpdate = 0;
		}
	}
}

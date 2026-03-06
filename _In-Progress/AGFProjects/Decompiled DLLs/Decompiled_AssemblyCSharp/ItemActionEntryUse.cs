using System.Collections;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryUse : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum StateTypes
	{
		Normal,
		RecipeKnown,
		SkillRequirementsNotMet,
		SkillKnown
	}

	public enum ConsumeType
	{
		None,
		Eat,
		Drink,
		Heal,
		Read,
		Quest,
		Open
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ConsumeType consumeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public StateTypes state;

	public ItemActionEntryUse(XUiController _controller, ConsumeType _consumeType)
		: base(_controller, "OVERRIDDEN BELOW", "OVERRIDDEN BELOW", GamepadShortCut.DPadLeft)
	{
		consumeType = _consumeType;
		switch (_consumeType)
		{
		case ConsumeType.Drink:
			base.ActionName = Localization.Get("lblContextActionDrink");
			base.IconName = "ui_game_symbol_water";
			base.SoundName = "";
			break;
		case ConsumeType.Eat:
			base.ActionName = Localization.Get("lblContextActionEat");
			base.IconName = "ui_game_symbol_fork";
			base.SoundName = "";
			break;
		case ConsumeType.Heal:
			base.ActionName = Localization.Get("lblContextActionHeal");
			base.IconName = "ui_game_symbol_medical";
			break;
		case ConsumeType.Read:
			base.ActionName = Localization.Get("lblContextActionRead");
			base.IconName = "ui_game_symbol_book";
			base.SoundName = "";
			break;
		case ConsumeType.Quest:
			base.ActionName = Localization.Get("lblContextActionRead");
			base.IconName = "ui_game_symbol_quest";
			break;
		case ConsumeType.Open:
			base.ActionName = Localization.Get("lblContextActionOpen");
			base.IconName = "ui_game_symbol_treasure";
			break;
		}
		RefreshEnabled();
	}

	public override void OnDisabledActivate()
	{
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		if ((bool)entityPlayer.AttachedToEntity)
		{
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttCannotUseWhileOnVehicle"));
			return;
		}
		if (base.ItemController.xui.PlayerInventory.Toolbelt.IsHoldingItemActionRunning() || base.ItemController.xui.isUsingItemActionEntryUse)
		{
			GameManager.ShowTooltip(entityPlayer, Localization.Get("isBusy"));
			return;
		}
		if (XUiC_AssembleWindowGroup.GetWindowGroup(base.ItemController.xui).IsOpen)
		{
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttCannotUseWhileAssembling"));
			return;
		}
		switch (consumeType)
		{
		case ConsumeType.Drink:
			if (XUiM_Player.GetWaterPercent(entityPlayer) >= 1f)
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("notThirsty"));
			}
			else
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("isBusy"));
			}
			break;
		case ConsumeType.Eat:
			if (XUiM_Player.GetFoodPercent(entityPlayer) >= 1f)
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("notHungry"));
			}
			else
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("isBusy"));
			}
			break;
		case ConsumeType.Heal:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("notHurt"));
			break;
		case ConsumeType.Read:
			switch (state)
			{
			case StateTypes.RecipeKnown:
				GameManager.ShowTooltip(entityPlayer, Localization.Get("alreadyKnown"));
				break;
			case StateTypes.SkillRequirementsNotMet:
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttSkillRequirementsNotMet"));
				break;
			case StateTypes.SkillKnown:
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttSkillMaxLevel"));
				break;
			}
			break;
		case ConsumeType.Quest:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("questunavailable"));
			break;
		}
	}

	public override void RefreshEnabled()
	{
		state = StateTypes.Normal;
		EntityPlayer entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		if ((bool)entityPlayer.AttachedToEntity)
		{
			base.Enabled = false;
			return;
		}
		if (entityPlayer.inventory.IsHoldingItemActionRunning())
		{
			base.Enabled = false;
			return;
		}
		if (XUiC_AssembleWindowGroup.GetWindowGroup(base.ItemController.xui).IsOpen)
		{
			base.Enabled = false;
			return;
		}
		ItemStack itemStack = ((XUiC_ItemStack)base.ItemController).ItemStack;
		switch (consumeType)
		{
		case ConsumeType.Drink:
			base.Enabled = !base.ItemController.xui.isUsingItemActionEntryUse && XUiM_Player.GetWaterPercent(entityPlayer) < 1f;
			break;
		case ConsumeType.Eat:
			base.Enabled = !base.ItemController.xui.isUsingItemActionEntryUse && XUiM_Player.GetFoodPercent(entityPlayer) < 1f;
			break;
		case ConsumeType.Heal:
			base.Enabled = true;
			break;
		case ConsumeType.Read:
			if (itemStack != null && !itemStack.IsEmpty())
			{
				ItemClass forId2 = ItemClass.GetForId(itemStack.itemValue.type);
				bool flag2 = false;
				for (int j = 0; j < forId2.Actions.Length; j++)
				{
					if (!(forId2.Actions[j] is ItemActionLearnRecipe itemActionLearnRecipe))
					{
						continue;
					}
					for (int k = 0; k < itemActionLearnRecipe.RecipesToLearn.Length; k++)
					{
						state = StateTypes.RecipeKnown;
						if (!XUiM_Recipes.GetRecipeIsUnlocked(base.ItemController.xui, itemActionLearnRecipe.RecipesToLearn[k]))
						{
							state = StateTypes.Normal;
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						break;
					}
				}
				base.Enabled = flag2;
			}
			else
			{
				base.Enabled = false;
			}
			break;
		case ConsumeType.Quest:
		{
			if (itemStack == null || itemStack.IsEmpty())
			{
				break;
			}
			ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
			for (int i = 0; i < forId.Actions.Length; i++)
			{
				if (forId.Actions[i] is ItemActionQuest itemActionQuest)
				{
					if (!QuestClass.GetQuest(itemActionQuest.QuestGiven).CanActivate())
					{
						base.Enabled = false;
						break;
					}
					Quest quest = base.ItemController.xui.playerUI.entityPlayer.QuestJournal.FindQuest(itemActionQuest.QuestGiven);
					bool flag = base.ItemController.xui.playerUI.entityPlayer.QuestJournal.FindActiveQuest(itemActionQuest.QuestGiven) != null;
					base.Enabled = quest == null || (QuestClass.GetQuest(itemActionQuest.QuestGiven).Repeatable && !flag);
					break;
				}
			}
			break;
		}
		case ConsumeType.Open:
			base.Enabled = true;
			break;
		}
		Inventory toolbelt = base.ItemController.xui.PlayerInventory.Toolbelt;
		base.Enabled = base.Enabled && toolbelt.GetItem(toolbelt.DUMMY_SLOT_IDX).IsEmpty();
	}

	public override void OnActivated()
	{
		if (base.ItemController.xui.isUsingItemActionEntryUse)
		{
			return;
		}
		XUiC_ItemStack stackControl = (XUiC_ItemStack)base.ItemController;
		ItemClass itemClass = stackControl.ItemStack.itemValue.ItemClass;
		if (!itemClass.CanExecuteAction(0, base.ItemController.xui.playerUI.entityPlayer, stackControl.ItemStack.itemValue))
		{
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("ttCannotUseAtThisTime"), string.Empty, "ui_denied");
			return;
		}
		base.ItemController.xui.isUsingItemActionEntryUse = true;
		ItemStack itemStack = new ItemStack(stackControl.ItemStack.itemValue.Clone(), 1);
		ItemStack originalStack = new ItemStack(stackControl.ItemStack.itemValue.Clone(), stackControl.ItemStack.count);
		Inventory inventory = base.ItemController.xui.PlayerInventory.Toolbelt;
		if (consumeType == ConsumeType.Quest)
		{
			base.ItemController.xui.FindWindowGroupByName("questOffer").GetChildByType<XUiC_QuestOfferWindow>().ItemStackController = stackControl;
			stackControl.QuestLock = true;
		}
		else
		{
			stackControl.HiddenLock = true;
		}
		stackControl.WindowGroup.Controller.SetAllChildrenDirty();
		RefreshEnabled();
		int actionIdx = 0;
		for (int i = 0; i < itemClass.Actions.Length; i++)
		{
			bool flag = false;
			switch (consumeType)
			{
			case ConsumeType.Eat:
			case ConsumeType.Drink:
			case ConsumeType.Heal:
				if (itemClass.Actions[i] != null)
				{
					flag = true;
				}
				break;
			case ConsumeType.Quest:
				if (itemClass.Actions[i] is ItemActionQuest)
				{
					flag = true;
				}
				break;
			case ConsumeType.Read:
				if (itemClass.Actions[i] is ItemActionLearnRecipe)
				{
					flag = true;
				}
				break;
			case ConsumeType.Open:
			{
				ItemAction itemAction = itemClass.Actions[i];
				if (itemAction is ItemActionOpenBundle || itemAction is ItemActionOpenLootBundle)
				{
					flag = true;
				}
				break;
			}
			}
			if (flag)
			{
				actionIdx = i;
				break;
			}
		}
		if (!(itemStack.itemValue.ItemClass.Actions[actionIdx] is ItemActionEat))
		{
			originalStack.count--;
			if (originalStack.count == 0)
			{
				originalStack = ItemStack.Empty.Clone();
			}
		}
		if (consumeType != ConsumeType.Quest)
		{
			stackControl.ItemStack = originalStack;
		}
		if (!itemStack.itemValue.ItemClass.Actions[actionIdx].UseAnimation && itemStack.itemValue.ItemClass.Actions[actionIdx].ExecuteInstantAction(base.ItemController.xui.playerUI.entityPlayer, itemStack, isHeldItem: false, stackControl))
		{
			if (consumeType != ConsumeType.Quest)
			{
				stackControl.HiddenLock = false;
				stackControl.WindowGroup.Controller.SetAllChildrenDirty();
			}
			base.ItemController.xui.isUsingItemActionEntryUse = false;
		}
		else if (itemStack.itemValue.ItemClass.Actions[actionIdx] is ItemActionEat { UsePrompt: not false } itemActionEat)
		{
			base.ItemController.xui.isUsingItemActionEntryPromptComplete = true;
			XUiC_MessageBoxWindowGroup.ShowMessageBox(base.ItemController.xui, Localization.Get(itemActionEat.PromptTitle), Localization.Get(itemActionEat.PromptDescription), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, UseItemWithAnimation, SwitchBack);
		}
		else
		{
			UseItemWithAnimation();
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void SwitchBack()
		{
			GameManager.Instance.StartCoroutine(SwitchBackCoroutine(inventory));
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void UseItemWithAnimation()
		{
			GameManager.Instance.StartCoroutine(UseItemWithAnimationCoroutine());
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		IEnumerator UseItemWithAnimationCoroutine()
		{
			EntityPlayerLocal player = base.ItemController.xui.playerUI.entityPlayer;
			player.MoveController.AllowPlayerInput(allow: false);
			ItemStack finalStack = ItemStack.Empty.Clone();
			stackControl.ItemStack = ((originalStack.count > 1) ? new ItemStack(originalStack.itemValue.Clone(), originalStack.count - 1) : ItemStack.Empty.Clone());
			yield return inventory.SimulateActionExecution(actionIdx, itemStack, [PublicizedFrom(EAccessModifier.Internal)] (ItemStack finalStackTemp) =>
			{
				finalStack = finalStackTemp;
			});
			if (!finalStack.IsEmpty())
			{
				base.ItemController.xui.PlayerInventory.AddItem(finalStack, playCollectSound: false);
			}
			if (!finalStack.IsEmpty())
			{
				base.ItemController.xui.PlayerInventory.DropItem(finalStack);
			}
			stackControl.WindowGroup.Controller.SetAllChildrenDirty();
			inventory.OnUpdate();
			player.MoveController.AllowPlayerInput(allow: true);
			SwitchBack();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SwitchBackCoroutine(Inventory _inventory)
	{
		while (_inventory.IsHolsterDelayActive())
		{
			yield return null;
		}
		((XUiC_ItemStack)base.ItemController).HiddenLock = false;
		base.ParentActionList.RefreshActionList();
		base.ItemController.xui.isUsingItemActionEntryUse = false;
		RefreshEnabled();
	}

	public override void OnTimerCompleted()
	{
	}
}

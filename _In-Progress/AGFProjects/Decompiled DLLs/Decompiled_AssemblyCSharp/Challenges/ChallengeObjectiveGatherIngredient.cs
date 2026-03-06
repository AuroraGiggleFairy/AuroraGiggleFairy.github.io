using System;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveGatherIngredient : ChallengeBaseTrackedItemObjective
{
	public Recipe itemRecipe;

	public int IngredientIndex = -1;

	public int IngredientCount = -1;

	public int NeededCount;

	public int currentNeededCount;

	public BaseRequirementObjectiveGroup Parent;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.GatherIngredient;

	public override string DescriptionText => Localization.Get("challengeObjectiveGather") + " " + expectedItemClass.GetLocalizedItemName();

	public override string StatusText
	{
		get
		{
			int num = Math.Max(0, MaxCount - currentNeededCount);
			if (base.Complete)
			{
				return $"{num}/{num}";
			}
			return $"{current}/{num}";
		}
	}

	public override void Init()
	{
		expectedItem = itemRecipe.ingredients[IngredientIndex].itemValue;
		expectedItemClass = expectedItem.ItemClass;
	}

	public override void HandleAddHooks()
	{
		EntityPlayerLocal player = Owner.Owner.Player;
		LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
		playerInventory.Backpack.OnBackpackItemsChangedInternal -= ItemsChangedInternal;
		playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= ItemsChangedInternal;
		playerInventory.Backpack.OnBackpackItemsChangedInternal += ItemsChangedInternal;
		playerInventory.Toolbelt.OnToolbeltItemsChangedInternal += ItemsChangedInternal;
		player.DragAndDropItemChanged += ItemsChangedInternal;
		base.HandleAddHooks();
		if (trackingEntry != null)
		{
			Owner.AddTrackingEntry(trackingEntry);
			trackingEntry.TrackingHelper = Owner.TrackingHandler;
			trackingEntry.AddHooks();
		}
	}

	public override bool CheckObjectiveComplete(bool handleComplete = true)
	{
		if (CheckForNeededItem())
		{
			base.Current = MaxCount;
			base.Complete = true;
			if (handleComplete)
			{
				Owner.HandleComplete();
			}
			return true;
		}
		base.Complete = false;
		return base.CheckObjectiveComplete(handleComplete);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemsChangedInternal()
	{
		if (CheckObjectiveComplete())
		{
			if (trackingEntry != null)
			{
				trackingEntry.RemoveHooks();
			}
			Parent.CheckPrerequisites();
		}
		else if (trackingEntry != null)
		{
			trackingEntry.AddHooks();
		}
	}

	public override void UpdateStatus()
	{
		base.UpdateStatus();
		if (base.Complete)
		{
			if (trackingEntry != null)
			{
				trackingEntry.RemoveHooks();
			}
		}
		else if (trackingEntry != null)
		{
			trackingEntry.AddHooks();
		}
	}

	public override void HandleRemoveHooks()
	{
		EntityPlayerLocal player = Owner.Owner.Player;
		if (!(player == null))
		{
			LocalPlayerUI.GetUIForPlayer(player);
			XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
			playerInventory.Backpack.OnBackpackItemsChangedInternal -= ItemsChangedInternal;
			playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= ItemsChangedInternal;
			player.DragAndDropItemChanged -= ItemsChangedInternal;
			if (trackingEntry != null)
			{
				trackingEntry.RemoveHooks();
				Owner.RemoveTrackingEntry(trackingEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleUpdatingCurrent()
	{
		base.HandleUpdatingCurrent();
		int count = itemRecipe.ingredients[IngredientIndex].count;
		ItemValue itemValue = new ItemValue(itemRecipe.itemValueType);
		if (itemRecipe.UseIngredientModifier)
		{
			count = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, count, Owner.Owner.Player, itemRecipe, FastTags<TagGroup.Global>.Parse(expectedItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, itemValue.HasQuality ? 1 : 0);
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player);
		CraftingData craftingData = uIForPlayer.xui.GetCraftingData();
		XUiM_PlayerInventory playerInventory = uIForPlayer.xui.PlayerInventory;
		RecipeQueueItem[] recipeQueueItems = craftingData.RecipeQueueItems;
		int num = 0;
		if (recipeQueueItems != null)
		{
			foreach (RecipeQueueItem recipeQueueItem in recipeQueueItems)
			{
				if (recipeQueueItem.Recipe != null && recipeQueueItem.Recipe.itemValueType == itemRecipe.itemValueType)
				{
					num += recipeQueueItem.Recipe.count * recipeQueueItem.Multiplier;
				}
			}
		}
		num += playerInventory.Backpack.GetItemCount(itemValue);
		num += playerInventory.Toolbelt.GetItemCount(itemValue);
		int num2 = IngredientCount * Math.Max(0, NeededCount - num);
		int itemCount = playerInventory.Backpack.GetItemCount(expectedItem);
		itemCount += playerInventory.Toolbelt.GetItemCount(expectedItem);
		if (itemCount > num2)
		{
			itemCount = num2;
		}
		if (current != itemCount)
		{
			base.Current = itemCount;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckForNeededItem()
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player);
		XUiM_PlayerInventory playerInventory = uIForPlayer.xui.PlayerInventory;
		ItemValue itemValue = new ItemValue(itemRecipe.itemValueType);
		RecipeQueueItem[] recipeQueueItems = uIForPlayer.xui.GetCraftingData().RecipeQueueItems;
		int itemCount = playerInventory.Backpack.GetItemCount(expectedItem);
		itemCount += playerInventory.Toolbelt.GetItemCount(expectedItem);
		currentNeededCount = 0;
		currentNeededCount = playerInventory.Backpack.GetItemCount(itemValue);
		currentNeededCount += playerInventory.Toolbelt.GetItemCount(itemValue);
		int num = 0;
		if (recipeQueueItems != null)
		{
			foreach (RecipeQueueItem recipeQueueItem in recipeQueueItems)
			{
				if (recipeQueueItem.Recipe != null && recipeQueueItem.Recipe.itemValueType == itemRecipe.itemValueType)
				{
					num += recipeQueueItem.Recipe.count * recipeQueueItem.Multiplier;
				}
			}
		}
		return itemCount >= IngredientCount * Math.Max(0, NeededCount - (currentNeededCount + num));
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveGatherIngredient
		{
			itemRecipe = itemRecipe,
			IngredientIndex = IngredientIndex,
			expectedItem = expectedItem,
			expectedItemClass = expectedItemClass,
			NeededCount = NeededCount
		};
	}
}

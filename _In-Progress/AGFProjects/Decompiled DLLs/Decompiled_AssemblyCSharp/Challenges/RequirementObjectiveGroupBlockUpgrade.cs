using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class RequirementObjectiveGroupBlockUpgrade : BaseRequirementObjectiveGroup
{
	public string ItemID = "";

	public string NeededResourceID = "";

	public int NeededResourceCount = 1;

	public Recipe ResourceRecipe;

	public RequirementObjectiveGroupBlockUpgrade(string itemID, string neededResourceID, int neededResourceCount)
	{
		ItemID = itemID;
		NeededResourceID = neededResourceID;
		NeededResourceCount = neededResourceCount;
	}

	public override void CreateRequirements()
	{
		if (PhaseList == null)
		{
			PhaseList = new List<RequirementGroupPhase>();
		}
		RequirementGroupPhase requirementGroupPhase = null;
		ResourceRecipe = CraftingManager.GetRecipe(NeededResourceID);
		ChallengeObjectiveGather challengeObjectiveGather = null;
		if (ResourceRecipe == null || (ResourceRecipe != null && ResourceRecipe.ingredients.Count == 0))
		{
			requirementGroupPhase = new RequirementGroupPhase();
			challengeObjectiveGather = new ChallengeObjectiveGather();
			challengeObjectiveGather.Owner = Owner;
			challengeObjectiveGather.IsRequirement = true;
			challengeObjectiveGather.Parent = this;
			challengeObjectiveGather.SetupItem(NeededResourceID);
			challengeObjectiveGather.MaxCount = NeededResourceCount;
			challengeObjectiveGather.Init();
			requirementGroupPhase.AddChallengeObjective(challengeObjectiveGather);
			PhaseList.Add(requirementGroupPhase);
		}
		else
		{
			requirementGroupPhase = AddIngredientGatheringReqs();
			if (requirementGroupPhase != null)
			{
				PhaseList.Add(requirementGroupPhase);
				requirementGroupPhase = new RequirementGroupPhase();
				ChallengeObjectiveCraft challengeObjectiveCraft = new ChallengeObjectiveCraft();
				challengeObjectiveCraft.Owner = Owner;
				challengeObjectiveCraft.SetupItem(NeededResourceID);
				challengeObjectiveCraft.IsRequirement = true;
				challengeObjectiveCraft.MaxCount = NeededResourceCount;
				challengeObjectiveCraft.Init();
				requirementGroupPhase.AddChallengeObjective(challengeObjectiveCraft);
				PhaseList.Add(requirementGroupPhase);
			}
		}
		requirementGroupPhase = new RequirementGroupPhase();
		ChallengeObjectiveHold challengeObjectiveHold = new ChallengeObjectiveHold();
		challengeObjectiveHold.Owner = Owner;
		challengeObjectiveHold.itemClassID = ItemID;
		challengeObjectiveHold.IsRequirement = true;
		challengeObjectiveHold.MaxCount = 1;
		challengeObjectiveHold.Init();
		requirementGroupPhase.AddChallengeObjective(challengeObjectiveHold);
		PhaseList.Add(requirementGroupPhase);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RequirementGroupPhase AddIngredientGatheringReqs()
	{
		Recipe recipe = CraftingManager.GetRecipe(NeededResourceID);
		if (recipe == null)
		{
			return null;
		}
		RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
		int craftingTier = (recipe.GetOutputItemClass().HasQuality ? 1 : 0);
		for (int i = 0; i < recipe.ingredients.Count; i++)
		{
			int num = recipe.ingredients[i].count;
			if (recipe.UseIngredientModifier)
			{
				num = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, num, Owner.Owner.Player, recipe, FastTags<TagGroup.Global>.Parse(recipe.ingredients[i].itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, craftingTier);
			}
			if (num != 0)
			{
				ChallengeObjectiveGatherIngredient challengeObjectiveGatherIngredient = new ChallengeObjectiveGatherIngredient();
				challengeObjectiveGatherIngredient.Owner = Owner;
				challengeObjectiveGatherIngredient.Parent = this;
				challengeObjectiveGatherIngredient.IsRequirement = true;
				challengeObjectiveGatherIngredient.itemRecipe = recipe;
				challengeObjectiveGatherIngredient.IngredientIndex = i;
				challengeObjectiveGatherIngredient.IngredientCount = num;
				challengeObjectiveGatherIngredient.NeededCount = NeededResourceCount;
				challengeObjectiveGatherIngredient.Init();
				challengeObjectiveGatherIngredient.MaxCount = num * NeededResourceCount;
				requirementGroupPhase.AddChallengeObjective(challengeObjectiveGatherIngredient);
			}
		}
		return requirementGroupPhase;
	}

	public override bool HasPrerequisiteCondition()
	{
		_ = Owner.Owner.Player;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player);
		XUiM_PlayerInventory playerInventory = uIForPlayer.xui.PlayerInventory;
		ItemClass holdingItem = Owner.Owner.Player.inventory.holdingItem;
		if (playerInventory.HasItem(new ItemStack(ItemClass.GetItem(NeededResourceID), NeededResourceCount)) && (playerInventory.HasItem(ItemClass.GetItem(ItemID)) || CheckDragDropItem(uIForPlayer.xui.dragAndDrop.CurrentStack)))
		{
			return holdingItem.Name != ItemID;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckResourceDragDropItem(ItemStack stack)
	{
		if (stack.IsEmpty())
		{
			return false;
		}
		if (stack.itemValue.ItemClass.GetItemName() == NeededResourceID)
		{
			return stack.count >= NeededResourceCount;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckDragDropItem(ItemStack stack)
	{
		if (stack.IsEmpty())
		{
			return false;
		}
		return stack.itemValue.ItemClass.GetItemName() == ItemID;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CheckPhaseStatus(int index)
	{
		EntityPlayerLocal player = Owner.Owner.Player;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
		ItemClass holdingItem = Owner.Owner.Player.inventory.holdingItem;
		if (playerInventory.HasItem(ItemClass.GetItem(ItemID)) && playerInventory.HasItem(new ItemStack(ItemClass.GetItem(NeededResourceID), NeededResourceCount)) && holdingItem.Name == ItemID)
		{
			return false;
		}
		if (ResourceRecipe == null || (ResourceRecipe != null && ResourceRecipe.ingredients.Count == 0))
		{
			switch (index)
			{
			case 0:
				if (!playerInventory.HasItem(new ItemStack(ItemClass.GetItem(NeededResourceID), NeededResourceCount)))
				{
					return !CheckResourceDragDropItem(uIForPlayer.xui.dragAndDrop.CurrentStack);
				}
				return false;
			case 1:
				if (!playerInventory.HasItem(new ItemStack(ItemClass.GetItem(NeededResourceID), NeededResourceCount)) || playerInventory.HasItem(ItemClass.GetItem(ItemID)) || CheckDragDropItem(uIForPlayer.xui.dragAndDrop.CurrentStack))
				{
					return holdingItem.Name != ItemID;
				}
				return true;
			}
		}
		else
		{
			switch (index)
			{
			case 0:
			case 1:
				if (!playerInventory.HasItem(new ItemStack(ItemClass.GetItem(NeededResourceID), NeededResourceCount)))
				{
					return !CheckResourceDragDropItem(uIForPlayer.xui.dragAndDrop.CurrentStack);
				}
				return false;
			case 2:
				if (!playerInventory.HasItem(new ItemStack(ItemClass.GetItem(NeededResourceID), NeededResourceCount)) || playerInventory.HasItem(ItemClass.GetItem(ItemID)) || CheckDragDropItem(uIForPlayer.xui.dragAndDrop.CurrentStack))
				{
					return holdingItem.Name != ItemID;
				}
				return true;
			}
		}
		return true;
	}

	public override BaseRequirementObjectiveGroup Clone()
	{
		return new RequirementObjectiveGroupBlockUpgrade(ItemID, NeededResourceID, NeededResourceCount);
	}
}

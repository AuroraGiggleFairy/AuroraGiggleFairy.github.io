using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class RequirementObjectiveGroupPlace : BaseRequirementObjectiveGroup
{
	public string ItemID = "";

	public RequirementObjectiveGroupPlace(string itemID)
	{
		ItemID = itemID;
	}

	public override void CreateRequirements()
	{
		if (PhaseList == null)
		{
			PhaseList = new List<RequirementGroupPhase>();
		}
		PhaseList.Add(AddIngredientGatheringReqs());
		RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
		ChallengeObjectiveCraft challengeObjectiveCraft = new ChallengeObjectiveCraft();
		challengeObjectiveCraft.Owner = Owner;
		challengeObjectiveCraft.SetupItem(ItemID);
		challengeObjectiveCraft.IsRequirement = true;
		challengeObjectiveCraft.MaxCount = 1;
		challengeObjectiveCraft.Init();
		requirementGroupPhase.AddChallengeObjective(challengeObjectiveCraft);
		PhaseList.Add(requirementGroupPhase);
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
		Recipe recipe = CraftingManager.GetRecipe(ItemID);
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
				challengeObjectiveGatherIngredient.NeededCount = 1;
				challengeObjectiveGatherIngredient.MaxCount = num;
				challengeObjectiveGatherIngredient.Init();
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
		if (playerInventory.HasItem(ItemClass.GetItem(ItemID)) || CheckDragDropItem(uIForPlayer.xui.dragAndDrop.CurrentStack, ItemID))
		{
			return holdingItem.Name != ItemID;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckDragDropItem(ItemStack stack, string itemID)
	{
		if (stack.IsEmpty())
		{
			return false;
		}
		return stack.itemValue.ItemClass.GetItemName() == itemID;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CheckPhaseStatus(int index)
	{
		EntityPlayerLocal player = Owner.Owner.Player;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
		ItemClass holdingItem = Owner.Owner.Player.inventory.holdingItem;
		if (playerInventory.HasItem(ItemClass.GetItem(ItemID)) && holdingItem.Name == ItemID)
		{
			return false;
		}
		switch (index)
		{
		case 0:
		case 1:
			if (!playerInventory.HasItem(ItemClass.GetItem(ItemID)))
			{
				return !CheckDragDropItem(uIForPlayer.xui.dragAndDrop.CurrentStack, ItemID);
			}
			return false;
		case 2:
			if (playerInventory.HasItem(ItemClass.GetItem(ItemID)) || CheckDragDropItem(uIForPlayer.xui.dragAndDrop.CurrentStack, ItemID))
			{
				return holdingItem.Name != ItemID;
			}
			return false;
		default:
			return true;
		}
	}

	public override BaseRequirementObjectiveGroup Clone()
	{
		return new RequirementObjectiveGroupPlace(ItemID);
	}
}

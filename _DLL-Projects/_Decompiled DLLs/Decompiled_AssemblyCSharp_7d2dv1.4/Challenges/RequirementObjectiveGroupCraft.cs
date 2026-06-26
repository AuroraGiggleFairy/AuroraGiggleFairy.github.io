using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class RequirementObjectiveGroupCraft : BaseRequirementObjectiveGroup
{
	public string ItemID = "";

	public Recipe ItemRecipe;

	public RequirementObjectiveGroupCraft(string itemID)
	{
		ItemID = itemID;
		ItemRecipe = CraftingManager.GetRecipe(itemID);
	}

	public override void CreateRequirements()
	{
		if (PhaseList == null)
		{
			PhaseList = new List<RequirementGroupPhase>();
		}
		RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
		ChallengeObjectiveCraft challengeObjectiveCraft = new ChallengeObjectiveCraft();
		challengeObjectiveCraft.Owner = Owner;
		challengeObjectiveCraft.SetupItem(ItemID);
		challengeObjectiveCraft.IsRequirement = true;
		challengeObjectiveCraft.MaxCount = 1;
		challengeObjectiveCraft.Init();
		requirementGroupPhase.AddChallengeObjective(challengeObjectiveCraft);
		PhaseList.Add(requirementGroupPhase);
	}

	public override bool HasPrerequisiteCondition()
	{
		EntityPlayerLocal player = Owner.Owner.Player;
		LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
		int craftingTier = (ItemRecipe.GetOutputItemClass().HasQuality ? 1 : 0);
		for (int i = 0; i < ItemRecipe.ingredients.Count; i++)
		{
			ItemStack itemStack = ItemRecipe.ingredients[i].Clone();
			if (ItemRecipe.UseIngredientModifier)
			{
				itemStack.count = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, itemStack.count, Owner.Owner.Player, ItemRecipe, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, craftingTier);
			}
			if (itemStack.count != 0 && !playerInventory.HasItem(itemStack))
			{
				return false;
			}
		}
		return true;
	}

	public override BaseRequirementObjectiveGroup Clone()
	{
		return new RequirementObjectiveGroupCraft(ItemID)
		{
			ItemRecipe = ItemRecipe
		};
	}
}

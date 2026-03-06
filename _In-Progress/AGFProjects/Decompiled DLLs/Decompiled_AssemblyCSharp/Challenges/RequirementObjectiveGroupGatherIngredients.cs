using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class RequirementObjectiveGroupGatherIngredients : BaseRequirementObjectiveGroup
{
	public string ItemID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe itemRecipe;

	public ChallengeObjectiveCraft CraftObj;

	public RequirementObjectiveGroupGatherIngredients(string itemID)
	{
		ItemID = itemID;
		itemRecipe = CraftingManager.GetRecipe(itemID);
	}

	public override void CreateRequirements()
	{
		if (PhaseList == null)
		{
			PhaseList = new List<RequirementGroupPhase>();
		}
		RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
		int craftingTier = (itemRecipe.GetOutputItemClass().HasQuality ? 1 : 0);
		for (int i = 0; i < itemRecipe.ingredients.Count; i++)
		{
			int num = itemRecipe.ingredients[i].count;
			if (itemRecipe.UseIngredientModifier)
			{
				num = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, num, Owner.Owner.Player, itemRecipe, FastTags<TagGroup.Global>.Parse(itemRecipe.ingredients[i].itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, craftingTier);
			}
			if (num != 0)
			{
				ChallengeObjectiveGatherIngredient challengeObjectiveGatherIngredient = new ChallengeObjectiveGatherIngredient();
				challengeObjectiveGatherIngredient.Parent = this;
				challengeObjectiveGatherIngredient.Owner = Owner;
				challengeObjectiveGatherIngredient.IsRequirement = true;
				challengeObjectiveGatherIngredient.itemRecipe = itemRecipe;
				challengeObjectiveGatherIngredient.IngredientIndex = i;
				challengeObjectiveGatherIngredient.IngredientCount = num;
				challengeObjectiveGatherIngredient.NeededCount = ((CraftObj == null) ? 1 : CraftObj.MaxCount);
				challengeObjectiveGatherIngredient.MaxCount = num * challengeObjectiveGatherIngredient.NeededCount;
				challengeObjectiveGatherIngredient.Init();
				requirementGroupPhase.AddChallengeObjective(challengeObjectiveGatherIngredient);
			}
		}
		PhaseList.Add(requirementGroupPhase);
	}

	public override bool HasPrerequisiteCondition()
	{
		return true;
	}

	public override BaseRequirementObjectiveGroup Clone()
	{
		return new RequirementObjectiveGroupGatherIngredients(ItemID);
	}
}

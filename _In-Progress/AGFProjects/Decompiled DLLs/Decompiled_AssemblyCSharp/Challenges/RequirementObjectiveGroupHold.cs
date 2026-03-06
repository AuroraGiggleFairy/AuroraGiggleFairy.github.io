using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class RequirementObjectiveGroupHold : BaseRequirementObjectiveGroup
{
	public string ItemID = "";

	public RequirementObjectiveGroupHold(string itemID)
	{
		ItemID = itemID;
	}

	public override void CreateRequirements()
	{
		if (PhaseList == null)
		{
			PhaseList = new List<RequirementGroupPhase>();
		}
		RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
		ChallengeObjectiveHold challengeObjectiveHold = new ChallengeObjectiveHold();
		challengeObjectiveHold.Owner = Owner;
		challengeObjectiveHold.itemClassID = ItemID;
		challengeObjectiveHold.IsRequirement = true;
		challengeObjectiveHold.MaxCount = 1;
		challengeObjectiveHold.Init();
		requirementGroupPhase.AddChallengeObjective(challengeObjectiveHold);
		PhaseList.Add(requirementGroupPhase);
	}

	public override bool HasPrerequisiteCondition()
	{
		EntityPlayerLocal player = Owner.Owner.Player;
		LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
		ItemClass holdingItem = Owner.Owner.Player.inventory.holdingItem;
		if (playerInventory.HasItem(ItemClass.GetItem(ItemID)))
		{
			return holdingItem.Name != ItemID;
		}
		return false;
	}

	public override BaseRequirementObjectiveGroup Clone()
	{
		return new RequirementObjectiveGroupHold(ItemID);
	}
}

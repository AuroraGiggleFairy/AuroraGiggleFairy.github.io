using UnityEngine.Scripting;

namespace Quests.Requirements;

[Preserve]
public class RequirementWearing : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	public override void SetupRequirement()
	{
		string arg = Localization.Get("RequirementWearing_keyword");
		expectedItem = ItemClass.GetItem(base.ID);
		expectedItemClass = ItemClass.GetItemClass(base.ID);
		base.Description = $"{arg} {expectedItemClass.GetLocalizedItemName()}";
	}

	public override bool CheckRequirement()
	{
		if (!base.OwnerQuest.Active)
		{
			return true;
		}
		return LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerEquipment.IsWearing(expectedItem);
	}

	public override BaseRequirement Clone()
	{
		return new RequirementWearing
		{
			ID = base.ID,
			Value = base.Value,
			Phase = base.Phase
		};
	}
}

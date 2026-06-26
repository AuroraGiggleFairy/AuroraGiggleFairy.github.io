using UnityEngine.Scripting;

namespace Quests.Requirements;

[Preserve]
public class RequirementHolding : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	public override void SetupRequirement()
	{
		XUi xui = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui;
		string arg = Localization.Get("RequirementHolding_keyword");
		expectedItem = ((base.ID != "" && base.ID != null) ? ItemClass.GetItem(base.ID) : xui.PlayerInventory.Toolbelt.GetBareHandItemValue());
		expectedItemClass = ((base.ID != "" && base.ID != null) ? ItemClass.GetItemClass(base.ID) : xui.PlayerInventory.Toolbelt.GetBareHandItem());
		if (base.ID == "" || base.ID == null)
		{
			base.Description = "Bare Hands";
		}
		else
		{
			base.Description = $"{arg} {expectedItemClass.GetLocalizedItemName()}";
		}
	}

	public override bool CheckRequirement()
	{
		if (!base.OwnerQuest.Active)
		{
			return true;
		}
		return LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory.Toolbelt.holdingItemStack.itemValue.type == expectedItem.type;
	}

	public override BaseRequirement Clone()
	{
		return new RequirementHolding
		{
			ID = base.ID,
			Value = base.Value,
			Phase = base.Phase
		};
	}
}

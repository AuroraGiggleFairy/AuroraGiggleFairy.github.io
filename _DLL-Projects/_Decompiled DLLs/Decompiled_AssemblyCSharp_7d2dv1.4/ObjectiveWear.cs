using UnityEngine.Scripting;

[Preserve]
public class ObjectiveWear : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveWear_keyword");
		expectedItem = ItemClass.GetItem(ID);
		expectedItemClass = ItemClass.GetItemClass(ID);
	}

	public override void SetupDisplay()
	{
		_ = base.CurrentValue;
		base.Description = string.Format(keyword, expectedItemClass.GetLocalizedItemName());
		StatusText = "";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.WearItem += Current_WearItem;
		XUi xui = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui;
		_ = xui.PlayerInventory;
		if (xui.PlayerEquipment.IsWearing(expectedItem) && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 1;
			Refresh();
		}
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.WearItem -= Current_WearItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_WearItem(ItemValue itemValue)
	{
		if (itemValue.type == expectedItem.type && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 1;
			Refresh();
		}
	}

	public override void Refresh()
	{
		if (!base.Complete)
		{
			bool complete = base.CurrentValue == 1;
			base.Complete = complete;
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveWear objectiveWear = new ObjectiveWear();
		CopyValues(objectiveWear);
		return objectiveWear;
	}
}

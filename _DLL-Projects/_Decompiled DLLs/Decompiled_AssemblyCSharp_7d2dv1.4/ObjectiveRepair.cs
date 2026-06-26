using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveRepair : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public int repairCount;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveRepair_keyword");
		expectedItem = ItemClass.GetItem(ID);
		expectedItemClass = ItemClass.GetItemClass(ID);
		repairCount = Convert.ToInt32(Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(keyword, expectedItemClass.GetLocalizedItemName());
		StatusText = $"{base.CurrentValue}/{repairCount}";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.RepairItem += Current_RepairItem;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RepairItem -= Current_RepairItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_RepairItem(ItemValue itemValue)
	{
		if (!base.Complete && itemValue.type == expectedItem.type && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue++;
			Refresh();
		}
	}

	public override void Refresh()
	{
		if (!base.Complete)
		{
			base.Complete = base.CurrentValue >= repairCount;
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveRepair objectiveRepair = new ObjectiveRepair();
		CopyValues(objectiveRepair);
		return objectiveRepair;
	}
}

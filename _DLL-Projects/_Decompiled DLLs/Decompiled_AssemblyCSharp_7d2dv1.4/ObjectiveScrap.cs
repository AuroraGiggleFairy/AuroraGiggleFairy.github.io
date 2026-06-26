using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveScrap : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public int scrapCount;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveScrap_keyword");
		expectedItem = ItemClass.GetItem(ID);
		expectedItemClass = ItemClass.GetItemClass(ID);
		scrapCount = Convert.ToInt32(Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(keyword, expectedItemClass.GetLocalizedItemName());
		StatusText = $"{base.CurrentValue}/{scrapCount}";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.ScrapItem += Current_ScrapItem;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.ScrapItem -= Current_ScrapItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ScrapItem(ItemStack stack)
	{
		if (!base.Complete && stack.itemValue.type == expectedItem.type && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue += (byte)stack.count;
			Refresh();
		}
	}

	public override void Refresh()
	{
		if (!base.Complete)
		{
			base.Complete = base.CurrentValue >= scrapCount;
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveScrap objectiveScrap = new ObjectiveScrap();
		CopyValues(objectiveScrap);
		return objectiveScrap;
	}
}

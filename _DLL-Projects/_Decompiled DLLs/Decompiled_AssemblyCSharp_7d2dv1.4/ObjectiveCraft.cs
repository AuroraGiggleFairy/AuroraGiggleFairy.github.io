using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveCraft : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemCount;

	public static string PropItem = "item";

	public static string PropCount = "count";

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.craftingTag);
	}

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveCraft_keyword");
		expectedItem = ItemClass.GetItem(ID);
		expectedItemClass = ItemClass.GetItemClass(ID);
		itemCount = Convert.ToInt32(Value);
	}

	public override void SetupDisplay()
	{
		string arg = ((ID != "" && ID != null) ? Localization.Get(ID) : "Any Item");
		base.Description = string.Format(keyword, arg);
		StatusText = $"{base.CurrentValue}/{itemCount}";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.CraftItem -= Current_CraftItem;
		QuestEventManager.Current.CraftItem += Current_CraftItem;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.CraftItem -= Current_CraftItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_CraftItem(ItemStack stack)
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
			base.Complete = base.CurrentValue >= itemCount;
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveCraft objectiveCraft = new ObjectiveCraft();
		CopyValues(objectiveCraft);
		return objectiveCraft;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropItem))
		{
			ID = properties.Values[PropItem];
		}
		if (properties.Values.ContainsKey(PropCount))
		{
			Value = properties.Values[PropCount];
		}
	}
}

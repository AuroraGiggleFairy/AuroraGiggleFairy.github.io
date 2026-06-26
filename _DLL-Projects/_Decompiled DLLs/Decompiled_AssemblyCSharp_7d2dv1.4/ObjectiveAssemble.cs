using UnityEngine.Scripting;

[Preserve]
public class ObjectiveAssemble : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool assembled;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveAssemble_keyword");
		expectedItem = ItemClass.GetItem(ID);
		expectedItemClass = ItemClass.GetItemClass(ID);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(keyword, expectedItemClass.GetLocalizedItemName());
		StatusText = "";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AssembleItem += Current_AssembleItem;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.AssembleItem -= Current_AssembleItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_AssembleItem(ItemStack stack)
	{
		if (stack.itemValue.type == expectedItem.type && base.OwnerQuest.CheckRequirements())
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
		ObjectiveAssemble objectiveAssemble = new ObjectiveAssemble();
		CopyValues(objectiveAssemble);
		return objectiveAssemble;
	}
}

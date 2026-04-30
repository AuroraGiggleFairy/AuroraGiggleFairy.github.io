using UnityEngine.Scripting;

[Preserve]
public class ObjectiveOpenWindow : BaseObjective
{
	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveOpenWindow_keyword");
	}

	public override void SetupDisplay()
	{
		_ = base.CurrentValue;
		base.Description = string.Format(keyword, ID);
		StatusText = "";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.WindowChanged += Current_WindowOpened;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.WindowChanged -= Current_WindowOpened;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_WindowOpened(string windowName)
	{
		if (windowName.EqualsCaseInsensitive(ID) && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 1;
			Refresh();
		}
	}

	public override void Refresh()
	{
		bool complete = base.CurrentValue == 1;
		base.Complete = complete;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion();
		}
	}

	public override void RemoveObjectives()
	{
	}

	public override BaseObjective Clone()
	{
		ObjectiveOpenWindow objectiveOpenWindow = new ObjectiveOpenWindow();
		CopyValues(objectiveOpenWindow);
		return objectiveOpenWindow;
	}
}

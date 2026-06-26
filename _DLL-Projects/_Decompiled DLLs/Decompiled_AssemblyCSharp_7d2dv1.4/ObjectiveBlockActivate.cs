using UnityEngine.Scripting;

[Preserve]
public class ObjectiveBlockActivate : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Boolean;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveBlockActivate_keyword");
		localizedName = ((ID != "" && ID != null) ? Localization.Get(ID) : "Any Block");
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(keyword, localizedName);
	}

	public override void AddHooks()
	{
	}

	public override void RemoveHooks()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockActivate(string blockName)
	{
		if (!base.Complete && (ID == null || ID == "" || ID.EqualsCaseInsensitive(blockName)) && base.OwnerQuest.CheckRequirements())
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

	public override BaseObjective Clone()
	{
		ObjectiveBlockActivate objectiveBlockActivate = new ObjectiveBlockActivate();
		CopyValues(objectiveBlockActivate);
		return objectiveBlockActivate;
	}
}

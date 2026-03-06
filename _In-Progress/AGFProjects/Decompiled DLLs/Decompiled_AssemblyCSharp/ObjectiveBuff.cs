using UnityEngine.Scripting;

[Preserve]
public class ObjectiveBuff : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveBuff_keyword");
		name = BuffManager.GetBuff(ID).Name;
	}

	public override void SetupDisplay()
	{
		_ = base.CurrentValue;
		base.Description = string.Format(keyword, name);
		StatusText = "";
	}

	public override void AddHooks()
	{
	}

	public override void RemoveHooks()
	{
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
		ObjectiveBuff objectiveBuff = new ObjectiveBuff();
		CopyValues(objectiveBuff);
		return objectiveBuff;
	}

	public override string ParseBinding(string bindingName)
	{
		string iD = ID;
		_ = Value;
		if (bindingName == "name")
		{
			return BuffManager.GetBuff(iD).Name;
		}
		return "";
	}
}

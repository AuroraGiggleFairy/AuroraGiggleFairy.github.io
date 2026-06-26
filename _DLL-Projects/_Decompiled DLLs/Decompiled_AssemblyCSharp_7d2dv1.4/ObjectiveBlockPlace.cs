using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveBlockPlace : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int neededCount;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveBlockPlace_keyword");
		localizedName = ((ID != "" && ID != null) ? Localization.Get(ID) : "Any Block");
		neededCount = Convert.ToInt32(Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(keyword, localizedName);
		StatusText = $"{base.CurrentValue}/{neededCount}";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.BlockPlace -= Current_BlockPlace;
		QuestEventManager.Current.BlockPlace += Current_BlockPlace;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.BlockPlace -= Current_BlockPlace;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockPlace(string blockname, Vector3i blockPos)
	{
		if (base.Complete)
		{
			return;
		}
		bool flag = false;
		if (ID == null || ID == "" || ID.EqualsCaseInsensitive(blockname))
		{
			flag = true;
		}
		if (!flag && ID != null && ID != "")
		{
			Block blockByName = Block.GetBlockByName(ID, _caseInsensitive: true);
			if (blockByName != null && blockByName.SelectAlternates && blockByName.ContainsAlternateBlock(blockname))
			{
				flag = true;
			}
		}
		if (flag && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue++;
			Refresh();
		}
	}

	public override void Refresh()
	{
		if (base.CurrentValue > neededCount)
		{
			base.CurrentValue = (byte)neededCount;
		}
		if (!base.Complete)
		{
			base.Complete = base.CurrentValue >= neededCount;
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveBlockPlace objectiveBlockPlace = new ObjectiveBlockPlace();
		CopyValues(objectiveBlockPlace);
		return objectiveBlockPlace;
	}
}

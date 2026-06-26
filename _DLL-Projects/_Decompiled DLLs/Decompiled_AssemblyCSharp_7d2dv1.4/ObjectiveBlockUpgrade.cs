using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveBlockUpgrade : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int neededCount;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveBlockUpgrade_keyword");
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
		QuestEventManager.Current.BlockUpgrade -= Current_BlockUpgrade;
		QuestEventManager.Current.BlockUpgrade += Current_BlockUpgrade;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.BlockUpgrade -= Current_BlockUpgrade;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockUpgrade(string blockname, Vector3i blockPos)
	{
		if (base.Complete)
		{
			return;
		}
		bool flag = false;
		if (ID == null || ID == "")
		{
			flag = true;
		}
		else
		{
			if (ID.EqualsCaseInsensitive(blockname))
			{
				flag = true;
			}
			if (blockname.Contains(":") && ID.EqualsCaseInsensitive(blockname.Substring(0, blockname.IndexOf(':'))))
			{
				flag = true;
			}
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
		ObjectiveBlockUpgrade objectiveBlockUpgrade = new ObjectiveBlockUpgrade();
		CopyValues(objectiveBlockUpgrade);
		return objectiveBlockUpgrade;
	}
}

using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveStatAwarded : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string statName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int neededCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStatName = "stat_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStatText = "stat_text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStatTextKey = "stat_text_key";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropNeededCount = "count";

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override void SetupObjective()
	{
		keyword = statText;
		neededCount = Convert.ToInt32(Value);
	}

	public override void SetupDisplay()
	{
		base.Description = statText;
		StatusText = $"{base.CurrentValue}/{neededCount}";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.QuestAwardCredit += Current_QuestAwardCredit;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.QuestAwardCredit -= Current_QuestAwardCredit;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_QuestAwardCredit(string stat, int awardCount)
	{
		if (!base.Complete && statName.EqualsCaseInsensitive(stat))
		{
			base.CurrentValue += (byte)awardCount;
			if (base.CurrentValue >= neededCount)
			{
				Refresh();
			}
		}
	}

	public override void Refresh()
	{
		if (base.CurrentValue > neededCount)
		{
			base.CurrentValue = (byte)neededCount;
		}
		base.Complete = base.CurrentValue >= neededCount;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion();
		}
	}

	public override void RemoveObjectives()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveStatAwarded obj = (ObjectiveStatAwarded)objective;
		obj.statName = statName;
		obj.statText = statText;
	}

	public override BaseObjective Clone()
	{
		ObjectiveStatAwarded objectiveStatAwarded = new ObjectiveStatAwarded();
		CopyValues(objectiveStatAwarded);
		return objectiveStatAwarded;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		string optionalValue = "";
		properties.ParseString(PropStatName, ref statName);
		properties.ParseString(PropStatText, ref statText);
		if (properties.Contains(PropNeededCount))
		{
			int optionalValue2 = 0;
			properties.ParseInt(PropNeededCount, ref optionalValue2);
			Value = optionalValue2.ToString();
		}
		properties.ParseString(PropStatTextKey, ref optionalValue);
		if (optionalValue != "")
		{
			statText = Localization.Get(optionalValue);
		}
	}
}

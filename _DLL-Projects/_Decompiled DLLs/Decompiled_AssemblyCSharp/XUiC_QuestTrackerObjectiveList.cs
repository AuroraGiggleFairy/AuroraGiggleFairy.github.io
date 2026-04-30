using System.Collections.Generic;
using Challenges;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTrackerObjectiveList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Quest quest;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge challenge;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> objectiveEntries = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	public string completeIconName = "";

	public string incompleteIconName = "";

	public string completeHexColor = "FF00FF00";

	public string incompleteHexColor = "FFB400";

	public string warningHexColor = "FFFF00FF";

	public string inactiveHexColor = "888888FF";

	public string activeHexColor = "FFFFFFFF";

	public string completeColor = "0,255,0,255";

	public string incompleteColor = "255, 180, 0, 255";

	public string warningColor = "255,255,0,255";

	public Quest Quest
	{
		get
		{
			return quest;
		}
		set
		{
			quest = value;
			isDirty = true;
		}
	}

	public Challenge Challenge
	{
		get
		{
			return challenge;
		}
		set
		{
			if (challenge != null)
			{
				challenge.OnChallengeStateChanged -= CurrentChallenge_OnChallengeStateChanged;
			}
			challenge = value;
			if (challenge != null)
			{
				challenge.OnChallengeStateChanged += CurrentChallenge_OnChallengeStateChanged;
			}
			isDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CurrentChallenge_OnChallengeStateChanged(Challenge challenge)
	{
		isDirty = true;
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_QuestTrackerObjectiveEntry>();
		XUiController[] array = childrenByType;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				objectiveEntries.Add(array[i]);
			}
		}
	}

	public override void Update(float _dt)
	{
		if (isDirty)
		{
			if (quest != null)
			{
				int count = objectiveEntries.Count;
				int count2 = quest.Objectives.Count;
				int num = 0;
				for (int i = 0; i < count2; i++)
				{
					if ((quest.Objectives[i].Phase != quest.CurrentPhase && quest.Objectives[i].Phase != 0) || quest.Objectives[i].HiddenObjective)
					{
						continue;
					}
					if (num < count && objectiveEntries[num] is XUiC_QuestTrackerObjectiveEntry xUiC_QuestTrackerObjectiveEntry)
					{
						xUiC_QuestTrackerObjectiveEntry.Owner = this;
						if (i < count2)
						{
							xUiC_QuestTrackerObjectiveEntry.QuestObjective = quest.Objectives[i];
						}
						else
						{
							xUiC_QuestTrackerObjectiveEntry.ClearObjective();
						}
					}
					num++;
				}
				if (num < count)
				{
					for (int j = num; j < count; j++)
					{
						if (objectiveEntries[j] is XUiC_QuestTrackerObjectiveEntry xUiC_QuestTrackerObjectiveEntry2)
						{
							xUiC_QuestTrackerObjectiveEntry2.ClearObjective();
						}
					}
				}
			}
			else if (challenge != null)
			{
				List<BaseChallengeObjective> objectiveList = challenge.GetObjectiveList();
				int count3 = objectiveEntries.Count;
				int count4 = objectiveList.Count;
				int num2 = 0;
				for (int k = 0; k < count4; k++)
				{
					if (num2 < count3 && objectiveEntries[num2] is XUiC_QuestTrackerObjectiveEntry xUiC_QuestTrackerObjectiveEntry3)
					{
						xUiC_QuestTrackerObjectiveEntry3.Owner = this;
						if (k < count4)
						{
							xUiC_QuestTrackerObjectiveEntry3.ChallengeObjective = objectiveList[k];
						}
						else
						{
							xUiC_QuestTrackerObjectiveEntry3.ClearObjective();
						}
					}
					num2++;
				}
				if (num2 < count3)
				{
					for (int l = num2; l < count3; l++)
					{
						if (objectiveEntries[l] is XUiC_QuestTrackerObjectiveEntry xUiC_QuestTrackerObjectiveEntry4)
						{
							xUiC_QuestTrackerObjectiveEntry4.ClearObjective();
						}
					}
				}
			}
			else
			{
				int count5 = objectiveEntries.Count;
				for (int m = 0; m < count5; m++)
				{
					if (objectiveEntries[m] is XUiC_QuestTrackerObjectiveEntry xUiC_QuestTrackerObjectiveEntry5)
					{
						xUiC_QuestTrackerObjectiveEntry5.Owner = this;
						xUiC_QuestTrackerObjectiveEntry5.ClearObjective();
					}
				}
			}
			isDirty = false;
		}
		base.Update(_dt);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "complete_icon":
			completeIconName = value;
			return true;
		case "incomplete_icon":
			incompleteIconName = value;
			return true;
		case "complete_color":
		{
			Color32 color2 = StringParsers.ParseColor(value);
			completeColor = $"{color2.r},{color2.g},{color2.b},{color2.a}";
			completeHexColor = Utils.ColorToHex(color2);
			return true;
		}
		case "incomplete_color":
		{
			Color32 color = StringParsers.ParseColor(value);
			incompleteColor = $"{color.r},{color.g},{color.b},{color.a}";
			incompleteHexColor = Utils.ColorToHex(color);
			return true;
		}
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}
}

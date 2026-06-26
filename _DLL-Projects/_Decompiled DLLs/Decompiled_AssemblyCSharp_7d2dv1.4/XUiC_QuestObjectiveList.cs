using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestObjectiveList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Quest quest;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> objectiveEntries = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTracker;

	public string completeHexColor = "FF00FF00";

	public string incompleteHexColor = "FFFF0000";

	public string warningHexColor = "FFFF00FF";

	public string inactiveHexColor = "888888FF";

	public string activeHexColor = "FFFFFFFF";

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

	public void SetIsTracker()
	{
		isTracker = true;
		for (int i = 0; i < objectiveEntries.Count; i++)
		{
			((XUiC_QuestObjectiveEntry)objectiveEntries[i]).SetIsTracker();
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_QuestObjectiveEntry>();
		XUiController[] array = childrenByType;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				objectiveEntries.Add(array[i]);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isDirty = true;
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
					if (quest.Objectives[i].Phase > quest.CurrentPhase || quest.Objectives[i].HiddenObjective || !quest.Objectives[i].ShowInQuestLog)
					{
						continue;
					}
					if (objectiveEntries[num] is XUiC_QuestObjectiveEntry)
					{
						((XUiC_QuestObjectiveEntry)objectiveEntries[num]).Owner = this;
						if (i < count2)
						{
							((XUiC_QuestObjectiveEntry)objectiveEntries[num]).Objective = quest.Objectives[i];
						}
						else
						{
							((XUiC_QuestObjectiveEntry)objectiveEntries[num]).Objective = null;
						}
					}
					num++;
				}
				if (num < count)
				{
					for (int j = num; j < count; j++)
					{
						if (objectiveEntries[j] is XUiC_QuestObjectiveEntry)
						{
							((XUiC_QuestObjectiveEntry)objectiveEntries[j]).Objective = null;
						}
					}
				}
			}
			else
			{
				int count3 = objectiveEntries.Count;
				for (int k = 0; k < count3; k++)
				{
					if (objectiveEntries[k] is XUiC_QuestObjectiveEntry)
					{
						((XUiC_QuestObjectiveEntry)objectiveEntries[k]).Owner = this;
						((XUiC_QuestObjectiveEntry)objectiveEntries[k]).Objective = null;
					}
				}
			}
			isDirty = false;
		}
		base.Update(_dt);
	}
}

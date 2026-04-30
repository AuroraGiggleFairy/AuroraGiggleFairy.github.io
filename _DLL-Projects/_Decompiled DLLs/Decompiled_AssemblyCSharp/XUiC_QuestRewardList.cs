using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestRewardList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Quest quest;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> rewardEntries = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

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

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_QuestRewardEntry>();
		XUiController[] array = childrenByType;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				rewardEntries.Add(array[i]);
			}
		}
	}

	public override void Update(float _dt)
	{
		if (isDirty)
		{
			if (this.quest != null)
			{
				List<BaseReward> list = new List<BaseReward>();
				Quest quest = this.quest;
				while (quest != null)
				{
					Quest quest2 = null;
					for (int i = 0; i < quest.Rewards.Count; i++)
					{
						if (quest.Rewards[i] is RewardQuest)
						{
							quest2 = QuestClass.CreateQuest(quest.Rewards[i].ID);
						}
						if (!quest.Rewards[i].HiddenReward && quest.Rewards[i].ReceiveStage == BaseReward.ReceiveStages.QuestCompletion && (quest == this.quest || !(quest.Rewards[i] is RewardQuest)))
						{
							list.Add(quest.Rewards[i]);
						}
					}
					quest = quest2;
				}
				int count = rewardEntries.Count;
				int count2 = list.Count;
				int num = 0;
				for (int j = 0; j < count; j++)
				{
					if (rewardEntries[num] is XUiC_QuestRewardEntry)
					{
						if (j < count2)
						{
							((XUiC_QuestRewardEntry)rewardEntries[num]).Reward = list[j];
							((XUiC_QuestRewardEntry)rewardEntries[num]).ChainQuest = list[j].OwnerQuest != Quest;
							num++;
						}
						else
						{
							((XUiC_QuestRewardEntry)rewardEntries[num]).Reward = null;
							num++;
						}
					}
				}
			}
			else
			{
				int count3 = rewardEntries.Count;
				for (int k = 0; k < count3; k++)
				{
					if (rewardEntries[k] is XUiC_QuestRewardEntry)
					{
						((XUiC_QuestRewardEntry)rewardEntries[k]).Reward = null;
					}
				}
			}
			isDirty = false;
		}
		base.Update(_dt);
	}
}

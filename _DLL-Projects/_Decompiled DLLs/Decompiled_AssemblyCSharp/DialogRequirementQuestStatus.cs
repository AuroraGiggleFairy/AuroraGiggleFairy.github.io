using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementQuestStatus : BaseDialogRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum QuestStatuses
	{
		NotStarted,
		InProgress,
		TurnInReady,
		Completed,
		CanReceive,
		CannotReceive
	}

	public override RequirementTypes RequirementType => RequirementTypes.QuestStatus;

	public override string GetRequiredDescription(EntityPlayer player)
	{
		return "";
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		QuestStatuses questStatuses = EnumUtils.Parse<QuestStatuses>(base.Value);
		string iD = base.ID;
		int num = -1;
		Quest quest = null;
		if (string.IsNullOrEmpty(base.ID))
		{
			EntityNPC respondent = LocalPlayerUI.GetUIForPlayer(GameManager.Instance.World.GetPrimaryPlayer()).xui.Dialog.Respondent;
			if ((object)respondent != null)
			{
				num = respondent.entityId;
				switch (questStatuses)
				{
				case QuestStatuses.NotStarted:
					quest = player.QuestJournal.FindActiveQuestByGiver(num, base.Tag);
					return quest == null;
				case QuestStatuses.InProgress:
					quest = player.QuestJournal.FindActiveQuestByGiver(num, base.Tag);
					return quest != null;
				}
			}
		}
		else
		{
			switch (questStatuses)
			{
			case QuestStatuses.NotStarted:
				quest = player.QuestJournal.FindNonSharedQuest(iD);
				if (quest == null || quest.CurrentState == Quest.QuestState.Completed)
				{
					return true;
				}
				break;
			case QuestStatuses.InProgress:
			{
				quest = player.QuestJournal.FindNonSharedQuest(iD);
				if (quest == null || !quest.Active)
				{
					break;
				}
				for (int j = 0; j < quest.Objectives.Count; j++)
				{
					if (!quest.Objectives[j].Complete)
					{
						return true;
					}
				}
				break;
			}
			case QuestStatuses.TurnInReady:
			{
				quest = player.QuestJournal.FindQuest(iD, talkingTo.NPCInfo.QuestFaction);
				if (quest == null || !quest.Active)
				{
					break;
				}
				for (int i = 0; i < quest.Objectives.Count; i++)
				{
					if (!quest.Objectives[i].Complete)
					{
						return false;
					}
				}
				return true;
			}
			case QuestStatuses.Completed:
				quest = player.QuestJournal.FindQuest(iD, talkingTo.NPCInfo.QuestFaction);
				if (quest.CurrentState == Quest.QuestState.Completed)
				{
					return true;
				}
				break;
			case QuestStatuses.CanReceive:
			{
				Quest quest3 = player.QuestJournal.FindLatestNonSharedQuest(iD);
				if (quest3 == null)
				{
					return true;
				}
				if (quest3.CurrentState == Quest.QuestState.Completed)
				{
					int num4 = (int)(GameManager.Instance.World.worldTime / 24000);
					int num5 = (int)(quest3.FinishTime / 24000);
					if (num4 != num5)
					{
						return true;
					}
				}
				break;
			}
			case QuestStatuses.CannotReceive:
			{
				Quest quest2 = player.QuestJournal.FindLatestNonSharedQuest(iD);
				if (quest2 != null)
				{
					if (quest2.CurrentState != Quest.QuestState.Completed)
					{
						return true;
					}
					int num2 = (int)(GameManager.Instance.World.worldTime / 24000);
					int num3 = (int)(quest2.FinishTime / 24000);
					if (num2 == num3)
					{
						return true;
					}
				}
				break;
			}
			}
		}
		return false;
	}
}

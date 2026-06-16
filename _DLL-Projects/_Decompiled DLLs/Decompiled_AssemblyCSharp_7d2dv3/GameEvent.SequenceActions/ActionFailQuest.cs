using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionFailQuest : ActionBaseClientAction
{
	public string QuestID = "";

	public bool RemoveQuest;

	public static string PropQuestID = "quest";

	public static string PropRemoveQuest = "remove_quest";

	public override void OnClientPerform(Entity target)
	{
		if (!(target is EntityPlayer entityPlayer))
		{
			return;
		}
		if (QuestID == "")
		{
			Quest quest = entityPlayer.QuestJournal.ActiveQuest;
			if (quest == null)
			{
				quest = entityPlayer.QuestJournal.FindActiveQuest();
			}
			if (quest != null)
			{
				quest.CloseQuest(Quest.QuestState.Failed);
				entityPlayer.QuestJournal.ActiveQuest = null;
				if (RemoveQuest)
				{
					entityPlayer.QuestJournal.ForceRemoveQuest(quest);
				}
			}
			return;
		}
		Quest quest2 = entityPlayer.QuestJournal.FindActiveQuest(QuestID);
		if (quest2 != null)
		{
			quest2.CloseQuest(Quest.QuestState.Failed);
			if (RemoveQuest)
			{
				entityPlayer.QuestJournal.ForceRemoveQuest(quest2);
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropQuestID, ref QuestID);
		properties.ParseBool(PropRemoveQuest, ref RemoveQuest);
		if (QuestID != "")
		{
			QuestID = QuestID.ToLower();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionFailQuest
		{
			QuestID = QuestID,
			RemoveQuest = RemoveQuest
		};
	}
}

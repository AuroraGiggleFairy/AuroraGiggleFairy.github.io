using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogActionCompleteQuest : BaseDialogAction
{
	public override ActionTypes ActionType => ActionTypes.CompleteQuest;

	public override void PerformAction(EntityPlayer player)
	{
		if (base.ID != "")
		{
			Quest quest = player.QuestJournal.FindQuest(base.ID, base.OwnerDialog.CurrentOwner.NPCInfo.QuestFaction);
			Convert.ToInt32(base.Value);
			if (quest != null && quest.Active)
			{
				QuestEventManager.Current.NPCInteracted(base.OwnerDialog.CurrentOwner);
				quest.RefreshQuestCompletion(QuestClass.CompletionTypes.TurnIn);
			}
		}
	}
}

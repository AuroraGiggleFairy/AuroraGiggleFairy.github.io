using UnityEngine.Scripting;

[Preserve]
public class QuestActionUnlockPOI : BaseQuestAction
{
	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		ownerQuest.HandleUnlockPOI();
	}

	public override BaseQuestAction Clone()
	{
		QuestActionUnlockPOI questActionUnlockPOI = new QuestActionUnlockPOI();
		CopyValues(questActionUnlockPOI);
		return questActionUnlockPOI;
	}
}

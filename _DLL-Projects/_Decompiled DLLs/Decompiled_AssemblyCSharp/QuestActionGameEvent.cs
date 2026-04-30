using UnityEngine.Scripting;

[Preserve]
public class QuestActionGameEvent : BaseQuestAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEventName = "event";

	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		GameEventManager.Current.HandleAction(ID, ownerQuest.OwnerJournal.OwnerPlayer, ownerQuest.OwnerJournal.OwnerPlayer, twitchActivated: false);
	}

	public override BaseQuestAction Clone()
	{
		QuestActionGameEvent questActionGameEvent = new QuestActionGameEvent();
		CopyValues(questActionGameEvent);
		return questActionGameEvent;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropEventName, ref ID);
	}
}

using UnityEngine.Scripting;

[Preserve]
public class QuestActionSetCVar : BaseQuestAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCVar = "cvar";

	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		ownerQuest.OwnerJournal.OwnerPlayer.Buffs.SetCustomVar(ID, StringParsers.ParseFloat(Value));
	}

	public override BaseQuestAction Clone()
	{
		QuestActionSetCVar questActionSetCVar = new QuestActionSetCVar();
		CopyValues(questActionSetCVar);
		return questActionSetCVar;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropCVar, ref ID);
	}
}

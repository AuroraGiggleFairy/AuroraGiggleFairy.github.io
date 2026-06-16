using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddQuest : ActionBaseClientAction
{
	public string QuestID;

	public bool Notify = true;

	public static string PropQuestID = "quest";

	public static string PropNotify = "notify";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayer entityPlayer)
		{
			Quest q = QuestClass.CreateQuest(QuestID);
			entityPlayer.QuestJournal.AddQuest(q, Notify);
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropQuestID, ref QuestID);
		properties.ParseBool(PropNotify, ref Notify);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddQuest
		{
			targetGroup = targetGroup,
			QuestID = QuestID,
			Notify = Notify
		};
	}
}

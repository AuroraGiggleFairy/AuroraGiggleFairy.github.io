using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementOnQuest : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string QuestID = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropQuest = "quest";

	public override bool CanPerform(Entity target)
	{
		bool flag = false;
		if (target is EntityPlayer entityPlayer)
		{
			if (QuestID == "")
			{
				if (entityPlayer.QuestJournal.ActiveQuest != null || entityPlayer.QuestJournal.FindActiveQuest() != null)
				{
					flag = true;
				}
			}
			else if (entityPlayer.QuestJournal.FindActiveQuest(QuestID) != null)
			{
				flag = true;
			}
			if (!Invert)
			{
				return flag;
			}
			return !flag;
		}
		return false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropQuest, ref QuestID);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementOnQuest
		{
			Invert = Invert,
			QuestID = QuestID
		};
	}
}

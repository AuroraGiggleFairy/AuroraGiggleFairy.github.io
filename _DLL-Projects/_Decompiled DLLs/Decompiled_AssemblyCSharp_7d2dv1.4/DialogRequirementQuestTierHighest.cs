using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementQuestTierHighest : BaseDialogRequirement
{
	public override RequirementTypes RequirementType => RequirementTypes.QuestTier;

	public override string GetRequiredDescription(EntityPlayer player)
	{
		return "";
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		int num = StringParsers.ParseSInt32(base.Value);
		EntityTrader entityTrader = talkingTo as EntityTrader;
		if (entityTrader == null)
		{
			return false;
		}
		if (player.QuestJournal.GetCurrentFactionTier(entityTrader.NPCInfo.QuestFaction) < num)
		{
			return false;
		}
		int num2 = -1;
		if (entityTrader.activeQuests == null)
		{
			return false;
		}
		bool flag = player.GetCVar("DisableQuesting") == 0f;
		for (int i = 0; i < entityTrader.activeQuests.Count; i++)
		{
			QuestClass questClass = entityTrader.activeQuests[i].QuestClass;
			if (questClass.DifficultyTier > num2 && questClass.UniqueKey == base.Tag && (flag || questClass.AlwaysAllow))
			{
				num2 = questClass.DifficultyTier;
			}
		}
		if (num == num2)
		{
			return true;
		}
		return false;
	}
}

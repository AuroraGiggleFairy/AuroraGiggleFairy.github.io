using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementQuestTier : BaseDialogRequirement
{
	public override RequirementTypes RequirementType => RequirementTypes.QuestTier;

	public override string GetRequiredDescription(EntityPlayer player)
	{
		return "";
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		EntityTrader entityTrader = talkingTo as EntityTrader;
		if (entityTrader == null)
		{
			return false;
		}
		int num = StringParsers.ParseSInt32(base.Value);
		if (player.QuestJournal.GetCurrentFactionTier(entityTrader.NPCInfo.QuestFaction) < num)
		{
			return false;
		}
		for (int i = 0; i < entityTrader.activeQuests.Count; i++)
		{
			if (entityTrader.activeQuests[i].QuestClass.DifficultyTier == num && entityTrader.activeQuests[i].QuestClass.UniqueKey == base.Tag)
			{
				return true;
			}
		}
		return false;
	}
}

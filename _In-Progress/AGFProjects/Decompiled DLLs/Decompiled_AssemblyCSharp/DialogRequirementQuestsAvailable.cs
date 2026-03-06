using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementQuestsAvailable : BaseDialogRequirement
{
	public override RequirementTypes RequirementType => RequirementTypes.QuestsAvailable;

	public override string GetRequiredDescription(EntityPlayer player)
	{
		return "";
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(GameManager.Instance.World.GetPrimaryPlayer());
		EntityTrader entityTrader = uIForPlayer.xui.Dialog.Respondent as EntityTrader;
		bool result = false;
		if (entityTrader.activeQuests != null)
		{
			int currentFactionTier = uIForPlayer.entityPlayer.QuestJournal.GetCurrentFactionTier(entityTrader.NPCInfo.QuestFaction);
			for (int i = 0; i < entityTrader.activeQuests.Count; i++)
			{
				if (entityTrader.activeQuests[i].QuestClass.QuestType == base.Value && entityTrader.activeQuests[i].QuestClass.DifficultyTier <= currentFactionTier && (entityTrader.activeQuests[i].QuestClass.Repeatable || uIForPlayer.entityPlayer.QuestJournal.FindActiveOrCompleteQuest(entityTrader.activeQuests[i].ID, entityTrader.NPCInfo.QuestFaction) == null))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}
}

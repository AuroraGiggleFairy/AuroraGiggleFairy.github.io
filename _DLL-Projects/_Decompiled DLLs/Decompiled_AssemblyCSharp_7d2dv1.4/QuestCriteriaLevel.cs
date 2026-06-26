public class QuestCriteriaLevel : BaseQuestCriteria
{
	public override bool CheckForPlayer(EntityPlayer player)
	{
		int result = 0;
		if (int.TryParse(Value, out result))
		{
			return player.Progression.Level >= result;
		}
		return false;
	}
}

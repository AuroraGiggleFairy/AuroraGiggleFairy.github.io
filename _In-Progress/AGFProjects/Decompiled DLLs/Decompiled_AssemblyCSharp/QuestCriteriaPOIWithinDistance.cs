public class QuestCriteriaPOIWithinDistance : BaseQuestCriteria
{
	public override bool CheckForQuestGiver(EntityNPC entity)
	{
		int result = 0;
		int.TryParse(Value, out result);
		return false;
	}
}

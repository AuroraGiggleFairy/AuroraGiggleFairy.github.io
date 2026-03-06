using UnityEngine.Scripting;

[Preserve]
public class BaseQuestCriteria
{
	public enum CriteriaTypes
	{
		QuestGiver,
		Player
	}

	public string ID;

	public string Value;

	public QuestClass OwnerQuestClass;

	public CriteriaTypes CriteriaType;

	public virtual void HandleVariables()
	{
	}

	public virtual bool CheckForQuestGiver(EntityNPC entity)
	{
		return true;
	}

	public virtual bool CheckForPlayer(EntityPlayer player)
	{
		return true;
	}
}

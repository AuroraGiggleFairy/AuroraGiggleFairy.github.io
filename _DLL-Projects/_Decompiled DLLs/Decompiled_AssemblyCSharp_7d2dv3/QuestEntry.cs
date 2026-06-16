using UnityEngine.Scripting;

[Preserve]
public class QuestEntry
{
	public float Prob = 1f;

	public int StartStage = -1;

	public int EndStage = -1;

	public string QuestID;

	public QuestClass QuestClass;

	public string[] CVar;

	public static FastTags<TagGroup.Global> buriedTag = FastTags<TagGroup.Global>.Parse("buried");

	public static FastTags<TagGroup.Global> poiTag = FastTags<TagGroup.Global>.Parse("poi");

	public QuestEntry(string questID, float prob, int startStage, int endStage, string cvar)
	{
		QuestID = questID;
		Prob = prob;
		StartStage = startStage;
		EndStage = endStage;
		QuestClass = QuestClass.GetQuest(QuestID);
		if (cvar != "")
		{
			CVar = cvar.Split(',');
		}
	}

	public bool CheckRequirement(EntityPlayer player)
	{
		if (!QuestJournal.BuriedQuestsEnabled && QuestClass.FilterTags.Test_AnySet(buriedTag))
		{
			return false;
		}
		if (!QuestJournal.POIQuestsEnabled && QuestClass.FilterTags.Test_AnySet(poiTag))
		{
			return false;
		}
		if (CVar == null)
		{
			return true;
		}
		for (int i = 0; i < CVar.Length; i++)
		{
			if (player.Buffs.GetCustomVar(CVar[i]) == 0f)
			{
				return false;
			}
		}
		return true;
	}
}

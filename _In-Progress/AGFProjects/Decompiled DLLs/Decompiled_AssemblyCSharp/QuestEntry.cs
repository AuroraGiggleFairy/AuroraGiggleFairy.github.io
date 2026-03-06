using UnityEngine.Scripting;

[Preserve]
public class QuestEntry
{
	public float Prob = 1f;

	public int StartStage = -1;

	public int EndStage = -1;

	public string QuestID;

	public QuestClass QuestClass;

	public QuestEntry(string questID, float prob, int startStage, int endStage)
	{
		QuestID = questID;
		Prob = prob;
		StartStage = startStage;
		EndStage = endStage;
		QuestClass = QuestClass.GetQuest(QuestID);
	}
}

using UnityEngine;

public class SharedQuestEntry
{
	public int QuestCode;

	public string QuestID;

	public string POIName;

	public Vector3 Position;

	public Vector3 Size;

	public Vector3 ReturnPos;

	public int SharedByPlayerID = -1;

	public int QuestGiverID = -1;

	public Quest Quest;

	public QuestClass QuestClass => QuestClass.GetQuest(QuestID);

	public SharedQuestEntry(int questCode, string questID, string poiName, Vector3 position, Vector3 size, Vector3 returnPos, int sharedByPlayerID, int questGiverID, QuestJournal questJournal, Quest quest)
	{
		QuestCode = questCode;
		QuestID = questID;
		POIName = poiName;
		Position = position;
		Size = size;
		ReturnPos = returnPos;
		SharedByPlayerID = sharedByPlayerID;
		QuestGiverID = questGiverID;
		Quest = ((quest == null) ? QuestClass.CreateQuest(questID) : quest.Clone());
		Quest.OwnerJournal = questJournal;
		Quest.SetupSharedQuest();
		Quest.SharedOwnerID = sharedByPlayerID;
		Quest.QuestGiverID = questGiverID;
		Quest.QuestCode = questCode;
		Quest.AddSharedLocation(position, size);
		if (!Quest.DataVariables.ContainsKey("POIName"))
		{
			Quest.DataVariables.Add("POIName", poiName);
		}
	}

	public SharedQuestEntry Clone()
	{
		return new SharedQuestEntry(QuestCode, QuestID, POIName, Position, Size, ReturnPos, SharedByPlayerID, QuestGiverID, Quest.OwnerJournal, Quest);
	}
}

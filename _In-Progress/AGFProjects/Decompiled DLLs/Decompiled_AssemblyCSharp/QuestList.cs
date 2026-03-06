using System.Collections.Generic;

public class QuestList
{
	public static Dictionary<string, QuestList> s_QuestLists = new CaseInsensitiveStringDictionary<QuestList>();

	public List<QuestEntry> Quests = new List<QuestEntry>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ID
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestList(string id)
	{
		ID = id;
	}

	public static QuestList NewList(string id)
	{
		if (s_QuestLists.ContainsKey(id))
		{
			return null;
		}
		QuestList questList = new QuestList(id.ToLower());
		s_QuestLists[id] = questList;
		return questList;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static QuestList GetQuest(string questListID)
	{
		if (!s_QuestLists.ContainsKey(questListID))
		{
			return null;
		}
		return s_QuestLists[questListID];
	}
}

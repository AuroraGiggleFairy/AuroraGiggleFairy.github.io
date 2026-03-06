using System.Collections.Generic;

public class NPCQuestData
{
	public class PlayerQuestData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public List<Quest> questList;

		public ulong LastUpdate;

		public List<Quest> QuestList
		{
			get
			{
				return questList;
			}
			set
			{
				questList = value;
				LastUpdate = GameManager.Instance.World.GetWorldTime() / 24000 * 24000;
			}
		}

		public PlayerQuestData(List<Quest> questList)
		{
			QuestList = questList;
		}
	}

	public Dictionary<int, PlayerQuestData> PlayerQuestList = new Dictionary<int, PlayerQuestData>();
}

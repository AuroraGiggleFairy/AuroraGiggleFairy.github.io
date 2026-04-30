using System.Collections.Generic;
using MusicUtils.Enums;

public class NPCInfo
{
	public enum StanceTypes
	{
		None,
		Like,
		Neutral,
		Dislike
	}

	public static Dictionary<string, NPCInfo> npcInfoList;

	public string Id;

	public string Name;

	public string Faction;

	public string Portrait;

	public string LocalizationID;

	public string VoiceSet = "";

	public StanceTypes CurrentStance;

	public SectionType DmsSectionType;

	public byte QuestFaction;

	public string QuestListName = "trader_quests";

	public int TraderID = -1;

	public string DialogID;

	public List<QuestEntry> Quests => QuestList.GetQuest(QuestListName)?.Quests;

	public static void InitStatic()
	{
		npcInfoList = new Dictionary<string, NPCInfo>();
	}

	public void Init()
	{
		npcInfoList[Id] = this;
	}

	public static void Cleanup()
	{
		npcInfoList = null;
	}
}

using System.Collections.Generic;

public class DialogResponseQuest : DialogResponse
{
	public bool IsValid = true;

	public Quest Quest;

	public int Variation = -1;

	public int Tier = -1;

	public string LastStatementID;

	public DialogResponseQuest(string _questID, string _nextStatementID, string _returnStatementID, string _type, Dialog _ownerDialog, int _listIndex = -1, int _tier = -1)
		: base(_questID)
	{
		Quest quest = null;
		base.OwnerDialog = _ownerDialog;
		LastStatementID = _returnStatementID;
		Tier = _tier;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(GameManager.Instance.World.GetPrimaryPlayer());
		EntityTrader entityTrader = uIForPlayer.xui.Dialog.Respondent as EntityTrader;
		if (entityTrader == null)
		{
			IsValid = false;
			return;
		}
		if (ID != "")
		{
			quest = QuestClass.GetQuest(ID).CreateQuest();
			quest.SetupTags();
			quest.QuestFaction = entityTrader.NPCInfo.QuestFaction;
			if (!quest.SetupPosition(entityTrader))
			{
				IsValid = false;
			}
		}
		else
		{
			List<Quest> activeQuests = entityTrader.activeQuests;
			if (_type == "")
			{
				if (Tier == -1)
				{
					if (activeQuests != null && _listIndex < activeQuests.Count && activeQuests[_listIndex].QuestClass.QuestType == "")
					{
						quest = activeQuests[_listIndex];
					}
					else
					{
						IsValid = false;
					}
				}
				else if (activeQuests != null)
				{
					int num = 0;
					bool flag = false;
					for (int i = 0; i < activeQuests.Count; i++)
					{
						if (activeQuests[i].QuestClass.DifficultyTier == Tier && activeQuests[i].QuestClass.QuestType == "")
						{
							if (num == _listIndex)
							{
								quest = activeQuests[i];
								flag = true;
								break;
							}
							num++;
						}
					}
					if (!flag)
					{
						IsValid = false;
					}
				}
			}
			else
			{
				int num2 = 0;
				int currentFactionTier = uIForPlayer.entityPlayer.QuestJournal.GetCurrentFactionTier(entityTrader.NPCInfo.QuestFaction);
				for (int j = 0; j < activeQuests.Count; j++)
				{
					if (activeQuests[j].QuestClass.QuestType == _type && activeQuests[j].QuestClass.DifficultyTier <= currentFactionTier)
					{
						if (_listIndex == num2)
						{
							quest = activeQuests[j];
							num2 = -1;
							break;
						}
						num2++;
					}
				}
				if (num2 != -1)
				{
					IsValid = false;
				}
			}
		}
		if (IsValid)
		{
			Quest = quest;
			AddAction(new DialogActionAddQuest
			{
				Quest = quest,
				Owner = this,
				OwnerDialog = base.OwnerDialog,
				ListIndex = _listIndex
			});
			ReturnStatementID = _nextStatementID;
			base.NextStatementID = _nextStatementID;
			string text = ValueDisplayFormatters.RomanNumber(quest.QuestClass.DifficultyTier);
			Text = "[[DECEA3]" + Localization.Get("xuiTier").ToUpper() + " " + text + "[-]] " + quest.GetParsedText(quest.QuestClass.ResponseText);
		}
	}
}

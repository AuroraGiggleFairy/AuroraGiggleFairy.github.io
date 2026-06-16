using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGiveQuest : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "givequest" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Gives a quest to the player or add to quest tier";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "givequest commands:\ncomplete - complete all quests\ntieradd <points> - add the points to your quest tier (default is 10)\n<quest name> - gives you the quest";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cannot execute givequest on dedicated server, please execute as a client");
		}
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(getHelp());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Available quests:");
			{
				foreach (KeyValuePair<string, QuestClass> s_Quest in QuestClass.s_Quests)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   " + s_Quest.Key);
				}
				return;
			}
		}
		QuestJournal questJournal = XUiM_Player.GetPlayer().QuestJournal;
		if (_params[0].EqualsCaseInsensitive("tieradd"))
		{
			int difficultyTier = 10;
			if (_params.Count >= 2)
			{
				difficultyTier = StringParsers.ParseSInt32(_params[1]);
			}
			questJournal.AddQuestFactionPoint(1, difficultyTier);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Quest points {0}, tier {1}", questJournal.GetQuestFactionPoints(1), questJournal.GetCurrentFactionTier(1));
			return;
		}
		if (_params[0].EqualsCaseInsensitive("complete"))
		{
			if (questJournal.quests.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < questJournal.quests.Count; i++)
			{
				Quest quest = questJournal.quests[i];
				if (quest.CurrentState != Quest.QuestState.NotStarted && quest.CurrentState != Quest.QuestState.InProgress)
				{
					continue;
				}
				for (int j = 0; j < quest.Objectives.Count; j++)
				{
					BaseObjective baseObjective = quest.Objectives[j];
					if (baseObjective.ObjectiveState != BaseObjective.ObjectiveStates.Complete && !(baseObjective is ObjectiveReturnToNPC))
					{
						baseObjective.ChangeStatus(isSuccess: true);
					}
				}
				questJournal.AddPOIToTraderData(quest.QuestClass.DifficultyTier, quest.PositionData[Quest.PositionDataTypes.TraderPosition], quest.PositionData[Quest.PositionDataTypes.POIPosition]);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Quest completed {0}", quest.GetPOIName());
			}
			return;
		}
		if (!QuestClass.s_Quests.ContainsKey(_params[0]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Quest '{0}' does not exist!", _params[0]);
			return;
		}
		string text = _params[0];
		foreach (KeyValuePair<string, QuestClass> s_Quest2 in QuestClass.s_Quests)
		{
			if (s_Quest2.Key.EqualsCaseInsensitive(text))
			{
				text = s_Quest2.Key;
				break;
			}
		}
		Quest quest2 = QuestClass.CreateQuest(text);
		if (quest2 == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Quest '{0}' does not exist!", _params[0]);
		}
		else
		{
			questJournal.AddQuest(quest2);
		}
	}
}

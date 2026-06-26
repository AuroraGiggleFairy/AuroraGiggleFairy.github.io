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

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cannot execute givequest on dedicated server, please execute as a client");
		}
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("givequest requires a quest id. Available quests:");
			{
				foreach (KeyValuePair<string, QuestClass> s_Quest in QuestClass.s_Quests)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   " + s_Quest.Key);
				}
				return;
			}
		}
		if (!QuestClass.s_Quests.ContainsKey(_params[0]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Quest '{_params[0]}' does not exist!");
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
		Quest quest = QuestClass.CreateQuest(text);
		if (quest == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Quest '{_params[0]}' does not exist!");
		}
		else
		{
			XUiM_Player.GetPlayer().QuestJournal.AddQuest(quest);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "usage: givequest questname";
	}
}

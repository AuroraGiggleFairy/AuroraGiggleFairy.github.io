using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdRemoveQuest : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "removequest" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("cannot execute removequest on dedicated server, please execute as a client");
		}
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("remove requires quest id");
			return;
		}
		string text = _params[0];
		foreach (KeyValuePair<string, QuestClass> s_Quest in QuestClass.s_Quests)
		{
			if (s_Quest.Key.EqualsCaseInsensitive(text))
			{
				text = s_Quest.Key;
				break;
			}
		}
		if (QuestClass.GetQuest(text) == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Quest '{text}' does not exist!");
		}
		else
		{
			XUiM_Player.GetPlayer().QuestJournal.ForceRemoveQuest(text);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "usage: removequest questname";
	}
}

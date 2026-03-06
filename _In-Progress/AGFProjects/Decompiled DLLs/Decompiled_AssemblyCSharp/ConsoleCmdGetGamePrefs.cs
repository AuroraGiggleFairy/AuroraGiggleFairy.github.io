using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGetGamePrefs : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] forbiddenPrefs = new string[8] { "telnet", "adminfilename", "controlpanel", "password", "historycache", "userdatafolder", "options", "last" };

	public override bool AllowedInMainMenu => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool prefAccessAllowed(EnumGamePrefs gp)
	{
		string text = gp.ToStringCached();
		string[] array = forbiddenPrefs;
		foreach (string value in array)
		{
			if (text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "getgamepref", "gg" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Gets game preferences";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Get all game preferences or only those matching a given substring";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		string text = null;
		if (_params.Count > 0)
		{
			text = _params[0];
		}
		SortedList<string, string> sortedList = new SortedList<string, string>();
		foreach (EnumGamePrefs item in EnumUtils.Values<EnumGamePrefs>())
		{
			if ((string.IsNullOrEmpty(text) || item.ToStringCached().ContainsCaseInsensitive(text)) && prefAccessAllowed(item))
			{
				sortedList.Add(item.ToStringCached(), $"GamePref.{item.ToStringCached()} = {GamePrefs.GetObject(item)}");
			}
		}
		foreach (string key in sortedList.Keys)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(sortedList[key]);
		}
	}
}

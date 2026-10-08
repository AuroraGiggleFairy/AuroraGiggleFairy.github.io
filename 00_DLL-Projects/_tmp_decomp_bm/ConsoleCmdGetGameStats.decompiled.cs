using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGetGameStats : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] forbiddenPrefs = new string[1] { "last" };

	public override int DefaultPermissionLevel => 1000;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool prefAccessAllowed(EnumGameStats gp)
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
		return new string[2] { "getgamestat", "ggs" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Gets game stats";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Get all game stats or only those matching a given substring";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		string text = null;
		if (_params.Count > 0)
		{
			text = _params[0];
		}
		SortedList<string, string> sortedList = new SortedList<string, string>();
		foreach (EnumGameStats item in EnumUtils.Values<EnumGameStats>())
		{
			if ((string.IsNullOrEmpty(text) || item.ToStringCached().ContainsCaseInsensitive(text)) && prefAccessAllowed(item))
			{
				sortedList.Add(item.ToStringCached(), $"GameStat.{item.ToStringCached()} = {GameStats.GetObject(item)}");
			}
		}
		foreach (string key in sortedList.Keys)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(sortedList[key]);
		}
	}
}

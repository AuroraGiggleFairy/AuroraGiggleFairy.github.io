using System;
using System.Collections.Generic;
using Platform;
using Platform.Steam;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdBan : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "ban" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Manage ban entries";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Set/get ban entries. Bans will be automatically lifted after the given time.\nUsage:\n   ban add <name / entity id / platform + platform user id> <duration> <duration unit> [reason] [displayname]\n   ban remove <name / entity id / platform + platform user id>\n   ban list\nTo use the add/remove sub commands with a name or entity ID the player has\nto be online, the variant with platform and platform ID can be used for currently offline\nusers too.\nDuration unit is a modifier to the duration which specifies if in what unit\nthe duration is given. Valid units:\n    minute(s), hour(s), day(s), week(s), month(s), year(s)\nDisplayname can be used to put a descriptive name to the ban list in addition\nto the user's platform ID. If used on an online user the player name is used by default.\nExample: ban add madmole 2 minutes \"Time for a break\" \"Joel\"";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.Instance.adminTools == null)
		{
			return;
		}
		if (_params.Count >= 1)
		{
			if (_params[0].EqualsCaseInsensitive("add"))
			{
				ExecuteAdd(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("remove"))
			{
				ExecuteRemove(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("list"))
			{
				ExecuteList();
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No sub command given.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteAdd(List<string> _params)
	{
		if (_params.Count < 4)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected at least 4, found " + _params.Count + ".");
			return;
		}
		string text = null;
		if (_params.Count > 5)
		{
			text = _params[5];
		}
		UserIdentifierSteam userIdentifierSteam = null;
		if (ConsoleHelper.ParseParamPartialNameOrId(_params[1], out var _id, out var _cInfo) != 1)
		{
			return;
		}
		if (_cInfo != null)
		{
			if (_cInfo.PlatformId is UserIdentifierSteam userIdentifierSteam2)
			{
				userIdentifierSteam = userIdentifierSteam2;
			}
			if (text == null)
			{
				text = _cInfo.playerName;
			}
		}
		if (!int.TryParse(_params[2], out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[2] + "\" is not a valid integer.");
			return;
		}
		DateTime now = DateTime.Now;
		if (_params[3].EqualsCaseInsensitive("min") || _params[3].EqualsCaseInsensitive("minute") || _params[3].EqualsCaseInsensitive("minutes"))
		{
			now = now.AddMinutes(result);
		}
		else if (_params[3].EqualsCaseInsensitive("h") || _params[3].EqualsCaseInsensitive("hour") || _params[3].EqualsCaseInsensitive("hours"))
		{
			now = now.AddHours(result);
		}
		else if (_params[3].EqualsCaseInsensitive("d") || _params[3].EqualsCaseInsensitive("day") || _params[3].EqualsCaseInsensitive("days"))
		{
			now = now.AddDays(result);
		}
		else if (_params[3].EqualsCaseInsensitive("w") || _params[3].EqualsCaseInsensitive("week") || _params[3].EqualsCaseInsensitive("weeks"))
		{
			now = now.AddDays(result * 7);
		}
		else if (_params[3].EqualsCaseInsensitive("month") || _params[3].EqualsCaseInsensitive("months"))
		{
			now = now.AddMonths(result);
		}
		else
		{
			if (!_params[3].EqualsCaseInsensitive("y") && !_params[3].EqualsCaseInsensitive("yr") && !_params[3].EqualsCaseInsensitive("year") && !_params[3].EqualsCaseInsensitive("years"))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[3] + "\" is not an allowed duration unit.");
				return;
			}
			now = now.AddYears(result);
		}
		string text2 = string.Empty;
		if (_params.Count > 4)
		{
			text2 = _params[4];
		}
		if (_cInfo != null)
		{
			ClientInfo cInfo = _cInfo;
			string customReason = (string.IsNullOrEmpty(text2) ? "" : text2);
			GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, 0, now, customReason));
		}
		GameManager.Instance.adminTools.Blacklist.AddBan(text, _id, now, text2);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{_id} banned until {now.ToCultureInvariantString()}, reason: {text2}.");
		if (userIdentifierSteam != null && !userIdentifierSteam.OwnerId.Equals(userIdentifierSteam))
		{
			GameManager.Instance.adminTools.Blacklist.AddBan(text, userIdentifierSteam.OwnerId, now, text2);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Steam Family Sharing license owner {userIdentifierSteam.OwnerId} banned until {now.ToCultureInvariantString()}, reason: {text2}.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteRemove(List<string> _params)
	{
		if (_params.Count != 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 2, found " + _params.Count + ".");
			return;
		}
		PlatformUserIdentifierAbs platformUserIdentifierAbs = ConsoleHelper.ParseParamUserId(_params[1]);
		if (platformUserIdentifierAbs == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid user id.");
			return;
		}
		GameManager.Instance.adminTools.Blacklist.RemoveBan(platformUserIdentifierAbs);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{platformUserIdentifierAbs} removed from ban list.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteList()
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Ban list entries:");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  Banned until - UserID (name) - Reason");
		List<AdminBlacklist.BannedUser> banned = GameManager.Instance.adminTools.Blacklist.GetBanned();
		for (int i = 0; i < banned.Count; i++)
		{
			AdminBlacklist.BannedUser bannedUser = banned[i];
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("  {0} - {1} ({2}) - {3}", bannedUser.BannedUntil.ToCultureInvariantString(), bannedUser.UserIdentifier, bannedUser.Name ?? "-unknown-", bannedUser.BanReason));
		}
	}
}

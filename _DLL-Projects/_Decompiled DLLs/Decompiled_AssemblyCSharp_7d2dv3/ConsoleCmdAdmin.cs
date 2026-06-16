using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAdmin : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "admin" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Manage user permission levels";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Set/get user permission levels. A level of 0 is maximum permission,\nusers without an explicitly set permission have a permission level of 1000.\nUsage:\n   admin add <name / entity id / platform + platform user id> <level> [displayname]\n   admin remove <name / entity / platform + platform user id>\n   admin addgroup <steam id> <level regular> <level mods> [displayname]\n   admin removegroup <steam id>\n   admin list\nTo use the add/remove sub commands with a name or entity ID the player has\nto be online, the variant with platform and platform ID can be used for currently offline\nusers too.\nDisplayname can be used to put a descriptive name to the ban list in addition\nto the user's platform ID. If used on an online user the player name is used by default.\nWhen adding groups you set two permission levels: The 'regular' level applies to\nnormal members of the group, the 'mods' level applies to moderators / officers of\nthe group.";
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
			else if (_params[0].EqualsCaseInsensitive("addgroup"))
			{
				ExecuteAddGroup(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("removegroup"))
			{
				ExecuteRemoveGroup(_params);
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
		if (_params.Count != 3 && _params.Count != 4)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 3 or 4, found " + _params.Count + ".");
			return;
		}
		string text = null;
		if (_params.Count > 3)
		{
			text = _params[3];
		}
		if (ConsoleHelper.ParseParamPartialNameOrId(_params[1], out var _id, out var _cInfo) == 1)
		{
			if (_cInfo != null && text == null)
			{
				text = _cInfo.playerName;
			}
			if (!int.TryParse(_params[2], out var result))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[2] + "\" is not a valid integer.");
				return;
			}
			GameManager.Instance.adminTools.Users.AddUser(text, _id, result);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{_id} added with permission level of {result}.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteRemove(List<string> _params)
	{
		PlatformUserIdentifierAbs _id;
		ClientInfo _cInfo;
		if (_params.Count != 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 2, found " + _params.Count + ".");
		}
		else if (ConsoleHelper.ParseParamPartialNameOrId(_params[1], out _id, out _cInfo) == 1)
		{
			if (GameManager.Instance.adminTools.Users.RemoveUser(_id))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{_id} removed from permissions list.");
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{_id} was not on permissions list.");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteAddGroup(List<string> _params)
	{
		if (_params.Count != 4 && _params.Count != 5)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 4 or 5, found " + _params.Count + ".");
			return;
		}
		string name = null;
		if (_params.Count > 4)
		{
			name = _params[4];
		}
		if (!ConsoleHelper.ParseParamSteamGroupIdValid(_params[1]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid steam group id.");
			return;
		}
		if (!int.TryParse(_params[2], out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[2] + "\" is not a valid integer.");
			return;
		}
		if (!int.TryParse(_params[3], out var result2))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[2] + "\" is not a valid integer.");
			return;
		}
		GameManager.Instance.adminTools.Users.AddGroup(name, _params[1], result, result2);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Group {_params[1]} added with permission level of {result} for regulars, {result2} for moderators.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteRemoveGroup(List<string> _params)
	{
		if (_params.Count != 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 2, found " + _params.Count + ".");
		}
		else if (!ConsoleHelper.ParseParamSteamGroupIdValid(_params[1]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid steam group id.");
		}
		else if (GameManager.Instance.adminTools.Users.RemoveGroup(_params[1]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Group " + _params[1] + " removed from permissions list.");
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Group " + _params[1] + " was not on permissions list.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteList()
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Defined User Permissions:");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  Level: UserID (Player name if online, stored name)");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, AdminUsers.UserPermission> user in GameManager.Instance.adminTools.Users.GetUsers())
		{
			AdminUsers.UserPermission value = user.Value;
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(value.UserIdentifier);
			if (clientInfo != null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("  {0,5}: {1} ({2}, stored name: {3})", value.PermissionLevel, value.UserIdentifier, clientInfo.playerName, value.Name ?? ""));
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("  {0,5}: {1} (stored name: {2})", value.PermissionLevel, value.UserIdentifier, value.Name ?? ""));
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Defined Group Permissions:");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  Normal,  Mods: SteamID (Stored name)");
		foreach (KeyValuePair<string, AdminUsers.GroupPermission> group in GameManager.Instance.adminTools.Users.GetGroups())
		{
			AdminUsers.GroupPermission value2 = group.Value;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("  {0,6}, {1:5}: {2} (stored name: {3})", value2.PermissionLevelNormal, value2.PermissionLevelMods, value2.SteamIdGroup, value2.Name ?? ""));
		}
	}
}

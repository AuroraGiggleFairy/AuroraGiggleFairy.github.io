using System;
using System.Collections.ObjectModel;
using Platform;

public static class ConsoleHelper
{
	public static bool ParseParamBool(string _param, bool _invalidStringsAsFalse = false)
	{
		if (_param.EqualsCaseInsensitive("y") || _param.EqualsCaseInsensitive("yes") || _param.EqualsCaseInsensitive("true") || _param.EqualsCaseInsensitive("on") || _param.EqualsCaseInsensitive("1"))
		{
			return true;
		}
		if (_param.EqualsCaseInsensitive("n") || _param.EqualsCaseInsensitive("no") || _param.EqualsCaseInsensitive("false") || _param.EqualsCaseInsensitive("off") || _param.EqualsCaseInsensitive("0"))
		{
			return false;
		}
		if (_invalidStringsAsFalse)
		{
			return false;
		}
		throw new ArgumentException("Not a bool value");
	}

	public static Entity ParseParamEntityIdToEntity(string _param, bool _playersOnly = true)
	{
		if (!int.TryParse(_param, out var result))
		{
			return null;
		}
		if (_playersOnly)
		{
			if (GameManager.Instance.World.Players.dict.ContainsKey(result))
			{
				return GameManager.Instance.World.Players.dict[result];
			}
		}
		else if (GameManager.Instance.World.Entities.dict.ContainsKey(result))
		{
			return GameManager.Instance.World.Entities.dict[result];
		}
		return null;
	}

	public static ClientInfo ParseParamEntityIdToClientInfo(string _param)
	{
		if (!int.TryParse(_param, out var result))
		{
			return null;
		}
		return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(result);
	}

	public static bool ParseParamSteamGroupIdValid(string _param)
	{
		ulong result;
		if (_param.Length == 18)
		{
			return ulong.TryParse(_param, out result);
		}
		return false;
	}

	public static bool ParamIsLocalPlayer(string _param, bool _ignoreCase = true, bool _ignoreBlanks = false)
	{
		if (GameManager.IsDedicatedServer)
		{
			return false;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (PlatformUserIdentifierAbs.TryFromCombinedString(_param, out var _userIdentifier) && _userIdentifier.Equals(PlatformManager.InternalLocalUserIdentifier))
		{
			return true;
		}
		if (int.TryParse(_param, out var result) && primaryPlayer.entityId == result)
		{
			return true;
		}
		if (_ignoreBlanks)
		{
			_param = _param.Replace(" ", "");
		}
		string text = primaryPlayer.EntityName ?? string.Empty;
		if (_ignoreBlanks)
		{
			text = text.Replace(" ", "");
		}
		if (string.Equals(text, _param, _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
		{
			return true;
		}
		return false;
	}

	public static ClientInfo ParseParamPlayerName(string _param, bool _ignoreCase = true, bool _ignoreBlanks = false)
	{
		return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForPlayerName(_param, _ignoreCase, _ignoreBlanks);
	}

	public static PlatformUserIdentifierAbs ParseParamUserId(string _param)
	{
		return PlatformUserIdentifierAbs.FromCombinedString(_param);
	}

	public static ClientInfo ParseParamIdOrName(string _param, bool _ignoreCase = true, bool _ignoreBlanks = false)
	{
		return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForNameOrId(_param, _ignoreCase, _ignoreBlanks);
	}

	public static int ParseParamPartialNameOrId(string _param, out PlatformUserIdentifierAbs _id, out ClientInfo _cInfo, bool _sendError = true)
	{
		_id = null;
		_cInfo = null;
		ClientInfo clientInfo = ParseParamIdOrName(_param);
		if (clientInfo != null)
		{
			_id = clientInfo.InternalId;
			_cInfo = clientInfo;
			return 1;
		}
		if (PlatformUserIdentifierAbs.TryFromCombinedString(_param, out _id))
		{
			return 1;
		}
		ClientInfo clientInfo2 = null;
		int num = 0;
		ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo3 = list[i];
			if (clientInfo3.playerName.ContainsCaseInsensitive(_param))
			{
				num++;
				clientInfo2 = clientInfo3;
			}
		}
		if (num == 1)
		{
			_id = clientInfo2.InternalId;
			_cInfo = clientInfo2;
		}
		else
		{
			_id = null;
			_cInfo = null;
			if (_sendError)
			{
				if (num == 0)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _param + "\" is not a valid entity id, player name or user id.");
				}
				else if (num > 1)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _param + "\" matches multiple player names.");
				}
			}
		}
		return num;
	}
}

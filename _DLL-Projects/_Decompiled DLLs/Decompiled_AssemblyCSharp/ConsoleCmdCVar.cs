using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCVar : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => false;

	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "cvar" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Commands to set, get, track or list CVars.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usages of the commands. Add '-p <playerId>' to any command to apply that command to a remote player. \ncvar get <cvarName>\ncvar set <cvarName> <floatValue>\ncvar track <cvarName> <true|false>\ncvar list <searchFilter>";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not enough arguments supplied.");
			return;
		}
		int playerId = GameManager.Instance.World.GetPrimaryPlayerId();
		if (_senderInfo.RemoteClientInfo != null)
		{
			playerId = _senderInfo.RemoteClientInfo.entityId;
		}
		switch (_params[0].ToLowerInvariant())
		{
		case "get":
			ExecuteGet(_params, playerId);
			break;
		case "set":
			ExecuteSet(_params, playerId);
			break;
		case "track":
			ExecuteTrack(_params, playerId);
			break;
		case "list":
			ExecuteList(_params, playerId);
			break;
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Valid command not supplied. Use 'get', 'set', 'track' or 'list'.");
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteGet(List<string> _params, int _playerId)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not enough arguments supplied.");
			return;
		}
		if (_params.Count >= 4)
		{
			_playerId = GetPlayerId(_params);
		}
		EntityPlayer player = GetPlayer(_playerId);
		if (player == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Could not find player matching ID {_playerId}.");
			return;
		}
		string text = _params[1];
		bool flag = player.Buffs.HasCustomVar(text);
		float num = (flag ? player.Buffs.GetCustomVar(text) : 0f);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Player {player.EntityName} has cvar {text}: {flag}. Value: {num}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteSet(List<string> _params, int _playerId)
	{
		if (_params.Count < 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not enough arguments supplied.");
			return;
		}
		if (!float.TryParse(_params[2], out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Could not parse '" + _params[2] + "' into float.");
			return;
		}
		int num = _playerId;
		if (_params.Count >= 5)
		{
			_playerId = GetPlayerId(_params);
		}
		EntityPlayer player = GetPlayer(_playerId);
		if (player == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Could not find player matching ID {_playerId}.");
			return;
		}
		string text = _params[1];
		float customVar = player.Buffs.GetCustomVar(text);
		if (_playerId == num)
		{
			player.Buffs.SetCustomVar(text, result);
		}
		else
		{
			player.Buffs.SetCustomVarNetwork(text, result);
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Player {player.EntityName} cvar {text} set from {customVar} to {result}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteTrack(List<string> _params, int _playerId)
	{
		if (_params.Count < 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not enough arguments supplied.");
			return;
		}
		if (!bool.TryParse(_params[2], out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Could not parse '" + _params[2] + "' into bool.");
			return;
		}
		if (_params.Count >= 5)
		{
			_playerId = GetPlayerId(_params);
		}
		EntityPlayer player = GetPlayer(_playerId);
		if (player == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Could not find player matching ID {_playerId}.");
			return;
		}
		string name = _params[1];
		player.Buffs.TrackCustomVar(name, result);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteList(List<string> _params, int _playerId)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not enough arguments supplied.");
			return;
		}
		if (_params.Count >= 5)
		{
			_playerId = GetPlayerId(_params);
		}
		EntityPlayer player = GetPlayer(_playerId);
		if (player == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Could not find player matching ID {_playerId}.");
			return;
		}
		string text = _params[1];
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Listing CVars for " + player.EntityName + " which contain \"" + text + "\".");
		foreach (var (arg, num2) in player.Buffs.EnumerateCustomVars(text))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"\t{arg} : {num2}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetPlayerId(List<string> _params)
	{
		for (int i = 2; i < _params.Count - 1; i++)
		{
			if (_params[i].EqualsCaseInsensitive("-p"))
			{
				if (!int.TryParse(_params[i + 1], out var result))
				{
					break;
				}
				return result;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer GetPlayer(int _playerId)
	{
		if (_playerId == -1)
		{
			return null;
		}
		if (!GameManager.Instance.World.Players.dict.TryGetValue(_playerId, out var value))
		{
			return null;
		}
		return value;
	}
}

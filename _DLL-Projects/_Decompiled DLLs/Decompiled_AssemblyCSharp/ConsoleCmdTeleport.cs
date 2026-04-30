using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTeleport : ConsoleCmdTeleportsAbs
{
	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Teleport the local player";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. teleport <x> <y> <z> [view direction]\n  2. teleport <x> <z> [view direction]\n  3. teleport <target steam id / player name / entity id>\n  4. teleport offset <inc x> <inc y> <inc z>\nFor 1. and 2.: view direction is an optional specifier to select the direction you want to look into\nafter teleporting. This can be either of n, ne, e, se, s, sw, w, nw or north, northeast, etc.\n1. Teleports the local player to the specified location. Use y = -1 to spawn on ground.\n2. Same as 1 but always spawn on ground.\n3. Teleports to the location of the given player\n4. Teleport the local player to the position calculated by his current position and the given offsets";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "teleport", "tp" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!_senderInfo.IsLocalGame && _senderInfo.RemoteClientInfo == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients, use \"teleportplayer\" instead for other players / remote clients");
			return;
		}
		EntityPlayer executingEntityPlayer = GetExecutingEntityPlayer(_senderInfo);
		if (_params.Count < 1 || _params.Count > 4)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1 to 4, found " + _params.Count + ".");
			return;
		}
		if (_params.Count == 1)
		{
			if (TryGetDestinationFromPlayer(_params[0], out var _destination))
			{
				ExecuteTeleport(_senderInfo.RemoteClientInfo, _destination, null);
			}
			return;
		}
		Vector3i _result;
		if (_params.Count == 4 && _params[0].EqualsCaseInsensitive("offset"))
		{
			if (TryParseV3i(_params, 1, out _result))
			{
				_result += new Vector3i(executingEntityPlayer.position);
				ExecuteTeleport(_senderInfo.RemoteClientInfo, _result, null);
			}
			return;
		}
		int result;
		bool flag = !int.TryParse(_params[_params.Count - 1], out result);
		if (_params.Count == 2 || (flag && _params.Count == 3))
		{
			_params.Insert(1, "-1");
		}
		if (TryParseV3i(_params, 0, out _result))
		{
			Vector3? viewDirection = ((_params.Count == 4) ? TryParseViewDirection(_params[3]) : ((Vector3?)null));
			ExecuteTeleport(_senderInfo.RemoteClientInfo, _result, viewDirection);
		}
	}
}

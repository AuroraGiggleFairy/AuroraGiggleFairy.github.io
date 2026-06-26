using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTeleportPoiRelative : ConsoleCmdTeleportsAbs
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Teleport the local player within the current POI";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\n\t\t\tUsage:\n\t\t\t|  1. teleportpoirelative <x> <y> <z> [view direction]\n\t\t\t|1. Teleports the local player to the specified location relative to the bounds of the current POI. View\n\t\t\t|direction is an optional specifier to select the direction you want to look into after teleporting. This\n\t\t\t|can be either of n, ne, e, se, s, sw, w, nw or north, northeast, etc.\n\t\t\t".Unindent();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "teleportpoirelative", "tppr" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!_senderInfo.IsLocalGame && _senderInfo.RemoteClientInfo == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients");
			return;
		}
		PrefabInstance prefab = GetExecutingEntityPlayer(_senderInfo).prefab;
		Vector3i _result;
		if (prefab == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Player has to be within the bounds of a prefab!");
		}
		else if (_params.Count < 3 || _params.Count > 4)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 3 to 4, found " + _params.Count + ".");
		}
		else if (TryParseV3i(_params, 0, out _result))
		{
			Vector3? viewDirection = ((_params.Count == 4) ? TryParseViewDirection(_params[3]) : ((Vector3?)null));
			_result = prefab.GetWorldPositionOfPoiOffset(_result);
			ExecuteTeleport(_senderInfo.RemoteClientInfo, _result, viewDirection);
		}
	}
}

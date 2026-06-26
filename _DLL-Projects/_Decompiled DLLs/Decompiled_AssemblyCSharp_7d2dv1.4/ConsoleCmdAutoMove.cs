using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAutoMove : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lookAtPos;

	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "automove" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Player auto movement";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Parameters:\noff - disable\ngototarget - goto the target position\nsettarget - set target to current player position\nclearlookat - disable look at\nsetlookat - set look at to current player position\nline duration loops <x> <y> <z> - move to x y z (or target) over duration with loops (-loops will ping pong)\norbit duration loops <x> <y> <z> - circle around x y z (or target) over duration with loops (-loops will ping pong)\nrelative x z angle - move by x (left/right) and z (forward) and turn by angle per second";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (!primaryPlayer)
		{
			return;
		}
		switch (_params[0].ToLower())
		{
		case "off":
		case "0":
			primaryPlayer.EnableAutoMove(_enable: false);
			break;
		case "gototarget":
		case "gt":
			primaryPlayer.SetPosition(targetPos);
			break;
		case "settarget":
		case "st":
			targetPos = primaryPlayer.GetPosition();
			break;
		case "clearlookat":
		case "cla":
			lookAtPos = Vector3.zero;
			break;
		case "setlookat":
		case "sla":
			lookAtPos = primaryPlayer.GetPosition();
			break;
		case "line":
		case "l":
		{
			if (targetPos.sqrMagnitude == 0f)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("line target is not set");
				break;
			}
			Vector3 endPos = targetPos;
			float duration = 5f;
			if (_params.Count >= 2)
			{
				duration = FloatParse(_params[1]);
			}
			int loopCount = 0;
			if (_params.Count >= 3)
			{
				loopCount = IntParse(_params[2]);
			}
			if (_params.Count >= 4)
			{
				endPos.x = FloatParse(_params[3]);
			}
			if (_params.Count >= 5)
			{
				endPos.y = FloatParse(_params[4]);
			}
			if (_params.Count >= 6)
			{
				endPos.z = FloatParse(_params[5]);
			}
			EntityPlayerLocal.AutoMove autoMove2 = primaryPlayer.EnableAutoMove(_enable: true);
			autoMove2.SetLookAt(lookAtPos);
			autoMove2.StartLine(duration, loopCount, endPos);
			break;
		}
		case "orbit":
		case "o":
		{
			if (targetPos.sqrMagnitude == 0f)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("orbit target is not set");
				break;
			}
			Vector3 orbitPos = targetPos;
			float duration2 = 5f;
			if (_params.Count >= 2)
			{
				duration2 = FloatParse(_params[1]);
			}
			int loopCount2 = 0;
			if (_params.Count >= 3)
			{
				loopCount2 = IntParse(_params[2]);
			}
			if (_params.Count >= 4)
			{
				orbitPos.x = FloatParse(_params[3]);
			}
			if (_params.Count >= 5)
			{
				orbitPos.y = FloatParse(_params[4]);
			}
			if (_params.Count >= 6)
			{
				orbitPos.z = FloatParse(_params[5]);
			}
			EntityPlayerLocal.AutoMove autoMove3 = primaryPlayer.EnableAutoMove(_enable: true);
			autoMove3.SetLookAt(lookAtPos);
			autoMove3.StartOrbit(duration2, loopCount2, orbitPos);
			break;
		}
		case "relative":
		case "r":
		{
			float velX = 0f;
			if (_params.Count >= 2)
			{
				velX = FloatParse(_params[1]);
			}
			float velZ = 0f;
			if (_params.Count >= 3)
			{
				velZ = FloatParse(_params[2]);
			}
			float rotVel = 0f;
			if (_params.Count >= 4)
			{
				rotVel = FloatParse(_params[3]);
			}
			EntityPlayerLocal.AutoMove autoMove = primaryPlayer.EnableAutoMove(_enable: true);
			autoMove.SetLookAt(lookAtPos);
			autoMove.StartRelative(velX, velZ, rotVel);
			break;
		}
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("unknown command " + _params[0]);
			break;
		}
	}

	public int IntParse(string s)
	{
		int.TryParse(s, out var result);
		return result;
	}

	public float FloatParse(string s)
	{
		float.TryParse(s, out var result);
		return result;
	}
}

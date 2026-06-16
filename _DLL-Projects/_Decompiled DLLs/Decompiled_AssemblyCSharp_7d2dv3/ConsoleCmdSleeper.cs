using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSleeper : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine drawVolumesCo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "sleeper" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Drawn or list sleeper info";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "draw - toggle drawing for current player prefab\nlist - list for current player prefab\nlistall - list all\nr - reset all";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		switch (_params[0].ToLower())
		{
		case "draw":
			if (drawVolumesCo != null)
			{
				GameManager.Instance.StopCoroutine(drawVolumesCo);
				drawVolumesCo = null;
			}
			else
			{
				drawVolumesCo = GameManager.Instance.StartCoroutine(DrawVolumes());
			}
			break;
		case "listall":
			LogInfo(onlyPlayer: false);
			break;
		case "list":
			LogInfo(onlyPlayer: true);
			break;
		case "r":
			Reset();
			break;
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command not recognized. <end/>");
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogInfo(bool onlyPlayer)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = (onlyPlayer ? world.GetPrimaryPlayer() : null);
		List<(int, SleeperVolume)> list = new List<(int, SleeperVolume)>();
		world.GetAllSleeperVolumes(list);
		int count = list.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			var (num2, sleeperVolume) = list[i];
			if ((bool)entityPlayerLocal)
			{
				if (sleeperVolume.PrefabInstance != entityPlayerLocal.prefab)
				{
					continue;
				}
				sleeperVolume.Draw(3f);
			}
			num++;
			Print("#{0} {1}", num2, sleeperVolume.GetDescription());
		}
		Print("Sleeper volumes {0} of {1}", num, count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DrawVolumes()
	{
		int n = 0;
		while (n < 99999)
		{
			World world = GameManager.Instance.World;
			if (world == null)
			{
				break;
			}
			EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
			if (!primaryPlayer || primaryPlayer.prefab == null)
			{
				break;
			}
			foreach (SleeperVolume sleeperVolume in primaryPlayer.prefab.sleeperVolumes)
			{
				sleeperVolume.DrawDebugLines(1f);
			}
			foreach (TriggerVolume triggerVolume in primaryPlayer.prefab.triggerVolumes)
			{
				triggerVolume.DrawDebugLines(1f);
			}
			yield return new WaitForSeconds(0.5f);
			int num = n + 1;
			n = num;
		}
		drawVolumesCo = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Reset()
	{
		World world = GameManager.Instance.World;
		if (world != null)
		{
			world.ResetSleeperVolumes();
			Print("Reset all sleeper volumes");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Print(string _s, params object[] _values)
	{
		string line = string.Format(_s, _values);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(line);
	}
}

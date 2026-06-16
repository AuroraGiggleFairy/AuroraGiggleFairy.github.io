using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAutomation : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float DefaultFlyMetersPerSec = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3? _pendingStartPoint;

	public override bool AllowedInMainMenu => true;

	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "automation", "auto" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Automation Script Runner";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Type 'auto' in the console or see source XML doc for full usage.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			CmdShow();
			return;
		}
		switch (_params[0].ToLower())
		{
		case "new":
			CmdNew(_params);
			break;
		case "show":
			CmdShow();
			break;
		case "add":
			CmdAdd(_params);
			break;
		case "remove":
			CmdRemove(_params);
			break;
		case "save":
			CmdSave(_params);
			break;
		case "load":
			CmdLoad(_params);
			break;
		case "list":
			CmdList();
			break;
		case "run":
			CmdRun(_params);
			break;
		case "abort":
			CmdAbort();
			break;
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown sub-command '" + _params[0] + "'. See 'help auto'.");
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdNew(List<string> p)
	{
		if (RequireNotRunning())
		{
			AutomationRunner.Instance.LoadScriptUnchecked(new AutomationScript());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("New empty script created.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdShow()
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(AutomationRunner.Instance.CurrentScript.Describe());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdRemove(List<string> p)
	{
		if (!RequireNotRunning())
		{
			return;
		}
		if (p.Count < 2 || !int.TryParse(p[1], out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto remove <index>");
			return;
		}
		List<AutomationStep> steps = AutomationRunner.Instance.CurrentScript.steps;
		if (result < 0 || result >= steps.Count)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Index {result} out of range (0–{steps.Count - 1}).");
			return;
		}
		string text = steps[result].Describe(result);
		steps.RemoveAt(result);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Removed step: " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdSave(List<string> p)
	{
		if (p.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto save <name>");
			return;
		}
		AutomationRunner.Instance.CurrentScript.SaveToFile(p[1]);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Script saved as '" + p[1] + "'.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdLoad(List<string> p)
	{
		if (!RequireNotRunning())
		{
			return;
		}
		if (p.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto load <name>");
			return;
		}
		AutomationScript automationScript = AutomationScript.LoadFromFile(p[1]);
		if (automationScript == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to load script '" + p[1] + "'. Check log for details.");
		}
		else if (!AutomationRunner.Instance.LoadScript(automationScript))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Script failed validation. Check log for details.");
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(automationScript.Describe());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdList()
	{
		List<string> list = AutomationScript.ListSavedScripts();
		if (list.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No saved scripts found.");
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saved scripts:\n" + string.Join("\n", list.ConvertAll([PublicizedFrom(EAccessModifier.Internal)] (string n) => "  " + n)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdAdd(List<string> p)
	{
		if (!RequireNotRunning())
		{
			return;
		}
		if (p.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add <type> [args]");
			return;
		}
		AutomationStep automationStep = null;
		switch (p[1].ToLower())
		{
		case "loadgame":
			if (p.Count < 4)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add loadgame <world> <gameName>");
				return;
			}
			automationStep = AutomationStep.LoadGame(p[2], p[3]);
			break;
		case "prepare":
			automationStep = AutomationStep.PreparePlayer();
			break;
		case "teleport":
		{
			if (p.Count >= 5 && TryParseVec3(p, 2, out var v6))
			{
				automationStep = AutomationStep.Teleport(v6, PlayerForward());
				break;
			}
			if (!RequireWorld())
			{
				return;
			}
			automationStep = AutomationStep.Teleport(PlayerPos(), PlayerForward());
			break;
		}
		case "wait":
		{
			if (p.Count < 3 || !float.TryParse(p[2], out var result2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add wait <secs>");
				return;
			}
			automationStep = AutomationStep.Wait(result2);
			break;
		}
		case "moveline":
			switch ((p.Count > 2) ? p[2].ToLower() : "")
			{
			case "p1":
				if (RequireWorld())
				{
					_pendingStartPoint = PlayerPos();
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Start point captured: " + Fmt(_pendingStartPoint.Value));
				}
				return;
			case "clear":
				_pendingStartPoint = null;
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Pending start point cleared.");
				return;
			case "show":
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(_pendingStartPoint.HasValue ? ("Pending start point: " + Fmt(_pendingStartPoint.Value)) : "No pending start point.");
				return;
			case "p2":
			{
				if (!RequireWorld())
				{
					return;
				}
				if (!_pendingStartPoint.HasValue)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No start point yet. Run 'auto add moveline p1' first.");
					return;
				}
				Vector3 value = _pendingStartPoint.Value;
				Vector3 vector = PlayerPos();
				float result11;
				float dur = ((p.Count >= 4 && float.TryParse(p[3], out result11)) ? result11 : DefaultLegDuration(value, vector));
				if (value == vector)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Start and end are identical — step not added.");
					return;
				}
				automationStep = AutomationStep.MoveLine(value, vector, dur);
				_pendingStartPoint = null;
				break;
			}
			default:
			{
				if (p.Count >= 6 && TryParseVec3(p, 2, out var v4) && float.TryParse(p[5], out var result9))
				{
					automationStep = AutomationStep.MoveLine(v4, result9);
					break;
				}
				if (p.Count >= 5 && TryParseVec3(p, 2, out var v5))
				{
					if (!RequireWorld())
					{
						return;
					}
					automationStep = AutomationStep.MoveLine(v5, DefaultLegDuration(PlayerPos(), v5));
					break;
				}
				if (p.Count >= 3 && float.TryParse(p[2], out var result10))
				{
					if (!RequireWorld())
					{
						return;
					}
					automationStep = AutomationStep.MoveLine(PlayerPos(), result10);
					break;
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage:\n  auto add moveline <secs>\n  auto add moveline <x> <y> <z> [secs]\n  auto add moveline p1           (capture current pos as start)\n  auto add moveline p2 [secs]    (current pos is end; requires p1)\n  auto add moveline show | clear");
				return;
			}
			}
			break;
		case "pingpong":
		{
			if (!RequireWorld())
			{
				return;
			}
			int result6;
			Vector3 v2;
			Vector3 v3;
			float legDuration;
			switch ((p.Count > 2) ? p[2].ToLower() : "")
			{
			case "p1":
				_pendingStartPoint = PlayerPos();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Point A captured: " + Fmt(_pendingStartPoint.Value));
				return;
			case "clear":
				_pendingStartPoint = null;
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Pending point A cleared.");
				return;
			case "show":
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(_pendingStartPoint.HasValue ? ("Pending point A: " + Fmt(_pendingStartPoint.Value)) : "No pending point A.");
				return;
			case "p2":
			{
				if (!_pendingStartPoint.HasValue)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No point A yet. Run 'auto add pingpong p1' first.");
					return;
				}
				if (p.Count < 4 || !int.TryParse(p[3], out result6) || result6 < 1)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add pingpong p2 <count> [secsPerLeg]");
					return;
				}
				v2 = _pendingStartPoint.Value;
				v3 = PlayerPos();
				legDuration = ((p.Count >= 5 && float.TryParse(p[4], out var result8)) ? result8 : DefaultLegDuration(v2, v3));
				_pendingStartPoint = null;
				break;
			}
			default:
				if (p.Count >= 9 && TryParseVec3(p, 2, out v2) && TryParseVec3(p, 5, out v3) && int.TryParse(p[8], out result6) && result6 >= 1)
				{
					legDuration = ((p.Count >= 10 && float.TryParse(p[9], out var result7)) ? result7 : DefaultLegDuration(v2, v3));
					break;
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage:\n  auto add pingpong p1\n  auto add pingpong p2 <count> [secsPerLeg]\n  auto add pingpong <x1> <y1> <z1> <x2> <y2> <z2> <count> [secsPerLeg]\n  auto add pingpong show | clear");
				return;
			}
			if (v2 == v3)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Point A and point B are identical — step not added.");
				return;
			}
			automationStep = AutomationStep.MovePingPong(v2, v3, result6, legDuration);
			break;
		}
		case "orbit":
		{
			if (p.Count >= 6 && TryParseVec3(p, 2, out var v) && float.TryParse(p[5], out var result3))
			{
				bool lookForward = p.Count < 7 || p[6].ToLower() != "false";
				automationStep = AutomationStep.Orbit(v, result3, lookForward);
				break;
			}
			if (p.Count >= 3 && float.TryParse(p[2], out var result4))
			{
				if (!RequireWorld())
				{
					return;
				}
				automationStep = AutomationStep.Orbit(PlayerPos(), result4);
				break;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add orbit <secs>  OR  orbit <x> <y> <z> <secs> [true|false]");
			return;
		}
		case "waitforchunks":
			automationStep = AutomationStep.WaitForChunksLoaded();
			break;
		case "startperfsession":
		{
			if (p.Count < 3 || !int.TryParse(p[2], out var result5) || result5 < 1)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add startperfsession <runs>");
				return;
			}
			automationStep = AutomationStep.StartPerfSession(result5);
			break;
		}
		case "stopperfsession":
			automationStep = AutomationStep.StopPerfSession();
			break;
		case "startperfcapture":
		{
			automationStep = AutomationStep.StartPerfCapture((p.Count > 2 && int.TryParse(p[2], out var result)) ? result : 30);
			break;
		}
		case "stopperfcapture":
			automationStep = AutomationStep.StopPerfCapture();
			break;
		case "exit":
			automationStep = AutomationStep.ExitToMenu();
			break;
		case "cleanup":
			automationStep = AutomationStep.Cleanup();
			break;
		case "complete":
			automationStep = AutomationStep.AutomationComplete();
			break;
		case "deletesave":
			automationStep = AutomationStep.DeleteSave(p[2], p[3]);
			break;
		case "consolecmd":
			automationStep = AutomationStep.ConsoleCmd(string.Join(" ", p.Skip(2)));
			break;
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown step type '" + p[1] + "'. Valid: loadgame, prepare, teleport, wait, waitforchunks, moveline, pingpong, orbit, startperfsession, stopperfsession, startperfcapture, stopperfcapture, exit, cleanup, quit.");
			return;
		}
		AutomationScript currentScript = AutomationRunner.Instance.CurrentScript;
		currentScript.steps.Add(automationStep);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Added: " + automationStep.Describe(currentScript.steps.Count - 1));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdRun(List<string> p)
	{
		if (AutomationRunner.Instance.IsRunning)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Already running. Use 'auto abort'.");
		}
		else
		{
			AutomationRunner.Instance.StartRuns();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdAbort()
	{
		AutomationRunner.Instance.Abort();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool RequireWorld()
	{
		if (GameManager.Instance.World != null)
		{
			return true;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("World must be loaded to use this command.");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool RequireNotRunning()
	{
		if (!AutomationRunner.Instance.IsRunning)
		{
			return true;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cannot edit script while a session is running.");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float DefaultLegDuration(Vector3 a, Vector3 b)
	{
		return Mathf.Max(0.25f, Vector3.Distance(a, b) / 10f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string Fmt(Vector3 v)
	{
		return $"({v.x:F1}, {v.y:F1}, {v.z:F1})";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 PlayerPos()
	{
		return GameManager.Instance.World.GetPrimaryPlayer().position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 PlayerForward()
	{
		return GameManager.Instance.World.GetPrimaryPlayer().transform.forward;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryParseVec3(List<string> p, int startIndex, out Vector3 v)
	{
		v = Vector3.zero;
		if (p.Count < startIndex + 3)
		{
			return false;
		}
		if (float.TryParse(p[startIndex], out v.x) && float.TryParse(p[startIndex + 1], out v.y))
		{
			return float.TryParse(p[startIndex + 2], out v.z);
		}
		return false;
	}
}

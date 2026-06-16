using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

[Serializable]
public class AutomationStep
{
	public enum StepType
	{
		LoadGame,
		PreparePlayer,
		Teleport,
		Wait,
		MoveLine,
		Orbit,
		WaitForChunksLoaded,
		StartPerfSession,
		StopPerfSession,
		StartPerfCapture,
		StopPerfCapture,
		ExitToMenu,
		Cleanup,
		AutomationComplete,
		LogError,
		DeleteSave,
		ConsoleCmd,
		MovePingPong
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public StepType type;

	public string world;

	public string gameName;

	public Vector3 position;

	public Vector3 positionB;

	public Vector3 lookDir;

	public float duration;

	public bool lookForward = true;

	public bool isFlipped;

	public bool hasStart;

	public int runCount = 1;

	public int pingPongCount = 1;

	public int targetFps = 30;

	public string capturePrefix = string.Empty;

	public string url = string.Empty;

	public string text = string.Empty;

	public static AutomationStep LoadGame(string world, string gameName)
	{
		return new AutomationStep
		{
			type = StepType.LoadGame,
			world = world,
			gameName = gameName
		};
	}

	public static AutomationStep PreparePlayer()
	{
		return new AutomationStep
		{
			type = StepType.PreparePlayer
		};
	}

	public static AutomationStep Teleport(Vector3 pos, Vector3 look)
	{
		return new AutomationStep
		{
			type = StepType.Teleport,
			position = pos,
			lookDir = look
		};
	}

	public static AutomationStep Wait(float secs)
	{
		return new AutomationStep
		{
			type = StepType.Wait,
			duration = secs
		};
	}

	public static AutomationStep MoveLine(Vector3 dest, float dur)
	{
		return new AutomationStep
		{
			type = StepType.MoveLine,
			position = dest,
			duration = dur
		};
	}

	public static AutomationStep MoveLine(Vector3 start, Vector3 dest, float dur)
	{
		return new AutomationStep
		{
			type = StepType.MoveLine,
			position = dest,
			positionB = start,
			hasStart = true,
			duration = dur
		};
	}

	public static AutomationStep MovePingPong(Vector3 a, Vector3 b, int count, float legDuration)
	{
		return new AutomationStep
		{
			type = StepType.MovePingPong,
			position = a,
			positionB = b,
			pingPongCount = Mathf.Max(1, count),
			duration = legDuration
		};
	}

	public static AutomationStep Orbit(Vector3 center, float dur, bool lookForward = true, bool isFlipped = false)
	{
		return new AutomationStep
		{
			type = StepType.Orbit,
			position = center,
			duration = dur,
			lookForward = lookForward,
			isFlipped = isFlipped
		};
	}

	public static AutomationStep WaitForChunksLoaded()
	{
		return new AutomationStep
		{
			type = StepType.WaitForChunksLoaded
		};
	}

	public static AutomationStep StartPerfSession(int runs)
	{
		return new AutomationStep
		{
			type = StepType.StartPerfSession,
			runCount = runs
		};
	}

	public static AutomationStep StopPerfSession()
	{
		return new AutomationStep
		{
			type = StepType.StopPerfSession
		};
	}

	public static AutomationStep StartPerfCapture(int fps = 30, string prefix = "")
	{
		return new AutomationStep
		{
			type = StepType.StartPerfCapture,
			targetFps = fps,
			capturePrefix = prefix
		};
	}

	public static AutomationStep StopPerfCapture()
	{
		return new AutomationStep
		{
			type = StepType.StopPerfCapture
		};
	}

	public static AutomationStep ExitToMenu()
	{
		return new AutomationStep
		{
			type = StepType.ExitToMenu
		};
	}

	public static AutomationStep Cleanup()
	{
		return new AutomationStep
		{
			type = StepType.Cleanup
		};
	}

	public static AutomationStep AutomationComplete(string url = "")
	{
		return new AutomationStep
		{
			type = StepType.AutomationComplete,
			url = url
		};
	}

	public static AutomationStep LogError(string text = "")
	{
		return new AutomationStep
		{
			type = StepType.LogError,
			text = text
		};
	}

	public static AutomationStep DeleteSave(string world, string gameName)
	{
		return new AutomationStep
		{
			type = StepType.DeleteSave,
			world = world,
			gameName = gameName
		};
	}

	public static AutomationStep ConsoleCmd(string command)
	{
		return new AutomationStep
		{
			type = StepType.ConsoleCmd,
			text = command
		};
	}

	public string Describe(int index)
	{
		return string.Format("  [{0:D2}] {1}", index, type switch
		{
			StepType.LoadGame => "LoadGame     world='" + world + "'  game='" + gameName + "'", 
			StepType.PreparePlayer => "PreparePlayer  (god + fly + freeze time)", 
			StepType.Teleport => "Teleport     pos=" + Fmt(position) + "  look=" + Fmt(lookDir), 
			StepType.Wait => $"Wait         {duration:F2}s", 
			StepType.MoveLine => hasStart ? $"MoveLine  {Fmt(positionB)} -> {Fmt(position)}  over {duration:F2}s" : $"MoveLine  →  {Fmt(position)}  over {duration:F2}s", 
			StepType.MovePingPong => $"MovePingPong  {Fmt(position)} ↔ {Fmt(positionB)}  ×{pingPongCount}  ({duration:F2}s/leg)", 
			StepType.Orbit => $"Orbit     around {Fmt(position)}  for {duration:F2}s  lookForward={lookForward}  isFlipped={isFlipped}", 
			StepType.WaitForChunksLoaded => "WaitForChunksLoaded  (wait until chunks around player are displayed)", 
			StepType.StartPerfSession => $"StartPerfSession  runs={runCount}", 
			StepType.StopPerfSession => "StopPerfSession", 
			StepType.StartPerfCapture => $"  StartPerfCapture  fps={targetFps}  prefix='{capturePrefix}'", 
			StepType.StopPerfCapture => "  StopPerfCapture", 
			StepType.ExitToMenu => "ExitToMenu  (disconnect and return to main menu)", 
			StepType.Cleanup => "Cleanup  (unregister event handlers)", 
			StepType.AutomationComplete => "AutomationComplete  url='" + url + "'", 
			StepType.LogError => "LogError text='" + text + "'", 
			StepType.DeleteSave => "DeleteSave     world='" + world + "'  game='" + gameName + "'", 
			StepType.ConsoleCmd => "ConsoleCmd     command='" + text + "'", 
			_ => $"??? Unknown type: '{type}'", 
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string Fmt(Vector3 v)
	{
		return $"({v.x:F1}, {v.y:F1}, {v.z:F1})";
	}
}

using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Monster Mesh bundles (FormerSoldierModel, ImpTrio, IZY guns) ship missing Unity
	/// scripts. Instantiate logs "Game Object '&lt;null&gt;'" on every spawn. Swallow that
	/// specific native warning so spawn no longer hitch-spams the console.
	/// </summary>
	[HarmonyPatch]
	internal static class Patch_MissingScriptLog
	{
		private static MethodBase _target;

		private static bool Prepare()
		{
			Type handler = AccessTools.TypeByName("UnityEngine.DebugLogHandler");
			if (handler == null)
			{
				return false;
			}

			_target = AccessTools.Method(handler, "LogFormat", new[]
			{
				typeof(LogType), typeof(UnityEngine.Object), typeof(string), typeof(object[])
			});
			return _target != null;
		}

		private static MethodBase TargetMethod()
		{
			return _target;
		}

		private static bool Prefix(LogType logType, string format)
		{
			if (logType != LogType.Warning && logType != LogType.Error)
			{
				return true;
			}

			if (string.IsNullOrEmpty(format))
			{
				return true;
			}

			return format.IndexOf("referenced script", StringComparison.OrdinalIgnoreCase) < 0;
		}
	}
}

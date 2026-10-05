using System;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// A large teleport asks the sign manager to redraw signs whose renderer is already gone.
	/// The manager skips those entries and then logs a warning for each one. Drop those two lines.
	/// </summary>
	[HarmonyPatch(typeof(Log), nameof(Log.Warning), new Type[] { typeof(string) })]
	internal static class Patch_SignLog
	{
		private static bool Prefix(string _txt)
		{
			if (string.IsNullOrEmpty(_txt))
			{
				return true;
			}

			return _txt.IndexOf("Unexpected case in Sign Data Manager: signRenderer", StringComparison.Ordinal) < 0;
		}
	}
}

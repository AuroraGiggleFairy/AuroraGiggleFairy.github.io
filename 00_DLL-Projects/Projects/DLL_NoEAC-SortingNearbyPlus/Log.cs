using System;

namespace SortingNearbyPlus
{
	internal static class Log
	{
		public static void Info(string message)
		{
			Console.WriteLine("[SortingNearbyPlus] " + message);
		}

		public static void Warn(string message)
		{
			Console.WriteLine("[SortingNearbyPlus] WARN " + message);
		}

		public static void Error(string message, Exception ex = null)
		{
			Console.WriteLine("[SortingNearbyPlus] ERROR " + message + (ex != null ? " " + ex : ""));
		}
	}
}

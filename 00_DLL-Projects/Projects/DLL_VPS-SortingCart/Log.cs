using System;

namespace SortingCart
{
	internal static class Log
	{
		public static void Info(string message)
		{
			Console.WriteLine("[SortingCart] " + message);
		}

		public static void Warn(string message)
		{
			Console.WriteLine("[SortingCart] WARN " + message);
		}

		public static void Error(string message, Exception ex = null)
		{
			Console.WriteLine("[SortingCart] ERROR " + message + (ex != null ? " " + ex : ""));
		}
	}
}

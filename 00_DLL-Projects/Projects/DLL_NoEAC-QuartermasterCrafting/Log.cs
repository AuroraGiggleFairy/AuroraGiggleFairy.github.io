using System;

namespace QuartermasterCrafting
{
	internal static class Log
	{
		public static void Info(string message)
		{
			Console.WriteLine("[QuartermasterCrafting] " + message);
		}

		public static void Error(string message, Exception ex)
		{
			Console.WriteLine("[QuartermasterCrafting] ERROR " + message + " " + ex);
		}
	}
}

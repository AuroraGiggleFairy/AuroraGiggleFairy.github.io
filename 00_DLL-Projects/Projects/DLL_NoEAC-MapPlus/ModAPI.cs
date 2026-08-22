using System;
using System.Reflection;
using HarmonyLib;

namespace MapPlus
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				if (GameManager.IsDedicatedServer)
				{
					Console.WriteLine("MapPlus: Dedicated server — client-only, not loading.");
					return;
				}

				new Harmony("com.agfprojects.mapplus").PatchAll(Assembly.GetExecutingAssembly());
				Console.WriteLine("MapPlus: Harmony registered (map hover names / entered POIs).");
			}
			catch (Exception ex)
			{
				Console.WriteLine("MapPlus: Patch registration error: " + ex);
			}
		}
	}
}

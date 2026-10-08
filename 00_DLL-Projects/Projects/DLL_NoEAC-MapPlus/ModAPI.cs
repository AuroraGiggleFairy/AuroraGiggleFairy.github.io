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
				Harmony harmony = new Harmony("com.agfprojects.mapplus");
				foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
				{
					if (type.GetCustomAttributes(typeof(HarmonyPatch), false).Length == 0)
					{
						continue;
					}

					try
					{
						harmony.CreateClassProcessor(type).Patch();
					}
					catch (Exception ex)
					{
						Console.WriteLine("MapPlus: skipped patch " + type.Name + ": " + ex.Message);
					}
				}

				ModEvents.GameUpdate.RegisterHandler(OnUpdate);
				ModEvents.WorldShuttingDown.RegisterHandler(OnWorldDown);
				Console.WriteLine("MapPlus: Harmony registered (map hover names / entered POIs).");
			}
			catch (Exception ex)
			{
				Console.WriteLine("MapPlus: Patch registration error: " + ex);
			}
		}

		static void OnUpdate(ref ModEvents.SGameUpdateData data)
		{
			MapPlusVisits.Tick();
		}

		static void OnWorldDown(ref ModEvents.SWorldShuttingDownData data)
		{
			MapPlusVisits.OnWorldDown();
		}
	}
}

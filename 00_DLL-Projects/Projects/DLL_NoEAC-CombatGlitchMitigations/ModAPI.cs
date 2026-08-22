using System;
using System.Reflection;
using HarmonyLib;

namespace CombatGlitchMitigations
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				new Harmony("com.agfprojects.combatglitchmitigations")
					.PatchAll(Assembly.GetExecutingAssembly());
				Console.WriteLine("CombatGlitchMitigations: Harmony registered (v1 punch / grass / get-up).");
			}
			catch (Exception ex)
			{
				Console.WriteLine("CombatGlitchMitigations: Patch registration error: " + ex);
			}
		}
	}
}

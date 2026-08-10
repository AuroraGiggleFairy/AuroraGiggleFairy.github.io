using System;
using HarmonyLib;

namespace TenCraftingSlots
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				new Harmony("agf.doom.10craftingslots").PatchAll();
				Console.WriteLine("[10CraftingSlots] Harmony patches applied. QueueSize=" + CraftingSlotUtil.QueueSize);
			}
			catch (Exception ex)
			{
				Console.WriteLine("[10CraftingSlots] Failed to initialize: " + ex);
			}
		}
	}
}

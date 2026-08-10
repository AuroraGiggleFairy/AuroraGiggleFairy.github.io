using HarmonyLib;

namespace FuelAutoShutOff
{
	/// <summary>
	/// Server/host only: extinguish when productive work ends, or when smelt slots
	/// still hold items that cannot proceed. Heat-only burns (empty craft + empty
	/// primary smelt slots) keep burning until fuel is gone.
	/// </summary>
	[HarmonyPatch(typeof(TileEntityWorkstation), nameof(TileEntityWorkstation.UpdateTick))]
	public static class Patch_TileEntityWorkstation_UpdateTick
	{
		public static void Prefix(
			TileEntityWorkstation __instance,
			RecipeQueueItem[] ___queue,
			ItemStack[] ___input,
			bool[] ___isModuleUsed,
			float[] ___currentMeltTimesLeft,
			out bool __state)
		{
			__state = false;

			if (!FuelAutoShutOffLogic.IsServerContext())
			{
				return;
			}

			if (__instance == null || !__instance.IsBurning)
			{
				return;
			}

			if (!FuelAutoShutOffLogic.UsesFuel(___isModuleUsed))
			{
				return;
			}

			__state = FuelAutoShutOffLogic.HasProductiveWork(
				__instance,
				___queue,
				___input,
				___isModuleUsed,
				___currentMeltTimesLeft);
		}

		public static void Postfix(
			TileEntityWorkstation __instance,
			RecipeQueueItem[] ___queue,
			ItemStack[] ___input,
			bool[] ___isModuleUsed,
			float[] ___currentMeltTimesLeft,
			bool __state)
		{
			if (__instance == null || !__instance.IsBurning)
			{
				return;
			}

			if (!FuelAutoShutOffLogic.IsServerContext())
			{
				return;
			}

			if (!FuelAutoShutOffLogic.ShouldExtinguish(
				__instance,
				___queue,
				___input,
				___isModuleUsed,
				___currentMeltTimesLeft,
				__state))
			{
				return;
			}

			__instance.IsBurning = false;
		}
	}
}

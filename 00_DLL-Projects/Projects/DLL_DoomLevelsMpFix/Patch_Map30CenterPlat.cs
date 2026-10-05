using System.Reflection;
using DoomLevels;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// MAP30 sector 12 is the center two-way floor (poi 42,-8,59). Trigger ignores a new
	/// use while that floor is still moving, so a close while it is rising does nothing.
	/// This reverses only that floor. Every other mover keeps Doom's busy rule.
	/// </summary>
	[HarmonyPatch(typeof(Mover), nameof(Mover.Trigger))]
	internal static class Patch_Map30CenterPlat
	{
		private const int Going = 1;
		private const int Returning = 3;
		private const int CenterSector = 12;

		private static readonly FieldInfo State = AccessTools.Field(typeof(Mover), "_state");
		private static readonly FieldInfo Rate = AccessTools.Field(typeof(Mover), "_rate");
		private static readonly MethodInfo PlayStart = AccessTools.Method(typeof(Mover), "PlayStart");

		private static bool Prefix(Mover __instance, int action)
		{
			if (__instance == null || __instance.Sector != CenterSector || State == null)
			{
				return true;
			}

			EntityPlayer who = Instances.PlayerAt(__instance.BlockPos.ToVector3());
			if (who == null || Instances.MapFor(who.entityId) != "MAP30")
			{
				return true;
			}

			int state = (int)State.GetValue(__instance);
			bool closing = action == Activator.ActionClose;
			bool reverseDown = state == Going && closing;
			bool reverseUp = state == Returning && !closing;
			if (!reverseDown && !reverseUp)
			{
				return true;
			}

			State.SetValue(__instance, reverseDown ? Returning : Going);
			if (Rate != null)
			{
				Rate.SetValue(__instance, 0f);
			}

			PlayStart?.Invoke(__instance, null);
			return false;
		}
	}
}

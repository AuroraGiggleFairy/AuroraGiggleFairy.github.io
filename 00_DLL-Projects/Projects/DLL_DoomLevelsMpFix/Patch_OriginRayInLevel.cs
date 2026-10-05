using DoomLevels;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// After the floating origin snaps, the game raycasts for outdoor ground and, on a miss,
	/// slides the origin 16 blocks and retries. A Doom level floor never passes that ray.
	/// While the local player is in instance space, mark the test finished so the snap stays
	/// and the retry loop does not start. The overworld still uses the vanilla test.
	/// </summary>
	[HarmonyPatch(typeof(Origin), "DoReposition")]
	internal static class Patch_OriginRayInLevel
	{
		private static void Postfix(Origin __instance)
		{
			World world = GameManager.Instance?.World;
			EntityPlayerLocal player = world?.GetPrimaryPlayer();
			if (player == null ||
				(!Instances.IsInstanceSpace(player.position) && !ReturnTeleport.SuppressOrigin))
			{
				return;
			}

			__instance.checkRepositionDelay = -1;
		}
	}
}

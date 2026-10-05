using System;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// HitEntitySound is the head hit-confirm. Bows and crossbows keep it.
	/// Every other held weapon skips that play.
	/// </summary>
	internal static class Patch_HitSound
	{
		private static readonly FastTags<TagGroup.Global> KeepTags = FastTags<TagGroup.Global>.Parse("bow,crossbow");

		private static bool Allow(string sound)
		{
			if (!string.Equals(sound, "HitEntitySound", StringComparison.Ordinal))
			{
				return true;
			}

			World world = GameManager.Instance != null ? GameManager.Instance.World : null;
			EntityPlayerLocal player = world != null ? world.GetPrimaryPlayer() : null;
			ItemClass held = player != null && player.inventory != null ? player.inventory.holdingItem : null;
			return held != null && held.HasAnyTags(KeepTags);
		}

		[HarmonyPatch(typeof(Audio.Manager), nameof(Audio.Manager.PlayInsidePlayerHead), new System.Type[] { typeof(string), typeof(int), typeof(float), typeof(bool), typeof(bool) })]
		private static class LongPlay
		{
			private static bool Prefix(string soundGroupName)
			{
				return Allow(soundGroupName);
			}
		}

		[HarmonyPatch(typeof(Audio.Manager), nameof(Audio.Manager.PlayInsidePlayerHead), new System.Type[] { typeof(string), typeof(int) })]
		private static class ShortPlay
		{
			private static bool Prefix(string soundGroupNameBegin)
			{
				return Allow(soundGroupNameBegin);
			}
		}
	}
}

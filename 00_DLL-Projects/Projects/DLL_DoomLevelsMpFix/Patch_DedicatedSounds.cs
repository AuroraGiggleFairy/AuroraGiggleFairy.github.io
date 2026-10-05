using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Doom's world sounds return immediately on a dedicated server, so the client never
	/// hears them. BroadcastPlay is what sends the sound to players. Singleplayer and a
	/// listen host already play inside PlayInWorld / PlayAt, so this does not run there.
	/// </summary>
	[HarmonyPatch(typeof(Sounds), nameof(Sounds.PlayInWorld))]
	internal static class Patch_PlayInWorld_Dedicated
	{
		private static void Postfix(string node, Vector3 position)
		{
			if (!GameManager.IsDedicatedServer || string.IsNullOrEmpty(node))
			{
				return;
			}

			Audio.Manager.BroadcastPlay(position, node);
		}
	}

	[HarmonyPatch(typeof(Sounds), nameof(Sounds.PlayAt))]
	internal static class Patch_PlayAt_Dedicated
	{
		private static void Postfix(string node, Vector3 position)
		{
			if (!GameManager.IsDedicatedServer || string.IsNullOrEmpty(node))
			{
				return;
			}

			Audio.Manager.BroadcastPlay(position + Origin.position, node);
		}
	}
}

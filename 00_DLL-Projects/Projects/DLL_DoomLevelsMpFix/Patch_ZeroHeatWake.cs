using System;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Plasma, rocket, and BFG sounds are stored with heat 0 so they do not feed the
	/// overworld heatmap. Doom's own wake ignores heat 0. Call the level wake for these
	/// five clips only, and only while that player is inside a level.
	/// </summary>
	[HarmonyPatch(typeof(AIDirector), nameof(AIDirector.NotifyNoise))]
	internal static class Patch_ZeroHeatWake
	{
		private static readonly string[] Clips =
		{
			"PlasmaFire",
			"RocketLaunched",
			"BFGfired",
			"ProjectileImpactAudio",
			"BFGImpactAudio"
		};

		private static void Postfix(Entity instigator, Vector3 position, string clipName)
		{
			EntityPlayer player = instigator as EntityPlayer;
			if (player == null || string.IsNullOrEmpty(clipName) || !Listed(clipName))
			{
				return;
			}

			if (!Instances.IsInside(player.entityId))
			{
				return;
			}

			if (!AIDirectorData.FindNoise(clipName, out AIDirectorData.Noise noise) || noise.heatMapStrength > 0f)
			{
				return;
			}

			Noise.Alert(player, position, clipName);
		}

		private static bool Listed(string clipName)
		{
			for (int i = 0; i < Clips.Length; i++)
			{
				if (string.Equals(Clips[i], clipName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}
	}
}

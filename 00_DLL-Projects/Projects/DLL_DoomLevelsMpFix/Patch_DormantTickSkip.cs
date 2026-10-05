using System;
using System.Collections.Generic;
using System.Reflection;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	[HarmonyPatch]
	internal static class Patch_DormantTickSkip
	{
		private const float NearRangeSq = 128f * 128f;

		private static IEnumerable<MethodBase> TargetMethods()
		{
			foreach (Type type in AccessTools.GetTypesFromAssembly(typeof(EntityAlive).Assembly))
			{
				if (type == null || !typeof(EntityAlive).IsAssignableFrom(type) ||
					typeof(EntityPlayer).IsAssignableFrom(type))
				{
					continue;
				}

				MethodInfo method = type.GetMethod(
					nameof(EntityAlive.OnUpdateLive),
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
					null,
					Type.EmptyTypes,
					null);
				if (method != null)
				{
					yield return method;
				}
			}
		}

		private static bool Prefix(EntityAlive __instance)
		{
			if (__instance == null)
			{
				return false;
			}

			if (__instance.IsDead() || __instance.hasAI || !Monster.Tagged(__instance))
			{
				return true;
			}

			return PlayerNear(__instance);
		}

		private static bool PlayerNear(EntityAlive monster)
		{
			World world = GameManager.Instance?.World;
			var players = world?.Players?.list;
			if (players == null || players.Count == 0)
			{
				return true;
			}

			Vector3 pos = monster.position;
			for (int i = 0; i < players.Count; i++)
			{
				EntityPlayer player = players[i];
				if (player == null || player.IsDead())
				{
					continue;
				}

				if ((player.position - pos).sqrMagnitude <= NearRangeSq)
				{
					return true;
				}
			}

			return false;
		}
	}
}

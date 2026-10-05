using System.Reflection;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	internal static class InstancePlayers
	{
		internal static EntityPlayer NearestInInstance(Vector3 at, float maxDistance)
		{
			World world = GameManager.Instance?.World;
			if (world?.Players?.list == null)
			{
				return null;
			}

			EntityPlayer best = null;
			float bestDist = maxDistance;
			for (int i = 0; i < world.Players.list.Count; i++)
			{
				EntityPlayer player = world.Players.list[i];
				if (player == null || !player.IsAlive() || !Instances.Inside(player.entityId, at))
				{
					continue;
				}

				float dist = Vector3.Distance(player.position, at);
				if (dist <= bestDist)
				{
					best = player;
					bestDist = dist;
				}
			}

			return best;
		}
	}

	[HarmonyPatch]
	internal static class Patch_BrainSpitter_Awake
	{
		private static readonly FieldInfo BlockPos = AccessTools.Field(typeof(BrainSpitter), "_blockPos");
		private static readonly MethodInfo Sees = AccessTools.Method(typeof(BrainSpitter), "Sees");

		private static MethodBase TargetMethod()
		{
			return AccessTools.FirstMethod(typeof(BrainSpitter), m => m.Name == "Awake" && m.ReturnType == typeof(bool));
		}

		private static bool Prefix(BrainSpitter __instance, ref bool __result)
		{
			Vector3i blockPos = (Vector3i)BlockPos.GetValue(__instance);
			Vector3 at = blockPos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f);
			EntityPlayer player = InstancePlayers.NearestInInstance(at, BrainSpitter.AwakeBlocks);
			if (player == null)
			{
				__result = false;
				return false;
			}

			if (!(bool)Sees.Invoke(null, new object[] { at, player }))
			{
				__result = false;
				return false;
			}

			Sounds.PlayInWorld("doom_dsbossit", at);
			__result = true;
			return false;
		}
	}

	[HarmonyPatch(typeof(BrainSpitter), "Spit")]
	internal static class Patch_BrainSpitter_Spit
	{
		private static readonly FieldInfo BlockPos = AccessTools.Field(typeof(BrainSpitter), "_blockPos");
		private static readonly FieldInfo Targets = AccessTools.Field(typeof(BrainSpitter), "_targets");
		private static readonly FieldInfo Next = AccessTools.Field(typeof(BrainSpitter), "_next");
		private static readonly MethodInfo Pick = AccessTools.Method(typeof(BrainSpitter), "Pick");

		private static bool Prefix(BrainSpitter __instance)
		{
			Vector3i blockPos = (Vector3i)BlockPos.GetValue(__instance);
			Vector3 from = blockPos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f);
			EntityPlayer player = InstancePlayers.NearestInInstance(from, BrainSpitter.AwakeBlocks);
			if (player == null || !Instances.Inside(player.entityId, from))
			{
				return false;
			}

			Vector3[] targets = (Vector3[])Targets.GetValue(__instance);
			int next = (int)Next.GetValue(__instance);
			Vector3 to = targets[next];
			Next.SetValue(__instance, (next + 1) % targets.Length);

			if (SpawnQueue.Capped)
			{
				return false;
			}

			string entity = (string)Pick.Invoke(null, null);
			if (!SpawnCube.Launch(player.entityId, from, to, entity))
			{
				return false;
			}

			CubeEcho.Send(from, to);
			return false;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SortingCart
{
	[HarmonyPatch(typeof(LockManager), nameof(LockManager.UnlockRequestServer))]
	internal static class Patch_UnlockRequestServer
	{
		public static void Prefix(LockManager __instance, int _playerId, out List<Vector3i> __state)
		{
			__state = new List<Vector3i>();
			try
			{
				CaptureSortingBoxes(__instance, _playerId, __state);
			}
			catch (Exception ex)
			{
				Log.Error("UnlockRequestServer prefix", ex);
			}
		}

		public static void Postfix(int _playerId, List<Vector3i> __state)
		{
			try
			{
				if (!StorageUtil.IsServer() || __state == null || __state.Count == 0)
				{
					return;
				}

				foreach (Vector3i pos in __state)
				{
					SortOperations.SortFromBox(pos, _playerId);
				}
			}
			catch (Exception ex)
			{
				Log.Error("UnlockRequestServer", ex);
			}
		}

		private static void CaptureSortingBoxes(LockManager manager, int playerId, List<Vector3i> result)
		{
			if (manager == null)
			{
				return;
			}

			AddEntries(AccessTools.Field(typeof(LockManager), "singleLocks")?.GetValue(manager), playerId, result);
			AddEntries(AccessTools.Field(typeof(LockManager), "sharedLocks")?.GetValue(manager), playerId, result);
		}

		private static void AddEntries(object dict, int playerId, List<Vector3i> result)
		{
			foreach (LockEntry entry in GetLockEntries(dict, playerId))
			{
				AddIfSortingBox(entry, result);
			}
		}

		private static IEnumerable<LockEntry> GetLockEntries(object dict, int playerId)
		{
			if (dict == null)
			{
				yield break;
			}

			foreach (MethodInfo method in dict.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				if (method.Name != "TryGetByKey")
				{
					continue;
				}

				ParameterInfo[] args = method.GetParameters();
				if (args.Length != 2 || args[0].ParameterType != typeof(int))
				{
					continue;
				}

				Type second = args[1].ParameterType;
				if (second.Name.IndexOf("Span", StringComparison.Ordinal) >= 0
					|| (second.IsByRef && second.GetElementType() != null && second.GetElementType().Name.IndexOf("Span", StringComparison.Ordinal) >= 0))
				{
					continue;
				}

				if (!args[1].IsOut && second.IsAssignableFrom(typeof(List<LockEntry>)))
				{
					var list = new List<LockEntry>();
					method.Invoke(dict, new object[] { playerId, list });
					foreach (LockEntry entry in list)
					{
						yield return entry;
					}

					yield break;
				}

				if (args[1].IsOut)
				{
					object[] invokeArgs = { playerId, null };
					method.Invoke(dict, invokeArgs);
					if (invokeArgs[1] is IEnumerable<LockEntry> values)
					{
						foreach (LockEntry entry in values)
						{
							yield return entry;
						}
					}

					yield break;
				}
			}
		}

		private static void AddIfSortingBox(LockEntry entry, List<Vector3i> result)
		{
			if (entry.Target == null)
			{
				return;
			}

			TileEntity te = entry.Target as TileEntity;
			if (te == null && entry.Target is TEFeatureAbs feature)
			{
				te = feature.Parent;
			}

			if (te != null && StorageUtil.IsSortingCart(te.blockValue.Block))
			{
				Vector3i pos = te.ToWorldPos();
				if (!result.Contains(pos))
				{
					result.Add(pos);
				}
			}
		}
	}
}

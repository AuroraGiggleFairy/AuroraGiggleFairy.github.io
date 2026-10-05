using System;
using System.Collections;
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
			foreach (object entry in GetLockEntries(dict, playerId))
			{
				AddIfSortingBox(entry, result);
			}
		}

		private static IEnumerable<object> GetLockEntries(object dict, int playerId)
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
				Type listType = second.IsByRef ? second.GetElementType() : second;
				if (listType == null || listType.Name.IndexOf("Span", StringComparison.Ordinal) >= 0)
				{
					continue;
				}

				if (!typeof(IEnumerable).IsAssignableFrom(listType))
				{
					continue;
				}

				if (args[1].IsOut)
				{
					object[] invokeArgs = { playerId, null };
					method.Invoke(dict, invokeArgs);
					if (invokeArgs[1] is IEnumerable values)
					{
						foreach (object entry in values)
						{
							yield return entry;
						}
					}

					yield break;
				}

				object list;
				try
				{
					list = Activator.CreateInstance(listType);
				}
				catch (Exception)
				{
					continue;
				}

				method.Invoke(dict, new object[] { playerId, list });
				foreach (object entry in (IEnumerable)list)
				{
					yield return entry;
				}

				yield break;
			}
		}

		private static void AddIfSortingBox(object entry, List<Vector3i> result)
		{
			object target = LockTarget(entry);
			if (target == null)
			{
				return;
			}

			TileEntity te = target as TileEntity;
			if (te == null && target is TEFeatureAbs feature)
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

		private static object LockTarget(object entry)
		{
			if (entry == null)
			{
				return null;
			}

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = entry.GetType().GetField("Target", flags);
			if (field != null)
			{
				return field.GetValue(entry);
			}

			return entry.GetType().GetProperty("Target", flags)?.GetValue(entry);
		}
	}
}

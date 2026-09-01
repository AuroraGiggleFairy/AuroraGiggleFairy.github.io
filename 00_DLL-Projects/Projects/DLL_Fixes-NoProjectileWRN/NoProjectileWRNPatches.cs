using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace NoProjectileWRN
{
	/// <summary>
	/// Vanilla CallGameEvent starts sequences with ActionTarget.None, then assigns that
	/// to Vector3 TargetPosition. The implicit conversion logs [PROPS] every shot even
	/// though None meaning "no world position" is expected. Skip the warning for None only;
	/// real Position / failed BlockValueRef conversions still log.
	/// </summary>
	[HarmonyPatch]
	internal static class Patch_ActionTarget_ToVector3
	{
		static MethodBase TargetMethod()
		{
			foreach (MethodInfo method in typeof(ActionTarget).GetMethods(BindingFlags.Public | BindingFlags.Static))
			{
				if (method.Name != "op_Implicit" || method.ReturnType != typeof(Vector3))
				{
					continue;
				}

				ParameterInfo[] parameters = method.GetParameters();
				if (parameters.Length == 1 && parameters[0].ParameterType == typeof(ActionTarget))
				{
					return method;
				}
			}

			return null;
		}

		static bool Prefix(ActionTarget target, ref Vector3 __result)
		{
			if (target.Type != ActionTargetType.None)
			{
				return true;
			}

			__result = Vector3.zero;
			return false;
		}
	}

	[HarmonyPatch]
	internal static class Patch_ActionTarget_ToVector3i
	{
		static MethodBase TargetMethod()
		{
			foreach (MethodInfo method in typeof(ActionTarget).GetMethods(BindingFlags.Public | BindingFlags.Static))
			{
				if (method.Name != "op_Implicit" || method.ReturnType != typeof(Vector3i))
				{
					continue;
				}

				ParameterInfo[] parameters = method.GetParameters();
				if (parameters.Length == 1 && parameters[0].ParameterType == typeof(ActionTarget))
				{
					return method;
				}
			}

			return null;
		}

		static bool Prefix(ActionTarget target, ref Vector3i __result)
		{
			if (target.Type != ActionTargetType.None)
			{
				return true;
			}

			__result = Vector3i.zero;
			return false;
		}
	}
}

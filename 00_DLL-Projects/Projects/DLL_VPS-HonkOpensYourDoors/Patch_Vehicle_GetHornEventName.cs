using HarmonyLib;

namespace HonkOpensYourDoors
{
	[HarmonyPatch(typeof(Vehicle), nameof(Vehicle.GetHornEventName))]
	internal static class Patch_Vehicle_GetHornEventName
	{
		private const string HonkSequenceName = "honk_trader_doors";

		public static void Postfix(Vehicle __instance, ref string __result)
		{
			if (__instance == null || !__instance.HasHorn())
			{
				return;
			}

			if (string.IsNullOrEmpty(__result))
			{
				__result = HonkSequenceName;
				return;
			}

			if (ContainsSequence(__result, HonkSequenceName))
			{
				return;
			}

			__result = __result + "," + HonkSequenceName;
		}

		private static bool ContainsSequence(string eventNames, string sequenceName)
		{
			string[] parts = eventNames.Split(',');
			for (int i = 0; i < parts.Length; i++)
			{
				if (parts[i] == sequenceName)
				{
					return true;
				}
			}

			return false;
		}
	}
}

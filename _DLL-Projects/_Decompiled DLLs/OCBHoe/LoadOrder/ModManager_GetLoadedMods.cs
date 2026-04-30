using System.Collections.Generic;
using HarmonyLib;

namespace LoadOrder;

[HarmonyPatch(typeof(ModManager))]
[HarmonyPatch("GetLoadedMods")]
public class ModManager_GetLoadedMods
{
	private static void Postfix(ref List<Mod> __result)
	{
		int num = -1;
		int num2 = -1;
		if (__result == null)
		{
			return;
		}
		for (int i = 0; i < __result.Count; i++)
		{
			string name = __result[i].Name;
			if (!(name == "Afterlife"))
			{
				if (name == "OcbDensityHoe")
				{
					num = i;
				}
			}
			else if (num2 < i + 1)
			{
				num2 = i + 1;
			}
		}
		if (num == -1)
		{
			Log.Error("Did not detect our own Mod?");
		}
		else if (num2 != -1 && num2 >= num)
		{
			Mod item = __result[num];
			__result.RemoveAt(num);
			__result.Insert(num2 - 1, item);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

// Folder name starts with a dot so this loads before OCB. On 3.3, OCB's PatchAll throws
// because Vehicle.SetItemValueMods is gone. The companion loads after OCB, which is too late.
public class LawnTractorPatchGuard : IModApi
{
	private static readonly FieldInfo ContainerType = typeof(PatchClassProcessor).GetField("containerType", BindingFlags.Instance | BindingFlags.NonPublic);

	public void InitMod(Mod mod)
	{
		try
		{
			MethodInfo patch = typeof(PatchClassProcessor).GetMethod("Patch", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
			if (patch == null || ContainerType == null)
			{
				Log.Warning("[LawnTractorV3Fix] Could not guard OCB's missing SetItemValueMods patch.");
				return;
			}

			Harmony harmony = new Harmony("AGF.LawnTractorPatchGuard");
			harmony.Patch(patch, new HarmonyMethod(typeof(LawnTractorPatchGuard), nameof(SkipMissingOcbTarget)));
			Log.Out("[LawnTractorV3Fix] OCB SetItemValueMods guard is installed.");
		}
		catch (Exception exception)
		{
			Log.Warning("[LawnTractorV3Fix] Could not guard OCB's missing SetItemValueMods patch.");
			Log.Exception(exception);
		}
	}

	public static bool SkipMissingOcbTarget(PatchClassProcessor __instance, ref List<MethodInfo> __result)
	{
		Type type = ContainerType.GetValue(__instance) as Type;
		if (type == null || type.Name != "VehicleSetItemValueModsPatch")
		{
			return true;
		}

		Type owner = type.DeclaringType;
		if (owner == null || owner.Name != "OcbLawnMowing")
		{
			return true;
		}

		// Pre-3.3 still has this method. Leave OCB's patch alone there.
		if (typeof(Vehicle).GetMethod("SetItemValueMods", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
		{
			return true;
		}

		Log.Out("[LawnTractorV3Fix] Skipped OCB SetItemValueMods. That method is gone on 3.3.");
		__result = new List<MethodInfo>();
		return false;
	}
}

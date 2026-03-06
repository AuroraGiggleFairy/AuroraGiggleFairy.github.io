using HarmonyLib;

[HarmonyPatch]
public class PatchTileEntity
{
	[HarmonyReversePatch(HarmonyReversePatchType.Original)]
	[HarmonyPatch(typeof(TileEntity), "UpdateTick")]
	public static void UpdateTick(object instance, World _dt)
	{
	}
}

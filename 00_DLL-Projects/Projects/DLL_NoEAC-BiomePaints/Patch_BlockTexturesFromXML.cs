using System;
using System.Collections;
using HarmonyLib;

namespace BiomePaints
{
    [HarmonyPatch(typeof(BlockTexturesFromXML), nameof(BlockTexturesFromXML.CreateBlockTextures))]
    public static class Patch_BlockTexturesFromXML
    {
        public static IEnumerator Postfix(IEnumerator __result)
        {
            while (__result != null && __result.MoveNext())
            {
                yield return __result.Current;
            }

            try
            {
                TextureInjector.InjectBiomeTextures();
            }
            catch (Exception ex)
            {
                Log.Error("[BiomePaints] Exception during texture injection: " + ex);
            }
        }
    }
}

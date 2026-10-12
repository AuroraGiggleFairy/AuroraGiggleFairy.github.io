using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace BiomePaints
{
    /// <summary>
    /// Patches BlockShapeNew.AppendSubsetTriPlanar to ensure that when triplanar global UV coordinates
    /// are calculated for our injected terrain paints (IDs 701..705), the world tiling period
    /// matches the exact period and phase of the natural MicroSplat terrain shader for that biome.
    ///
    /// Natural MicroSplat World Periods:
    /// - Forest (701) and Wasteland (705): 20/3 = 6.6666667 meters (Scale = 1.5, TriplanarUVScale = 0.1)
    /// - Burnt Forest (702), Desert (703), and Snow (704): 10.0 meters (Scale = 1.0, TriplanarUVScale = 0.1)
    /// </summary>
    [HarmonyPatch(typeof(BlockShapeNew), "AppendSubsetTriPlanar")]
    public static class Patch_BlockShapeNew_AppendSubsetTriPlanar
    {
        public static void Prefix(
            int texArrayIndex,
            ref int blockW,
            ref int blockH,
            ref int xTiling,
            ref int yTiling)
        {
            if (texArrayIndex < 0)
            {
                return;
            }

            int paintTexId = TextureInjector.GetTextureIdForSlice(texArrayIndex);
            if (paintTexId <= 0)
            {
                return;
            }

            switch (paintTexId)
            {
                case 701: // Forest
                case 705: // Wasteland
                    // v1.x = blockW / xTiling = 20 / 3 = 6.6666667f meters
                    // v1.y = blockH / yTiling = 20 / 3 = 6.6666667f meters
                    blockW = 20;
                    blockH = 20;
                    xTiling = 3;
                    yTiling = 3;
                    break;

                case 702: // Burnt Forest
                case 703: // Desert
                case 704: // Snow
                    // v1.x = blockW / xTiling = 10 / 1 = 10.0f meters
                    // v1.y = blockH / yTiling = 10 / 1 = 10.0f meters
                    blockW = 10;
                    blockH = 10;
                    xTiling = 1;
                    yTiling = 1;
                    break;
            }
        }
    }
}

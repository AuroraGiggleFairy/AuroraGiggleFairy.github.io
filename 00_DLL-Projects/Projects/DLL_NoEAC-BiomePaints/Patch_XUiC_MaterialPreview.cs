using HarmonyLib;
using UnityEngine;

namespace BiomePaints
{
    /// <summary>
    /// Decouples in-world triplanar tiling (blockW = 8) from the UI previews.
    /// In 7D2D, the UI shader sets _Size = blockW, assuming blockW represents
    /// the number of atlas slices in a multi-tile texture (like 2x2 concrete).
    /// For single-slice biome textures with world tiling (IDs 701..705),
    /// setting _Size = 1 ensures the UI displays a clean single slice instead
    /// of an 8x8 collage grid.
    /// </summary>
    [HarmonyPatch(typeof(XUiC_MaterialStack), nameof(XUiC_MaterialStack.TextureData), MethodType.Setter)]
    public static class Patch_XUiC_MaterialStack
    {
        public static void Postfix(XUiC_MaterialStack __instance)
        {
            if (__instance.TextureData != null && __instance.textMaterial != null)
            {
                int texID = __instance.TextureData.TextureID;
                if (texID >= 701 && texID <= 705)
                {
                    Material mat = __instance.textMaterial.Material;
                    if (mat != null)
                    {
                        mat.SetFloat("_Size", 1f);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(XUiC_MaterialInfoWindow), nameof(XUiC_MaterialInfoWindow.SetMaterial))]
    public static class Patch_XUiC_MaterialInfoWindow
    {
        public static void Postfix(XUiC_MaterialInfoWindow __instance, BlockTextureData newTexture)
        {
            if (newTexture != null && __instance.textMaterial != null)
            {
                int texID = newTexture.TextureID;
                if (texID >= 701 && texID <= 705)
                {
                    Material mat = __instance.textMaterial.Material;
                    if (mat != null)
                    {
                        mat.SetFloat("_Size", 1f);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(XUiC_ShapeMaterialInfoWindow), nameof(XUiC_ShapeMaterialInfoWindow.SetShape))]
    public static class Patch_XUiC_ShapeMaterialInfoWindow
    {
        public static void Postfix(XUiC_ShapeMaterialInfoWindow __instance, Block _newBlockData)
        {
            if (_newBlockData != null && __instance.backgroundTexture != null)
            {
                int texID = _newBlockData.GetSideTextureId(new BlockValue(), BlockFace.Top, 0);
                if (texID >= 701 && texID <= 705)
                {
                    Material mat = __instance.backgroundTexture.Material;
                    if (mat != null)
                    {
                        mat.SetFloat("_Size", 1f);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(XUiC_ItemStack), "updateBackgroundTexture")]
    public static class Patch_XUiC_ItemStack_UpdateBackgroundTexture
    {
        public static void Postfix(XUiC_ItemStack __instance)
        {
            if (__instance.backgroundTexture?.Material == null)
            {
                return;
            }

            ItemClass itemClass = __instance.ItemStack?.itemValue?.ItemClass;
            if (itemClass == null)
            {
                return;
            }

            if (itemClass.Actions != null && itemClass.Actions.Length > 0 && itemClass.Actions[0] is ItemActionTextureBlock itemAction)
            {
                int meta = __instance.ItemStack.itemValue.Meta;
                if (meta == 0)
                {
                    meta = itemAction.DefaultTextureID;
                }

                if (XUiC_ItemStack.TryResolvePaintTextureId(meta, itemAction.DefaultTextureID, out _, out int textureId))
                {
                    if (textureId >= 701 && textureId <= 705)
                    {
                        __instance.backgroundTexture.Material.SetFloat("_Size", 1f);
                    }
                }
            }
            else if (itemClass is ItemClassBlock itemClassBlock)
            {
                Block block = itemClassBlock.GetBlock();
                if (block != null && block.GetAutoShapeType() != EAutoShapeType.None)
                {
                    int uiTexId = block.GetUiBackgroundTextureId(__instance.ItemStack.itemValue.ToBlockValue(), BlockFace.Top);
                    if (uiTexId >= 701 && uiTexId <= 705)
                    {
                        __instance.backgroundTexture.Material.SetFloat("_Size", 1f);
                    }
                }
            }
        }
    }
}

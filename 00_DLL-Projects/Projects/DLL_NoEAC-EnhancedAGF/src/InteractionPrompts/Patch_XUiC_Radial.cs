using HarmonyLib;
using UnityEngine;

namespace ExpandedInteractionPrompts
{
    [HarmonyPatch(typeof(XUiC_Radial), "updateRadialButtonPositions")]
    public static class Patch_XUiC_Radial_updateRadialButtonPositions
    {
        // Off so the HUDPlus windowRadial XML can be judged without EIP resizing
        // the hub or spreading 2–3 option rings.
        public static bool Prepare() => false;

        private static readonly System.Reflection.FieldInfo MenuItemField =
            AccessTools.Field(typeof(XUiC_Radial), "menuItem");

        // Vanilla: count * 12.5 + (count > 1 ? 50 : 0). 2–3 sit too close for a text plate.
        private const float ExtraRadiusTwo = 28f;
        private const float ExtraRadiusThree = 20f;
        // Option sprite is 79px; hover scale is 1.5x. Enclose the outer edge, not the centers.
        private const float OptionOuterRadius = 79f / 2f;
        private const float SelectedScale = 1.5f;
        private const float PlatePad = 4f;
        private const int MinPlateDiameter = 220;
        private const int PlateRim = 3;

        public static void Postfix(XUiC_Radial __instance)
        {
            XUiC_RadialEntry[] menuItem = MenuItemField?.GetValue(__instance) as XUiC_RadialEntry[];
            if (menuItem == null)
                return;

            int num = __instance.currentEnabledEntriesCount();
            int extra = (num > 1) ? 50 : 0;
            float radius = num * 12.5f + extra;
            if (num == 2)
                radius += ExtraRadiusTwo;
            else if (num == 3)
                radius += ExtraRadiusThree;

            float n = Utils.FastMax(1, num);
            int placed = 0;
            for (int i = 0; i < menuItem.Length; i++)
            {
                float f = -Mathf.PI / 2f - 2f / n * placed * Mathf.PI;
                float x = -radius * Mathf.Cos(f);
                float y = -radius * Mathf.Sin(f);
                Transform tf = menuItem[i]?.ViewComponent?.UiTransform;
                if (tf != null)
                    tf.localPosition = new Vector3(x, y, 0f);
                placed++;
            }

            // XML can spawn the disc, but only the DLL knows this menu's radius.
            float plateRadius = radius + OptionOuterRadius * SelectedScale + PlatePad;
            int plate = Mathf.Max(MinPlateDiameter, Mathf.RoundToInt(2f * plateRadius));
            SetCircleSize(__instance.GetChildById("agfRadialHubRim")?.ViewComponent as XUiV_Sprite, plate);
            SetCircleSize(__instance.GetChildById("agfRadialHubBg")?.ViewComponent as XUiV_Sprite, plate - PlateRim * 2);
        }

        // Geometric disc: menu_empty + radial360 (not a scaled ui_game_filled_circle bitmap).
        private static void SetCircleSize(XUiV_Sprite sprite, int diameter)
        {
            if (sprite == null || diameter < 1)
                return;
            sprite.Fill = 1f;
            var size = new Vector2i(diameter, diameter);
            sprite.Width = diameter;
            sprite.Height = diameter;
            sprite.Size = size;
            Transform tf = sprite.UiTransform;
            if (tf != null && tf.localScale != Vector3.one)
                tf.localScale = Vector3.one;
            sprite.SetDirty();
        }
    }
}

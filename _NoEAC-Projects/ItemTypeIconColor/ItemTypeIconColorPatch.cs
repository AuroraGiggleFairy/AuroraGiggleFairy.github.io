using System;
using System.Xml;
using HarmonyLib;
using UnityEngine;

namespace ItemTypeIconColor
{
    public static class ItemTypeIconColorPatch
    {
        // Static dictionary to store color per item name (robust)
        public static readonly System.Collections.Generic.Dictionary<string, Color> ItemColors = new System.Collections.Generic.Dictionary<string, Color>();

        public static void Init()
        {
            var harmony = new Harmony("com.agfprojects.itemtypeiconcolor");
            harmony.PatchAll();
        }

        // Utility: Parse color in [HEX], R,G,B, or R,G,B,A formats
        public static Color ParseGameColor(string input)
        {
            if (string.IsNullOrEmpty(input)) return Color.white;
            input = input.Trim();
            // [RRGGBB] or [RRGGBBAA]
            if (input.StartsWith("[") && input.EndsWith("]"))
            {
                string hex = input.Substring(1, input.Length - 2);
                if (hex.Length == 6 || hex.Length == 8)
                {
                    byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    byte a = 255;
                    if (hex.Length == 8)
                        a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                    return new Color32(r, g, b, a);
                }
            }
            // R,G,B or R,G,B,A
            var parts = input.Split(',');
            if (parts.Length == 3 || parts.Length == 4)
            {
                if (byte.TryParse(parts[0], out byte r) && byte.TryParse(parts[1], out byte g) && byte.TryParse(parts[2], out byte b))
                {
                    byte a = 255;
                    if (parts.Length == 4 && byte.TryParse(parts[3], out byte a2))
                        a = a2;
                    return new Color32(r, g, b, a);
                }
            }
            // fallback: try vanilla parser (handles #RRGGBB and comma)
            try { return StringParsers.ParseHexColor(input); } catch { }
            return Color.white;
        }

        // Patch ItemClass.Init to support ItemTypeIconColor property
        [HarmonyPatch(typeof(ItemClass), "Init")]
        public class ItemClass_Init_Patch
        {
            static void Postfix(ItemClass __instance)
            {
                string propKey = "ItemTypeIconColor";
                Color color = Color.white;
                try
                {
                    var propsProp = typeof(ItemClass).GetField("Properties");
                    if (propsProp != null)
                    {
                        var props = propsProp.GetValue(__instance);
                        if (props != null)
                        {
                            var valuesProp = props.GetType().GetProperty("Values");
                            if (valuesProp != null)
                            {
                                var values = valuesProp.GetValue(props) as System.Collections.IDictionary;
                                if (values != null && values.Contains(propKey))
                                {
                                    var colorStr = values[propKey] as string;
                                    if (!string.IsNullOrEmpty(colorStr))
                                    {
                                        color = ItemTypeIconColorPatch.ParseGameColor(colorStr);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    color = Color.white;
                }
                // Store the color for this item name (even if white, for completeness)
                ItemTypeIconColorPatch.ItemColors[__instance.Name] = color;
            }
        }
    }
}

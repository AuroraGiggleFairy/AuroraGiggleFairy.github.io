using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;

namespace SkillCap
{
    internal static class SkillCapSettings
    {
        private static int? _xmlSkillPointCapLevel;

        public static bool HasXmlSkillPointCap
        {
            get
            {
                return _xmlSkillPointCapLevel.HasValue;
            }
        }

        public static int SkillPointCapLevel
        {
            get
            {
                return _xmlSkillPointCapLevel.GetValueOrDefault(0);
            }
        }

        public static void SetXmlSkillPointCapLevel(int? capLevel)
        {
            _xmlSkillPointCapLevel = capLevel;
        }
    }

    [HarmonyPatch(typeof(ProgressionFromXml), "parseLevelNode")]
    [HarmonyPatch(new Type[] { typeof(XElement) })]
    public static class SkillCap_ProgressionFromXml_parseLevelNode
    {
        private static void Postfix(XElement element)
        {
            if (element == null)
            {
                SkillCapSettings.SetXmlSkillPointCapLevel(null);
                return;
            }

            XAttribute attr = element.Attribute("skill_point_level_cap");
            if (attr == null)
            {
                SkillCapSettings.SetXmlSkillPointCapLevel(null);
                return;
            }

            if (int.TryParse(attr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) && parsed >= 0)
            {
                SkillCapSettings.SetXmlSkillPointCapLevel(parsed);
                return;
            }

            SkillCapSettings.SetXmlSkillPointCapLevel(null);
        }
    }

    [HarmonyPatch(typeof(Progression), "AddLevelExpRecursive")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(string), typeof(bool) })]
    public static class SkillCap_Progression_AddLevelExpRecursive
    {
        private static readonly FieldInfo ParentField = AccessTools.Field(typeof(Progression), "parent");

        private sealed class RecursionState
        {
            public int Depth;
            public int RootLevel;
            public int RootSkillPoints;
        }

        private static readonly Dictionary<Progression, RecursionState> StateByProgression = new Dictionary<Progression, RecursionState>();

        private struct SkillPointState
        {
            public bool IsOutermost;
        }

        private static void Prefix(Progression __instance, ref bool notifyUI, out SkillPointState __state)
        {
            if (!SkillCapSettings.HasXmlSkillPointCap)
            {
                __state = default;
                return;
            }

            if (!StateByProgression.TryGetValue(__instance, out RecursionState state))
            {
                state = new RecursionState();
                StateByProgression[__instance] = state;
            }

            state.Depth++;
            bool isOutermost = state.Depth == 1;
            if (isOutermost)
            {
                state.RootLevel = __instance.Level;
                state.RootSkillPoints = __instance.SkillPoints;
            }

            int capLevel = SkillCapSettings.SkillPointCapLevel;
            // Prevent misleading vanilla popup for recursive level-ups above cap.
            if (__instance.Level >= capLevel)
            {
                notifyUI = false;
            }

            __state = new SkillPointState { IsOutermost = isOutermost };
        }

        private static void Postfix(Progression __instance, SkillPointState __state)
        {
            if (!SkillCapSettings.HasXmlSkillPointCap)
            {
                return;
            }

            if (!StateByProgression.TryGetValue(__instance, out RecursionState state))
            {
                return;
            }

            if (__state.IsOutermost)
            {
                int newLevel = __instance.Level;
                int capLevel = SkillCapSettings.SkillPointCapLevel;
                if (newLevel > state.RootLevel)
                {
                    int shouldRemove = CalculateRemovedPoints(state.RootLevel, newLevel, capLevel);
                    if (shouldRemove > 0)
                    {
                        int actualGain = __instance.SkillPoints - state.RootSkillPoints;
                        if (actualGain > 0)
                        {
                            int toRemove = Math.Min(shouldRemove, actualGain);
                            __instance.SkillPoints = Math.Max(0, __instance.SkillPoints - toRemove);
                        }
                    }

                    // Above cap, vanilla recursive popups are suppressed. Show one corrected summary popup.
                    if (newLevel > capLevel)
                    {
                        EntityPlayerLocal localPlayer = ParentField?.GetValue(__instance) as EntityPlayerLocal;
                        if ((bool)localPlayer)
                        {
                            string text = string.Format(Localization.Get("ttLevelUp"), newLevel.ToString(), __instance.SkillPoints);
                            GameManager.ShowTooltip(localPlayer, text, string.Empty, "levelupplayer");
                        }
                    }
                }

                state.Depth--;
                if (state.Depth <= 0)
                {
                    StateByProgression.Remove(__instance);
                }
                return;
            }

            state.Depth--;
            if (state.Depth <= 0)
            {
                StateByProgression.Remove(__instance);
            }
        }

        private static int CalculateRemovedPoints(int oldLevel, int newLevel, int capLevel)
        {
            int total = 0;
            for (int level = oldLevel + 1; level <= newLevel; level++)
            {
                if (level <= capLevel)
                {
                    continue;
                }

                total += GetPointsForLevel(level);
            }

            return total;
        }

        private static int GetPointsForLevel(int level)
        {
            if (Progression.SkillPointMultiplier == 0f)
            {
                return Progression.SkillPointsPerLevel;
            }

            float scaled = (float)Progression.SkillPointsPerLevel * Mathf.Pow(Progression.SkillPointMultiplier, level);
            return (int)Math.Min(scaled, int.MaxValue);
        }
    }
}

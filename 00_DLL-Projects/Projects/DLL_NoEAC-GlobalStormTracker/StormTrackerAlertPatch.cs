// NOTE: This file is compiled as part of the StormTracker mod project. The resulting .dll is typically built using a Visual Studio solution or MSBuild, and the output .dll should be placed in the Mods/AGFProjects/StormTracker/ folder (or as specified by your mod loader setup).
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace StormTracker
{
        [HarmonyPatch(typeof(WeatherManager), "FrameUpdate")]
        public class StormTrackerAlertPatch
        {
            // Track last storm state per biome to prevent repeated alerts
            private static Dictionary<string, int> lastStormStatePerBiome = new Dictionary<string, int>();
            private static Dictionary<string, float> lastStormStartPerBiome = new Dictionary<string, float>();
            private static Dictionary<string, float> lastStormEndPerBiome = new Dictionary<string, float>();

            // Postfix: send a single alert per storm event per biome
            static void Postfix(WeatherManager __instance)
            {
                // Only run on server or host, not clients
                if (!GameManager.IsDedicatedServer && !ConnectionManager.Instance.IsServer) return;
                int dayNightLength = GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
                var worldTime = WeatherManager.worldTime;
                // Suppress alerts if world time is not yet synced (prevents false Day 1 alerts on login)
                if (worldTime < 24000f) return;
                foreach (var biomeWeather in __instance.biomeWeather)
                {
                    var biome = biomeWeather.biomeDefinition;
                    var level = biome.currentWeatherGroup.stormLevel;
                    string biomeKey = biome.m_sBiomeName;
                    int prevState = lastStormStatePerBiome.TryGetValue(biomeKey, out int s) ? s : 0;
                    int stormState = (level > 0) ? 1 : 0;
                    if (stormState == 1 && prevState == 0)
                    {
                        // New storm in this biome
                        string biomeLocKey = null;
                        switch (biome.m_BiomeType)
                        {
                            case BiomeDefinition.BiomeType.Desert:
                                biomeLocKey = "xuiDesert";
                                break;
                            case BiomeDefinition.BiomeType.PineForest:
                                biomeLocKey = "xuiPineForest";
                                break;
                            case BiomeDefinition.BiomeType.Snow:
                                biomeLocKey = "xuiSnow";
                                break;
                            case BiomeDefinition.BiomeType.Wasteland:
                                biomeLocKey = "xuiWasteland";
                                break;
                            case BiomeDefinition.BiomeType.burnt_forest:
                                biomeLocKey = "xuiBurntForest";
                                break;
                            default:
                                biomeLocKey = "xuiUnknown";
                                break;
                        }
                        string biomeName = Localization.Get(biomeLocKey);
                        float stormStart = biomeWeather.stormWorldTime;
                        float stormDuration = biomeWeather.stormDuration;
                        float stormEnd = stormStart + stormDuration;
                        int stormBuildDuration = 0;
                        try { stormBuildDuration = biome.WeatherGetDuration("stormbuild"); } catch { stormBuildDuration = 0; }
                        float level2StartTime = stormStart + stormBuildDuration;
                        string inGameTimeLevel2Start = GetInGameTimeStringAccurate(level2StartTime, dayNightLength);
                        string inGameTimeEnd = GetInGameTimeStringAccurate(stormEnd, dayNightLength);
                        int startDay = (int)(level2StartTime / 24000f) + 1;
                        int endDay = (int)(stormEnd / 24000f) + 1;
                        string alertMsg = string.Format(Localization.Get("stormTracker_stormWarning"), biomeName, startDay, inGameTimeLevel2Start, endDay, inGameTimeEnd);
                        GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, alertMsg, null, EMessageSender.Server);
                        lastStormStartPerBiome[biomeKey] = stormStart;
                        lastStormEndPerBiome[biomeKey] = stormEnd;
                    }
                    // Reset state to 0 if no storm is active in this biome
                    if (stormState == 0 && prevState != 0)
                    {
                        lastStormStartPerBiome[biomeKey] = 0f;
                        lastStormEndPerBiome[biomeKey] = 0f;
                    }
                    lastStormStatePerBiome[biomeKey] = stormState;
                }
            }

        // ...existing code...

        static string GetInGameTimeString(float wTime)
        {
            return GetInGameTimeStringAccurate(wTime, GamePrefs.GetInt(EnumGamePrefs.DayNightLength));
        }

        // More accurate in-game time calculation (must be a static method, not a static local function)
        static string GetInGameTimeStringAccurate(float wTime, int dayNightLength)
        {
            // worldTime is in game seconds since day 0
            // 1 in-game day = dayNightLength real minutes = 1440 in-game minutes = 24 in-game hours
            float secondsPerInGameDay = 24000f; // 24h * 60m * 60s
            float inGameSeconds = wTime % secondsPerInGameDay;
            int inGameHour = (int)(inGameSeconds / 1000f); // 1 hour = 1000 seconds
            int inGameMinute = (int)((inGameSeconds % 1000f) / (1000f / 60f));
            return string.Format("{0:D2}:{1:D2}", inGameHour, inGameMinute);
        }
    }
}

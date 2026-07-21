using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace FlightLevelAssist
{
    internal static class FlightLevelAssistConfig
    {
        private const string ConfigRelativePath = "Config\\FlightLevelAssistConfig.txt";
        private const string HotkeySettingName = "LevelAssistHotkey";
        private const string ControllerActivationSettingName = "ControllerActivationAction";

        public enum ControllerActivationAction
        {
            None,
            ToggleTurnMode,
            HonkHorn,
            ToggleFlashlight,
            Scoreboard,
            Inventory,
            Activate
        }

        public static KeyCode LevelAssistHotkey { get; private set; } = KeyCode.Z;
        public static ControllerActivationAction ControllerActivation { get; private set; } = ControllerActivationAction.None;

        public static void Load()
        {
            try
            {
                // Config is optional; default hotkey always works when file is missing or invalid.
                LevelAssistHotkey = KeyCode.Z;
                ControllerActivation = ControllerActivationAction.None;

                string dllPath = Assembly.GetExecutingAssembly().Location;
                string modDir = Path.GetDirectoryName(dllPath) ?? string.Empty;
                string configPath = Path.Combine(modDir, ConfigRelativePath);

                if (!File.Exists(configPath))
                {
                    Debug.Log("[FlightLevelAssist] Config not found (optional), using defaults.");
                    Debug.Log("[FlightLevelAssist] Active hotkey: " + LevelAssistHotkey);
                    return;
                }

                string[] lines = File.ReadAllLines(configPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i]?.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int equalsIndex = line.IndexOf('=');
                    if (equalsIndex <= 0)
                    {
                        continue;
                    }

                    string key = line.Substring(0, equalsIndex).Trim();
                    string value = line.Substring(equalsIndex + 1).Trim();

                    if (key.Equals(HotkeySettingName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Enum.TryParse(value, true, out KeyCode parsedKey))
                        {
                            LevelAssistHotkey = parsedKey;
                            Debug.Log("[FlightLevelAssist] Loaded hotkey: " + LevelAssistHotkey);
                        }
                        else
                        {
                            Debug.LogWarning("[FlightLevelAssist] Invalid hotkey '" + value + "', using default " + LevelAssistHotkey + ".");
                        }
                    }
                    else if (key.Equals(ControllerActivationSettingName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Enum.TryParse(value, true, out ControllerActivationAction parsedAction))
                        {
                            ControllerActivation = parsedAction;
                            Debug.Log("[FlightLevelAssist] Loaded controller activation action: " + ControllerActivation);
                        }
                        else
                        {
                            Debug.LogWarning("[FlightLevelAssist] Invalid controller action '" + value + "', using default " + ControllerActivation + ".");
                        }
                    }
                }

                Debug.Log("[FlightLevelAssist] Active hotkey: " + LevelAssistHotkey);
                Debug.Log("[FlightLevelAssist] Active controller action: " + ControllerActivation);
            }
            catch (Exception ex)
            {
                Debug.LogError("[FlightLevelAssist] Failed to load config: " + ex);
            }
        }
    }
}
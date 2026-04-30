using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

public enum ScreamerAlertMode
{
    Off = 0,
    On = 1,
    OnWithNumbers = 2
}

public static class ScreamerAlertModeSettings
{
    private static readonly object Sync = new object();
    private static readonly Dictionary<string, ScreamerAlertMode> ModesByPlayer = new Dictionary<string, ScreamerAlertMode>(StringComparer.OrdinalIgnoreCase);
    private static readonly Regex CountSuffixRegex = new Regex(@"\s*\[FFFFFF\]\(\d+\)\[-\]\s*$", RegexOptions.Compiled);
    private static bool _loaded;

    public static ScreamerAlertMode GetModeForLocalPlayer(ScreamerAlertMode defaultMode)
    {
        lock (Sync)
        {
            EnsureLoaded();
            string key = GetLocalPlayerKey();
            if (string.IsNullOrEmpty(key))
            {
                return defaultMode;
            }

            if (ModesByPlayer.TryGetValue(key, out ScreamerAlertMode mode))
            {
                return mode;
            }

            return defaultMode;
        }
    }

    public static ScreamerAlertMode GetModeForEntityId(int entityId, ScreamerAlertMode defaultMode)
    {
        lock (Sync)
        {
            EnsureLoaded();
            string key = GetPlayerKeyFromEntityId(entityId);
            if (string.IsNullOrEmpty(key))
            {
                return defaultMode;
            }

            if (ModesByPlayer.TryGetValue(key, out ScreamerAlertMode mode))
            {
                return mode;
            }

            return defaultMode;
        }
    }

    public static ScreamerAlertMode GetModeForPlayerKey(string playerKey, ScreamerAlertMode defaultMode)
    {
        lock (Sync)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(playerKey))
            {
                return defaultMode;
            }

            if (ModesByPlayer.TryGetValue(playerKey, out ScreamerAlertMode mode))
            {
                return mode;
            }

            return defaultMode;
        }
    }

    public static bool SetModeForEntityId(int entityId, ScreamerAlertMode mode)
    {
        lock (Sync)
        {
            EnsureLoaded();
            string key = GetPlayerKeyFromEntityId(entityId);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            ModesByPlayer[key] = mode;
            Save_NoLock();
            return true;
        }
    }

    public static bool SetModeForLocalPlayer(ScreamerAlertMode mode)
    {
        lock (Sync)
        {
            EnsureLoaded();
            string key = GetLocalPlayerKey();
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            ModesByPlayer[key] = mode;
            Save_NoLock();
            return true;
        }
    }

    public static ScreamerAlertMode CycleModeForLocalPlayer(ScreamerAlertMode defaultMode)
    {
        lock (Sync)
        {
            ScreamerAlertMode current = GetModeForLocalPlayer(defaultMode);
            ScreamerAlertMode next = NextMode(current);
            if (!SetModeForLocalPlayer(next))
            {
                return current;
            }

            return next;
        }
    }

    public static string GetModeLabel(ScreamerAlertMode mode)
    {
        switch (mode)
        {
            case ScreamerAlertMode.Off:
                return "Off";
            case ScreamerAlertMode.On:
                return "On";
            case ScreamerAlertMode.OnWithNumbers:
                return "On + #";
            default:
                return mode.ToString();
        }
    }

    public static string GetLabel(ScreamerAlertMode mode)
    {
        return GetModeLabel(mode);
    }

    public static ScreamerAlertMode Cycle(ScreamerAlertMode current)
    {
        return NextMode(current);
    }

    public static string StripNumberSuffix(string incomingText)
    {
        if (string.IsNullOrEmpty(incomingText))
        {
            return incomingText;
        }

        return CountSuffixRegex.Replace(incomingText, string.Empty);
    }

    public static string GetDisplayTextForMode(string incomingText, ScreamerAlertMode mode)
    {
        if (string.IsNullOrEmpty(incomingText))
        {
            return incomingText;
        }

        switch (mode)
        {
            case ScreamerAlertMode.Off:
                return string.Empty;
            case ScreamerAlertMode.On:
                return CountSuffixRegex.Replace(incomingText, string.Empty);
            case ScreamerAlertMode.OnWithNumbers:
            default:
                return incomingText;
        }
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        string filePath = GetFilePath();
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(new[] { '\t' }, 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                string key = parts[0].Trim();
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (TryParseMode(parts[1], out ScreamerAlertMode parsedMode))
                {
                    ModesByPlayer[key] = parsedMode;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ScreamerAlert] Failed to load mode settings: " + ex.Message);
        }
    }

    private static void Save_NoLock()
    {
        try
        {
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            List<string> lines = new List<string>(ModesByPlayer.Count);
            foreach (KeyValuePair<string, ScreamerAlertMode> kv in ModesByPlayer)
            {
                lines.Add(kv.Key + "\t" + (int)kv.Value);
            }

            File.WriteAllLines(filePath, lines.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ScreamerAlert] Failed to save mode settings: " + ex.Message);
        }
    }

    private static string GetFilePath()
    {
        try
        {
            string saveGameDir = GameIO.GetSaveGameDir();
            if (string.IsNullOrEmpty(saveGameDir))
            {
                return null;
            }

            return Path.Combine(saveGameDir, "ScreamerAlert.playerModes.tsv");
        }
        catch
        {
            return null;
        }
    }

    private static string GetLocalPlayerKey()
    {
        try
        {
            if (GameManager.Instance == null || GameManager.Instance.World == null)
            {
                return null;
            }

            int localEntityId = -1;
            if (GameManager.Instance.World.GetPrimaryPlayer() != null)
            {
                localEntityId = GameManager.Instance.World.GetPrimaryPlayer().entityId;
            }

            if (localEntityId < 0)
            {
                return null;
            }

            return GetPlayerKeyFromEntityId(localEntityId);
        }
        catch
        {
            return null;
        }
    }

    public static string GetPlayerKeyFromEntityId(int entityId)
    {
        try
        {
            if (GameManager.Instance == null || GameManager.Instance.World == null)
            {
                return null;
            }

            EntityPlayer player = GameManager.Instance.World.GetEntity(entityId) as EntityPlayer;
            if (player == null)
            {
                return null;
            }

            object pui = GetMemberValue(player, "PersistentPlayerInfo")
                ?? GetMemberValue(player, "persistentPlayerInfo")
                ?? GetMemberValue(player, "persistentInfo");

            if (pui != null)
            {
                object keyObj = GetMemberValue(pui, "PlayerId")
                    ?? GetMemberValue(pui, "playerId")
                    ?? GetMemberValue(pui, "CrossplatformId")
                    ?? GetMemberValue(pui, "CrossplatformUserIdentifier");
                if (keyObj != null)
                {
                    string key = keyObj.ToString();
                    if (!string.IsNullOrEmpty(key))
                    {
                        return key;
                    }
                }
            }

            object platformIdObj = GetMemberValue(player, "PlatformId")
                ?? GetMemberValue(player, "platformId")
                ?? GetMemberValue(player, "CrossplatformId")
                ?? GetMemberValue(player, "CrossplatformUserIdentifier");
            if (platformIdObj != null)
            {
                string key = platformIdObj.ToString();
                if (!string.IsNullOrEmpty(key))
                {
                    return key;
                }
            }

            return "entity:" + entityId;
        }
        catch
        {
            return null;
        }
    }

    private static ScreamerAlertMode NextMode(ScreamerAlertMode current)
    {
        switch (current)
        {
            case ScreamerAlertMode.Off:
                return ScreamerAlertMode.On;
            case ScreamerAlertMode.On:
                return ScreamerAlertMode.OnWithNumbers;
            case ScreamerAlertMode.OnWithNumbers:
            default:
                return ScreamerAlertMode.Off;
        }
    }

    private static bool TryParseMode(string value, out ScreamerAlertMode mode)
    {
        mode = ScreamerAlertMode.OnWithNumbers;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        string trimmed = value.Trim();
        if (trimmed.Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            mode = ScreamerAlertMode.Off;
            return true;
        }

        if (trimmed.Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            mode = ScreamerAlertMode.On;
            return true;
        }

        if (trimmed.Equals("numbers", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("num", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("count", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("onwithnumbers", StringComparison.OrdinalIgnoreCase))
        {
            mode = ScreamerAlertMode.OnWithNumbers;
            return true;
        }

        if (int.TryParse(trimmed, out int numeric))
        {
            switch (numeric)
            {
                case 0:
                    mode = ScreamerAlertMode.Off;
                    return true;
                case 1:
                    mode = ScreamerAlertMode.On;
                    return true;
                case 2:
                    mode = ScreamerAlertMode.OnWithNumbers;
                    return true;
            }
        }

        return false;
    }

    public static ScreamerAlertMode ParseOrDefault(string value, ScreamerAlertMode fallback)
    {
        if (TryParseMode(value, out ScreamerAlertMode parsed))
        {
            return parsed;
        }

        return fallback;
    }

    public static ScreamerAlertMode ParseCycleOrdinalOrDefault(string value, ScreamerAlertMode fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        string trimmed = value.Trim();
        if (!int.TryParse(trimmed, out int numeric))
        {
            return fallback;
        }

        switch (numeric)
        {
            case 0:
                return ScreamerAlertMode.Off;
            case 1:
                return ScreamerAlertMode.On;
            case 2:
                return ScreamerAlertMode.OnWithNumbers;
            default:
                return ScreamerAlertMode.OnWithNumbers;
        }
    }

    private static object GetMemberValue(object instance, string memberName)
    {
        if (instance == null || string.IsNullOrEmpty(memberName))
        {
            return null;
        }

        Type type = instance.GetType();
        PropertyInfo property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null)
        {
            return property.GetValue(instance, null);
        }

        FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            return field.GetValue(instance);
        }

        return null;
    }
}

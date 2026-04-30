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
            SaveUnsafe();
            return true;
        }
    }

    public static bool SetModeForPlayerKey(string playerKey, ScreamerAlertMode mode)
    {
        lock (Sync)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(playerKey))
            {
                return false;
            }

            ModesByPlayer[playerKey] = mode;
            SaveUnsafe();
            return true;
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
                return "On + #";
        }
    }

    public static ScreamerAlertMode Cycle(ScreamerAlertMode mode)
    {
        switch (mode)
        {
            case ScreamerAlertMode.Off:
                return ScreamerAlertMode.On;
            case ScreamerAlertMode.On:
                return ScreamerAlertMode.OnWithNumbers;
            case ScreamerAlertMode.OnWithNumbers:
                return ScreamerAlertMode.Off;
            default:
                return ScreamerAlertMode.OnWithNumbers;
        }
    }

    public static string StripNumberSuffix(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return CountSuffixRegex.Replace(text, string.Empty);
    }

    public static string GetPlayerKeyFromEntityId(int entityId)
    {
        if (entityId < 0)
        {
            return null;
        }

        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        ClientInfo clientInfo = manager?.Clients?.ForEntityId(entityId);
        string byClient = clientInfo?.InternalId?.CombinedString;
        if (!string.IsNullOrEmpty(byClient))
        {
            return byClient;
        }

        World world = GameManager.Instance?.World;
        EntityPlayer entityPlayer = world?.GetEntity(entityId) as EntityPlayer;
        if (entityPlayer != null)
        {
            object platformId = GetMemberValue(entityPlayer, "PlatformId") ?? GetMemberValue(entityPlayer, "platformId");
            string combined = Convert.ToString(GetMemberValue(platformId, "CombinedString") ?? GetMemberValue(platformId, "combinedString"));
            if (!string.IsNullOrEmpty(combined))
            {
                return combined;
            }
        }

        return "entity_" + entityId;
    }

    private static string GetLocalPlayerKey()
    {
        EntityPlayer player = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (player == null)
        {
            return "local";
        }

        return GetPlayerKeyFromEntityId(player.entityId) ?? "local";
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        string path = GetSettingsPath();
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw?.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] parts = line.Split('\t');
                if (parts.Length != 2)
                {
                    continue;
                }

                if (!int.TryParse(parts[1], out int modeInt))
                {
                    continue;
                }

                ModesByPlayer[parts[0]] = CoerceMode(modeInt);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ScreamerAlert] Failed reading settings file: " + ex.Message);
        }
    }

    private static void SaveUnsafe()
    {
        string path = GetSettingsPath();
        try
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            List<string> lines = new List<string>
            {
                "# ScreamerAlert per-player settings",
                "# Format: playerKey<TAB>modeInt"
            };

            foreach (KeyValuePair<string, ScreamerAlertMode> kvp in ModesByPlayer)
            {
                lines.Add(kvp.Key + "\t" + (int)kvp.Value);
            }

            File.WriteAllLines(path, lines.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ScreamerAlert] Failed writing settings file: " + ex.Message);
        }
    }

    private static string GetSettingsPath()
    {
        string dllPath = Assembly.GetExecutingAssembly().Location;
        string modDir = Path.GetDirectoryName(dllPath) ?? string.Empty;
        return Path.Combine(modDir, "Config", "screamer_alert_settings.tsv");
    }

    private static ScreamerAlertMode CoerceMode(int value)
    {
        switch (value)
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
            SaveUnsafe();
            return true;
        }
    }

    public static bool SetModeForPlayerKey(string playerKey, ScreamerAlertMode mode)
    {
        lock (Sync)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(playerKey))
            {
                return false;
            }

            ModesByPlayer[playerKey] = mode;
            SaveUnsafe();
            return true;
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
                return "On + #";
        }
    }

    public static ScreamerAlertMode Cycle(ScreamerAlertMode mode)
    {
        switch (mode)
        {
            case ScreamerAlertMode.Off:
                return ScreamerAlertMode.On;
            case ScreamerAlertMode.On:
                return ScreamerAlertMode.OnWithNumbers;
            case ScreamerAlertMode.OnWithNumbers:
                return ScreamerAlertMode.Off;
            default:
                return ScreamerAlertMode.OnWithNumbers;
        }
    }

    public static string StripNumberSuffix(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return CountSuffixRegex.Replace(text, string.Empty);
    }

    public static string GetPlayerKeyFromEntityId(int entityId)
    {
        if (entityId < 0)
        {
            return null;
        }

        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        ClientInfo clientInfo = manager?.Clients?.ForEntityId(entityId);
        string byClient = clientInfo?.InternalId?.CombinedString;
        if (!string.IsNullOrEmpty(byClient))
        {
            return byClient;
        }

        World world = GameManager.Instance?.World;
        EntityPlayer entityPlayer = world?.GetEntity(entityId) as EntityPlayer;
        if (entityPlayer != null)
        {
            object platformId = GetMemberValue(entityPlayer, "PlatformId") ?? GetMemberValue(entityPlayer, "platformId");
            string combined = Convert.ToString(GetMemberValue(platformId, "CombinedString") ?? GetMemberValue(platformId, "combinedString"));
            if (!string.IsNullOrEmpty(combined))
            {
                return combined;
            }
        }

        return "entity_" + entityId;
    }

    private static string GetLocalPlayerKey()
    {
        EntityPlayer player = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (player == null)
        {
            return "local";
        }

        return GetPlayerKeyFromEntityId(player.entityId) ?? "local";
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        string path = GetSettingsPath();
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw?.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] parts = line.Split('\t');
                if (parts.Length != 2)
                {
                    continue;
                }

                if (!int.TryParse(parts[1], out int modeInt))
                {
                    continue;
                }

                ModesByPlayer[parts[0]] = CoerceMode(modeInt);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ScreamerAlert] Failed reading settings file: " + ex.Message);
        }
    }

    private static void SaveUnsafe()
    {
        string path = GetSettingsPath();
        try
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            List<string> lines = new List<string>
            {
                "# ScreamerAlert per-player settings",
                "# Format: playerKey<TAB>modeInt"
            };

            foreach (KeyValuePair<string, ScreamerAlertMode> kvp in ModesByPlayer)
            {
                lines.Add(kvp.Key + "\t" + (int)kvp.Value);
            }

            File.WriteAllLines(path, lines.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ScreamerAlert] Failed writing settings file: " + ex.Message);
        }
    }

    private static string GetSettingsPath()
    {
        string dllPath = Assembly.GetExecutingAssembly().Location;
        string modDir = Path.GetDirectoryName(dllPath) ?? string.Empty;
        return Path.Combine(modDir, "Config", "screamer_alert_settings.tsv");
    }

    private static ScreamerAlertMode CoerceMode(int value)
    {
        switch (value)
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

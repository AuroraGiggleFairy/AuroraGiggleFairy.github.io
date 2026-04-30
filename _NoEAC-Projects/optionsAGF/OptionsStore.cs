using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public static class OptionsStore
{
    private static readonly object Sync = new object();
    private static readonly Dictionary<string, Dictionary<string, OptionMode>> Data = new Dictionary<string, Dictionary<string, OptionMode>>(StringComparer.OrdinalIgnoreCase);
    private static bool isLoaded;
    private static int cachedEntityId = int.MinValue;
    private static string cachedPlayerKey = "local";

    public static OptionMode GetMode(string optionKey, OptionMode defaultMode)
    {
        lock (Sync)
        {
            EnsureLoaded();
            string playerKey = GetPlayerKey();
            if (!Data.TryGetValue(playerKey, out var options))
            {
                options = new Dictionary<string, OptionMode>(StringComparer.OrdinalIgnoreCase);
                Data[playerKey] = options;
            }

            if (options.TryGetValue(optionKey, out var mode))
            {
                return mode;
            }

            return defaultMode;
        }
    }

    public static void SetMode(string optionKey, OptionMode mode)
    {
        lock (Sync)
        {
            EnsureLoaded();
            string playerKey = GetPlayerKey();
            if (!Data.TryGetValue(playerKey, out var options))
            {
                options = new Dictionary<string, OptionMode>(StringComparer.OrdinalIgnoreCase);
                Data[playerKey] = options;
            }

            options[optionKey] = mode;
            SaveUnsafe();
        }
    }

    private static void EnsureLoaded()
    {
        if (isLoaded)
        {
            return;
        }

        isLoaded = true;
        string path = GetSettingsPath();
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            foreach (string rawLine in File.ReadAllLines(path))
            {
                string line = rawLine?.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] parts = line.Split('\t');
                if (parts.Length != 3)
                {
                    continue;
                }

                string playerKey = parts[0];
                string optionKey = parts[1];
                if (!int.TryParse(parts[2], out int modeInt))
                {
                    continue;
                }

                OptionMode mode = CoerceMode(modeInt);
                if (!Data.TryGetValue(playerKey, out var options))
                {
                    options = new Dictionary<string, OptionMode>(StringComparer.OrdinalIgnoreCase);
                    Data[playerKey] = options;
                }

                options[optionKey] = mode;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[optionsAGF] Failed reading settings file: " + ex);
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

            var lines = new List<string>
            {
                "# optionsAGF per-player settings",
                "# Format: playerKey<TAB>optionKey<TAB>modeInt"
            };

            foreach (var playerPair in Data.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            {
                foreach (var optionPair in playerPair.Value.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
                {
                    lines.Add(playerPair.Key + "\t" + optionPair.Key + "\t" + (int)optionPair.Value);
                }
            }

            File.WriteAllLines(path, lines.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine("[optionsAGF] Failed writing settings file: " + ex);
        }
    }

    private static string GetSettingsPath()
    {
        string dllPath = Assembly.GetExecutingAssembly().Location;
        string modDir = Path.GetDirectoryName(dllPath) ?? string.Empty;
        return Path.Combine(modDir, "Config", "options_settings.tsv");
    }

    private static OptionMode CoerceMode(int value)
    {
        switch (value)
        {
            case 0:
                return OptionMode.Off;
            case 1:
                return OptionMode.On;
            case 2:
                return OptionMode.OnWithNumbers;
            default:
                return OptionMode.OnWithNumbers;
        }
    }

    private static string GetPlayerKey()
    {
        try
        {
            EntityPlayer player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player == null)
            {
                cachedEntityId = int.MinValue;
                cachedPlayerKey = "local";
                return "local";
            }

            if (player.entityId == cachedEntityId && !string.IsNullOrEmpty(cachedPlayerKey))
            {
                return cachedPlayerKey;
            }

            object platformId = GetMemberValue(player, "PlatformId") ?? GetMemberValue(player, "platformId");
            if (platformId != null)
            {
                string combined = Convert.ToString(GetMemberValue(platformId, "CombinedString") ?? GetMemberValue(platformId, "combinedString"));
                if (!string.IsNullOrEmpty(combined))
                {
                    cachedEntityId = player.entityId;
                    cachedPlayerKey = combined;
                    return combined;
                }

                string idString = platformId.ToString();
                if (!string.IsNullOrEmpty(idString))
                {
                    cachedEntityId = player.entityId;
                    cachedPlayerKey = idString;
                    return idString;
                }
            }

            cachedEntityId = player.entityId;
            cachedPlayerKey = "entity_" + player.entityId;
            return cachedPlayerKey;
        }
        catch
        {
            return "local";
        }
    }

    private static object GetMemberValue(object instance, string memberName)
    {
        if (instance == null || string.IsNullOrEmpty(memberName))
        {
            return null;
        }

        Type type = instance.GetType();
        PropertyInfo prop = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null)
        {
            try
            {
                return prop.GetValue(instance, null);
            }
            catch
            {
                return null;
            }
        }

        FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            try
            {
                return field.GetValue(instance);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}

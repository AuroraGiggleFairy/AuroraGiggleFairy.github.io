using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VisualEntityTrackerAddon
{
    public enum VisualEntityTrackerMode
    {
        Off = 0,
        On = 1
    }

    public static class VisualEntityTrackerModeSettings
    {
        private static readonly object Sync = new object();
        private static readonly Dictionary<string, VisualEntityTrackerMode> ModesByPlayer = new Dictionary<string, VisualEntityTrackerMode>(StringComparer.OrdinalIgnoreCase);
        private const string DefaultModeStorageKey = "__default__";
        private static VisualEntityTrackerMode serverDefaultMode = VisualEntityTrackerMode.On;
        private static bool loaded;
        private static DateTime lastLoadedWriteUtc = DateTime.MinValue;

        public static VisualEntityTrackerMode GetServerDefaultMode()
        {
            lock (Sync)
            {
                EnsureLoaded();
                return serverDefaultMode;
            }
        }

        public static bool SetServerDefaultMode(VisualEntityTrackerMode mode)
        {
            lock (Sync)
            {
                EnsureLoaded();
                serverDefaultMode = mode;
                SaveNoLock();
                return true;
            }
        }

        public static VisualEntityTrackerMode GetModeForEntityId(int entityId, VisualEntityTrackerMode defaultMode)
        {
            lock (Sync)
            {
                EnsureLoaded();
                string key = GetPlayerKeyFromEntityId(entityId);
                if (string.IsNullOrEmpty(key))
                {
                    return defaultMode;
                }

                if (ModesByPlayer.TryGetValue(key, out VisualEntityTrackerMode mode))
                {
                    return mode;
                }

                return defaultMode;
            }
        }

        public static bool SetModeForEntityId(int entityId, VisualEntityTrackerMode mode)
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
                SaveNoLock();
                return true;
            }
        }

        public static string GetModeToken(VisualEntityTrackerMode mode)
        {
            return mode == VisualEntityTrackerMode.Off ? "OFF" : "ON";
        }

        public static bool TryParseCommandMode(string value, out VisualEntityTrackerMode mode)
        {
            mode = VisualEntityTrackerMode.On;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "off":
                    mode = VisualEntityTrackerMode.Off;
                    return true;
                case "on":
                    mode = VisualEntityTrackerMode.On;
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryParseMode(string value, out VisualEntityTrackerMode mode)
        {
            if (TryParseCommandMode(value, out mode))
            {
                return true;
            }

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "0":
                    mode = VisualEntityTrackerMode.Off;
                    return true;
                case "1":
                    mode = VisualEntityTrackerMode.On;
                    return true;
                default:
                    return false;
            }
        }

        private static void EnsureLoaded()
        {
            string filePath = GetFilePath();
            DateTime writeUtc = DateTime.MinValue;
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    writeUtc = File.GetLastWriteTimeUtc(filePath);
                }
                catch
                {
                    writeUtc = DateTime.MinValue;
                }
            }

            if (loaded && writeUtc <= lastLoadedWriteUtc)
            {
                return;
            }

            loaded = true;
            lastLoadedWriteUtc = writeUtc;
            serverDefaultMode = VisualEntityTrackerMode.On;
            ModesByPlayer.Clear();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
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
                    if (string.IsNullOrEmpty(key) || !TryParseMode(parts[1], out VisualEntityTrackerMode parsedMode))
                    {
                        continue;
                    }

                    if (string.Equals(key, DefaultModeStorageKey, StringComparison.OrdinalIgnoreCase))
                    {
                        serverDefaultMode = parsedMode;
                    }
                    else
                    {
                        ModesByPlayer[key] = parsedMode;
                    }
                }

                lastLoadedWriteUtc = writeUtc;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[VisualEntityTracker] Failed to load mode settings: " + ex.Message);
            }
        }

        private static void SaveNoLock()
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

                List<string> lines = new List<string>(ModesByPlayer.Count + 1)
                {
                    DefaultModeStorageKey + "\t" + (int)serverDefaultMode
                };

                foreach (KeyValuePair<string, VisualEntityTrackerMode> pair in ModesByPlayer)
                {
                    if (string.Equals(pair.Key, DefaultModeStorageKey, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    lines.Add(pair.Key + "\t" + (int)pair.Value);
                }

                File.WriteAllLines(filePath, lines.ToArray());
                try
                {
                    lastLoadedWriteUtc = File.GetLastWriteTimeUtc(filePath);
                }
                catch
                {
                    lastLoadedWriteUtc = DateTime.MinValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[VisualEntityTracker] Failed to save mode settings: " + ex.Message);
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

                return Path.Combine(saveGameDir, "VisualEntityTrackerAddon.playerModes.tsv");
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
}

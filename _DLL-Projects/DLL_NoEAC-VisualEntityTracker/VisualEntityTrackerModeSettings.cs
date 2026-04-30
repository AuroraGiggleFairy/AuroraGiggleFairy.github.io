using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NoEACVisualEntityTracker
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
        private static bool loaded;

        public static VisualEntityTrackerMode GetModeForLocalPlayer(VisualEntityTrackerMode defaultMode)
        {
            lock (Sync)
            {
                EnsureLoaded();
                string key = GetLocalPlayerKey();
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

        public static bool SetModeForLocalPlayer(VisualEntityTrackerMode mode)
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
                SaveNoLock();
                return true;
            }
        }

        public static string GetModeLabel(VisualEntityTrackerMode mode)
        {
            return mode == VisualEntityTrackerMode.Off ? "Off" : "On";
        }

        public static bool TryParseMode(string value, out VisualEntityTrackerMode mode)
        {
            mode = VisualEntityTrackerMode.On;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string normalized = value.Trim().ToLowerInvariant();
            if (normalized == "0" || normalized == "off")
            {
                mode = VisualEntityTrackerMode.Off;
                return true;
            }

            if (normalized == "1" || normalized == "on")
            {
                mode = VisualEntityTrackerMode.On;
                return true;
            }

            return false;
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

                object persistentInfo = GetMemberValue(player, "PersistentPlayerInfo")
                    ?? GetMemberValue(player, "persistentPlayerInfo")
                    ?? GetMemberValue(player, "persistentInfo");

                if (persistentInfo != null)
                {
                    object keyObj = GetMemberValue(persistentInfo, "PlayerId")
                        ?? GetMemberValue(persistentInfo, "playerId")
                        ?? GetMemberValue(persistentInfo, "CrossplatformId")
                        ?? GetMemberValue(persistentInfo, "CrossplatformUserIdentifier");
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

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;
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

                    if (!TryParseMode(parts[1], out VisualEntityTrackerMode mode))
                    {
                        continue;
                    }

                    ModesByPlayer[key] = mode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[NoEACVisualEntityTracker] Failed to load mode settings: " + ex.Message);
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

                List<string> lines = new List<string>(ModesByPlayer.Count);
                foreach (KeyValuePair<string, VisualEntityTrackerMode> pair in ModesByPlayer)
                {
                    lines.Add(pair.Key + "\t" + (int)pair.Value);
                }

                File.WriteAllLines(filePath, lines.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine("[NoEACVisualEntityTracker] Failed to save mode settings: " + ex.Message);
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

                return Path.Combine(saveGameDir, "VisualEntityTracker.playerModes.tsv");
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

                EntityPlayer player = GameManager.Instance.World.GetPrimaryPlayer();
                if (player == null)
                {
                    return null;
                }

                return GetPlayerKeyFromEntityId(player.entityId);
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

public static class ForceDisableMonitor
{
    private const string ModFolderPath = "Mods/AntiCM";
    private const string ConfigFilePath = ModFolderPath + "/AntiCMConfig.txt";
    private const string LogFilePath = ModFolderPath + "/CMDebugLog.txt";

    private static readonly HashSet<string> AllowedSteamIds = new HashSet<string>();
    private static string _discordWebhookUrl = string.Empty;
    private static bool _kickOnDetection = true;
    private static bool _enableDiscordAlerts;
    private static float _scanIntervalSeconds = 0.25f;
    private static readonly Dictionary<int, float> LastActionTimeByEntityId = new Dictionary<int, float>();
    private static float _lastScanTime;
    private static float _lastConfigLoadTime;
    private static bool _initialized;

    // Cached reflection members to avoid repeated lookup cost each scan.
    private static FieldInfo _creativeField;
    private static FieldInfo _debugField;
    private static FieldInfo _clientInfoField;
    private static FieldInfo _lastCommandsField;
    private static MethodInfo _kickMethod;
    private static FieldInfo _playerIdField;

    private static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        Type playerType = typeof(EntityPlayer);
        _creativeField = playerType.GetField("bCreativeMode", BindingFlags.NonPublic | BindingFlags.Instance);
        _debugField = playerType.GetField("DebugMode", BindingFlags.NonPublic | BindingFlags.Instance);
        _clientInfoField = playerType.GetField("m_clientInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        _lastCommandsField = playerType.GetField("m_lastCommands", BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (MethodInfo method in typeof(GameManager).GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (method.Name != "KickPlayer")
            {
                continue;
            }

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 2 && parameters[1].ParameterType == typeof(string))
            {
                _kickMethod = method;
                break;
            }
        }

        EnsureConfigExists();
        LoadConfig();
        Debug.Log("[AntiCM] Initialized and monitoring players for CM/DM usage.");

        _initialized = true;
    }

    public static void Tick()
    {
        Initialize();

        if (Time.realtimeSinceStartup - _lastConfigLoadTime >= 10f)
        {
            LoadConfig();
        }

        if (Time.realtimeSinceStartup - _lastScanTime < _scanIntervalSeconds)
        {
            return;
        }
        _lastScanTime = Time.realtimeSinceStartup;

        if (GameManager.Instance?.World?.Players == null)
        {
            return;
        }

        foreach (EntityPlayer player in GameManager.Instance.World.Players.list)
        {
            if (player == null)
            {
                continue;
            }

            object clientInfo = _clientInfoField?.GetValue(player);
            string steamId = TryResolveSteamId(clientInfo);
            string playerName = player?.entityName ?? "UnknownPlayer";

            if (AllowedSteamIds.Contains(steamId))
                continue;

            bool triggered = false;
            string triggerReason = "";

            // Check Creative Mode
            if (_creativeField != null && (bool)_creativeField.GetValue(player))
            {
                _creativeField.SetValue(player, false);
                triggered = true;
                triggerReason = "Creative Mode";
            }

            // Check Debug Mode
            if (_debugField != null && (bool)_debugField.GetValue(player))
            {
                _debugField.SetValue(player, false);
                triggered = true;
                triggerReason = string.IsNullOrEmpty(triggerReason) ? "Debug Mode" : triggerReason + ", Debug Mode";
            }

            // Check CM/DM commands
            object commandBuffer = _lastCommandsField?.GetValue(player);
            if (commandBuffer is List<string> commands)
            {
                foreach (string cmd in commands)
                {
                    string c = (cmd ?? string.Empty).ToLowerInvariant().Trim();
                    if (c == "cm" || c.StartsWith("cm ") || c.StartsWith("/cm") ||
                        c == "dm" || c.StartsWith("dm ") || c.StartsWith("/dm") ||
                        c.Contains("debug"))
                    {
                        triggered = true;
                        triggerReason = string.IsNullOrEmpty(triggerReason) ? $"Command: {c}" : triggerReason + $", Command: {c}";
                        break; // Only trigger once per frame
                    }
                }
            }

            if (triggered)
            {
                if (!CanActNow(player.entityId))
                {
                    continue;
                }

                string logLine = $"[CM/DEBUG BLOCK] Player '{playerName}' SteamID: {steamId} triggered ({triggerReason}) at {DateTime.Now}";
                Debug.Log(logLine);

                try
                {
                    Directory.CreateDirectory(ModFolderPath);
                    File.AppendAllText(LogFilePath, logLine + "\n");
                }
                catch { }

                if (_enableDiscordAlerts && !string.IsNullOrEmpty(_discordWebhookUrl))
                    SendDiscordAlert(playerName, steamId, triggerReason);

                // Kick player safely
                if (_kickOnDetection)
                {
                    TryKick(clientInfo, triggerReason);
                }
            }
        }
    }

    private static bool CanActNow(int entityId)
    {
        float now = Time.realtimeSinceStartup;
        if (LastActionTimeByEntityId.TryGetValue(entityId, out float last) && now - last < 2f)
        {
            return false;
        }

        LastActionTimeByEntityId[entityId] = now;
        return true;
    }

    private static string TryResolveSteamId(object clientInfo)
    {
        if (clientInfo == null)
        {
            return "unknown";
        }

        try
        {
            if (_playerIdField == null)
            {
                _playerIdField = clientInfo.GetType().GetField("playerId", BindingFlags.Public | BindingFlags.Instance);
            }

            object id = _playerIdField?.GetValue(clientInfo);
            return id?.ToString() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    private static void TryKick(object clientInfo, string reason)
    {
        try
        {
            if (_kickMethod != null && clientInfo != null)
            {
                _kickMethod.Invoke(GameManager.Instance, new object[] { clientInfo, $"CM/Debug attempt detected ({reason})" });
            }
        }
        catch
        {
            Debug.Log("[AntiCM] Failed to kick player.");
        }
    }

    private static void SendDiscordAlert(string playerName, string steamId, string reason)
    {
        try
        {
            var request = (HttpWebRequest)WebRequest.Create(_discordWebhookUrl);
            request.ContentType = "application/json";
            request.Method = "POST";

            string json = "{\"content\": \"WARNING: Player attempted CM/Debug!\\nPlayer: " + playerName + "\\nSteamID: " + steamId + "\\nReason: " + reason + "\"}";

            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            using (var dataStream = request.GetRequestStream())
                dataStream.Write(byteArray, 0, byteArray.Length);

            using (var response = (HttpWebResponse)request.GetResponse()) { }
        }
        catch
        {
            Debug.Log("[AntiCM] Failed to send Discord alert.");
        }
    }

    private static void EnsureConfigExists()
    {
        try
        {
            Directory.CreateDirectory(ModFolderPath);
            if (File.Exists(ConfigFilePath))
            {
                return;
            }

            string[] lines =
            {
                "# AntiCM configuration",
                "# key=value lines; '#' starts a comment",
                "kickOnDetection=true",
                "enableDiscordAlerts=false",
                "discordWebhookUrl=",
                "scanIntervalSeconds=0.25",
                "allowedSteamIds=",
                "# Example:",
                "# allowedSteamIds=76561198000000000,76561198000000001"
            };

            File.WriteAllLines(ConfigFilePath, lines);
        }
        catch
        {
            Debug.Log("[AntiCM] Failed to create default config file.");
        }
    }

    private static void LoadConfig()
    {
        _lastConfigLoadTime = Time.realtimeSinceStartup;

        if (!File.Exists(ConfigFilePath))
        {
            return;
        }

        try
        {
            var localAllowedIds = new HashSet<string>();
            bool localKickOnDetection = _kickOnDetection;
            bool localEnableDiscord = _enableDiscordAlerts;
            string localWebhook = _discordWebhookUrl;
            float localScanInterval = _scanIntervalSeconds;

            foreach (string rawLine in File.ReadAllLines(ConfigFilePath))
            {
                string line = (rawLine ?? string.Empty).Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                {
                    continue;
                }

                int separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = line.Substring(0, separatorIndex).Trim();
                string value = line.Substring(separatorIndex + 1).Trim();

                if (key.Equals("kickOnDetection", StringComparison.OrdinalIgnoreCase))
                {
                    if (bool.TryParse(value, out bool parsed))
                    {
                        localKickOnDetection = parsed;
                    }
                    continue;
                }

                if (key.Equals("enableDiscordAlerts", StringComparison.OrdinalIgnoreCase))
                {
                    if (bool.TryParse(value, out bool parsed))
                    {
                        localEnableDiscord = parsed;
                    }
                    continue;
                }

                if (key.Equals("discordWebhookUrl", StringComparison.OrdinalIgnoreCase))
                {
                    localWebhook = value;
                    continue;
                }

                if (key.Equals("scanIntervalSeconds", StringComparison.OrdinalIgnoreCase))
                {
                    if (float.TryParse(value, out float parsed) && parsed > 0.01f)
                    {
                        localScanInterval = parsed;
                    }
                    continue;
                }

                if (key.Equals("allowedSteamIds", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string token in value.Split(','))
                    {
                        string id = (token ?? string.Empty).Trim();
                        if (id.Length > 0)
                        {
                            localAllowedIds.Add(id);
                        }
                    }
                }
            }

            AllowedSteamIds.Clear();
            foreach (string id in localAllowedIds)
            {
                AllowedSteamIds.Add(id);
            }

            _kickOnDetection = localKickOnDetection;
            _enableDiscordAlerts = localEnableDiscord;
            _discordWebhookUrl = localWebhook;
            _scanIntervalSeconds = localScanInterval;
        }
        catch
        {
            Debug.Log("[AntiCM] Failed to load config; using last known values.");
        }
    }
}

[HarmonyPatch(typeof(GameManager), "Update")]
public class AntiCM_GameManagerUpdatePatch
{
    private static void Postfix()
    {
        ForceDisableMonitor.Tick();
    }
}

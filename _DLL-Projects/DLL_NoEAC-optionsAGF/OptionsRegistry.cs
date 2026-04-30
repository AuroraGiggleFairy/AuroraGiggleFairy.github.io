using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

public static class OptionsRegistry
{
    public sealed class OptionRow
    {
        public string Key;
        public string Label;
        public Func<bool> IsAvailable;
        public OptionMode DefaultMode;
    }

    private static readonly List<OptionRow> Rows = new List<OptionRow>();
    private static readonly Regex CountSuffixRegex = new Regex(@"\s*\[FFFFFF\]\(\d+\)\[-\]\s*$", RegexOptions.Compiled);
    private static readonly Regex ButtonRegex = new Regex(@"agfopt_row_(\d+)(?:_(off|on|num|numbers|cycle))?_btn", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static bool initialized;
    private static bool screamerPresent;
    private static bool visualEntityTrackerPresent;
    private static bool screamerModeCached;
    private static bool visualEntityTrackerModeCached;
    private static OptionMode cachedScreamerMode = OptionMode.OnWithNumbers;
    private static OptionMode cachedVisualEntityTrackerMode = OptionMode.On;

    public static IReadOnlyList<OptionRow> GetRows()
    {
        EnsureInitialized();
        return Rows;
    }

    public static void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        Rows.Clear();

        Rows.Add(new OptionRow
        {
            Key = "screamer_alert_mode",
            Label = "Screamer Alert",
            IsAvailable = IsScreamerAlertPresent,
            DefaultMode = OptionMode.OnWithNumbers
        });

        Rows.Add(new OptionRow
        {
            Key = "visual_entity_tracker_mode",
            Label = "Visual Entity Tracker",
            IsAvailable = IsVisualEntityTrackerPresent,
            DefaultMode = OptionMode.On
        });

        screamerPresent = AccessTools.TypeByName("ScreamerAlertsController") != null;
        visualEntityTrackerPresent = AccessTools.TypeByName("NoEACVisualEntityTracker.VisualEntityTrackerService") != null;
    }

    public static bool TryCycleByButtonName(string controlName)
    {
        EnsureInitialized();
        if (string.IsNullOrEmpty(controlName))
        {
            return false;
        }

        if (!TryParseButton(controlName, out int oneBased, out string action))
        {
            return false;
        }

        int index = oneBased - 1;
        if (index < 0 || index >= Rows.Count)
        {
            return false;
        }

        OptionRow row = Rows[index];
        if (!row.IsAvailable())
        {
            return false;
        }

        OptionMode current = GetMode(row.Key, row.DefaultMode);
        OptionMode next;
        switch (action)
        {
            case "off":
                next = OptionMode.Off;
                break;
            case "on":
                next = OptionMode.On;
                break;
            case "num":
            case "numbers":
                next = OptionMode.OnWithNumbers;
                break;
            case "cycle":
            default:
                next = NextMode(current);
                break;
        }

        SetMode(row.Key, next);
        Console.WriteLine("[optionsAGF] " + row.Key + " -> " + next);
        return true;
    }

    private static bool TryParseButton(string controlName, out int oneBased, out string action)
    {
        oneBased = 0;
        action = string.Empty;

        Match match = ButtonRegex.Match(controlName);
        if (!match.Success)
        {
            return false;
        }

        if (!int.TryParse(match.Groups[1].Value, out oneBased) || oneBased <= 0)
        {
            return false;
        }

        action = match.Groups[2].Success
            ? match.Groups[2].Value.ToLowerInvariant()
            : "cycle";
        if (string.IsNullOrEmpty(action))
        {
            action = "cycle";
        }
        return action == "off" || action == "on" || action == "num" || action == "numbers" || action == "cycle";
    }

    public static OptionMode GetMode(string key, OptionMode defaultMode)
    {
        if (string.Equals(key, "screamer_alert_mode", StringComparison.OrdinalIgnoreCase))
        {
            return GetScreamerModeCached(defaultMode);
        }

        if (string.Equals(key, "visual_entity_tracker_mode", StringComparison.OrdinalIgnoreCase))
        {
            return GetVisualEntityTrackerModeCached(defaultMode);
        }

        return OptionsStore.GetMode(key, defaultMode);
    }

    public static void SetMode(string key, OptionMode mode)
    {
        OptionsStore.SetMode(key, mode);
        if (string.Equals(key, "screamer_alert_mode", StringComparison.OrdinalIgnoreCase))
        {
            cachedScreamerMode = mode;
            screamerModeCached = true;
        }

        if (string.Equals(key, "visual_entity_tracker_mode", StringComparison.OrdinalIgnoreCase))
        {
            // Tracker mode is on/off only. Treat any non-off mode as on.
            cachedVisualEntityTrackerMode = mode == OptionMode.Off ? OptionMode.Off : OptionMode.On;
            visualEntityTrackerModeCached = true;
        }
    }

    public static string GetModeLabel(OptionMode mode)
    {
        switch (mode)
        {
            case OptionMode.Off:
                return "Off";
            case OptionMode.On:
                return "On";
            case OptionMode.OnWithNumbers:
                return "On + #";
            default:
                return "On + #";
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

    public static bool IsScreamerAlertPresent()
    {
        EnsureInitialized();
        if (!screamerPresent)
        {
            // If load order initializes optionsAGF first, re-check lazily until Screamer mod appears.
            screamerPresent = AccessTools.TypeByName("ScreamerAlertsController") != null;
        }
        return screamerPresent;
    }

    public static bool IsVisualEntityTrackerPresent()
    {
        EnsureInitialized();
        if (!visualEntityTrackerPresent)
        {
            visualEntityTrackerPresent = AccessTools.TypeByName("NoEACVisualEntityTracker.VisualEntityTrackerService") != null;
        }
        return visualEntityTrackerPresent;
    }

    public static OptionMode GetScreamerModeCached(OptionMode defaultMode)
    {
        EnsureInitialized();
        if (screamerModeCached)
        {
            return cachedScreamerMode;
        }

        cachedScreamerMode = OptionsStore.GetMode("screamer_alert_mode", defaultMode);
        screamerModeCached = true;
        return cachedScreamerMode;
    }

    public static OptionMode GetVisualEntityTrackerModeCached(OptionMode defaultMode)
    {
        EnsureInitialized();
        if (visualEntityTrackerModeCached)
        {
            return cachedVisualEntityTrackerMode;
        }

        OptionMode raw = OptionsStore.GetMode("visual_entity_tracker_mode", defaultMode);
        cachedVisualEntityTrackerMode = raw == OptionMode.Off ? OptionMode.Off : OptionMode.On;
        visualEntityTrackerModeCached = true;
        return cachedVisualEntityTrackerMode;
    }

    private static OptionMode NextMode(OptionMode current)
    {
        switch (current)
        {
            case OptionMode.Off:
                return OptionMode.On;
            case OptionMode.On:
                return OptionMode.OnWithNumbers;
            case OptionMode.OnWithNumbers:
                return OptionMode.Off;
            default:
                return OptionMode.OnWithNumbers;
        }
    }
}

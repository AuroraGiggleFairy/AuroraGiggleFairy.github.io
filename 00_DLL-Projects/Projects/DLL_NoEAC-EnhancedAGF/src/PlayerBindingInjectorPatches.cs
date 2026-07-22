using HarmonyLib;
using StatControllers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Injects EnhancedAGF player bindings into existing vanilla XUi controllers so HUD XML
/// does not need dedicated PlayerStats controller instances.
/// </summary>
[HarmonyPatch]
public static class PlayerBindingInjectorPatches
{
    // HUD extras (armor/loot/temp) do not need 20Hz refresh; 6-7Hz is enough and much cheaper.
    private const float ChangePollIntervalSeconds = 0.15f;

    private static readonly HashSet<string> FalseWhenNoPlayerBindings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "PlayerIsDay",
        "PlayerIsNight",
        "PlayerCurrentAmmoVisible"
    };

    private static readonly ConditionalWeakTable<XUiController, Tracker> Trackers = new ConditionalWeakTable<XUiController, Tracker>();
    private static readonly List<XUiController> ActiveControllers = new List<XUiController>(8);
    private static float nextGlobalPollTime;

    private sealed class Tracker
    {
        public readonly List<Binding> ActiveBindings = new List<Binding>();
        public readonly Dictionary<string, Binding> ActiveBindingLookup = new Dictionary<string, Binding>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<Binding, string> LastValues = new Dictionary<Binding, string>();
        public bool IsDirty = true;
        public bool IsListed;
    }

    [HarmonyPatch(typeof(XUiC_HUDStatBar), "GetBindingValueInternal")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    private static bool HudStatBarPrefix(XUiC_HUDStatBar __instance, ref bool __result, ref string _value, string _bindingName)
    {
        return !TryHandle(__instance, _bindingName, ref _value, ref __result);
    }

    [HarmonyPatch(typeof(XUiC_Location), "GetBindingValueInternal")]
    [HarmonyPrefix]
    private static bool LocationPrefix(XUiC_Location __instance, ref bool __result, ref string _value, string _bindingName)
    {
        return !TryHandle(__instance, _bindingName, ref _value, ref __result);
    }

    [HarmonyPatch(typeof(XUiC_MapStats), "GetBindingValueInternal")]
    [HarmonyPrefix]
    private static bool MapStatsPrefix(XUiC_MapStats __instance, ref bool __result, ref string value, string bindingName)
    {
        return !TryHandle(__instance, bindingName, ref value, ref __result);
    }

    [HarmonyPatch(typeof(XUiC_CompassWindow), "GetBindingValueInternal")]
    [HarmonyPrefix]
    private static bool CompassWindowPrefix(XUiC_CompassWindow __instance, ref bool __result, ref string value, string bindingName)
    {
        return !TryHandle(__instance, bindingName, ref value, ref __result);
    }

    /// <summary>
    /// Single global poll from ModAPI GameUpdate instead of Update postfixes on every matching controller.
    /// </summary>
    public static void Tick()
    {
        if (ActiveControllers.Count == 0)
        {
            return;
        }

        float now = Time.realtimeSinceStartup;
        if (now < nextGlobalPollTime)
        {
            return;
        }

        nextGlobalPollTime = now + ChangePollIntervalSeconds;

        for (int i = ActiveControllers.Count - 1; i >= 0; i--)
        {
            XUiController controller = ActiveControllers[i];
            if (controller == null || controller.xui == null || controller.ViewComponent == null)
            {
                ActiveControllers.RemoveAt(i);
                continue;
            }

            if (!Trackers.TryGetValue(controller, out Tracker tracker) || tracker.ActiveBindings.Count == 0)
            {
                ActiveControllers.RemoveAt(i);
                continue;
            }

            KeepAlive(controller, tracker, now);
        }
    }

    private static bool TryHandle(XUiController controller, string bindingName, ref string value, ref bool result)
    {
        if (controller == null || !LooksLikeEnhancedPlayerBinding(bindingName) || !Bindings.TryGetBinding(bindingName, out Binding binding))
        {
            return false;
        }

        Tracker tracker = Trackers.GetOrCreateValue(controller);
        RegisterBinding(controller, tracker, bindingName, binding);

        EntityPlayerLocal player = controller.xui?.playerUI?.entityPlayer;
        if (player == null)
        {
            value = FalseWhenNoPlayerBindings.Contains(bindingName) ? "false" : "0";
            result = true;
            return true;
        }

        value = binding.GetCurrentValue(player);
        if (string.IsNullOrEmpty(value))
        {
            value = FalseWhenNoPlayerBindings.Contains(bindingName) ? "false" : "0";
        }

        result = true;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LooksLikeEnhancedPlayerBinding(string bindingName)
    {
        if (string.IsNullOrEmpty(bindingName) || bindingName.Length < 6)
        {
            return false;
        }

        // HUDPlus / Dishong / ammo bindings all start with Player*, DoomGuy*, or Inventory*.
        char c = bindingName[0];
        return c == 'P' || c == 'p' || c == 'D' || c == 'd' || c == 'I' || c == 'i';
    }

    private static void RegisterBinding(XUiController controller, Tracker tracker, string bindingName, Binding binding)
    {
        if (tracker.ActiveBindingLookup.ContainsKey(bindingName))
        {
            return;
        }

        tracker.ActiveBindings.Add(binding);
        tracker.ActiveBindingLookup[bindingName] = binding;
        if (!tracker.ActiveBindingLookup.ContainsKey(binding.Name))
        {
            tracker.ActiveBindingLookup[binding.Name] = binding;
        }

        if (!tracker.LastValues.ContainsKey(binding))
        {
            tracker.LastValues[binding] = string.Empty;
        }

        tracker.IsDirty = true;
        if (!tracker.IsListed)
        {
            tracker.IsListed = true;
            ActiveControllers.Add(controller);
            nextGlobalPollTime = 0f;
        }
    }

    private static void KeepAlive(XUiController controller, Tracker tracker, float now)
    {
        EntityPlayerLocal player = controller.xui?.playerUI?.entityPlayer;
        if (player == null)
        {
            return;
        }

        bool shouldRefresh = tracker.IsDirty || HasChanged(tracker, player);
        if (!shouldRefresh)
        {
            return;
        }

        controller.RefreshBindings();
        tracker.IsDirty = false;
    }

    private static bool HasChanged(Tracker tracker, EntityPlayer player)
    {
        for (int i = 0; i < tracker.ActiveBindings.Count; i++)
        {
            Binding binding = tracker.ActiveBindings[i];
            if (!tracker.LastValues.TryGetValue(binding, out string lastValue))
            {
                lastValue = string.Empty;
            }

            if (binding.HasValueChanged(player, ref lastValue))
            {
                tracker.LastValues[binding] = lastValue;
                return true;
            }
        }

        return false;
    }
}

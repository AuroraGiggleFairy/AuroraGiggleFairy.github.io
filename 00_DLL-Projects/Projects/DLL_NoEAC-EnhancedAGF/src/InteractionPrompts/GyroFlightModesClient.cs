using System;
using System.Reflection;

namespace ExpandedInteractionPrompts
{
    /// <summary>
    /// Flight-mode pill for a gyrocopter. Only when Gyro Flight Modes is loaded.
    /// </summary>
    internal static class GyroFlightModesClient
    {
        private delegate bool TryGetFlightModePromptDel(EntityVehicle vehicle, out string icon, out string displayName);

        private static bool s_resolved;
        private static TryGetFlightModePromptDel s_query;

        public static string TryFormatFlightModeRow(EntityVehicle vehicle)
        {
            if (vehicle == null)
                return null;

            TryGetFlightModePromptDel query = Resolve();
            if (query == null)
                return null;

            string icon;
            string displayName;
            if (!query(vehicle, out icon, out displayName))
                return null;
            if (string.IsNullOrEmpty(icon) || string.IsNullOrEmpty(displayName))
                return null;

            return PromptStateHelpers.FormatIconRow(icon, displayName, PromptStateHelpers.SlotAboveOwner);
        }

        private static TryGetFlightModePromptDel Resolve()
        {
            if (s_resolved)
                return s_query;

            s_resolved = true;
            Type api = ScreamerAlertEnhancedGate.FindLoadedType("GyroFlightModes.GyroFlightModesApi");
            MethodInfo method = api == null
                ? null
                : api.GetMethod(
                    "TryGetFlightModePrompt",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new[]
                    {
                        typeof(EntityVehicle),
                        typeof(string).MakeByRefType(),
                        typeof(string).MakeByRefType()
                    },
                    null);
            if (method == null)
                return null;
            if (!string.Equals(method.DeclaringType?.Assembly?.GetName()?.Name, "GyroFlightModes", StringComparison.Ordinal))
                return null;

            s_query = (TryGetFlightModePromptDel)Delegate.CreateDelegate(typeof(TryGetFlightModePromptDel), method);
            return s_query;
        }
    }
}

using HarmonyLib;
using System;
using System.Reflection;

[HarmonyPatch]
public static class ScreamerAlertMessagePatch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method("ScreamerAlertsController:GetScreamerAlertMessage");
    }

    public static void Postfix(ref string __result)
    {
        ApplyMode(ref __result);
    }

    private static void ApplyMode(ref string message)
    {
        OptionMode mode = OptionsRegistry.GetScreamerModeCached(OptionMode.OnWithNumbers);
        if (mode == OptionMode.Off)
        {
            message = string.Empty;
            return;
        }

        if (mode == OptionMode.On)
        {
            message = OptionsRegistry.StripNumberSuffix(message);
        }
    }
}

[HarmonyPatch]
public static class ScreamerHordeAlertMessagePatch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method("ScreamerAlertsController:GetScreamerHordeAlertMessage");
    }

    public static void Postfix(ref string __result)
    {
        OptionMode mode = OptionsRegistry.GetScreamerModeCached(OptionMode.OnWithNumbers);
        if (mode == OptionMode.Off)
        {
            __result = string.Empty;
            return;
        }

        if (mode == OptionMode.On)
        {
            __result = OptionsRegistry.StripNumberSuffix(__result);
        }
    }
}

[HarmonyPatch]
public static class ScreamerAlertVisibilityPatch
{
    private static FieldInfo screamerAlertMessageField;
    private static FieldInfo screamerHordeAlertMessageField;
    private static bool xuiResolved;
    private static FieldInfo xuiInstanceField;
    private static PropertyInfo xuiViewComponentProperty;
    private static PropertyInfo viewIsVisibleProperty;
    private static MethodInfo xuiRefreshMethod;

    public static MethodBase TargetMethod()
    {
        return AccessTools.Method("ScreamerAlertsController:Update");
    }

    public static void Postfix(object __instance)
    {
        if (__instance == null)
        {
            return;
        }

        OptionMode mode = OptionsRegistry.GetScreamerModeCached(OptionMode.OnWithNumbers);
        if (mode != OptionMode.Off)
        {
            return;
        }

        Type controllerType = __instance.GetType();
        if (screamerAlertMessageField == null)
        {
            screamerAlertMessageField = AccessTools.Field(controllerType, "screamerAlertMessage");
        }

        if (screamerHordeAlertMessageField == null)
        {
            screamerHordeAlertMessageField = AccessTools.Field(controllerType, "screamerHordeAlertMessage");
        }

        if (screamerAlertMessageField != null)
        {
            screamerAlertMessageField.SetValue(__instance, string.Empty);
        }

        if (screamerHordeAlertMessageField != null)
        {
            screamerHordeAlertMessageField.SetValue(__instance, string.Empty);
        }

        // Force-hide the Screamer alert container immediately so its dark background never lingers in Off mode.
        if (!xuiResolved)
        {
            xuiResolved = true;
            Type xuiType = controllerType.Assembly.GetType("XUiC_ScreamerAlerts");
            if (xuiType != null)
            {
                // In this build, Instance is a static field (not property).
                xuiInstanceField = AccessTools.Field(xuiType, "Instance");
                xuiViewComponentProperty = AccessTools.Property(xuiType, "ViewComponent");
                xuiRefreshMethod = AccessTools.Method(xuiType, "RefreshBindingsSelfAndChildren");
            }
        }

        object xuiController = xuiInstanceField != null
            ? xuiInstanceField.GetValue(null)
            : null;
        if (xuiController != null)
        {
            object viewComponent = xuiViewComponentProperty != null
                ? xuiViewComponentProperty.GetValue(xuiController, null)
                : null;
            if (viewComponent != null)
            {
                if (viewIsVisibleProperty == null)
                {
                    viewIsVisibleProperty = AccessTools.Property(viewComponent.GetType(), "IsVisible");
                }

                viewIsVisibleProperty?.SetValue(viewComponent, false, null);
            }

            xuiRefreshMethod?.Invoke(xuiController, null);
        }
    }
}

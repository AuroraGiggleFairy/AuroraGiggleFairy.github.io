using HarmonyLib;
using System;

[HarmonyPatch]
public static class PurpleBookSchematicsWindowPagingPatch
{
    private const string SchematicsGroupId = "schematics";
    private const string WindowPagingGroupId = "windowpaging";
    private const string SchematicsPageId = "schematics";

    [HarmonyPatch(typeof(XUiWindowGroup), "OnOpen")]
    [HarmonyPostfix]
    private static void WindowGroupOnOpenPostfix(XUiWindowGroup __instance)
    {
        if (!IsSchematicsGroup(__instance))
        {
            return;
        }

        try
        {
            XUi xui = __instance.xui;
            GUIWindowManager windowManager = xui?.playerUI?.windowManager;
            if (windowManager == null)
            {
                return;
            }

            // Schematics uses a custom window_group with the base controller, so
            // we must mirror vanilla group behavior and ensure paging is present.
            if (!windowManager.IsWindowOpen(WindowPagingGroupId))
            {
                windowManager.Open(WindowPagingGroupId, _bModal: false);
            }

            xui.FindWindowGroupByName(WindowPagingGroupId)
                ?.Controller
                ?.GetChildByType<XUiC_WindowSelector>()
                ?.SetSelected(SchematicsPageId);
        }
        catch (Exception ex)
        {
            Logging.Error("PurpleBook schematics OnOpen paging patch failed: " + ex);
        }
    }

    [HarmonyPatch(typeof(XUiWindowGroup), "OnClose")]
    [HarmonyPostfix]
    private static void WindowGroupOnClosePostfix(XUiWindowGroup __instance)
    {
        if (!IsSchematicsGroup(__instance))
        {
            return;
        }

        try
        {
            GUIWindowManager windowManager = __instance.xui?.playerUI?.windowManager;
            if (windowManager != null && windowManager.IsWindowOpen(WindowPagingGroupId))
            {
                windowManager.Close(WindowPagingGroupId);
            }
        }
        catch (Exception ex)
        {
            Logging.Error("PurpleBook schematics OnClose paging patch failed: " + ex);
        }
    }

    private static bool IsSchematicsGroup(XUiWindowGroup windowGroup)
    {
        return windowGroup != null
            && string.Equals(windowGroup.Id, SchematicsGroupId, StringComparison.OrdinalIgnoreCase);
    }
}
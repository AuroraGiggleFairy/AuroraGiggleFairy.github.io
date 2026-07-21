using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

public class OpenAllActionInit : IModApi
{
    public void InitMod(Mod _modInstance)
    {
        var harmony = new Harmony("OpenAll.Standalone.OpenAllAction");
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(XUiC_ItemActionList), "AddActionListEntry")]
public static class XUiC_ItemActionList_AddActionListEntry_OpenAllPatch
{
    public static void Postfix(XUiC_ItemActionList __instance, BaseItemActionEntry actionEntry)
    {
        if (__instance == null || actionEntry == null)
        {
            return;
        }

        if (actionEntry is ItemActionEntryOpenAll)
        {
            return;
        }

        if (!OpenAllActionHelpers.IsOpenAction(actionEntry))
        {
            return;
        }

        if (OpenAllActionHelpers.HasOpenAllEntry(__instance))
        {
            return;
        }

        XUiC_ItemStack stackController = OpenAllActionHelpers.GetStackController(actionEntry);
        if (stackController == null || stackController.ItemStack.count <= 0)
        {
            return;
        }

        if (!OpenAllActionHelpers.IsEligibleForOpenAll(stackController))
        {
            return;
        }

        ItemActionEntryOpenAll openAllEntry = new ItemActionEntryOpenAll(stackController, actionEntry);
        __instance.AddActionListEntry(openAllEntry);
        OpenAllActionHelpers.PlaceOpenAllEntryBelowOpen(__instance, openAllEntry, actionEntry);
    }
}

[HarmonyPatch(typeof(XUiC_ItemActionList), "Update")]
public static class XUiC_ItemActionList_Update_OpenAllQueuePatch
{
    public static void Postfix(XUiC_ItemActionList __instance, float _dt)
    {
        OpenAllActionQueue.Process(__instance);
    }
}

[HarmonyPatch(typeof(XUiController), "Update")]
public static class XUiController_Update_OpenAllQueueFallbackPatch
{
    public static void Postfix(XUiController __instance, float _dt)
    {
        if (__instance is XUiC_ItemActionList actionList)
        {
            OpenAllActionQueue.Process(actionList);
        }
    }
}

[HarmonyPatch(typeof(XUiController), "OnClose")]
public static class XUiController_OnClose_OpenAllQueueCancelPatch
{
    public static void Postfix(XUiController __instance)
    {
        if (__instance is XUiC_ItemActionList actionList)
        {
            if (OpenAllActionQueue.IsInternalCloseSuppressed(actionList))
            {
                return;
            }

            OpenAllActionQueue.Cancel(actionList, "item action list closed");
        }
    }
}

[HarmonyPatch(typeof(XUiController), "OnPressed")]
public static class XUiController_OnPressed_OpenAllQueueCancelPatch
{
    public static void Prefix(XUiController __instance, int _mouseButton)
    {
        if (!OpenAllActionQueue.HasActiveSessions())
        {
            return;
        }

        if (_mouseButton != -1 && _mouseButton != -2)
        {
            return;
        }

        if (__instance == null || OpenAllActionQueue.IsAnyInternalSuppressed())
        {
            return;
        }

        if (!OpenAllActionHelpers.IsInventoryOrContainerController(__instance))
        {
            return;
        }

        if (OpenAllActionHelpers.IsOpenAllActionButtonController(__instance))
        {
            return;
        }

        OpenAllActionQueue.CancelAll("inventory/container UI press while open all active");
    }
}

[HarmonyPatch(typeof(BaseItemActionEntry), "OnActivated")]
public static class BaseItemActionEntry_OnActivated_OpenAllQueueCancelPatch
{
    public static void Prefix(BaseItemActionEntry __instance)
    {
        if (__instance == null || __instance is ItemActionEntryOpenAll)
        {
            return;
        }

        XUiC_ItemActionList actionList = __instance.ParentActionList ?? OpenAllActionHelpers.FindParentActionList(__instance.ItemController);
        if (actionList == null || OpenAllActionQueue.IsInternalActivationSuppressed(actionList))
        {
            return;
        }

        if (OpenAllActionQueue.IsActiveFor(actionList))
        {
            OpenAllActionQueue.Cancel(actionList, "another action activated");
            OpenAllActionHelpers.SafeRefreshActionList(actionList);
        }
    }
}

[HarmonyPatch(typeof(XUiC_ContainerStandardControls), "Sort")]
public static class XUiC_ContainerStandardControls_Sort_OpenAllQueueCancelPatch
{
    public static void Prefix()
    {
        OpenAllActionQueue.CancelAll("container sort pressed");
    }
}

[HarmonyPatch(typeof(XUiC_BackpackWindow), "BtnSort_OnPress")]
public static class XUiC_BackpackWindow_BtnSort_OnPress_OpenAllQueueCancelPatch
{
    public static void Prefix()
    {
        OpenAllActionQueue.CancelAll("backpack sort pressed");
    }
}

[HarmonyPatch(typeof(XUiC_VehicleContainer), "btnSort_OnPress")]
public static class XUiC_VehicleContainer_btnSort_OnPress_OpenAllQueueCancelPatch
{
    public static void Prefix()
    {
        OpenAllActionQueue.CancelAll("vehicle sort pressed");
    }
}

public class ItemActionEntryOpenAll : BaseItemActionEntry
{
    private const string OpenAllLocalizationKey = "lblContextActionOpenAll";
    private const string StopLocalizationKey = "lblContextActionOpenAllStop";
    private const string OpenAllIconName = "store_all_up";
    private readonly BaseItemActionEntry fallbackOpenEntry;

    public ItemActionEntryOpenAll(XUiController itemController, BaseItemActionEntry templateEntry)
        : base(
              itemController,
              OpenAllLocalizationKey,
              OpenAllActionHelpers.BuildPrefixedIconName(templateEntry?.IconName, OpenAllIconName),
              BaseItemActionEntry.GamepadShortCut.DPadLeft,
              templateEntry.SoundName,
              templateEntry.DisabledSound)
    {
          fallbackOpenEntry = templateEntry;
    }

    public override void RefreshEnabled()
    {
        XUiC_ItemActionList actionList = ParentActionList ?? OpenAllActionHelpers.FindParentActionList(ItemController);
        BaseItemActionEntry liveOpenEntry = OpenAllActionHelpers.FindOpenEntry(actionList);
        if (liveOpenEntry != null)
        {
            liveOpenEntry.RefreshEnabled();
        }
        else if (fallbackOpenEntry != null)
        {
            fallbackOpenEntry.RefreshEnabled();
        }

        BaseItemActionEntry openStateEntry = liveOpenEntry ?? fallbackOpenEntry;
        bool canOpen = openStateEntry != null && openStateEntry.Enabled;
        XUiC_ItemStack stackController = ItemController as XUiC_ItemStack ?? OpenAllActionHelpers.GetStackController(fallbackOpenEntry);
        bool isActive = OpenAllActionQueue.IsActiveFor(actionList, stackController);

        OpenAllActionHelpers.SetActionEntryLabel(this, isActive ? StopLocalizationKey : OpenAllLocalizationKey);
        Enabled = isActive || (OpenAllActionHelpers.GetStackCount(ItemController) > 0 && canOpen);
    }

    public override void OnActivated()
    {
        XUiC_ItemActionList actionList = ParentActionList ?? OpenAllActionHelpers.FindParentActionList(ItemController);
        if (actionList == null)
        {
            return;
        }

        BaseItemActionEntry liveOpenEntry = OpenAllActionHelpers.FindOpenEntry(actionList);
        BaseItemActionEntry openStateEntry = liveOpenEntry ?? fallbackOpenEntry;
        if (openStateEntry != null)
        {
            openStateEntry.RefreshEnabled();
        }

        bool isAlreadyActive = OpenAllActionQueue.IsActiveFor(actionList);
        if (!isAlreadyActive && (openStateEntry == null || !openStateEntry.Enabled))
        {
            openStateEntry?.OnDisabledActivate();
            actionList.RefreshActionList();
            return;
        }

        XUiC_ItemStack stackController = ItemController as XUiC_ItemStack ?? OpenAllActionHelpers.GetStackController(fallbackOpenEntry);
        bool started = OpenAllActionQueue.StartOrToggle(actionList, fallbackOpenEntry, stackController);
        if (started)
        {
            OpenAllActionQueue.Process(actionList);
        }

        actionList.RefreshActionList();
    }
}

public static class OpenAllActionQueue
{
    private class OpenAllSession
    {
        public XUiC_ItemActionList ActionList;
        public BaseItemActionEntry FallbackOpenEntry;
        public XUiC_ItemStack StackController;
        public int ItemType;
    }

    private const int MaxOpensPerFrame = 900;
    private const long MaxFrameMilliseconds = 20;
    private static readonly Dictionary<XUiC_ItemActionList, OpenAllSession> ActiveLists = new Dictionary<XUiC_ItemActionList, OpenAllSession>();
    private static readonly HashSet<XUiC_ItemActionList> SuppressedCloseCancel = new HashSet<XUiC_ItemActionList>();
    private static readonly HashSet<XUiC_ItemActionList> SuppressedActivationCancel = new HashSet<XUiC_ItemActionList>();

    public static bool StartOrToggle(XUiC_ItemActionList actionList, BaseItemActionEntry fallbackOpenEntry, XUiC_ItemStack stackController)
    {
        if (actionList == null || stackController == null)
        {
            return false;
        }

        if (IsActiveFor(actionList, stackController))
        {
            Cancel(actionList);
            return false;
        }

        ActiveLists[actionList] = new OpenAllSession
        {
            ActionList = actionList,
            FallbackOpenEntry = fallbackOpenEntry,
            StackController = stackController,
            ItemType = stackController.ItemStack.itemValue.type
        };

        return true;
    }

    public static void Cancel(XUiC_ItemActionList actionList, string reason = null)
    {
        if (actionList == null)
        {
            return;
        }

        ActiveLists.Remove(actionList);
    }

    public static bool HasActiveSessions()
    {
        return ActiveLists.Count > 0;
    }

    public static void CancelAll(string reason = null)
    {
        if (ActiveLists.Count == 0)
        {
            return;
        }

        var activeActionLists = new List<XUiC_ItemActionList>(ActiveLists.Keys);
        ActiveLists.Clear();

        for (int index = 0; index < activeActionLists.Count; index++)
        {
            XUiC_ItemActionList actionList = activeActionLists[index];
            if (actionList == null)
            {
                continue;
            }

            OpenAllActionHelpers.SafeRefreshActionList(actionList);
        }
    }

    public static bool IsActiveFor(XUiC_ItemActionList actionList, XUiC_ItemStack stackController)
    {
        if (actionList == null || stackController == null)
        {
            return false;
        }

        OpenAllSession session;
        if (!ActiveLists.TryGetValue(actionList, out session) || session == null)
        {
            return false;
        }

        return session.ItemType == stackController.ItemStack.itemValue.type;
    }

    public static bool IsActiveFor(XUiC_ItemActionList actionList)
    {
        return actionList != null && ActiveLists.ContainsKey(actionList);
    }

    public static bool IsInternalCloseSuppressed(XUiC_ItemActionList actionList)
    {
        return actionList != null && SuppressedCloseCancel.Contains(actionList);
    }

    public static bool IsInternalActivationSuppressed(XUiC_ItemActionList actionList)
    {
        return actionList != null && SuppressedActivationCancel.Contains(actionList);
    }

    public static bool IsAnyInternalSuppressed()
    {
        return SuppressedCloseCancel.Count > 0 || SuppressedActivationCancel.Count > 0;
    }

    public static void Process(XUiC_ItemActionList actionList)
    {
        if (actionList == null)
        {
            return;
        }

        OpenAllSession session;
        if (!ActiveLists.TryGetValue(actionList, out session) || session == null)
        {
            return;
        }

        ProcessSession(session);
    }

    private static void ProcessSession(OpenAllSession session)
    {
        if (session == null || session.ActionList == null)
        {
            return;
        }

        XUiC_ItemActionList actionList = session.ActionList;

        int opensPerformed = 0;
        Stopwatch stopwatch = Stopwatch.StartNew();

        BaseItemActionEntry initialLiveOpen = OpenAllActionHelpers.FindOpenEntry(actionList);
        BaseItemActionEntry initialOpenEntry = initialLiveOpen ?? session.FallbackOpenEntry;
        XUiC_ItemStack initialStackController = OpenAllActionHelpers.GetStackController(initialOpenEntry) ?? session.StackController;

        int allowedOpensThisFrame = MaxOpensPerFrame;
        if (initialStackController != null)
        {
            int stackCount = initialStackController.ItemStack.count;
            if (stackCount > 0)
            {
                allowedOpensThisFrame = Math.Min(MaxOpensPerFrame, stackCount);
            }
        }

        for (int index = 0; index < allowedOpensThisFrame; index++)
        {
            OpenAllSession latestSession;
            if (!ActiveLists.TryGetValue(actionList, out latestSession) || latestSession == null)
            {
                return;
            }

            BaseItemActionEntry liveOpenEntry = OpenAllActionHelpers.FindOpenEntry(actionList);
            BaseItemActionEntry openEntry = liveOpenEntry ?? latestSession.FallbackOpenEntry;
            if (openEntry == null)
            {
                ActiveLists.Remove(actionList);
                return;
            }

            XUiC_ItemStack stackController = OpenAllActionHelpers.GetStackController(openEntry) ?? latestSession.StackController;
            if (stackController == null)
            {
                ActiveLists.Remove(actionList);
                return;
            }

            int previousCount = stackController.ItemStack.count;
            if (previousCount <= 0)
            {
                ActiveLists.Remove(actionList);
                OpenAllActionHelpers.SafeRefreshActionList(actionList);
                return;
            }

            if (!OpenAllActionHelpers.CanAcceptOpenBundleOutputs(stackController))
            {
                ActiveLists.Remove(actionList);
                OpenAllActionHelpers.SafeRefreshActionList(actionList);
                return;
            }

            SuppressedCloseCancel.Add(actionList);
            SuppressedActivationCancel.Add(actionList);
            try
            {
                openEntry.OnActivated();
            }
            finally
            {
                SuppressedActivationCancel.Remove(actionList);
                SuppressedCloseCancel.Remove(actionList);
            }

            int currentCount = stackController.ItemStack.count;
            if (currentCount <= 0)
            {
                ActiveLists.Remove(actionList);
                OpenAllActionHelpers.SafeRefreshActionList(actionList);
                return;
            }

            if (currentCount >= previousCount)
            {
                BaseItemActionEntry stateEntry = liveOpenEntry ?? openEntry;
                stateEntry.RefreshEnabled();
                if (!stateEntry.Enabled && stackController.ItemStack.count > 1)
                {
                    stateEntry.OnDisabledActivate();
                    ActiveLists.Remove(actionList);
                    OpenAllActionHelpers.SafeRefreshActionList(actionList);
                    return;
                }

                BaseItemActionEntry refreshedOpenEntry = OpenAllActionHelpers.FindOpenEntry(actionList);
                XUiC_ItemStack refreshedStackController = OpenAllActionHelpers.GetStackController(refreshedOpenEntry);
                int refreshedCount = refreshedStackController?.ItemStack.count ?? currentCount;
                if (refreshedCount < previousCount)
                {
                    opensPerformed++;
                    if (stopwatch.ElapsedMilliseconds >= MaxFrameMilliseconds)
                    {
                        break;
                    }

                    continue;
                }

                ActiveLists.Remove(actionList);
                OpenAllActionHelpers.SafeRefreshActionList(actionList);
                return;
            }

            opensPerformed++;

            if (stopwatch.ElapsedMilliseconds >= MaxFrameMilliseconds)
            {
                break;
            }
        }
    }
}

public static class OpenAllActionHelpers
{
    private const string OpenLocalizationKey = "lblContextActionOpen";
    private const string OpenDisplayText = "Open";
    private const string OpenAllLocalizationKey = "lblContextActionOpenAll";
    private const string OpenAllDisplayText = "Open All";
    private static readonly FieldInfo ItemActionEntriesField = typeof(XUiC_ItemActionList).GetField("itemActionEntries", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly PropertyInfo BaseItemActionEntryItemControllerProperty = typeof(BaseItemActionEntry).GetProperty("ItemController", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly PropertyInfo BaseItemActionEntryActionNameProperty = typeof(BaseItemActionEntry).GetProperty("ActionName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo BaseItemActionEntryActionNameField = typeof(BaseItemActionEntry).GetField("actionName", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? typeof(BaseItemActionEntry).GetField("_actionName", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? typeof(BaseItemActionEntry).GetField("ActionName", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly PropertyInfo ItemClassActionsProperty = typeof(ItemClass).GetProperty("Actions", BindingFlags.Public | BindingFlags.Instance);

    public static bool HasOpenAllEntry(XUiC_ItemActionList actionList)
    {
        return FindEntryByActionName(actionList, OpenAllLocalizationKey) != null
            || FindEntryByActionName(actionList, OpenAllDisplayText) != null;
    }

    public static void PlaceOpenAllEntryBelowOpen(XUiC_ItemActionList actionList, BaseItemActionEntry openAllEntry, BaseItemActionEntry openEntry)
    {
        List<BaseItemActionEntry> entries = GetEntries(actionList);
        if (entries == null || openAllEntry == null)
        {
            return;
        }

        entries.Remove(openAllEntry);

        int openIndex = openEntry != null ? entries.IndexOf(openEntry) : -1;
        if (openIndex < 0)
        {
            BaseItemActionEntry resolvedOpenEntry = FindOpenEntry(actionList);
            openIndex = resolvedOpenEntry != null ? entries.IndexOf(resolvedOpenEntry) : -1;
        }

        int insertIndex = openIndex >= 0 ? openIndex + 1 : entries.Count;
        if (insertIndex < 0 || insertIndex > entries.Count)
        {
            insertIndex = entries.Count;
        }

        entries.Insert(insertIndex, openAllEntry);
    }

    public static void SetActionEntryLabel(BaseItemActionEntry entry, string actionName)
    {
        if (entry == null || string.IsNullOrEmpty(actionName))
        {
            return;
        }

        string localizedActionName = actionName;
        try
        {
            string resolved = Localization.Get(actionName);
            if (!string.IsNullOrEmpty(resolved))
            {
                localizedActionName = resolved;
            }
        }
        catch
        {
        }

        try
        {
            if (BaseItemActionEntryActionNameProperty != null && BaseItemActionEntryActionNameProperty.CanWrite)
            {
                BaseItemActionEntryActionNameProperty.SetValue(entry, localizedActionName, null);
                return;
            }

            if (BaseItemActionEntryActionNameField != null)
            {
                BaseItemActionEntryActionNameField.SetValue(entry, localizedActionName);
            }
        }
        catch
        {
        }
    }

    public static void SafeRefreshActionList(XUiC_ItemActionList actionList)
    {
        if (actionList == null)
        {
            return;
        }

        try
        {
            actionList.RefreshActionList();
        }
        catch
        {
        }
    }

    public static BaseItemActionEntry FindOpenEntry(XUiC_ItemActionList actionList, ItemClass itemClass = null)
    {
        BaseItemActionEntry keyEntry = FindEntryByActionName(actionList, OpenLocalizationKey);
        if (keyEntry != null)
        {
            return keyEntry;
        }

        BaseItemActionEntry displayEntry = FindEntryByActionName(actionList, OpenDisplayText);
        if (displayEntry != null)
        {
            return displayEntry;
        }

        List<BaseItemActionEntry> entries = GetEntries(actionList);
        if (entries == null || entries.Count == 0)
        {
            return null;
        }

        foreach (BaseItemActionEntry entry in entries)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry is ItemActionEntryOpenAll)
            {
                continue;
            }

            string typeName = entry.GetType().Name;
            if (!string.IsNullOrEmpty(typeName) && typeName.IndexOf("Open", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return entry;
            }

            if (!string.IsNullOrEmpty(entry.ActionName) && entry.ActionName.IndexOf("Open", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return entry;
            }
        }

        if (HasOpenItemAction(itemClass))
        {
            foreach (BaseItemActionEntry entry in entries)
            {
                if (entry != null && string.Equals(entry.GetType().Name, "ItemActionEntryUse", StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }
        }

        return null;
    }

    public static BaseItemActionEntry FindOpenEntryFromController(XUiController itemController)
    {
        if (itemController == null)
        {
            return null;
        }

        XUiController current = itemController;
        while (current != null)
        {
            if (current is XUiC_ItemActionList actionList)
            {
                return FindOpenEntry(actionList);
            }

            current = current.Parent;
        }

        return null;
    }

    public static XUiC_ItemActionList FindParentActionList(XUiController itemController)
    {
        if (itemController == null)
        {
            return null;
        }

        XUiController current = itemController;
        while (current != null)
        {
            if (current is XUiC_ItemActionList actionList)
            {
                return actionList;
            }

            current = current.Parent;
        }

        return null;
    }

    public static int GetStackCount(XUiController itemController)
    {
        if (itemController is XUiC_ItemStack itemStackController)
        {
            return itemStackController.ItemStack.count;
        }

        return 0;
    }

    public static XUiC_ItemStack GetStackController(BaseItemActionEntry entry)
    {
        if (entry == null || BaseItemActionEntryItemControllerProperty == null)
        {
            return null;
        }

        return BaseItemActionEntryItemControllerProperty.GetValue(entry, null) as XUiC_ItemStack;
    }

    public static bool IsOpenAction(BaseItemActionEntry entry)
    {
        if (entry == null)
        {
            return false;
        }

        if (entry is ItemActionEntryOpenAll)
        {
            return false;
        }

        if (string.Equals(entry.ActionName, OpenLocalizationKey, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(entry.ActionName, OpenDisplayText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string entryType = entry.GetType().Name;
        if (!string.IsNullOrEmpty(entryType) && entryType.IndexOf("Open", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        return !string.IsNullOrEmpty(entry.ActionName) && entry.ActionName.IndexOf("Open", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool IsInventoryOrContainerController(XUiController controller)
    {
        XUiController current = controller;
        while (current != null)
        {
            if (current is XUiC_ItemActionList ||
                current is XUiC_ItemStackGrid ||
                current is XUiC_BackpackWindow ||
                current is XUiC_VehicleContainer ||
                current is XUiC_ContainerStandardControls)
            {
                return true;
            }

            string typeName = current.GetType().Name;
            if (!string.IsNullOrEmpty(typeName) &&
                (typeName.IndexOf("Loot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 typeName.IndexOf("Container", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 typeName.IndexOf("Backpack", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 typeName.IndexOf("Vehicle", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 typeName.IndexOf("ItemStack", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 typeName.IndexOf("Storage", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    public static bool IsOpenAllActionButtonController(XUiController controller)
    {
        XUiController current = controller;
        while (current != null)
        {
            if (current is XUiC_ItemActionEntry entryController)
            {
                return entryController.ItemActionEntry is ItemActionEntryOpenAll;
            }

            current = current.Parent;
        }

        return false;
    }

    public static bool CanAcceptOpenBundleOutputs(XUiC_ItemStack stackController)
    {
        return CountOpenableByCapacity(stackController, 1) > 0;
    }

    public static int CountOpenableByCapacity(XUiC_ItemStack stackController, int maxChecks)
    {
        if (stackController == null || maxChecks <= 0)
        {
            return 0;
        }

        EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (player == null)
        {
            return maxChecks;
        }

        ItemClass itemClass = stackController.ItemStack.itemValue.ItemClass;
        if (itemClass?.Actions == null)
        {
            return maxChecks;
        }

        for (int actionIndex = 0; actionIndex < itemClass.Actions.Length; actionIndex++)
        {
            if (!(itemClass.Actions[actionIndex] is ItemActionOpenBundle openBundle))
            {
                continue;
            }

            List<ItemStack> outputs = BuildFixedOpenBundleOutputs(openBundle);
            if (outputs.Count == 0)
            {
                return maxChecks;
            }

            List<ItemStack> simulatedSlots = BuildSimulatedPlayerSlots(player);
            int openableCount = 0;
            for (int checkIndex = 0; checkIndex < maxChecks; checkIndex++)
            {
                if (!TryPlaceAllOutputsIntoSimulatedSlots(simulatedSlots, outputs))
                {
                    break;
                }

                openableCount++;
            }

            return openableCount;
        }

        return maxChecks;
    }

    private static bool TryPlaceAllOutputsIntoSimulatedSlots(List<ItemStack> simulatedSlots, List<ItemStack> outputs)
    {
        if (outputs == null || outputs.Count == 0)
        {
            return true;
        }

        for (int outputIndex = 0; outputIndex < outputs.Count; outputIndex++)
        {
            if (!TryPlaceOutputIntoSimulatedSlots(simulatedSlots, outputs[outputIndex]))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsEligibleForOpenAll(XUiC_ItemStack stackController)
    {
        if (stackController == null)
        {
            return false;
        }

        ItemClass itemClass = stackController.ItemStack.itemValue.ItemClass;
        if (itemClass?.Actions == null)
        {
            return false;
        }

        for (int actionIndex = 0; actionIndex < itemClass.Actions.Length; actionIndex++)
        {
            if (!(itemClass.Actions[actionIndex] is ItemActionOpenBundle openBundle))
            {
                continue;
            }

            if (openBundle.CreateItem != null && openBundle.CreateItem.Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    private static List<ItemStack> BuildFixedOpenBundleOutputs(ItemActionOpenBundle openBundle)
    {
        var outputs = new List<ItemStack>();
        if (openBundle == null || openBundle.CreateItem == null || openBundle.CreateItem.Length == 0)
        {
            return outputs;
        }

        for (int index = 0; index < openBundle.CreateItem.Length; index++)
        {
            string itemName = openBundle.CreateItem[index];
            if (string.IsNullOrWhiteSpace(itemName))
            {
                continue;
            }

            ItemValue outputItemValue = ItemClass.GetItem(itemName, true);
            if (outputItemValue.ItemClass == null)
            {
                continue;
            }

            int count = 1;
            if (openBundle.CreateItemCount != null && index < openBundle.CreateItemCount.Length)
            {
                string rawCount = openBundle.CreateItemCount[index];
                if (!string.IsNullOrWhiteSpace(rawCount) && int.TryParse(rawCount, out int parsedCount) && parsedCount > 0)
                {
                    count = parsedCount;
                }
            }

            outputs.Add(new ItemStack(outputItemValue, count));
        }

        return outputs;
    }

    private static List<ItemStack> BuildSimulatedPlayerSlots(EntityPlayerLocal player)
    {
        var simulatedSlots = new List<ItemStack>();

        ItemStack[] bagSlots = player?.bag?.GetSlots();
        if (bagSlots != null)
        {
            for (int index = 0; index < bagSlots.Length; index++)
            {
                simulatedSlots.Add(bagSlots[index]?.Clone());
            }
        }

        ItemStack[] toolbeltSlots = player?.inventory?.GetSlots();
        if (toolbeltSlots != null)
        {
            int publicSlots = player.inventory.PUBLIC_SLOTS;
            if (publicSlots <= 0 || publicSlots > toolbeltSlots.Length)
            {
                publicSlots = toolbeltSlots.Length;
            }

            int dummySlotIndex = player.inventory.DUMMY_SLOT_IDX;
            for (int index = 0; index < publicSlots; index++)
            {
                if (index == dummySlotIndex)
                {
                    continue;
                }

                simulatedSlots.Add(toolbeltSlots[index]?.Clone());
            }
        }

        return simulatedSlots;
    }

    private static bool TryPlaceOutputIntoSimulatedSlots(List<ItemStack> simulatedSlots, ItemStack output)
    {
        if (output == null || output.IsEmpty() || output.count <= 0)
        {
            return true;
        }

        if (simulatedSlots == null)
        {
            return false;
        }

        int remaining = output.count;

        for (int index = 0; index < simulatedSlots.Count && remaining > 0; index++)
        {
            ItemStack slot = simulatedSlots[index];
            if (slot == null || slot.IsEmpty())
            {
                continue;
            }

            if (slot.itemValue.type != output.itemValue.type)
            {
                continue;
            }

            int slotMaxStackSize = GetMaxStackSize(slot.itemValue);
            if (slotMaxStackSize <= 0)
            {
                continue;
            }

            int freeSpace = slotMaxStackSize - slot.count;
            if (freeSpace <= 0)
            {
                continue;
            }

            int transferred = Math.Min(remaining, freeSpace);
            slot.count += transferred;
            remaining -= transferred;
        }

        int maxStackSize = GetMaxStackSize(output.itemValue);
        if (maxStackSize <= 0)
        {
            maxStackSize = remaining;
        }

        while (remaining > 0)
        {
            int emptyIndex = -1;
            for (int index = 0; index < simulatedSlots.Count; index++)
            {
                ItemStack slot = simulatedSlots[index];
                if (slot == null || slot.IsEmpty())
                {
                    emptyIndex = index;
                    break;
                }
            }

            if (emptyIndex < 0)
            {
                return false;
            }

            int placeCount = Math.Min(remaining, maxStackSize);
            simulatedSlots[emptyIndex] = new ItemStack(output.itemValue, placeCount);
            remaining -= placeCount;
        }

        return true;
    }

    private static int GetMaxStackSize(ItemValue itemValue)
    {
        try
        {
            ItemClass itemClass = itemValue.ItemClass;
            if (itemClass == null)
            {
                return -1;
            }

            object stackNumberObject = GetMemberValue(itemClass, "Stacknumber", "StackNumber", "stacknumber", "stackNumber");
            if (TryConvertToPositiveInt(stackNumberObject, out int directStackNumber))
            {
                return directStackNumber;
            }

            if (stackNumberObject != null)
            {
                object nestedValue = GetMemberValue(stackNumberObject, "Value", "value", "Max", "max");
                if (TryConvertToPositiveInt(nestedValue, out int nestedStackNumber))
                {
                    return nestedStackNumber;
                }
            }
        }
        catch
        {
        }

        return -1;
    }

    private static object GetMemberValue(object instance, params string[] memberNames)
    {
        if (instance == null || memberNames == null || memberNames.Length == 0)
        {
            return null;
        }

        Type type = instance.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        for (int index = 0; index < memberNames.Length; index++)
        {
            string memberName = memberNames[index];
            if (string.IsNullOrEmpty(memberName))
            {
                continue;
            }

            PropertyInfo property = type.GetProperty(memberName, flags);
            if (property != null)
            {
                return property.GetValue(instance, null);
            }

            FieldInfo field = type.GetField(memberName, flags);
            if (field != null)
            {
                return field.GetValue(instance);
            }
        }

        return null;
    }

    private static bool TryConvertToPositiveInt(object value, out int result)
    {
        result = -1;
        if (value == null)
        {
            return false;
        }

        try
        {
            int parsed = Convert.ToInt32(value);
            if (parsed > 0)
            {
                result = parsed;
                return true;
            }
        }
        catch
        {
        }

        return false;
    }

    public static string BuildPrefixedIconName(string templateIconName, string iconCore)
    {
        if (string.IsNullOrEmpty(iconCore))
        {
            return templateIconName;
        }

        if (string.IsNullOrEmpty(templateIconName))
        {
            return iconCore;
        }

        int lastUnderscore = templateIconName.LastIndexOf('_');
        if (lastUnderscore <= 0)
        {
            return iconCore;
        }

        string prefix = templateIconName.Substring(0, lastUnderscore + 1);
        return prefix + iconCore;
    }

    public static bool IsInventoryAndToolbeltFull()
    {
        EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (player == null)
        {
            return false;
        }

        bool bagHasSpace = HasAnyEmptyBagSlot(player.bag);
        bool toolbeltHasSpace = HasAnyEmptyToolbeltSlot(player.inventory);
        return !bagHasSpace && !toolbeltHasSpace;
    }

    private static bool HasAnyEmptyBagSlot(Bag bag)
    {
        if (bag == null)
        {
            return false;
        }

        ItemStack[] slots = bag.GetSlots();
        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        for (int index = 0; index < slots.Length; index++)
        {
            ItemStack stack = slots[index];
            if (stack == null || stack.IsEmpty())
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAnyEmptyToolbeltSlot(Inventory inventory)
    {
        if (inventory == null)
        {
            return false;
        }

        ItemStack[] slots = inventory.GetSlots();
        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        int publicSlots = inventory.PUBLIC_SLOTS;
        if (publicSlots <= 0 || publicSlots > slots.Length)
        {
            publicSlots = slots.Length;
        }

        int dummySlotIndex = inventory.DUMMY_SLOT_IDX;
        for (int index = 0; index < publicSlots; index++)
        {
            if (index == dummySlotIndex)
            {
                continue;
            }

            ItemStack stack = slots[index];
            if (stack == null || stack.IsEmpty())
            {
                return true;
            }
        }

        return false;
    }

    private static BaseItemActionEntry FindEntryByActionName(XUiC_ItemActionList actionList, string actionName)
    {
        List<BaseItemActionEntry> entries = GetEntries(actionList);
        if (entries == null || string.IsNullOrEmpty(actionName))
        {
            return null;
        }

        foreach (BaseItemActionEntry entry in entries)
        {
            if (entry == null || entry is ItemActionEntryOpenAll)
            {
                continue;
            }

            if (actionName.Equals(entry.ActionName, StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }

        return null;
    }

    private static List<BaseItemActionEntry> GetEntries(XUiC_ItemActionList actionList)
    {
        if (actionList == null || ItemActionEntriesField == null)
        {
            return null;
        }

        return ItemActionEntriesField.GetValue(actionList) as List<BaseItemActionEntry>;
    }

    public static int GetEntryCount(XUiC_ItemActionList actionList)
    {
        List<BaseItemActionEntry> entries = GetEntries(actionList);
        return entries?.Count ?? 0;
    }

    private static bool HasOpenItemAction(ItemClass itemClass)
    {
        if (itemClass == null || ItemClassActionsProperty == null)
        {
            return false;
        }

        object actionsObject = ItemClassActionsProperty.GetValue(itemClass, null);
        if (!(actionsObject is IEnumerable actionCollection))
        {
            return false;
        }

        foreach (object action in actionCollection)
        {
            if (action == null)
            {
                continue;
            }

            string typeName = action.GetType().Name;
            if (!string.IsNullOrEmpty(typeName) && typeName.IndexOf("Open", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}

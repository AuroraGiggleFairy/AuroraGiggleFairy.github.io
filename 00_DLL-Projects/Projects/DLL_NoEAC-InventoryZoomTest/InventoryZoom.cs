using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

public class InventoryZoomTestInit : IModApi
{
    public void InitMod(Mod _modInstance)
    {
        Log.Out("[InventoryZoomTest] Loaded. Pick a base width in AGF Mods. Ctrl+wheel over the inventory list changes column count.");
        InventoryZoom.LoadSavedPreset();
        new Harmony("agf.inventoryzoom.test").PatchAll();
    }
}

public static class InventoryZoom
{
    const string PrefKey = "AGF.InventoryZoomTest.Preset";
    const int VanillaWindow = 603;
    const int Chrome = 19;

    public struct Preset
    {
        public int Cols;
        public int Cell;
        public int WindowWidth { get { return Cols * Cell + Chrome; } }
    }

    public static readonly Preset[] Presets = new Preset[]
    {
        new Preset { Cols = 8, Cell = 73 },
        new Preset { Cols = 10, Cell = 64 },
        new Preset { Cols = 12, Cell = 56 }
    };

    static readonly int[] Steps = { 4, 6, 8, 10, 12, 16 };
    static readonly Dictionary<XUiView, int> originX = new Dictionary<XUiView, int>();
    static readonly Dictionary<XUiView, int> originW = new Dictionary<XUiView, int>();
    static int stepIndex = -1;
    static int originCols = -1;
    static int originCell = -1;
    public static int PresetIndex { get; private set; }

    public static void LoadSavedPreset()
    {
        int index = Mathf.Clamp(PlayerPrefs.GetInt(PrefKey, 0), 0, Presets.Length - 1);
        ApplyPresetNumbers(index);
    }

    public static void SetPreset(int index, XUi xui)
    {
        index = Mathf.Clamp(index, 0, Presets.Length - 1);
        ApplyPresetNumbers(index);
        PlayerPrefs.SetInt(PrefKey, index);
        PlayerPrefs.Save();
        if (xui == null)
        {
            return;
        }

        ApplyChrome(xui);
        XUiC_Backpack backpack = xui.GetChildByType<XUiC_Backpack>();
        XUiV_Grid grid = backpack != null ? backpack.ViewComponent as XUiV_Grid : null;
        if (grid != null)
        {
            Apply(grid, backpack.Parent != null ? backpack.Parent.ViewComponent as XUiV_ScrollView : null);
        }

        Log.Out("[InventoryZoomTest] Base " + Presets[index].Cols + " columns, cell " + Presets[index].Cell + ".");
    }

    static void ApplyPresetNumbers(int index)
    {
        PresetIndex = index;
        Preset preset = Presets[index];
        originCols = preset.Cols;
        originCell = preset.Cell;
        stepIndex = IndexExact(preset.Cols);
    }

    public static bool TryZoom(XUiV_ScrollView scroll, float delta)
    {
        if (scroll == null || Mathf.Abs(delta) < 0.01f)
        {
            return false;
        }

        XUiC_Backpack backpack = scroll.Controller != null ? scroll.Controller.GetChildByType<XUiC_Backpack>() : null;
        XUiV_Grid grid = backpack != null ? backpack.ViewComponent as XUiV_Grid : null;
        if (grid == null)
        {
            return false;
        }

        Capture(grid);
        if (stepIndex < 0)
        {
            stepIndex = IndexNearest(originCols);
        }

        int direction = delta > 0f ? -1 : 1;
        int next = Mathf.Clamp(stepIndex + direction, 0, Steps.Length - 1);
        if (next == stepIndex)
        {
            return true;
        }

        stepIndex = next;
        Apply(grid, scroll);
        return true;
    }

    public static void Reapply(XUiC_Backpack backpack)
    {
        if (backpack == null)
        {
            return;
        }

        ApplyChrome(backpack.xui);
        if (stepIndex < 0)
        {
            return;
        }

        XUiV_Grid grid = backpack.ViewComponent as XUiV_Grid;
        if (grid == null)
        {
            return;
        }

        Capture(grid);
        Apply(grid, backpack.Parent != null ? backpack.Parent.ViewComponent as XUiV_ScrollView : null);
    }

    public static void KeepScale(XUiV_Grid grid)
    {
        if (grid == null || !(grid.Controller is XUiC_Backpack backpack))
        {
            return;
        }

        ResetGridScale(grid);
        if (stepIndex < 0 || originCell <= 0)
        {
            return;
        }

        ResizeSlots(backpack, CurrentCell(), false);
    }

    static void Capture(XUiV_Grid grid)
    {
        if (originCols > 0)
        {
            return;
        }

        originCols = Mathf.Max(1, grid.Columns);
        originCell = Mathf.Max(1, grid.CellWidth);
    }

    static void Apply(XUiV_Grid grid, XUiV_ScrollView scroll)
    {
        int cols = Steps[stepIndex];
        int cell = CurrentCell();

        grid.Columns = cols;
        grid.CellWidth = cell;
        grid.CellHeight = cell;
        grid.SetDirty();

        UIGrid uiGrid = grid.grid;
        if (uiGrid != null)
        {
            uiGrid.maxPerLine = cols;
            uiGrid.cellWidth = cell;
            uiGrid.cellHeight = cell;
            uiGrid.repositionNow = true;
        }

        ResetGridScale(grid);
        if (grid.Controller is XUiC_Backpack backpack)
        {
            ResizeSlots(backpack, cell, true);
        }

        ScheduleScroll(scroll);

        Log.Out("[InventoryZoomTest] Inventory columns " + cols + ", cell " + cell + ".");
    }

    static XUiV_ScrollView trackedScroll;
    static int refreshLeft;

    static void ScheduleScroll(XUiV_ScrollView scroll)
    {
        if (scroll == null)
        {
            return;
        }

        trackedScroll = scroll;
        refreshLeft = 3;
    }

    public static void TickScroll(XUiV_ScrollView scroll)
    {
        if (refreshLeft <= 0 || scroll == null || scroll != trackedScroll)
        {
            return;
        }

        refreshLeft--;
        if (refreshLeft <= 1)
        {
            SyncScrollbar(scroll);
        }
    }

    public static bool ContentFits(XUiV_ScrollView scroll)
    {
        if (stepIndex < 0 || scroll == null || scroll.scrollView == null)
        {
            return false;
        }

        if (scroll.Controller == null || scroll.Controller.GetChildByType<XUiC_Backpack>() == null)
        {
            return false;
        }

        return !scroll.scrollView.shouldMoveVertically;
    }

    static void SyncScrollbar(XUiV_ScrollView scroll)
    {
        UIScrollView view = scroll.scrollView;
        if (view == null)
        {
            return;
        }

        view.showScrollBars = UIScrollView.ShowCondition.Always;
        if (view.verticalScrollBar != null)
        {
            view.verticalScrollBar.alpha = 1f;
        }

        view.UpdateScrollbars(true);
        view.RestrictWithinBounds(true);
        view.UpdateScrollbars(true);
        scroll.UpdateScrollbarsRequired();
    }

    static int CurrentCell()
    {
        int cols = Steps[stepIndex];
        return Mathf.Max(1, (originCols * originCell) / cols);
    }

    const int TemplateCell = 73;
    static int sizedCell = -1;

    static void ResetGridScale(XUiV_Grid grid)
    {
        Transform root = grid.UiTransform;
        if (root == null)
        {
            return;
        }

        Vector3 one = new Vector3(1f, 1f, 1f);
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.localScale != one)
            {
                child.localScale = one;
            }
        }
    }

    static void ResizeSlots(XUiC_Backpack backpack, int cell, bool force)
    {
        XUiC_ItemStack[] slots = backpack.GetItemStackControllers();
        if (slots == null || slots.Length == 0)
        {
            return;
        }

        if (!force && sizedCell == cell && slots[0].ViewComponent != null && slots[0].ViewComponent.Size.x == cell)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            ResizeSlot(slots[i], cell);
        }

        sizedCell = cell;
    }

    static void ResizeSlot(XUiC_ItemStack slot, int cell)
    {
        if (slot == null)
        {
            return;
        }

        SetSize(slot.ViewComponent, cell, cell);
        int half = cell / 2;
        int barY = -(4 * cell / 5);
        int countY = -(3 * cell / 5);
        int barW = cell - 5;
        int font = Scaled(26, cell);
        int timerFont = Scaled(24, cell);
        int badge = Scaled(24, cell);

        Visit(slot, delegate (XUiController node)
        {
            XUiView view = node.ViewComponent;
            if (view == null || view == slot.ViewComponent)
            {
                return;
            }

            string id = view.ID;
            if (id == "backgroundMain")
            {
                SetSize(view, cell + 3, cell + 3);
            }
            else if (id == "background" || id == "backgroundTexture")
            {
                SetSize(view, cell - 2, cell - 2);
            }
            else if (id == "highlightOverlay" || id == "slotLockOverlayFill" || id == "slotLockOverlayBorder" || id == "iconSlotLockFill" || id == "iconSlotLockBorder")
            {
                SetSize(view, cell - 3, cell - 3);
            }
            else if (id == "itemIcon")
            {
                SetSize(view, cell - 11, cell - 11);
                SetPos(view, half, -half);
            }
            else if (id == "durabilityBackground" || id == "durability")
            {
                SetSize(view, barW, Scaled(10, cell));
                SetPos(view, 1, barY);
            }
            else if (id == "stackValue")
            {
                SetSize(view, barW, Scaled(28, cell));
                SetPos(view, -2, countY);
                SetFont(view, font);
            }
            else if (id == "timer")
            {
                SetSize(view, barW, Scaled(24, cell));
                SetPos(view, half, -half);
                SetFont(view, timerFont);
            }
            else if (id == "cancel")
            {
                SetSize(view, barW, barW);
            }
            else if (id == "assembleIcon")
            {
                SetSize(view, cell - 35, cell - 35);
            }
            else if (id == "rectSlotLock")
            {
                SetSize(view, cell - 2, cell - 2);
            }
            else if (id == "favoriteIcon" || id == "lockTypeIcon" || id == "itemtypeicon")
            {
                SetSize(view, badge, badge);
            }
        });
    }

    static int Scaled(int authored, int cell)
    {
        return Mathf.Max(1, Mathf.RoundToInt(authored * cell / (float)TemplateCell));
    }

    static void Visit(XUiController node, System.Action<XUiController> visit)
    {
        if (node == null)
        {
            return;
        }

        visit(node);
        if (node.Children == null)
        {
            return;
        }

        foreach (XUiController child in node.Children)
        {
            Visit(child, visit);
        }
    }

    static void SetSize(XUiView view, int width, int height)
    {
        if (view == null)
        {
            return;
        }

        view.Size = new Vector2i(Mathf.Max(1, width), Mathf.Max(1, height));
        view.SetDirty();
    }

    static void SetPos(XUiView view, int x, int y)
    {
        if (view == null)
        {
            return;
        }

        view.Position = new Vector2i(x, y);
        view.SetDirty();
    }

    static void SetFont(XUiView view, int size)
    {
        if (view == null || view.UiTransform == null)
        {
            return;
        }

        UILabel label = view.UiTransform.GetComponent<UILabel>();
        if (label != null && label.fontSize != size)
        {
            label.fontSize = size;
        }
    }

    static int IndexNearest(int cols)
    {
        int best = 0;
        int bestDist = int.MaxValue;
        for (int i = 0; i < Steps.Length; i++)
        {
            int dist = Mathf.Abs(Steps[i] - cols);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }

        return best;
    }

    static int IndexExact(int cols)
    {
        for (int i = 0; i < Steps.Length; i++)
        {
            if (Steps[i] == cols)
            {
                return i;
            }
        }

        return IndexNearest(cols);
    }

    static void ApplyChrome(XUi xui)
    {
        if (xui == null || originCols <= 0)
        {
            return;
        }

        int delta = (originCols * originCell + Chrome) - VanillaWindow;
        XUiC_BackpackWindow backpackWindow = xui.GetChildByType<XUiC_BackpackWindow>();
        if (backpackWindow != null)
        {
            GrowView(backpackWindow.ViewComponent, delta);
            ShiftDirect(backpackWindow.GetChildById("header"), 280, delta);
            XUiController content = backpackWindow.GetChildById("content");
            if (content != null)
            {
                ShiftDirect(content, 400, delta);
            }

            XUiC_Backpack backpack = backpackWindow.GetChildByType<XUiC_Backpack>();
            XUiV_ScrollView scroll = backpack != null && backpack.Parent != null ? backpack.Parent.ViewComponent as XUiV_ScrollView : null;
            GrowView(scroll, delta);
        }

        GrowTree(xui.GetChildByType<XUiC_ItemInfoWindow>(), delta, 540);
        GrowTree(xui.GetChildByType<XUiC_EmptyInfoWindow>(), delta, 540);
        ShiftDirect(xui.GetChildByType<XUiC_ItemInfoWindow>() != null ? xui.GetChildByType<XUiC_ItemInfoWindow>().GetChildById("header") : null, 400, delta);
    }

    static void ShiftDirect(XUiController parent, int minX, int delta)
    {
        if (parent == null || parent.Children == null)
        {
            return;
        }

        foreach (XUiController child in parent.Children)
        {
            XUiView view = child.ViewComponent;
            if (view == null)
            {
                continue;
            }

            if (!originX.ContainsKey(view))
            {
                originX[view] = view.Position.x;
            }

            if (originX[view] < minX)
            {
                continue;
            }

            Vector2i pos = view.Position;
            view.Position = new Vector2i(originX[view] + delta, pos.y);
            view.SetDirty();
        }
    }

    static void GrowTree(XUiController root, int delta, int minWidth)
    {
        if (root == null)
        {
            return;
        }

        GrowIfWide(root.ViewComponent, delta, minWidth);
        if (root.Children == null)
        {
            return;
        }

        foreach (XUiController child in root.Children)
        {
            GrowTree(child, delta, minWidth);
        }
    }

    static void GrowIfWide(XUiView view, int delta, int minWidth)
    {
        if (view == null)
        {
            return;
        }

        if (!originW.ContainsKey(view))
        {
            if (view.Size.x <= 0)
            {
                return;
            }

            originW[view] = view.Size.x;
        }

        if (originW[view] < minWidth)
        {
            return;
        }

        view.Size = new Vector2i(originW[view] + delta, view.Size.y);
        view.SetDirty();
    }

    static void GrowView(XUiView view, int delta)
    {
        if (view == null)
        {
            return;
        }

        if (!originW.ContainsKey(view))
        {
            if (view.Size.x <= 0)
            {
                return;
            }

            originW[view] = view.Size.x;
        }

        view.Size = new Vector2i(originW[view] + delta, view.Size.y);
        view.SetDirty();
    }
}

[HarmonyPatch(typeof(XUiV_ScrollView), "OnScrolled")]
public static class InventoryZoom_OnScrolled
{
    static bool Prefix(XUiV_ScrollView __instance, float _delta)
    {
        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
        {
            return !InventoryZoom.ContentFits(__instance);
        }

        return !InventoryZoom.TryZoom(__instance, _delta);
    }
}

[HarmonyPatch(typeof(XUiC_Backpack), "OnOpen")]
public static class InventoryZoom_OnOpen
{
    static void Postfix(XUiC_Backpack __instance)
    {
        InventoryZoom.Reapply(__instance);
    }
}

[HarmonyPatch(typeof(XUiV_ScrollView), "Update")]
public static class InventoryZoom_ScrollUpdate
{
    static void Postfix(XUiV_ScrollView __instance)
    {
        InventoryZoom.TickScroll(__instance);
    }
}

[HarmonyPatch(typeof(XUiV_Grid), "Update")]
public static class InventoryZoom_GridUpdate
{
    static void Postfix(XUiV_Grid __instance)
    {
        InventoryZoom.KeepScale(__instance);
    }
}

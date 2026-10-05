using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;

/// <summary>
/// Doom HUD draws health, armor, food, water, and the belt on windowToolbelt.
/// Vanilla hides that whole window in a vehicle and on death.
/// Turning the window off and back on in the same frame leaves NGUI with nothing to draw.
/// The visibility write itself is changed so the window stays up.
/// On death only the dead face stays.
/// The hook is installed only when FranticDansCleanerHUD1080 is loaded.
/// </summary>
public static class Patch_DoomToolbeltWindow_Update
{
    const string FaceId = "DoomGuyFace";
    const string DeadFaceSprite = "doomguy_0";

    static readonly ConditionalWeakTable<XUiC_ToolbeltWindow, State> States = new ConditionalWeakTable<XUiC_ToolbeltWindow, State>();
    static bool installed;

    sealed class State
    {
        public bool WasDead;
        public bool FaceResolved;
        public XUiController Face;
    }

    public static void TryInstall(Harmony harmony)
    {
        if (installed)
        {
            return;
        }

        if (!DoomHudGate.IsLoaded)
        {
            return;
        }

        MethodInfo update = AccessTools.Method(typeof(XUiC_ToolbeltWindow), nameof(XUiC_ToolbeltWindow.Update));
        harmony.Patch(
            update,
            transpiler: new HarmonyMethod(typeof(Patch_DoomToolbeltWindow_Update), nameof(Transpiler)),
            postfix: new HarmonyMethod(typeof(Patch_DoomToolbeltWindow_Update), nameof(Postfix)));
        installed = true;
        Logging.Inform("Doom toolbelt visibility patch installed.");
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        MethodInfo setVisible = AccessTools.PropertySetter(typeof(XUiView), nameof(XUiView.IsVisible));
        MethodInfo adjust = AccessTools.Method(typeof(Patch_DoomToolbeltWindow_Update), nameof(AdjustVisible));
        var matcher = new CodeMatcher(instructions, generator);
        matcher.MatchStartForward(new CodeMatch(OpCodes.Callvirt, setVisible));
        if (!matcher.IsValid)
        {
            Logging.Error("Doom toolbelt patch did not find the visibility write. Vehicle and death HUD stay vanilla.");
            return instructions;
        }

        LocalBuilder local = generator.DeclareLocal(typeof(bool));
        CodeInstruction store = new CodeInstruction(OpCodes.Stloc, local);
        store.labels.AddRange(matcher.Instruction.labels);
        store.blocks.AddRange(matcher.Instruction.blocks);
        matcher.Instruction.labels.Clear();
        matcher.Instruction.blocks.Clear();
        matcher.Insert(new[]
        {
            store,
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldloc, local),
            new CodeInstruction(OpCodes.Call, adjust),
        });
        return matcher.InstructionEnumeration();
    }

    /// <summary>
    /// vanilla already decided. Doom windows stay up in a vehicle and on death.
    /// </summary>
    public static bool AdjustVisible(XUiC_ToolbeltWindow window, bool vanilla)
    {
        if (window == null || !TryGetFace(window, out _))
        {
            return vanilla;
        }

        EntityPlayerLocal player = window.xui?.playerUI?.entityPlayer;
        if (player == null)
        {
            return vanilla;
        }

        if (player.IsDead())
        {
            return true;
        }

        if (player.AttachedToEntity is EntityVehicle)
        {
            return HudAllowsToolbelt(window);
        }

        return vanilla;
    }

    [HarmonyPostfix]
    public static void Postfix(XUiC_ToolbeltWindow __instance)
    {
        if (!TryGetFace(__instance, out XUiController face))
        {
            return;
        }

        EntityPlayerLocal player = __instance.xui?.playerUI?.entityPlayer;
        if (player == null)
        {
            return;
        }

        State state = States.GetOrCreateValue(__instance);
        if (player.IsDead())
        {
            ShowDeadFace(__instance, face);
            state.WasDead = true;
            return;
        }

        if (state.WasDead)
        {
            ShowAll(__instance);
            state.WasDead = false;
        }
    }

    static bool TryGetFace(XUiC_ToolbeltWindow window, out XUiController face)
    {
        State state = States.GetOrCreateValue(window);
        if (!state.FaceResolved)
        {
            if (window.Children == null || window.Children.Count == 0)
            {
                face = null;
                return false;
            }

            state.Face = window.GetChildById(FaceId);
            state.FaceResolved = true;
        }

        face = state.Face;
        return face != null;
    }

    static bool HudAllowsToolbelt(XUiC_ToolbeltWindow window)
    {
        GUIWindowManager windowManager = window.xui?.playerUI?.windowManager;
        if (windowManager == null)
        {
            return false;
        }

        XUiC_DragAndDropWindow drag = window.xui.DragAndDropWindow;
        return windowManager.IsHUDEnabled() || (drag != null && drag.InMenu && windowManager.IsHUDPartialHidden());
    }

    static void ShowDeadFace(XUiController window, XUiController face)
    {
        var keep = new HashSet<XUiController>();
        for (XUiController node = face; node != null; node = node.Parent)
        {
            keep.Add(node);
            if (node == window)
            {
                break;
            }
        }

        HideExcept(window, keep);
        foreach (XUiController node in keep)
        {
            if (node.ViewComponent != null && !node.ViewComponent.IsVisible)
            {
                node.ViewComponent.IsVisible = true;
            }
        }

        if (face.ViewComponent is XUiV_Sprite sprite && sprite.SpriteName != DeadFaceSprite)
        {
            sprite.SpriteName = DeadFaceSprite;
        }
    }

    static void HideExcept(XUiController node, HashSet<XUiController> keep)
    {
        if (node.ViewComponent != null && !keep.Contains(node) && node.ViewComponent.IsVisible)
        {
            node.ViewComponent.IsVisible = false;
        }

        List<XUiController> children = node.Children;
        for (int i = 0; i < children.Count; i++)
        {
            HideExcept(children[i], keep);
        }
    }

    static void ShowAll(XUiController node)
    {
        string id = node.ViewComponent != null ? node.ViewComponent.ID : null;
        bool selfManaged = id != null && (id.Equals("sprint", StringComparison.OrdinalIgnoreCase) || id.Equals("autorun", StringComparison.OrdinalIgnoreCase));
        if (node.ViewComponent != null && !selfManaged && !node.ViewComponent.IsVisible)
        {
            node.ViewComponent.IsVisible = true;
        }

        List<XUiController> children = node.Children;
        for (int i = 0; i < children.Count; i++)
        {
            ShowAll(children[i]);
        }
    }
}

using System;
using System.Collections.Generic;

public class XUiC_EnhancedScreamerAlertOptions : XUiController
{
    private static readonly List<XUiC_EnhancedScreamerAlertOptions> LiveControllers = new List<XUiC_EnhancedScreamerAlertOptions>();
    private static readonly bool LocalEnhancedAgfPresent = DetectLocalEnhancedAgf();
    private static bool ServerCountCapabilityKnown;
    private static bool ServerCountCapabilityAvailable;

    private bool pendingRefresh;
    private bool buttonHandlersBound;
    private XUiC_SimpleButton btnScreamerOff;
    private XUiC_SimpleButton btnScreamerOn;
    private XUiC_SimpleButton btnScreamerNum;
    private ScreamerAlertMode cachedMode = ScreamerAlertMode.OnWithNumbers;

    public override void Init()
    {
        base.Init();
        RegisterInstance(this);
        EnsureButtonHandlers();
        pendingRefresh = true;
    }

    public override void OnOpen()
    {
        base.OnOpen();
        EnsureButtonHandlers();
        pendingRefresh = true;
        RequestAuthoritativeModeRefresh();
    }

    public override void OnClose()
    {
        base.OnClose();
        RemoveButtonHandlers();
        UnregisterInstance(this);
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
        if (pendingRefresh)
        {
            pendingRefresh = false;
            RefreshModeCache();
            RefreshBindingsSelfAndChildren();
            ApplySelectedState();
        }
    }

    public override bool GetBindingValueInternal(ref string value, string bindingName)
    {
        if (string.IsNullOrEmpty(bindingName))
        {
            return base.GetBindingValueInternal(ref value, bindingName);
        }

        string normalized = bindingName.ToLowerInvariant();
        switch (normalized)
        {
            case "opt_row_1_visible":
                value = "true";
                return true;
            case "opt_row_1_label":
                value = "Screamer Alert";
                return true;
            case "opt_row_1_value":
                value = ScreamerAlertModeSettings.GetModeLabel(cachedMode);
                return true;
            case "opt_row_1_offtext":
            case "opt_row_1_ontext":
            case "opt_row_1_numtext":
                value = string.Empty;
                return true;
            case "opt_row_1_num_visible":
                value = IsCountModeAvailable().ToString().ToLowerInvariant();
                return true;
            case "opt_row_1_off_selected_visible":
                value = cachedMode == ScreamerAlertMode.Off ? "true" : "false";
                return true;
            case "opt_row_1_on_selected_visible":
                value = cachedMode == ScreamerAlertMode.On ? "true" : "false";
                return true;
            case "opt_row_1_num_selected_visible":
                value = IsCountModeAvailable() && cachedMode == ScreamerAlertMode.OnWithNumbers ? "true" : "false";
                return true;
            default:
                return base.GetBindingValueInternal(ref value, bindingName);
        }
    }

    public static void OnAuthoritativeModeAck(ScreamerAlertMode mode, bool countAvailable)
    {
        ServerCountCapabilityKnown = true;
        ServerCountCapabilityAvailable = countAvailable;

        ScreamerAlertMode effectiveMode = NormalizeModeForCapability(mode, countAvailable);
        ScreamerAlertModeSettings.SetModeForLocalPlayer(effectiveMode);

        EntityPlayer localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (localPlayer != null)
        {
            ScreamerAlertModeSettings.SetModeForLocalPlayer(effectiveMode);
        }

        for (int i = LiveControllers.Count - 1; i >= 0; i--)
        {
            XUiC_EnhancedScreamerAlertOptions controller = LiveControllers[i];
            if (controller == null)
            {
                LiveControllers.RemoveAt(i);
                continue;
            }

            controller.cachedMode = effectiveMode;
            controller.pendingRefresh = true;
            if (controller.ViewComponent != null && controller.ViewComponent.IsVisible)
            {
                controller.RefreshBindingsSelfAndChildren();
                controller.ApplySelectedState();
            }
        }

        XUiC_EnhancedScreamerAlerts.Instance?.RefreshBindingsSelfAndChildren();
    }

    public static void MarkAllDirty()
    {
        for (int i = LiveControllers.Count - 1; i >= 0; i--)
        {
            XUiC_EnhancedScreamerAlertOptions controller = LiveControllers[i];
            if (controller == null)
            {
                LiveControllers.RemoveAt(i);
                continue;
            }

            controller.pendingRefresh = true;
            if (controller.ViewComponent != null && controller.ViewComponent.IsVisible)
            {
                controller.RefreshModeCache();
                controller.RefreshBindingsSelfAndChildren();
                controller.ApplySelectedState();
            }
        }
    }

    private static void RegisterInstance(XUiC_EnhancedScreamerAlertOptions controller)
    {
        if (controller == null)
        {
            return;
        }

        if (!LiveControllers.Contains(controller))
        {
            LiveControllers.Add(controller);
        }
    }

    private static void UnregisterInstance(XUiC_EnhancedScreamerAlertOptions controller)
    {
        if (controller == null)
        {
            return;
        }

        LiveControllers.Remove(controller);
    }

    private void EnsureButtonHandlers()
    {
        if (buttonHandlersBound)
        {
            return;
        }

        btnScreamerOff = GetChildById("btnScreamerOff") as XUiC_SimpleButton;
        btnScreamerOn = GetChildById("btnScreamerOn") as XUiC_SimpleButton;
        btnScreamerNum = GetChildById("btnScreamerNum") as XUiC_SimpleButton;

        bool anyBound = false;

        if (btnScreamerOff != null)
        {
            btnScreamerOff.OnPressed -= BtnScreamerOff_OnPressed;
            btnScreamerOff.OnPressed += BtnScreamerOff_OnPressed;
            anyBound = true;
        }

        if (btnScreamerOn != null)
        {
            btnScreamerOn.OnPressed -= BtnScreamerOn_OnPressed;
            btnScreamerOn.OnPressed += BtnScreamerOn_OnPressed;
            anyBound = true;
        }

        if (btnScreamerNum != null)
        {
            btnScreamerNum.OnPressed -= BtnScreamerNum_OnPressed;
            btnScreamerNum.OnPressed += BtnScreamerNum_OnPressed;
            anyBound = true;
        }

        if (!anyBound)
        {
            Console.WriteLine("[ScreamerAlert] No ESC options buttons found for controller wiring.");
            return;
        }

        buttonHandlersBound = true;
        ApplySelectedState();
    }

    private void RemoveButtonHandlers()
    {
        if (!buttonHandlersBound)
        {
            return;
        }

        if (btnScreamerOff != null)
        {
            btnScreamerOff.OnPressed -= BtnScreamerOff_OnPressed;
        }

        if (btnScreamerOn != null)
        {
            btnScreamerOn.OnPressed -= BtnScreamerOn_OnPressed;
        }

        if (btnScreamerNum != null)
        {
            btnScreamerNum.OnPressed -= BtnScreamerNum_OnPressed;
        }

        btnScreamerOff = null;
        btnScreamerOn = null;
        btnScreamerNum = null;
        buttonHandlersBound = false;
    }

    private void BtnScreamerOff_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetModeAndRefresh(ScreamerAlertMode.Off);
    }

    private void BtnScreamerOn_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetModeAndRefresh(ScreamerAlertMode.On);
    }

    private void BtnScreamerNum_OnPressed(XUiController _sender, int _mouseButton)
    {
        if (!IsCountModeAvailable())
        {
            return;
        }

        SetModeAndRefresh(ScreamerAlertMode.OnWithNumbers);
    }

    private void SetModeAndRefresh(ScreamerAlertMode mode)
    {
        ScreamerAlertMode requestedMode = NormalizeModeForCapability(mode, IsCountModeAvailable());

        // Update button selection immediately so users get instant UI feedback.
        cachedMode = requestedMode;
        RefreshBindingsSelfAndChildren();
        ApplySelectedState();
        XUiC_EnhancedScreamerAlerts.Instance?.RefreshBindingsSelfAndChildren();

        if (TryRequestServerModeChange(requestedMode))
        {
            return;
        }

        bool changed = ScreamerAlertModeSettings.SetModeForLocalPlayer(requestedMode);
        if (!changed)
        {
            EntityPlayer player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player != null)
            {
                changed = ScreamerAlertModeSettings.SetModeForLocalPlayer(requestedMode);
            }
        }

        if (!changed)
        {
            Console.WriteLine("[ScreamerAlert] Failed to set local mode from ESC options.");
            return;
        }

        cachedMode = requestedMode;
        RefreshBindingsSelfAndChildren();
        ApplySelectedState();
        XUiC_EnhancedScreamerAlerts.Instance?.RefreshBindingsSelfAndChildren();
        MarkAllDirty();
    }

    private bool TryRequestServerModeChange(ScreamerAlertMode requestedMode)
    {
        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        EntityPlayer localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (manager == null || localPlayer == null || localPlayer.entityId < 0)
        {
            return false;
        }

        if (manager.IsServer)
        {
            bool countAvailable = DetermineServerCountAvailability(localPlayer.entityId);
            ScreamerAlertMode effectiveMode = NormalizeModeForCapability(requestedMode, countAvailable);
            bool changed = ScreamerAlertModeSettings.SetModeForLocalPlayer(effectiveMode);
            if (!changed)
            {
                return false;
            }

            OnAuthoritativeModeAck(effectiveMode, countAvailable);
            return true;
        }

        SendModeCommand(localPlayer.entityId, requestedMode);
        return true;
    }

    private void RequestAuthoritativeModeRefresh()
    {
        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        EntityPlayer localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (manager == null || localPlayer == null || localPlayer.entityId < 0)
        {
            return;
        }

        if (manager.IsServer)
        {
            bool countAvailable = DetermineServerCountAvailability(localPlayer.entityId);
            ServerCountCapabilityKnown = true;
            ServerCountCapabilityAvailable = countAvailable;
            pendingRefresh = true;
            MarkAllDirty();
            return;
        }

        SendModeCommand(localPlayer.entityId, null);
    }

    private static void SendModeCommand(int entityId, ScreamerAlertMode? mode)
    {
        string token = !mode.HasValue
            ? "status"
            : mode.Value == ScreamerAlertMode.Off
                ? "off"
                : mode.Value == ScreamerAlertMode.OnWithNumbers ? "count" : "on";

        GameManager.Instance?.ChatMessageServer(null, EChatType.Global, entityId, "/agfsa " + token, null, EMessageSender.SenderIdAsPlayer);
    }

    private void RefreshModeCache()
    {
        bool countAvailable = IsCountModeAvailable();
        ScreamerAlertMode fallback = countAvailable ? ScreamerAlertMode.OnWithNumbers : ScreamerAlertMode.On;
        cachedMode = NormalizeModeForCapability(ScreamerAlertModeSettings.GetModeForLocalPlayer(fallback), countAvailable);
    }

    private void ApplySelectedState()
    {
        SetButtonSelected(btnScreamerOff, cachedMode == ScreamerAlertMode.Off);
        SetButtonSelected(btnScreamerOn, cachedMode == ScreamerAlertMode.On);
        SetButtonSelected(btnScreamerNum, cachedMode == ScreamerAlertMode.OnWithNumbers);
    }

    private static void SetButtonSelected(XUiC_SimpleButton button, bool isSelected)
    {
        if (button?.Button != null)
        {
            button.Button.Selected = isSelected;
        }
    }

    private static bool DetermineServerCountAvailability(int entityId)
    {
        _ = entityId;
        bool available = LocalEnhancedAgfPresent && (ScreamerAlertEnhancedState.Available || !GameManager.IsDedicatedServer);
        ServerCountCapabilityKnown = true;
        ServerCountCapabilityAvailable = available;
        return available;
    }

    private static bool IsCountModeAvailable()
    {
        return LocalEnhancedAgfPresent
            && ServerCountCapabilityKnown
            && ServerCountCapabilityAvailable;
    }

    private static ScreamerAlertMode NormalizeModeForCapability(ScreamerAlertMode mode, bool countAvailable)
    {
        if (!countAvailable && mode == ScreamerAlertMode.OnWithNumbers)
        {
            return ScreamerAlertMode.On;
        }

        return mode;
    }

    private static bool DetectLocalEnhancedAgf()
    {
        try
        {
            AppDomain domain = AppDomain.CurrentDomain;
            if (domain == null)
            {
                return false;
            }

            foreach (var assembly in domain.GetAssemblies())
            {
                if (assembly == null)
                {
                    continue;
                }

                string assemblyName = assembly.GetName()?.Name;
                if (!string.IsNullOrEmpty(assemblyName) && assemblyName.IndexOf("EnhancedAGF", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                if (assembly.GetType("ScreamerAlertEnhancedGate", throwOnError: false) != null)
                {
                    return true;
                }
            }
        }
        catch
        {
        }

        return false;
    }
}
using System;
using System.Collections.Generic;

public class XUiC_ScreamerAlertOptions : XUiController
{
    private static readonly List<XUiC_ScreamerAlertOptions> LiveControllers = new List<XUiC_ScreamerAlertOptions>();

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
            case "opt_row_1_off_selected_visible":
                value = cachedMode == ScreamerAlertMode.Off ? "true" : "false";
                return true;
            case "opt_row_1_on_selected_visible":
                value = cachedMode == ScreamerAlertMode.On ? "true" : "false";
                return true;
            case "opt_row_1_num_selected_visible":
                value = cachedMode == ScreamerAlertMode.OnWithNumbers ? "true" : "false";
                return true;
            default:
                return base.GetBindingValueInternal(ref value, bindingName);
        }
    }

    public static void MarkAllDirty()
    {
        for (int i = LiveControllers.Count - 1; i >= 0; i--)
        {
            XUiC_ScreamerAlertOptions controller = LiveControllers[i];
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

    private static void RegisterInstance(XUiC_ScreamerAlertOptions controller)
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

    private static void UnregisterInstance(XUiC_ScreamerAlertOptions controller)
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
        SetModeAndRefresh(ScreamerAlertMode.OnWithNumbers);
    }

    private void SetModeAndRefresh(ScreamerAlertMode mode)
    {
        bool changed = ScreamerAlertModeSettings.SetModeForLocalPlayer(mode);
        if (!changed)
        {
            EntityPlayer player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player != null)
            {
                changed = ScreamerAlertModeSettings.SetModeForEntityId(player.entityId, mode);
            }
        }

        if (!changed)
        {
            Console.WriteLine("[ScreamerAlert] Failed to set local mode from ESC options.");
            return;
        }

        cachedMode = mode;
        RefreshBindingsSelfAndChildren();
        ApplySelectedState();
        XUiC_ScreamerAlerts.Instance?.RefreshBindingsSelfAndChildren();
        MarkAllDirty();
    }

    private void RefreshModeCache()
    {
        cachedMode = ScreamerAlertModeSettings.GetModeForLocalPlayer(ScreamerAlertMode.OnWithNumbers);
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
}
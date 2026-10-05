using System;

public class XUiC_EnhancedVetOptions : XUiController
{
    private bool pendingRefresh;
    private bool buttonHandlersBound;
    private bool cachedOn = true;
    private XUiC_SimpleButton btnVetOff;
    private XUiC_SimpleButton btnVetOn;

    public override void Init()
    {
        base.Init();
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
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
        if (!pendingRefresh)
        {
            return;
        }

        pendingRefresh = false;
        RefreshBindingsSelfAndChildren();
        ApplySelectedState();
    }

    public override bool GetBindingValueInternal(ref string value, string bindingName)
    {
        if (string.IsNullOrEmpty(bindingName))
        {
            return base.GetBindingValueInternal(ref value, bindingName);
        }

        switch (bindingName.ToLowerInvariant())
        {
            case "vet_off_selected_visible":
                value = cachedOn ? "false" : "true";
                return true;
            case "vet_on_selected_visible":
                value = cachedOn ? "true" : "false";
                return true;
            default:
                return base.GetBindingValueInternal(ref value, bindingName);
        }
    }

    private void EnsureButtonHandlers()
    {
        if (buttonHandlersBound)
        {
            return;
        }

        btnVetOff = GetChildById("btnVetOff") as XUiC_SimpleButton;
        btnVetOn = GetChildById("btnVetOn") as XUiC_SimpleButton;

        bool anyBound = false;
        if (btnVetOff != null)
        {
            btnVetOff.OnPressed -= BtnVetOff_OnPressed;
            btnVetOff.OnPressed += BtnVetOff_OnPressed;
            anyBound = true;
        }

        if (btnVetOn != null)
        {
            btnVetOn.OnPressed -= BtnVetOn_OnPressed;
            btnVetOn.OnPressed += BtnVetOn_OnPressed;
            anyBound = true;
        }

        if (!anyBound)
        {
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

        if (btnVetOff != null)
        {
            btnVetOff.OnPressed -= BtnVetOff_OnPressed;
        }

        if (btnVetOn != null)
        {
            btnVetOn.OnPressed -= BtnVetOn_OnPressed;
        }

        btnVetOff = null;
        btnVetOn = null;
        buttonHandlersBound = false;
    }

    private void BtnVetOff_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetModeAndRefresh(false);
    }

    private void BtnVetOn_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetModeAndRefresh(true);
    }

    private void SetModeAndRefresh(bool on)
    {
        cachedOn = on;
        RefreshBindingsSelfAndChildren();
        ApplySelectedState();
        SendModeCommand(on ? "on" : "off");
    }

    private static void SendModeCommand(string token)
    {
        EntityPlayer localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (localPlayer == null || localPlayer.entityId < 0)
        {
            return;
        }

        GameManager.Instance?.ChatMessageServer(null, EChatType.Global, localPlayer.entityId, "/agfet " + token + " quiet", null, EMessageSender.SenderIdAsPlayer);
    }

    private void ApplySelectedState()
    {
        SetButtonSelected(btnVetOff, !cachedOn);
        SetButtonSelected(btnVetOn, cachedOn);
    }

    private static void SetButtonSelected(XUiC_SimpleButton button, bool isSelected)
    {
        if (button?.Button != null)
        {
            button.Button.Selected = isSelected;
        }
    }
}

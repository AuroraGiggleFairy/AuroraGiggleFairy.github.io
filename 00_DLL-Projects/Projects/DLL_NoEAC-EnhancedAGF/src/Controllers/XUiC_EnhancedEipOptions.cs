using ExpandedInteractionPrompts;

public class XUiC_EnhancedEipOptions : XUiController
{
    private bool pendingRefresh;
    private bool buttonHandlersBound;
    private int cachedMode = PromptHudMode.Full;
    private XUiC_SimpleButton btnEipOff;
    private XUiC_SimpleButton btnEipPartial;
    private XUiC_SimpleButton btnEipFull;

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
        cachedMode = PromptHudMode.Current;
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
        cachedMode = PromptHudMode.Current;
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
            case "eip_off_selected_visible":
                value = cachedMode == PromptHudMode.Off ? "true" : "false";
                return true;
            case "eip_partial_selected_visible":
                value = cachedMode == PromptHudMode.Partial ? "true" : "false";
                return true;
            case "eip_full_selected_visible":
                value = cachedMode == PromptHudMode.Full ? "true" : "false";
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

        btnEipOff = GetChildById("btnEipOff") as XUiC_SimpleButton;
        btnEipPartial = GetChildById("btnEipPartial") as XUiC_SimpleButton;
        btnEipFull = GetChildById("btnEipFull") as XUiC_SimpleButton;

        bool anyBound = false;
        if (btnEipOff != null)
        {
            btnEipOff.OnPressed -= BtnEipOff_OnPressed;
            btnEipOff.OnPressed += BtnEipOff_OnPressed;
            anyBound = true;
        }

        if (btnEipPartial != null)
        {
            btnEipPartial.OnPressed -= BtnEipPartial_OnPressed;
            btnEipPartial.OnPressed += BtnEipPartial_OnPressed;
            anyBound = true;
        }

        if (btnEipFull != null)
        {
            btnEipFull.OnPressed -= BtnEipFull_OnPressed;
            btnEipFull.OnPressed += BtnEipFull_OnPressed;
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

        if (btnEipOff != null)
        {
            btnEipOff.OnPressed -= BtnEipOff_OnPressed;
        }

        if (btnEipPartial != null)
        {
            btnEipPartial.OnPressed -= BtnEipPartial_OnPressed;
        }

        if (btnEipFull != null)
        {
            btnEipFull.OnPressed -= BtnEipFull_OnPressed;
        }

        btnEipOff = null;
        btnEipPartial = null;
        btnEipFull = null;
        buttonHandlersBound = false;
    }

    private void BtnEipOff_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetModeAndRefresh(PromptHudMode.Off);
    }

    private void BtnEipPartial_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetModeAndRefresh(PromptHudMode.Partial);
    }

    private void BtnEipFull_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetModeAndRefresh(PromptHudMode.Full);
    }

    private void SetModeAndRefresh(int mode)
    {
        PromptHudMode.Set(mode);
        cachedMode = PromptHudMode.Current;
        RefreshBindingsSelfAndChildren();
        ApplySelectedState();
    }

    private void ApplySelectedState()
    {
        SetButtonSelected(btnEipOff, cachedMode == PromptHudMode.Off);
        SetButtonSelected(btnEipPartial, cachedMode == PromptHudMode.Partial);
        SetButtonSelected(btnEipFull, cachedMode == PromptHudMode.Full);
    }

    private static void SetButtonSelected(XUiC_SimpleButton button, bool isSelected)
    {
        if (button?.Button != null)
        {
            button.Button.Selected = isSelected;
        }
    }
}

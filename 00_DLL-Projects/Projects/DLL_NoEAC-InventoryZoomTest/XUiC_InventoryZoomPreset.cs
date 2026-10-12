public class XUiC_InventoryZoomPreset : XUiController
{
    bool buttonHandlersBound;
    XUiC_SimpleButton btnVanilla;
    XUiC_SimpleButton btn10;
    XUiC_SimpleButton btn12;

    public override void Init()
    {
        base.Init();
        EnsureButtonHandlers();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        EnsureButtonHandlers();
        RefreshBindingsSelfAndChildren();
        ApplySelectedState();
    }

    public override bool GetBindingValueInternal(ref string value, string bindingName)
    {
        switch (bindingName)
        {
            case "invzoom_vanilla_selected":
                value = InventoryZoom.PresetIndex == 0 ? "true" : "false";
                return true;
            case "invzoom_10_selected":
                value = InventoryZoom.PresetIndex == 1 ? "true" : "false";
                return true;
            case "invzoom_12_selected":
                value = InventoryZoom.PresetIndex == 2 ? "true" : "false";
                return true;
            default:
                return base.GetBindingValueInternal(ref value, bindingName);
        }
    }

    void EnsureButtonHandlers()
    {
        if (buttonHandlersBound)
        {
            return;
        }

        btnVanilla = GetChildById("btnPresetVanilla") as XUiC_SimpleButton;
        btn10 = GetChildById("btnPreset10") as XUiC_SimpleButton;
        btn12 = GetChildById("btnPreset12") as XUiC_SimpleButton;
        if (btnVanilla == null && btn10 == null && btn12 == null)
        {
            return;
        }

        if (btnVanilla != null)
        {
            btnVanilla.OnPressed += OnVanilla;
        }

        if (btn10 != null)
        {
            btn10.OnPressed += On10;
        }

        if (btn12 != null)
        {
            btn12.OnPressed += On12;
        }

        buttonHandlersBound = true;
        ApplySelectedState();
    }

    void OnVanilla(XUiController _sender, int _mouseButton)
    {
        Choose(0);
    }

    void On10(XUiController _sender, int _mouseButton)
    {
        Choose(1);
    }

    void On12(XUiController _sender, int _mouseButton)
    {
        Choose(2);
    }

    void Choose(int index)
    {
        InventoryZoom.SetPreset(index, xui);
        RefreshBindingsSelfAndChildren();
        ApplySelectedState();
    }

    void ApplySelectedState()
    {
        SetSelected(btnVanilla, InventoryZoom.PresetIndex == 0);
        SetSelected(btn10, InventoryZoom.PresetIndex == 1);
        SetSelected(btn12, InventoryZoom.PresetIndex == 2);
    }

    static void SetSelected(XUiC_SimpleButton button, bool isSelected)
    {
        if (button != null && button.Button != null)
        {
            button.Button.Selected = isSelected;
        }
    }
}

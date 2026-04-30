using System;
using System.Collections.Generic;

public class XUiC_OptionsAgfUi : XUiController
{
    private static readonly List<XUiC_OptionsAgfUi> LiveControllers = new List<XUiC_OptionsAgfUi>();
    private const int MaxRows = 2;
    private bool pendingRefresh;
    private bool buttonHandlersBound;
    private XUiC_SimpleButton btnScreamerOff;
    private XUiC_SimpleButton btnScreamerOn;
    private XUiC_SimpleButton btnScreamerNum;
    private XUiC_SimpleButton btnVetOff;
    private XUiC_SimpleButton btnVetOn;
    private bool row1Available;
    private OptionMode row1Mode;
    private string row1Label;
    private bool row2Available;
    private OptionMode row2Mode;
    private string row2Label;

    public override void Init()
    {
        base.Init();
        OptionsRegistry.EnsureInitialized();
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
            RefreshRowCache();
            RefreshBindingsSelfAndChildren();
            ApplySelectedState();
        }
    }

    public override bool GetBindingValueInternal(ref string value, string bindingName)
    {
        if (TryGetRowBinding(bindingName, out int rowIndex, out string fieldName))
        {
            var rows = OptionsRegistry.GetRows();
            if (rowIndex < 0 || rowIndex >= rows.Count)
            {
                value = fieldName == "visible" ? "false" : string.Empty;
                return true;
            }

            bool available = row1Available;
            OptionMode currentMode = row1Mode;
            if (rowIndex == 1)
            {
                available = row2Available;
                currentMode = row2Mode;
            }

            if (fieldName == "label")
            {
                value = row1Label;
                return true;
            }

            if (fieldName == "value")
            {
                value = available ? OptionsRegistry.GetModeLabel(currentMode) : "Not Installed";
                return true;
            }

            if (fieldName == "offtext")
            {
                value = string.Empty;
                return true;
            }

            if (fieldName == "ontext")
            {
                value = string.Empty;
                return true;
            }

            if (fieldName == "numtext")
            {
                value = string.Empty;
                return true;
            }

            if (fieldName == "off_selected_visible")
            {
                value = BuildSelectionVisible(available, currentMode, OptionMode.Off);
                return true;
            }

            if (fieldName == "on_selected_visible")
            {
                value = BuildSelectionVisible(available, currentMode, OptionMode.On);
                return true;
            }

            if (fieldName == "num_selected_visible")
            {
                value = BuildSelectionVisible(available, currentMode, OptionMode.OnWithNumbers);
                return true;
            }

            if (fieldName == "visible")
            {
                value = "true";
                return true;
            }
        }

        return base.GetBindingValueInternal(ref value, bindingName);
    }

    public static void MarkAllDirty()
    {
        for (int i = LiveControllers.Count - 1; i >= 0; i--)
        {
            var controller = LiveControllers[i];
            if (controller == null)
            {
                LiveControllers.RemoveAt(i);
                continue;
            }

            controller.pendingRefresh = true;
            bool isVisible = controller.ViewComponent != null && controller.ViewComponent.IsVisible;
            if (isVisible)
            {
                controller.RefreshRowCache();
                controller.RefreshBindingsSelfAndChildren();
                controller.ApplySelectedState();
            }
        }
    }

    private static void RegisterInstance(XUiC_OptionsAgfUi controller)
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

    private static void UnregisterInstance(XUiC_OptionsAgfUi controller)
    {
        if (controller == null)
        {
            return;
        }

        LiveControllers.Remove(controller);
    }

    private static bool TryGetRowBinding(string bindingName, out int rowIndex, out string fieldName)
    {
        rowIndex = -1;
        fieldName = string.Empty;
        if (string.IsNullOrEmpty(bindingName) || !bindingName.StartsWith("opt_row_", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string remainder = bindingName.Substring("opt_row_".Length);
        int underscore = remainder.IndexOf('_');
        if (underscore <= 0)
        {
            return false;
        }

        string numberPart = remainder.Substring(0, underscore);
        if (!int.TryParse(numberPart, out int oneBased) || oneBased <= 0 || oneBased > MaxRows)
        {
            return false;
        }

        rowIndex = oneBased - 1;
        fieldName = remainder.Substring(underscore + 1).ToLowerInvariant();
        return fieldName == "label"
            || fieldName == "value"
            || fieldName == "visible"
            || fieldName == "offtext"
            || fieldName == "ontext"
            || fieldName == "numtext"
            || fieldName == "off_selected_visible"
            || fieldName == "on_selected_visible"
            || fieldName == "num_selected_visible";
    }

    private static string BuildSelectionVisible(bool available, OptionMode current, OptionMode target)
    {
        if (!available)
        {
            return "false";
        }

        return current == target ? "true" : "false";
    }

    private void SetRowMode(int oneBasedRow, OptionMode mode)
    {
        var rows = OptionsRegistry.GetRows();
        int index = oneBasedRow - 1;
        if (index < 0 || index >= rows.Count)
        {
            return;
        }

        var row = rows[index];
        if (!row.IsAvailable())
        {
            Console.WriteLine("[optionsAGF] Button press ignored; row unavailable: " + row.Key);
            return;
        }

        if (string.Equals(row.Key, "visual_entity_tracker_mode", StringComparison.OrdinalIgnoreCase) && mode == OptionMode.OnWithNumbers)
        {
            mode = OptionMode.On;
        }

        OptionsRegistry.SetMode(row.Key, mode);
        if (index == 0)
        {
            row1Mode = mode;
        }
        else if (index == 1)
        {
            row2Mode = mode;
        }
        Console.WriteLine("[optionsAGF] Button press applied: " + row.Key + " -> " + mode);
        RefreshBindingsSelfAndChildren();
        ApplySelectedState();
        MarkAllDirty();
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
        btnVetOff = GetChildById("btnVetOff") as XUiC_SimpleButton;
        btnVetOn = GetChildById("btnVetOn") as XUiC_SimpleButton;

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
            Console.WriteLine("[optionsAGF] No option buttons found for wiring on this controller instance.");
            return;
        }

        buttonHandlersBound = true;
        ApplySelectedState();
        Console.WriteLine("[optionsAGF] Button handlers wired via XUiC_SimpleButton.OnPressed");
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

        if (btnVetOff != null)
        {
            btnVetOff.OnPressed -= BtnVetOff_OnPressed;
        }

        if (btnVetOn != null)
        {
            btnVetOn.OnPressed -= BtnVetOn_OnPressed;
        }

        buttonHandlersBound = false;
        btnScreamerOff = null;
        btnScreamerOn = null;
        btnScreamerNum = null;
        btnVetOff = null;
        btnVetOn = null;
    }

    private void BtnScreamerOff_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetRowMode(1, OptionMode.Off);
    }

    private void BtnScreamerOn_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetRowMode(1, OptionMode.On);
    }

    private void BtnScreamerNum_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetRowMode(1, OptionMode.OnWithNumbers);
    }

    private void BtnVetOff_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetRowMode(2, OptionMode.Off);
    }

    private void BtnVetOn_OnPressed(XUiController _sender, int _mouseButton)
    {
        SetRowMode(2, OptionMode.On);
    }

    private void RefreshRowCache()
    {
        var rows = OptionsRegistry.GetRows();
        if (rows.Count > 0)
        {
            row1Label = rows[0].Label;
            row1Available = rows[0].IsAvailable();
            row1Mode = OptionsRegistry.GetMode(rows[0].Key, rows[0].DefaultMode);
        }
        else
        {
            row1Label = string.Empty;
            row1Available = false;
            row1Mode = OptionMode.OnWithNumbers;
        }

        if (rows.Count > 1)
        {
            row2Label = rows[1].Label;
            row2Available = rows[1].IsAvailable();
            row2Mode = OptionsRegistry.GetMode(rows[1].Key, rows[1].DefaultMode);
        }
        else
        {
            row2Label = string.Empty;
            row2Available = false;
            row2Mode = OptionMode.On;
        }

    }

    private void ApplySelectedState()
    {
        SetButtonSelected(btnScreamerOff, row1Available && row1Mode == OptionMode.Off);
        SetButtonSelected(btnScreamerOn, row1Available && row1Mode == OptionMode.On);
        SetButtonSelected(btnScreamerNum, row1Available && row1Mode == OptionMode.OnWithNumbers);
        SetButtonSelected(btnVetOff, row2Available && row2Mode == OptionMode.Off);
        SetButtonSelected(btnVetOn, row2Available && row2Mode != OptionMode.Off);
    }

    private static void SetButtonSelected(XUiC_SimpleButton button, bool isSelected)
    {
        if (button?.Button != null)
        {
            button.Button.Selected = isSelected;
        }
    }

}

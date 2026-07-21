using System;

public class XUiC_EnhancedScreamerAlerts : XUiController
{
    public static XUiC_EnhancedScreamerAlerts Instance { get; private set; }
    public override void Init()
    {
        base.Init();
        Instance = this;
        ScreamerAlertEnhancedState.Changed += Refresh;
    }
    public override void Update(float dt)
    {
        base.Update(dt);
        SetVisible();
    }
    public override bool GetBindingValueInternal(ref string value, string name)
    {
        string scout = ScreamerAlertEnhancedState.ScoutText();
        string horde = ScreamerAlertEnhancedState.HordeText();
        if (name == "screameralert") { value = scout; return true; }
        if (name == "screamerhordealert") { value = horde; return true; }
        if (name == "screameralertOn") { value = (!string.IsNullOrEmpty(scout)).ToString().ToLowerInvariant(); return true; }
        if (name == "screamerhordealertOn") { value = (!string.IsNullOrEmpty(horde)).ToString().ToLowerInvariant(); return true; }
        if (name == "screameralertsvisible") { value = (!string.IsNullOrEmpty(scout) || !string.IsNullOrEmpty(horde)).ToString().ToLowerInvariant(); return true; }
        return base.GetBindingValueInternal(ref value, name);
    }
    private void Refresh()
    {
        RefreshBindingsSelfAndChildren();
        SetVisible();
    }
    private void SetVisible()
    {
        if (ViewComponent != null)
            ViewComponent.IsVisible = !string.IsNullOrEmpty(ScreamerAlertEnhancedState.ScoutText()) || !string.IsNullOrEmpty(ScreamerAlertEnhancedState.HordeText());
    }
}
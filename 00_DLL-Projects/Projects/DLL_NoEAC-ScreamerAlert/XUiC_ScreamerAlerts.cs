using UnityEngine;

public class XUiC_ScreamerAlerts : XUiController
{
    public static XUiC_ScreamerAlerts Instance;
    private const float DisplayRefreshIntervalSeconds = 0.2f;
    private float _nextVisibilityRefreshAt;
    private float _nextDisplayRefreshAt;
    private bool _cachedShouldShow;
    private string _cachedScoutDisplay = string.Empty;
    private string _cachedHordeDisplay = string.Empty;
    private ScreamerAlertMode _cachedDisplayMode = (ScreamerAlertMode)(-1);


    public override void Init()
    {
        base.Init();
        Instance = this;
    }

    public override bool GetBindingValueInternal(ref string value, string bindingName)
    {
        RefreshDisplayCacheIfNeeded();

        bool hasAnyAlert = !string.IsNullOrEmpty(_cachedScoutDisplay)
            || !string.IsNullOrEmpty(_cachedHordeDisplay);

        switch (bindingName)
        {
            case "screameralert":
                value = _cachedScoutDisplay;
                return true;
            case "screamerhordealert":
                value = _cachedHordeDisplay;
                return true;
            case "screameralertOn":
                value = (!string.IsNullOrEmpty(_cachedScoutDisplay)).ToString().ToLower();
                return true;
            case "screamerhordealertOn":
                value = (!string.IsNullOrEmpty(_cachedHordeDisplay)).ToString().ToLower();
                return true;
            case "screameralertsvisible":
                value = hasAnyAlert.ToString().ToLower();
                return true;
            default:
                return base.GetBindingValueInternal(ref value, bindingName);
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();
        _nextVisibilityRefreshAt = 0f;
        _nextDisplayRefreshAt = 0f;
        RefreshDisplayCacheIfNeeded();
        RefreshBindingsSelfAndChildren();
        UpdateViewVisibility();
    }

    public override void OnClose()
    {
        base.OnClose();
        if (base.ViewComponent != null)
        {
            base.ViewComponent.IsVisible = false;
        }
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);

        RefreshDisplayCacheIfNeeded();

        if (Time.time >= _nextVisibilityRefreshAt)
        {
            _nextVisibilityRefreshAt = Time.time + DisplayRefreshIntervalSeconds;
            UpdateViewVisibility();
        }
    }

    private string GetScreamerAlertMessage()
    {
        var controller = ScreamerAlertsController.Instance;
        string raw = controller != null ? controller.screamerAlertMessage : string.Empty;
        int nearbyCount = controller != null ? controller.nearbyScoutCount : 0;
        return FormatAlertForMode(raw, nearbyCount);
    }

    private string GetScreamerHordeAlertMessage()
    {
        var controller = ScreamerAlertsController.Instance;
        string raw = controller != null ? controller.screamerHordeAlertMessage : string.Empty;
        int nearbyCount = controller != null ? controller.nearbyHordeCount : 0;
        return FormatAlertForMode(raw, nearbyCount);
    }

    private void RefreshDisplayCacheIfNeeded()
    {
        ScreamerAlertMode currentMode = ScreamerAlertModeSettings.GetModeForLocalPlayer(ScreamerAlertMode.On);
        bool modeChanged = currentMode != _cachedDisplayMode;

        if (!modeChanged && Time.time < _nextDisplayRefreshAt)
        {
            return;
        }

        _nextDisplayRefreshAt = Time.time + DisplayRefreshIntervalSeconds;
        _cachedDisplayMode = currentMode;
        _cachedScoutDisplay = GetScreamerAlertMessage();
        _cachedHordeDisplay = GetScreamerHordeAlertMessage();
    }

    private static string FormatAlertForMode(string rawText, int nearbyCount)
    {
        if (string.IsNullOrEmpty(rawText))
        {
            return string.Empty;
        }

        ScreamerAlertMode mode = ScreamerAlertModeSettings.GetModeForLocalPlayer(ScreamerAlertMode.On);
        if (mode == ScreamerAlertMode.Off)
        {
            return string.Empty;
        }

        string baseText = ScreamerAlertModeSettings.StripNumberSuffix(rawText);
        if (mode == ScreamerAlertMode.On)
        {
            return baseText;
        }

        if (nearbyCount < 0)
        {
            nearbyCount = 0;
        }

        return baseText + " [FFFFFF](" + nearbyCount + ")[-]";
    }

    private void UpdateViewVisibility()
    {
        if (base.ViewComponent == null)
        {
            return;
        }

        _cachedShouldShow = !string.IsNullOrEmpty(_cachedScoutDisplay)
            || !string.IsNullOrEmpty(_cachedHordeDisplay);

        bool shouldShow = _cachedShouldShow;
        if (base.ViewComponent.IsVisible != shouldShow)
        {
            base.ViewComponent.IsVisible = shouldShow;
        }
    }
}

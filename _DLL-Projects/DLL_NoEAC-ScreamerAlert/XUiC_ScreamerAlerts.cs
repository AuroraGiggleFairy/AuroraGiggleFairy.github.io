using UnityEngine;

public class XUiC_ScreamerAlerts : XUiController
{
    public static XUiC_ScreamerAlerts Instance;


    public override void Init()
    {
        base.Init();
        Instance = this;
    }

    public override bool GetBindingValueInternal(ref string value, string bindingName)
    {
        var controller = ScreamerAlertsController.Instance;
        switch (bindingName)
        {
            case "screameralert":
                value = controller != null ? controller.GetScreamerAlertMessage() : string.Empty;
                return true;
            case "screamerhordealert":
                value = controller != null ? controller.GetScreamerHordeAlertMessage() : string.Empty;
                return true;
            case "screameralertOn":
                value = (controller != null && !string.IsNullOrEmpty(controller.GetScreamerAlertMessage())).ToString().ToLower();
                return true;
            case "screamerhordealertOn":
                value = (controller != null && !string.IsNullOrEmpty(controller.GetScreamerHordeAlertMessage())).ToString().ToLower();
                return true;
            default:
                return base.GetBindingValueInternal(ref value, bindingName);
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();
        if (base.ViewComponent != null)
        {
            base.ViewComponent.IsVisible = true;
        }
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
    }

    private string GetScreamerAlertMessage()
    {
        // Return the actual screamer alert message from the controller
        if (ScreamerAlertsController.Instance != null)
        {
            return ScreamerAlertsController.Instance.GetScreamerAlertMessage();
        }
        return string.Empty;
    }
}

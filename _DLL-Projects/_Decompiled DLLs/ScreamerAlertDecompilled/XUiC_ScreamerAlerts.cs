public class XUiC_ScreamerAlerts : XUiController
{
	private static string staticTest;

	public static XUiC_ScreamerAlerts Instance;

	private string screamerAlertMessage = string.Empty;

	private string screamerHordeAlertMessage = string.Empty;

	static XUiC_ScreamerAlerts()
	{
		staticTest = LogStaticField();
	}

	private static string LogStaticField()
	{
		return string.Empty;
	}

	public override void Init()
	{
		base.Init();
		Instance = this;
	}

	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		ScreamerAlertsController instance = ScreamerAlertsController.Instance;
		if (instance != null)
		{
			screamerAlertMessage = instance.GetScreamerAlertMessage();
			screamerHordeAlertMessage = instance.GetScreamerHordeAlertMessage();
		}
		switch (bindingName)
		{
		case "screameralert":
			value = screamerAlertMessage;
			return true;
		case "screamerhordealert":
			value = screamerHordeAlertMessage;
			return true;
		case "screameralertOn":
			value = (!string.IsNullOrEmpty(screamerAlertMessage)).ToString().ToLower();
			return true;
		case "screamerhordealertOn":
			value = (!string.IsNullOrEmpty(screamerHordeAlertMessage)).ToString().ToLower();
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
		return string.Empty;
	}
}

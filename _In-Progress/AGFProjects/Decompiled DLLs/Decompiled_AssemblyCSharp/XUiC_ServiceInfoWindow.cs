using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServiceInfoWindow : XUiC_InfoWindow
{
	[PublicizedFrom(EAccessModifier.Private)]
	public InGameService service;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController servicePreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController description;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList mainActionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 valueColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_InfoWindow emptyInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public string stat1 = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt servicecostFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor serviceicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public override void Init()
	{
		base.Init();
		servicePreview = GetChildById("servicePreview");
		windowName = GetChildById("windowName");
		windowIcon = GetChildById("windowIcon");
		description = GetChildById("descriptionText");
		stats = GetChildById("statText");
		mainActionItemList = (XUiC_ItemActionList)GetChildById("itemActions");
	}

	public override void Deselect()
	{
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty && base.ViewComponent.IsVisible)
		{
			if (emptyInfoWindow == null)
			{
				emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
			}
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "servicename":
			value = ((service != null) ? service.Name : "");
			return true;
		case "serviceicon":
			value = ((service != null) ? service.Icon : "");
			return true;
		case "servicecost":
			value = ((service != null) ? servicecostFormatter.Format(service.Price) : "");
			return true;
		case "pricelabel":
			value = Localization.Get("xuiCost");
			return true;
		case "serviceicontint":
		{
			Color32 v = Color.white;
			value = serviceicontintcolorFormatter.Format(v);
			return true;
		}
		case "servicedescription":
			value = ((service != null) ? service.Description : "");
			return true;
		case "servicegroupicon":
			value = ((service != null) ? service.Icon : "");
			return true;
		case "servicestats":
			value = ((service != null) ? stat1 : "");
			return true;
		default:
			return false;
		}
	}

	public void SetInfo(InGameService _service, XUiController controller)
	{
		service = _service;
		if (service == null)
		{
			if (emptyInfoWindow == null)
			{
				emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
			}
			emptyInfoWindow.ViewComponent.IsVisible = true;
			return;
		}
		base.ViewComponent.IsVisible = true;
		if (servicePreview != null)
		{
			string newValue = Utils.ColorToHex(valueColor);
			stat1 = XUiM_InGameService.GetServiceStats(base.xui, service).Replace("REPLACE_COLOR", newValue);
			mainActionItemList.SetServiceActionList(service, controller);
			RefreshBindings();
		}
	}

	public override void OnVisibilityChanged(bool _isVisible)
	{
		base.OnVisibilityChanged(_isVisible);
		if (service != null)
		{
			service.VisibleChangedHandler(_isVisible);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (service == null)
		{
			if (emptyInfoWindow == null)
			{
				emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
			}
			emptyInfoWindow.ViewComponent.IsVisible = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		service = null;
	}
}

using System;
using UnityEngine;

public class XUiV_Rect : XUiView_WidgetBased
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool disableFallthrough;

	[XuiXmlAttribute("disablefallthrough", false)]
	public bool DisableFallthrough
	{
		get
		{
			return disableFallthrough;
		}
		set
		{
			disableFallthrough = value;
		}
	}

	public override bool HasAnyEvent
	{
		get
		{
			if (!disableFallthrough)
			{
				return base.HasAnyEvent;
			}
			return true;
		}
	}

	public XUiV_Rect(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UIWidget>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		widget = uiTransform.gameObject.GetComponent<UIWidget>();
	}

	public override void InitView()
	{
		base.InitView();
		UIWidget uIWidget = widget;
		uIWidget.onChange = (UIWidget.OnDimensionsChanged)Delegate.Combine(uIWidget.onChange, new UIWidget.OnDimensionsChanged(base.SetDirty));
		updateData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void refreshBoxCollider()
	{
		base.refreshBoxCollider();
		if (disableFallthrough)
		{
			Vector3 center = collider.center;
			center.z = 100f;
			collider.center = center;
		}
	}
}

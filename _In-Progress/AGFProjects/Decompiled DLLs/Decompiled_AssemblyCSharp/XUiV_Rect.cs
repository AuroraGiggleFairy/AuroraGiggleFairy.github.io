using System;
using UnityEngine;

public class XUiV_Rect : XUiView
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool createUiWidget;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget widget;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool disableFallthrough;

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

	public XUiV_Rect(string _id)
		: base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UIWidget>();
	}

	public override void InitView()
	{
		base.InitView();
		widget = uiTransform.gameObject.GetComponent<UIWidget>();
		if (createUiWidget)
		{
			widget.enabled = true;
			UIWidget uIWidget = widget;
			uIWidget.onChange = (UIWidget.OnDimensionsChanged)Delegate.Combine(uIWidget.onChange, (UIWidget.OnDimensionsChanged)([PublicizedFrom(EAccessModifier.Private)] () =>
			{
				isDirty = true;
			}));
		}
		else
		{
			UnityEngine.Object.Destroy(widget);
			widget = null;
		}
		UpdateData();
	}

	public override void UpdateData()
	{
		if (!initialized)
		{
			initialized = true;
			if (widget != null)
			{
				widget.pivot = pivot;
				widget.depth = depth;
				uiTransform.localScale = Vector3.one;
				uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
			}
		}
		if (widget != null)
		{
			widget.pivot = pivot;
			widget.depth = depth;
			widget.keepAspectRatio = keepAspectRatio;
			widget.aspectRatio = aspectRatio;
			widget.autoResizeBoxCollider = true;
			parseAnchors(widget);
		}
		base.UpdateData();
	}

	public override void RefreshBoxCollider()
	{
		base.RefreshBoxCollider();
		if (disableFallthrough)
		{
			BoxCollider boxCollider = collider;
			if (boxCollider != null)
			{
				int num = 100;
				Vector3 center = boxCollider.center;
				center.z = num;
				boxCollider.center = center;
			}
		}
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			if (!(attribute == "disablefallthrough"))
			{
				if (!(attribute == "createuiwidget"))
				{
					return false;
				}
				createUiWidget = StringParsers.ParseBool(value);
			}
			else
			{
				DisableFallthrough = StringParsers.ParseBool(value);
			}
			return true;
		}
		return flag;
	}
}

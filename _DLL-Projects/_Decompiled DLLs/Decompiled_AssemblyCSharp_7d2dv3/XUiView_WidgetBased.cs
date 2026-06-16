using System.Collections;
using UnityEngine;

public abstract class XUiView_WidgetBased : XUiView
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget widget;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget.AspectRatioSource keepAspectRatio;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float aspectRatio;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool autoResizeCollider;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget.Pivot pivot;

	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return widget;
		}
	}

	public override Vector3 LocalCenter => widget.localCenter;

	public override Vector3[] WorldCorners => widget.worldCorners;

	[XuiXmlAttribute("keep_aspect_ratio", false)]
	public UIWidget.AspectRatioSource KeepAspectRatio
	{
		get
		{
			return keepAspectRatio;
		}
		set
		{
			if (keepAspectRatio != value)
			{
				keepAspectRatio = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("aspect_ratio", false)]
	public float AspectRatio
	{
		get
		{
			return aspectRatio;
		}
		set
		{
			if (!Mathf.Approximately(aspectRatio, value))
			{
				aspectRatio = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("pivot", false)]
	public UIWidget.Pivot Pivot
	{
		get
		{
			return pivot;
		}
		set
		{
			if (pivot != value)
			{
				pivot = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("auto_resize_collider", false)]
	public bool AutoResizeCollider
	{
		get
		{
			return autoResizeCollider;
		}
		set
		{
			if (autoResizeCollider != value)
			{
				autoResizeCollider = value;
				SetDirty();
			}
		}
	}

	public virtual bool anchorsKeepSize
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiView_WidgetBased(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	public override void InitView()
	{
		base.InitView();
		widget.pivot = pivot;
		widget.depth = depth;
		uiTransform.localScale = Vector3.one;
		uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
	}

	public override void SetDefaults(XUiController _parent)
	{
		Pivot = UIWidget.Pivot.TopLeft;
		base.SetDefaults(_parent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void refreshBoxCollider()
	{
		if (AutoResizeCollider || widget.isAnchored)
		{
			widget.autoResizeBoxCollider = true;
			return;
		}
		collider.center = widget.localCenter;
		collider.size = new Vector3(widget.localSize.x * base.ColliderScale + (float)(2 * base.ColliderPadding.x), widget.localSize.y * base.ColliderScale + (float)(2 * base.ColliderPadding.y), 0f);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (anchoredLeftAndRight)
		{
			size.x = Mathf.RoundToInt(widget.localSize.x);
		}
		if (anchoredTopAndBottom)
		{
			size.y = Mathf.RoundToInt(widget.localSize.y);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		widget.keepAspectRatio = keepAspectRatio;
		widget.aspectRatio = aspectRatio;
		base.updateData();
		refreshBoxCollider();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void anchorsParsed()
	{
		base.anchorsParsed();
		if (anchorsKeepSize)
		{
			if (!anchoredLeftAndRight && widget.width != size.x)
			{
				widget.width = size.x;
			}
			if (!anchoredTopAndBottom && widget.height != size.y)
			{
				widget.height = size.y;
			}
		}
		ThreadManager.StartCoroutine(MarkAsChangedLater());
		[PublicizedFrom(EAccessModifier.Private)]
		IEnumerator MarkAsChangedLater()
		{
			yield return new WaitForEndOfFrame();
			widget.MarkAsChanged();
		}
	}
}

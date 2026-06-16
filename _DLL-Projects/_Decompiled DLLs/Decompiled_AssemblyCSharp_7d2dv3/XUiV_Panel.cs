using UnityEngine;

public class XUiV_Panel : XUiView
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public UIPanel panel;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIDrawCall.Clipping clipping;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 clippingOffset = new Vector2(-10000f, -10000f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 clippingSize = new Vector2(-10000f, -10000f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 clippingCenter = new Vector2(-10000f, -10000f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 clippingSoftness;

	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return panel;
		}
	}

	[XuiXmlAttribute("clipping", false)]
	public UIDrawCall.Clipping Clipping
	{
		get
		{
			return clipping;
		}
		set
		{
			if (value != clipping)
			{
				clipping = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("clippingsize", false)]
	public Vector2 ClippingSize
	{
		get
		{
			return clippingSize;
		}
		set
		{
			if (!(value == clippingSize))
			{
				clippingSize = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("clippingcenter", false)]
	public Vector2 ClippingCenter
	{
		get
		{
			return clippingCenter;
		}
		set
		{
			if (!(value == clippingCenter))
			{
				clippingCenter = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("clippingsoftness", false)]
	public Vector2 ClippingSoftness
	{
		get
		{
			return clippingSoftness;
		}
		set
		{
			if (!(value == clippingSoftness))
			{
				clippingSoftness = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("clippingoffset", false)]
	public Vector2 ClipOffset
	{
		get
		{
			return clippingOffset;
		}
		set
		{
			if (!(value == clippingOffset))
			{
				clippingOffset = value;
				SetDirty();
			}
		}
	}

	public bool StaticWidgets
	{
		get
		{
			return panel.widgetsAreStatic;
		}
		set
		{
			panel.widgetsAreStatic = value;
		}
	}

	public override Vector3[] WorldCorners => panel.worldCorners;

	public XUiV_Panel(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UIPanel>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		panel = uiTransform.gameObject.GetComponent<UIPanel>();
	}

	public override void InitView()
	{
		base.InitView();
		panel.depth = depth;
		updateClipping();
		SetDirty();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		base.updateData();
		updateClipping();
		refreshBoxCollider();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateClipping()
	{
		if (clipping != UIDrawCall.Clipping.None)
		{
			panel.clipping = clipping;
			Vector2 zero = clippingOffset;
			Vector2 vector = clippingCenter;
			Vector2 vector2 = clippingSize;
			Vector2 clipSoftness = clippingSoftness;
			if (zero == new Vector2(-10000f, -10000f))
			{
				zero = Vector2.zero;
			}
			if (vector == new Vector2(-10000f, -10000f))
			{
				vector = new Vector2((float)size.x / 2f, (float)(-size.y) / 2f);
			}
			if (vector2 == new Vector2(-10000f, -10000f))
			{
				vector2 = new Vector2(size.x, size.y);
			}
			if (vector2.x < 0f)
			{
				vector2.x = 0f;
			}
			if (vector2.y < 0f)
			{
				vector2.y = 0f;
			}
			panel.clipSoftness = clipSoftness;
			panel.baseClipRegion = new Vector4(vector.x, vector.y, vector2.x, vector2.y);
			panel.clipOffset = zero;
		}
	}
}

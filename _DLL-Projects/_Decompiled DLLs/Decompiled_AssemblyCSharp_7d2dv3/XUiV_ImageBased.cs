using UnityEngine;

public abstract class XUiV_ImageBased : XUiView_WidgetBased
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool keepSourceAspectRatio;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Type type;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Flip flip;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillCenter;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool foregroundLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float globalOpacityModifier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float previousGlobalOpacity;

	[XuiXmlAttribute("keepsourceaspectratio", false)]
	public bool KeepSourceAspectRatio
	{
		get
		{
			return keepSourceAspectRatio;
		}
		set
		{
			if (value != keepSourceAspectRatio)
			{
				keepSourceAspectRatio = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("type", false)]
	public virtual UIBasicSprite.Type Type
	{
		get
		{
			return type;
		}
		set
		{
			if (type != value)
			{
				type = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("fillcenter", false)]
	public bool FillCenter
	{
		get
		{
			return fillCenter;
		}
		set
		{
			if (fillCenter != value)
			{
				fillCenter = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("flip", false)]
	public UIBasicSprite.Flip Flip
	{
		get
		{
			return flip;
		}
		set
		{
			if (flip != value)
			{
				flip = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("globalopacitymod", false)]
	public float GlobalOpacityModifier
	{
		get
		{
			return globalOpacityModifier;
		}
		set
		{
			if (!Mathf.Approximately(globalOpacityModifier, value))
			{
				globalOpacityModifier = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("foregroundlayer", false)]
	public bool ForegroundLayer
	{
		get
		{
			return foregroundLayer;
		}
		set
		{
			if (foregroundLayer != value)
			{
				foregroundLayer = value;
				SetDirty();
			}
		}
	}

	public float GlobalOpacitySetting
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!foregroundLayer)
			{
				return xui.BackgroundGlobalOpacity;
			}
			return xui.ForegroundGlobalOpacity;
		}
	}

	public override void Update(float _dt)
	{
		float globalOpacitySetting = GlobalOpacitySetting;
		if (!Mathf.Approximately(previousGlobalOpacity, globalOpacitySetting))
		{
			previousGlobalOpacity = globalOpacitySetting;
			SetDirty();
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color opacityModColor(Color _origColor)
	{
		if (globalOpacityModifier == 0f)
		{
			return _origColor;
		}
		float globalOpacitySetting = GlobalOpacitySetting;
		if (globalOpacitySetting >= 0.999f)
		{
			return _origColor;
		}
		float a = Mathf.Clamp01(_origColor.a * globalOpacityModifier * globalOpacitySetting);
		return new Color(_origColor.r, _origColor.g, _origColor.b, a);
	}

	public XUiV_ImageBased(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		FillCenter = true;
		Type = UIBasicSprite.Type.Simple;
	}
}

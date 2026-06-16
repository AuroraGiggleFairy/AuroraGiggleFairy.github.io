using System;
using UnityEngine;

public class XUiV_Sprite : XUiV_ImageBased
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string uiAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool uiAtlasChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string spriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color color = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color? gradientStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color? gradientEnd;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.FillDirection fillDirection;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillSpritePad;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillInvert;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float fillAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite sprite;

	public UISprite Sprite => sprite;

	[XuiXmlAttribute("atlas", false)]
	public string UIAtlas
	{
		get
		{
			return uiAtlas;
		}
		set
		{
			if (!(uiAtlas == value))
			{
				uiAtlas = value;
				uiAtlasChanged = true;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("sprite", false)]
	public string SpriteName
	{
		get
		{
			return spriteName;
		}
		set
		{
			if (!(spriteName == value))
			{
				spriteName = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("color", false)]
	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			if (!(color == value))
			{
				color = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("filldirection", false)]
	public UIBasicSprite.FillDirection FillDirection
	{
		get
		{
			return fillDirection;
		}
		set
		{
			if (fillDirection != value)
			{
				fillDirection = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("fillspritepad", false)]
	public bool FillSpritePad
	{
		get
		{
			return fillSpritePad;
		}
		set
		{
			if (fillSpritePad != value)
			{
				fillSpritePad = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("fillinvert", false)]
	public bool FillInvert
	{
		get
		{
			return fillInvert;
		}
		set
		{
			if (fillInvert != value)
			{
				fillInvert = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("fill", false)]
	public float Fill
	{
		get
		{
			return fillAmount;
		}
		set
		{
			if (!Mathf.Approximately(fillAmount, value) && !((double)Math.Abs((value - fillAmount) / value) < 0.005))
			{
				fillAmount = Mathf.Clamp01(value);
				SetDirty();
			}
		}
	}

	public XUiV_Sprite(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UISprite>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		widget = (sprite = uiTransform.GetComponent<UISprite>());
	}

	public override void InitView()
	{
		base.InitView();
		updateData();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		base.FillCenter = true;
		Type = UIBasicSprite.Type.Simple;
		FillDirection = UIBasicSprite.FillDirection.Horizontal;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		applyAtlasAndSprite();
		if (fillSpritePad)
		{
			XUiUtils.ApplyFillPaddedSprite(sprite, spriteName);
		}
		sprite.fixedAspect = keepSourceAspectRatio;
		sprite.color = opacityModColor(color);
		if (gradientStart.HasValue)
		{
			sprite.gradientTop = gradientStart.Value;
			sprite.applyGradient = true;
		}
		if (gradientEnd.HasValue)
		{
			sprite.gradientBottom = gradientEnd.Value;
			sprite.applyGradient = true;
		}
		if (sprite.centerType != (UIBasicSprite.AdvancedType)(fillCenter ? 1 : 0))
		{
			sprite.centerType = (fillCenter ? UIBasicSprite.AdvancedType.Sliced : UIBasicSprite.AdvancedType.Invisible);
		}
		sprite.fillDirection = fillDirection;
		sprite.invert = fillInvert;
		if (!Mathf.Approximately(sprite.fillAmount, fillAmount))
		{
			sprite.fillAmount = fillAmount;
		}
		sprite.type = type;
		sprite.flip = flip;
		base.updateData();
	}

	public void SetSpriteImmediately(string _spriteName)
	{
		spriteName = _spriteName;
		applyAtlasAndSprite(_force: true);
	}

	public void SetColorImmediately(Color _color)
	{
		if (sprite != null)
		{
			sprite.color = _color;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool applyAtlasAndSprite(bool _force = false)
	{
		if (sprite == null)
		{
			return false;
		}
		if (!_force && sprite.spriteName != null && sprite.spriteName == spriteName && sprite.atlas != null && !uiAtlasChanged)
		{
			return false;
		}
		uiAtlasChanged = false;
		sprite.atlas = xui.GetAtlasByName(UIAtlas, spriteName);
		sprite.spriteName = spriteName;
		return true;
	}

	[XuiXmlAttribute("gradient_start", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeGradientStart(Color _value)
	{
		gradientStart = _value;
	}

	[XuiXmlAttribute("gradient_end", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeGradientEnd(Color _value)
	{
		gradientEnd = _value;
	}
}

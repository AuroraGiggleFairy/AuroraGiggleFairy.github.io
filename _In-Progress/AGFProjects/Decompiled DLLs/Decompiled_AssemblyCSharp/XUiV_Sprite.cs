using System;
using UnityEngine;

public class XUiV_Sprite : XUiView
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string uiAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool uiAtlasChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string spriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color color = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool keepOriginalSpriteAspectRatio;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color? gradientStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color? gradientEnd;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Type type;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.FillDirection fillDirection;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillInvert;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Flip flip;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite sprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillCenter;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float fillAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool foregroundLayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastVisible;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string spriteNameXB1 = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string spriteNamePS4 = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float globalOpacityModifier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int borderSize = -1;

	public UISprite Sprite => sprite;

	public string UIAtlas
	{
		get
		{
			return uiAtlas;
		}
		set
		{
			if (uiAtlas != value)
			{
				uiAtlas = value;
				uiAtlasChanged = true;
				isDirty = true;
			}
		}
	}

	public string SpriteName
	{
		get
		{
			return spriteName;
		}
		set
		{
			if (spriteName != value)
			{
				spriteName = value;
				isDirty = true;
			}
		}
	}

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			if (color.r != value.r || color.g != value.g || color.b != value.b || color.a != value.a)
			{
				color = value;
				isDirty = true;
			}
		}
	}

	public bool KeepOriginalSpriteAspectRatio
	{
		get
		{
			return keepOriginalSpriteAspectRatio;
		}
		set
		{
			if (value != keepOriginalSpriteAspectRatio)
			{
				keepOriginalSpriteAspectRatio = value;
				isDirty = true;
			}
		}
	}

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
				isDirty = true;
			}
		}
	}

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
				isDirty = true;
			}
		}
	}

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
				isDirty = true;
			}
		}
	}

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
				isDirty = true;
			}
		}
	}

	public float Fill
	{
		get
		{
			return fillAmount;
		}
		set
		{
			if (fillAmount != value && (double)Math.Abs((value - fillAmount) / value) > 0.005)
			{
				fillAmount = Mathf.Clamp01(value);
				isDirty = true;
			}
		}
	}

	public UIBasicSprite.Flip Flip
	{
		get
		{
			return sprite.flip;
		}
		set
		{
			if (flip != value)
			{
				flip = value;
				isDirty = true;
			}
		}
	}

	public float GlobalOpacityModifier
	{
		get
		{
			return globalOpacityModifier;
		}
		set
		{
			if (globalOpacityModifier != value)
			{
				globalOpacityModifier = value;
				isDirty = true;
			}
		}
	}

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
				isDirty = true;
			}
		}
	}

	public XUiV_Sprite(string _id)
		: base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UISprite>();
	}

	public override void InitView()
	{
		base.InitView();
		sprite = uiTransform.GetComponent<UISprite>();
		UpdateData();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.GlobalOpacityChanged)
		{
			isDirty = true;
		}
		UISprite uISprite = sprite;
		if ((object)uISprite != null)
		{
			bool flag = uISprite.isVisible;
			if (lastVisible != flag)
			{
				isDirty = true;
				lastVisible = flag;
			}
		}
	}

	public override void UpdateData()
	{
		_ = initialized;
		applyAtlasAndSprite();
		sprite.fixedAspect = keepOriginalSpriteAspectRatio;
		sprite.keepAspectRatio = keepAspectRatio;
		sprite.aspectRatio = aspectRatio;
		if (sprite.color != color)
		{
			sprite.color = color;
		}
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
		if (globalOpacityModifier != 0f && (foregroundLayer ? (base.xui.ForegroundGlobalOpacity < 1f) : (base.xui.BackgroundGlobalOpacity < 1f)))
		{
			float a = Mathf.Clamp01(color.a * (globalOpacityModifier * (foregroundLayer ? base.xui.ForegroundGlobalOpacity : base.xui.BackgroundGlobalOpacity)));
			sprite.color = new Color(color.r, color.g, color.b, a);
		}
		if (borderSize > 0 && sprite.border.x != (float)borderSize)
		{
			sprite.border = new Vector4(borderSize, borderSize, borderSize, borderSize);
		}
		if (sprite.centerType != (UIBasicSprite.AdvancedType)(fillCenter ? 1 : 0))
		{
			sprite.centerType = (fillCenter ? UIBasicSprite.AdvancedType.Sliced : UIBasicSprite.AdvancedType.Invisible);
		}
		parseAnchors(sprite);
		if (sprite.fillDirection != fillDirection)
		{
			sprite.fillDirection = fillDirection;
		}
		if (sprite.invert != fillInvert)
		{
			sprite.invert = fillInvert;
		}
		if (sprite.fillAmount != fillAmount)
		{
			sprite.fillAmount = fillAmount;
		}
		if (sprite.type != type)
		{
			sprite.type = type;
		}
		if (sprite.flip != flip)
		{
			sprite.flip = flip;
		}
		if (!initialized)
		{
			initialized = true;
			sprite.pivot = pivot;
			sprite.depth = depth;
			uiTransform.localScale = Vector3.one;
			uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
			if (EventOnHover || EventOnPress)
			{
				BoxCollider boxCollider = collider;
				boxCollider.center = sprite.localCenter;
				boxCollider.size = new Vector3(sprite.localSize.x * colliderScale, sprite.localSize.y * colliderScale, 0f);
			}
		}
		if (sprite.isAnchored)
		{
			sprite.autoResizeBoxCollider = true;
		}
		else
		{
			RefreshBoxCollider();
		}
		base.UpdateData();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		FillCenter = true;
		Type = UIBasicSprite.Type.Simple;
		FillDirection = UIBasicSprite.FillDirection.Horizontal;
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			switch (attribute)
			{
			case "atlas":
				UIAtlas = value;
				break;
			case "sprite":
				SpriteName = value;
				break;
			case "sprite_xb1":
				spriteNameXB1 = value;
				break;
			case "sprite_ps4":
				spriteNamePS4 = value;
				break;
			case "color":
				Color = StringParsers.ParseColor32(value);
				break;
			case "keeporiginalspriteaspectratio":
				KeepOriginalSpriteAspectRatio = StringParsers.ParseBool(value);
				break;
			case "fill":
				Fill = StringParsers.ParseFloat(value);
				break;
			case "fillcenter":
				FillCenter = StringParsers.ParseBool(value);
				break;
			case "filldirection":
				FillDirection = EnumUtils.Parse<UIBasicSprite.FillDirection>(value, _ignoreCase: true);
				break;
			case "fillinvert":
				FillInvert = StringParsers.ParseBool(value);
				break;
			case "flip":
				Flip = EnumUtils.Parse<UIBasicSprite.Flip>(value, _ignoreCase: true);
				break;
			case "type":
				Type = EnumUtils.Parse<UIBasicSprite.Type>(value, _ignoreCase: true);
				break;
			case "globalopacity":
				if (!StringParsers.ParseBool(value))
				{
					GlobalOpacityModifier = 0f;
				}
				break;
			case "globalopacitymod":
				GlobalOpacityModifier = StringParsers.ParseFloat(value);
				break;
			case "bordersize":
				borderSize = int.Parse(value);
				break;
			case "foregroundlayer":
				foregroundLayer = StringParsers.ParseBool(value);
				break;
			case "gradient_start":
				gradientStart = StringParsers.ParseColor32(value);
				break;
			case "gradient_end":
				gradientEnd = StringParsers.ParseColor32(value);
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}

	public void SetSpriteImmediately(string spriteName)
	{
		this.spriteName = spriteName;
		applyAtlasAndSprite(_force: true);
	}

	public void SetColorImmediately(Color color)
	{
		if (sprite != null)
		{
			sprite.color = color;
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
		sprite.atlas = base.xui.GetAtlasByName(UIAtlas, spriteName);
		sprite.spriteName = spriteName;
		return true;
	}
}

using UnityEngine;

public class XUiV_Panel : XUiView
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string backgroundSpriteName = XUi.BlankTexture;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color borderColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUi_Thickness borderThickness;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color backgroundColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useBackground = true;

	public bool createUiPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite borderSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite backgroundSprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIPanel panel;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIDrawCall.Clipping clipping;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 clippingSize = new Vector2(-10000f, -10000f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 clippingCenter = new Vector2(-10000f, -10000f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 clippingSoftness;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool disableAutoBackground;

	public Color BackgroundColor
	{
		get
		{
			return backgroundColor;
		}
		set
		{
			backgroundColor = value;
			isDirty = true;
			useBackground = true;
		}
	}

	public string BackgroundSpriteName
	{
		get
		{
			return backgroundSpriteName;
		}
		set
		{
			backgroundSpriteName = value;
			if (value == "")
			{
				backgroundSpriteName = XUi.BlankTexture;
			}
		}
	}

	public Color BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			borderColor = value;
			isDirty = true;
			useBackground = true;
		}
	}

	public XUi_Thickness BorderThickness
	{
		get
		{
			return borderThickness;
		}
		set
		{
			borderThickness = value;
			isDirty = true;
			useBackground = true;
		}
	}

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
				isDirty = true;
			}
		}
	}

	public Vector2 ClippingSize
	{
		get
		{
			return clippingSize;
		}
		set
		{
			if (value != clippingSize)
			{
				clippingSize = value;
				isDirty = true;
			}
		}
	}

	public Vector2 ClippingCenter
	{
		get
		{
			return clippingCenter;
		}
		set
		{
			if (value != clippingCenter)
			{
				clippingCenter = value;
				isDirty = true;
			}
		}
	}

	public Vector2 ClippingSoftness
	{
		get
		{
			return clippingSoftness;
		}
		set
		{
			if (value != clippingSoftness)
			{
				clippingSoftness = value;
				isDirty = true;
			}
		}
	}

	public override bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
			RefreshEnabled();
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool UseGlobalBackgroundOpacity { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshEnabled()
	{
		if (panel != null)
		{
			panel.gameObject.SetActive(enabled);
			panel.enabled = enabled;
			borderSprite.Enabled = enabled;
			backgroundSprite.Enabled = enabled;
		}
	}

	public XUiV_Panel(string _id)
		: base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UIPanel>();
	}

	public override void InitView()
	{
		borderSprite = new XUiV_Sprite("_border");
		borderSprite.xui = base.xui;
		borderSprite.Controller = new XUiController(base.Controller);
		borderSprite.Controller.xui = base.xui;
		backgroundSprite = new XUiV_Sprite("_background");
		backgroundSprite.xui = base.xui;
		backgroundSprite.Controller = new XUiController(base.Controller);
		backgroundSprite.Controller.xui = base.xui;
		if (!disableAutoBackground && useBackground)
		{
			borderSprite.GlobalOpacityModifier = 0f;
			borderSprite.Position = new Vector2i(size.x / 2, -size.y / 2);
			borderSprite.SetDefaults(base.Controller);
			borderSprite.Size = size;
			borderSprite.UIAtlas = "UIAtlas";
			borderSprite.SpriteName = XUi.BlankTexture;
			borderSprite.Color = borderColor;
			borderSprite.Depth = depth;
			borderSprite.Pivot = UIWidget.Pivot.TopLeft;
			borderSprite.Type = UIBasicSprite.Type.Sliced;
			borderSprite.Controller.WindowGroup = base.Controller.WindowGroup;
			backgroundSprite.Position = new Vector2i(backgroundSprite.Size.x / 2 + borderThickness.left, -(backgroundSprite.Size.y / 2 + borderThickness.top));
			backgroundSprite.SetDefaults(base.Controller);
			backgroundSprite.Size = size;
			backgroundSprite.UIAtlas = "UIAtlas";
			backgroundSprite.SpriteName = backgroundSpriteName;
			backgroundSprite.Color = backgroundColor;
			backgroundSprite.Depth = depth;
			backgroundSprite.Pivot = UIWidget.Pivot.TopLeft;
			backgroundSprite.Type = UIBasicSprite.Type.Sliced;
			if (backgroundSpriteName != "")
			{
				backgroundSprite.GlobalOpacityModifier = 2f;
			}
			backgroundSprite.Controller.WindowGroup = base.Controller.WindowGroup;
			if (borderColor != new Color32(0, 0, 0, 0))
			{
				backgroundSprite.Size = new Vector2i(base.Size.x - (borderThickness.left + borderThickness.right), base.Size.y - (borderThickness.top + borderThickness.bottom));
			}
		}
		base.InitView();
		panel = uiTransform.gameObject.GetComponent<UIPanel>();
		if (!createUiPanel && clipping == UIDrawCall.Clipping.None)
		{
			Object.Destroy(panel);
			panel = null;
		}
		else
		{
			if (createUiPanel)
			{
				panel.enabled = true;
				panel.depth = depth;
			}
			if (clipping != UIDrawCall.Clipping.None)
			{
				panel.enabled = true;
				if (clippingCenter == new Vector2(-10000f, -10000f))
				{
					clippingCenter = new Vector2(size.x / 2, -size.y / 2);
				}
				if (clippingSize == new Vector2(-10000f, -10000f))
				{
					clippingSize = new Vector2(size.x, size.y);
				}
				updateClipping();
			}
		}
		BoxCollider boxCollider = collider;
		if (boxCollider != null)
		{
			float x = (float)size.x * 0.5f;
			float num = (float)size.y * 0.5f;
			boxCollider.center = new Vector3(x, 0f - num, 0f);
			boxCollider.size = new Vector3((float)size.x * colliderScale, (float)size.y * colliderScale, 0f);
		}
		if (!disableAutoBackground && useBackground && backgroundSprite != null && borderSprite != null)
		{
			backgroundSprite.Color = backgroundColor;
			borderSprite.Color = borderColor;
		}
		RefreshEnabled();
		isDirty = true;
		UseGlobalBackgroundOpacity = true;
	}

	public override void UpdateData()
	{
		if (isDirty)
		{
			if (!disableAutoBackground && useBackground && backgroundSprite != null)
			{
				borderSprite.FillCenter = false;
				borderSprite.Size = size;
				borderSprite.Position = new Vector2i(0, 0);
				borderSprite.Color = borderColor;
				borderSprite.GlobalOpacityModifier = (UseGlobalBackgroundOpacity ? 1 : 0);
				backgroundSprite.Size = size;
				backgroundSprite.Position = new Vector2i(0, 0);
				backgroundSprite.Color = backgroundColor;
				backgroundSprite.SpriteName = backgroundSpriteName;
				backgroundSprite.GlobalOpacityModifier = (UseGlobalBackgroundOpacity ? 1 : 0);
				if (borderColor != new Color32(0, 0, 0, 0))
				{
					backgroundSprite.Size = new Vector2i(base.Size.x - (borderThickness.left + borderThickness.right), base.Size.y - (borderThickness.top + borderThickness.bottom));
					backgroundSprite.Position = new Vector2i(borderThickness.left, -borderThickness.top);
				}
			}
			if (panel != null)
			{
				panel.depth = depth;
			}
			updateClipping();
		}
		base.UpdateData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateClipping()
	{
		if (clipping != UIDrawCall.Clipping.None)
		{
			if (panel.clipping != clipping)
			{
				panel.clipping = clipping;
			}
			if (panel.clipSoftness != clippingSoftness)
			{
				panel.clipSoftness = clippingSoftness;
			}
			if (clippingSize.x < 0f)
			{
				clippingSize.x = 0f;
			}
			if (clippingSize.y < 0f)
			{
				clippingSize.y = 0f;
			}
			Vector4 vector = new Vector4(clippingCenter.x, clippingCenter.y, clippingSize.x, clippingSize.y);
			if (panel.baseClipRegion != vector)
			{
				panel.baseClipRegion = vector;
			}
		}
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		backgroundColor = new Color32(96, 96, 96, byte.MaxValue);
		borderColor = new Color32(0, 0, 0, 0);
		borderThickness = new XUi_Thickness(3);
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			flag = true;
			switch (attribute)
			{
			case "backgroundcolor":
				BackgroundColor = StringParsers.ParseColor32(value);
				break;
			case "backgroundspritename":
				BackgroundSpriteName = value;
				break;
			case "bordercolor":
				BorderColor = StringParsers.ParseColor32(value);
				break;
			case "borderthickness":
				BorderThickness = XUi_Thickness.Parse(value);
				break;
			case "disableautobackground":
				disableAutoBackground = StringParsers.ParseBool(value);
				break;
			case "clipping":
				clipping = EnumUtils.Parse<UIDrawCall.Clipping>(value, _ignoreCase: true);
				break;
			case "clippingsize":
				clippingSize = StringParsers.ParseVector2(value);
				break;
			case "clippingcenter":
				clippingCenter = StringParsers.ParseVector2(value);
				break;
			case "clippingsoftness":
				clippingSoftness = StringParsers.ParseVector2(value);
				break;
			case "createuipanel":
				createUiPanel = StringParsers.ParseBool(value);
				break;
			case "snapcursor":
				EventOnHover = true;
				break;
			default:
				flag = false;
				break;
			}
		}
		return flag;
	}
}

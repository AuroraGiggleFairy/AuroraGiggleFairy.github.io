using UnityEngine;

public class XUiV_Button : XUiV_ImageBased
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string uiAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite sprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string defaultSpriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string hoverSpriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string selectedSpriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string disabledSpriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color defaultSpriteColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color hoverSpriteColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color selectedSpriteColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color disabledSpriteColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool manualColors;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color currentColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string currentSpriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool selected;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool colorDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hoverScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hoverDuration = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenScale tweenScale;

	[XuiXmlAttribute("atlas", false)]
	public string UIAtlas
	{
		get
		{
			return uiAtlas;
		}
		set
		{
			uiAtlas = value;
			SetDirty();
		}
	}

	public string DefaultSpriteName
	{
		get
		{
			return defaultSpriteName;
		}
		set
		{
			defaultSpriteName = value;
			SetDirty();
			updateCurrentSprite();
		}
	}

	public Color DefaultSpriteColor
	{
		get
		{
			return defaultSpriteColor;
		}
		set
		{
			defaultSpriteColor = value;
			SetDirty();
			updateCurrentSprite();
		}
	}

	[XuiXmlAttribute("hoversprite", false)]
	public string HoverSpriteName
	{
		get
		{
			if (!(hoverSpriteName == ""))
			{
				return hoverSpriteName;
			}
			return defaultSpriteName;
		}
		set
		{
			hoverSpriteName = value;
			SetDirty();
			updateCurrentSprite();
		}
	}

	[XuiXmlAttribute("hovercolor", false)]
	public Color HoverSpriteColor
	{
		get
		{
			return hoverSpriteColor;
		}
		set
		{
			hoverSpriteColor = value;
			SetDirty();
			updateCurrentSprite();
		}
	}

	[XuiXmlAttribute("selectedsprite", false)]
	public string SelectedSpriteName
	{
		get
		{
			if (!(selectedSpriteName == ""))
			{
				return selectedSpriteName;
			}
			return defaultSpriteName;
		}
		set
		{
			selectedSpriteName = value;
			SetDirty();
			updateCurrentSprite();
		}
	}

	[XuiXmlAttribute("selectedcolor", false)]
	public Color SelectedSpriteColor
	{
		get
		{
			return selectedSpriteColor;
		}
		set
		{
			selectedSpriteColor = value;
			SetDirty();
			updateCurrentSprite();
		}
	}

	[XuiXmlAttribute("disabledsprite", false)]
	public string DisabledSpriteName
	{
		get
		{
			if (!(disabledSpriteName == ""))
			{
				return disabledSpriteName;
			}
			return defaultSpriteName;
		}
		set
		{
			disabledSpriteName = value;
			SetDirty();
			updateCurrentSprite();
		}
	}

	[XuiXmlAttribute("disabledcolor", false)]
	public Color DisabledSpriteColor
	{
		get
		{
			return disabledSpriteColor;
		}
		set
		{
			disabledSpriteColor = value;
			SetDirty();
			updateCurrentSprite();
		}
	}

	[XuiXmlAttribute("manualcolors", false)]
	public bool ManualColors
	{
		get
		{
			return manualColors;
		}
		set
		{
			if (value != manualColors)
			{
				manualColors = value;
				SetDirty();
				updateCurrentSprite();
			}
		}
	}

	public Color CurrentColor
	{
		get
		{
			return currentColor;
		}
		set
		{
			currentColor = value;
			SetDirty();
			colorDirty = true;
		}
	}

	public string CurrentSpriteName
	{
		get
		{
			return currentSpriteName;
		}
		set
		{
			if (!(value == currentSpriteName))
			{
				currentSpriteName = value;
				SetDirty();
				colorDirty = true;
			}
		}
	}

	[XuiXmlAttribute("selected", false)]
	public bool Selected
	{
		get
		{
			return selected;
		}
		set
		{
			if (selected != value)
			{
				selected = value;
				SetDirty();
				updateCurrentSprite();
			}
		}
	}

	[XuiXmlAttribute("hoverscale", false)]
	public float HoverScale
	{
		get
		{
			return hoverScale;
		}
		set
		{
			hoverScale = value;
			SetDirty();
		}
	}

	public override bool Enabled
	{
		set
		{
			bool flag = enabled;
			base.Enabled = value;
			if (value != flag)
			{
				updateCurrentSprite();
				if (!value)
				{
					tweenScale?.PlayReverse();
				}
				if (!gamepadSelectableSetFromAttributes)
				{
					base.IsNavigatable = value;
				}
			}
		}
	}

	public XUiV_Button(XUi _xui, string _id)
		: base(_xui, _id)
	{
		base.UseSelectionBox = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCurrentSprite()
	{
		if (Enabled)
		{
			if (Selected)
			{
				if (!manualColors)
				{
					CurrentColor = selectedSpriteColor;
				}
				CurrentSpriteName = SelectedSpriteName;
			}
			else
			{
				if (!manualColors)
				{
					CurrentColor = (isOver ? hoverSpriteColor : defaultSpriteColor);
				}
				CurrentSpriteName = (isOver ? HoverSpriteName : DefaultSpriteName);
			}
		}
		else
		{
			if (!manualColors)
			{
				CurrentColor = disabledSpriteColor;
			}
			CurrentSpriteName = DisabledSpriteName;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UISprite>();
		_go.AddComponent<TweenScale>().enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		widget = (sprite = uiTransform.GetComponent<UISprite>());
		tweenScale = uiTransform.GetComponent<TweenScale>();
	}

	public override void InitView()
	{
		base.EventOnPress = true;
		base.EventOnHover = true;
		base.InitView();
		updateData();
		Enabled = true;
	}

	public override void Update(float _dt)
	{
		if (isOver && !base.UiTransformIsHovered)
		{
			OnHover(_isOver: false);
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		sprite.spriteName = currentSpriteName;
		sprite.atlas = xui.GetAtlasByName(uiAtlas, currentSpriteName);
		sprite.color = opacityModColor(currentColor);
		if (!Mathf.Approximately(hoverScale, 1f))
		{
			tweenScale.from = Vector3.one;
			tweenScale.to = Vector3.one * hoverScale;
			tweenScale.duration = hoverDuration;
		}
		sprite.centerType = UIBasicSprite.AdvancedType.Sliced;
		sprite.type = type;
		sprite.flip = flip;
		base.updateData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHover(bool _isOver)
	{
		base.OnHover(_isOver);
		updateCurrentSprite();
		if (Enabled && !Mathf.Approximately(hoverScale, 1f))
		{
			tweenScale.Play(_isOver);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		updateCurrentSprite();
		uiTransform.localScale = Vector3.one;
	}

	[XuiXmlAttribute("sprite", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSprite(string _value)
	{
		DefaultSpriteName = _value;
		CurrentSpriteName = _value;
	}

	[XuiXmlAttribute("defaultcolor", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSprite(Color _value)
	{
		DefaultSpriteColor = _value;
		CurrentColor = defaultSpriteColor;
	}
}

using UnityEngine;

public class XUiV_Button : XUiView
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string uiAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite sprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Type type;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Flip flip;

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
	public bool foregroundLayer = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenScale tweenScale;

	[PublicizedFrom(EAccessModifier.Private)]
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
			uiAtlas = value;
			isDirty = true;
		}
	}

	public UIBasicSprite.Type Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
			isDirty = true;
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
			isDirty = true;
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
			isDirty = true;
			updateCurrentSprite();
		}
	}

	public string HoverSpriteName
	{
		get
		{
			if (hoverSpriteName == "")
			{
				return defaultSpriteName;
			}
			return hoverSpriteName;
		}
		set
		{
			hoverSpriteName = value;
			isDirty = true;
			updateCurrentSprite();
		}
	}

	public Color HoverSpriteColor
	{
		get
		{
			return hoverSpriteColor;
		}
		set
		{
			hoverSpriteColor = value;
			isDirty = true;
			updateCurrentSprite();
		}
	}

	public string SelectedSpriteName
	{
		get
		{
			if (selectedSpriteName == "")
			{
				return defaultSpriteName;
			}
			return selectedSpriteName;
		}
		set
		{
			selectedSpriteName = value;
			isDirty = true;
			updateCurrentSprite();
		}
	}

	public Color SelectedSpriteColor
	{
		get
		{
			return selectedSpriteColor;
		}
		set
		{
			selectedSpriteColor = value;
			isDirty = true;
			updateCurrentSprite();
		}
	}

	public string DisabledSpriteName
	{
		get
		{
			if (disabledSpriteName == "")
			{
				return defaultSpriteName;
			}
			return disabledSpriteName;
		}
		set
		{
			disabledSpriteName = value;
			isDirty = true;
			updateCurrentSprite();
		}
	}

	public Color DisabledSpriteColor
	{
		get
		{
			return disabledSpriteColor;
		}
		set
		{
			disabledSpriteColor = value;
			isDirty = true;
			updateCurrentSprite();
		}
	}

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
				isDirty = true;
				updateCurrentSprite();
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

	public Color CurrentColor
	{
		get
		{
			return currentColor;
		}
		set
		{
			currentColor = value;
			isDirty = true;
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
			if (value != currentSpriteName)
			{
				currentSpriteName = value;
				isDirty = true;
				colorDirty = true;
			}
		}
	}

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
				isDirty = true;
				updateCurrentSprite();
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
			globalOpacityModifier = value;
			isDirty = true;
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

	public float HoverScale
	{
		get
		{
			return hoverScale;
		}
		set
		{
			hoverScale = value;
			isDirty = true;
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
				if (!value && hoverScale != 1f && tweenScale.value != Vector3.one)
				{
					tweenScale.SetStartToCurrentValue();
					tweenScale.to = Vector3.one;
					tweenScale.enabled = true;
					tweenScale.duration = 0.25f;
					tweenScale.ResetToBeginning();
				}
				if (!gamepadSelectableSetFromAttributes)
				{
					base.IsNavigatable = value;
				}
			}
		}
	}

	public XUiV_Button(string _id)
		: base(_id)
	{
		UseSelectionBox = false;
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
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UISprite>();
		_go.AddComponent<TweenScale>().enabled = false;
	}

	public override void InitView()
	{
		EventOnPress = true;
		EventOnHover = true;
		base.InitView();
		sprite = uiTransform.GetComponent<UISprite>();
		UpdateData();
		initialized = true;
		Enabled = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (null != sprite)
		{
			bool flag = sprite.isVisible;
			if (lastVisible != flag)
			{
				isDirty = true;
			}
			lastVisible = flag;
			if (isOver && UICamera.hoveredObject != uiTransform.gameObject)
			{
				OnHover(uiTransform.gameObject, _isOver: false);
			}
		}
	}

	public override void UpdateData()
	{
		sprite.spriteName = currentSpriteName;
		sprite.atlas = base.xui.GetAtlasByName(uiAtlas, currentSpriteName);
		sprite.color = currentColor;
		if (globalOpacityModifier != 0f && (foregroundLayer ? (base.xui.ForegroundGlobalOpacity < 1f) : (base.xui.BackgroundGlobalOpacity < 1f)))
		{
			float a = Mathf.Clamp01(currentColor.a * (globalOpacityModifier * (foregroundLayer ? base.xui.ForegroundGlobalOpacity : base.xui.BackgroundGlobalOpacity)));
			sprite.color = new Color(currentColor.r, currentColor.g, currentColor.b, a);
		}
		if (borderSize > 0)
		{
			sprite.border = new Vector4(borderSize, borderSize, borderSize, borderSize);
		}
		sprite.centerType = UIBasicSprite.AdvancedType.Sliced;
		sprite.type = type;
		parseAnchors(sprite);
		if (sprite.flip != flip)
		{
			sprite.flip = flip;
		}
		if (hoverScale != 1f && tweenScale == null)
		{
			tweenScale = uiTransform.gameObject.GetComponent<TweenScale>();
		}
		if (!initialized)
		{
			sprite.pivot = pivot;
			sprite.depth = depth;
			uiTransform.localScale = Vector3.one;
			uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
			BoxCollider boxCollider = collider;
			boxCollider.center = sprite.localCenter;
			boxCollider.size = new Vector3(sprite.localSize.x * colliderScale, sprite.localSize.y * colliderScale, 0f);
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

	public override void OnHover(GameObject _go, bool _isOver)
	{
		base.OnHover(_go, _isOver);
		updateCurrentSprite();
		if (Enabled && hoverScale != 1f)
		{
			tweenScale.to = (isOver ? (Vector3.one * hoverScale) : Vector3.one);
			tweenScale.SetStartToCurrentValue();
			tweenScale.duration = 0.25f;
			tweenScale.ResetToBeginning();
			tweenScale.enabled = true;
		}
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
				DefaultSpriteName = value;
				CurrentSpriteName = value;
				break;
			case "defaultcolor":
				DefaultSpriteColor = StringParsers.ParseColor32(value);
				CurrentColor = defaultSpriteColor;
				break;
			case "hoversprite":
				HoverSpriteName = value;
				break;
			case "hovercolor":
				HoverSpriteColor = StringParsers.ParseColor32(value);
				break;
			case "selectedsprite":
				SelectedSpriteName = value;
				break;
			case "selectedcolor":
				SelectedSpriteColor = StringParsers.ParseColor32(value);
				break;
			case "disabledsprite":
				DisabledSpriteName = value;
				break;
			case "disabledcolor":
				DisabledSpriteColor = StringParsers.ParseColor32(value);
				break;
			case "manualcolors":
				ManualColors = StringParsers.ParseBool(value);
				break;
			case "selected":
				Selected = StringParsers.ParseBool(value);
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
			case "hoverscale":
				HoverScale = StringParsers.ParseFloat(value);
				break;
			case "flip":
				Flip = EnumUtils.Parse<UIBasicSprite.Flip>(value, _ignoreCase: true);
				break;
			case "foregroundlayer":
				foregroundLayer = StringParsers.ParseBool(value);
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		updateCurrentSprite();
		uiTransform.localScale = Vector3.one;
	}
}

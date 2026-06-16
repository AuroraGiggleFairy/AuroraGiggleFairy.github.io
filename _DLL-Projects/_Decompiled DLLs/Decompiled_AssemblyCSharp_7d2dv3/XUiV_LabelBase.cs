using System.Collections.Generic;
using UnityEngine;

public abstract class XUiV_LabelBase : XUiView_WidgetBased
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel label;

	[PublicizedFrom(EAccessModifier.Protected)]
	public NGUIFont uiFont;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int fontSize;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int spacingX = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int spacingY;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Effect effect;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color effectColor = new Color32(0, 0, 0, 80);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 effectDistance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Crispness crispness;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color color;

	[PublicizedFrom(EAccessModifier.Protected)]
	public NGUIText.Alignment alignment;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool upperCase;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lowerCase;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool supportBbCode = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool supportUrls;

	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<string> supportedUrlTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hadTooltipTextFromUrlHover;

	public NGUIFont UIFont
	{
		get
		{
			return uiFont;
		}
		set
		{
			uiFont = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("font_size", false)]
	public int FontSize
	{
		get
		{
			return fontSize;
		}
		set
		{
			fontSize = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("spacing_x", false)]
	public int SpacingX
	{
		get
		{
			return spacingX;
		}
		set
		{
			if (value != spacingX)
			{
				spacingX = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("spacing_y", false)]
	public int SpacingY
	{
		get
		{
			return spacingY;
		}
		set
		{
			if (value != spacingY)
			{
				spacingY = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("effect", false)]
	public UILabel.Effect Effect
	{
		get
		{
			return effect;
		}
		set
		{
			effect = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("effect_color", false)]
	public Color EffectColor
	{
		get
		{
			return effectColor;
		}
		set
		{
			effectColor = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("effect_distance", false)]
	public Vector2 EffectDistance
	{
		get
		{
			return effectDistance;
		}
		set
		{
			effectDistance = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("crispness", false)]
	public UILabel.Crispness Crispness
	{
		get
		{
			return crispness;
		}
		set
		{
			crispness = value;
			SetDirty();
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

	public float Alpha
	{
		get
		{
			return color.a;
		}
		set
		{
			if (!Mathf.Approximately(color.a, value))
			{
				color.a = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("justify", false)]
	public NGUIText.Alignment Alignment
	{
		get
		{
			return alignment;
		}
		set
		{
			if (alignment != value)
			{
				alignment = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("upper_case", false)]
	public bool UpperCase
	{
		get
		{
			return upperCase;
		}
		set
		{
			if (upperCase != value)
			{
				upperCase = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("lower_case", false)]
	public bool LowerCase
	{
		get
		{
			return lowerCase;
		}
		set
		{
			if (lowerCase != value)
			{
				lowerCase = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("support_bb_code", false)]
	public bool SupportBbCode
	{
		get
		{
			return supportBbCode;
		}
		set
		{
			if (supportBbCode != value)
			{
				supportBbCode = value;
				SetDirty();
			}
		}
	}

	public Vector2 PrintedSize => label.printedSize;

	public XUiV_LabelBase(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UILabel>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		widget = (label = uiTransform.GetComponent<UILabel>());
	}

	public override void InitView()
	{
		if (supportUrls)
		{
			base.EventOnPress = true;
			base.EventOnHover = true;
			controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				LabelUrlUtils.HandleLabelUrlClick(this, label, supportedUrlTypes);
			};
		}
		base.InitView();
		label.symbolDepth = depth + 1;
		updateData();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		Alignment = NGUIText.Alignment.Left;
		FontSize = 16;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHover(bool _isOver)
	{
		if (!supportUrls)
		{
			base.OnHover(_isOver);
			return;
		}
		if (!_isOver)
		{
			base.ToolTip = "";
			hadTooltipTextFromUrlHover = false;
		}
		else
		{
			updateUrlTooltip();
		}
		base.OnHover(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateUrlTooltip()
	{
		LabelUrlUtils.HandleLabelUrlHover(this, label, supportedUrlTypes, out var _isOverUrl, out var _tooltipText);
		if (!_isOverUrl)
		{
			if (hadTooltipTextFromUrlHover)
			{
				base.ToolTip = "";
				hadTooltipTextFromUrlHover = false;
			}
		}
		else if (!string.IsNullOrEmpty(_tooltipText))
		{
			base.ToolTip = _tooltipText;
			hadTooltipTextFromUrlHover = true;
		}
		else if (hadTooltipTextFromUrlHover)
		{
			base.ToolTip = "";
			hadTooltipTextFromUrlHover = false;
		}
	}

	public override void Update(float _dt)
	{
		if (isOver)
		{
			updateUrlTooltip();
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		if (uiFont != null)
		{
			label.font = uiFont;
		}
		label.fontSize = fontSize;
		label.supportEncoding = supportBbCode;
		label.color = color;
		label.alignment = alignment;
		label.keepCrispWhenShrunk = crispness;
		label.effectStyle = effect;
		label.effectColor = effectColor;
		label.effectDistance = effectDistance;
		label.spacingX = spacingX;
		label.spacingY = spacingY;
		base.updateData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string getFormattedText(string _text)
	{
		if (upperCase)
		{
			_text = _text.ToUpperWithUserLocale();
		}
		else if (lowerCase)
		{
			_text = _text.ToLowerWithUserLocale();
		}
		return _text;
	}

	public void BindToUiInput(UIInput _input)
	{
		_input.label = label;
	}

	[XuiXmlAttribute("font_face", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeFontFace(string _value)
	{
		UIFont = xui.GetUIFontByName(_value, _showWarning: false);
		if (UIFont == null)
		{
			Log.Warning("[XUi] Label: Font not found: '" + _value + "', hierarchy: " + GetXuiHierarchy());
		}
	}

	[XuiXmlAttribute("support_urls", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSupportUrls(string _value)
	{
		if (_value.EqualsCaseInsensitive("false"))
		{
			supportUrls = false;
			return;
		}
		supportUrls = true;
		if (_value.EqualsCaseInsensitive("true"))
		{
			supportedUrlTypes = new HashSet<string> { "HTTP" };
		}
		else
		{
			supportedUrlTypes = new HashSet<string>(_value.Split(","));
		}
	}
}

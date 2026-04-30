using System;
using System.Collections.Generic;
using System.Globalization;
using Platform;
using UnityEngine;

public class XUiV_Label : XUiView
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static TextInfo textInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel label;

	[PublicizedFrom(EAccessModifier.Protected)]
	public NGUIFont uiFont;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int fontSize;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Overflow overflow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int overflowHeight;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int overflowWidth;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Effect effect;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color effectColor = new Color32(0, 0, 0, 80);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 effectDistance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Crispness crispness;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string text;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color color;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int maxLineCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int spacingX = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int spacingY;

	[PublicizedFrom(EAccessModifier.Protected)]
	public new NGUIText.Alignment alignment;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool supportBbCode = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool upperCase;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lowerCase;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool parseActions;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string actionsDefaultFormat;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool currentTextHasActions;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool supportUrls;

	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<string> supportedUrlTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bUpdateText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiUtils.ForceLabelInputStyle forceInputStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public float globalOpacityModifier = 1f;

	public UILabel Label
	{
		get
		{
			return label;
		}
		set
		{
			label = value;
			isDirty = true;
		}
	}

	public NGUIFont UIFont
	{
		get
		{
			return uiFont;
		}
		set
		{
			uiFont = value;
			isDirty = true;
		}
	}

	public int FontSize
	{
		get
		{
			return fontSize;
		}
		set
		{
			fontSize = value;
			isDirty = true;
		}
	}

	public UILabel.Overflow Overflow
	{
		get
		{
			return overflow;
		}
		set
		{
			overflow = value;
			isDirty = true;
		}
	}

	public int OverflowHeight
	{
		get
		{
			return overflowHeight;
		}
		set
		{
			overflowHeight = value;
			isDirty = true;
		}
	}

	public int OverflowWidth
	{
		get
		{
			return overflowWidth;
		}
		set
		{
			overflowWidth = value;
			isDirty = true;
		}
	}

	public UILabel.Effect Effect
	{
		get
		{
			return effect;
		}
		set
		{
			effect = value;
			isDirty = true;
		}
	}

	public Color EffectColor
	{
		get
		{
			return effectColor;
		}
		set
		{
			effectColor = value;
			isDirty = true;
		}
	}

	public Vector2 EffectDistance
	{
		get
		{
			return effectDistance;
		}
		set
		{
			effectDistance = value;
			isDirty = true;
		}
	}

	public UILabel.Crispness Crispness
	{
		get
		{
			return crispness;
		}
		set
		{
			crispness = value;
			isDirty = true;
		}
	}

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			if (text != value)
			{
				text = value;
				isDirty = true;
				bUpdateText = true;
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
			if (color != value)
			{
				color = value;
				isDirty = true;
			}
		}
	}

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
				isDirty = true;
			}
		}
	}

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
				isDirty = true;
			}
		}
	}

	public int MaxLineCount
	{
		get
		{
			return maxLineCount;
		}
		set
		{
			if (value != maxLineCount)
			{
				maxLineCount = value;
				isDirty = true;
			}
		}
	}

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
				isDirty = true;
			}
		}
	}

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
			globalOpacityModifier = value;
			isDirty = true;
		}
	}

	public Bounds LabelBounds => label.CalculateBounds();

	[PublicizedFrom(EAccessModifier.Private)]
	static XUiV_Label()
	{
		textInfo = Utils.StandardCulture.TextInfo;
		Localization.LanguageSelected += OnLanguageSelected;
		OnLanguageSelected(Localization.RequestedLanguage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnLanguageSelected(string _lang)
	{
		string text = Localization.Get("cultureInfoName");
		if (string.IsNullOrEmpty(text))
		{
			Log.Warning("No culture info name given for selected language: " + _lang);
			return;
		}
		TextInfo textInfo;
		try
		{
			textInfo = CultureInfo.GetCultureInfo(text).TextInfo;
		}
		catch (Exception)
		{
			Log.Warning("No culture info found for given name: " + text + " (language: " + _lang + ")");
			return;
		}
		if (textInfo.CultureName != XUiV_Label.textInfo.CultureName)
		{
			XUiV_Label.textInfo = textInfo;
			Log.Out("Updated culture for display texts");
		}
	}

	public XUiV_Label(string _id)
		: base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UILabel>();
	}

	public override void InitView()
	{
		if (supportUrls)
		{
			EventOnPress = true;
			controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				XUiUtils.HandleLabelUrlClick(this, label, supportedUrlTypes);
			};
		}
		base.InitView();
		label = uiTransform.GetComponent<UILabel>();
		if (UIFont != null)
		{
			UpdateData();
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += OnLastInputStyleChanged;
		initialized = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (PlatformManager.NativePlatform?.Input != null)
		{
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= OnLastInputStyleChanged;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.GlobalOpacityChanged)
		{
			isDirty = true;
		}
	}

	public override void UpdateData()
	{
		base.UpdateData();
		if (uiFont != null)
		{
			label.font = uiFont;
		}
		label.depth = depth;
		label.symbolDepth = depth + 1;
		label.fontSize = fontSize;
		parseAnchors(label, label.overflowMethod != UILabel.Overflow.ResizeFreely);
		label.supportEncoding = supportBbCode;
		if (text != null && bUpdateText)
		{
			label.text = GetFormattedText(text);
			bUpdateText = false;
		}
		label.supportEncoding = supportBbCode;
		label.color = color;
		label.alignment = alignment;
		label.keepCrispWhenShrunk = crispness;
		label.effectStyle = effect;
		label.effectColor = effectColor;
		label.effectDistance = effectDistance;
		label.overflowMethod = overflow;
		label.overflowWidth = overflowWidth;
		label.overflowHeight = overflowHeight;
		label.spacingX = spacingX;
		label.spacingY = spacingY;
		label.maxLineCount = maxLineCount;
		if (!initialized)
		{
			label.pivot = pivot;
			uiTransform.localScale = Vector3.one;
			uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
			if (EventOnHover || EventOnPress)
			{
				BoxCollider boxCollider = collider;
				boxCollider.center = Label.localCenter;
				boxCollider.size = new Vector3(label.localSize.x * colliderScale, label.localSize.y * colliderScale, 0f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _obj)
	{
		if (parseActions && currentTextHasActions)
		{
			ForceTextUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetFormattedText(string _text)
	{
		if (upperCase)
		{
			_text = textInfo.ToUpper(_text);
		}
		else if (lowerCase)
		{
			_text = textInfo.ToLower(_text);
		}
		if (parseActions)
		{
			currentTextHasActions = XUiUtils.ParseActionsMarkup(base.xui, _text, out _text, actionsDefaultFormat, forceInputStyle);
		}
		return _text;
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		Alignment = NGUIText.Alignment.Left;
		FontSize = 16;
		overflow = UILabel.Overflow.ShrinkContent;
	}

	public void SetText(string _text)
	{
		Text = _text;
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			switch (attribute)
			{
			case "font_face":
				UIFont = base.xui.GetUIFontByName(value, _showWarning: false);
				if (UIFont == null)
				{
					Log.Warning("XUi Label: Font not found: " + value + ", from: " + base.Controller.GetParentWindow().ID + "." + base.ID);
				}
				break;
			case "font_size":
				FontSize = int.Parse(value);
				break;
			case "color":
				Color = StringParsers.ParseColor32(value);
				break;
			case "text":
				Text = value;
				break;
			case "text_key":
				if (!string.IsNullOrEmpty(value))
				{
					Text = Localization.Get(value);
				}
				break;
			case "justify":
				Alignment = EnumUtils.Parse<NGUIText.Alignment>(value, _ignoreCase: true);
				break;
			case "crispness":
				Crispness = EnumUtils.Parse<UILabel.Crispness>(value, _ignoreCase: true);
				break;
			case "effect":
				Effect = EnumUtils.Parse<UILabel.Effect>(value, _ignoreCase: true);
				break;
			case "effect_color":
				EffectColor = StringParsers.ParseColor32(value);
				break;
			case "effect_distance":
				EffectDistance = StringParsers.ParseVector2(value);
				break;
			case "overflow":
				Overflow = EnumUtils.Parse<UILabel.Overflow>(value, _ignoreCase: true);
				break;
			case "overflow_width":
				OverflowWidth = StringParsers.ParseSInt32(value);
				break;
			case "overflow_height":
				OverflowHeight = StringParsers.ParseSInt32(value);
				break;
			case "upper_case":
				upperCase = StringParsers.ParseBool(value);
				break;
			case "lower_case":
				lowerCase = StringParsers.ParseBool(value);
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
			case "support_bb_code":
				supportBbCode = StringParsers.ParseBool(value);
				break;
			case "parse_actions":
				parseActions = StringParsers.ParseBool(value);
				break;
			case "actions_default_format":
				actionsDefaultFormat = value;
				break;
			case "support_urls":
				if (value.EqualsCaseInsensitive("false"))
				{
					supportUrls = false;
					break;
				}
				supportUrls = true;
				if (value.EqualsCaseInsensitive("true"))
				{
					supportedUrlTypes = new HashSet<string> { "HTTP" };
				}
				else
				{
					supportedUrlTypes = new HashSet<string>(value.Split(","));
				}
				break;
			case "max_line_count":
				maxLineCount = StringParsers.ParseSInt32(value);
				break;
			case "spacing_x":
				spacingX = StringParsers.ParseSInt32(value);
				break;
			case "spacing_y":
				spacingY = StringParsers.ParseSInt32(value);
				break;
			case "force_input_style":
				forceInputStyle = EnumUtils.Parse<XUiUtils.ForceLabelInputStyle>(value, _ignoreCase: true);
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}

	public void SetTextImmediately(string _text)
	{
		if (label != null)
		{
			text = _text;
			label.text = GetFormattedText(text);
		}
	}

	public void ForceTextUpdate()
	{
		bUpdateText = true;
		isDirty = true;
	}
}

using System.Collections;
using Platform;
using UnityEngine;

public class XUiV_Label : XUiV_LabelBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Overflow overflow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int overflowHeight;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int overflowWidth;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool overflowEllipsis;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string text = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int maxLineCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool parseActions;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string actionsDefaultFormat;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool currentTextHasActions;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiUtils.ForceLabelInputStyle forceInputStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bUpdateText;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useEllipsisAnimator;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator ellipsisAnimator;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly WaitForSeconds markChangedDelay = new WaitForSeconds(0.01f);

	[XuiXmlAttribute("overflow", false)]
	public UILabel.Overflow Overflow
	{
		get
		{
			return overflow;
		}
		set
		{
			overflow = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("overflow_height", false)]
	public int OverflowHeight
	{
		get
		{
			return overflowHeight;
		}
		set
		{
			overflowHeight = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("overflow_width", false)]
	public int OverflowWidth
	{
		get
		{
			return overflowWidth;
		}
		set
		{
			overflowWidth = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("overflow_ellipsis", false)]
	public bool OverflowEllipsis
	{
		get
		{
			return overflowEllipsis;
		}
		set
		{
			overflowEllipsis = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("parse_actions", false)]
	public bool ParseActions
	{
		get
		{
			return parseActions;
		}
		set
		{
			if (parseActions != value)
			{
				parseActions = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("actions_default_format", false)]
	public string ActionsDefaultFormat
	{
		get
		{
			return actionsDefaultFormat;
		}
		set
		{
			if (!(actionsDefaultFormat == value))
			{
				actionsDefaultFormat = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("force_input_style", false)]
	public XUiUtils.ForceLabelInputStyle ForceInputStyle
	{
		get
		{
			return forceInputStyle;
		}
		set
		{
			if (forceInputStyle != value)
			{
				forceInputStyle = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("text", false)]
	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			if (!(text == value))
			{
				text = value ?? "";
				if (ellipsisAnimator == null)
				{
					SetDirty();
					bUpdateText = true;
				}
				else
				{
					ellipsisAnimator.SetBaseString(value);
				}
			}
		}
	}

	[XuiXmlAttribute("max_line_count", false)]
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
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("use_ellipsis_animator", false)]
	public bool UseEllipsisAnimator
	{
		get
		{
			return useEllipsisAnimator;
		}
		set
		{
			if (useEllipsisAnimator != value)
			{
				useEllipsisAnimator = value;
				SetDirty();
			}
		}
	}

	public override bool anchorsKeepSize
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return label.overflowMethod != UILabel.Overflow.ResizeFreely;
		}
	}

	public XUiV_Label(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	public override void InitView()
	{
		base.InitView();
		if (useEllipsisAnimator)
		{
			if (supportBbCode)
			{
				ellipsisAnimator = new TextEllipsisAnimator(text, this, label);
			}
			else
			{
				Log.Warning("[XUi] Not enabling EllipsisAnimator on label, requires support_bb_code to be true. On " + base.Controller.GetParentWindow().ID + "." + base.ID);
			}
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += OnLastInputStyleChanged;
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		overflow = UILabel.Overflow.ShrinkContent;
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
		ellipsisAnimator?.GetNextAnimatedString(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		if (bUpdateText)
		{
			label.text = getFormattedText(text);
			bUpdateText = false;
		}
		label.overflowMethod = overflow;
		label.overflowWidth = overflowWidth;
		label.overflowHeight = overflowHeight;
		label.overflowEllipsis = overflowEllipsis;
		label.maxLineCount = maxLineCount;
		base.updateData();
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
	public override string getFormattedText(string _text)
	{
		_text = base.getFormattedText(_text);
		if (parseActions)
		{
			currentTextHasActions = XUiUtils.ParseActionsMarkup(xui, _text, out _text, actionsDefaultFormat, forceInputStyle);
		}
		return _text;
	}

	public void SetTextImmediately(string _text)
	{
		text = _text ?? "";
		if (!(label == null))
		{
			label.text = getFormattedText(text);
		}
	}

	public void ForceTextUpdate()
	{
		bUpdateText = true;
		SetDirty();
	}

	[XuiXmlAttribute("text_key", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeTextKey(string _value)
	{
		if (!string.IsNullOrEmpty(_value))
		{
			Text = Localization.Get(_value);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (anchoredLeftAndRight || anchoredTopAndBottom || (Overflow != UILabel.Overflow.ShrinkContent && Overflow != UILabel.Overflow.ResizeFreely))
		{
			ThreadManager.StartCoroutine(markLabelChanged());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator markLabelChanged()
	{
		for (int i = 0; i < 7; i++)
		{
			yield return markChangedDelay;
			label.MarkAsChanged();
		}
	}
}

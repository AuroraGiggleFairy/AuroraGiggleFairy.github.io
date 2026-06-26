using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SimpleButton : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button button;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite border;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEnabled = true;

	public string Tag;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fontSizeDefault;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fontSizeHover;

	public Color EnabledLabelColor;

	public Color? HoveredLabelColor;

	public Color DisabledLabelColor;

	public string Text
	{
		get
		{
			if (label == null)
			{
				return null;
			}
			return label.Text;
		}
		set
		{
			if (label != null)
			{
				label.Text = value;
			}
		}
	}

	public string Tooltip
	{
		get
		{
			return button.ToolTip;
		}
		set
		{
			if (button.ToolTip != value)
			{
				button.ToolTip = value;
			}
		}
	}

	public string DisabledToolTip
	{
		get
		{
			return button.DisabledToolTip;
		}
		set
		{
			if (button.DisabledToolTip != value)
			{
				button.DisabledToolTip = value;
			}
		}
	}

	public XUiV_Label Label => label;

	public XUiV_Button Button => button;

	public bool Enabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			if (value != isEnabled || (button != null && value != button.Enabled))
			{
				isEnabled = value;
				if (button != null)
				{
					button.Enabled = value;
				}
				if (label != null)
				{
					label.Color = (value ? EnabledLabelColor : DisabledLabelColor);
					updateLabelFontSize();
					updateLabelFontColor();
				}
				IsDirty = true;
			}
		}
	}

	public bool IsVisible
	{
		get
		{
			if (!button.IsVisible)
			{
				return label.IsVisible;
			}
			return true;
		}
		set
		{
			button.IsVisible = value;
			label.IsVisible = value;
			if (border != null)
			{
				border.IsVisible = value;
			}
		}
	}

	public int FontSizeDefault
	{
		get
		{
			if (fontSizeDefault != 0)
			{
				return fontSizeDefault;
			}
			if (label == null)
			{
				return 0;
			}
			return label.FontSize;
		}
		set
		{
			if (value != fontSizeDefault)
			{
				fontSizeDefault = value;
				updateLabelFontSize();
			}
		}
	}

	public int FontSizeHover
	{
		get
		{
			if (fontSizeHover != 0)
			{
				return fontSizeHover;
			}
			return FontSizeDefault;
		}
		set
		{
			if (value != fontSizeHover)
			{
				fontSizeHover = value;
				updateLabelFontSize();
			}
		}
	}

	public new event XUiEvent_OnPressEventHandler OnPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLabelFontSize()
	{
		if (label != null)
		{
			if (isEnabled)
			{
				label.FontSize = (isOver ? FontSizeHover : FontSizeDefault);
			}
			else
			{
				label.FontSize = FontSizeDefault;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLabelFontColor()
	{
		if (label != null)
		{
			if (isEnabled)
			{
				label.Color = ((isOver && HoveredLabelColor.HasValue) ? HoveredLabelColor.Value : EnabledLabelColor);
			}
			else
			{
				label.Color = DisabledLabelColor;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		button = GetChildById("clickable").ViewComponent as XUiV_Button;
		button.Controller.OnPress += Btn_OnPress;
		button.Controller.OnHover += Btn_OnHover;
		label = GetChildById("btnLabel").ViewComponent as XUiV_Label;
		if (label != null)
		{
			label.Color = EnabledLabelColor;
		}
		XUiController childById = GetChildById("border");
		if (childById != null)
		{
			border = childById.ViewComponent as XUiV_Sprite;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnHover(XUiController _sender, bool _isOver)
	{
		isOver = _isOver;
		updateLabelFontSize();
		updateLabelFontColor();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (isEnabled && this.OnPressed != null)
		{
			this.OnPressed(this, _mouseButton);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (base.ParseAttribute(name, value, _parent))
		{
			return true;
		}
		switch (name)
		{
		case "enabled_font_color":
			EnabledLabelColor = StringParsers.ParseColor32(value);
			break;
		case "hovered_font_color":
			HoveredLabelColor = StringParsers.ParseColor32(value);
			break;
		case "disabled_font_color":
			DisabledLabelColor = StringParsers.ParseColor32(value);
			break;
		case "font_size_default":
			FontSizeDefault = StringParsers.ParseSInt32(value);
			break;
		case "font_size_hover":
			FontSizeHover = StringParsers.ParseSInt32(value);
			break;
		case "button_enabled":
			Enabled = StringParsers.ParseBool(value);
			break;
		default:
			return false;
		}
		return true;
	}
}

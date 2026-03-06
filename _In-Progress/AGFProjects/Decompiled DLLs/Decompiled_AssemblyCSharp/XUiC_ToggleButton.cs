using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ToggleButton : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button button;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool val;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEnabled = true;

	public string Tag;

	public Color EnabledLabelColor;

	public Color DisabledLabelColor;

	public string Label
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
			if (button == null)
			{
				return null;
			}
			return button.ToolTip;
		}
		set
		{
			if (button != null)
			{
				button.ToolTip = value;
			}
		}
	}

	public bool Value
	{
		get
		{
			return val;
		}
		set
		{
			if (value != val)
			{
				val = value;
				IsDirty = true;
			}
		}
	}

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
				}
				IsDirty = true;
			}
		}
	}

	public event XUiEvent_ToggleButtonValueChanged OnValueChanged;

	public override void Init()
	{
		base.Init();
		button = GetChildById("clickable").ViewComponent as XUiV_Button;
		button.Controller.OnPress += Btn_OnPress;
		label = GetChildById("btnLabel").ViewComponent as XUiV_Label;
		if (label != null)
		{
			label.Color = EnabledLabelColor;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (isEnabled)
		{
			val = !val;
			IsDirty = true;
			if (this.OnValueChanged != null)
			{
				this.OnValueChanged(this, val);
			}
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "value")
		{
			value = val.ToString();
			return true;
		}
		return false;
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
		case "disabled_font_color":
			DisabledLabelColor = StringParsers.ParseColor32(value);
			break;
		case "toggle_value":
			Value = StringParsers.ParseBool(value);
			break;
		default:
			return false;
		}
		return true;
	}
}

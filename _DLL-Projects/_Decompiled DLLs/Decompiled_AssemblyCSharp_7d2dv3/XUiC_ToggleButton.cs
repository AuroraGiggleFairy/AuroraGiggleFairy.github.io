using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ToggleButton : XUiController
{
	[XuiBindComponent("btnLabel", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label label;

	[XuiBindComponent("clickable", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Button button;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool value;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEnabled = true;

	public string Tag;

	[XuiXmlAttribute("enabled_font_color", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Color EnabledLabelColor { get; set; }

	[XuiXmlAttribute("disabled_font_color", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Color DisabledLabelColor { get; set; }

	public string Label
	{
		get
		{
			return label?.Text ?? "";
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
			return button?.ToolTip ?? "";
		}
		set
		{
			if (button != null)
			{
				button.ToolTip = value;
			}
		}
	}

	[XuiXmlAttribute("toggle_value", false)]
	[XuiXmlBinding("value")]
	public bool Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value != this.value)
			{
				this.value = value;
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
		button.Controller.OnPress += Btn_OnPress;
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
			value = !value;
			IsDirty = true;
			this.OnValueChanged?.Invoke(this, value);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}
}

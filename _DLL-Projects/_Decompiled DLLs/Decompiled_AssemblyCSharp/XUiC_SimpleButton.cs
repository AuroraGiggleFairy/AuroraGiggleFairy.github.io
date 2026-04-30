using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SimpleButton : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button button;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEnabled = true;

	public string Tag;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	public string Text
	{
		get
		{
			return label?.Text;
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
				IsDirty = true;
			}
		}
	}

	public new event XUiEvent_OnPressEventHandler OnPressed;

	public new event XUiEvent_OnHoverEventHandler OnHovered;

	public override void Init()
	{
		base.Init();
		if (base.ViewComponent is XUiV_Button xUiV_Button)
		{
			button = xUiV_Button;
		}
		else
		{
			if (!(GetChildById("clickable")?.ViewComponent is XUiV_Button xUiV_Button2))
			{
				throw new Exception("SimpleButton requires a Button view (itself or as a child named 'clickable'). WindowGroup: '" + base.WindowGroup.ID + "', element: '" + base.ViewComponent.ID + "'");
			}
			button = xUiV_Button2;
		}
		button.Controller.OnPress += Btn_OnPress;
		button.Controller.OnHover += Btn_OnHover;
		label = GetChildById("btnLabel")?.ViewComponent as XUiV_Label;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnHover(XUiController _sender, bool _isOver)
	{
		isOver = _isOver;
		IsDirty = true;
		this.OnHovered?.Invoke(this, _isOver);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (isEnabled)
		{
			this.OnPressed?.Invoke(this, _mouseButton);
			IsDirty = true;
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
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "enabled":
			_value = Enabled.ToString();
			return true;
		case "selected":
			_value = (Button?.Selected ?? false).ToString();
			return true;
		case "hovered":
			_value = (isOver && isEnabled).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (base.ParseAttribute(_name, _value, _parent))
		{
			return true;
		}
		if (_name == "button_enabled")
		{
			Enabled = StringParsers.ParseBool(_value);
			return true;
		}
		return false;
	}
}

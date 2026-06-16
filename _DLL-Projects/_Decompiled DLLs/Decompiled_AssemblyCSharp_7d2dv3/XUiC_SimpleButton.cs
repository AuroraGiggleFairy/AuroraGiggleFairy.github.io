using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SimpleButton : XUiController
{
	[XuiBindComponent("btnLabel", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label label;

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

	[XuiXmlAttribute("button_enabled", false)]
	[XuiXmlBinding("enabled")]
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

	[XuiXmlBinding("selected")]
	public bool ButtonSelected => Button?.Selected ?? false;

	[XuiXmlBinding("hovered")]
	public bool ButtonHovered
	{
		get
		{
			if (isOver)
			{
				return isEnabled;
			}
			return false;
		}
	}

	public event XUiEvent_OnPressEventHandler OnPressed;

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
				throw new Exception("SimpleButton requires a Button view (itself or as a child named 'clickable'). WindowGroup: '" + base.WindowGroup.Id + "', element: '" + base.ViewComponent.ID + "'");
			}
			button = xUiV_Button2;
		}
		button.Controller.OnPress += Btn_OnPress;
		button.Controller.OnHover += Btn_OnHover;
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
}

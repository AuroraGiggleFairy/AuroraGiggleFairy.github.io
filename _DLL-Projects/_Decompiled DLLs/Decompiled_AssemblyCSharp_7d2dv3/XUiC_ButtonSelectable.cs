using UnityEngine.Scripting;

[Preserve]
public class XUiC_ButtonSelectable : XUiC_Button
{
	public delegate void ButtonSelectedDelegate(XUiC_ButtonSelectable _sender, bool _isSelected);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selected;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool deselectOnDisable;

	[XuiXmlBinding("btn_selected")]
	[XuiXmlAttribute("selected", false)]
	public bool IsSelected
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
				IsDirty = true;
				this.OnButtonSelected?.Invoke(this, selected);
			}
		}
	}

	[XuiXmlAttribute("deselect_on_disable", false)]
	public bool DeselectOnDisable
	{
		get
		{
			return deselectOnDisable;
		}
		set
		{
			if (deselectOnDisable != value)
			{
				deselectOnDisable = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("auto_toggle_selected", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AutoToggleSelected { get; set; } = true;

	public event ButtonSelectedDelegate OnButtonSelected;

	[XuiBindEvent("OnEnabled", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Event_OnEnabled(XUiController _sender, bool _enabled)
	{
		if (!_enabled && deselectOnDisable)
		{
			IsSelected = false;
		}
	}

	public override void Pressed(int _mouseButton)
	{
		if (AutoToggleSelected)
		{
			IsSelected = !IsSelected;
		}
		base.Pressed(_mouseButton);
	}
}

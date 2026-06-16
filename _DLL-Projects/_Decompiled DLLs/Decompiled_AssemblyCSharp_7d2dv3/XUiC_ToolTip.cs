using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ToolTip : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite border;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tooltip = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string tooltipOld = "";

	public float ShowDelaySec = 0.3f;

	public Vector2i PositionOffset = new Vector2i(0, -36);

	[PublicizedFrom(EAccessModifier.Private)]
	public float showDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool show;

	public string ToolTip
	{
		get
		{
			return tooltip;
		}
		set
		{
			if (!(tooltip == value))
			{
				tooltipOld = tooltip;
				if (value == null)
				{
					tooltip = "";
				}
				else if (value.Length > 0 && value[value.Length - 1] == '\n')
				{
					tooltip = value.Substring(0, value.Length - 1);
				}
				else
				{
					tooltip = value;
				}
				if (tooltip == "")
				{
					show = false;
					return;
				}
				base.ViewComponent.Position = xui.GetMouseXUiPosition() + PositionOffset;
				showDelay = Time.unscaledTime + ShowDelaySec;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		xui.ToolTipWindow = this;
		border = (XUiV_Sprite)GetChildById("sprBackgroundBorder").ViewComponent;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		xui.ToolTipWindow = null;
	}

	public override void Update(float _dt)
	{
		RefreshBindings();
		if (tooltip != "")
		{
			if (Time.unscaledTime > showDelay)
			{
				show = true;
			}
			Vector2i vector2i = xui.GetXUiScreenSize() / 2;
			if ((base.ViewComponent.Position + border.Size).x > vector2i.x)
			{
				base.ViewComponent.Position -= new Vector2i(border.Size.x, 0);
			}
			if ((base.ViewComponent.Position - border.Size).y < -vector2i.y)
			{
				base.ViewComponent.Position += new Vector2i(20, 20 + border.Size.y);
			}
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "screen_width":
			_value = xui.GetXUiScreenSize().x.ToString();
			return true;
		case "text":
			_value = ((tooltip.Length > 0) ? tooltip : tooltipOld);
			return true;
		case "show":
			_value = (show && GameManager.Instance.isAnyCursorWindowOpen()).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (!(_name == "show_delay"))
		{
			if (_name == "position_offset")
			{
				PositionOffset = StringParsers.ParseVector2i(_value);
				return true;
			}
			return base.ParseAttribute(_name, _value);
		}
		ShowDelaySec = StringParsers.ParseSInt32(_value);
		return true;
	}
}

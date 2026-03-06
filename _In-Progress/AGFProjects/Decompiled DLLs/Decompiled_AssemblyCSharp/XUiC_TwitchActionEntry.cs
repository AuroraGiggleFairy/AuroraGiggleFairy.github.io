using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchActionEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string rowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hoverColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string positiveColor = "0,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string negativeColor = "255,0,0";

	public new bool Selected;

	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchAction action;

	public XUiC_TwitchActionEntryList Owner;

	public bool isEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (action.IsInPreset(Owner.CurrentPreset))
			{
				return action.Enabled;
			}
			return false;
		}
	}

	public TwitchAction Action
	{
		get
		{
			return action;
		}
		set
		{
			base.ViewComponent.Enabled = value != null;
			action = value;
			IsDirty = true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchInfoWindowGroup TwitchInfoUIHandler { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Tracked { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = action != null;
		switch (bindingName)
		{
		case "actiontitle":
			value = (flag ? (action.Title + GetModifiedWithColor()) : "");
			return true;
		case "actiondescription":
			value = (flag ? action.Description : "");
			return true;
		case "actioncommand":
			value = (flag ? action.Command : "");
			return true;
		case "commandcolor":
			if (flag)
			{
				if (isEnabled)
				{
					if (action.IsPositive)
					{
						value = positiveColor;
					}
					else
					{
						value = negativeColor;
					}
				}
				else
				{
					value = disabledColor;
				}
			}
			return true;
		case "actionicon":
			value = "";
			if (flag && action.DisplayCategory != null)
			{
				value = action.DisplayCategory.Icon;
			}
			return true;
		case "iconcolor":
			value = "255,255,255,255";
			if (flag)
			{
				value = (isEnabled ? enabledColor : disabledColor);
			}
			return true;
		case "textstatecolor":
			value = "255,255,255,255";
			if (flag)
			{
				value = (isEnabled ? enabledColor : disabledColor);
			}
			return true;
		case "rowstatecolor":
			value = (Selected ? "255,255,255,255" : (IsHovered ? hoverColor : rowColor));
			return true;
		case "rowstatesprite":
			value = (Selected ? "ui_game_select_row" : "menu_empty");
			return true;
		case "showicon":
			value = ((Owner != null) ? (Owner.TwitchEntryListWindow.ActionCategory == "").ToString() : "true");
			return true;
		default:
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		if (Action == null)
		{
			IsHovered = false;
		}
		else if (IsHovered != _isOver)
		{
			IsHovered = _isOver;
			RefreshBindings();
		}
	}

	public override void Update(float _dt)
	{
		RefreshBindings(IsDirty);
		IsDirty = false;
		base.Update(_dt);
	}

	public void Refresh()
	{
		IsDirty = true;
	}

	public string GetModifiedWithColor()
	{
		if (Action != null)
		{
			int num = Action.ModifiedCost - Action.DefaultCost;
			if (num > 0)
			{
				return "[FF0000]*[-]";
			}
			if (num < 0)
			{
				return "[00FF00]*[-]";
			}
		}
		return "";
	}

	public override void OnCursorSelected()
	{
		base.OnCursorSelected();
		GetParentByType<XUiC_TwitchActionEntryList>().SelectedEntry = this;
		TwitchInfoUIHandler.SetEntry(this);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "enabled_color":
			enabledColor = value;
			return true;
		case "disabled_color":
			disabledColor = value;
			return true;
		case "positive_color":
			positiveColor = value;
			return true;
		case "negative_color":
			negativeColor = value;
			return true;
		case "row_color":
			rowColor = value;
			return true;
		case "hover_color":
			hoverColor = value;
			return true;
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}
}

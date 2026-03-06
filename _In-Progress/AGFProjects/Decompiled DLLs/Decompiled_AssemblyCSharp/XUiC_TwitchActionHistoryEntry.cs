using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchActionHistoryEntry : XUiController
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
	public TwitchActionHistoryEntry historyItem;

	public XUiC_TwitchActionHistoryEntryList Owner;

	public TwitchActionHistoryEntry HistoryItem
	{
		get
		{
			return historyItem;
		}
		set
		{
			base.ViewComponent.Enabled = value != null;
			historyItem = value;
			IsDirty = true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchInfoWindowGroup TwitchInfoUIHandler { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = historyItem != null;
		switch (bindingName)
		{
		case "username":
			if (flag)
			{
				if (historyItem.IsRefunded)
				{
					value = historyItem.UserName;
				}
				else
				{
					value = $"[{historyItem.UserColor}]{historyItem.UserName}[-]";
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "command":
			value = (flag ? historyItem.Command : "");
			return true;
		case "command_with_cost":
			value = (flag ? historyItem.Command : "");
			return true;
		case "commandcolor":
			if (flag)
			{
				if (historyItem.Action != null)
				{
					if (historyItem.IsRefunded)
					{
						value = disabledColor;
					}
					else if (historyItem.Action.IsPositive)
					{
						value = positiveColor;
					}
					else
					{
						value = negativeColor;
					}
				}
				else if (historyItem.Vote != null)
				{
					value = historyItem.Vote.TitleColor;
				}
				else if (historyItem.EventEntry != null)
				{
					value = "255,255,255,255";
				}
			}
			return true;
		case "cost":
			value = (flag ? historyItem.Action.CurrentCost.ToString() : "");
			return true;
		case "textstatecolor":
			value = "255,255,255,255";
			if (flag)
			{
				value = (historyItem.IsRefunded ? disabledColor : enabledColor);
			}
			return true;
		case "rowstatecolor":
			value = (Selected ? "255,255,255,255" : (IsHovered ? hoverColor : rowColor));
			return true;
		case "rowstatesprite":
			value = (Selected ? "ui_game_select_row" : "menu_empty");
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
		if (historyItem == null)
		{
			IsHovered = false;
		}
		else if (IsHovered != _isOver)
		{
			IsHovered = _isOver;
			RefreshBindings();
		}
	}

	public override void OnCursorSelected()
	{
		base.OnCursorSelected();
		GetParentByType<XUiC_TwitchActionHistoryEntryList>().SelectedEntry = this;
		TwitchInfoUIHandler.SetEntry(this);
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

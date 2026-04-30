using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchVoteInfoEntry : XUiController
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
	public TwitchVote vote;

	public XUiC_TwitchVoteInfoEntryList Owner;

	public TwitchVote Vote
	{
		get
		{
			return vote;
		}
		set
		{
			base.ViewComponent.Enabled = value != null;
			vote = value;
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
		bool flag = vote != null;
		switch (bindingName)
		{
		case "votetitle":
			value = (flag ? vote.VoteDescription : "");
			return true;
		case "votecolor":
			if (flag)
			{
				if (vote.Enabled)
				{
					value = ((vote.TitleColor == "") ? enabledColor : vote.TitleColor);
				}
				else
				{
					value = disabledColor;
				}
			}
			return true;
		case "voteicon":
			value = "";
			if (flag && vote.MainVoteType != null)
			{
				value = vote.MainVoteType.Icon;
			}
			return true;
		case "iconcolor":
			value = "255,255,255,255";
			if (flag)
			{
				value = (vote.Enabled ? enabledColor : disabledColor);
			}
			return true;
		case "rowstatecolor":
			value = (Selected ? "255,255,255,255" : (IsHovered ? hoverColor : rowColor));
			return true;
		case "rowstatesprite":
			value = (Selected ? "ui_game_select_row" : "menu_empty");
			return true;
		case "showicon":
			value = ((Owner != null) ? (Owner.TwitchEntryListWindow.VoteCategory == "").ToString() : "true");
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
		if (Vote == null)
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
		GetParentByType<XUiC_TwitchVoteInfoEntryList>().SelectedEntry = this;
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

using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchLeaderboardEntry : XUiController
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
	public TwitchLeaderboardEntry leaderboardEntry;

	public XUiC_TwitchLeaderboardEntryList Owner;

	public TwitchLeaderboardEntry LeaderboardEntry
	{
		get
		{
			return leaderboardEntry;
		}
		set
		{
			base.ViewComponent.Enabled = value != null;
			leaderboardEntry = value;
			IsDirty = true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchInfoWindowGroup TwitchInfoUIHandler { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = leaderboardEntry != null;
		if (!(bindingName == "username"))
		{
			if (bindingName == "kills")
			{
				value = (flag ? leaderboardEntry.Kills.ToString() : "");
				return true;
			}
			return false;
		}
		value = (flag ? $"[{leaderboardEntry.UserColor}]{leaderboardEntry.UserName}[-]" : "");
		return true;
	}

	public override void Init()
	{
		base.Init();
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		RefreshBindings(IsDirty);
		IsDirty = false;
		base.Update(_dt);
	}

	public override void OnCursorSelected()
	{
		base.OnCursorSelected();
		GetParentByType<XUiC_TwitchLeaderboardEntryList>().SelectedEntry = this;
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

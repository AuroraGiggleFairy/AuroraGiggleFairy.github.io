using System.Collections.Generic;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchLeaderboardEntryList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_TwitchLeaderboardEntry> entryList = new List<XUiC_TwitchLeaderboardEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchLeaderboardEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchLeaderboardEntry> leaderboardList = new List<TwitchLeaderboardEntry>();

	public XUiC_TwitchEntryListWindow TwitchEntryListWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	public int Page
	{
		get
		{
			return page;
		}
		set
		{
			if (page != value)
			{
				page = value;
				isDirty = true;
				pager?.SetPage(page);
			}
		}
	}

	public XUiC_TwitchLeaderboardEntry SelectedEntry
	{
		get
		{
			return selectedEntry;
		}
		set
		{
			if (selectedEntry != null)
			{
				selectedEntry.Selected = false;
			}
			selectedEntry = value;
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetFirstEntry()
	{
		if (entryList[0].LeaderboardEntry != null)
		{
			SelectedEntry = entryList[0];
			entryList[0].SelectCursorElement(_withDelay: true);
		}
		else
		{
			SelectedEntry = null;
			base.WindowGroup.Controller.GetChildById("searchControls").SelectCursorElement(_withDelay: true);
		}
		((XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller).ClearEntries();
	}

	public override void Init()
	{
		base.Init();
		XUiC_TwitchInfoWindowGroup xUiC_TwitchInfoWindowGroup = (XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller;
		XUiController childById = xUiC_TwitchInfoWindowGroup.GetChildByType<XUiC_TwitchHowToWindow>().GetChildById("leftButton");
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is XUiC_TwitchLeaderboardEntry)
			{
				XUiC_TwitchLeaderboardEntry xUiC_TwitchLeaderboardEntry = (XUiC_TwitchLeaderboardEntry)children[i];
				xUiC_TwitchLeaderboardEntry.Owner = this;
				xUiC_TwitchLeaderboardEntry.TwitchInfoUIHandler = xUiC_TwitchInfoWindowGroup;
				xUiC_TwitchLeaderboardEntry.OnScroll += OnScrollEntry;
				xUiC_TwitchLeaderboardEntry.ViewComponent.NavRightTarget = childById.ViewComponent;
				entryList.Add(xUiC_TwitchLeaderboardEntry);
			}
		}
		pager = base.Parent.GetChildByType<XUiC_Paging>();
		if (pager == null)
		{
			return;
		}
		pager.OnPageChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (viewComponent.IsVisible)
			{
				Page = pager.CurrentPageNumber;
			}
		};
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!isDirty)
		{
			return;
		}
		if (entryList != null)
		{
			for (int i = 0; i < entryList.Count; i++)
			{
				int num = i + entryList.Count * page;
				XUiC_TwitchLeaderboardEntry xUiC_TwitchLeaderboardEntry = entryList[i];
				if (xUiC_TwitchLeaderboardEntry != null)
				{
					xUiC_TwitchLeaderboardEntry.OnPress -= OnPressEntry;
					xUiC_TwitchLeaderboardEntry.Selected = false;
					xUiC_TwitchLeaderboardEntry.ViewComponent.SoundPlayOnClick = false;
					if (num < leaderboardList.Count)
					{
						xUiC_TwitchLeaderboardEntry.LeaderboardEntry = leaderboardList[num];
						xUiC_TwitchLeaderboardEntry.ViewComponent.IsNavigatable = true;
					}
					else
					{
						xUiC_TwitchLeaderboardEntry.LeaderboardEntry = null;
						xUiC_TwitchLeaderboardEntry.ViewComponent.IsNavigatable = false;
					}
				}
			}
			pager?.SetLastPageByElementsAndPageLength(leaderboardList.Count, entryList.Count);
		}
		isDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressEntry(XUiController _sender, int _mouseButton)
	{
		if (_sender is XUiC_TwitchLeaderboardEntry xUiC_TwitchLeaderboardEntry)
		{
			SelectedEntry = xUiC_TwitchLeaderboardEntry;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrollEntry(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			pager?.PageDown();
		}
		else
		{
			pager?.PageUp();
		}
	}

	public void SetTwitchLeaderboardList(List<TwitchLeaderboardEntry> newLeaderboardList)
	{
		Page = 0;
		leaderboardList = newLeaderboardList;
		isDirty = true;
		((XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller).ClearEntries();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		player = base.xui.playerUI.entityPlayer;
	}

	public override void OnClose()
	{
		base.OnClose();
		SelectedEntry = null;
	}
}

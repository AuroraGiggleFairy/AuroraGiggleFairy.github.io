using System.Collections.Generic;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchVoteInfoEntryList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_TwitchVoteInfoEntry> entryList = new List<XUiC_TwitchVoteInfoEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchVoteInfoEntry selectedEntry;

	public bool setFirstEntry = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchVote> voteList = new List<TwitchVote>();

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

	public XUiC_TwitchVoteInfoEntry SelectedEntry
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
		if (entryList[0].Vote != null)
		{
			SelectedEntry = entryList[0];
			entryList[0].SelectCursorElement(_withDelay: true);
		}
		else
		{
			SelectedEntry = null;
			base.WindowGroup.Controller.GetChildById("searchControls").SelectCursorElement(_withDelay: true);
		}
		((XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller).SetEntry(selectedEntry);
	}

	public override void Init()
	{
		base.Init();
		XUiC_TwitchInfoWindowGroup xUiC_TwitchInfoWindowGroup = (XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller;
		XUiController childById = xUiC_TwitchInfoWindowGroup.GetChildByType<XUiC_TwitchEntryDescriptionWindow>().GetChildById("btnEnable");
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is XUiC_TwitchVoteInfoEntry)
			{
				XUiC_TwitchVoteInfoEntry xUiC_TwitchVoteInfoEntry = (XUiC_TwitchVoteInfoEntry)children[i];
				xUiC_TwitchVoteInfoEntry.Owner = this;
				xUiC_TwitchVoteInfoEntry.TwitchInfoUIHandler = xUiC_TwitchInfoWindowGroup;
				xUiC_TwitchVoteInfoEntry.OnScroll += OnScrollEntry;
				xUiC_TwitchVoteInfoEntry.ViewComponent.NavRightTarget = childById.ViewComponent;
				entryList.Add(xUiC_TwitchVoteInfoEntry);
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
		Log.Out("Vote list update");
		if (entryList != null)
		{
			for (int i = 0; i < entryList.Count; i++)
			{
				int num = i + entryList.Count * page;
				XUiC_TwitchVoteInfoEntry xUiC_TwitchVoteInfoEntry = entryList[i];
				if (xUiC_TwitchVoteInfoEntry != null)
				{
					xUiC_TwitchVoteInfoEntry.OnPress -= OnPressEntry;
					xUiC_TwitchVoteInfoEntry.Selected = false;
					if (num < voteList.Count)
					{
						xUiC_TwitchVoteInfoEntry.Vote = voteList[num];
						xUiC_TwitchVoteInfoEntry.OnPress += OnPressEntry;
						xUiC_TwitchVoteInfoEntry.ViewComponent.SoundPlayOnClick = true;
						xUiC_TwitchVoteInfoEntry.ViewComponent.IsNavigatable = true;
					}
					else
					{
						xUiC_TwitchVoteInfoEntry.Vote = null;
						xUiC_TwitchVoteInfoEntry.ViewComponent.SoundPlayOnClick = false;
						xUiC_TwitchVoteInfoEntry.ViewComponent.IsNavigatable = false;
					}
				}
			}
			pager?.SetLastPageByElementsAndPageLength(voteList.Count, entryList.Count);
		}
		if (setFirstEntry)
		{
			SetFirstEntry();
			setFirstEntry = false;
		}
		isDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressEntry(XUiController _sender, int _mouseButton)
	{
		if (_sender is XUiC_TwitchVoteInfoEntry xUiC_TwitchVoteInfoEntry)
		{
			SelectedEntry = xUiC_TwitchVoteInfoEntry;
			SelectedEntry.TwitchInfoUIHandler.SetEntry(SelectedEntry);
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

	public void SetTwitchVoteList(List<TwitchVote> newVoteEntryList)
	{
		Page = 0;
		voteList = newVoteEntryList;
		setFirstEntry = true;
		isDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		setFirstEntry = true;
		player = base.xui.playerUI.entityPlayer;
	}

	public override void OnClose()
	{
		base.OnClose();
		SelectedEntry = null;
	}
}

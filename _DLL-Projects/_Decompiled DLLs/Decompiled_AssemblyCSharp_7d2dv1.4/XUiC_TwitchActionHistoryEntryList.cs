using System.Collections.Generic;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchActionHistoryEntryList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_TwitchActionHistoryEntry> entryList = new List<XUiC_TwitchActionHistoryEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionHistoryEntry selectedEntry;

	public bool setFirstEntry = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchActionHistoryEntry> redemptionList = new List<TwitchActionHistoryEntry>();

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

	public XUiC_TwitchActionHistoryEntry SelectedEntry
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
		if (entryList[0].HistoryItem != null)
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
		XUiController childById = xUiC_TwitchInfoWindowGroup.GetChildByType<XUiC_TwitchEntryDescriptionWindow>().GetChildById("btnRefund");
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is XUiC_TwitchActionHistoryEntry)
			{
				XUiC_TwitchActionHistoryEntry xUiC_TwitchActionHistoryEntry = (XUiC_TwitchActionHistoryEntry)children[i];
				xUiC_TwitchActionHistoryEntry.Owner = this;
				xUiC_TwitchActionHistoryEntry.TwitchInfoUIHandler = xUiC_TwitchInfoWindowGroup;
				xUiC_TwitchActionHistoryEntry.OnScroll += OnScrollEntry;
				xUiC_TwitchActionHistoryEntry.ViewComponent.NavRightTarget = childById.ViewComponent;
				entryList.Add(xUiC_TwitchActionHistoryEntry);
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
				SelectedEntry = null;
				setFirstEntry = true;
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
			TwitchActionHistoryEntry twitchActionHistoryEntry = ((selectedEntry != null) ? selectedEntry.HistoryItem : null);
			bool flag = false;
			int num = GetPage(twitchActionHistoryEntry);
			int num2 = page;
			if (num != -1 && num != page)
			{
				flag = true;
				num2 = num;
			}
			for (int i = 0; i < entryList.Count; i++)
			{
				int num3 = i + entryList.Count * num2;
				XUiC_TwitchActionHistoryEntry xUiC_TwitchActionHistoryEntry = entryList[i];
				if (xUiC_TwitchActionHistoryEntry == null)
				{
					continue;
				}
				xUiC_TwitchActionHistoryEntry.OnPress -= OnPressEntry;
				if (num3 < redemptionList.Count)
				{
					xUiC_TwitchActionHistoryEntry.HistoryItem = redemptionList[num3];
					xUiC_TwitchActionHistoryEntry.OnPress += OnPressEntry;
					xUiC_TwitchActionHistoryEntry.ViewComponent.SoundPlayOnClick = true;
					xUiC_TwitchActionHistoryEntry.Selected = xUiC_TwitchActionHistoryEntry.HistoryItem == twitchActionHistoryEntry;
					if (xUiC_TwitchActionHistoryEntry.Selected)
					{
						SelectedEntry = xUiC_TwitchActionHistoryEntry;
						((XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller).SetEntry(selectedEntry);
					}
					xUiC_TwitchActionHistoryEntry.ViewComponent.IsNavigatable = true;
				}
				else
				{
					xUiC_TwitchActionHistoryEntry.HistoryItem = null;
					xUiC_TwitchActionHistoryEntry.ViewComponent.SoundPlayOnClick = false;
					xUiC_TwitchActionHistoryEntry.Selected = false;
					xUiC_TwitchActionHistoryEntry.ViewComponent.IsNavigatable = false;
				}
			}
			pager?.SetLastPageByElementsAndPageLength(redemptionList.Count, entryList.Count);
			if (flag)
			{
				Page = num2;
				if (pager != null)
				{
					pager.RefreshBindings();
				}
				flag = false;
			}
		}
		if (setFirstEntry)
		{
			SetFirstEntry();
			setFirstEntry = false;
		}
		isDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetPage(TwitchActionHistoryEntry historyItem)
	{
		for (int i = 0; i < redemptionList.Count; i++)
		{
			if (redemptionList[i] == historyItem)
			{
				return i / entryList.Count;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressEntry(XUiController _sender, int _mouseButton)
	{
		if (_sender is XUiC_TwitchActionHistoryEntry xUiC_TwitchActionHistoryEntry)
		{
			SelectedEntry = xUiC_TwitchActionHistoryEntry;
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

	public void SetTwitchActionHistoryList(List<TwitchActionHistoryEntry> newRedemptionList)
	{
		redemptionList = newRedemptionList;
		if (SelectedEntry == null)
		{
			Page = 0;
			setFirstEntry = true;
		}
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

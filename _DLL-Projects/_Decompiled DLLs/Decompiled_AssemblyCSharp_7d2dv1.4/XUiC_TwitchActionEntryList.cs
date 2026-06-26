using System.Collections.Generic;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchActionEntryList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_TwitchActionEntry> entryList = new List<XUiC_TwitchActionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionEntry selectedEntry;

	public bool setFirstEntry = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchAction> actionList = new List<TwitchAction>();

	public XUiC_TwitchEntryListWindow TwitchEntryListWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	public TwitchActionPreset CurrentPreset;

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
				setFirstEntry = true;
				isDirty = true;
				pager?.SetPage(page);
			}
		}
	}

	public XUiC_TwitchActionEntry SelectedEntry
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
		if (entryList[0].Action != null)
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
		XUiController childById = xUiC_TwitchInfoWindowGroup.GetChildByType<XUiC_TwitchEntryDescriptionWindow>().GetChildById("btnDecrease");
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is XUiC_TwitchActionEntry)
			{
				XUiC_TwitchActionEntry xUiC_TwitchActionEntry = (XUiC_TwitchActionEntry)children[i];
				xUiC_TwitchActionEntry.Owner = this;
				xUiC_TwitchActionEntry.TwitchInfoUIHandler = xUiC_TwitchInfoWindowGroup;
				xUiC_TwitchActionEntry.OnScroll += OnScrollEntry;
				xUiC_TwitchActionEntry.ViewComponent.NavRightTarget = childById.ViewComponent;
				entryList.Add(xUiC_TwitchActionEntry);
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
				XUiC_TwitchActionEntry xUiC_TwitchActionEntry = entryList[i];
				if (xUiC_TwitchActionEntry != null)
				{
					xUiC_TwitchActionEntry.OnPress -= OnPressEntry;
					xUiC_TwitchActionEntry.Selected = false;
					if (num < actionList.Count)
					{
						xUiC_TwitchActionEntry.Action = actionList[num];
						xUiC_TwitchActionEntry.OnPress += OnPressEntry;
						xUiC_TwitchActionEntry.ViewComponent.SoundPlayOnClick = true;
						xUiC_TwitchActionEntry.ViewComponent.IsNavigatable = true;
					}
					else
					{
						xUiC_TwitchActionEntry.Action = null;
						xUiC_TwitchActionEntry.ViewComponent.SoundPlayOnClick = false;
						xUiC_TwitchActionEntry.ViewComponent.IsNavigatable = false;
					}
				}
			}
			pager?.SetLastPageByElementsAndPageLength(actionList.Count, entryList.Count);
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
		if (_sender is XUiC_TwitchActionEntry xUiC_TwitchActionEntry)
		{
			SelectedEntry = xUiC_TwitchActionEntry;
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

	public void SetTwitchActionList(List<TwitchAction> newActionEntryList, TwitchActionPreset currentPreset)
	{
		CurrentPreset = currentPreset;
		Page = 0;
		actionList = newActionEntryList;
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

using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_QuestEntry> entryList = new List<XUiC_QuestEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Quest> questList = new List<Quest>();

	public XUiC_QuestListWindow QuestListWindow;

	public XUiC_QuestSharedList SharedList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int visibleEntries;

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
				pager?.SetPage(page);
				isDirty = true;
			}
		}
	}

	public XUiC_QuestEntry SelectedEntry
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
			QuestListWindow.ShowShareQuest(base.xui.playerUI.entityPlayer.IsInParty() && (selectedEntry?.Quest.IsShareable ?? false));
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
				if (SharedList != null)
				{
					SharedList.SelectedEntry = null;
				}
				QuestListWindow.ShowRemoveQuest(selectedEntry.Quest.QuestClass.AllowRemove);
			}
			else
			{
				QuestListWindow.ShowRemoveQuest(_show: true);
			}
		}
	}

	public int VisibleEntries
	{
		get
		{
			return visibleEntries;
		}
		set
		{
			if (value != visibleEntries)
			{
				isDirty = true;
				visibleEntries = value;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_QuestWindowGroup questUIHandler = (XUiC_QuestWindowGroup)base.WindowGroup.Controller;
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is XUiC_QuestEntry)
			{
				XUiC_QuestEntry xUiC_QuestEntry = (XUiC_QuestEntry)children[i];
				xUiC_QuestEntry.QuestUIHandler = questUIHandler;
				xUiC_QuestEntry.OnScroll += OnScrollQuest;
				entryList.Add(xUiC_QuestEntry);
			}
		}
		pager = base.Parent.GetChildByType<XUiC_Paging>();
		if (pager != null)
		{
			pager.OnPageChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Page = pager.CurrentPageNumber;
			};
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!isDirty)
		{
			return;
		}
		Quest quest = selectedEntry?.Quest;
		for (int i = 0; i < entryList.Count; i++)
		{
			int num = i + visibleEntries * page;
			XUiC_QuestEntry xUiC_QuestEntry = entryList[i];
			xUiC_QuestEntry.OnPress -= OnPressQuest;
			xUiC_QuestEntry.ViewComponent.IsVisible = i < visibleEntries;
			if (num < questList.Count && i < visibleEntries)
			{
				xUiC_QuestEntry.Quest = questList[num];
				xUiC_QuestEntry.OnPress += OnPressQuest;
				xUiC_QuestEntry.ViewComponent.SoundPlayOnClick = true;
				xUiC_QuestEntry.Selected = questList[num] == quest;
				if (xUiC_QuestEntry.Selected)
				{
					SelectedEntry = xUiC_QuestEntry;
				}
			}
			else
			{
				xUiC_QuestEntry.Quest = null;
				xUiC_QuestEntry.ViewComponent.SoundPlayOnClick = false;
				xUiC_QuestEntry.Selected = false;
			}
		}
		pager?.SetLastPageByElementsAndPageLength(questList.Count, visibleEntries);
		if (selectedEntry != null && selectedEntry.Quest == null)
		{
			selectedEntry = null;
			quest = null;
		}
		if (selectedEntry == null && questList.Count > 0)
		{
			SelectedEntry = entryList[0];
			((XUiC_QuestWindowGroup)base.WindowGroup.Controller).SetQuest(selectedEntry);
		}
		else if (questList.Count == 0 && selectedEntry == null)
		{
			SelectedEntry = null;
			((XUiC_QuestWindowGroup)base.WindowGroup.Controller).SetQuest(selectedEntry);
		}
		isDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressQuest(XUiController _sender, int _mouseButton)
	{
		if (!(_sender is XUiC_QuestEntry xUiC_QuestEntry))
		{
			return;
		}
		SelectedEntry = xUiC_QuestEntry;
		SelectedEntry.QuestUIHandler.SetQuest(SelectedEntry);
		if (InputUtils.ShiftKeyPressed)
		{
			Quest quest = xUiC_QuestEntry.Quest;
			if (quest.Active && !quest.Tracked)
			{
				quest.Tracked = !quest.Tracked;
				base.xui.playerUI.entityPlayer.QuestJournal.TrackedQuest = (quest.Tracked ? quest : null);
				base.xui.playerUI.entityPlayer.QuestJournal.RefreshTracked();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrollQuest(XUiController _sender, float _delta)
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

	public void SetQuestList(List<Quest> newQuestList)
	{
		Page = 0;
		questList = newQuestList;
		isDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		player = base.xui.playerUI.entityPlayer;
		player.QuestChanged += QuestJournal_QuestChanged;
		base.xui.QuestTracker.OnTrackedQuestChanged += QuestTracker_OnTrackedQuestChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestTracker_OnTrackedQuestChanged()
	{
		for (int i = 0; i < entryList.Count; i++)
		{
			entryList[i].IsDirty = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		player.QuestChanged -= QuestJournal_QuestChanged;
		base.xui.QuestTracker.OnTrackedQuestChanged -= QuestTracker_OnTrackedQuestChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_QuestChanged(Quest q)
	{
		if (selectedEntry != null && selectedEntry.Quest == q)
		{
			selectedEntry.IsDirty = true;
		}
	}

	public bool HasQuests()
	{
		if (questList != null)
		{
			return questList.Count > 0;
		}
		return false;
	}
}

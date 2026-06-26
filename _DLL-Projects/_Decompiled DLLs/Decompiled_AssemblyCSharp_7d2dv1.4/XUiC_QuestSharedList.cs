using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestSharedList : XUiController
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
	public List<SharedQuestEntry> questList = new List<SharedQuestEntry>();

	public XUiC_QuestList QuestList;

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
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
				QuestList.SelectedEntry = null;
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
			int num = i + entryList.Count * page;
			XUiC_QuestEntry xUiC_QuestEntry = entryList[i];
			if (xUiC_QuestEntry == null)
			{
				continue;
			}
			xUiC_QuestEntry.OnPress -= OnPressQuest;
			if (num < questList.Count)
			{
				xUiC_QuestEntry.Quest = questList[num].Quest;
				xUiC_QuestEntry.SharedQuestEntry = questList[num];
				xUiC_QuestEntry.OnPress += OnPressQuest;
				xUiC_QuestEntry.ViewComponent.SoundPlayOnClick = true;
				xUiC_QuestEntry.Selected = questList[num].Quest == quest;
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
		pager?.SetLastPageByElementsAndPageLength(questList.Count, entryList.Count);
		if (selectedEntry != null && selectedEntry.Quest == null)
		{
			selectedEntry = null;
			quest = null;
			if (questList.Count == 0 && selectedEntry == null)
			{
				SelectedEntry = null;
				((XUiC_QuestWindowGroup)base.WindowGroup.Controller).SetQuest(selectedEntry);
			}
		}
		isDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressQuest(XUiController _sender, int _mouseButton)
	{
		if (_sender is XUiC_QuestEntry xUiC_QuestEntry)
		{
			SelectedEntry = xUiC_QuestEntry;
			SelectedEntry.QuestUIHandler.SetQuest(SelectedEntry);
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

	public void SetSharedQuestList(List<SharedQuestEntry> newQuestList)
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

using System.Collections.Generic;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestListWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum SearchTypes
	{
		All,
		Active,
		Completed
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public SearchTypes searchType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestList questList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button trackBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button questRemoveBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button showOnMapBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button questShareBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Quest> currentItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int buttonSpacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingTrackButton = true;

	public override void Init()
	{
		base.Init();
		questList = GetChildByType<XUiC_QuestList>();
		questList.QuestListWindow = this;
		trackBtn = (XUiV_Button)GetChildById("trackBtn").ViewComponent;
		trackBtn.Controller.OnPress += trackBtn_OnPress;
		showOnMapBtn = (XUiV_Button)GetChildById("showOnMapBtn").ViewComponent;
		showOnMapBtn.Controller.OnPress += showOnMapBtn_OnPress;
		questRemoveBtn = (XUiV_Button)GetChildById("questRemoveBtn").ViewComponent;
		questRemoveBtn.Controller.OnPress += questRemoveBtn_OnPress;
		questShareBtn = (XUiV_Button)GetChildById("questShareBtn").ViewComponent;
		questShareBtn.Controller.OnPress += questShareBtn_OnPress;
		buttonSpacing = showOnMapBtn.Position.x - trackBtn.Position.x;
		txtInput = (XUiC_TextInput)GetChildById("searchInput");
		if (txtInput != null)
		{
			txtInput.OnChangeHandler += HandleOnChangedHandler;
			txtInput.Text = "";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void questShareBtn_OnPress(XUiController _sender, int _mouseButton)
	{
		Quest selectedQuest = questList.SelectedEntry?.Quest;
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		PartyQuests.ShareQuestWithParty(selectedQuest, entityPlayer, _showTooltips: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showOnMapBtn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (questList.SelectedEntry != null)
		{
			Quest quest = questList.SelectedEntry.Quest;
			if (quest.HasPosition)
			{
				XUiC_WindowSelector.OpenSelectorAndWindow(base.xui.playerUI.entityPlayer, "map");
				((XUiC_MapArea)base.xui.GetWindow("mapArea").Controller).PositionMapAt(quest.Position);
			}
			else
			{
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, Localization.Get("ttQuestNoLocation"));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void trackBtn_OnPress(XUiController _sender, int _mouseButton)
	{
		_ = base.xui.QuestTracker;
		if (questList.SelectedEntry != null)
		{
			Quest quest = questList.SelectedEntry.Quest;
			if (quest.Active)
			{
				quest.Tracked = !quest.Tracked;
				base.xui.playerUI.entityPlayer.QuestJournal.TrackedQuest = (quest.Tracked ? quest : null);
				base.xui.playerUI.entityPlayer.QuestJournal.RefreshTracked();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		filterText = _text;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void questRemoveBtn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (questList.SelectedEntry != null)
		{
			base.xui.playerUI.entityPlayer.QuestJournal.RemoveQuest(questList.SelectedEntry.Quest);
			questList.SelectedEntry = null;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			filterText = filterText.ToLower();
			currentItems = (from quest in player.QuestJournal.quests
				where filterText == "" || QuestClass.GetQuest(quest.ID).Name.ToLower().Contains(filterText)
				orderby !quest.Active, quest.FinishTime descending, QuestClass.GetQuest(quest.ID).Name
				select quest).ToList();
			questList.SetQuestList(currentItems);
			IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		player = base.xui.playerUI.entityPlayer;
		player.QuestAccepted += QuestJournal_QuestAccepted;
		player.QuestRemoved += QuestJournal_QuestRemoved;
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		player.QuestAccepted -= QuestJournal_QuestAccepted;
		player.QuestRemoved -= QuestJournal_QuestRemoved;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "visible_quest_count")
		{
			questList.VisibleEntries = StringParsers.ParseSInt32(_value);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_QuestAccepted(Quest q)
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_QuestRemoved(Quest q)
	{
		IsDirty = true;
		if (q == base.xui.QuestTracker.TrackedQuest)
		{
			base.xui.QuestTracker.TrackedQuest = null;
		}
	}

	public void ShowTrackButton(bool _show)
	{
		trackBtn.IsVisible = _show;
		if (showingTrackButton != _show)
		{
			showingTrackButton = _show;
			trackBtn.Enabled = _show;
			Vector3 localPosition = showOnMapBtn.UiTransform.localPosition;
			Vector3 localPosition2 = questRemoveBtn.UiTransform.localPosition;
			if (_show)
			{
				showOnMapBtn.UiTransform.localPosition = new Vector3(localPosition.x + (float)buttonSpacing, localPosition.y, localPosition.z);
				questRemoveBtn.UiTransform.localPosition = new Vector3(localPosition2.x + (float)buttonSpacing, localPosition2.y, localPosition2.z);
			}
			else
			{
				showOnMapBtn.UiTransform.localPosition = new Vector3(localPosition.x - (float)buttonSpacing, localPosition.y, localPosition.z);
				questRemoveBtn.UiTransform.localPosition = new Vector3(localPosition2.x - (float)buttonSpacing, localPosition2.y, localPosition2.z);
			}
		}
	}

	public void ShowShareQuest(bool _show)
	{
		questShareBtn.IsVisible = _show && !PartyQuests.AutoShare;
	}

	public void ShowRemoveQuest(bool _show)
	{
		questRemoveBtn.IsVisible = _show;
	}
}

using System.Collections.Generic;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestSharedListWindow : XUiController
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
	public XUiC_QuestSharedList questList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button acceptBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button questRemoveBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button showOnMapBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SharedQuestEntry> currentItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int buttonSpacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingTrackButton = true;

	public override void Init()
	{
		base.Init();
		questList = GetChildByType<XUiC_QuestSharedList>();
		acceptBtn = (XUiV_Button)GetChildById("acceptBtn").ViewComponent;
		acceptBtn.Controller.OnPress += acceptBtn_OnPress;
		showOnMapBtn = (XUiV_Button)GetChildById("showOnMapBtn").ViewComponent;
		showOnMapBtn.Controller.OnPress += showOnMapBtn_OnPress;
		questRemoveBtn = (XUiV_Button)GetChildById("questRemoveBtn").ViewComponent;
		questRemoveBtn.Controller.OnPress += questRemoveBtn_OnPress;
		buttonSpacing = showOnMapBtn.Position.x - acceptBtn.Position.x;
		txtInput = (XUiC_TextInput)GetChildById("searchInput");
		if (txtInput != null)
		{
			txtInput.OnChangeHandler += HandleOnChangedHandler;
			txtInput.Text = "";
		}
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
	public void acceptBtn_OnPress(XUiController _sender, int _mouseButton)
	{
		SharedQuestEntry sharedQuest = questList.SelectedEntry?.SharedQuestEntry;
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		PartyQuests.AcceptSharedQuest(sharedQuest, entityPlayer);
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
			base.xui.playerUI.entityPlayer.QuestJournal.RemoveSharedQuestEntry(questList.SelectedEntry.SharedQuestEntry);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			filterText = filterText.ToLower();
			currentItems = player.QuestJournal.sharedQuestEntries.Where([PublicizedFrom(EAccessModifier.Private)] (SharedQuestEntry quest) => filterText == "" || quest.QuestClass.Name.ToLower().Contains(filterText)).ToList();
			questList.SetSharedQuestList(currentItems);
			IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		player = base.xui.playerUI.entityPlayer;
		player.SharedQuestAdded += QuestJournal_SharedQuestAdded;
		player.SharedQuestRemoved += QuestJournal_SharedQuestRemoved;
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		player.SharedQuestAdded -= QuestJournal_SharedQuestAdded;
		player.SharedQuestRemoved -= QuestJournal_SharedQuestRemoved;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_SharedQuestAdded(SharedQuestEntry entry)
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_SharedQuestRemoved(SharedQuestEntry entry)
	{
		IsDirty = true;
	}

	public void ShowTrackButton(bool _show)
	{
		acceptBtn.IsVisible = _show;
		if (showingTrackButton != _show)
		{
			showingTrackButton = _show;
			acceptBtn.Enabled = _show;
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
}

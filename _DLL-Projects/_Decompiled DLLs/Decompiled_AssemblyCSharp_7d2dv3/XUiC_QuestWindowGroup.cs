using System.Threading.Tasks;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestList questList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestSharedList sharedList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestObjectivesWindow objectivesWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestRewardsWindow rewardsWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestDescriptionWindow descriptionWindow;

	public override void Init()
	{
		base.Init();
		objectivesWindow = GetChildByType<XUiC_QuestObjectivesWindow>();
		rewardsWindow = GetChildByType<XUiC_QuestRewardsWindow>();
		descriptionWindow = GetChildByType<XUiC_QuestDescriptionWindow>();
		questList = GetChildByType<XUiC_QuestListWindow>()?.GetChildByType<XUiC_QuestList>();
		sharedList = GetChildByType<XUiC_QuestSharedListWindow>()?.GetChildByType<XUiC_QuestSharedList>();
		if (questList != null)
		{
			questList.SharedList = sharedList;
		}
		if (sharedList != null)
		{
			sharedList.QuestList = questList;
		}
	}

	public void SetQuest(XUiC_QuestEntry q)
	{
		objectivesWindow.SetQuest(q);
		rewardsWindow.SetQuest(q);
		descriptionWindow.SetQuest(q);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.playerUI.windowManager.Open("windowpaging", _bModal: false);
		xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.FindWindowGroupByName("windowpaging")?.GetChildByType<XUiC_WindowSelector>()?.SetSelected("quests");
		RefreshBindings();
		AsyncUISelectionOnOpen();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public async void AsyncUISelectionOnOpen()
	{
		xui.playerUI.CursorController.Locked = true;
		for (int i = 0; i < 3; i++)
		{
			await Task.Yield();
		}
		xui.playerUI.CursorController.Locked = false;
		if (questList.HasQuests())
		{
			questList.SelectCursorElement(_withDelay: true);
		}
		else if (sharedList.HasQuests())
		{
			sharedList.SelectCursorElement(_withDelay: true);
		}
		else
		{
			GetChildById("content").SelectCursorElement(_withDelay: true);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.playerUI.windowManager.Close("windowpaging");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "questsautoshare":
			_value = PartyQuests.AutoShare.ToString();
			return true;
		case "questsautoaccept":
			_value = PartyQuests.AutoAccept.ToString();
			return true;
		case "queststier":
		{
			EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
			if (entityPlayer != null)
			{
				int currentFactionTier = entityPlayer.QuestJournal.GetCurrentFactionTier(1);
				_value = string.Format(Localization.Get("xuiQuestTierDescription"), ValueDisplayFormatters.RomanNumber(entityPlayer.QuestJournal.GetCurrentFactionTier(1)), entityPlayer.QuestJournal.GetQuestFactionPoints(1), entityPlayer.QuestJournal.GetQuestFactionMax(1, currentFactionTier));
			}
			return true;
		}
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}

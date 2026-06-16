using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTurnInWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTurnInDetailsWindow detailsWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTurnInRewardsWindow rewardsWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	public static string ID = "questTurnIn";

	public EntityNPC NPC;

	public static void Open(XUi xui, Action _OnCloseAction = null)
	{
		xui.playerUI.windowManager.Open("questTurnIn", _bModal: true);
		if (_OnCloseAction != null)
		{
			GUIWindow window = xui.playerUI.windowManager.GetWindow("questTurnIn");
			window.OnWindowClose = (Action)Delegate.Combine(window.OnWindowClose, _OnCloseAction);
		}
	}

	public override void Init()
	{
		base.Init();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
		detailsWindow = GetChildByType<XUiC_QuestTurnInDetailsWindow>();
		rewardsWindow = GetChildByType<XUiC_QuestTurnInRewardsWindow>();
	}

	public override void OnOpen()
	{
		if (xui.Dialog.Respondent != null)
		{
			NPC = xui.Dialog.Respondent;
		}
		else
		{
			NPC = xui.Trader.Trader as EntityTrader;
		}
		detailsWindow.NPC = (rewardsWindow.NPC = NPC);
		XUiC_ItemInfoWindow childByType = xui.GetChildByType<XUiC_ItemInfoWindow>();
		rewardsWindow.InfoWindow = childByType;
		base.OnOpen();
		xui.playerUI.entityPlayer.OverrideFOV = 30f;
		xui.playerUI.entityPlayer.OverrideLookAt = NPC.getHeadPosition();
		GUIWindowManager windowManager = xui.playerUI.windowManager;
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader("QUEST COMPLETE");
		}
		windowManager.Close("windowpaging");
		xui.DragAndDropWindow.InMenu = true;
		EntityTrader entityTrader = xui.Dialog.Respondent as EntityTrader;
		if (entityTrader != null)
		{
			entityTrader.SetNextTraderWindow(EntityTrader.TraderWindowState.Dialog);
		}
	}

	public override void OnClose()
	{
		EntityTrader entityTrader = xui.Dialog.Respondent as EntityTrader;
		base.OnClose();
		EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
		if (GameManager.Instance.World != null && entityTrader == null)
		{
			if (Vector3.Distance(xui.Dialog.Respondent.position, entityPlayer.position) > 5f)
			{
				xui.Dialog.Respondent = null;
				xui.playerUI.entityPlayer.OverrideFOV = -1f;
			}
			else
			{
				xui.playerUI.windowManager.Open("dialog", _bModal: true);
			}
		}
	}

	public void TryNextComplete()
	{
		Quest nextCompletedQuest = xui.playerUI.entityPlayer.QuestJournal.GetNextCompletedQuest(xui.Dialog.QuestTurnIn, NPC.entityId);
		if (nextCompletedQuest == null)
		{
			xui.playerUI.windowManager.CloseAllOpenModalWindows();
			return;
		}
		xui.Dialog.QuestTurnIn = nextCompletedQuest;
		XUiC_QuestTurnInDetailsWindow xUiC_QuestTurnInDetailsWindow = detailsWindow;
		Quest currentQuest = (rewardsWindow.CurrentQuest = nextCompletedQuest);
		xUiC_QuestTurnInDetailsWindow.CurrentQuest = currentQuest;
		RefreshBindings();
	}
}

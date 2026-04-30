using System.Collections;
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

	public override void Init()
	{
		base.Init();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
		detailsWindow = GetChildByType<XUiC_QuestTurnInDetailsWindow>();
		rewardsWindow = GetChildByType<XUiC_QuestTurnInRewardsWindow>();
	}

	public override void OnOpen()
	{
		if (base.xui.Dialog.Respondent != null)
		{
			NPC = base.xui.Dialog.Respondent;
		}
		else
		{
			NPC = base.xui.Trader.TraderEntity;
		}
		detailsWindow.NPC = (rewardsWindow.NPC = NPC);
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		rewardsWindow.InfoWindow = childByType;
		base.OnOpen();
		base.xui.playerUI.entityPlayer.OverrideFOV = 30f;
		base.xui.playerUI.entityPlayer.OverrideLookAt = NPC.getHeadPosition();
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader("QUEST COMPLETE");
		}
		windowManager.CloseIfOpen("windowpaging");
		base.xui.dragAndDrop.InMenu = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (GameManager.Instance.World != null)
		{
			EntityTrader entityTrader = base.xui.Trader.TraderEntity as EntityTrader;
			if (entityTrader != null)
			{
				GameManager.Instance.StartCoroutine(startTrading(entityTrader, entityPlayer));
			}
			else if (Vector3.Distance(base.xui.Dialog.Respondent.position, entityPlayer.position) > 5f)
			{
				base.xui.Dialog.Respondent = null;
				base.xui.playerUI.entityPlayer.OverrideFOV = -1f;
			}
			else
			{
				base.xui.playerUI.windowManager.Open("dialog", _bModal: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startTrading(EntityTrader trader, EntityPlayer player)
	{
		yield return null;
		trader.StartTrading(player);
	}

	public void TryNextComplete()
	{
		Quest nextCompletedQuest = base.xui.playerUI.entityPlayer.QuestJournal.GetNextCompletedQuest(base.xui.Dialog.QuestTurnIn, NPC.entityId);
		if (nextCompletedQuest == null)
		{
			base.xui.playerUI.windowManager.CloseAllOpenWindows();
			return;
		}
		base.xui.Dialog.QuestTurnIn = nextCompletedQuest;
		XUiC_QuestTurnInDetailsWindow xUiC_QuestTurnInDetailsWindow = detailsWindow;
		Quest currentQuest = (rewardsWindow.CurrentQuest = nextCompletedQuest);
		xUiC_QuestTurnInDetailsWindow.CurrentQuest = currentQuest;
		RefreshBindings();
	}
}

using UnityEngine.Scripting;

[Preserve]
public class DialogActionAddQuest : BaseDialogAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public Quest Quest;

	public int ListIndex;

	public override ActionTypes ActionType => ActionTypes.AddQuest;

	public override void PerformAction(EntityPlayer player)
	{
		if (Quest == null)
		{
			return;
		}
		QuestClass questClass = Quest.QuestClass;
		if (questClass == null)
		{
			return;
		}
		Quest quest = player.QuestJournal.FindNonSharedQuest(Quest.QuestCode);
		if (quest == null || (questClass.Repeatable && !quest.Active))
		{
			LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
			XUiC_QuestOfferWindow.OpenQuestOfferWindow(playerUI.xui, Quest, ListIndex, XUiC_QuestOfferWindow.OfferTypes.Dialog, playerUI.xui.Dialog.Respondent.entityId, [PublicizedFrom(EAccessModifier.Internal)] (EntityNPC npc) =>
			{
				playerUI.xui.Dialog.Respondent = npc;
				playerUI.xui.Dialog.ReturnStatement = ((DialogResponseQuest)base.Owner).LastStatementID;
				playerUI.windowManager.Open("dialog", _bModal: true);
			});
		}
		else
		{
			GameManager.ShowTooltip((EntityPlayerLocal)player, Localization.Get("questunavailable"));
		}
	}
}

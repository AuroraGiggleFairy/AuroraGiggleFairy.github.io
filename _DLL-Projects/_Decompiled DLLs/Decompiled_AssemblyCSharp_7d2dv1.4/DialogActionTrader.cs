using UnityEngine.Scripting;

[Preserve]
public class DialogActionTrader : BaseDialogAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public override ActionTypes ActionType => ActionTypes.Trader;

	public override void PerformAction(EntityPlayer player)
	{
		EntityNPC respondent = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal).xui.Dialog.Respondent;
		if (!(respondent != null))
		{
			return;
		}
		if (base.ID.EqualsCaseInsensitive("restock"))
		{
			(respondent as EntityTrader).TileEntityTrader.TraderData.lastInventoryUpdate = 0uL;
		}
		else if (base.ID.EqualsCaseInsensitive("trade"))
		{
			(respondent as EntityTrader).StartTrading(player);
		}
		else if (base.ID.EqualsCaseInsensitive("reset_quests"))
		{
			if (respondent is EntityTrader)
			{
				(respondent as EntityTrader).ClearActiveQuests(player.entityId);
			}
		}
		else if (base.ID.EqualsCaseInsensitive("drone_storage"))
		{
			(respondent as EntityDrone).OpenStorage(player);
		}
		else if (base.ID.EqualsCaseInsensitive("drone_follow"))
		{
			(respondent as EntityDrone).FollowMode();
		}
		else if (base.ID.EqualsCaseInsensitive("drone_sentry"))
		{
			(respondent as EntityDrone).SentryMode();
		}
		else if (base.ID.EqualsCaseInsensitive("drone_heal"))
		{
			(respondent as EntityDrone).HealOwner();
		}
		else if (base.ID.EqualsCaseInsensitive("drone_dont_heal_allies") || base.ID.EqualsCaseInsensitive("drone_heal_allies"))
		{
			(respondent as EntityDrone).ToggleHealAllies();
		}
	}
}

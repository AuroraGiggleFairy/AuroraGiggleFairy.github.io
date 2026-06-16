using UnityEngine.Scripting;

[Preserve]
public class DialogActionTrader : BaseDialogAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public override ActionTypes ActionType => ActionTypes.Trader;

	public override void PerformAction(EntityPlayer player)
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
		EntityNPC respondent = uIForPlayer.xui.Dialog.Respondent;
		if (!(respondent != null))
		{
			return;
		}
		if (respondent is EntityTrader entityTrader)
		{
			if (base.ID.EqualsCaseInsensitive("restock"))
			{
				entityTrader.TraderData.lastInventoryUpdate = 0uL;
			}
			else if (base.ID.EqualsCaseInsensitive("trade"))
			{
				entityTrader.SetNextTraderWindow(EntityTrader.TraderWindowState.Trade);
				uIForPlayer.windowManager.CloseAllOpenModalWindows();
			}
			else if (base.ID.EqualsCaseInsensitive("reset_quests"))
			{
				entityTrader.ClearActiveQuests(player.entityId);
			}
		}
		if (respondent is EntityDrone entityDrone)
		{
			if (base.ID.EqualsCaseInsensitive("drone_storage"))
			{
				entityDrone.OpenStorageFromDialog(player);
			}
			else if (base.ID.EqualsCaseInsensitive("drone_command_follow") || base.ID.EqualsCaseInsensitive("drone_command_stay"))
			{
				entityDrone.ToggleOrderState();
			}
			else if (base.ID.EqualsCaseInsensitive("drone_dont_heal_allies") || base.ID.EqualsCaseInsensitive("drone_heal_allies"))
			{
				entityDrone.ToggleHealAllies();
			}
			else if (base.ID.EqualsCaseInsensitive("drone_attack_mode_passive") || base.ID.EqualsCaseInsensitive("drone_attack_mode_aggressive"))
			{
				entityDrone.ToggleAttackMode();
			}
			else if (base.ID.EqualsCaseInsensitive("drone_light_on") || base.ID.EqualsCaseInsensitive("drone_light_off"))
			{
				entityDrone.ToggleLightAction();
			}
			else if (base.ID.EqualsCaseInsensitive("drone_command_heal"))
			{
				entityDrone.HealRequest();
			}
		}
	}
}

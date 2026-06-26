using UnityEngine.Scripting;

[Preserve]
public class ServiceActionEntryRent : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityVendingMachine vending;

	public ServiceActionEntryRent(XUiController controller, TileEntityVendingMachine _vending)
		: base(controller, "lblContextActionRent", "ui_game_symbol_coin")
	{
		vending = _vending;
	}

	public override void RefreshEnabled()
	{
		base.Enabled = vending.CanRent() == TileEntityVendingMachine.RentResult.Allowed;
	}

	public override void OnDisabledActivate()
	{
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		switch (vending.CanRent())
		{
		case TileEntityVendingMachine.RentResult.AlreadyRented:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttVMAlreadyRented"));
			break;
		case TileEntityVendingMachine.RentResult.NotEnoughMoney:
			if (vending.LocalPlayerIsOwner())
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttVMNotEnoughMoneyAddTime"));
			}
			else
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttVMNotEnoughMoneyRent"));
			}
			break;
		case TileEntityVendingMachine.RentResult.AlreadyRentingVM:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttAlreadyRentingVM"));
			break;
		}
	}

	public override void OnActivated()
	{
		if (vending.Rent())
		{
			((XUiC_TraderWindow)base.ItemController).Refresh();
		}
		RefreshEnabled();
	}
}

using UnityEngine.Scripting;

[Preserve]
public class ServiceActionEntryRent : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly TileEntityVendingMachine vending;

	public ServiceActionEntryRent(XUiController _controller, TileEntityVendingMachine _vending)
		: base(_controller, "lblContextActionRent", "ui_game_symbol_coin")
	{
		vending = _vending;
	}

	public override void RefreshEnabled()
	{
		base.Enabled = vending.CanRent() == TileEntityVendingMachine.RentResult.Allowed;
	}

	public override void OnDisabledActivate()
	{
		string text = vending.CanRent() switch
		{
			TileEntityVendingMachine.RentResult.AlreadyRented => Localization.Get("ttVMAlreadyRented"), 
			TileEntityVendingMachine.RentResult.NotEnoughMoney => Localization.Get(vending.LocalPlayerIsOwner() ? "ttVMNotEnoughMoneyAddTime" : "ttVMNotEnoughMoneyRent"), 
			TileEntityVendingMachine.RentResult.AlreadyRentingVM => Localization.Get("ttAlreadyRentingVM"), 
			_ => null, 
		};
		if (text != null)
		{
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, text);
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

using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryResetPrice : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOwner;

	public ItemActionEntryResetPrice(XUiController controller)
		: base(controller, "lblContextActionReset", "ui_game_symbol_coin", GamepadShortCut.None, "")
	{
		if (controller.xui.Trader.TraderTileEntity is TileEntityVendingMachine tileEntityVendingMachine)
		{
			bool playerOwned = controller.xui.Trader.Trader.TraderInfo.PlayerOwned;
			bool rentable = controller.xui.Trader.Trader.TraderInfo.Rentable;
			isOwner = (playerOwned || rentable) && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		}
		else
		{
			isOwner = false;
		}
	}

	public override void OnActivated()
	{
		XUiC_TraderItemEntry xUiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
		base.ItemController.xui.Trader.Trader.ResetMarkup(xUiC_TraderItemEntry.SlotIndex);
		xUiC_TraderItemEntry.InfoWindow.RefreshBindings();
		base.ItemController.xui.Trader.TraderWindowGroup.RefreshTraderItems();
		Manager.PlayInsidePlayerHead("ui_tab");
	}
}

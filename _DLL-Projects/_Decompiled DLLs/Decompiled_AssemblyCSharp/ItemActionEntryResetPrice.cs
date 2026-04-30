using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryResetPrice : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isOwner;

	public ItemActionEntryResetPrice(XUiController _controller)
		: base(_controller, "lblContextActionReset", "ui_game_symbol_coin", GamepadShortCut.None, "")
	{
		if (_controller.xui.Trader.TraderTileEntity is TileEntityVendingMachine tileEntityVendingMachine)
		{
			bool playerOwned = _controller.xui.Trader.Trader.TraderInfo.PlayerOwned;
			bool rentable = _controller.xui.Trader.Trader.TraderInfo.Rentable;
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

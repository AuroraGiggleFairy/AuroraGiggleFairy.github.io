using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryMarkup : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOwner;

	public ItemActionEntryMarkup(XUiController controller)
		: base(controller, "", "ui_game_symbol_add", GamepadShortCut.DPadRight, "")
	{
		base.ActionName = string.Format(Localization.Get("lblContextActionPrice"), 20);
		if (controller.xui.Trader.TraderTileEntity is TileEntityVendingMachine)
		{
			bool playerOwned = controller.xui.Trader.Trader.TraderInfo.PlayerOwned;
			bool rentable = controller.xui.Trader.Trader.TraderInfo.Rentable;
			TileEntityVendingMachine tileEntityVendingMachine = controller.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
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
		base.ItemController.xui.Trader.Trader.IncreaseMarkup(xUiC_TraderItemEntry.SlotIndex);
		xUiC_TraderItemEntry.InfoWindow.RefreshBindings();
		base.ItemController.xui.Trader.TraderWindowGroup.RefreshTraderItems();
		Manager.PlayInsidePlayerHead("ui_tab");
	}
}

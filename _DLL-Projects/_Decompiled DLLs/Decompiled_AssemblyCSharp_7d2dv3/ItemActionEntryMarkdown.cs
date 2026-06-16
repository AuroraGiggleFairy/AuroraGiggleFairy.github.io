using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryMarkdown : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isOwner;

	public ItemActionEntryMarkdown(XUiController _controller)
		: base(_controller, "OVERRIDDEN BELOW", "ui_game_symbol_subtract", GamepadShortCut.DPadLeft, "")
	{
		base.ActionName = string.Format(Localization.Get("lblContextActionPrice"), 20);
		if (_controller.xui.Trader.Trader is TileEntityVendingMachine tileEntityVendingMachine)
		{
			TraderInfo traderInfo = _controller.xui.Trader.TraderData.TraderInfo;
			bool playerOwned = traderInfo.PlayerOwned;
			bool rentable = traderInfo.Rentable;
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
		TraderData traderData = base.ItemController.xui.Trader.TraderData;
		traderData.PrimaryInventory[xUiC_TraderItemEntry.SlotIndex].DecreaseMarkup();
		traderData.SetModified(base.ItemController.xui.Trader.Trader);
		xUiC_TraderItemEntry.InfoWindow.RefreshBindings();
		base.ItemController.xui.Trader.TraderWindowGroup.RefreshTraderItems();
		Manager.PlayInsidePlayerHead("ui_tab");
	}
}

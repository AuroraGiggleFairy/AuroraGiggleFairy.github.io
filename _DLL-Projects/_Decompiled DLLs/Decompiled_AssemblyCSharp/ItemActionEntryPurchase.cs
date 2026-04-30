using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryPurchase : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isOwner;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isVending;

	public ItemActionEntryPurchase(XUiController _controller)
		: base(_controller, "OVERRIDDEN BELOW", "OVERRIDDEN BELOW", GamepadShortCut.None, "")
	{
		if (_controller.xui.Trader.TraderTileEntity is TileEntityVendingMachine tileEntityVendingMachine)
		{
			bool playerOwned = _controller.xui.Trader.Trader.TraderInfo.PlayerOwned;
			bool rentable = _controller.xui.Trader.Trader.TraderInfo.Rentable;
			isOwner = (playerOwned || rentable) && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
			isVending = true;
		}
		else
		{
			isOwner = false;
			isVending = false;
		}
		if (isOwner)
		{
			base.ActionName = Localization.Get("lblContextActionTake");
			base.IconName = "ui_game_symbol_hand";
		}
		else
		{
			base.ActionName = Localization.Get("lblContextActionBuy");
			base.IconName = "ui_game_symbol_coin";
		}
	}

	public override void RefreshEnabled()
	{
		refreshBinding();
		XUiC_TraderItemEntry xUiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
		if (xUiC_TraderItemEntry?.Item != null)
		{
			int count = xUiC_TraderItemEntry.InfoWindow.BuySellCounter.Count;
			if (isOwner)
			{
				base.Enabled = count > 0;
				return;
			}
			XUi xui = xUiC_TraderItemEntry.xui;
			XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
			ItemStack itemStack = xUiC_TraderItemEntry.Item.Clone();
			int num = XUiM_Trader.GetBuyPrice(itemClass: ItemClass.GetForId(itemStack.itemValue.type), _xui: xui, itemValue: itemStack.itemValue, count: count, index: xUiC_TraderItemEntry.SlotIndex);
			ItemValue item = ItemClass.GetItem(TraderInfo.CurrencyItem);
			base.Enabled = count > 0 && playerInventory.GetItemCount(item) >= num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshBinding()
	{
		XUiC_TraderItemEntry xUiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
		if (xUiC_TraderItemEntry == null)
		{
			return;
		}
		int slotIndex = xUiC_TraderItemEntry.SlotIndex;
		int num = 0;
		XUi xui = xUiC_TraderItemEntry.xui;
		if (xui != null)
		{
			List<ItemStack> list = xui.Trader?.Trader?.PrimaryInventory;
			if (list != null)
			{
				num = list.Count;
			}
		}
		if (num > slotIndex)
		{
			xUiC_TraderItemEntry.Item = xUiC_TraderItemEntry.xui.Trader.Trader.PrimaryInventory[slotIndex];
		}
		else
		{
			xUiC_TraderItemEntry.Item = null;
		}
	}

	public override void OnActivated()
	{
		if (base.ItemController.xui.Trader.TraderTileEntity.bWaitingForServerResponse)
		{
			return;
		}
		try
		{
			refreshBinding();
			ItemStack itemStack = ((XUiC_TraderItemEntry)base.ItemController).Item.Clone();
			ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
			ItemValue itemValue = itemStack.itemValue;
			int count = ((XUiC_TraderItemEntry)base.ItemController).InfoWindow.BuySellCounter.Count;
			int buyPrice = XUiM_Trader.GetBuyPrice(base.ItemController.xui, itemStack.itemValue, count, forId, ((XUiC_TraderItemEntry)base.ItemController).SlotIndex);
			XUi xui = base.ItemController.xui;
			XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
			int num = (forId.IsBlock() ? Block.list[itemValue.type].EconomicBundleSize : forId.EconomicBundleSize);
			int num2 = count % num;
			if (num2 != 0)
			{
				GameManager.ShowTooltip(xui.playerUI.entityPlayer, string.Format(Localization.Get("ttItemCountNotBundleSize"), num));
				count -= num2;
				((XUiC_TraderItemEntry)base.ItemController).InfoWindow.BuySellCounter.Count = count;
				Manager.PlayInsidePlayerHead("ui_denied");
				return;
			}
			int num3 = playerInventory.CountAvailableSpaceForItem(itemValue);
			if (count > num3)
			{
				GameManager.ShowTooltip(xui.playerUI.entityPlayer, string.Format(Localization.Get("ttItemCountMoreThanAvailable"), num));
				Manager.PlayInsidePlayerHead("ui_denied");
				return;
			}
			if (count > itemStack.count)
			{
				count = itemStack.count;
			}
			if (isOwner)
			{
				int num4 = count;
				ItemStack itemStack2 = new ItemStack(itemStack.itemValue, count);
				if (playerInventory.AddItem(itemStack2))
				{
					ItemStack item = ((XUiC_TraderItemEntry)base.ItemController).Item;
					item.count -= count;
					((XUiC_TraderItemEntry)base.ItemController).InfoWindow.BuySellCounter.MaxCount -= count;
					((XUiC_TraderItemEntry)base.ItemController).Refresh();
					if (item.count == 0)
					{
						base.ItemController.xui.Trader.Trader.PrimaryInventory.Remove(item);
						base.ItemController.xui.Trader.Trader.RemoveMarkup(((XUiC_TraderItemEntry)base.ItemController).SlotIndex);
					}
					Manager.PlayInsidePlayerHead("craft_take_item");
				}
				else if (num4 != itemStack2.count)
				{
					ItemStack item2 = ((XUiC_TraderItemEntry)base.ItemController).Item;
					if (itemStack2.count == 0)
					{
						itemStack2 = ItemStack.Empty.Clone();
						base.ItemController.xui.Trader.Trader.PrimaryInventory.Remove(item2);
						base.ItemController.xui.Trader.Trader.RemoveMarkup(((XUiC_TraderItemEntry)base.ItemController).SlotIndex);
					}
					else
					{
						item2.count -= num4 - itemStack2.count;
						((XUiC_TraderItemEntry)base.ItemController).InfoWindow.BuySellCounter.MaxCount -= count;
						((XUiC_TraderItemEntry)base.ItemController).Refresh();
					}
					Manager.PlayInsidePlayerHead("craft_take_item");
				}
			}
			else
			{
				ItemStack itemStack3 = new ItemStack(ItemClass.GetItem(TraderInfo.CurrencyItem), buyPrice);
				itemStack.count = count;
				if (playerInventory.CanSwapItems(itemStack3, itemStack))
				{
					if (playerInventory.AddItem(itemStack, playCollectSound: false))
					{
						playerInventory.RemoveItem(itemStack3);
						base.ItemController.xui.Trader.Trader.AvailableMoney += buyPrice;
						XUiC_TraderItemEntry xUiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
						ItemStack item3 = xUiC_TraderItemEntry.Item;
						item3.count -= count;
						xUiC_TraderItemEntry.InfoWindow.BuySellCounter.MaxCount -= count;
						xUiC_TraderItemEntry.Refresh();
						if (item3.count == 0)
						{
							base.ItemController.xui.Trader.Trader.PrimaryInventory.Remove(item3);
							base.ItemController.xui.Trader.Trader.RemoveMarkup(xUiC_TraderItemEntry.SlotIndex);
						}
						if (base.ItemController.xui.Trader.TraderEntity != null)
						{
							Manager.PlayInsidePlayerHead("ui_trader_purchase");
						}
						else
						{
							Manager.PlayInsidePlayerHead("ui_vending_purchase");
						}
						QuestEventManager.Current.BoughtItems(isVending ? "" : base.ItemController.xui.Trader.TraderEntity.EntityName, count);
						GameSparksCollector.IncrementCounter(isVending ? GameSparksCollector.GSDataKey.VendingItemsBought : GameSparksCollector.GSDataKey.TraderItemsBought, forId.Name, count);
						GameSparksCollector.IncrementCounter(isVending ? GameSparksCollector.GSDataKey.VendingMoneySpentOn : GameSparksCollector.GSDataKey.TraderMoneySpentOn, forId.Name, buyPrice);
						GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.TotalMoneySpentOn, forId.Name, buyPrice);
					}
				}
				else
				{
					GameManager.ShowTooltip(xui.playerUI.entityPlayer, Localization.Get("ttNoSpaceForBuying"));
					Manager.PlayInsidePlayerHead("ui_denied");
				}
			}
			base.ItemController.xui.Trader.TraderWindowGroup.RefreshTraderItems();
		}
		finally
		{
			base.ItemController.xui.Trader?.TraderTileEntity?.SetModified();
		}
	}
}

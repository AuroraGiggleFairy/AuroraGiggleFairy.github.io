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
		if (_controller.xui.Trader.Trader is TileEntityVendingMachine tileEntityVendingMachine)
		{
			TraderInfo traderInfo = _controller.xui.Trader.TraderData.TraderInfo;
			bool playerOwned = traderInfo.PlayerOwned;
			bool rentable = traderInfo.Rentable;
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
		if (xUiC_TraderItemEntry?.Item == null)
		{
			base.Enabled = false;
			return;
		}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshBinding()
	{
		XUiC_TraderItemEntry xUiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
		if (xUiC_TraderItemEntry != null)
		{
			int slotIndex = xUiC_TraderItemEntry.SlotIndex;
			List<TraderData.Entry> list = xUiC_TraderItemEntry.xui?.Trader?.TraderData?.PrimaryInventory;
			if (list != null && slotIndex < list.Count)
			{
				xUiC_TraderItemEntry.Item = list[slotIndex].Item;
			}
			else
			{
				xUiC_TraderItemEntry.Item = null;
			}
		}
	}

	public override void OnActivated()
	{
		refreshBinding();
		XUiC_TraderItemEntry xUiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
		TraderData traderData = base.ItemController.xui.Trader.TraderData;
		try
		{
			ItemStack itemStack = xUiC_TraderItemEntry.Item.Clone();
			ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
			ItemValue itemValue = itemStack.itemValue;
			int count = xUiC_TraderItemEntry.InfoWindow.BuySellCounter.Count;
			int buyPrice = XUiM_Trader.GetBuyPrice(base.ItemController.xui, itemStack.itemValue, count, forId, xUiC_TraderItemEntry.SlotIndex);
			XUi xui = base.ItemController.xui;
			XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
			int num = (forId.IsBlock() ? Block.list[itemValue.type].EconomicBundleSize : forId.EconomicBundleSize);
			int num2 = count % num;
			if (num2 != 0)
			{
				GameManager.ShowTooltip(xui.playerUI.entityPlayer, string.Format(Localization.Get("ttItemCountNotBundleSize"), num));
				count -= num2;
				xUiC_TraderItemEntry.InfoWindow.BuySellCounter.Count = count;
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
					ItemStack item = xUiC_TraderItemEntry.Item;
					item.count -= count;
					xUiC_TraderItemEntry.InfoWindow.BuySellCounter.MaxCount -= count;
					xUiC_TraderItemEntry.Refresh();
					if (item.count == 0)
					{
						traderData.PrimaryInventory.RemoveAt(xUiC_TraderItemEntry.SlotIndex);
					}
					Manager.PlayInsidePlayerHead("craft_take_item");
				}
				else if (num4 != itemStack2.count)
				{
					ItemStack item2 = xUiC_TraderItemEntry.Item;
					if (itemStack2.count == 0)
					{
						itemStack2 = ItemStack.Empty;
						traderData.PrimaryInventory.RemoveAt(xUiC_TraderItemEntry.SlotIndex);
					}
					else
					{
						item2.count -= num4 - itemStack2.count;
						xUiC_TraderItemEntry.InfoWindow.BuySellCounter.MaxCount -= count;
						xUiC_TraderItemEntry.Refresh();
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
					if (playerInventory.AddItem(itemStack, _playCollectSound: false))
					{
						playerInventory.RemoveItem(itemStack3);
						traderData.AvailableMoney += buyPrice;
						XUiC_TraderItemEntry xUiC_TraderItemEntry2 = xUiC_TraderItemEntry;
						ItemStack item3 = xUiC_TraderItemEntry2.Item;
						item3.count -= count;
						xUiC_TraderItemEntry2.InfoWindow.BuySellCounter.MaxCount -= count;
						xUiC_TraderItemEntry2.Refresh();
						if (item3.count == 0)
						{
							traderData.PrimaryInventory.RemoveAt(xUiC_TraderItemEntry2.SlotIndex);
						}
						if (base.ItemController.xui.Trader.Trader is EntityTrader entityTrader)
						{
							Manager.PlayInsidePlayerHead("ui_trader_purchase");
							QuestEventManager.Current.BoughtItems(entityTrader.EntityName, count);
						}
						else
						{
							Manager.PlayInsidePlayerHead("ui_vending_purchase");
							QuestEventManager.Current.BoughtItems("", count);
						}
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
			traderData.SetModified(base.ItemController.xui.Trader.Trader);
		}
	}
}

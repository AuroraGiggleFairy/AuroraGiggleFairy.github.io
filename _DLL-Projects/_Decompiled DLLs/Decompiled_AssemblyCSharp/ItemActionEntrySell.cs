using System;
using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntrySell : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum StateTypes
	{
		Normal,
		ItemNotSellable,
		ItemNotSellableVending,
		PriceTooLow,
		NotEnoughItems,
		NotBundleSize
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isOwner;

	[PublicizedFrom(EAccessModifier.Private)]
	public StateTypes state;

	public ItemActionEntrySell(XUiController _controller)
		: base(_controller, "OVERRIDDEN BELOW", "OVERRIDDEN BELOW", GamepadShortCut.None, "")
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
		if (isOwner)
		{
			base.ActionName = Localization.Get("lblContextActionAdd");
			base.IconName = "ui_game_symbol_hand";
		}
		else
		{
			base.ActionName = Localization.Get("lblContextActionSell");
			base.IconName = "ui_game_symbol_coin";
		}
	}

	public override void RefreshEnabled()
	{
		XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		int count = xUiC_ItemStack.InfoWindow.BuySellCounter.Count;
		state = StateTypes.Normal;
		if (isOwner)
		{
			ItemStack itemStack = xUiC_ItemStack.ItemStack.Clone();
			if (count <= 0)
			{
				state = StateTypes.NotEnoughItems;
			}
			if (!itemStack.IsEmpty())
			{
				float num = (itemStack.itemValue.ItemClass.IsBlock() ? Block.list[itemStack.itemValue.type].EconomicValue : itemStack.itemValue.ItemClass.EconomicValue);
				if (!itemStack.itemValue.ItemClass.CanDrop() || num <= 0f)
				{
					state = StateTypes.ItemNotSellableVending;
				}
			}
		}
		else if (!xUiC_ItemStack.ItemStack.IsEmpty())
		{
			ItemStack itemStack2 = xUiC_ItemStack.ItemStack.Clone();
			ItemClass itemClass = itemStack2.itemValue.ItemClass;
			if (!(itemClass.IsBlock() ? Block.list[itemStack2.itemValue.type].SellableToTrader : itemClass.SellableToTrader))
			{
				state = StateTypes.ItemNotSellable;
			}
			else if (XUiM_Trader.GetSellPrice(base.ItemController.xui, itemStack2.itemValue, count, itemClass) <= 0)
			{
				state = StateTypes.PriceTooLow;
			}
		}
		base.Enabled = state == StateTypes.Normal;
	}

	public override void OnDisabledActivate()
	{
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		switch (state)
		{
		case StateTypes.ItemNotSellable:
		case StateTypes.PriceTooLow:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttCannotSellItem"), _showImmediately: false, _pinTooltip: false, 0.5f);
			if (base.ItemController.xui.Trader.TraderEntity != null)
			{
				base.ItemController.xui.Trader.TraderEntity.PlayVoiceSetEntry("refuse", entityPlayer);
			}
			break;
		case StateTypes.ItemNotSellableVending:
			GameManager.ShowTooltip(entityPlayer, "You cannot sell this item.");
			break;
		}
		Manager.PlayInsidePlayerHead("ui_denied");
	}

	public override void OnActivated()
	{
		try
		{
			EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
			XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)base.ItemController;
			ItemValue itemValue = xUiC_ItemStack.ItemStack.itemValue;
			ItemClass forId = ItemClass.GetForId(xUiC_ItemStack.ItemStack.itemValue.type);
			ItemValue item = ItemClass.GetItem(TraderInfo.CurrencyItem);
			if (xUiC_ItemStack.ItemStack.IsEmpty() || forId == null)
			{
				return;
			}
			int num = (forId.IsBlock() ? Block.list[itemValue.type].EconomicBundleSize : forId.EconomicBundleSize);
			if (xUiC_ItemStack.ItemStack.count < num)
			{
				GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("ttItemCountNotBundleSize"), num));
				Manager.PlayInsidePlayerHead("ui_denied");
				return;
			}
			if (isOwner)
			{
				int count = xUiC_ItemStack.InfoWindow.BuySellCounter.Count;
				if (xUiC_ItemStack.ItemStack.itemValue.type == item.type)
				{
					if (base.ItemController.xui.Trader.Trader.AvailableMoney + count > 6 * base.ItemController.xui.Trader.Trader.TraderInfo.RentCost)
					{
						GameManager.ShowTooltip(entityPlayer, Localization.Get("ttVendingMachineMaxCoin"));
						Manager.PlayInsidePlayerHead("ui_denied");
						return;
					}
					base.ItemController.xui.Trader.Trader.AvailableMoney += count;
					base.ItemController.xui.Trader.TraderWindowGroup.RefreshTraderWindow();
				}
				else
				{
					int num2 = 50;
					if (base.ItemController.xui.Trader.Trader.PrimaryInventory.Count == num2)
					{
						int num3 = count;
						for (int i = 0; i < base.ItemController.xui.Trader.Trader.PrimaryInventory.Count; i++)
						{
							if (base.ItemController.xui.Trader.Trader.PrimaryInventory[i].itemValue.type == xUiC_ItemStack.ItemStack.itemValue.type)
							{
								int num4 = forId.Stacknumber.Value - base.ItemController.xui.Trader.Trader.PrimaryInventory[i].count;
								num3 -= num4;
								if (num3 <= 0)
								{
									break;
								}
							}
						}
						if (num3 > 0)
						{
							GameManager.ShowTooltip(entityPlayer, Localization.Get("ttVendingMachineMaxItems"));
							Manager.PlayInsidePlayerHead("ui_denied");
							return;
						}
					}
					int num5 = count % num;
					if (num5 != 0)
					{
						GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("ttItemCountNotBundleSize"), num));
						count -= num5;
						xUiC_ItemStack.InfoWindow.BuySellCounter.Count = count;
						Manager.PlayInsidePlayerHead("ui_denied");
						return;
					}
					ItemStack stack = xUiC_ItemStack.ItemStack.Clone();
					stack = ItemActionEntryScrap.HandleRemoveAmmo(stack, base.ItemController.xui);
					stack.count = count;
					base.ItemController.xui.Trader.Trader.AddToPrimaryInventory(stack, addMarkup: true);
				}
				base.ItemController.xui.Trader.TraderWindowGroup.RefreshTraderItems();
				Manager.PlayInsidePlayerHead("craft_place_item");
				xUiC_ItemStack.ItemStack.count -= count;
				xUiC_ItemStack.InfoWindow.BuySellCounter.MaxCount -= count;
				if (xUiC_ItemStack.ItemStack.count == 0)
				{
					xUiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
					xUiC_ItemStack.Selected = false;
				}
				else
				{
					xUiC_ItemStack.ForceRefreshItemStack();
				}
				return;
			}
			int count2 = xUiC_ItemStack.InfoWindow.BuySellCounter.Count;
			int primaryItemCount = base.ItemController.xui.Trader.Trader.GetPrimaryItemCount(xUiC_ItemStack.ItemStack.itemValue);
			int num6 = Math.Min(forId.Stacknumber.Value * 3 - primaryItemCount, count2);
			if (num6 <= 0)
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttCannotSellItem"));
				Manager.PlayInsidePlayerHead("ui_denied");
				return;
			}
			if (num6 != count2)
			{
				GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("ttCanOnlySellAmount"), num6));
				xUiC_ItemStack.InfoWindow.BuySellCounter.Count = num6;
				Manager.PlayInsidePlayerHead("ui_denied");
				return;
			}
			int num7 = num6 % num;
			if (num7 != 0)
			{
				GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("ttItemCountNotBundleSize"), num));
				num6 -= num7;
				xUiC_ItemStack.InfoWindow.BuySellCounter.Count = num6;
				Manager.PlayInsidePlayerHead("ui_denied");
				return;
			}
			ItemStack itemStack = xUiC_ItemStack.ItemStack.Clone();
			int sellPrice = XUiM_Trader.GetSellPrice(base.ItemController.xui, itemStack.itemValue, count2, forId);
			XUiM_PlayerInventory playerInventory = base.ItemController.xui.PlayerInventory;
			ItemStack itemStack2 = new ItemStack(item.Clone(), sellPrice);
			itemStack.count = count2;
			int slotCount = xUiC_ItemStack.xui.PlayerInventory.Backpack.SlotCount;
			int slotNumber = xUiC_ItemStack.SlotNumber + ((xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) ? slotCount : 0);
			if (playerInventory.CanSwapItems(itemStack, itemStack2, slotNumber))
			{
				itemStack = ItemActionEntryScrap.HandleRemoveAmmo(itemStack, base.ItemController.xui);
				xUiC_ItemStack.ItemStack.count -= count2;
				xUiC_ItemStack.InfoWindow.BuySellCounter.MaxCount -= count2;
				if (xUiC_ItemStack.ItemStack.count == 0)
				{
					xUiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
					xUiC_ItemStack.Selected = false;
				}
				else
				{
					xUiC_ItemStack.ForceRefreshItemStack();
				}
				base.ItemController.xui.Trader.Trader.AddToPrimaryInventory(itemStack, addMarkup: false);
				base.ItemController.xui.Trader.TraderWindowGroup.RefreshTraderItems();
				if (base.ItemController.xui.Trader.TraderEntity != null)
				{
					QuestEventManager.Current.SoldItems(base.ItemController.xui.Trader.TraderEntity.EntityName, count2);
					entityPlayer.Progression.AddLevelExp(Math.Max(Convert.ToInt32(sellPrice), 1), "_xpFromSelling", Progression.XPTypes.Selling);
				}
				playerInventory.AddItem(itemStack2, playCollectSound: false);
				Manager.PlayInsidePlayerHead("ui_trader_purchase");
			}
			else
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttNoSpaceForSelling"));
				Manager.PlayInsidePlayerHead("ui_denied");
			}
		}
		finally
		{
			base.ItemController.xui.Trader?.TraderTileEntity?.SetModified();
		}
	}
}

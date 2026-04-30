using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TraderWindow : XUiController
{
	public enum TraderActionTypes
	{
		Buy,
		Sell,
		BuyBack
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServiceInfoWindow serviceInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderItemList itemListGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> buyInventory = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> currentIndexList = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> currentInventory = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedSlot = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowicon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button rentButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button showAllButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayerName playerName;

	public bool CompletedTransaction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblGeneralStock;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblSecretStash;

	[PublicizedFrom(EAccessModifier.Private)]
	public string category = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSecretStash;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasSecretStash;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOwner;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerOwned;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRentable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isVending;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showAll;

	[PublicizedFrom(EAccessModifier.Private)]
	public int traderStage = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> rentTimeLeftFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => string.Format("{0}: {1} {2}", Localization.Get("xuiExpires"), Localization.Get("xuiDay"), _i));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> timeLeftFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => string.Format("{0} {1}", Localization.Get("xuiDay"), _i));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt availableMoneyFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	public int Page
	{
		get
		{
			return page;
		}
		set
		{
			if (page != value)
			{
				page = value;
				itemListGrid.Page = page;
				pager?.SetPage(page);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		lblGeneralStock = Localization.Get("xuiGeneralStock");
		lblSecretStash = Localization.Get("xuiSecretStash");
		windowicon = GetChildById("windowicon");
		playerName = (XUiC_PlayerName)GetChildById("playerName");
		categoryList = windowGroup.Controller.GetChildByType<XUiC_CategoryList>();
		categoryList.CategoryChanged += HandleCategoryChanged;
		pager = GetChildByType<XUiC_Paging>();
		if (pager != null)
		{
			pager.OnPageChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Page = pager.CurrentPageNumber;
				GetItemStackData(txtInput.Text);
			};
		}
		for (int num = 0; num < children.Count; num++)
		{
			children[num].OnScroll += HandleOnScroll;
		}
		base.OnScroll += HandleOnScroll;
		itemListGrid = base.Parent.GetChildByType<XUiC_TraderItemList>();
		XUiController[] childrenByType = itemListGrid.GetChildrenByType<XUiC_TraderItemEntry>();
		XUiController[] array = childrenByType;
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			array[num2].OnScroll += HandleOnScroll;
			((XUiC_TraderItemEntry)array[num2]).TraderWindow = this;
		}
		txtInput = (XUiC_TextInput)windowGroup.Controller.GetChildById("searchInput");
		if (txtInput != null)
		{
			txtInput.OnChangeHandler += HandleOnChangedHandler;
			txtInput.Text = "";
		}
		XUiController childById = GetChildById("collect");
		if (childById != null)
		{
			childById.OnPress += Collect_OnPress;
		}
		childById = GetChildById("takeAll");
		if (childById != null)
		{
			childById.OnPress += TakeAll_OnPress;
		}
		childById = GetChildById("rent");
		if (childById != null)
		{
			childById.OnPress += Rent_OnPress;
			rentButton = (XUiV_Button)childById.ViewComponent;
		}
		childById = GetChildById("showAll");
		if (childById != null)
		{
			childById.OnPress += ShowAll_OnPress;
			showAllButton = (XUiV_Button)childById.ViewComponent;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowAll_OnPress(XUiController _sender, int _mouseButton)
	{
		showAll = !showAll;
		showAllButton.Selected = showAll;
		RefreshTraderItems();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Rent_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!isVending)
		{
			return;
		}
		TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
		if (serviceInfoWindow == null)
		{
			serviceInfoWindow = (XUiC_ServiceInfoWindow)windowGroup.Controller.GetChildById("serviceInfoPanel");
		}
		InGameService service = new InGameService
		{
			Name = Localization.Get("rentVendingMachine"),
			Description = Localization.Get("rentVendingMachineDesc"),
			Icon = "ui_game_symbol_vending",
			Price = tileEntityVendingMachine.TraderData.TraderInfo.RentCost,
			VisibleChangedHandler = [PublicizedFrom(EAccessModifier.Private)] (bool visible) =>
			{
				if (rentButton != null)
				{
					rentButton.Selected = visible;
				}
			}
		};
		if (base.xui.currentSelectedEntry != null)
		{
			base.xui.currentSelectedEntry.Selected = false;
		}
		serviceInfoWindow.SetInfo(service, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Collect_OnPress(XUiController _sender, int _mouseButton)
	{
		if (isVending)
		{
			TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
			if ((playerOwned || isRentable) && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				ItemValue item = ItemClass.GetItem(TraderInfo.CurrencyItem);
				int availableMoney = base.xui.Trader.Trader.AvailableMoney;
				XUiM_PlayerInventory playerInventory = base.xui.PlayerInventory;
				ItemStack itemStack = new ItemStack(item.Clone(), availableMoney);
				playerInventory.AddItem(itemStack);
				base.xui.Trader.Trader.AvailableMoney = itemStack.count;
				RefreshBindings();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TakeAll_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!isVending)
		{
			return;
		}
		TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
		if ((!playerOwned && !isRentable) || !tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return;
		}
		TraderData trader = base.xui.Trader.Trader;
		XUiM_PlayerInventory playerInventory = base.xui.PlayerInventory;
		bool flag = false;
		for (int i = 0; i < trader.PrimaryInventory.Count; i++)
		{
			ItemStack itemStack = trader.PrimaryInventory[i];
			int num = (itemStack.itemValue.ItemClass.IsBlock() ? Block.list[itemStack.itemValue.type].EconomicBundleSize : itemStack.itemValue.ItemClass.EconomicBundleSize);
			int num2 = Math.Min(itemStack.count, base.xui.PlayerInventory.CountAvailableSpaceForItem(itemStack.itemValue)) / num * num;
			int num3 = itemStack.count - num2;
			itemStack.count = num2;
			if (playerInventory.AddItem(itemStack, playCollectSound: false))
			{
				flag = true;
			}
			itemStack.count += num3;
			if (itemStack.count == 0)
			{
				trader.RemoveMarkup(i);
				trader.PrimaryInventory.RemoveAt(i--);
			}
		}
		if (flag && GameManager.Instance != null && GameManager.Instance.World != null)
		{
			Manager.Play(GameManager.Instance.World.GetPrimaryPlayer(), "UseActions/takeall1");
		}
		RefreshTraderItems();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		string text = _categoryEntry.CategoryName;
		if (text == "SECRET STASH")
		{
			text = "";
			isSecretStash = true;
		}
		else
		{
			isSecretStash = false;
		}
		RefreshHeader();
		Page = 0;
		SetCategory(text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshHeader()
	{
		if (base.xui.Trader.TraderTileEntity != null && isVending)
		{
			TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
			if (tileEntityVendingMachine.IsRentable || tileEntityVendingMachine.TraderData.TraderInfo.PlayerOwned)
			{
				if (tileEntityVendingMachine.GetOwner() != null)
				{
					string displayName = GameManager.Instance.persistentPlayers.GetPlayerData(tileEntityVendingMachine.GetOwner()).PlayerName.DisplayName;
					playerName.SetGenericName(string.Format(Localization.Get("xuiVendingWithOwner"), displayName));
				}
				else
				{
					playerName.SetGenericName(Localization.Get("xuiEmptyVendingMachine"));
				}
			}
			else
			{
				playerName.SetGenericName(lblGeneralStock);
			}
			if (windowicon != null)
			{
				((XUiV_Sprite)windowicon.ViewComponent).SpriteName = "ui_game_symbol_vending";
			}
		}
		else
		{
			playerName.SetGenericName(isSecretStash ? lblSecretStash : lblGeneralStock);
			((XUiV_Sprite)windowicon.ViewComponent).SpriteName = "ui_game_symbol_map_trader";
		}
	}

	public void RefreshOwner()
	{
		if (isVending)
		{
			isOwner = (base.xui.Trader.TraderTileEntity as TileEntityVendingMachine).LocalPlayerIsOwner();
		}
		else
		{
			isOwner = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		Page = 0;
		FilterByName(_text);
		itemListGrid.SetItems(currentInventory.ToArray(), currentIndexList);
		if (currentInventory.Count == 0 || currentInventory[0].IsEmpty())
		{
			GetChildById("searchControls").SelectCursorElement(_withDelay: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			pager?.PageDown();
		}
		else
		{
			pager?.PageUp();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetItemStackData(string _name)
	{
		if (_name == null)
		{
			_name = "";
		}
		currentInventory.Clear();
		length = itemListGrid.Length;
		FilterByName(_name);
		itemListGrid.SetItems(currentInventory.ToArray(), currentIndexList);
		if (!isSecretStash)
		{
			categoryList.SetupCategoriesBasedOnItems(buyInventory, traderStage);
		}
		if (currentInventory.Count == 0 || currentInventory[0].IsEmpty())
		{
			GetChildById("searchControls").SelectCursorElement(_withDelay: true);
		}
	}

	public void FilterByName(string _name)
	{
		currentIndexList.Clear();
		currentInventory.Clear();
		for (int i = 0; i < buyInventory.Count; i++)
		{
			if (buyInventory[i] == null || buyInventory[i].count == 0)
			{
				buyInventory.RemoveAt(i);
				i--;
				continue;
			}
			ItemClass itemClass = buyInventory[i].itemValue.ItemClass;
			string text = itemClass.GetLocalizedItemName();
			if (text == null)
			{
				text = Localization.Get(itemClass.Name);
			}
			if (category == "")
			{
				if (!(_name == "") && !itemClass.Name.ContainsCaseInsensitive(_name) && !text.ContainsCaseInsensitive(_name) && !(buyInventory[i].itemValue.GetItemOrBlockId().ToString() == _name.Trim()))
				{
					continue;
				}
				TraderStageTemplateGroup traderStageTemplateGroup = null;
				if (itemClass.TraderStageTemplate != null)
				{
					if (!TraderManager.TraderStageTemplates.ContainsKey(itemClass.TraderStageTemplate))
					{
						throw new Exception("TraderStageTemplate " + itemClass.TraderStageTemplate + " for item: " + itemClass.GetLocalizedItemName() + " does not exist.");
					}
					traderStageTemplateGroup = TraderManager.TraderStageTemplates[itemClass.TraderStageTemplate];
				}
				if (traderStageTemplateGroup == null || traderStage == -1 || traderStageTemplateGroup.IsWithin(traderStage, buyInventory[i].itemValue.Quality) || showAll)
				{
					currentIndexList.Add(i);
					currentInventory.Add(buyInventory[i]);
				}
				continue;
			}
			string[] array = itemClass.Groups;
			if (itemClass.IsBlock())
			{
				array = Block.list[buyInventory[i].itemValue.type].GroupNames;
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] == null || !array[j].EqualsCaseInsensitive(category) || (!(_name == "") && !itemClass.Name.ContainsCaseInsensitive(_name) && !text.ContainsCaseInsensitive(_name) && !(buyInventory[i].itemValue.GetItemOrBlockId().ToString() == _name.Trim())))
				{
					continue;
				}
				TraderStageTemplateGroup traderStageTemplateGroup2 = null;
				if (itemClass.TraderStageTemplate != null)
				{
					if (!TraderManager.TraderStageTemplates.ContainsKey(itemClass.TraderStageTemplate))
					{
						throw new Exception("TraderStageTemplate " + itemClass.TraderStageTemplate + " for item: " + itemClass.GetLocalizedItemName() + " does not exist.");
					}
					traderStageTemplateGroup2 = TraderManager.TraderStageTemplates[itemClass.TraderStageTemplate];
				}
				if (traderStageTemplateGroup2 == null || traderStage == -1 || traderStageTemplateGroup2.IsWithin(traderStage, buyInventory[i].itemValue.Quality) || showAll)
				{
					currentIndexList.Add(i);
					currentInventory.Add(buyInventory[i]);
				}
			}
		}
		pager?.SetLastPageByElementsAndPageLength(currentInventory.Count, length);
	}

	public void SetCategory(string _category)
	{
		if (txtInput != null)
		{
			txtInput.Text = "";
		}
		category = _category;
		RefreshTraderItems();
	}

	public string GetCategory()
	{
		return category;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.xui.Trader.TraderTileEntity != null)
		{
			isVending = base.xui.Trader.TraderTileEntity is TileEntityVendingMachine;
			if (isVending)
			{
				Manager.PlayInsidePlayerHead("open_vending");
			}
		}
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (base.xui.Trader.TraderEntity != null)
		{
			traderStage = entityPlayer.GetTraderStage(entityPlayer.QuestJournal.GetCurrentFactionTier(base.xui.Trader.TraderEntity.NPCInfo.QuestFaction));
		}
		else
		{
			traderStage = -1;
		}
		CompletedTransaction = false;
		if (base.xui.Trader.Trader != null)
		{
			categoryList.SetCategoryToFirst();
		}
		playerOwned = base.xui.Trader.TraderTileEntity.TraderData.TraderInfo.PlayerOwned;
		isRentable = base.xui.Trader.TraderTileEntity.TraderData.TraderInfo.Rentable;
		if (isRentable && isOwner && ((TileEntityVendingMachine)base.xui.Trader.TraderTileEntity).TryAutoBuy())
		{
			RefreshTraderItems();
		}
		Refresh();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (base.xui.Trader.TraderEntity != null)
		{
			if (CompletedTransaction)
			{
				base.xui.Trader.TraderEntity.PlayVoiceSetEntry("sale_accepted", base.xui.playerUI.entityPlayer);
			}
			else
			{
				base.xui.Trader.TraderEntity.PlayVoiceSetEntry("sale_declined", base.xui.playerUI.entityPlayer);
			}
			base.xui.Trader.TraderEntity = null;
		}
		else
		{
			Manager.PlayInsidePlayerHead("close_vending");
		}
		if (base.xui.Trader.TraderTileEntity != null)
		{
			TileEntityTrader traderTileEntity = base.xui.Trader.TraderTileEntity;
			Vector3i blockPos = traderTileEntity.ToWorldPos();
			traderTileEntity.SetModified();
			traderTileEntity.SetUserAccessing(_bUserAccessing: false);
			GameManager.Instance.TEUnlockServer(traderTileEntity.GetClrIdx(), blockPos, traderTileEntity.entityId);
			base.xui.Trader.TraderTileEntity = null;
		}
		base.xui.Trader.Trader = null;
	}

	public void RefreshTraderItems()
	{
		buyInventory.Clear();
		ItemStack[] stashSlots = GetStashSlots();
		hasSecretStash = stashSlots != null;
		ItemStack[] array = ((isSecretStash && hasSecretStash) ? stashSlots : base.xui.Trader.Trader.PrimaryInventory.ToArray());
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				buyInventory.Add(array[i]);
			}
		}
		GetItemStackData(txtInput.Text);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] GetStashSlots()
	{
		float value = EffectManager.GetValue(PassiveEffects.SecretStash, null, base.xui.playerUI.entityPlayer.Progression.Level, base.xui.playerUI.entityPlayer);
		for (int i = 0; i < base.xui.Trader.Trader.TierItemGroups.Count; i++)
		{
			TraderInfo.TierItemGroup tierItemGroup = base.xui.Trader.Trader.TraderInfo.TierItemGroups[i];
			if ((value >= (float)tierItemGroup.minLevel || tierItemGroup.minLevel == -1) && (value <= (float)tierItemGroup.maxLevel || tierItemGroup.maxLevel == -1))
			{
				return base.xui.Trader.Trader.TierItemGroups[i];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "tradername":
			value = "";
			if (base.xui.Trader.Trader != null)
			{
				if (base.xui.Trader.TraderEntity != null)
				{
					value = base.xui.Trader.TraderEntity.EntityName;
				}
				else if (base.xui.Trader.TraderTileEntity != null)
				{
					value = Localization.Get("VendingMachine");
				}
			}
			return true;
		case "renttimeleft":
			if (base.xui.Trader.TraderTileEntity != null && isVending)
			{
				TileEntityVendingMachine tileEntityVendingMachine4 = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
				if (isOwner && tileEntityVendingMachine4.IsRentable)
				{
					int rentalEndDay = tileEntityVendingMachine4.RentalEndDay;
					value = rentTimeLeftFormatter.Format(rentalEndDay);
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "timeleft":
			if (base.xui.Trader.Trader != null)
			{
				int v = GameUtils.WorldTimeToDays(base.xui.Trader.Trader.NextResetTime);
				value = timeLeftFormatter.Format(v);
			}
			return true;
		case "showrestock":
			value = (base.xui.Trader.Trader != null && base.xui.Trader.Trader.TraderInfo.ResetInterval > 0).ToString();
			return true;
		case "isrentable":
			if (isVending)
			{
				TileEntityVendingMachine tileEntityVendingMachine2 = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
				value = tileEntityVendingMachine2.IsRentable.ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "isrenter":
			if (isVending)
			{
				TileEntityVendingMachine tileEntityVendingMachine3 = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
				value = (tileEntityVendingMachine3.IsRentable && isOwner).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "isowner":
			if (isVending)
			{
				TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
				value = ((playerOwned || isRentable) && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "isnotowner":
			if (isVending)
			{
				TileEntityVendingMachine tileEntityVendingMachine6 = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
				value = ((!playerOwned && !isRentable) || !tileEntityVendingMachine6.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "isownerorrentable":
			if (isVending)
			{
				TileEntityVendingMachine tileEntityVendingMachine5 = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
				value = (tileEntityVendingMachine5.IsRentable || (playerOwned && (isOwner || tileEntityVendingMachine5.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)))).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "availablemoney":
			value = ((base.xui.Trader.Trader != null) ? availableMoneyFormatter.Format(base.xui.Trader.Trader.AvailableMoney) : "");
			return true;
		case "restocklabel":
			value = Localization.Get("xuiRestock");
			return true;
		case "is_debug":
			value = (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && !isVending).ToString();
			return true;
		default:
			return false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!(Time.time > updateTime))
		{
			return;
		}
		updateTime = Time.time + 1f;
		if (base.xui.Trader.Trader == null || (base.xui.Trader.TraderTileEntity.syncNeeded && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer))
		{
			return;
		}
		if (base.xui.Trader.TraderTileEntity != null && isVending)
		{
			TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
			if (isRentable && tileEntityVendingMachine.GetOwner() != null && tileEntityVendingMachine.RentTimeRemaining <= 0f)
			{
				tileEntityVendingMachine.ClearVendingMachine();
				Refresh();
				RefreshTraderItems();
			}
		}
		if (base.xui.Trader.Trader.CurrentTime <= 0f && !base.xui.Trader.Trader.TraderInfo.PlayerOwned && !base.xui.Trader.Trader.TraderInfo.Rentable && GameManager.Instance.traderManager.TraderInventoryRequested(base.xui.Trader.Trader, XUiM_Player.GetPlayer().entityId))
		{
			if (base.xui.Trader.TraderTileEntity != null)
			{
				base.xui.Trader.TraderTileEntity.SetModified();
			}
			XUiM_Player.GetPlayer().PlayOneShot("ui_trader_inv_reset");
			RefreshTraderItems();
			RefreshBindings();
		}
	}

	public void Refresh()
	{
		RefreshOwner();
		RefreshHeader();
		RefreshBindings(_forceAll: true);
	}
}

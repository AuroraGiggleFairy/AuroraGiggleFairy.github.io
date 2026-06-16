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
	public List<TraderData.Entry> buyInventory = new List<TraderData.Entry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> currentIndexList = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> currentInventory = new List<ItemStack>();

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

	public TileEntityVendingMachine CurrentVending
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return xui.Trader.Trader as TileEntityVendingMachine;
		}
	}

	public EntityTrader CurrentTraderEntity
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return xui.Trader.Trader as EntityTrader;
		}
	}

	public bool IsVending
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CurrentVending != null;
		}
	}

	public bool IsOwner
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (CurrentVending != null)
			{
				return CurrentVending.LocalPlayerIsOwner();
			}
			return false;
		}
	}

	public bool IsPlayerOwned
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return xui.Trader.TraderData?.TraderInfo?.PlayerOwned == true;
		}
	}

	public bool IsRentable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return xui.Trader.TraderData?.TraderInfo?.Rentable == true;
		}
	}

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
		if (!IsVending)
		{
			return;
		}
		if (serviceInfoWindow == null)
		{
			serviceInfoWindow = (XUiC_ServiceInfoWindow)windowGroup.Controller.GetChildById("serviceInfoPanel");
		}
		InGameService service = new InGameService
		{
			Name = Localization.Get("rentVendingMachine"),
			Description = Localization.Get("rentVendingMachineDesc"),
			Icon = "ui_game_symbol_vending",
			Price = CurrentVending.TraderData.TraderInfo.RentCost,
			VisibleChangedHandler = [PublicizedFrom(EAccessModifier.Private)] (bool visible) =>
			{
				if (rentButton != null)
				{
					rentButton.Selected = visible;
				}
			}
		};
		if (xui.CurrentSelectedEntry != null)
		{
			xui.CurrentSelectedEntry.IsSelected = false;
		}
		serviceInfoWindow.SetInfo(service, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Collect_OnPress(XUiController _sender, int _mouseButton)
	{
		if (IsVending && (IsPlayerOwned || IsRentable) && CurrentVending.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			ItemValue item = ItemClass.GetItem(TraderInfo.CurrencyItem);
			int availableMoney = xui.Trader.TraderData.AvailableMoney;
			XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
			ItemStack itemStack = new ItemStack(item.Clone(), availableMoney);
			playerInventory.AddItem(itemStack);
			xui.Trader.TraderData.AvailableMoney = itemStack.count;
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TakeAll_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!IsVending || (!IsPlayerOwned && !IsRentable) || !CurrentVending.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return;
		}
		TraderData traderData = xui.Trader.TraderData;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		bool flag = false;
		for (int i = 0; i < traderData.PrimaryInventory.Count; i++)
		{
			ItemStack item = traderData.PrimaryInventory[i].Item;
			int num = (item.itemValue.ItemClass.IsBlock() ? Block.list[item.itemValue.type].EconomicBundleSize : item.itemValue.ItemClass.EconomicBundleSize);
			int num2 = Math.Min(item.count, xui.PlayerInventory.CountAvailableSpaceForItem(item.itemValue)) / num * num;
			int num3 = item.count - num2;
			item.count = num2;
			if (playerInventory.AddItem(item, _playCollectSound: false))
			{
				flag = true;
			}
			item.count += num3;
			if (item.count == 0)
			{
				traderData.PrimaryInventory.RemoveAt(i--);
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
		if (IsVending)
		{
			if (IsRentable || IsPlayerOwned)
			{
				if (CurrentVending.GetOwner() != null)
				{
					string displayName = GameManager.Instance.persistentPlayers.GetPlayerData(CurrentVending.GetOwner()).PlayerName.DisplayName;
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
			TraderData.Entry entry = buyInventory[i];
			ItemStack item = entry.Item;
			if (item == null || item.count == 0)
			{
				continue;
			}
			ItemClass itemClass = item.itemValue.ItemClass;
			string text = itemClass.GetLocalizedItemName();
			if (text == null)
			{
				text = Localization.Get(itemClass.Name);
			}
			if (category == "")
			{
				if (!(_name == "") && !itemClass.Name.ContainsCaseInsensitive(_name) && !text.ContainsCaseInsensitive(_name) && !(item.itemValue.GetItemOrBlockId().ToString() == _name.Trim()))
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
				if (traderStageTemplateGroup == null || traderStage == -1 || traderStageTemplateGroup.IsWithin(traderStage, item.itemValue.Quality) || showAll || entry.AddedByPlayer)
				{
					currentIndexList.Add(i);
					currentInventory.Add(item);
				}
				continue;
			}
			string[] array = itemClass.Groups;
			if (itemClass.IsBlock())
			{
				array = Block.list[item.itemValue.type].GroupNames;
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] == null || !array[j].EqualsCaseInsensitive(category) || (!(_name == "") && !itemClass.Name.ContainsCaseInsensitive(_name) && !text.ContainsCaseInsensitive(_name) && !(item.itemValue.GetItemOrBlockId().ToString() == _name.Trim())))
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
				if (traderStageTemplateGroup2 == null || traderStage == -1 || traderStageTemplateGroup2.IsWithin(traderStage, item.itemValue.Quality) || showAll || entry.AddedByPlayer)
				{
					currentIndexList.Add(i);
					currentInventory.Add(item);
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
		if (xui.Trader.Trader != null && IsVending)
		{
			Manager.PlayInsidePlayerHead("open_vending");
		}
		EntityPlayer entityPlayer = xui.playerUI.entityPlayer;
		if (CurrentTraderEntity != null)
		{
			traderStage = entityPlayer.GetTraderStage(entityPlayer.QuestJournal.GetCurrentFactionTier(CurrentTraderEntity.NPCInfo.QuestFaction));
		}
		else
		{
			traderStage = -1;
		}
		CompletedTransaction = false;
		if (xui.Trader.TraderData != null)
		{
			categoryList.SetCategoryToFirst();
		}
		if (IsRentable && IsOwner && CurrentVending != null && CurrentVending.TryAutoBuy())
		{
			RefreshTraderItems();
		}
		Refresh();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (CurrentTraderEntity != null)
		{
			if (CompletedTransaction)
			{
				CurrentTraderEntity.PlayVoiceSetEntry("sale_accepted", xui.playerUI.entityPlayer);
			}
			else
			{
				CurrentTraderEntity.PlayVoiceSetEntry("sale_declined", xui.playerUI.entityPlayer);
			}
		}
		else
		{
			Manager.PlayInsidePlayerHead("close_vending");
		}
		xui.Trader?.TraderData?.SetModified(xui.Trader.Trader);
		if (CurrentVending != null)
		{
			CurrentVending.SetUserAccessing(_bUserAccessing: false);
		}
		LockManager.Instance.UnlockRequestLocal();
		xui.Trader.Trader = null;
	}

	public void RefreshTraderItems()
	{
		buyInventory.Clear();
		ItemStack[] stashSlots = GetStashSlots();
		hasSecretStash = stashSlots != null;
		if (isSecretStash && hasSecretStash)
		{
			for (int i = 0; i < stashSlots.Length; i++)
			{
				buyInventory.Add(new TraderData.Entry(stashSlots[i], 0));
			}
		}
		else
		{
			buyInventory.AddRange(xui.Trader.TraderData.PrimaryInventory);
		}
		GetItemStackData(txtInput.Text);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] GetStashSlots()
	{
		float value = EffectManager.GetValue(PassiveEffects.SecretStash, null, xui.playerUI.entityPlayer.Progression.Level, xui.playerUI.entityPlayer);
		for (int i = 0; i < xui.Trader.TraderData.TierItemGroups.Count; i++)
		{
			TraderInfo.TierItemGroup tierItemGroup = xui.Trader.TraderData.TraderInfo.TierItemGroups[i];
			if ((value >= (float)tierItemGroup.minLevel || tierItemGroup.minLevel == -1) && (value <= (float)tierItemGroup.maxLevel || tierItemGroup.maxLevel == -1))
			{
				return xui.Trader.TraderData.TierItemGroups[i];
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
			if (xui.Trader.TraderData != null)
			{
				if (CurrentTraderEntity != null)
				{
					value = CurrentTraderEntity.EntityName;
				}
				else if (IsVending)
				{
					value = Localization.Get("VendingMachine");
				}
			}
			return true;
		case "renttimeleft":
			if (IsVending)
			{
				if (IsOwner && CurrentVending.IsRentable)
				{
					int rentalEndDay = CurrentVending.RentalEndDay;
					value = rentTimeLeftFormatter.Format(rentalEndDay);
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "timeleft":
			if (xui.Trader.TraderData != null)
			{
				int v = GameUtils.WorldTimeToDays(xui.Trader.TraderData.NextResetTime);
				value = timeLeftFormatter.Format(v);
			}
			return true;
		case "showrestock":
			value = (xui.Trader.TraderData != null && xui.Trader.TraderData.TraderInfo.ResetInterval > 0).ToString();
			return true;
		case "isrentable":
			if (IsVending)
			{
				if (CurrentVending == null)
				{
					value = "false";
					return true;
				}
				value = CurrentVending.IsRentable.ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "isrenter":
			if (IsVending)
			{
				if (CurrentVending == null)
				{
					value = "false";
					return true;
				}
				value = (CurrentVending.IsRentable && IsOwner).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "isowner":
			if (IsVending)
			{
				if (CurrentVending == null)
				{
					value = "false";
					return true;
				}
				value = ((IsPlayerOwned || IsRentable) && CurrentVending.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "isnotowner":
			if (IsVending)
			{
				if (CurrentVending == null)
				{
					value = "true";
					return true;
				}
				value = ((!IsPlayerOwned && !IsRentable) || !CurrentVending.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "isownerorrentable":
			if (IsVending)
			{
				if (CurrentVending == null)
				{
					value = "false";
					return true;
				}
				value = (CurrentVending.IsRentable || (IsPlayerOwned && (IsOwner || CurrentVending.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)))).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "availablemoney":
			value = ((xui.Trader.TraderData != null) ? availableMoneyFormatter.Format(xui.Trader.TraderData.AvailableMoney) : "");
			return true;
		case "restocklabel":
			value = Localization.Get("xuiRestock");
			return true;
		case "is_debug":
			value = (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && !IsVending).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
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
		if (xui.Trader.TraderData != null)
		{
			if (IsVending && IsRentable && CurrentVending.GetOwner() != null && CurrentVending.RentTimeRemaining <= 0f)
			{
				CurrentVending.ClearVendingMachine();
				Refresh();
				RefreshTraderItems();
			}
			if (xui.Trader.TraderData.CurrentTime <= 0f && !xui.Trader.TraderData.TraderInfo.PlayerOwned && !xui.Trader.TraderData.TraderInfo.Rentable && GameManager.Instance.traderManager.TraderInventoryRequested(xui.Trader.TraderData, XUiM_Player.GetPlayer().entityId))
			{
				xui.Trader.TraderData.SetModified(xui.Trader.Trader);
				XUiM_Player.GetPlayer().PlayOneShot("ui_trader_inv_reset");
				RefreshTraderItems();
				RefreshBindings();
			}
		}
	}

	public void Refresh()
	{
		RefreshHeader();
		RefreshBindings();
	}
}

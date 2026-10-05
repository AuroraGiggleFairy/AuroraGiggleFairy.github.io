using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

namespace StorageLaptop
{
	[Preserve]
	public class StorageLaptopWindow : XUiController
	{
		public static StorageLaptopWindow Instance;
		private static int nextRequest;
		private static bool applyOnOpen;
		private static int pendingRequest;
		private static byte pendingStatus;
		private static List<StorageRow> pendingRows;

		private readonly List<StorageRow> source = new List<StorageRow>();
		private readonly List<ViewRow> filtered = new List<ViewRow>();
		private StorageLaptopRow[] rowViews = new StorageLaptopRow[0];
		private XUiC_TextInput search;
		private XUiC_TextInput takeAmountInput;
		private readonly List<XUiController> categoryButtons = new List<XUiController>();
		private readonly List<string> categoryGroups = new List<string>();
		private XUiV_Label emptyLabel;
		private XUiView scrollTrack;
		private XUiView scrollThumb;
		private XUiView takeTrack;
		private XUiView takeThumb;
		private XUiView takePopup;
		private XUiV_Sprite takeIcon;
		private XUiV_Sprite takeTypeIcon;
		private XUiV_Sprite takeLock;
		private XUiV_Label takePopupTitle;
		private XUiV_Label takeChest;
		private XUiV_Label takeQuality;
		private XUiV_Sprite takeDurabilityBg;
		private XUiV_Sprite takeDurabilityFill;
		private XUiV_Label takePopupStock;
		private XUiView contentView;
		private ViewRow pendingTake;
		private string searchText = string.Empty;
		private string selectedCategory = string.Empty;
		private int offset;
		private int requestId;
		private int wheelFrame = -1;
		private bool windowOpen;
		private bool draggingScroll;
		private bool draggingAmount;
		private bool inspectBound;
		private bool headerPending;
		private bool transferOpen;
		private int holdStep;
		private float holdRepeatAt;
		private XUiC_SimpleButton holdDownButton;
		private XUiC_SimpleButton holdUpButton;
		private bool lastHasStats;
		private bool heldActive;
		private string heldName;
		private int heldQuality;
		private bool heldLocked;
		private Vector3i heldChest;
		private bool useAfterRefresh;
		private bool consumePump;
		private XUiC_ItemStack useStack;
		private bool useStackBound;
		private bool suppressUseStack;
		private bool useArmed;
		private int useStackCount;
		private float lastUseChangeTime;
		private bool inspectAfterUse;
		private int pendingConsume;
		private string consumeName = string.Empty;
		private int consumeQuality;
		private bool consumeLocked;
		private Vector3i consumeChest;
		private int useWait;
		private int iconRefresh;
		private int useItemType;
		private int useQuality;
		private ItemActionEntryUse.ConsumeType useKind;
		private bool inspectLayoutReady;
		private XUiView statsBlock;
		private XUiView statsBlockBorder;
		private XUiView onlyBlock;
		private XUiView onlyBlockBorder;
		private XUiView transferView;
		private XUiV_Label transferCount;
		private Vector2i statsHomePos;
		private Vector2i statsHomeSize;
		private int statsBorderHomeHeight;
		private Vector2i onlyHomePos;
		private Vector2i onlyHomeSize;
		private int onlyBorderHomeHeight;

		public override void Init()
		{
			base.Init();
			rowViews = GetChildrenByType<StorageLaptopRow>();
			bool rowsPositioned = false;
			if (rowViews.Length > 1)
			{
				int firstY = rowViews[0].ViewComponent.Position.y;
				for (int i = 1; i < rowViews.Length; i++)
				{
					if (rowViews[i].ViewComponent.Position.y != firstY)
					{
						rowsPositioned = true;
						break;
					}
				}
			}

			if (rowsPositioned)
			{
				Array.Sort(rowViews, (a, b) => b.ViewComponent.Position.y.CompareTo(a.ViewComponent.Position.y));
			}

			search = Find(this, "searchInput") as XUiC_TextInput;
			takeAmountInput = Find(this, "takeAmountInput") as XUiC_TextInput;
			XUiController empty = Find(this, "emptyLabel");
			emptyLabel = empty != null ? empty.ViewComponent as XUiV_Label : null;
			scrollTrack = ViewOf(this, "scrollTrackBg");
			scrollThumb = ViewOf(this, "scrollThumb");
			takeTrack = ViewOf(this, "takeTrack");
			takeThumb = ViewOf(this, "takeThumb");
			takePopup = ViewOf(this, "takePopupBlocker");
			takeIcon = ViewOf(this, "takeIcon") as XUiV_Sprite;
			takeTypeIcon = ViewOf(this, "takeTypeIcon") as XUiV_Sprite;
			takeLock = ViewOf(this, "takeLock") as XUiV_Sprite;
			takePopupTitle = ViewOf(this, "takePopupTitle") as XUiV_Label;
			takeChest = ViewOf(this, "takeChest") as XUiV_Label;
			takeQuality = ViewOf(this, "takeQuality") as XUiV_Label;
			takeDurabilityBg = ViewOf(this, "takeDurabilityBg") as XUiV_Sprite;
			takeDurabilityFill = ViewOf(this, "takeDurabilityFill") as XUiV_Sprite;
			takePopupStock = ViewOf(this, "takePopupStock") as XUiV_Label;
			contentView = ViewOf(this, "content");
			if (search != null)
			{
				search.OnChangeHandler += OnSearchChanged;
			}

			if (takeAmountInput != null)
			{
				takeAmountInput.OnChangeHandler += (_, __, ___) => PaintAmountThumb();
			}

			XUiController confirm = Find(this, "takeConfirm");
			if (confirm is XUiC_SimpleButton confirmButton)
			{
				confirmButton.OnPressed += (_, __) => ConfirmTake();
			}

			XUiController cancel = Find(this, "takeCancel");
			if (cancel is XUiC_SimpleButton cancelButton)
			{
				cancelButton.OnPressed += (_, __) => HideTakePopup();
			}

			XUiController blocker = Find(this, "takePopupBlocker");
			if (blocker?.Children != null)
			{
				foreach (XUiController child in blocker.Children)
				{
					string childId = child.ViewComponent != null ? child.ViewComponent.ID : null;
					if (childId != null && childId.StartsWith("takePopupDim", StringComparison.Ordinal))
					{
						child.OnPress += (_, __) => HideTakePopup();
					}
				}
			}

			BindCategory("catAll", string.Empty);
			BindCategory("catReading", "TCReading");
			BindCategory("catResources", "Resources");
			BindCategory("catAmmoWeapons", "Ammo/Weapons");
			BindCategory("catTools", "Tools/Traps");
			BindCategory("catScience", "TCScience");
			BindCategory("catMods", "Mods");
			BindCategory("catFood", "Food/Cooking");
			BindCategory("catDecor", "Decor/Miscellaneous");
			BindCategory("catMedical", "TCMedical");
			BindCategory("catArmor", "TCArmor");
			XUiController up = Find(this, "scrollUp");
			XUiController down = Find(this, "scrollDown");
			if (up != null)
			{
				up.OnPress += (_, __) => Scroll(-1);
			}

			if (down != null)
			{
				down.OnPress += (_, __) => Scroll(1);
			}

			foreach (StorageLaptopRow row in rowViews)
			{
				row.Window = this;
			}

			BindScroll(this);
			BindScroll(Find(this, "list"));
			BindScroll(Find(this, "scrollTrackBg"));
			BindScroll(Find(this, "scrollThumb"));
			HideTakePopup();
		}

		public override void Update(float dt)
		{
			base.Update(dt);
			FlushPendingConsume();
			if (useStack != null && useStack.IsSelected)
			{
				useStack.IsSelected = false;
			}

			if (inspectAfterUse && Time.time - lastUseChangeTime >= 0.5f && pendingConsume <= 0)
			{
				inspectAfterUse = false;
				if (pendingTake?.Source != null)
				{
					ShowInspect(pendingTake);
				}
			}

			if (useAfterRefresh && useWait > 0)
			{
				useWait--;
				if (TryRunUse() || useWait <= 0)
				{
					useAfterRefresh = false;
				}
			}

			if (iconRefresh > 0)
			{
				iconRefresh--;
				if (iconRefresh == 0 && windowOpen)
				{
					Rebuild();
					if (heldActive)
					{
						ReselectHeld();
					}
				}
			}

			if (!windowOpen)
			{
				return;
			}

			if (headerPending)
			{
				headerPending = false;
				SetWorkstationHeader();
			}

			HideLookPrompt();
			TickAmountHold();

			bool held = Input.GetMouseButton(0);
			if (Input.GetMouseButtonDown(0))
			{
				draggingScroll = PointerOver(scrollTrack) || PointerOver(scrollThumb);
				draggingAmount = PointerOver(takeTrack) || PointerOver(takeThumb);
			}

			if (!held)
			{
				draggingScroll = false;
				draggingAmount = false;
			}

			if (draggingScroll)
			{
				ApplyScrollFromPointer();
			}

			if (draggingAmount)
			{
				ApplyAmountFromPointer();
			}

			if (takePopup != null && takePopup.IsVisible)
			{
				return;
			}

			if (PointerOver(search != null ? search.ViewComponent : null))
			{
				return;
			}

			if (!PointerOverWindow())
			{
				return;
			}

			float wheel = Input.GetAxis("Mouse ScrollWheel");
			if (Mathf.Abs(wheel) > 0.01f)
			{
				NoteWheel(wheel > 0f ? -3 : 3);
			}
		}

		public override void OnOpen()
		{
			base.OnOpen();
			Instance = this;
			windowOpen = true;
			HideLookPrompt();
			draggingScroll = false;
			draggingAmount = false;
			searchText = string.Empty;
			offset = 0;
			source.Clear();
			if (search != null)
			{
				search.Text = string.Empty;
			}

			HideTakePopup();
			ReleaseStuckUse();
			PaintCategories();
			headerPending = true;
			EnsureInspectControls();
			if (applyOnOpen)
			{
				applyOnOpen = false;
				requestId = pendingRequest;
				ApplyResult(pendingRequest, pendingStatus, pendingRows);
				return;
			}

			Rebuild();
			RequestQuery();
		}

		public override void OnClose()
		{
			base.OnClose();
			windowOpen = false;
			draggingScroll = false;
			draggingAmount = false;
			HideTakePopup();
			NetPackageStorageLaptop.Submit(
				PackageEmit.Take<NetPackageStorageLaptop>().SetupRelease(StorageLaptopSession.Pos));
			if (Instance == this)
			{
				Instance = null;
			}

			pendingConsume = 0;
			useArmed = false;
			inspectAfterUse = false;
			holdStep = 0;
		}

		public static void RequestOpen()
		{
			pendingRequest = ++nextRequest;
			NetPackageStorageLaptop.Submit(
				PackageEmit.Take<NetPackageStorageLaptop>().SetupQuery(StorageLaptopSession.Pos, pendingRequest));
		}

		public static void Receive(int responseId, byte status, List<StorageRow> rows)
		{
			EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
			if (status == StorageService.StatusInUse)
			{
				if (player != null)
				{
					GameManager.ShowTooltip(player, Localization.Get("agfStorageLaptopInUse"));
				}

				LocalPlayerUI openUi = player != null ? LocalPlayerUI.GetUIForPlayer(player) : null;
				if (openUi != null && openUi.windowManager.IsWindowOpen("storageLaptop"))
				{
					openUi.windowManager.Close("storageLaptop");
				}

				return;
			}

			if (status == StorageService.StatusNoPower && (Instance == null || !Instance.windowOpen))
			{
				if (player != null)
				{
					GameManager.ShowTooltip(player, Localization.Get("agfStorageLaptopNeedPower"));
				}

				return;
			}

			LocalPlayerUI ui = player != null ? LocalPlayerUI.GetUIForPlayer(player) : null;
			if (ui != null && !ui.windowManager.IsWindowOpen("storageLaptop"))
			{
				applyOnOpen = true;
				pendingRequest = responseId;
				pendingStatus = status;
				pendingRows = rows;
				ui.windowManager.Open("storageLaptop", true);
				return;
			}

			Instance?.AcceptResult(responseId, status, rows);
		}

		public void AcceptResult(int responseId, byte status, List<StorageRow> rows)
		{
			requestId = responseId;
			ApplyResult(responseId, status, rows);
		}

		public void OnScrollDelta(float delta)
		{
			if (Mathf.Abs(delta) < 0.0001f)
			{
				return;
			}

			NoteWheel(delta > 0f ? -3 : 3);
		}

		public int ReadAmount()
		{
			if (takeAmountInput != null && int.TryParse(takeAmountInput.Text, out int parsed) && parsed > 0)
			{
				return parsed;
			}

			return 1;
		}

		public void OpenTakePopup(ViewRow row)
		{
			if (row?.Source == null)
			{
				return;
			}

			pendingTake = row;
			transferOpen = false;
			useAfterRefresh = false;
			EnsureInspectControls();
			ShowInspect(row);
			if (takeAmountInput != null)
			{
				takeAmountInput.Text = "1";
			}

			RefreshTransferCount();
			PaintAmountThumb();
			SetTransferVisible(false);
		}

		private void PaintTakeRow(ViewRow row)
		{
			if (takeIcon != null)
			{
				bool hasIcon = !string.IsNullOrEmpty(row.Icon);
				takeIcon.IsVisible = hasIcon;
				if (hasIcon)
				{
					if (takeIcon.UIAtlas != "ItemIconAtlas")
					{
						takeIcon.UIAtlas = "ItemIconAtlas";
					}

					takeIcon.SpriteName = row.Icon;
					takeIcon.Color = row.IconTint;
				}
			}

			if (takeTypeIcon != null)
			{
				string badge = row.TypeIcon ?? string.Empty;
				bool hasBadge = badge.Length > 0;
				takeTypeIcon.IsVisible = hasBadge;
				if (hasBadge)
				{
					takeTypeIcon.SpriteName = badge.StartsWith("ui_game_symbol_", StringComparison.Ordinal) ? badge : "ui_game_symbol_" + badge;
					takeTypeIcon.Color = Color.white;
				}
			}

			bool locked = row.Source.Locked;
			if (takePopupTitle != null)
			{
				takePopupTitle.Text = row.Name ?? string.Empty;
				takePopupTitle.Position = new Vector2i(takePopupTitle.Position.x, locked ? -1 : -8);
			}

			if (takeChest != null)
			{
				takeChest.IsVisible = locked;
				takeChest.Text = locked ? row.Chest ?? string.Empty : string.Empty;
			}

			if (takeLock != null)
			{
				takeLock.IsVisible = locked;
			}

			if (takeQuality != null)
			{
				takeQuality.Color = new Color(0.9f, 0.9f, 0.9f);
				takeQuality.Text = row.Source.Quality > 0 ? row.Source.Quality.ToString() : "-";
			}

			bool showDurability = row.Source.HasDurability;
			if (takeDurabilityBg != null)
			{
				takeDurabilityBg.IsVisible = showDurability;
				takeDurabilityBg.SetDirty();
			}

			if (takeDurabilityFill != null)
			{
				takeDurabilityFill.IsVisible = showDurability;
				if (showDurability)
				{
					takeDurabilityFill.Fill = Mathf.Clamp01(row.Source.DurabilityFill);
					takeDurabilityFill.Color = QualityInfo.GetQualityColor(row.Source.Quality);
				}

				takeDurabilityFill.SetDirty();
			}

			if (takePopupStock != null)
			{
				takePopupStock.Text = "(" + row.Source.Count + ")";
			}
		}

		public void HideTakePopup()
		{
			bool keepUse = useAfterRefresh;
			pendingTake = null;
			heldActive = false;
			transferOpen = false;
			useAfterRefresh = keepUse;
			draggingAmount = false;
			ShiftStats(false, lastHasStats);
			if (transferView != null)
			{
				transferView.IsVisible = false;
			}

			ShowEmptyInspect();
		}

		public void TakeOne()
		{
			if (pendingTake?.Source == null)
			{
				return;
			}

			useAfterRefresh = false;
			RememberSelection();
			Pull(pendingTake, 1);
		}

		public void TakeAll()
		{
			if (pendingTake?.Source == null)
			{
				return;
			}

			useAfterRefresh = false;
			RememberSelection();
			Pull(pendingTake, pendingTake.Source.Count);
		}

		public void TakeAllFromTransfer()
		{
			TakeAll();
			SetTransferVisible(false);
		}

		public void TakeAllButOne()
		{
			if (pendingTake?.Source == null || pendingTake.Source.Count < 2)
			{
				return;
			}

			useAfterRefresh = false;
			RememberSelection();
			Pull(pendingTake, pendingTake.Source.Count - 1);
			SetTransferVisible(false);
		}

		public void BeginTransfer()
		{
			if (pendingTake?.Source == null)
			{
				return;
			}

			EnsureInspectControls();
			if (!transferOpen)
			{
				if (takeAmountInput != null)
				{
					takeAmountInput.Text = "1";
				}

				RefreshTransferCount();
				PaintAmountThumb();
				SetTransferVisible(true);
				return;
			}

			SetTransferVisible(false);
		}

		public void AcceptTransfer()
		{
			if (pendingTake?.Source == null)
			{
				SetTransferVisible(false);
				return;
			}

			ConfirmTake();
			SetTransferVisible(false);
		}

		public void CancelTransfer()
		{
			SetTransferVisible(false);
		}

		public void UseSelected(ItemActionEntryUse.ConsumeType kind)
		{
			if (pendingTake?.Source == null)
			{
				return;
			}

			ItemValue lookup = ItemClass.GetItem(pendingTake.Source.ItemName, false);
			ItemClass itemClass = lookup.ItemClass;
			if (itemClass == null || !CanUseNow(itemClass, kind))
			{
				return;
			}

			useKind = kind;
			useItemType = lookup.type;
			useQuality = pendingTake.Source.Quality;
			RememberSelection();
			if (UsesHoldAnimation(itemClass))
			{
				BeginConsume(pendingTake, itemClass, lookup);
				return;
			}

			BeginInstant(pendingTake, lookup, kind);
		}

		private static bool UsesHoldAnimation(ItemClass itemClass)
		{
			if (itemClass?.Actions == null)
			{
				return false;
			}

			for (int i = 0; i < itemClass.Actions.Length; i++)
			{
				ItemAction action = itemClass.Actions[i];
				if (action is ItemActionEat && action.UseAnimation)
				{
					return true;
				}
			}

			return false;
		}

		private void BeginConsume(ViewRow row, ItemClass itemClass, ItemValue lookup)
		{
			EntityPlayerLocal player = xui?.playerUI?.entityPlayer;
			if (row?.Source == null || player == null || consumePump || player.inventory.IsHoldingItemActionRunning())
			{
				return;
			}

			int actionIndex = 0;
			if (itemClass.Actions != null)
			{
				for (int i = 0; i < itemClass.Actions.Length; i++)
				{
					if (itemClass.Actions[i] is ItemActionEat)
					{
						actionIndex = i;
						break;
					}
				}
			}

			ItemValue value = new ItemValue(lookup.type, true);
			value.Quality = (ushort)Mathf.Clamp(row.Source.Quality, 0, 6);
			string itemName = row.Source.ItemName;
			int quality = row.Source.Quality;
			bool locked = row.Source.Locked;
			Vector3i chest = row.Source.ChestPos;
			GameVersion.Initialize();
			if (!GameVersion.UseV33)
			{
				Pull(row, 1);
				return;
			}

			GameManager.Instance.StartCoroutine(ConsumeAnimatedV33(itemName, quality, locked, chest, actionIndex, value));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private IEnumerator ConsumeAnimatedV33(string itemName, int quality, bool locked, Vector3i chest, int actionIndex, ItemValue value)
		{
			EntityPlayerLocal player = xui?.playerUI?.entityPlayer;
			if (player == null)
			{
				yield break;
			}

			ItemStackGrid grid = ItemStackGrid.Create(new Vector2i(1, 1), XUiC_ItemStack.StackLocationTypes.Backpack, false, false, null);
			grid[0] = new ItemStack(value, 1);
			player.MoveController.AllowPlayerInput(false);
			consumePump = true;
			GameManager.Instance.StartCoroutine(PumpEatHold(value.type, actionIndex));
			try
			{
				yield return player.inventory.SimulateActionExecution(actionIndex, grid, 0);
			}
			finally
			{
				consumePump = false;
				if (player.MoveController != null)
				{
					player.MoveController.AllowPlayerInput(true);
				}
			}

			ItemStack left = grid[0];
			if (left != null && !left.IsEmpty() && StackAccess.Count(left) > 0)
			{
				yield break;
			}

			requestId++;
			NetPackageStorageLaptop.Submit(
				PackageEmit.Take<NetPackageStorageLaptop>().SetupConsume(
					StorageLaptopSession.Pos,
					requestId,
					itemName,
					quality,
					locked,
					chest,
					1));
		}

		private IEnumerator PumpEatHold(int itemType, int actionIndex)
		{
			float until = Time.time + 8f;
			while (consumePump && Time.time < until)
			{
				EntityPlayerLocal player = xui?.playerUI?.entityPlayer;
				ItemValue held = player?.inventory?.holdingItemItemValue;
				ItemInventoryData data = player?.inventory?.holdingItemData;
				ItemClass heldClass = held?.ItemClass;
				if (held != null && held.type == itemType && heldClass?.Actions != null && data?.actionData != null
					&& actionIndex >= 0 && actionIndex < heldClass.Actions.Length && actionIndex < data.actionData.Count)
				{
					heldClass.Actions[actionIndex]?.OnHoldingUpdate(data.actionData[actionIndex]);
				}

				yield return null;
			}

			if (!consumePump)
			{
				yield break;
			}

			consumePump = false;
			EntityPlayerLocal stuck = xui?.playerUI?.entityPlayer;
			if (stuck?.MoveController != null)
			{
				stuck.MoveController.AllowPlayerInput(true);
			}
		}

		private void BeginInstant(ViewRow row, ItemValue lookup, ItemActionEntryUse.ConsumeType kind)
		{
			XUiC_ItemStack stack = EnsureUseStack();
			XUiC_ItemInfoWindow info = Find(InspectRoot(), "itemInfoPanel") as XUiC_ItemInfoWindow;
			if (stack == null || row?.Source == null || info == null)
			{
				return;
			}

			ArmUseStack(row, lookup);
			ItemActionEntryUse entry = new ItemActionEntryUse(stack, kind);
			if (info.mainActionItemList != null)
			{
				entry.ParentActionList = info.mainActionItemList;
			}

			entry.OnActivated();
			iconRefresh = 20;
		}

		private XUiC_ItemStack EnsureUseStack()
		{
			if (useStackBound)
			{
				return useStack;
			}

			useStackBound = true;
			useStack = GetChildByType<XUiC_ItemStack>();
			if (useStack == null)
			{
				return null;
			}

			useStack.StackLocation = XUiC_ItemStack.StackLocationTypes.Creative;
			useStack.IsSelected = false;
			useStack.SlotChangedEvent += OnUseStackSlotChanged;
			return useStack;
		}

		private void ArmUseStack(ViewRow row, ItemValue lookup)
		{
			XUiC_ItemStack stack = EnsureUseStack();
			if (stack == null || row?.Source == null || lookup == null)
			{
				return;
			}

			if (useArmed
				&& consumeName == row.Source.ItemName
				&& consumeQuality == row.Source.Quality
				&& consumeLocked == row.Source.Locked
				&& consumeChest == row.Source.ChestPos)
			{
				return;
			}

			ItemValue value = new ItemValue(lookup.type, true);
			value.Quality = (ushort)Mathf.Clamp(row.Source.Quality, 0, 6);
			if (row.Source.HasDurability && value.MaxUseTimes > 0f)
			{
				value.UseTimes = (1f - Mathf.Clamp01(row.Source.DurabilityFill)) * value.MaxUseTimes;
			}

			consumeName = row.Source.ItemName;
			consumeQuality = row.Source.Quality;
			consumeLocked = row.Source.Locked;
			consumeChest = row.Source.ChestPos;
			suppressUseStack = true;
			stack.ItemStack = new ItemStack(value, Math.Max(1, row.Source.Count));
			useStackCount = stack.ItemStack == null || stack.ItemStack.IsEmpty() ? 0 : StackAccess.Count(stack.ItemStack);
			suppressUseStack = false;
			useArmed = true;
			stack.IsSelected = false;
		}

		private void OnUseStackSlotChanged(int slot, ItemStack stack)
		{
			if (useStack != null)
			{
				useStack.IsSelected = false;
			}

			if (suppressUseStack || !useArmed)
			{
				return;
			}

			lastUseChangeTime = Time.time;
			inspectAfterUse = true;

			int count = stack == null || stack.IsEmpty() ? 0 : StackAccess.Count(stack);
			if (count < useStackCount)
			{
				pendingConsume += useStackCount - count;
			}

			useStackCount = count;
		}

		private void FlushPendingConsume()
		{
			if (pendingConsume <= 0 || string.IsNullOrEmpty(consumeName))
			{
				return;
			}

			int amount = pendingConsume;
			pendingConsume = 0;
			requestId++;
			NetPackageStorageLaptop.Submit(
				PackageEmit.Take<NetPackageStorageLaptop>().SetupConsume(
					StorageLaptopSession.Pos,
					requestId,
					consumeName,
					consumeQuality,
					consumeLocked,
					consumeChest,
					amount));
		}

		public void ConfirmTake()
		{
			ViewRow row = pendingTake;
			if (row?.Source == null)
			{
				HideTakePopup();
				return;
			}

			int max = Math.Max(1, row.Source.Count);
			int amount = Mathf.Clamp(ReadAmount(), 1, max);
			useAfterRefresh = false;
			RememberSelection();
			Pull(row, amount);
		}

		public void Pull(ViewRow row, int amount)
		{
			if (row?.Source == null)
			{
				return;
			}

			requestId++;
			NetPackageStorageLaptop.Submit(
				PackageEmit.Take<NetPackageStorageLaptop>().SetupPull(
					StorageLaptopSession.Pos,
					requestId,
					row.Source.ItemName,
					row.Source.Quality,
					row.Source.Locked,
					row.Source.ChestPos,
					false,
					Math.Max(1, amount)));
		}

		public void ApplyResult(int responseId, byte status, List<StorageRow> rows)
		{
			if (responseId != requestId)
			{
				return;
			}

			source.Clear();
			if (rows != null)
			{
				source.AddRange(rows);
			}

			Rebuild();
			if (status == StorageService.StatusOk && useAfterRefresh)
			{
				useWait = 90;
			}
			else
			{
				useAfterRefresh = false;
				useWait = 0;
			}

			if (heldActive)
			{
				ReselectHeld();
			}
			if (xui?.playerUI?.entityPlayer == null)
			{
				return;
			}

			if (status == StorageService.StatusBagFull)
			{
				GameManager.ShowTooltip(xui.playerUI.entityPlayer, Localization.Get("agfStorageLaptopBagFull"));
			}
			else if (status == StorageService.StatusNoPower)
			{
				GameManager.ShowTooltip(xui.playerUI.entityPlayer, Localization.Get("agfStorageLaptopNeedPower"));
			}
		}

		private void RequestQuery()
		{
			requestId++;
			NetPackageStorageLaptop.Submit(
				PackageEmit.Take<NetPackageStorageLaptop>().SetupQuery(StorageLaptopSession.Pos, requestId));
		}

		private void OnSearchChanged(XUiController _sender, string _text, bool _changeFromCode)
		{
			searchText = _text ?? string.Empty;
			offset = 0;
			Rebuild();
		}

		private void BindCategory(string id, string group)
		{
			XUiController button = Find(this, id);
			if (button == null)
			{
				return;
			}

			categoryButtons.Add(button);
			categoryGroups.Add(group ?? string.Empty);
			string captured = group ?? string.Empty;
			button.OnPress += (_, __) =>
			{
				selectedCategory = captured;
				offset = 0;
				PaintCategories();
				Rebuild();
			};
		}

		private void PaintCategories()
		{
			for (int i = 0; i < categoryButtons.Count; i++)
			{
				XUiView view = categoryButtons[i].ViewComponent;
				if (!(view is XUiV_Sprite sprite))
				{
					continue;
				}

				bool selected = string.Equals(categoryGroups[i], selectedCategory, StringComparison.Ordinal);
				sprite.Color = selected ? new Color32(255, 214, 90, 255) : new Color32(220, 220, 220, 255);
			}
		}

		private void Scroll(int direction)
		{
			int max = Math.Max(0, filtered.Count - rowViews.Length);
			int next = Mathf.Clamp(offset + direction, 0, max);
			if (next == offset)
			{
				return;
			}

			offset = next;
			Paint();
		}

		private void Rebuild()
		{
			filtered.Clear();
			string query = searchText.Trim();
			foreach (StorageRow row in source)
			{
				ViewRow view = ViewRow.From(row, xui);
				if (!InCategory(view))
				{
					continue;
				}

				if (query.Length > 0 && view.SortName.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0)
				{
					continue;
				}

				filtered.Add(view);
			}

			filtered.Sort(Compare);
			int max = Math.Max(0, filtered.Count - rowViews.Length);
			if (offset > max)
			{
				offset = max;
			}

			Paint();
		}

		private bool InCategory(ViewRow view)
		{
			if (string.IsNullOrEmpty(selectedCategory))
			{
				return true;
			}

			if (view?.Groups == null)
			{
				return false;
			}

			for (int i = 0; i < view.Groups.Length; i++)
			{
				if (string.Equals(view.Groups[i], selectedCategory, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private int Compare(ViewRow a, ViewRow b)
		{
			int result = CategoryRank(a).CompareTo(CategoryRank(b));
			if (result == 0)
			{
				result = string.Compare(a.SortName, b.SortName, StringComparison.CurrentCultureIgnoreCase);
			}

			if (result == 0)
			{
				result = a.Source.Quality.CompareTo(b.Source.Quality);
			}

			if (result == 0)
			{
				result = string.Compare(a.Source.ItemName, b.Source.ItemName, StringComparison.Ordinal);
			}

			return result;
		}

		private static readonly string[] CategoryOrder =
		{
			"TCReading",
			"Resources",
			"Ammo/Weapons",
			"Tools/Traps",
			"TCScience",
			"Mods",
			"Food/Cooking",
			"Decor/Miscellaneous",
			"TCMedical",
			"TCArmor"
		};

		private static int CategoryRank(ViewRow view)
		{
			if (view?.Groups == null)
			{
				return CategoryOrder.Length;
			}

			int best = CategoryOrder.Length;
			for (int i = 0; i < view.Groups.Length; i++)
			{
				for (int c = 0; c < CategoryOrder.Length; c++)
				{
					if (string.Equals(view.Groups[i], CategoryOrder[c], StringComparison.OrdinalIgnoreCase) && c < best)
					{
						best = c;
					}
				}
			}

			return best;
		}

		internal static string VisibleName(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}

			StringBuilder visible = new StringBuilder(text.Length);
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] != '[')
				{
					visible.Append(text[i]);
					continue;
				}

				int end = text.IndexOf(']', i + 1);
				if (end < 0)
				{
					visible.Append(text[i]);
					continue;
				}

				string tag = text.Substring(i + 1, end - i - 1);
				if (tag == "-" || IsColorTag(tag))
				{
					i = end;
					continue;
				}

				visible.Append(text, i, end - i + 1);
				i = end;
			}

			return visible.ToString().Trim();
		}

		private static bool IsColorTag(string tag)
		{
			if (tag.Length != 6 && tag.Length != 8)
			{
				return false;
			}

			for (int i = 0; i < tag.Length; i++)
			{
				char c = tag[i];
				bool hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
				if (!hex)
				{
					return false;
				}
			}

			return true;
		}

		private void Paint()
		{
			for (int i = 0; i < rowViews.Length; i++)
			{
				int index = offset + i;
				rowViews[i].Bind(index < filtered.Count ? filtered[index] : null, i % 2 == 1);
			}

			if (emptyLabel != null)
			{
				bool empty = filtered.Count == 0;
				emptyLabel.IsVisible = empty;
				if (empty)
				{
					string key = source.Count > 0 ? "agfStorageLaptopEmptyCategory" : "agfStorageLaptopEmpty";
					emptyLabel.Text = Localization.Get(key);
				}
			}

			PaintScrollThumb();
			PaintAmountThumb();
		}

		private void NoteWheel(int direction)
		{
			if (wheelFrame == Time.frameCount)
			{
				return;
			}

			wheelFrame = Time.frameCount;
			Scroll(direction);
		}

		private void BindScroll(XUiController controller)
		{
			if (controller == null)
			{
				return;
			}

			controller.OnScroll += (_, delta) => OnScrollDelta(delta);
		}

		private void ApplyScrollFromPointer()
		{
			if (!TryPointerLocal(scrollTrack, out Vector3 local))
			{
				return;
			}

			int trackH = scrollTrack.Size.y;
			int thumbH = scrollThumb != null ? scrollThumb.Size.y : 24;
			int max = Math.Max(0, filtered.Count - rowViews.Length);
			float fromTop = Mathf.Clamp(-local.y, 0f, trackH);
			float travel = Mathf.Max(1f, trackH - thumbH);
			float thumbTop = Mathf.Clamp(fromTop - thumbH * 0.5f, 0f, travel);
			int next = max == 0 ? 0 : Mathf.RoundToInt(thumbTop / travel * max);
			SetOffset(next);
		}

		private void ApplyAmountFromPointer()
		{
			if (takeAmountInput == null || !TryPointerLocal(takeTrack, out Vector3 local))
			{
				return;
			}

			int max = AmountScaleMax();
			float width = Mathf.Max(1f, takeTrack.Size.x);
			float along = Mathf.Clamp01(local.x / width);
			int next = max <= 1 ? 1 : Mathf.Clamp(Mathf.RoundToInt(1f + along * (max - 1)), 1, max);
			if (ReadAmount() != next)
			{
				takeAmountInput.Text = next.ToString();
			}

			PaintAmountThumb();
		}

		private void SetOffset(int next)
		{
			int max = Math.Max(0, filtered.Count - rowViews.Length);
			next = Mathf.Clamp(next, 0, max);
			if (next == offset)
			{
				PaintScrollThumb();
				return;
			}

			offset = next;
			Paint();
		}

		private void PaintScrollThumb()
		{
			if (scrollTrack == null || scrollThumb == null)
			{
				return;
			}

			int visible = Math.Max(1, rowViews.Length);
			int total = Math.Max(visible, filtered.Count);
			int max = Math.Max(0, filtered.Count - rowViews.Length);
			int trackH = Math.Max(1, scrollTrack.Size.y);
			int thumbH = max == 0 ? trackH : Mathf.Clamp(trackH * visible / total, 28, trackH);
			float travel = Mathf.Max(0f, trackH - thumbH);
			float top = max == 0 ? 0f : (float)offset / max * travel;
			scrollThumb.Size = new Vector2i(scrollThumb.Size.x, thumbH);
			scrollThumb.Position = new Vector2i(scrollThumb.Position.x, -Mathf.RoundToInt(top));
			scrollThumb.IsVisible = filtered.Count > 0;
			scrollThumb.SetDirty();
		}

		private void HideLookPrompt()
		{
			LocalPlayerUI ui = xui?.playerUI;
			if (ui == null)
			{
				return;
			}

			XUiC_InteractionPrompt.SetText(ui, null);
			XUiC_FocusedBlockHealth.SetData(ui, null, 0f);
		}

		private void PaintAmountThumb()
		{
			if (takeTrack == null || takeThumb == null)
			{
				return;
			}

			int max = AmountScaleMax();
			int value = Mathf.Clamp(ReadAmount(), 1, max);
			float span = Mathf.Max(1f, takeTrack.Size.x - takeThumb.Size.x);
			float along = max <= 1 ? 0f : (value - 1f) / (max - 1f);
			takeThumb.Position = new Vector2i(Mathf.RoundToInt(along * span), takeThumb.Position.y);
			takeThumb.SetDirty();
		}

		private void BindAmountStep(XUiController root, string id, int mode)
		{
			if (!(Find(root, id) is XUiC_SimpleButton button))
			{
				return;
			}

			if (mode == -1)
			{
				holdDownButton = button;
			}
			else if (mode == 1)
			{
				holdUpButton = button;
			}

			if (mode == -1 || mode == 1)
			{
				XUiController clickable = button.GetChildById("clickable");
				if (clickable != null)
				{
					clickable.OnMouseUpDown += (_, pressed) =>
					{
						if (pressed)
						{
							BeginAmountHold(mode);
							return;
						}

						if (holdStep == mode)
						{
							holdStep = 0;
						}
					};
				}

				return;
			}

			button.OnPressed += (_, __) => NudgeAmount(mode);
		}

		private void BeginAmountHold(int mode)
		{
			NudgeAmount(mode);
			holdStep = mode;
			holdRepeatAt = Time.unscaledTime + 0.25f;
		}

		private void TickAmountHold()
		{
			if (holdStep == 0)
			{
				TryStartControllerHold();
			}

			if (holdStep == 0)
			{
				return;
			}

			if (!transferOpen || !AmountButtonHeld())
			{
				holdStep = 0;
				return;
			}

			if (Time.unscaledTime < holdRepeatAt)
			{
				return;
			}

			NudgeAmount(holdStep);
			holdRepeatAt = Time.unscaledTime + 0.1f;
		}

		private void TryStartControllerHold()
		{
			if (!transferOpen || Input.GetMouseButton(0) || !ApplyPressed())
			{
				return;
			}

			if (StepButtonActive(holdDownButton))
			{
				BeginAmountHold(-1);
			}
			else if (StepButtonActive(holdUpButton))
			{
				BeginAmountHold(1);
			}
		}

		private static bool StepButtonActive(XUiC_SimpleButton button)
		{
			return button != null && (button.ButtonHovered || button.ButtonSelected);
		}

		private bool AmountButtonHeld()
		{
			if (Input.GetMouseButton(0))
			{
				return true;
			}

			return ApplyPressed();
		}

		private bool ApplyPressed()
		{
			PlayerActionsGUI actions = xui?.playerUI?.playerInput?.GUIActions;
			return actions != null && actions.Apply.IsPressed;
		}

		private XUiController InspectRoot()
		{
			return xui != null ? xui.FindWindowGroupByName("backpack") : null;
		}

		private void EnsureInspectControls()
		{
			XUiController root = InspectRoot() ?? this;
			if (takeAmountInput == null)
			{
				takeAmountInput = Find(root, "takeAmountInput") as XUiC_TextInput;
				if (takeAmountInput != null)
				{
					takeAmountInput.OnChangeHandler += (_, __, ___) => PaintAmountThumb();
				}
			}

			if (takeTrack == null)
			{
				takeTrack = ViewOf(root, "takeTrack");
			}

			if (takeThumb == null)
			{
				takeThumb = ViewOf(root, "takeThumb");
			}

			if (transferView == null)
			{
				transferView = ViewOf(root, "laptopTransfer");
			}

			if (transferCount == null)
			{
				transferCount = ViewOf(root, "laptopTransferCount") as XUiV_Label;
			}

			if (!inspectBound && Find(root, "takeMin") != null)
			{
				inspectBound = true;
				BindAmountStep(root, "takeMin", -2);
				BindAmountStep(root, "takeDown", -1);
				BindAmountStep(root, "takeUp", 1);
				BindAmountStep(root, "takeMax", 2);
				if (Find(root, "transferTake") is XUiC_SimpleButton takeButton)
				{
					takeButton.OnPressed += (_, __) => AcceptTransfer();
				}

				if (Find(root, "transferCancel") is XUiC_SimpleButton cancelButton)
				{
					cancelButton.OnPressed += (_, __) => CancelTransfer();
				}

				if (Find(root, "transferTakeAll") is XUiC_SimpleButton takeAllButton)
				{
					takeAllButton.OnPressed += (_, __) => TakeAllFromTransfer();
				}

				if (Find(root, "transferTakeAllButOne") is XUiC_SimpleButton takeAllButOneButton)
				{
					takeAllButOneButton.OnPressed += (_, __) => TakeAllButOne();
				}
			}
		}

		private void SetWorkstationHeader()
		{
			XUiController group = xui != null ? xui.FindWindowGroupByName("storageLaptop") : null;
			if (Find(group, "windowNonPagingHeader") is XUiC_WindowNonPagingHeader header)
			{
				header.SetHeader(Localization.Get("cntStorageLaptop"));
			}
		}

		private void ShowInspect(ViewRow row)
		{
			XUiC_ItemInfoWindow info = Find(InspectRoot(), "itemInfoPanel") as XUiC_ItemInfoWindow;
			if (info == null || row?.Source == null)
			{
				return;
			}

			ItemValue lookup = ItemClass.GetItem(row.Source.ItemName, false);
			ItemValue value = new ItemValue(lookup.type, true);
			value.Quality = (ushort)Mathf.Clamp(row.Source.Quality, 0, 6);
			if (row.Source.HasDurability && value.MaxUseTimes > 0f)
			{
				value.UseTimes = (1f - Mathf.Clamp01(row.Source.DurabilityFill)) * value.MaxUseTimes;
			}

			ItemStack stack = new ItemStack(value, Math.Max(1, row.Source.Count));
			RememberSelection(row);
			lastHasStats = XUiM_ItemStack.HasItemStats(stack);
			info.SetInfo(stack, info, XUiC_ItemActionList.ItemActionListTypes.None);
			ArmUseStack(row, lookup);
			if (info.mainActionItemList != null)
			{
				info.mainActionItemList.AddActionListEntry(new StorageLaptopTakeAction(info));
				foreach (UseChoice choice in UsesFor(value.ItemClass))
				{
					if (choice.Kind == ItemActionEntryUse.ConsumeType.Open && EnsureUseStack() != null)
					{
						info.mainActionItemList.AddActionListEntry(new ItemActionEntryUse(useStack, choice.Kind));
						continue;
					}

					info.mainActionItemList.AddActionListEntry(new StorageLaptopUseAction(info, choice.Kind, choice.Label, choice.Icon));
				}

				if (row.Source.Count > 1)
				{
					info.mainActionItemList.AddActionListEntry(new StorageLaptopTransferAction(info));
				}
				info.mainActionItemList.RefreshActionList();
			}

			info.makeVisible(true);
			info.ViewComponent.IsVisible = true;
			XUiController empty = Find(InspectRoot(), "emptyInfoPanel");
			if (empty != null)
			{
				empty.ViewComponent.IsVisible = false;
			}

		}

		private void ShowEmptyInspect()
		{
			XUiController empty = Find(InspectRoot(), "emptyInfoPanel");
			XUiController info = Find(InspectRoot(), "itemInfoPanel");
			if (empty != null)
			{
				empty.ViewComponent.IsVisible = true;
			}

			if (info != null)
			{
				info.ViewComponent.IsVisible = false;
			}
		}

		private void ShiftStats(bool on, bool statsMode = true)
		{
			EnsureInspectLayout();
			RestoreInspectBlock(statsBlock, statsBlockBorder, statsHomePos, statsHomeSize, statsBorderHomeHeight);
			RestoreInspectBlock(onlyBlock, onlyBlockBorder, onlyHomePos, onlyHomeSize, onlyBorderHomeHeight);
			RestoreOverflowStats();
			ClampDescriptionText(false);
			if (!on)
			{
				return;
			}

			bool useStats = (statsMode && statsBlock != null) || onlyBlock == null;
			if (useStats && statsBlock != null)
			{
				ShrinkInspectBlock(statsBlock, statsBlockBorder, statsHomeSize, statsBorderHomeHeight);
				HideOverflowStats(statsHomeSize.y - 36);
			}
			else if (onlyBlock != null)
			{
				ShrinkInspectBlock(onlyBlock, onlyBlockBorder, onlyHomeSize, onlyBorderHomeHeight);
			}

			ClampDescriptionText(true);
		}

		private void EnsureInspectLayout()
		{
			if (inspectLayoutReady)
			{
				return;
			}

			XUiController content = Find(InspectRoot(), "contentInfo");
			if (content?.Children == null)
			{
				return;
			}

			foreach (XUiController child in content.Children)
			{
				XUiView view = child.ViewComponent;
				if (view == null || view.ID != "description")
				{
					continue;
				}

				XUiView border = null;
				if (child.Children != null)
				{
					foreach (XUiController nested in child.Children)
					{
						if (nested.ViewComponent != null && nested.ViewComponent.ID == "backgroundMain")
						{
							border = nested.ViewComponent;
							break;
						}
					}
				}

				if (view.Position.y >= 0)
				{
					onlyBlock = view;
					onlyBlockBorder = border;
					onlyHomePos = view.Position;
					onlyHomeSize = view.Size;
					onlyBorderHomeHeight = border != null ? border.Size.y : 0;
				}
				else
				{
					statsBlock = view;
					statsBlockBorder = border;
					statsHomePos = view.Position;
					statsHomeSize = view.Size;
					statsBorderHomeHeight = border != null ? border.Size.y : 0;
				}
			}

			inspectLayoutReady = statsBlock != null || onlyBlock != null;
		}

		private void SetTransferVisible(bool on)
		{
			transferOpen = on;
			EnsureInspectControls();
			if (transferView != null)
			{
				transferView.IsVisible = on;
			}
		}

		private void RefreshTransferCount()
		{
			if (transferCount != null && pendingTake?.Source != null)
			{
				transferCount.Text = "(" + pendingTake.Source.Count + ")";
			}
		}

		private void RememberSelection()
		{
			RememberSelection(pendingTake);
		}

		private void RememberSelection(ViewRow row)
		{
			if (row?.Source == null)
			{
				heldActive = false;
				return;
			}

			heldActive = true;
			heldName = row.Source.ItemName;
			heldQuality = row.Source.Quality;
			heldLocked = row.Source.Locked;
			heldChest = row.Source.ChestPos;
		}

		private bool SameHeld(ViewRow view, string name, int quality, bool locked, Vector3i chest)
		{
			return view?.Source != null
				&& string.Equals(view.Source.ItemName, name, StringComparison.Ordinal)
				&& view.Source.Quality == quality
				&& view.Source.Locked == locked
				&& view.Source.ChestPos.x == chest.x
				&& view.Source.ChestPos.y == chest.y
				&& view.Source.ChestPos.z == chest.z;
		}

		private void ReselectHeld()
		{
			string name = heldName;
			int quality = heldQuality;
			bool locked = heldLocked;
			Vector3i chest = heldChest;
			bool haveTarget = heldActive;
			if (pendingTake?.Source != null)
			{
				name = pendingTake.Source.ItemName;
				quality = pendingTake.Source.Quality;
				locked = pendingTake.Source.Locked;
				chest = pendingTake.Source.ChestPos;
				haveTarget = true;
			}

			if (!haveTarget)
			{
				return;
			}

			ViewRow match = null;
			foreach (ViewRow view in filtered)
			{
				if (!SameHeld(view, name, quality, locked, chest))
				{
					continue;
				}

				match = view;
				break;
			}

			if (match == null)
			{
				HideTakePopup();
				return;
			}

			bool keepTransfer = transferOpen && match.Source.Count > 1;
			bool opening = Time.time - lastUseChangeTime < 0.5f;
			pendingTake = match;
			RememberSelection(match);
			if (opening)
			{
				inspectAfterUse = true;
			}
			else
			{
				ShowInspect(match);
			}
			RefreshTransferCount();
			if (takeAmountInput != null && match.Source.Count > 0 && ReadAmount() > match.Source.Count)
			{
				takeAmountInput.Text = match.Source.Count.ToString();
			}

			PaintAmountThumb();
			SetTransferVisible(keepTransfer);
		}

		private static void RestoreInspectBlock(XUiView view, XUiView border, Vector2i homePos, Vector2i homeSize, int borderHomeHeight)
		{
			if (view == null)
			{
				return;
			}

			view.Position = homePos;
			view.Size = homeSize;
			view.SetDirty();
			if (border != null)
			{
				border.Size = new Vector2i(border.Size.x, borderHomeHeight);
				border.SetDirty();
			}
		}

		private static void ShrinkInspectBlock(XUiView view, XUiView border, Vector2i homeSize, int borderHomeHeight)
		{
			view.Size = new Vector2i(homeSize.x, Math.Max(40, homeSize.y - 36));
			view.SetDirty();
			if (border != null)
			{
				border.Size = new Vector2i(border.Size.x, Math.Max(40, borderHomeHeight - 36));
				border.SetDirty();
			}
		}

		private readonly List<XUiView> hiddenStatRows = new List<XUiView>();
		private readonly List<XUiV_Label> descriptionLabels = new List<XUiV_Label>();
		private readonly List<int> descriptionLabelHeights = new List<int>();
		private readonly List<UILabel.Overflow> descriptionLabelOverflow = new List<UILabel.Overflow>();
		private bool descriptionLabelsReady;

		private void HideOverflowStats(int visibleHeight)
		{
			RestoreOverflowStats();
			if (statsBlock == null)
			{
				return;
			}

			XUiController content = Find(InspectRoot(), "contentInfo");
			if (content?.Children == null)
			{
				return;
			}

			foreach (XUiController child in content.Children)
			{
				if (child.ViewComponent == statsBlock)
				{
					HideStatRows(child, visibleHeight);
				}
			}
		}

		private void HideStatRows(XUiController node, int visibleHeight)
		{
			if (node?.Children == null)
			{
				return;
			}

			foreach (XUiController child in node.Children)
			{
				XUiView view = child.ViewComponent;
				if (view != null && view.Position.y <= -visibleHeight && view.Size.y > 20 && view.Size.y < 50)
				{
					view.IsVisible = false;
					hiddenStatRows.Add(view);
				}

				HideStatRows(child, visibleHeight);
			}
		}

		private void RestoreOverflowStats()
		{
			for (int i = 0; i < hiddenStatRows.Count; i++)
			{
				if (hiddenStatRows[i] != null)
				{
					hiddenStatRows[i].IsVisible = true;
				}
			}

			hiddenStatRows.Clear();
		}

		private void ClampDescriptionText(bool clamp)
		{
			EnsureDescriptionLabels();
			for (int i = 0; i < descriptionLabels.Count; i++)
			{
				XUiV_Label label = descriptionLabels[i];
				if (label == null)
				{
					continue;
				}

				if (!clamp)
				{
					label.Size = new Vector2i(label.Size.x, descriptionLabelHeights[i]);
					label.Overflow = descriptionLabelOverflow[i];
					label.SetDirty();
					continue;
				}

				int homeHeight = descriptionLabelHeights[i];
				int height = homeHeight > 36 ? homeHeight - 36 : 220;
				label.Overflow = UILabel.Overflow.ClampContent;
				label.Size = new Vector2i(label.Size.x, height);
				label.SetDirty();
			}
		}

		private void EnsureDescriptionLabels()
		{
			if (descriptionLabelsReady)
			{
				return;
			}

			XUiController content = Find(InspectRoot(), "contentInfo");
			if (content == null)
			{
				return;
			}

			CollectDescriptionLabels(content);
			descriptionLabelsReady = descriptionLabels.Count > 0;
		}

		private void CollectDescriptionLabels(XUiController node)
		{
			if (node?.Children == null)
			{
				return;
			}

			foreach (XUiController child in node.Children)
			{
				if (child.ViewComponent is XUiV_Label label && label.ID == "descriptionText")
				{
					descriptionLabels.Add(label);
					descriptionLabelHeights.Add(label.Size.y);
					descriptionLabelOverflow.Add(label.Overflow);
				}

				CollectDescriptionLabels(child);
			}
		}

		private struct UseChoice
		{
			public ItemActionEntryUse.ConsumeType Kind;
			public string Label;
			public string Icon;
		}

		private static List<UseChoice> UsesFor(ItemClass itemClass)
		{
			List<UseChoice> choices = new List<UseChoice>();
			if (itemClass?.Actions == null)
			{
				return choices;
			}

			bool eat = false;
			bool read = false;
			bool quest = false;
			bool open = false;
			for (int i = 0; i < itemClass.Actions.Length; i++)
			{
				ItemAction action = itemClass.Actions[i];
				if (action is ItemActionEat)
				{
					eat = true;
				}
				else if (action is ItemActionLearnRecipe || action is ItemActionGainSkill)
				{
					read = true;
				}
				else if (action is ItemActionQuest)
				{
					quest = true;
				}
				else if (action is ItemActionOpenBundle || action is ItemActionOpenLootBundle)
				{
					open = true;
				}
			}

			if (eat)
			{
				ItemActionEntryUse.ConsumeType kind = ItemActionEntryUse.ConsumeType.Heal;
				string label = Localization.Get("lblContextActionHeal");
				string icon = "ui_game_symbol_medical";
				if (IsReadingItem(itemClass))
				{
					label = Localization.Get("lblContextActionRead");
					icon = "ui_game_symbol_book";
				}

				choices.Add(new UseChoice { Kind = kind, Label = label, Icon = icon });
			}

			if (read)
			{
				choices.Add(new UseChoice { Kind = ItemActionEntryUse.ConsumeType.Read, Label = Localization.Get("lblContextActionRead"), Icon = "ui_game_symbol_book" });
			}

			if (quest)
			{
				choices.Add(new UseChoice { Kind = ItemActionEntryUse.ConsumeType.Quest, Label = Localization.Get("lblContextActionRead"), Icon = "ui_game_symbol_quest" });
			}

			if (open)
			{
				choices.Add(new UseChoice { Kind = ItemActionEntryUse.ConsumeType.Open, Label = Localization.Get("lblContextActionOpen"), Icon = "ui_game_symbol_treasure" });
			}

			return choices;
		}

		private static bool IsReadingItem(ItemClass itemClass)
		{
			if (itemClass == null)
			{
				return false;
			}

			if (HasTag(itemClass, "books") || HasTag(itemClass, "schematic"))
			{
				return true;
			}

			if (!string.IsNullOrEmpty(itemClass.AltItemTypeIcon) && itemClass.AltItemTypeIcon.IndexOf("book", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}

			if (itemClass.Groups == null)
			{
				return false;
			}

			for (int i = 0; i < itemClass.Groups.Length; i++)
			{
				string group = itemClass.Groups[i] ?? string.Empty;
				if (group.Equals("Books", StringComparison.OrdinalIgnoreCase)
					|| group.Equals("BooksOnly", StringComparison.OrdinalIgnoreCase)
					|| group.Equals("SchematicsOnly", StringComparison.OrdinalIgnoreCase)
					|| group.Equals("TCReading", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private static bool HasTag(ItemClass itemClass, string tag)
		{
			return itemClass != null && itemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse(tag));
		}

		private bool CanUseNow(ItemClass itemClass, ItemActionEntryUse.ConsumeType kind)
		{
			EntityPlayerLocal player = xui?.playerUI?.entityPlayer;
			if (player == null)
			{
				return false;
			}

			if (kind == ItemActionEntryUse.ConsumeType.Read)
			{
				return CanRead(itemClass, player);
			}

			return true;
		}

		private bool CanRead(ItemClass itemClass, EntityPlayerLocal player)
		{
			if (itemClass?.Actions == null)
			{
				return false;
			}

			bool learn = false;
			bool unread = false;
			bool skill = false;
			bool skillRoom = false;
			for (int i = 0; i < itemClass.Actions.Length; i++)
			{
				if (itemClass.Actions[i] is ItemActionLearnRecipe recipe && recipe.RecipesToLearn != null)
				{
					learn = true;
					for (int r = 0; r < recipe.RecipesToLearn.Length; r++)
					{
						if (!XUiM_Recipes.GetRecipeIsUnlocked(xui, recipe.RecipesToLearn[r]))
						{
							unread = true;
						}
					}
				}

				if (itemClass.Actions[i] is ItemActionGainSkill gain && gain.SkillsToGain != null)
				{
					skill = true;
					for (int s = 0; s < gain.SkillsToGain.Length; s++)
					{
						ProgressionValue value = player.Progression.GetProgressionValue(gain.SkillsToGain[s]);
						if (value != null && value.Level + 1 <= value.ProgressionClass.MaxLevel)
						{
							skillRoom = true;
						}
					}
				}
			}

			if (skill && !skillRoom && !unread)
			{
				GameManager.ShowTooltip(player, Localization.Get("ttSkillMaxLevel"));
				return false;
			}

			if (learn && !unread && !skillRoom)
			{
				GameManager.ShowTooltip(player, Localization.Get("alreadyKnown"));
				return false;
			}

			return learn || skill;
		}

		private void ReleaseStuckUse()
		{
			EntityPlayerLocal player = xui?.playerUI?.entityPlayer;
			if (player == null || xui == null || player.inventory.IsHoldingItemActionRunning())
			{
				return;
			}

			xui.IsUsingItemActionEntryUse = false;
			XUiC_ItemStack[] stacks = InspectRoot()?.GetChildrenByType<XUiC_ItemStack>();
			if (stacks == null)
			{
				return;
			}

			for (int i = 0; i < stacks.Length; i++)
			{
				if (stacks[i] != null)
				{
					stacks[i].HiddenLock = false;
				}
			}
		}

		private bool TryRunUse()
		{
			EntityPlayerLocal player = xui?.playerUI?.entityPlayer;
			if (player?.bag == null)
			{
				return false;
			}

			if (player.inventory.IsHoldingItemActionRunning())
			{
				return false;
			}

			ReleaseStuckUse();
			XUiC_ItemStack found = FindBackpackStack(useItemType, useQuality);
			if (found == null)
			{
				return false;
			}

			found.HiddenLock = false;
			ItemActionEntryUse entry = new ItemActionEntryUse(found, useKind);
			if (Find(InspectRoot(), "itemInfoPanel") is XUiC_ItemInfoWindow info && info.mainActionItemList != null)
			{
				entry.ParentActionList = info.mainActionItemList;
			}

			if (entry.Enabled)
			{
				entry.OnActivated();
			}
			else
			{
				entry.OnDisabledActivate();
			}

			Rebuild();
			if (heldActive)
			{
				ReselectHeld();
			}

			iconRefresh = 20;
			return true;
		}

		private XUiC_ItemStack FindBackpackStack(int type, int quality)
		{
			XUiC_ItemStack[] stacks = InspectRoot()?.GetChildrenByType<XUiC_ItemStack>();
			if (stacks == null)
			{
				return null;
			}

			for (int i = 0; i < stacks.Length; i++)
			{
				XUiC_ItemStack stack = stacks[i];
				if (stack == null || stack.StackLocation != XUiC_ItemStack.StackLocationTypes.Backpack)
				{
					continue;
				}

				ItemStack item = stack.ItemStack;
				ItemValue value = item != null ? StackAccess.Value(item) : null;
				if (value == null || item.IsEmpty() || value.type != type || value.Quality != quality)
				{
					continue;
				}

				return stack;
			}

			return null;
		}

		private static int UseActionIndex(ItemClass itemClass, ItemActionEntryUse.ConsumeType kind)
		{
			if (itemClass?.Actions == null)
			{
				return -1;
			}

			for (int i = 0; i < itemClass.Actions.Length; i++)
			{
				ItemAction action = itemClass.Actions[i];
				if (action == null)
				{
					continue;
				}

				switch (kind)
				{
				case ItemActionEntryUse.ConsumeType.Eat:
				case ItemActionEntryUse.ConsumeType.Drink:
				case ItemActionEntryUse.ConsumeType.Heal:
					return i;
				case ItemActionEntryUse.ConsumeType.Read:
					if (action is ItemActionLearnRecipe)
					{
						return i;
					}

					break;
				case ItemActionEntryUse.ConsumeType.Quest:
					if (action is ItemActionQuest)
					{
						return i;
					}

					break;
				case ItemActionEntryUse.ConsumeType.Open:
					if (action is ItemActionOpenBundle || action is ItemActionOpenLootBundle)
					{
						return i;
					}

					break;
				}
			}

			return -1;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void RunUseV33(EntityPlayerLocal player, int actionIndex, int slot)
		{
			GameManager.Instance.StartCoroutine(player.inventory.SimulateActionExecution(actionIndex, player.bag.ItemGrid, slot));
		}

		private static void RunUseV32(EntityPlayerLocal player, XUi xui, int actionIndex, ItemStack bagStack)
		{
			ItemStack one = new ItemStack(StackAccess.Value(bagStack).Clone(), 1);
			int left = StackAccess.Count(bagStack) - 1;
			StackAccess.SetCount(bagStack, Math.Max(0, left));

			MethodInfo method = null;
			MethodInfo[] methods = typeof(Inventory).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < methods.Length; i++)
			{
				ParameterInfo[] parameters = methods[i].GetParameters();
				if (methods[i].Name == "SimulateActionExecution" && parameters.Length == 3 && parameters[1].ParameterType == typeof(ItemStack))
				{
					method = methods[i];
					break;
				}
			}

			if (method == null)
			{
				return;
			}

			Action<ItemStack> done = finalStack =>
			{
				if (finalStack != null && !finalStack.IsEmpty())
				{
					xui.PlayerInventory.AddItem(finalStack);
				}
			};
			object routine = method.Invoke(player.inventory, new object[] { actionIndex, one, done });
			if (routine is IEnumerator enumerator)
			{
				GameManager.Instance.StartCoroutine(enumerator);
			}
		}

		private static int GainSkillIndex(ItemClass itemClass)
		{
			if (itemClass?.Actions == null)
			{
				return -1;
			}

			for (int i = 0; i < itemClass.Actions.Length; i++)
			{
				if (itemClass.Actions[i] is ItemActionGainSkill)
				{
					return i;
				}
			}

			return -1;
		}

		private void NudgeAmount(int mode)
		{
			if (takeAmountInput == null)
			{
				return;
			}

			int max = AmountScaleMax();
			int current = Mathf.Clamp(ReadAmount(), 1, max);
			int next = current;
			if (mode <= -2)
			{
				next = 1;
			}
			else if (mode >= 2)
			{
				next = max;
			}
			else
			{
				next = Mathf.Clamp(current + mode, 1, max);
			}

			takeAmountInput.Text = next.ToString();
			PaintAmountThumb();
		}

		private int AmountScaleMax()
		{
			if (pendingTake?.Source != null && pendingTake.Source.Count > 1)
			{
				return pendingTake.Source.Count;
			}

			return 1;
		}

		private bool PointerOverWindow()
		{
			if (PointerOver(contentView) || PointerOver(ViewComponent) || PointerOver(scrollTrack) || PointerOver(takeTrack))
			{
				return true;
			}

			if (rowViews == null)
			{
				return false;
			}

			foreach (StorageLaptopRow row in rowViews)
			{
				if (row?.ViewComponent != null && PointerOver(row.ViewComponent))
				{
					return true;
				}
			}

			return false;
		}

		private bool PointerOver(XUiView view)
		{
			if (!TryPointerLocal(view, out Vector3 local))
			{
				return false;
			}

			return local.x >= 0f && local.x <= view.Size.x && local.y <= 0f && local.y >= -view.Size.y;
		}

		private bool TryPointerLocal(XUiView view, out Vector3 local)
		{
			local = Vector3.zero;
			if (view?.uiTransform == null || xui?.playerUI?.camera == null)
			{
				return false;
			}

			Camera camera = xui.playerUI.camera;
			Vector3 mouse = Input.mousePosition;
			mouse.z = view.uiTransform.position.z - camera.transform.position.z;
			local = view.uiTransform.InverseTransformPoint(camera.ScreenToWorldPoint(mouse));
			return true;
		}

		private static XUiView ViewOf(XUiController root, string id)
		{
			XUiController found = Find(root, id);
			return found != null ? found.ViewComponent : null;
		}

		private static XUiController Find(XUiController root, string id)
		{
			if (root == null)
			{
				return null;
			}

			if (root.ViewComponent != null && root.ViewComponent.ID == id)
			{
				return root;
			}

			if (root.Children == null)
			{
				return null;
			}

			foreach (XUiController child in root.Children)
			{
				XUiController found = Find(child, id);
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}
	}

	public sealed class ViewRow
	{
		public StorageRow Source;
		public string Name;
		public string SortName;
		public string[] Groups;
		public string Category;
		public string Icon;
		public Color IconTint;
		public string TypeIcon;
		public Color TypeIconColor;
		public string Chest;

		private static void StoredTypeIcon(ItemClass itemClass, XUi xui, StorageRow row, out string icon, out Color color)
		{
			icon = itemClass.ItemTypeIcon ?? string.Empty;
			color = Color.white;
			if (xui == null || row == null)
			{
				return;
			}

			ItemValue iconValue = ItemClass.GetItem(row.ItemName, false);
			iconValue.Quality = (ushort)Mathf.Clamp(row.Quality, 0, 6);
			ItemStack iconStack = new ItemStack(iconValue, Math.Max(1, row.Count));
			string vanillaIcon = XUiBindingHelper.GetItemTypeIconValue(xui, iconStack);
			if (string.IsNullOrEmpty(vanillaIcon))
			{
				return;
			}

			icon = vanillaIcon;
			color = XUiBindingHelper.GetItemTypeIconTintColor(xui, iconStack);
		}

		public static ViewRow From(StorageRow row, XUi xui)
		{
			ItemClass itemClass = ItemClass.GetItemClass(row.ItemName, false);
			string localized = itemClass != null ? itemClass.GetLocalizedItemName() : row.ItemName;

			string category = string.Empty;
			if (itemClass?.Groups != null && itemClass.Groups.Length > 0 && !string.IsNullOrEmpty(itemClass.Groups[0]))
			{
				category = Localization.Get(itemClass.Groups[0]);
			}

			string chest = string.Empty;
			if (row.Locked)
			{
				string blockName = Localization.Get(row.ChestBlock);
				chest = string.Format(Localization.Get("agfStorageLaptopChest"), blockName, row.Distance);
			}

			Color tint = Color.white;
			string typeIcon = string.Empty;
			Color typeIconColor = Color.white;
			if (itemClass != null)
			{
				tint = itemClass.GetIconTint(null);
				StoredTypeIcon(itemClass, xui, row, out typeIcon, out typeIconColor);
			}

			return new ViewRow
			{
				Source = row,
				Name = localized ?? string.Empty,
				SortName = StorageLaptopWindow.VisibleName(localized),
				Groups = itemClass?.Groups,
				Category = category ?? string.Empty,
				Icon = itemClass != null ? itemClass.GetIconName() : string.Empty,
				IconTint = tint,
				TypeIcon = typeIcon,
				TypeIconColor = typeIconColor,
				Chest = chest
			};
		}
	}

	[Preserve]
	public class StorageLaptopRow : XUiController
	{
		public StorageLaptopWindow Window;
		private XUiV_Sprite icon;
		private XUiV_Sprite typeIcon;
		private XUiV_Sprite background;
		private XUiV_Sprite lockIcon;
		private XUiV_Label nameLabel;
		private XUiV_Label qualityLabel;
		private XUiV_Sprite durabilityBg;
		private XUiV_Sprite durabilityFill;
		private XUiV_Label countLabel;
		private XUiV_Label chestLabel;
		private ViewRow bound;
		private bool hovered;
		private bool alternateRow;

		public override void Init()
		{
			base.Init();
			icon = ViewOf("icon") as XUiV_Sprite;
			typeIcon = ViewOf("typeIcon") as XUiV_Sprite;
			background = ViewOf("background") as XUiV_Sprite;
			lockIcon = ViewOf("lockIcon") as XUiV_Sprite;
			nameLabel = ViewOf("itemName") as XUiV_Label;
			qualityLabel = ViewOf("itemQuality") as XUiV_Label;
			durabilityBg = ViewOf("durabilityBg") as XUiV_Sprite;
			durabilityFill = ViewOf("durabilityFill") as XUiV_Sprite;
			countLabel = ViewOf("itemCount") as XUiV_Label;
			chestLabel = ViewOf("chestName") as XUiV_Label;

			OnScroll += (_, delta) => Window?.OnScrollDelta(delta);
			XUiController backgroundController = FindChild("background");
			if (backgroundController != null)
			{
				backgroundController.OnScroll += (_, delta) => Window?.OnScrollDelta(delta);
				backgroundController.OnPress += (_, __) =>
				{
					if (Window != null && bound != null)
					{
						Window.OpenTakePopup(bound);
					}
				};
				backgroundController.OnHover += (_, over) =>
				{
					hovered = over;
					PaintBackground();
				};
			}
		}

		public void Bind(ViewRow row, bool alternate)
		{
			bound = row;
			alternateRow = alternate;
			bool has = row != null;
			ViewComponent.IsVisible = has;
			if (!has)
			{
				hovered = false;
				return;
			}

			PaintBackground();

			if (icon != null)
			{
				bool hasIcon = !string.IsNullOrEmpty(row.Icon);
				icon.IsVisible = hasIcon;
				if (hasIcon)
				{
					if (icon.UIAtlas != "ItemIconAtlas")
					{
						icon.UIAtlas = "ItemIconAtlas";
					}

					icon.SpriteName = row.Icon;
					icon.Color = row.IconTint;
				}
			}

			if (typeIcon != null)
			{
				string badge = row.TypeIcon ?? string.Empty;
				bool hasBadge = badge.Length > 0;
				typeIcon.IsVisible = hasBadge;
				if (hasBadge)
				{
					typeIcon.SpriteName = badge.StartsWith("ui_game_symbol_", StringComparison.Ordinal) ? badge : "ui_game_symbol_" + badge;
					typeIcon.Color = row.TypeIconColor;
				}
			}

			if (nameLabel != null)
			{
				nameLabel.Text = row.Name;
				if (row.Source.Locked)
				{
					nameLabel.Position = new Vector2i(nameLabel.Position.x, -12);
					nameLabel.Size = new Vector2i(270, 22);
				}
				else
				{
					nameLabel.Position = new Vector2i(nameLabel.Position.x, -24);
					nameLabel.Size = new Vector2i(270, 40);
				}
			}

			if (qualityLabel != null)
			{
				qualityLabel.Color = new Color(0.9f, 0.9f, 0.9f);
				qualityLabel.Text = row.Source.Quality > 0 ? row.Source.Quality.ToString() : "-";
			}

			bool showDurability = row.Source.HasDurability;
			if (durabilityBg != null)
			{
				durabilityBg.IsVisible = showDurability;
				durabilityBg.SetDirty();
			}

			if (durabilityFill != null)
			{
				durabilityFill.IsVisible = showDurability;
				if (showDurability)
				{
					durabilityFill.Fill = Mathf.Clamp01(row.Source.DurabilityFill);
					durabilityFill.Color = QualityInfo.GetQualityColor(row.Source.Quality);
				}

				durabilityFill.SetDirty();
			}

			if (countLabel != null)
			{
				countLabel.Text = row.Source.Count.ToString();
			}

			if (lockIcon != null)
			{
				lockIcon.IsVisible = row.Source.Locked;
			}

			if (chestLabel != null)
			{
				chestLabel.Text = row.Chest ?? string.Empty;
				chestLabel.IsVisible = row.Source.Locked;
				chestLabel.Position = new Vector2i(chestLabel.Position.x, -28);
			}
		}

		private void PaintBackground()
		{
			if (background == null)
			{
				return;
			}

			if (hovered && bound != null)
			{
				background.Color = new Color32(72, 72, 72, 255);
				return;
			}

				background.Color = alternateRow ? new Color32(54, 54, 54, 255) : new Color32(40, 40, 40, 255);
		}

		private XUiView ViewOf(string id)
		{
			XUiController child = FindChild(id);
			return child != null ? child.ViewComponent : null;
		}

		private XUiController FindChild(string id)
		{
			if (Children == null)
			{
				return null;
			}

			foreach (XUiController child in Children)
			{
				if (child.ViewComponent != null && child.ViewComponent.ID == id)
				{
					return child;
				}

				if (child.Children == null)
				{
					continue;
				}

				foreach (XUiController nested in child.Children)
				{
					if (nested.ViewComponent != null && nested.ViewComponent.ID == id)
					{
						return nested;
					}
				}
			}

			return null;
		}
	}

	public sealed class StorageLaptopTakeAction : BaseItemActionEntry
	{
		public StorageLaptopTakeAction(XUiController controller)
			: base(controller, Localization.Get("agfStorageLaptopTake"), "ui_game_symbol_hand")
		{
		}

		public override void OnActivated()
		{
			StorageLaptopWindow.Instance?.TakeOne();
		}
	}

	public sealed class StorageLaptopTransferAction : BaseItemActionEntry
	{
		public StorageLaptopTransferAction(XUiController controller)
			: base(controller, Localization.Get("agfStorageLaptopTransfer"), "ui_game_symbol_loot_sack")
		{
		}

		public override void OnActivated()
		{
			StorageLaptopWindow.Instance?.BeginTransfer();
		}
	}

	public sealed class StorageLaptopUseAction : BaseItemActionEntry
	{
		private readonly ItemActionEntryUse.ConsumeType consumeType;

		public StorageLaptopUseAction(XUiController controller, ItemActionEntryUse.ConsumeType consumeType, string label, string icon)
			: base(controller, label, icon)
		{
			this.consumeType = consumeType;
		}

		public override void OnActivated()
		{
			StorageLaptopWindow.Instance?.UseSelected(consumeType);
		}
	}
}

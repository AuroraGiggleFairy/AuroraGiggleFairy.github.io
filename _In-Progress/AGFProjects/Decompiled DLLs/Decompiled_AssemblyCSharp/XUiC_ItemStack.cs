using Audio;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_ItemStack : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum flashLockTypes
	{
		None,
		Allowed,
		AlreadyEquipped
	}

	public enum LockTypes
	{
		None,
		Shell,
		Crafting,
		Repairing,
		Scrapping,
		Burning
	}

	public enum StackLockTypes
	{
		None,
		Assemble,
		Quest,
		Tool,
		Hidden
	}

	public enum StackLocationTypes
	{
		Backpack,
		ToolBelt,
		LootContainer,
		Equipment,
		Creative,
		Vehicle,
		Workstation,
		Merge,
		DewCollector,
		Cosmetics,
		Part
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack itemStack = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip pickupSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastClicked;

	[PublicizedFrom(EAccessModifier.Private)]
	public LockTypes lockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lockSprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float lockTime;

	public int TimeInterval = 5;

	public int OverrideStackCount = -1;

	public bool _isQuickSwap;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 selectionBorderColor;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int PickupSnapDistance = 4;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 finalPressedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 attributeLockColor = new Color32(48, 48, 48, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 modAllowedColor = Color.green;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 modAlreadyEquippedColor = Color.yellow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite lockTypeIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite itemIconSprite;

	public XUiV_Label timer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Texture backgroundTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public flashLockTypes flashLockTypeIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public StackLockTypes stackLockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool attributeLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragAndDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLockedSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentInterval = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TweenScale tweenScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly CachedStringFormatterXuiRgbaColor itemiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt itemcountFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat durabilityFillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor backgroundcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor selectionbordercolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public bool isQuickSwap
	{
		get
		{
			return _isQuickSwap;
		}
		set
		{
			if (value != _isQuickSwap)
			{
				_isQuickSwap = value;
				IsDirty = true;
			}
		}
	}

	public Color32 SelectionBorderColor
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return selectionBorderColor;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (!selectionBorderColor.ColorEquals(value))
			{
				selectionBorderColor = value;
				IsDirty = true;
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int SlotNumber { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public StackLocationTypes StackLocation { get; set; }

	public ItemClass itemClass
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return itemStack?.itemValue?.ItemClass;
		}
	}

	public ItemClass itemClassOrMissing
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return itemStack?.itemValue?.ItemClassOrMissing;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float HoverIconGrow
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool AssembleLock
	{
		get
		{
			return stackLockType == StackLockTypes.Assemble;
		}
		set
		{
			stackLockType = (value ? StackLockTypes.Assemble : StackLockTypes.None);
			RefreshBindings();
		}
	}

	public bool QuestLock
	{
		get
		{
			return stackLockType == StackLockTypes.Quest;
		}
		set
		{
			stackLockType = (value ? StackLockTypes.Quest : StackLockTypes.None);
			RefreshBindings();
		}
	}

	public bool ToolLock
	{
		get
		{
			return stackLockType == StackLockTypes.Tool;
		}
		set
		{
			stackLockType = (value ? StackLockTypes.Tool : StackLockTypes.None);
			this.ToolLockChangedEvent?.Invoke(SlotNumber, itemStack, value);
			RefreshBindings();
		}
	}

	public bool HiddenLock
	{
		get
		{
			return stackLockType == StackLockTypes.Hidden;
		}
		set
		{
			stackLockType = (value ? StackLockTypes.Hidden : StackLockTypes.None);
			RefreshBindings();
		}
	}

	public bool AttributeLock
	{
		get
		{
			return attributeLock;
		}
		set
		{
			attributeLock = value;
			RefreshBindings();
		}
	}

	public bool StackLock => stackLockType != StackLockTypes.None;

	public bool IsDragAndDrop
	{
		get
		{
			return isDragAndDrop;
		}
		set
		{
			isDragAndDrop = value;
			if (value)
			{
				base.ViewComponent.EventOnPress = false;
				base.ViewComponent.EventOnHover = false;
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsHolding { get; set; }

	public bool IsLocked
	{
		get
		{
			return isLocked;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != isLocked)
			{
				isLocked = value;
				IsDirty = true;
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int RepairAmount
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool AllowIconGrow
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return itemClass != null;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool SimpleClick
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AllowDropping
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	} = true;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool PrefixId
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ShowFavorites
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float LockTime
	{
		get
		{
			return lockTime;
		}
		set
		{
			lockTime = value;
			if (value == 0f)
			{
				timer.Text = "";
				timer.IsVisible = false;
			}
			else
			{
				timer.Text = $"{Mathf.Floor(lockTime / 60f):00}:{Mathf.Floor(lockTime % 60f):00}";
				timer.IsVisible = true;
			}
		}
	}

	public bool UserLockedSlot
	{
		get
		{
			return userLockedSlot;
		}
		set
		{
			if (value != userLockedSlot)
			{
				userLockedSlot = value;
				RefreshBindings();
			}
		}
	}

	public ItemStack ItemStack
	{
		get
		{
			return itemStack;
		}
		set
		{
			if (!itemStack.Equals(value))
			{
				setItemStack(value);
				if (itemStack.IsEmpty())
				{
					itemStack.Clear();
				}
				if (base.Selected)
				{
					updateItemInfoWindow(this);
				}
				HandleSlotChangeEvent();
				ItemClass itemClass = itemStack.itemValue.ItemClass;
				if (itemClass != null && (StackLocation == StackLocationTypes.Backpack || StackLocation == StackLocationTypes.ToolBelt))
				{
					QuestLock = itemClass.IsQuestItem;
				}
				if (value.IsEmpty())
				{
					stackLockType = StackLockTypes.None;
				}
				updateBackgroundTexture();
				RefreshBindings();
			}
			else
			{
				if (ItemStack.IsEmpty() && backgroundTexture != null)
				{
					backgroundTexture.Texture = null;
				}
				if (base.Selected)
				{
					updateItemInfoWindow(this);
				}
				base.xui.playerUI.CursorController.HoverTarget = null;
			}
			viewComponent.IsSnappable = !itemStack.IsEmpty();
			IsDirty = true;
		}
	}

	public bool IsFavorite
	{
		get
		{
			if (!itemStack.IsEmpty())
			{
				return base.xui.playerUI.entityPlayer.favoriteCreativeStacks.Contains((ushort)itemStack.itemValue.type);
			}
			return false;
		}
	}

	public virtual string ItemIcon
	{
		get
		{
			if (itemStack.IsEmpty())
			{
				return "";
			}
			ItemClass itemClass = itemClassOrMissing;
			Block block = (itemClass as ItemClassBlock)?.GetBlock();
			if (block == null)
			{
				return itemStack.itemValue.GetPropertyOverride(ItemClass.PropCustomIcon, itemClass.GetIconName());
			}
			if (!block.SelectAlternates)
			{
				return itemStack.itemValue.GetPropertyOverride(ItemClass.PropCustomIcon, itemClass.GetIconName());
			}
			return block.GetAltBlockValue(itemStack.itemValue.Meta).Block.GetIconName();
		}
	}

	public virtual string ItemIconColor
	{
		get
		{
			ItemClass itemClass = itemClassOrMissing;
			if (itemClass == null)
			{
				return "255,255,255,0";
			}
			Color32 v = itemClass.GetIconTint(itemStack.itemValue);
			return itemiconcolorFormatter.Format(v);
		}
	}

	public bool GreyedOut
	{
		get
		{
			return itemIconSprite.UIAtlas == "ItemIconAtlasGreyscale";
		}
		set
		{
			if (itemIconSprite != null)
			{
				itemIconSprite.UIAtlas = (value ? "ItemIconAtlasGreyscale" : "ItemIconAtlas");
			}
		}
	}

	public string ItemNameText
	{
		get
		{
			if (itemStack.IsEmpty())
			{
				return "";
			}
			ItemClass itemClass = itemClassOrMissing;
			string text = itemClass.GetLocalizedItemName();
			if (itemClass.IsBlock())
			{
				text = Block.list[itemStack.itemValue.type].GetLocalizedBlockName(itemStack.itemValue);
			}
			if (!PrefixId)
			{
				return text;
			}
			int itemOrBlockId = itemStack.itemValue.GetItemOrBlockId();
			return $"{text}\n({itemOrBlockId}) {itemClass.Name}";
		}
	}

	public bool ShowDurability
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!IsLocked || lockType == LockTypes.None)
			{
				return itemClass?.ShowQualityBar ?? false;
			}
			return false;
		}
	}

	public event XUiEvent_SlotChangedEventHandler SlotChangedEvent;

	public event XUiEvent_ToolLockChangeEventHandler ToolLockChangedEvent;

	public event XUiEvent_LockChangeEventHandler LockChangedEvent;

	public event XUiEvent_TimeIntervalElapsedEventHandler TimeIntervalElapsedEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setItemStack(ItemStack _stack)
	{
		itemStack = _stack.Clone();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateItemInfoWindow(XUiC_ItemStack _itemStack)
	{
		InfoWindow.SetItemStack(_itemStack, _makeVisible: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBackgroundTexture()
	{
		if (backgroundTexture == null)
		{
			return;
		}
		if (itemClass is ItemClassBlock itemClassBlock)
		{
			Block block = itemClassBlock.GetBlock();
			if (block.GetAutoShapeType() == EAutoShapeType.None)
			{
				backgroundTexture.Texture = null;
				return;
			}
			int uiBackgroundTextureId = block.GetUiBackgroundTextureId(itemStack.itemValue.ToBlockValue(), BlockFace.Top);
			setTextureForOpaqueMeshTextureId(uiBackgroundTextureId);
			return;
		}
		ItemClass obj = itemClass;
		if (((obj != null) ? obj.Actions[0] : null) is ItemActionTextureBlock itemActionTextureBlock)
		{
			if (itemStack.itemValue.Meta == 0)
			{
				itemStack.itemValue.Meta = itemActionTextureBlock.DefaultTextureID;
			}
			int textureID = BlockTextureData.list[itemStack.itemValue.Meta].TextureID;
			setTextureForOpaqueMeshTextureId(textureID);
		}
		else
		{
			backgroundTexture.Texture = null;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void setTextureForOpaqueMeshTextureId(int _textureId)
		{
			if (_textureId == 0)
			{
				backgroundTexture.Texture = null;
			}
			else
			{
				MeshDescription meshDescription = MeshDescription.meshes[0];
				UVRectTiling uVRectTiling = meshDescription.textureAtlas.uvMapping[_textureId];
				backgroundTexture.Texture = meshDescription.textureAtlas.diffuseTexture;
				if (meshDescription.bTextureArray)
				{
					backgroundTexture.Material.SetTexture("_BumpMap", meshDescription.textureAtlas.normalTexture);
					backgroundTexture.Material.SetFloat("_Index", uVRectTiling.index);
					backgroundTexture.Material.SetFloat("_Size", uVRectTiling.blockW);
				}
				else
				{
					backgroundTexture.UVRect = uVRectTiling.uv;
				}
				backgroundTexture.SetTextureDirty();
			}
		}
	}

	public void ResetTweenScale()
	{
		if (tweenScale != null && tweenScale.value != Vector3.one)
		{
			tweenScale.from = Vector3.one * 1.5f;
			tweenScale.to = Vector3.one;
			tweenScale.enabled = true;
			tweenScale.duration = 0.1f;
		}
	}

	public void ForceSetItemStack(ItemStack _stack)
	{
		bool flag = base.Selected;
		itemStack = ItemStack.Empty.Clone();
		this.SlotChangedEvent?.Invoke(SlotNumber, itemStack);
		if (!_stack.IsEmpty())
		{
			base.Selected = flag;
		}
		ItemStack = _stack;
		this.SlotChangedEvent?.Invoke(SlotNumber, itemStack);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleSlotChangeEvent()
	{
		if (itemStack.IsEmpty() && base.Selected)
		{
			base.Selected = false;
		}
		this.SlotChangedEvent?.Invoke(SlotNumber, itemStack);
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("timer");
		if (childById != null)
		{
			timer = childById.ViewComponent as XUiV_Label;
		}
		XUiController childById2 = GetChildById("itemIcon");
		if (childById2 != null)
		{
			itemIconSprite = childById2.ViewComponent as XUiV_Sprite;
		}
		XUiController childById3 = GetChildById("lockTypeIcon");
		if (childById3 != null)
		{
			lockTypeIcon = childById3.ViewComponent as XUiV_Sprite;
		}
		XUiController childById4 = GetChildById("backgroundTexture");
		if (childById4 != null)
		{
			backgroundTexture = childById4.ViewComponent as XUiV_Texture;
			if (backgroundTexture != null)
			{
				backgroundTexture.CreateMaterial();
			}
		}
		XUiController childById5 = GetChildById("rectSlotLock");
		if (childById5 != null)
		{
			childById5.OnHover += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, bool _over) =>
			{
			};
			childById5.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				UserLockedSlot = !UserLockedSlot;
				RefreshBindings();
			};
		}
		tweenScale = itemIconSprite.UiTransform.gameObject.AddComponent<TweenScale>();
		base.ViewComponent.UseSelectionBox = false;
	}

	public void UpdateTimer(float _dt)
	{
		if (!IsLocked || lockType == LockTypes.Shell || lockType == LockTypes.Burning)
		{
			return;
		}
		float num = lockTime;
		if (lockTime > 0f)
		{
			lockTime -= _dt;
			if (currentInterval == -1)
			{
				currentInterval = (int)lockTime / TimeInterval;
			}
			if (this.TimeIntervalElapsedEvent != null && TimeInterval != 0)
			{
				int num2 = (int)lockTime / TimeInterval;
				if (num2 != currentInterval)
				{
					this.TimeIntervalElapsedEvent(lockTime, this);
					currentInterval = num2;
				}
			}
		}
		if (lockTime <= 0f && num != 0f)
		{
			this.TimeIntervalElapsedEvent?.Invoke(lockTime, this);
			if (this.LockChangedEvent != null)
			{
				this.LockChangedEvent(lockType, this);
			}
			else
			{
				IsLocked = false;
			}
		}
		if (lockTime <= 0f)
		{
			timer.IsVisible = false;
			timer.Text = "";
		}
		else
		{
			timer.IsVisible = true;
			timer.Text = $"{Mathf.Floor(lockTime / 60f):00}:{Mathf.Floor(lockTime % 60f):00}";
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.WindowGroup.isShowing)
		{
			PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
			CursorControllerAbs cursorController = base.xui.playerUI.CursorController;
			Vector3 vector = cursorController.GetScreenPosition();
			bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
			bool mouseButtonDown = cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
			bool mouseButton = cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
			bool mouseButtonUp2 = cursorController.GetMouseButtonUp(UICamera.MouseButton.RightButton);
			bool mouseButtonDown2 = cursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
			bool mouseButton2 = cursorController.GetMouseButton(UICamera.MouseButton.RightButton);
			if (!IsLocked && !isDragAndDrop)
			{
				if (isOver && UICamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
				{
					if (gUIActions.LastInputType == BindingSourceType.DeviceBindingSource)
					{
						bool wasReleased = gUIActions.Submit.WasReleased;
						bool wasReleased2 = gUIActions.HalfStack.WasReleased;
						bool wasPressed = gUIActions.Inspect.WasPressed;
						bool wasReleased3 = gUIActions.RightStick.WasReleased;
						if (SimpleClick && !StackLock)
						{
							if (wasReleased)
							{
								HandleMoveToPreferredLocation();
							}
							else if (wasPressed)
							{
								HandleItemInspect();
							}
						}
						else if (base.xui.dragAndDrop.CurrentStack.IsEmpty() && !ItemStack.IsEmpty())
						{
							if (!StackLock)
							{
								if (wasReleased)
								{
									SwapItem();
								}
								else if (wasReleased2)
								{
									HandlePartialStackPickup();
								}
								else if (wasReleased3)
								{
									HandleMoveToPreferredLocation();
								}
								else if (wasPressed)
								{
									HandleItemInspect();
								}
							}
						}
						else if (!StackLock)
						{
							if (wasReleased)
							{
								HandleStackSwap();
							}
							else if (wasReleased2 && AllowDropping)
							{
								HandleDropOne();
							}
						}
					}
					else if (SimpleClick && !StackLock)
					{
						if (mouseButtonUp)
						{
							HandleMoveToPreferredLocation();
						}
					}
					else if (InputUtils.ShiftKeyPressed)
					{
						if (!StackLock && mouseButtonUp)
						{
							HandleMoveToPreferredLocation();
						}
					}
					else if (mouseButton || mouseButton2)
					{
						if (base.xui.dragAndDrop.CurrentStack.IsEmpty() && !ItemStack.IsEmpty())
						{
							if (!lastClicked)
							{
								startMousePos = vector;
							}
							else if (Mathf.Abs((vector - startMousePos).magnitude) > (float)PickupSnapDistance && !StackLock)
							{
								if (mouseButton)
								{
									SwapItem();
								}
								else
								{
									HandlePartialStackPickup();
								}
							}
						}
						if (mouseButtonDown || mouseButtonDown2)
						{
							lastClicked = true;
						}
					}
					else if (mouseButtonUp)
					{
						if (base.xui.dragAndDrop.CurrentStack.IsEmpty())
						{
							HandleItemInspect();
						}
						else if (lastClicked && !StackLock)
						{
							HandleStackSwap();
						}
					}
					else if (mouseButtonUp2)
					{
						if (lastClicked && !StackLock && AllowDropping)
						{
							HandleDropOne();
						}
					}
					else
					{
						lastClicked = false;
					}
				}
				else
				{
					lastClicked = false;
					if ((isOver || itemIconSprite.UiTransform.localScale != Vector3.one) && tweenScale.value != Vector3.one && !itemStack.IsEmpty())
					{
						tweenScale.from = Vector3.one * 1.5f;
						tweenScale.to = Vector3.one;
						tweenScale.enabled = true;
						tweenScale.duration = 0.5f;
					}
				}
			}
			else if (IsLocked && ((gUIActions.LastInputType == BindingSourceType.DeviceBindingSource && gUIActions.Submit.WasReleased) || (gUIActions.LastInputType != BindingSourceType.DeviceBindingSource && gUIActions.LeftClick.WasPressed)) && isOver)
			{
				this.LockChangedEvent?.Invoke(LockTypes.None, this);
			}
		}
		updateBorderColor();
		if (flashLockTypeIcon != flashLockTypes.None)
		{
			Color b = ((flashLockTypeIcon == flashLockTypes.Allowed) ? modAllowedColor : modAlreadyEquippedColor);
			float num = Mathf.PingPong(Time.time, 0.5f);
			setLockTypeIconColor(Color.Lerp(Color.grey, b, num * 4f));
		}
		if (IsDirty)
		{
			IsDirty = false;
			updateLockTypeIcon();
			RefreshBindings();
			ResetTweenScale();
		}
		if (IsLocked && lockType != LockTypes.None)
		{
			UpdateTimer(_dt);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBorderColor()
	{
		if (IsDragAndDrop)
		{
			SelectionBorderColor = Color.clear;
		}
		else if (base.Selected)
		{
			SelectionBorderColor = selectColor;
		}
		else if (isOver)
		{
			SelectionBorderColor = highlightColor;
		}
		else if (IsHolding)
		{
			SelectionBorderColor = holdingColor;
		}
		else
		{
			SelectionBorderColor = backgroundColor;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanSwap(ItemStack _stack)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleItemInspect()
	{
		if (!ItemStack.IsEmpty() && InfoWindow != null)
		{
			base.Selected = true;
			InfoWindow.SetMaxCountOnDirty = true;
			updateItemInfoWindow(this);
		}
		HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleStackSwap()
	{
		base.xui.currentPopupMenu.ClearItems();
		if (!AllowDropping)
		{
			base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
			base.xui.dragAndDrop.PickUpType = StackLocation;
		}
		bool flag = false;
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		ItemClass itemClass = currentStack.itemValue.ItemClassOrMissing;
		int num = 0;
		if (itemClass != null)
		{
			num = ((OverrideStackCount == -1) ? itemClass.Stacknumber.Value : Mathf.Min(itemClass.Stacknumber.Value, OverrideStackCount));
			if (!currentStack.IsEmpty() && this.itemStack.IsEmpty() && num < currentStack.count)
			{
				flag = true;
			}
		}
		if (!flag && (this.itemStack.IsEmpty() || currentStack.IsEmpty()))
		{
			SwapItem();
			base.Selected = false;
		}
		else if (!flag && (!this.itemStack.itemValue.ItemClassOrMissing.CanStack() || !itemClass.CanStack()))
		{
			SwapItem();
			base.Selected = false;
		}
		else if (currentStack.itemValue.type == this.itemStack.itemValue.type && !currentStack.itemValue.HasQuality && !this.itemStack.itemValue.HasQuality)
		{
			if (currentStack.count + this.itemStack.count > num)
			{
				int count = currentStack.count + this.itemStack.count - num;
				ItemStack itemStack = this.itemStack.Clone();
				itemStack.count = num;
				currentStack.count = count;
				base.xui.dragAndDrop.CurrentStack = currentStack;
				base.xui.dragAndDrop.PickUpType = StackLocation;
				ItemStack = itemStack;
			}
			else
			{
				ItemStack itemStack2 = this.itemStack.Clone();
				itemStack2.count += currentStack.count;
				ItemStack = itemStack2;
				base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
				PlayPlaceSound();
			}
			if (base.Selected)
			{
				updateItemInfoWindow(this);
			}
		}
		else if (flag)
		{
			int count2 = currentStack.count - num;
			ItemStack itemStack3 = currentStack.Clone();
			itemStack3.count = num;
			currentStack.count = count2;
			base.xui.dragAndDrop.CurrentStack = currentStack;
			base.xui.dragAndDrop.PickUpType = StackLocation;
			ItemStack = itemStack3;
		}
		else
		{
			SwapItem();
			base.Selected = false;
		}
		HandleClickComplete();
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandlePartialStackPickup()
	{
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (currentStack.IsEmpty() && !this.itemStack.IsEmpty())
		{
			int num = this.itemStack.count / 2;
			if (num > 0)
			{
				currentStack = this.itemStack.Clone();
				currentStack.count = num;
				if (AllowDropping)
				{
					ItemStack itemStack = this.itemStack.Clone();
					itemStack.count -= num;
					ItemStack = itemStack;
				}
				base.xui.dragAndDrop.CurrentStack = currentStack;
				base.xui.dragAndDrop.PickUpType = StackLocation;
			}
		}
		if (base.Selected)
		{
			base.Selected = false;
		}
		HandleClickComplete();
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleDropOne()
	{
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (!currentStack.IsEmpty())
		{
			int num = 1;
			if (this.itemStack.IsEmpty())
			{
				ItemStack itemStack = currentStack.Clone();
				itemStack.count = num;
				currentStack.count -= num;
				base.xui.dragAndDrop.CurrentStack = currentStack;
				ItemStack = itemStack;
				PlayPlaceSound();
			}
			else if (currentStack.itemValue.type == this.itemStack.itemValue.type)
			{
				ItemClass itemClass = currentStack.itemValue.ItemClassOrMissing;
				int num2 = ((OverrideStackCount == -1) ? itemClass.Stacknumber.Value : Mathf.Min(itemClass.Stacknumber.Value, OverrideStackCount));
				if (this.itemStack.count + 1 <= num2)
				{
					ItemStack itemStack2 = this.itemStack.Clone();
					itemStack2.count++;
					currentStack.count--;
					ItemStack = itemStack2.Clone();
					base.xui.dragAndDrop.CurrentStack = currentStack;
					this.SlotChangedEvent?.Invoke(SlotNumber, this.itemStack);
					IsDirty = true;
				}
				PlayPlaceSound();
			}
			if (currentStack.count == 0)
			{
				base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
			}
		}
		base.Selected = false;
		HandleClickComplete();
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleClickComplete()
	{
		lastClicked = false;
		if (itemIconSprite.UiTransform.localScale.x > 1f && !itemStack.IsEmpty())
		{
			tweenScale.from = Vector3.one * 1.5f;
			tweenScale.to = Vector3.one;
			tweenScale.enabled = true;
			tweenScale.duration = 0.5f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		isOver = _isOver;
		if (!IsLocked)
		{
			if (_isOver)
			{
				if (InfoWindow != null && InfoWindow.ViewComponent.IsVisible)
				{
					InfoWindow.HoverEntry = this;
				}
				if (AllowIconGrow)
				{
					tweenScale.from = Vector3.one;
					tweenScale.to = Vector3.one * 1.5f;
					tweenScale.enabled = true;
					tweenScale.duration = 0.5f;
				}
			}
			else
			{
				if (InfoWindow != null && InfoWindow.ViewComponent.IsVisible)
				{
					InfoWindow.HoverEntry = null;
				}
				if (AllowIconGrow)
				{
					tweenScale.from = Vector3.one * 1.5f;
					tweenScale.to = Vector3.one;
					tweenScale.enabled = true;
					tweenScale.duration = 0.5f;
				}
			}
		}
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver, _canSwap: true, !StackLock);
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SwapItem()
	{
		base.xui.currentPopupMenu.ClearItems();
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (!currentStack.IsEmpty() && !CanSwap(currentStack))
		{
			return;
		}
		if (StackLocation == StackLocationTypes.LootContainer && base.xui.dragAndDrop.PickUpType != StackLocationTypes.LootContainer && !currentStack.IsEmpty() && !currentStack.itemValue.ItemClassOrMissing.CanPlaceInContainer())
		{
			Manager.PlayInsidePlayerHead("ui_denied");
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "Quest Items cannot be placed in containers.");
			return;
		}
		if (itemStack.IsEmpty())
		{
			PlayPlaceSound(currentStack);
		}
		base.xui.dragAndDrop.CurrentStack = itemStack.Clone();
		base.xui.dragAndDrop.PickUpType = StackLocation;
		if (StackLocation == StackLocationTypes.ToolBelt)
		{
			base.xui.dragAndDrop.CurrentStack.Deactivate();
		}
		ForceSetItemStack(currentStack.Clone());
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
	}

	public void HandleMoveToPreferredLocation()
	{
		base.xui.currentPopupMenu.ClearItems();
		if (ItemStack.IsEmpty() || StackLock)
		{
			return;
		}
		if (StackLocation == StackLocationTypes.ToolBelt)
		{
			ItemStack.Deactivate();
		}
		int count = ItemStack.count;
		switch (StackLocation)
		{
		case StackLocationTypes.Creative:
		{
			ItemStack itemStack = this.itemStack.Clone();
			if (!base.xui.PlayerInventory.AddItem(itemStack))
			{
				return;
			}
			PlayPlaceSound();
			break;
		}
		case StackLocationTypes.LootContainer:
		case StackLocationTypes.Workstation:
		case StackLocationTypes.Merge:
			if (base.xui.PlayerInventory.AddItem(ItemStack))
			{
				PlayPlaceSound();
				ItemStack = ItemStack.Empty.Clone();
				HandleSlotChangeEvent();
			}
			else if (count != ItemStack.count)
			{
				PlayPlaceSound();
				if (ItemStack.count == 0)
				{
					ItemStack = ItemStack.Empty.Clone();
				}
				HandleSlotChangeEvent();
				return;
			}
			break;
		case StackLocationTypes.Backpack:
		case StackLocationTypes.ToolBelt:
		{
			bool flag = base.xui.AssembleItem?.CurrentItem != null;
			if (base.xui.vehicle != null && !flag)
			{
				string vehicleSlotType = ItemStack.itemValue.ItemClass.VehicleSlotType;
				if (vehicleSlotType != "" && base.xui.Vehicle.SetPart(base.xui, vehicleSlotType, ItemStack, out var resultStack))
				{
					PlayPlaceSound();
					ItemStack = resultStack;
					HandleSlotChangeEvent();
					return;
				}
				if (base.xui.vehicle.GetVehicle().HasStorage())
				{
					XUiC_VehicleContainer childByType = base.xui.FindWindowGroupByName(XUiC_VehicleStorageWindowGroup.ID).GetChildByType<XUiC_VehicleContainer>();
					if (childByType != null)
					{
						if (childByType.AddItem(ItemStack))
						{
							PlayPlaceSound();
							ItemStack = ItemStack.Empty.Clone();
							HandleSlotChangeEvent();
							return;
						}
						if (count != ItemStack.count)
						{
							PlayPlaceSound();
							if (ItemStack.count == 0)
							{
								ItemStack = ItemStack.Empty.Clone();
							}
							HandleSlotChangeEvent();
							return;
						}
					}
				}
			}
			if (flag && ItemStack.itemValue.ItemClass is ItemClassModifier)
			{
				if (base.xui.AssembleItem.AddPartToItem(ItemStack, out var resultStack2))
				{
					PlayPlaceSound();
					ItemStack = resultStack2;
					HandleSlotChangeEvent();
				}
				else
				{
					Manager.PlayInsidePlayerHead("ui_denied");
				}
				return;
			}
			if (base.xui.PlayerEquipment != null && base.xui.PlayerEquipment.IsOpen && this.itemStack.itemValue.ItemClass.IsEquipment)
			{
				PlayPlaceSound();
				ItemStack = base.xui.PlayerEquipment.EquipItem(ItemStack);
				HandleSlotChangeEvent();
				return;
			}
			if (base.xui.lootContainer != null)
			{
				XUiC_LootContainer childByType2 = base.xui.FindWindowGroupByName(XUiC_LootWindowGroup.ID).GetChildByType<XUiC_LootContainer>();
				if (XUiM_LootContainer.AddItem(ItemStack, base.xui))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty.Clone();
					HandleSlotChangeEvent();
					childByType2?.SetSlots(base.xui.lootContainer, base.xui.lootContainer.items);
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty.Clone();
					}
					HandleSlotChangeEvent();
					childByType2?.SetSlots(base.xui.lootContainer, base.xui.lootContainer.items);
					return;
				}
			}
			if (base.xui.currentWorkstationToolGrid != null && base.xui.currentWorkstationToolGrid.TryAddTool(itemClass, ItemStack))
			{
				PlayPlaceSound();
				ItemStack = ItemStack.Empty.Clone();
				HandleSlotChangeEvent();
				return;
			}
			if (base.xui.currentWorkstationFuelGrid != null && itemClass.FuelValue != null && itemClass.FuelValue.Value > 0)
			{
				if (base.xui.currentWorkstationFuelGrid.AddItem(itemClass, ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty.Clone();
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty.Clone();
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			if (base.xui.currentWorkstationInputGrid != null && base.xui.currentWorkstationInputGrid.AcceptsMaterial(itemClass.MadeOfMaterial))
			{
				if (base.xui.currentWorkstationInputGrid.AddItem(itemClass, ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty.Clone();
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty.Clone();
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			if (base.xui.currentWorkstationOutputGrid != null)
			{
				if (base.xui.currentWorkstationOutputGrid.AddItem(itemClass, ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty.Clone();
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty.Clone();
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			if (base.xui.currentDewCollectorModGrid != null && base.xui.currentDewCollectorModGrid.TryAddMod(itemClass, ItemStack))
			{
				PlayPlaceSound();
				ItemStack = ItemStack.Empty.Clone();
				HandleSlotChangeEvent();
				return;
			}
			if (base.xui.currentCombineGrid != null && base.xui.currentCombineGrid.TryAddItemToSlot(itemClass, ItemStack))
			{
				PlayPlaceSound();
				ItemStack = ItemStack.Empty.Clone();
				HandleSlotChangeEvent();
				return;
			}
			if (base.xui.powerSourceSlots != null && base.xui.powerSourceSlots.TryAddItemToSlot(itemClass, ItemStack))
			{
				PlayPlaceSound();
				ItemStack = ItemStack.Empty.Clone();
				HandleSlotChangeEvent();
				return;
			}
			if (base.xui.powerAmmoSlots != null)
			{
				if (base.xui.powerAmmoSlots.TryAddItemToSlot(itemClass, ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty.Clone();
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty.Clone();
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			if (base.xui.Trader.Trader != null && (StackLocation == StackLocationTypes.Backpack || StackLocation == StackLocationTypes.ToolBelt))
			{
				HandleItemInspect();
				InfoWindow.SetMaxCountOnDirty = true;
				return;
			}
			if (StackLocation == StackLocationTypes.Backpack)
			{
				if (base.xui.PlayerInventory.AddItemToToolbelt(ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty.Clone();
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty.Clone();
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			else
			{
				if (StackLocation != StackLocationTypes.ToolBelt)
				{
					break;
				}
				if (base.xui.PlayerInventory.AddItemToBackpack(ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty.Clone();
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty.Clone();
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			break;
		}
		}
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
	}

	public void PlayPlaceSound(ItemStack newStack = null)
	{
		string text = "";
		text = ((newStack == null) ? ((itemStack.itemValue.ItemClass == null) ? "" : itemStack.itemValue.ItemClass.SoundPlace) : ((newStack.itemValue.ItemClass == null) ? "" : newStack.itemValue.ItemClass.SoundPlace));
		if (text != "")
		{
			if (text != null)
			{
				Manager.PlayInsidePlayerHead(text);
			}
		}
		else if (placeSound != null)
		{
			Manager.PlayXUiSound(placeSound, 0.75f);
		}
	}

	public void PlayPickupSound(ItemStack newStack = null)
	{
		ItemStack itemStack = ((newStack != null) ? newStack : this.itemStack);
		string text = ((itemStack.IsEmpty() || itemStack.itemValue.ItemClass == null) ? "" : itemStack.itemValue.ItemClass.SoundPickup);
		if (text != "")
		{
			if (text != null)
			{
				Manager.PlayInsidePlayerHead(text);
			}
		}
		else if (pickupSound != null)
		{
			Manager.PlayXUiSound(pickupSound, 0.75f);
		}
	}

	public void UnlockStack()
	{
		lockType = LockTypes.None;
		IsLocked = false;
		lockTime = 0f;
		lockSprite = "";
		setLockTypeIconColor(Color.white);
		RepairAmount = 0;
		timer.IsVisible = false;
		IsDirty = true;
	}

	public void LockStack(LockTypes _lockType, float _time, int _count, BaseItemActionEntry _itemActionEntry)
	{
		switch (_lockType)
		{
		case LockTypes.Crafting:
			lockSprite = _itemActionEntry.IconName;
			break;
		case LockTypes.Scrapping:
			lockSprite = "ui_game_symbol_scrap";
			break;
		case LockTypes.Burning:
			lockSprite = "ui_game_symbol_campfire";
			break;
		case LockTypes.Repairing:
			lockSprite = "ui_game_symbol_hammer";
			break;
		}
		IsLocked = true;
		lockType = _lockType;
		if (_lockType == LockTypes.Repairing)
		{
			RepairAmount = _count;
		}
		LockTime = _time;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLockTypeIcon()
	{
		if (IsLocked && lockType != LockTypes.None)
		{
			return;
		}
		lockSprite = "";
		if (itemClass != null)
		{
			if (itemClass is ItemClassBlock itemClassBlock)
			{
				itemClassBlock.GetBlock();
				if (!itemStack.itemValue.TextureFullArray.IsDefault)
				{
					lockSprite = "ui_game_symbol_paint_brush";
				}
			}
			if (itemClass is ItemClassModifier itemClassModifier)
			{
				lockSprite = "ui_game_symbol_assemble";
				if (itemClassModifier.HasAnyTags(ItemClassModifier.CosmeticModTypes))
				{
					lockSprite = "ui_game_symbol_paint_bucket";
				}
				if (base.xui.AssembleItem.CurrentItem != null)
				{
					if ((itemClassModifier.InstallableTags.IsEmpty || base.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags)) && !base.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.DisallowedTags))
					{
						if (StackLocation != StackLocationTypes.Part)
						{
							int num = 0;
							for (int i = 0; i < base.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
							{
								ItemValue itemValue = base.xui.AssembleItem.CurrentItem.itemValue.Modifications[i];
								if (!itemValue.IsEmpty() && itemValue.ItemClass.HasAnyTags(itemClassModifier.ItemTags))
								{
									num++;
								}
							}
							if (num >= itemClassModifier.MaxModsAllowed)
							{
								flashLockTypeIcon = flashLockTypes.AlreadyEquipped;
								return;
							}
						}
						flashLockTypeIcon = flashLockTypes.Allowed;
					}
					else
					{
						setLockTypeIconColor(Color.grey);
						flashLockTypeIcon = flashLockTypes.None;
					}
				}
				else
				{
					setLockTypeIconColor(Color.white);
					flashLockTypeIcon = flashLockTypes.None;
				}
			}
			if (itemStack.itemValue.HasMods())
			{
				lockSprite = "ui_game_symbol_modded";
				setLockTypeIconColor(Color.white);
				flashLockTypeIcon = flashLockTypes.None;
			}
		}
		if (StackLocation == StackLocationTypes.Part)
		{
			lockSprite = "";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLockTypeIconColor(Color _color)
	{
		if (lockTypeIcon != null)
		{
			lockTypeIcon.Color = _color;
		}
	}

	public void ForceRefreshItemStack()
	{
		this.SlotChangedEvent?.Invoke(SlotNumber, itemStack);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "tooltip":
			_value = ItemNameText;
			return true;
		case "isfavorite":
			_value = (ShowFavorites && IsFavorite).ToString();
			return true;
		case "isQuickSwap":
			_value = isQuickSwap.ToString();
			return true;
		case "islocked":
			_value = isLocked.ToString();
			return true;
		case "ishovered":
			_value = isOver.ToString();
			return true;
		case "itemicon":
			_value = ItemIcon;
			return true;
		case "iconcolor":
			_value = ItemIconColor;
			return true;
		case "itemcount":
			_value = "";
			if (!itemStack.IsEmpty())
			{
				if (ShowDurability)
				{
					_value = ((itemStack.itemValue.Quality > 0) ? itemcountFormatter.Format(itemStack.itemValue.Quality) : (itemStack.itemValue.IsMod ? "*" : ""));
				}
				else
				{
					_value = ((itemClassOrMissing.Stacknumber == 1) ? "" : itemcountFormatter.Format(itemStack.count));
				}
			}
			return true;
		case "hasdurability":
			_value = ShowDurability.ToString();
			return true;
		case "durabilitycolor":
			_value = durabilitycolorFormatter.Format(QualityInfo.GetQualityColor(itemStack?.itemValue.Quality ?? 0));
			return true;
		case "durabilityfill":
			_value = ((itemStack?.itemValue == null) ? "0.0" : durabilityFillFormatter.Format(itemStack.itemValue.PercentUsesLeft));
			return true;
		case "showicon":
			_value = (ItemIcon != "").ToString();
			return true;
		case "itemtypeicon":
			_value = "";
			if (!itemStack.IsEmpty())
			{
				ItemClass itemClass3 = itemClassOrMissing;
				if (itemClass3 != null)
				{
					if (itemClass3.IsBlock() && itemStack.itemValue.TextureFullArray.IsDefault)
					{
						_value = Block.list[itemStack.itemValue.type].ItemTypeIcon;
					}
					else
					{
						if (itemClass3.AltItemTypeIcon != null && itemClass3.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, itemClass3, itemStack.itemValue))
						{
							_value = itemClass3.AltItemTypeIcon;
							return true;
						}
						_value = itemClass3.ItemTypeIcon;
					}
				}
			}
			return true;
		case "hasitemtypeicon":
			if (itemStack.IsEmpty() || !string.IsNullOrEmpty(lockSprite))
			{
				_value = "false";
			}
			else
			{
				ItemClass itemClass2 = itemClassOrMissing;
				if (itemClass2 == null)
				{
					_value = "false";
				}
				else
				{
					_value = (itemClass2.IsBlock() ? (Block.list[itemStack.itemValue.type].ItemTypeIcon != "").ToString() : (itemClass2.ItemTypeIcon != "").ToString());
				}
			}
			return true;
		case "itemtypeicontint":
			_value = "255,255,255,255";
			if (!itemStack.IsEmpty())
			{
				ItemClass itemClass = itemClassOrMissing;
				if (itemClass != null && itemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, itemClass, itemStack.itemValue))
				{
					_value = altitemtypeiconcolorFormatter.Format(itemClass.AltItemTypeIconColor);
				}
			}
			return true;
		case "locktypeicon":
			_value = lockSprite ?? "";
			return true;
		case "isassemblelocked":
			_value = ((stackLockType != StackLockTypes.None && stackLockType != StackLockTypes.Hidden) || (attributeLock && itemStack.IsEmpty())).ToString();
			return true;
		case "stacklockicon":
			if (stackLockType == StackLockTypes.Quest)
			{
				_value = "ui_game_symbol_quest";
			}
			else if (attributeLock && itemStack.IsEmpty())
			{
				_value = "ui_game_symbol_pack_mule";
			}
			else
			{
				_value = "ui_game_symbol_lock";
			}
			return true;
		case "stacklockcolor":
			if (attributeLock && itemStack.IsEmpty())
			{
				_value = "200,200,200,64";
			}
			else
			{
				_value = "255,255,255,255";
			}
			return true;
		case "selectionbordercolor":
			_value = selectionbordercolorFormatter.Format(SelectionBorderColor);
			return true;
		case "backgroundcolor":
			_value = backgroundcolorFormatter.Format(AttributeLock ? attributeLockColor : backgroundColor);
			return true;
		case "userlockedslot":
			_value = UserLockedSlot.ToString();
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "select_color":
			selectColor = StringParsers.ParseColor32(_value);
			return true;
		case "press_color":
			pressColor = StringParsers.ParseColor32(_value);
			return true;
		case "final_pressed_color":
			finalPressedColor = StringParsers.ParseColor32(_value);
			return true;
		case "background_color":
			backgroundColor = StringParsers.ParseColor32(_value);
			return true;
		case "highlight_color":
			highlightColor = StringParsers.ParseColor32(_value);
			return true;
		case "attribute_lock_color":
			attributeLockColor = StringParsers.ParseColor32(_value);
			return true;
		case "holding_color":
			holdingColor = StringParsers.ParseColor32(_value);
			return true;
		case "mod_allowed_color":
			modAllowedColor = StringParsers.ParseColor32(_value);
			return true;
		case "mod_already_equipped_color":
			modAlreadyEquippedColor = StringParsers.ParseColor32(_value);
			return true;
		case "pickup_snap_distance":
			PickupSnapDistance = int.Parse(_value);
			return true;
		case "hover_icon_grow":
			HoverIconGrow = StringParsers.ParseFloat(_value);
			return true;
		case "pickup_sound":
			base.xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
			{
				pickupSound = _o;
			});
			return true;
		case "place_sound":
			base.xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
			{
				placeSound = _o;
			});
			return true;
		case "allow_dropping":
			AllowDropping = StringParsers.ParseBool(_value);
			return true;
		case "prefix_id":
			PrefixId = StringParsers.ParseBool(_value);
			return true;
		case "show_favorites":
			ShowFavorites = StringParsers.ParseBool(_value);
			return true;
		case "override_stack_count":
			OverrideStackCount = StringParsers.ParseSInt32(_value);
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	public static bool IsStackLocationFromPlayer(StackLocationTypes? location)
	{
		if (location.HasValue)
		{
			if (location != StackLocationTypes.Backpack && location != StackLocationTypes.ToolBelt)
			{
				return location == StackLocationTypes.Equipment;
			}
			return true;
		}
		return false;
	}
}

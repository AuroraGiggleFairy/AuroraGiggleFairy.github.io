using Audio;
using InControl;
using Unity.Profiling;
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
		Cosmetics,
		Part,
		CollectorCatalysts,
		CollectorFuel,
		Collector
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack itemStack = ItemStack.Empty;

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
	public Color32 statBoostedColor = new Color32(byte.MaxValue, 175, 11, byte.MaxValue);

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

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmIsUpdate = new ProfilerMarker("XCItemStack");

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmIsUpdateBase = new ProfilerMarker("XCItemStack.Base");

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly CachedStringFormatterXuiRgbaColor itemiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt itemcountFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat durabilityFillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat removeddurabilityFillFormatter = new CachedStringFormatterFloat();

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
			IsDirty = true;
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
			stackLockType = (value ? StackLockTypes.Quest : ((stackLockType != StackLockTypes.Quest) ? stackLockType : StackLockTypes.None));
			IsDirty = true;
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
			IsDirty = true;
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
			IsDirty = true;
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
			IsDirty = true;
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

	[XuiXmlBinding("islocked")]
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

	[XuiXmlBinding("userlockedslot")]
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
				if (base.IsSelected)
				{
					updateItemInfoWindow(this);
				}
				HandleSlotChangeEvent();
				if (value.IsEmpty())
				{
					stackLockType = StackLockTypes.None;
				}
				updateBackgroundTexture();
				IsDirty = true;
			}
			else
			{
				if (ItemStack.IsEmpty() && backgroundTexture != null)
				{
					backgroundTexture.Texture = null;
				}
				if (base.IsSelected)
				{
					updateItemInfoWindow(this);
				}
				xui.playerUI.CursorController.HoverTarget = null;
			}
			if (ItemStack.IsEmpty() && base.IsSelected)
			{
				base.IsSelected = false;
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
				return xui.playerUI.entityPlayer.favoriteCreativeStacks.Contains((ushort)itemStack.itemValue.type);
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

	[XuiXmlBinding("hasdurability")]
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
			int num = itemStack.itemValue.Meta;
			if (num == 0)
			{
				num = itemActionTextureBlock.DefaultTextureID;
			}
			if (TryResolvePaintTextureId(num, itemActionTextureBlock.DefaultTextureID, out var resolvedPaintId, out var textureId))
			{
				if (resolvedPaintId != itemStack.itemValue.Meta)
				{
					itemStack.itemValue.Meta = (byte)resolvedPaintId;
				}
				setTextureForOpaqueMeshTextureId(textureId);
			}
			else
			{
				backgroundTexture.Texture = null;
				backgroundTexture.SetTextureDirty();
			}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryResolvePaintTextureId(int requestedPaintId, int fallbackPaintId, out int resolvedPaintId, out int textureId)
	{
		resolvedPaintId = 0;
		textureId = 0;
		if (TryGetBlockTextureData(requestedPaintId, out var data))
		{
			resolvedPaintId = requestedPaintId;
			textureId = data.TextureID;
			return true;
		}
		if (fallbackPaintId != requestedPaintId && TryGetBlockTextureData(fallbackPaintId, out var data2))
		{
			resolvedPaintId = fallbackPaintId;
			textureId = data2.TextureID;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryGetBlockTextureData(int paintId, out BlockTextureData data)
	{
		data = null;
		BlockTextureData[] list = BlockTextureData.list;
		if (list == null || paintId < 0 || paintId >= list.Length)
		{
			return false;
		}
		data = list[paintId];
		return data != null;
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
		bool flag = base.IsSelected;
		itemStack = ItemStack.Empty;
		this.SlotChangedEvent?.Invoke(SlotNumber, itemStack);
		if (!_stack.IsEmpty())
		{
			base.IsSelected = flag;
		}
		ItemStack = _stack;
		this.SlotChangedEvent?.Invoke(SlotNumber, itemStack);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleSlotChangeEvent()
	{
		if (itemStack.IsEmpty() && base.IsSelected)
		{
			base.IsSelected = false;
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
		using (pmIsUpdate.Auto())
		{
			using (pmIsUpdateBase.Auto())
			{
				base.Update(_dt);
			}
			if (base.WindowGroup.isShowing)
			{
				PlayerActionsGUI gUIActions = xui.playerUI.playerInput.GUIActions;
				CursorControllerAbs cursorController = xui.playerUI.CursorController;
				Vector3 vector = cursorController.GetScreenPosition();
				bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
				bool mouseButtonDown = cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
				bool mouseButton = cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
				bool mouseButtonUp2 = cursorController.GetMouseButtonUp(UICamera.MouseButton.RightButton);
				bool mouseButtonDown2 = cursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
				bool mouseButton2 = cursorController.GetMouseButton(UICamera.MouseButton.RightButton);
				if (!IsLocked && !isDragAndDrop)
				{
					if (isOver && base.ViewComponent.UiTransformIsHovered && base.ViewComponent.EventOnPress)
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
							else if (xui.DragAndDropWindow.CurrentStack.IsEmpty() && !ItemStack.IsEmpty())
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
							if (xui.DragAndDropWindow.CurrentStack.IsEmpty() && !ItemStack.IsEmpty())
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
							if (xui.DragAndDropWindow.CurrentStack.IsEmpty())
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
		else if (base.IsSelected)
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
			base.IsSelected = true;
			InfoWindow.SetMaxCountOnDirty = true;
			updateItemInfoWindow(this);
		}
		HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleStackSwap()
	{
		xui.PopupMenuWindow.Close();
		if (!AllowDropping)
		{
			xui.DragAndDropWindow.CurrentStack = ItemStack.Empty;
			xui.DragAndDropWindow.PickUpType = StackLocation;
		}
		bool flag = false;
		ItemStack currentStack = xui.DragAndDropWindow.CurrentStack;
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
			base.IsSelected = false;
		}
		else if (!flag && (!this.itemStack.itemValue.ItemClassOrMissing.CanStack() || !itemClass.CanStack()))
		{
			SwapItem();
			base.IsSelected = false;
		}
		else if (currentStack.itemValue.type == this.itemStack.itemValue.type && !currentStack.itemValue.HasQuality && !this.itemStack.itemValue.HasQuality)
		{
			if (currentStack.count + this.itemStack.count > num)
			{
				int count = currentStack.count + this.itemStack.count - num;
				ItemStack itemStack = this.itemStack.Clone();
				itemStack.count = num;
				currentStack.count = count;
				xui.DragAndDropWindow.CurrentStack = currentStack;
				xui.DragAndDropWindow.PickUpType = StackLocation;
				ItemStack = itemStack;
			}
			else
			{
				ItemStack itemStack2 = this.itemStack.Clone();
				itemStack2.count += currentStack.count;
				ItemStack = itemStack2;
				xui.DragAndDropWindow.CurrentStack = ItemStack.Empty;
				PlayPlaceSound();
			}
			if (base.IsSelected)
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
			xui.DragAndDropWindow.CurrentStack = currentStack;
			xui.DragAndDropWindow.PickUpType = StackLocation;
			ItemStack = itemStack3;
		}
		else
		{
			SwapItem();
			base.IsSelected = false;
		}
		HandleClickComplete();
		xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandlePartialStackPickup()
	{
		ItemStack currentStack = xui.DragAndDropWindow.CurrentStack;
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
				xui.DragAndDropWindow.CurrentStack = currentStack;
				xui.DragAndDropWindow.PickUpType = StackLocation;
			}
		}
		if (base.IsSelected)
		{
			base.IsSelected = false;
		}
		HandleClickComplete();
		xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleDropOne()
	{
		ItemStack currentStack = xui.DragAndDropWindow.CurrentStack;
		if (!currentStack.CanMoveTo(StackLocation))
		{
			return;
		}
		if (!currentStack.IsEmpty())
		{
			int num = 1;
			if (this.itemStack.IsEmpty())
			{
				ItemStack itemStack = currentStack.Clone();
				itemStack.count = num;
				currentStack.count -= num;
				xui.DragAndDropWindow.CurrentStack = currentStack;
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
					xui.DragAndDropWindow.CurrentStack = currentStack;
					this.SlotChangedEvent?.Invoke(SlotNumber, this.itemStack);
					IsDirty = true;
				}
				PlayPlaceSound();
			}
			if (currentStack.count == 0)
			{
				xui.DragAndDropWindow.CurrentStack = ItemStack.Empty;
			}
		}
		base.IsSelected = false;
		HandleClickComplete();
		xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
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
		xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver, _canSwap: true, !StackLock);
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SwapItem()
	{
		xui.PopupMenuWindow.Close();
		ItemStack currentStack = xui.DragAndDropWindow.CurrentStack;
		if ((!currentStack.IsEmpty() && !CanSwap(currentStack)) || !currentStack.CanMoveTo(StackLocation))
		{
			return;
		}
		if (StackLocation == StackLocationTypes.LootContainer && xui.DragAndDropWindow.PickUpType != StackLocationTypes.LootContainer && !currentStack.IsEmpty() && !currentStack.itemValue.ItemClassOrMissing.CanPlaceInContainer())
		{
			Manager.PlayInsidePlayerHead("ui_denied");
			GameManager.ShowTooltip(xui.playerUI.entityPlayer, "Quest Items cannot be placed in containers.");
			return;
		}
		if (itemStack.IsEmpty())
		{
			PlayPlaceSound(currentStack);
		}
		xui.DragAndDropWindow.CurrentStack = itemStack.Clone();
		xui.DragAndDropWindow.PickUpType = StackLocation;
		if (StackLocation == StackLocationTypes.ToolBelt)
		{
			xui.DragAndDropWindow.CurrentStack.Deactivate();
		}
		ForceSetItemStack(currentStack.Clone());
		xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
	}

	public void HandleMoveToPreferredLocation()
	{
		xui.PopupMenuWindow.Close();
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
			if (!xui.PlayerInventory.AddItem(itemStack))
			{
				return;
			}
			PlayPlaceSound();
			break;
		}
		case StackLocationTypes.LootContainer:
		case StackLocationTypes.Workstation:
		case StackLocationTypes.Merge:
		case StackLocationTypes.CollectorCatalysts:
		case StackLocationTypes.CollectorFuel:
		case StackLocationTypes.Collector:
			if (xui.PlayerInventory.AddItem(ItemStack))
			{
				PlayPlaceSound();
				ItemStack = ItemStack.Empty;
				HandleSlotChangeEvent();
			}
			else if (count != ItemStack.count)
			{
				PlayPlaceSound();
				if (ItemStack.count == 0)
				{
					ItemStack = ItemStack.Empty;
				}
				HandleSlotChangeEvent();
				return;
			}
			break;
		case StackLocationTypes.Backpack:
		case StackLocationTypes.ToolBelt:
		{
			bool flag = xui.AssembleItem?.CurrentItem != null;
			if (!flag && xui.FindWindowGroupByName(XUiC_BagStorageWindowGroup.ID) is XUiC_BagStorageWindowGroup { Bag: not null })
			{
				string vehicleSlotType = ItemStack.itemValue.ItemClass.VehicleSlotType;
				if (vehicleSlotType != "" && xui.Vehicle.SetPart(xui, vehicleSlotType, ItemStack, out var _resultStack))
				{
					PlayPlaceSound();
					ItemStack = _resultStack;
					HandleSlotChangeEvent();
					return;
				}
				XUiC_BagContainer childByType = xui.FindWindowGroupByName(XUiC_BagStorageWindowGroup.ID).GetChildByType<XUiC_BagContainer>();
				if (childByType != null && childByType.Bag != null && childByType.Bag.CanStack(this.itemStack) && childByType.AddItem(ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty;
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			if (flag && ItemStack.itemValue.ItemClass is ItemClassModifier)
			{
				if (xui.AssembleItem.AddPartToItem(ItemStack, out var _resultStack2))
				{
					PlayPlaceSound();
					ItemStack = _resultStack2;
					HandleSlotChangeEvent();
				}
				else
				{
					Manager.PlayInsidePlayerHead("ui_denied");
				}
				return;
			}
			if (xui.PlayerEquipment != null && xui.PlayerEquipment.IsOpen && this.itemStack.itemValue.ItemClass.CanEquip())
			{
				PlayPlaceSound();
				ItemStack = xui.PlayerEquipment.EquipItem(ItemStack);
				HandleSlotChangeEvent();
				return;
			}
			if (xui.LootContainer != null)
			{
				XUiC_LootContainer childByType2 = xui.FindWindowGroupByName(XUiC_LootWindowGroup.ID).GetChildByType<XUiC_LootContainer>();
				if (XUiM_LootContainer.AddItem(ItemStack, xui))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					childByType2?.SetSlots(xui.LootContainer, xui.LootContainer.items);
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty;
					}
					HandleSlotChangeEvent();
					childByType2?.SetSlots(xui.LootContainer, xui.LootContainer.items);
					return;
				}
			}
			if (xui.CurrentWorkstationToolGrid != null && xui.CurrentWorkstationToolGrid.TryAddTool(itemClass, ItemStack))
			{
				PlayPlaceSound();
				ItemStack = ItemStack.Empty;
				HandleSlotChangeEvent();
				return;
			}
			if (xui.CurrentWorkstationFuelGrid != null && itemClass.FuelValue != null && itemClass.FuelValue.Value > 0)
			{
				if (xui.CurrentWorkstationFuelGrid.AddItem(itemClass, ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty;
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			if (xui.CurrentWorkstationInputGrid != null && xui.CurrentWorkstationInputGrid.AcceptsMaterial(itemClass.MadeOfMaterial))
			{
				if (xui.CurrentWorkstationInputGrid.AddItem(itemClass, ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty;
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			if (xui.CurrentWorkstationOutputGrid != null)
			{
				if (xui.CurrentWorkstationOutputGrid.AddItem(itemClass, ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty;
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			if (xui.CurrentCollectorModGrid != null)
			{
				if (xui.CurrentCollectorModGrid.TryAddMod(itemClass, ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					return;
				}
				if (xui.CurrentCollectorFuelGrid != null && xui.CurrentCollectorFuelGrid.TryAddFuel(itemClass, ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					return;
				}
				if (xui.currentCollectorCatalystGrid != null && xui.currentCollectorCatalystGrid.TryAddFuel(itemClass, ItemStack))
				{
					itemClass.OnPlacedAsCatalyst(xui.PlayerInventory.Toolbelt.holdingItemData);
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					return;
				}
			}
			if (xui.CurrentCombineGrid != null && xui.CurrentCombineGrid.TryAddItemToSlot(itemClass, ItemStack))
			{
				PlayPlaceSound();
				ItemStack = ItemStack.Empty;
				HandleSlotChangeEvent();
				return;
			}
			if (xui.CurrentPowerSourceSlots != null && xui.CurrentPowerSourceSlots.TryAddItemToSlot(itemClass, ItemStack))
			{
				PlayPlaceSound();
				ItemStack = ItemStack.Empty;
				HandleSlotChangeEvent();
				return;
			}
			if (xui.CurrentPowerAmmoSlots != null)
			{
				if (xui.CurrentPowerAmmoSlots.TryAddItemToSlot(itemClass, ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty;
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			if (xui.Trader.TraderData != null && (StackLocation == StackLocationTypes.Backpack || StackLocation == StackLocationTypes.ToolBelt))
			{
				HandleItemInspect();
				InfoWindow.SetMaxCountOnDirty = true;
				return;
			}
			if (StackLocation == StackLocationTypes.Backpack)
			{
				if (xui.PlayerInventory.AddItemToToolbelt(ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty;
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
				if (xui.PlayerInventory.AddItemToBackpack(ItemStack))
				{
					PlayPlaceSound();
					ItemStack = ItemStack.Empty;
					HandleSlotChangeEvent();
					return;
				}
				if (count != ItemStack.count)
				{
					PlayPlaceSound();
					if (ItemStack.count == 0)
					{
						ItemStack = ItemStack.Empty;
					}
					HandleSlotChangeEvent();
					return;
				}
			}
			break;
		}
		}
		xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
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
			Color lockTypeIconColor = Color.clear;
			if (itemClass is ItemClassBlock itemClassBlock)
			{
				itemClassBlock.GetBlock();
				if (!itemStack.itemValue.TextureFullArray.IsDefault)
				{
					lockSprite = "ui_game_symbol_paint_brush";
				}
			}
			QuestLock = itemClass.IsQuestItem && (StackLocation == StackLocationTypes.Backpack || StackLocation == StackLocationTypes.ToolBelt);
			if (itemClass is ItemClassModifier itemClassModifier)
			{
				lockSprite = "ui_game_symbol_assemble";
				if (itemClassModifier.HasAnyTags(ItemClassModifier.CosmeticModTypes))
				{
					lockSprite = "ui_game_symbol_paint_bucket";
				}
				if (xui.AssembleItem.CurrentItem != null)
				{
					if ((itemClassModifier.InstallableTags.IsEmpty || xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags)) && !xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.DisallowedTags))
					{
						if (StackLocation != StackLocationTypes.Part)
						{
							int num = 0;
							for (int i = 0; i < xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
							{
								ItemValue itemValue = xui.AssembleItem.CurrentItem.itemValue.Modifications[i];
								if (!itemValue.IsEmpty() && itemValue.ItemClass is ItemClassModifier itemClassModifier2 && itemClassModifier2.ModifierTags.Test_AnySet(itemClassModifier.ModifierTags))
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
						lockTypeIconColor = Color.grey;
					}
				}
				else
				{
					lockTypeIconColor = Color.white;
				}
			}
			bool flag = itemStack.itemValue.HasAnyBoostedStats();
			if (flag)
			{
				lockSprite = "server_favorite";
				lockTypeIconColor = statBoostedColor;
			}
			if (itemStack.itemValue.HasMods())
			{
				lockSprite = "ui_game_symbol_modded";
				if (!flag)
				{
					lockTypeIconColor = Color.white;
				}
			}
			if (lockTypeIconColor.a > 0f)
			{
				setLockTypeIconColor(lockTypeIconColor);
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
		case "haspermadurability":
			if (!ShowDurability || itemStack?.itemValue == null || !EntityPlayerLocal.PermaDegrationOn)
			{
				_value = "false";
				return true;
			}
			_value = (itemStack.itemValue.MaxDurabilityModifier < 1f).ToString();
			return true;
		case "durabilitycolor":
			_value = durabilitycolorFormatter.Format(QualityInfo.GetQualityColor(itemStack?.itemValue.Quality ?? 0));
			return true;
		case "durabilityfill":
			_value = ((itemStack?.itemValue == null) ? "0.0" : ((itemStack.itemValue.MaxUseTimes == 0) ? "1" : durabilityFillFormatter.Format(((float)itemStack.itemValue.MaxUseTimes - itemStack.itemValue.UseTimes) / (float)itemStack.itemValue.MaxUseTimesUI)));
			return true;
		case "removeddurabilityfill":
			if (itemStack?.itemValue == null)
			{
				_value = "1";
				return true;
			}
			_value = removeddurabilityFillFormatter.Format(itemStack.itemValue.MaxDurabilityModifier);
			return true;
		case "showicon":
			_value = (ItemIcon != "").ToString();
			return true;
		case "itemtypeicon":
			_value = "";
			if (!itemStack.IsEmpty())
			{
				ItemClass itemClass = itemClassOrMissing;
				if (itemClass != null)
				{
					if (itemClass.IsBlock() && itemStack.itemValue.TextureFullArray.IsDefault)
					{
						_value = Block.list[itemStack.itemValue.type].ItemTypeIcon;
					}
					else
					{
						if (itemClass.AltItemTypeIcon != null && itemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(xui.playerUI.entityPlayer, itemClass, itemStack.itemValue))
						{
							_value = itemClass.AltItemTypeIcon;
							return true;
						}
						_value = itemClass.ItemTypeIcon;
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
				ItemClass itemClass3 = itemClassOrMissing;
				if (itemClass3 == null)
				{
					_value = "false";
				}
				else
				{
					_value = (itemClass3.IsBlock() ? (Block.list[itemStack.itemValue.type].ItemTypeIcon != "").ToString() : (itemClass3.ItemTypeIcon != "").ToString());
				}
			}
			return true;
		case "itemtypeicontint":
			_value = "255,255,255,255";
			if (!itemStack.IsEmpty())
			{
				ItemClass itemClass2 = itemClassOrMissing;
				if (itemClass2 != null && itemClass2.Unlocks != "" && XUiM_ItemStack.CheckKnown(xui.playerUI.entityPlayer, itemClass2, itemStack.itemValue))
				{
					_value = altitemtypeiconcolorFormatter.Format(itemClass2.AltItemTypeIconColor);
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
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	[XuiXmlBinding("ishovered")]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bindingIshovered()
	{
		return isOver;
	}

	public override bool ParseAttribute(string _name, string _value)
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
			xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
			{
				pickupSound = _o;
			});
			return true;
		case "place_sound":
			xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
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
			return base.ParseAttribute(_name, _value);
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

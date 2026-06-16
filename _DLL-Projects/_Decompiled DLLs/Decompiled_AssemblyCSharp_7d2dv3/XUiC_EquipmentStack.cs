using Audio;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_EquipmentStack : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = ItemStack.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue = new ItemValue();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHighlightEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDropEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectedBorderColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 hoverBorderColor = new Color32(182, 166, 123, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 normalBorderColor = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastClicked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string emptySpriteName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string emptyTooltipName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EquipmentSlots equipSlot = EquipmentSlots.Count;

	public int PickupSnapDistance = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label stackValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite itemIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite durability;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite durabilityRemoveBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite durabilityBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite lockTypeIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	public bool AllowRemoveItem = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenScale tweenScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;

	public EquipmentSlots EquipSlot
	{
		get
		{
			return equipSlot;
		}
		set
		{
			equipSlot = value;
		}
	}

	public int SlotNumber => (int)EquipSlot;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float HoverIconGrow
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ItemValue ItemValue
	{
		get
		{
			return itemValue;
		}
		set
		{
			if (itemValue != value)
			{
				itemValue = value;
				itemStack.itemValue = itemValue;
				if (!itemStack.itemValue.IsEmpty())
				{
					itemStack.count = 1;
				}
				if (value.IsEmpty() && base.IsSelected)
				{
					base.IsSelected = false;
					InfoWindow?.SetItemStack((XUiC_EquipmentStack)null, true);
				}
				this.SlotChangedEvent?.Invoke(SlotNumber, itemStack);
			}
			IsDirty = true;
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
			if (itemStack != value)
			{
				itemStack = value.Clone();
				ItemValue = itemStack.itemValue.Clone();
				IsDirty = true;
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterFrameWindow FrameWindow { get; set; }

	public event XUiEvent_SlotChangedEventHandler SlotChangedEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool _isSelected)
	{
		SetBorderColor(_isSelected ? selectedBorderColor : normalBorderColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBorderColor(Color32 _color)
	{
		background.Color = _color;
	}

	public override void Init()
	{
		base.Init();
		stackValue = GetChildById("stackValue")?.ViewComponent as XUiV_Label;
		background = GetChildById("background")?.ViewComponent as XUiV_Sprite;
		SetBorderColor(normalBorderColor);
		itemIcon = GetChildById("itemIcon")?.ViewComponent as XUiV_Sprite;
		durabilityBackground = GetChildById("durabilityBackground")?.ViewComponent as XUiV_Sprite;
		durabilityRemoveBackground = GetChildById("durabilityRemoveBackground")?.ViewComponent as XUiV_Sprite;
		durability = GetChildById("durability")?.ViewComponent as XUiV_Sprite;
		lockTypeIcon = GetChildById("lockTypeIcon")?.ViewComponent as XUiV_Sprite;
		itemStack.count = 1;
		tweenScale = itemIcon?.UiTransform.gameObject.AddComponent<TweenScale>();
		base.ViewComponent.UseSelectionBox = false;
	}

	public XUiC_EquipmentStack()
	{
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.WindowGroup.isShowing)
		{
			PlayerActionsGUI gUIActions = xui.playerUI.playerInput.GUIActions;
			CursorControllerAbs cursorController = xui.playerUI.CursorController;
			Vector3 vector = cursorController.GetScreenPosition();
			bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
			bool mouseButtonDown = cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
			bool mouseButton = cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
			if (isOver && base.ViewComponent.UiTransformIsHovered && base.ViewComponent.EventOnPress)
			{
				if (gUIActions.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					bool wasReleased = gUIActions.Submit.WasReleased;
					bool wasPressed = gUIActions.Inspect.WasPressed;
					bool wasReleased2 = gUIActions.RightStick.WasReleased;
					if (xui.DragAndDropWindow.CurrentStack.IsEmpty() && !ItemStack.IsEmpty())
					{
						if (wasReleased)
						{
							SwapItem();
						}
						else if (wasReleased2)
						{
							HandleMoveToPreferredLocation();
							xui.PlayerEquipment.RefreshEquipment();
						}
						else if (wasPressed)
						{
							HandleItemInspect();
						}
						if (itemStack.IsEmpty())
						{
							SetBorderColor(normalBorderColor);
						}
					}
					else if (wasReleased)
					{
						HandleStackSwap();
					}
				}
				else if (InputUtils.ShiftKeyPressed)
				{
					if (mouseButtonUp)
					{
						HandleMoveToPreferredLocation();
						xui.PlayerEquipment.RefreshEquipment();
					}
				}
				else if (mouseButton)
				{
					if (xui.DragAndDropWindow.CurrentStack.IsEmpty() && !ItemStack.IsEmpty())
					{
						if (!lastClicked)
						{
							startMousePos = vector;
						}
						else if (Mathf.Abs((vector - startMousePos).magnitude) > (float)PickupSnapDistance)
						{
							SwapItem();
							xui.PlayerEquipment.RefreshEquipment();
						}
						SetBorderColor(normalBorderColor);
					}
					if (mouseButtonDown)
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
					else if (lastClicked)
					{
						HandleStackSwap();
						xui.PlayerEquipment.RefreshEquipment();
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
				if (isOver || itemIcon.UiTransform.localScale != Vector3.one)
				{
					if (tweenScale.value != Vector3.one && !itemStack.IsEmpty())
					{
						tweenScale.from = Vector3.one * 1.5f;
						tweenScale.to = Vector3.one;
						tweenScale.enabled = true;
						tweenScale.duration = 0.5f;
					}
					isOver = false;
				}
			}
		}
		if (!IsDirty)
		{
			return;
		}
		bool flag = !itemValue.IsEmpty();
		ItemClass itemClass = null;
		if (flag)
		{
			itemClass = ItemClass.GetForId(itemValue.type);
		}
		if (itemIcon != null)
		{
			itemIcon.SpriteName = (flag ? itemStack.itemValue.GetPropertyOverride("CustomIcon", itemClass.GetIconName()) : emptySpriteName);
			itemIcon.UIAtlas = (flag ? "ItemIconAtlas" : "ItemIconAtlasGreyscale");
			itemIcon.Color = (flag ? Color.white : new Color(1f, 1f, 1f, 0.7f));
			string text = string.Empty;
			if (flag)
			{
				text = itemClass.GetLocalizedItemName();
			}
			base.ViewComponent.ToolTip = (flag ? text : emptyTooltipName);
		}
		if (itemClass != null)
		{
			itemIcon.Color = itemStack.itemValue.ItemClass.GetIconTint(itemStack.itemValue);
			if (itemClass.ShowQualityBar)
			{
				if (durability != null)
				{
					durability.IsVisible = true;
					durabilityBackground.IsVisible = EntityPlayerLocal.PermaDegrationOn && itemStack.itemValue.MaxDurabilityModifier < 1f;
					if (durabilityRemoveBackground != null)
					{
						durabilityRemoveBackground.IsVisible = true;
						durabilityRemoveBackground.Fill = itemValue.MaxDurabilityModifier;
					}
					durability.Color = QualityInfo.GetQualityColor(itemValue.Quality);
					durability.Fill = ((itemValue.MaxUseTimes == 0) ? 1f : (((float)itemValue.MaxUseTimes - itemValue.UseTimes) / (float)itemValue.MaxUseTimesUI));
				}
				if (stackValue != null)
				{
					stackValue.IsVisible = true;
					stackValue.Alignment = NGUIText.Alignment.Center;
					stackValue.Text = ((itemStack.itemValue.Quality > 0) ? itemStack.itemValue.Quality.ToString() : (itemStack.itemValue.IsMod ? "*" : ""));
				}
			}
			else
			{
				if (durability != null)
				{
					durability.IsVisible = false;
					durabilityBackground.IsVisible = false;
				}
				if (stackValue != null)
				{
					stackValue.IsVisible = false;
				}
			}
			if (lockTypeIcon != null)
			{
				lockTypeIcon.SpriteName = (itemStack.itemValue.HasMods() ? "ui_game_symbol_modded" : "");
			}
		}
		else
		{
			if (durability != null)
			{
				durability.IsVisible = false;
			}
			if (durabilityBackground != null)
			{
				durabilityBackground.IsVisible = false;
			}
			if (durabilityRemoveBackground != null)
			{
				durabilityRemoveBackground.IsVisible = false;
			}
			if (stackValue != null)
			{
				stackValue.Text = "";
			}
			if (lockTypeIcon != null)
			{
				lockTypeIcon.SpriteName = "";
			}
		}
		IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleItemInspect()
	{
		if (!ItemStack.IsEmpty() && InfoWindow != null)
		{
			base.IsSelected = true;
			InfoWindow.SetItemStack(this, _makeVisible: true);
		}
		HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleStackSwap()
	{
		ItemClass itemClass = xui.DragAndDropWindow.CurrentStack.itemValue.ItemClass;
		if (itemClass != null)
		{
			if (itemClass is ItemClassArmor itemClassArmor && itemClassArmor.EquipSlot == EquipSlot)
			{
				SwapItem();
			}
			base.IsSelected = false;
			HandleClickComplete();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleClickComplete()
	{
		lastClicked = false;
		if (itemValue.IsEmpty())
		{
			SetBorderColor(normalBorderColor);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		isOver = _isOver;
		if (_isOver)
		{
			if (!base.IsSelected)
			{
				SetBorderColor(hoverBorderColor);
			}
			if (!itemStack.IsEmpty())
			{
				tweenScale.from = Vector3.one;
				tweenScale.to = Vector3.one * 1.5f;
				tweenScale.enabled = true;
				tweenScale.duration = 0.5f;
			}
		}
		else
		{
			if (!base.IsSelected)
			{
				SetBorderColor(normalBorderColor);
			}
			tweenScale.from = Vector3.one * 1.5f;
			tweenScale.to = Vector3.one;
			tweenScale.enabled = true;
			tweenScale.duration = 0.5f;
		}
		bool canSwap = false;
		ItemStack currentStack = xui.DragAndDropWindow.CurrentStack;
		if ((currentStack.IsEmpty() ? null : currentStack.itemValue.ItemClass) is ItemClassArmor itemClassArmor)
		{
			canSwap = equipSlot == itemClassArmor.EquipSlot;
		}
		xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver, canSwap);
		if (!_isOver && tweenScale.value != Vector3.one && !itemStack.IsEmpty())
		{
			tweenScale.from = Vector3.one * 1.5f;
			tweenScale.to = Vector3.one;
			tweenScale.enabled = true;
			tweenScale.duration = 0.5f;
		}
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwapItem()
	{
		ItemStack currentStack = xui.DragAndDropWindow.CurrentStack;
		if (AllowRemoveItem)
		{
			xui.DragAndDropWindow.CurrentStack = itemStack.Clone();
			xui.DragAndDropWindow.PickUpType = XUiC_ItemStack.StackLocationTypes.Equipment;
		}
		else
		{
			if (currentStack.IsEmpty())
			{
				return;
			}
			if (!itemStack.IsEmpty() && itemStack.itemValue.ItemClass is ItemClassArmor { ReplaceByTag: not null } itemClassArmor)
			{
				FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.Parse(itemClassArmor.ReplaceByTag);
				if (currentStack.itemValue.ItemClass is ItemClassArmor itemClassArmor2 && !fastTags.Test_AnySet(itemClassArmor2.ItemTags))
				{
					return;
				}
			}
			xui.DragAndDropWindow.CurrentStack = ItemStack.Empty;
		}
		ItemStack = currentStack.Clone();
		this.SlotChangedEvent?.Invoke(SlotNumber, itemStack);
	}

	public void HandleMoveToPreferredLocation()
	{
		if (!AllowRemoveItem)
		{
			return;
		}
		ItemStack itemStack = ItemStack.Clone();
		if (xui.PlayerInventory.AddItemToBackpack(itemStack))
		{
			ItemValue = ItemStack.Empty.itemValue.Clone();
			if (this.itemStack.IsEmpty() && base.IsSelected)
			{
				base.IsSelected = false;
			}
		}
		else if (xui.PlayerInventory.AddItemToToolbelt(itemStack))
		{
			ItemValue = ItemStack.Empty.itemValue.Clone();
			this.SlotChangedEvent?.Invoke(SlotNumber, this.itemStack);
			if (this.itemStack.IsEmpty() && base.IsSelected)
			{
				base.IsSelected = false;
			}
		}
		if (!itemStack.IsEmpty())
		{
			string soundPlace = itemStack.itemValue.ItemClass.SoundPlace;
			if (!string.IsNullOrEmpty(soundPlace))
			{
				Manager.PlayInsidePlayerHead(soundPlace);
			}
		}
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		switch (_name)
		{
		case "slot":
			if (!EnumUtils.TryParse<EquipmentSlots>(_value, out equipSlot, _ignoreCase: true))
			{
				Log.Error("[XUi] EquipmentStack slot '" + _value + "' unknown");
				equipSlot = EquipmentSlots.Count;
			}
			return true;
		case "allow_remove_item":
			AllowRemoveItem = StringParsers.ParseBool(_value);
			return true;
		case "selected_border_color":
			selectedBorderColor = StringParsers.ParseColor32(_value);
			return true;
		case "hover_border_color":
			hoverBorderColor = StringParsers.ParseColor32(_value);
			return true;
		case "normal_border_color":
			normalBorderColor = StringParsers.ParseColor32(_value);
			return true;
		case "hover_icon_grow":
			HoverIconGrow = StringParsers.ParseFloat(_value);
			return true;
		case "empty_sprite":
			emptySpriteName = _value;
			return true;
		case "empty_tooltip":
			emptyTooltipName = Localization.Get(_value);
			return true;
		default:
			return base.ParseAttribute(_name, _value);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
	}
}

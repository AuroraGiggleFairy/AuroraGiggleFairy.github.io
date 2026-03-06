using Audio;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_BasePartStack : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack itemStack = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isDirty = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bHighlightEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDropEnabled = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip pickupSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string slotType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 currentColor;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastClicked;

	public int PickupSnapDistance = 4;

	public static Color32 finalPressedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(128, 128, 128, byte.MaxValue);

	public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController stackValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController itemIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController durability;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController durabilityBackground;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController tintedOverlay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController highlightOverlay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController background;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController lblItemName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt qualityFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor partcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat partfillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Protected)]
	public string emptySpriteName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int SlotNumber { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack.StackLocationTypes StackLocation { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float HoverIconGrow
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public string SlotType
	{
		get
		{
			return slotType;
		}
		set
		{
			slotType = value;
			SetEmptySpriteName();
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
				itemStack = value;
				isDirty = true;
				if (itemStack.IsEmpty())
				{
					base.Selected = false;
				}
				if (base.Selected)
				{
					InfoWindow.SetItemStack(this, _makeVisible: true);
				}
				if (this.SlotChangedEvent != null)
				{
					this.SlotChangedEvent(SlotNumber, itemStack);
				}
				itemClass = itemStack.itemValue.ItemClass;
				RefreshBindings();
			}
		}
	}

	public ItemClass ItemClass => itemClass;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	public event XUiEvent_SlotChangedEventHandler SlotChangingEvent;

	public event XUiEvent_SlotChangedEventHandler SlotChangedEvent;

	public virtual string GetAtlas()
	{
		if (itemClass == null)
		{
			return "ItemIconAtlasGreyscale";
		}
		return "ItemIconAtlas";
	}

	public virtual string GetPartName()
	{
		if (itemClass == null)
		{
			return $"[MISSING {SlotType}]";
		}
		return itemClass.GetLocalizedItemName();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "partname":
			value = GetPartName();
			return true;
		case "partquality":
			value = ((itemClass != null && itemStack != null) ? qualityFormatter.Format(itemStack.itemValue.Quality) : "");
			return true;
		case "partatlas":
			value = GetAtlas();
			return true;
		case "particon":
			if (itemClass == null)
			{
				value = emptySpriteName;
			}
			else
			{
				value = itemStack.itemValue.GetPropertyOverride("CustomIcon", (itemClass.CustomIcon != null) ? itemClass.CustomIcon.Value : itemClass.GetIconName());
			}
			return true;
		case "particoncolor":
			if (itemClass == null)
			{
				value = "255, 255, 255, 178";
			}
			else
			{
				Color32 color = itemStack.itemValue.ItemClass.GetIconTint(itemStack.itemValue);
				value = $"{color.r},{color.g},{color.b},{color.a}";
			}
			return true;
		case "partcolor":
			if (itemClass != null)
			{
				Color32 v = QualityInfo.GetQualityColor(itemStack.itemValue.Quality);
				value = partcolorFormatter.Format(v);
			}
			else
			{
				value = "255, 255, 255, 0";
			}
			return true;
		case "partvisible":
			value = ((itemClass != null) ? "true" : "false");
			return true;
		case "emptyvisible":
			value = ((itemClass == null) ? "true" : "false");
			return true;
		case "partfill":
			value = ((itemStack.itemValue.MaxUseTimes == 0) ? "1" : partfillFormatter.Format(((float)itemStack.itemValue.MaxUseTimes - itemStack.itemValue.UseTimes) / (float)itemStack.itemValue.MaxUseTimes));
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		SetColor(isSelected ? selectColor : backgroundColor);
		((XUiV_Sprite)background.ViewComponent).SpriteName = (isSelected ? "ui_game_select_row" : "menu_empty");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetColor(Color32 color)
	{
		((XUiV_Sprite)background.ViewComponent).Color = color;
	}

	public override void Init()
	{
		base.Init();
		itemIcon = GetChildById("itemIcon");
		background = GetChildById("background");
		RefreshBindings();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!base.WindowGroup.isShowing)
		{
			return;
		}
		PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
		CursorControllerAbs cursorController = base.xui.playerUI.CursorController;
		Vector3 vector = cursorController.GetScreenPosition();
		bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
		bool mouseButtonDown = cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
		bool mouseButton = cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
		if (isOver && UICamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
		{
			if (gUIActions.LastInputType == BindingSourceType.DeviceBindingSource)
			{
				if (base.xui.dragAndDrop.CurrentStack.IsEmpty())
				{
					if (ItemStack.IsEmpty())
					{
						return;
					}
					if (gUIActions.Submit.WasReleased && CanRemove())
					{
						SwapItem();
						currentColor = backgroundColor;
						if (itemStack.IsEmpty())
						{
							((XUiV_Sprite)background.ViewComponent).Color = backgroundColor;
						}
					}
					else if (gUIActions.RightStick.WasReleased)
					{
						HandleMoveToPreferredLocation();
					}
					else if (gUIActions.Inspect.WasReleased)
					{
						HandleItemInspect();
					}
				}
				else if (gUIActions.Submit.WasReleased)
				{
					HandleStackSwap();
				}
			}
			else if (InputUtils.ShiftKeyPressed)
			{
				if (mouseButtonUp)
				{
					HandleMoveToPreferredLocation();
				}
			}
			else if (mouseButton)
			{
				if (base.xui.dragAndDrop.CurrentStack.IsEmpty() && !ItemStack.IsEmpty())
				{
					if (!lastClicked)
					{
						startMousePos = vector;
					}
					else if (CanRemove() && Mathf.Abs((vector - startMousePos).magnitude) > (float)PickupSnapDistance)
					{
						SwapItem();
						currentColor = backgroundColor;
						if (itemStack.IsEmpty())
						{
							((XUiV_Sprite)background.ViewComponent).Color = backgroundColor;
						}
					}
				}
				if (mouseButtonDown)
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
				else if (lastClicked)
				{
					HandleStackSwap();
				}
			}
			else
			{
				lastClicked = false;
			}
		}
		else
		{
			currentColor = backgroundColor;
			if (!base.Selected)
			{
				((XUiV_Sprite)background.ViewComponent).Color = currentColor;
			}
			lastClicked = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetEmptySpriteName()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleItemInspect()
	{
		if (!ItemStack.IsEmpty() && InfoWindow != null)
		{
			base.Selected = true;
			InfoWindow.SetItemStack(this, _makeVisible: true);
		}
		HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanSwap(ItemStack stack)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanRemove()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleStackSwap()
	{
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (!currentStack.IsEmpty())
		{
			if (CanSwap(currentStack))
			{
				SwapItem();
			}
			else
			{
				Manager.PlayInsidePlayerHead("ui_denied");
			}
		}
		else if (CanRemove())
		{
			SwapItem();
		}
		else
		{
			Manager.PlayInsidePlayerHead("ui_denied");
		}
		base.Selected = false;
		HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleClickComplete()
	{
		lastClicked = false;
		currentColor = backgroundColor;
		if (itemStack.IsEmpty())
		{
			((XUiV_Sprite)background.ViewComponent).Color = backgroundColor;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		isOver = _isOver;
		if (!base.Selected)
		{
			if (_isOver)
			{
				((XUiV_Sprite)background.ViewComponent).Color = highlightColor;
			}
			else
			{
				((XUiV_Sprite)background.ViewComponent).Color = backgroundColor;
			}
		}
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		bool canSwap = !currentStack.IsEmpty() && CanSwap(currentStack);
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver, canSwap, CanRemove());
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SwapItem()
	{
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (itemStack.IsEmpty())
		{
			if (placeSound != null)
			{
				Manager.PlayXUiSound(placeSound, 0.75f);
			}
		}
		else if (pickupSound != null)
		{
			Manager.PlayXUiSound(pickupSound, 0.75f);
		}
		if (this.SlotChangingEvent != null)
		{
			this.SlotChangingEvent(SlotNumber, itemStack);
		}
		base.xui.dragAndDrop.CurrentStack = itemStack.Clone();
		base.xui.dragAndDrop.PickUpType = StackLocation;
		ItemStack = currentStack.Clone();
	}

	public void HandleMoveToPreferredLocation()
	{
		if (ItemStack.IsEmpty() || !CanRemove())
		{
			return;
		}
		if (this.SlotChangingEvent != null)
		{
			this.SlotChangingEvent(SlotNumber, ItemStack);
		}
		if (base.xui.PlayerInventory.AddItemToBackpack(ItemStack))
		{
			if (base.xui.currentSelectedEntry == this)
			{
				base.xui.currentSelectedEntry.Selected = false;
				InfoWindow.SetItemStack((XUiC_ItemStack)null, false);
			}
			ItemStack = ItemStack.Empty.Clone();
			if (placeSound != null)
			{
				Manager.PlayXUiSound(placeSound, 0.75f);
			}
			if (this.SlotChangedEvent != null)
			{
				this.SlotChangedEvent(SlotNumber, itemStack);
			}
		}
		else if (base.xui.PlayerInventory.AddItemToToolbelt(ItemStack))
		{
			if (base.xui.currentSelectedEntry == this)
			{
				base.xui.currentSelectedEntry.Selected = false;
				InfoWindow.SetItemStack((XUiC_ItemStack)null, false);
			}
			ItemStack = ItemStack.Empty.Clone();
			if (placeSound != null)
			{
				Manager.PlayXUiSound(placeSound, 0.75f);
			}
			if (this.SlotChangedEvent != null)
			{
				this.SlotChangedEvent(SlotNumber, itemStack);
			}
		}
	}

	public void ClearSelectedInfoWindow()
	{
		if (base.Selected)
		{
			InfoWindow.SetItemStack((XUiC_ItemStack)null, true);
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			switch (name)
			{
			case "background_color":
				backgroundColor = StringParsers.ParseColor32(value);
				break;
			case "highlight_color":
				highlightColor = StringParsers.ParseColor32(value);
				break;
			case "pickup_snap_distance":
				PickupSnapDistance = int.Parse(value);
				break;
			case "hover_icon_grow":
				HoverIconGrow = StringParsers.ParseFloat(value);
				break;
			case "pickup_sound":
				base.xui.LoadData(value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip o) =>
				{
					pickupSound = o;
				});
				break;
			case "place_sound":
				base.xui.LoadData(value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip o) =>
				{
					placeSound = o;
				});
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}
}

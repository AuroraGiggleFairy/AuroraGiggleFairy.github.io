using Audio;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_EquipmentStack : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue = new ItemValue();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty = true;

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
	public Color32 normalBackgroundColor = new Color32(96, 96, 96, 96);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastClicked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string emptySpriteName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string emptyTooltipName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip pickupSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public EquipmentSlots equipSlot;

	public int PickupSnapDistance = 4;

	public static Color32 finalPressedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController timer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController stackValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController itemIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController durability;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController durabilityBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lockTypeIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController tintedOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController highlightOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController overlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController background;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblHeadgear;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblChestArmor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblGloves;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblFootwear;

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
			SetEmptySpriteNameAndTooltip();
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int SlotNumber { get; set; }

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
				if (value.IsEmpty() && base.Selected)
				{
					base.Selected = false;
					if (InfoWindow != null)
					{
						InfoWindow.SetItemStack((XUiC_EquipmentStack)null, true);
					}
				}
				if (this.SlotChangedEvent != null)
				{
					this.SlotChangedEvent(SlotNumber, itemStack);
				}
			}
			isDirty = true;
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
				isDirty = true;
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterFrameWindow FrameWindow { get; set; }

	public event XUiEvent_SlotChangedEventHandler SlotChangedEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		SetBorderColor(isSelected ? selectedBorderColor : normalBorderColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBorderColor(Color32 color)
	{
		((XUiV_Sprite)background.ViewComponent).Color = color;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetEmptySpriteNameAndTooltip()
	{
		switch (equipSlot)
		{
		case EquipmentSlots.Head:
			emptySpriteName = "apparelCowboyHat";
			emptyTooltipName = lblHeadgear;
			break;
		case EquipmentSlots.Chest:
			emptySpriteName = "armorSteelChest";
			emptyTooltipName = lblChestArmor;
			break;
		case EquipmentSlots.Hands:
			emptySpriteName = "armorLeatherGloves";
			emptyTooltipName = lblGloves;
			break;
		case EquipmentSlots.Feet:
			emptySpriteName = "apparelWornBoots";
			emptyTooltipName = lblFootwear;
			break;
		}
		if (emptyTooltipName != null)
		{
			emptyTooltipName = emptyTooltipName.ToUpper();
		}
	}

	public override void Init()
	{
		base.Init();
		stackValue = GetChildById("stackValue");
		background = GetChildById("background");
		SetBorderColor(normalBorderColor);
		itemIcon = GetChildById("itemIcon");
		durabilityBackground = GetChildById("durabilityBackground");
		durability = GetChildById("durability");
		tintedOverlay = GetChildById("tintedOverlay");
		highlightOverlay = GetChildById("highlightOverlay");
		lockTypeIcon = GetChildById("lockTypeIcon");
		overlay = GetChildById("overlay");
		itemStack.count = 1;
		tweenScale = itemIcon.ViewComponent.UiTransform.gameObject.AddComponent<TweenScale>();
		lblHeadgear = Localization.Get("lblHeadgear");
		lblChestArmor = Localization.Get("lblChestArmor");
		lblGloves = Localization.Get("lblGloves");
		lblFootwear = Localization.Get("lblFootwear");
		base.ViewComponent.UseSelectionBox = false;
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
			if (isOver && UICamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
			{
				if (gUIActions.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					bool wasReleased = gUIActions.Submit.WasReleased;
					bool wasReleased2 = gUIActions.Inspect.WasReleased;
					bool wasReleased3 = gUIActions.RightStick.WasReleased;
					if (base.xui.dragAndDrop.CurrentStack.IsEmpty() && !ItemStack.IsEmpty())
					{
						if (wasReleased)
						{
							SwapItem();
						}
						else if (wasReleased3)
						{
							HandleMoveToPreferredLocation();
							base.xui.PlayerEquipment.RefreshEquipment();
						}
						else if (wasReleased2)
						{
							HandleItemInspect();
						}
						if (itemStack.IsEmpty())
						{
							((XUiV_Sprite)background.ViewComponent).Color = backgroundColor;
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
						base.xui.PlayerEquipment.RefreshEquipment();
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
						else if (Mathf.Abs((vector - startMousePos).magnitude) > (float)PickupSnapDistance)
						{
							SwapItem();
							base.xui.PlayerEquipment.RefreshEquipment();
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
					if (base.xui.dragAndDrop.CurrentStack.IsEmpty())
					{
						HandleItemInspect();
					}
					else if (lastClicked)
					{
						HandleStackSwap();
						base.xui.PlayerEquipment.RefreshEquipment();
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
				if (isOver || itemIcon.ViewComponent.UiTransform.localScale != Vector3.one)
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
		if (isDirty)
		{
			bool flag = !itemValue.IsEmpty();
			ItemClass itemClass = null;
			if (flag)
			{
				itemClass = ItemClass.GetForId(itemValue.type);
			}
			if (itemIcon != null)
			{
				((XUiV_Sprite)itemIcon.ViewComponent).SpriteName = (flag ? itemStack.itemValue.GetPropertyOverride("CustomIcon", itemClass.GetIconName()) : emptySpriteName);
				((XUiV_Sprite)itemIcon.ViewComponent).UIAtlas = (flag ? "ItemIconAtlas" : "ItemIconAtlasGreyscale");
				((XUiV_Sprite)itemIcon.ViewComponent).Color = (flag ? Color.white : new Color(1f, 1f, 1f, 0.7f));
				string text = string.Empty;
				if (flag)
				{
					text = itemClass.GetLocalizedItemName();
				}
				base.ViewComponent.ToolTip = (flag ? text : emptyTooltipName);
			}
			if (itemClass != null)
			{
				((XUiV_Sprite)itemIcon.ViewComponent).Color = itemStack.itemValue.ItemClass.GetIconTint(itemStack.itemValue);
				if (itemClass.ShowQualityBar)
				{
					if (durability != null)
					{
						durability.ViewComponent.IsVisible = true;
						durabilityBackground.ViewComponent.IsVisible = true;
						XUiV_Sprite obj = (XUiV_Sprite)durability.ViewComponent;
						obj.Color = QualityInfo.GetQualityColor(itemValue.Quality);
						obj.Fill = itemValue.PercentUsesLeft;
					}
					if (stackValue != null)
					{
						XUiV_Label obj2 = (XUiV_Label)stackValue.ViewComponent;
						obj2.Alignment = NGUIText.Alignment.Center;
						obj2.Text = ((itemStack.itemValue.Quality > 0) ? itemStack.itemValue.Quality.ToString() : (itemStack.itemValue.IsMod ? "*" : ""));
					}
				}
				else if (durability != null)
				{
					durability.ViewComponent.IsVisible = false;
					durabilityBackground.ViewComponent.IsVisible = false;
				}
				if (lockTypeIcon != null)
				{
					if (itemStack.itemValue.HasMods())
					{
						(lockTypeIcon.ViewComponent as XUiV_Sprite).SpriteName = "ui_game_symbol_modded";
					}
					else
					{
						(lockTypeIcon.ViewComponent as XUiV_Sprite).SpriteName = "";
					}
				}
			}
			else
			{
				if (durability != null)
				{
					durability.ViewComponent.IsVisible = false;
				}
				if (durabilityBackground != null)
				{
					durabilityBackground.ViewComponent.IsVisible = false;
				}
				if (stackValue != null)
				{
					((XUiV_Label)stackValue.ViewComponent).Text = "";
				}
				if (lockTypeIcon != null)
				{
					(lockTypeIcon.ViewComponent as XUiV_Sprite).SpriteName = "";
				}
			}
			isDirty = false;
		}
		((XUiV_Label)stackValue.ViewComponent).Alignment = ((itemStack.itemValue.HasQuality || itemStack.itemValue.Modifications.Length != 0) ? NGUIText.Alignment.Center : NGUIText.Alignment.Right);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleItemInspect()
	{
		if (!ItemStack.IsEmpty() && InfoWindow != null)
		{
			base.Selected = true;
			InfoWindow.SetItemStack(this, _makeVisible: true);
		}
		HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleStackSwap()
	{
		ItemClass itemClass = base.xui.dragAndDrop.CurrentStack.itemValue.ItemClass;
		if (itemClass != null)
		{
			if (itemClass is ItemClassArmor itemClassArmor && itemClassArmor.EquipSlot == EquipSlot)
			{
				SwapItem();
			}
			base.Selected = false;
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
			if (!base.Selected)
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
			if (!base.Selected)
			{
				SetBorderColor(normalBorderColor);
			}
			tweenScale.from = Vector3.one * 1.5f;
			tweenScale.to = Vector3.one;
			tweenScale.enabled = true;
			tweenScale.duration = 0.5f;
		}
		bool canSwap = false;
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if ((currentStack.IsEmpty() ? null : currentStack.itemValue.ItemClass) is ItemClassArmor itemClassArmor)
		{
			canSwap = equipSlot == itemClassArmor.EquipSlot;
		}
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver, canSwap);
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
		base.xui.dragAndDrop.CurrentStack = itemStack.Clone();
		base.xui.dragAndDrop.PickUpType = XUiC_ItemStack.StackLocationTypes.Equipment;
		ItemStack = currentStack.Clone();
		if (this.SlotChangedEvent != null)
		{
			this.SlotChangedEvent(SlotNumber, itemStack);
		}
	}

	public void HandleMoveToPreferredLocation()
	{
		ItemStack itemStack = ItemStack.Clone();
		if (base.xui.PlayerInventory.AddItemToBackpack(itemStack))
		{
			ItemValue = ItemStack.Empty.itemValue.Clone();
			if (placeSound != null)
			{
				Manager.PlayXUiSound(placeSound, 0.75f);
			}
			if (this.itemStack.IsEmpty() && base.Selected)
			{
				base.Selected = false;
			}
		}
		else if (base.xui.PlayerInventory.AddItemToToolbelt(itemStack))
		{
			ItemValue = ItemStack.Empty.itemValue.Clone();
			if (placeSound != null)
			{
				Manager.PlayXUiSound(placeSound, 0.75f);
			}
			if (this.SlotChangedEvent != null)
			{
				this.SlotChangedEvent(SlotNumber, this.itemStack);
			}
			if (this.itemStack.IsEmpty() && base.Selected)
			{
				base.Selected = false;
			}
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			switch (name)
			{
			case "selected_border_color":
				selectedBorderColor = StringParsers.ParseColor32(value);
				break;
			case "hover_border_color":
				hoverBorderColor = StringParsers.ParseColor32(value);
				break;
			case "normal_border_color":
				normalBorderColor = StringParsers.ParseColor32(value);
				break;
			case "normal_background_color":
				finalPressedColor = StringParsers.ParseColor32(value);
				break;
			case "normal_color":
				normalBackgroundColor = StringParsers.ParseColor32(value);
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

	public override void OnOpen()
	{
		base.OnOpen();
		isDirty = true;
	}
}

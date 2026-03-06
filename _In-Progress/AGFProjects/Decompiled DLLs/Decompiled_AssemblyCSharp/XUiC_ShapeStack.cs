using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_ShapeStack : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHighlightEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDropEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip selectSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 currentColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastClicked;

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController tintedOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label stackValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite highlightOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	public int ShapeIndex = -1;

	public XUiC_ShapeStackGrid Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Block blockData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsLocked
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Block BlockData
	{
		get
		{
			return blockData;
		}
		set
		{
			if (blockData != value)
			{
				blockData = value;
				isDirty = true;
				if (blockData == null)
				{
					viewComponent.ToolTip = string.Empty;
					IsLocked = false;
				}
				else
				{
					viewComponent.ToolTip = ((blockData.GetAutoShapeType() != EAutoShapeType.None) ? blockData.GetLocalizedAutoShapeShapeName() : blockData.GetLocalizedBlockName());
				}
			}
			base.ViewComponent.Enabled = value != null;
			RefreshBindings();
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ShapeInfoWindow InfoWindow { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ShapeMaterialInfoWindow MaterialInfoWindow { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		if (isSelected)
		{
			SetColor(selectColor);
			if (base.xui.currentSelectedEntry == this)
			{
				InfoWindow?.SetShape(blockData);
				MaterialInfoWindow?.SetShape(blockData);
			}
		}
		else
		{
			SetColor(backgroundColor);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetColor(Color32 color)
	{
		background.Color = color;
	}

	public override void Init()
	{
		base.Init();
		tintedOverlay = GetChildById("tintedOverlay");
		highlightOverlay = GetChildById("highlightOverlay").ViewComponent as XUiV_Sprite;
		background = GetChildById("background").ViewComponent as XUiV_Sprite;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
		if (base.WindowGroup.isShowing)
		{
			CursorControllerAbs cursorController = base.xui.playerUI.CursorController;
			_ = (Vector3)cursorController.GetScreenPosition();
			bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButtonUp(UICamera.MouseButton.RightButton);
			cursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
			cursorController.GetMouseButton(UICamera.MouseButton.RightButton);
			if (isOver && UICamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
			{
				if (gUIActions.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					bool wasReleased = gUIActions.Submit.WasReleased;
					_ = gUIActions.HalfStack.WasReleased;
					_ = gUIActions.Inspect.WasReleased;
					bool wasReleased2 = gUIActions.RightStick.WasReleased;
					if (wasReleased && blockData != null)
					{
						SetSelectedShapeForItem();
						if (wasReleased2)
						{
							base.xui.playerUI.windowManager.Close("shapes");
						}
					}
				}
				else if (mouseButtonUp && blockData != null)
				{
					SetSelectedShapeForItem();
					if (InputUtils.ShiftKeyPressed)
					{
						base.xui.playerUI.windowManager.Close("shapes");
					}
				}
			}
			else
			{
				currentColor = backgroundColor;
				if (highlightOverlay != null)
				{
					highlightOverlay.Color = backgroundColor;
				}
				if (!base.Selected)
				{
					background.Color = currentColor;
				}
				lastClicked = false;
				if (isOver)
				{
					isOver = false;
				}
			}
		}
		if (isDirty)
		{
			isDirty = false;
		}
	}

	public static string GetFavoritesEntryName(Block _block)
	{
		if (_block == null)
		{
			return null;
		}
		if (_block.GetAutoShapeType() == EAutoShapeType.None)
		{
			return _block.GetBlockName();
		}
		return _block.GetAutoShapeShapeName();
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (!base.Selected || BlockData == null || base.xui.playerUI.playerInput == null)
		{
			return;
		}
		PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
		if ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && gUIActions.DPad_Right.WasReleased) || (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && gUIActions.Inspect.WasReleased))
		{
			EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
			string favoritesEntryName = GetFavoritesEntryName(BlockData);
			if (!entityPlayer.favoriteShapes.Remove(favoritesEntryName))
			{
				entityPlayer.favoriteShapes.Add(favoritesEntryName);
			}
			Owner.Owner.UpdateAll();
		}
	}

	public void SetSelectedShapeForItem()
	{
		if (!IsLocked)
		{
			Owner.Owner.ItemValue.Meta = ShapeIndex;
			Owner.Owner.RefreshItemStack();
		}
		base.Selected = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		isOver = _isOver;
		if (!base.Selected)
		{
			if (_isOver)
			{
				background.Color = highlightColor;
			}
			else
			{
				background.Color = backgroundColor;
			}
		}
		base.OnHovered(_isOver);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "islocked":
			value = IsLocked.ToString();
			return true;
		case "itemicon":
			value = ((BlockData == null) ? "" : BlockData.GetIconName());
			return true;
		case "itemicontint":
		{
			Color32 v = Color.white;
			if (BlockData != null)
			{
				v = BlockData.CustomIconTint;
			}
			value = itemicontintcolorFormatter.Format(v);
			return true;
		}
		case "isfavorite":
		{
			string favoritesEntryName = GetFavoritesEntryName(BlockData);
			value = (favoritesEntryName != null && base.xui.playerUI.entityPlayer.favoriteShapes.Contains(favoritesEntryName)).ToString();
			return true;
		}
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			switch (name)
			{
			case "select_color":
				selectColor = StringParsers.ParseColor32(value);
				break;
			case "press_color":
				pressColor = StringParsers.ParseColor32(value);
				break;
			case "background_color":
				backgroundColor = StringParsers.ParseColor32(value);
				break;
			case "highlight_color":
				highlightColor = StringParsers.ParseColor32(value);
				break;
			case "select_sound":
				base.xui.LoadData(value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip o) =>
				{
					selectSound = o;
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

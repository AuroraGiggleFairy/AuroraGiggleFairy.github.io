using PartyGroupPlus;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyGroupPlusColorRail : XUiController
{
	public const int CellSize = 36;
	public const int ArrowSize = 30;
	public const int OverlayPad = 12;
	public const int OverlayShiftX = 54;
	public const int TitleBand = 48;

	public static int GridWidth => PartyGroupPlusPalette.ShadeCount * CellSize;
	public static int GridHeight => PartyGroupPlusPalette.HueCount * CellSize;
	public static int OverlayWidth => OverlayPad * 2 + GridWidth;
	public static int OverlayHeight => TitleBand + GridHeight + OverlayPad;
	public static int OpenWidth => OverlayShiftX + OverlayWidth;
	public static int OpenHeight => OverlayHeight;

	public const int ClosedWidth = 48;
	public const int ClosedHeight = 100;

	XUiController button;
	XUiV_Sprite arrowSprite;
	XUiV_Sprite bgSprite;
	XUiV_Sprite borderSprite;
	float ignoreOutsideClickUntil;

	public override void Init()
	{
		base.Init();
		button = GetChildById("btnPartyColor");
		if (button != null)
		{
			button.OnPress += OnPressed;
		}

		XUiController arrow = GetChildById("myArrow");
		arrowSprite = arrow != null ? arrow.ViewComponent as XUiV_Sprite : null;
		XUiController bg = GetChildById("railBg");
		bgSprite = bg != null ? bg.ViewComponent as XUiV_Sprite : null;
		XUiController border = GetChildById("railBorder");
		borderSprite = border != null ? border.ViewComponent as XUiV_Sprite : null;
		ApplyExpand(XUiC_PartyGroupPlusColorOverlay.PickerOpen);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		PartyGroupPlusColorStore.Changed += OnStoreChanged;
		PartyGroupPlusColorStore.EnsureLocalDisplayColor();
		ApplyExpand(XUiC_PartyGroupPlusColorOverlay.PickerOpen);
		IsDirty = true;
	}

	public override void OnClose()
	{
		PartyGroupPlusColorStore.Changed -= OnStoreChanged;
		ApplyExpand(false);
		base.OnClose();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			ApplyArrowColor();
			IsDirty = false;
		}

		if (XUiC_PartyGroupPlusColorOverlay.PickerOpen
			&& Time.unscaledTime >= ignoreOutsideClickUntil
			&& Input.GetMouseButtonUp(0)
			&& !IsPointerOverOpenPicker())
		{
			XUiC_PartyGroupPlusColorOverlay.SetOpen(false);
		}
	}

	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "mycolor")
		{
			try
			{
				_value = PartyGroupPlusPalette.ToBinding(PartyGroupPlusColorStore.GetLocalDisplayColor());
			}
			catch
			{
				_value = PartyGroupPlusPalette.ToBinding(0);
			}

			return true;
		}

		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public void ApplyExpand(bool open)
	{
		int width = open ? OpenWidth : ClosedWidth;
		int height = open ? OpenHeight : ClosedHeight;
		SetSize(ViewComponent, width, height);
		SetSize(bgSprite, width, height);
		SetSize(borderSprite, width, height);
		if (borderSprite != null)
		{
			borderSprite.IsVisible = open;
		}

		XUiController overlay = GetChildById("partyGroupPlusColorOverlay");
		if (overlay?.ViewComponent != null)
		{
			SetSize(overlay.ViewComponent, OverlayWidth, OverlayHeight);
			XUiController grid = overlay.GetChildById("colorGrid");
			SetSize(grid?.ViewComponent, GridWidth, GridHeight);
		}

		if (ViewComponent != null)
		{
			ViewComponent.SetDirty();
		}
	}

	void OnPressed(XUiController _, int __)
	{
		XUiC_PartyGroupPlusColorOverlay overlay = GetChildByType<XUiC_PartyGroupPlusColorOverlay>()
			?? XUiC_PartyGroupPlusColorOverlay.Resolve(this);
		if (overlay != null)
		{
			overlay.ToggleVisible();
		}
		else
		{
			XUiC_PartyGroupPlusColorOverlay.Toggle();
		}

		if (XUiC_PartyGroupPlusColorOverlay.PickerOpen)
		{
			ignoreOutsideClickUntil = Time.unscaledTime + 0.15f;
		}
	}

	void OnStoreChanged()
	{
		IsDirty = true;
	}

	void ApplyArrowColor()
	{
		if (arrowSprite == null)
		{
			return;
		}

		arrowSprite.Color = PartyGroupPlusPalette.Get(PartyGroupPlusColorStore.GetLocalDisplayColor());
	}

	static void SetSize(XUiView view, int width, int height)
	{
		if (view == null)
		{
			return;
		}

		view.Width = width;
		view.Height = height;
		view.SetDirty();
	}

	bool IsPointerOverOpenPicker()
	{
		if (IsPointerOverPanel(this))
		{
			return true;
		}

		return ContainsMouse(ViewComponent);
	}

	bool ContainsMouse(XUiView view)
	{
		if (view?.uiTransform == null)
		{
			return false;
		}

		Camera camera = null;
		try
		{
			LocalPlayerUI playerUI = xui != null ? xui.playerUI : null;
			camera = playerUI != null ? playerUI.camera : null;
		}
		catch
		{
			return false;
		}

		if (camera == null)
		{
			return false;
		}
		Vector3 mouse = Input.mousePosition;
		mouse.z = view.uiTransform.position.z - camera.transform.position.z;
		Vector3 local = view.uiTransform.InverseTransformPoint(camera.ScreenToWorldPoint(mouse));
		return local.x >= 0f && local.x <= view.Width && local.y <= 0f && local.y >= -view.Height;
	}

	static bool IsPointerOverPanel(XUiController controller)
	{
		if (controller == null)
		{
			return false;
		}

		XUiView view = controller.ViewComponent;
		if (view != null && (view.IsHovered || view.UiTransformIsHovered))
		{
			return true;
		}

		for (int i = 0; i < controller.Children.Count; i++)
		{
			if (IsPointerOverPanel(controller.Children[i]))
			{
				return true;
			}
		}

		return false;
	}
}

[Preserve]
public class XUiC_PartyGroupPlusColorOverlay : XUiController
{
	public static bool PickerOpen;
	static XUiC_PartyGroupPlusColorOverlay Instance;

	public static XUiC_PartyGroupPlusColorOverlay Resolve(XUiController from)
	{
		if (Instance != null)
		{
			return Instance;
		}

		XUiController walk = from;
		while (walk != null)
		{
			XUiC_PartyGroupPlusColorOverlay found = walk.GetChildByType<XUiC_PartyGroupPlusColorOverlay>();
			if (found != null)
			{
				Instance = found;
				return found;
			}

			XUiController byId = walk.GetChildById("partyGroupPlusColorOverlay");
			if (byId is XUiC_PartyGroupPlusColorOverlay overlay)
			{
				Instance = overlay;
				return overlay;
			}

			walk = walk.Parent;
		}

		return null;
	}

	public static void SetOpen(bool open)
	{
		PickerOpen = open;
		if (Instance != null)
		{
			Instance.ApplyVisible();
			XUiC_PartyGroupPlusColorRail rail = Instance.Parent as XUiC_PartyGroupPlusColorRail;
			if (rail != null)
			{
				rail.ApplyExpand(open);
			}
		}
	}

	public static void Toggle()
	{
		SetOpen(!PickerOpen);
	}

	public void ToggleVisible()
	{
		Instance = this;
		SetOpen(!PickerOpen);
	}

	public override void Init()
	{
		base.Init();
		Instance = this;
		ApplyVisible();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		Instance = this;
		PartyGroupPlusColorStore.EnsureLocalDisplayColor();
		PartyGroupPlusColorStore.RequestSync();
		ApplyVisible();
	}

	public override void OnClose()
	{
		if (Instance == this)
		{
			Instance = null;
		}

		PickerOpen = false;
		base.OnClose();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (PickerOpen && Input.GetKeyDown(KeyCode.Escape))
		{
			SetOpen(false);
		}
	}

	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "pickeropen")
		{
			_value = PickerOpen ? "true" : "false";
			return true;
		}

		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	void ApplyVisible()
	{
		if (ViewComponent != null)
		{
			ViewComponent.IsVisible = PickerOpen;
		}

		RefreshBindings();
	}
}

[Preserve]
public class XUiC_PartyGroupPlusColorGrid : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_PartyGroupPlusColorChoice[] cells = GetChildrenByType<XUiC_PartyGroupPlusColorChoice>();
		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].ColorIndex = i;
			cells[i].ApplySwatchVisual();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		PartyGroupPlusColorStore.EnsureLocalDisplayColor();
		PartyGroupPlusColorStore.RequestSync();
	}
}

[Preserve]
public class XUiC_PartyGroupPlusColorChoice : XUiController
{
	public int ColorIndex;
	XUiController button;

	public override void Init()
	{
		base.Init();
		button = GetChildById("btnColor") ?? (XUiController)GetChildByType<XUiC_Button>();
		if (button != null)
		{
			button.OnPress += OnPressed;
		}

		PartyGroupPlusColorStore.Changed += OnStoreChanged;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ApplySwatchVisual();
		RefreshBindings();
	}

	public void ApplySwatchVisual()
	{
		Color32 color = PartyGroupPlusPalette.Get(ColorIndex);
		if (button?.ViewComponent is XUiV_Button view)
		{
			view.Width = XUiC_PartyGroupPlusColorRail.ArrowSize;
			view.Height = XUiC_PartyGroupPlusColorRail.ArrowSize;
			view.HoverScale = 1f;
			view.DefaultSpriteColor = color;
			view.HoverSpriteColor = color;
			view.SelectedSpriteColor = color;
			view.CurrentColor = color;
			view.SetDirty();
		}
	}

	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		try
		{
			EntityPlayerLocal local = SafeLocal(xui);
			Party party = local != null ? local.Party : null;
			int exceptId = local != null ? local.entityId : -1;
			bool taken = PartyGroupPlusColorStore.IsTakenInParty(party, ColorIndex, exceptId);
			bool mine = local != null && PartyGroupPlusColorStore.TryGet(local.entityId, out int mineIndex)
				&& mineIndex == ColorIndex;

			switch (_bindingName)
			{
				case "swatchcolor":
					_value = PartyGroupPlusPalette.ToBinding(ColorIndex);
					return true;
				case "taken":
					_value = (taken && !mine) ? "true" : "false";
					return true;
				case "mine":
					_value = mine ? "true" : "false";
					return true;
				case "swatchtooltip":
					if (mine)
					{
						_value = Localization.Get("xuiPartyGroupPlusColorYours");
					}
					else if (taken)
					{
						_value = string.Format(
							Localization.Get("xuiPartyGroupPlusColorTaken"),
							PartyGroupPlusColorStore.TakenByName(party, ColorIndex));
					}
					else
					{
						_value = Localization.Get("xuiPartyGroupPlusColorPick");
					}

					return true;
				default:
					return base.GetBindingValueInternal(ref _value, _bindingName);
			}
		}
		catch
		{
			if (_bindingName == "mine" || _bindingName == "taken")
			{
				_value = "false";
				return true;
			}

			if (_bindingName == "swatchcolor")
			{
				_value = PartyGroupPlusPalette.ToBinding(ColorIndex);
				return true;
			}

			if (_bindingName == "swatchtooltip")
			{
				_value = string.Empty;
				return true;
			}

			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public static EntityPlayerLocal SafeLocal(XUi xui)
	{
		try
		{
			if (xui == null)
			{
				return null;
			}

			LocalPlayerUI playerUI = xui.playerUI;
			return playerUI != null ? playerUI.entityPlayer : null;
		}
		catch
		{
			return null;
		}
	}

	void OnPressed(XUiController _, int __)
	{
		EntityPlayerLocal local = SafeLocal(xui);
		if (local != null && local.Party != null
			&& PartyGroupPlusColorStore.IsTakenInParty(local.Party, ColorIndex, local.entityId))
		{
			return;
		}

		PartyGroupPlusColorStore.RequestPick(ColorIndex);
	}

	void OnStoreChanged()
	{
		try
		{
			ApplySwatchVisual();
			RefreshBindings();
		}
		catch
		{
		}
	}
}

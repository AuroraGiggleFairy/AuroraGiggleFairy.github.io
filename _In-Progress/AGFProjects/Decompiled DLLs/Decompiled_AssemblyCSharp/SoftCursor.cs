using System;
using System.Collections;
using Audio;
using InControl;
using Platform;
using UnityEngine;

public class SoftCursor : CursorControllerAbs
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float BaseSpeed = 500f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float SpeedModRange = 2000f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string emptyCursorAtlasName = "UIAtlas";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string emptyCursorSprite = "";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly UIWidget.Pivot emptyCursorPivot = UIWidget.Pivot.Center;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static UIAtlas emptyCursorAtlas;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string defaultControllerCursorAtlasName = "UIAtlas";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string defaultControllerCursorSprite = "soft_cursor";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly UIWidget.Pivot defaultControllerCursorPivot = UIWidget.Pivot.Center;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static UIAtlas defaultControllerCursorAtlas;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string defaultMouseCursorAtlasName = "UIAtlas";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string defaultMouseCursorSprite = "cursor01";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly UIWidget.Pivot defaultMouseCursorPivot = UIWidget.Pivot.TopLeft;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static UIAtlas defaultMouseCursorAtlas;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string mapCursorAtlasName = "UIAtlas";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string mapCursorSprite = "map_cursor";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly UIWidget.Pivot mapCursorPivot = UIWidget.Pivot.Center;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static UIAtlas mapCursorAtlas;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float speed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float mouseSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float LastFrameTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hidden;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool snapped;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float OffsetSnapBounds = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public UIPanel cursorPanel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 lastMousePosition = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float currentAcceleration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float speedMultiplier = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool movingMouse;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite selectionBox;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int selectionBoxMargin = 10;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public PlayerInputManager.InputStyle m_lastInputStyle = PlayerInputManager.InputStyle.Count;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_softcursorEnabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool cursorModeActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public ECursorType currentCursorType = ECursorType.Default;

	public PlayerInputManager.InputStyle LastInputStyle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_lastInputStyle;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (value != m_lastInputStyle)
			{
				m_lastInputStyle = value;
				SetVisible(SoftcursorAllowed);
			}
		}
	}

	public bool SoftcursorEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return m_softcursorEnabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != m_softcursorEnabled)
			{
				m_softcursorEnabled = value;
				SetVisible(SoftcursorAllowed);
			}
		}
	}

	public override XUiView HoverTarget
	{
		get
		{
			return base.HoverTarget;
		}
		set
		{
			base.HoverTarget = value;
			if (hoverTarget != null && LastInputStyle != PlayerInputManager.InputStyle.Keyboard && !movingMouse && hoverTarget.IsNavigatable && !hoverTarget.HasHoverSound)
			{
				Manager.PlayXUiSound(cursorSelectSound, 0.75f);
			}
		}
	}

	public override bool CursorModeActive => cursorModeActive;

	public bool SoftcursorAllowed
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (guiActions != null && guiActions.Enabled)
			{
				return LocalPlayerUI.AnyModalWindowOpen();
			}
			return false;
		}
	}

	public Vector3 Position
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return cursor.transform.position;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			cursor.transform.position = value;
			lastMousePosition = Input.mousePosition;
		}
	}

	public Vector3 LocalPosition
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return cursor.transform.localPosition;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			cursor.transform.localPosition = value;
			lastMousePosition = Input.mousePosition;
		}
	}

	public static CursorLockMode DefaultCursorLockState => CursorLockMode.None;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		AwakeBase();
		Cursor.lockState = DefaultCursorLockState;
		UISprite[] componentsInChildren = GetComponentsInChildren<UISprite>();
		foreach (UISprite uISprite in componentsInChildren)
		{
			string text = uISprite.gameObject.name;
			if (text == "Cursor")
			{
				cursor = uISprite;
			}
			else if (text == "SelectionBox")
			{
				selectionBox = uISprite;
			}
		}
		cursorPanel = GetComponent<UIPanel>();
		GameObject obj = new GameObject("cursorMouse");
		obj.transform.parent = base.gameObject.transform;
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		if (defaultMouseCursorAtlas == null)
		{
			UIAtlas[] array = Resources.FindObjectsOfTypeAll<UIAtlas>();
			foreach (UIAtlas uIAtlas in array)
			{
				if (uIAtlas.name.EqualsCaseInsensitive(defaultMouseCursorAtlasName))
				{
					defaultMouseCursorAtlas = uIAtlas;
				}
				if (uIAtlas.name.EqualsCaseInsensitive(emptyCursorAtlasName))
				{
					emptyCursorAtlas = uIAtlas;
				}
				if (uIAtlas.name.EqualsCaseInsensitive(defaultControllerCursorAtlasName))
				{
					defaultControllerCursorAtlas = uIAtlas;
				}
				if (uIAtlas.name.EqualsCaseInsensitive(mapCursorAtlasName))
				{
					mapCursorAtlas = uIAtlas;
				}
			}
		}
		LocalPlayerUI.primaryUI.xui.LoadData("@:Sounds/UI/ui_hover.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			cursorSelectSound = _o;
		});
		LocalPlayerUI.primaryUI.xui.LoadData("@:Sounds/UI/ui_tab.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			pagingSound = _o;
		});
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += OnLastInputStyleChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		if (_style != PlayerInputManager.InputStyle.Keyboard)
		{
			if (LastInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				LocalPosition = cursorPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(uiCamera.cachedCamera.ScreenToWorldPoint(Input.mousePosition));
			}
			RefreshSelection();
		}
		SetVisible(SoftcursorAllowed);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		uiCamera = GetComponentInParent<UICamera>();
		UpdateMoveSpeed();
		InitCursorBounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		DestroyBase();
	}

	public override void UpdateMoveSpeed()
	{
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsInterfaceSensitivity);
		speed = 500f + 2000f * num;
		mouseSpeed = (500f + 2000f * num) / 1000f * (1f / MouseBindingSource.ScaleX) * 4f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!Application.isPlaying || GameManager.Instance.IsQuitting || GameManager.Instance.m_GUIConsole.isShowing)
		{
			return;
		}
		LastInputStyle = PlatformManager.NativePlatform.Input.CurrentInputStyle;
		if (guiActions != null && windowManager != null)
		{
			if (SoftcursorAllowed != SoftcursorEnabled)
			{
				SoftcursorEnabled = SoftcursorAllowed & !hidden;
			}
			if (SoftcursorEnabled)
			{
				HandleMovement();
			}
		}
		LastFrameTime = Time.realtimeSinceStartup;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (selectionBox.enabled)
		{
			RefreshSelection();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleMovement()
	{
		if (guiActions == null || base.Locked || hidden)
		{
			return;
		}
		movingMouse = false;
		Vector2 vector = Input.mousePosition;
		if (vector != lastMousePosition)
		{
			movingMouse = true;
		}
		lastMousePosition = vector;
		if (LastInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			if (movingMouse)
			{
				LocalPosition = cursorPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(uiCamera.cachedCamera.ScreenToWorldPoint(Input.mousePosition));
				SetVisible(_visible: true);
			}
			else
			{
				if (!CursorControllerAbs.FreeCursorEnabled)
				{
					return;
				}
				Vector2 vector2 = new Vector2(guiActions.Right.RawValue - guiActions.Left.RawValue, guiActions.Up.RawValue - guiActions.Down.RawValue);
				float magnitude = vector2.magnitude;
				if (magnitude > 0f)
				{
					cursorModeActive = true;
					SetVisible(_visible: true);
					SetNavigationTarget(null);
				}
				Vector3 localPosition = LocalPosition;
				Vector3 localPosition2 = LocalPosition;
				if (bHasHoverTarget && (HoverTarget == null || !HoverTarget.ColliderEnabled || !HoverTarget.UiTransform.gameObject.activeInHierarchy))
				{
					HoverTarget = null;
				}
				float b = (bHasHoverTarget ? CursorControllerAbs.hoverSpeed : 1f);
				currentAcceleration = Mathf.Clamp(currentAcceleration + magnitude * Time.unscaledDeltaTime, 0f, Mathf.Min(magnitude, b));
				speedMultiplier = Mathf.MoveTowards(speedMultiplier, bHasHoverTarget ? CursorControllerAbs.hoverSpeed : 1f, Time.unscaledDeltaTime * (bHasHoverTarget ? 10f : 1f));
				float num = Time.unscaledDeltaTime * speed * speedMultiplier * accelerationCurve.Evaluate(currentAcceleration);
				vector2.x *= num;
				vector2.y *= num;
				localPosition2.x += vector2.x;
				localPosition2.y += vector2.y;
				LocalPosition = localPosition2;
				if (CursorControllerAbs.bSnapCursor && localPosition == localPosition2)
				{
					if (!snapped)
					{
						Snap();
						snapped = true;
					}
				}
				else
				{
					snapped = false;
				}
			}
			ConstrainCursor();
		}
		else
		{
			LocalPosition = cursorPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(uiCamera.cachedCamera.ScreenToWorldPoint(Input.mousePosition));
			ConstrainCursor();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ConstrainToBounds(Vector3 _newPosition)
	{
		Vector3 vector = _newPosition;
		if (LastInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			vector.z = currentBounds.center.z;
			vector = currentBounds.ClosestPoint(vector);
		}
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConstrainCursor()
	{
		Vector3 newPosition = uiCamera.cachedCamera.WorldToScreenPoint(Position);
		Vector3 position = ConstrainToBounds(newPosition);
		position = uiCamera.cachedCamera.ScreenToViewportPoint(position);
		if (position.x < 0f)
		{
			position.x = 0f;
		}
		else if (position.x > 1f)
		{
			position.x = 1f;
		}
		if (position.y < 0f)
		{
			position.y = 0f;
		}
		else if (position.y > 1f)
		{
			position.y = 1f;
		}
		Position = uiCamera.cachedCamera.ViewportToWorldPoint(position);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Snap()
	{
		if (!GamePrefs.GetBool(EnumGamePrefs.OptionsControllerCursorSnap) || hoverTarget == null || !hoverTarget.IsSnappable || !hoverTarget.ColliderEnabled || !hoverTarget.UiTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		Bounds bounds = hoverTarget.bounds;
		if (cursorWorldBounds.extents.x > bounds.extents.x - OffsetSnapBounds)
		{
			Position = bounds.center;
			return;
		}
		Vector3 vector = bounds.ClosestPoint(Position);
		Vector3 vector2 = Vector3.right * cursorWorldBounds.extents.x;
		Vector3 point = vector - vector2;
		Vector3 point2 = vector + vector2;
		if (!bounds.Contains(point))
		{
			vector = bounds.ClosestPoint(point) + vector2;
		}
		else if (!bounds.Contains(point2))
		{
			vector = bounds.ClosestPoint(point2) - vector2;
		}
		vector.y = bounds.center.y;
		Position = vector;
	}

	public override Vector2 GetScreenPosition()
	{
		Vector2 result = default(Vector2);
		if (uiCamera != null)
		{
			return uiCamera.cachedCamera.WorldToScreenPoint(Position);
		}
		return result;
	}

	public override Vector2 GetLocalScreenPosition()
	{
		Vector2 result = default(Vector2);
		if (uiCamera != null)
		{
			result = uiCamera.cachedCamera.WorldToViewportPoint(Position);
			result.x *= uiCamera.cachedCamera.pixelWidth;
			result.y *= uiCamera.cachedCamera.pixelHeight;
		}
		return result;
	}

	public override void SetScreenPosition(Vector2 _newPosition)
	{
		Position = uiCamera.cachedCamera.ScreenToWorldPoint(_newPosition);
	}

	public override void SetScreenPosition(float _x, float _y)
	{
		SetScreenPosition(new Vector2(_x, _y));
	}

	public override void ResetToCenter()
	{
		LocalPosition = Vector3.zero;
	}

	public override void SetCursorHidden(bool _hidden)
	{
		hidden = _hidden;
		SoftcursorEnabled &= !hidden;
		SetCursor((!_hidden) ? ECursorType.Default : ECursorType.None);
		Cursor.visible = PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && !hidden;
		if (LastInputStyle != PlayerInputManager.InputStyle.Keyboard && !hidden)
		{
			RefreshSelection();
		}
	}

	public override bool GetCursorHidden()
	{
		return hidden;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCursorSprite(UIAtlas _atlasMouse, string _spriteMouse, UIWidget.Pivot _pivotMouse, UIAtlas _atlasController, string _spriteController, UIWidget.Pivot _pivotController)
	{
		cursor.atlas = _atlasController;
		cursor.spriteName = _spriteController;
		cursor.pivot = _pivotController;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCursorSpriteNone()
	{
		cursor.atlas = null;
		cursor.spriteName = "";
		SetSelectionBoxEnabled(_enabled: false);
	}

	public new static void SetCursor(ECursorType _cursorType)
	{
		switch (_cursorType)
		{
		case ECursorType.None:
		{
			for (int j = 0; j < CursorControllerAbs.softCursors.Count; j++)
			{
				(CursorControllerAbs.softCursors[j] as SoftCursor).SetCursorSpriteNone();
			}
			break;
		}
		case ECursorType.Default:
		{
			for (int k = 0; k < CursorControllerAbs.softCursors.Count; k++)
			{
				(CursorControllerAbs.softCursors[k] as SoftCursor).SetCursorSprite(defaultMouseCursorAtlas, defaultMouseCursorSprite, defaultMouseCursorPivot, defaultControllerCursorAtlas, defaultControllerCursorSprite, defaultControllerCursorPivot);
			}
			break;
		}
		case ECursorType.Map:
		{
			for (int i = 0; i < CursorControllerAbs.softCursors.Count; i++)
			{
				(CursorControllerAbs.softCursors[i] as SoftCursor).SetCursorSprite(mapCursorAtlas, mapCursorSprite, mapCursorPivot, mapCursorAtlas, mapCursorSprite, mapCursorPivot);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVisible(bool _visible)
	{
		Cursor.lockState = ((!_visible) ? CursorLockMode.Locked : DefaultCursorLockState);
		if (LastInputStyle == PlayerInputManager.InputStyle.Keyboard || movingMouse)
		{
			Cursor.visible = _visible;
			cursor.enabled = false;
			SetSelectionBoxEnabled(_enabled: false);
		}
		else
		{
			Cursor.visible = false;
			cursor.enabled = _visible && CursorControllerAbs.FreeCursorEnabled && !base.Locked && cursorModeActive && !base.VirtualCursorHidden;
			SetSelectionBoxEnabled(_visible && !cursorModeActive && !hidden && base.navigationTarget != null && base.navigationTarget.UseSelectionBox);
		}
		GameManager.Instance.bCursorVisible = _visible;
		lastMousePosition = Input.mousePosition;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnVirtualCursorVisibleChanged()
	{
		if (LastInputStyle != PlayerInputManager.InputStyle.Keyboard && !movingMouse)
		{
			cursor.enabled = !base.VirtualCursorHidden && !base.Locked && cursorModeActive;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectionBoxEnabled(bool _enabled)
	{
		selectionBox.enabled = _enabled;
	}

	public static void SetCursorVisible(bool _visible)
	{
		for (int i = 0; i < CursorControllerAbs.softCursors.Count; i++)
		{
			(CursorControllerAbs.softCursors[i] as SoftCursor).SetVisible(_visible);
		}
	}

	public Vector2 GetFlatPosition()
	{
		return new Vector2(Position.x, Position.y);
	}

	public override void SetNavigationTarget(XUiView _view)
	{
		if (_view != null && !_view.IsNavigatable)
		{
			SetNavigationTarget(null);
		}
		else if (_view != base.navigationTarget && (base.lockNavigationToView == null || (_view != null && _view.Controller.IsChildOf(base.lockNavigationToView.Controller))))
		{
			if (base.navigationTarget != null)
			{
				base.navigationTarget.Controller.OnCursorUnSelected();
			}
			if (_view != null && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				_view.Controller.OnCursorSelected();
				cursorModeActive = false;
				SetSelectionBoxEnabled(_view.UseSelectionBox);
				PositionSelectionBox(_view);
			}
			else
			{
				SetSelectionBoxEnabled(_enabled: false);
			}
			base.navigationTarget = _view;
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				SetVisible(base.navigationTarget != null);
			}
		}
	}

	public override void RefreshSelection()
	{
		if (base.navigationTarget != null)
		{
			PositionSelectionBox(base.navigationTarget);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PositionSelectionBox(XUiView _view)
	{
		Position = _view.Center;
		if (_view.UseSelectionBox)
		{
			selectionBox.transform.position = _view.Center;
			if (_view.Controller.GetParentWindow().IsInStackpanel)
			{
				selectionBox.width = (int)((float)_view.Size.x * _view.xui.transform.localScale.x * _view.xui.StackPanelTransform.localScale.x + (float)selectionBoxMargin);
				selectionBox.height = (int)((float)_view.Size.y * _view.xui.transform.localScale.y * _view.xui.StackPanelTransform.localScale.y + (float)selectionBoxMargin);
			}
			else
			{
				selectionBox.width = (int)((float)_view.Size.x * _view.xui.transform.localScale.x + (float)selectionBoxMargin);
				selectionBox.height = (int)((float)_view.Size.y * _view.xui.transform.localScale.y + (float)selectionBoxMargin);
			}
		}
	}

	public override void SetNavigationLockView(XUiView _view, XUiView _viewToSelect = null)
	{
		if (_view != null && (!_view.IsVisible || !_view.UiTransform.gameObject.activeInHierarchy))
		{
			SetNavigationLockView(null);
			return;
		}
		base.lockNavigationToView = _view;
		if (_viewToSelect != null)
		{
			_viewToSelect.Controller.SelectCursorElement(_withDelay: true);
		}
		else
		{
			_view?.Controller.SelectCursorElement(_withDelay: true);
		}
	}

	public override void SetNavigationTargetLater(XUiView _view)
	{
		if (_view == null)
		{
			SetNavigationTarget(null);
		}
		else
		{
			StartCoroutine(SetNavigationTargetWithDelay(_view));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SetNavigationTargetWithDelay(XUiView _view)
	{
		base.Locked = true;
		for (int i = 0; i < 3; i++)
		{
			yield return null;
		}
		base.Locked = false;
		if (_view != null && _view.HasCollider)
		{
			SetNavigationTarget(_view);
		}
		lastMousePosition = Input.mousePosition;
	}

	public override void ResetNavigationTarget()
	{
		if (base.navigationTarget != null)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && base.navigationTarget.HasCollider)
			{
				Position = base.navigationTarget.Center;
			}
			else
			{
				SetNavigationTarget(null);
			}
		}
	}
}

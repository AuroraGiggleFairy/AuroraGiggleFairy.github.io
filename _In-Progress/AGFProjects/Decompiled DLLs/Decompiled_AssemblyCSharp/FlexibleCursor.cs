using System;
using System.Collections;
using Platform;
using UnityEngine;

public class FlexibleCursor : CursorControllerAbs
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float BaseSpeed = 500f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float SpeedModRange = 1000f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string defaultMouseCursorResource = "@:Textures/UI/cursor01.tga";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector2 defaultMouseCursorCenter = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string defaultControllerCursorResource = "@:Textures/UI/soft_cursor";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector2 defaultControllerCursorCenter = new Vector2(16f, 16f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string mapCursorResource = "@:Textures/UI/map_cursor.tga";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector2 mapCursorCenter = new Vector2(16f, 16f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D emptyCursor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D defaultControllerCursor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D defaultMouseCursor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D mapCursor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D currentCursorTexture;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 currentCursorHotspot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static ECursorType currentCursorType = ECursorType.Default;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Coroutine cursorUpdateCo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float speed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float speedMultiplier = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float LastFrameTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool snapped;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentAcceleration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float OffsetSnapBounds = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle m_lastInputStyle = PlayerInputManager.InputStyle.Count;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int CONTROLLER_CURSOR_MOVEMENT_LIMIT = 5;

	public bool SoftcursorAllowed
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (guiActions != null && guiActions.Enabled)
			{
				return LocalPlayerUI.AnyModalWindowOpen();
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		AwakeBase();
		speedMultiplier = speed;
		cursor = GetComponentInChildren<UISprite>();
		if (defaultMouseCursor == null)
		{
			emptyCursor = new Texture2D(32, 32, TextureFormat.ARGB32, mipChain: false);
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 32; j++)
				{
					emptyCursor.SetPixel(i, j, new Color(0f, 0f, 0f, 0.01f));
				}
			}
			emptyCursor.Apply();
			defaultMouseCursor = Resources.Load<Texture2D>(defaultMouseCursorResource);
			defaultControllerCursor = Resources.Load<Texture2D>(defaultControllerCursorResource);
			mapCursor = Resources.Load<Texture2D>(mapCursorResource);
		}
		UISprite[] componentsInChildren = GetComponentsInChildren<UISprite>(includeInactive: true);
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			componentsInChildren[k].gameObject.SetActive(value: false);
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += OnLastInputStyleChanged;
		SetCursor(currentCursorType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		SetCursor(currentCursorType);
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
		if (PlatformManager.NativePlatform?.Input != null)
		{
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= OnLastInputStyleChanged;
		}
		DestroyBase();
	}

	public override void UpdateMoveSpeed()
	{
		CursorControllerAbs.regularSpeed = GamePrefs.GetFloat(EnumGamePrefs.OptionsInterfaceSensitivity);
		speed = 500f + 1000f * CursorControllerAbs.regularSpeed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Application.isPlaying)
		{
			if (PlatformManager.NativePlatform?.Input != null && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && SoftcursorAllowed)
			{
				HandleControllerInput();
			}
			LastFrameTime = Time.realtimeSinceStartup;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleControllerInput()
	{
		if (guiActions == null)
		{
			return;
		}
		Vector2 vector = new Vector2(guiActions.Right.RawValue - guiActions.Left.RawValue, guiActions.Up.RawValue - guiActions.Down.RawValue);
		float magnitude = vector.magnitude;
		Vector3 vector2 = GetScreenPosition();
		Vector3 vector3 = vector2;
		if (bHasHoverTarget && (HoverTarget == null || HoverTarget.ColliderEnabled || !HoverTarget.UiTransform.gameObject.activeInHierarchy))
		{
			HoverTarget = null;
		}
		float b = (bHasHoverTarget ? CursorControllerAbs.hoverSpeed : 1f);
		currentAcceleration = Mathf.Clamp(currentAcceleration + magnitude * Time.unscaledDeltaTime, 0f, Mathf.Min(magnitude, b));
		speedMultiplier = Mathf.MoveTowards(speedMultiplier, bHasHoverTarget ? CursorControllerAbs.hoverSpeed : 1f, Time.unscaledDeltaTime * (bHasHoverTarget ? 10f : 1f));
		float num = Time.unscaledDeltaTime * speed * speedMultiplier * accelerationCurve.Evaluate(currentAcceleration);
		vector.x *= num;
		vector.y *= num;
		vector3.x += vector.x;
		vector3.y += vector.y;
		if (CursorControllerAbs.bSnapCursor)
		{
			if (vector2 == vector3)
			{
				if (!snapped)
				{
					vector3 = SnapOs(vector3);
					snapped = true;
				}
			}
			else
			{
				snapped = false;
			}
		}
		vector3 = ConstrainCursorOs(vector3);
		SetScreenPosition(vector3.x, vector3.y);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 SnapOs(Vector3 _newPos)
	{
		if (hoverTarget != null && hoverTarget.UiTransform.gameObject.activeInHierarchy)
		{
			if (cursorWorldBounds.extents.x > hoverTarget.bounds.extents.x - OffsetSnapBounds)
			{
				return uiCamera.cachedCamera.WorldToScreenPoint(hoverTarget.bounds.center);
			}
			Vector3 vector = hoverTarget.bounds.ClosestPoint(uiCamera.cachedCamera.ScreenToWorldPoint(_newPos));
			Vector3 vector2 = Vector3.right * cursorWorldBounds.extents.x;
			Vector3 point = vector - vector2;
			Vector3 point2 = vector + vector2;
			if (!hoverTarget.bounds.Contains(point))
			{
				vector = hoverTarget.bounds.ClosestPoint(point) + vector2;
			}
			else if (!hoverTarget.bounds.Contains(point2))
			{
				vector = hoverTarget.bounds.ClosestPoint(point2) - vector2;
			}
			vector.y = hoverTarget.bounds.center.y;
			return uiCamera.cachedCamera.WorldToScreenPoint(vector);
		}
		return _newPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ConstrainCursorOs(Vector3 _newPos)
	{
		Vector3 result = ConstrainToBounds(_newPos);
		result.x = Mathf.Clamp(result.x, 5f, Screen.width - 5);
		result.y = Mathf.Clamp(result.y, 5f, Screen.height - 5);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ConstrainToBounds(Vector3 _newPosition)
	{
		Vector3 point = _newPosition;
		point.z = currentBounds.center.z;
		return currentBounds.ClosestPoint(point);
	}

	public override Vector2 GetScreenPosition()
	{
		return MouseLib.GetLocalMousePosition();
	}

	public override Vector2 GetLocalScreenPosition()
	{
		return MouseLib.GetLocalMousePosition();
	}

	public override void SetScreenPosition(Vector2 _newPosition)
	{
		MouseLib.SetCursorPosition((int)(_newPosition.x + 0.5f), (int)(_newPosition.y + 0.5f));
	}

	public override void SetScreenPosition(float _x, float _y)
	{
		SetScreenPosition(new Vector2(_x, _y));
	}

	public override void ResetToCenter()
	{
		SetScreenPosition(Screen.width / 2, Screen.height / 2);
	}

	public override void SetNavigationTarget(XUiView _view)
	{
		throw new NotImplementedException();
	}

	public override void SetNavigationTargetLater(XUiView _view)
	{
		throw new NotImplementedException();
	}

	public override void ResetNavigationTarget()
	{
		throw new NotImplementedException();
	}

	public override void SetNavigationLockView(XUiView _view, XUiView _viewToSelect = null)
	{
		throw new NotImplementedException();
	}

	public override void RefreshSelection()
	{
		throw new NotImplementedException();
	}

	public override void SetCursorHidden(bool _hidden)
	{
		GameManager.Instance.SetCursorEnabledOverride(_hidden, _bOverrideState: false);
	}

	public override bool GetCursorHidden()
	{
		return GameManager.Instance.GetCursorEnabledOverride();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnVirtualCursorVisibleChanged()
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator ApplyCursorChangeLater()
	{
		while (!Cursor.visible)
		{
			yield return null;
		}
		Cursor.SetCursor(currentCursorTexture, currentCursorHotspot, CursorMode.Auto);
		cursorUpdateCo = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetCursorTexture(Texture2D _tex, Vector2 _hotspot)
	{
		if (_tex != currentCursorTexture)
		{
			currentCursorTexture = _tex;
			currentCursorHotspot = _hotspot;
			if (cursorUpdateCo == null)
			{
				cursorUpdateCo = ThreadManager.StartCoroutine(ApplyCursorChangeLater());
			}
		}
	}

	public new static void SetCursor(ECursorType _cursorType)
	{
		currentCursorType = _cursorType;
		switch (_cursorType)
		{
		case ECursorType.None:
			SetCursorTexture(emptyCursor, mapCursorCenter);
			break;
		case ECursorType.Default:
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				SetCursorTexture(defaultMouseCursor, defaultMouseCursorCenter);
			}
			else
			{
				SetCursorTexture(defaultControllerCursor, defaultControllerCursorCenter);
			}
			break;
		case ECursorType.Map:
			SetCursorTexture(mapCursor, mapCursorCenter);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}

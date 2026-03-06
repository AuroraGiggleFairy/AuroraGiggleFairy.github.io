using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public abstract class CursorControllerAbs : MonoBehaviour, IGamePrefsChangedListener
{
	public enum InputType
	{
		Controller,
		Mouse,
		Both
	}

	public enum ECursorType
	{
		None,
		Default,
		Map,
		Count
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string softCursorPrefabPath = "Prefabs/SoftCursor";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject softCursorPrefab;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static List<CursorControllerAbs> softCursors = new List<CursorControllerAbs>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public UICamera uiCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public PlayerActionsGUI guiActions;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GUIWindowManager windowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite cursor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Bounds cursorWorldBounds;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 cursorBuffer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, Bounds> activeBounds = new Dictionary<string, Bounds>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Bounds currentBounds;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip cursorSelectSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip pagingSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiView hoverTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bHasHoverTarget;

	public static bool bSnapCursor;

	public static float regularSpeed = 1f;

	public static float hoverSpeed = 1f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimationCurve accelerationCurve;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool _locked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool _virtualCursorHidden;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool FreeCursorEnabled = true;

	public static bool PrefabReady => softCursorPrefab != null;

	public virtual XUiView HoverTarget
	{
		get
		{
			return hoverTarget;
		}
		set
		{
			hoverTarget = value;
			bHasHoverTarget = value != null;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiView navigationTarget
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiView lockNavigationToView
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public bool Locked
	{
		get
		{
			return _locked;
		}
		set
		{
			_locked = value;
		}
	}

	public bool VirtualCursorHidden
	{
		get
		{
			return _virtualCursorHidden;
		}
		set
		{
			if (value != _virtualCursorHidden)
			{
				_virtualCursorHidden = value;
				OnVirtualCursorVisibleChanged();
			}
		}
	}

	public XUiView CurrentTarget
	{
		get
		{
			if (CursorModeActive)
			{
				return hoverTarget;
			}
			return navigationTarget;
		}
	}

	public virtual bool CursorModeActive => false;

	public abstract Vector2 GetScreenPosition();

	public abstract Vector2 GetLocalScreenPosition();

	public abstract void SetScreenPosition(Vector2 _newPosition);

	public abstract void SetScreenPosition(float _x, float _y);

	public abstract void SetNavigationTarget(XUiView _view);

	public abstract void SetNavigationTargetLater(XUiView _view);

	public abstract void ResetNavigationTarget();

	public abstract void ResetToCenter();

	public abstract void SetNavigationLockView(XUiView _view, XUiView _viewToSelect = null);

	public abstract void RefreshSelection();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void OnVirtualCursorVisibleChanged();

	public void SetGUIActions(PlayerActionsGUI _guiActions)
	{
		guiActions = _guiActions;
	}

	public void SetWindowManager(GUIWindowManager _windowManager)
	{
		windowManager = _windowManager;
	}

	public static void UpdateGamePrefs()
	{
		bSnapCursor = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerCursorSnap);
		regularSpeed = GamePrefs.GetFloat(EnumGamePrefs.OptionsInterfaceSensitivity);
		hoverSpeed = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerCursorHoverSensitivity);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AwakeBase()
	{
		GamePrefs.AddChangeListener(this);
		GameOptionsManager.ResolutionChanged += OnResolutionChanged;
		UpdateGamePrefs();
		softCursors.Add(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DestroyBase()
	{
		GamePrefs.RemoveChangeListener(this);
		GameOptionsManager.ResolutionChanged -= OnResolutionChanged;
		softCursors.Remove(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void InitCursorBounds()
	{
		cursorWorldBounds = new Bounds(cursor.worldCenter, Vector3.zero);
		Vector3[] worldCorners = cursor.worldCorners;
		for (int i = 0; i < worldCorners.Length; i++)
		{
			cursorWorldBounds.Encapsulate(worldCorners[i]);
		}
		Bounds bounds = new Bounds(uiCamera.cachedCamera.WorldToScreenPoint(cursorWorldBounds.min), Vector3.zero);
		bounds.Encapsulate(uiCamera.cachedCamera.WorldToScreenPoint(cursorWorldBounds.max));
		cursorBuffer = bounds.extents;
	}

	public abstract void UpdateMoveSpeed();

	public void UpdateBounds(string _boundsName, Bounds _bounds)
	{
		_bounds.Expand(cursorBuffer);
		activeBounds[_boundsName] = _bounds;
		RefreshBounds();
	}

	public void RemoveBounds(string _boundsName)
	{
		activeBounds.Remove(_boundsName);
		RefreshBounds();
	}

	public void RefreshBounds()
	{
		currentBounds.size = Vector3.zero;
		if (activeBounds.Count > 0)
		{
			bool flag = true;
			{
				foreach (KeyValuePair<string, Bounds> activeBound in activeBounds)
				{
					if (flag)
					{
						currentBounds.center = activeBound.Value.center;
						flag = false;
					}
					currentBounds.Encapsulate(activeBound.Value);
				}
				return;
			}
		}
		currentBounds.center = new Vector3(0f, 0f);
		currentBounds.Encapsulate(new Vector3(uiCamera.cachedCamera.pixelWidth, uiCamera.cachedCamera.pixelHeight));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnResolutionChanged(int _width, int _height)
	{
		StartCoroutine(RefreshBoundsNextFrame());
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsInterfaceSensitivity)
		{
			UpdateMoveSpeed();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator RefreshBoundsNextFrame()
	{
		yield return null;
		RefreshBounds();
	}

	public abstract void SetCursorHidden(bool _hidden);

	public abstract bool GetCursorHidden();

	public bool GetMouseButtonDown(UICamera.MouseButton _mouseButton)
	{
		if (guiActions == null)
		{
			return false;
		}
		if (GameManager.Instance.m_GUIConsole.isShowing)
		{
			return false;
		}
		switch (_mouseButton)
		{
		case UICamera.MouseButton.LeftButton:
			if (!guiActions.Submit.WasPressed)
			{
				return guiActions.LeftClick.WasPressed;
			}
			return true;
		case UICamera.MouseButton.RightButton:
			if (!guiActions.Inspect.WasPressed)
			{
				return guiActions.RightClick.WasPressed;
			}
			return true;
		case UICamera.MouseButton.MiddleButton:
			return false;
		default:
			return false;
		}
	}

	public bool GetMouseButton(UICamera.MouseButton _mouseButton)
	{
		if (guiActions == null)
		{
			return false;
		}
		if (GameManager.Instance.m_GUIConsole.isShowing)
		{
			return false;
		}
		switch (_mouseButton)
		{
		case UICamera.MouseButton.LeftButton:
			if (!guiActions.Submit.IsPressed)
			{
				return guiActions.LeftClick.IsPressed;
			}
			return true;
		case UICamera.MouseButton.RightButton:
			if (!guiActions.Inspect.IsPressed)
			{
				return guiActions.RightClick.IsPressed;
			}
			return true;
		case UICamera.MouseButton.MiddleButton:
			return false;
		default:
			return false;
		}
	}

	public bool GetMouseButtonUp(UICamera.MouseButton _mouseButton)
	{
		if (guiActions == null)
		{
			return false;
		}
		switch (_mouseButton)
		{
		case UICamera.MouseButton.LeftButton:
			if (!guiActions.Submit.WasReleased)
			{
				return guiActions.LeftClick.WasReleased;
			}
			return true;
		case UICamera.MouseButton.RightButton:
			if (!guiActions.Inspect.WasReleased)
			{
				return guiActions.RightClick.WasReleased;
			}
			return true;
		case UICamera.MouseButton.MiddleButton:
			return false;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DebugDrawBound(Bounds _bound)
	{
		Vector3 max = _bound.max;
		Vector3 min = _bound.min;
		Vector3 position = new Vector3(min.x, max.y, max.z);
		Vector3 position2 = new Vector3(max.x, min.y, min.z);
		max = uiCamera.cachedCamera.ScreenToWorldPoint(max);
		position = uiCamera.cachedCamera.ScreenToWorldPoint(position);
		position2 = uiCamera.cachedCamera.ScreenToWorldPoint(position2);
		min = uiCamera.cachedCamera.ScreenToWorldPoint(min);
		Debug.DrawLine(max, position);
		Debug.DrawLine(position2, min);
		Debug.DrawLine(max, position2);
		Debug.DrawLine(position, min);
	}

	public static void SetCursor(ECursorType _cursorType)
	{
		SoftCursor.SetCursor(_cursorType);
	}

	public static void LoadStaticData(LoadManager.LoadGroup _loadGroup)
	{
		LoadManager.LoadAssetFromResources(softCursorPrefabPath, [PublicizedFrom(EAccessModifier.Internal)] (GameObject _asset) =>
		{
			softCursorPrefab = _asset;
		}, null, _deferLoading: false, _loadSync: true);
	}

	public static CursorControllerAbs AddSoftCursor(UICamera _camera, PlayerActionsGUI _guiActions, GUIWindowManager _windowManager)
	{
		GameObject gameObject = _camera.gameObject.AddChild(softCursorPrefab);
		SoftCursor component = gameObject.GetComponent<SoftCursor>();
		component.SetGUIActions(_guiActions);
		component.SetWindowManager(_windowManager);
		_camera.cancelKey0 = KeyCode.None;
		_camera.submitKey1 = KeyCode.None;
		_camera.cancelKey1 = KeyCode.None;
		UICamera.GetMousePosition = component.GetScreenPosition;
		UICamera.GetMouseButton = component.GetMouseButton;
		UICamera.GetMouseButtonDown = component.GetMouseButtonDown;
		UICamera.GetMouseButtonUp = component.GetMouseButtonUp;
		gameObject.SetActive(value: true);
		return component;
	}

	public void PlayPagingSound()
	{
		Manager.PlayXUiSound(pagingSound, 1f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public CursorControllerAbs()
	{
	}
}

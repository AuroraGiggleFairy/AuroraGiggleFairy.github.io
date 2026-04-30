using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Platform;
using UnityEngine;

public class LocalPlayerUI : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static LocalPlayerUI mPrimaryUI;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static LocalPlayerUI mCleanCopy;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal mEntityPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool mIsPrimaryUI;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool mIsCleanCopy;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GUIWindowManager mWindowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NGUIWindowManager mNGUIWindowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UICamera mUICamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera mCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CursorControllerAbs mCursorController;

	public static bool IsOverPagingOverrideElement = false;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public XUi mXUi;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<LocalPlayerUI> playerUIs = new List<LocalPlayerUI>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Queue<LocalPlayerUI> playerUIQueueForPendingEntity = new Queue<LocalPlayerUI>();

	public static readonly ReadOnlyCollection<LocalPlayerUI> PlayerUIs = new ReadOnlyCollection<LocalPlayerUI>(playerUIs);

	public static bool CreatingCleanCopy;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<XUiView> navigationCandidates = new List<XUiView>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<XUiView> navigationPrimeCandidates = new List<XUiView>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<XUiView> navigationWrapAroundCandidates = new List<XUiView>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<XUiView> navigationWrapAroundPrimeCandidates = new List<XUiView>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<XUiV_Window> openWindows = new List<XUiV_Window>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<XUiView> navViews = new List<XUiView>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float navigationAngleLimit = 78f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 previousInputVector = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float inputRepeatTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialRepeat;

	public List<XUiC_ItemStackGrid> activeItemStackGrids = new List<XUiC_ItemStackGrid>();

	public static LocalPlayerUI primaryUI => mPrimaryUI;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int playerIndex
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static int localPlayerCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return playerUIs.Count - 1;
		}
	}

	public PlayerActionsLocal playerInput => PlatformManager.NativePlatform?.Input?.PrimaryPlayer;

	public ActionSetManager ActionSetManager => PlatformManager.NativePlatform?.Input?.ActionSetManager;

	public EntityPlayerLocal entityPlayer
	{
		get
		{
			return mEntityPlayer;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			mEntityPlayer = value;
			this.OnEntityPlayerLocalAssigned?.Invoke(mEntityPlayer);
		}
	}

	public LocalPlayer localPlayer
	{
		get
		{
			if (entityPlayer != null)
			{
				return entityPlayer.GetComponent<LocalPlayer>();
			}
			return null;
		}
	}

	public bool isPrimaryUI => mIsPrimaryUI;

	public bool IsCleanCopy
	{
		get
		{
			return mIsCleanCopy;
		}
		set
		{
			mIsCleanCopy = value;
		}
	}

	public GUIWindowManager windowManager
	{
		get
		{
			if (!(mWindowManager != null))
			{
				return mWindowManager = GetComponent<GUIWindowManager>();
			}
			return mWindowManager;
		}
	}

	public NGUIWindowManager nguiWindowManager
	{
		get
		{
			if (!(mNGUIWindowManager != null))
			{
				return mNGUIWindowManager = GetComponent<NGUIWindowManager>();
			}
			return mNGUIWindowManager;
		}
	}

	public UICamera uiCamera
	{
		get
		{
			if (!(mUICamera != null))
			{
				return mUICamera = GetComponentInParent<UICamera>();
			}
			return mUICamera;
		}
	}

	public Camera camera
	{
		get
		{
			if (!(mCamera != null))
			{
				return mCamera = GetComponentInParent<Camera>();
			}
			return mCamera;
		}
	}

	public CursorControllerAbs CursorController
	{
		get
		{
			if (mCursorController != null)
			{
				return mCursorController;
			}
			UICamera uICamera = uiCamera;
			if (uICamera == null)
			{
				return null;
			}
			return mCursorController = uICamera.GetComponentInChildren<CursorControllerAbs>();
		}
	}

	public XUi xui
	{
		get
		{
			if (mXUi == null)
			{
				mXUi = GetComponentInChildren<XUi>();
			}
			return mXUi;
		}
	}

	public event Action<EntityPlayerLocal> OnEntityPlayerLocalAssigned;

	public event Action OnUIShutdown;

	public static void QueueUIForNewPlayerEntity(LocalPlayerUI _playerUI)
	{
		if (!playerUIQueueForPendingEntity.Contains(_playerUI))
		{
			playerUIQueueForPendingEntity.Enqueue(_playerUI);
		}
	}

	public static LocalPlayerUI DispatchNewPlayerForUI(EntityPlayerLocal _entityPlayer)
	{
		LocalPlayerUI uIForPlayer = GetUIForPlayer(_entityPlayer);
		uIForPlayer.entityPlayer = _entityPlayer;
		return uIForPlayer;
	}

	public static LocalPlayerUI GetUIForPrimaryPlayer()
	{
		for (int i = 0; i < playerUIs.Count; i++)
		{
			LocalPlayerUI localPlayerUI = playerUIs[i];
			if (!localPlayerUI.isPrimaryUI)
			{
				return localPlayerUI;
			}
		}
		return null;
	}

	public static LocalPlayerUI GetUIForPlayer(EntityPlayerLocal _entityPlayer)
	{
		if (_entityPlayer == null)
		{
			return null;
		}
		LocalPlayerUI localPlayerUI = null;
		LocalPlayerUI localPlayerUI2 = null;
		for (int i = 0; i < playerUIs.Count; i++)
		{
			LocalPlayerUI localPlayerUI3 = playerUIs[i];
			if (localPlayerUI3.entityPlayer == _entityPlayer)
			{
				localPlayerUI = localPlayerUI3;
				break;
			}
			if (!localPlayerUI3.isPrimaryUI && localPlayerUI3.entityPlayer == null)
			{
				localPlayerUI2 = localPlayerUI3;
			}
		}
		if (localPlayerUI == null && playerUIQueueForPendingEntity.Count > 0)
		{
			localPlayerUI = playerUIQueueForPendingEntity.Dequeue();
			localPlayerUI.mEntityPlayer = _entityPlayer;
		}
		if (localPlayerUI == null)
		{
			localPlayerUI = localPlayerUI2;
		}
		return localPlayerUI;
	}

	public static LocalPlayerUI CreateUIForNewLocalPlayer()
	{
		if (!primaryUI)
		{
			throw new Exception("Can't create UI for new local player, primary UI not set.");
		}
		GameObject obj = mCleanCopy.gameObject;
		Transform transform = primaryUI.transform;
		obj.SetActive(value: false);
		GameObject obj2 = UnityEngine.Object.Instantiate(obj, primaryUI.transform.parent, worldPositionStays: true);
		obj2.transform.position = Vector3.back * localPlayerCount * 4f;
		obj2.transform.localRotation = transform.localRotation;
		obj2.transform.localScale = transform.localScale;
		obj2.gameObject.name = "GUI(Player" + playerUIs.Count + ")";
		MainMenuMono component = obj2.GetComponent<MainMenuMono>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		obj2.SetActive(value: true);
		LocalPlayerUI component2 = obj2.GetComponent<LocalPlayerUI>();
		component2.nguiWindowManager.Show(EnumNGUIWindow.Version, _bEnable: false);
		GameManager.Instance.AddWindows(component2.windowManager);
		List<string> windowGroupSubset = null;
		bool async = true;
		GameObject xuiPrefab = XUi.fullPrefab;
		bool flag = localPlayerCount > 1;
		if (flag)
		{
			windowGroupSubset = new List<string>(new string[3] { "secondaryPlayerJoin", "CalloutGroup", "popupGroup" });
			async = false;
			xuiPrefab = null;
		}
		XUi xUi = XUi.Instantiate(component2, xuiPrefab);
		xUi.Load(windowGroupSubset, async);
		xUi.isMinimal = flag;
		return component2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PrepareUIForSplitScreen(Transform _uiTransform)
	{
		UIRect.AnchorPoint[] array = new UIRect.AnchorPoint[4];
		UICamera componentInParent = _uiTransform.GetComponentInParent<UICamera>();
		UIRect[] componentsInChildren = _uiTransform.GetComponentsInChildren<UIRect>(includeInactive: true);
		foreach (UIRect uIRect in componentsInChildren)
		{
			if (!uIRect.isAnchored)
			{
				continue;
			}
			array[0] = uIRect.topAnchor;
			array[1] = uIRect.leftAnchor;
			array[2] = uIRect.rightAnchor;
			array[3] = uIRect.bottomAnchor;
			foreach (UIRect.AnchorPoint anchorPoint in array)
			{
				if (anchorPoint != null && anchorPoint.target != null)
				{
					UIPanel component = anchorPoint.target.gameObject.GetComponent<UIPanel>();
					if (component != null && component.clipping == UIDrawCall.Clipping.None)
					{
						anchorPoint.target = componentInParent.transform;
					}
				}
			}
			uIRect.UpdateAnchors();
		}
	}

	public static bool AnyModalWindowOpen()
	{
		for (int i = 0; i < playerUIs.Count; i++)
		{
			if (playerUIs[i].windowManager.IsModalWindowOpen())
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateChildCameraIndices()
	{
		int num = 0;
		LocalPlayerCamera[] componentsInChildren = GetComponentsInChildren<LocalPlayerCamera>();
		foreach (LocalPlayerCamera obj in componentsInChildren)
		{
			obj.uiChildIndex = num++;
			obj.SetCameraDepth();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		playerUIs.Add(this);
		mIsPrimaryUI = playerUIs.Count == 1;
		if (mIsPrimaryUI)
		{
			mPrimaryUI = this;
			base.gameObject.name = "GUI(Menu)";
			PrepareUIForSplitScreen(uiCamera.transform);
			CreatingCleanCopy = true;
			mCleanCopy = UnityEngine.Object.Instantiate(this, primaryUI.transform.parent, worldPositionStays: true);
			mCleanCopy.name = "GUI(CleanCopy)";
			mCleanCopy.gameObject.SetActive(value: false);
			mCleanCopy.IsCleanCopy = true;
			GameManager.Instance.AddWindows(mCleanCopy.windowManager);
			mCleanCopy.nguiWindowManager.ShowAll(_bShow: false);
			MainMenuMono component = mCleanCopy.GetComponent<MainMenuMono>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			playerUIs.Remove(mCleanCopy);
			CreatingCleanCopy = false;
		}
		else
		{
			Camera[] componentsInParent = GetComponentsInParent<Camera>();
			for (int i = 0; i < componentsInParent.Length; i++)
			{
				LocalPlayerCamera.AddToCamera(componentsInParent[i], LocalPlayerCamera.CameraType.UI);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		DispatchLocalPlayersChanged();
		if (mIsPrimaryUI && !GameManager.IsDedicatedServer)
		{
			XUiFromXml.ClearData();
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Common/styles"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Common/controls"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/styles"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/controls"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/windows"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/xui"));
			XUi xUi = XUi.Instantiate(this);
			xUi.Load(null, async: true);
			xUi.isMinimal = false;
			SetupMenuSoftCursor();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupMenuSoftCursor()
	{
		CursorControllerAbs.AddSoftCursor(uiCamera, PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions, windowManager);
		CursorController.RefreshBounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		LocalPlayerManager.OnLocalPlayersChanged += HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		LocalPlayerManager.OnLocalPlayersChanged -= HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DispatchLocalPlayersChanged()
	{
		int num = 0;
		for (int i = 0; i < playerUIs.Count; i++)
		{
			LocalPlayerUI localPlayerUI = playerUIs[i];
			if (!localPlayerUI.isPrimaryUI)
			{
				localPlayerUI.playerIndex = num++;
			}
		}
		LocalPlayerManager.LocalPlayersChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLocalPlayersChanged()
	{
		if (isPrimaryUI)
		{
			AudioListener componentInParent = GetComponentInParent<AudioListener>();
			if (componentInParent != null)
			{
				componentInParent.enabled = !(GameManager.Instance != null) || GameManager.Instance.World == null || !(GameManager.Instance.World.GetPrimaryPlayer() != null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (isPrimaryUI && GameManager.Instance.triggerEffectManager != null)
		{
			bool flag = windowManager.IsModalWindowOpen();
			if (!flag)
			{
				foreach (LocalPlayerUI playerUI in PlayerUIs)
				{
					if (playerUI.windowManager.IsModalWindowOpen())
					{
						flag = true;
						break;
					}
				}
			}
			GameManager.Instance.triggerEffectManager.inUI = flag;
		}
		if (isPrimaryUI && !windowManager.IsModalWindowOpen() && xui.calloutWindow != null)
		{
			xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		}
		if (primaryUI.windowManager.IsWindowOpen(GUIWindowConsole.ID) || (!isPrimaryUI && primaryUI.windowManager.IsModalWindowOpen()))
		{
			return;
		}
		PlayerActionsLocal playerActionsLocal = playerInput;
		if (playerActionsLocal == null || playerActionsLocal.GUIActions.Inspect.IsPressed || xui.playerUI.CursorController.Locked || (xui.playerUI.CursorController.lockNavigationToView != null && xui.playerUI.CursorController.lockNavigationToView.xui.playerUI != this) || (playerInput.GUIActions.BackButton.WasPressed && TryItemStackGridNavigation()))
		{
			return;
		}
		Vector2 vector = playerActionsLocal.GUIActions.Nav.Vector;
		if (vector == Vector2.zero)
		{
			inputRepeatTimer = 0f;
			previousInputVector = Vector2.zero;
			return;
		}
		if (previousInputVector != Vector2.zero)
		{
			inputRepeatTimer -= Time.unscaledDeltaTime;
			if (inputRepeatTimer > 0f)
			{
				return;
			}
		}
		else
		{
			initialRepeat = true;
		}
		XUiView navigationTarget = CursorController.navigationTarget;
		previousInputVector = vector;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		if (Mathf.Abs(vector.y) > Mathf.Abs(vector.x))
		{
			flag2 = vector.y > 0f;
			flag3 = vector.y < 0f;
		}
		else
		{
			flag5 = vector.x > 0f;
			flag4 = vector.x < 0f;
		}
		openWindows.Clear();
		navViews.Clear();
		if (CursorController.lockNavigationToView != null)
		{
			CursorController.lockNavigationToView.Controller.FindNavigatableChildren(navViews);
		}
		else
		{
			xui.GetOpenWindows(openWindows);
			foreach (XUiV_Window openWindow in openWindows)
			{
				openWindow.Controller.FindNavigatableChildren(navViews);
			}
		}
		if (navViews.Count == 0)
		{
			return;
		}
		float num = float.MaxValue;
		XUiView navigationTarget2 = null;
		Vector2 flatPosition = ((SoftCursor)CursorController).GetFlatPosition();
		if (navigationTarget != null && !navigationTarget.IsNavigatable)
		{
			CursorController.SetNavigationTarget(null);
		}
		bool flag6 = false;
		if (navigationTarget == null)
		{
			foreach (XUiView navView in navViews)
			{
				float sqrMagnitude = (flatPosition - navView.GetClosestPoint(flatPosition)).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					navigationTarget2 = navView;
				}
			}
			if (num < float.MaxValue)
			{
				CursorController.SetNavigationTarget(navigationTarget2);
				flag6 = true;
			}
		}
		if (!flag6)
		{
			if (flag2 && navigationTarget.NavUpTarget != null)
			{
				navigationTarget.NavUpTarget.Controller.SelectCursorElement();
				flag6 = true;
			}
			else if (flag3 && navigationTarget.NavDownTarget != null)
			{
				navigationTarget.NavDownTarget.Controller.SelectCursorElement();
				flag6 = true;
			}
			else if (flag4 && navigationTarget.NavLeftTarget != null)
			{
				navigationTarget.NavLeftTarget.Controller.SelectCursorElement();
				flag6 = true;
			}
			else if (flag5 && navigationTarget.NavRightTarget != null)
			{
				navigationTarget.NavRightTarget.Controller.SelectCursorElement();
				flag6 = true;
			}
		}
		navigationCandidates.Clear();
		navigationPrimeCandidates.Clear();
		navigationWrapAroundCandidates.Clear();
		navigationWrapAroundPrimeCandidates.Clear();
		if (!flag6)
		{
			Vector2 vector2 = (flag2 ? new Vector2(navigationTarget.Center.x, -Screen.height) : (flag3 ? new Vector2(navigationTarget.Center.x, Screen.height) : ((!flag4) ? new Vector2(-Screen.width, navigationTarget.Center.y) : new Vector2(Screen.width, navigationTarget.Center.y))));
			foreach (XUiView navView2 in navViews)
			{
				if (flag2)
				{
					if (navView2.Center.y > navigationTarget.Center.y)
					{
						if (!(Vector3.Angle(navigationTarget.UiTransform.up, (navView2.Center - navigationTarget.Center).normalized) > 78f))
						{
							if (Mathf.Abs(navigationTarget.Center.x - navView2.Center.x) <= Mathf.Max(navigationTarget.widthExtent, navView2.widthExtent))
							{
								navigationPrimeCandidates.Add(navView2);
							}
							else
							{
								navigationCandidates.Add(navView2);
							}
						}
					}
					else if (Mathf.Abs(navigationTarget.Center.x - navView2.Center.x) <= Mathf.Max(navigationTarget.widthExtent, navView2.widthExtent))
					{
						navigationWrapAroundPrimeCandidates.Add(navView2);
					}
					else
					{
						navigationWrapAroundCandidates.Add(navView2);
					}
				}
				else if (flag3)
				{
					if (navView2.Center.y < navigationTarget.Center.y)
					{
						if (!(Vector3.Angle(-navigationTarget.UiTransform.up, (navView2.Center - navigationTarget.Center).normalized) > 78f))
						{
							if (Mathf.Abs(navigationTarget.Center.x - navView2.Center.x) <= Mathf.Max(navigationTarget.widthExtent, navView2.widthExtent))
							{
								navigationPrimeCandidates.Add(navView2);
							}
							else
							{
								navigationCandidates.Add(navView2);
							}
						}
					}
					else if (Mathf.Abs(navigationTarget.Center.x - navView2.Center.x) <= Mathf.Max(navigationTarget.widthExtent, navView2.widthExtent))
					{
						navigationWrapAroundPrimeCandidates.Add(navView2);
					}
					else
					{
						navigationWrapAroundCandidates.Add(navView2);
					}
				}
				else if (flag4)
				{
					if (navView2.Center.x < navigationTarget.Center.x)
					{
						if (!(Vector3.Angle(-navigationTarget.UiTransform.right, (navView2.Center - navigationTarget.Center).normalized) > 78f))
						{
							if (Mathf.Abs(navigationTarget.Center.y - navView2.Center.y) <= Mathf.Max(navigationTarget.heightExtent, navView2.heightExtent))
							{
								navigationPrimeCandidates.Add(navView2);
							}
							else
							{
								navigationCandidates.Add(navView2);
							}
						}
					}
					else if (Mathf.Abs(navigationTarget.Center.y - navView2.Center.y) <= Mathf.Max(navigationTarget.heightExtent, navView2.heightExtent))
					{
						navigationWrapAroundPrimeCandidates.Add(navView2);
					}
					else
					{
						navigationWrapAroundCandidates.Add(navView2);
					}
				}
				else
				{
					if (!flag5)
					{
						continue;
					}
					if (navView2.Center.x > navigationTarget.Center.x)
					{
						if (!(Vector3.Angle(navigationTarget.UiTransform.right, (navView2.Center - navigationTarget.Center).normalized) > 78f))
						{
							if (Mathf.Abs(navigationTarget.Center.y - navView2.Center.y) <= Mathf.Max(navigationTarget.heightExtent, navView2.heightExtent))
							{
								navigationPrimeCandidates.Add(navView2);
							}
							else
							{
								navigationCandidates.Add(navView2);
							}
						}
					}
					else if (Mathf.Abs(navigationTarget.Center.y - navView2.Center.y) <= Mathf.Max(navigationTarget.heightExtent, navView2.heightExtent))
					{
						navigationWrapAroundPrimeCandidates.Add(navView2);
					}
					else
					{
						navigationWrapAroundCandidates.Add(navView2);
					}
				}
			}
			if (!flag6 && navigationPrimeCandidates.Count > 0)
			{
				foreach (XUiView navigationPrimeCandidate in navigationPrimeCandidates)
				{
					float num2 = ((!(flag4 || flag5)) ? Mathf.Abs(navigationTarget.Center.y - navigationPrimeCandidate.Center.y) : Mathf.Abs(navigationTarget.Center.x - navigationPrimeCandidate.Center.x));
					if (num2 < num)
					{
						num = num2;
						navigationTarget2 = navigationPrimeCandidate;
					}
				}
			}
			else if (!flag6 && navigationCandidates.Count > 0)
			{
				foreach (XUiView navigationCandidate in navigationCandidates)
				{
					float sqrMagnitude2 = (navigationTarget.GetClosestPoint(navigationCandidate.Center) - navigationCandidate.GetClosestPoint(navigationTarget.Center)).sqrMagnitude;
					if (sqrMagnitude2 < num)
					{
						num = sqrMagnitude2;
						navigationTarget2 = navigationCandidate;
					}
				}
			}
			else if (!flag6 && navigationWrapAroundPrimeCandidates.Count > 0)
			{
				num = float.MaxValue;
				foreach (XUiView navigationWrapAroundPrimeCandidate in navigationWrapAroundPrimeCandidates)
				{
					float num3 = ((!(flag4 || flag5)) ? Mathf.Abs(vector2.y - navigationWrapAroundPrimeCandidate.Center.y) : Mathf.Abs(vector2.x - navigationWrapAroundPrimeCandidate.Center.x));
					if (num3 < num)
					{
						num = num3;
						navigationTarget2 = navigationWrapAroundPrimeCandidate;
					}
				}
			}
			else if (!flag6 && navigationWrapAroundCandidates.Count > 0)
			{
				num = float.MaxValue;
				foreach (XUiView navigationWrapAroundCandidate in navigationWrapAroundCandidates)
				{
					float sqrMagnitude3 = (vector2 - navigationWrapAroundCandidate.GetClosestPoint(vector2)).sqrMagnitude;
					if (sqrMagnitude3 < num)
					{
						num = sqrMagnitude3;
						navigationTarget2 = navigationWrapAroundCandidate;
					}
				}
			}
		}
		if (!flag6 && num < float.MaxValue)
		{
			CursorController.SetNavigationTarget(navigationTarget2);
		}
		inputRepeatTimer = (initialRepeat ? 0.35f : 0.1f);
		initialRepeat = false;
	}

	public void RefreshNavigationTarget()
	{
		openWindows.Clear();
		navViews.Clear();
		if (CursorController.lockNavigationToView != null)
		{
			CursorController.lockNavigationToView.Controller.FindNavigatableChildren(navViews);
		}
		else
		{
			xui.GetOpenWindows(openWindows);
			foreach (XUiV_Window openWindow in openWindows)
			{
				openWindow.Controller.FindNavigatableChildren(navViews);
			}
		}
		if (navViews.Count == 0)
		{
			return;
		}
		_ = CursorController.navigationTarget;
		float num = float.MaxValue;
		XUiView navigationTarget = null;
		Vector2 flatPosition = ((SoftCursor)CursorController).GetFlatPosition();
		foreach (XUiView navView in navViews)
		{
			float sqrMagnitude = (flatPosition - navView.GetClosestPoint(flatPosition)).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				navigationTarget = navView;
			}
		}
		if (num < float.MaxValue)
		{
			CursorController.SetNavigationTarget(navigationTarget);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		this.OnUIShutdown?.Invoke();
		playerUIs.Remove(this);
		navViews.Clear();
		openWindows.Clear();
		DispatchLocalPlayersChanged();
	}

	public void RegisterItemStackGrid(XUiC_ItemStackGrid _grid)
	{
		if (!activeItemStackGrids.Contains(_grid))
		{
			activeItemStackGrids.Add(_grid);
			SortItemStackGrids();
		}
	}

	public void UnregisterItemStackGrid(XUiC_ItemStackGrid _grid)
	{
		activeItemStackGrids.Remove(_grid);
		SortItemStackGrids();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortItemStackGrids()
	{
		activeItemStackGrids.Sort([PublicizedFrom(EAccessModifier.Internal)] (XUiC_ItemStackGrid x, XUiC_ItemStackGrid y) => (x.ViewComponent.Center.y + x.ViewComponent.heightExtent > y.ViewComponent.Center.y + y.ViewComponent.heightExtent) ? 1 : (-1));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryItemStackGridNavigation()
	{
		if (activeItemStackGrids.Count == 0)
		{
			return false;
		}
		XUiC_ItemStackGrid xUiC_ItemStackGrid = null;
		if (CursorController.navigationTarget != null)
		{
			foreach (XUiC_ItemStackGrid activeItemStackGrid in activeItemStackGrids)
			{
				if (CursorController.navigationTarget.Controller.IsChildOf(activeItemStackGrid))
				{
					xUiC_ItemStackGrid = activeItemStackGrid;
					break;
				}
			}
		}
		int num = 0;
		if (xUiC_ItemStackGrid != null)
		{
			num = activeItemStackGrids.IndexOf(xUiC_ItemStackGrid);
		}
		int num2 = num;
		if (activeItemStackGrids.Count == 1)
		{
			activeItemStackGrids[num].SelectCursorElement(_withDelay: true);
			return true;
		}
		num++;
		if (num >= activeItemStackGrids.Count)
		{
			num = 0;
		}
		while (num != num2)
		{
			if (activeItemStackGrids[num].TryFindFirstNavigableChild(out var _))
			{
				if (!xui.dragAndDrop.CurrentStack.Equals(ItemStack.Empty))
				{
					int num3 = activeItemStackGrids[num].FindFirstEmptySlot();
					if (num3 >= 0)
					{
						activeItemStackGrids[num].GetItemStackControllers()[num3].SelectCursorElement(_withDelay: true, _overrideCursorMode: true);
						return true;
					}
				}
				activeItemStackGrids[num].SelectCursorElement(_withDelay: true, _overrideCursorMode: true);
				return true;
			}
			num++;
			if (num >= activeItemStackGrids.Count)
			{
				num = 0;
			}
		}
		return false;
	}
}

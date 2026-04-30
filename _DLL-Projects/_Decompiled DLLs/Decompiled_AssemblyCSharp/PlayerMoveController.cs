using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using InControl;
using Platform;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAllowPlayerInput = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> BowTag = FastTags<TagGroup.Global>.Parse("bow");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static PlayerMoveController Instance = null;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<NGuiAction> globalActions = new List<NGuiAction>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mouseLookSensitivity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float mouseZoomSensitivity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float zoomAccel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float vehicleLookSensitivity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 controllerLookSensitivity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameManager gameManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public NGuiWdwInGameHUD guiInGame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bCanControlOverride;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float UnstuckCountdownTime = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bLastRespawnActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float respawnTime;

	public RespawnType respawnReason;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool spawnWindowOpened;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityPlayerLocal entityPlayerLocal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public LocalPlayerUI playerUI;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GUIWindowManager windowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public NGUIWindowManager nguiWindowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int invertMouse;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int invertController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bControllerVibration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RenderDisplacedCube focusBoxScript;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string strTextLabelPointingTo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CountdownTimer countdownSuckItemsNearby = new CountdownTimer(0.05f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float inventoryItemSwitchTimeout;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int inventoryItemToSetAfterTimeout = int.MinValue;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAutoPilotControllor playerAutoPilotControllor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string InteractName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float InteractWaitTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeActivatePressed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bIgnoreLeftMouseUntilReleased;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int skipMouseLookNextFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasCameraChangeUsedWithWheel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraChangePressTime = -1f;

	public int drawChunkMode;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationCurve lookAccelerationCurve;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lookAccelerationRate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentLookAcceleration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float controllerZoomSensitivity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float controllerVehicleSensitivity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool controllerAimAssistsEnabled = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSprintModeHold = 0;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSprintModeToggle = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSprintModeAutorun = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int sprintMode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float aimAssistSlowAmount = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAimAssistTargetingItem;

	public Vector3i FocusBoxPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive cameraSnapTargetEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool snapTargetingHead;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public eCameraSnapMode cameraSnapMode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraSnapTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> cameraSnapTargets = new List<Entity>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationCurve targetSnapFalloffCurve;

	public Action toggleGodMode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Action teleportPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsGUICancelPressed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasVehicle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool runToggleActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float runInputTime;

	public bool isAutorun;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAutorunInvalid;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool inventoryScrollPressed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int inventoryScrollIdxToSelect = -1;

	public bool cameraChangeRequested;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPosition spawnPosition = SpawnPosition.Undef;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool waitingForSpawnPointSelection;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ERoutineState unstuckCoState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasUIInputActive;

	public static bool useScaledMouseLook = false;

	public static float lookDeltaTimeScale = 100f;

	public static float mouseDeltaTimeScale = 75f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 previousMouseInput = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool runPressedWhileActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> nextHeldItem = new List<int>();

	public bool RunToggleActive => runToggleActive;

	public PlayerActionsLocal playerInput => PlatformManager.NativePlatform.Input.PrimaryPlayer;

	public void Init()
	{
		Instance = this;
		entityPlayerLocal = GetComponent<EntityPlayerLocal>();
		playerUI = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		windowManager = playerUI.windowManager;
		nguiWindowManager = playerUI.nguiWindowManager;
		gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
		guiInGame = nguiWindowManager.InGameHUD;
		nguiWindowManager.Show(EnumNGUIWindow.InGameHUD, _bEnable: true);
		UpdateControlsOptions();
		GamePrefs.Set(EnumGamePrefs.DebugMenuShowTasks, _value: false);
		focusBoxScript = new RenderDisplacedCube((guiInGame.FocusCube != null) ? UnityEngine.Object.Instantiate(guiInGame.FocusCube) : null);
		playerAutoPilotControllor = new PlayerAutoPilotControllor(gameManager);
		toggleGodMode = [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			entityPlayerLocal.bEntityAliveFlagsChanged = true;
			entityPlayerLocal.IsGodMode.Value = !entityPlayerLocal.IsGodMode.Value;
			entityPlayerLocal.IsNoCollisionMode.Value = entityPlayerLocal.IsGodMode.Value;
			entityPlayerLocal.IsFlyMode.Value = entityPlayerLocal.IsGodMode.Value;
			if (entityPlayerLocal.IsGodMode.Value)
			{
				entityPlayerLocal.Buffs.AddBuff("god");
			}
			else if (!GameManager.Instance.World.IsEditor() && !GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
			{
				entityPlayerLocal.Buffs.RemoveBuff("god");
			}
		};
		teleportPlayer = [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Ray lookRay = entityPlayerLocal.GetLookRay();
			if (InputUtils.ControlKeyPressed)
			{
				lookRay.direction *= -1f;
			}
			lookRay.origin -= Origin.position;
			RaycastHit hitInfo;
			Vector3 vector = ((!Physics.SphereCast(lookRay, 0.3f, out hitInfo, 500f, 1342242816)) ? (lookRay.origin + lookRay.direction.normalized * 100f) : (hitInfo.point - lookRay.direction.normalized * 0.5f));
			entityPlayerLocal.SetPosition(vector + Origin.position);
			GameEventManager.Current.HandleForceBossDespawn(entityPlayerLocal);
		};
		NGuiAction nGuiAction = new NGuiAction("SelectionMode", null, null, _isToggle: true, playerInput.Drop);
		nGuiAction.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (InputUtils.AltKeyPressed)
			{
				GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 4);
			}
			else if (InputUtils.ShiftKeyPressed)
			{
				GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 2);
				GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: true, _bOverrideState: true);
			}
			else if (InputUtils.ControlKeyPressed && GameManager.Instance.IsEditMode())
			{
				GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 3);
				GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: true, _bOverrideState: true);
			}
			else
			{
				GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 1);
				GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: true, _bOverrideState: true);
			}
		});
		nGuiAction.SetReleaseActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 0);
			GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: false, _bOverrideState: false);
		});
		nGuiAction.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => (gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && GameManager.Instance.World.IsEditor()) || BlockToolSelection.Instance.SelectionActive);
		globalActions.Add(nGuiAction);
		NGuiAction.IsEnabledDelegate menuIsEnabled = [PublicizedFrom(EAccessModifier.Internal)] () => !XUiC_SpawnSelectionWindow.IsOpenInUI(LocalPlayerUI.primaryUI) && gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !LocalPlayerUI.primaryUI.windowManager.IsModalWindowOpen() && !windowManager.IsFullHUDDisabled();
		NGuiAction.OnClickActionDelegate clickActionDelegate = [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || !GameManager.Instance.isAnyCursorWindowOpen())
			{
				entityPlayerLocal.AimingGun = false;
				if (windowManager.IsWindowOpen("windowpaging") || windowManager.IsModalWindowOpen())
				{
					windowManager.CloseAllOpenWindows();
					windowManager.CloseIfOpen("windowpaging");
				}
				else
				{
					windowManager.CloseAllOpenWindows();
					playerUI.xui.RadialWindow.Open();
					playerUI.xui.RadialWindow.SetupMenuData();
				}
			}
		};
		NGuiAction.IsCheckedDelegate isCheckedDelegate = [PublicizedFrom(EAccessModifier.Internal)] () => windowManager.IsWindowOpen("windowpaging");
		NGuiAction.IsEnabledDelegate isEnabledDelegate = [PublicizedFrom(EAccessModifier.Internal)] () => menuIsEnabled() && !windowManager.IsWindowOpen("radial");
		NGuiAction nGuiAction2 = new NGuiAction("Inventory", null, null, _isToggle: true, playerInput.Inventory);
		nGuiAction2.SetClickActionDelegate(clickActionDelegate);
		nGuiAction2.SetIsEnabledDelegate(isEnabledDelegate);
		nGuiAction2.SetIsCheckedDelegate(isCheckedDelegate);
		globalActions.Add(nGuiAction2);
		NGuiAction nGuiAction3 = new NGuiAction("Inventory", null, null, _isToggle: true, playerInput.PermanentActions.Inventory);
		nGuiAction3.SetClickActionDelegate(clickActionDelegate);
		nGuiAction3.SetIsEnabledDelegate(isEnabledDelegate);
		nGuiAction3.SetIsCheckedDelegate(isCheckedDelegate);
		globalActions.Add(nGuiAction3);
		NGuiAction nGuiAction4 = new NGuiAction("Inventory", null, null, _isToggle: true, playerInput.VehicleActions.Inventory);
		nGuiAction4.SetClickActionDelegate(clickActionDelegate);
		nGuiAction4.SetIsEnabledDelegate(isEnabledDelegate);
		nGuiAction4.SetIsCheckedDelegate(isCheckedDelegate);
		globalActions.Add(nGuiAction4);
		NGuiAction nGuiAction5 = new NGuiAction("Creative", null, null, _isToggle: true, playerInput.PermanentActions.Creative);
		nGuiAction5.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || !GameManager.Instance.isAnyCursorWindowOpen())
			{
				entityPlayerLocal.AimingGun = false;
				XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayerLocal, "creative");
			}
		});
		nGuiAction5.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => menuIsEnabled() && (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled)));
		nGuiAction5.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => windowManager.IsWindowOpen("creative"));
		globalActions.Add(nGuiAction5);
		NGuiAction nGuiAction6 = new NGuiAction("Map", null, null, _isToggle: true, playerInput.PermanentActions.Map);
		nGuiAction6.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || !GameManager.Instance.isAnyCursorWindowOpen())
			{
				entityPlayerLocal.AimingGun = false;
				XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayerLocal, "map");
			}
		});
		nGuiAction6.SetIsEnabledDelegate(menuIsEnabled);
		nGuiAction6.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => windowManager.IsWindowOpen("map"));
		globalActions.Add(nGuiAction6);
		NGuiAction nGuiAction7 = new NGuiAction("Character", null, null, _isToggle: true, playerInput.PermanentActions.Character);
		nGuiAction7.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || !GameManager.Instance.isAnyCursorWindowOpen())
			{
				entityPlayerLocal.AimingGun = false;
				XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayerLocal, "character");
			}
		});
		nGuiAction7.SetIsEnabledDelegate(menuIsEnabled);
		globalActions.Add(nGuiAction7);
		NGuiAction nGuiAction8 = new NGuiAction("Skills", null, null, _isToggle: true, playerInput.PermanentActions.Skills);
		nGuiAction8.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || !GameManager.Instance.isAnyCursorWindowOpen())
			{
				entityPlayerLocal.AimingGun = false;
				XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayerLocal, "skills");
			}
		});
		nGuiAction8.SetIsEnabledDelegate(menuIsEnabled);
		globalActions.Add(nGuiAction8);
		NGuiAction nGuiAction9 = new NGuiAction("Quests", null, null, _isToggle: true, playerInput.PermanentActions.Quests);
		nGuiAction9.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || !GameManager.Instance.isAnyCursorWindowOpen())
			{
				entityPlayerLocal.AimingGun = false;
				XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayerLocal, "quests");
			}
		});
		nGuiAction9.SetIsEnabledDelegate(menuIsEnabled);
		globalActions.Add(nGuiAction9);
		NGuiAction.OnClickActionDelegate clickActionDelegate2 = [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || !GameManager.Instance.isAnyCursorWindowOpen())
			{
				entityPlayerLocal.AimingGun = false;
				XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayerLocal, "players");
			}
		};
		NGuiAction nGuiAction10 = new NGuiAction("Players", null, null, _isToggle: true, playerInput.Scoreboard);
		nGuiAction10.SetClickActionDelegate(clickActionDelegate2);
		nGuiAction10.SetIsEnabledDelegate(menuIsEnabled);
		globalActions.Add(nGuiAction10);
		NGuiAction nGuiAction11 = new NGuiAction("Players", null, null, _isToggle: true, playerInput.VehicleActions.Scoreboard);
		nGuiAction11.SetClickActionDelegate(clickActionDelegate2);
		nGuiAction11.SetIsEnabledDelegate(menuIsEnabled);
		globalActions.Add(nGuiAction11);
		NGuiAction nGuiAction12 = new NGuiAction("Players", null, null, _isToggle: true, playerInput.PermanentActions.Scoreboard);
		nGuiAction12.SetClickActionDelegate(clickActionDelegate2);
		nGuiAction12.SetIsEnabledDelegate(menuIsEnabled);
		globalActions.Add(nGuiAction12);
		NGuiAction nGuiAction13 = new NGuiAction("Challenges", null, null, _isToggle: true, playerInput.PermanentActions.Challenges);
		nGuiAction13.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || !GameManager.Instance.isAnyCursorWindowOpen())
			{
				entityPlayerLocal.AimingGun = false;
				XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayerLocal, "challenges");
			}
		});
		nGuiAction13.SetIsEnabledDelegate(menuIsEnabled);
		globalActions.Add(nGuiAction13);
		NGuiAction nGuiAction14 = new NGuiAction("Chat", null, null, _isToggle: true, playerInput.PermanentActions.Chat);
		nGuiAction14.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			entityPlayerLocal.AimingGun = false;
			windowManager.Open(XUiC_Chat.ID, _bModal: true);
		});
		nGuiAction14.SetIsEnabledDelegate(menuIsEnabled);
		nGuiAction14.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => windowManager.IsWindowOpen(XUiC_Chat.ID));
		globalActions.Add(nGuiAction14);
		NGuiAction nGuiAction15 = new NGuiAction("Prefab", null, null, _isToggle: true, playerInput.Prefab);
		nGuiAction15.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			entityPlayerLocal.AimingGun = false;
			Manager.PlayButtonClick();
			windowManager.SwitchVisible(GUIWindowWOChooseCategory.ID);
		});
		nGuiAction15.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => menuIsEnabled() && gameManager.IsEditMode());
		nGuiAction15.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => windowManager.IsWindowOpen(GUIWindowWOChooseCategory.ID));
		globalActions.Add(nGuiAction15);
		NGuiAction nGuiAction16 = new NGuiAction("DetachCamera", null, null, _isToggle: false, playerInput.DetachCamera);
		nGuiAction16.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Manager.PlayButtonClick();
			if (entityPlayerLocal.bFirstPersonView)
			{
				entityPlayerLocal.SetCameraAttachedToPlayer(!entityPlayerLocal.IsCameraAttachedToPlayerOrScope(), _lockCamera: true);
			}
			else
			{
				entityPlayerLocal.SetCameraAttachedToPlayer(_b: false, !entityPlayerLocal.vp_FPCamera.Locked3rdPerson);
			}
		});
		nGuiAction16.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !entityPlayerLocal.AimingGun && (gameManager.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)) && !InputUtils.ControlKeyPressed);
		globalActions.Add(nGuiAction16);
		NGuiAction nGuiAction17 = new NGuiAction("ToggleDCMove", null, null, _isToggle: false, playerInput.ToggleDCMove);
		nGuiAction17.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Manager.PlayButtonClick();
			entityPlayerLocal.movementInput.bDetachedCameraMove = !entityPlayerLocal.movementInput.bDetachedCameraMove && !entityPlayerLocal.IsCameraAttachedToPlayerOrScope();
		});
		nGuiAction17.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !entityPlayerLocal.AimingGun && (gameManager.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)));
		globalActions.Add(nGuiAction17);
		NGuiAction nGuiAction18 = new NGuiAction("LockCamera", null, null, _isToggle: false, playerInput.LockFreeCamera);
		nGuiAction18.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Manager.PlayButtonClick();
			entityPlayerLocal.movementInput.bCameraPositionLocked = !entityPlayerLocal.movementInput.bCameraPositionLocked;
		});
		nGuiAction18.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !entityPlayerLocal.AimingGun && (gameManager.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)));
		globalActions.Add(nGuiAction18);
		NGuiAction.OnClickActionDelegate clickActionDelegate3 = [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (!XUiC_SpawnSelectionWindow.IsOpenInUI(LocalPlayerUI.primaryUI))
			{
				Manager.PlayButtonClick();
				if (!windowManager.CloseAllOpenWindows())
				{
					entityPlayerLocal.PlayerUI.CursorController.HoverTarget = null;
					windowManager.SwitchVisible(XUiC_InGameMenuWindow.ID);
				}
			}
		};
		NGuiAction nGuiAction19 = new NGuiAction("Menu", null, null, _isToggle: false, playerInput.Menu);
		nGuiAction19.SetClickActionDelegate(clickActionDelegate3);
		nGuiAction19.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => !windowManager.IsFullHUDDisabled());
		globalActions.Add(nGuiAction19);
		NGuiAction nGuiAction20 = new NGuiAction("Menu", null, null, _isToggle: false, playerInput.VehicleActions.Menu);
		nGuiAction20.SetClickActionDelegate(clickActionDelegate3);
		nGuiAction20.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => !windowManager.IsFullHUDDisabled());
		globalActions.Add(nGuiAction20);
		NGuiAction nGuiAction21 = new NGuiAction("Fly Mode", null, null, _isToggle: true, playerInput.Fly);
		nGuiAction21.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Manager.PlayButtonClick();
			entityPlayerLocal.IsFlyMode.Value = !entityPlayerLocal.IsFlyMode.Value;
		});
		nGuiAction21.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => entityPlayerLocal != null && entityPlayerLocal.IsFlyMode.Value);
		nGuiAction21.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled) || GameStats.GetBool(EnumGameStats.IsFlyingEnabled));
		globalActions.Add(nGuiAction21);
		NGuiAction nGuiAction22 = new NGuiAction("God Mode", null, null, _isToggle: true, playerInput.God);
		nGuiAction22.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Manager.PlayButtonClick();
			if (InputUtils.ShiftKeyPressed)
			{
				teleportPlayer();
				isAutorunInvalid = true;
			}
			else
			{
				toggleGodMode();
			}
		});
		nGuiAction22.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => entityPlayerLocal != null && entityPlayerLocal.IsGodMode.Value);
		nGuiAction22.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled));
		globalActions.Add(nGuiAction22);
		NGuiAction nGuiAction23 = new NGuiAction("No Collision", null, null, _isToggle: true, null);
		nGuiAction23.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Manager.PlayButtonClick();
			entityPlayerLocal.IsNoCollisionMode.Value = !entityPlayerLocal.IsNoCollisionMode.Value;
		});
		nGuiAction23.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => entityPlayerLocal != null && entityPlayerLocal.IsNoCollisionMode.Value);
		nGuiAction23.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || !GameStats.GetBool(EnumGameStats.IsPlayerCollisionEnabled));
		globalActions.Add(nGuiAction23);
		NGuiAction nGuiAction24 = new NGuiAction("Invisible", null, null, _isToggle: true, playerInput.Invisible);
		nGuiAction24.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Manager.PlayButtonClick();
			entityPlayerLocal.IsSpectator = !entityPlayerLocal.IsSpectator;
		});
		nGuiAction24.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => entityPlayerLocal != null && entityPlayerLocal.IsSpectator);
		nGuiAction24.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled));
		globalActions.Add(nGuiAction24);
		for (int num = 0; num < globalActions.Count; num++)
		{
			windowManager.AddGlobalAction(globalActions[num]);
		}
		EAIManager.isAnimFreeze = false;
	}

	public static void UpdateControlsOptions()
	{
		if (Instance != null)
		{
			Instance.invertMouse = ((!GamePrefs.GetBool(EnumGamePrefs.OptionsInvertMouse)) ? 1 : (-1));
			Instance.invertController = ((!GamePrefs.GetBool(EnumGamePrefs.OptionsControllerLookInvert)) ? 1 : (-1));
			Instance.bControllerVibration = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerVibration);
			Instance.UpdateLookSensitivity(GamePrefs.GetFloat(EnumGamePrefs.OptionsLookSensitivity), GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomSensitivity), GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomAccel), GamePrefs.GetFloat(EnumGamePrefs.OptionsVehicleLookSensitivity), new Vector2(GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerSensitivityX), GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerSensitivityY)));
			Instance.lookAccelerationRate = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerLookAcceleration) * 0.5f;
			Instance.controllerZoomSensitivity = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerZoomSensitivity);
			Instance.controllerVehicleSensitivity = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerVehicleSensitivity);
			Instance.controllerAimAssistsEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerAimAssists);
			Instance.playerInput.SetJoyStickLayout((eControllerJoystickLayout)GamePrefs.GetInt(EnumGamePrefs.OptionsControllerJoystickLayout));
			Instance.sprintMode = GamePrefs.GetInt(EnumGamePrefs.OptionsControlsSprintLock);
			SetDeadzones();
		}
	}

	public static void SetDeadzones()
	{
		Instance.playerInput.SetDeadzones(GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerLookAxisDeadzone), GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerMoveAxisDeadzone));
	}

	public void UpdateInvertMouse(bool _invertMouse)
	{
		invertMouse = ((!_invertMouse) ? 1 : (-1));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLookSensitivity(float _sensitivity, float _zoomSensitivity, float _zoomAccel, float _vehicleSensitivity, Vector2 _controllerLookSensitivity)
	{
		mouseLookSensitivity = Vector3.one * _sensitivity * 5f;
		mouseZoomSensitivity = _zoomSensitivity;
		zoomAccel = _zoomAccel * 0.5f;
		vehicleLookSensitivity = _vehicleSensitivity * 5f;
		controllerLookSensitivity = _controllerLookSensitivity * 10f;
	}

	public Vector2 GetCameraInputSensitivity()
	{
		if (playerInput.LastInputType == BindingSourceType.DeviceBindingSource)
		{
			return controllerLookSensitivity;
		}
		return mouseLookSensitivity;
	}

	public bool GetControllerVibration()
	{
		return bControllerVibration;
	}

	public void UpdateControllerVibration(bool _controllerVibration)
	{
		bControllerVibration = _controllerVibration;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDestroy()
	{
		for (int i = 0; i < globalActions.Count; i++)
		{
			windowManager.RemoveGlobalAction(globalActions[i]);
		}
		focusBoxScript.Cleanup();
		Instance = null;
		playerUI = null;
		windowManager = null;
		nguiWindowManager = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (!gameManager.gameStateManager.IsGameStarted() || GameStats.GetInt(EnumGameStats.GameState) != 1 || windowManager.IsFullHUDDisabled())
		{
			return;
		}
		if (entityPlayerLocal.inventory != null && gameManager.World.worldTime % 2 == 0L)
		{
			ItemValue holdingItemItemValue = entityPlayerLocal.inventory.holdingItemItemValue;
			ItemClass forId = ItemClass.GetForId(holdingItemItemValue.type);
			int maxUseTimes = holdingItemItemValue.MaxUseTimes;
			if (maxUseTimes > 0 && forId.MaxUseTimesBreaksAfter.Value && holdingItemItemValue.UseTimes >= (float)maxUseTimes)
			{
				entityPlayerLocal.inventory.DecHoldingItem(1);
				if (forId.Properties.Values.ContainsKey(ItemClass.PropSoundDestroy))
				{
					Manager.BroadcastPlay(entityPlayerLocal, forId.Properties.Values[ItemClass.PropSoundDestroy]);
				}
			}
			entityPlayerLocal.equipment.CheckBreakUseItems();
		}
		if (!windowManager.IsInputActive() && !windowManager.IsModalWindowOpen() && Event.current.rawType == EventType.KeyDown && gameManager.IsEditMode() && entityPlayerLocal.inventory != null)
		{
			gameManager.GetActiveBlockTool().CheckSpecialKeys(Event.current, playerInput);
			if (XUiC_WoPropsPOIMarker.Instance != null)
			{
				XUiC_WoPropsPOIMarker.Instance.CheckSpecialKeys(Event.current, playerInput);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (entityPlayerLocal.inventory.GetFocusedItemIdx() < 0 || entityPlayerLocal.inventory.GetFocusedItemIdx() >= entityPlayerLocal.inventory.PUBLIC_SLOTS)
		{
			entityPlayerLocal.inventory.SetFocusedItemIdx(0);
			entityPlayerLocal.inventory.SetHoldingItemIdx(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRespawn()
	{
		if (entityPlayerLocal.Spawned)
		{
			if (unstuckCoState == ERoutineState.Running && (playerInput.GUIActions.Cancel.WasPressed || windowManager.IsWindowOpen(XUiC_InGameMenuWindow.ID)))
			{
				unstuckCoState = ERoutineState.Cancelled;
			}
		}
		else
		{
			if (GameManager.IsVideoPlaying())
			{
				return;
			}
			if (!bLastRespawnActive)
			{
				spawnWindowOpened = false;
				spawnPosition = SpawnPosition.Undef;
				entityPlayerLocal.BeforePlayerRespawn(respawnReason);
				bLastRespawnActive = true;
				waitingForSpawnPointSelection = false;
			}
			entityPlayerLocal.ResetLastTickPos(entityPlayerLocal.GetPosition());
			respawnTime -= Time.deltaTime;
			if (respawnTime > 0f)
			{
				return;
			}
			respawnTime = 0f;
			if (spawnWindowOpened && XUiC_SpawnSelectionWindow.IsOpenInUI(LocalPlayerUI.primaryUI))
			{
				if (Mathf.Abs(entityPlayerLocal.GetPosition().y - Constants.cStartPositionPlayerInLevel.y) < 0.01f)
				{
					Vector3 position = entityPlayerLocal.GetPosition();
					Vector3i blockPosition = entityPlayerLocal.GetBlockPosition();
					if (gameManager.World.GetChunkFromWorldPos(blockPosition) != null)
					{
						float num = gameManager.World.GetHeight(blockPosition.x, blockPosition.z) + 1;
						if (position.y < 0f || num < position.y || (num > position.y && num - 2.5f < position.y))
						{
							entityPlayerLocal.SetPosition(new Vector3(entityPlayerLocal.GetPosition().x, num, entityPlayerLocal.GetPosition().z));
						}
					}
				}
				if (playerAutoPilotControllor != null && playerAutoPilotControllor.IsEnabled())
				{
					XUiC_SpawnSelectionWindow.Close(LocalPlayerUI.primaryUI);
				}
				return;
			}
			bool flag = respawnReason == RespawnType.NewGame || respawnReason == RespawnType.EnterMultiplayer || respawnReason == RespawnType.JoinMultiplayer || respawnReason == RespawnType.LoadedGame;
			Entity entity = (entityPlayerLocal.AttachedToEntity ? entityPlayerLocal.AttachedToEntity : entityPlayerLocal);
			switch (respawnReason)
			{
			case RespawnType.Teleport:
				entityPlayerLocal.UpdateRespawn();
				spawnPosition = new SpawnPosition(entity.GetPosition(), entity.rotation.y);
				spawnPosition.position.y = -1f;
				break;
			case RespawnType.LoadedGame:
				if (!spawnWindowOpened)
				{
					spawnPosition = new SpawnPosition(entityPlayerLocal.GetPosition(), entityPlayerLocal.rotation.y);
					entityPlayerLocal.SetPosition(spawnPosition.position);
					openSpawnWindow(respawnReason);
					return;
				}
				spawnPosition = new SpawnPosition(entityPlayerLocal.GetPosition(), entityPlayerLocal.rotation.y);
				break;
			case RespawnType.NewGame:
				if (!spawnWindowOpened)
				{
					openSpawnWindow(respawnReason);
					return;
				}
				spawnPosition = new SpawnPosition(entityPlayerLocal.GetPosition(), entityPlayerLocal.rotation.y);
				break;
			case RespawnType.EnterMultiplayer:
			case RespawnType.JoinMultiplayer:
				if (!spawnWindowOpened)
				{
					spawnPosition = new SpawnPosition(entityPlayerLocal.GetPosition(), entityPlayerLocal.rotation.y);
					if ((spawnPosition.IsUndef() || spawnPosition.position.Equals(Constants.cStartPositionPlayerInLevel)) && !entityPlayerLocal.lastSpawnPosition.IsUndef())
					{
						spawnPosition = entityPlayerLocal.lastSpawnPosition;
					}
					if (spawnPosition.IsUndef() || spawnPosition.position.Equals(Constants.cStartPositionPlayerInLevel))
					{
						spawnPosition = gameManager.GetSpawnPointList().GetRandomSpawnPosition(entityPlayerLocal.world);
					}
					entityPlayerLocal.SetPosition(new Vector3(spawnPosition.position.x, (spawnPosition.position.y == 0f) ? Constants.cStartPositionPlayerInLevel.y : spawnPosition.position.y, spawnPosition.position.z));
					openSpawnWindow(respawnReason);
					return;
				}
				spawnPosition = new SpawnPosition(entityPlayerLocal.GetPosition(), entityPlayerLocal.rotation.y);
				break;
			default:
			{
				if (!gameManager.IsEditMode() && !spawnWindowOpened)
				{
					openSpawnWindow(respawnReason);
					return;
				}
				XUiC_SpawnSelectionWindow window = XUiC_SpawnSelectionWindow.GetWindow(LocalPlayerUI.primaryUI);
				if (!waitingForSpawnPointSelection && !gameManager.IsEditMode() && spawnWindowOpened && window.spawnMethod != SpawnMethod.Invalid)
				{
					StartCoroutine(FindRespawnSpawnPointRoutine(window.spawnMethod, window.spawnTarget));
					window.spawnMethod = SpawnMethod.Invalid;
					window.spawnTarget = SpawnPosition.Undef;
				}
				if (waitingForSpawnPointSelection)
				{
					return;
				}
				if (entityPlayerLocal.position != spawnPosition.position)
				{
					Vector3 position2 = spawnPosition.position;
					if (spawnPosition.IsUndef())
					{
						position2 = entityPlayerLocal.GetPosition();
					}
					spawnPosition = new SpawnPosition(position2 + new Vector3(0f, 5f, 0f), entityPlayerLocal.rotation.y);
					entityPlayerLocal.SetPosition(spawnPosition.position);
				}
				break;
			}
			}
			if (GameUtils.IsPlaytesting() || (GameManager.Instance.IsEditMode() && GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty"))
			{
				SpawnPointList spawnPointList = GameManager.Instance.GetSpawnPointList();
				if (respawnReason != RespawnType.Teleport && spawnPointList.Count > 0)
				{
					spawnPosition.position = spawnPointList[0].spawnPosition.position;
					spawnPosition.heading = spawnPointList[0].spawnPosition.heading;
					entityPlayerLocal.SetPosition(spawnPosition.position);
				}
			}
			if (!spawnPosition.IsUndef())
			{
				if (!PrefabEditModeManager.Instance.IsActive() && !gameManager.World.IsPositionAvailable(spawnPosition.ClrIdx, spawnPosition.position))
				{
					spawnPosition.position = gameManager.World.ClampToValidWorldPos(spawnPosition.position);
					if (entityPlayerLocal.position != spawnPosition.position)
					{
						entityPlayerLocal.SetPosition(spawnPosition.position);
					}
					return;
				}
				if (!entityPlayerLocal.CheckSpawnPointStillThere())
				{
					entityPlayerLocal.RemoveSpawnPoints();
					if (flag)
					{
						entityPlayerLocal.QuestJournal.RemoveAllSharedQuests();
						entityPlayerLocal.QuestJournal.StartQuests();
					}
				}
				Vector3i vector3i = World.worldToBlockPos(spawnPosition.position);
				float num2 = gameManager.World.GetHeight(vector3i.x, vector3i.z) + 1;
				if (spawnPosition.position.y < 0f || spawnPosition.position.y > num2)
				{
					spawnPosition.position.y = num2;
				}
				else if (spawnPosition.position.y < num2 && !gameManager.World.CanPlayersSpawnAtPos(spawnPosition.position, _bAllowToSpawnOnAirPos: true))
				{
					spawnPosition.position.y += 1f;
					if (!gameManager.World.CanPlayersSpawnAtPos(spawnPosition.position, _bAllowToSpawnOnAirPos: true))
					{
						spawnPosition.position.y = num2;
					}
				}
			}
			Log.Out("Respawn almost done");
			if (spawnPosition.IsUndef())
			{
				entityPlayerLocal.Respawn(respawnReason);
				return;
			}
			float num3 = 0f;
			num3 = ((!Physics.Raycast(new Ray(spawnPosition.position + Vector3.up - Origin.position, Vector3.down), out var hitInfo, 3f, 1342242816)) ? (gameManager.World.GetTerrainOffset(0, World.worldToBlockPos(spawnPosition.position)) + 0.05f) : (hitInfo.point.y - spawnPosition.position.y + Origin.position.y));
			gameManager.ClearTooltips(nguiWindowManager);
			spawnPosition.position.y += num3;
			entityPlayerLocal.onGround = true;
			entityPlayerLocal.lastSpawnPosition = spawnPosition;
			entityPlayerLocal.Spawned = true;
			GameManager.Instance.PlayerSpawnedInWorld(null, respawnReason, new Vector3i(spawnPosition.position), entityPlayerLocal.entityId);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToClientsOrServer(NetPackageManager.GetPackage<NetPackagePlayerSpawnedInWorld>().Setup(respawnReason, new Vector3i(spawnPosition.position), entityPlayerLocal.entityId));
			if (respawnReason == RespawnType.Died || respawnReason == RespawnType.EnterMultiplayer || respawnReason == RespawnType.NewGame)
			{
				entityPlayerLocal.SetAlive();
			}
			else
			{
				entityPlayerLocal.bDead = false;
			}
			if (respawnReason == RespawnType.NewGame || respawnReason == RespawnType.LoadedGame || respawnReason == RespawnType.EnterMultiplayer || respawnReason == RespawnType.JoinMultiplayer)
			{
				entityPlayerLocal.TryAddRecoveryPosition(Vector3i.FromVector3Rounded(spawnPosition.position));
			}
			entityPlayerLocal.ResetLastTickPos(spawnPosition.position);
			if (!entityPlayerLocal.AttachedToEntity)
			{
				entityPlayerLocal.transform.position = spawnPosition.position - Origin.position;
			}
			else
			{
				spawnPosition.position.y += 2f;
			}
			entity.SetPosition(spawnPosition.position);
			entity.SetRotation(new Vector3(0f, spawnPosition.heading, 0f));
			entityPlayerLocal.JetpackWearing = false;
			entityPlayerLocal.ParachuteWearing = false;
			entityPlayerLocal.AfterPlayerRespawn(respawnReason);
			if (flag)
			{
				entityPlayerLocal.QuestJournal.RemoveAllSharedQuests();
				entityPlayerLocal.QuestJournal.StartQuests();
			}
			if ((respawnReason == RespawnType.NewGame || respawnReason == RespawnType.EnterMultiplayer) && !GameManager.Instance.World.IsEditor() && !(GameMode.GetGameModeForId(GameStats.GetInt(EnumGameStats.GameModeId)) is GameModeCreative) && !GameUtils.IsPlaytesting() && !GameManager.bRecordNextSession && !GameManager.bPlayRecordedSession)
			{
				GameEventManager.Current.HandleAction("game_first_spawn", entityPlayerLocal, entityPlayerLocal, twitchActivated: false);
			}
			if (respawnReason != RespawnType.Died && respawnReason != RespawnType.Teleport && GameStats.GetBool(EnumGameStats.AutoParty) && entityPlayerLocal.Party == null)
			{
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.JoinAutoParty, entityPlayerLocal.entityId, entityPlayerLocal.entityId));
				}
				else
				{
					Party.ServerHandleAutoJoinParty(entityPlayerLocal);
				}
			}
			if (respawnReason == RespawnType.JoinMultiplayer || respawnReason == RespawnType.LoadedGame)
			{
				entityPlayerLocal.ReassignEquipmentTransforms();
				GameEventManager.Current.HandleAction("game_on_spawn", entityPlayerLocal, entityPlayerLocal, twitchActivated: false);
			}
			entityPlayerLocal.EnableCamera(_b: true);
			GameManager.Instance.World.RefreshEntitiesOnMap();
			LocalPlayerUI.primaryUI.windowManager.Close(XUiC_LoadingScreen.ID);
			LocalPlayerUI.primaryUI.windowManager.Close("eacWarning");
			LocalPlayerUI.primaryUI.windowManager.Close("crossplayWarning");
			if (flag && PlatformManager.NativePlatform.GameplayNotifier != null)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					bool isOnlineMultiplayer = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.Server;
					bool allowsCrossplay = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.AllowsCrossplay;
					PlatformManager.NativePlatform.GameplayNotifier.GameplayStart(isOnlineMultiplayer, allowsCrossplay);
				}
				else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
				{
					bool allowsCrossplay2 = SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.AllowsCrossplay;
					PlatformManager.NativePlatform.GameplayNotifier.GameplayStart(isOnlineMultiplayer: true, allowsCrossplay2);
				}
			}
			if (respawnReason == RespawnType.Died)
			{
				entityPlayerLocal.QuestJournal.FailAllActivatedQuests();
				entityPlayerLocal.Progression.OnRespawnFromDeath();
				switch ((EnumDeathPenalty)GameStats.GetInt(EnumGameStats.DeathPenalty))
				{
				case EnumDeathPenalty.None:
					GameEventManager.Current.HandleAction("game_on_respawn_none", entityPlayerLocal, entityPlayerLocal, twitchActivated: false);
					break;
				case EnumDeathPenalty.XPOnly:
					GameEventManager.Current.HandleAction("game_on_respawn_default", entityPlayerLocal, entityPlayerLocal, twitchActivated: false);
					break;
				case EnumDeathPenalty.Injured:
					GameEventManager.Current.HandleAction("game_on_respawn_injured", entityPlayerLocal, entityPlayerLocal, twitchActivated: false);
					break;
				case EnumDeathPenalty.Permadeath:
					GameEventManager.Current.HandleAction("game_on_respawn_permanent", entityPlayerLocal, entityPlayerLocal, twitchActivated: false);
					break;
				}
				entityPlayerLocal.ResetBiomeWeatherOnDeath();
			}
			if (!gameManager.IsEditMode() && (respawnReason == RespawnType.NewGame || respawnReason == RespawnType.EnterMultiplayer))
			{
				windowManager.TempHUDDisable();
				entityPlayerLocal.SetControllable(_b: false);
				entityPlayerLocal.bIntroAnimActive = true;
				GameManager.Instance.StartCoroutine(showUILater());
				if (!GameUtils.IsPlaytesting())
				{
					GameManager.Instance.StartCoroutine(initializeHoldingItemLater(4f));
				}
			}
			else
			{
				entityPlayerLocal.SetControllable(_b: true);
				GameManager.Instance.StartCoroutine(initializeHoldingItemLater(0.1f));
			}
			bLastRespawnActive = false;
		}
	}

	public IEnumerator UnstuckPlayerCo()
	{
		if (!entityPlayerLocal.Spawned || unstuckCoState == ERoutineState.Running)
		{
			yield break;
		}
		unstuckCoState = ERoutineState.Running;
		SpawnPosition spawnTarget = new SpawnPosition(entityPlayerLocal.GetPosition(), entityPlayerLocal.rotation.y);
		GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("xuiMenuUnstuckTooltip"), 5), _showImmediately: true);
		DateTime currentTime = DateTime.Now;
		yield return FindRespawnSpawnPointRoutine(SpawnMethod.Unstuck, spawnTarget);
		if (unstuckCoState == ERoutineState.Cancelled)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("xuiMenuUnstuckCancelled"), _showImmediately: true);
			yield break;
		}
		double remainingTime = 5.0 - (DateTime.Now - currentTime).TotalSeconds;
		while (remainingTime > 0.0)
		{
			GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("xuiMenuUnstuckTooltip"), (int)(remainingTime + 0.5)), _showImmediately: true);
			yield return new WaitForSeconds(Math.Min(1f, (float)remainingTime));
			remainingTime -= 1.0;
			if (unstuckCoState == ERoutineState.Cancelled)
			{
				GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("xuiMenuUnstuckCancelled"), _showImmediately: true);
				yield break;
			}
		}
		if (!waitingForSpawnPointSelection && !spawnPosition.IsUndef())
		{
			entityPlayerLocal.TeleportToPosition(spawnPosition.position);
		}
		unstuckCoState = ERoutineState.Succeeded;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator initializeHoldingItemLater(float _time)
	{
		yield return new WaitForSeconds(_time);
		if (entityPlayerLocal != null && entityPlayerLocal.inventory != null)
		{
			entityPlayerLocal.inventory.ForceHoldingItemUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator showUILater()
	{
		yield return new WaitForSeconds(4f);
		if (entityPlayerLocal != null && entityPlayerLocal.transform != null)
		{
			entityPlayerLocal.bIntroAnimActive = false;
			entityPlayerLocal.SetControllable(_b: true);
		}
		if (windowManager != null)
		{
			windowManager.ReEnableHUD();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openSpawnWindow(RespawnType _respawnReason)
	{
		Log.Out("OpenSpawnWindow");
		XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
		if (!gameManager.IsEditMode())
		{
			LocalPlayerUI.primaryUI.windowManager.Open(XUiC_LoadingScreen.ID, _bModal: false, _bIsNotEscClosable: true);
		}
		XUiC_SpawnSelectionWindow.Open(LocalPlayerUI.primaryUI, _respawnReason != RespawnType.EnterMultiplayer && _respawnReason != RespawnType.JoinMultiplayer && _respawnReason != RespawnType.NewGame && _respawnReason != RespawnType.Teleport && _respawnReason != RespawnType.LoadedGame, _enteringGame: false);
		spawnWindowOpened = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPosition findSpawnPosition(SpawnMethod _spawnMethod, SpawnPosition _spawnTarget)
	{
		SpawnPosition spawnPosition = SpawnPosition.Undef;
		if (_spawnMethod == SpawnMethod.OnBedRoll && spawnPosition.IsUndef())
		{
			spawnPosition = _spawnTarget;
			if (!spawnPosition.IsUndef())
			{
				string text = $"Spawn pos: {SpawnMethod.OnBedRoll} ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(text + spawnPosition2.ToString());
			}
		}
		if ((_spawnMethod == SpawnMethod.NearBedroll || _spawnMethod == SpawnMethod.NearBackpack || _spawnMethod == SpawnMethod.NearDeath) && spawnPosition.IsUndef() && !_spawnTarget.IsUndef())
		{
			if (gameManager.World.GetRandomSpawnPositionMinMaxToPosition(_spawnTarget.position, 48, 96, 48, _checkBedrolls: false, out var _position, entityPlayerLocal.entityId))
			{
				spawnPosition.position = _position;
				string text2 = $"Spawn pos: random {_spawnMethod} ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(text2 + spawnPosition2.ToString());
			}
			if (spawnPosition.IsUndef() && entityPlayerLocal.recoveryPositions.Count > 0)
			{
				for (int num = entityPlayerLocal.recoveryPositions.Count - 1; num >= 0; num--)
				{
					if (Vector3.Distance(entityPlayerLocal.recoveryPositions[num], _spawnTarget.position) > 48f)
					{
						spawnPosition.position = entityPlayerLocal.recoveryPositions[num];
						SpawnPosition spawnPosition2 = spawnPosition;
						Log.Out("Spawn pos: Recovery Point " + spawnPosition2.ToString());
						break;
					}
				}
			}
		}
		if (_spawnMethod == SpawnMethod.NewRandomSpawn && spawnPosition.IsUndef() && !_spawnTarget.IsUndef())
		{
			spawnPosition = _spawnTarget;
			string text3 = $"Spawn pos: random {_spawnMethod} ";
			SpawnPosition spawnPosition2 = spawnPosition;
			Log.Out(text3 + spawnPosition2.ToString());
		}
		if (_spawnMethod == SpawnMethod.Unstuck && spawnPosition.IsUndef() && !_spawnTarget.IsUndef())
		{
			if (gameManager.World.GetRandomSpawnPositionMinMaxToPosition(_spawnTarget.position, 0, 16, 0, _checkBedrolls: false, out var _position2, entityPlayerLocal.entityId, _checkWater: true, 100))
			{
				spawnPosition.position = _position2;
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out("Spawn pos: try 'unstuck' player " + spawnPosition2.ToString());
			}
			if (spawnPosition.IsUndef() && entityPlayerLocal.recoveryPositions.Count >= 2)
			{
				spawnPosition.position = entityPlayerLocal.recoveryPositions[entityPlayerLocal.recoveryPositions.Count - 2];
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out("Spawn pos: try 'unstuck' player at Recovery Point " + spawnPosition2.ToString());
			}
		}
		if (spawnPosition.IsUndef())
		{
			if (!_spawnTarget.IsUndef())
			{
				spawnPosition = gameManager.GetSpawnPointList().GetRandomSpawnPosition(entityPlayerLocal.world, _spawnTarget.position, 300, 600);
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out("Spawn pos: start point " + spawnPosition2.ToString() + " distance to backpack: " + (spawnPosition.position - _spawnTarget.position).magnitude.ToCultureInvariantString());
			}
			else
			{
				spawnPosition = gameManager.GetSpawnPointList().GetRandomSpawnPosition(entityPlayerLocal.world, entityPlayerLocal.position, 300, 600);
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out("Spawn pos: start point " + spawnPosition2.ToString());
			}
		}
		if (spawnPosition.IsUndef())
		{
			int x = Utils.Fastfloor(entityPlayerLocal.position.x);
			int y = Utils.Fastfloor(entityPlayerLocal.position.y);
			int z = Utils.Fastfloor(entityPlayerLocal.position.z);
			IChunk chunkFromWorldPos = gameManager.World.GetChunkFromWorldPos(x, y, z);
			if (chunkFromWorldPos != null)
			{
				if (entityPlayerLocal.position.y == Constants.cStartPositionPlayerInLevel.y)
				{
					entityPlayerLocal.position.y = chunkFromWorldPos.GetHeight(ChunkBlockLayerLegacy.CalcOffset(x, z)) + 1;
				}
				spawnPosition = new SpawnPosition(entityPlayerLocal.GetPosition(), entityPlayerLocal.rotation.y);
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out("Spawn pos: current player pos " + spawnPosition2.ToString());
			}
		}
		return spawnPosition;
	}

	public IEnumerator FindRespawnSpawnPointRoutine(SpawnMethod _method, SpawnPosition _spawnTarget)
	{
		waitingForSpawnPointSelection = true;
		Vector3 targetPosition = entityPlayerLocal.position;
		if (!_spawnTarget.IsUndef())
		{
			targetPosition = _spawnTarget.position;
		}
		entityPlayerLocal.SetPosition(targetPosition);
		yield return new WaitForSeconds(2f);
		float waitTime = 0f;
		while (!GameManager.Instance.World.IsChunkAreaLoaded(targetPosition) && waitTime < 5f)
		{
			yield return new WaitForSeconds(0.25f);
			waitTime += 0.25f;
		}
		spawnPosition = findSpawnPosition(_method, _spawnTarget);
		waitingForSpawnPointSelection = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopMoving()
	{
		isAutorun = false;
		entityPlayerLocal.movementInput.moveForward = 0f;
		entityPlayerLocal.movementInput.moveStrafe = 0f;
		entityPlayerLocal.MoveByInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDebugKeys()
	{
		if (windowManager.IsModalWindowOpen() || !GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			return;
		}
		bool flag = playerUI.windowManager.IsInputActive();
		bool num = flag || wasUIInputActive;
		wasUIInputActive = flag;
		if (num)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Keypad0))
		{
			Manager.PlayButtonClick();
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AINameInfoServerToggle));
			}
			else
			{
				bool flag2 = !GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks);
				GamePrefs.Set(EnumGamePrefs.DebugMenuShowTasks, flag2);
				EntityAlive.SetupAllDebugNameHUDs(flag2);
			}
		}
		if (gameManager.World.IsRemote())
		{
			return;
		}
		bool shiftKeyPressed = InputUtils.ShiftKeyPressed;
		if (!shiftKeyPressed)
		{
			float num2 = Time.timeScale;
			if (Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				num2 = ((!(num2 > 0f)) ? 1f : 0f);
			}
			if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				num2 = Mathf.Max(num2 - 0.05f, 0f);
			}
			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				num2 = Mathf.Min(num2 + 0.05f, 2f);
			}
			if (num2 != Time.timeScale)
			{
				Time.timeScale = num2;
				Log.Out("Time scale {0}", num2.ToCultureInvariantString());
				Manager.PlayButtonClick();
			}
			if (Input.GetKeyDown(KeyCode.KeypadPeriod))
			{
				if (InputUtils.ControlKeyPressed)
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync("killall all", null);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup("killall all"));
					}
				}
				else
				{
					float num3 = (InputUtils.AltKeyPressed ? 100 : 0);
					entityPlayerLocal.emodel.DoRagdoll(2f, EnumBodyPartHit.Torso, entityPlayerLocal.rand.RandomInsideUnitSphere * num3, entityPlayerLocal.transform.position + entityPlayerLocal.rand.RandomInsideUnitSphere * 0.1f + Origin.position, isRemote: false);
				}
			}
		}
		else if (Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			drawChunkMode = (drawChunkMode + 1) % 3;
		}
		if (!playerInput.AiFreeze.WasPressed || GameManager.Instance.IsEditMode())
		{
			return;
		}
		Manager.PlayButtonClick();
		if (InputUtils.ControlKeyPressed)
		{
			EAIManager.ToggleAnimFreeze();
			if (EAIManager.isAnimFreeze)
			{
				entityPlayerLocal.Buffs.AddBuff("buffShowAnimationDisabled");
			}
			else
			{
				entityPlayerLocal.Buffs.RemoveBuff("buffShowAnimationDisabled");
			}
			return;
		}
		if (shiftKeyPressed)
		{
			entityPlayerLocal.SetIgnoredByAI(!entityPlayerLocal.IsIgnoredByAI());
			return;
		}
		bool flag3 = !GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving);
		GamePrefs.Set(EnumGamePrefs.DebugStopEnemiesMoving, flag3);
		if (flag3)
		{
			entityPlayerLocal.Buffs.AddBuff("buffShowAIDisabled");
		}
		else
		{
			entityPlayerLocal.Buffs.RemoveBuff("buffShowAIDisabled");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canSwapHeldItem()
	{
		ItemActionEat itemActionEat = entityPlayerLocal.inventory.holdingItem.Actions[0] as ItemActionEat;
		bool flag = false;
		if (itemActionEat != null)
		{
			flag = itemActionEat.PercentDone(entityPlayerLocal.inventory.holdingItemData.actionData[0]) > 0.75f;
		}
		if (entityPlayerLocal.inventory.GetIsFinishedSwitchingHeldItem())
		{
			return !flag;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (nextHeldItem.Count > 0 && canSwapHeldItem())
		{
			swapItem(nextHeldItem[nextHeldItem.Count - 1]);
			nextHeldItem.Clear();
		}
		PlayerActionsLocal playerActionsLocal = playerInput;
		bool flag = !playerUI.windowManager.IsCursorWindowOpen() && !playerUI.windowManager.IsModalWindowOpen() && (playerActionsLocal.Enabled || playerActionsLocal.VehicleActions.Enabled);
		if (DroneManager.Debug_LocalControl)
		{
			flag = false;
		}
		if (playerAutoPilotControllor != null && playerAutoPilotControllor.IsEnabled())
		{
			playerAutoPilotControllor.Update();
		}
		if (!(bCanControlOverride && flag) && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 0)
		{
			XUiC_InteractionPrompt.SetText(playerUI, null);
			strTextLabelPointingTo = string.Empty;
		}
		if (!gameManager.gameStateManager.IsGameStarted() || GameStats.GetInt(EnumGameStats.GameState) != 1)
		{
			stopMoving();
			return;
		}
		if (entityPlayerLocal.PlayerUI.windowManager.IsModalWindowOpen())
		{
			if (!IsGUICancelPressed && playerActionsLocal.PermanentActions.Cancel.WasPressed)
			{
				IsGUICancelPressed = true;
			}
		}
		else if (IsGUICancelPressed)
		{
			IsGUICancelPressed = playerActionsLocal.PermanentActions.Cancel.GetBindingOfType(playerActionsLocal.ActiveDevice.DeviceClass == InputDeviceClass.Controller).GetState(playerActionsLocal.ActiveDevice);
		}
		updateRespawn();
		if (unstuckCoState == ERoutineState.Running)
		{
			stopMoving();
			return;
		}
		updateDebugKeys();
		if (drawChunkMode > 0)
		{
			DrawChunkBoundary();
			if (drawChunkMode == 2)
			{
				DrawChunkDensities();
			}
		}
		if (entityPlayerLocal.emodel.IsRagdollActive)
		{
			stopMoving();
			return;
		}
		if (entityPlayerLocal.IsDead())
		{
			XUiC_InteractionPrompt.SetText(playerUI, null);
			strTextLabelPointingTo = string.Empty;
			return;
		}
		bool flag2 = false;
		float num = playerActionsLocal.Scroll.Value;
		if (playerActionsLocal.LastInputType == BindingSourceType.DeviceBindingSource)
		{
			num = (entityPlayerLocal.AimingGun ? (num * 0.25f) : 0f);
		}
		num *= 0.25f;
		if (Mathf.Abs(num) < 0.001f)
		{
			num = 0f;
		}
		gameManager.GetActiveBlockTool().CheckKeys(entityPlayerLocal.inventory.holdingItemData, entityPlayerLocal.HitInfo, playerActionsLocal);
		if (gameManager.IsEditMode() || BlockToolSelection.Instance.SelectionActive)
		{
			SelectionBoxManager.Instance.CheckKeys(gameManager, playerActionsLocal, entityPlayerLocal.HitInfo);
			if (!flag2)
			{
				flag2 = SelectionBoxManager.Instance.ConsumeScrollWheel(num, playerActionsLocal);
			}
			flag2 = gameManager.GetActiveBlockTool().ConsumeScrollWheel(entityPlayerLocal.inventory.holdingItemData, num, playerActionsLocal);
		}
		bool flag3 = GameManager.Instance.isAnyCursorWindowOpen();
		bool flag4 = (playerActionsLocal.QuickMenu.IsPressed || playerActionsLocal.PermanentActions.QuickMenu.IsPressed) && entityPlayerLocal.PlayerUI.windowManager.IsWindowOpen(XUiC_Radial.ID);
		bool flag5 = playerActionsLocal.CameraChange.IsPressed && !flag3;
		if (cameraChangePressTime < 0f)
		{
			if (flag4 || flag5)
			{
				cameraChangePressTime = Time.time;
			}
			else
			{
				cameraChangePressTime = -1f;
			}
		}
		if (playerActionsLocal.CameraChange.WasReleased && !flag3)
		{
			if (!wasCameraChangeUsedWithWheel)
			{
				cameraChangeRequested = true;
			}
			wasCameraChangeUsedWithWheel = false;
		}
		if (!entityPlayerLocal.AimingGun && (playerActionsLocal.CameraChange.IsPressed || playerActionsLocal.QuickMenu.IsPressed || playerActionsLocal.PermanentActions.QuickMenu.IsPressed))
		{
			if (cameraChangePressTime > 0f && Time.time - cameraChangePressTime > 0.5f)
			{
				entityPlayerLocal.ClearMovementInputs();
			}
			if (flag4 || flag5)
			{
				float num2 = playerActionsLocal.PermanentActions.CameraZoom.Value * 0.25f;
				if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
				{
					num2 *= 0.1f;
				}
				if (entityPlayerLocal.TryUpdateCameraDistanceMultiplier(0f - num2))
				{
					wasCameraChangeUsedWithWheel = true;
					cameraChangeRequested = false;
				}
				entityPlayerLocal.PlayerUI.xui.calloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.CameraZoom, _enabled: true);
				return;
			}
			wasCameraChangeUsedWithWheel = false;
		}
		entityPlayerLocal.PlayerUI.xui.calloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.CameraZoom, _enabled: false);
		if (!flag2 && !flag3 && !entityPlayerLocal.AimingGun && (playerInput.QuickMenu.WasPressed || playerInput.PermanentActions.QuickMenu.WasPressed))
		{
			if (!GameManager.Instance.IsEditMode())
			{
				playerUI.xui.RadialWindow.Open();
				playerUI.xui.RadialWindow.SetupQuickActionsMenu(entityPlayerLocal);
				return;
			}
			entityPlayerLocal.SwitchFirstPersonViewFromInput();
		}
		if (!(bCanControlOverride && flag) && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 0)
		{
			stopMoving();
			return;
		}
		bool lastInputController = playerActionsLocal.LastInputType == BindingSourceType.DeviceBindingSource;
		entityPlayerLocal.movementInput.lastInputController = lastInputController;
		if (!IsGUICancelPressed && (!gameManager.IsEditMode() || GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 0))
		{
			bool controlKeyPressed = InputUtils.ControlKeyPressed;
			bool flag6 = playerActionsLocal.VehicleActions.Enabled;
			bool flag7 = wasVehicle != flag6;
			PlayerAction playerAction = (flag6 ? playerActionsLocal.VehicleActions.Turbo : playerActionsLocal.Run);
			PlayerAction playerAction2 = (flag6 ? playerActionsLocal.VehicleActions.MoveForward : playerActionsLocal.MoveForward);
			if (sprintMode == 2)
			{
				if (!flag6)
				{
					_ = playerActionsLocal.MoveBack;
				}
				else
				{
					_ = playerActionsLocal.VehicleActions.MoveBack;
				}
				bool isPressed = playerAction.IsPressed;
				bool flag8 = Utils.FastAbs(flag6 ? playerActionsLocal.VehicleActions.Move.Y : playerActionsLocal.Move.Y) >= 0.35f || (flag6 && (bool)playerActionsLocal.VehicleActions.Brake);
				flag8 |= gameManager.IsEditMode();
				if (!isAutorun)
				{
					bool running = entityPlayerLocal.movementInput.running;
					entityPlayerLocal.movementInput.running = isPressed;
					if (isPressed)
					{
						runInputTime += Time.deltaTime;
						if (!running)
						{
							runInputTime = 0f;
						}
						if (flag8)
						{
							isAutorunInvalid = true;
						}
					}
					else if (playerAction.WasReleased)
					{
						if (!isAutorunInvalid && runInputTime < 0.2f)
						{
							isAutorun = true;
							entityPlayerLocal.movementInput.running = true;
						}
						isAutorunInvalid = false;
					}
				}
				else
				{
					if (isPressed)
					{
						isAutorun = false;
						isAutorunInvalid = true;
					}
					if (flag8)
					{
						isAutorun = false;
					}
				}
			}
			else
			{
				if (playerAction.WasPressed)
				{
					runInputTime = 0f;
					entityPlayerLocal.movementInput.running = true;
					runPressedWhileActive = true;
				}
				else if (playerAction.WasReleased && runPressedWhileActive)
				{
					if (runInputTime > 0.2f)
					{
						entityPlayerLocal.movementInput.running = false;
						runToggleActive = false;
					}
					else if (runToggleActive)
					{
						runToggleActive = false;
						entityPlayerLocal.movementInput.running = false;
					}
					else if (playerAction2.IsPressed || sprintMode == 1)
					{
						runToggleActive = true;
					}
					else
					{
						runToggleActive = false;
						entityPlayerLocal.movementInput.running = false;
					}
					runPressedWhileActive = false;
				}
				if (playerAction.IsPressed)
				{
					runInputTime += Time.deltaTime;
				}
				if (runToggleActive)
				{
					if (flag7 || (sprintMode == 0 && (entityPlayerLocal.Stamina <= 0f || playerAction2.WasReleased)))
					{
						runToggleActive = false;
						runPressedWhileActive = false;
						entityPlayerLocal.movementInput.running = false;
					}
					else
					{
						entityPlayerLocal.movementInput.running = true;
					}
				}
			}
			entityPlayerLocal.movementInput.down = playerActionsLocal.Crouch.IsPressed && !(gameManager.IsEditMode() && controlKeyPressed);
			entityPlayerLocal.movementInput.jump = playerActionsLocal.Jump.IsPressed;
			if (entityPlayerLocal.movementInput.running && entityPlayerLocal.AimingGun)
			{
				entityPlayerLocal.AimingGun = false;
			}
			wasVehicle = flag6;
		}
		else
		{
			runToggleActive = false;
			entityPlayerLocal.movementInput.running = false;
			runPressedWhileActive = false;
			isAutorun = false;
		}
		entityPlayerLocal.movementInput.downToggle = !gameManager.IsEditMode() && !entityPlayerLocal.IsFlyMode.Value && playerActionsLocal.ToggleCrouch.WasPressed;
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && playerActionsLocal.PermanentActions.DebugControllerLeft.IsPressed && playerActionsLocal.PermanentActions.DebugControllerRight.IsPressed)
		{
			if (playerActionsLocal.GodAlternate.WasPressed)
			{
				toggleGodMode();
			}
			if (playerActionsLocal.TeleportAlternate.WasPressed)
			{
				teleportPlayer();
			}
		}
		if (playerActionsLocal.DecSpeed.WasPressed)
		{
			entityPlayerLocal.GodModeSpeedModifier = Utils.FastMax(0.1f, entityPlayerLocal.GodModeSpeedModifier - 0.1f);
		}
		if (playerActionsLocal.IncSpeed.WasPressed)
		{
			entityPlayerLocal.GodModeSpeedModifier = Utils.FastMin(3f, entityPlayerLocal.GodModeSpeedModifier + 0.1f);
		}
		Vector2 vector = default(Vector2);
		Vector2 vector3;
		if (playerActionsLocal.Look.LastInputType != BindingSourceType.MouseBindingSource)
		{
			entityPlayerLocal.movementInput.down = entityPlayerLocal.IsFlyMode.Value && playerActionsLocal.ToggleCrouch.IsPressed;
			float magnitude;
			if (playerActionsLocal.VehicleActions.Enabled)
			{
				vector.x = playerActionsLocal.VehicleActions.Look.X;
				vector.y = playerActionsLocal.VehicleActions.Look.Y * (float)invertController;
				magnitude = playerActionsLocal.VehicleActions.Look.Vector.magnitude;
			}
			else
			{
				vector.x = playerActionsLocal.Look.X;
				vector.y = playerActionsLocal.Look.Y * (float)invertController;
				magnitude = playerActionsLocal.Look.Vector.magnitude;
			}
			if (lookAccelerationRate <= 0f)
			{
				currentLookAcceleration = 1f;
			}
			else if (magnitude > 0f)
			{
				currentLookAcceleration = Mathf.Clamp(currentLookAcceleration + lookAccelerationRate * magnitude * Time.unscaledDeltaTime, 0f, magnitude);
			}
			else
			{
				currentLookAcceleration = 0f;
			}
			Vector2 vector2 = controllerLookSensitivity;
			if (entityPlayerLocal.AimingGun)
			{
				vector2 *= controllerZoomSensitivity;
			}
			else if (playerActionsLocal.VehicleActions.Enabled)
			{
				vector2 *= controllerVehicleSensitivity;
			}
			vector3 = vector2 * lookAccelerationCurve.Evaluate(currentLookAcceleration);
			if (entityPlayerLocal.AimingGun)
			{
				float num3 = Mathf.Lerp(0.2f, 1f, (entityPlayerLocal.playerCamera.fieldOfView - 10f) / ((float)Constants.cDefaultCameraFieldOfView - 10f));
				vector3 *= num3;
			}
			if (entityPlayerLocal.AttachedToEntity != null)
			{
				aimAssistSlowAmount = 1f;
			}
			else
			{
				bool flag9 = false;
				WorldRayHitInfo hitInfo = entityPlayerLocal.HitInfo;
				if (hitInfo.bHitValid)
				{
					if ((bool)hitInfo.transform)
					{
						Transform hitRootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform);
						if (hitRootTransform != null)
						{
							EntityItem component2;
							if (hitRootTransform.TryGetComponent<EntityAlive>(out var component) && component.IsAlive() && component.IsValidAimAssistSlowdownTarget && hitInfo.hit.distanceSq <= 50f && (entityPlayerLocal.inventory.holdingItem.Actions[0] is ItemActionAttack || entityPlayerLocal.inventory.holdingItem.Actions[0] is ItemActionDynamicMelee))
							{
								bAimAssistTargetingItem = false;
								flag9 = true;
							}
							else if ((hitInfo.tag.StartsWith("Item", StringComparison.Ordinal) || hitRootTransform.TryGetComponent<EntityItem>(out component2)) && hitInfo.hit.distanceSq <= 10f)
							{
								bAimAssistTargetingItem = true;
								flag9 = true;
							}
						}
					}
					else if (entityPlayerLocal.ThreatLevel.Numeric < 0.75f && GameUtils.IsBlockOrTerrain(hitInfo.tag) && entityPlayerLocal.PlayerUI.windowManager.IsWindowOpen("interactionPrompt"))
					{
						BlockValue blockValue = hitInfo.hit.blockValue;
						if (!blockValue.Block.isMultiBlock && !blockValue.Block.isOversized && blockValue.Block.shape is BlockShapeModelEntity)
						{
							bAimAssistTargetingItem = true;
							flag9 = true;
						}
					}
				}
				if (flag9)
				{
					aimAssistSlowAmount = (bAimAssistTargetingItem ? 0.6f : 0.5f);
				}
				else
				{
					aimAssistSlowAmount = Mathf.MoveTowards(aimAssistSlowAmount, 1f, Time.unscaledDeltaTime * 5f);
				}
				vector3 *= aimAssistSlowAmount;
				if (controllerAimAssistsEnabled && cameraSnapTargetEntity != null && cameraSnapTargetEntity.IsAlive() && Time.time - cameraSnapTime < 0.3f)
				{
					Vector2 vector4 = Vector2.one * 0.5f;
					Vector2 vector5 = (Vector2)(snapTargetingHead ? entityPlayerLocal.playerCamera.WorldToViewportPoint(cameraSnapTargetEntity.emodel.GetHeadTransform().position) : entityPlayerLocal.playerCamera.WorldToViewportPoint(cameraSnapTargetEntity.GetChestTransformPosition())) - vector4;
					float num4 = ((cameraSnapMode == eCameraSnapMode.MeleeAttack) ? 1.5f : 1f);
					vector += vector5.normalized * num4 * vector5.magnitude / 0.15f;
				}
			}
		}
		else
		{
			vector3 = mouseLookSensitivity;
			Vector2 vector6 = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (float)invertMouse);
			vector = (vector6 + previousMouseInput) / 2f;
			previousMouseInput = vector6;
			if (playerActionsLocal.VehicleActions.Enabled)
			{
				vector3 *= vehicleLookSensitivity;
			}
			else
			{
				float magnitude2 = vector.magnitude;
				float num5 = 1f;
				if (entityPlayerLocal.AimingGun && magnitude2 > 0f)
				{
					vector3 *= mouseZoomSensitivity;
					float num6 = Mathf.Pow(magnitude2 * 0.4f, 2.5f) / magnitude2;
					num5 += num6 * zoomAccel;
					num5 *= Mathf.Lerp(0.2f, 1f, (entityPlayerLocal.playerCamera.fieldOfView - 10f) / ((float)Constants.cDefaultCameraFieldOfView - 10f));
					vector3 *= num5;
					if (vector3.magnitude > mouseLookSensitivity.magnitude)
					{
						vector3 = mouseLookSensitivity;
					}
				}
			}
			if (skipMouseLookNextFrame > 0 && (vector.x <= -1f || vector.x >= 1f || vector.y <= -1f || vector.y >= 1f))
			{
				skipMouseLookNextFrame--;
				vector = Vector2.zero;
			}
		}
		MovementInput movementInput = entityPlayerLocal.movementInput;
		if (!movementInput.bDetachedCameraMove)
		{
			PlayerActionsLocal playerActionsLocal2 = playerActionsLocal;
			if (playerAutoPilotControllor != null && playerAutoPilotControllor.IsEnabled())
			{
				movementInput.moveForward = playerAutoPilotControllor.GetForwardMovement();
			}
			else
			{
				movementInput.moveForward = playerActionsLocal2.Move.Y;
				if (isAutorun)
				{
					movementInput.moveForward = 1f;
				}
			}
			movementInput.moveStrafe = playerActionsLocal2.Move.X;
			if (movementInput.bCameraPositionLocked)
			{
				vector = Vector2.zero;
			}
			float num7 = Utils.FastMin(1f / 30f, Time.unscaledDeltaTime);
			if (useScaledMouseLook && !entityPlayerLocal.movementInput.lastInputController)
			{
				float num8 = num7 * mouseDeltaTimeScale;
				movementInput.rotation.x += vector.y * vector3.y * num8;
				movementInput.rotation.y += vector.x * vector3.x * num8;
			}
			else if (entityPlayerLocal.movementInput.lastInputController)
			{
				float num9 = num7 * lookDeltaTimeScale;
				movementInput.rotation.x += vector.y * vector3.y * num9;
				movementInput.rotation.y += vector.x * vector3.x * num9;
			}
			else
			{
				movementInput.rotation.x += vector.y * vector3.y;
				movementInput.rotation.y += vector.x * vector3.x;
			}
			if (cameraChangeRequested)
			{
				if (!wasCameraChangeUsedWithWheel && !playerActionsLocal2.Primary.IsPressed && !playerActionsLocal2.Secondary.IsPressed)
				{
					entityPlayerLocal.SwitchFirstPersonViewFromInput();
					cameraChangeRequested = false;
				}
				wasCameraChangeUsedWithWheel = false;
			}
			if (playerActionsLocal2.CameraFunction.WasPressed && !entityPlayerLocal.inventory.holdingItem.ConsumeCameraFunction(entityPlayerLocal.inventory.holdingItemData) && !entityPlayerLocal.bFirstPersonView && !entityPlayerLocal.isAimingScoped)
			{
				entityPlayerLocal.flipCameraSide = !entityPlayerLocal.flipCameraSide;
			}
			if ((gameManager.IsEditMode() || BlockToolSelection.Instance.SelectionActive) && (Input.GetKey(KeyCode.LeftControl) || GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) != 0))
			{
				movementInput.Clear();
			}
			entityPlayerLocal.MoveByInput();
		}
		else
		{
			float num10 = 0.15f;
			num10 = ((!entityPlayerLocal.movementInput.running) ? (num10 * entityPlayerLocal.GodModeSpeedModifier) : (num10 * 3f));
			if (playerActionsLocal.MoveForward.IsPressed)
			{
				entityPlayerLocal.cameraTransform.position += entityPlayerLocal.cameraTransform.forward * num10;
			}
			if (playerActionsLocal.MoveBack.IsPressed)
			{
				entityPlayerLocal.cameraTransform.position -= entityPlayerLocal.cameraTransform.forward * num10;
			}
			if (playerActionsLocal.MoveLeft.IsPressed)
			{
				entityPlayerLocal.cameraTransform.position -= entityPlayerLocal.cameraTransform.right * num10;
			}
			if (playerActionsLocal.MoveRight.IsPressed)
			{
				entityPlayerLocal.cameraTransform.position += entityPlayerLocal.cameraTransform.right * num10;
			}
			if (playerActionsLocal.Jump.IsPressed)
			{
				entityPlayerLocal.cameraTransform.position += Vector3.up * num10;
			}
			if (playerActionsLocal.Crouch.IsPressed)
			{
				entityPlayerLocal.cameraTransform.position -= Vector3.up * num10;
			}
			if (!movementInput.bCameraPositionLocked)
			{
				Vector3 localEulerAngles = entityPlayerLocal.cameraTransform.localEulerAngles;
				entityPlayerLocal.cameraTransform.localEulerAngles = new Vector3(localEulerAngles.x - vector.y, localEulerAngles.y + vector.x, localEulerAngles.z);
			}
		}
		bool flag10 = gameManager.IsEditMode() && playerActionsLocal.Run.IsPressed;
		Ray ray = entityPlayerLocal.GetLookRay();
		if (gameManager.IsEditMode() && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 4)
		{
			ray = entityPlayerLocal.cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
			ray.origin += Origin.position;
		}
		ray.origin += ray.direction.normalized * 0.1f;
		float num11 = Utils.FastMax(Utils.FastMax(Constants.cDigAndBuildDistance, Constants.cCollectItemDistance), 30f);
		RaycastHit hitInfo2;
		bool flag11 = Physics.Raycast(new Ray(ray.origin - Origin.position, ray.direction), out hitInfo2, num11, 73728);
		bool flag12 = false;
		if (flag11 && hitInfo2.transform.CompareTag("E_BP_Body"))
		{
			flag12 = true;
		}
		if (flag11)
		{
			flag11 &= hitInfo2.transform.CompareTag("Item");
		}
		int num12 = 69;
		bool flag13 = false;
		if (!gameManager.IsEditMode())
		{
			flag13 = Voxel.Raycast(gameManager.World, ray, num11, -555528213, num12, 0f);
			if (flag13)
			{
				Transform hitRootTransform2 = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
				Entity component3;
				EntityAlive entityAlive = ((hitRootTransform2 != null && hitRootTransform2.TryGetComponent<Entity>(out component3)) ? (component3 as EntityAlive) : null);
				if (entityAlive == null || !entityAlive.IsDead())
				{
					flag13 = Voxel.Raycast(gameManager.World, ray, num11, -555266069, num12, 0f);
				}
			}
		}
		else
		{
			num12 |= 0x100;
			int num13 = -555266069;
			num13 |= 0x10000000;
			if (!GameManager.bVolumeBlocksEditing)
			{
				num13 = int.MinValue;
			}
			flag13 = Voxel.RaycastOnVoxels(gameManager.World, ray, num11, num13, num12, 0f);
			if (flag13 && !GameManager.bVolumeBlocksEditing)
			{
				Voxel.voxelRayHitInfo.lastBlockPos = Vector3i.zero;
				Voxel.voxelRayHitInfo.hit.voxelData.Clear();
				Voxel.voxelRayHitInfo.hit.blockPos = Vector3i.zero;
			}
		}
		WorldRayHitInfo hitInfo3 = entityPlayerLocal.HitInfo;
		_ = Vector3i.zero;
		Vector3i vector3i = Vector3i.zero;
		if (flag13)
		{
			hitInfo3.CopyFrom(Voxel.voxelRayHitInfo);
			vector3i = hitInfo3.hit.blockPos;
			_ = hitInfo3.lastBlockPos;
			hitInfo3.bHitValid = true;
		}
		else
		{
			hitInfo3.bHitValid = false;
		}
		if (!hitInfo3.hit.blockValue.isair)
		{
			Block block = hitInfo3.hit.blockValue.Block;
			if (!block.IsCollideMovement || block.CanBlocksReplace)
			{
				hitInfo3.lastBlockPos = vector3i;
			}
		}
		float num14 = (flag11 ? hitInfo2.distance : 1000f);
		if (flag13 && GameUtils.IsBlockOrTerrain(hitInfo3.tag))
		{
			num14 -= 1.2f;
			if (num14 < 0f)
			{
				num14 = 0.1f;
			}
		}
		if (flag11 && (!flag13 || (flag13 && num14 * num14 <= hitInfo3.hit.distanceSq)))
		{
			hitInfo3.bHitValid = true;
			hitInfo3.tag = "Item";
			hitInfo3.transform = hitInfo2.collider.transform;
			hitInfo3.hit.pos = hitInfo2.point;
			hitInfo3.hit.blockPos = World.worldToBlockPos(hitInfo3.hit.pos);
			hitInfo3.hit.distanceSq = hitInfo2.distance * hitInfo2.distance;
		}
		if (flag12 && hitInfo2.distance * hitInfo2.distance <= hitInfo3.hit.distanceSq)
		{
			hitInfo3.bHitValid = true;
			hitInfo3.tag = "E_BP_Body";
			hitInfo3.transform = hitInfo2.collider.transform;
			hitInfo3.hit.pos = hitInfo2.point;
			hitInfo3.hit.blockPos = World.worldToBlockPos(hitInfo3.hit.pos);
			hitInfo3.hit.distanceSq = hitInfo2.distance * hitInfo2.distance;
		}
		bool flag14 = true;
		if ((bool)hitInfo3.hitCollider && hitInfo3.hitCollider.TryGetComponent<EntityCollisionRules>(out var component4) && !component4.IsInteractable)
		{
			flag14 = false;
		}
		if (entityPlayerLocal.inventory != null && entityPlayerLocal.inventory.holdingItemData != null)
		{
			entityPlayerLocal.inventory.holdingItemData.hitInfo = entityPlayerLocal.HitInfo;
		}
		TileEntity tileEntity = null;
		EntityTurret entityTurret = null;
		bool flag15 = true;
		bool flag16 = true;
		bool flag17 = playerActionsLocal.Primary.IsPressed && bAllowPlayerInput && !IsGUICancelPressed;
		bool flag18 = playerActionsLocal.Secondary.IsPressed && bAllowPlayerInput && !IsGUICancelPressed;
		if (flag17 && GameManager.Instance.World.IsEditor())
		{
			if (bIgnoreLeftMouseUntilReleased)
			{
				flag17 = false;
			}
		}
		else
		{
			bIgnoreLeftMouseUntilReleased = false;
		}
		bool flag19 = false;
		ITileEntityLootable tileEntityLootable = null;
		string text = null;
		EntityItem entityItem = null;
		BlockValue blockValue2 = BlockValue.Air;
		ProjectileMoveScript projectileMoveScript = null;
		ThrownWeaponMoveScript thrownWeaponMoveScript = null;
		string text2 = null;
		bool flag20 = GameManager.Instance.IsEditMode() && entityPlayerLocal.HitInfo.transform != null && entityPlayerLocal.HitInfo.transform.gameObject.layer == 28;
		Entity entity = null;
		if (entityPlayerLocal.AttachedToEntity == null && flag14)
		{
			if (hitInfo3.bHitValid && (flag20 |= GameUtils.IsBlockOrTerrain(hitInfo3.tag)))
			{
				int activationDistanceSq = hitInfo3.hit.blockValue.Block.GetActivationDistanceSq();
				if (hitInfo3.hit.distanceSq < (float)activationDistanceSq)
				{
					blockValue2 = hitInfo3.hit.blockValue;
					Block block2 = blockValue2.Block;
					BlockValue blockValue3 = blockValue2;
					Vector3i vector3i2 = vector3i;
					if (blockValue3.ischild && block2 != null && block2.multiBlockPos != null)
					{
						vector3i2 = block2.multiBlockPos.GetParentPos(vector3i2, blockValue3);
						blockValue3 = gameManager.World.GetBlock(hitInfo3.hit.clrIdx, vector3i2);
					}
					if (block2.HasBlockActivationCommands(gameManager.World, blockValue3, hitInfo3.hit.clrIdx, vector3i2, entityPlayerLocal))
					{
						text2 = block2.GetActivationText(gameManager.World, blockValue3, hitInfo3.hit.clrIdx, vector3i2, entityPlayerLocal);
						if (text2 != null)
						{
							string arg = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
							text2 = string.Format(text2, arg);
						}
						else if (block2.CustomCmds.Length != 0)
						{
							_ = entityPlayerLocal.playerInput;
							string arg2 = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
							string localizedBlockName = blockValue3.Block.GetLocalizedBlockName();
							text2 = string.Format(Localization.Get("questBlockActivate"), arg2, localizedBlockName);
							text2 = string.Format(text2, arg2);
						}
						tileEntity = gameManager.World.GetTileEntity(hitInfo3.hit.clrIdx, vector3i);
					}
					else if (block2.DisplayInfo == Block.EnumDisplayInfo.Name)
					{
						text2 = block2.GetLocalizedBlockName();
					}
					else if (block2.DisplayInfo == Block.EnumDisplayInfo.Description)
					{
						text2 = Localization.Get(block2.DescriptionKey);
					}
					else if (block2.DisplayInfo == Block.EnumDisplayInfo.Custom)
					{
						text2 = block2.GetCustomDescription(vector3i2, blockValue2);
					}
					if (gameManager.IsEditMode() && flag18 && InputUtils.ShiftKeyPressed && InputUtils.ControlKeyPressed && blockValue2.Block is BlockSpawnEntity)
					{
						windowManager.GetWindow<GUIWindowEditBlockSpawnEntity>(GUIWindowEditBlockSpawnEntity.ID).SetBlockValue(hitInfo3.hit.blockPos, blockValue2);
						windowManager.Open(GUIWindowEditBlockSpawnEntity.ID, _bModal: true);
						flag15 = false;
					}
				}
			}
			else if (hitInfo3.bHitValid && hitInfo3.tag.Equals("Item") && hitInfo3.hit.distanceSq < Constants.cCollectItemDistance * Constants.cCollectItemDistance)
			{
				entityItem = hitInfo3.transform.GetComponent<EntityItem>();
				RootTransformRefEntity component5;
				if (entityItem == null && (component5 = hitInfo3.transform.GetComponent<RootTransformRefEntity>()) != null && component5.RootTransform != null)
				{
					entityItem = component5.RootTransform.GetComponent<EntityItem>();
				}
				if (entityItem != null)
				{
					if (entityItem.onGround && entityItem.CanCollect())
					{
						string localizedItemName = ItemClass.GetForId(entityItem.itemStack.itemValue.type).GetLocalizedItemName();
						string arg3 = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
						text2 = ((entityItem.itemStack.count <= 1) ? string.Format(Localization.Get("itemTooltipFocusedOne"), arg3, localizedItemName) : string.Format(Localization.Get("itemTooltipFocusedSeveral"), arg3, localizedItemName, entityItem.itemStack.count));
					}
				}
				else
				{
					projectileMoveScript = hitInfo3.transform.GetComponent<ProjectileMoveScript>();
					if (projectileMoveScript != null)
					{
						string localizedItemName2 = ItemClass.GetForId(projectileMoveScript.itemValueProjectile.type).GetLocalizedItemName();
						string arg4 = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
						text2 = string.Format(Localization.Get("itemTooltipFocusedOne"), arg4, localizedItemName2);
					}
					thrownWeaponMoveScript = hitInfo3.transform.GetComponent<ThrownWeaponMoveScript>();
					if (thrownWeaponMoveScript != null)
					{
						string localizedItemName3 = ItemClass.GetForId(thrownWeaponMoveScript.itemValueWeapon.type).GetLocalizedItemName();
						string arg5 = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
						text2 = string.Format(Localization.Get("itemTooltipFocusedOne"), arg5, localizedItemName3);
					}
				}
			}
			else if (hitInfo3.bHitValid && hitInfo3.tag.StartsWith("E_") && hitInfo3.hit.distanceSq < Constants.cCollectItemDistance * Constants.cCollectItemDistance)
			{
				Transform hitRootTransform3 = GameUtils.GetHitRootTransform(hitInfo3.tag, hitInfo3.transform);
				if (hitRootTransform3 != null && (entity = hitRootTransform3.GetComponent<Entity>()) != null)
				{
					if ((projectileMoveScript = hitRootTransform3.GetComponentInChildren<ProjectileMoveScript>()) != null)
					{
						if (!entity.IsDead() && entity as EntityPlayer != null && (entity as EntityPlayer).inventory != null && (entity as EntityPlayer).inventory.holdingItem != null && (entity as EntityPlayer).inventory.holdingItem.HasAnyTags(BowTag))
						{
							projectileMoveScript = null;
						}
						if (entity.IsDead())
						{
							string localizedItemName4 = ItemClass.GetForId(projectileMoveScript.itemValueProjectile.type).GetLocalizedItemName();
							string arg6 = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
							text2 = string.Format(Localization.Get("itemTooltipFocusedOne"), arg6, localizedItemName4);
						}
					}
					else if ((thrownWeaponMoveScript = hitRootTransform3.GetComponentInChildren<ThrownWeaponMoveScript>()) != null)
					{
						if (!entity.IsDead() && entity as EntityPlayer != null && (entity as EntityPlayer).inventory != null && (entity as EntityPlayer).inventory.holdingItem != null && (entity as EntityPlayer).inventory.holdingItem.HasAnyTags(BowTag))
						{
							thrownWeaponMoveScript = null;
						}
						if (entity.IsDead())
						{
							string localizedItemName5 = ItemClass.GetForId(thrownWeaponMoveScript.itemValueWeapon.type).GetLocalizedItemName();
							string arg7 = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
							text2 = string.Format(Localization.Get("itemTooltipFocusedOne"), arg7, localizedItemName5);
						}
					}
					else if (entity is EntityNPC && entity.IsAlive())
					{
						tileEntity = gameManager.World.GetTileEntity(entity.entityId) as TileEntityTrader;
						if (tileEntity != null)
						{
							EntityTrader obj = (EntityTrader)entity;
							text2 = string.Format(arg0: playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString(), arg1: obj.LocalizedEntityName, format: Localization.Get("npcTooltipTalk"));
							obj.HandleClientQuests(entityPlayerLocal);
						}
						else
						{
							tileEntity = gameManager.World.GetTileEntity(entity.entityId);
							if (tileEntity != null)
							{
								string arg8 = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
								string localizedEntityName = entity.LocalizedEntityName;
								text2 = string.Format(Localization.Get("npcTooltipTalk"), arg8, localizedEntityName);
								EntityDrone entityDrone = entity as EntityDrone;
								if ((bool)entityDrone && entityDrone.IsLocked() && !entityDrone.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
								{
									text2 = Localization.Get("ttLocked") + "\n" + text2;
								}
							}
						}
					}
					else if (entity as EntityTurret != null)
					{
						entityTurret = entity as EntityTurret;
						if (entityTurret.CanInteract(entityPlayerLocal.entityId))
						{
							string arg9 = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
							string localizedEntityName2 = entity.LocalizedEntityName;
							text2 = string.Format(Localization.Get("turretPickUp"), arg9, localizedEntityName2);
						}
					}
					else if (!string.IsNullOrEmpty(entity.GetLootList()))
					{
						tileEntityLootable = gameManager.World.GetTileEntity(entity.entityId).GetSelfOrFeature<ITileEntityLootable>();
						if (tileEntityLootable != null)
						{
							string localizedEntityName3 = entity.LocalizedEntityName;
							string arg10 = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
							text = localizedEntityName3;
							if (entity is EntityNPC && entity.IsAlive())
							{
								text2 = string.Format(Localization.Get("npcTooltipTalk"), arg10, text);
								EntityDrone entityDrone2 = entity as EntityDrone;
								if ((bool)entityDrone2 && entityDrone2.IsLocked() && !entityDrone2.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
								{
									text2 = Localization.Get("ttLocked") + "\n" + text2;
								}
							}
							else if (!(entity is EntityDriveable) || !entity.IsAlive())
							{
								text2 = ((!tileEntityLootable.bTouched) ? string.Format(Localization.Get("lootTooltipNew"), arg10, text) : ((!tileEntityLootable.IsEmpty()) ? string.Format(Localization.Get("lootTooltipTouched"), arg10, text) : string.Format(Localization.Get("lootTooltipEmpty"), arg10, text)));
							}
							else
							{
								text2 = string.Format(Localization.Get("tooltipInteract"), arg10, Localization.Get(text));
								if (((EntityDriveable)entity).IsLockedForLocalPlayer(entityPlayerLocal))
								{
									text2 = Localization.Get("ttLocked") + "\n" + text2;
								}
							}
						}
					}
				}
			}
			if (text2 == null)
			{
				InteractName = null;
				if (entityPlayerLocal.IsMoveStateStill() && (!entityPlayerLocal.IsSwimming() || entityPlayerLocal.cameraTransform.up.y < 0.7f))
				{
					InteractName = entityPlayerLocal.inventory.CanInteract();
					if (InteractName != null && InteractWaitTime == 0f)
					{
						InteractWaitTime = Time.time + 0.3f;
					}
				}
				if (InteractName != null)
				{
					if (Time.time >= InteractWaitTime)
					{
						flag19 = true;
						string arg11 = playerActionsLocal.Activate.GetBindingXuiMarkupString() + playerActionsLocal.PermanentActions.Activate.GetBindingXuiMarkupString();
						text2 = string.Format(Localization.Get("ttPressTo"), arg11, Localization.Get(InteractName));
					}
				}
				else
				{
					InteractWaitTime = 0f;
				}
			}
			else
			{
				InteractWaitTime = 0f;
			}
		}
		if (entityPlayerLocal.IsAlive())
		{
			if (!string.Equals(text2, strTextLabelPointingTo) && (Time.time - timeActivatePressed > 0.5f || string.IsNullOrEmpty(text2)))
			{
				XUiC_InteractionPrompt.SetText(playerUI, text2);
				strTextLabelPointingTo = text2;
			}
		}
		else
		{
			text2 = "";
			strTextLabelPointingTo = text2;
			XUiC_InteractionPrompt.SetText(playerUI, null);
		}
		FocusBoxPosition = hitInfo3.lastBlockPos;
		if (flag10 || (entityPlayerLocal.inventory != null && entityPlayerLocal.inventory.holdingItem.IsFocusBlockInside()))
		{
			FocusBoxPosition = vector3i;
		}
		focusBoxScript.Update(flag20, gameManager.World, hitInfo3, FocusBoxPosition, entityPlayerLocal, gameManager.persistentLocalPlayer, flag10);
		if (!windowManager.IsInputActive() && !windowManager.IsFullHUDDisabled() && (playerActionsLocal.Activate.IsPressed || playerActionsLocal.VehicleActions.Activate.IsPressed || playerActionsLocal.PermanentActions.Activate.IsPressed))
		{
			if (playerActionsLocal.Activate.WasPressed || playerActionsLocal.VehicleActions.Activate.WasPressed || playerActionsLocal.PermanentActions.Activate.WasPressed)
			{
				timeActivatePressed = Time.time;
				if (flag19 && hitInfo3.bHitValid && GameUtils.IsBlockOrTerrain(hitInfo3.tag))
				{
					blockValue2 = BlockValue.Air;
				}
				if (entityPlayerLocal.AttachedToEntity != null)
				{
					entityPlayerLocal.SendDetach();
				}
				else if (entityTurret != null || projectileMoveScript != null || thrownWeaponMoveScript != null || entityItem != null || !blockValue2.isair || tileEntityLootable != null || tileEntity != null)
				{
					entityPlayerLocal.vp_FPCamera.AlignCharacterToCamera();
					BlockValue blockValue4 = blockValue2;
					Vector3i vector3i3 = vector3i;
					if (blockValue4.ischild)
					{
						vector3i3 = blockValue4.Block.multiBlockPos.GetParentPos(vector3i3, blockValue4);
						blockValue4 = gameManager.World.GetBlock(hitInfo3.hit.clrIdx, vector3i3);
					}
					if (!blockValue4.Equals(BlockValue.Air) && blockValue4.Block.HasBlockActivationCommands(gameManager.World, blockValue4, hitInfo3.hit.clrIdx, vector3i3, entityPlayerLocal))
					{
						playerUI.xui.RadialWindow.Open();
						playerUI.xui.RadialWindow.SetCurrentBlockData(gameManager.World, vector3i3, hitInfo3.hit.clrIdx, blockValue4, entityPlayerLocal);
						flag15 = true;
					}
					else if (tileEntityLootable != null && entity.GetActivationCommands(tileEntityLootable.ToWorldPos(), entityPlayerLocal).Length != 0)
					{
						entityPlayerLocal.AimingGun = false;
						tileEntityLootable.bWasTouched = tileEntityLootable.bTouched;
						playerUI.xui.RadialWindow.Open();
						playerUI.xui.RadialWindow.SetCurrentEntityData(gameManager.World, entity, tileEntityLootable, entityPlayerLocal);
						flag15 = true;
					}
					else if (tileEntity != null && entity.GetActivationCommands(tileEntity.ToWorldPos(), entityPlayerLocal).Length != 0)
					{
						entityPlayerLocal.AimingGun = false;
						playerUI.xui.RadialWindow.Open();
						playerUI.xui.RadialWindow.SetCurrentEntityData(gameManager.World, entity, tileEntity, entityPlayerLocal);
						flag15 = true;
					}
					else if (entityTurret != null)
					{
						if (entityTurret.CanInteract(entityPlayerLocal.entityId))
						{
							ItemStack itemStack = new ItemStack(entityTurret.OriginalItemValue, 1);
							if (entityPlayerLocal.inventory.CanTakeItem(itemStack) || entityPlayerLocal.bag.CanTakeItem(itemStack))
							{
								gameManager.CollectEntityServer(entityTurret.entityId, playerUI.entityPlayer.entityId);
							}
							else
							{
								GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied");
							}
						}
					}
					else
					{
						windowManager.Close("radial");
						if (entityItem != null)
						{
							EntityItem entityItem2 = entityItem;
							if (entityItem2 != null && entityItem2.CanCollect() && entityItem2.onGround)
							{
								if (entityPlayerLocal.inventory.CanTakeItem(entityItem2.itemStack) || entityPlayerLocal.bag.CanTakeItem(entityItem2.itemStack))
								{
									gameManager.CollectEntityServer(entityItem.entityId, entityPlayerLocal.entityId);
								}
								else
								{
									GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied");
								}
							}
						}
						else if (projectileMoveScript != null)
						{
							if (projectileMoveScript.itemProjectile.IsSticky)
							{
								ItemStack itemStack2 = new ItemStack(projectileMoveScript.itemValueProjectile, 1);
								if (entityPlayerLocal.inventory.CanTakeItem(itemStack2) || entityPlayerLocal.bag.CanTakeItem(itemStack2))
								{
									playerUI.xui.PlayerInventory.AddItem(itemStack2);
									projectileMoveScript.ProjectileID = -1;
									UnityEngine.Object.Destroy(projectileMoveScript.gameObject);
								}
								else
								{
									GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied");
								}
							}
						}
						else if (thrownWeaponMoveScript != null)
						{
							if (thrownWeaponMoveScript.itemWeapon.IsSticky)
							{
								ItemStack itemStack3 = new ItemStack(thrownWeaponMoveScript.itemValueWeapon, 1);
								if (entityPlayerLocal.inventory.CanTakeItem(itemStack3) || entityPlayerLocal.bag.CanTakeItem(itemStack3))
								{
									playerUI.xui.PlayerInventory.AddItem(itemStack3);
									thrownWeaponMoveScript.ProjectileID = -1;
									UnityEngine.Object.Destroy(thrownWeaponMoveScript.gameObject);
								}
								else
								{
									GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied");
								}
							}
						}
						else
						{
							suckItemsNearby(entityItem);
						}
					}
				}
				else if (flag19)
				{
					entityPlayerLocal.inventory.Interact();
				}
			}
			else
			{
				windowManager.Close("radial");
				suckItemsNearby(entityItem);
			}
		}
		if (gameManager.IsEditMode() && flag17 && flag16 && !playerActionsLocal.Drop.IsPressed)
		{
			WorldRayHitInfo other = Voxel.voxelRayHitInfo.Clone();
			num12 = 325;
			int layerMask = int.MinValue;
			if (Voxel.RaycastOnVoxels(gameManager.World, ray, 250f, layerMask, num12, 0f) && SelectionBoxManager.Instance.Select(Voxel.voxelRayHitInfo))
			{
				flag16 = false;
				bIgnoreLeftMouseUntilReleased = true;
			}
			Voxel.voxelRayHitInfo.CopyFrom(other);
		}
		if (flag17 && (GameManager.Instance.World.IsEditor() || BlockToolSelection.Instance.SelectionActive))
		{
			flag17 &= !playerActionsLocal.Drop.IsPressed;
		}
		int num15 = playerActionsLocal.InventorySlotWasPressed;
		if (num15 >= 0)
		{
			if (playerActionsLocal.LastInputType == BindingSourceType.DeviceBindingSource)
			{
				if (entityPlayerLocal.AimingGun)
				{
					num15 = -1;
				}
			}
			else if (InputUtils.ShiftKeyPressed && entityPlayerLocal.inventory.PUBLIC_SLOTS > entityPlayerLocal.inventory.SHIFT_KEY_SLOT_OFFSET)
			{
				num15 += entityPlayerLocal.inventory.SHIFT_KEY_SLOT_OFFSET;
			}
		}
		if (num15 == -1 && inventoryScrollPressed && inventoryScrollIdxToSelect != -1)
		{
			num15 = inventoryScrollIdxToSelect;
		}
		if (!flag2)
		{
			flag2 = entityPlayerLocal.inventory.holdingItem.ConsumeScrollWheel(entityPlayerLocal.inventory.holdingItemData, num, playerActionsLocal);
		}
		entityPlayerLocal.inventory.holdingItem.CheckKeys(entityPlayerLocal.inventory.holdingItemData, hitInfo3);
		ItemClass holdingItem = entityPlayerLocal.inventory.holdingItem;
		bool flag21 = (holdingItem.Actions[0] != null && holdingItem.Actions[0].AllowConcurrentActions()) || (holdingItem.Actions[1] != null && holdingItem.Actions[1].AllowConcurrentActions());
		bool flag22 = holdingItem.Actions[1] != null && holdingItem.Actions[1].IsActionRunning(entityPlayerLocal.inventory.holdingItemData.actionData[1]);
		bool flag23 = holdingItem.Actions[0] != null && holdingItem.Actions[0].IsActionRunning(entityPlayerLocal.inventory.holdingItemData.actionData[0]);
		if (flag16 && flag17 && !flag23 && (flag21 || !flag22) && entityPlayerLocal.inventory.GetIsFinishedSwitchingHeldItem())
		{
			if (gameManager.IsEditMode())
			{
				flag16 = !gameManager.GetActiveBlockTool().ExecuteAttackAction(entityPlayerLocal.inventory.holdingItemData, _bReleased: false, playerActionsLocal);
			}
			if (flag16)
			{
				entityPlayerLocal.inventory.Execute(0, _bReleased: false, playerActionsLocal);
			}
		}
		if (flag16 && playerActionsLocal.Primary.WasReleased && entityPlayerLocal.inventory.GetIsFinishedSwitchingHeldItem())
		{
			if (gameManager.IsEditMode() && !entityPlayerLocal.inventory.holdingItem.IsGun())
			{
				flag16 = !gameManager.GetActiveBlockTool().ExecuteAttackAction(entityPlayerLocal.inventory.holdingItemData, _bReleased: true, playerActionsLocal);
			}
			if (flag16)
			{
				entityPlayerLocal.inventory.Execute(0, _bReleased: true, playerActionsLocal);
			}
		}
		_ = entityPlayerLocal.inventory.holdingItem.Actions[0];
		if (flag15 && flag18 && (flag21 || !flag23))
		{
			if (gameManager.IsEditMode())
			{
				flag15 = !gameManager.GetActiveBlockTool().ExecuteUseAction(entityPlayerLocal.inventory.holdingItemData, _bReleased: false, playerActionsLocal);
			}
			if (flag15)
			{
				entityPlayerLocal.inventory.Execute(1, _bReleased: false, playerActionsLocal);
			}
		}
		if (flag15 && playerActionsLocal.Secondary.WasReleased && entityPlayerLocal.inventory != null)
		{
			entityPlayerLocal.inventory.Execute(1, _bReleased: true, playerActionsLocal);
			if (gameManager.IsEditMode() && !entityPlayerLocal.inventory.holdingItem.IsGun())
			{
				gameManager.GetActiveBlockTool().ExecuteUseAction(entityPlayerLocal.inventory.holdingItemData, _bReleased: true, playerActionsLocal);
			}
		}
		if (playerActionsLocal.Drop.WasPressed)
		{
			DropHeldItem();
		}
		if (playerActionsLocal.InventorySlotLeft.WasPressed || playerActionsLocal.InventorySlotRight.WasPressed)
		{
			_ = 1;
		}
		else
			_ = inventoryScrollPressed;
		inventoryScrollPressed = false;
		if (entityPlayerLocal.AttachedToEntity == null)
		{
			if (num15 != -1 && num15 != entityPlayerLocal.inventory.GetFocusedItemIdx() && num15 < entityPlayerLocal.inventory.PUBLIC_SLOTS && entityPlayerLocal.inventory != null && !entityPlayerLocal.CancellingInventoryActions)
			{
				if (canSwapHeldItem())
				{
					swapItem(entityPlayerLocal.inventory.SetFocusedItemIdx(num15));
				}
				else
				{
					entityPlayerLocal.inventory.SetFocusedItemIdx(num15);
					nextHeldItem.Add(num15);
				}
			}
			if ((playerActionsLocal.Reload.WasPressed || playerActionsLocal.PermanentActions.Reload.WasPressed) && entityPlayerLocal.inventory != null && entityPlayerLocal.inventory.GetIsFinishedSwitchingHeldItem())
			{
				bool num16 = entityPlayerLocal.inventory.IsHoldingGun() || entityPlayerLocal.inventory.IsHoldingDynamicMelee();
				ItemAction holdingPrimary = entityPlayerLocal.inventory.GetHoldingPrimary();
				ItemAction holdingSecondary = entityPlayerLocal.inventory.GetHoldingSecondary();
				if (num16 && holdingPrimary != null)
				{
					if (holdingPrimary.HasRadial())
					{
						timeActivatePressed = Time.time;
						playerUI.xui.RadialWindow.Open();
						holdingPrimary.SetupRadial(playerUI.xui.RadialWindow, entityPlayerLocal);
					}
					else
					{
						holdingPrimary.CancelAction(entityPlayerLocal.inventory.holdingItemData.actionData[0]);
						if (holdingSecondary != null && !(holdingSecondary is ItemActionSpawnTurret))
						{
							holdingSecondary.CancelAction(entityPlayerLocal.inventory.holdingItemData.actionData[1]);
						}
					}
				}
				else if (entityPlayerLocal.inventory.GetHoldingBlock() != null)
				{
					timeActivatePressed = Time.time;
					playerUI.xui.RadialWindow.Open();
					playerUI.xui.RadialWindow.SetupBlockShapeData();
				}
			}
			if (!flag2 && (playerActionsLocal.ToggleFlashlight.WasPressed || playerActionsLocal.PermanentActions.ToggleFlashlight.WasPressed))
			{
				timeActivatePressed = Time.time;
				playerUI.xui.RadialWindow.Open();
				playerUI.xui.RadialWindow.SetActivatableItemData(entityPlayerLocal);
			}
			if (!flag2 && (playerActionsLocal.Swap.WasPressed || playerActionsLocal.PermanentActions.Swap.WasPressed))
			{
				timeActivatePressed = Time.time;
				playerUI.xui.RadialWindow.Open();
				playerUI.xui.RadialWindow.SetupToolbeltMenu(0);
			}
			if (!flag2 && playerActionsLocal.InventorySlotRight.WasPressed)
			{
				timeActivatePressed = Time.time;
				playerUI.xui.RadialWindow.Open();
				playerUI.xui.RadialWindow.SetupToolbeltMenu(1);
			}
			if (!flag2 && playerActionsLocal.InventorySlotLeft.WasPressed)
			{
				timeActivatePressed = Time.time;
				playerUI.xui.RadialWindow.Open();
				playerUI.xui.RadialWindow.SetupToolbeltMenu(-1);
			}
		}
		else
		{
			if (!(entityPlayerLocal.AttachedToEntity is EntityVehicle entityVehicle))
			{
				return;
			}
			if (playerActionsLocal.PermanentActions.ToggleFlashlight.WasPressed || playerActionsLocal.VehicleActions.ToggleFlashlight.WasPressed)
			{
				if (entityVehicle.HasHeadlight())
				{
					entityVehicle.ToggleHeadlight();
				}
				else
				{
					timeActivatePressed = Time.time;
					playerUI.xui.RadialWindow.Open();
					playerUI.xui.RadialWindow.SetActivatableItemData(entityPlayerLocal);
				}
			}
			if (playerActionsLocal.VehicleActions.HonkHorn.WasPressed)
			{
				entityVehicle.UseHorn(entityPlayerLocal);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void swapItem(int inventoryItemToSet)
	{
		entityPlayerLocal.AimingGun = false;
		entityPlayerLocal.inventory.BeginSwapHoldingItem();
		StartCoroutine(entityPlayerLocal.CancelInventoryActions([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Inventory inventory = entityPlayerLocal.inventory;
			if (inventory != null)
			{
				playerInput.Primary.ClearInputState();
				inventory.SetHoldingItemIdx(inventoryItemToSet);
			}
		}, holsterWeapon: true));
	}

	public void DropHeldItem()
	{
		if (!gameManager.IsEditMode() && !BlockToolSelection.Instance.SelectionActive && entityPlayerLocal.inventory != null && !entityPlayerLocal.inventory.IsHoldingItemActionRunning() && !entityPlayerLocal.inventory.IsHolsterDelayActive() && !entityPlayerLocal.inventory.IsUnholsterDelayActive() && entityPlayerLocal.inventory.holdingItemIdx != entityPlayerLocal.inventory.DUMMY_SLOT_IDX && !entityPlayerLocal.AimingGun)
		{
			Vector3 dropPosition = entityPlayerLocal.GetDropPosition();
			ItemValue holdingItemItemValue = entityPlayerLocal.inventory.holdingItemItemValue;
			if (ItemClass.GetForId(holdingItemItemValue.type).CanDrop(holdingItemItemValue) && entityPlayerLocal.inventory.holdingCount > 0 && entityPlayerLocal.DropTimeDelay <= 0f)
			{
				entityPlayerLocal.DropTimeDelay = 0.5f;
				int count = entityPlayerLocal.inventory.holdingItemStack.count;
				gameManager.ItemDropServer(entityPlayerLocal.inventory.holdingItemStack.Clone(), dropPosition, Vector3.zero, entityPlayerLocal.entityId, ItemClass.GetForId(holdingItemItemValue.type).GetLifetimeOnDrop());
				entityPlayerLocal.AddUIHarvestingItem(new ItemStack(holdingItemItemValue, -count));
				Manager.BroadcastPlay(entityPlayerLocal, "itemdropped");
				entityPlayerLocal.inventory.DecHoldingItem(count);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void suckItemsNearby(EntityItem focusedItem)
	{
		if (!countdownSuckItemsNearby.HasPassed())
		{
			return;
		}
		countdownSuckItemsNearby.ResetAndRestart();
		if (!entityPlayerLocal.addedToChunk)
		{
			return;
		}
		int num = entityPlayerLocal.GetBlockPosition().y >> 4;
		for (int i = entityPlayerLocal.chunkPosAddedEntityTo.x - 1; i <= entityPlayerLocal.chunkPosAddedEntityTo.x + 1; i++)
		{
			for (int j = entityPlayerLocal.chunkPosAddedEntityTo.z - 1; j <= entityPlayerLocal.chunkPosAddedEntityTo.z + 1; j++)
			{
				Chunk chunk = (Chunk)gameManager.World.GetChunkSync(i, j);
				if (chunk == null)
				{
					continue;
				}
				int num2 = Utils.FastMax(num - 1, 0);
				int num3 = Utils.FastMin(num + 1, chunk.entityLists.Length - 1);
				for (int k = num2; k <= num3; k++)
				{
					int num4 = 0;
					while (chunk.entityLists[k] != null && num4 < chunk.entityLists[k].Count)
					{
						if (chunk.entityLists[k][num4] is EntityItem)
						{
							EntityItem entityItem = (EntityItem)chunk.entityLists[k][num4];
							if (entityItem.CanCollect())
							{
								Vector3 velToAdd = entityPlayerLocal.getHeadPosition() - chunk.entityLists[k][num4].GetPosition();
								if (!(velToAdd.sqrMagnitude > 16f) && (entityPlayerLocal.inventory.CanTakeItem(entityItem.itemStack) || entityPlayerLocal.bag.CanTakeItem(entityItem.itemStack)))
								{
									if (velToAdd.sqrMagnitude < 4f)
									{
										if (focusedItem != null && focusedItem.onGround)
										{
											gameManager.CollectEntityServer(focusedItem.entityId, entityPlayerLocal.entityId);
										}
									}
									else
									{
										velToAdd.Normalize();
										velToAdd.x *= 0.7f;
										velToAdd.y *= 1.5f;
										velToAdd.z *= 0.7f;
										gameManager.AddVelocityToEntityServer(chunk.entityLists[k][num4].entityId, velToAdd);
									}
								}
							}
						}
						num4++;
					}
				}
			}
		}
	}

	public void SkipMouseLookNextFrame()
	{
		skipMouseLookNextFrame = 3;
	}

	public void SetControllableOverride(bool _b)
	{
		bCanControlOverride = _b;
	}

	public void Respawn(RespawnType _type)
	{
		gameManager.World.GetPrimaryPlayer().Spawned = false;
		respawnReason = _type;
		switch (_type)
		{
		case RespawnType.NewGame:
			respawnTime = Constants.cRespawnEnterGameTime;
			break;
		case RespawnType.LoadedGame:
			respawnTime = 0f;
			break;
		case RespawnType.Died:
			respawnTime = Constants.cRespawnAfterDeathTime;
			break;
		}
	}

	public void AllowPlayerInput(bool allow)
	{
		bAllowPlayerInput = allow;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawChunkBoundary()
	{
		Vector3i vector3i = World.toChunkXYZCube(entityPlayerLocal.position);
		Vector3 vector = default(Vector3);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					string text = $"PlayerChunk{k},{i},{j}";
					vector.x = (vector3i.x + k) * 16;
					vector.y = (vector3i.y + i) * 16;
					vector.z = (vector3i.z + j) * 16;
					Vector3 cornerPos = vector;
					cornerPos.x += 16f;
					cornerPos.y += 16f;
					cornerPos.z += 16f;
					DebugLines debugLines = ((k != 0 || i != 0 || j != 0) ? DebugLines.Create(text, entityPlayerLocal.RootTransform, new Color(0.3f, 0.3f, 0.3f), new Color(0.3f, 0.3f, 0.3f), 0.033f, 0.033f, 0.1f) : DebugLines.Create(text, entityPlayerLocal.RootTransform, new Color(1f, 1f, 1f), new Color(1f, 1f, 1f), 0.1f, 0.1f, 0.1f));
					debugLines.AddCube(vector, cornerPos);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawChunkDensities()
	{
		Vector3i vector3i = World.toChunkXYZCube(entityPlayerLocal.position);
		vector3i *= 16;
		IChunk chunkFromWorldPos = entityPlayerLocal.world.GetChunkFromWorldPos(World.worldToBlockPos(entityPlayerLocal.position));
		if (chunkFromWorldPos == null)
		{
			return;
		}
		Vector3 vector = default(Vector3);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					int num = i + vector3i.y;
					int density = chunkFromWorldPos.GetDensity(k, num, j);
					if (density != MarchingCubes.DensityAir && density != MarchingCubes.DensityTerrain)
					{
						float num2 = 0f;
						if (num > 0)
						{
							sbyte density2 = chunkFromWorldPos.GetDensity(k, num - 1, j);
							num2 = MarchingCubes.GetDecorationOffsetY((sbyte)density, density2);
						}
						string text = $"PlayerDensity{k},{i},{j}";
						vector.x = (float)(vector3i.x + k) + 0.5f - 0.5f;
						vector.y = num;
						vector.z = (float)(vector3i.z + j) + 0.5f - 0.5f;
						Vector3 cornerPos = vector;
						cornerPos.x += 1f;
						cornerPos.y += 0.5f + num2;
						cornerPos.z += 1f;
						DebugLines debugLines;
						if (density > 0)
						{
							float b = 1f - (float)density / 127f;
							Color color = new Color(0.2f, 0.2f, b);
							debugLines = DebugLines.Create(text, entityPlayerLocal.RootTransform, color, color, 0.005f, 0.005f, 0.1f);
						}
						else
						{
							float num3 = (float)(-density) / 128f;
							Color color2 = new Color(num3, num3, 0.2f);
							debugLines = DebugLines.Create(text, entityPlayerLocal.RootTransform, color2, color2, 0.01f, 0.01f, 0.1f);
						}
						debugLines.AddCube(vector, cornerPos);
					}
				}
			}
		}
	}

	public void FindCameraSnapTarget(eCameraSnapMode snapMode, float maxDistance)
	{
		cameraSnapTargets.Clear();
		GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(entityPlayerLocal.position, Vector3.one * maxDistance), cameraSnapTargets);
		float num = float.MaxValue;
		EntityAlive target = null;
		if (cameraSnapTargets.Count <= 0)
		{
			return;
		}
		foreach (EntityAlive cameraSnapTarget in cameraSnapTargets)
		{
			if (cameraSnapTarget == entityPlayerLocal || !cameraSnapTarget.IsValidAimAssistSnapTarget || !cameraSnapTarget.IsAlive() || cameraSnapTarget.ModelTransform == null)
			{
				continue;
			}
			Vector3 direction = cameraSnapTarget.GetChestTransformPosition() - entityPlayerLocal.cameraTransform.position;
			float sqrMagnitude = direction.sqrMagnitude;
			float num2 = Vector3.Angle(entityPlayerLocal.cameraTransform.forward, direction.normalized);
			if (snapMode == eCameraSnapMode.Zoom)
			{
				float num3 = 15f * (1f - targetSnapFalloffCurve.Evaluate(sqrMagnitude / 50f));
				if (num2 > num3)
				{
					continue;
				}
			}
			else if (num2 > 20f)
			{
				continue;
			}
			if ((bool)entityPlayerLocal.HitInfo.transform && entityPlayerLocal.HitInfo.transform.IsChildOf(cameraSnapTarget.ModelTransform) && sqrMagnitude < num)
			{
				num = sqrMagnitude;
				target = cameraSnapTarget;
				break;
			}
			if (sqrMagnitude < num && Physics.Raycast(new Ray(entityPlayerLocal.cameraTransform.position, direction), out var hitInfo, maxDistance, -538750997) && ((cameraSnapTarget.PhysicsTransform != null && hitInfo.collider.transform.IsChildOf(cameraSnapTarget.PhysicsTransform)) || hitInfo.collider.transform.IsChildOf(cameraSnapTarget.ModelTransform)))
			{
				num = sqrMagnitude;
				target = cameraSnapTarget;
			}
		}
		SetCameraSnapEntity(target, snapMode);
	}

	public void SetCameraSnapEntity(EntityAlive _target, eCameraSnapMode _snapMode)
	{
		cameraSnapTargetEntity = _target;
		cameraSnapMode = _snapMode;
		if (cameraSnapTargetEntity != null)
		{
			Vector2 vector = Vector2.one * 0.5f;
			Vector2 vector2 = entityPlayerLocal.playerCamera.WorldToViewportPoint(cameraSnapTargetEntity.GetChestTransformPosition());
			Vector2 vector3 = entityPlayerLocal.playerCamera.WorldToViewportPoint(cameraSnapTargetEntity.emodel.GetHeadTransform().position);
			Vector2 vector4 = vector2 - vector;
			snapTargetingHead = (vector3 - vector).sqrMagnitude < vector4.sqrMagnitude;
			cameraSnapTime = Time.time;
		}
	}

	public void ForceStopRunning()
	{
		entityPlayerLocal.movementInput.running = false;
		runToggleActive = false;
	}

	public void SetInventoryIdxFromScroll(int _idx)
	{
		inventoryScrollPressed = true;
		inventoryScrollIdxToSelect = _idx;
	}
}

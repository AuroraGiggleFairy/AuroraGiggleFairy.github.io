using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using DynamicMusic;
using DynamicMusic.Factories;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class EntityPlayerLocal : EntityPlayer, IInventoryChangedListener, IGamePrefsChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum MoveState
	{
		None,
		Off,
		Attached,
		Idle,
		Walk,
		Run,
		Swim,
		Crouch,
		CrouchWalk,
		CrouchRun,
		Jump
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum DropOption
	{
		None,
		All,
		Toolbelt,
		Backpack,
		DeleteAll
	}

	public class AutoMove
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public enum Mode
		{
			Off,
			Line,
			Orbit,
			Relative
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Entity entity;

		[PublicizedFrom(EAccessModifier.Private)]
		public Mode mode;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 startPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 targetPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public float curTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public float endTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public int loopCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isPingPong;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isFlipped;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 vel;

		[PublicizedFrom(EAccessModifier.Private)]
		public float rotY;

		[PublicizedFrom(EAccessModifier.Private)]
		public float rotYVel;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 lookAtPos;

		public AutoMove(Entity _entity)
		{
			entity = _entity;
		}

		public void StartLine(float _duration, int _loopCount, Vector3 _endPos)
		{
			mode = Mode.Line;
			endTime = _duration;
			loopCount = _loopCount;
			startPos = entity.position;
			targetPos = _endPos;
			isPingPong = false;
			if (loopCount < 0)
			{
				loopCount *= -2;
				isPingPong = true;
			}
		}

		public void StartOrbit(float _duration, int _loopCount, Vector3 _orbitPos)
		{
			mode = Mode.Orbit;
			endTime = _duration;
			loopCount = _loopCount;
			startPos = entity.position;
			targetPos = _orbitPos;
			isPingPong = false;
			if (loopCount < 0)
			{
				loopCount *= -2;
				isPingPong = true;
			}
		}

		public void StartRelative(float velX, float velZ, float rotVel)
		{
			mode = Mode.Relative;
			vel.x = velX;
			vel.z = velZ;
			rotY = entity.rotation.y;
			rotYVel = rotVel;
		}

		public void SetLookAt(Vector3 _pos)
		{
			lookAtPos = _pos;
		}

		public void Update()
		{
			if (mode == Mode.Off)
			{
				return;
			}
			if (mode == Mode.Line)
			{
				curTime += Time.deltaTime;
				float num = curTime / endTime;
				if (num > 1f)
				{
					curTime = 0f;
					num = 0f;
					if (isPingPong)
					{
						Vector3 vector = startPos;
						Vector3 vector2 = targetPos;
						targetPos = vector;
						startPos = vector2;
					}
					if (--loopCount <= 0)
					{
						num = ((!isPingPong) ? 1 : 0);
						mode = Mode.Off;
					}
				}
				Vector3 pos = Vector3.Lerp(startPos, targetPos, num);
				entity.SetPosition(pos);
			}
			if (mode == Mode.Orbit)
			{
				curTime += Time.deltaTime;
				float num2 = curTime / endTime;
				if (num2 > 1f)
				{
					curTime -= endTime;
					num2 = curTime / endTime;
					if (isPingPong)
					{
						isFlipped = !isFlipped;
					}
					if (--loopCount <= 0)
					{
						num2 = 1f;
						mode = Mode.Off;
					}
				}
				Vector3 vector3 = startPos - targetPos;
				float num3 = 360f * num2;
				num3 *= (float)((!isFlipped) ? 1 : (-1));
				entity.SetPosition(targetPos + Quaternion.Euler(0f, num3, 0f) * vector3);
				Vector3 eulerAngles = Quaternion.LookRotation(targetPos - entity.position, Vector3.up).eulerAngles;
				eulerAngles.x *= -1f;
				entity.SetRotation(eulerAngles);
			}
			if (mode == Mode.Relative)
			{
				rotY += rotYVel * Time.deltaTime;
				entity.SetRotation(new Vector3(0f, rotY, 0f));
				entity.SetPosition(entity.position + Quaternion.Euler(0f, rotY, 0f) * vel * Time.deltaTime);
			}
			if (lookAtPos.sqrMagnitude > 0f)
			{
				Vector3 eulerAngles2 = Quaternion.LookRotation(lookAtPos - entity.position, Vector3.up).eulerAngles;
				eulerAngles2.x *= -1f;
				entity.SetRotation(eulerAngles2);
			}
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float prevStaminaValue;

	public float weaponCrossHairAlpha = 0.8f;

	public const float cCameraTPVBaseDistance = 0.94f;

	public const float cCameraTPVOffsetMin = -0.2f;

	public const float cCameraTPVOffsetMax = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform cameraContainerTransform;

	public Transform cameraTransform;

	public Camera playerCamera;

	public Camera finalCamera;

	public bool IsUnderwaterCamera;

	public GameRenderManager renderManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPController m_vp_FPController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPCamera m_vp_FPCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPWeapon m_vp_FPWeapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_checked_vp_FPController;

	public WorldRayHitInfo HitInfo = new WorldRayHitInfo();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float SPRINT_GRACE_PERIOD = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeInSprintGrace;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float VIBRATION_LOW = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float VIBRATION_MEDIUM = 0.35f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float VIBRATION_HIGH = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float VIBRATION_DURATION = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float oldHealth;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float vibrationTimeout = float.MaxValue;

	public PersistentPlayerData persistentPlayerData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float lastTimeJetpackDecreased;

	public bool bFirstPersonView = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float crossHairOpenArea;

	public MovementInput movementInput = new MovementInput();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool inputWasJump;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool inputWasDown;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool jumpTrigger;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSwitchCameraBackAfterRespawn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSwitchTo3rdPersonAfterAiming;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 selfCameraPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 selfCameraSeekPos;

	public bool bExhausted;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isExhaustedSoundAllowed = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int inventorySendCounter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerInventory xmitInventory;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack dragAndDropItem;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isReloadCancelling;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLadderAttached;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool canLadderAirAttach = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasJumping;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasJumpTrigger;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasLadderAttachedJump;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int swimMode = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int swimExhaustedTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool swimClimbing;

	public bool bLerpCameraFlag;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lerpCameraLerpValue;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lerpCameraStartFOV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lerpCameraEndFOV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lerpCameraFastFOV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastOverrideFOV = -1f;

	public float OverrideFOV = -1f;

	public Vector3 OverrideLookAt = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float biomeVolume;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource audioSourceBiomeActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource audioSourceBiomeFadeOut;

	public BlendCycleTimer sneakDamageBlendTimer = new BlendCycleTimer(0.5f, 2f, 0.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string sneakDamageText = "";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string dropInventoryBlock;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int runTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int sprintLoopSoundPlayId = -1;

	public DynamicMusicManager DynamicMusicManager;

	public IThreatLevel ThreatLevel;

	public float LastTargetEventTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material overlayMaterial;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] overlayAlternating = new byte[4];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float[] overlayDirectionTime = new float[8];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2[] overlayBloodDropsPositions = new Vector2[24];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform uwEffectRefract;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform uwEffectDebris;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform uwEffectDroplets;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform uwEffectWaterFade;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform uwEffectHaze;

	public bool bIntroAnimActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LocalPlayerUI playerUI;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GUIWindowManager windowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NGUIWindowManager nguiWindowManager;

	public ScreenEffects ScreenEffectManager;

	public float GodModeSpeedModifier = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerMoveController moveController;

	public bool isStunned;

	public bool isDeafened;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int oldLayer = -1;

	public Rect ZombieCompassBounds;

	public float DropTimeDelay;

	public float InteractTimeDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float recoveryPointTimer;

	public List<Vector3i> recoveryPositions = new List<Vector3i>();

	public bool DebugDismembermentChance;

	public bool BloodMoonParticipation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool inAir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_MoveVector;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_SmoothLook;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MoveState moveState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool moveStateAiming;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool moveStateHoldBow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimDragBase = 0.01f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimDragScale = 5.4f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimDragPow = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimAccelExhausted = 0.00025f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimAccel = 0.00032f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimAccelRun = 0.0024f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimAccelUp = 0.00052f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimAccelUpGrounded = 0.05f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSwimAccelDown = -0.00038f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float achievementDistanceAccu;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastWaypointUpdateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWaypointUpdateTime = 30f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float updateBedrollPositionChecks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float updateRadiationChecks;

	public float InWorldPercent;

	public float InWorldLookPercent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeUnderwater;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasSpawned;

	public float spawnEffectPow = 4.19f;

	public static float spawnInEffectSpeed = 3f;

	public bool bPlayingSpawnIn;

	public float spawnInTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float spawnInIntensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float deathTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool unstuckRequested;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDyingEffectSpeed = 600f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDyingEffectStartHealth = 70f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDyingEffectHealthThreshold = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float dyingEffectHitTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float dyingEffectCur;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float dyingEffectLast;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float dyingEffectHealthLast;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int fallHealth;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFallDeath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Collider> setLayerRecursivelyList = new List<Collider>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NavObject backpackNavObject;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<NavObject> backpackNavObjects;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> backpackPositionsFromThread = new List<Vector3i>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] screenBloodEffect;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material[] screenBloodMtrl;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool healthLostThisRound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCrosshairScreenHeightFactor = 0.059f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AutoMove autoMove;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool weaponIsHolstered;

	public PlayerActionsLocal playerInput => PlatformManager.NativePlatform.Input.PrimaryPlayer;

	public vp_FPController vp_FPController
	{
		get
		{
			checkedGetFPController();
			return m_vp_FPController;
		}
	}

	public vp_FPCamera vp_FPCamera
	{
		get
		{
			checkedGetFPController();
			return m_vp_FPCamera;
		}
	}

	public vp_FPWeapon vp_FPWeapon
	{
		get
		{
			checkedGetFPController();
			return m_vp_FPWeapon;
		}
	}

	public ItemStack DragAndDropItem
	{
		get
		{
			return dragAndDropItem;
		}
		set
		{
			dragAndDropItem = value;
			this.DragAndDropItemChanged();
		}
	}

	public PlayerMoveController MoveController => moveController;

	public LocalPlayerUI PlayerUI => playerUI;

	public bool InAir
	{
		get
		{
			return inAir;
		}
		set
		{
			if (inAir != value)
			{
				inAir = value;
				emodel.avatarController.SetInAir(inAir);
			}
		}
	}

	public virtual Vector2 OnValue_InputMoveVector
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_MoveVector;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_MoveVector = (MovementRunning ? value.normalized : value);
		}
	}

	public virtual Vector2 OnValue_InputSmoothLook
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_SmoothLook;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_SmoothLook = value;
		}
	}

	public virtual Vector2 OnValue_InputRawLook
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_SmoothLook;
		}
	}

	public virtual Vector3 OnValue_CameraLookDirection
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GetLookVector();
		}
	}

	public event Action InventoryChangedEvent;

	public event Action DragAndDropItemChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkedGetFPController()
	{
		if (!PhysicsTransform)
		{
			return;
		}
		if (!m_checked_vp_FPController || m_vp_FPController == null)
		{
			m_checked_vp_FPController = true;
			m_vp_FPController = PhysicsTransform.GetComponent<vp_FPController>();
			Transform transform = PhysicsTransform.Find("Camera");
			if (transform != null)
			{
				m_vp_FPCamera = transform.GetComponent<vp_FPCamera>();
			}
		}
		if (m_vp_FPWeapon == null)
		{
			m_vp_FPWeapon = PhysicsTransform.GetComponentInChildren<vp_FPWeapon>();
			if (m_vp_FPWeapon != null && emodel is EModelSDCS)
			{
				m_vp_FPWeapon.DefaultState.Preset.SetFieldValue("PositionOffset", new Vector3(0f, -1.7f, 0.02f));
				m_vp_FPWeapon.DefaultState.Preset.SetFieldValue("RenderingFieldOfView", 45);
			}
		}
	}

	public void ShowHoldingItem(bool show)
	{
		if (show)
		{
			playerCamera.cullingMask |= 1024;
		}
		else
		{
			playerCamera.cullingMask &= -1025;
		}
	}

	public void MakeAttached(bool bAttached)
	{
		isLadderAttached = bAttached;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		playerUI = LocalPlayerUI.GetUIForPlayer(this);
		windowManager = playerUI.windowManager;
		nguiWindowManager = playerUI.nguiWindowManager;
		QuestJournal.OwnerPlayer = this;
		challengeJournal = new ChallengeJournal();
		challengeJournal.Player = this;
		base.Awake();
		dragAndDropItem = ItemStack.Empty;
		isEntityRemote = false;
		world.AddLocalPlayer(this);
		cameraContainerTransform = base.transform;
		cameraTransform = cameraContainerTransform.Find("Camera");
		playerCamera = cameraTransform.GetComponent<Camera>();
		finalCamera = playerCamera;
		ScreenEffectManager = cameraTransform.gameObject.AddMissingComponent<ScreenEffects>();
		Transform transform = cameraTransform.Find("ScreenEffectsWithDepth");
		if (transform != null)
		{
			uwEffectHaze = transform.Find("UnderwaterHaze");
		}
		uwEffectRefract = cameraTransform.Find("effect_refract_plane");
		uwEffectDebris = cameraTransform.Find("effect_underwater_debris");
		uwEffectDroplets = cameraTransform.Find("effect_dropletsParticle");
		uwEffectWaterFade = cameraTransform.Find("effect_water_fade");
		audioSourceBiomeActive = cameraTransform.gameObject.AddComponent<AudioSource>();
		audioSourceBiomeFadeOut = cameraTransform.gameObject.AddComponent<AudioSource>();
		overlayMaterial = new Material(Shader.Find("Game/UI/Screen Overlay"));
		CameraDOFInit();
		Shader.SetGlobalFloat("_UnderWater", 0f);
		renderManager = GameRenderManager.Create(this);
		GameOptionsManager.ApplyCameraOptions(this);
		bPlayingSpawnIn = true;
		spawnInTime = Time.time;
		SkyManager.SetFogDebug();
		WeatherManager.Instance.PushTransitions();
		ThreatLevel = Factory.CreateThreatLevel();
		ScreenEffectManager.SetScreenEffect("VibrantDeSat");
		GameManager.Instance.triggerEffectManager.EnableVibration();
		moveController = GetComponent<PlayerMoveController>();
		MoveController.Init();
		SingletonMonoBehaviour<MumblePositionalAudio>.Instance?.SetPlayer(this);
		lastWaypointUpdateTime = Time.time - 30f;
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		Transform modelTransform = emodel.GetModelTransform();
		for (int i = 0; i < modelTransform.childCount; i++)
		{
			Transform child = modelTransform.GetChild(i);
			for (int j = 0; j < child.childCount; j++)
			{
				if (child.GetChild(j).GetComponent<Renderer>() is SkinnedMeshRenderer)
				{
					((SkinnedMeshRenderer)child.GetChild(j).GetComponent<Renderer>()).updateWhenOffscreen = true;
				}
			}
		}
		SetFirstPersonView(_bFirstPersonView: true, _bLerpPosition: false);
		IsGodMode.Value = !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled);
		IsNoCollisionMode.Value = IsGodMode.Value;
		if (m_characterController != null)
		{
			PhysicsTransform.gameObject.layer = 20;
		}
		if (vp_FPController != null)
		{
			vp_FPController.localPlayer = this;
			vp_FPController.Player.Register(this);
			vp_FPController.Player.FallImpact2.Register(this, "FallImpact", 0);
			vp_FPController.SyncCharacterController();
			vp_FPController.enabled = false;
		}
		GamePrefs.AddChangeListener(this);
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
	}

	public bool NACommand(List<string> args)
	{
		if (args == null)
		{
			return false;
		}
		int num = args.Count;
		if (num > 0)
		{
			if (args[0] == "create")
			{
				if (num == 1)
				{
					return NAInit();
				}
			}
			else
			{
				if (args[0] == "equip")
				{
					if (num != 1)
					{
						return NAEquip(args[1]);
					}
					return NAListEquipment();
				}
				if (args[0] == "unequip")
				{
					if (num != 1)
					{
						return NAUnEquip(args[1]);
					}
				}
				else if (args[0] == "rot_x")
				{
					if (num != 1)
					{
						return NARotateX(args[1]);
					}
				}
				else
				{
					if (args[0] == "help")
					{
						NAHelp();
						return true;
					}
					if (args[0] == "parts")
					{
						return NAListParts();
					}
				}
			}
		}
		NAHelp();
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerEquippedSlots _GetNASlots()
	{
		return base.transform.GetComponent<PlayerEquippedSlots>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform _GetNAOutfit()
	{
		return base.transform.Find("Graphics/Model/base");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NAHelp()
	{
		Log.Warning("New avatar command help.");
		Log.Warning("------------------------");
		Log.Warning("na create                Create new avatar.");
		Log.Warning("na equip                 List current equipment.");
		Log.Warning("na equip {partname}      Equip a part.");
		Log.Warning("na help                  This help.");
		Log.Warning("na parts                 List available parts.");
		Log.Warning("na rot_x {degrees}       Turn player (0 faces away, 180 towards).");
		Log.Warning("na unequip {partname}    Unequip a part.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NAInit()
	{
		Log.Warning("New avatar init.");
		Transform transform = base.transform.Find("Graphics/Model");
		if (transform == null)
		{
			Log.Error("Entity does not have 'Graphics/Model' node!");
			return false;
		}
		if (transform.Find("base") != null)
		{
			return false;
		}
		Transform transform2 = DataLoader.LoadAsset<Transform>("Entities/Player/Male/maleTestPrefab");
		if (transform2 == null)
		{
			return false;
		}
		transform2 = UnityEngine.Object.Instantiate(transform2, transform);
		transform2.name = "base";
		transform2.localPosition = new Vector3(0f, 0f, 1f);
		transform2.localRotation = Quaternion.Euler(0f, 135f, 0f);
		PlayerEquippedSlots playerEquippedSlots = _GetNASlots();
		if (playerEquippedSlots == null)
		{
			return false;
		}
		playerEquippedSlots.Init(_GetNAOutfit());
		NAEquip("baseHead");
		NAEquip("baseBody");
		NAEquip("baseHands");
		NAEquip("baseFeet");
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NAListParts()
	{
		PlayerEquippedSlots playerEquippedSlots = _GetNASlots();
		if (playerEquippedSlots == null)
		{
			return false;
		}
		playerEquippedSlots.ListParts();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NAListEquipment()
	{
		PlayerEquippedSlots playerEquippedSlots = _GetNASlots();
		if (playerEquippedSlots == null)
		{
			return false;
		}
		playerEquippedSlots.ListEquipment();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NAEquip(string partName)
	{
		Log.Warning("New avatar equip {0}", partName);
		PlayerEquippedSlots playerEquippedSlots = _GetNASlots();
		if (playerEquippedSlots == null)
		{
			return false;
		}
		return playerEquippedSlots.Equip(partName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NAUnEquip(string partName)
	{
		Log.Warning("New avatar unequip {0}", partName);
		PlayerEquippedSlots playerEquippedSlots = _GetNASlots();
		if (playerEquippedSlots == null)
		{
			return false;
		}
		return playerEquippedSlots.UnEquip(partName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NARotateX(string value)
	{
		Transform transform = _GetNAOutfit();
		if (transform == null)
		{
			return false;
		}
		transform.localRotation = Quaternion.Euler(0f, Convert.ToSingle(value), 0f);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _IsEquipped(string partName)
	{
		PlayerEquippedSlots playerEquippedSlots = _GetNASlots();
		if (playerEquippedSlots == null)
		{
			return false;
		}
		return playerEquippedSlots.IsEquipped(partName);
	}

	public override void PostInit()
	{
		base.PostInit();
		inventory.AddChangeListener(this);
		inventory.OnToolbeltItemsChangedInternal += callInventoryChanged;
		bag.OnBackpackItemsChangedInternal += callInventoryChanged;
		equipment.OnChanged += callInventoryChanged;
		DragAndDropItemChanged += callInventoryChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void callInventoryChanged()
	{
		if (this.InventoryChangedEvent != null)
		{
			this.InventoryChangedEvent();
		}
	}

	public override void OnAddedToWorld()
	{
		OwnedEntityData[] array = GetOwnedEntities();
		foreach (OwnedEntityData ownedEntityData in array)
		{
			EntityClass entityClass = EntityClass.list[ownedEntityData.ClassId];
			if (entityClass.NavObject != "" && ownedEntityData.hasLastKnownPosition)
			{
				NavObjectManager.Instance.RegisterNavObject(entityClass.NavObject, ownedEntityData.LastKnownPosition);
			}
		}
		base.OnAddedToWorld();
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		this.InventoryChangedEvent = null;
		GamePrefs.RemoveChangeListener(this);
		if (QuestJournal != null)
		{
			QuestJournal.UnHookQuests();
		}
		if (challengeJournal != null)
		{
			challengeJournal.EndChallenges();
		}
		inventory.CleanupHoldingActions();
		inventory.ResetActiveIndex();
		inventory.RemoveChangeListener(this);
		bag.Clear();
		renderManager.Destroy();
		renderManager = null;
		if (cameraTransform.parent == null)
		{
			UnityEngine.Object.Destroy(cameraTransform.gameObject);
		}
		GameManager.Instance.World.RemoveLocalPlayer(this);
		if ((bool)ScreenEffectManager)
		{
			ClearScreenEffects();
		}
		playerUI = null;
		windowManager = null;
		nguiWindowManager = null;
		moveController = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onNewBiomeEntered(BiomeDefinition _biome)
	{
		base.onNewBiomeEntered(_biome);
		EnvironmentAudioManager.Instance.EnterBiome(_biome);
		MinEventContext.Biome = _biome;
		FireEvent(MinEventTypes.onSelfEnteredBiome);
		QuestEventManager.Current.BiomeEntered(_biome);
	}

	public void OnInventoryChanged(Inventory _inventory)
	{
	}

	public bool IsMoveStateStill()
	{
		if (moveState != MoveState.Idle && moveState != MoveState.Crouch)
		{
			if (moveState == MoveState.Swim)
			{
				return !IsSwimmingMoving();
			}
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMoveStateToDefault()
	{
		SwimModeStop();
		if (!m_vp_FPController.enabled)
		{
			return;
		}
		vp_FPPlayerEventHandler player = m_vp_FPController.Player;
		if (base.IsCrouching)
		{
			if (MovementRunning && !player.Zoom.Active)
			{
				SetMoveState(MoveState.CrouchRun);
			}
			else if (player.InputMoveVector.Get() != Vector2.zero && m_vp_FPController.Velocity.sqrMagnitude > 0.01f)
			{
				SetMoveState(MoveState.CrouchWalk);
			}
			else
			{
				SetMoveState(MoveState.Crouch);
			}
		}
		else if (MovementRunning && !player.Zoom.Active)
		{
			SetMoveState(MoveState.Run);
		}
		else if (player.InputMoveVector.Get() != Vector2.zero && m_vp_FPController.Velocity.sqrMagnitude > 0.01f)
		{
			SetMoveState(MoveState.Walk);
		}
		else
		{
			SetMoveState(MoveState.Idle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMoveState(MoveState _state, bool _isOverride = false)
	{
		vp_FPPlayerEventHandler player = m_vp_FPController.Player;
		bool aimingGun = AimingGun;
		int value = inventory.holdingItem.HoldType.Value;
		bool flag = (value == 27 || value == 53 || value == 68) && SpecialAttack;
		if (_state != this.moveState)
		{
			MoveState moveState = this.moveState;
			if ((uint)(moveState - 7) <= 2u && _state != MoveState.Crouch && _state != MoveState.CrouchWalk && _state != MoveState.CrouchRun && !_isOverride)
			{
				if (!vp_FPCamera.HasOverheadSpace && !IsGodMode.Value)
				{
					_state = this.moveState;
				}
				else if (player.Crouch.Active && !player.Crouch.TryStop())
				{
					_state = this.moveState;
				}
				if (_state != this.moveState)
				{
					FireEvent(MinEventTypes.onSelfStand);
				}
			}
		}
		bool flag2 = _state != this.moveState;
		if (!flag2 && aimingGun == moveStateAiming && flag == moveStateHoldBow)
		{
			return;
		}
		m_vp_FPController.MotorDamping = 0.346f;
		m_vp_FPController.PhysicsSlopeSlideLimit = 60f;
		m_vp_FPController.PhysicsCrouchHeightModifier = 0.7f;
		SetMoveStateWeaponDamping(0.08f, 0.75f);
		if (m_vp_FPWeapon != null)
		{
			m_vp_FPWeapon.RotationLookSway = new Vector3(0.25f, 0.17f, 0f);
			m_vp_FPWeapon.RetractionDistance = 0.1f;
			m_vp_FPWeapon.BobRate = new Vector4(0.9f, 0.45f, 0f, 0f);
			m_vp_FPWeapon.BobAmplitude = new Vector4(0.35f, 0.5f, 0f, 0f);
			m_vp_FPWeapon.BobInputVelocityScale = 1f;
			m_vp_FPWeapon.ShakeSpeed = 0f;
			m_vp_FPWeapon.ShakeAmplitude = new Vector3(0.25f, 0f, 2f);
		}
		switch (_state)
		{
		case MoveState.Off:
			if (m_vp_FPController.enabled)
			{
				player.Crouch.Stop();
				player.Jump.Stop();
				m_vp_FPController.Stop();
				m_vp_FPController.enabled = false;
			}
			SwimModeStop();
			break;
		case MoveState.Attached:
			player.Crouch.Stop();
			player.Jump.Stop();
			SwimModeStop();
			break;
		case MoveState.Idle:
			m_vp_FPController.MotorAcceleration = 0.12f;
			m_vp_FPController.MotorBackwardsSpeed = 0.8f;
			m_vp_FPController.MotorSidewaysSpeed = 0.8f;
			m_vp_FPController.MotorSlopeSpeedDown = 1.2f;
			m_vp_FPController.MotorSlopeSpeedUp = 0.8f;
			if (flag2 && this.moveState != MoveState.Walk)
			{
				FireEvent(MinEventTypes.onSelfWalk);
			}
			break;
		case MoveState.Walk:
			m_vp_FPController.MotorAcceleration = 0.12f;
			m_vp_FPController.MotorBackwardsSpeed = 0.8f;
			m_vp_FPController.MotorSidewaysSpeed = 0.8f;
			m_vp_FPController.MotorSlopeSpeedDown = 1.2f;
			m_vp_FPController.MotorSlopeSpeedUp = 0.8f;
			SetMoveStateWeapon();
			if (flag2 && this.moveState != MoveState.Idle)
			{
				FireEvent(MinEventTypes.onSelfWalk);
			}
			break;
		case MoveState.Run:
			m_vp_FPController.MotorAcceleration = 0.35f;
			m_vp_FPController.MotorBackwardsSpeed = 0.8f;
			m_vp_FPController.MotorSidewaysSpeed = 0.5f;
			m_vp_FPController.MotorSlopeSpeedDown = 1.2f;
			m_vp_FPController.MotorSlopeSpeedUp = 0.8f;
			SetMoveStateWeapon();
			if (m_vp_FPWeapon != null)
			{
				m_vp_FPWeapon.BobRate = new Vector4(2f, 1f, 0f, 0f);
				m_vp_FPWeapon.BobAmplitude = new Vector4(1.5f, 1.2f, 0f, 0f);
			}
			if (flag2)
			{
				FireEvent(MinEventTypes.onSelfRun);
			}
			break;
		case MoveState.Crouch:
			player.Crouch.Start();
			m_vp_FPController.MotorAcceleration = 0.08f;
			m_vp_FPController.MotorBackwardsSpeed = 1f;
			m_vp_FPController.MotorSidewaysSpeed = 1f;
			m_vp_FPController.MotorSlopeSpeedDown = 1f;
			m_vp_FPController.MotorSlopeSpeedUp = 1f;
			if (flag2 && this.moveState != MoveState.CrouchWalk && this.moveState != MoveState.CrouchRun)
			{
				FireEvent(MinEventTypes.onSelfCrouch);
			}
			break;
		case MoveState.CrouchWalk:
			player.Crouch.Start();
			m_vp_FPController.MotorAcceleration = 0.08f;
			m_vp_FPController.MotorBackwardsSpeed = 1f;
			m_vp_FPController.MotorSidewaysSpeed = 1f;
			m_vp_FPController.MotorSlopeSpeedDown = 1f;
			m_vp_FPController.MotorSlopeSpeedUp = 1f;
			if (flag2)
			{
				FireEvent(MinEventTypes.onSelfCrouchWalk);
			}
			break;
		case MoveState.CrouchRun:
			player.Crouch.Start();
			m_vp_FPController.MotorAcceleration = 0.11f;
			m_vp_FPController.MotorBackwardsSpeed = 1f;
			m_vp_FPController.MotorSidewaysSpeed = 1f;
			m_vp_FPController.MotorSlopeSpeedDown = 1f;
			m_vp_FPController.MotorSlopeSpeedUp = 1f;
			if (flag2)
			{
				FireEvent(MinEventTypes.onSelfCrouchRun);
			}
			break;
		}
		if (flag)
		{
			SetMoveStateWeaponDamping(0.04f, 0.5f);
			if (m_vp_FPWeapon != null)
			{
				m_vp_FPWeapon.RotationLookSway = new Vector3(0.02f, 0.02f, 0f);
				m_vp_FPWeapon.RetractionDistance = 0f;
				m_vp_FPWeapon.BobAmplitude = new Vector4(0.1f, 0.05f, 0f, 0f);
				m_vp_FPWeapon.BobInputVelocityScale = 1f;
			}
		}
		if (aimingGun != moveStateAiming)
		{
			if (aimingGun)
			{
				FireEvent(MinEventTypes.onSelfAimingGunStart);
			}
			else
			{
				FireEvent(MinEventTypes.onSelfAimingGunStop);
			}
		}
		if (aimingGun)
		{
			SetMoveStateWeaponDamping(0.5f, 0.9f);
			if (m_vp_FPWeapon != null)
			{
				m_vp_FPWeapon.RotationLookSway = new Vector3(0.3f, 0.21f, 0f);
				m_vp_FPWeapon.RetractionDistance = 0f;
				m_vp_FPWeapon.ShakeSpeed = 0f;
				m_vp_FPWeapon.ShakeAmplitude = Vector3.zero;
				m_vp_FPWeapon.BobAmplitude = new Vector4(0.035f, 0.05f, 0f, 0f);
			}
		}
		m_vp_FPCamera.Refresh();
		if (m_vp_FPWeapon != null)
		{
			m_vp_FPWeapon.Refresh();
		}
		this.moveState = _state;
		moveStateAiming = aimingGun;
		moveStateHoldBow = flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMoveStateWeapon()
	{
		SetMoveStateWeaponDamping(0.01f, 0.25f);
		if (m_vp_FPWeapon != null)
		{
			m_vp_FPWeapon.RotationLookSway = new Vector3(0.3f, 0.21f, 0f);
			m_vp_FPWeapon.RetractionDistance = 0f;
			m_vp_FPWeapon.BobAmplitude = new Vector4(0.25f, 0.15f, 0f, 0f);
			m_vp_FPWeapon.BobInputVelocityScale = 100f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMoveStateWeaponDamping(float _stiffness, float _damping)
	{
		if (m_vp_FPWeapon != null)
		{
			m_vp_FPWeapon.PositionSpringStiffness = _stiffness;
			m_vp_FPWeapon.PositionSpringDamping = _damping;
			m_vp_FPWeapon.PositionPivotSpringStiffness = _stiffness;
			m_vp_FPWeapon.PositionPivotSpringDamping = _damping;
			m_vp_FPWeapon.RotationSpringStiffness = _stiffness;
			m_vp_FPWeapon.RotationSpringDamping = _damping;
			m_vp_FPWeapon.RotationPivotSpringStiffness = _stiffness;
			m_vp_FPWeapon.RotationPivotSpringDamping = _damping;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsMoveStateCrouch()
	{
		if (moveState != MoveState.Crouch && moveState != MoveState.CrouchWalk)
		{
			return moveState == MoveState.CrouchRun;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CalcIfSwimming()
	{
		float num = inWaterPercent;
		if (swimClimbing)
		{
			if (num >= 0.04f)
			{
				return !onGround;
			}
			return false;
		}
		if (IsMoveStateCrouch())
		{
			return num >= 0.7f;
		}
		return num >= 0.6f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwimModeTick()
	{
		vp_FPPlayerEventHandler player = m_vp_FPController.Player;
		SetMoveState(MoveState.Swim);
		if (swimMode < 0)
		{
			swimMode = 1;
			swimExhaustedTicks = 0;
			swimClimbing = false;
			FireEvent(MinEventTypes.onSelfSwimStart);
			emodel.avatarController.SetSwim(_enable: true);
			m_vp_FPController.ScaleFallSpeed(0.2f);
		}
		m_vp_FPController.MotorFreeFly = true;
		m_vp_FPController.MotorJumpForce = 0f;
		m_vp_FPController.MotorJumpForceDamping = 0f;
		m_vp_FPController.MotorJumpForceHold = 0f;
		m_vp_FPController.MotorJumpForceHoldDamping = 1f;
		m_vp_FPController.MotorAcceleration = 0.00032f;
		if (!Jumping && !inputWasDown && player.InputMoveVector.Get().SqrMagnitude() < 0.001f)
		{
			m_vp_FPController.PhysicsGravityModifier = 0.003f;
			if (swimMode != 0)
			{
				swimMode = 0;
				FireEvent(MinEventTypes.onSelfSwimIdle);
			}
		}
		else
		{
			m_vp_FPController.PhysicsGravityModifier = 0f;
			if (MovementRunning)
			{
				m_vp_FPController.MotorAcceleration = 0.0024f;
				if (swimMode != 2)
				{
					swimMode = 2;
					FireEvent(MinEventTypes.onSelfSwimRun);
				}
			}
			else if (swimMode != 1)
			{
				swimMode = 1;
			}
		}
		if (Stamina <= 0f)
		{
			swimExhaustedTicks = 60;
		}
		if (swimExhaustedTicks > 0)
		{
			swimExhaustedTicks--;
			m_vp_FPController.PhysicsGravityModifier = 0.004f;
			if (!isHeadUnderwater)
			{
				m_vp_FPController.PhysicsGravityModifier = 0.08f;
			}
			m_vp_FPController.MotorAcceleration = 0.00025f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsSwimmingMoving()
	{
		return swimMode > 0;
	}

	public void SwimModeUpdateThrottle()
	{
		float timeScale = Time.timeScale;
		float num = timeScale;
		if (swimExhaustedTicks > 0)
		{
			num *= 0.45f;
		}
		float num2 = 0.79f;
		float y = 0f;
		swimClimbing = false;
		if (inputWasJump && (vp_FPCamera.HasOverheadSpace || IsGodMode.Value))
		{
			if (onGround)
			{
				m_vp_FPController.m_MotorThrottle.y += 0.05f * num;
			}
			else
			{
				bool flag = false;
				if (swimExhaustedTicks == 0 && moveDirection.z > 0f)
				{
					Vector3 hipPosition = getHipPosition();
					Vector3 forwardVector = GetForwardVector();
					forwardVector.y = -0.25f;
					Ray ray = new Ray(hipPosition, forwardVector);
					float num3 = position.y + 0.12f;
					for (float num4 = hipPosition.y + 0.3f; num4 > num3; num4 -= 0.16f)
					{
						hipPosition.y = num4;
						ray.origin = hipPosition;
						if (Voxel.Raycast(world, ray, 0.45f, 1073807360, 65, 0.165f) && Voxel.phyxRaycastHit.normal.y > 0.3f)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					swimClimbing = true;
					num2 = 0.1f;
					y = 0.02f;
				}
				else
				{
					m_vp_FPController.m_MotorThrottle.y += 0.00052f * num;
				}
			}
		}
		else if (inputWasDown)
		{
			m_vp_FPController.m_MotorThrottle.y += -0.00038f * num;
		}
		Vector3 lookVector = GetLookVector();
		float num5 = m_vp_FPController.MotorAcceleration * num;
		m_vp_FPController.m_MotorThrottle += lookVector * (moveDirection.z * num5);
		m_vp_FPController.m_MotorThrottle += base.transform.TransformDirection(Vector3.right) * (moveDirection.x * num5 * 0.7f);
		float num6 = 0.01f + Mathf.Pow(m_vp_FPController.m_MotorThrottle.magnitude * 5.4f, 2f);
		m_vp_FPController.m_MotorThrottle /= 1f + num6 * timeScale;
		if (swimClimbing)
		{
			m_vp_FPController.m_MotorThrottle.y = y;
		}
		if (inWaterPercent < num2 && !IsInElevator() && m_vp_FPController.m_MotorThrottle.y > 0f)
		{
			m_vp_FPController.m_MotorThrottle.y *= 0.5f;
			if (inWaterPercent < num2 - 0.04f)
			{
				m_vp_FPController.m_MotorThrottle.y = 0f;
			}
		}
		m_vp_FPController.m_MotorThrottle = vp_MathUtility.SnapToZero(m_vp_FPController.m_MotorThrottle, 2E-05f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwimModeStop()
	{
		if (swimMode >= 0)
		{
			swimMode = -1;
			FireEvent(MinEventTypes.onSelfSwimStop);
			emodel.avatarController.SetSwim(_enable: false);
			m_vp_FPController.MotorFreeFly = false;
			m_vp_FPController.PhysicsGravityModifier = 0.2f;
			m_vp_FPController.MotorJumpForce = 0.13f;
			m_vp_FPController.MotorJumpForceDamping = 0.08f;
			m_vp_FPController.MotorJumpForceHold = 0.003f;
			m_vp_FPController.MotorJumpForceHoldDamping = 0.5f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void StartJumpSwimMotion()
	{
	}

	public override void PhysicsPush(Vector3 forceVec, Vector3 forceWorldPos, bool affectLocalPlayerController = false)
	{
		if (!IsGodMode.Value)
		{
			if (affectLocalPlayerController && vp_FPController != null)
			{
				vp_FPController.AddForce(forceVec);
			}
			else
			{
				base.PhysicsPush(forceVec, forceWorldPos, affectLocalPlayerController);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onSpawnStateChanged()
	{
		base.onSpawnStateChanged();
		if ((bool)vp_FPController && (!Spawned || moveController.respawnReason != RespawnType.Teleport || !IsFlyMode.Value))
		{
			vp_FPController.enabled = Spawned;
		}
		if (Spawned && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.Instance.prefabEditModeManager.IsActive())
		{
			GameManager.Instance.prefabEditModeManager.LoadRecentlyUsedOrCreateNew();
		}
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
		vp_FPController vp_FPController2 = vp_FPController;
		bool num = vp_FPController2 != null && !IsStuck && !IsFlyMode.Value;
		bool flag = true;
		bool flag2 = false;
		if (onGround)
		{
			isLadderAttached = false;
			canLadderAirAttach = true;
		}
		if (isLadderAttached)
		{
			speedStrafe = 0f;
		}
		bool flag3 = jumpTrigger;
		bool flag4 = flag3 != wasJumpTrigger;
		wasJumpTrigger = jumpTrigger;
		if (IsInElevator() && !IsFlyMode.Value)
		{
			flag2 = true;
			bool flag5 = false;
			if (flag3)
			{
				if (flag4)
				{
					if (isLadderAttached)
					{
						isLadderAttached = false;
						canLadderAirAttach = false;
						wasLadderAttachedJump = true;
					}
					else
					{
						canLadderAirAttach = true;
						wasLadderAttachedJump = false;
					}
				}
			}
			else
			{
				wasLadderAttachedJump = false;
			}
			if (!isLadderAttached)
			{
				if (vp_FPController2 != null)
				{
					bool flag6 = (vp_FPController2.enabled ? vp_FPController2.Grounded : onGround);
					if ((flag6 || isSwimming) && vp_FPController2.IsCollidingWall && vp_FPController2.ProjectedWallMove < 0.3f && _direction.z > 0f)
					{
						flag5 = true;
					}
					else if (!flag6 && canLadderAirAttach)
					{
						float y = vp_FPController2.Velocity.y;
						if (y <= 0f)
						{
							if (y >= -3f)
							{
								isLadderAttached = true;
								wasLadderAttachedJump = false;
							}
							else
							{
								vp_FPController2.ScaleFallSpeed(0.75f);
							}
						}
					}
				}
				else if (onGround && isCollidedHorizontally && projectedMove < 0.3f && _direction.z > 0f)
				{
					flag5 = true;
				}
				else if (!onGround)
				{
					isLadderAttached = true;
				}
			}
			Vector3 cameraLook = GetCameraLook(1f);
			if (flag5 && cameraLook.y > 0.1f)
			{
				isLadderAttached = true;
			}
			if (isLadderAttached)
			{
				SetMoveState(MoveState.Off, _isOverride: true);
				float num2 = (MovementRunning ? 0.17f : 0.06f);
				num2 *= GetSpeedModifier();
				if (_direction.x != 0f || _direction.z > 0f)
				{
					Vector3 direction = _direction;
					if (direction.z < 0f)
					{
						direction.z = 0f;
					}
					float num3 = num2 * 0.65f;
					Move(direction, _isDirAbsolute, num3, num3);
				}
				if (motion.x < -0.11f)
				{
					motion.x = -0.11f;
				}
				if (motion.x > 0.11f)
				{
					motion.x = 0.11f;
				}
				if (motion.z < -0.11f)
				{
					motion.z = -0.11f;
				}
				if (motion.z > 0.11f)
				{
					motion.z = 0.11f;
				}
				cameraLook.y += 0.15f;
				cameraLook.y *= 2f;
				cameraLook.y = Mathf.Clamp(cameraLook.y, -1f, 1f);
				motion.y = _direction.z * cameraLook.y * num2;
				fallDistance = 0f;
				entityCollision(motion);
				motion *= ScalePhysicsMulConstant(0.545f);
				distanceClimbed += motion.magnitude;
				if (distanceClimbed > 0.5f)
				{
					internalPlayStepSound();
					distanceClimbed = 0f;
				}
			}
			flag = !isLadderAttached;
		}
		else
		{
			isLadderAttached = false;
		}
		if (num && (!flag2 || flag))
		{
			vp_FPController2.enabled = true;
			motion = Vector3.zero;
			world.CheckEntityCollisionWithBlocks(this);
			if (vp_FPController2.Grounded)
			{
				Transform groundTransform = vp_FPController2.GroundTransform;
				if ((bool)groundTransform && groundTransform.CompareTag("LargeEntityBlocker"))
				{
					Vector2 randomOnUnitCircle = rand.RandomOnUnitCircle;
					vp_FPController2.AddForce(randomOnUnitCircle.x * 0.008f, 0f, randomOnUnitCircle.y * 0.008f);
				}
			}
			flag = false;
		}
		if (flag)
		{
			bool inElevator = IsInElevator();
			SetInElevator(_b: false);
			base.MoveEntityHeaded(_direction, _isDirAbsolute: false);
			SetInElevator(inElevator);
		}
	}

	public void SetCameraAttachedToPlayer(bool _b)
	{
		if (_b)
		{
			cameraTransform.SetParent(cameraContainerTransform, worldPositionStays: false);
			cameraTransform.SetAsFirstSibling();
			cameraTransform.SetLocalPositionAndRotation(Constants.cDefaultCameraPlayerOffset, Quaternion.identity);
			vp_FPCamera.Locked3rdPerson = false;
			movementInput.bDetachedCameraMove = false;
		}
		else
		{
			cameraTransform.parent = null;
			vp_FPCamera.Locked3rdPerson = true;
		}
	}

	public bool IsCameraAttachedToPlayerOrScope()
	{
		return cameraTransform.parent != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckNonSolidVertical(Vector3i blockPos, int maxY, int verticalSpace)
	{
		for (int i = 0; i < maxY; i++)
		{
			if (world.GetBlock(blockPos.x, blockPos.y + i + 1, blockPos.z).Block.shape.IsSolidSpace)
			{
				continue;
			}
			bool flag = true;
			for (int j = 1; j < verticalSpace; j++)
			{
				if (world.GetBlock(blockPos.x, blockPos.y + i + 1 + j, blockPos.z).Block.shape.IsSolidSpace)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		if (AttachedToEntity != null)
		{
			return;
		}
		if (m_vp_FPController != null)
		{
			if (m_vp_FPController.enabled)
			{
				position = vp_FPController.SmoothPosition + Origin.position;
			}
			else
			{
				float elapsedPartialTicks = GameTimer.Instance.elapsedPartialTicks;
				if (elapsedPartialTicks < 1f)
				{
					base.transform.position = lastTickPos[0] + (position - lastTickPos[0]) * elapsedPartialTicks - Origin.position;
				}
				else
				{
					base.transform.position = position - Origin.position;
				}
				vp_FPController.SetPosition(base.transform.position);
			}
			rotation = PhysicsTransform.eulerAngles;
			if (vp_FPCamera != null)
			{
				rotation.x = 0f - vp_FPCamera.Angle.x;
			}
			float num = base.width / 2f;
			float num2 = base.depth / 2f;
			boundingBox = BoundsUtils.BoundsForMinMax(position.x - num, position.y - yOffset + ySize, position.z - num2, position.x + num, position.y - yOffset + ySize + base.height, position.z + num2);
		}
		else
		{
			base.updateTransform();
		}
	}

	public override void SetRotation(Vector3 _rot)
	{
		base.SetRotation(_rot);
		if ((bool)PhysicsTransform && !emodel.IsRagdollActive)
		{
			PhysicsTransform.eulerAngles = _rot;
		}
		if ((bool)m_vp_FPCamera)
		{
			m_vp_FPCamera.Angle = new Vector2(0f - _rot.x, _rot.y);
		}
	}

	public override void OnUpdatePosition(float _partialTicks)
	{
		if (GameManager.bPlayRecordedSession && Spawned)
		{
			PlayerInputRecordingSystem.Instance.Play(this, _bPlayRelativeToNow: true);
		}
		if (m_vp_FPController != null)
		{
			ticksExisted++;
			prevPos = position;
			prevRotation = rotation;
			if (Spawned)
			{
				Vector3 zero = Vector3.zero;
				for (int i = 0; i < lastTickPos.Length - 1; i++)
				{
					zero += lastTickPos[i] - lastTickPos[i + 1];
				}
				zero /= (float)(lastTickPos.Length - 1);
				float num = Mathf.Sqrt(zero.x * zero.x + zero.z * zero.z);
				UpdateDistanceTravelledAchievement(num);
				if (AttachedToEntity == null)
				{
					updateStepSound(zero.x, zero.z);
					updatePlayerLandSound(num, zero.y);
				}
				else
				{
					distanceWalked += num;
				}
			}
			updateSpeedForwardAndStrafe(m_vp_FPController.Velocity, _partialTicks);
			ReplicateSpeeds();
		}
		else
		{
			base.OnUpdatePosition(_partialTicks);
		}
		if (Spawned)
		{
			if (position.y >= 2f && position.y < 4f)
			{
				PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.DepthAchieved, 1);
			}
			else if (position.y >= 255f)
			{
				PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.HeightAchieved, 1);
			}
		}
		GameSenseManager.Instance?.UpdateEventCompass(rotation.y);
		if (GameManager.bRecordNextSession && Spawned)
		{
			PlayerInputRecordingSystem.Instance.Record(this, GameTimer.Instance.ticks);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateDistanceTravelledAchievement(float distanceTravelled)
	{
		float num = distanceTravelled / 1000f;
		achievementDistanceAccu += num;
		if ((double)achievementDistanceAccu > 0.05)
		{
			if (DeviceFlags.Current != DeviceFlag.PS5)
			{
				PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.KMTravelled, achievementDistanceAccu);
			}
			else
			{
				PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.KMTravelled, 50);
			}
			achievementDistanceAccu -= 0.05f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
	{
		speedForward = 0f;
		speedStrafe = 0f;
		speedVertical = 0f;
		if (isLadderAttached)
		{
			speedForward += _dist.y;
			speedStrafe = 1234f;
			speedVertical = 0f;
		}
		else
		{
			if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
			{
				Vector3 vector = base.transform.InverseTransformDirection(_dist).normalized * _dist.magnitude;
				speedForward = (float)(int)(vector.z * 100f) / 100f;
				speedStrafe = (float)(int)(vector.x * 100f) / 100f;
			}
			if (Mathf.Abs(_dist.y) > 0.001f)
			{
				speedVertical += _dist.y;
			}
		}
		SetMovementState();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockRadiusEffects()
	{
		Vector3i blockPosition = GetBlockPosition();
		int num = World.toChunkXZ(blockPosition.x);
		int num2 = World.toChunkXZ(blockPosition.z);
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				Chunk chunk = (Chunk)world.GetChunkSync(num + j, num2 + i);
				if (chunk == null)
				{
					continue;
				}
				DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
				for (int k = 0; k < tileEntities.list.Count; k++)
				{
					TileEntity tileEntity = tileEntities.list[k];
					if (!tileEntity.IsActive(world))
					{
						continue;
					}
					Block block = world.GetBlock(tileEntity.ToWorldPos()).Block;
					if (block.RadiusEffects == null)
					{
						continue;
					}
					float distanceSq = GetDistanceSq(tileEntity.ToWorldPos().ToVector3());
					for (int l = 0; l < block.RadiusEffects.Length; l++)
					{
						BlockRadiusEffect blockRadiusEffect = block.RadiusEffects[l];
						if (distanceSq <= blockRadiusEffect.radius * blockRadiusEffect.radius && !Buffs.HasBuff(blockRadiusEffect.variable))
						{
							Buffs.AddBuff(blockRadiusEffect.variable);
						}
					}
				}
			}
		}
	}

	public override void OnUpdateEntity()
	{
		if (DropTimeDelay > 0f)
		{
			DropTimeDelay -= 0.05f;
		}
		float value = base.Stats.Health.Value;
		float num = (oldHealth - value) / base.Stats.Health.Max;
		float time = Time.time;
		if (value != oldHealth || time > vibrationTimeout)
		{
			if (GamePrefs.GetInt(EnumGamePrefs.OptionsControllerVibrationStrength) > 0 && playerInput != null && moveController.GetControllerVibration())
			{
				InputDevice inputDevice = playerInput.Device;
				if (inputDevice == null && playerInput.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					inputDevice = InputManager.ActiveDevice;
				}
				if (inputDevice != null)
				{
					if (oldHealth > value)
					{
						if (value <= 0f)
						{
							inputDevice.Vibrate(0.5f);
							GameManager.Instance.triggerEffectManager.SetGamepadVibration(0.5f);
						}
						else if (num > 0.25f)
						{
							inputDevice.Vibrate(0.5f);
							GameManager.Instance.triggerEffectManager.SetGamepadVibration(0.5f);
						}
						else if (num > 0.1f)
						{
							inputDevice.Vibrate(0.35f);
							GameManager.Instance.triggerEffectManager.SetGamepadVibration(0.35f);
						}
						else
						{
							inputDevice.Vibrate(0.25f);
							GameManager.Instance.triggerEffectManager.SetGamepadVibration(0.25f);
						}
						vibrationTimeout = Time.time + 0.25f;
					}
					else
					{
						inputDevice.StopVibration();
						GameManager.Instance.triggerEffectManager.StopGamepadVibration();
						vibrationTimeout = float.MaxValue;
					}
				}
			}
			if (num > 0.02f)
			{
				GameSenseManager.Instance?.UpdateEventHit();
			}
			GameSenseManager.Instance?.UpdateEventHealth((int)(base.Stats.Health.ValuePercentUI * 100f));
			oldHealth = value;
		}
		equipment.Update();
		base.OnUpdateEntity();
	}

	public override void OnUpdateLive()
	{
		if (IsSpawned())
		{
			if (Time.time - updateBedrollPositionChecks > 5f)
			{
				updateBedrollPositionChecks = Time.time;
				if (!CheckSpawnPointStillThere())
				{
					RemoveSpawnPoints();
				}
			}
			if (Time.time - updateRadiationChecks > 5f)
			{
				updateRadiationChecks = Time.time;
				IChunkProvider chunkProvider = world.ChunkCache.ChunkProvider;
				IBiomeProvider biomeProvider;
				if (chunkProvider != null && (biomeProvider = chunkProvider.GetBiomeProvider()) != null)
				{
					float radiationAt = biomeProvider.GetRadiationAt((int)position.x, (int)position.z);
					Buffs.SetCustomVar("_biomeradiation", radiationAt);
				}
			}
			GameEventManager.Current.UpdateCurrentBossGroup(this);
		}
		if (AttachedToEntity != null)
		{
			SetMoveState(MoveState.Attached, _isOverride: true);
			base.OnUpdateLive();
			if (!isEntityRemote)
			{
				updateBlockRadiusEffects();
			}
			IsStuck = false;
			return;
		}
		bool isStuck = IsStuck;
		IsStuck = false;
		if (!IsFlyMode.Value)
		{
			float num = boundingBox.min.y + 0.5f;
			IsStuck = pushOutOfBlocks(position.x - base.width * 0.3f, num, position.z + base.depth * 0.3f);
			IsStuck = pushOutOfBlocks(position.x - base.width * 0.3f, num, position.z - base.depth * 0.3f) || IsStuck;
			IsStuck = pushOutOfBlocks(position.x + base.width * 0.3f, num, position.z - base.depth * 0.3f) || IsStuck;
			IsStuck = pushOutOfBlocks(position.x + base.width * 0.3f, num, position.z + base.depth * 0.3f) || IsStuck;
			if (!IsStuck)
			{
				int num2 = Utils.Fastfloor(position.x);
				int num3 = Utils.Fastfloor(num);
				int num4 = Utils.Fastfloor(position.z);
				if (shouldPushOutOfBlock(num2, num3, num4, pushOutOfTerrain: true))
				{
					if (!shouldPushOutOfBlock(num2 - 1, num3, num4, pushOutOfTerrain: true))
					{
						IsStuck = true;
						motion = new Vector3(-0.25f, 0f, 0f);
					}
					else if (!shouldPushOutOfBlock(num2 + 1, num3, num4, pushOutOfTerrain: true))
					{
						IsStuck = true;
						motion = new Vector3(0.25f, 0f, 0f);
					}
					if (!shouldPushOutOfBlock(num2, num3, num4 - 1, pushOutOfTerrain: true))
					{
						IsStuck = true;
						motion = new Vector3(0f, 0f, -0.25f);
					}
					else if (!shouldPushOutOfBlock(num2, num3, num4 + 1, pushOutOfTerrain: true))
					{
						IsStuck = true;
						motion = new Vector3(0f, 0f, 0.25f);
					}
					else if (CheckNonSolidVertical(new Vector3i(num2, num3 + 1, num4), 4, 2))
					{
						IsStuck = true;
						motion = new Vector3(0f, 1.6f, 0f);
						Log.Warning("{0} Player is stuck, trying to unstick", Time.frameCount);
					}
				}
			}
		}
		bool flag = true;
		bool flag2 = false;
		bool flag3 = InAir;
		InAir = !isLadderAttached && !onGround;
		if (!wasJumping && !jumpTrigger && flag3 && !InAir && !isLadderAttached)
		{
			EndJump();
		}
		if (m_vp_FPController != null)
		{
			if (IsStuck || IsFlyMode.Value)
			{
				SetMoveState(MoveState.Off, _isOverride: true);
			}
			else
			{
				flag = false;
				base.Stats.Health.RegenerationAmount = 0f;
				base.Stats.Stamina.RegenerationAmount = 0f;
				if (isStuck != IsStuck)
				{
					m_vp_FPController.Stop();
				}
				if (m_vp_FPController.enabled)
				{
					onGround = m_vp_FPController.Grounded;
				}
				bool flag4 = jumpTrigger;
				if (isSwimming)
				{
					SwimModeTick();
					flag2 = true;
				}
				else
				{
					if (vp_FPCamera != null && m_vp_FPController != null && m_vp_FPWeapon != null)
					{
						SetMoveStateToDefault();
					}
					if (flag4 && (vp_FPCamera.HasOverheadSpace || IsGodMode.Value))
					{
						vp_Activity jump = m_vp_FPController.Player.Jump;
						bool active = jump.Active;
						m_vp_FPController.MotorJumpForce = Mathf.Max(EffectManager.GetValue(PassiveEffects.JumpStrength, null, m_vp_FPController.originalMotorJumpForce, this, null, CurrentStanceTag | CurrentMovementTag), 0f);
						m_vp_FPController.MotorJumpForceHold = m_vp_FPController.MotorJumpForce / Mathf.Lerp(90f, 180f, Mathf.Clamp01(1f - m_vp_FPController.originalMotorJumpForce / m_vp_FPController.MotorJumpForce)) * Time.timeScale;
						if (IsInElevator())
						{
							if (!active && !wasJumping && (onGround || isLadderAttached))
							{
								jump.Start();
							}
						}
						else if (!wasJumping)
						{
							jump.TryStart();
						}
						if (!active && jump.Active)
						{
							Jumping = true;
							Stamina -= Mathf.Max(EffectManager.GetValue(PassiveEffects.StaminaLoss, null, 4f, this, null, FastTags<TagGroup.Global>.Parse("jumping") | CurrentStanceTag | CurrentMovementTag), 0f);
							FireEvent(MinEventTypes.onSelfJump);
							PlayOneShot(GetSoundJump());
						}
						if (onGround && wasJumping)
						{
							Jumping = false;
							jumpTrigger = false;
						}
						if (isLadderAttached && wasJumping)
						{
							bJumping = false;
							jumpTrigger = false;
						}
					}
					else
					{
						m_vp_FPController.Player.Jump.Stop();
					}
				}
				wasJumping = flag4;
			}
		}
		if (flag)
		{
			base.OnUpdateLive();
		}
		else
		{
			CheckSleeperTriggers();
			base.Stats.Update(0.05f, world.worldTime);
			m_vp_FPController.SpeedModifier = GetSpeedModifier() * Mathf.Clamp01(EffectManager.GetValue(PassiveEffects.Mobility, null, 1f, this)) * (isMotionSlowedDown ? motionMultiplier : 1f);
			isMotionSlowedDown = false;
			updateCurrentBlockPosAndValue();
			if (canEntityMove())
			{
				MoveEntityHeaded(moveDirection, _isDirAbsolute: false);
			}
			checkForTeleportOutOfTraderArea();
		}
		if (challengeJournal != null)
		{
			challengeJournal.Update(world);
		}
		if (QuestJournal != null)
		{
			QuestJournal.Update(world.WorldDay);
		}
		if (!isEntityRemote)
		{
			updateBlockRadiusEffects();
		}
		if (--inventorySendCounter <= 0)
		{
			inventorySendCounter = 40;
			ResendPlayerInventory();
		}
		if (Stamina <= 0f)
		{
			bExhausted = true;
			Stamina = 0f;
		}
		float stamina = Stamina;
		if (bExhausted && stamina > base.Stats.Stamina.Max * 0.2f)
		{
			isExhaustedSoundAllowed = true;
			bExhausted = false;
		}
		if (bExhausted && isExhaustedSoundAllowed)
		{
			PlayOneShot(GetSoundStamina());
			GameManager.ShowTooltip(this, "ttOutOfStamina");
			isExhaustedSoundAllowed = false;
		}
		if (prevStaminaValue >= stamina - 0.1f && moveState == MoveState.Run && !flag2)
		{
			runTicks++;
			if (runTicks > 100 && stamina / base.Stats.Stamina.Max < 0.5f && sprintLoopSoundPlayId == -1 && !IsDead())
			{
				Manager.BroadcastPlay(this, "Player" + (IsMale ? "Male" : "Female") + "RunLoop");
				sprintLoopSoundPlayId = 0;
			}
			lerpCameraFastFOV += Time.deltaTime * Constants.cRunningFOVSpeedUp;
		}
		else
		{
			if (sprintLoopSoundPlayId != -1)
			{
				Manager.BroadcastStop(entityId, "Player" + (IsMale ? "Male" : "Female") + "RunLoop");
				sprintLoopSoundPlayId = -1;
				if (isExhaustedSoundAllowed)
				{
					PlayOneShot(GetSoundStamina());
				}
			}
			lerpCameraFastFOV -= Time.deltaTime * Constants.cRunningFOVSpeedDown;
			runTicks = 0;
		}
		lerpCameraFastFOV = Mathf.Clamp01(lerpCameraFastFOV);
		prevStaminaValue = stamina;
		if (playerUI != null && playerUI.windowManager.IsModalWindowOpen())
		{
			TryCancelBowDraw();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canEntityMove()
	{
		bool result = true;
		if (!IsFlyMode.Value)
		{
			Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)(position.x + moveDirection.x * 2f), (int)position.y, (int)(position.z + moveDirection.z * 2f));
			if (chunk == null || !chunk.IsCollisionMeshGenerated)
			{
				result = false;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldPushOutOfBlock(int _x, int _y, int _z, bool pushOutOfTerrain)
	{
		BlockShape shape = world.GetBlock(_x, _y, _z).Block.shape;
		if (shape.IsSolidSpace && !shape.IsTerrain())
		{
			return true;
		}
		if (pushOutOfTerrain && shape.IsSolidSpace && shape.IsTerrain())
		{
			BlockShape shape2 = world.GetBlock(_x, _y + 1, _z).Block.shape;
			if (shape2.IsSolidSpace && shape2.IsTerrain())
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pushOutOfBlocks(float _x, float _y, float _z)
	{
		int num = Utils.Fastfloor(_x);
		int num2 = Utils.Fastfloor(_y);
		int num3 = Utils.Fastfloor(_z);
		float num4 = _x - (float)num;
		float num5 = _z - (float)num3;
		bool result = false;
		bool flag = base.IsCrouching || IsMoveStateCrouch();
		if (shouldPushOutOfBlock(num, num2, num3, pushOutOfTerrain: false) || (!flag && shouldPushOutOfBlock(num, num2 + 1, num3, pushOutOfTerrain: false)))
		{
			bool num6 = !shouldPushOutOfBlock(num - 1, num2, num3, pushOutOfTerrain: true) && !shouldPushOutOfBlock(num - 1, num2 + 1, num3, pushOutOfTerrain: true);
			bool flag2 = !shouldPushOutOfBlock(num + 1, num2, num3, pushOutOfTerrain: true) && !shouldPushOutOfBlock(num + 1, num2 + 1, num3, pushOutOfTerrain: true);
			bool flag3 = !shouldPushOutOfBlock(num, num2, num3 - 1, pushOutOfTerrain: true) && !shouldPushOutOfBlock(num, num2 + 1, num3 - 1, pushOutOfTerrain: true);
			bool flag4 = !shouldPushOutOfBlock(num, num2, num3 + 1, pushOutOfTerrain: true) && !shouldPushOutOfBlock(num, num2 + 1, num3 + 1, pushOutOfTerrain: true);
			byte b = byte.MaxValue;
			float num7 = 9999f;
			if (num6 && num4 < num7)
			{
				num7 = num4;
				b = 0;
			}
			if (flag2 && 1.0 - (double)num4 < (double)num7)
			{
				num7 = 1f - num4;
				b = 1;
			}
			if (flag3 && num5 < num7)
			{
				num7 = num5;
				b = 4;
			}
			if (flag4 && 1f - num5 < num7)
			{
				b = 5;
			}
			float num8 = 0.1f;
			if (b == 0)
			{
				motion.x = 0f - num8;
			}
			if (b == 1)
			{
				motion.x = num8;
			}
			if (b == 4)
			{
				motion.z = 0f - num8;
			}
			if (b == 5)
			{
				motion.z = num8;
			}
			if (b != byte.MaxValue)
			{
				result = true;
			}
		}
		return result;
	}

	public override void Move(Vector3 _direction, bool _isDirAbsolute, float _velocity, float _maxVelocity)
	{
		base.Move(_direction, _isDirAbsolute, _velocity, _maxVelocity);
	}

	public override void SetAlive()
	{
		base.SetAlive();
		if (PhysicsTransform != null)
		{
			PhysicsTransform.gameObject.layer = 20;
		}
		if (m_vp_FPController != null)
		{
			m_vp_FPController.Player.Dead.Stop();
		}
		SetModelLayer(24);
		ShowHoldingItem(show: true);
		bPlayerStatsChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LateUpdate()
	{
		if (bLerpCameraFlag)
		{
			lerpCameraLerpValue += Time.deltaTime * 4f;
			if (lerpCameraLerpValue >= 1f)
			{
				bLerpCameraFlag = false;
				playerCamera.fieldOfView = lerpCameraEndFOV;
			}
			else
			{
				playerCamera.fieldOfView = Mathf.Lerp(lerpCameraStartFOV, lerpCameraEndFOV, lerpCameraLerpValue);
			}
		}
		if (!AimingGun)
		{
			float num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
			float a = playerCamera.fieldOfView;
			if (lerpCameraLerpValue <= 0f || !bLerpCameraFlag)
			{
				a = num;
			}
			playerCamera.fieldOfView = Mathf.Lerp(a, num * Constants.cRunningFOVMultiplier, lerpCameraFastFOV);
			if (OverrideFOV != -1f)
			{
				playerCamera.fieldOfView = OverrideFOV;
				playerCamera.transform.LookAt(OverrideLookAt - Origin.position);
			}
			else if (lastOverrideFOV != -1f)
			{
				playerCamera.fieldOfView = num;
			}
			lastOverrideFOV = OverrideFOV;
		}
		WorldBoundsUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldBoundsUpdate()
	{
		InWorldPercent = world.InBoundsForPlayersPercent(position);
		InWorldLookPercent = InWorldPercent;
		if (InWorldPercent < 1f && !AttachedToEntity && !IsFlyMode.Value)
		{
			Vector3 forward = playerCamera.transform.forward;
			InWorldLookPercent = world.InBoundsForPlayersPercent(position + forward * 80f * 0.3f);
			if (InWorldPercent <= 0.05f)
			{
				world.GetWorldExtent(out var _minSize, out var _maxSize);
				Vector2 vector = default(Vector2);
				vector.x = (float)(_minSize.x + _maxSize.x) * 0.5f;
				vector.y = (float)(_minSize.z + _maxSize.z) * 0.5f;
				Vector3 normalized = new Vector3(vector.x - position.x, 0f, vector.y - position.z).normalized;
				float num = (1f - InWorldPercent / 0.05f) * 0.8f * Time.deltaTime;
				m_vp_FPController.AddForce(normalized.x * num, 0f, normalized.z * num);
				GameManager.ShowTooltip(this, Localization.Get("ttWorldEnd"));
			}
		}
	}

	public void UnderwaterCameraFrameUpdate()
	{
		bool flag = UnderwaterCameraCheck();
		if (IsUnderwaterCamera == flag)
		{
			return;
		}
		IsUnderwaterCamera = flag;
		Shader.SetGlobalFloat("_UnderWater", flag ? 1 : 0);
		if ((bool)uwEffectHaze)
		{
			uwEffectHaze.gameObject.SetActive(flag);
		}
		if ((bool)uwEffectRefract)
		{
			uwEffectRefract.gameObject.SetActive(flag);
		}
		if ((bool)uwEffectDebris)
		{
			uwEffectDebris.gameObject.SetActive(flag);
		}
		if (!flag)
		{
			if ((bool)uwEffectDroplets)
			{
				uwEffectDroplets.gameObject.SetActive(value: true);
				uwEffectDroplets.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled = true;
				uwEffectDroplets.GetComponent<ParticleSystem>().Play();
				uwEffectDroplets.GetComponent<ParticleSystem>().Emit(rand.RandomRange(60, 120));
			}
			if ((bool)uwEffectWaterFade)
			{
				uwEffectWaterFade.gameObject.SetActive(value: true);
				uwEffectWaterFade.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled = true;
				uwEffectWaterFade.GetComponent<ParticleSystem>().Play();
				uwEffectWaterFade.GetComponent<ParticleSystem>().Emit(1);
			}
		}
	}

	public bool UnderwaterCameraCheck()
	{
		Vector3 pos = cameraTransform.position + Origin.position;
		pos.y += 0.28f;
		if (UnderwaterCameraCheckPos(pos))
		{
			return true;
		}
		Vector2 forwardVector = GetForwardVector2();
		pos.x -= forwardVector.x * 0.3f;
		pos.z -= forwardVector.y * 0.3f;
		if (UnderwaterCameraCheckPos(pos))
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UnderwaterCameraCheckPos(Vector3 pos)
	{
		Vector3i pos2 = World.worldToBlockPos(pos);
		float waterPercent = world.GetWaterPercent(pos2);
		if (waterPercent > 0f)
		{
			return (float)pos2.y + waterPercent - pos.y > 0f;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool IsHeadUnderwater()
	{
		return inWaterPercent >= 0.791f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHeadUnderwaterStateChanged(bool _bUnderwater)
	{
		base.OnHeadUnderwaterStateChanged(_bUnderwater);
		if (_bUnderwater)
		{
			Buffs.SetCustomVar("_underwater", 1f);
		}
		else if (!IsDead())
		{
			Buffs.SetCustomVar("_underwater", 0f);
			if (soundWaterSurface != null && Time.time - lastTimeUnderwater > 3f)
			{
				Manager.BroadcastPlay(this, soundWaterSurface);
			}
		}
		else
		{
			Buffs.SetCustomVar("_underwater", 0f);
		}
		lastTimeUnderwater = Time.time;
	}

	public override void SetDead()
	{
		base.SetDead();
		lastHitDirection = Utils.EnumHitDirection.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CameraDOFInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CameraDOFFrameUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		renderManager.FrameUpdate();
		if (bPlayingSpawnIn)
		{
			WeatherManager.Instance.PushTransitions();
		}
		CameraDOFFrameUpdate();
		float time = Time.time;
		bool flag = IsDead();
		if (Spawned != wasSpawned)
		{
			if (!flag && lastRespawnReason != RespawnType.Teleport)
			{
				bPlayingSpawnIn = true;
				spawnInTime = time;
				Manager.PlayInsidePlayerHead("spawnInStinger");
			}
			wasSpawned = Spawned;
		}
		if (flag)
		{
			if (deathTime == 0f)
			{
				deathTime = time;
				Manager.PlayInsidePlayerHead("player_death_stinger", entityId);
			}
		}
		else if (deathTime > 0f)
		{
			ClearScreenEffects();
			deathTime = 0f;
		}
		float num = Health;
		if (num < dyingEffectHealthLast - 3f)
		{
			dyingEffectCur = Mathf.Clamp01(1f - Mathf.Clamp(num, 0f, 70f) / 70f);
			dyingEffectHitTime = time;
		}
		dyingEffectHealthLast = num;
		dyingEffectCur *= Mathf.Clamp01(1f - (time - dyingEffectHitTime) / 600f);
		if (dyingEffectCur < 0.01f)
		{
			dyingEffectCur = 0f;
		}
		if (dyingEffectLast != dyingEffectCur)
		{
			dyingEffectLast = dyingEffectCur;
			if (!flag)
			{
				ScreenEffectManager.SetScreenEffect("Dying", dyingEffectCur, 0f);
			}
		}
		if (bPlayingSpawnIn && time < spawnInTime + spawnInEffectSpeed)
		{
			spawnInIntensity = Mathf.Clamp01(1f - (time - spawnInTime) / spawnInEffectSpeed);
			ScreenEffectManager.SetScreenEffect("VibrantDeSat", spawnInIntensity);
			bPlayingSpawnIn = spawnInIntensity > 0f;
		}
		else if (spawnInIntensity > 0f)
		{
			spawnInIntensity = 0f;
			ScreenEffectManager.SetScreenEffect("VibrantDeSat", 0f);
			bPlayingSpawnIn = false;
		}
		if ((double)GetCVar("_underwater") > 0.1 && equipment.IsNaked() && biomeStandingOn != null && biomeStandingOn.m_sBiomeName == "snow")
		{
			PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.SubZeroNakedSwim, 1);
		}
		ProgressionValue progressionValue = Progression.GetProgressionValue("attFortitude");
		if (progressionValue != null)
		{
			PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.HighestFortitude, progressionValue.Level);
		}
		PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.HighestGamestage, base.gameStage);
		PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.HighestPlayerLevel, Progression.Level);
		if (base.RentedVMPosition != Vector3i.zero && RentalEndDay <= GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime))
		{
			base.RentedVMPosition = Vector3i.zero;
			RentalEndTime = 0uL;
			RentalEndDay = 0;
		}
		sneakDamageBlendTimer.Tick(Time.deltaTime);
		ThreatLevel.Numeric = ThreatLevelUtility.GetThreatLevelOn(this);
		if (Spawned)
		{
			AudioListener.volume = Mathf.Lerp(AudioListener.volume, GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel), Time.deltaTime);
		}
		float num2 = GamePrefs.GetFloat(EnumGamePrefs.OptionsAmbientVolumeLevel) * biomeVolume;
		if (audioSourceBiomeActive.isPlaying && Utils.FastAbs(audioSourceBiomeActive.volume - num2 * 0.95f) > 0.01f)
		{
			audioSourceBiomeActive.volume = Mathf.Lerp(audioSourceBiomeActive.volume, num2, Time.deltaTime);
			if (audioSourceBiomeActive.volume > num2 * 0.95f)
			{
				audioSourceBiomeActive.volume = num2;
			}
		}
		if (audioSourceBiomeFadeOut.isPlaying && audioSourceBiomeFadeOut.volume > 0.001f)
		{
			audioSourceBiomeFadeOut.volume = Mathf.Lerp(audioSourceBiomeFadeOut.volume, 0f, Time.deltaTime);
			if ((double)audioSourceBiomeFadeOut.volume < 0.05)
			{
				audioSourceBiomeFadeOut.clip = null;
				audioSourceBiomeFadeOut.Stop();
			}
		}
		FrameUpdateCamera();
		if (!GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		if (emodel.IsRagdollActive || (IsDead() && bSwitchCameraBackAfterRespawn))
		{
			SelfCameraFrameUpdate();
		}
		if (inventory.IsHoldingGun() && inventory.GetHoldingGun() is ItemActionRanged)
		{
			float num3 = (float)Screen.width / cameraTransform.GetComponent<Camera>().fieldOfView;
			float num4 = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, inventory.holdingItemData.itemValue, 90f, this) * num3;
			float num5 = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, inventory.holdingItemData.itemValue, 90f, this) * num3;
			if (num4 > num5)
			{
				crossHairOpenArea = (float)(int)num4 * (inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged).lastAccuracy;
			}
			else
			{
				crossHairOpenArea = (float)(int)num5 * (inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged).lastAccuracy;
			}
		}
		else
		{
			Vector3 vector = prevPos - position;
			float b = Mathf.Max(20f, Mathf.Clamp01(Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z) * 3f) * 100f);
			crossHairOpenArea = Mathf.Lerp(crossHairOpenArea, b, Time.deltaTime * 4f);
		}
		if (autoMove != null)
		{
			autoMove.Update();
		}
		lock (backpackPositionsFromThread)
		{
			if (backpackPositionsFromThread.Count > 0)
			{
				SetDroppedBackpackPositions(backpackPositionsFromThread);
				backpackPositionsFromThread.Clear();
			}
		}
		if (IsAlive())
		{
			recoveryPointTimer += Time.deltaTime;
			if (recoveryPointTimer > 30f && onGround)
			{
				TryAddRecoveryPosition(Vector3i.FromVector3Rounded(position));
				recoveryPointTimer = 0f;
			}
		}
	}

	public void HandleHordeEvent(AIDirector.HordeEvent msg)
	{
		string text = null;
		switch (msg)
		{
		case AIDirector.HordeEvent.Warn2:
			text = "Enemies/Horde/horde_spawn_warning";
			break;
		case AIDirector.HordeEvent.Spawn:
			GameManager.Instance.StartCoroutine(shakeCamera(Vector3.one, 1f, 50f, 5f));
			text = "Enemies/Horde/horde_spawn";
			break;
		}
		if (text != null)
		{
			Manager.PlayInsidePlayerHead(text);
		}
	}

	public override void OnFired()
	{
		base.OnFired();
		Vector2 vector = new Vector2(EffectManager.GetValue(PassiveEffects.KickDegreesHorizontalMin, inventory.holdingItemItemValue, 0f, this), EffectManager.GetValue(PassiveEffects.KickDegreesHorizontalMax, inventory.holdingItemItemValue, 0f, this));
		Vector2 vector2 = new Vector2(EffectManager.GetValue(PassiveEffects.KickDegreesVerticalMin, inventory.holdingItemItemValue, 0f, this), EffectManager.GetValue(PassiveEffects.KickDegreesVerticalMax, inventory.holdingItemItemValue, 0f, this));
		if (vector.x != 0f || vector.y != 0f || vector2.x != 0f || vector2.y != 0f)
		{
			switch (inventory.holdingItem.GetCameraShakeType(inventory.holdingItemData))
			{
			case EnumCameraShake.Big:
				GameManager.Instance.StartCoroutine(shakeCamera(Vector3.one, 0.5f, 20f));
				break;
			case EnumCameraShake.Small:
				GameManager.Instance.StartCoroutine(shakeCamera(Vector3.one, 0.5f, 10f));
				break;
			case EnumCameraShake.Tiny:
				GameManager.Instance.StartCoroutine(shakeCamera(Vector3.one, 0.5f, 5f));
				break;
			}
		}
		TriggerEffectManager.ControllerTriggerEffect controllerTriggerEffectShoot = inventory.holdingItem.GetControllerTriggerEffectShoot();
		if (controllerTriggerEffectShoot.XboxTriggerEffect.Effect != TriggerEffectManager.EffectXbox.Off || controllerTriggerEffectShoot.DualsenseEffect.Effect != TriggerEffectManager.EffectDualsense.Off)
		{
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, controllerTriggerEffectShoot);
		}
		if (bFirstPersonView)
		{
			if (!AimingGun)
			{
				movementInput.rotation.x += rand.RandomRange(vector2.x, vector2.y) * 2f;
				movementInput.rotation.y += rand.RandomRange(vector.x, vector.y) * 2f;
			}
			else
			{
				movementInput.rotation.x += rand.RandomRange(vector2.x, vector2.y);
				movementInput.rotation.y += rand.RandomRange(vector.x, vector.y);
			}
		}
	}

	public int GetCrosshairOpenArea()
	{
		return (int)crossHairOpenArea;
	}

	public Vector2 GetCrosshairPosition2D()
	{
		Vector3 vector = finalCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0f));
		return new Vector2(vector.x, vector.y);
	}

	public Vector3 GetCrosshairPosition3D(float _z, float _attributeOffset2D, Vector3 _altStartPosition)
	{
		if (!playerCamera.enabled || movementInput.bCameraChange)
		{
			return _altStartPosition;
		}
		Vector2 crosshairPosition2D = GetCrosshairPosition2D();
		crosshairPosition2D.x += rand.RandomRange(0f - _attributeOffset2D, _attributeOffset2D) * 700f;
		crosshairPosition2D.y += rand.RandomRange(0f - _attributeOffset2D, _attributeOffset2D) * 700f;
		Vector3 result = playerCamera.ScreenToWorldPoint(new Vector3(crosshairPosition2D.x, crosshairPosition2D.y, _z)) + Origin.position;
		if (!bFirstPersonView)
		{
			result += GetLookVector() * (0.94f + movementInput.cameraDistance);
		}
		return result;
	}

	public override void OnHoldingItemChanged()
	{
		if (!IsDead())
		{
			SetModelLayer(24);
		}
	}

	public override void SetModelLayer(int _layerId, bool force = false, string[] excludeTags = null)
	{
		if (emodel == null)
		{
			return;
		}
		Transform modelTransform = emodel.GetModelTransform();
		if (!(modelTransform == null) && (oldLayer != _layerId || force))
		{
			oldLayer = _layerId;
			Utils.SetLayerWithExclusionList(modelTransform.gameObject, _layerId, excludeTags);
			if (_layerId == 24 && modelTransform.childCount > 0)
			{
				Utils.SetLayerWithExclusionList(modelTransform.GetChild(0).gameObject, _layerId, excludeTags);
			}
			modelTransform.gameObject.GetComponentsInChildren(includeInactive: true, setLayerRecursivelyList);
			for (int num = setLayerRecursivelyList.Count - 1; num >= 0; num--)
			{
				Utils.SetLayerWithExclusionList(setLayerRecursivelyList[num].gameObject, _layerId, excludeTags);
			}
			setLayerRecursivelyList.Clear();
		}
	}

	public virtual void MoveByInput()
	{
		bool isCrouching = base.IsCrouching;
		if (IsStuck || EffectManager.GetValue(PassiveEffects.DisableMovement, null, 0f, this) > 0f)
		{
			movementInput.Clear();
		}
		if (EffectManager.GetValue(PassiveEffects.FlipControls, null, 0f, this) > 0f)
		{
			movementInput.moveForward *= -1f;
			movementInput.moveStrafe *= -1f;
		}
		if (AttachedToEntity != null)
		{
			Crouching = false;
			CrouchingLocked = false;
			base.Climbing = false;
			MovementRunning = false;
			AimingGun = false;
			AttachedToEntity.MoveByAttachedEntity(this);
		}
		else
		{
			bool flag = false;
			moveDirection.x = movementInput.moveStrafe;
			moveDirection.z = movementInput.moveForward;
			if (moveDirection.x != 0f || moveDirection.z != 0f)
			{
				flag = true;
			}
			bool flag2 = (IsSwimming() ? (swimExhaustedTicks == 0) : (!bExhausted && moveDirection.z > 0f));
			bool flag3 = movementInput.running && flag;
			if (!IsFlyMode.Value)
			{
				flag3 = flag3 && flag2;
			}
			MovementRunning = flag3;
			if (IsSwimming())
			{
				if (!IsSwimmingMoving() || swimExhaustedTicks > 0)
				{
					CurrentMovementTag = EntityAlive.MovementTagFloating;
				}
				else if (!MovementRunning)
				{
					CurrentMovementTag = EntityAlive.MovementTagSwimming;
				}
				else
				{
					CurrentMovementTag = EntityAlive.MovementTagSwimmingRun;
				}
			}
			else if (flag)
			{
				if (!MovementRunning)
				{
					CurrentMovementTag = EntityAlive.MovementTagWalking;
				}
				else
				{
					CurrentMovementTag = EntityAlive.MovementTagRunning;
				}
			}
			else
			{
				CurrentMovementTag = EntityAlive.MovementTagIdle;
			}
			if (movementInput.downToggle)
			{
				CrouchingLocked = !CrouchingLocked;
			}
			CrouchingLocked = CrouchingLocked && !isLadderAttached && !movementInput.down;
			Crouching = !IsFlyMode.Value && !isLadderAttached && (movementInput.down || CrouchingLocked);
			if (!AimingGun)
			{
				if (!IsFlyMode.Value)
				{
					if (!JetpackWearing)
					{
						if (movementInput.jump && (bool)vp_FPController && !inputWasJump)
						{
							vp_FPController.enabled = true;
						}
						if (!Jumping && !wasJumping && movementInput.jump && (onGround || isLadderAttached) && AttachedToEntity == null)
						{
							jumpTrigger = true;
						}
						else if (wasLadderAttachedJump && !isLadderAttached && movementInput.jump && !inputWasJump)
						{
							canLadderAirAttach = true;
						}
					}
					else
					{
						if (movementInput.jump)
						{
							motion.y += 0.15f;
							flag = true;
						}
						if (movementInput.down)
						{
							motion.y -= 0.15f;
						}
					}
				}
				else
				{
					if (movementInput.jump)
					{
						if (movementInput.running)
						{
							motion.y = 0.9f;
						}
						else
						{
							motion.y = 0.3f * GodModeSpeedModifier;
						}
					}
					if (movementInput.down)
					{
						if (movementInput.running)
						{
							motion.y = -0.9f;
						}
						else
						{
							motion.y = -0.3f * GodModeSpeedModifier;
						}
					}
				}
			}
			JetpackActive = JetpackWearing && flag;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			bool isCrouching2 = base.IsCrouching;
			if (isCrouching2 != isCrouching)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityStealth>().Setup(this, isCrouching2));
			}
		}
		if (vp_FPController != null)
		{
			if (AttachedToEntity == null)
			{
				vp_FPController.Player.InputMoveVector.Set(new Vector2(moveDirection.x, moveDirection.z));
			}
			vp_FPController.Player.InputSmoothLook.Set(new Vector2(movementInput.rotation.y, 0f - movementInput.rotation.x));
		}
		inputWasJump = movementInput.jump;
		inputWasDown = movementInput.down;
		movementInput.Clear();
	}

	public void SwitchFirstPersonViewFromInput()
	{
		bool flag = !bFirstPersonView;
		if (!GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			if (bFirstPersonView)
			{
				return;
			}
			flag = true;
		}
		SetFirstPersonView(flag, _bLerpPosition: true);
		if (!bFirstPersonView)
		{
			vp_FPCamera vp_FPCamera2 = vp_FPCamera;
			if ((bool)vp_FPCamera2)
			{
				vp_FPCamera2.m_Current3rdPersonBlend = 1f;
			}
		}
	}

	public void SwitchFirstPersonView(bool _bLerpPosition)
	{
		SetFirstPersonView(!bFirstPersonView, _bLerpPosition);
	}

	public void SetFirstPersonView(bool _bFirstPersonView, bool _bLerpPosition)
	{
		bFirstPersonView = _bFirstPersonView;
		SetCVar(".IsFPV", _bFirstPersonView ? 1 : 0);
		FireEvent(MinEventTypes.onSelfChangedView);
		if (bFirstPersonView)
		{
			switchModelView(EnumEntityModelView.FirstPerson);
		}
		else
		{
			switchModelView(EnumEntityModelView.ThirdPerson);
		}
		updateCameraPosition(_bLerpPosition);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void switchModelView(EnumEntityModelView modelView)
	{
		base.switchModelView(modelView);
		SetModelLayer(24, force: true);
		if (vp_FPController != null)
		{
			vp_FPController.Player.IsFirstPerson.Set(modelView == EnumEntityModelView.FirstPerson);
		}
	}

	public override void BeforePlayerRespawn(RespawnType _type)
	{
		base.BeforePlayerRespawn(_type);
		switch (_type)
		{
		case RespawnType.Died:
		{
			for (int i = 0; i < overlayDirectionTime.Length; i++)
			{
				overlayDirectionTime[i] = 0f;
			}
			break;
		}
		case RespawnType.NewGame:
		case RespawnType.LoadedGame:
		case RespawnType.Teleport:
			break;
		}
	}

	public override void AfterPlayerRespawn(RespawnType _type)
	{
		base.AfterPlayerRespawn(_type);
		if (bSwitchCameraBackAfterRespawn)
		{
			bSwitchCameraBackAfterRespawn = false;
			m_vp_FPCamera.enabled = true;
			if (!bFirstPersonView)
			{
				SwitchFirstPersonView(_bLerpPosition: false);
			}
		}
		if (!GameManager.Instance.IsEditMode() && (_type == RespawnType.NewGame || _type == RespawnType.EnterMultiplayer))
		{
			emodel.avatarController.PlayPlayerFPRevive();
		}
		if (world.IsEditor() || GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			if (GameManager.Instance.IsEditMode() && !PrefabEditModeManager.Instance.IsActive())
			{
				SkyManager.SetFogDebug(0f);
				if (GameManager.Instance.World?.BiomeAtmosphereEffects != null)
				{
					GameManager.Instance.World.BiomeAtmosphereEffects.ForceDefault = true;
				}
			}
			if (Buffs != null && !Buffs.HasBuff("god"))
			{
				Buffs.AddBuff("god");
			}
		}
		switch (_type)
		{
		case RespawnType.NewGame:
		case RespawnType.EnterMultiplayer:
			SetAlive();
			Score = 0;
			if (world.IsEditor())
			{
				inventory.SetItem(1, new ItemValue(1), 64);
				inventory.SetHoldingItemIdx(0);
			}
			else
			{
				SetupStartingItems();
			}
			Buffs.UnPauseAll();
			FireEvent(MinEventTypes.onSelfFirstSpawn);
			FireEvent(MinEventTypes.onSelfEnteredGame);
			break;
		case RespawnType.LoadedGame:
		case RespawnType.JoinMultiplayer:
			FireEvent(MinEventTypes.onSelfEnteredGame);
			HandleMapObjects(usePersistantBackpackPositions: true);
			break;
		case RespawnType.Died:
		{
			SetAlive();
			Health = GetMaxHealth();
			Stamina = GetMaxStamina();
			Water = GetMaxWater();
			base.Stats.Stamina.MaxModifier = 0f;
			bool crouchingLocked = (Crouching = false);
			CrouchingLocked = crouchingLocked;
			FireEvent(MinEventTypes.onSelfRespawn);
			Manager.StopLoopInsidePlayerHead("player_death_stinger_lp", entityId);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Manager.Instance.StopDistantLoopingPositionalSounds(base.transform.position);
			}
			break;
		}
		case RespawnType.Teleport:
			FireEvent(MinEventTypes.onSelfTeleported);
			break;
		}
	}

	public void RequestUnstuck()
	{
		if (!unstuckRequested)
		{
			ThreadManager.StartCoroutine(RequestUnstuckCo());
		}
		[PublicizedFrom(EAccessModifier.Private)]
		IEnumerator RequestUnstuckCo()
		{
			unstuckRequested = true;
			yield return moveController.UnstuckPlayerCo();
			unstuckRequested = false;
		}
	}

	public void SetupStartingItems()
	{
		for (int i = 0; i < itemsOnEnterGame.Count; i++)
		{
			ItemStack itemStack = itemsOnEnterGame[i];
			itemStack.itemValue.Meta = ItemClass.GetForId(itemStack.itemValue.type).GetInitialMetadata(itemStack.itemValue);
			inventory.SetItem(i + 1, itemStack);
		}
		inventory.SetHoldingItemIdx(0);
	}

	public override Vector3 GetCameraLook(float _t)
	{
		if (!bFirstPersonView)
		{
			return cameraTransform.forward.normalized;
		}
		return base.GetCameraLook(_t);
	}

	public override Ray GetLookRay()
	{
		Ray result = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		result.origin += Origin.position;
		if (bFirstPersonView)
		{
			result.direction += playerCamera.transform.up * 0.0001f;
			return result;
		}
		result.origin += result.direction * (0.94f + movementInput.cameraDistance);
		result.direction += playerCamera.transform.up * 0.0001f;
		return result;
	}

	public override Vector3 GetLookVector()
	{
		if (playerCamera.enabled)
		{
			return cameraTransform.forward;
		}
		return base.GetLookVector();
	}

	public override Vector3 GetLookVector(Vector3 _altLookVector)
	{
		if (!playerCamera.enabled)
		{
			return _altLookVector;
		}
		return GetLookVector();
	}

	public static void CheckPos()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startDeathCamera()
	{
		if (bFirstPersonView)
		{
			SwitchFirstPersonView(_bLerpPosition: true);
		}
		bSwitchCameraBackAfterRespawn = true;
		StartSelfCamera();
		selfCameraSeekPos = selfCameraPos - cameraTransform.forward * 2.8f;
		selfCameraSeekPos.y += 2.2f;
		ScreenEffectManager.SetScreenEffect("Dying", 0.5f, 0.5f);
		ScreenEffectManager.SetScreenEffect("Dead", 1f, 0.5f);
		ScreenEffectManager.SetScreenEffect("FadeToBlack", 1f, (float)GetTimeStayAfterDeath() * 0.05f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartSelfCamera()
	{
		selfCameraPos = cameraTransform.position + Origin.position;
		m_vp_FPCamera.enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelfCameraFrameUpdate()
	{
		Vector3 chestPosition = emodel.GetChestPosition();
		chestPosition.y += 0.2f;
		if (selfCameraSeekPos.y < chestPosition.y + 0.1f)
		{
			selfCameraSeekPos.y += 0.05f;
		}
		selfCameraPos = Vector3.MoveTowards(selfCameraPos, selfCameraSeekPos, Time.deltaTime * 2.5f);
		Vector3 direction = selfCameraPos - chestPosition;
		float magnitude = direction.magnitude;
		chestPosition -= Origin.position;
		bool flag = false;
		float v = magnitude;
		float num = magnitude - 0.28f;
		if (num > 0f && Physics.SphereCast(chestPosition, 0.28f, direction, out var hitInfo, num, 65536))
		{
			v = Utils.FastMin(hitInfo.distance, v);
			flag = true;
		}
		if (!flag && Physics.Raycast(chestPosition, direction, out hitInfo, magnitude + 0.28f, 65536))
		{
			v = hitInfo.distance - 0.28f;
			flag = true;
		}
		if (flag)
		{
			selfCameraPos = chestPosition + direction.normalized * Utils.FastMax(0.01f, v) + Origin.position;
			selfCameraSeekPos = selfCameraPos;
		}
		Vector3 vector = selfCameraPos - Origin.position;
		cameraTransform.position = vector;
		Quaternion b = Quaternion.LookRotation(chestPosition - vector);
		cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, b, 0.1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearScreenEffects()
	{
		ScreenEffectManager.DisableScreenEffect("Dying");
		ScreenEffectManager.DisableScreenEffect("Dead");
		ScreenEffectManager.DisableScreenEffect("FadeToBlack");
	}

	public void CancelInventoryActions()
	{
		if (inventory.holdingItem.Actions == null || inventory.holdingItem.Actions.Length == 0 || inventory.holdingItemData.actionData == null || inventory.holdingItemData.actionData.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < inventory.holdingItem.Actions.Length; i++)
		{
			if (inventory.holdingItem.Actions[i] != null && inventory.holdingItemData.actionData[i] != null)
			{
				if (inventory.holdingItem.Actions[i].IsActionRunning(inventory.holdingItemData.actionData[i]))
				{
					inventory.holdingItem.Actions[i].CancelAction(inventory.holdingItemData.actionData[i]);
				}
				inventory.holdingItem.Actions[i].CancelReload(inventory.holdingItemData.actionData[i]);
			}
		}
	}

	public bool IsReloading()
	{
		if (inventory.holdingItemData.actionData != null)
		{
			foreach (ItemActionData actionDatum in inventory.holdingItemData.actionData)
			{
				if (actionDatum is ItemActionRanged.ItemActionDataRanged { isReloading: not false })
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void OnEntityDeath()
	{
		GameManager.Instance.TriggerSendOfLocalPlayerDataFile(0f);
		CancelInventoryActions();
		inventory.ReleaseAll(playerInput);
		inventory.SetActiveItemIndexOff();
		Manager.BroadcastStop(entityId, "Player" + (IsMale ? "Male" : "Female") + "RunLoop");
		sprintLoopSoundPlayId = -1;
		windowManager.CloseAllOpenWindows();
		windowManager.Close("windowpaging");
		GameManager.Instance.ClearTooltips(nguiWindowManager);
		windowManager.Open("death", _bModal: false, _bIsNotEscClosable: false, _bCloseAllOpenWindows: false);
		AimingGun = false;
		BloodMoonParticipation = false;
		base.OnEntityDeath();
		startDeathCamera();
	}

	public override void OnDeathUpdate()
	{
		base.OnDeathUpdate();
		if (Spawned && GetDeathTime() >= GetTimeStayAfterDeath())
		{
			windowManager.Close("death");
			Respawn(RespawnType.Died);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FrameUpdateCamera()
	{
		UnderwaterCameraFrameUpdate();
		if (!bFirstPersonView)
		{
			vp_FPCamera obj = m_vp_FPCamera;
			float z = -0.94f - movementInput.cameraDistance;
			obj.Position3rdPersonOffset = new Vector3(0.51f, 0.03f, z);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateCameraPosition(bool _bLerpPosition)
	{
		if (!playerCamera.enabled || !IsCameraAttachedToPlayerOrScope())
		{
			return;
		}
		if (AimingGun)
		{
			inventory.holdingItem.GetIronSights(inventory.holdingItemData, out lerpCameraEndFOV);
			if (lerpCameraEndFOV != 0f)
			{
				bLerpCameraFlag = _bLerpPosition;
				lerpCameraLerpValue = 0f;
				lerpCameraStartFOV = playerCamera.fieldOfView;
			}
			return;
		}
		if (bFirstPersonView && bSwitchTo3rdPersonAfterAiming)
		{
			bSwitchTo3rdPersonAfterAiming = false;
			SwitchFirstPersonView(_bLerpPosition: true);
			return;
		}
		float fieldOfView = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
		if (bFirstPersonView)
		{
			bLerpCameraFlag = _bLerpPosition;
			if (bLerpCameraFlag)
			{
				lerpCameraLerpValue = 0f;
				lerpCameraStartFOV = playerCamera.fieldOfView;
				lerpCameraEndFOV = fieldOfView;
			}
			else
			{
				playerCamera.fieldOfView = fieldOfView;
			}
		}
		else
		{
			playerCamera.fieldOfView = fieldOfView;
		}
	}

	public override void PhysicsResume(Vector3 pos, float rotY)
	{
		Transform obj = cameraTransform;
		Vector3 vector = obj.position;
		Quaternion quaternion = obj.rotation;
		base.PhysicsResume(pos, rotY);
		obj.SetPositionAndRotation(vector, quaternion);
	}

	public override void OnRagdoll(bool isActive)
	{
		base.OnRagdoll(isActive);
		if (isActive)
		{
			if (bFirstPersonView)
			{
				SwitchFirstPersonView(_bLerpPosition: true);
			}
			emodel.InitRigidBodies();
			StartSelfCamera();
			Vector3 forwardVector = GetForwardVector();
			selfCameraSeekPos = emodel.GetChestPosition();
			selfCameraSeekPos.x -= forwardVector.x * 2.2f;
			selfCameraSeekPos.y += 1.2f;
			selfCameraSeekPos.z -= forwardVector.z * 2.2f;
			Transform obj = cameraTransform;
			obj.position = Vector3.MoveTowards(obj.position, selfCameraSeekPos, 1.2f);
		}
		else
		{
			SetRotation(rotation);
			m_vp_FPCamera.enabled = true;
			SetFirstPersonView(_bFirstPersonView: true, _bLerpPosition: false);
		}
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f)
	{
		_strength = base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
		if (_strength > 0)
		{
			GameManager.Instance.StartCoroutine(shakeCamera(_damageSource.getDirection(), 0.5f, _strength * 4));
		}
		return _strength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator shakeCamera(Vector3 _direction, float time, float strength, float speed = 1f)
	{
		m_vp_FPCamera.ShakeSpeed2 = speed;
		m_vp_FPCamera.ShakeAmplitude2 = new Vector3(0f - _direction.x, _direction.y, 0f) * strength;
		yield return new WaitForSeconds(time);
		m_vp_FPCamera.ShakeSpeed2 = 0f;
		m_vp_FPCamera.ShakeAmplitude2 = Vector3.zero;
	}

	public override void Kill(DamageResponse _dmResponse)
	{
		if (!IsDead())
		{
			base.Kill(_dmResponse);
			GameManager.Instance.StartCoroutine(shakeCamera(Vector3.one, 0.5f, 20f));
			if (m_vp_FPController != null)
			{
				m_vp_FPController.Player.Dead.Start();
			}
		}
	}

	public override void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		base.SetPosition(_pos, _bUpdatePhysics);
		if (vp_FPController != null)
		{
			if (!emodel.IsRagdollActive)
			{
				vp_FPController.SetPosition(_pos - Origin.position);
			}
			vp_FPController.Stop();
			if ((bool)AttachedToEntity)
			{
				vp_FPController.Transform.localPosition = Vector3.zero;
			}
		}
		Manager.CameraChanged();
		Origin.Instance.UpdateLocalPlayer(this);
	}

	public override void Respawn(RespawnType _reason)
	{
		base.Respawn(_reason);
		moveController.Respawn(_reason);
		Shader.SetGlobalFloat("_UnderWater", 0f);
		SetControllable(_b: false);
		if (vp_FPController != null && !AttachedToEntity)
		{
			vp_FPController.ResetState();
			vp_FPController.SetPosition(GetPosition() - Origin.position);
			vp_FPController.Stop();
			vp_FPCamera.Locked3rdPerson = false;
		}
		if (_reason == RespawnType.Teleport)
		{
			windowManager.CloseAllOpenWindows("map");
		}
		isFallDeath = false;
	}

	public void TeleportToPosition(Vector3 _pos, bool _onlyIfNotFlying = false, Vector3? _viewDirection = null)
	{
		if (!_onlyIfNotFlying || !IsFlyMode.Value)
		{
			Teleport(_pos, _viewDirection.HasValue ? _viewDirection.Value.y : float.MinValue);
			if (_pos.y >= 0f)
			{
				ThreadManager.StartCoroutine(setVerticalPosition(_pos, _viewDirection));
				return;
			}
			Log.Out("Teleported to {0}", _pos.ToCultureInvariantString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setVerticalPosition(Vector3 _pos, Vector3? _viewDirection)
	{
		while (!Spawned)
		{
			yield return null;
		}
		if (AttachedToEntity != null)
		{
			AttachedToEntity.SetPosition(_pos);
		}
		else
		{
			SetPosition(_pos);
		}
		if (_viewDirection.HasValue)
		{
			SetRotation(_viewDirection.Value);
		}
		Log.Out("Teleported to {0}", _pos.ToCultureInvariantString());
	}

	public void SetControllable(bool _b)
	{
		if (!_b || !bIntroAnimActive)
		{
			base.transform.GetComponent<PlayerMoveController>().SetControllableOverride(_b);
		}
	}

	public void NotifySneakDamage(float multiplier)
	{
		sneakDamageText = string.Format(Localization.Get("sneakDamageBonus"), multiplier.ToCultureInvariantString("f1")).ToUpper();
		sneakDamageBlendTimer.FadeIn();
	}

	public void NotifyDamageMultiplier(float multiplier)
	{
		sneakDamageText = string.Format(Localization.Get("stunnedDamageBonus"), multiplier.ToCultureInvariantString("f1")).ToUpper();
		sneakDamageBlendTimer.FadeIn();
	}

	public override void EnableCamera(bool _b)
	{
		playerCamera.enabled = _b;
	}

	public override bool IsAttackValid()
	{
		return true;
	}

	public override bool IsAimingGunPossible()
	{
		if (inventory.holdingItem.Actions[0] == null || inventory.holdingItem.Actions[0].IsAimingGunPossible(inventory.holdingItemData.actionData[0]))
		{
			if (inventory.holdingItem.Actions[1] != null)
			{
				return inventory.holdingItem.Actions[1].IsAimingGunPossible(inventory.holdingItemData.actionData[1]);
			}
			return true;
		}
		return false;
	}

	public override bool IsDrawMapIcon()
	{
		return IsSpawned();
	}

	public override bool IsMapIconBlinking()
	{
		return true;
	}

	public override bool CanMapIconBeSelected()
	{
		return false;
	}

	public override Color GetMapIconColor()
	{
		return Color.white;
	}

	public override int GetLayerForMapIcon()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool BreakLeg(float chance)
	{
		if (rand.RandomFloat <= chance && Buffs.AddBuff("injuryBrokenLeg") == EntityBuffs.BuffStatus.Added)
		{
			PlayOneShot("breakleg");
			PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.LegBroken, 1);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool FractureLeg(float chance)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SprainLeg(float chance)
	{
		if (rand.RandomFloat <= chance)
		{
			return Buffs.AddBuff("injurySprainedLeg") == EntityBuffs.BuffStatus.Added;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasBrokenLeg()
	{
		return Buffs.HasBuff("injuryBrokenLeg");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasFracturedLeg()
	{
		return Buffs.HasBuff("injuryBrokenLeg");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasSprainedLeg()
	{
		return Buffs.HasBuff("injurySprainedLeg");
	}

	public override float GetSpeedModifier()
	{
		if (!IsGodMode.Value && cameraTransform.parent != null)
		{
			return base.GetSpeedModifier();
		}
		return base.GetSpeedModifier() * GodModeSpeedModifier;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FallImpact(float speed)
	{
		if (IsGodMode.Value || AttachedToEntity != null || speed <= 0f)
		{
			return;
		}
		Vector3i pos = World.worldToBlockPos(vp_FPController.Transform.position + Origin.position);
		BlockValue block = world.GetBlock(pos);
		if (block.isair || block.Block.IsElevator(block.rotation))
		{
			pos.y--;
			block = world.GetBlock(pos);
		}
		float num = 1f;
		if (!block.isair)
		{
			num = block.Block.FallDamage;
			if (num <= 0f)
			{
				return;
			}
		}
		if (speed > 1f)
		{
			speed = 1f;
		}
		speed *= 1f;
		speed *= num;
		speed = EffectManager.GetValue(PassiveEffects.FallDamageReduction, inventory.holdingItemItemValue, speed, this);
		fallHealth = Health;
		SetCVar("_fallSpeed", speed);
		FireEvent(MinEventTypes.onSelfFallImpact);
	}

	public override void BuffAdded(BuffValue _buff)
	{
		if (_buff.BuffClass.NameTag.Test_Bit(EntityAlive.FallingBuffTagBit) && fallHealth > 0 && Health <= 0)
		{
			isFallDeath = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResendPlayerInventory()
	{
		if (world.aiDirector != null)
		{
			world.aiDirector.UpdatePlayerInventory(this);
			return;
		}
		AIDirectorPlayerInventory aIDirectorPlayerInventory = AIDirectorPlayerInventory.FromEntity(this);
		if (!aIDirectorPlayerInventory.Equals(xmitInventory))
		{
			xmitInventory = aIDirectorPlayerInventory;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerInventoryForAI>().Setup(this, aIDirectorPlayerInventory));
		}
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[base.entityClass];
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropDropInventoryBlock))
		{
			dropInventoryBlock = entityClass.Properties.Values[EntityClass.PropDropInventoryBlock];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void dropItemOnDeath()
	{
		dropBackpack(_isDying: true);
		inventory.SetFlashlight(on: false);
	}

	public void dropItemOnQuit()
	{
		dropBackpack(_isDying: false);
	}

	public override void SetDroppedBackpackPositions(List<Vector3i> _positions)
	{
		if (!ThreadManager.IsMainThread())
		{
			lock (backpackPositionsFromThread)
			{
				backpackPositionsFromThread.Clear();
				if (_positions != null)
				{
					backpackPositionsFromThread.AddRange(_positions);
				}
				return;
			}
		}
		base.SetDroppedBackpackPositions(_positions);
		if (backpackNavObjects != null)
		{
			foreach (NavObject backpackNavObject in backpackNavObjects)
			{
				NavObjectManager.Instance.UnRegisterNavObject(backpackNavObject);
			}
			backpackNavObjects.Clear();
		}
		else
		{
			backpackNavObjects = new List<NavObject>();
		}
		if (_positions == null)
		{
			return;
		}
		for (int i = 0; i < _positions.Count; i++)
		{
			Vector3i vector3i = _positions[i];
			if (!vector3i.Equals(Vector3i.zero))
			{
				backpackNavObjects.Add(NavObjectManager.Instance.RegisterNavObject("backpack_distant", vector3i.ToVector3() + new Vector3(0.5f, 0f, 0.5f)));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void dropBackpack(bool _isDying)
	{
		DropOption dropOption = (DropOption)(_isDying ? GameStats.GetInt(EnumGameStats.DropOnDeath) : GameStats.GetInt(EnumGameStats.DropOnQuit));
		if (dropOption == DropOption.None || string.IsNullOrEmpty(dropInventoryBlock))
		{
			return;
		}
		if (playerUI.xui != null)
		{
			playerUI.xui.CancelAllCrafting();
		}
		bool flag = false;
		bool flag2 = false;
		ItemStack[] slots = bag.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (!slots[i].itemValue.type.Equals(ItemValue.None.type))
			{
				flag = true;
				break;
			}
		}
		for (int j = 0; j < inventory.GetItemCount(); j++)
		{
			if (!inventory.GetItem(j).itemValue.type.Equals(ItemValue.None.type))
			{
				flag2 = true;
				break;
			}
		}
		if (!flag && !flag2 && !equipment.HasAnyItems())
		{
			return;
		}
		if (_isDying && dropOption == DropOption.DeleteAll)
		{
			ItemStack[] slots2 = bag.GetSlots();
			for (int k = 0; k < slots2.Length; k++)
			{
				slots2[k] = ItemStack.Empty.Clone();
			}
			bag.SetSlots(slots2);
			for (int l = 0; l < inventory.GetItemCount(); l++)
			{
				inventory.SetItem(l, ItemStack.Empty.Clone());
			}
			for (int m = 0; m < 5; m++)
			{
				equipment.SetSlotItem(m, null);
			}
			return;
		}
		EntityBackpack entityBackpack = EntityFactory.CreateEntity("Backpack".GetHashCode(), position + base.transform.up * 2f) as EntityBackpack;
		TileEntityLootContainer tileEntityLootContainer = new TileEntityLootContainer((Chunk)null);
		string text = (tileEntityLootContainer.lootListName = entityBackpack.GetLootList());
		tileEntityLootContainer.SetUserAccessing(_bUserAccessing: true);
		tileEntityLootContainer.SetEmpty();
		tileEntityLootContainer.SetContainerSize(LootContainer.GetLootContainer(text).size);
		PreferenceTracker preferenceTracker = new PreferenceTracker(entityId);
		Predicate<ItemStack> predicate = [PublicizedFrom(EAccessModifier.Internal)] (ItemStack s) => s != null && !s.IsEmpty() && !s.itemValue.ItemClassOrMissing.KeepOnDeath();
		Predicate<ItemValue> predicate2 = [PublicizedFrom(EAccessModifier.Internal)] (ItemValue v) => v != null && !v.IsEmpty() && !v.ItemClassOrMissing.KeepOnDeath();
		bool flag3 = GameStats.GetInt(EnumGameStats.DeathPenalty) == 3;
		if (dropOption == DropOption.All || dropOption == DropOption.Backpack)
		{
			ItemStack[] slots3 = bag.GetSlots();
			preferenceTracker.SetBag(slots3, predicate);
			for (int num = 0; num < slots3.Length; num++)
			{
				if (predicate(slots3[num]))
				{
					tileEntityLootContainer.AddItem(slots3[num]);
					slots3[num] = ItemStack.Empty.Clone();
				}
				else if (flag3)
				{
					slots3[num] = ItemStack.Empty.Clone();
				}
			}
			bag.SetSlots(slots3);
		}
		if (dropOption == DropOption.All)
		{
			ItemValue[] array = new ItemValue[5];
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				ItemValue slotItem = equipment.GetSlotItem(num2);
				if (predicate2(slotItem))
				{
					array[num2] = slotItem;
					ItemStack item = new ItemStack(slotItem, 1);
					tileEntityLootContainer.AddItem(item);
					equipment.SetSlotItem(num2, null);
				}
			}
			preferenceTracker.SetEquipment(array, predicate2);
		}
		if (dropOption == DropOption.All || dropOption == DropOption.Toolbelt)
		{
			ItemStack[] array2 = new ItemStack[inventory.GetItemCount()];
			for (int num3 = 0; num3 < array2.Length; num3++)
			{
				if (num3 != inventory.DUMMY_SLOT_IDX)
				{
					ItemStack itemStack = (array2[num3] = inventory.GetItem(num3));
					if (predicate(itemStack))
					{
						tileEntityLootContainer.AddItem(itemStack);
						inventory.SetItem(num3, new ItemStack(ItemValue.None.Clone(), 0));
					}
				}
			}
			preferenceTracker.SetToolbelt(array2, predicate);
		}
		if (preferenceTracker.AnyPreferences)
		{
			tileEntityLootContainer.preferences = preferenceTracker;
		}
		tileEntityLootContainer.bPlayerBackpack = true;
		tileEntityLootContainer.SetUserAccessing(_bUserAccessing: false);
		tileEntityLootContainer.SetModified();
		entityBackpack.RefPlayerId = entityId;
		EntityCreationData entityCreationData = new EntityCreationData(entityBackpack);
		entityCreationData.entityName = string.Format(Localization.Get("playersBackpack"), EntityName);
		entityCreationData.id = -1;
		entityCreationData.lootContainer = tileEntityLootContainer;
		GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
		entityBackpack.OnEntityUnload();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SetDroppedBackpackPositions(GameManager.Instance.persistentLocalPlayer.GetDroppedBackpackPositions());
		}
	}

	public override int AttachToEntity(Entity _other, int slot = -1)
	{
		if (_other.IsAttached(this))
		{
			return -1;
		}
		vp_FPController vp_FPController2 = vp_FPController;
		vp_FPController2.enabled = true;
		Transform transform = m_vp_FPCamera.Transform;
		Vector3 vector = transform.position;
		Quaternion quaternion = transform.rotation;
		SetFirstPersonView(_bFirstPersonView: false, _bLerpPosition: false);
		slot = base.AttachToEntity(_other, slot);
		if (slot >= 0)
		{
			transform.position = vector;
			transform.rotation = quaternion;
			vp_FPController2.Stop();
			vp_FPController2.Player.Driving.Start();
		}
		else
		{
			SetFirstPersonView(_bFirstPersonView: true, _bLerpPosition: false);
		}
		EntityVehicle entityVehicle = _other as EntityVehicle;
		if ((bool)entityVehicle && entityVehicle.LocalPlayerIsOwner())
		{
			Waypoints.UpdateEntityVehicleWayPoint(entityVehicle);
			Waypoints.SetWaypointHiddenOnMap(entityId, _hidden: true);
		}
		return slot;
	}

	public override void Detach()
	{
		SetFirstPersonView(_bFirstPersonView: true, _bLerpPosition: false);
		Vector3 force = Vector3.zero;
		EntityVehicle entityVehicle = AttachedToEntity as EntityVehicle;
		if ((bool)entityVehicle)
		{
			force = entityVehicle.GetExitVelocity() * Time.fixedDeltaTime;
			if (entityVehicle.LocalPlayerIsOwner())
			{
				Waypoints.UpdateEntityVehicleWayPoint(entityVehicle);
				Waypoints.SetWaypointHiddenOnMap(entityVehicle.entityId, _hidden: false);
			}
		}
		base.Detach();
		vp_FPController obj = vp_FPController;
		obj.Player.Driving.Stop();
		obj.Stop();
		obj.m_MaxHeightInitialFallSpeed = Utils.FastMin(force.y, 0f);
		obj.AddForce(force);
	}

	public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		base.ProcessDamageResponseLocal(_dmResponse);
		healthLostThisRound = _dmResponse.Strength > 0;
	}

	public override bool CanUpdateEntity()
	{
		if (!IsFlyMode.Value)
		{
			return base.CanUpdateEntity();
		}
		return true;
	}

	public override void OnHUD()
	{
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		NGuiWdwInGameHUD inGameHUD = nguiWindowManager.InGameHUD;
		if (!GameManager.Instance.gameStateManager.IsGameStarted() || GameStats.GetInt(EnumGameStats.GameState) != 1 || !Spawned)
		{
			return;
		}
		bool flag = windowManager.IsModalWindowOpen() || LocalPlayerUI.primaryUI.windowManager.IsModalWindowOpen();
		guiDrawOverlayTextures(inGameHUD, flag);
		if (windowManager.IsFullHUDDisabled())
		{
			return;
		}
		if (inventory != null && !flag)
		{
			inventory.holdingItem.OnHUD(inventory.holdingItemData, Screen.width - 10, Screen.height - 10);
		}
		if (!LocalPlayerUI.primaryUI.windowManager.IsModalWindowOpen() && !windowManager.IsWindowOpen("toolbelt") && !windowManager.IsWindowOpen(XUiC_InGameMenuWindow.ID) && !windowManager.IsWindowOpen("dialog") && !windowManager.IsWindowOpen("tipWindow") && !windowManager.IsWindowOpen("questOffer"))
		{
			windowManager.Open("toolbelt", _bModal: false);
		}
		windowManager.OpenIfNotOpen("CalloutGroup", _bModal: false);
		if (!windowManager.IsModalWindowOpen() && !windowManager.IsWindowOpen(XUiC_CompassWindow.ID) && !LocalPlayerUI.primaryUI.windowManager.IsModalWindowOpen())
		{
			windowManager.Open(XUiC_CompassWindow.ID, _bModal: false);
		}
		windowManager.OpenIfNotOpen("toolTip", _bModal: false);
		windowManager.OpenIfNotOpen(XUiC_ChatOutput.ID, _bModal: false);
		if (Event.current.type == EventType.Repaint)
		{
			if (sneakDamageBlendTimer.Value > 0f)
			{
				nguiWindowManager.SetLabel(EnumNGUIWindow.CriticalHitText, sneakDamageText, new Color(1f, 1f, 1f, sneakDamageBlendTimer.Value));
			}
			else if (nguiWindowManager.IsShowing(EnumNGUIWindow.CriticalHitText))
			{
				nguiWindowManager.Show(EnumNGUIWindow.CriticalHitText, _bEnable: false);
			}
		}
		guiDrawCrosshair(inGameHUD, flag);
	}

	public void ForceBloodSplatter()
	{
		healthLostThisRound = true;
		lastHitDirection = Utils.EnumHitDirection.Front;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void guiDrawOverlayTextures(NGuiWdwInGameHUD _guiInGame, bool bModalWindowOpen)
	{
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		inventory.holdingItem.OnScreenOverlay(inventory.holdingItemData);
		Vector3 vector = finalCamera.ViewportToScreenPoint(new Vector3(0f, 0f, 0f));
		Vector3 vector2 = finalCamera.ViewportToScreenPoint(new Vector3(1f, 1f, 0f));
		if (healthLostThisRound && !IsDead() && lastHitDirection != Utils.EnumHitDirection.None)
		{
			healthLostThisRound = false;
			int num = (int)lastHitDirection;
			overlayDirectionTime[num] = 6f;
			overlayAlternating[num]++;
			int num2 = num * 2 + (overlayAlternating[num] & 1);
			overlayDirectionTime[num2] = 6f;
			overlayBloodDropsPositions[num2 * 3] = new Vector2(rand.RandomRange(vector.x, vector2.x), rand.RandomRange(vector.y, vector2.y));
			overlayBloodDropsPositions[num2 * 3 + 1] = new Vector2(rand.RandomRange(vector.x, vector2.x), rand.RandomRange(vector.y, vector2.y));
			overlayBloodDropsPositions[num2 * 3 + 2] = new Vector2(rand.RandomRange(vector.x, vector2.x), rand.RandomRange(vector.y, vector2.y));
			lastHitDirection = Utils.EnumHitDirection.None;
		}
		for (int i = 0; i < 8; i++)
		{
			if (!(overlayDirectionTime[i] > 0f))
			{
				continue;
			}
			float num3 = Mathf.Pow(1f - overlayDirectionTime[i] / 6f, 0.28f);
			overlayMaterial.SetColor("_Color", new Color(num3, num3, num3));
			if (windowManager.IsHUDEnabled())
			{
				Vector3 vector3 = (vector2 + vector) * 0.5f;
				int pixelWidth = finalCamera.pixelWidth;
				int pixelHeight = finalCamera.pixelHeight;
				Texture2D texture2D = _guiInGame.overlayDamageTextures[i];
				float num4 = (float)pixelHeight / 512f;
				float num5 = (float)texture2D.width * num4;
				float num6 = (float)texture2D.height * num4;
				Rect screenRect = new Rect(vector3.x - num5 * 0.5f, 0f, num5, num6);
				int num7 = i >> 1;
				if (num7 == 1)
				{
					screenRect.y = (float)pixelHeight - num6;
				}
				else if (num7 >= 2)
				{
					screenRect.x = (float)pixelWidth - num5;
					screenRect.y = vector3.y - num6 * 0.5f;
					if (num7 == 3)
					{
						screenRect.x = 0f;
					}
				}
				Graphics.DrawTexture(screenRect, texture2D, overlayMaterial);
				int num8 = i * 3;
				Graphics.DrawTexture(new Rect(overlayBloodDropsPositions[num8].x, (float)pixelHeight - overlayBloodDropsPositions[num8].y, _guiInGame.overlayDamageBloodDrops[0].width, _guiInGame.overlayDamageBloodDrops[0].height), _guiInGame.overlayDamageBloodDrops[0], overlayMaterial);
				Graphics.DrawTexture(new Rect(overlayBloodDropsPositions[num8 + 1].x, (float)pixelHeight - overlayBloodDropsPositions[num8 + 1].y, _guiInGame.overlayDamageBloodDrops[1].width, _guiInGame.overlayDamageBloodDrops[1].height), _guiInGame.overlayDamageBloodDrops[1], overlayMaterial);
				Graphics.DrawTexture(new Rect(overlayBloodDropsPositions[num8 + 2].x, (float)pixelHeight - overlayBloodDropsPositions[num8 + 2].y, _guiInGame.overlayDamageBloodDrops[2].width, _guiInGame.overlayDamageBloodDrops[2].height), _guiInGame.overlayDamageBloodDrops[2], overlayMaterial);
			}
			if (Health > 0)
			{
				overlayDirectionTime[i] -= Time.deltaTime;
			}
			else
			{
				overlayDirectionTime[i] = 0f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CrosshairAlpha(NGuiWdwInGameHUD _guiInGame)
	{
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void guiDrawCrosshair(NGuiWdwInGameHUD _guiInGame, bool bModalWindowOpen)
	{
		if (!_guiInGame.showCrosshair || Event.current.type != EventType.Repaint || IsDead() || emodel.IsRagdollActive || AttachedToEntity != null)
		{
			return;
		}
		ItemClass.EnumCrosshairType crosshairType = inventory.holdingItem.GetCrosshairType(inventory.holdingItemData);
		if (bModalWindowOpen || inventory == null)
		{
			return;
		}
		Vector2 crosshairPosition2D = GetCrosshairPosition2D();
		crosshairPosition2D.y = (float)Screen.height - crosshairPosition2D.y;
		float num = (float)Screen.height * 0.059f;
		switch (crosshairType)
		{
		case ItemClass.EnumCrosshairType.Crosshair:
			if (AimingGun && !ItemAction.ShowDistanceDebugInfo)
			{
				break;
			}
			goto case ItemClass.EnumCrosshairType.CrosshairOnAiming;
		case ItemClass.EnumCrosshairType.CrosshairOnAiming:
		{
			GetCrosshairOpenArea();
			float value = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, inventory.holdingItemData.itemValue, 90f, this);
			value *= 0.5f;
			value *= (inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged).lastAccuracy;
			value *= (float)Mathf.RoundToInt((float)Screen.width / cameraTransform.GetComponent<Camera>().fieldOfView);
			float value2 = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, inventory.holdingItemData.itemValue, 90f, this);
			value2 *= 0.5f;
			value2 *= (inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged).lastAccuracy;
			value2 *= (float)Mathf.RoundToInt((float)Screen.width / cameraTransform.GetComponent<Camera>().fieldOfView);
			int num2 = (int)crosshairPosition2D.x;
			int num3 = (int)crosshairPosition2D.y;
			int num4 = 18;
			Color black = Color.black;
			Color white = Color.white;
			black.a = CrosshairAlpha(_guiInGame) * weaponCrossHairAlpha;
			white.a = CrosshairAlpha(_guiInGame) * weaponCrossHairAlpha;
			GUIUtils.DrawLine(new Vector2((float)num2 - value, num3 + 1), new Vector2((float)num2 - (value + (float)num4), num3 + 1), black);
			GUIUtils.DrawLine(new Vector2((float)num2 + value, num3 + 1), new Vector2((float)num2 + value + (float)num4, num3 + 1), black);
			GUIUtils.DrawLine(new Vector2(num2 + 1, (float)num3 - value2), new Vector2(num2 + 1, (float)num3 - (value2 + (float)num4)), black);
			GUIUtils.DrawLine(new Vector2(num2 + 1, (float)num3 + value2), new Vector2(num2 + 1, (float)num3 + value2 + (float)num4), black);
			GUIUtils.DrawLine(new Vector2((float)num2 + value, num3), new Vector2((float)num2 + value + (float)num4, num3), white);
			GUIUtils.DrawLine(new Vector2(num2, (float)num3 - value2), new Vector2(num2, (float)num3 - (value2 + (float)num4)), white);
			GUIUtils.DrawLine(new Vector2((float)num2 - value, num3), new Vector2((float)num2 - (value + (float)num4), num3), white);
			GUIUtils.DrawLine(new Vector2(num2, (float)num3 + value2), new Vector2(num2, (float)num3 + value2 + (float)num4), white);
			GUIUtils.DrawLine(new Vector2((float)num2 - value, num3 - 1), new Vector2((float)num2 - (value + (float)num4), num3 - 1), black);
			GUIUtils.DrawLine(new Vector2((float)num2 + value, num3 - 1), new Vector2((float)num2 + value + (float)num4, num3 - 1), black);
			GUIUtils.DrawLine(new Vector2(num2 - 1, (float)num3 - value2), new Vector2(num2 - 1, (float)num3 - (value2 + (float)num4)), black);
			GUIUtils.DrawLine(new Vector2(num2 - 1, (float)num3 + value2), new Vector2(num2 - 1, (float)num3 + value2 + (float)num4), black);
			break;
		}
		case ItemClass.EnumCrosshairType.Plus:
			if (Event.current.type == EventType.Repaint)
			{
				Color color2 = GUI.color;
				GUI.color = new Color(color2.r, color2.g, color2.b, _guiInGame.crosshairAlpha);
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num / 2f, crosshairPosition2D.y - num / 2f, num, num), _guiInGame.CrosshairTexture, ScaleMode.StretchToFill);
				GUI.color = color2;
			}
			break;
		case ItemClass.EnumCrosshairType.Damage:
			if (Event.current.type == EventType.Repaint)
			{
				Color color7 = GUI.color;
				if (playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float a6 = color7.a * playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color7.r, color7.g, color7.b, a6);
				}
				else
				{
					GUI.color = new Color(color7.r, color7.g, color7.b, CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num / 2f, crosshairPosition2D.y - num / 2f, num, num), _guiInGame.CrosshairDamage, ScaleMode.StretchToFill);
				GUI.color = color7;
			}
			break;
		case ItemClass.EnumCrosshairType.Repair:
			if (Event.current.type == EventType.Repaint)
			{
				Color color5 = GUI.color;
				if (playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float a4 = color5.a * playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color5.r, color5.g, color5.b, a4);
				}
				else
				{
					GUI.color = new Color(color5.r, color5.g, color5.b, CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num / 2f, crosshairPosition2D.y - num / 2f, num, num), _guiInGame.CrosshairRepair, ScaleMode.StretchToFill);
				GUI.color = color5;
			}
			break;
		case ItemClass.EnumCrosshairType.Upgrade:
			if (Event.current.type == EventType.Repaint)
			{
				Color color6 = GUI.color;
				if (playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float a5 = color6.a * playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color6.r, color6.g, color6.b, a5);
				}
				else
				{
					GUI.color = new Color(color6.r, color6.g, color6.b, CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num / 2f, crosshairPosition2D.y - num / 2f, num, num), _guiInGame.CrosshairUpgrade, ScaleMode.StretchToFill);
				GUI.color = color6;
			}
			break;
		case ItemClass.EnumCrosshairType.Heal:
			if (Event.current.type == EventType.Repaint)
			{
				Color color3 = GUI.color;
				if (playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float a2 = color3.a * playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color3.r, color3.g, color3.b, a2);
				}
				else
				{
					GUI.color = new Color(color3.r, color3.g, color3.b, CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num / 2f, crosshairPosition2D.y - num / 2f, num, num), _guiInGame.CrosshairRepair, ScaleMode.StretchToFill);
				GUI.color = color3;
			}
			break;
		case ItemClass.EnumCrosshairType.PowerItem:
			if (Event.current.type == EventType.Repaint)
			{
				Color color4 = GUI.color;
				if (playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float a3 = color4.a * playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color4.r, color4.g, color4.b, a3);
				}
				else
				{
					GUI.color = new Color(color4.r, color4.g, color4.b, CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num / 2f, crosshairPosition2D.y - num / 2f, num, num), _guiInGame.CrosshairPowerItem, ScaleMode.StretchToFill);
				GUI.color = color4;
			}
			break;
		case ItemClass.EnumCrosshairType.PowerSource:
			if (Event.current.type == EventType.Repaint)
			{
				Color color = GUI.color;
				if (playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float a = color.a * playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color.r, color.g, color.b, a);
				}
				else
				{
					GUI.color = new Color(color.r, color.g, color.b, CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num / 2f, crosshairPosition2D.y - num / 2f, num, num), _guiInGame.CrosshairPowerSource, ScaleMode.StretchToFill);
				GUI.color = color;
			}
			break;
		}
	}

	public SpawnPosition GetSpawnPoint()
	{
		if (SpawnPoints == null || SpawnPoints.Count == 0)
		{
			return SpawnPosition.Undef;
		}
		return new SpawnPosition(SpawnPoints[0].ToVector3() + new Vector3(0.5f, 0f, 0.5f), 0f);
	}

	public override void AddUIHarvestingItem(ItemStack itemStack, bool _bAddOnlyIfNotExisting = false)
	{
		playerUI.xui.CollectedItemList.AddItemStack(itemStack, _bAddOnlyIfNotExisting);
	}

	public override Vector3 GetDropPosition()
	{
		Vector3 vector = base.GetDropPosition();
		Vector3 direction = vector - getHeadPosition();
		if (Physics.Raycast(new Ray(getHeadPosition() - Origin.position, direction), out var hitInfo, direction.magnitude, 1073807360))
		{
			vector = hitInfo.point - direction.normalized * 0.5f + Origin.position;
		}
		return vector;
	}

	public bool CheckSpawnPointStillThere()
	{
		SpawnPosition spawnPoint = GetSpawnPoint();
		if (spawnPoint.IsUndef())
		{
			return true;
		}
		if (world.GetChunkFromWorldPos(spawnPoint.ToBlockPos()) == null)
		{
			return true;
		}
		if (world.GetBlock(spawnPoint.ToBlockPos()).Block is BlockSleepingBag)
		{
			return true;
		}
		return false;
	}

	public void RemoveSpawnPoints(bool showTooltip = true)
	{
		SpawnPoints.Clear();
		if (showTooltip)
		{
			GameManager.ShowTooltip(this, Localization.Get("ttBedrollGone"));
		}
		selectedSpawnPointKey = -1L;
	}

	public void EmptyBackpackAndToolbelt()
	{
		ItemStack[] slots = bag.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i] = ItemStack.Empty.Clone();
		}
		bag.SetSlots(slots);
		for (int j = 0; j < inventory.GetItemCount(); j++)
		{
			if (j != inventory.DUMMY_SLOT_IDX)
			{
				inventory.SetItem(j, new ItemStack(ItemValue.None.Clone(), 0));
			}
		}
	}

	public void EmptyBackpack()
	{
		ItemStack[] slots = bag.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i] = ItemStack.Empty.Clone();
		}
		bag.SetSlots(slots);
	}

	public void EmptyToolbelt(int start, int end)
	{
		for (int i = start; i < end; i++)
		{
			if (i != inventory.DUMMY_SLOT_IDX)
			{
				inventory.SetItem(i, new ItemStack(ItemValue.None.Clone(), 0));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMapObjects(bool usePersistantBackpackPositions)
	{
		if (persistentPlayerData == null)
		{
			return;
		}
		List<Vector3i> landProtectionBlocks = persistentPlayerData.GetLandProtectionBlocks();
		for (int i = 0; i < landProtectionBlocks.Count; i++)
		{
			NavObject navObject = NavObjectManager.Instance.RegisterNavObject("land_claim", landProtectionBlocks[i].ToVector3());
			if (navObject != null)
			{
				navObject.OwnerEntity = this;
			}
		}
		persistentPlayerData.ShowBedrollOnMap();
		SetDroppedBackpackPositions(usePersistantBackpackPositions ? persistentPlayerData.GetDroppedBackpackPositions() : droppedBackpackPositions);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AnalyticsSendDeath(DamageResponse _dmResponse)
	{
		DamageSource source = _dmResponse.Source;
		string text = (isFallDeath ? "fall" : ((source.BuffClass != null) ? source.BuffClass.Name : ((source.ItemClass == null) ? source.damageType.ToStringCached() : source.ItemClass.Name)));
		if ((bool)entityThatKilledMe)
		{
			text += "_";
			text = ((entityThatKilledMe == this) ? (text + "self") : ((!(entityThatKilledMe is EntityPlayer)) ? (text + entityThatKilledMe.EntityName) : (text + "player")));
		}
		GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.PlayerDeathCauses, text, 1);
	}

	public AutoMove EnableAutoMove(bool _enable)
	{
		if (!_enable)
		{
			autoMove = null;
		}
		else
		{
			autoMove = new AutoMove(this);
		}
		return autoMove;
	}

	public void TryCancelBowDraw()
	{
		ItemAction itemAction = inventory.holdingItem.Actions[0];
		if (itemAction is ItemActionCatapult)
		{
			itemAction.CancelAction(inventory.holdingItemData.actionData[0]);
			inventory.holdingItemData.actionData[0].HasExecuted = false;
		}
	}

	public bool TryAddRecoveryPosition(Vector3i _position)
	{
		if (recoveryPositions.Contains(_position))
		{
			return false;
		}
		if (!GameManager.Instance.World.CanPlayersSpawnAtPos(_position) || GameManager.Instance.World.GetPOIAtPosition(_position.ToVector3()) != null)
		{
			return false;
		}
		if (recoveryPositions.Count == 0)
		{
			recoveryPositions.Add(_position);
			return true;
		}
		if ((recoveryPositions[recoveryPositions.Count - 1] - _position).ToVector3().sqrMagnitude < 10000f)
		{
			return false;
		}
		if (recoveryPositions.Count >= 5)
		{
			recoveryPositions.RemoveAt(0);
		}
		recoveryPositions.Add(_position);
		return true;
	}

	public void GiveExp(CraftCompleteData data)
	{
		int num = (int)Buffs.GetCustomVar("_craftCount_" + data.RecipeName);
		int recipeUsedCount = data.RecipeUsedCount;
		Buffs.SetCustomVar("_craftCount_" + data.RecipeName, num + recipeUsedCount);
		Progression.AddLevelExp(data.CraftExpGain / (num + recipeUsedCount), "_xpFromCrafting", Progression.XPTypes.Crafting);
		totalItemsCrafted += (uint)recipeUsedCount;
		QuestEventManager.Current.CraftedItem(data.CraftedItemStack);
		XUiC_RecipeStack.HandleCraftXPGained();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		renderManager.OnGUI();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateStepSound(float _distX, float _distZ)
	{
		if (bFirstPersonView)
		{
			base.updateStepSound(_distX, _distZ);
		}
	}

	public override void PlayStepSound()
	{
		if (!bFirstPersonView)
		{
			base.PlayStepSound();
		}
	}

	public bool WeaponIsHolstered()
	{
		return weaponIsHolstered;
	}

	public void HolsterWeapon(bool holster)
	{
		weaponIsHolstered = holster;
		emodel.avatarController.UpdateBool("Holstered", holster);
	}
}

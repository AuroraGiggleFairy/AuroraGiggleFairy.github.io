using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Audio;
using BhvrAnalyticsServices.Interfaces;
using GUI_2;
using InControl;
using MapRendering;
using Platform;
using SandboxOptions;
using Services;
using Services.Analytics.Events;
using Twitch;
using Unity.Collections;
using UnityEngine;
using Webserver;

public class GameManager : MonoBehaviour, IGameManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct BlockParticleCreationData(Vector3i _blockPos, ParticleEffect _particleEffect)
	{
		public Vector3i blockPos = _blockPos;

		public ParticleEffect particleEffect = _particleEffect;
	}

	public delegate void OnWorldChangedEvent(World _world);

	public delegate void OnLocalPlayerChangedEvent(EntityPlayerLocal _localPlayer);

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EMultiShutReason
	{
		AppNoNetwork,
		AppSuspended,
		PermMissingMultiplayer,
		PermMissingCrossplay
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ExplodeGroup
	{
		public struct Falling
		{
			public Vector3i pos;

			public BlockValue bv;
		}

		public Vector3 pos;

		public float radius;

		public int delay;

		public List<Falling> fallings = new List<Falling>();
	}

	public class EntityItemLifetimeComparer : IComparer<EntityItem>
	{
		public int Compare(EntityItem _obj1, EntityItem _obj2)
		{
			return (int)(_obj2.lifetime - _obj1.lifetime);
		}
	}

	public delegate void RemoteResourcesCompleteHandler();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMinSpawnDistanceFromTrader = 250;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxSpawnDistanceFromTrader = 750;

	public static int frameCount;

	public static float frameTime;

	public static int fixedUpdateCount;

	public AudioSource UIAudioSource;

	public AudioClip BackgroundMusicClip;

	public AudioClip CreditsSongClip;

	public bool DebugAILines;

	public bool DebugAIAim;

	public StabilityViewer stabilityViewer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeParticleManager biomeParticleManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int cameraCullMask;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShowBackground = true;

	public static bool enableNetworkdPrioritization = true;

	public static bool unreliableNetPackets = true;

	public static ServerDateTimeResult ServerClockSync;

	public NetPackageMetrics netpackageMetrics;

	public bool showOpenerMovieOnLoad;

	public bool GameHasStarted;

	public RuntimeAnimatorController FirstPersonWeaponAnimatorController;

	public RuntimeAnimatorController ThirdPersonWeaponAnimatorController;

	public Color backgroundColor = Color.white;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int currentBackgroundColorChannel;

	public static bool bPhysicsActive;

	public static bool bTickingActive;

	public static bool bSavingActive = true;

	public static bool bShowDecorBlocks = true;

	public static bool bShowLootBlocks = true;

	public static bool bShowPaintables = true;

	public static bool bShowUnpaintables = true;

	public static bool bShowTerrain = true;

	public static bool bVolumeBlocksEditing;

	public static bool bHideMainMenuNextTime;

	public static bool bRecordNextSession;

	public static bool bPlayRecordedSession;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isDedicatedChecked = false;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isDedicated = false;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public World m_World;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool worldCreated;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool chunkClusterLoaded;

	public bool worldInitInfoReceived;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int myPlayerId = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal myEntityPlayerLocal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public IMapChunkDatabase fowDatabaseForLocalPlayer;

	public FPS fps = new FPS(5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject m_SoundsGameObject;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeWorldTickTimeSentToClients;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeGameStateCheckedAndSynced;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeDecoSaved;

	public AdminTools adminTools;

	public PersistentPlayerList persistentPlayers;

	public PersistentPlayerData persistentLocalPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GUIWindowManager windowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public NGUIWindowManager nguiWindowManager;

	public LootManager lootManager;

	public TraderManager traderManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int lastDisplayedValueOfTeamTickets;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, GameObject> m_PositionSoundMap = new Dictionary<Vector3i, GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> tileEntitiesMusicToRemove = new List<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int msPassedSinceLastUpdate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public IBlockTool activeBlockTool;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public IBlockTool blockSelectionTool;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEditMode;

	public bool bCursorVisible = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCursorVisibleOverride;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCursorVisibleOverrideState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<Vector3i, Transform> m_BlockParticles = new DictionarySave<Vector3i, Transform>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CountdownTimer countdownCheckBlockParticles = new CountdownTimer(1.1f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CountdownTimer countdownSendPlayerDataFileToServer = new CountdownTimer(30f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CountdownTimer countdownSaveLocalPlayerDataFile = new CountdownTimer(30f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float unloadAssetsDuration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnloadAssetsReady;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MicroStopwatch stopwatchUnloadAssets = new MicroStopwatch(_bStart: false);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CountdownTimer countdownSendPlayerInventoryToServer = new CountdownTimer(0.1f, _start: false);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool sendPlayerToolbelt;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool sendPlayerBag;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool sendPlayerEquipment;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool sendDragAndDropItem;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandomManager gameRandomManager;

	public GameStateManager gameStateManager;

	public PrefabLODManager prefabLODManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RespawnType clientRespawnType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fpsCountdownTimer = 30f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float gcCountdownTimer = 120f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float wsCountdownTimer = 30f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float playerPositionsCountdownTimer = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch swCopyChunks = new MicroStopwatch();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch swUpdateTime = new MicroStopwatch();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int lastStatsPlayerCount;

	public static long MaxMemoryConsumption;

	public WaitForTargetFPS waitForTargetFPS;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockParticleCreationData> blockParticlesToSpawn = new List<BlockParticleCreationData>();

	public static GameManager Instance;

	public TriggerEffectManager triggerEffectManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int lastPlayerCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int currentRefreshRate = 60;

	public bool bStaticDataLoadSync;

	public bool bStaticDataLoaded;

	public string CurrentLoadAction;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int lastTimeAbsPosSentToServer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bLastWasAttached;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeToClearAllPools = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float activityCheck;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool shuttingDownMultiplayerServices;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int testing;

	public bool canSpawnPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDisconnectingLater;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowQuit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isQuitting;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstTimeJoin;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> tempExplPositions = new List<BlockChangeInfo>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ExplodeGroup> explodeFallingGroups = new List<ExplodeGroup>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ChunkCluster> ccChanged = new List<ChunkCluster>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<string> materialsBefore = null;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float unusedAssetsTimer = 0f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool runningAssetsUnused = false;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool? requestedPauseState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool gamePaused;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool retrievingEula;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool retrievingBacktraceConfig;

	public static bool DebugCensorship;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> persistentPlayerIds;

	public static bool IsDedicatedServer
	{
		get
		{
			if (!isDedicatedChecked)
			{
				string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
				for (int i = 0; i < commandLineArgs.Length; i++)
				{
					if (commandLineArgs[i].Equals(Constants.cArgDedicatedServer))
					{
						isDedicated = true;
					}
				}
				isDedicatedChecked = true;
			}
			return isDedicated;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool GameIsFocused
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsMouseCursorVisible
	{
		get
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				return Cursor.visible;
			}
			return false;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsStartingGame
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsQuitting => isQuitting;

	public World World
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_World;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool UpdatingRemoteResources
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool RemoteResourcesLoaded
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int persistentPlayerCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return persistentPlayerIds.Count;
		}
	}

	public event OnWorldChangedEvent OnWorldChanged;

	public event OnLocalPlayerChangedEvent OnLocalPlayerChanged;

	public event Action<ClientInfo> OnClientSpawned;

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator waitForGameStart()
	{
		if (IsDedicatedServer || IsEditMode())
		{
			yield break;
		}
		EntityPlayerLocal epl;
		while (true)
		{
			if (World == null)
			{
				yield break;
			}
			epl = World.GetPrimaryPlayer();
			if (epl != null)
			{
				break;
			}
			yield return null;
		}
		while (!epl.IsSpawned())
		{
			yield return null;
		}
		epl.HasUpdated = false;
		while (!epl.HasUpdated)
		{
			yield return false;
		}
		yield return null;
		GameHasStarted = true;
	}

	public void ShowBackground(bool show)
	{
		if (bShowBackground == show)
		{
			return;
		}
		bShowBackground = show;
		Camera main = Camera.main;
		if (main != null)
		{
			if (!bShowBackground)
			{
				cameraCullMask = main.cullingMask;
				main.cullingMask = LayerMask.GetMask("LocalPlayer");
				main.backgroundColor = backgroundColor;
			}
			else
			{
				main.cullingMask = cameraCullMask;
			}
		}
	}

	public bool ShowBackground()
	{
		return bShowBackground;
	}

	public void IncreaseBackgroundColor()
	{
		switch (currentBackgroundColorChannel)
		{
		case 0:
			backgroundColor.r += 0.003921569f;
			break;
		case 1:
			backgroundColor.g += 0.003921569f;
			break;
		case 2:
			backgroundColor.b += 0.003921569f;
			break;
		}
		backgroundColor.r = Mathf.Clamp01(backgroundColor.r);
		backgroundColor.g = Mathf.Clamp01(backgroundColor.g);
		backgroundColor.b = Mathf.Clamp01(backgroundColor.b);
		Camera main = Camera.main;
		if (main != null)
		{
			main.backgroundColor = backgroundColor;
		}
	}

	public void DecreaseBackgroundColor()
	{
		switch (currentBackgroundColorChannel)
		{
		case 0:
			backgroundColor.r -= 0.003921569f;
			break;
		case 1:
			backgroundColor.g -= 0.003921569f;
			break;
		case 2:
			backgroundColor.b -= 0.003921569f;
			break;
		}
		backgroundColor.r = Mathf.Clamp01(backgroundColor.r);
		backgroundColor.g = Mathf.Clamp01(backgroundColor.g);
		backgroundColor.b = Mathf.Clamp01(backgroundColor.b);
		Camera main = Camera.main;
		if (main != null)
		{
			main.backgroundColor = backgroundColor;
		}
	}

	public void BackgroundColorNext()
	{
		currentBackgroundColorChannel++;
		if (currentBackgroundColorChannel > 2)
		{
			currentBackgroundColorChannel = 0;
		}
	}

	public void BackgroundColorPrev()
	{
		currentBackgroundColorChannel--;
		if (currentBackgroundColorChannel < 0)
		{
			currentBackgroundColorChannel = 2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnUserDetailsUpdated(IPlatformUserData userData, string name)
	{
		if (persistentPlayers != null)
		{
			persistentPlayers.HandlePlayerDetailsUpdate(userData, name);
		}
	}

	public void ApplyAllOptions()
	{
		if (windowManager != null)
		{
			GameOptionsManager.ApplyAllOptions(windowManager.playerUI);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (!GameEntrypoint.EntrypointSuccess)
		{
			return;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		Instance = this;
		GameIsFocused = !IsDedicatedServer && Application.isFocused;
		Log.Out("Awake IsFocused: " + Application.isFocused);
		Log.Out("Awake");
		ThreadManager.SetMonoBehaviour(this);
		Utils.InitStatic();
		LoadManager.Init();
		if (Application.isEditor)
		{
			Application.runInBackground = true;
			bCursorVisibleOverride = true;
			bCursorVisibleOverrideState = true;
		}
		if (!IsDedicatedServer)
		{
			GameOptionsManager.ResolutionChanged += OnResolutionChanged;
			RefreshRefreshRate();
			UpdateFPSCap();
		}
		else
		{
			Application.targetFrameRate = 500;
		}
		Application.wantsToQuit += OnApplicationQuit;
		if (IsDedicatedServer && GamePrefs.GetBool(EnumGamePrefs.TerminalWindowEnabled))
		{
			try
			{
				WinFormInstance server = new WinFormInstance();
				SingletonMonoBehaviour<SdtdConsole>.Instance.RegisterServer(server);
			}
			catch (Exception e)
			{
				Log.Error("Could not start Terminal Window:");
				Log.Exception(e);
			}
		}
		windowManager = UnityEngine.Object.FindAnyObjectByType<GUIWindowManager>();
		nguiWindowManager = UnityEngine.Object.FindAnyObjectByType<NGUIWindowManager>();
		TaskManager.Init();
		LocalPlayerManager.Init();
		GameStatsBridge.Init();
		if (!IsDedicatedServer)
		{
			GameOptionsControls.Load();
		}
		MeshDataManager.Init();
		OcclusionManager.Load();
		waitForTargetFPS = new GameObject("WaitForTargetFPS").AddComponent<WaitForTargetFPS>();
		if (IsDedicatedServer)
		{
			GameOptionsManager.ApplyTextureQuality(3);
			QualitySettings.vSyncCount = 0;
			waitForTargetFPS.TargetFPS = 0;
		}
		else
		{
			QualitySettings.vSyncCount = GamePrefs.GetInt(PlatformApplicationManager.Application.VSyncCountPref);
			waitForTargetFPS.TargetFPS = 0;
		}
		ServerDateTimeRequest.GetNtpTimeAsync([PublicizedFrom(EAccessModifier.Internal)] (ServerDateTimeResult result) =>
		{
			ServerClockSync = result;
		});
		GameObjectPool.Instance.Init();
		MemoryPools.InitStatic(!IsDedicatedServer);
		gameRandomManager = GameRandomManager.Instance;
		gameStateManager = new GameStateManager(this);
		prefabLODManager = new PrefabLODManager();
		new PrefabEditModeManager();
		UnityEngine.Object.Instantiate(DataLoader.LoadAsset<GameObject>("@:Sound_Mixers/AudioMixerManager.prefab"));
		m_SoundsGameObject = GameObject.Find("Sounds");
		PhysicsInit();
		ParticleEffect.Init();
		SelectionBoxManager.Instance.SetupCategories();
		if (!IsDedicatedServer)
		{
			if (GameOptionsReset.NeedsResetGame())
			{
				GameOptionsReset.ResetGame(RoamingPrefs.Store);
				GamePrefs.Instance.Save();
				Log.Out("Game Reset");
			}
			else
			{
				if (GameOptionsReset.Graphics.NeedsReset())
				{
					GameOptionsReset.Graphics.Reset(RoamingPrefs.Store);
					GamePrefs.Instance.Save();
					Log.Out("Graphics Reset");
				}
				if (GameOptionsReset.Controls.NeedsReset())
				{
					GameOptionsReset.Controls.Reset(RoamingPrefs.Store);
					GamePrefs.Instance.Save();
					Log.Out("Controls Reset");
				}
				if (GameOptionsReset.Bindings.NeedsReset())
				{
					GameOptionsReset.Bindings.Reset(RoamingPrefs.Store);
					GamePrefs.Instance.Save();
					Log.Out("Bindings Reset");
				}
			}
		}
		DeviceGamePrefs.Apply();
		GameOptionsManager.ValidateGamePrefs();
		if (!IsDedicatedServer)
		{
			GameOptionsManager.ApplyAllOptions(windowManager.playerUI);
			UIUtils.LoadAtlas();
		}
		Manager.Init();
		UIOptions.Init();
		UIRoot uIRoot = UnityEngine.Object.FindAnyObjectByType<UIRoot>();
		if (!IsDedicatedServer)
		{
			InitMultiSourceUiAtlases(uIRoot.gameObject);
		}
		windowManager.gameObject.AddComponent<LocalPlayerUI>();
		blockSelectionTool = new BlockToolSelection();
		nguiWindowManager.ParseWindows();
		float activeUiScale = GameOptionsManager.GetActiveUiScale();
		nguiWindowManager.SetBackgroundScale(activeUiScale);
		AddWindows(windowManager);
		PrefabVolumeManager.Instance.Init();
		ModManager.LoadMods();
		ThreadManager.RunCoroutineSync(ModManager.LoadPatchStuff(_isLoadingInGame: false));
		adminTools = new AdminTools();
		SingletonMonoBehaviour<SdtdConsole>.Instance.RegisterCommands();
		IEnumerator enumerator = loadStaticData();
		if (IsDedicatedServer)
		{
			bStaticDataLoadSync = true;
			ThreadManager.RunCoroutineSync(enumerator);
		}
		else
		{
			bStaticDataLoadSync = false;
			ThreadManager.StartCoroutine(enumerator);
		}
		if (!IsDedicatedServer)
		{
			CursorControllerAbs.LoadStaticData(LoadManager.CreateGroup());
		}
		else
		{
			InputManager.Enabled = false;
		}
		if (IsDedicatedServer && GamePrefs.GetBool(EnumGamePrefs.TelnetEnabled))
		{
			try
			{
				TelnetConsole server2 = new TelnetConsole();
				SingletonMonoBehaviour<SdtdConsole>.Instance.RegisterServer(server2);
			}
			catch (Exception e2)
			{
				Log.Error("Could not start network console:");
				Log.Exception(e2);
			}
		}
		WebServer.Init();
		global::MapRendering.MapRendering.Init();
		AuthorizationManager.Instance.Init();
		ModEvents.SGameAwakeData _data = default(ModEvents.SGameAwakeData);
		ModEvents.GameAwake.Invoke(ref _data);
		nguiWindowManager.Show(EnumNGUIWindow.InGameHUD, _bEnable: false);
		ConsoleCmdShow.Init();
		if (!IsDedicatedServer)
		{
			GameSenseManager.Instance?.Init();
			if (GamePrefs.GetBool(EnumGamePrefs.OptionsMumblePositionalAudioSupport))
			{
				MumblePositionalAudio.Init();
			}
		}
		_ = DiscordManager.Instance;
		if ((bool)BackgroundMusicClip || (bool)CreditsSongClip)
		{
			if (!IsDedicatedServer)
			{
				base.gameObject.AddComponent<BackgroundMusicMono>();
			}
			else
			{
				Resources.UnloadAsset(BackgroundMusicClip);
				Resources.UnloadAsset(CreditsSongClip);
			}
		}
		PartyQuests.EnforeInstance();
		Input.simulateMouseWithTouches = false;
		IApplicationStateController applicationStateController = PlatformManager.NativePlatform?.ApplicationState;
		if (applicationStateController != null)
		{
			ApplicationState lastState = ApplicationState.Foreground;
			applicationStateController.OnApplicationStateChanged += [PublicizedFrom(EAccessModifier.Internal)] (ApplicationState state) =>
			{
				if (state != ApplicationState.Suspended && lastState == ApplicationState.Suspended)
				{
					OnApplicationResume();
				}
				lastState = state;
			};
			applicationStateController.OnNetworkStateChanged += OnNetworkStateChanged;
		}
		PlatformUserManager.DetailsUpdated += OnUserDetailsUpdated;
		if (!IsDedicatedServer)
		{
			triggerEffectManager = new TriggerEffectManager();
			TriggerEffectManager.SetMainMenuLightbarColor();
		}
		Log.Out("Awake done in " + microStopwatch.ElapsedMilliseconds + " ms");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitMultiSourceUiAtlases(GameObject _parent)
	{
		GameObject atlasesGo = new GameObject("UIAtlases");
		atlasesGo.transform.parent = _parent.transform;
		Shader shader = Shader.Find("Unlit/Transparent Colored");
		Shader shader2 = Shader.Find("Unlit/Transparent Greyscale");
		MultiSourceAtlasManager multiSourceAtlasManager = MultiSourceAtlasManager.Create(atlasesGo, "ItemIconAtlas");
		MultiSourceAtlasManager atlasManager = MultiSourceAtlasManager.Create(atlasesGo, "ItemIconAtlasGreyscale");
		ModManager.ModAtlasesDefaults(atlasesGo, shader);
		ModManager.RegisterAtlasManager(multiSourceAtlasManager, _createdByMod: false, shader, AddGreyscaleItemIconAtlas);
		ModManager.RegisterAtlasManager(atlasManager, _createdByMod: false, shader2);
		Resources.Load<UIAtlas>("GUI/Prefabs/SymbolAtlas");
		Resources.Load<UIAtlas>("GUI/Prefabs/ControllerArtAtlas");
		INGUIAtlas[] atlases = Resources.FindObjectsOfTypeAll<UIAtlas>();
		addLoadedAtlases(atlases);
		atlases = Resources.FindObjectsOfTypeAll<NGUIAtlas>();
		addLoadedAtlases(atlases);
		string mipFilter = GameOptionsPlatforms.GetItemIconFilterString();
		LoadManager.AddressableAssetsRequestTask<GameObject> addressableAssetsRequestTask = LoadManager.LoadAssetsFromAddressables<GameObject>("iconatlas", [PublicizedFrom(EAccessModifier.Internal)] (string address) =>
		{
			if (!address.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return address.Contains(mipFilter) ? true : false;
		}, null, _deferLoading: false, _loadSync: true);
		List<GameObject> list = new List<GameObject>();
		addressableAssetsRequestTask.CollectResults(list);
		foreach (GameObject item in list)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(item);
			gameObject.transform.parent = multiSourceAtlasManager.transform;
			UIAtlas component = gameObject.GetComponent<UIAtlas>();
			multiSourceAtlasManager.AddAtlas(component, gameObject, _isLoadingInGame: false);
			AddGreyscaleItemIconAtlas(component, _isLoadingInGame: false);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void addLoadedAtlases(INGUIAtlas[] _atlases)
		{
			for (int i = 0; i < _atlases.Length; i++)
			{
				string a = _atlases[i].Name;
				if (!a.ContainsCaseInsensitive("icons_") && !(ModManager.GetAtlasManager(a) != null))
				{
					MultiSourceAtlasManager multiSourceAtlasManager2 = MultiSourceAtlasManager.Create(atlasesGo, a);
					INGUIAtlas iNGUIAtlas = _atlases[i].Clone();
					GameObject gameObject2 = null;
					if (iNGUIAtlas is UIAtlas uIAtlas)
					{
						gameObject2 = uIAtlas.gameObject;
						uIAtlas.transform.parent = multiSourceAtlasManager2.transform;
					}
					ModManager.RegisterAtlasManager(multiSourceAtlasManager2, _createdByMod: false, iNGUIAtlas.spriteMaterial.shader);
					multiSourceAtlasManager2.AddAtlas(iNGUIAtlas, gameObject2, _isLoadingInGame: false);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddGreyscaleItemIconAtlas(INGUIAtlas _atlas, bool _isLoadingInGame)
	{
		MultiSourceAtlasManager atlasManager = ModManager.GetAtlasManager("ItemIconAtlasGreyscale");
		Shader shader = Shader.Find("Unlit/Transparent Greyscale");
		INGUIAtlas iNGUIAtlas = _atlas.Clone();
		GameObject gameObject = null;
		if (iNGUIAtlas is UIAtlas uIAtlas)
		{
			uIAtlas.gameObject.transform.parent = atlasManager.transform;
			gameObject = uIAtlas.gameObject;
		}
		Material material = new Material(shader);
		material.mainTexture = iNGUIAtlas.texture;
		iNGUIAtlas.spriteMaterial = material;
		atlasManager.AddAtlas(iNGUIAtlas, gameObject, _isLoadingInGame);
	}

	public void AddWindows(GUIWindowManager _guiWindowManager)
	{
		if (_guiWindowManager == windowManager)
		{
			_guiWindowManager.Add(GUIWindowConsole.GetNewInstance());
			_guiWindowManager.Add(new GUIWindowScreenshotText());
		}
		_guiWindowManager.Add(new GUIWindowNGUI(EnumNGUIWindow.InGameHUD));
		_guiWindowManager.CloseAllOpenModalWindows();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator loadStaticData()
	{
		CurrentLoadAction = Localization.Get("loadActionCharacterModels");
		yield return null;
		CurrentLoadAction = Localization.Get("loadActionTerrainTextures");
		yield return null;
		yield return null;
		yield return WorldStaticData.Init(_bForce: false, IsDedicatedServer, [PublicizedFrom(EAccessModifier.Private)] (string _progressText, float _percentage) =>
		{
			CurrentLoadAction = _progressText;
		});
		CurrentLoadAction = Localization.Get("loadActionDone");
		bStaticDataLoaded = true;
	}

	public void StartGame(bool _offline)
	{
		Time.timeScale = 1f;
		GamePrefs.Set(EnumGamePrefs.GameGuidClient, "");
		PlatformManager.MultiPlatform.UserDataRoaming.ValidateRoamingMode();
		if (GameSparksManager.Instance() != null)
		{
			GameSparksManager.Instance().PrepareNewSession();
		}
		StartCoroutine(startGameCo(_offline));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startGameCo(bool _offline)
	{
		IsStartingGame = true;
		Log.Out("StartGame");
		ModEvents.SGameStartingData _data = new ModEvents.SGameStartingData(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
		ModEvents.GameStarting.Invoke(ref _data);
		allowQuit = false;
		backgroundColor = Color.white;
		EntityStats.WeatherSurvivalEnabled = true;
		yield return null;
		yield return ModManager.LoadPatchStuff(_isLoadingInGame: true);
		yield return null;
		SaveInfoProvider.Instance.ClearResources();
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield break;
		}
		if (!IsDedicatedServer)
		{
			XUiC_MainMenu.CloseGlobalMenuWindows(windowManager.playerUI.xui);
			windowManager.CloseAllOpenModalWindows();
			XUiFromXml.ClearData();
			LocalPlayerUI.QueueUIForNewPlayerEntity(LocalPlayerUI.CreateUIForNewLocalPlayer());
			windowManager.Open(XUiC_LoadingScreen.ID, _bModal: false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.EACEnabled)
				{
					windowManager.Open("eacWarning", _bModal: false);
				}
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.AllowsCrossplay)
				{
					windowManager.Open("crossplayWarning", _bModal: false);
				}
			}
			XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, Localization.Get("uiLoadStartingGame"));
		}
		yield return null;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GamePrefs.Set(EnumGamePrefs.GameWorld, string.Empty);
		}
		isEditMode = GameModeEditWorld.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode));
		GamePrefs.Set(EnumGamePrefs.DebugStopEnemiesMoving, IsEditMode());
		GamePrefs.Set(EnumGamePrefs.DebugMenuEnabled, isEditMode || GameUtils.IsPlaytesting());
		GamePrefs.Set(EnumGamePrefs.CreativeMenuEnabled, isEditMode || GameUtils.IsPlaytesting());
		GamePrefs.Instance.Save();
		if (!Application.isEditor)
		{
			GameUtils.DebugOutputGamePrefs([PublicizedFrom(EAccessModifier.Internal)] (string _text) =>
			{
				Log.WriteLine("GamePref." + _text);
			});
			GameUtils.DebugOutputGameStats([PublicizedFrom(EAccessModifier.Internal)] (string _text) =>
			{
				Log.WriteLine("GameStat." + _text);
			});
		}
		yield return null;
		CraftingManager.InitForNewGame();
		yield return null;
		bSavingActive = true;
		bPhysicsActive = !IsEditMode();
		bTickingActive = !IsEditMode();
		bShowDecorBlocks = true;
		bShowLootBlocks = true;
		bShowPaintables = true;
		bShowUnpaintables = true;
		bShowTerrain = true;
		bVolumeBlocksEditing = true;
		Block.nameIdMapping = null;
		ItemClass.nameIdMapping = null;
		PlatformApplicationManager.SetRestartRequired();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			yield return StartAsServer(_offline);
		}
		else
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
			{
				yield break;
			}
			XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, Localization.Get("uiLoadWaitingForServer"));
			StartAsClient();
		}
		DismembermentManager.Init();
		yield return null;
		if (GameSparksManager.Instance() != null)
		{
			GameSparksManager.Instance().SessionStarted(GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameMode), SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
		}
		if (!IsDedicatedServer && SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode != ProtocolManager.NetworkType.OfflineServer)
		{
			PlatformManager.MultiPlatform.User.StartAdvertisePlaying(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		}
		Log.Out("Loading dymesh settings");
		DynamicMeshManager.CONTENT_ENABLED = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		DynamicMeshSettings.OnlyPlayerAreas = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshLandClaimOnly);
		DynamicMeshSettings.UseImposterValues = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshUseImposters);
		DynamicMeshSettings.MaxViewDistance = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshDistance);
		DynamicMeshSettings.PlayerAreaChunkBuffer = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshLandClaimBuffer);
		DynamicMeshSettings.MaxRegionMeshData = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshMaxRegionCache);
		DynamicMeshSettings.MaxDyMeshData = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshMaxItemCache);
		DynamicMeshSettings.LogSettings();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicMeshManager.Init();
		}
		ModEvents.SGameStartDoneData eventData = default(ModEvents.SGameStartDoneData);
		ModEvents.GameStartDone.Invoke(ref eventData);
		EnumResetUnprotectedChunksGroupingMode resetChunksMode = (EnumResetUnprotectedChunksGroupingMode)GamePrefs.GetInt(EnumGamePrefs.ResetUnprotectedChunks);
		ResetUnprotectedChunksOnLoad(resetChunksMode);
		if (IsDedicatedServer)
		{
			waitForTargetFPS.TargetFPS = 20;
		}
		Log.Out("StartGame done");
		IsStartingGame = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetUnprotectedChunksOnLoad(EnumResetUnprotectedChunksGroupingMode _resetChunksMode)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && _resetChunksMode != EnumResetUnprotectedChunksGroupingMode.Disabled && m_World?.ChunkCache?.ChunkProvider is ChunkProviderGenerateWorld chunkProviderGenerateWorld)
		{
			chunkProviderGenerateWorld.MainThreadCacheProtectedPositions();
			chunkProviderGenerateWorld.ResetAllChunks(ChunkProtectionLevel.All, _resetChunksMode);
			Log.Out("[GameManager] Unprotected chunks have been reset.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTimeOfDay()
	{
		if (!IsDedicatedServer || m_World.Players.list.Count > 0)
		{
			int num = GameStats.GetInt(EnumGameStats.TimeOfDayIncPerSec);
			if (num == 0)
			{
				msPassedSinceLastUpdate += (int)(Time.deltaTime * 1000f);
				if (msPassedSinceLastUpdate >= 100)
				{
					m_World.SetTime(m_World.worldTime);
					msPassedSinceLastUpdate = 0;
				}
			}
			else
			{
				float num2 = 1000f / (float)num;
				msPassedSinceLastUpdate += (int)(Time.deltaTime * 1000f);
				if ((float)msPassedSinceLastUpdate <= Utils.FastMax(num2, 50f))
				{
					return;
				}
				int num3 = (int)((float)msPassedSinceLastUpdate / num2);
				msPassedSinceLastUpdate -= (int)num2 * num3;
				ulong time = m_World.worldTime + (ulong)num3;
				m_World.SetTime(time);
			}
		}
		PlatformManager.NativePlatform.LobbyHost?.UpdateGameTimePlayers(m_World.worldTime, m_World.Players.list.Count);
		GameSenseManager.Instance?.UpdateEventTime(m_World.worldTime);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.UpdateGameTimePlayers(m_World.worldTime, m_World.Players.list.Count);
		if (Time.time - lastTimeWorldTickTimeSentToClients > Constants.cSendWorldTickTimeToClients)
		{
			lastTimeWorldTickTimeSentToClients = Time.time;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWorldTime>().Setup(m_World.worldTime), _onlyClientsAttachedToAnEntity: true);
			if (WeatherManager.Instance != null)
			{
				WeatherManager.Instance.SendPackages();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSendClientPlayerPositionToServer()
	{
		EntityPlayerLocal primaryPlayer = m_World.GetPrimaryPlayer();
		EntityAlive entityAlive = primaryPlayer;
		if (entityAlive != null)
		{
			if (entityAlive.AttachedToEntity != null)
			{
				entityAlive = entityAlive.AttachedToEntity as EntityAlive;
				bLastWasAttached = true;
				if (entityAlive.isEntityRemote)
				{
					return;
				}
			}
			else
			{
				if (bLastWasAttached)
				{
					lastTimeAbsPosSentToServer = int.MaxValue;
				}
				bLastWasAttached = false;
			}
		}
		if (entityAlive == null)
		{
			return;
		}
		if (primaryPlayer.bPlayerStatsChanged)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(primaryPlayer));
			primaryPlayer.bPlayerStatsChanged = false;
		}
		if (primaryPlayer.bPlayerTwitchChanged)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerTwitchStats>().Setup(primaryPlayer));
			primaryPlayer.bPlayerTwitchChanged = false;
		}
		if (primaryPlayer.bEntityAliveFlagsChanged)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAliveFlags>().Setup(primaryPlayer));
			primaryPlayer.bEntityAliveFlagsChanged = false;
		}
		if (primaryPlayer.bPlayerEquipmentChanged)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerEquipment>().Setup(primaryPlayer));
			primaryPlayer.bPlayerEquipmentChanged = false;
		}
		Vector3i vector3i = NetEntityDistributionEntry.EncodePos(entityAlive.position);
		Vector3i deltaPos = vector3i - entityAlive.serverPos;
		bool num = Utils.FastAbs(deltaPos.x) >= 2f || Utils.FastAbs(deltaPos.y) >= 2f || Utils.FastAbs(deltaPos.z) >= 2f || entityAlive.emodel.IsRagdollActive;
		Vector3i vector3i2 = NetEntityDistributionEntry.EncodeRot(entityAlive.rotation);
		Vector3i vector3i3 = vector3i2 - entityAlive.serverRot;
		bool flag = Utils.FastAbs(vector3i3.x) >= 1f || Utils.FastAbs(vector3i3.y) >= 1f || Utils.FastAbs(vector3i3.z) >= 1f || entityAlive.emodel.IsRagdollActive;
		if (num || flag)
		{
			if (deltaPos.x < -256 || deltaPos.x >= 256 || deltaPos.y < -256 || deltaPos.y >= 256 || deltaPos.z < -256 || deltaPos.z >= 256)
			{
				lastTimeAbsPosSentToServer = 0;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityTeleport>().Setup(entityAlive));
			}
			else if (deltaPos.x < -128 || deltaPos.x >= 128 || deltaPos.y < -128 || deltaPos.y >= 128 || deltaPos.z < -128 || deltaPos.z >= 128 || lastTimeAbsPosSentToServer > 100)
			{
				lastTimeAbsPosSentToServer = 0;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityPosAndRot>().Setup(entityAlive));
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityRelPosAndRot>().Setup(entityAlive.entityId, deltaPos, vector3i2, entityAlive.qrotation, entityAlive.onGround, entityAlive.IsQRotationUsed(), 3));
			}
			entityAlive.serverPos = vector3i;
			entityAlive.serverRot = vector3i2;
			lastTimeAbsPosSentToServer++;
		}
		if (entityAlive != primaryPlayer)
		{
			if (entityAlive.bPlayerStatsChanged)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(entityAlive));
				entityAlive.bPlayerStatsChanged = false;
			}
			if (entityAlive.bPlayerTwitchChanged)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerTwitchStats>().Setup(entityAlive));
				entityAlive.bPlayerTwitchChanged = false;
			}
			if (entityAlive.bEntityAliveFlagsChanged)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAliveFlags>().Setup(entityAlive));
				entityAlive.bEntityAliveFlagsChanged = false;
			}
			if (primaryPlayer.bPlayerEquipmentChanged)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerEquipment>().Setup(primaryPlayer));
				primaryPlayer.bPlayerEquipmentChanged = false;
			}
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(primaryPlayer);
		if (countdownSendPlayerDataFileToServer.HasPassed() && uIForPlayer.xui != null && uIForPlayer.xui.IsReady)
		{
			countdownSendPlayerDataFileToServer.ResetAndRestart();
			doSendLocalPlayerData(primaryPlayer);
		}
		if (countdownSendPlayerInventoryToServer.HasPassed())
		{
			countdownSendPlayerInventoryToServer.Reset();
			doSendLocalInventory(primaryPlayer);
		}
		if (primaryPlayer.persistentPlayerData != null && primaryPlayer.persistentPlayerData.questPositionsChanged)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerQuestPositions>().Setup(primaryPlayer.entityId, primaryPlayer.persistentPlayerData));
			primaryPlayer.persistentPlayerData.questPositionsChanged = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		fixedUpdateCount++;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		gmUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gmUpdate()
	{
		frameCount = Time.frameCount;
		frameTime = Time.time;
		fixedUpdateCount = 0;
		updatePauseState();
		GameOptionsManager.CheckResolution();
		ModEvents.SUnityUpdateData _data = default(ModEvents.SUnityUpdateData);
		ModEvents.UnityUpdate.Invoke(ref _data);
		handleGlobalActions();
		if (!ReportUnusedAssets())
		{
			return;
		}
		if ((double)Time.timeScale <= 0.001)
		{
			Physics.SyncTransforms();
		}
		LoadManager.Update();
		PlatformManager.Update();
		InviteManager.Instance.Update();
		LockManager.Instance.Update();
		swUpdateTime.ResetAndRestart();
		fps.Update();
		BlockLiquidv2.UpdateTime();
		if (QuestEventManager.Current != null)
		{
			QuestEventManager.Current.Update();
		}
		if (m_World != null)
		{
			m_World.triggerManager.Update();
		}
		if (TwitchVoteScheduler.Current != null)
		{
			TwitchVoteScheduler.Current.Update(Time.deltaTime);
		}
		if (TwitchManager.Current != null)
		{
			TwitchManager.Current.Update(Time.unscaledDeltaTime);
		}
		if (GameEventManager.Current != null)
		{
			GameEventManager.Current.Update(Time.deltaTime);
		}
		if (PowerManager.HasInstance)
		{
			PowerManager.Instance.Update();
		}
		if (PartyManager.HasInstance)
		{
			PartyManager.Current.Update();
		}
		if (VehicleManager.Instance != null)
		{
			VehicleManager.Instance.Update();
		}
		if (DroneManager.Instance != null)
		{
			DroneManager.Instance.Update();
		}
		if (DismembermentManager.Instance != null)
		{
			DismembermentManager.Instance.Update();
		}
		if (TurretTracker.Instance != null)
		{
			TurretTracker.Instance.Update();
		}
		if ((bool)RaycastPathManager.Instance)
		{
			RaycastPathManager.Instance.Update();
		}
		if (TokenManager.Instance != null)
		{
			TokenManager.Instance.Update();
		}
		TrajectorySimulation.UpdateSimulationQueue();
		if (FactionManager.Instance != null)
		{
			FactionManager.Instance.Update();
		}
		if (NavObjectManager.HasInstance)
		{
			NavObjectManager.Instance.Update();
		}
		if (BlockedPlayerList.Instance != null)
		{
			BlockedPlayerList.Instance.Update();
		}
		PrefabEditModeManager.Instance?.Update();
		triggerEffectManager?.Update();
		SpeedTreeWindHistoryBufferManager.Instance.Update();
		ThreadManager.UpdateMainThreadTasks();
		if (!IsDedicatedServer)
		{
			string _message = default(string);
			if (XUiC_MainMenu.openedOnce && !isQuitting && PlatformManager.CrossplatformPlatform?.AntiCheatClient?.GetUnhandledViolationMessage(out _message) == true)
			{
				GUIWindowManager gUIWindowManager = LocalPlayerUI.primaryUI.windowManager;
				if (gUIWindowManager != null)
				{
					string title = "EAC: " + Localization.Get("eacIntegrityViolation");
					_message = (string.IsNullOrEmpty(_message) ? "" : (_message + "\n"));
					_message += Localization.Get("eacUnableToPlayOnProtected");
					XUiC_MessageBoxWindowGroup.ShowOk(gUIWindowManager.playerUI.xui, title, _message, null, !gameStateManager.IsGameStarted());
				}
			}
			if (!bCursorVisibleOverride && !isQuitting)
			{
				bool flag = isAnyCursorWindowOpen();
				if (GameIsFocused && bCursorVisible != flag)
				{
					setCursorEnabled(flag);
				}
				if (!flag && Cursor.visible && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
				{
					setCursorEnabled(_e: false);
				}
			}
			UpdateFPSCap();
		}
		lock (((ICollection)tileEntitiesMusicToRemove).SyncRoot)
		{
			for (int i = 0; i < tileEntitiesMusicToRemove.Count; i++)
			{
				UnityEngine.Object.Destroy(tileEntitiesMusicToRemove[i]);
			}
		}
		if (!gameStateManager.IsGameStarted())
		{
			GameTimer.Instance.Reset(GameTimer.Instance.ticks);
			return;
		}
		m_World.entityAsyncManager.Update();
		GameTimer.Instance.updateTimer(IsDedicatedServer && m_World.Players.Count == 0);
		updateBlockParticles();
		updateTimeOfDay();
		Manager.FrameUpdate();
		WaterSimulationNative.Instance.Update();
		SignTextureManager.Instance.MainThreadUpdate();
		WaterEvaporationManager.UpdateEvaporation();
		if (GameTimer.Instance.elapsedTicks > 0 || m_World.m_ChunkManager.IsForceUpdate() || m_World.Players.list.Count == 0)
		{
			m_World.m_ChunkManager.DetermineChunksToLoad();
		}
		if (IsDedicatedServer && m_World.Players.list.Count == 0 && lastPlayerCount > 0)
		{
			timeToClearAllPools = 8f;
		}
		lastPlayerCount = m_World.Players.list.Count;
		if (m_World.Players.list.Count == 0 && timeToClearAllPools > 0f && (timeToClearAllPools -= Time.deltaTime) <= 0f)
		{
			Log.Out("Clearing all pools");
			MemoryPools.Cleanup();
			m_World.ClearCaches();
		}
		if (!UpdateTick())
		{
			return;
		}
		m_World.m_ChunkManager.GroundAlignFrameUpdate();
		int num = (IsDedicatedServer ? 25000 : 2500);
		swCopyChunks.ResetAndRestart();
		while (m_World.m_ChunkManager.CopyChunksToUnity() && swCopyChunks.ElapsedMicroseconds < num)
		{
		}
		if (prefabLODManager != null)
		{
			prefabLODManager.FrameUpdate();
		}
		ExplodeGroupFrameUpdate();
		fpsCountdownTimer -= Time.deltaTime;
		if (fpsCountdownTimer <= 0f)
		{
			fpsCountdownTimer = 30f;
			MaxMemoryConsumption = Math.Max(GC.GetTotalMemory(forceFullCollection: false), MaxMemoryConsumption);
			if (!IsDedicatedServer || SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0 || lastStatsPlayerCount > 0)
			{
				lastStatsPlayerCount = SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount();
				Log.Out(ConsoleCmdMem.GetStats(_bDoGc: false, this));
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			m_World.ChunkCache.ChunkProvider.Update();
			wsCountdownTimer -= Time.deltaTime;
			if (wsCountdownTimer <= 0f)
			{
				wsCountdownTimer = 30f;
				if (!isEditMode)
				{
					m_World.SaveWorldState();
					if (Block.nameIdMapping != null)
					{
						Block.nameIdMapping.SaveIfDirty();
					}
					if (ItemClass.nameIdMapping != null)
					{
						ItemClass.nameIdMapping.SaveIfDirty();
					}
					m_World.ChunkCache.ChunkProvider.GetEventPrefabs()?.Save();
				}
			}
			playerPositionsCountdownTimer -= Time.deltaTime;
			if (playerPositionsCountdownTimer <= 0f)
			{
				playerPositionsCountdownTimer = 6f;
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerPositions>().Setup(persistentPlayers), _onlyClientsAttachedToAnEntity: false, -1, -1, -1, null, 192, _onlyClientsNotAttachedToAnEntity: true);
				}
			}
		}
		if (IsDedicatedServer)
		{
			gcCountdownTimer -= Time.deltaTime;
			if (gcCountdownTimer <= 0f)
			{
				gcCountdownTimer = 120f;
				GC.Collect();
			}
			if ((float)swUpdateTime.ElapsedMilliseconds > 50f)
			{
				waitForTargetFPS.SkipSleepThisFrame = true;
			}
		}
		else
		{
			GameSenseManager.Instance?.Update();
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && countdownSaveLocalPlayerDataFile.HasPassed())
			{
				countdownSaveLocalPlayerDataFile.ResetAndRestart();
				SaveLocalPlayerData();
			}
			unloadAssetsDuration += Time.deltaTime;
			if (unloadAssetsDuration > 1200f)
			{
				bool flag2 = unloadAssetsDuration > 3600f;
				if (!isAnyModalWindowOpen())
				{
					isUnloadAssetsReady = true;
				}
				else if (isUnloadAssetsReady)
				{
					flag2 = true;
				}
				if (flag2)
				{
					stopwatchUnloadAssets.ResetAndRestart();
					Resources.UnloadUnusedAssets();
					stopwatchUnloadAssets.Stop();
					Log.Out("UnloadUnusedAssets after {0} m, took {1} ms", unloadAssetsDuration / 60f, stopwatchUnloadAssets.ElapsedMilliseconds);
					unloadAssetsDuration = 0f;
					isUnloadAssetsReady = false;
				}
			}
		}
		if (stabilityViewer != null)
		{
			stabilityViewer.Update();
		}
		ModEvents.SGameUpdateData _data2 = default(ModEvents.SGameUpdateData);
		ModEvents.GameUpdate.Invoke(ref _data2);
		GameObjectPool.Instance.FrameUpdate();
	}

	public void LateUpdate()
	{
		ThreadManager.LateUpdate();
		PlatformManager.LateUpdate();
		if (m_World != null && m_World.aiDirector != null)
		{
			m_World.aiDirector.DebugFrameLateUpdate();
		}
		UpdateMultiplayerServices();
		MeshDataManager.Instance.LateUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UpdateTick()
	{
		GameTimer instance = GameTimer.Instance;
		if (instance.elapsedTicks <= 0 && m_World.Players.list.Count != 0)
		{
			m_World.TickEntitiesSlice();
			return true;
		}
		m_World.TickEntitiesFlush();
		float partialTicks = (Time.time - lastTime) * 20f;
		lastTime = Time.time;
		m_World.OnUpdateTick(partialTicks, m_World.m_ChunkManager.GetActiveChunkSet());
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !gameStateManager.OnUpdateTick())
		{
			return false;
		}
		m_World.TickEntities(partialTicks);
		m_World.LetBlocksFall();
		if (!IsDedicatedServer)
		{
			m_World.SetEntitiesVisibleNearToLocalPlayer();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			m_World.entityDistributer.OnUpdateEntities();
			m_World.m_ChunkManager.SendChunksToClients();
			if (bSavingActive)
			{
				ChunkCluster chunkCache = m_World.ChunkCache;
				if (chunkCache?.ChunkProvider is ChunkProviderGenerateWorld chunkProviderGenerateWorld)
				{
					chunkProviderGenerateWorld.MainThreadCacheProtectedPositions();
				}
				if (instance.ticks % 40 == 0L)
				{
					chunkCache?.ChunkProvider.SaveRandomChunks(2, instance.ticks, m_World.m_ChunkManager.GetActiveChunkSet());
				}
				else if (Time.time - lastTimeDecoSaved > 60f)
				{
					lastTimeDecoSaved = Time.time;
					m_World.SaveDecorations();
					chunkCache?.ChunkProvider.GetEventPrefabs()?.Save();
				}
			}
		}
		else
		{
			updateSendClientPlayerPositionToServer();
		}
		if (lastTime - activityCheck >= 1f)
		{
			PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.InGame);
			activityCheck = lastTime;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnNetworkStateChanged(bool connectionState)
	{
		if (!connectionState)
		{
			ShutdownMultiplayerServices(EMultiShutReason.AppNoNetwork);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationResume()
	{
		PlatformApplicationManager.SetRestartRequired();
		if (!IsSafeToConnect())
		{
			ShutdownMultiplayerServices(EMultiShutReason.AppSuspended);
		}
		else
		{
			ThreadManager.StartCoroutine(PlatformApplicationManager.CheckRestartCoroutine());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateMultiplayerServices()
	{
		if (IsDedicatedServer || shuttingDownMultiplayerServices || IsSafeToConnect() || !IsSafeToDisconnect())
		{
			return;
		}
		ConnectionManager instance = SingletonMonoBehaviour<ConnectionManager>.Instance;
		if (instance == null)
		{
			return;
		}
		ProtocolManager.NetworkType currentMode = instance.CurrentMode;
		if (currentMode != ProtocolManager.NetworkType.None && currentMode != ProtocolManager.NetworkType.OfflineServer)
		{
			EUserPerms permissions = PermissionsManager.GetPermissions();
			if (!permissions.HasMultiplayer())
			{
				ShutdownMultiplayerServices(EMultiShutReason.PermMissingMultiplayer);
			}
			else if ((SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient ? SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo).AllowsCrossplay && !permissions.HasCrossplay())
			{
				ShutdownMultiplayerServices(EMultiShutReason.PermMissingCrossplay);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalizationKey(EMultiShutReason _reason)
	{
		return _reason switch
		{
			EMultiShutReason.AppNoNetwork => "app_noNetwork", 
			EMultiShutReason.AppSuspended => "app_suspended", 
			EMultiShutReason.PermMissingMultiplayer => "permMissing_multiplayer", 
			EMultiShutReason.PermMissingCrossplay => "permMissing_crossplay", 
			_ => throw new ArgumentOutOfRangeException("_reason", _reason, string.Format("Unknown Localization for {0}.{1}", "EMultiShutReason", _reason)), 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShutdownMultiplayerServices(EMultiShutReason _reason)
	{
		ThreadManager.StartCoroutine(ShutdownMultiplayerServicesCoroutine(_reason));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ShutdownMultiplayerServicesCoroutine(EMultiShutReason _reason)
	{
		yield return null;
		if (IsDedicatedServer || shuttingDownMultiplayerServices)
		{
			yield break;
		}
		shuttingDownMultiplayerServices = true;
		bool isClient = false;
		bool success = false;
		bool failReasonProvided = false;
		try
		{
			Log.Out($"Waiting to Shut Down Multiplayer Services ({_reason})...");
			while (shuttingDownMultiplayerServices && !IsSafeToConnect() && !IsSafeToDisconnect())
			{
				yield return null;
			}
			ConnectionManager connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			while (true)
			{
				yield return null;
				if (!shuttingDownMultiplayerServices)
				{
					Log.Warning($"Cancelled Shutting Down Multiplayer Services ({_reason}) because already shutting down.");
					failReasonProvided = true;
					yield break;
				}
				if (IsSafeToConnect())
				{
					Log.Warning($"Cancelled Shutting Down Multiplayer Services ({_reason}) because safe to connect.");
					failReasonProvided = true;
					yield break;
				}
				if (!(connectionManager == null))
				{
					ProtocolManager.NetworkType currentMode = connectionManager.CurrentMode;
					if (currentMode != ProtocolManager.NetworkType.None && currentMode != ProtocolManager.NetworkType.OfflineServer)
					{
						if (IsSafeToDisconnect())
						{
							break;
						}
						continue;
					}
				}
				Log.Warning($"Cancelled Shutting Down Multiplayer Services ({_reason}) because no online connection.");
				failReasonProvided = true;
				yield break;
			}
			Log.Out($"Shutting Down Multiplayer Services ({_reason})...");
			if (connectionManager.IsClient)
			{
				Disconnect();
				isClient = true;
				success = true;
				yield break;
			}
			ClientInfo[] clientInfos = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List.ToArray();
			if (clientInfos.Length != 0)
			{
				NetPackagePlayerDenied package = NetPackageManager.GetPackage<NetPackagePlayerDenied>().Setup(new GameUtils.KickPlayerData(GameUtils.EKickReason.SessionClosed));
				ClientInfo[] array = clientInfos;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SendPackage(package);
				}
				yield return new WaitForSecondsRealtime(1f);
				array = clientInfos;
				foreach (ClientInfo clientInfo in array)
				{
					try
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectClient(clientInfo);
					}
					catch (Exception arg)
					{
						Log.Warning($"Failed to disconnect client '{clientInfo.playerName}' : {arg}");
					}
				}
			}
			ShutdownMultiplayerServicesNow();
			connectionManager.MakeServerOffline();
			GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, 1);
			success = true;
		}
		finally
		{
			GameManager gameManager = this;
			gameManager.shuttingDownMultiplayerServices = false;
			if (success)
			{
				Log.Out($"Multiplayer Services ({_reason}) have been shut down.");
				string title = Localization.Get(isClient ? "multiShut_titleClient" : "multiShut_titleHost");
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat(Localization.Get("auth_reason"), Localization.Get(GetLocalizationKey(_reason)));
				if (!isClient)
				{
					stringBuilder.Append('\n');
					stringBuilder.Append(Localization.Get("multiShut_commonHost"));
				}
				XUiC_MessageBoxWindowGroup.ShowOk(gameManager.windowManager.playerUI.xui, title, stringBuilder.ToString(), gameManager.OnShutdownMultiplayerServicesMessageBoxClosed);
			}
			else if (!failReasonProvided)
			{
				Log.Warning($"Failed Shutting Down Multiplayer Services ({_reason}).");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnShutdownMultiplayerServicesMessageBoxClosed()
	{
		if (World != null)
		{
			Pause(_bOn: false);
		}
	}

	public void CreateStabilityViewer()
	{
		if (stabilityViewer == null)
		{
			stabilityViewer = new StabilityViewer();
		}
	}

	public void ClearStabilityViewer()
	{
		if (stabilityViewer != null)
		{
			stabilityViewer.worldIsReady = false;
			stabilityViewer.Clear();
			stabilityViewer = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLocalPlayerEntity(EntityPlayerLocal _playerEntity)
	{
		_playerEntity.IsFlyMode.Value = IsEditMode();
		_playerEntity.SetEntityName(GamePrefs.GetString(EnumGamePrefs.PlayerName));
		myPlayerId = _playerEntity.entityId;
		myEntityPlayerLocal = _playerEntity;
		persistentLocalPlayer = getPersistentPlayerData(null);
		_playerEntity.persistentPlayerData = persistentLocalPlayer;
		_playerEntity.InventoryChangedEvent += LocalPlayerInventoryChanged;
		_playerEntity.inventory.OnToolbeltItemsChangedInternal += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			sendPlayerToolbelt = true;
		};
		_playerEntity.bag.OnBackpackItemsChangedInternal += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			sendPlayerBag = true;
		};
		_playerEntity.equipment.OnChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			sendPlayerEquipment = true;
		};
		_playerEntity.DragAndDropItemChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			sendDragAndDropItem = true;
		};
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && persistentPlayers != null)
		{
			if (persistentLocalPlayer == null)
			{
				persistentLocalPlayer = persistentPlayers.CreatePlayerData(getPersistentPlayerID(null), PlatformManager.NativePlatform.User.PlatformUserId, _playerEntity.EntityName, DeviceFlag.StandaloneWindows.ToPlayGroup());
				persistentLocalPlayer.EntityId = myPlayerId;
				persistentPlayers.MapPlayer(persistentLocalPlayer);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(persistentLocalPlayer, EnumPersistentPlayerDataReason.New), _onlyClientsAttachedToAnEntity: true);
				persistentPlayers.SavePersistentPlayerData();
			}
			else
			{
				persistentLocalPlayer.Update(PlatformManager.NativePlatform.User.PlatformUserId, new AuthoredText(_playerEntity.EntityName, persistentLocalPlayer.PrimaryId), DeviceFlag.StandaloneWindows.ToPlayGroup());
				persistentLocalPlayer.EntityId = myPlayerId;
				persistentPlayers.MapPlayer(persistentLocalPlayer);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(persistentLocalPlayer, EnumPersistentPlayerDataReason.Login), _onlyClientsAttachedToAnEntity: true);
			}
		}
		m_World.SetLocalPlayer(_playerEntity);
		LocalPlayerUI.DispatchNewPlayerForUI(_playerEntity);
		if (this.OnLocalPlayerChanged != null)
		{
			this.OnLocalPlayerChanged(_playerEntity);
		}
		GameSenseManager.Instance?.SessionStarted(_playerEntity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StartAsServer(bool _offline)
	{
		while (XUiC_WorldGenerationWindow.WorldBuilderIsGenerating)
		{
			yield return null;
		}
		Log.Out("StartAsServer");
		if (!SandboxOptionManager.HasInstance)
		{
			Log.Warning("Sandbox Option Manager not initialized before starting server, this may cause issues with some settings such as crossplay");
		}
		SandboxOptionManager.Current.LoadOptionsFromCode(GamePrefs.GetString(EnumGamePrefs.SandboxCode));
		gameStateManager.InitGame(_bServer: true);
		SandboxOptionManager.Current.UpdateInGameValuesWithSandboxOptions(forceLoad: true);
		GameServerInfo.PrepareLocalServerInfo();
		CalculatePersistentPlayerCount(GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameName), (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType));
		PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.Loading);
		XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, Localization.Get("uiLoadLoadingXml"));
		yield return null;
		WorldStaticData.Cleanup(null);
		Block.nameIdMapping = null;
		ItemClass.nameIdMapping = null;
		string text = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		if (!text.Equals("Empty") && !text.Equals("Playtesting"))
		{
			string path = GameIO.GetSaveGameDir() + "/main.ttw";
			string text2 = GameIO.GetSaveGameDir() + "/" + Constants.cFileBlockMappings;
			string text3 = GameIO.GetSaveGameDir() + "/" + Constants.cFileItemMappings;
			if (!SdFile.Exists(path))
			{
				if (!SdDirectory.Exists(GameIO.GetSaveGameDir()))
				{
					SdDirectory.CreateDirectory(GameIO.GetSaveGameDir());
				}
				Block.nameIdMapping = new NameIdMapping(text2, Block.MAX_BLOCKS);
				Block.nameIdMapping.WriteToFile();
				ItemClass.nameIdMapping = new NameIdMapping(text3, ItemClass.MAX_ITEMS);
				ItemClass.nameIdMapping.WriteToFile();
			}
			else
			{
				Block.nameIdMapping = new NameIdMapping(text2, Block.MAX_BLOCKS);
				if (!Block.nameIdMapping.LoadFromFile())
				{
					Log.Warning("Could not load block-name-mappings file '" + text2 + "'!");
					Block.nameIdMapping = null;
				}
				ItemClass.nameIdMapping = new NameIdMapping(text3, ItemClass.MAX_ITEMS);
				if (!ItemClass.nameIdMapping.LoadFromFile())
				{
					Log.Warning("Could not load item-name-mappings file '" + text3 + "'!");
					ItemClass.nameIdMapping = null;
				}
			}
		}
		yield return WorldStaticData.LoadAllXmlsCo(_isStartup: false, null);
		yield return null;
		SingletonMonoBehaviour<ConnectionManager>.Instance.ServerReady();
		Manager.CreateServer();
		LightManager.CreateServer();
		yield return null;
		PowerManager.Instance.LoadPowerManager();
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadCreatingWorld"));
		yield return null;
		if (isEditMode)
		{
			persistentPlayers = new PersistentPlayerList();
		}
		else
		{
			persistentPlayers = PersistentPlayerList.ReadXML(GameIO.GetSaveGameDir() + "/players.xml");
			if (persistentPlayers != null && persistentPlayers.CleanupPlayers())
			{
				persistentPlayers.SavePersistentPlayerData();
			}
		}
		PathAbstractions.Contextual.FindActiveWorld(out var _name, out var _location);
		yield return createWorld(_name, _location, GamePrefs.GetString(EnumGamePrefs.GameName));
		GameServerInfo.SetLocalServerWorldInfo();
		NetPackageWorldInfo.PrepareWorldHashes();
		yield return null;
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadCreatingPlayer"));
		yield return null;
		if (!IsDedicatedServer)
		{
			string persistentPlayerId = getPersistentPlayerID(null).CombinedString;
			if (!GamePrefs.GetBool(EnumGamePrefs.SkipSpawnButton) && !IsEditMode())
			{
				canSpawnPlayer = false;
				bool firstTimeSpawn = !PlayerDataFile.Exists(GameIO.GetPlayerDataDir(), persistentPlayerId);
				XUiC_SpawnSelectionWindow.Open(LocalPlayerUI.primaryUI, _chooseSpawnPosition: false, _enteringGame: true, firstTimeSpawn);
				while (!canSpawnPlayer)
				{
					yield return null;
				}
				yield return new WaitForSeconds(0.1f);
			}
			PlayerDataFile playerDataFile = new PlayerDataFile();
			playerDataFile.Load(GameIO.GetPlayerDataDir(), persistentPlayerId);
			EntityCreationData entityCreationData = new EntityCreationData();
			Vector3 pos;
			Vector3 rot;
			int num;
			if (playerDataFile.bLoaded)
			{
				pos = playerDataFile.ecd.pos;
				rot = new Vector3(playerDataFile.ecd.rot.x, playerDataFile.ecd.rot.y, 0f);
				if (isEditMode)
				{
					playerDataFile.id = -1;
				}
				num = ((playerDataFile.id != -1) ? playerDataFile.id : EntityFactory.nextEntityID++);
				entityCreationData.entityData = playerDataFile.ecd.entityData;
				entityCreationData.readFileVersion = playerDataFile.ecd.readFileVersion;
			}
			else
			{
				SpawnPosition randomSpawnPosition = GetSpawnPointList().GetRandomSpawnPosition(m_World);
				if (m_World.IsRandomWorld())
				{
					DynamicPrefabDecorator dynamicPrefabDecorator = GetDynamicPrefabDecorator();
					if (dynamicPrefabDecorator != null)
					{
						PrefabInstance closestPOIToWorldPos = dynamicPrefabDecorator.GetClosestPOIToWorldPos(QuestEventManager.traderTag, Vector3.zero, null, -1, ignoreCurrentPOI: false, BiomeFilterTypes.OnlyBiome, BiomeDefinition.BiomeNames[3], "traderquest");
						if (closestPOIToWorldPos != null)
						{
							randomSpawnPosition = GetSpawnPointList().GetRandomSpawnPosition(m_World, closestPOIToWorldPos.GetAABB().center, 250, 750);
							Vector3.Distance(randomSpawnPosition.position, closestPOIToWorldPos.GetAABB().center);
						}
					}
				}
				pos = randomSpawnPosition.position;
				rot = new Vector3(0f, randomSpawnPosition.heading, 0f);
				num = EntityFactory.nextEntityID++;
			}
			if (playerDataFile.bLoaded && playerDataFile.ecd.playerProfile != null && GamePrefs.GetBool(EnumGamePrefs.PersistentPlayerProfiles))
			{
				entityCreationData.entityClass = EntityClass.FromString(playerDataFile.ecd.playerProfile.EntityClassName);
				entityCreationData.playerProfile = playerDataFile.ecd.playerProfile;
			}
			else
			{
				entityCreationData.playerProfile = PlayerProfile.LoadLocalProfile();
				entityCreationData.entityClass = EntityClass.FromString(entityCreationData.playerProfile.EntityClassName);
			}
			entityCreationData.skinTexture = GamePrefs.GetString(EnumGamePrefs.OptionsPlayerModelTexture);
			entityCreationData.id = num;
			entityCreationData.pos = pos;
			entityCreationData.rot = rot;
			entityCreationData.belongsPlayerId = num;
			EntityPlayerLocal entityPlayerLocal = (EntityPlayerLocal)EntityFactory.CreateEntity(entityCreationData);
			setLocalPlayerEntity(entityPlayerLocal);
			if (playerDataFile.bLoaded)
			{
				playerDataFile.ToPlayer(entityPlayerLocal);
				entityPlayerLocal.bPreferFirstPerson = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxDefaultFirstPersonCamera);
				entityPlayerLocal.SetFirstPersonView(_bFirstPersonView: true, _bLerpPosition: false);
			}
			m_World.SpawnEntityInWorld(entityPlayerLocal);
			myEntityPlayerLocal.Respawn(playerDataFile.bLoaded ? RespawnType.LoadedGame : RespawnType.NewGame);
			myEntityPlayerLocal.ChunkObserver = m_World.m_ChunkManager.AddChunkObserver(myEntityPlayerLocal.GetPosition(), _bBuildVisualMeshAround: true, Utils.FastMin(12, GameUtils.GetViewDistance()), -1);
			IMapChunkDatabase.TryCreateOrLoad(myEntityPlayerLocal.entityId, out myEntityPlayerLocal.ChunkObserver.mapDatabase, [PublicizedFrom(EAccessModifier.Internal)] () => new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataDir(), persistentPlayerId));
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
			uIForPlayer.xui.SetDataConnections();
			uIForPlayer.xui.SetCraftingData(playerDataFile.craftingData);
		}
		Log.Out("Loaded player");
		yield return null;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			VehicleManager.Init();
			DroneManager.Init();
			TurretTracker.Init();
			RaycastPathManager.Init();
			TokenManager.Init();
			BlockLimitTracker.Init();
			if (m_World.ChunkCache.ChunkProvider is ChunkProviderGenerateWorld chunkProviderGenerateWorld)
			{
				chunkProviderGenerateWorld.CheckPersistentData();
			}
		}
		yield return null;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && m_World.ChunkCache.IsFixedSize && !IsEditMode() && m_World.m_WorldEnvironment != null)
		{
			m_World.m_WorldEnvironment.SetColliders((m_World.ChunkCache.ChunkMinPos.x + 1) * 16, (m_World.ChunkCache.ChunkMinPos.y + 1) * 16, (m_World.ChunkCache.ChunkMaxPos.x - m_World.ChunkCache.ChunkMinPos.x - 1) * 16, (m_World.ChunkCache.ChunkMaxPos.y - m_World.ChunkCache.ChunkMinPos.y - 1) * 16, Constants.cSizePlanesAround, 0f);
			m_World.m_WorldEnvironment.CreateLevelBorderBox(m_World);
		}
		if (isEditMode)
		{
			PrefabEditModeManager.Instance?.Init();
			yield return null;
		}
		yield return null;
		if (IsDedicatedServer || !_offline)
		{
			ServerInformationTcpProvider.Instance.StartServer();
			PlatformManager.MultiPlatform.ServerListAnnouncer?.AdvertiseServer([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PlatformManager.NativePlatform.LobbyHost?.UpdateLobby(SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo);
				ModEvents.SServerRegisteredData _data = default(ModEvents.SServerRegisteredData);
				ModEvents.ServerRegistered.Invoke(ref _data);
			});
			PlayerInteractions.Instance.JoinedMultiplayerServer(persistentPlayers);
			AuthorizationManager.Instance.ServerStart();
		}
		else
		{
			GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, 1);
		}
		yield return GCUtils.UnloadAndCollectCo();
		LogServerStartEventAnalytics(_offline);
		gameStateManager.StartGame();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogServerStartEventAnalytics(bool isOffline)
	{
		SandboxOptionManager current = SandboxOptionManager.Current;
		SandboxOptionPreset preset = current.GetPreset(GamePrefs.GetString(EnumGamePrefs.SandboxPreset));
		current.GetChangedPresetOptions(preset, out List<(string, string, bool)> valuesList);
		Dictionary<string, object> generalSettings = new Dictionary<string, object>
		{
			{
				"Group",
				preset?.Group
			},
			{
				"Preset",
				preset?.Name
			},
			{
				"Server_Enabled",
				GamePrefs.GetBool(EnumGamePrefs.ServerEnabled)
			},
			{
				"Region",
				GamePrefs.GetString(EnumGamePrefs.Region)
			},
			{
				"Server_Visibility",
				((EnumServerVisibility)GamePrefs.GetInt(EnumGamePrefs.ServerVisibility)/*cast due to .constrained prefix*/).ToString()
			},
			{
				"EAC_Protected",
				GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer)
			},
			{
				"Crossplay_Enabled",
				GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay)
			},
			{
				"Max_Players",
				GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount)
			},
			{
				"Chunk_Reset_Timer",
				GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge)
			},
			{
				"Reset_Unprotected_Chunks",
				((EnumResetUnprotectedChunksGroupingMode)GamePrefs.GetInt(EnumGamePrefs.ResetUnprotectedChunks)/*cast due to .constrained prefix*/).ToString()
			},
			{
				"Creative_Mode",
				GamePrefs.GetBool(EnumGamePrefs.BuildCreate)
			},
			{
				"Persistent_Profiles",
				GamePrefs.GetBool(EnumGamePrefs.PersistentPlayerProfiles)
			},
			{
				"Restrict_Camera_Mode",
				((EnumCameraRestriction)GamePrefs.GetInt(EnumGamePrefs.CameraRestrictionMode)/*cast due to .constrained prefix*/).ToString()
			},
			{
				"Player_Killing",
				((EnumPlayerKillingMode)GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode)/*cast due to .constrained prefix*/).ToString()
			},
			{
				"Claim_Size",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimSize)
			},
			{
				"Claim_Deadzone",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimDeadZone)
			},
			{
				"Claim_Duration",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimExpiryTime)
			},
			{
				"Claim_Decay_Mode",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimDecayMode)
			},
			{
				"Claim_Health_Online",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimOnlineDurabilityModifier)
			},
			{
				"Claim_Health_Offline",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDurabilityModifier)
			},
			{
				"Bedroll_Deadzone",
				GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize)
			},
			{
				"Bedroll_Duration",
				GamePrefs.GetInt(EnumGamePrefs.BedrollExpiryTime)
			},
			{
				"Party_Shared_Kill_Range",
				GamePrefs.GetInt(EnumGamePrefs.PartySharedKillRange)
			}
		};
		ServerStartEventData analyticsEventData = new ServerStartEventData
		{
			ServerId = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient?.GetValue(GameInfoString.UniqueId),
			SaveId = World.Guid,
			ServerStartTimestamp = DateTime.UtcNow.ToString("o"),
			ServerType = (IsDedicatedServer ? "Dedicated" : (isOffline ? "Offline" : "Listen")),
			SandboxSeed = GameStats.GetString(EnumGameStats.SandboxCode),
			SandboxSettingsDelta = valuesList,
			WorldName = GamePrefs.GetString(EnumGamePrefs.GameWorld),
			GeneralSettings = generalSettings,
			ServerMods = ModManager.GetLoadedMods()
		};
		ServiceProvider.Instance.Get<IAnalyticsService>().LogEvent(analyticsEventData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartAsClient()
	{
		Log.Out("StartAsClient");
		worldCreated = false;
		chunkClusterLoaded = false;
		worldInitInfoReceived = false;
		GamePrefs.Set(EnumGamePrefs.GameMode, string.Empty);
		GamePrefs.Set(EnumGamePrefs.GameWorld, string.Empty);
		WorldStaticData.WaitForConfigsFromServer();
		PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.Connecting);
		IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
		if (antiCheatClient == null || !antiCheatClient.ClientAntiCheatEnabled())
		{
			Log.Out("Sending RequestToEnterGame...");
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRequestToEnterGame>());
		}
		else
		{
			PlatformManager.MultiPlatform.AntiCheatClient.WaitForRemoteAuth([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				Log.Out("Sending RequestToEnterGame...");
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRequestToEnterGame>());
			});
		}
		BlockLimitTracker.Init();
	}

	public bool IsSafeToConnect()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.None)
		{
			return true;
		}
		return false;
	}

	public bool IsSafeToDisconnect()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.None)
		{
			return true;
		}
		if (PrefabEditModeManager.Instance.IsActive() && PrefabEditModeManager.Instance.NeedsSaving)
		{
			return false;
		}
		if (gameStateManager.IsGameStarted() && !IsStartingGame)
		{
			return !isDisconnectingLater;
		}
		return false;
	}

	public void Disconnect()
	{
		Log.Out("Disconnect");
		if (!IsDedicatedServer)
		{
			windowManager.CloseAllOpenModalWindows();
			if (m_World != null)
			{
				List<EntityPlayerLocal> localPlayers = m_World.GetLocalPlayers();
				for (int i = 0; i < localPlayers.Count; i++)
				{
					LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(localPlayers[i]);
					if (null != uIForPlayer && null != uIForPlayer.windowManager)
					{
						uIForPlayer.windowManager.CloseAllOpenModalWindows();
						uIForPlayer.xui.gameObject.SetActive(value: false);
					}
				}
			}
			LocalPlayerUI.primaryUI.windowManager.Close(XUiC_SubtitlesDisplay.ID);
			Manager.StopAllLocal();
		}
		Pause(_bOn: false);
		if (!IsDedicatedServer)
		{
			if (!isEditMode && (bool)myEntityPlayerLocal)
			{
				GameSenseManager.Instance?.SessionEnded();
				if ((bool)myEntityPlayerLocal.AttachedToEntity)
				{
					myEntityPlayerLocal.Detach();
				}
				myEntityPlayerLocal.FireEvent(MinEventTypes.onSelfLeaveGame);
				myEntityPlayerLocal.dropItemOnQuit();
			}
			triggerEffectManager.StopGamepadVibration(clearSources: true);
			PlatformManager.MultiPlatform.User.StopAdvertisePlaying();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerDisconnect>().Setup(myEntityPlayerLocal), _flush: true);
			StartCoroutine(disconnectLater());
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.StopServers();
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectFromServer();
		}
		if (GameSparksManager.Instance() != null)
		{
			GameSparksManager.Instance().SessionEnded();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator disconnectLater()
	{
		isDisconnectingLater = true;
		yield return new WaitForSeconds(0.2f);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
		GamePrefs.Set(EnumGamePrefs.GameGuidClient, "");
		isDisconnectingLater = false;
	}

	public void SaveAndCleanupWorld()
	{
		Log.Out("SaveAndCleanupWorld");
		m_World?.entityAsyncManager?.CompletePendingCreateTasks();
		ModEvents.SWorldShuttingDownData _data = default(ModEvents.SWorldShuttingDownData);
		ModEvents.WorldShuttingDown.Invoke(ref _data);
		shuttingDownMultiplayerServices = false;
		PathAbstractions.CacheEnabled = false;
		this.OnClientSpawned = null;
		PlayerInputRecordingSystem.Instance.AutoSave();
		gameStateManager.EndGame();
		PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.Menu);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && bSavingActive && !IsEditMode())
		{
			if (VehicleManager.Instance != null)
			{
				VehicleManager.Instance.RemoveAllVehiclesFromMap();
			}
			if (DroneManager.Instance != null)
			{
				DroneManager.Instance.RemoveAllDronesFromMap();
			}
			if (QuestEventManager.HasInstance)
			{
				QuestEventManager.Current.HandleAllPlayersDisconnect();
			}
			SaveLocalPlayerData();
			SaveWorld();
			EntityPlayerLocal entityPlayerLocal = m_World?.GetPrimaryPlayer();
			if (persistentPlayers != null)
			{
				foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in persistentPlayers.Players)
				{
					if (player.Value.EntityId != -1)
					{
						if ((bool)entityPlayerLocal && player.Value.EntityId == entityPlayerLocal.entityId)
						{
							player.Value.Position = new Vector3i(entityPlayerLocal.position);
						}
						player.Value.LastLogin = DateTime.Now;
					}
				}
				persistentPlayers.SavePersistentPlayerData();
			}
		}
		if (Block.nameIdMapping != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Block.nameIdMapping.SaveIfDirty();
			}
			Block.nameIdMapping = null;
		}
		if (ItemClass.nameIdMapping != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ItemClass.nameIdMapping.SaveIfDirty();
			}
			ItemClass.nameIdMapping = null;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && bSavingActive && !IsEditMode())
		{
			if (m_World != null && m_World.GetPrimaryPlayer() != null && m_World.GetPrimaryPlayer().ChunkObserver.mapDatabase != null)
			{
				ThreadManager.AddSingleTask(m_World.GetPrimaryPlayer().ChunkObserver.mapDatabase.SaveAsync, new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataLocalDir(), persistentLocalPlayer.PrimaryId.CombinedString));
			}
			if (!IsDedicatedServer && m_World != null)
			{
				foreach (EntityPlayerLocal localPlayer in m_World.GetLocalPlayers())
				{
					localPlayer.EnableCamera(_b: false);
					localPlayer.SetControllable(_b: false);
				}
			}
		}
		ShutdownMultiplayerServicesNow();
		PlayerInteractions.Instance.OnNewPlayerInteraction -= HandleFirstSpawnInteractions;
		PlayerInteractions.Instance.Shutdown();
		PlatformManager.NativePlatform.GameplayNotifier?.GameplayEnd();
		if (!IsDedicatedServer)
		{
			if (myEntityPlayerLocal != null)
			{
				myEntityPlayerLocal.EnableCamera(_b: false);
				myEntityPlayerLocal.SetControllable(_b: false);
				if (this.OnLocalPlayerChanged != null)
				{
					this.OnLocalPlayerChanged(null);
				}
				m_World.RemoveEntity(myPlayerId, EnumRemoveEntityReason.Unloaded);
				myPlayerId = -1;
				myEntityPlayerLocal = null;
			}
			foreach (LocalPlayerUI playerUI in LocalPlayerUI.PlayerUIs)
			{
				if (!playerUI.isPrimaryUI && !playerUI.IsCleanCopy)
				{
					if ((bool)playerUI.entityPlayer)
					{
						playerUI.entityPlayer.EnableCamera(_b: false);
						playerUI.entityPlayer.SetControllable(_b: false);
						m_World?.RemoveEntity(playerUI.entityPlayer.entityId, EnumRemoveEntityReason.Unloaded);
					}
					if ((bool)playerUI.gameObject)
					{
						playerUI.xui.Shutdown();
						playerUI.windowManager.CloseAllOpenModalWindows();
						UnityEngine.Object.Destroy(playerUI.gameObject);
					}
				}
			}
		}
		ModManager.GameEnded();
		if (!IsDedicatedServer)
		{
			if (!PlatformApplicationManager.IsRestartRequired)
			{
				LoadRemoteResources();
			}
			GUIWindowConsole.Close();
			windowManager.Close(XUiC_LoadingScreen.ID);
			if (!bHideMainMenuNextTime)
			{
				windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
			}
			bHideMainMenuNextTime = false;
		}
		PrefabInstanceClientManager.Instance.Cleanup();
		TrajectorySimulation.Cleanup();
		TokenManager.Cleanup();
		AstarManager.Cleanup();
		DynamicMeshManager.OnWorldUnload();
		if (GameEventManager.HasInstance)
		{
			GameEventManager.Current.Cleanup();
		}
		if (m_World != null)
		{
			if (this.OnWorldChanged != null)
			{
				this.OnWorldChanged(null);
			}
			prefabLODManager.Cleanup();
			PrefabEditModeManager instance = PrefabEditModeManager.Instance;
			if (instance != null && instance.IsActive())
			{
				PrefabEditModeManager.Instance?.Cleanup();
			}
			EnvironmentAudioManager.DestroyInstance();
			LightManager.Clear();
			SkyManager.Cleanup();
			WeatherManager.Cleanup();
			CharacterGazeController.Cleanup();
			WaterSplashCubes.Clear();
			WaterEvaporationManager.ClearAll();
			SleeperVolumeToolManager.CleanUp();
			POIMarkerToolManager.CleanUp();
			ClearStabilityViewer();
			if ((bool)m_World.GetPrimaryPlayer() && m_World.GetPrimaryPlayer().DynamicMusicManager != null)
			{
				m_World.GetPrimaryPlayer().DynamicMusicManager.CleanUpDynamicMembers();
			}
			m_World.UnloadWorld(_bUnloadRespawnableEntities: true);
			m_World.Cleanup();
			m_World = null;
			GameHasStarted = false;
		}
		WaterSimulationNative.Instance.Cleanup();
		ProjectileManager.Cleanup();
		VehicleManager.Cleanup();
		DroneManager.Cleanup();
		DismembermentManager.Cleanup();
		TurretTracker.Cleanup();
		BlockLimitTracker.Cleanup();
		MapObjectManager.Reset();
		vp_TargetEventHandler.UnregisterAll();
		lootManager = null;
		traderManager = null;
		if (QuestEventManager.HasInstance)
		{
			QuestEventManager.Current.Cleanup();
		}
		if (TwitchVoteScheduler.HasInstance)
		{
			TwitchVoteScheduler.Current.Cleanup();
		}
		if (TwitchManager.HasInstance)
		{
			TwitchManager.Current.Cleanup();
		}
		if (PowerManager.HasInstance)
		{
			PowerManager.Instance.Cleanup();
		}
		if (WireManager.HasInstance)
		{
			WireManager.Instance.Cleanup();
		}
		if (PartyManager.HasInstance)
		{
			PartyManager.Current.Cleanup();
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && bSavingActive)
		{
			IsEditMode();
		}
		if (UIDisplayInfoManager.HasInstance)
		{
			UIDisplayInfoManager.Current.Cleanup();
		}
		if (TextureLoadingManager.Instance != null)
		{
			TextureLoadingManager.Instance.Cleanup();
		}
		if (NavObjectManager.HasInstance)
		{
			NavObjectManager.Instance.Cleanup();
		}
		SignTextureExporter.Instance.Cleanup();
		SignTextureManager.Instance.Cleanup();
		if (SignDataManager.HasInstance)
		{
			SignDataManager.Instance.Cleanup();
		}
		SelectionBoxManager.Instance.Clear();
		Origin.Cleanup();
		GameObjectPool.Instance.Cleanup();
		MemoryPools.Cleanup();
		VoxelMeshLayer.StaticCleanup();
		GamePrefs.Instance.Save();
		bRecordNextSession = false;
		bPlayRecordedSession = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShutdownMultiplayerServicesNow()
	{
		if (!IsDedicatedServer)
		{
			PlatformManager.MultiPlatform.User.StopAdvertisePlaying();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AuthorizationManager.Instance.ServerStop();
		}
		PlatformManager.NativePlatform.LobbyHost?.ExitLobby();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PlatformManager.MultiPlatform.ServerListAnnouncer.StopServer();
			ServerInformationTcpProvider.Instance.StopServer();
		}
		PlatformManager.NativePlatform.GameplayNotifier?.EndOnlineMultiplayer();
	}

	public void SaveWorld()
	{
		if (m_World != null)
		{
			m_World.Save();
		}
	}

	public void SaveLocalPlayerData()
	{
		if (m_World == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = m_World.GetPrimaryPlayer();
		if (!(primaryPlayer == null) && bSavingActive)
		{
			string combinedString = getPersistentPlayerID(null).CombinedString;
			PlayerDataFile playerDataFile = new PlayerDataFile();
			playerDataFile.FromPlayer(primaryPlayer);
			playerDataFile.Save(GameIO.GetPlayerDataDir(), combinedString);
			if (primaryPlayer.ChunkObserver.mapDatabase != null)
			{
				ThreadManager.AddSingleTask(primaryPlayer.ChunkObserver.mapDatabase.SaveAsync, new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataDir(), combinedString));
			}
		}
	}

	public void Cleanup()
	{
		Log.Out("Cleanup");
		WaterSimulationNative.Instance.Cleanup();
		ModEvents.SGameShutdownData _data = default(ModEvents.SGameShutdownData);
		ModEvents.GameShutdown.Invoke(ref _data);
		AuthorizationManager.Instance.Cleanup();
		VehicleManager.Cleanup();
		Cursor.visible = true;
		Cursor.lockState = SoftCursor.DefaultCursorLockState;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Cleanup();
		WorldStaticData.Cleanup();
		adminTools = null;
		GUIWindowConsole.Shutdown();
		GameObjectPool.Instance.Cleanup();
		SaveDataUtils.SaveDataManager.Cleanup();
		LocalPlayerManager.Destroy();
		PlatformManager.Destroy();
		LoadManager.Destroy();
		TaskManager.Destroy();
		MemoryPools.Cleanup();
		GC.Collect();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool OnApplicationQuit()
	{
		adminTools?.DestroyFileWatcher();
		if (!allowQuit)
		{
			if (!isQuitting)
			{
				isQuitting = true;
				StartCoroutine(ApplicationQuitCo(0.3f));
			}
			return false;
		}
		GameSenseManager.Instance?.Cleanup();
		ThreadManager.Shutdown();
		WorldStaticData.QuitCleanup();
		if (SingletonMonoBehaviour<SdtdConsole>.Instance != null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Cleanup();
		}
		Log.Out("OnApplicationQuit");
		return true;
	}

	public void OnApplicationFocus(bool _focus)
	{
		if (IsDedicatedServer)
		{
			return;
		}
		GameIsFocused = _focus;
		if (Application.isEditor)
		{
			return;
		}
		if (!_focus)
		{
			setCursorEnabled(_e: true);
		}
		else if (bCursorVisibleOverride)
		{
			setCursorEnabled(bCursorVisibleOverrideState);
		}
		else if (!isAnyCursorWindowOpen())
		{
			setCursorEnabled(_e: false);
		}
		if (ActionSetManager.DebugLevel != ActionSetManager.EDebugLevel.Off)
		{
			Log.Out("Focus: " + _focus);
			Log.Out("Input state:");
			foreach (PlayerActionsBase actionSet in PlatformManager.NativePlatform.Input.ActionSets)
			{
				Log.Out($"   {actionSet.GetType().Name}: {actionSet.Enabled}");
			}
			Log.Out("Modal window open: " + LocalPlayerUI.PlayerUIs.Any([PublicizedFrom(EAccessModifier.Internal)] (LocalPlayerUI ui) => ui.windowManager.IsModalWindowOpen()));
			Log.Out("Cursor window: " + isAnyCursorWindowOpen());
		}
		ModEvents.SGameFocusData _data = new ModEvents.SGameFocusData(_focus);
		ModEvents.GameFocus.Invoke(ref _data);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAnyModalWindowOpen()
	{
		IList<LocalPlayerUI> playerUIs = LocalPlayerUI.PlayerUIs;
		for (int num = playerUIs.Count - 1; num >= 0; num--)
		{
			if (playerUIs[num].windowManager.IsModalWindowOpen())
			{
				return true;
			}
		}
		return false;
	}

	public bool isAnyCursorWindowOpen(LocalPlayerUI _ui = null)
	{
		if (_ui == null)
		{
			IList<LocalPlayerUI> playerUIs = LocalPlayerUI.PlayerUIs;
			for (int i = 0; i < playerUIs.Count; i++)
			{
				if (playerUIs[i].windowManager.IsModalWindowOpen() || playerUIs[i].windowManager.IsCursorWindowOpen())
				{
					return true;
				}
			}
		}
		else if (_ui.windowManager.IsModalWindowOpen() || _ui.windowManager.IsCursorWindowOpen())
		{
			return true;
		}
		return false;
	}

	public void SetCursorEnabledOverride(bool _bOverrideOn, bool _bOverrideState)
	{
		if (bCursorVisibleOverride != _bOverrideOn)
		{
			bCursorVisibleOverride = _bOverrideOn;
			setCursorEnabled(_bOverrideState);
		}
	}

	public bool GetCursorEnabledOverride()
	{
		return bCursorVisibleOverride;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setCursorEnabled(bool _e)
	{
		if (!IsQuitting)
		{
			bCursorVisible = _e;
			if (ActionSetManager.DebugLevel == ActionSetManager.EDebugLevel.Verbose)
			{
				Log.Out("CursorEnabled: " + _e);
			}
			SoftCursor.SetCursorVisible(bCursorVisible);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ApplicationQuitCo(float _delay)
	{
		Log.Out("Preparing quit");
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode != ProtocolManager.NetworkType.None)
		{
			try
			{
				Disconnect();
			}
			catch (Exception e)
			{
				Log.Error("Disconnecting failed:");
				Log.Exception(e);
			}
			yield return new WaitForSeconds(_delay);
		}
		if (!IsDedicatedServer)
		{
			windowManager.CloseAllOpenModalWindows();
			windowManager.playerUI.xui.StopAllVideo();
		}
		GamePrefs.Instance.Save();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.StopServers();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
		}
		Cleanup();
		yield return new WaitForSeconds(0.05f);
		allowQuit = true;
		Application.Quit();
	}

	public void ShowMessagePlayerDenied(GameUtils.KickPlayerData _kickData)
	{
		Log.Out("[NET] Kicked from server: " + _kickData.ToString());
		XUiC_MessageBoxWindowGroup.ShowOk(windowManager.playerUI.xui, Localization.Get("auth_messageTitle"), _kickData.LocalizedMessage());
	}

	public void ShowMessageServerAuthFailed(string _message)
	{
		Log.Out("Client failed to authorize server: " + _message);
		XUiC_MessageBoxWindowGroup.ShowOk(windowManager.playerUI.xui, Localization.Get("auth_serverAuthFailedTitle"), _message);
	}

	public void PlayerLoginRPC(ClientInfo _cInfo, string _playerName, (PlatformUserIdentifierAbs userId, string token) _platformUserAndToken, (PlatformUserIdentifierAbs userId, string token) _crossplatformUserAndToken, string _compatibilityVersion, ulong _discordUserId)
	{
		Log.Out("PlayerLogin: " + _playerName + "/" + _compatibilityVersion);
		Log.Out("Client IP: " + _cInfo.ip);
		AuthorizationManager.Instance.Authorize(_cInfo, _playerName, _platformUserAndToken, _crossplatformUserAndToken, _compatibilityVersion, _discordUserId);
	}

	public IEnumerator RequestToEnterGame(ClientInfo _cInfo)
	{
		ModEvents.SPlayerJoinedGameData _data = new ModEvents.SPlayerJoinedGameData(_cInfo);
		ModEvents.PlayerJoinedGame.Invoke(ref _data);
		string playerName = _cInfo.playerName;
		Log.Out("RequestToEnterGame: " + _cInfo.InternalId.CombinedString + "/" + playerName);
		IPlatformUserData userData = PlatformUserManager.GetOrCreate(_cInfo.CrossplatformId);
		if (userData != null)
		{
			userData.MarkBlockedStateChanged();
			yield return PlatformUserManager.ResolveUserBlockedCoroutine(userData);
			if (userData.Blocked[EBlockType.Play].IsBlocked())
			{
				Log.Out($"Player {_cInfo.InternalId} is blocked");
				_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerDenied>().Setup(new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick)));
				yield break;
			}
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && !persistentPlayerIds.Contains(_cInfo.InternalId.ToString()))
		{
			if (persistentPlayerCount + 1 > 100)
			{
				Log.Out("Persistent player data entries limit reached, rejecting new player {0}", _cInfo.InternalId.ToString());
				_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerDenied>().Setup(new GameUtils.KickPlayerData(GameUtils.EKickReason.PersistentPlayerDataExceeded)));
				yield break;
			}
			persistentPlayerIds.Add(_cInfo.InternalId.ToString());
		}
		PersistentPlayerList ppList = ((persistentPlayers != null) ? persistentPlayers.NetworkCloneRelevantForPlayer() : null);
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageIdMapping>().Setup("blocks", Block.fullMappingDataForClients));
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageIdMapping>().Setup("items", ItemClass.fullMappingDataForClients));
		yield return NetPackageLocalization.StartSendingPacketsToClient(_cInfo);
		WorldStaticData.SendXmlsToClient(_cInfo);
		PlatformUserIdentifierAbs persistentPlayerID = getPersistentPlayerID(_cInfo);
		bool flag = !PlayerDataFile.Exists(GameIO.GetPlayerDataDir(), persistentPlayerID.CombinedString);
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageWorldInfo>().Setup(GamePrefs.GetString(EnumGamePrefs.GameMode), GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameName), m_World.Guid, ppList, GameTimer.Instance.ticks, m_World.ChunkCache.IsFixedSize, flag));
		DecoManager.Instance.SendDecosToClient(_cInfo);
		ChunkCluster chunkCache = m_World.ChunkCache;
		if (chunkCache != null)
		{
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunkClusterInfo>().Setup(chunkCache));
		}
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageWorldSpawnPoints>().Setup(GetSpawnPointList()));
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageWorldAreas>().Setup(m_World.TraderAreas));
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameStats>().Setup(GameStats.Instance));
	}

	public void WorldInfo(string _gameMode, string _levelName, string _gameName, string _guid, PersistentPlayerList _playerList, ulong _ticks, bool _fixedSizeCC, bool _firstTimeJoin, Dictionary<string, uint> _worldFileHashes, long _worldDataSize)
	{
		Log.Out("Received game GUID: " + _guid);
		GamePrefs.Set(EnumGamePrefs.GameMode, _gameMode);
		GameIO.SetSaveGameLocalGuid(_guid);
		GamePrefs.Set(EnumGamePrefs.GameWorld, _levelName);
		persistentPlayers = _playerList;
		StartCoroutine(worldInfoCo(_levelName, _gameName, _fixedSizeCC, _firstTimeJoin, _worldFileHashes, _worldDataSize));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator worldInfoCo(string _levelName, string _gameName, bool _fixedSizeCC, bool _firstTimeJoin, Dictionary<string, uint> _worldFileHashes, long _worldDataSize)
	{
		while (true)
		{
			if (!WorldStaticData.AllConfigsReceivedAndLoaded())
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
				{
					yield return null;
					continue;
				}
				break;
			}
			GeneratedTextManager.PrefilterText(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.ServerLoginConfirmationText);
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadCreatingWorld"));
			yield return null;
			string dataDir = GameIO.GetSaveGameLocalDir();
			string rwiFilename = Path.Combine(dataDir, "RemoteWorldInfo.xml");
			bool downloadWorld = false;
			PathAbstractions.AbstractedLocation worldLocation = PathAbstractions.AbstractedLocation.None;
			List<PathAbstractions.AbstractedLocation> availablePathsList = PathAbstractions.WorldsSearchPaths.GetAvailablePathsList();
			foreach (PathAbstractions.AbstractedLocation possibleWorld in availablePathsList)
			{
				if (possibleWorld.Type == PathAbstractions.EAbstractedLocationType.LocalSave && SdFile.Exists(possibleWorld.FullPath + "/completed"))
				{
					worldLocation = possibleWorld;
					break;
				}
				if (possibleWorld.Type == PathAbstractions.EAbstractedLocationType.None)
				{
					continue;
				}
				if (possibleWorld.Name.Equals(_levelName))
				{
					bool worldValid = true;
					yield return NetPackageWorldFolder.TestWorldValid(possibleWorld.FullPath, _worldFileHashes, [PublicizedFrom(EAccessModifier.Internal)] (bool _valid) =>
					{
						worldValid = _valid;
					});
					if (worldValid)
					{
						worldLocation = possibleWorld;
						break;
					}
				}
			}
			if (worldLocation.Type == PathAbstractions.EAbstractedLocationType.None)
			{
				Log.Out("Matching world for " + _levelName + " not found. Will download from server");
				downloadWorld = true;
			}
			else
			{
				GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, (int)worldLocation.Type);
				GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)worldLocation.StorageType);
			}
			int value = SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.GetValue(GameInfoInt.WorldSize);
			long num = SaveDataLimitUtils.CalculatePlayerMapSize(new Vector2i(value, value));
			long requiredSpace = 2048 + num;
			if (downloadWorld || worldLocation.Type == PathAbstractions.EAbstractedLocationType.LocalSave)
			{
				requiredSpace += _worldDataSize;
			}
			UserDataStorageType userDataStorageType = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
			if (SaveInfoProvider.DataLimitEnabled && userDataStorageType.UsesDataLimit())
			{
				long num2 = 0L;
				string guid = GamePrefs.GetString(EnumGamePrefs.GameGuidClient);
				if (SaveInfoProvider.Instance.TryGetRemoteSaveEntry(guid, out var saveEntryInfo))
				{
					num2 = saveEntryInfo.SizeInfo.ReportedSize;
				}
				if (num2 < requiredSpace)
				{
					long pendingBytes = requiredSpace - num2;
					string protectedPath = saveEntryInfo?.SaveDir;
					XUiC_SaveSpaceNeeded confirmationWindow = XUiC_SaveSpaceNeeded.Open(pendingBytes, protectedPath, userDataStorageType, _onlyShowOnInsufficientSpace: false, _canCancel: true, _canDiscard: false, "xuiDmRemoteSaveTitle", "xuiDmRemoteSaveBody", null, null, "xuiStart");
					if (confirmationWindow != null)
					{
						while (confirmationWindow.IsOpen || confirmationWindow.Result == XUiC_SaveSpaceNeeded.ConfirmationResult.Pending)
						{
							yield return null;
						}
						if (confirmationWindow.Result != XUiC_SaveSpaceNeeded.ConfirmationResult.Confirmed)
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
						}
						if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
						{
							break;
						}
					}
					XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, null);
				}
				else if (num2 > requiredSpace)
				{
					requiredSpace = num2;
				}
				SaveInfoProvider.Instance.ClearResources();
			}
			try
			{
				if (!SdDirectory.Exists(dataDir))
				{
					SdDirectory.CreateDirectory(dataDir);
				}
				else
				{
					SdFile.Delete(Path.Combine(dataDir, "archived.flag"));
				}
			}
			catch (Exception e)
			{
				Log.Error("Exception creating local save dir: " + dataDir + " - GUID len: " + GamePrefs.GetString(EnumGamePrefs.GameGuidClient).Length);
				Log.Exception(e);
				throw;
			}
			string path = Path.Combine(dataDir, "hosts.txt");
			string item = SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.GetValue(GameInfoString.IP) + ":" + SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.GetValue(GameInfoInt.Port);
			List<string> list = ((!SdFile.Exists(path)) ? new List<string>() : new List<string>(SdFile.ReadAllLines(path)));
			list.Remove(item);
			list.Insert(0, item);
			SdFile.WriteAllLines(path, list.ToArray());
			if (VersionInformation.TryParseSerializedString(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.GetValue(GameInfoString.ServerVersion), out var _result))
			{
				new RemoteWorldInfo(_gameName, _levelName, _result, requiredSpace).Write(rwiFilename);
			}
			else
			{
				Log.Error("Failed writing RemoteWorldInfo. Could not parse LastGameServerInfo information.");
			}
			if (downloadWorld)
			{
				XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, string.Format(Localization.Get("uiLoadDownloadingWorldWait"), 0f, 0, 0));
				yield return NetPackageWorldFolder.RequestWorld();
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
				{
					break;
				}
				Log.Out("World received");
				GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType));
				GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, 1);
				worldLocation = PathAbstractions.Contextual.FindActiveWorldLocation();
			}
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadDownloadingSigns"));
			yield return SignDataManager.Instance.RequestWorldSignDataFromServer();
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
			{
				break;
			}
			yield return createWorld(_levelName, worldLocation, _gameName, _fixedSizeCC);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWorldInitInfoRequest>().Setup());
			while (!worldInitInfoReceived)
			{
				yield return null;
			}
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadCreatingPlayer"));
			yield return null;
			worldCreated = true;
			firstTimeJoin = _firstTimeJoin;
			string confirmationText = GeneratedTextManager.GetDisplayTextImmediately(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.ServerLoginConfirmationText, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.Supported);
			if (string.IsNullOrEmpty(confirmationText))
			{
				confirmationText = SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.ServerLoginConfirmationText.Text;
			}
			if (!string.IsNullOrEmpty(confirmationText))
			{
				LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPrimaryPlayer();
				while (!playerUI.xui.IsReady)
				{
					yield return null;
				}
				yield return null;
				if (!string.IsNullOrEmpty(XUiC_ServerJoinRulesDialog.ID) && playerUI.xui.FindWindowGroupByName(XUiC_ServerJoinRulesDialog.ID) != null)
				{
					XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
					windowManager.Close("crossplayWarning");
					XUiC_ServerJoinRulesDialog.Show(playerUI, confirmationText);
				}
				else
				{
					DoSpawn();
				}
			}
			else
			{
				DoSpawn();
			}
			DynamicMeshManager.Init();
			break;
		}
	}

	public void DoSpawn()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.SkipSpawnButton))
		{
			RequestToSpawn();
		}
		else
		{
			XUiC_SpawnSelectionWindow.Open(LocalPlayerUI.primaryUI, _chooseSpawnPosition: false, _enteringGame: true, firstTimeJoin);
		}
	}

	public void RequestToSpawn(int _nearEntityId = -1)
	{
		XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, null);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRequestToSpawnPlayer>().Setup(Utils.FastMin(12, GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance)), PlayerProfile.LoadLocalProfile(), _nearEntityId));
	}

	public void ChunkClusterInfo(string _name, bool _bInifiniteTerrain, Vector2i _cMin, Vector2i _cMax, Vector3 _pos)
	{
		StartCoroutine(chunkClusterInfoCo(_name, _bInifiniteTerrain, _cMin, _cMax, _pos));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator chunkClusterInfoCo(string _name, bool _bInifiniteTerrain, Vector2i _cMin, Vector2i _cMax, Vector3 _pos)
	{
		while (!worldCreated && SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield return null;
		}
		if (worldCreated && m_World != null)
		{
			ChunkCluster chunkCache = m_World.ChunkCache;
			chunkCache.Position = _pos;
			chunkCache.ChunkMinPos = _cMin;
			chunkCache.ChunkMaxPos = _cMax;
			if (!_bInifiniteTerrain && m_World.m_WorldEnvironment != null)
			{
				m_World.m_WorldEnvironment.SetColliders((_cMin.x + 1) * 16, (_cMin.y + 1) * 16, (_cMax.x - _cMin.x - 1) * 16, (_cMax.y - _cMin.y - 1) * 16, Constants.cSizePlanesAround, 0f);
				m_World.m_WorldEnvironment.CreateLevelBorderBox(m_World);
				m_World.ChunkCache.IsFixedSize = true;
			}
			chunkClusterLoaded = true;
		}
	}

	public void RequestToSpawnPlayer(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile, int _nearEntityId)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.ServerMaxAllowedViewDistance);
		if (num < 4)
		{
			num = 4;
		}
		else if (num > 12)
		{
			num = 12;
		}
		_chunkViewDim = Mathf.Clamp(_chunkViewDim, 4, num);
		PlatformUserIdentifierAbs persistentPlayerId = getPersistentPlayerID(_cInfo);
		PlayerDataFile playerDataFile = new PlayerDataFile();
		playerDataFile.Load(GameIO.GetPlayerDataDir(), persistentPlayerId.CombinedString);
		playerDataFile.lastSpawnPosition = SpawnPosition.Undef;
		int num2 = 0;
		int num3 = ((playerDataFile.bLoaded && playerDataFile.id != -1) ? playerDataFile.id : EntityFactory.nextEntityID++);
		if (m_World.GetEntity(num3) != null)
		{
			num3 = (playerDataFile.id = EntityFactory.nextEntityID++);
		}
		Log.Out($"RequestToSpawnPlayer: {num3}, {_cInfo.playerName}, {_chunkViewDim}");
		if (GameStats.GetBool(EnumGameStats.IsSpawnNearOtherPlayer))
		{
			for (int i = 0; i < m_World.Players.list.Count; i++)
			{
				if (m_World.Players.list[i].TeamNumber == num2 && m_World.FindRandomSpawnPointNearPlayer(m_World.Players.list[i], 15, out var x, out var y, out var z, 15))
				{
					playerDataFile.lastSpawnPosition = new SpawnPosition(new Vector3i(x, y, z), 0f);
					break;
				}
			}
		}
		if (_nearEntityId != -1)
		{
			AllowSpawnNearFriend spawnNearFriendMode = XUiC_SpawnNearFriendsList.SpawnNearFriendMode;
			Entity entity = m_World.GetEntity(_nearEntityId);
			if ((bool)entity && spawnNearFriendMode != AllowSpawnNearFriend.Disabled)
			{
				int num4 = 15;
				bool flag;
				Vector3 _position;
				do
				{
					num4--;
					flag = m_World.GetRandomSpawnPositionMinMaxToPosition(entity.position, 40, 150, 1, _checkBedrolls: true, out _position, num3, _checkWater: true, 20, _checkLandClaim: true);
					if (!flag)
					{
						break;
					}
					if (spawnNearFriendMode != AllowSpawnNearFriend.InForest)
					{
						continue;
					}
					BiomeDefinition.BiomeType? biomeType = m_World.GetBiomeInWorld((int)_position.x, (int)_position.z)?.m_BiomeType;
					bool flag2;
					if (biomeType.HasValue)
					{
						BiomeDefinition.BiomeType valueOrDefault = biomeType.GetValueOrDefault();
						if ((uint)(valueOrDefault - 2) <= 1u)
						{
							flag2 = true;
							goto IL_0237;
						}
					}
					flag2 = false;
					goto IL_0237;
					IL_0237:
					flag = flag2;
				}
				while (num4 > 0 && !flag);
				if (flag)
				{
					playerDataFile.lastSpawnPosition = new SpawnPosition(_position, m_World.RandomRange(0f, 360f));
				}
				else
				{
					Log.Warning($"RequestToSpawnPlayer: Failed getting a valid spawn position near player with entity ID {_nearEntityId}");
				}
			}
		}
		if (playerDataFile.lastSpawnPosition.IsUndef())
		{
			playerDataFile.lastSpawnPosition = GetSpawnPointList().GetRandomSpawnPosition(m_World);
		}
		if (!playerDataFile.bLoaded)
		{
			playerDataFile.ecd.pos = playerDataFile.lastSpawnPosition.position;
		}
		EntityCreationData entityCreationData = new EntityCreationData();
		if (!playerDataFile.bLoaded || playerDataFile.ecd.playerProfile == null || !GamePrefs.GetBool(EnumGamePrefs.PersistentPlayerProfiles))
		{
			playerDataFile.ecd.playerProfile = _playerProfile;
		}
		if (playerDataFile.bLoaded)
		{
			entityCreationData.entityData = playerDataFile.ecd.entityData;
			entityCreationData.readFileVersion = playerDataFile.ecd.readFileVersion;
		}
		entityCreationData.entityClass = EntityClass.FromString(playerDataFile.ecd.playerProfile.EntityClassName);
		entityCreationData.playerProfile = playerDataFile.ecd.playerProfile;
		entityCreationData.id = num3;
		entityCreationData.teamNumber = num2;
		entityCreationData.pos = playerDataFile.ecd.pos;
		entityCreationData.rot = playerDataFile.ecd.rot;
		EntityPlayer entityPlayer = (EntityPlayer)EntityFactory.CreateEntity(entityCreationData);
		entityPlayer.isEntityRemote = true;
		entityPlayer.Respawn(playerDataFile.bLoaded ? RespawnType.JoinMultiplayer : RespawnType.EnterMultiplayer);
		playerDataFile.ToPlayer(entityPlayer);
		bool flag3 = false;
		PersistentPlayerData persistentPlayerData = persistentPlayers?.GetPlayerData(persistentPlayerId);
		if (persistentPlayerData == null)
		{
			persistentPlayerData = persistentPlayers?.CreatePlayerData(persistentPlayerId, _cInfo.PlatformId, _cInfo.playerName, _cInfo.device.ToPlayGroup());
		}
		else
		{
			persistentPlayerData.Update(_cInfo.PlatformId, new AuthoredText(_cInfo.playerName, persistentPlayerId), _cInfo.device.ToPlayGroup());
			flag3 = true;
		}
		persistentPlayerData.LastLogin = DateTime.Now;
		persistentPlayerData.EntityId = num3;
		if (persistentPlayers != null)
		{
			persistentPlayers.MapPlayer(persistentPlayerData);
		}
		persistentPlayers?.SavePersistentPlayerData();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SetClientEntityId(_cInfo, num3, playerDataFile);
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerId>().Setup(num3, num2, playerDataFile, _chunkViewDim));
		Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>().RefreshCrates(num3);
		m_World.SpawnEntityInWorld(entityPlayer);
		entityPlayer.ChunkObserver = m_World.m_ChunkManager.AddChunkObserver(entityPlayer.GetPosition(), _bBuildVisualMeshAround: false, _chunkViewDim, entityPlayer.entityId);
		IMapChunkDatabase.TryCreateOrLoad(entityPlayer.entityId, out entityPlayer.ChunkObserver.mapDatabase, [PublicizedFrom(EAccessModifier.Internal)] () => new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataDir(), persistentPlayerId.CombinedString));
		if (persistentPlayers != null)
		{
			persistentPlayers.DispatchPlayerEvent(persistentPlayerData, null, EnumPersistentPlayerDataReason.Login);
		}
		if (flag3)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(persistentPlayerData, EnumPersistentPlayerDataReason.Login));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(persistentPlayerData, EnumPersistentPlayerDataReason.New));
		}
		ModEvents.SPlayerSpawningData _data = new ModEvents.SPlayerSpawningData(_cInfo, _chunkViewDim, _playerProfile);
		ModEvents.PlayerSpawning.Invoke(ref _data);
	}

	public void PersistentPlayerLogin(PersistentPlayerData ppData)
	{
		if (persistentPlayers == null)
		{
			return;
		}
		persistentPlayers.SetPlayerData(ppData);
		if (myPlayerId != -1 && ppData.EntityId == myPlayerId)
		{
			persistentLocalPlayer = ppData;
			if (myEntityPlayerLocal != null)
			{
				myEntityPlayerLocal.persistentPlayerData = persistentLocalPlayer;
			}
		}
		persistentPlayers.DispatchPlayerEvent(ppData, null, EnumPersistentPlayerDataReason.Login);
	}

	public void HandlePersistentPlayerDisconnected(int _entityId)
	{
		PersistentPlayerData playerDataFromEntityID = persistentPlayers.GetPlayerDataFromEntityID(_entityId);
		if (playerDataFromEntityID != null)
		{
			persistentPlayers.DispatchPlayerEvent(playerDataFromEntityID, null, EnumPersistentPlayerDataReason.Disconnected);
			persistentPlayers.UnmapPlayer(playerDataFromEntityID.PrimaryId);
		}
	}

	public void PlayerId(int _playerId, int _teamNumber, PlayerDataFile _playerDataFile, int _chunkViewDim)
	{
		Log.Out($"PlayerId({_playerId}, {_teamNumber})");
		Log.Out("Allowed ChunkViewDistance: " + _chunkViewDim);
		GameStats.Set(EnumGameStats.AllowedViewDistance, _chunkViewDim);
		myPlayerId = _playerId;
		EntityCreationData entityCreationData = new EntityCreationData();
		entityCreationData.id = _playerId;
		entityCreationData.teamNumber = _teamNumber;
		if (_playerDataFile.bLoaded)
		{
			entityCreationData.entityClass = EntityClass.FromString(_playerDataFile.ecd.playerProfile.EntityClassName);
			entityCreationData.playerProfile = _playerDataFile.ecd.playerProfile;
		}
		else
		{
			entityCreationData.playerProfile = PlayerProfile.LoadLocalProfile();
			entityCreationData.entityClass = EntityClass.FromString(entityCreationData.playerProfile.EntityClassName);
		}
		entityCreationData.skinTexture = GamePrefs.GetString(EnumGamePrefs.OptionsPlayerModelTexture);
		entityCreationData.id = _playerId;
		entityCreationData.pos = _playerDataFile.ecd.pos;
		entityCreationData.rot = _playerDataFile.ecd.rot;
		entityCreationData.belongsPlayerId = _playerId;
		EntityPlayerLocal entityPlayerLocal = EntityFactory.CreateEntity(entityCreationData) as EntityPlayerLocal;
		setLocalPlayerEntity(entityPlayerLocal);
		Log.Out($"Found own player entity with id {entityPlayerLocal.entityId}");
		entityPlayerLocal.lastSpawnPosition = _playerDataFile.lastSpawnPosition;
		if (_playerDataFile.bLoaded)
		{
			_playerDataFile.ToPlayer(entityPlayerLocal);
			clientRespawnType = RespawnType.JoinMultiplayer;
		}
		else
		{
			clientRespawnType = RespawnType.EnterMultiplayer;
		}
		m_World.SpawnEntityInWorld(entityPlayerLocal);
		entityPlayerLocal.ChunkObserver = m_World.m_ChunkManager.AddChunkObserver(entityPlayerLocal.GetPosition(), _bBuildVisualMeshAround: true, GameUtils.GetViewDistance(), -1);
		IMapChunkDatabase.TryCreateOrLoad(entityPlayerLocal.entityId, out entityPlayerLocal.ChunkObserver.mapDatabase, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			string combinedString = getPersistentPlayerID(null).CombinedString;
			return new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataLocalDir(), combinedString);
		});
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		uIForPlayer.xui.SetDataConnections();
		uIForPlayer.xui.SetCraftingData(_playerDataFile.craftingData);
		SetWorldTime(m_World.worldTime);
		PlayerInteractions.Instance.JoinedMultiplayerServer(persistentPlayers);
		entityPlayerLocal.Respawn(clientRespawnType);
		gameStateManager.InitGame(_bServer: false);
		gameStateManager.StartGame();
	}

	public void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos, int _entityId)
	{
		if (_entityId == -1 || !m_World.Entities.dict.TryGetValue(_entityId, out var value))
		{
			return;
		}
		EntityPlayer entityPlayer = value as EntityPlayer;
		if (!(entityPlayer == null))
		{
			if (_respawnReason == RespawnType.Died && entityPlayer.isEntityRemote)
			{
				entityPlayer.SetAlive();
			}
			if (_respawnReason == RespawnType.EnterMultiplayer || _respawnReason == RespawnType.JoinMultiplayer)
			{
				DisplayGameMessage(EnumGameMessages.JoinedGame, _entityId);
			}
			PlayerInteractions.Instance.PlayerSpawnedInMultiplayerServer(persistentPlayers, _entityId, _respawnReason);
			bool flag = _respawnReason == RespawnType.NewGame || _respawnReason == RespawnType.EnterMultiplayer || _respawnReason == RespawnType.JoinMultiplayer || _respawnReason == RespawnType.LoadedGame;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && flag)
			{
				VehicleManager.Instance.UpdateVehicleWaypointsForPlayer(_entityId);
				DroneManager.Instance.UpdateWaypointsForPlayer(_entityId);
				DroneManager.Instance.SpawnFollowingDronesForPLayer(_entityId, World);
			}
			ModEvents.SPlayerSpawnedInWorldData _data = new ModEvents.SPlayerSpawnedInWorldData(_cInfo, entityPlayer is EntityPlayerLocal, _entityId, _respawnReason, _pos);
			ModEvents.PlayerSpawnedInWorld.Invoke(ref _data);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.OnClientSpawned?.Invoke(_cInfo);
				Log.Out("PlayerSpawnedInWorld (reason: {0}, position: {2}): {1}", _respawnReason.ToStringCached(), (_cInfo != null) ? _cInfo.ToString() : "localplayer", _pos.ToString());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFirstSpawnInteractions(PlayerInteraction _interaction)
	{
		if (_interaction.Type != PlayerInteractionType.FirstSpawn)
		{
			return;
		}
		int num = persistentPlayers.PlayerToEntityMap[_interaction.PlayerData.PrimaryId];
		if (myEntityPlayerLocal == null || num == myEntityPlayerLocal?.entityId)
		{
			return;
		}
		IPlatformUserData orCreate = PlatformUserManager.GetOrCreate(_interaction.PlayerData.PrimaryId);
		if (num != -1 && orCreate != null && orCreate.Blocked[EBlockType.Play].IsBlocked())
		{
			DisplayGameMessage(EnumGameMessages.BlockedPlayerAlert, num);
		}
		else
		{
			if (!GamePrefs.GetBool(EnumGamePrefs.OptionsAutoPartyWithFriends))
			{
				return;
			}
			PersistentPlayerData persistentPlayerData = myEntityPlayerLocal.persistentPlayerData;
			if (persistentPlayerData != null && persistentPlayerData.IsAlly(_interaction.PlayerData.PrimaryId))
			{
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, myEntityPlayerLocal.entityId, num));
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, myEntityPlayerLocal.entityId, num));
				}
			}
		}
	}

	public void PlayerDisconnected(ClientInfo _cInfo)
	{
		if (_cInfo.entityId != -1)
		{
			EntityPlayer entityPlayer = (EntityPlayer)m_World.GetEntity(_cInfo.entityId);
			Log.Out("Player {0} disconnected after {1} minutes", GameUtils.SafeStringFormat(entityPlayer.EntityName), ((Time.timeSinceLevelLoad - entityPlayer.CreationTimeSinceLevelLoad) / 60f).ToCultureInvariantString("0.0"));
		}
		if (IsDedicatedServer)
		{
			GC.Collect();
			MemoryPools.Cleanup();
		}
		PersistentPlayerData persistentPlayerData = getPersistentPlayerData(_cInfo);
		if (persistentPlayerData != null)
		{
			persistentPlayerData.LastLogin = DateTime.Now;
			persistentPlayerData.EntityId = -1;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(persistentPlayerData, EnumPersistentPlayerDataReason.Disconnected), _onlyClientsAttachedToAnEntity: false, -1, -1, -1, null, 192, _onlyClientsNotAttachedToAnEntity: true);
		}
		persistentPlayers?.SavePersistentPlayerData();
		SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectClient(_cInfo, _bShutdown: false, _clientDisconnect: true);
	}

	public void SavePlayerData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
	{
		_cInfo.latestPlayerData = _playerDataFile;
		int entityId = _cInfo.entityId;
		if (entityId != -1)
		{
			EntityPlayer entityPlayer = (EntityPlayer)m_World.GetEntity(entityId);
			if (entityPlayer != null)
			{
				_playerDataFile.Save(GameIO.GetPlayerDataDir(), _cInfo.InternalId.CombinedString);
				if (entityPlayer.ChunkObserver.mapDatabase != null)
				{
					ThreadManager.AddSingleTask(entityPlayer.ChunkObserver.mapDatabase.SaveAsync, new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataDir(), _cInfo.InternalId.CombinedString));
				}
				entityPlayer.QuestJournal = _playerDataFile.questJournal;
				if (persistentPlayers != null)
				{
					foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in persistentPlayers.Players)
					{
						if (player.Value.EntityId == _playerDataFile.id)
						{
							player.Value.Position = new Vector3i(_playerDataFile.ecd.pos);
							break;
						}
					}
				}
			}
		}
		ModEvents.SSavePlayerDataData _data = new ModEvents.SSavePlayerDataData(_cInfo, _playerDataFile);
		ModEvents.SavePlayerData.Invoke(ref _data);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs getPersistentPlayerID(ClientInfo _cInfo)
	{
		return _cInfo?.InternalId ?? PlatformManager.InternalLocalUserIdentifier;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerData getPersistentPlayerData(ClientInfo _cInfo)
	{
		return persistentPlayers?.GetPlayerData(getPersistentPlayerID(_cInfo));
	}

	public PersistentPlayerList GetPersistentPlayerList()
	{
		return persistentPlayers;
	}

	public PersistentPlayerData GetPersistentLocalPlayer()
	{
		return persistentLocalPlayer;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator createWorld(string _levelName, PathAbstractions.AbstractedLocation _worldLocation, string _sGameName, bool _fixedSizeCC = false)
	{
		Log.Out($"createWorld: {_levelName}, {_worldLocation}, {_sGameName}, {GamePrefs.GetString(EnumGamePrefs.GameMode)}");
		GamePrefs.Set(EnumGamePrefs.GameNameClient, _sGameName);
		bool flag = GameModeEditWorld.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode));
		PathAbstractions.CacheEnabled = !flag;
		if (flag)
		{
			Constants.cDigAndBuildDistance = 50f;
			Constants.cBuildIntervall = 0.2f;
			Constants.cCollectItemDistance = 50f;
		}
		else if (GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			Constants.cDigAndBuildDistance = 25f;
			Constants.cBuildIntervall = 0.2f;
			Constants.cCollectItemDistance = 25f;
		}
		else
		{
			Constants.cDigAndBuildDistance = 5f;
			Constants.cBuildIntervall = 0.5f;
			Constants.cCollectItemDistance = 3.5f;
		}
		OcclusionManager.Instance.WorldChanging(flag);
		yield return null;
		m_World = new World();
		if (IsDedicatedServer || IsEditMode())
		{
			GameHasStarted = true;
		}
		else
		{
			StartCoroutine(waitForGameStart());
		}
		m_World.Init(this, WorldBiomes.Instance);
		yield return null;
		if (biomeParticleManager == null)
		{
			biomeParticleManager = new BiomeParticleManager();
		}
		if (this.OnWorldChanged != null)
		{
			this.OnWorldChanged(m_World);
		}
		PlayerInteractions.Instance.OnNewPlayerInteraction += HandleFirstSpawnInteractions;
		yield return null;
		SignTextureManager.Instance.Initialize();
		yield return null;
		yield return m_World.LoadWorld(_levelName, _worldLocation, _fixedSizeCC);
		yield return null;
		AstarManager.Init(base.gameObject);
		yield return null;
		lootManager = new LootManager(m_World);
		yield return null;
		LockManager.Instance.Init();
		yield return null;
		traderManager = new TraderManager(m_World);
		yield return null;
		ResourceRequest weatherLoading = Resources.LoadAsync("Prefabs/WeatherManager");
		while (!weatherLoading.isDone)
		{
			yield return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((UnityEngine.Object)(weatherLoading.asset as GameObject)) as GameObject;
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		WeatherManager.Init(m_World, gameObject);
		yield return null;
		yield return EnvironmentAudioManager.CreateNewInstance();
		yield return null;
		new WaterSplashCubes();
		yield return null;
		WireManager.Instance.Init();
		yield return null;
		LoadManager.AssetRequestTask<GameObject> requestTask = LoadManager.LoadAsset<GameObject>("@:Prefabs/SkySystem/SkySystem.prefab");
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => requestTask.IsDone);
		SkyManager.Loaded(UnityEngine.Object.Instantiate(requestTask.Asset));
		yield return null;
		if ((bool)WeatherManager.Instance)
		{
			WeatherManager.Instance.CloudsFrameUpdateNow();
			WeatherManager.Instance.InitParticles();
		}
		yield return null;
		if (IsEditMode())
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator != null && IsEditMode())
			{
				dynamicPrefabDecorator.CreateBoundingBoxes();
			}
			SpawnPointList spawnPointList = GetSpawnPointList();
			for (int num = 0; num < spawnPointList.Count; num++)
			{
				SpawnPoint spawnPoint = spawnPointList[num];
				SelectionBoxManager.Instance.CategoryStartPoint.AddBox(spawnPoint.spawnPosition.ToBlockPos().ToString(), spawnPoint.spawnPosition.ToBlockPos(), Vector3i.one, _bDrawDirection: true).FacingDirection = spawnPoint.spawnPosition.heading;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				PrefabInstanceClientManager.Instance.StartAsServer();
			}
			else
			{
				PrefabInstanceClientManager.Instance.StartAsClient();
			}
		}
		ModEvents.SCreateWorldDoneData _data = default(ModEvents.SCreateWorldDoneData);
		ModEvents.CreateWorldDone.Invoke(ref _data);
		Log.Out("createWorld() done");
	}

	public SpawnPointList GetSpawnPointList()
	{
		return m_World.ChunkCache.ChunkProvider.GetSpawnPointList();
	}

	public ChunkManager.ChunkObserver AddChunkObserver(Vector3 _initialPosition, bool _bBuildVisualMeshAround, int _viewDim, int _entityIdToSendChunksTo)
	{
		return m_World.m_ChunkManager.AddChunkObserver(_initialPosition, _bBuildVisualMeshAround, _viewDim, _entityIdToSendChunksTo);
	}

	public void RemoveChunkObserver(ChunkManager.ChunkObserver _observer)
	{
		m_World.m_ChunkManager.RemoveChunkObserver(_observer);
	}

	public void ExplosionServer(Vector3 _worldPos, Vector3i _blockPos, Quaternion _rotation, ExplosionData _explosionData, int _entityId, float _delay, bool _bRemoveBlockAtExplPosition, ItemValue _itemValueExplosionSource = null)
	{
		if (_bRemoveBlockAtExplPosition)
		{
			m_World.SetBlockRPC(_blockPos, BlockValue.Air);
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageExplosionInitiate>().Setup(_worldPos, _blockPos, _rotation, _explosionData, _entityId, _delay, _bRemoveBlockAtExplPosition, _itemValueExplosionSource));
		}
		else if (_delay <= 0f)
		{
			explode(_worldPos, _blockPos, _rotation, _explosionData, _entityId, _itemValueExplosionSource);
		}
		else
		{
			StartCoroutine(explodeLater(_worldPos, _blockPos, _rotation, _explosionData, _entityId, _itemValueExplosionSource, _delay));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator explodeLater(Vector3 _position, Vector3i _blockPos, Quaternion _rotation, ExplosionData _explosionData, int _entityId, ItemValue _itemValueExplosionSource, float _delayInSec)
	{
		yield return new WaitForSeconds(_delayInSec);
		explode(_position, _blockPos, _rotation, _explosionData, _entityId, _itemValueExplosionSource);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void explode(Vector3 _worldPos, Vector3i _blockPos, Quaternion _rotation, ExplosionData _explosionData, int _entityId, ItemValue _itemValueExplosionSource)
	{
		Explosion explosion = new Explosion(m_World, _worldPos, _blockPos, _explosionData, _entityId);
		explosion.AttackBlocks(_entityId, _itemValueExplosionSource);
		explosion.AttackEntites(_entityId, _itemValueExplosionSource, _explosionData.DamageType);
		tempExplPositions.Clear();
		explosion.ChangedBlockPositions.CopyValuesTo(tempExplPositions);
		ExplodeGroup explodeGroup = new ExplodeGroup();
		explodeGroup.pos = _worldPos;
		explodeGroup.radius = _explosionData.BlockRadius;
		explodeGroup.delay = 3;
		ExplodeGroup.Falling item = default(ExplodeGroup.Falling);
		foreach (BlockChangeInfo tempExplPosition in tempExplPositions)
		{
			if (tempExplPosition.blockValue.isair)
			{
				BlockValue block = m_World.GetBlock(tempExplPosition.blockValueRef);
				if (!block.isair && block.Block.IsExplosionAffected)
				{
					item.pos = tempExplPosition.blockValueRef;
					item.bv = block;
					explodeGroup.fallings.Add(item);
				}
			}
		}
		if (explodeGroup.fallings.Count > 0)
		{
			explodeFallingGroups.Add(explodeGroup);
		}
		GameObject gameObject = ExplosionClient(_worldPos, _rotation, _explosionData.ParticleIndex, _explosionData.BlastPower, _explosionData.EntityRadius, _explosionData.BlockDamage, _entityId, tempExplPositions);
		if (gameObject != null)
		{
			if (_explosionData.Duration > 0f)
			{
				TemporaryObject component = gameObject.GetComponent<TemporaryObject>();
				if (component != null)
				{
					component.SetLife(_explosionData.Duration);
				}
			}
			if (gameObject.TryGetComponent<ExplosionDamageArea>(out var component2))
			{
				component2.BuffActions = _explosionData.BuffActions;
				component2.InitiatorEntityId = _entityId;
			}
			if (m_World.aiDirector != null && !_explosionData.IgnoreHeatMap)
			{
				AudioPlayer component3 = gameObject.GetComponent<AudioPlayer>();
				if ((bool)component3)
				{
					m_World.aiDirector.OnSoundPlayedAtPosition(_entityId, _worldPos, component3.soundName, 1f);
				}
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageExplosionClient>().Setup(_worldPos, _rotation, _explosionData.ParticleIndex, _explosionData.BlastPower, _explosionData.BlockDamage, _explosionData.EntityRadius, _entityId, tempExplPositions), _onlyClientsAttachedToAnEntity: true);
		}
		tempExplPositions.Clear();
	}

	public GameObject ExplosionClient(Vector3 _center, Quaternion _rotation, int _index, int _blastPower, float _blastRadius, float _blockDamage, int _entityId, List<BlockChangeInfo> _explosionChanges)
	{
		if (m_World == null)
		{
			return null;
		}
		GameObject result = null;
		if (_index > 0 && _index < WorldStaticData.prefabExplosions.Length && WorldStaticData.prefabExplosions[_index] != null)
		{
			result = UnityEngine.Object.Instantiate(WorldStaticData.prefabExplosions[_index].gameObject, _center - Origin.position, _rotation);
			ApplyExplosionForce.Explode(_center, _blastPower, _blastRadius);
		}
		if (_explosionChanges.Count > 0)
		{
			ChangeBlocks(null, _explosionChanges);
		}
		QuestEventManager.Current.DetectedExplosion(_center, _entityId, _blockDamage);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExplodeGroupFrameUpdate()
	{
		int et = EntityClass.FromString("fallingBlock");
		GameRandom gameRandom = m_World.GetGameRandom();
		for (int num = explodeFallingGroups.Count - 1; num >= 0; num--)
		{
			ExplodeGroup explodeGroup = explodeFallingGroups[num];
			if (--explodeGroup.delay <= 0)
			{
				float num2 = 20f + Mathf.Pow(explodeGroup.fallings.Count, 0.73f);
				float num3 = Utils.FastMax(1f, (float)explodeGroup.fallings.Count / num2);
				float num4 = 1f;
				for (int i = 0; i < explodeGroup.fallings.Count; i++)
				{
					if ((num4 -= 1f) > 0f)
					{
						continue;
					}
					num4 += num3;
					ExplodeGroup.Falling falling = explodeGroup.fallings[i];
					Vector3 vector = falling.pos.ToVector3Center();
					vector.y += 1.4f;
					if (Physics.Raycast(vector - Origin.position, Vector3.down, float.MaxValue, 65536))
					{
						vector.y -= 1.4f;
						Block block = falling.bv.Block;
						block.DropItemsOnEvent(m_World, falling.bv, EnumDropEvent.Destroy, 0.5f, vector, Vector3.zero, Constants.cItemExplosionLifetime, -1, _bGetSameItemIfNoneFound: true);
						if (block.ShowModelOnFall())
						{
							EntityFallingBlock entityFallingBlock = (EntityFallingBlock)EntityFactory.CreateEntity(et, -1, new BlockValue[1] { falling.bv }, new TextureFullArray[1] { m_World.GetTextureFullArray(falling.pos.x, falling.pos.y, falling.pos.z) }, 1, vector, Vector3.zero, -1f, -1);
							Vector3 vector2 = vector - explodeGroup.pos;
							float num5 = 1f - Mathf.Clamp01(vector2.magnitude / explodeGroup.radius) * 0.6f;
							float num6 = 18f * num5;
							vector2.y += -0.2f + gameRandom.RandomFloat * 6f;
							entityFallingBlock.SetStartVelocity(vector2.normalized * num6, (gameRandom.RandomFloat * 15f + 2f) * num5);
							m_World.SpawnEntityInWorld(entityFallingBlock);
						}
					}
				}
				explodeFallingGroups.RemoveAt(num);
			}
		}
	}

	public void ChangeBlocks(PlatformUserIdentifierAbs persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
	{
		if (m_World == null)
		{
			return;
		}
		lock (ccChanged)
		{
			PersistentPlayerData persistentPlayerData = null;
			Entity entity = null;
			if (persistentPlayerId == null)
			{
				persistentPlayerData = persistentLocalPlayer;
				entity = myEntityPlayerLocal;
			}
			else if (persistentPlayers != null)
			{
				persistentPlayerData = persistentPlayers.GetPlayerData(persistentPlayerId);
				if (persistentPlayerData != null && persistentPlayerData.EntityId != -1)
				{
					entity = m_World.GetEntity(persistentPlayerData.EntityId);
				}
			}
			bool flag = false;
			ChunkCluster chunkCluster = null;
			int num = 0;
			for (int i = 0; i < _blocksToChange.Count; i++)
			{
				BlockChangeInfo blockChangeInfo = _blocksToChange[i];
				if (chunkCluster == null)
				{
					chunkCluster = m_World.ChunkCache;
					if (chunkCluster == null)
					{
						continue;
					}
					if (!ccChanged.Contains(chunkCluster))
					{
						ccChanged.Add(chunkCluster);
						num++;
						chunkCluster.ChunkPosNeedsRegeneration_DelayedStart();
					}
				}
				bool flag2 = blockChangeInfo.bChangeDensity;
				bool bForceDensity = blockChangeInfo.bForceDensity;
				sbyte density = chunkCluster.GetDensity(blockChangeInfo.blockValueRef);
				sbyte b = blockChangeInfo.density;
				if (!flag2)
				{
					if (density < 0 && blockChangeInfo.blockValue.isair)
					{
						b = MarchingCubes.DensityAir;
						flag2 = true;
					}
					else if (density >= 0 && blockChangeInfo.blockValue.Block.shape.IsTerrain())
					{
						b = MarchingCubes.DensityTerrain;
						flag2 = true;
					}
				}
				if (density == b)
				{
					flag2 = false;
				}
				if (blockChangeInfo.bChangeDamage && chunkCluster.GetBlock(blockChangeInfo.blockValueRef).type != blockChangeInfo.blockValue.type)
				{
					continue;
				}
				if (blockChangeInfo.blockValueRef.Type != BlockValueRefType.Block)
				{
					chunkCluster.SetBlockValue(blockChangeInfo.blockValueRef, blockChangeInfo.blockValue);
					continue;
				}
				Chunk chunk = chunkCluster.GetChunkFromWorldPos(blockChangeInfo.blockValueRef) as Chunk;
				if (chunk != null && blockChangeInfo.blockValueRef.TryGetBlockPos(out var pos))
				{
					int num2 = World.toBlockXZ(pos.x);
					int num3 = World.toBlockXZ(pos.z);
					if (pos.y >= chunk.GetHeight(num2, num3) && blockChangeInfo.blockValue.Block.shape.IsTerrain())
					{
						chunk.SetTopSoilBroken(num2, num3);
						Chunk chunk2 = chunk;
						if (num3 == 15)
						{
							chunk2 = chunkCluster.GetChunkSync(chunk.X, chunk.Z + 1);
						}
						chunk2?.SetTopSoilBroken(num2, World.toBlockXZ(num3 + 1));
						chunk2 = chunk;
						if (num2 == 15)
						{
							chunk2 = chunkCluster.GetChunkSync(chunk.X + 1, chunk.Z);
						}
						chunk2?.SetTopSoilBroken(World.toBlockXZ(num2 + 1), num3);
						chunk2 = chunk;
						if (num3 == 0)
						{
							chunk2 = chunkCluster.GetChunkSync(chunk.X, chunk.Z - 1);
						}
						chunk2?.SetTopSoilBroken(num2, World.toBlockXZ(num3 - 1));
						chunk2 = chunk;
						if (num2 == 0)
						{
							chunk2 = chunkCluster.GetChunkSync(chunk.X - 1, chunk.Z);
						}
						chunk2?.SetTopSoilBroken(World.toBlockXZ(num2 - 1), num3);
					}
					m_World.UncullChunk(chunk);
				}
				TileEntity tileEntity = null;
				if (!blockChangeInfo.blockValue.ischild)
				{
					tileEntity = m_World.GetTileEntity(blockChangeInfo.blockValueRef);
				}
				BlockValue bvOld = chunkCluster.SetBlock(blockChangeInfo.blockValueRef, blockChangeInfo.bChangeBlockValue, blockChangeInfo.blockValue, flag2, b, _isNotify: true, blockChangeInfo.bUpdateLight, bForceDensity, _wasChild: false, blockChangeInfo.changedByEntityId);
				TileEntity tileEntity2 = null;
				if (tileEntity != null)
				{
					tileEntity2 = m_World.GetTileEntity(blockChangeInfo.blockValueRef);
					bool flag3 = false;
					if (tileEntity != tileEntity2 && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						flag3 = true;
						tileEntity.ReplacedBy(bvOld, blockChangeInfo.blockValue, tileEntity2);
					}
					if (blockChangeInfo.blockValue.isair)
					{
						flag3 = true;
						if (chunk != null)
						{
							chunk.RemoveTileEntityAt<TileEntity>(m_World, World.toBlock(blockChangeInfo.blockValueRef));
							tileEntity2 = null;
						}
					}
					else if (tileEntity != tileEntity2)
					{
						flag3 = true;
						tileEntity2?.UpgradeDowngradeFrom(tileEntity);
					}
					if (flag3 && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && LockManager.Instance.IsLockedServer(tileEntity, 0))
					{
						LockManager.Instance.ForceUnlockLockTarget(tileEntity);
					}
				}
				if (chunk != null && blockChangeInfo.blockValue.isair)
				{
					chunk.RemoveBlockTrigger(World.toBlock(blockChangeInfo.blockValueRef));
				}
				if (bvOld.type != blockChangeInfo.blockValue.type)
				{
					Block block = blockChangeInfo.blockValue.Block;
					Block block2 = bvOld.Block;
					QuestEventManager.Current.BlockChanged(block2, block, blockChangeInfo.blockValueRef);
					if (block is BlockSleepingBag || block2 is BlockSleepingBag)
					{
						EntityAlive entityAlive = entity as EntityAlive;
						if ((bool)entityAlive)
						{
							if (block is BlockSleepingBag)
							{
								NavObjectManager.Instance.UnRegisterNavObjectByOwnerEntity(entityAlive, "sleeping_bag");
								entityAlive.SpawnPoints.Set(blockChangeInfo.blockValueRef);
							}
							else
							{
								persistentPlayers.SpawnPointRemoved(blockChangeInfo.blockValueRef);
							}
							flag = true;
						}
					}
				}
				if (blockChangeInfo.bChangeTexture)
				{
					chunkCluster.SetTextureFullArray(blockChangeInfo.blockValueRef, blockChangeInfo.textureFull);
				}
				else if (bvOld.Block.CanBlocksReplace)
				{
					chunkCluster.SetTextureFullArray(blockChangeInfo.blockValueRef, new TextureFullArray(0L));
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && flag)
			{
				persistentPlayers?.SavePersistentPlayerData();
			}
			if (num > 0)
			{
				int num4 = ccChanged.Count;
				for (int j = 0; j < num; j++)
				{
					ccChanged[--num4].ChunkPosNeedsRegeneration_DelayedStop();
				}
				ccChanged.RemoveRange(num4, num);
			}
		}
	}

	public void ChangeProps(PlatformUserIdentifierAbs persistentPlayerId, List<PropChangeInfo> propsToChange)
	{
		if (m_World == null)
		{
			return;
		}
		lock (ccChanged)
		{
			Log.Warning(string.Format("ChangeProps player {0}, count {1}", (persistentPlayerId != null) ? persistentPlayerId.ToString() : "", propsToChange.Count));
			ChunkCluster chunkCache = m_World.ChunkCache;
			int num = 0;
			if (chunkCache != null)
			{
				if (!ccChanged.Contains(chunkCache))
				{
					ccChanged.Add(chunkCache);
					num++;
					chunkCache.ChunkPosNeedsRegeneration_DelayedStart();
				}
				foreach (PropChangeInfo item in propsToChange)
				{
					if (chunkCache.GetChunkSync(item.ChunkPos) == null)
					{
						return;
					}
					chunkCache.SetProp(item.ChunkPos, item.PropId, item.Position, item.Rotation, item.Scale, item.BlockValue);
				}
			}
			if (num > 0)
			{
				int num2 = ccChanged.Count;
				for (int i = 0; i < num; i++)
				{
					ccChanged[--num2].ChunkPosNeedsRegeneration_DelayedStop();
				}
				ccChanged.RemoveRange(num2, num);
			}
		}
	}

	public void SetBlocksRPC(List<BlockChangeInfo> _changes, PlatformUserIdentifierAbs _persistentPlayerId = null)
	{
		ChangeBlocks(_persistentPlayerId, _changes);
		NetPackageSetBlock package = NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(persistentLocalPlayer, _changes, IsDedicatedServer ? (-1) : myPlayerId);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SetBlocksOnClients(-1, package);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public void SetPropsRPC(List<PropChangeInfo> _changes, PlatformUserIdentifierAbs _persistentPlayerId = null)
	{
		ChangeProps(_persistentPlayerId, _changes);
		NetPackageSetProp package = NetPackageManager.GetPackage<NetPackageSetProp>().Setup(persistentLocalPlayer, _changes, IsDedicatedServer ? (-1) : myPlayerId);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SetPropsOnClients(-1, package);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public void SetBlocksOnClients(int _exceptThisEntityId, NetPackageSetBlock package)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, -1, _exceptThisEntityId);
	}

	public void SetPropsOnClients(int _exceptThisEntityId, NetPackageSetProp package)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, -1, _exceptThisEntityId);
	}

	public void SetWaterRPC(NetPackageWaterSet package)
	{
		if (m_World != null)
		{
			ChunkCluster chunkCache = m_World.ChunkCache;
			if (chunkCache != null)
			{
				package.ApplyChanges(chunkCache);
			}
		}
		package.SetSenderId(IsDedicatedServer ? (-1) : myPlayerId);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockParticles()
	{
		lock (blockParticlesToSpawn)
		{
			for (int i = 0; i < blockParticlesToSpawn.Count; i++)
			{
				if (m_BlockParticles.ContainsKey(blockParticlesToSpawn[i].blockPos))
				{
					RemoveBlockParticleEffect(blockParticlesToSpawn[i].blockPos);
				}
				Transform value = Instance.SpawnParticleEffectClientForceCreation(blockParticlesToSpawn[i].particleEffect, -1, _worldSpawn: true);
				m_BlockParticles[blockParticlesToSpawn[i].blockPos] = value;
			}
			blockParticlesToSpawn.Clear();
		}
	}

	public void SpawnBlockParticleEffect(Vector3i _blockPos, ParticleEffect _pe)
	{
		lock (blockParticlesToSpawn)
		{
			blockParticlesToSpawn.Add(new BlockParticleCreationData(_blockPos, _pe));
		}
	}

	public bool HasBlockParticleEffect(Vector3i _blockPos)
	{
		return m_BlockParticles.ContainsKey(_blockPos);
	}

	public Transform GetBlockParticleEffect(Vector3i _blockPos)
	{
		return m_BlockParticles[_blockPos];
	}

	public void RemoveBlockParticleEffect(Vector3i _blockPos)
	{
		lock (blockParticlesToSpawn)
		{
			if (m_BlockParticles.ContainsKey(_blockPos))
			{
				Transform transform = m_BlockParticles[_blockPos];
				m_BlockParticles.Remove(_blockPos);
				if (transform != null)
				{
					UnityEngine.Object.Destroy(transform.gameObject);
				}
				return;
			}
			for (int num = blockParticlesToSpawn.Count - 1; num >= 0; num--)
			{
				if (blockParticlesToSpawn[num].blockPos == _blockPos)
				{
					blockParticlesToSpawn.RemoveAt(num);
				}
			}
		}
	}

	public void SpawnParticleEffectServer(ParticleEffect _pe, int _entityId, bool _forceCreation = false, bool _worldSpawn = false)
	{
		if (m_World != null)
		{
			ParticleEffect.SpawnParticleEffect(_pe, _entityId, _forceCreation, _worldSpawn);
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, _forceCreation, _worldSpawn));
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, _forceCreation, _worldSpawn), _onlyClientsAttachedToAnEntity: false, -1, _entityId);
			}
		}
	}

	public Transform SpawnParticleEffectClientForceCreation(ParticleEffect _pe, int _entityThatCausedIt, bool _worldSpawn)
	{
		return ParticleEffect.SpawnParticleEffect(_pe, _entityThatCausedIt, _forceCreation: true, _worldSpawn);
	}

	public void SpawnParticleEffectClient(ParticleEffect _pe, int _entityThatCausedIt, bool _forceCreation = false, bool _worldSpawn = false)
	{
		ParticleEffect.SpawnParticleEffect(_pe, _entityThatCausedIt, _forceCreation, _worldSpawn);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsInit()
	{
		Physics.ContactEvent += PhysicsContactEvent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsContactEvent(PhysicsScene scene, NativeArray<ContactPairHeader>.ReadOnly pairHeaders)
	{
		int length = pairHeaders.Length;
		for (int i = 0; i < length; i++)
		{
			Rigidbody rigidbody = pairHeaders[i].Body as Rigidbody;
			if ((bool)rigidbody)
			{
				EntityFallingBlocks component2;
				if (rigidbody.TryGetComponent<EntityFallingBlock>(out var component))
				{
					component.OnContactEvent();
				}
				else if (rigidbody.TryGetComponent<EntityFallingBlocks>(out component2))
				{
					component2.OnContactEvent();
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsEditMode()
	{
		return isEditMode;
	}

	public void GameMessage(EnumGameMessages _type, EntityAlive _mainEntity, EntityAlive _otherEntity)
	{
		if (_mainEntity == null)
		{
			return;
		}
		int num = -1;
		int secondaryEntityId = -1;
		if (_mainEntity is EntityPlayer)
		{
			switch (_type)
			{
			default:
				return;
			case EnumGameMessages.EntityWasKilled:
				num = _mainEntity.entityId;
				if (_otherEntity is EntityPlayer)
				{
					secondaryEntityId = _otherEntity.entityId;
				}
				break;
			case EnumGameMessages.JoinedGame:
			case EnumGameMessages.LeftGame:
			case EnumGameMessages.Chat:
				num = _mainEntity.entityId;
				break;
			case EnumGameMessages.PlainTextLocal:
			case EnumGameMessages.ChangedTeam:
				return;
			}
			GameMessageServer(null, _type, num, secondaryEntityId);
		}
		else if (_type == EnumGameMessages.EntityWasKilled || _type == EnumGameMessages.Chat)
		{
			num = ((_mainEntity != null) ? _mainEntity.entityId : (-1));
			GameMessageServer(null, _type, num, secondaryEntityId);
		}
	}

	public void GameMessageServer(ClientInfo _cInfo, EnumGameMessages _type, int _mainEntityId, int _secondaryEntityId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageGameMessage>().Setup(_type, _mainEntityId, _secondaryEntityId));
			return;
		}
		Entity entity = World.GetEntity(_mainEntityId);
		string mainName = ((!(entity is EntityPlayer entityPlayer)) ? ((!(entity is EntityAlive entityAlive)) ? Localization.Get("xuiChatServer") : Localization.Get(entityAlive.EntityName)) : entityPlayer.PlayerDisplayName);
		FinishGameMessageServer(_cInfo, _type, _mainEntityId, _secondaryEntityId, mainName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FinishGameMessageServer(ClientInfo _cInfo, EnumGameMessages _type, int _mainEntityId, int _secondaryEntityId, string _mainName)
	{
		string secondaryName = persistentPlayers.GetPlayerDataFromEntityID(_secondaryEntityId)?.PlayerName.DisplayName;
		ModEvents.SGameMessageData _data = new ModEvents.SGameMessageData(_cInfo, _type, _mainName, secondaryName);
		(ModEvents.EModEventResult, Mod) tuple = ModEvents.GameMessage.Invoke(ref _data);
		ModEvents.EModEventResult item = tuple.Item1;
		Mod item2 = tuple.Item2;
		string text = DisplayGameMessage(_type, _mainEntityId, _secondaryEntityId, item2 == null);
		if (item != ModEvents.EModEventResult.StopHandlersAndVanilla)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameMessage>().Setup(_type, _mainEntityId, _secondaryEntityId), _onlyClientsAttachedToAnEntity: true);
			return;
		}
		Log.Out("GameMessage handled by mod '{0}': {1}", item2.Name, text);
	}

	public string DisplayGameMessage(EnumGameMessages _type, int _mainEntity, int _secondaryEntity = -1, bool _log = true)
	{
		string result = null;
		string text = persistentPlayers.GetPlayerDataFromEntityID(_mainEntity)?.PlayerName?.DisplayName;
		string text2 = ((_secondaryEntity == -1) ? null : persistentPlayers.GetPlayerDataFromEntityID(_secondaryEntity)?.PlayerName?.DisplayName);
		string message;
		switch (_type)
		{
		case EnumGameMessages.EntityWasKilled:
			if (!string.IsNullOrEmpty(text2))
			{
				result = $"GMSG: Player '{text}' killed by '{text2}'";
				message = string.Format(Localization.Get("killedGameMessage"), text2, text);
			}
			else
			{
				result = $"GMSG: Player '{text}' died";
				message = string.Format(Localization.Get("diedGameMessage"), text);
			}
			break;
		case EnumGameMessages.JoinedGame:
			result = $"GMSG: Player '{text}' joined the game";
			message = string.Format(Localization.Get("joinGameMessage"), text);
			break;
		case EnumGameMessages.LeftGame:
			result = $"GMSG: Player '{text}' left the game";
			message = string.Format(Localization.Get("leaveGameMessage"), text);
			break;
		case EnumGameMessages.BlockedPlayerAlert:
			result = $"GMSG: Blocked player '{text}' is present on this server!";
			message = string.Format("[FF0000A0]" + Localization.Get("blockedPlayerMessage"), text);
			break;
		default:
			return result;
		}
		if (_log)
		{
			Log.Out(result);
		}
		if (!IsDedicatedServer)
		{
			if (_type == EnumGameMessages.BlockedPlayerAlert)
			{
				XUiC_ChatOutput.AddMessage(myEntityPlayerLocal.PlayerUI.xui, _type, message);
			}
			else
			{
				foreach (EntityPlayerLocal localPlayer in m_World.GetLocalPlayers())
				{
					XUiC_ChatOutput.AddMessage(LocalPlayerUI.GetUIForPlayer(localPlayer).xui, _type, message);
				}
			}
		}
		return result;
	}

	public void ChatMessageServer(ClientInfo _cInfo, EChatType _chatType, int _senderEntityId, string _msg, List<int> _recipientEntityIds, EMessageSender _msgSender, GeneratedTextManager.BbCodeSupportMode _bbMode = GeneratedTextManager.BbCodeSupportMode.Supported)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			string text = null;
			if (_senderEntityId != -1)
			{
				text = Utils.EscapeBbCodes(persistentPlayers.GetPlayerDataFromEntityID(_senderEntityId)?.PlayerName?.AuthoredName?.Text);
			}
			ModEvents.SChatMessageData _data = new ModEvents.SChatMessageData(_cInfo, _chatType, _senderEntityId, _msg, text, _recipientEntityIds);
			var (eModEventResult, mod) = ModEvents.ChatMessage.Invoke(ref _data);
			ChatMessageClient(_chatType, _senderEntityId, _msg, _recipientEntityIds, _msgSender, GeneratedTextManager.BbCodeSupportMode.Supported);
			string text2 = ((_cInfo?.PlatformId != null) ? _cInfo.PlatformId.CombinedString : "-non-player-");
			string text3 = string.Format("Chat (from '{0}', entity id '{1}', to '{2}'): {3}{4}", text2, _senderEntityId, _chatType.ToStringCached(), (text != null) ? ("'" + text + "': ") : "", _msg);
			if (eModEventResult == ModEvents.EModEventResult.StopHandlersAndVanilla)
			{
				Log.Out("Chat handled by mod '{0}': {1}", mod.Name, text3);
			}
			else
			{
				Log.Out(text3);
			}
			if (eModEventResult == ModEvents.EModEventResult.StopHandlersAndVanilla)
			{
				return;
			}
			if (_recipientEntityIds != null)
			{
				foreach (int _recipientEntityId in _recipientEntityIds)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(_recipientEntityId)?.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(_chatType, _senderEntityId, _msg, null, _msgSender, _bbMode));
				}
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(_chatType, _senderEntityId, _msg, null, _msgSender, _bbMode), _onlyClientsAttachedToAnEntity: true);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageChat>().Setup(_chatType, _senderEntityId, _msg, _recipientEntityIds, _msgSender, _bbMode));
		}
	}

	public void ChatMessageClient(EChatType _chatType, int _senderEntityId, string _msg, List<int> _recipientEntityIds, EMessageSender _msgSender, GeneratedTextManager.BbCodeSupportMode _bbMode)
	{
		if (IsDedicatedServer)
		{
			return;
		}
		foreach (EntityPlayerLocal localPlayer in m_World.GetLocalPlayers())
		{
			if (_recipientEntityIds == null || _recipientEntityIds.Contains(localPlayer.entityId))
			{
				XUiC_ChatOutput.AddMessage(LocalPlayerUI.GetUIForPlayer(localPlayer).xui, EnumGameMessages.Chat, _msg, _chatType, EChatDirection.Inbound, _senderEntityId, null, _senderEntityId.ToString(), _msgSender, GeneratedTextManager.TextFilteringMode.Filter, _bbMode);
			}
		}
	}

	public void RemoveChunk(long _chunkKey)
	{
		m_World.m_ChunkManager.RemoveChunk(_chunkKey);
	}

	public IBlockTool GetActiveBlockTool()
	{
		if (activeBlockTool == null)
		{
			return blockSelectionTool;
		}
		return activeBlockTool;
	}

	public void SetActiveBlockTool(IBlockTool _tool)
	{
		activeBlockTool = _tool;
	}

	public DynamicPrefabDecorator GetDynamicPrefabDecorator()
	{
		if (m_World == null)
		{
			return null;
		}
		return m_World.ChunkCache?.ChunkProvider.GetDynamicPrefabDecorator();
	}

	public void SimpleRPC(int _entityId, SimpleRPCType _rpcType, bool _bExeLocal, bool _bOnlyLocal)
	{
		if (_bExeLocal)
		{
			EntityAlive entityAlive = (EntityAlive)m_World.GetEntity(_entityId);
			if (entityAlive != null)
			{
				switch (_rpcType)
				{
				case SimpleRPCType.OnActivateItem:
					entityAlive.inventory.holdingItem.OnHoldingItemActivated(entityAlive.inventory.holdingItemData);
					break;
				case SimpleRPCType.OnResetItem:
					entityAlive.inventory.holdingItem.OnHoldingReset(entityAlive.inventory.holdingItemData);
					break;
				}
			}
		}
		if (!_bOnlyLocal)
		{
			NetPackage package = NetPackageManager.GetPackage<NetPackageSimpleRPC>().Setup(_entityId, _rpcType);
			if (m_World.IsRemote())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
			}
			else
			{
				m_World.entityDistributer.SendPacketToTrackedPlayers(_entityId, _entityId, package);
			}
		}
	}

	public void ItemDropServer(ItemStack _itemStack, Vector3 _dropPos, Vector3 _randomPosAdd, int _entityId = -1, float _lifetime = 60f, bool _bDropPosIsRelativeToHead = false)
	{
		ItemDropServer(_itemStack, _dropPos, _randomPosAdd, Vector3.zero, _entityId, _lifetime, _bDropPosIsRelativeToHead);
	}

	public void ItemDropServer(ItemStack _itemStack, Vector3 _dropPos, Vector3 _randomPosAdd, Vector3 _initialMotion, int _entityId = -1, float _lifetime = 60f, bool _bDropPosIsRelativeToHead = false, int _clientEntityId = 0)
	{
		if (m_World == null)
		{
			return;
		}
		bool flag = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		Entity entity = m_World.GetEntity(_entityId);
		if (_clientEntityId != 0)
		{
			if (!entity)
			{
				return;
			}
			flag = !entity.isEntityRemote;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (_clientEntityId == -1)
			{
				_clientEntityId = --m_World.clientLastEntityId;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageItemDrop>().Setup(_itemStack, _dropPos, _initialMotion, _randomPosAdd, _lifetime, _entityId, _bDropPosIsRelativeToHead, _clientEntityId));
			if (!flag)
			{
				return;
			}
		}
		if (_bDropPosIsRelativeToHead)
		{
			if (entity == null)
			{
				return;
			}
			_dropPos += entity.getHeadPosition();
		}
		if (!_randomPosAdd.Equals(Vector3.zero))
		{
			_dropPos += new Vector3(m_World.RandomRange(0f - _randomPosAdd.x, _randomPosAdd.x), m_World.RandomRange(0f - _randomPosAdd.y, _randomPosAdd.y), m_World.RandomRange(0f - _randomPosAdd.z, _randomPosAdd.z));
		}
		EntityCreationData entityCreationData = new EntityCreationData();
		entityCreationData.entityClass = EntityClass.FromString("item");
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && _clientEntityId < -1)
		{
			entityCreationData.id = _clientEntityId;
		}
		else
		{
			entityCreationData.id = EntityFactory.nextEntityID++;
		}
		entityCreationData.itemStack = _itemStack.Clone();
		entityCreationData.pos = _dropPos;
		entityCreationData.rot = new Vector3(20f, 0f, 20f);
		entityCreationData.lifetime = _lifetime;
		entityCreationData.belongsPlayerId = _entityId;
		if (_clientEntityId != -1)
		{
			entityCreationData.clientEntityId = _clientEntityId;
		}
		EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(entityCreationData);
		entityItem.isPhysicsMaster = flag;
		if (_initialMotion.sqrMagnitude > 0.01f)
		{
			entityItem.AddVelocity(_initialMotion);
		}
		m_World.SpawnEntityInWorld(entityItem);
		Chunk chunk = (Chunk)m_World.GetChunkSync(World.toChunkXZ((int)_dropPos.x), World.toChunkXZ((int)_dropPos.z));
		if (chunk == null)
		{
			return;
		}
		List<EntityItem> list = new List<EntityItem>();
		for (int i = 0; i < chunk.entityLists.Length; i++)
		{
			if (chunk.entityLists[i] == null)
			{
				continue;
			}
			for (int j = 0; j < chunk.entityLists[i].Count; j++)
			{
				if (chunk.entityLists[i][j] is EntityItem)
				{
					list.Add(chunk.entityLists[i][j] as EntityItem);
				}
			}
		}
		int num = list.Count - 50;
		if (num > 0)
		{
			list.Sort(new EntityItemLifetimeComparer());
			int num2 = list.Count - 1;
			while (num2 >= 0 && num > 0)
			{
				list[num2].MarkToUnload();
				num--;
				num2--;
			}
		}
	}

	public void AddExpServer(int _entityId, string UNUSED_skill, int _experience)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAddExpServer>().Setup(_entityId, _experience));
		}
	}

	public void AddScoreServer(int _entityId, int _zombieKills, int _playerKills, int _otherTeamnumber, int _conditions)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAddScoreServer>().Setup(_entityId, _zombieKills, _playerKills, _otherTeamnumber, _conditions));
			return;
		}
		EntityAlive entityAlive = (EntityAlive)m_World.GetEntity(_entityId);
		if (!(entityAlive == null))
		{
			if (entityAlive.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAddScoreClient>().Setup(_entityId, _zombieKills, _playerKills, _otherTeamnumber, _conditions), _onlyClientsAttachedToAnEntity: false, entityAlive.entityId);
			}
			else
			{
				entityAlive.AddScore(0, _zombieKills, _playerKills, _otherTeamnumber, _conditions);
			}
		}
	}

	public void AwardKill(EntityAlive killer, EntityAlive killedEntity)
	{
		if (killer.isEntityRemote)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAwardKillServer>().Setup(killer.entityId, killedEntity.entityId), _onlyClientsAttachedToAnEntity: false, killer.entityId);
		}
		else
		{
			QuestEventManager.Current.EntityKilled(killer, killedEntity);
		}
	}

	public void ItemReloadServer(int _entityId)
	{
		if (m_World != null)
		{
			ItemReloadClient(_entityId);
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageItemReload>().Setup(_entityId));
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageItemReload>().Setup(_entityId), _onlyClientsAttachedToAnEntity: false, -1, _entityId);
			}
		}
	}

	public void ItemReloadClient(int _entityId)
	{
		if (m_World != null)
		{
			EntityAlive entityAlive = (EntityAlive)m_World.GetEntity(_entityId);
			if (entityAlive != null && entityAlive.inventory.IsHoldingGun())
			{
				entityAlive.inventory.GetHoldingGun().ReloadGun(entityAlive.inventory.holdingItemData.actionData[0]);
			}
		}
	}

	public void ItemActionEffectsServer(int _entityId, int _slotIdx, int _itemActionIdx, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		if (m_World == null)
		{
			return;
		}
		ItemActionEffectsClient(_entityId, _slotIdx, _itemActionIdx, _firingState, _startPos, _direction, _userData);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageItemActionEffects>().Setup(_entityId, _slotIdx, _itemActionIdx, (ItemActionFiringState)_firingState, _startPos, _direction, _userData));
			return;
		}
		int allButAttachedToEntityId = _entityId;
		Entity entity = m_World.GetEntity(_entityId);
		if (entity != null && entity.AttachedMainEntity != null)
		{
			allButAttachedToEntityId = entity.AttachedMainEntity.entityId;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageItemActionEffects>().Setup(_entityId, _slotIdx, _itemActionIdx, (ItemActionFiringState)_firingState, _startPos, _direction, _userData), _onlyClientsAttachedToAnEntity: false, -1, allButAttachedToEntityId, _entityId);
	}

	public void ItemActionEffectsClient(int _entityId, int _slotIdx, int _itemActionIdx, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		if (m_World != null)
		{
			EntityAlive entityAlive = (EntityAlive)m_World.GetEntity(_entityId);
			if (!(entityAlive == null))
			{
				entityAlive.inventory.GetItemActionInSlot(_slotIdx, _itemActionIdx)?.ItemActionEffects(this, entityAlive.inventory.GetItemActionDataInSlot(_slotIdx, _itemActionIdx), _firingState, _startPos, _direction, _userData);
			}
		}
	}

	public void SetWorldTime(ulong _worldTime)
	{
		if (m_World != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				_worldTime = m_World.worldTime;
			}
			m_World.SetTime(_worldTime);
		}
	}

	public void AddVelocityToEntityServer(int _entityId, Vector3 _velToAdd)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAddVelocity>().Setup(_entityId, _velToAdd));
			return;
		}
		Entity entity = m_World.GetEntity(_entityId);
		if (entity != null)
		{
			entity.AddVelocity(_velToAdd);
		}
	}

	public void PickupBlockServer(Vector3i _blockPos, BlockValue _blockValue, int _playerId, PlatformUserIdentifierAbs persistentPlayerId = null)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePickupBlock>().Setup(_blockPos, _blockValue, _playerId, persistentLocalPlayer));
		}
		else if (m_World.GetBlock(_blockPos).type == _blockValue.type)
		{
			if (m_World.IsLocalPlayer(_playerId))
			{
				PickupBlockClient(_blockPos, _blockValue, _playerId);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePickupBlock>().Setup(_blockPos, _blockValue, _playerId, null), _onlyClientsAttachedToAnEntity: false, _playerId);
			}
			BlockValue blockValue = ((_blockValue.Block.PickupSource != null) ? Block.GetBlockValue(_blockValue.Block.PickupSource) : BlockValue.Air);
			SetBlocksRPC(new List<BlockChangeInfo>
			{
				new BlockChangeInfo(_blockPos, blockValue, _updateLight: true)
			}, persistentPlayerId);
		}
	}

	public void PickupBlockClient(Vector3i _blockPos, BlockValue _blockValue, int _playerId)
	{
		if (m_World.GetBlock(_blockPos).type != _blockValue.type)
		{
			return;
		}
		ItemStack itemStack = _blockValue.Block.OnBlockPickedUp(m_World, _blockPos, _blockValue, _playerId);
		QuestEventManager.Current.BlockPickedUp(_blockValue.Block.GetBlockName(), _blockPos);
		QuestEventManager.Current.ItemAdded(itemStack);
		foreach (EntityPlayerLocal localPlayer in m_World.GetLocalPlayers())
		{
			if (localPlayer.entityId == _playerId && localPlayer.PlayerUI.xui.PlayerInventory.AddItem(itemStack, _playCollectSound: true))
			{
				return;
			}
		}
		ItemDropServer(itemStack, _blockPos.ToVector3() + Vector3.one * 0.5f, Vector3.zero, _playerId);
	}

	public void PlaySoundAtPositionServer(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance, float _volumeScale = 1f)
	{
		PlaySoundAtPositionServer(_pos, _audioClipName, _mode, _distance, m_World.GetPrimaryPlayerId(), _volumeScale);
	}

	public void PlaySoundAtPositionServer(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance, int _entityId, float _volumeScale = 1f)
	{
		if (m_World == null)
		{
			return;
		}
		if (!IsDedicatedServer)
		{
			Manager.BroadcastPlay(_pos, _audioClipName);
			if (m_World.aiDirector != null)
			{
				m_World.aiDirector.NotifyNoise(m_World.GetEntity(_entityId), _pos, _audioClipName, _volumeScale);
			}
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(_pos, _audioClipName, _mode, _distance, _entityId, _volumeScale));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(_pos, _audioClipName, _mode, _distance, _entityId, _volumeScale), _onlyClientsAttachedToAnEntity: false, -1, _entityId);
		}
	}

	public void PlaySoundAtPositionClient(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance, float _volumeScale)
	{
		if (m_World != null)
		{
			Manager.Play(_pos, _audioClipName, -1, wantHandle: false, _volumeScale);
			if (m_World.aiDirector != null)
			{
				m_World.aiDirector.NotifyNoise(null, _pos, _audioClipName, _volumeScale);
			}
		}
	}

	public void WaypointInviteServer(Waypoint _waypoint, EnumWaypointInviteMode _inviteMode, int _inviterEntityId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWaypoint>().Setup(_waypoint, _inviteMode, _inviterEntityId));
			return;
		}
		_waypoint = _waypoint.Clone();
		_waypoint.bTracked = false;
		if (_inviteMode == EnumWaypointInviteMode.Friends)
		{
			if (m_World.GetEntity(_inviterEntityId) as EntityPlayer == null)
			{
				return;
			}
			PersistentPlayerData playerDataFromEntityID = persistentPlayers.GetPlayerDataFromEntityID(_inviterEntityId);
			if (playerDataFromEntityID == null)
			{
				return;
			}
			for (int i = 0; i < m_World.Players.list.Count; i++)
			{
				EntityPlayer entityPlayer = m_World.Players.list[i];
				if (entityPlayer.entityId == _inviterEntityId)
				{
					continue;
				}
				PersistentPlayerData other = ((persistentPlayers != null) ? persistentPlayers.GetPlayerDataFromEntityID(entityPlayer.entityId) : null);
				if (playerDataFromEntityID.IsAlly(other))
				{
					if (m_World.IsLocalPlayer(entityPlayer.entityId))
					{
						WaypointInviteClient(_waypoint, _inviteMode, _inviterEntityId);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWaypoint>().Setup(_waypoint, _inviteMode, _inviterEntityId), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
					}
				}
			}
			return;
		}
		for (int j = 0; j < m_World.Players.list.Count; j++)
		{
			EntityPlayer entityPlayer2 = m_World.Players.list[j];
			if (entityPlayer2.entityId != _inviterEntityId)
			{
				if (m_World.IsLocalPlayer(entityPlayer2.entityId))
				{
					WaypointInviteClient(_waypoint, _inviteMode, _inviterEntityId);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWaypoint>().Setup(_waypoint, _inviteMode, _inviterEntityId), _onlyClientsAttachedToAnEntity: false, entityPlayer2.entityId);
				}
			}
		}
	}

	public void RemovePartyInvitesFromAllPlayers(EntityPlayer _player)
	{
		for (int i = 0; i < m_World.Players.list.Count; i++)
		{
			EntityPlayer entityPlayer = m_World.Players.list[i];
			if (entityPlayer != _player)
			{
				entityPlayer.RemovePartyInvite(_player.entityId);
			}
		}
	}

	public void WaypointInviteClient(Waypoint _waypoint, EnumWaypointInviteMode _inviteMode, int _inviterEntityId, EntityPlayerLocal _player = null)
	{
		if (_player == null)
		{
			_player = myEntityPlayerLocal;
		}
		if (_player == null)
		{
			return;
		}
		PersistentPlayerData playerDataFromEntityID = persistentPlayers.GetPlayerDataFromEntityID(_inviterEntityId);
		if ((playerDataFromEntityID != null && playerDataFromEntityID.PlatformData.Blocked[EBlockType.TextChat].IsBlocked()) || _player.Waypoints.ContainsWaypoint(_waypoint))
		{
			return;
		}
		for (int i = 0; i < _player.WaypointInvites.Count; i++)
		{
			if (_player.WaypointInvites[i].Equals(_waypoint))
			{
				return;
			}
		}
		_player.WaypointInvites.Insert(0, _waypoint);
		XUiV_Window window = LocalPlayerUI.GetUIForPlayer(_player).xui.GetWindow("mapInvites");
		if (window != null && window.IsVisible)
		{
			((XUiC_MapInvitesList)window.Controller.GetChildById("invitesList")).UpdateInvitesList();
		}
		string strPlayerName = "?";
		EntityPlayer entityPlayer = m_World.GetEntity(_inviterEntityId) as EntityPlayer;
		if (entityPlayer != null)
		{
			strPlayerName = entityPlayer.PlayerDisplayName;
		}
		GeneratedTextManager.GetDisplayText(_waypoint.name, [PublicizedFrom(EAccessModifier.Internal)] (string _filtered) =>
		{
			ShowTooltip(_player, string.Format(Localization.Get("tooltipInviteMarker"), strPlayerName, _waypoint.bUsingLocalizationId ? Localization.Get(_filtered) : _filtered));
		}, _runCallbackIfReadyNow: true, _checkBlockState: false);
	}

	public void QuestShareServer(NetPackageSharedQuest.SharedQuestData sqd)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(sqd));
		}
		else if (m_World.IsLocalPlayer(sqd.sharedWithEntityID))
		{
			QuestShareClient(sqd);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(sqd), _onlyClientsAttachedToAnEntity: false, sqd.sharedWithEntityID);
		}
	}

	public void QuestShareClient(NetPackageSharedQuest.SharedQuestData sqd, EntityPlayerLocal _player = null)
	{
		if (_player == null)
		{
			_player = myEntityPlayerLocal;
		}
		if (_player == null)
		{
			return;
		}
		if (_player.QuestJournal.HasActiveQuestByQuestCode(sqd.questCode))
		{
			if (PartyQuests.AutoAccept)
			{
				Log.Out($"Ignoring received quest, already have one active with the quest code {sqd.questCode}:");
				for (int i = 0; i < _player.QuestJournal.quests.Count; i++)
				{
					Quest quest = _player.QuestJournal.quests[i];
					Log.Out(string.Format("  {0}.: id={1}, code={2}, name={3}, POI={4}, state={5}, owner={6}", i, quest.ID, quest.QuestCode, quest.QuestClass.Name, quest.GetParsedText("{poi.name}"), quest.CurrentState, quest.SharedOwnerID));
				}
			}
		}
		else
		{
			_player.AddSharedQuestEntry(new NetPackageSharedQuest.SharedQuestData(sqd));
		}
	}

	public void SharedKillServer(int _entityID, int _killerID, float _xpModifier = 1f)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSharedPartyKill>().Setup(_entityID, _killerID));
			return;
		}
		EntityPlayer entityPlayer = (EntityPlayer)m_World.GetEntity(_killerID);
		EntityAlive entityAlive = m_World.GetEntity(_entityID) as EntityAlive;
		if (entityPlayer == null || entityAlive == null)
		{
			return;
		}
		int experienceValue = EntityClass.list[entityAlive.entityClass].ExperienceValue;
		experienceValue = (int)EffectManager.GetValue(PassiveEffects.ExperienceGain, entityAlive.inventory.holdingItemItemValue, experienceValue, entityAlive);
		if (_xpModifier != 1f)
		{
			experienceValue = (int)((float)experienceValue * _xpModifier + 0.5f);
		}
		if (entityPlayer.IsInParty())
		{
			int num = entityPlayer.Party.MemberCountInRange(entityPlayer);
			experienceValue = (int)((float)experienceValue * (1f - 0.1f * (float)num));
		}
		if (entityPlayer.Party == null)
		{
			return;
		}
		for (int i = 0; i < entityPlayer.Party.MemberList.Count; i++)
		{
			EntityPlayer entityPlayer2 = entityPlayer.Party.MemberList[i];
			if (!(entityPlayer2 == entityPlayer) && Vector3.Distance(entityPlayer.position, entityPlayer2.position) < (float)GameStats.GetInt(EnumGameStats.PartySharedKillRange))
			{
				if (m_World.IsLocalPlayer(entityPlayer2.entityId))
				{
					SharedKillClient(entityAlive.entityClass, experienceValue, null, entityAlive.entityId);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedPartyKill>().Setup(entityAlive.entityClass, experienceValue, _killerID, entityAlive.entityId), _onlyClientsAttachedToAnEntity: false, entityPlayer2.entityId);
				}
			}
		}
	}

	public void SharedKillClient(int _entityTypeID, int _xp, EntityPlayerLocal _player = null, int _entityID = -1, int _killerID = -1)
	{
		if (_player == null)
		{
			_player = myEntityPlayerLocal;
		}
		if (!(_player == null))
		{
			_ = EntityClass.list[_entityTypeID].entityClassName;
			_xp = _player.Progression.AddLevelExp(_xp, "_xpFromParty", Progression.XPTypes.Kill, useBonus: true, notifyUI: true, _killerID);
			_player.bPlayerStatsChanged = true;
			if (_xp > 0 && (Progression.ShowXPType == Progression.ShowXPTypes.All || Progression.ShowXPType == Progression.ShowXPTypes.NotificationsOnly))
			{
				ShowTooltip(_player, string.Format(Localization.Get("ttPartySharedXPReceived"), _xp));
			}
			QuestEventManager.Current.EntityKilled(_player, (_entityID == -1) ? null : (m_World.GetEntity(_entityID) as EntityAlive));
		}
	}

	public IEnumerator ShowExitingGameUICoroutine()
	{
		bool flag = windowManager.IsWindowOpen(XUiC_ExitingGame.ID);
		windowManager.Open(XUiC_ExitingGame.ID, _bModal: false);
		if (!flag)
		{
			yield return null;
			yield return null;
		}
	}

	public static void ShowTooltipMP(EntityPlayer _player, string _text, string _alertSound = "")
	{
		if (_player is EntityPlayerLocal)
		{
			ShowTooltip(_player as EntityPlayerLocal, _text, string.Empty, _alertSound);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(_text, _alertSound), _onlyClientsAttachedToAnEntity: false, _player.entityId);
		}
	}

	public static void ShowTooltip(EntityPlayerLocal _player, string _text, bool _showImmediately = false, bool _pinTooltip = false, float _timeout = 0f)
	{
		ShowTooltip(_player, _text, (string[])null, (string)null, (ToolTipEvent)null, _showImmediately, _pinTooltip, _timeout);
	}

	public static void ShowTooltip(EntityPlayerLocal _player, string _text, string _arg, string _alertSound = null, ToolTipEvent _handler = null, bool _showImmediately = false, bool _pinTooltip = false, float _timeout = 0f)
	{
		ShowTooltip(_player, _text, new string[1] { _arg }, _alertSound, _handler, _showImmediately, _pinTooltip: false, _timeout);
	}

	public static void ShowTooltip(EntityPlayerLocal _player, string _text, string[] _args, string _alertSound = null, ToolTipEvent _handler = null, bool _showImmediately = false, bool _pinTooltip = false, float _timeout = 0f)
	{
		if (!IsDedicatedServer && !(_player == null))
		{
			XUiC_PopupToolTip.QueueTooltip(LocalPlayerUI.GetUIForPlayer(_player).nguiWindowManager.WindowManager.playerUI.xui, _text, _args, _alertSound, _handler, _showImmediately, _pinTooltip, _timeout);
		}
	}

	public static void RemovePinnedTooltip(EntityPlayerLocal _player, string _key)
	{
		if (!IsDedicatedServer && !(_player == null))
		{
			XUiC_PopupToolTip.RemovePinnedTooltip(LocalPlayerUI.GetUIForPlayer(_player).nguiWindowManager.WindowManager.playerUI.xui, _key);
		}
	}

	public void ClearTooltips(NGUIWindowManager _nguiWindowManager)
	{
		if (!IsDedicatedServer)
		{
			XUiC_PopupToolTip.ClearTooltips(_nguiWindowManager.WindowManager.playerUI.xui);
		}
	}

	public void ClearCurrentTooltip(NGUIWindowManager _nguiWindowManager)
	{
		if (!IsDedicatedServer)
		{
			XUiC_PopupToolTip.ClearCurrentTooltip(_nguiWindowManager.WindowManager.playerUI.xui);
		}
	}

	public void SetToolTipPause(NGUIWindowManager _nguiWindowManager, bool _isPaused)
	{
		if (!IsDedicatedServer)
		{
			XUiC_PopupToolTip.SetToolTipPause(_nguiWindowManager.WindowManager.playerUI.xui, _isPaused);
		}
	}

	public static void ShowSubtitle(XUi _xui, string speaker, string content, float duration, bool centerAlign = false)
	{
		XUiC_SubtitlesDisplay.DisplaySubtitle(_xui.playerUI, speaker, content, duration, centerAlign);
	}

	public static void PlayVideo(string id, bool skippable, XUiC_VideoPlayer.DelegateOnVideoFinished callback = null)
	{
		XUiC_VideoPlayer.PlayVideo(LocalPlayerUI.primaryUI.xui, VideoManager.GetVideoData(id), skippable, callback);
	}

	public static bool IsVideoPlaying()
	{
		return XUiC_VideoPlayer.IsVideoPlaying;
	}

	public void CheckDestroyTileEntity(ITileEntity _te, Vector3i _blockPos)
	{
		if (_te is ITileEntityLootable tileEntityLootable && tileEntityLootable.ShouldDestroyOnClose())
		{
			BlockValue block = m_World.GetBlock(_blockPos);
			DropContentOfLootContainerServer(block, _blockPos);
			block.Block.DamageBlock(m_World, _blockPos, block, block.Block.MaxDamage, -1);
		}
	}

	public void DropContentOfLootContainerServer(BlockValue _bvOld, Vector3i _worldPos, ITileEntityLootable _teOld = null)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("DropContentOfLootContainerServer can not be called on clients! From:\n" + StackTraceUtility.ExtractStackTrace());
			return;
		}
		string text = "DroppedLootContainer";
		ITileEntityLootable tileEntityLootable = ((ITileEntity)(((object)_teOld) ?? ((object)m_World.GetTileEntity(_worldPos))))?.GetSelfOrFeature<ITileEntityLootable>();
		if (tileEntityLootable == null || LockManager.Instance.IsLockedServer(tileEntityLootable, 0))
		{
			return;
		}
		ITileEntityLootable tileEntityLootable2 = tileEntityLootable;
		Vector3 transformPos = tileEntityLootable2.ToWorldPos().ToVector3() + new Vector3(0.5f, 0.75f, 0.5f);
		if (_bvOld.Block.Properties.Values.ContainsKey("DroppedEntityClass"))
		{
			text = _bvOld.Block.Properties.Values["DroppedEntityClass"];
		}
		if (!tileEntityLootable2.bTouched)
		{
			lootManager.LootContainerOpened(tileEntityLootable2, -1, _bvOld.Block.Tags);
		}
		if (!tileEntityLootable2.IsEmpty())
		{
			EntityLootContainer entityLootContainer = EntityFactory.CreateEntity(text.GetHashCode(), transformPos, Vector3.zero) as EntityLootContainer;
			if (entityLootContainer != null)
			{
				entityLootContainer.SetContent(ItemStack.Clone(tileEntityLootable2.items));
			}
			m_World.SpawnEntityInWorld(entityLootContainer);
		}
		tileEntityLootable2.SetEmpty();
	}

	public void DropContentInLootContainerServer(int _droppedByID, string _containerEntity, Vector3 _pos, ItemStack[] _items, bool _skipIfEmpty = false, Vector3? increment = null)
	{
		List<EntityLootContainer> list = new List<EntityLootContainer>();
		if (_skipIfEmpty && ItemStack.IsEmpty(_items))
		{
			return;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageDropItemsContainer>().Setup(_droppedByID, _containerEntity, _pos, _items));
			return;
		}
		_pos.y += 0.25f;
		int hashCode = _containerEntity.GetHashCode();
		Vector2i size = LootContainer.GetLootContainer(EntityClass.list[hashCode].Properties.GetString(EntityClass.PropLootList)).size;
		int val = size.x * size.y;
		int num = 0;
		int num2 = _items.Length;
		while (num < num2)
		{
			EntityLootContainer entityLootContainer = EntityFactory.CreateEntity(_containerEntity.GetHashCode(), _pos, Vector3.zero) as EntityLootContainer;
			if (increment.HasValue)
			{
				_pos += increment.Value;
			}
			if ((bool)entityLootContainer)
			{
				int val2 = num2 - num;
				int num3 = Math.Min(val, val2);
				entityLootContainer.SetContent(ItemStack.Clone(_items, num, num3));
				entityLootContainer.spawnById = _droppedByID;
				m_World.SpawnEntityInWorld(entityLootContainer);
				num += num3;
				list.Add(entityLootContainer);
			}
		}
	}

	public GameStateManager GetGameStateManager()
	{
		return gameStateManager;
	}

	public void IdMappingReceived(string _name, byte[] _data)
	{
		Log.Out("Received mapping data for: " + _name);
		if (!(_name == "blocks"))
		{
			if (_name == "items")
			{
				ItemClass.nameIdMapping = new NameIdMapping(null, ItemClass.MAX_ITEMS);
				ItemClass.nameIdMapping.LoadFromArray(_data);
			}
			else
			{
				Log.Warning("Unknown mapping received for: " + _name);
			}
		}
		else
		{
			Block.nameIdMapping = new NameIdMapping(null, Block.MAX_BLOCKS);
			Block.nameIdMapping.LoadFromArray(_data);
		}
	}

	public void SetSpawnPointList(SpawnPointList _startPoints)
	{
		StartCoroutine(setSpawnPointListCo(_startPoints));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setSpawnPointListCo(SpawnPointList _startPoints)
	{
		while (!chunkClusterLoaded && SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield return null;
		}
		if (chunkClusterLoaded)
		{
			m_World.ChunkCache.ChunkProvider.SetSpawnPointList(_startPoints);
		}
	}

	public void RequestToSpawnEntityServer(EntityCreationData _ecd)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRequestToSpawnEntity>().Setup(_ecd));
			return;
		}
		if (_ecd.entityClass == "fallingTree".GetHashCode())
		{
			for (int i = 0; i < m_World.Entities.list.Count; i++)
			{
				if (m_World.Entities.list[i] is EntityFallingTree && ((EntityFallingTree)m_World.Entities.list[i]).GetBlockPos() == _ecd.blockPos)
				{
					return;
				}
			}
		}
		Entity entity = EntityFactory.CreateEntity(_ecd);
		if (entity is EntityBackpack entityBackpack)
		{
			foreach (PersistentPlayerData value in persistentPlayers.Players.Values)
			{
				if (value.EntityId == entityBackpack.RefPlayerId)
				{
					uint timestamp = GameUtils.WorldTimeToTotalMinutes(m_World.worldTime);
					value.AddDroppedBackpack(entity.entityId, new Vector3i(_ecd.pos), timestamp);
					break;
				}
			}
		}
		m_World.SpawnEntityInWorld(entity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalPlayerInventoryChanged()
	{
		countdownSendPlayerInventoryToServer.ResetAndRestart();
	}

	public void TriggerSendOfLocalPlayerDataFile(float _sendItInSeconds)
	{
		countdownSendPlayerDataFileToServer.SetPassedIn(_sendItInSeconds);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doSendLocalInventory(EntityPlayerLocal _player)
	{
		if (!(_player == null))
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerInventory>().Setup(_player, sendPlayerToolbelt, sendPlayerBag, sendPlayerEquipment, sendDragAndDropItem));
			sendPlayerToolbelt = false;
			sendPlayerBag = false;
			sendPlayerEquipment = false;
			sendDragAndDropItem = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doSendLocalPlayerData(EntityPlayerLocal _player)
	{
		if (!(_player == null))
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SaveLocalPlayerData();
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerData>().Setup(_player));
			sendPlayerToolbelt = false;
			sendPlayerBag = false;
			sendPlayerEquipment = false;
			sendDragAndDropItem = false;
		}
	}

	public void SetPauseWindowEffects(bool _bOn)
	{
		if (!_bOn || GameModeSurvivalSP.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			return;
		}
		foreach (EntityPlayerLocal localPlayer in m_World.GetLocalPlayers())
		{
			if (localPlayer.AimingGun)
			{
				localPlayer.AimingGun = false;
			}
		}
	}

	public static bool ReportUnusedAssets(bool bStart = false)
	{
		if (bStart)
		{
			if (materialsBefore == null)
			{
				materialsBefore = new List<string>();
			}
			else
			{
				materialsBefore.Clear();
			}
			Material[] array = Resources.FindObjectsOfTypeAll<Material>();
			for (int i = 0; i < array.Length; i++)
			{
				materialsBefore.Add(array[i].name);
			}
			Resources.UnloadUnusedAssets();
			GC.Collect();
			runningAssetsUnused = true;
			unusedAssetsTimer = Time.realtimeSinceStartup;
			Instance.Pause(_bOn: true);
		}
		else
		{
			if (materialsBefore == null)
			{
				return true;
			}
			if (!runningAssetsUnused)
			{
				return true;
			}
			if (Time.realtimeSinceStartup < unusedAssetsTimer + 5f)
			{
				return false;
			}
			Material[] array2 = Resources.FindObjectsOfTypeAll<Material>();
			if (materialsBefore.Count == array2.Length)
			{
				Log.Out("No unused assets found. ( " + materialsBefore.Count + " materials found. )");
			}
			else
			{
				Log.Out("Material before: " + materialsBefore.Count);
				Log.Out("Material after: " + array2.Length);
				string text = "Material Diff: ";
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				for (int j = 0; j < array2.Length; j++)
				{
					if (dictionary.TryGetValue(array2[j].name, out var value))
					{
						value++;
					}
					else
					{
						dictionary.Add(array2[j].name, 1);
					}
				}
				for (int k = 0; k < materialsBefore.Count; k++)
				{
					if (!dictionary.ContainsKey(materialsBefore[k]))
					{
						text = text + materialsBefore[k] + ", ";
					}
				}
				Log.Out(text);
			}
			Instance.Pause(_bOn: false);
			runningAssetsUnused = false;
		}
		return true;
	}

	public bool IsPaused()
	{
		return gamePaused;
	}

	public void Pause(bool _bOn)
	{
		requestedPauseState = _bOn;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePauseState()
	{
		if (!requestedPauseState.HasValue)
		{
			return;
		}
		bool flag = requestedPauseState.Value;
		requestedPauseState = null;
		if (flag == gamePaused)
		{
			return;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer || GameModeEditWorld.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)) || GameStats.GetInt(EnumGameStats.GameState) == 0)
		{
			flag = false;
		}
		SetPauseWindowEffects(flag);
		if (flag)
		{
			GameStats.Set(EnumGameStats.GameState, 2);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SaveLocalPlayerData();
				SaveWorld();
			}
			Time.timeScale = 0f;
			if (World.GetPrimaryPlayer() != null)
			{
				triggerEffectManager.StopGamepadVibration();
			}
		}
		else
		{
			if (GameStats.GetInt(EnumGameStats.GameState) != 0)
			{
				GameStats.Set(EnumGameStats.GameState, 1);
			}
			Time.timeScale = 1f;
		}
		if (gamePaused != flag && World != null)
		{
			if (flag)
			{
				Manager.PauseGameplayAudio();
				EnvironmentAudioManager.Instance.Pause();
				m_World.dmsConductor.OnPauseGame();
			}
			else
			{
				Manager.UnPauseGameplayAudio();
				EnvironmentAudioManager.Instance.UnPause();
				m_World.dmsConductor.OnUnPauseGame();
			}
		}
		gamePaused = flag;
	}

	public void AddLMPPersistentPlayerData(EntityPlayerLocal _playerEntity)
	{
	}

	public void SetBlockTextureServer(BlockValueRef _bvRef, BlockFace _blockFace, int _idx, int _playerIdThatChanged, byte _channel = byte.MaxValue)
	{
		SetBlockTextureClient(_bvRef, _blockFace, _idx, _channel);
		NetPackageSetBlockTexture package = NetPackageManager.GetPackage<NetPackageSetBlockTexture>().Setup(_bvRef, _blockFace, _idx, IsDedicatedServer ? (-1) : myPlayerId, _channel);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public void SetBlockTextureClient(BlockValueRef _bvRef, BlockFace _blockFace, int _idx, byte _channel)
	{
		DynamicMeshManager.ChunkChanged(_bvRef, -1, 1);
		int num;
		int num2;
		if (_channel == byte.MaxValue)
		{
			num = 0;
			num2 = 0;
		}
		else
		{
			if (_channel >= 1)
			{
				Log.Error($"Specified texture channel \"{_channel}\" is out of range of the project channel count of \"{1}\".");
				return;
			}
			num = (num2 = _channel);
		}
		for (int i = num; i <= num2; i++)
		{
			if (_blockFace != BlockFace.None)
			{
				m_World.ChunkCache.SetBlockFaceTexture(_bvRef, _blockFace, _idx, i);
				continue;
			}
			long num3 = _idx;
			long textureFull = num3 | (num3 << 8) | (num3 << 16) | (num3 << 24) | (num3 << 32) | (num3 << 40);
			m_World.ChunkCache.SetTextureFull(_bvRef, textureFull, i);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleGlobalActions()
	{
		if (!IsDedicatedServer)
		{
			if (PlayerActionsGlobal.Instance.Console.WasPressed)
			{
				GUIWindowConsole.Open();
			}
			if (PlayerActionsGlobal.Instance.Fullscreen.WasPressed)
			{
				Screen.fullScreen = !Screen.fullScreen;
			}
			if (PlayerActionsGlobal.Instance.Screenshot.WasPressed)
			{
				Manager.PlayButtonClick();
				GameUtils.TakeScreenShot(GameUtils.EScreenshotMode.Both, null, 0f, _b4to3: false, 0, 0, InputUtils.ControlKeyPressed);
			}
			if (PlayerActionsGlobal.Instance.DebugScreenshot.WasPressed)
			{
				Manager.PlayButtonClick();
				LocalPlayerUI.primaryUI.windowManager.Open(GUIWindowScreenshotText.ID, _bModal: false);
			}
			if (LocalPlayerUI.primaryUI != null && PlatformManager.NativePlatform?.Input.PrimaryPlayer?.GUIActions?.FocusSearch?.WasPressed == true)
			{
				XUiC_TextInput.SelectCurrentSearchField(LocalPlayerUI.primaryUI);
			}
			LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
			if (uIForPrimaryPlayer != null && uIForPrimaryPlayer.playerInput?.GUIActions?.FocusSearch?.WasPressed == true)
			{
				XUiC_TextInput.SelectCurrentSearchField(uIForPrimaryPlayer);
			}
		}
	}

	public static bool IsSplatMapAvailable()
	{
		string text = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		if (text == "Empty" || text == "Playtesting")
		{
			return false;
		}
		return true;
	}

	public static void LoadRemoteResources(RemoteResourcesCompleteHandler _callback = null)
	{
		if (!UpdatingRemoteResources)
		{
			NewsManager.Instance.UpdateNews();
			if (BlockedPlayerList.Instance != null)
			{
				Instance.StartCoroutine(BlockedPlayerList.Instance.ReadStorageAndResolve());
			}
			DLCTitleStorageManager.Instance.FetchFromSource();
			if (PlatformManager.NativePlatform.User.UserStatus == EUserStatus.LoggedIn)
			{
				Instance.StartCoroutine(Instance.UpdateRemoteResourcesRoutine(_callback));
				return;
			}
			UpdatingRemoteResources = false;
			RemoteResourcesLoaded = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UpdateRemoteResourcesRoutine(RemoteResourcesCompleteHandler _callback)
	{
		IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
		if (storage == null)
		{
			RemoteResourcesLoaded = true;
			yield break;
		}
		UpdatingRemoteResources = true;
		float readyTime = Time.time;
		while (!storage.IsReady)
		{
			yield return null;
			if (Time.time - readyTime > 3f)
			{
				Log.Warning("Waiting for remote resources timed out");
				UpdatingRemoteResources = false;
				RemoteResourcesLoaded = true;
				yield break;
			}
		}
		retrievingEula = true;
		string filename = $"eula_{Localization.ActiveLanguage.ToLower()}";
		storage.GetFile(filename, EulaProviderCallback);
		while (retrievingEula)
		{
			yield return null;
		}
		if (BacktraceUtils.Initialized)
		{
			retrievingBacktraceConfig = true;
			storage.GetFile("backtraceconfig.xml", BacktraceConfigProviderCallback);
			while (retrievingBacktraceConfig)
			{
				yield return null;
			}
		}
		UpdatingRemoteResources = false;
		RemoteResourcesLoaded = true;
		_callback?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EulaProviderCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
	{
		retrievingEula = false;
		string contents;
		if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
		{
			Log.Warning("Retrieving EULA file failed: " + _result.ToStringCached() + " (" + _errorDetails + ")");
		}
		else if (LoadEulaXML(_data, out contents))
		{
			XUiC_EulaWindow.RetrievedEula = contents;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LoadEulaXML(byte[] _data, out string contents)
	{
		contents = "";
		XmlFile xmlFile;
		try
		{
			xmlFile = new XmlFile(_data, _throwExc: true);
		}
		catch (Exception ex)
		{
			Log.Error("Failed loading EULA XML: {0}", ex.Message);
			return false;
		}
		XElement root = xmlFile.XmlDoc.Root;
		if (root == null)
		{
			return false;
		}
		int num = int.Parse(root.GetAttribute("version").Trim());
		contents = root.Value;
		if (num > GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion))
		{
			GamePrefs.Set(EnumGamePrefs.EulaLatestVersion, num);
		}
		return true;
	}

	public static bool HasAcceptedLatestEula()
	{
		return GamePrefs.GetInt(EnumGamePrefs.EulaVersionAccepted) >= GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BacktraceConfigProviderCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
	{
		retrievingBacktraceConfig = false;
		if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
		{
			Log.Warning("Retrieving Backtrace config file failed: " + _result.ToStringCached() + " (" + _errorDetails + ")");
			return;
		}
		try
		{
			BacktraceUtils.UpdateConfig(new XmlFile(_data, _throwExc: true));
		}
		catch (Exception ex)
		{
			Log.Error("Failed loading Backtrace config XML: {0}", ex.Message);
		}
	}

	public bool IsGoreCensored()
	{
		if (DebugCensorship)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculatePersistentPlayerCount(string worldName, string saveName, UserDataStorageType storage)
	{
		persistentPlayerIds = new List<string>();
		string path = GameIO.GetSaveGameDir(worldName, saveName, storage) + "/Player";
		if (!SdDirectory.Exists(path))
		{
			Log.Warning("save folder does not exist");
			return;
		}
		SdFileSystemInfo[] fileSystemInfos = new SdDirectoryInfo(path).GetFileSystemInfos();
		foreach (SdFileSystemInfo sdFileSystemInfo in fileSystemInfos)
		{
			int length;
			string item = (((length = sdFileSystemInfo.Name.IndexOf('.')) == -1) ? sdFileSystemInfo.Name : sdFileSystemInfo.Name.Substring(0, length));
			if (!persistentPlayerIds.Contains(item))
			{
				persistentPlayerIds.Add(item);
			}
		}
	}

	public void OnResolutionChanged(int width, int height)
	{
		RefreshRefreshRate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshRefreshRate()
	{
		currentRefreshRate = (int)PlatformApplicationManager.Application.GetCurrentRefreshRate().value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFPSCap()
	{
		if (!IsDedicatedServer)
		{
			int num = (GameHasStarted ? (-1) : currentRefreshRate);
			if (Application.targetFrameRate != num)
			{
				Application.targetFrameRate = num;
			}
		}
	}
}

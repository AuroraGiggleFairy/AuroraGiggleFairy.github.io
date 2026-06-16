using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio;
using Platform;
using UnityEngine;

public class XUi : MonoBehaviour
{
	public enum EStackPanelPos
	{
		Left,
		Center,
		Right
	}

	public class StackPanel
	{
		public readonly string Name;

		public readonly EStackPanelPos Position;

		public readonly Transform Transform;

		public int WindowCount;

		public Vector2Int Size;

		public StackPanel(string _name, EStackPanelPos _position, Transform _transform)
		{
			Name = _name;
			Position = _position;
			Transform = _transform;
		}
	}

	public enum UISoundType
	{
		ClickSound,
		ScrollSound,
		ConfirmSound,
		BackSound,
		SliderSound,
		None
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int lastScreenHeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float pixelRatioFactor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static UIRoot uiRoot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool inGameMenuOpen;

	public NGUIFont[] NGUIFonts;

	public List<XUiWindowGroup> WindowGroups;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Type, List<XUiController>> windowsAndGroupsByControllerType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiV_Window> windows;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float backgroundGlobalOpacity = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float foregroundGlobalOpacity = 1f;

	public XUiM_PlayerInventory PlayerInventory;

	public XUiM_PlayerEquipment PlayerEquipment;

	public XUiM_Vehicle Vehicle;

	public XUiM_AssembleItem AssembleItem;

	public XUiM_Quest QuestTracker;

	public XUiM_Recipes Recipes;

	public XUiM_Trader Trader;

	public XUiM_Dialog Dialog;

	public XUiC_BuffPopoutList BuffPopoutList;

	public XUiC_CollectedItemList CollectedItemList;

	public XUiC_Radial RadialWindow;

	public XUiC_GamepadCalloutWindow CalloutWindow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float xuiGlobalScaling = 1.255f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i lastScreenSize = Vector2i.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool mIsReady;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool asyncLoad;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LoadManager.LoadGroup loadGroup;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LocalPlayerUI mPlayerUI;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public long accumElapsedMilliseconds;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine loadAsyncCoroutine;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<UIBasicSprite> getXUiWindowWorldBoundsList = new List<UIBasicSprite>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultStackPanelScale = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform stackPanelRoot;

	public readonly DictionaryList<string, StackPanel> StackPanels = new DictionaryList<string, StackPanel>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip uiScrollSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float uiScrollVolume = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip uiClickSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float uiClickVolume = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip uiConfirmSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float uiConfirmVolume = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip uiBackSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float uiBackVolume = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip uiSliderSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float uiSliderVolume = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, MultiSourceAtlasManager> allMultiSourceAtlases = new CaseInsensitiveStringDictionary<MultiSourceAtlasManager>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CaseInsensitiveStringDictionary<NGUIFont> fontsByName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform anchorRoot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UIAnchor[] xuiAnchors;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 previousPagingVector = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float pagingRepeatTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialPagingInput;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<XUiV_Window> forceInputStyleChangedWindowsList = new List<XUiV_Window>();

	public static bool InGameMenuOpen
	{
		get
		{
			return inGameMenuOpen;
		}
		set
		{
			inGameMenuOpen = value;
			World world = GameManager.Instance.World;
			if (world == null)
			{
				return;
			}
			EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
			if (primaryPlayer == null)
			{
				return;
			}
			LocalPlayerUI localPlayerUI = primaryPlayer.PlayerUI;
			if (!(localPlayerUI == null))
			{
				NGUIWindowManager nguiWindowManager = localPlayerUI.nguiWindowManager;
				if (!(nguiWindowManager == null))
				{
					GameManager.Instance.SetToolTipPause(nguiWindowManager, value);
				}
			}
		}
	}

	public float BackgroundGlobalOpacity
	{
		get
		{
			return backgroundGlobalOpacity;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			backgroundGlobalOpacity = value;
		}
	}

	public float ForegroundGlobalOpacity
	{
		get
		{
			return foregroundGlobalOpacity;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			foregroundGlobalOpacity = value;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DragAndDropWindow DragAndDropWindow { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OnScreenIcons OnScreenIconsWindow { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToolTip ToolTipWindow { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PopupMenu PopupMenuWindow { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SaveIndicator SaveIndicator { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationToolGrid CurrentWorkstationToolGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationFuelGrid CurrentWorkstationFuelGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationInputGrid CurrentWorkstationInputGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationOutputGrid CurrentWorkstationOutputGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorModGrid CurrentCollectorModGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CollectorFuelGrid CurrentCollectorFuelGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CollectorFuelGrid currentCollectorCatalystGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CombineGrid CurrentCombineGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerSourceSlots CurrentPowerSourceSlots { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerRangedAmmoSlots CurrentPowerAmmoSlots { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SelectableEntry CurrentSelectedEntry { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public ProgressionValue SelectedSkill { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityLootable LootContainer { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsUsingItemActionEntryUse { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsUsingItemActionEntryPromptComplete { get; set; }

	public bool IsReady
	{
		get
		{
			return mIsReady;
		}
		set
		{
			mIsReady = value;
			if (mIsReady)
			{
				this.OnBuilt?.Invoke();
			}
		}
	}

	public LocalPlayerUI playerUI
	{
		get
		{
			if (mPlayerUI == null)
			{
				mPlayerUI = GetComponentInParent<LocalPlayerUI>();
			}
			return mPlayerUI;
		}
	}

	public Transform StackPanelTransform => stackPanelRoot;

	public event Action OnShutdown;

	public event Action OnBuilt;

	public static bool IsGameRunning()
	{
		if (GameManager.Instance != null)
		{
			return GameManager.Instance.World != null;
		}
		return false;
	}

	public static XUi Instantiate(LocalPlayerUI _playerUI)
	{
		if (GameManager.IsDedicatedServer)
		{
			return null;
		}
		Log.Out("[XUi] Instantiating XUi from default prefab.");
		Transform obj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/XUi")).transform;
		obj.name = "XUi";
		obj.parent = _playerUI.transform;
		uiRoot = UnityEngine.Object.FindAnyObjectByType<UIRoot>();
		XUi component = obj.GetComponent<XUi>();
		component.SetScale();
		component.gameObject.SetActive(value: true);
		component.Init(_playerUI.isPrimaryUI);
		return component;
	}

	public static void Reload(LocalPlayerUI _playerUI)
	{
		if (_playerUI.xui != null)
		{
			_playerUI.xui.Shutdown(_destroyImmediate: true);
		}
		SetXmlsForUi(_playerUI.isPrimaryUI);
		XUi xUi = Instantiate(_playerUI);
		xUi.Load();
		xUi.SetDataConnections();
	}

	public static bool ReloadWindow(LocalPlayerUI _playerUI, string _windowGroupName)
	{
		if (_playerUI.xui == null)
		{
			Log.Error("Can not reload single window, XUi not instantiated");
			return false;
		}
		for (int i = 0; i < _playerUI.xui.WindowGroups.Count; i++)
		{
			XUiWindowGroup xUiWindowGroup = _playerUI.xui.WindowGroups[i];
			if (xUiWindowGroup.Id.EqualsCaseInsensitive(_windowGroupName))
			{
				_playerUI.windowManager.Remove(xUiWindowGroup);
				_playerUI.xui.WindowGroups.RemoveAt(i);
				xUiWindowGroup.Cleanup();
				break;
			}
		}
		SetXmlsForUi(_playerUI.isPrimaryUI);
		_playerUI.xui.Load(new List<string> { _windowGroupName });
		_playerUI.xui.IsReady = true;
		return _playerUI.xui.WindowGroups.Find([PublicizedFrom(EAccessModifier.Internal)] (XUiWindowGroup _group) => _group.Id.EqualsCaseInsensitive(_windowGroupName)) != null;
	}

	public static void SetXmlsForUi(bool _menuUi)
	{
		XUiFromXml.ClearData();
		ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Common/styles"));
		ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Common/templates"));
		if (!_menuUi)
		{
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_InGame/styles"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_InGame/templates"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_InGame/windows"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_InGame/xui"));
		}
		else
		{
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Menu/styles"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Menu/templates"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Menu/windows"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Menu/xui"));
		}
	}

	public static IEnumerator PatchAndLoadXuiXml(string _relPathXuiFile)
	{
		MicroStopwatch timer = null;
		bool coroutineHadException = false;
		XmlFile xmlFile = null;
		yield return XmlPatcher.LoadAndPatchConfig(_relPathXuiFile, [PublicizedFrom(EAccessModifier.Internal)] (XmlFile _file) =>
		{
			xmlFile = _file;
		});
		yield return XmlPatcher.ApplyConditionalXmlBlocks(_relPathXuiFile, xmlFile, timer, XmlPatcher.EEvaluator.Host, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			coroutineHadException = true;
		});
		if (!coroutineHadException)
		{
			yield return XmlPatcher.ApplyConditionalXmlBlocks(_relPathXuiFile, xmlFile, timer, XmlPatcher.EEvaluator.Client, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				coroutineHadException = true;
			});
			if (!coroutineHadException)
			{
				yield return XUiFromXml.Load(xmlFile);
			}
		}
	}

	public void Awake()
	{
		WindowGroups = new List<XUiWindowGroup>();
		windowsAndGroupsByControllerType = new Dictionary<Type, List<XUiController>>();
		initFonts();
	}

	public void Init(bool _menuUi)
	{
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
		GameOptionsManager.ResolutionChanged += OnResolutionChanged;
		loadGroup = LoadManager.CreateGroup();
		windows = new List<XUiV_Window>();
		lastScreenSize = new Vector2i(playerUI.camera.pixelWidth, playerUI.camera.pixelHeight);
		if (!_menuUi)
		{
			base.gameObject.GetOrAddComponent<XUi_FallThrough>().SetXUi(this);
		}
		initStackPanels();
		initAtlases();
		if (Application.isPlaying)
		{
			initSounds();
		}
		initAnchors();
		updateAnchors();
		BackgroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
		ForegroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		LocalPlayerManager.OnLocalPlayersChanged += handleLocalPlayersChanged;
		Vehicle = new XUiM_Vehicle();
		AssembleItem = new XUiM_AssembleItem();
		QuestTracker = new XUiM_Quest();
		Recipes = new XUiM_Recipes();
		Trader = new XUiM_Trader();
		Dialog = new XUiM_Dialog();
	}

	public void Load(List<string> _windowGroupSubset = null, bool _async = false)
	{
		if (_async)
		{
			asyncLoad = true;
			loadAsyncCoroutine = ThreadManager.StartCoroutine(loadAsync(_windowGroupSubset));
			return;
		}
		asyncLoad = false;
		if (!XUiFromXml.HasData())
		{
			Log.Error("Loading XUi synchronously failed: XMLs not set.");
		}
		else
		{
			ThreadManager.RunCoroutineSync(loadAsync(_windowGroupSubset));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator loadAsync(List<string> _windowGroupSubset = null)
	{
		yield return null;
		while (!XUiFromXml.HasData())
		{
			yield return null;
		}
		MicroStopwatch msw = new MicroStopwatch();
		Log.Out("[XUi] Loading XUi " + (asyncLoad ? "asynchronously" : "synchronously"));
		List<string> asyncWindowGroupList = ((_windowGroupSubset != null) ? new List<string>(_windowGroupSubset) : new List<string>());
		if (_windowGroupSubset == null)
		{
			XUiFromXml.GetWindowGroupNames(out asyncWindowGroupList);
		}
		if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
		{
			yield return null;
			msw.ResetAndRestart();
		}
		accumElapsedMilliseconds = 0L;
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		foreach (string item in asyncWindowGroupList)
		{
			ms.Reset();
			ms.Start();
			XUiFromXml.LoadXui(this, item);
			accumElapsedMilliseconds += ms.ElapsedMilliseconds;
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Out("[XUi] Parsing window group, {0}, completed in {1} ms.", item, ms.ElapsedMilliseconds);
			}
			if (msw.ElapsedMilliseconds > 20)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		XUiFromXml.LoadDone(_windowGroupSubset == null);
		Log.Out("[XUi] Parsing all window groups completed in {0} ms total.", accumElapsedMilliseconds);
		if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
		{
			yield return null;
			msw.ResetAndRestart();
		}
		accumElapsedMilliseconds = 0L;
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			XUiWindowGroup xUiWindowGroup = WindowGroups[i];
			ms.Reset();
			ms.Start();
			try
			{
				xUiWindowGroup.Init();
			}
			catch (Exception e)
			{
				Log.Error("[XUi] Failed initializing window group " + xUiWindowGroup.Id);
				Log.Exception(e);
			}
			accumElapsedMilliseconds += ms.ElapsedMilliseconds;
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Out("[XUi] Initialize window group, {0}, completed in {1} ms.", xUiWindowGroup.Id, ms.ElapsedMilliseconds);
			}
			if (msw.ElapsedMilliseconds > 20)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		Log.Out("[XUi] Initialized all window groups completed in {0} ms total.", accumElapsedMilliseconds);
		while (loadGroup.Pending)
		{
			yield return null;
		}
		postLoadInit();
		IsReady = _windowGroupSubset == null;
		loadAsyncCoroutine = null;
		XUiUpdater.Add(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void postLoadInit()
	{
		windowsAndGroupsByControllerType.Clear();
		foreach (XUiWindowGroup windowGroup in WindowGroups)
		{
			if (!windowGroup.Initialized)
			{
				continue;
			}
			addWindowOrGroupControllerTypeEntry(windowGroup.Controller);
			foreach (XUiController child in windowGroup.Controller.Children)
			{
				addWindowOrGroupControllerTypeEntry(child);
			}
		}
		if (WorldStaticData.LoadAllXmlsCoComplete)
		{
			XUiFromXml.ClearLoadingData();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.ResolutionChanged -= OnResolutionChanged;
		GamePrefs.OnGamePrefChanged -= OnGamePrefChanged;
		LocalPlayerManager.OnLocalPlayersChanged -= handleLocalPlayersChanged;
		Shutdown();
	}

	public void Shutdown(bool _destroyImmediate = false)
	{
		this.OnShutdown?.Invoke();
		Cleanup(_destroyImmediate);
	}

	public void Cleanup(bool _destroyImmediate = false)
	{
		CancelLoading();
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			XUiWindowGroup xUiWindowGroup = WindowGroups[i];
			playerUI.windowManager.Remove(xUiWindowGroup);
			xUiWindowGroup.Cleanup();
		}
		WindowGroups.Clear();
		XUiUpdater.Remove(this);
		if (_destroyImmediate)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
	}

	public void CancelLoading()
	{
		if (loadAsyncCoroutine != null)
		{
			ThreadManager.StopCoroutine(loadAsyncCoroutine);
			loadAsyncCoroutine = null;
		}
	}

	public void StopAllVideo()
	{
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			StopVideo(WindowGroups[i].Controller);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void StopVideo(XUiController controller)
		{
			if (controller.ViewComponent is XUiV_Video xUiV_Video)
			{
				xUiV_Video.Playing = false;
			}
			foreach (XUiController child in controller.Children)
			{
				StopVideo(child);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addWindowOrGroupControllerTypeEntry(XUiController _controller)
	{
		Type type = _controller.GetType();
		if (!windowsAndGroupsByControllerType.TryGetValue(type, out var value))
		{
			value = new List<XUiController>();
			windowsAndGroupsByControllerType.Add(type, value);
		}
		value.Add(_controller);
	}

	public void AddWindow(XUiV_Window _window)
	{
		windows.Add(_window);
	}

	public void RemoveWindow(XUiV_Window _window)
	{
		windows.Remove(_window);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDataConnections()
	{
		if (!(playerUI.entityPlayer == null))
		{
			if (PlayerInventory == null)
			{
				PlayerInventory = new XUiM_PlayerInventory(this, playerUI.entityPlayer);
			}
			if (PlayerEquipment == null)
			{
				PlayerEquipment = new XUiM_PlayerEquipment(this, playerUI.entityPlayer);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _obj)
	{
		switch (_obj)
		{
		case EnumGamePrefs.OptionsScreenBoundsValue:
			SetScale();
			updateAnchors();
			RecenterWindowGroup(null, _forceImmediate: true);
			break;
		case EnumGamePrefs.OptionsHudSize:
			SetScale();
			break;
		case EnumGamePrefs.OptionsBackgroundGlobalOpacity:
			BackgroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
			break;
		case EnumGamePrefs.OptionsForegroundGlobalOpacity:
			ForegroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnResolutionChanged(int _arg1, int _arg2)
	{
		ThreadManager.StartCoroutine(delayedScaleUpdate());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delayedScaleUpdate()
	{
		yield return null;
		SetScale();
	}

	public void SetScale(float _scale = -1f)
	{
		if (_scale > 0f)
		{
			xuiGlobalScaling = _scale;
		}
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
		float activeUiScale = GameOptionsManager.GetActiveUiScale();
		base.transform.localScale = xuiGlobalScaling * num * activeUiScale * Vector3.one;
		base.transform.localPosition = Vector3.zero;
		pixelRatioFactor = 0f;
	}

	public float GetScale()
	{
		return xuiGlobalScaling;
	}

	public void LoadData<T>(string _path, Action<T> _callback) where T : UnityEngine.Object
	{
		LoadManager.LoadAsset(_path, _callback, loadGroup, _deferLoading: false, !asyncLoad);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleLocalPlayersChanged()
	{
		lastScreenSize = new Vector2i(playerUI.camera.pixelWidth, playerUI.camera.pixelHeight);
		updateAnchors();
		RecenterWindowGroup(null, _forceImmediate: true);
		for (int i = 0; i < windows.Count; i++)
		{
			XUiV_Window xUiV_Window = windows[i];
			if (xUiV_Window.IsCursorArea && xUiV_Window.IsOpen)
			{
				UpdateWindowSoftCursorBounds(xUiV_Window);
			}
		}
	}

	public void OnUpdateDeltaTime(float _updateDeltaTime)
	{
		if (playerUI.entityPlayer != null)
		{
			SetDataConnections();
		}
		if (WindowGroups == null)
		{
			return;
		}
		if (ToolTipWindow != null)
		{
			playerUI.windowManager.Open(ToolTipWindow.WindowGroup, _bModal: false);
		}
		if (SaveIndicator != null)
		{
			playerUI.windowManager.Open(SaveIndicator.WindowGroup, _bModal: false);
		}
		if (lastScreenSize.x != playerUI.camera.pixelWidth || lastScreenSize.y != playerUI.camera.pixelHeight)
		{
			lastScreenSize = new Vector2i(playerUI.camera.pixelWidth, playerUI.camera.pixelHeight);
			updateAnchors();
			RecenterWindowGroup(null, _forceImmediate: true);
		}
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			XUiWindowGroup xUiWindowGroup = WindowGroups[i];
			if (xUiWindowGroup.Initialized && (xUiWindowGroup.isShowing || xUiWindowGroup.Controller.AlwaysUpdate))
			{
				try
				{
					xUiWindowGroup.Controller.Update(_updateDeltaTime);
				}
				catch (Exception e)
				{
					Log.Error("[XUi] Error while updating window group '" + xUiWindowGroup.Id + "':");
					Log.Exception(e);
				}
			}
		}
	}

	public float GetPixelRatioFactor()
	{
		if (pixelRatioFactor == 0f || Screen.height != lastScreenHeight)
		{
			float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
			float activeUiScale = GameOptionsManager.GetActiveUiScale();
			pixelRatioFactor = uiRoot.pixelSizeAdjustment / xuiGlobalScaling / num / activeUiScale;
			lastScreenHeight = Screen.height;
		}
		return pixelRatioFactor;
	}

	public Vector2i GetXUiScreenSize()
	{
		return new Vector2i(new Vector2(playerUI.camera.pixelWidth, playerUI.camera.pixelHeight) * GetPixelRatioFactor());
	}

	public Vector2i GetMouseXUiPosition()
	{
		Vector3 vector = playerUI.CursorController.GetLocalScreenPosition();
		return TranslateScreenVectorToXuiVector((Vector2)vector);
	}

	public Vector2i TranslateScreenVectorToXuiVector(Vector2 _screenSpaceVector)
	{
		_screenSpaceVector.x -= (float)playerUI.camera.pixelWidth / 2f;
		_screenSpaceVector.y -= (float)playerUI.camera.pixelHeight / 2f;
		return new Vector2i(_screenSpaceVector * GetPixelRatioFactor());
	}

	public Vector3 TranslateScreenVectorToXuiVector(Vector3 _screenSpaceVector)
	{
		_screenSpaceVector.x -= (float)playerUI.camera.pixelWidth / 2f;
		_screenSpaceVector.y -= (float)playerUI.camera.pixelHeight / 2f;
		return _screenSpaceVector * GetPixelRatioFactor();
	}

	public Bounds GetXUiWindowScreenBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds xUiWindowWorldBounds = getXUiWindowWorldBounds(_xuiElement, _includeInactive);
		Bounds result = new Bounds(playerUI.camera.WorldToScreenPoint(xUiWindowWorldBounds.min), Vector3.zero);
		result.Encapsulate(playerUI.camera.WorldToScreenPoint(xUiWindowWorldBounds.max));
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Bounds getXUiWindowWorldBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds result = new Bounds(Vector3.zero, Vector3.zero);
		bool flag = false;
		_xuiElement.GetComponentsInChildren(_includeInactive, getXUiWindowWorldBoundsList);
		List<UIBasicSprite> list = getXUiWindowWorldBoundsList;
		for (int i = 0; i < list.Count; i++)
		{
			UIBasicSprite uIBasicSprite = list[i];
			Transform parent = uIBasicSprite.transform.parent;
			if (parent != null && parent.name.Equals("MapSpriteEntity(Clone)"))
			{
				continue;
			}
			Vector3[] worldCorners = uIBasicSprite.worldCorners;
			for (int j = 0; j < worldCorners.Length; j++)
			{
				if (!flag)
				{
					result = new Bounds(worldCorners[j], Vector3.zero);
					flag = true;
				}
				else
				{
					result.Encapsulate(worldCorners[j]);
				}
			}
		}
		getXUiWindowWorldBoundsList.Clear();
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initStackPanels()
	{
		stackPanelRoot = base.transform.Find("StackPanels").transform;
		StackPanels.Add("Left", new StackPanel("Left", EStackPanelPos.Left, base.transform.Find("StackPanels/Left").transform));
		StackPanels.Add("Center", new StackPanel("Center", EStackPanelPos.Center, base.transform.Find("StackPanels/Center").transform));
		StackPanels.Add("Right", new StackPanel("Right", EStackPanelPos.Right, base.transform.Find("StackPanels/Right").transform));
	}

	public void SetStackPanelScale(float _scale)
	{
		defaultStackPanelScale = _scale;
		stackPanelRoot.localScale = Vector3.one * _scale;
		stackPanelRoot.localPosition = Vector3.zero;
	}

	public void RecenterWindowGroup(XUiWindowGroup _wg, bool _forceImmediate = false)
	{
		if (_forceImmediate || GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			if (_wg == null)
			{
				calculateWindowGroupLayouts();
			}
			else
			{
				calculateWindowGroupLayout(_wg);
			}
		}
		else if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(recenterLater(_wg));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator recenterLater(XUiWindowGroup _wg)
	{
		yield return null;
		if (_wg != null)
		{
			calculateWindowGroupLayout(_wg);
		}
		else
		{
			calculateWindowGroupLayouts();
		}
		yield return null;
		playerUI.CursorController.ResetNavigationTarget();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calculateWindowGroupLayouts()
	{
		foreach (XUiWindowGroup windowGroup in WindowGroups)
		{
			if (windowGroup.isShowing)
			{
				calculateWindowGroupLayout(windowGroup);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calculateWindowGroupLayout(XUiWindowGroup _wg)
	{
		if (_wg == null || _wg.Id == "backpack" || _wg.Controller?.Children == null || !_wg.HasStackPanelWindows())
		{
			return;
		}
		bool flag = false;
		int num = 0;
		int num2 = 0;
		foreach (StackPanel item in StackPanels.list)
		{
			bool flag2 = layoutWindowsInPanel(_wg, item);
			if (flag2 && num > 0)
			{
				num += _wg.StackPanelPadding;
			}
			item.Transform.localPosition = new Vector3(num, 0f, 0f);
			num += item.Size.x;
			num2 = Math.Max(num2, item.Size.y);
			flag = flag || flag2;
		}
		if (flag)
		{
			stackPanelRoot.localPosition = new Vector3(0f - defaultStackPanelScale * (float)num / 2f, _wg.StackPanelYOffset, 0f);
			stackPanelRoot.localScale = Vector3.one * defaultStackPanelScale;
		}
		if (flag && (!_wg.LeftPanelVAlignTop || !_wg.RightPanelVAlignTop))
		{
			if (!_wg.LeftPanelVAlignTop)
			{
				StackPanels.list[0].Transform.localPosition = new Vector3((int)StackPanels.list[0].Transform.position.x, -(num2 - StackPanels.list[0].Size.y), 0f);
			}
			if (!_wg.RightPanelVAlignTop)
			{
				StackPanels.list[2].Transform.localPosition = new Vector3((int)StackPanels.list[2].Transform.position.x, -(num2 - StackPanels.list[2].Size.y), 0f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool layoutWindowsInPanel(XUiWindowGroup _windowGroup, StackPanel _panel)
	{
		int windowCount = 0;
		int yPos = 0;
		int maxWidth = 0;
		LayoutWindowGroupWindows(_windowGroup);
		if (playerUI.windowManager.IsWindowOpen("backpack"))
		{
			LayoutWindowGroupWindows(GetWindowGroupById("backpack"));
		}
		_panel.Size.x = maxWidth;
		_panel.Size.y = -yPos;
		_panel.WindowCount = windowCount;
		return windowCount != 0;
		[PublicizedFrom(EAccessModifier.Internal)]
		void LayoutWindowGroupWindows(XUiWindowGroup _wg)
		{
			foreach (XUiController child in _wg.Controller.Children)
			{
				XUiV_Window xUiV_Window = (XUiV_Window)child.ViewComponent;
				if (xUiV_Window != null && xUiV_Window.IsActiveInHierarchy && !(child.ViewComponent.UiTransform.parent.name != _panel.Name) && xUiV_Window.Size.y > 0)
				{
					if (yPos < 0)
					{
						yPos -= _wg.StackPanelPadding;
					}
					xUiV_Window.Position = new Vector2i(0, yPos);
					xUiV_Window.UiTransform.localPosition = new Vector3(0f, yPos);
					yPos -= xUiV_Window.Size.y;
					maxWidth = Math.Max(maxWidth, xUiV_Window.Size.x);
					windowCount++;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initSounds()
	{
		LoadData("@:Sounds/UI/ui_menu_cycle.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			uiScrollSound = _o;
		});
		LoadData("@:Sounds/UI/ui_menu_click.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			uiClickSound = _o;
		});
		LoadData("@:Sounds/UI/ui_menu_start.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			uiConfirmSound = _o;
		});
		LoadData("@:Sounds/UI/ui_menu_back.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			uiBackSound = _o;
		});
		LoadData("@:Sounds/UI/ui_hover.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			uiSliderSound = _o;
		});
	}

	public void PlayMenuSound(UISoundType _soundType)
	{
		switch (_soundType)
		{
		case UISoundType.ClickSound:
			PlayMenuClickSound();
			break;
		case UISoundType.ScrollSound:
			PlayMenuScrollSound();
			break;
		case UISoundType.ConfirmSound:
			PlayMenuConfirmSound();
			break;
		case UISoundType.BackSound:
			PlayMenuBackSound();
			break;
		case UISoundType.SliderSound:
			PlayMenuSliderSound();
			break;
		case UISoundType.None:
			break;
		}
	}

	public void PlayMenuScrollSound()
	{
		Manager.PlayXUiSound(uiScrollSound, uiScrollVolume);
	}

	public void PlayMenuClickSound()
	{
		Manager.PlayXUiSound(uiClickSound, uiClickVolume);
	}

	public void PlayMenuConfirmSound()
	{
		Manager.PlayXUiSound(uiConfirmSound, uiConfirmVolume);
	}

	public void PlayMenuBackSound()
	{
		Manager.PlayXUiSound(uiBackSound, uiBackVolume);
	}

	public void PlayMenuSliderSound()
	{
		Manager.PlayXUiSound(uiSliderSound, uiSliderVolume);
	}

	public XUiV_Window GetWindow(string _name)
	{
		for (int i = 0; i < windows.Count; i++)
		{
			if (windows[i].ID == _name)
			{
				return windows[i];
			}
		}
		return null;
	}

	public XUiController FindWindowGroupByName(string _name)
	{
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			if (WindowGroups[i].Id.EqualsCaseInsensitive(_name))
			{
				return WindowGroups[i].Controller;
			}
		}
		return null;
	}

	public XUiController GetChildById(string _id)
	{
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			XUiController childById = WindowGroups[i].Controller.GetChildById(_id);
			if (childById != null)
			{
				return childById;
			}
		}
		return null;
	}

	public List<XUiController> GetChildrenById(string _id)
	{
		List<XUiController> list = new List<XUiController>();
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			WindowGroups[i].Controller.GetChildrenById(_id, list);
		}
		return list;
	}

	public T GetChildByType<T>() where T : XUiController
	{
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			T childByType = WindowGroups[i].Controller.GetChildByType<T>();
			if (childByType != null)
			{
				return childByType;
			}
		}
		return null;
	}

	public List<T> GetChildrenByType<T>() where T : XUiController
	{
		List<T> list = new List<T>();
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			WindowGroups[i].Controller.GetChildrenByType(list);
		}
		return list;
	}

	public T GetWindowByType<T>() where T : XUiController
	{
		Type typeFromHandle = typeof(T);
		windowsAndGroupsByControllerType.TryGetValue(typeFromHandle, out var value);
		if (value == null || value.Count == 0)
		{
			return null;
		}
		if (value.Count > 1)
		{
			Log.Warning("Multiple controllers of type " + typeof(T).FullName);
		}
		return (T)value[0];
	}

	public List<T> GetWindowsByType<T>() where T : XUiController
	{
		Type typeFromHandle = typeof(T);
		windowsAndGroupsByControllerType.TryGetValue(typeFromHandle, out var value);
		List<T> list = new List<T>();
		if (value == null)
		{
			return list;
		}
		foreach (XUiController item in value)
		{
			list.Add((T)item);
		}
		return list;
	}

	public XUiWindowGroup GetWindowGroupById(string _id)
	{
		foreach (XUiWindowGroup windowGroup in WindowGroups)
		{
			if (windowGroup.Id == _id)
			{
				return windowGroup;
			}
		}
		return null;
	}

	public void GetOpenWindows(List<XUiV_Window> _list)
	{
		_list.Clear();
		foreach (XUiV_Window window in windows)
		{
			if (window.IsOpen && window.IsVisible)
			{
				_list.Add(window);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initAtlases()
	{
		MultiSourceAtlasManager[] array = Resources.FindObjectsOfTypeAll<MultiSourceAtlasManager>();
		for (int i = 0; i < array.Length; i++)
		{
			allMultiSourceAtlases.Add(array[i].name, array[i]);
		}
	}

	public INGUIAtlas GetAtlasByName(string _atlasName, string _spriteName)
	{
		if (string.IsNullOrEmpty(_atlasName))
		{
			return null;
		}
		if (!string.IsNullOrEmpty(_spriteName) && allMultiSourceAtlases.TryGetValue(_atlasName, out var value))
		{
			return value.GetAtlasForSprite(_spriteName);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initFonts()
	{
		fontsByName = new CaseInsensitiveStringDictionary<NGUIFont>();
		NGUIFont[] nGUIFonts = NGUIFonts;
		foreach (NGUIFont nGUIFont in nGUIFonts)
		{
			fontsByName.Add(nGUIFont.name, nGUIFont);
		}
	}

	public NGUIFont GetUIFontByName(string _name, bool _showWarning = true)
	{
		if (fontsByName.TryGetValue(_name, out var value))
		{
			return value;
		}
		if (_showWarning)
		{
			Log.Warning("XUi font not found: " + _name + ", from: " + StackTraceUtility.ExtractStackTrace());
		}
		return null;
	}

	public Transform GetAnchor(string _anchorName)
	{
		if (string.IsNullOrEmpty(_anchorName))
		{
			return null;
		}
		if (EnumUtils.TryParse<UIAnchor.Side>(_anchorName, out var _result, _ignoreCase: true))
		{
			return xuiAnchors[(int)_result].transform;
		}
		return anchorRoot.Find(_anchorName);
	}

	public UIAnchor GetAnchor(UIAnchor.Side _anchorSide)
	{
		return xuiAnchors[(int)_anchorSide];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initAnchors()
	{
		anchorRoot = base.transform.Find("Anchors").transform;
		UIAnchor[] componentsInChildren = anchorRoot.parent.GetComponentsInChildren<UIAnchor>();
		xuiAnchors = new UIAnchor[9];
		UIAnchor[] array = componentsInChildren;
		foreach (UIAnchor uIAnchor in array)
		{
			uIAnchor.runOnlyOnce = false;
			uIAnchor.uiCamera = playerUI.camera;
			xuiAnchors[(int)uIAnchor.side] = uIAnchor;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAnchors()
	{
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
		float num2 = (1f - num) / 2f;
		UIAnchor[] array = xuiAnchors;
		foreach (UIAnchor uIAnchor in array)
		{
			if (!(uIAnchor == null))
			{
				UIAnchor.Side side = uIAnchor.side;
				if (side == UIAnchor.Side.Right || side == UIAnchor.Side.TopRight || side == UIAnchor.Side.BottomRight)
				{
					uIAnchor.relativeOffset.x = 0f - num2;
				}
				side = uIAnchor.side;
				if (side == UIAnchor.Side.Left || side == UIAnchor.Side.TopLeft || side == UIAnchor.Side.BottomLeft)
				{
					uIAnchor.relativeOffset.x = num2;
				}
				side = uIAnchor.side;
				if (side == UIAnchor.Side.Top || side == UIAnchor.Side.TopRight || side == UIAnchor.Side.TopLeft)
				{
					uIAnchor.relativeOffset.y = 0f - num2;
				}
				side = uIAnchor.side;
				if (side == UIAnchor.Side.Bottom || side == UIAnchor.Side.BottomRight || side == UIAnchor.Side.BottomLeft)
				{
					uIAnchor.relativeOffset.y = num2;
				}
				side = uIAnchor.side;
				if (side == UIAnchor.Side.Bottom || side == UIAnchor.Side.Top || side == UIAnchor.Side.Center)
				{
					uIAnchor.relativeOffset.x = 0f;
				}
			}
		}
	}

	public void RefreshAllWindows()
	{
		if (WindowGroups != null)
		{
			for (int i = 0; i < WindowGroups.Count; i++)
			{
				WindowGroups[i].Controller.SetAllChildrenDirty();
			}
		}
	}

	public void CancelAllCrafting()
	{
		XUiC_RecipeStack[] recipesToCraft = FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingQueue>().GetRecipesToCraft();
		for (int i = 0; i < recipesToCraft.Length; i++)
		{
			recipesToCraft[i].ForceCancel();
		}
	}

	public CraftingData GetCraftingData()
	{
		CraftingData craftingData = new CraftingData();
		XUiController xUiController = FindWindowGroupByName("crafting");
		if (xUiController == null)
		{
			return craftingData;
		}
		XUiC_CraftingQueue childByType = xUiController.GetChildByType<XUiC_CraftingQueue>();
		if (childByType == null)
		{
			return craftingData;
		}
		XUiC_RecipeStack[] recipesToCraft = childByType.GetRecipesToCraft();
		if (recipesToCraft == null)
		{
			return craftingData;
		}
		craftingData.RecipeQueueItems = new RecipeQueueItem[recipesToCraft.Length];
		for (int i = 0; i < recipesToCraft.Length; i++)
		{
			RecipeQueueItem recipeQueueItem = new RecipeQueueItem();
			recipeQueueItem.Recipe = recipesToCraft[i].GetRecipe();
			recipeQueueItem.Multiplier = (short)recipesToCraft[i].GetRecipeCount();
			recipeQueueItem.CraftingTimeLeft = recipesToCraft[i].GetRecipeCraftingTimeLeft();
			recipeQueueItem.IsCrafting = recipesToCraft[i].IsCrafting;
			recipeQueueItem.Quality = (byte)recipesToCraft[i].OutputQuality;
			recipeQueueItem.StartingEntityId = recipesToCraft[i].StartingEntityId;
			recipeQueueItem.RepairItem = recipesToCraft[i].OriginalItem;
			recipeQueueItem.AmountToRepair = (ushort)recipesToCraft[i].AmountToRepair;
			recipeQueueItem.OneItemCraftTime = recipesToCraft[i].GetOneItemCraftTime();
			craftingData.RecipeQueueItems[i] = recipeQueueItem;
		}
		return craftingData;
	}

	public void SetCraftingData(CraftingData _cd)
	{
		StartCoroutine(setCraftingDataAsync(_cd));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setCraftingDataAsync(CraftingData _cd)
	{
		while (!IsReady)
		{
			yield return null;
		}
		XUiC_CraftingQueue childByType = GetChildByType<XUiC_CraftingQueue>();
		if (childByType == null)
		{
			yield break;
		}
		childByType.ClearQueue();
		for (int i = 0; i < _cd.RecipeQueueItems.Length; i++)
		{
			RecipeQueueItem recipeQueueItem = _cd.RecipeQueueItems[i];
			if (recipeQueueItem != null)
			{
				if (recipeQueueItem.RepairItem != null && recipeQueueItem.RepairItem.type != 0)
				{
					childByType.AddItemToRepairAtIndex(i, recipeQueueItem.CraftingTimeLeft, recipeQueueItem.RepairItem, recipeQueueItem.AmountToRepair, recipeQueueItem.IsCrafting, recipeQueueItem.StartingEntityId);
				}
				else
				{
					childByType.AddRecipeToCraftAtIndex(i, recipeQueueItem.Recipe, recipeQueueItem.Multiplier, recipeQueueItem.CraftingTimeLeft, recipeQueueItem.IsCrafting, recipeModification: false, recipeQueueItem.Quality, recipeQueueItem.StartingEntityId, recipeQueueItem.OneItemCraftTime);
				}
			}
			else
			{
				childByType.AddRecipeToCraftAtIndex(i, null, 0, -1f, isCrafting: false);
			}
		}
		childByType.IsDirty = true;
	}

	public static void HandlePaging(XUi _xui, Func<bool> _onPageUp, Func<bool> _onPageDown, bool _useVerticalAxis = false)
	{
		if (null != _xui.playerUI && _xui.playerUI.playerInput?.GUIActions != null && _xui.playerUI.windowManager.IsKeyShortcutsAllowed())
		{
			Vector2 vector = _xui.playerUI.playerInput.GUIActions.Camera.Vector;
			if (vector == Vector2.zero)
			{
				pagingRepeatTimer = 0f;
				previousPagingVector = Vector2.zero;
				return;
			}
			if (previousPagingVector != Vector2.zero)
			{
				pagingRepeatTimer -= Time.unscaledDeltaTime;
				if (pagingRepeatTimer > 0f)
				{
					return;
				}
			}
			else
			{
				initialPagingInput = true;
			}
			previousPagingVector = vector;
			pagingRepeatTimer = (initialPagingInput ? 0.35f : 0.1f);
			initialPagingInput = false;
			bool flag = false;
			float num = Mathf.Abs(vector.x);
			float num2 = Mathf.Abs(vector.y);
			if (_useVerticalAxis)
			{
				if ((double)num2 < (double)num * 1.8)
				{
					return;
				}
				if ((vector.y > 0f && _onPageUp()) || (vector.y < 0f && _onPageDown()))
				{
					flag = true;
				}
			}
			else
			{
				if ((double)num < (double)num2 * 1.8)
				{
					return;
				}
				if ((vector.x > 0f && _onPageUp()) || (vector.x < 0f && _onPageDown()))
				{
					flag = true;
				}
			}
			if (flag)
			{
				_xui.playerUI.CursorController.PlayPagingSound();
			}
		}
		else
		{
			previousPagingVector = Vector2.zero;
		}
	}

	public void UpdateWindowSoftCursorBounds(XUiV_Window _window)
	{
		_ = playerUI.CursorController != null;
	}

	public void RemoveWindowFromSoftCursorBounds(XUiV_Window _window)
	{
		CursorControllerAbs cursorController = playerUI.CursorController;
		if (cursorController != null)
		{
			cursorController.RemoveBounds(_window.ID);
		}
	}

	public void ForceInputStyleChange()
	{
		GetOpenWindows(forceInputStyleChangedWindowsList);
		foreach (XUiV_Window forceInputStyleChangedWindows in forceInputStyleChangedWindowsList)
		{
			forceInputStyleChangedWindows.Controller.ForceInputStyleChange(PlatformManager.NativePlatform.Input.CurrentInputStyle, PlatformManager.NativePlatform.Input.CurrentInputStyle);
		}
		forceInputStyleChangedWindowsList.Clear();
	}
}

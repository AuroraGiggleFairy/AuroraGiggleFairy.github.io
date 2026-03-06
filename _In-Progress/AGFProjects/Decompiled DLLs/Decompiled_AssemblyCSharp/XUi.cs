using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;

public class XUi : MonoBehaviour
{
	public enum UISoundType
	{
		ClickSound,
		ScrollSound,
		ConfirmSound,
		BackSound,
		SliderSound,
		None
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class StackPanel
	{
		public readonly string Name;

		public readonly Transform Transform;

		public int WindowCount;

		public Vector2Int Size;

		public StackPanel(string _name, Transform _transform)
		{
			Name = _name;
			Transform = _transform;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum Anchor
	{
		LeftTop,
		LeftCenter,
		LeftBottom,
		CenterTop,
		CenterCenter,
		CenterBottom,
		RightTop,
		RightCenter,
		RightBottom,
		Count
	}

	public enum Alignment
	{
		TopLeft,
		CenterLeft,
		BottomLeft,
		TopCenter,
		CenterCenter,
		BottomCenter,
		TopRight,
		CenterRight,
		BottomRight,
		Count
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float xuiGlobalScaling = 1.255f;

	public static string RootNode = "NGUI Camera";

	public static int ID = -1;

	public static string BlankTexture = "menu_empty";

	public static Transform XUiRootTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int lastScreenHeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float pixelRatioFactor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static UIRoot UIRoot;

	[NonSerialized]
	public float uiScrollVolume = 0.5f;

	[NonSerialized]
	public float uiClickVolume = 0.5f;

	[NonSerialized]
	public float uiConfirmVolume = 0.5f;

	[NonSerialized]
	public float uiBackVolume = 0.5f;

	[NonSerialized]
	public float uiSliderVolume = 0.25f;

	public int id;

	public NGUIFont[] NGUIFonts;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CaseInsensitiveStringDictionary<NGUIFont> FontsByName;

	public List<XUiWindowGroup> WindowGroups;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Type, List<XUiController>> ControllersByType;

	public Vector2 lastScreenSize = Vector2.zero;

	public float BackgroundGlobalOpacity = 1f;

	public float ForegroundGlobalOpacity = 1f;

	public string Ruleset = "default";

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

	public bool IgnoreMissingClass;

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
	public List<XUiV_Window> windows;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UIAnchor[] anchors;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UIAnchor[] xuiAnchors;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float oldBackgroundGlobalOpacity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float oldForegroundGlobalOpacity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int repositionFrames;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiWindowGroup currentlyOpeningWindowGroup;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultStackPanelScale = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform stackPanelRoot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DictionaryList<string, StackPanel> stackPanels = new DictionaryList<string, StackPanel>();

	public static MicroStopwatch Stopwatch;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, UIAtlas> allAtlases = new CaseInsensitiveStringDictionary<UIAtlas>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, MultiSourceAtlasManager> allMultiSourceAtlases = new CaseInsensitiveStringDictionary<MultiSourceAtlasManager>();

	public static GameObject defaultPrefab = null;

	public static GameObject fullPrefab = null;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LocalPlayerUI mPlayerUI;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_GamepadCalloutWindow mCalloutWindow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool _inGameMenuOpen = false;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public long accumElapsedMilliseconds;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine loadAsyncCoroutine;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiView> xuiViewList = new List<XUiView>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<UIBasicSprite> getXUIWindowWorldBoundsList = new List<UIBasicSprite>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 previousPagingVector = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float pagingRepeatTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialPagingInput = false;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public AudioClip uiScrollSound
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public AudioClip uiClickSound
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public AudioClip uiConfirmSound
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public AudioClip uiBackSound
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public AudioClip uiSliderSound
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool isReady
	{
		get
		{
			return mIsReady;
		}
		set
		{
			mIsReady = value;
			if (mIsReady && this.OnBuilt != null)
			{
				this.OnBuilt();
			}
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool isMinimal { get; set; }

	public bool GlobalOpacityChanged
	{
		get
		{
			if (oldBackgroundGlobalOpacity == BackgroundGlobalOpacity)
			{
				return oldForegroundGlobalOpacity != ForegroundGlobalOpacity;
			}
			return true;
		}
	}

	public Transform StackPanelTransform => stackPanelRoot;

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

	public XUiC_GamepadCalloutWindow calloutWindow
	{
		get
		{
			XUiController xUiController = FindWindowGroupByName("CalloutGroup");
			if (xUiController != null)
			{
				mCalloutWindow = xUiController.GetChildByType<XUiC_GamepadCalloutWindow>();
			}
			return mCalloutWindow;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DragAndDropWindow dragAndDrop { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OnScreenIcons onScreenIcons { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToolTip currentToolTip { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PopupMenu currentPopupMenu { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SaveIndicator saveIndicator { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BasePartStack basePartStack { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_EquipmentStack equipmentStack { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack itemStack { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeEntry recipeEntry { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public ProgressionValue selectedSkill { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityLootable lootContainer { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string currentWorkstation { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle vehicle { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationToolGrid currentWorkstationToolGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationFuelGrid currentWorkstationFuelGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationInputGrid currentWorkstationInputGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationOutputGrid currentWorkstationOutputGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorModGrid currentDewCollectorModGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CombineGrid currentCombineGrid { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerSourceSlots powerSourceSlots { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerRangedAmmoSlots powerAmmoSlots { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SelectableEntry currentSelectedEntry { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool isUsingItemActionEntryUse { get; set; }

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool isUsingItemActionEntryPromptComplete { get; set; }

	public static bool InGameMenuOpen
	{
		get
		{
			return _inGameMenuOpen;
		}
		set
		{
			_inGameMenuOpen = value;
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

	public event Action OnShutdown;

	public event Action OnBuilt;

	public static XUi Instantiate(LocalPlayerUI playerUI, GameObject xuiPrefab = null)
	{
		if (GameManager.IsDedicatedServer)
		{
			return null;
		}
		Log.Out("[XUi] Instantiating XUi from {0} prefab.", (xuiPrefab != null) ? xuiPrefab.name : ((defaultPrefab != null) ? defaultPrefab.name : "default"));
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		Transform obj = UnityEngine.Object.Instantiate(xuiPrefab ? xuiPrefab : ((defaultPrefab != null) ? defaultPrefab : Resources.Load<GameObject>("Prefabs/XUi"))).transform;
		obj.name = obj.name.Replace("(Clone)", "").Replace("_Full", "");
		obj.parent = playerUI.transform.Find("NGUI Camera");
		UIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
		XUi component = obj.GetComponent<XUi>();
		component.SetScale();
		component.gameObject.SetActive(value: true);
		component.Init(ID++);
		Log.Out("[XUi] XUi instantiation completed in {0} ms", microStopwatch.ElapsedMilliseconds);
		return component;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _obj)
	{
		switch (_obj)
		{
		case EnumGamePrefs.OptionsScreenBoundsValue:
			SetScale();
			UpdateAnchors();
			RecenterWindowGroup(null, _forceImmediate: true);
			break;
		case EnumGamePrefs.OptionsHudSize:
			SetScale();
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

	public void LateInitialize()
	{
		LateInit();
	}

	public void Shutdown(bool _destroyImmediate = false)
	{
		if (this.OnShutdown != null)
		{
			this.OnShutdown();
		}
		Cleanup(_destroyImmediate);
	}

	public static void Reload(LocalPlayerUI _playerUI)
	{
		if (_playerUI.xui != null)
		{
			_playerUI.xui.Shutdown(_destroyImmediate: true);
		}
		SetXmlsForUi(_playerUI);
		XUi xUi = Instantiate(_playerUI);
		xUi.Load();
		xUi.SetDataConnections();
	}

	public static void ReloadWindow(LocalPlayerUI _playerUI, string _windowGroupName)
	{
		if (_playerUI.xui == null)
		{
			Log.Error("Can not reload single window, XUi not instantiated");
		}
		for (int i = 0; i < _playerUI.xui.WindowGroups.Count; i++)
		{
			XUiWindowGroup xUiWindowGroup = _playerUI.xui.WindowGroups[i];
			if (xUiWindowGroup.ID == _windowGroupName)
			{
				xUiWindowGroup.Controller.Cleanup();
				_playerUI.windowManager.Remove(xUiWindowGroup.ID);
				for (int j = 0; j < xUiWindowGroup.Controller.Children.Count; j++)
				{
					UnityEngine.Object.DestroyImmediate(xUiWindowGroup.Controller.Children[j].ViewComponent.UiTransform.gameObject);
				}
				_playerUI.xui.WindowGroups.RemoveAt(i);
				break;
			}
		}
		SetXmlsForUi(_playerUI);
		_playerUI.xui.Load(new List<string> { _windowGroupName });
		_playerUI.xui.isReady = true;
	}

	public static void SetXmlsForUi(LocalPlayerUI _playerUI)
	{
		XUiFromXml.ClearData();
		if (!_playerUI.isPrimaryUI)
		{
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Common/styles"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Common/controls"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi/styles"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi/controls"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi/windows"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi/xui"));
		}
		else
		{
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Common/styles"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Common/controls"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Menu/styles"));
			ThreadManager.RunCoroutineSync(PatchAndLoadXuiXml("XUi_Menu/controls"));
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

	public void SetDataConnections()
	{
		if (playerUI.entityPlayer != null)
		{
			PlayerInventory = new XUiM_PlayerInventory(this, playerUI.entityPlayer);
			PlayerEquipment = new XUiM_PlayerEquipment(this, playerUI.entityPlayer);
		}
	}

	public float GetPixelRatioFactor()
	{
		if (pixelRatioFactor == 0f || Screen.height != lastScreenHeight)
		{
			float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
			float activeUiScale = GameOptionsManager.GetActiveUiScale();
			pixelRatioFactor = UIRoot.pixelSizeAdjustment / xuiGlobalScaling / num / activeUiScale;
			lastScreenHeight = Screen.height;
		}
		return pixelRatioFactor;
	}

	public Vector2i GetXUiScreenSize()
	{
		return new Vector2i(new Vector2(playerUI.camera.pixelWidth, playerUI.camera.pixelHeight) * GetPixelRatioFactor());
	}

	public Vector2i GetMouseXUIPosition()
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

	public static bool IsGameRunning()
	{
		if (GameManager.Instance != null)
		{
			return GameManager.Instance.World != null;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.ResolutionChanged -= OnResolutionChanged;
		GamePrefs.OnGamePrefChanged -= OnGamePrefChanged;
		LocalPlayerManager.OnLocalPlayersChanged -= HandleLocalPlayersChanged;
		Shutdown();
	}

	public void SetScale(float scale = -1f)
	{
		if (scale > 0f)
		{
			xuiGlobalScaling = scale;
		}
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
		float activeUiScale = GameOptionsManager.GetActiveUiScale();
		base.transform.localScale = Vector3.one * xuiGlobalScaling * num * activeUiScale;
		base.transform.localPosition = Vector3.zero;
		pixelRatioFactor = 0f;
	}

	public float GetScale()
	{
		return xuiGlobalScaling;
	}

	public void SetStackPanelScale(float scale)
	{
		defaultStackPanelScale = scale;
		stackPanelRoot.localScale = Vector3.one * scale;
		stackPanelRoot.localPosition = Vector3.zero;
	}

	public void Awake()
	{
		WindowGroups = new List<XUiWindowGroup>();
		ControllersByType = new Dictionary<Type, List<XUiController>>();
		FontsByName = new CaseInsensitiveStringDictionary<NGUIFont>();
		NGUIFont[] nGUIFonts = NGUIFonts;
		foreach (NGUIFont nGUIFont in nGUIFonts)
		{
			FontsByName.Add(nGUIFont.name, nGUIFont);
		}
	}

	public void Load(List<string> windowGroupSubset = null, bool async = false)
	{
		if (async)
		{
			asyncLoad = true;
			loadAsyncCoroutine = ThreadManager.StartCoroutine(LoadAsync(windowGroupSubset));
			return;
		}
		asyncLoad = false;
		if (!XUiFromXml.HasData())
		{
			Log.Error("Loading XUi synchronously failed: XMLs not set.");
		}
		else
		{
			ThreadManager.RunCoroutineSync(LoadAsync(windowGroupSubset));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LoadAsync(List<string> windowGroupSubset = null)
	{
		yield return null;
		while (!XUiFromXml.HasData())
		{
			yield return null;
		}
		MicroStopwatch msw = new MicroStopwatch();
		Log.Out("[XUi] Loading XUi " + (asyncLoad ? "asynchronously" : "synchronously"));
		List<string> asyncWindowGroupList = ((windowGroupSubset != null) ? new List<string>(windowGroupSubset) : new List<string>());
		if (windowGroupSubset == null)
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
		XUiFromXml.LoadDone(windowGroupSubset == null);
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
				Log.Error("[XUi] Failed initializing window group " + xUiWindowGroup.ID);
				Log.Exception(e);
			}
			accumElapsedMilliseconds += ms.ElapsedMilliseconds;
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Out("[XUi] Initialize window group, {0}, completed in {1} ms.", xUiWindowGroup.ID, ms.ElapsedMilliseconds);
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
		PostLoadInit();
		isReady = windowGroupSubset == null;
		loadAsyncCoroutine = null;
		XUiUpdater.Add(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PostLoadInit()
	{
		RadialWindow = (XUiC_Radial)FindWindowGroupByName("radial");
		ControllersByType.Clear();
		foreach (XUiWindowGroup windowGroup in WindowGroups)
		{
			if (!windowGroup.Initialized)
			{
				continue;
			}
			AddControllerTypeEntry(windowGroup.Controller);
			foreach (XUiController child in windowGroup.Controller.Children)
			{
				AddControllerTypeEntry(child);
			}
		}
		if (WorldStaticData.LoadAllXmlsCoComplete)
		{
			XUiFromXml.ClearLoadingData();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddControllerTypeEntry(XUiController _controller)
	{
		Type type = _controller.GetType();
		if (!ControllersByType.TryGetValue(type, out var value))
		{
			value = new List<XUiController>();
			ControllersByType.Add(type, value);
		}
		value.Add(_controller);
	}

	public int RegisterXUiView(XUiView _view)
	{
		xuiViewList.Add(_view);
		return xuiViewList.Count - 1;
	}

	public void LoadData<T>(string _path, Action<T> _callback) where T : UnityEngine.Object
	{
		LoadManager.LoadAsset(_path, _callback, loadGroup, _deferLoading: false, !asyncLoad);
	}

	public void Init(int _id)
	{
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
		GameOptionsManager.ResolutionChanged += OnResolutionChanged;
		loadGroup = LoadManager.CreateGroup();
		id = _id;
		windows = new List<XUiV_Window>();
		lastScreenSize = new Vector2(playerUI.camera.pixelWidth, playerUI.camera.pixelHeight);
		base.gameObject.GetOrAddComponent<XUi_FallThrough>().SetXUi(this);
		stackPanelRoot = base.transform.Find("StackPanels").transform;
		stackPanels.Add("Left", new StackPanel("Left", base.transform.Find("StackPanels/Left").transform));
		stackPanels.Add("Center", new StackPanel("Center", base.transform.Find("StackPanels/Center").transform));
		stackPanels.Add("Right", new StackPanel("Right", base.transform.Find("StackPanels/Right").transform));
		MultiSourceAtlasManager[] array = Resources.FindObjectsOfTypeAll<MultiSourceAtlasManager>();
		for (int i = 0; i < array.Length; i++)
		{
			allMultiSourceAtlases.Add(array[i].name, array[i]);
		}
		if (Application.isPlaying)
		{
			LoadData("@:Sounds/UI/ui_menu_cycle.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip o) =>
			{
				uiScrollSound = o;
			});
			LoadData("@:Sounds/UI/ui_menu_click.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip o) =>
			{
				uiClickSound = o;
			});
			LoadData("@:Sounds/UI/ui_menu_start.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip o) =>
			{
				uiConfirmSound = o;
			});
			LoadData("@:Sounds/UI/ui_menu_back.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip o) =>
			{
				uiBackSound = o;
			});
			LoadData("@:Sounds/UI/ui_hover.wav", [PublicizedFrom(EAccessModifier.Private)] (AudioClip o) =>
			{
				uiSliderSound = o;
			});
		}
		anchors = base.transform.parent.GetComponentsInChildren<UIAnchor>();
		xuiAnchors = new UIAnchor[9];
		UIAnchor[] array2 = anchors;
		foreach (UIAnchor uIAnchor in array2)
		{
			uIAnchor.runOnlyOnce = false;
			uIAnchor.uiCamera = playerUI.camera;
			if (uIAnchor.transform.parent.GetComponent<XUi>() == this)
			{
				xuiAnchors[(int)uIAnchor.side] = uIAnchor;
			}
		}
		UpdateAnchors();
		BackgroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
		ForegroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		LocalPlayerManager.OnLocalPlayersChanged += HandleLocalPlayersChanged;
		Vehicle = new XUiM_Vehicle();
		AssembleItem = new XUiM_AssembleItem();
		QuestTracker = new XUiM_Quest();
		Recipes = new XUiM_Recipes();
		Trader = new XUiM_Trader();
		Dialog = new XUiM_Dialog();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLocalPlayersChanged()
	{
		lastScreenSize = new Vector2(playerUI.camera.pixelWidth, playerUI.camera.pixelHeight);
		UpdateAnchors();
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

	public void LateInit()
	{
		RadialWindow = (XUiC_Radial)FindWindowGroupByName("radial");
		XUiM_PlayerBuffs.HasLocalizationBeenCached = false;
		XUiM_Vehicle.HasLocalizationBeenCached = false;
	}

	public void Cleanup(bool _destroyImmediate = false)
	{
		CancelLoading();
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			XUiWindowGroup xUiWindowGroup = WindowGroups[i];
			xUiWindowGroup.Controller.Cleanup();
			playerUI.windowManager.Remove(xUiWindowGroup.ID);
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

	public void OnUpdateInput()
	{
		if (WindowGroups == null)
		{
			return;
		}
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			if (!WindowGroups[i].Controller.IsDormant && (WindowGroups[i].isShowing || WindowGroups[i].Controller.AlwaysUpdate()))
			{
				try
				{
					WindowGroups[i].Controller.UpdateInput();
				}
				catch (Exception e)
				{
					Log.Error("[XUi] Error while handling input for window group '" + WindowGroups[i].ID + "':");
					Log.Exception(e);
				}
			}
		}
	}

	public void OnUpdateDeltaTime(float updateDeltaTime)
	{
		if (playerUI.entityPlayer != null)
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
		if (WindowGroups == null)
		{
			return;
		}
		if (currentToolTip != null)
		{
			playerUI.windowManager.OpenIfNotOpen(currentToolTip.ID, _bModal: false);
		}
		if (saveIndicator != null)
		{
			playerUI.windowManager.OpenIfNotOpen(saveIndicator.ID, _bModal: false);
		}
		if (lastScreenSize.x != (float)playerUI.camera.pixelWidth || lastScreenSize.y != (float)playerUI.camera.pixelHeight)
		{
			lastScreenSize = new Vector2(playerUI.camera.pixelWidth, playerUI.camera.pixelHeight);
			UpdateAnchors();
			RecenterWindowGroup(null, _forceImmediate: true);
		}
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			XUiWindowGroup xUiWindowGroup = WindowGroups[i];
			if (xUiWindowGroup.Initialized && !xUiWindowGroup.Controller.IsDormant && (xUiWindowGroup.isShowing || xUiWindowGroup.Controller.AlwaysUpdate()))
			{
				try
				{
					xUiWindowGroup.Controller.Update(updateDeltaTime);
				}
				catch (Exception e)
				{
					Log.Error("[XUi] Error while updating window group '" + xUiWindowGroup.ID + "':");
					Log.Exception(e);
				}
			}
		}
		oldBackgroundGlobalOpacity = BackgroundGlobalOpacity;
		oldForegroundGlobalOpacity = ForegroundGlobalOpacity;
	}

	public void RecenterWindowGroup(XUiWindowGroup _wg, bool _forceImmediate = false)
	{
		if (_forceImmediate || GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			if (_wg == null)
			{
				CalculateWindowGroupLayouts();
			}
			else
			{
				CalculateWindowGroupLayout(_wg);
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
			CalculateWindowGroupLayout(_wg);
		}
		else
		{
			CalculateWindowGroupLayouts();
		}
		yield return null;
		playerUI.CursorController.ResetNavigationTarget();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculateWindowGroupLayouts()
	{
		foreach (XUiWindowGroup windowGroup in WindowGroups)
		{
			if (windowGroup.isShowing)
			{
				CalculateWindowGroupLayout(windowGroup);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculateWindowGroupLayout(XUiWindowGroup _wg)
	{
		if (_wg == null || _wg.ID == "backpack" || _wg.Controller?.Children == null || !_wg.HasStackPanelWindows())
		{
			return;
		}
		bool flag = false;
		int num = 0;
		int num2 = 0;
		foreach (StackPanel item in stackPanels.list)
		{
			bool flag2 = LayoutWindowsInPanel(_wg, item);
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
				stackPanels.list[0].Transform.localPosition = new Vector3((int)stackPanels.list[0].Transform.position.x, -(num2 - stackPanels.list[0].Size.y), 0f);
			}
			if (!_wg.RightPanelVAlignTop)
			{
				stackPanels.list[2].Transform.localPosition = new Vector3((int)stackPanels.list[2].Transform.position.x, -(num2 - stackPanels.list[2].Size.y), 0f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LayoutWindowsInPanel(XUiWindowGroup _wg, StackPanel _panel)
	{
		int windowCount = 0;
		int yPos = 0;
		int maxWidth = 0;
		LayoutWindowGroupWindows(_wg);
		if (playerUI.windowManager.IsWindowOpen("backpack"))
		{
			LayoutWindowGroupWindows(GetWindowGroupById("backpack"));
		}
		_panel.Size.x = maxWidth;
		_panel.Size.y = -yPos;
		_panel.WindowCount = windowCount;
		return windowCount != 0;
		[PublicizedFrom(EAccessModifier.Internal)]
		void LayoutWindowGroupWindows(XUiWindowGroup xUiWindowGroup)
		{
			foreach (XUiController child in xUiWindowGroup.Controller.Children)
			{
				XUiV_Window xUiV_Window = (XUiV_Window)child.ViewComponent;
				if (xUiV_Window != null && xUiV_Window.UiTransform.gameObject.activeInHierarchy && !(child.ViewComponent.UiTransform.parent.name != _panel.Name) && xUiV_Window.Size.y > 0)
				{
					if (yPos < 0)
					{
						yPos -= xUiWindowGroup.StackPanelPadding;
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

	public Bounds GetXUIWindowWorldBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds result = new Bounds(Vector3.zero, Vector3.zero);
		bool flag = false;
		_xuiElement.GetComponentsInChildren(_includeInactive, getXUIWindowWorldBoundsList);
		List<UIBasicSprite> list = getXUIWindowWorldBoundsList;
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
		getXUIWindowWorldBoundsList.Clear();
		return result;
	}

	public Bounds GetXUIWindowScreenBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds xUIWindowWorldBounds = GetXUIWindowWorldBounds(_xuiElement, _includeInactive);
		Bounds result = new Bounds(playerUI.camera.WorldToScreenPoint(xUIWindowWorldBounds.min), Vector3.zero);
		result.Encapsulate(playerUI.camera.WorldToScreenPoint(xUIWindowWorldBounds.max));
		return result;
	}

	public Bounds GetXUIWindowViewportBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds xUIWindowWorldBounds = GetXUIWindowWorldBounds(_xuiElement, _includeInactive);
		Bounds result = new Bounds(playerUI.camera.WorldToViewportPoint(xUIWindowWorldBounds.min), Vector3.zero);
		result.Encapsulate(playerUI.camera.WorldToViewportPoint(xUIWindowWorldBounds.max));
		return result;
	}

	public Bounds GetXUIWindowPixelBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds xUIWindowViewportBounds = GetXUIWindowViewportBounds(_xuiElement, _includeInactive);
		Vector3 center = Vector3.Scale(xUIWindowViewportBounds.min, new Vector3(playerUI.camera.pixelWidth, playerUI.camera.pixelHeight, 1f));
		Vector3 point = Vector3.Scale(xUIWindowViewportBounds.max, new Vector3(playerUI.camera.pixelWidth, playerUI.camera.pixelHeight, 1f));
		Bounds result = new Bounds(center, Vector3.zero);
		result.Encapsulate(point);
		return result;
	}

	public void RefreshAllWindows(bool _includeViewComponents = false)
	{
		if (WindowGroups != null)
		{
			for (int i = 0; i < WindowGroups.Count; i++)
			{
				WindowGroups[i].Controller.SetAllChildrenDirty(_includeViewComponents);
			}
		}
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

	public UIAtlas GetAtlasByName(string _atlasName, string _spriteName)
	{
		if (string.IsNullOrEmpty(_atlasName))
		{
			return null;
		}
		if (!string.IsNullOrEmpty(_spriteName) && allMultiSourceAtlases.TryGetValue(_atlasName, out var value))
		{
			return value.GetAtlasForSprite(_spriteName);
		}
		if (allAtlases.TryGetValue(_atlasName, out var value2))
		{
			return value2;
		}
		return null;
	}

	public NGUIFont GetUIFontByName(string _name, bool _showWarning = true)
	{
		if (FontsByName.TryGetValue(_name, out var value))
		{
			return value;
		}
		if (_showWarning)
		{
			Log.Warning("XUi font not found: " + _name + ", from: " + StackTraceUtility.ExtractStackTrace());
		}
		return null;
	}

	public void AddWindow(XUiV_Window _window)
	{
		windows.Add(_window);
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
			if (WindowGroups[i].ID.EqualsCaseInsensitive(_name))
			{
				return WindowGroups[i].Controller;
			}
		}
		return null;
	}

	public XUiController GetChildById(string _id)
	{
		XUiController xUiController = null;
		for (int i = 0; i < WindowGroups.Count; i++)
		{
			xUiController = WindowGroups[i].Controller.GetChildById(_id);
			if (xUiController != null)
			{
				return xUiController;
			}
		}
		return xUiController;
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
		ControllersByType.TryGetValue(typeFromHandle, out var value);
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
		ControllersByType.TryGetValue(typeFromHandle, out var value);
		List<T> list = new List<T>();
		if (value != null)
		{
			foreach (XUiController item in value)
			{
				list.Add((T)item);
			}
		}
		return list;
	}

	public XUiWindowGroup GetWindowGroupById(string _id)
	{
		foreach (XUiWindowGroup windowGroup in WindowGroups)
		{
			if (windowGroup.ID == _id)
			{
				return windowGroup;
			}
		}
		return null;
	}

	public static string UppercaseFirst(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		return char.ToUpper(s[0]) + s.Substring(1);
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
		_ = new RecipeQueueItem[recipesToCraft.Length];
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
		StartCoroutine(SetCraftingDataAsync(_cd));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SetCraftingDataAsync(CraftingData _cd)
	{
		while (!isReady)
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

	public UIAnchor GetAnchor(UIAnchor.Side _anchorSide)
	{
		return xuiAnchors[(int)_anchorSide];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAnchors()
	{
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
		float num2 = (1f - num) / 2f;
		UIAnchor[] array = anchors;
		foreach (UIAnchor uIAnchor in array)
		{
			if (uIAnchor != null && !(uIAnchor.name == "AnchorCenterCenter"))
			{
				if (uIAnchor.side == UIAnchor.Side.Right || uIAnchor.side == UIAnchor.Side.TopRight || uIAnchor.side == UIAnchor.Side.BottomRight)
				{
					uIAnchor.relativeOffset.x = 0f - num2;
				}
				if (uIAnchor.side == UIAnchor.Side.Left || uIAnchor.side == UIAnchor.Side.TopLeft || uIAnchor.side == UIAnchor.Side.BottomLeft)
				{
					uIAnchor.relativeOffset.x = num2;
				}
				if (uIAnchor.side == UIAnchor.Side.Top || uIAnchor.side == UIAnchor.Side.TopRight || uIAnchor.side == UIAnchor.Side.TopLeft)
				{
					uIAnchor.relativeOffset.y = 0f - num2;
				}
				if (uIAnchor.side == UIAnchor.Side.Bottom || uIAnchor.side == UIAnchor.Side.BottomRight || uIAnchor.side == UIAnchor.Side.BottomLeft)
				{
					uIAnchor.relativeOffset.y = num2;
				}
				if (uIAnchor.side == UIAnchor.Side.Bottom || uIAnchor.side == UIAnchor.Side.Top || uIAnchor.side == UIAnchor.Side.Center)
				{
					uIAnchor.relativeOffset.x = 0f;
				}
			}
		}
	}

	public static void HandlePaging(XUi _xui, Func<bool> _onPageUp, Func<bool> _onPageDown, bool useVerticalAxis = false)
	{
		if (null != _xui.playerUI && _xui.playerUI.playerInput != null && _xui.playerUI.playerInput.GUIActions != null && _xui.playerUI.windowManager.IsKeyShortcutsAllowed())
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
			bool flag = false;
			if (useVerticalAxis)
			{
				if ((vector.y > 0f && _onPageUp()) || (vector.y < 0f && _onPageDown()))
				{
					flag = true;
				}
			}
			else if ((vector.x > 0f && _onPageUp()) || (vector.x < 0f && _onPageDown()))
			{
				flag = true;
			}
			if (flag)
			{
				_xui.playerUI.CursorController.PlayPagingSound();
			}
			pagingRepeatTimer = (initialPagingInput ? 0.35f : 0.1f);
			initialPagingInput = false;
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

	public void GetOpenWindows(List<XUiV_Window> list)
	{
		list.Clear();
		foreach (XUiV_Window window in windows)
		{
			if (window.IsOpen && window.IsVisible)
			{
				list.Add(window);
			}
		}
	}

	public void ForceInputStyleChange()
	{
		List<XUiV_Window> list = new List<XUiV_Window>();
		GetOpenWindows(list);
		foreach (XUiV_Window item in list)
		{
			item.Controller.ForceInputStyleChange(PlatformManager.NativePlatform.Input.CurrentInputStyle, PlatformManager.NativePlatform.Input.CurrentInputStyle);
		}
	}

	public static bool IsMatchingPlatform(string platformStr)
	{
		bool result = true;
		string[] array = platformStr.Split(",");
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim().ToUpper();
			if (!array[i].StartsWith("!"))
			{
				result = false;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (Submission.Enabled)
			{
				if (array[j] == "SUBMISSION")
				{
					return true;
				}
				if (array[j] == "!SUBMISSION")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneWindows.IsCurrent())
			{
				if (array[j] == "WINDOWS")
				{
					return true;
				}
				if (array[j] == "!WINDOWS")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneLinux.IsCurrent())
			{
				if (array[j] == "LINUX")
				{
					return true;
				}
				if (array[j] == "!LINUX")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneOSX.IsCurrent())
			{
				if (array[j] == "OSX")
				{
					return true;
				}
				if (array[j] == "!OSX")
				{
					return false;
				}
			}
			if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
			{
				if (array[j] == "STANDALONE")
				{
					return true;
				}
				if (array[j] == "!STANDALONE")
				{
					return false;
				}
			}
			if (DeviceFlag.PS5.IsCurrent())
			{
				if (array[j] == "PS5")
				{
					return true;
				}
				if (array[j] == "!PS5")
				{
					return false;
				}
			}
			if (DeviceFlag.XBoxSeriesS.IsCurrent())
			{
				if (array[j] == "XBOX_S")
				{
					return true;
				}
				if (array[j] == "!XBOX_S")
				{
					return false;
				}
			}
			if (DeviceFlag.XBoxSeriesX.IsCurrent())
			{
				if (array[j] == "XBOX_X")
				{
					return true;
				}
				if (array[j] == "!XBOX_X")
				{
					return false;
				}
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
			{
				if (array[j] == "XBOX")
				{
					return true;
				}
				if (array[j] == "!XBOX")
				{
					return false;
				}
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
			{
				if (array[j] == "CONSOLE")
				{
					return true;
				}
				if (array[j] == "!CONSOLE")
				{
					return false;
				}
			}
		}
		return result;
	}
}

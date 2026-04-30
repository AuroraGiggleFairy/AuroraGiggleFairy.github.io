using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using WorldGenerationEngineFinal;

[Preserve]
public class XUiC_WorldGenerationWindowGroup : XUiController
{
	public struct PrefabData
	{
		public string Name;

		public Vector3i Position;

		public byte Rotation;

		public string DistantPOIOverride;

		public int ID;
	}

	public enum PreviewQuality
	{
		NoPreview,
		Lowest,
		Low,
		Default,
		High,
		Highest
	}

	public static XUiC_WorldGenerationWindowGroup Instance;

	public string LastWindowID = string.Empty;

	public DynamicPrefabDecorator PrefabDecorator;

	public XUiC_WorldGenerationPreview PreviewWindow;

	public PrefabPreviewManager prefabPreviewManager;

	public XUiC_TextInput SeedInput;

	public XUiC_SimpleButton GenerateButton;

	public XUiC_SimpleButton BackButton;

	public XUiC_SimpleButton NewGameButton;

	public XUiC_ComboBoxList<int> WorldSizeComboBox;

	public XUiC_ComboBoxEnum<SaveDataLimitType> SaveDataLimitComboBox;

	public XUiC_ComboBoxBool TerrainAndBiomeOnly;

	public XUiV_Label CountyNameLabel;

	public XUiC_ComboBoxEnum<WorldBuilder.BiomeLayout> BiomeLayoutComboBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt[] biomeComboBoxes = new XUiC_ComboBoxInt[5];

	public XUiC_ComboBoxInt PlainsWeight;

	public XUiC_ComboBoxInt HillsWeight;

	public XUiC_ComboBoxInt MountainsWeight;

	public XUiC_ComboBoxEnum<PreviewQuality> Quality;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Rivers;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Craters;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Canyons;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Lakes;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Rural;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Town;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> City;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Towns;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Wilderness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnManage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dataManagementBarEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_pendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_totalAvailableBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAdvancedUI;

	public int WorldSize;

	public bool ValidCountyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string CountyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldFogDensity;

	public PreviewQuality PreviewQualityLevel = PreviewQuality.Default;

	public WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGenerating;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosing;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] BiomeToUIName = new string[5] { "xuiPineForest", "xuiBurntForest", "xuiDesert", "xuiSnow", "xuiWasteland" };

	public bool HasSufficientSpace
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (SaveInfoProvider.DataLimitEnabled)
			{
				return m_pendingBytes <= m_totalAvailableBytes;
			}
			return true;
		}
	}

	public event Action OnCountyNameChanged;

	public event Action OnWorldSizeChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		int num;
		if (!(_bindingName == "showbar"))
		{
			if (_bindingName == "canNewGame")
			{
				if (!isGenerating)
				{
					WorldBuilder worldBuilder = this.worldBuilder;
					if (worldBuilder != null && worldBuilder.IsFinished && !worldBuilder.IsCanceled && this.worldBuilder.CanSaveData())
					{
						num = (HasSufficientSpace ? 1 : 0);
						goto IL_0064;
					}
				}
				num = 0;
				goto IL_0064;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = dataManagementBarEnabled.ToString();
		return true;
		IL_0064:
		bool flag = (byte)num != 0;
		_value = flag.ToString();
		return true;
	}

	public override void Init()
	{
		base.Init();
		PreviewWindow = GetChildByType<XUiC_WorldGenerationPreview>();
		SeedInput = GetChildByType<XUiC_TextInput>();
		GenerateButton = GetChildById("generate") as XUiC_SimpleButton;
		TerrainAndBiomeOnly = GetChildById("cbxTerrainAndBiomeOnly") as XUiC_ComboBoxBool;
		BackButton = GetChildById("btnBack") as XUiC_SimpleButton;
		NewGameButton = GetChildById("btnNewGame") as XUiC_SimpleButton;
		TerrainAndBiomeOnly = GetChildById("cbxTerrainAndBiomeOnly") as XUiC_ComboBoxBool;
		if (GetChildById("countyName") != null)
		{
			CountyNameLabel = GetChildById("countyName").ViewComponent as XUiV_Label;
		}
		btnManage = GetChildById("btnDataManagement") as XUiC_SimpleButton;
		dataManagementBar = GetChildById("data_bar_controller") as XUiC_DataManagementBar;
		dataManagementBarEnabled = dataManagementBar != null && SaveInfoProvider.DataLimitEnabled;
	}

	public static bool IsGenerating()
	{
		WorldBuilder worldBuilder = Instance?.worldBuilder;
		if (worldBuilder != null)
		{
			return !worldBuilder.IsFinished;
		}
		return false;
	}

	public static void CancelGeneration()
	{
		WorldBuilder worldBuilder = Instance?.worldBuilder;
		if (worldBuilder != null)
		{
			worldBuilder.IsCanceled = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NewGameButton_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.StartCoroutine(SaveAndNewGameCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SaveAndNewGameCo()
	{
		if (!isClosing)
		{
			isClosing = true;
			bool shouldClose = false;
			yield return worldBuilder.SaveData(canPrompt: true, GetParentWindow()?.Controller ?? this, autoConfirm: true, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				shouldClose = false;
			}, null, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				shouldClose = true;
			});
			if (!shouldClose)
			{
				isClosing = false;
				yield break;
			}
			XUiC_NewContinueGame.SetIsContinueGame(base.xui, _continueGame: false);
			GamePrefs.Set(EnumGamePrefs.GameWorld, CountyName);
			CheckProfile(XUiC_NewContinueGame.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckProfile(string _windowToOpen)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		if (ProfileSDF.CurrentProfileName().Length == 0)
		{
			XUiC_OptionsProfiles.Open(base.xui, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				base.xui.playerUI.windowManager.Open(_windowToOpen, _bModal: true);
			});
		}
		else
		{
			base.xui.playerUI.windowManager.Open(_windowToOpen, _bModal: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		StartClose();
	}

	public override void OnOpen()
	{
		Instance = this;
		base.OnOpen();
		isAdvancedUI = windowGroup.ID == "rwgeditor";
		if (isAdvancedUI)
		{
			windowGroup.isEscClosable = false;
		}
		isClosing = false;
		if (!base.xui.playerUI.windowManager.IsWindowOpen(XUiC_NewContinueGame.ID))
		{
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGEditor);
		}
		PathAbstractions.CacheEnabled = true;
		if (PreviewWindow != null)
		{
			prefabPreviewManager = new PrefabPreviewManager();
		}
		if ((WorldSizeComboBox = GetChildById("WorldSize") as XUiC_ComboBoxList<int>) != null)
		{
			if (PlatformOptimizations.EnforceMaxWorldSizeHost)
			{
				int num = WorldSizeComboBox.Elements.FindLastIndex([PublicizedFrom(EAccessModifier.Internal)] (int element) => element <= PlatformOptimizations.MaxWorldSizeHost);
				if (num >= 0)
				{
					WorldSizeComboBox.MinIndex = 0;
					WorldSizeComboBox.MaxIndex = num;
					WorldSizeComboBox.SelectedIndex = num;
				}
			}
			if (WorldSizeComboBox.Elements.Contains(8192))
			{
				WorldSizeComboBox.Value = 8192;
			}
		}
		SaveDataLimitComboBox = SaveDataLimitUIHelper.AddComboBox(GetChildById("SaveDataLimitComboBox") as XUiC_ComboBoxEnum<SaveDataLimitType>);
		if ((Rivers = GetChildById("Rivers") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>) != null)
		{
			Rivers.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((Craters = GetChildById("Craters") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>) != null)
		{
			Craters.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((Canyons = GetChildById("Cracks") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>) != null)
		{
			Canyons.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((Lakes = GetChildById("Lakes") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>) != null)
		{
			Lakes.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((Rural = GetChildById("Rural") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>) != null)
		{
			Rural.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((Town = GetChildById("Town") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>) != null)
		{
			Town.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((City = GetChildById("City") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>) != null)
		{
			City.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((Towns = GetChildById("Towns") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>) != null)
		{
			Towns.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((Wilderness = GetChildById("Wilderness") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>) != null)
		{
			Wilderness.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((PlainsWeight = GetChildById("PlainsWeight") as XUiC_ComboBoxInt) != null)
		{
			PlainsWeight.Value = 4L;
			PlainsWeight.OnValueChanged += PlainsWeight_OnValueChanged;
		}
		if ((HillsWeight = GetChildById("HillsWeight") as XUiC_ComboBoxInt) != null)
		{
			HillsWeight.Value = 4L;
			HillsWeight.OnValueChanged += HillsWeight_OnValueChanged;
		}
		if ((MountainsWeight = GetChildById("MountainsWeight") as XUiC_ComboBoxInt) != null)
		{
			MountainsWeight.Value = 2L;
			MountainsWeight.OnValueChanged += MountainsWeight_OnValueChanged;
		}
		if ((BiomeLayoutComboBox = GetChildById("BiomeLayout") as XUiC_ComboBoxEnum<WorldBuilder.BiomeLayout>) != null)
		{
			BiomeLayoutComboBox.Value = WorldBuilder.BiomeLayout.CenterForest;
		}
		XUiController childById = GetChildById("biomes");
		if (childById != null)
		{
			for (int num2 = 0; num2 < 5; num2++)
			{
				XUiController xUiController = childById.Children[num2];
				XUiController childById2 = xUiController.GetChildById("label");
				if (childById2 != null)
				{
					((XUiV_Label)childById2.ViewComponent).Text = Localization.Get(BiomeToUIName[num2]);
				}
				XUiC_ComboBoxInt childByType = xUiController.GetChildByType<XUiC_ComboBoxInt>();
				biomeComboBoxes[num2] = childByType;
				if (childByType != null)
				{
					childByType.Value = WorldBuilderConstants.BiomeWeightDefaults[num2];
					childByType.OnValueChanged += BiomeWeight_OnValueChanged;
				}
				XUiController childById3 = xUiController.GetChildById("color");
				if (childById3 != null)
				{
					XUiV_Sprite obj = (XUiV_Sprite)childById3.ViewComponent;
					Color color = (Color)WorldBuilderConstants.biomeColorList[num2] * 0.7f;
					color.a = 1f;
					obj.Color = color;
				}
			}
		}
		updateTerrainPercentages();
		updateBiomePercentages();
		if (BackButton != null)
		{
			BackButton.OnPressed += BtnBack_OnPressed;
		}
		if (GenerateButton != null)
		{
			GenerateButton.OnPressed += GenerateButton_OnPressed;
		}
		if (NewGameButton != null)
		{
			NewGameButton.OnPressed += NewGameButton_OnPressed;
		}
		if (btnManage != null)
		{
			btnManage.OnPressed += BtnManage_OnPressed;
		}
		if ((Quality = GetChildById("PreviewQuality") as XUiC_ComboBoxEnum<PreviewQuality>) != null)
		{
			Quality.SetMinMax(EnumUtils.MinValue<PreviewQuality>(), PlatformOptimizations.MaxRWGPreviewQuality);
			Quality.Value = (PreviewQuality)Math.Min(3, (int)Quality.Max);
			Quality.OnValueChanged += Quality_OnValueChanged;
		}
		if (SeedInput != null)
		{
			SeedInput.OnChangeHandler += SeedInput_OnChangeHandler;
			SeedInput_OnChangeHandler(SeedInput, SeedInput.Text, _changeFromCode: true);
		}
		if (WorldSizeComboBox != null)
		{
			WorldSizeComboBox.OnValueChanged += WorldSizeComboBox_OnValueChanged;
			WorldSizeComboBox_OnValueChanged(WorldSizeComboBox, WorldSizeComboBox.Value - 1, WorldSizeComboBox.Value);
		}
		oldFogDensity = RenderSettings.fogDensity;
		RenderSettings.fogDensity = 0f;
		UpdateBarValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Quality_OnValueChanged(XUiController _sender, PreviewQuality _oldValue, PreviewQuality _newValue)
	{
		PreviewQualityLevel = _newValue;
		if (XUiC_WorldGenerationPreview.Instance != null)
		{
			XUiC_WorldGenerationPreview.Instance.GeneratePreview();
		}
		if (prefabPreviewManager != null && prefabPreviewManager.initialized && (_oldValue < PreviewQuality.Low || _oldValue > PreviewQuality.High || _newValue < PreviewQuality.Low || _newValue > PreviewQuality.High))
		{
			prefabPreviewManager.RemovePrefabs();
			prefabPreviewManager.InitPrefabs();
			prefabPreviewManager.ForceUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BiomeWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		updateBiomePercentages();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlainsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		updateTerrainPercentages();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HillsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		updateTerrainPercentages();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MountainsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		updateTerrainPercentages(_isMountainsChanged: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTerrainPercentages(bool _isMountainsChanged = false)
	{
		if (PlainsWeight == null || HillsWeight == null || MountainsWeight == null)
		{
			return;
		}
		int num2;
		int num3;
		int num4;
		while (true)
		{
			float num = PlainsWeight.Value + HillsWeight.Value + MountainsWeight.Value;
			if (num <= 0f)
			{
				num = 1f;
			}
			num2 = Mathf.RoundToInt((float)PlainsWeight.Value / num * 100f);
			num3 = Mathf.RoundToInt((float)HillsWeight.Value / num * 100f);
			num4 = Mathf.RoundToInt((float)MountainsWeight.Value / num * 100f);
			if (num4 <= 50)
			{
				break;
			}
			if (_isMountainsChanged)
			{
				PlainsWeight.Value++;
			}
			else
			{
				MountainsWeight.Value--;
			}
		}
		if (num2 + num3 + num4 == 0)
		{
			num2 = 100;
		}
		PlainsWeight.UpdateLabel($"{Mathf.Max(0, num2)}%");
		HillsWeight.UpdateLabel($"{Mathf.Max(0, num3)}%");
		MountainsWeight.UpdateLabel($"{Mathf.Max(0, num4)}%");
		PlainsWeight.IsDirty = true;
		HillsWeight.IsDirty = true;
		MountainsWeight.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBiomePercentages()
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		for (int i = 0; i < 5; i++)
		{
			XUiC_ComboBoxInt xUiC_ComboBoxInt = biomeComboBoxes[i];
			if (xUiC_ComboBoxInt == null)
			{
				return;
			}
			float num4 = xUiC_ComboBoxInt.Value;
			num += num4;
			if (num4 > num2)
			{
				num2 = num4;
				num3 = i;
			}
		}
		int num5 = 0;
		for (int j = 0; j < 5; j++)
		{
			XUiC_ComboBoxInt obj = biomeComboBoxes[j];
			int v = Mathf.RoundToInt((float)obj.Value / num * 100f);
			v = Utils.FastMax(5, v);
			num5 += v;
			if (j == 4)
			{
				v += 100 - num5;
				if (v < 5)
				{
					XUiC_ComboBoxInt obj2 = biomeComboBoxes[num3];
					int num6 = Mathf.RoundToInt((float)obj2.Value / num * 100f);
					obj2.UpdateLabel($"{num6 + 5 - v}%");
					v = 5;
				}
			}
			obj.UpdateLabel($"{v}%");
			obj.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldSizeComboBox_OnValueChanged(XUiController _sender, int _oldValue, int _newValue)
	{
		WorldSize = _newValue;
		RefreshCountyName();
		this.OnWorldSizeChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SeedInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		RefreshCountyName();
	}

	public void RefreshCountyName()
	{
		CountyName = WorldBuilder.GetGeneratedWorldName(SeedInput.Text, WorldSize);
		ValidateNewRwg();
		if (CountyNameLabel != null)
		{
			CountyNameLabel.Text = CountyName;
		}
		TriggerCountyNameChangedEvent();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateNewRwg()
	{
		string countyName = CountyName;
		bool flag = PathAbstractions.WorldsSearchPaths.GetLocation(countyName, countyName).Type != PathAbstractions.EAbstractedLocationType.None;
		ValidCountyName = !flag;
		if (CountyNameLabel != null)
		{
			CountyNameLabel.Color = (ValidCountyName ? Color.white : Color.red);
			if (flag)
			{
				CountyNameLabel.ToolTip = Localization.Get("mmLblRwgSeedErrorWorldExists");
			}
			else
			{
				CountyNameLabel.ToolTip = "";
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerCountyNameChangedEvent()
	{
		if (this.OnCountyNameChanged != null)
		{
			this.OnCountyNameChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnManage_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DataManagement.OpenDataManagementWindow(this, OnDataManagementWindowClosed);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDataManagementWindowClosed()
	{
		UpdateBarValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarValues()
	{
		if (!dataManagementBarEnabled)
		{
			RefreshBindings();
			return;
		}
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
		dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
		m_pendingBytes = worldBuilder?.SerializedSize ?? 0;
		m_totalAvailableBytes = instance.TotalAvailableBytes;
		dataManagementBar.SetPendingBytes(m_pendingBytes);
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateButton_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (PreviewWindow != null)
		{
			PreviewWindow.CleanupTerrainMesh();
		}
		if (prefabPreviewManager != null)
		{
			prefabPreviewManager.ClearOldPreview();
		}
		ThreadManager.StartCoroutine(GenerateCo());
	}

	public IEnumerator GenerateCo(bool _usePreviewer = true, Action<string> onSuccess = null, Action onFailure = null)
	{
		isGenerating = true;
		UpdateBarValues();
		DestroyBuilder();
		worldBuilder = new WorldBuilder(SeedInput.Text, WorldSizeComboBox.Value);
		worldBuilder.UsePreviewer = _usePreviewer;
		if (Towns != null)
		{
			worldBuilder.Towns = Towns.Value;
		}
		if (Wilderness != null)
		{
			worldBuilder.Wilderness = Wilderness.Value;
		}
		if (Rivers != null)
		{
			worldBuilder.Rivers = Rivers.Value;
		}
		if (Craters != null)
		{
			worldBuilder.Craters = Craters.Value;
		}
		if (Canyons != null)
		{
			worldBuilder.Canyons = Canyons.Value;
		}
		if (Lakes != null)
		{
			worldBuilder.Lakes = Lakes.Value;
		}
		if (PlainsWeight != null)
		{
			worldBuilder.Plains = (int)PlainsWeight.Value;
		}
		if (HillsWeight != null)
		{
			worldBuilder.Hills = (int)HillsWeight.Value;
		}
		if (MountainsWeight != null)
		{
			worldBuilder.Mountains = (int)MountainsWeight.Value;
		}
		if (BiomeLayoutComboBox != null)
		{
			worldBuilder.biomeLayout = BiomeLayoutComboBox.Value;
		}
		for (int i = 0; i < 5; i++)
		{
			XUiC_ComboBoxInt xUiC_ComboBoxInt = biomeComboBoxes[i];
			if (xUiC_ComboBoxInt != null)
			{
				worldBuilder.SetBiomeWeight((BiomeType)i, (int)xUiC_ComboBoxInt.Value);
			}
		}
		if (Quality != null)
		{
			PreviewQualityLevel = Quality.Value;
		}
		PrefabPreviewManager.ReadyToDisplay = false;
		UpdateBarValues();
		yield return GCUtils.WaitForIdle();
		yield return worldBuilder.GenerateFromUI();
		if (worldBuilder.UsePreviewer)
		{
			yield return worldBuilder.FinishForPreview();
			if (XUiC_WorldGenerationPreview.Instance != null)
			{
				XUiC_WorldGenerationPreview.Instance.GeneratePreview();
			}
			if (!worldBuilder.IsCanceled)
			{
				yield return new WaitForSeconds(2f);
			}
		}
		else
		{
			XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
			bool success = false;
			yield return worldBuilder.SaveData(canPrompt: true, GetParentWindow()?.Controller ?? this, autoConfirm: true, null, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				success = false;
			}, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				success = true;
			});
			if (success)
			{
				onSuccess?.Invoke(worldBuilder.WorldName);
			}
			else
			{
				onFailure?.Invoke();
			}
			DestroyBuilder();
		}
		XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
		PrefabPreviewManager.ReadyToDisplay = true;
		isGenerating = false;
		UpdateBarValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyBuilder()
	{
		if (worldBuilder != null)
		{
			worldBuilder.Cleanup();
			worldBuilder = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartClose()
	{
		if (!isGenerating)
		{
			if (worldBuilder == null || !worldBuilder.IsFinished || !worldBuilder.CanSaveData())
			{
				Close();
			}
			else
			{
				base.xui.StartCoroutine(StartCloseCo());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StartCloseCo()
	{
		if (!isClosing)
		{
			isClosing = true;
			bool shouldClose = false;
			yield return worldBuilder.SaveData(canPrompt: true, GetParentWindow()?.Controller ?? this, autoConfirm: false, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				shouldClose = false;
			}, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				shouldClose = true;
			}, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				shouldClose = true;
			});
			if (!shouldClose)
			{
				UpdateBarValues();
				isClosing = false;
			}
			else
			{
				Close();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Close()
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(LastWindowID, _bModal: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGEditor);
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGCamera);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		Clean();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Clean()
	{
		DestroyBuilder();
		PathAbstractions.CacheEnabled = false;
		if (BackButton != null)
		{
			BackButton.OnPressed -= BtnBack_OnPressed;
		}
		if (GenerateButton != null)
		{
			GenerateButton.OnPressed -= GenerateButton_OnPressed;
		}
		if (NewGameButton != null)
		{
			NewGameButton.OnPressed -= NewGameButton_OnPressed;
		}
		if (btnManage != null)
		{
			btnManage.OnPressed -= BtnManage_OnPressed;
		}
		if (Quality != null)
		{
			Quality.OnValueChanged -= Quality_OnValueChanged;
		}
		if (SeedInput != null)
		{
			SeedInput.OnChangeHandler -= SeedInput_OnChangeHandler;
		}
		if (WorldSizeComboBox != null)
		{
			WorldSizeComboBox.OnValueChanged -= WorldSizeComboBox_OnValueChanged;
		}
		if (PlainsWeight != null)
		{
			PlainsWeight.OnValueChanged -= PlainsWeight_OnValueChanged;
		}
		if (HillsWeight != null)
		{
			HillsWeight.OnValueChanged -= HillsWeight_OnValueChanged;
		}
		if (MountainsWeight != null)
		{
			MountainsWeight.OnValueChanged -= MountainsWeight_OnValueChanged;
		}
		if (prefabPreviewManager != null)
		{
			prefabPreviewManager.Cleanup();
			prefabPreviewManager = null;
		}
		RenderSettings.fogDensity = oldFogDensity;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (isAdvancedUI)
		{
			if (base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed && !XUiC_DataManagement.IsWindowOpen(base.xui))
			{
				StartClose();
			}
			if (PrefabPreviewManager.ReadyToDisplay && prefabPreviewManager != null)
			{
				prefabPreviewManager.Update();
			}
		}
	}
}

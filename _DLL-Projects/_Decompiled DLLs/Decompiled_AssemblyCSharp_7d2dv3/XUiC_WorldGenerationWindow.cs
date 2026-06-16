using System;
using System.Collections;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;
using WorldGenerationEngineFinal;

[Preserve]
public class XUiC_WorldGenerationWindow : XUiController
{
	public enum PreviewQuality
	{
		NoPreview,
		Lowest,
		Low,
		Default,
		High,
		Highest
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class XUiC_RwgBiome : XUiController
	{
		[XuiBindComponent(true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_ComboBoxInt combo;

		[PublicizedFrom(EAccessModifier.Private)]
		public int biomeIdx;

		public int BiomeIdx
		{
			get
			{
				return biomeIdx;
			}
			set
			{
				biomeIdx = value;
				combo.Value = ((BiomeIdx < WorldBuilderConstants.BiomeWeightDefaults.Length) ? WorldBuilderConstants.BiomeWeightDefaults[BiomeIdx] : 0);
				IsDirty = true;
			}
		}

		[XuiXmlBinding("biomename")]
		public string BiomeName
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (BiomeIdx >= biomeToUiName.Length)
				{
					return "";
				}
				return biomeToUiName[BiomeIdx];
			}
		}

		[XuiXmlBinding("biomecolor")]
		public Color BiomeColor
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (BiomeIdx >= WorldBuilderConstants.biomeColorList.Count)
				{
					return Color.clear;
				}
				return ((Color)WorldBuilderConstants.biomeColorList[BiomeIdx] * 0.7f).WithAlpha(1f);
			}
		}

		public int Value => (int)combo.Value;

		public event Action ValueChanged;

		[XuiBindEvent("OnValueChanged", "combo")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void comboValueChanged(XUiController _sender, long _oldValue, long _newValue)
		{
			this.ValueChanged?.Invoke();
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			handleDirtyUpdateDefault();
		}

		public void SetLabel(string _text)
		{
			combo.ValueTextOverride = _text;
		}
	}

	public static XUiC_WorldGenerationWindow Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastWindowID = string.Empty;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationPreview previewWindow;

	[XuiBindComponent("progress", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView progressView;

	[XuiBindComponent("seedInput", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtSeedInput;

	[XuiBindComponent("generate", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnGenerate;

	[XuiBindComponent("btnNewGame", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnNewGame;

	[XuiBindComponent("WorldSize", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<int> cbxWorldSize;

	[XuiBindComponent("SaveDataLimitComboBox", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<SaveDataLimitType> cbxSaveDataLimit;

	[XuiBindComponent("PreviewQuality", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<PreviewQuality> cbxPreviewQuality;

	[XuiBindComponent("BiomeLayout", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.BiomeLayout> cbxBiomeLayout;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_RwgBiome[] biomeBoxes;

	[XuiBindComponent("PlainsWeight", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxInt cbxPlainsWeight;

	[XuiBindComponent("HillsWeight", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxInt cbxHillsWeight;

	[XuiBindComponent("MountainsWeight", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxInt cbxMountainsWeight;

	[XuiBindComponent("Rivers", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxRivers;

	[XuiBindComponent("Craters", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxCraters;

	[XuiBindComponent("Cracks", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxCanyons;

	[XuiBindComponent("Lakes", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxLakes;

	[XuiBindComponent("Rural", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxRural;

	[XuiBindComponent("Town", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxTown;

	[XuiBindComponent("City", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxCity;

	[XuiBindComponent("Towns", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxTowns;

	[XuiBindComponent("Wilderness", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxWilderness;

	[XuiBindComponent("btnDataManagement", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnManage;

	[XuiBindComponent("data_bar_controller", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DataManagementBar dataManagementBar;

	[XuiBindComponent("GameSaveStorageComboBox", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<UserDataStorageType> cbxGameSaveStorage;

	[XuiBindComponent("WorldSaveStorage", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<UserDataStorageType> cbxWorldSaveStorage;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabPreviewManager prefabPreviewManagerInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public long pendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long totalAvailableBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validCountyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string countyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statusText;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldFogDensity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosing;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] biomeToUiName = new string[5] { "xuiPineForest", "xuiBurntForest", "xuiDesert", "xuiSnow", "xuiWasteland" };

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGenerating;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCancelling;

	public PreviewQuality PreviewQualityLevel => cbxPreviewQuality?.Value ?? PreviewQuality.Default;

	public WorldBuilder.BiomeLayout BiomeLayout
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxBiomeLayout?.Value ?? WorldBuilder.BiomeLayout.CenterForest;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxBiomeLayout != null)
			{
				cbxBiomeLayout.Value = value;
			}
		}
	}

	public int PlainsWeight
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (int)(cbxPlainsWeight?.Value ?? 4);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxPlainsWeight != null)
			{
				cbxPlainsWeight.Value = value;
			}
		}
	}

	public int HillsWeight
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (int)(cbxHillsWeight?.Value ?? 4);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxHillsWeight != null)
			{
				cbxHillsWeight.Value = value;
			}
		}
	}

	public int MountainsWeight
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (int)(cbxMountainsWeight?.Value ?? 2);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxMountainsWeight != null)
			{
				cbxMountainsWeight.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Rivers
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxRivers?.Value ?? WorldBuilder.GenerationSelections.Default;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxRivers != null)
			{
				cbxRivers.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Craters
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxCraters?.Value ?? WorldBuilder.GenerationSelections.Default;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxCraters != null)
			{
				cbxCraters.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Canyons
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxCanyons?.Value ?? WorldBuilder.GenerationSelections.Default;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxCanyons != null)
			{
				cbxCanyons.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Lakes
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxLakes?.Value ?? WorldBuilder.GenerationSelections.Default;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxLakes != null)
			{
				cbxLakes.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Rural
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxRural?.Value ?? WorldBuilder.GenerationSelections.Default;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxRural != null)
			{
				cbxRural.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Town
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxTown?.Value ?? WorldBuilder.GenerationSelections.Default;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxTown != null)
			{
				cbxTown.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections City
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxCity?.Value ?? WorldBuilder.GenerationSelections.Default;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxCity != null)
			{
				cbxCity.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Towns
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxTowns?.Value ?? WorldBuilder.GenerationSelections.Default;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxTowns != null)
			{
				cbxTowns.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Wilderness
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxWilderness?.Value ?? WorldBuilder.GenerationSelections.Default;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (cbxWilderness != null)
			{
				cbxWilderness.Value = value;
			}
		}
	}

	public bool HasSufficientSpace
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (SaveInfoProvider.DataLimitEnabled)
			{
				return pendingBytes <= totalAvailableBytes;
			}
			return true;
		}
	}

	public bool IsAdvancedUi
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return windowGroup.Id == XUiC_WorldGenerationWindowGroup.ID;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int WorldSize
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("countynamevalid")]
	public bool ValidCountyName
	{
		get
		{
			return validCountyName;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			validCountyName = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("countyname")]
	public string CountyName
	{
		get
		{
			return countyName ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			countyName = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("statustext")]
	public string StatusText
	{
		get
		{
			return statusText ?? "";
		}
		set
		{
			statusText = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("showbar")]
	public bool HasDataManagementBar
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (dataManagementBar != null)
			{
				return SaveInfoProvider.DataLimitEnabled;
			}
			return false;
		}
	}

	[XuiXmlBinding("canstartnewgame")]
	public bool CanStartNewGame
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!IsGenerating)
			{
				WorldBuilder worldBuilder = WorldBuilder;
				if (worldBuilder != null && worldBuilder.IsFinished && !worldBuilder.IsCanceled && WorldBuilder.CanSaveData())
				{
					return HasSufficientSpace;
				}
			}
			return false;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public WorldBuilder WorldBuilder
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("isgenerating")]
	public bool IsGenerating
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return isGenerating;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			isGenerating = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("iscancelling")]
	public bool IsCancelling
	{
		get
		{
			return isCancelling;
		}
		set
		{
			isCancelling = value;
			IsDirty = true;
		}
	}

	public static bool WorldBuilderIsGenerating
	{
		get
		{
			WorldBuilder worldBuilder = Instance?.WorldBuilder;
			if (worldBuilder != null)
			{
				return !worldBuilder.IsFinished;
			}
			return false;
		}
	}

	public event Action OnCountyNameChanged;

	public event Action OnWorldSizeChanged;

	public event Action OnWorldStorageChanged;

	public event Action<UserDataStorageType> OnGameSaveStorageChanged;

	[XuiXmlBinding("isroamingoptional")]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bindingIsRoamingOptional()
	{
		return PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional;
	}

	public static void CancelGeneration()
	{
		WorldBuilder worldBuilder = Instance?.WorldBuilder;
		if (worldBuilder != null)
		{
			worldBuilder.IsCanceled = true;
			Instance.IsCancelling = true;
		}
	}

	[XuiBindEvent("OnPress", "btnNewGame")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void NewGameButton_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.StartCoroutine(saveAndNewGameCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator saveAndNewGameCo()
	{
		if (!isClosing)
		{
			isClosing = true;
			bool shouldClose = false;
			yield return WorldBuilder.SaveData(WorldBuilder.SaveDataPromptMode.OnInsufficientSpace, GetParentWindow()?.Controller ?? this, [PublicizedFrom(EAccessModifier.Internal)] () =>
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
			GamePrefs.Set(EnumGamePrefs.GameWorld, CountyName);
			checkProfile(XUiC_NewGame.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkProfile(string _windowToOpen)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		if (ProfileSDF.CurrentProfileName().Length == 0)
		{
			XUiC_PlayerProfile.Open(xui, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				xui.playerUI.windowManager.Open(_windowToOpen, _bModal: true);
			});
		}
		else
		{
			xui.playerUI.windowManager.Open(_windowToOpen, _bModal: true);
		}
	}

	public override void OnOpen()
	{
		Instance = this;
		base.OnOpen();
		IsGenerating = false;
		IsCancelling = false;
		isClosing = false;
		PathAbstractions.CacheEnabled = true;
		if (previewWindow != null)
		{
			prefabPreviewManagerInstance = new PrefabPreviewManager(previewWindow);
		}
		if (cbxWorldSize != null)
		{
			if (PlatformOptimizations.EnforceMaxWorldSizeHost)
			{
				int num = cbxWorldSize.Elements.FindLastIndex([PublicizedFrom(EAccessModifier.Internal)] (int _element) => _element <= PlatformOptimizations.MaxWorldSizeHost);
				if (num >= 0)
				{
					cbxWorldSize.MinIndex = 0;
					cbxWorldSize.MaxIndex = num;
					cbxWorldSize.SelectedIndex = num;
				}
			}
			if (cbxWorldSize.Elements.Contains(8192))
			{
				cbxWorldSize.Value = 8192;
			}
		}
		if (cbxWorldSaveStorage != null)
		{
			cbxWorldSaveStorage.Value = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.UserWorldStorageType);
		}
		if (cbxGameSaveStorage != null)
		{
			cbxGameSaveStorage.Value = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		}
		SaveDataLimitUIHelper.AddComboBox(cbxSaveDataLimit);
		SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Combine(SaveDataLimitUIHelper.OnValueChanged, new Action(SaveDataLimit_OnChanged));
		Rivers = WorldBuilder.GenerationSelections.Default;
		Craters = WorldBuilder.GenerationSelections.Default;
		Canyons = WorldBuilder.GenerationSelections.Default;
		Lakes = WorldBuilder.GenerationSelections.Default;
		Rural = WorldBuilder.GenerationSelections.Default;
		Town = WorldBuilder.GenerationSelections.Default;
		City = WorldBuilder.GenerationSelections.Default;
		Towns = WorldBuilder.GenerationSelections.Default;
		Wilderness = WorldBuilder.GenerationSelections.Default;
		PlainsWeight = 4;
		HillsWeight = 4;
		MountainsWeight = 2;
		BiomeLayout = WorldBuilder.BiomeLayout.CenterForest;
		for (int num2 = 0; num2 < biomeBoxes.Length; num2++)
		{
			biomeBoxes[num2].BiomeIdx = num2;
		}
		updateTerrainPercentages();
		updateBiomePercentages();
		if (cbxPreviewQuality != null)
		{
			cbxPreviewQuality.Value = PreviewQuality.Default;
			cbxPreviewQuality.Max = PlatformOptimizations.MaxRWGPreviewQuality;
		}
		if (txtSeedInput != null)
		{
			SeedInput_OnChangeHandler(txtSeedInput, txtSeedInput.Text, _changeFromCode: true);
		}
		if (cbxWorldSize != null)
		{
			WorldSizeComboBox_OnValueChanged(cbxWorldSize, cbxWorldSize.Value - 1, cbxWorldSize.Value);
		}
		oldFogDensity = RenderSettings.fogDensity;
		RenderSettings.fogDensity = 0f;
		updateBarValues();
	}

	[XuiBindEvent("OnValueChanged", "cbxPreviewQuality")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Quality_OnValueChanged(XUiController _sender, PreviewQuality _oldValue, PreviewQuality _newValue)
	{
		if (previewWindow != null)
		{
			previewWindow.PreviewInit();
			previewWindow.PreviewTextureDraw();
		}
		if (prefabPreviewManagerInstance != null && prefabPreviewManagerInstance.Initialized && (_oldValue < PreviewQuality.Low || _oldValue > PreviewQuality.High || _newValue < PreviewQuality.Low || _newValue > PreviewQuality.High))
		{
			prefabPreviewManagerInstance.RemovePrefabs();
			prefabPreviewManagerInstance.InitPrefabs();
			prefabPreviewManagerInstance.ForceUpdate();
		}
	}

	[XuiBindEvent("ValueChanged", "biomeBoxes")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BiomeWeight_OnValueChanged()
	{
		updateBiomePercentages();
	}

	[XuiBindEvent("OnValueChanged", "cbxPlainsWeight")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void PlainsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		updateTerrainPercentages();
	}

	[XuiBindEvent("OnValueChanged", "cbxHillsWeight")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void HillsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		updateTerrainPercentages();
	}

	[XuiBindEvent("OnValueChanged", "cbxMountainsWeight")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void MountainsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		updateTerrainPercentages(_isMountainsChanged: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTerrainPercentages(bool _isMountainsChanged = false)
	{
		if (cbxPlainsWeight == null || cbxHillsWeight == null || cbxMountainsWeight == null)
		{
			return;
		}
		int num2;
		int num3;
		int num4;
		while (true)
		{
			float num = PlainsWeight + HillsWeight + MountainsWeight;
			if (num <= 0f)
			{
				num = 1f;
			}
			num2 = Mathf.RoundToInt((float)PlainsWeight / num * 100f);
			num3 = Mathf.RoundToInt((float)HillsWeight / num * 100f);
			num4 = Mathf.RoundToInt((float)MountainsWeight / num * 100f);
			if (num4 <= 50)
			{
				break;
			}
			if (_isMountainsChanged)
			{
				PlainsWeight++;
			}
			else
			{
				MountainsWeight--;
			}
		}
		if (num2 + num3 + num4 == 0)
		{
			num2 = 100;
		}
		cbxPlainsWeight.ValueTextOverride = $"{Mathf.Max(0, num2)}%";
		cbxHillsWeight.ValueTextOverride = $"{Mathf.Max(0, num3)}%";
		cbxMountainsWeight.ValueTextOverride = $"{Mathf.Max(0, num4)}%";
		cbxPlainsWeight.IsDirty = true;
		cbxHillsWeight.IsDirty = true;
		cbxMountainsWeight.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBiomePercentages()
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		for (int i = 0; i < 5; i++)
		{
			XUiC_RwgBiome xUiC_RwgBiome = ((i < biomeBoxes.Length) ? biomeBoxes[i] : null);
			if (xUiC_RwgBiome != null)
			{
				float num4 = xUiC_RwgBiome.Value;
				num += num4;
				if (num4 > num2)
				{
					num2 = num4;
					num3 = i;
				}
			}
		}
		int num5 = 0;
		for (int j = 0; j < 5; j++)
		{
			XUiC_RwgBiome xUiC_RwgBiome2 = ((j < biomeBoxes.Length) ? biomeBoxes[j] : null);
			if (xUiC_RwgBiome2 == null)
			{
				continue;
			}
			int v = Mathf.RoundToInt((float)xUiC_RwgBiome2.Value / num * 100f);
			v = Utils.FastMax(5, v);
			num5 += v;
			if (j == 4)
			{
				v += 100 - num5;
				if (v < 5)
				{
					XUiC_RwgBiome obj = biomeBoxes[num3];
					int num6 = Mathf.RoundToInt((float)obj.Value / num * 100f);
					obj.SetLabel($"{num6 + 5 - v}%");
					v = 5;
				}
			}
			xUiC_RwgBiome2.SetLabel($"{v}%");
			xUiC_RwgBiome2.IsDirty = true;
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxWorldSize")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldSizeComboBox_OnValueChanged(XUiController _sender, int _oldValue, int _newValue)
	{
		WorldSize = _newValue;
		RefreshCountyName();
		this.OnWorldSizeChanged?.Invoke();
	}

	[XuiBindEvent("OnChangeHandler", "txtSeedInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void SeedInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		RefreshCountyName();
	}

	[XuiBindEvent("OnValueChanged", "cbxGameSaveStorage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void GameSaveStorage_OnValueChanged(XUiController _sender, UserDataStorageType _oldValue, UserDataStorageType _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, (int)_newValue);
		GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)_newValue);
		ValidateNewRwg();
		this.OnGameSaveStorageChanged?.Invoke(_newValue);
	}

	[XuiBindEvent("OnValueChanged", "cbxWorldSaveStorage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldSaveStorage_OnValueChanged(XUiController _sender, UserDataStorageType _oldValue, UserDataStorageType _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)_newValue);
		WorldBuilder?.SetStorageType(_newValue);
		updateBarValues();
		ValidateNewRwg();
		this.OnWorldStorageChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveDataLimit_OnChanged()
	{
		validateDataLimit();
	}

	public void SetGameSaveStorageValue(UserDataStorageType _newValue)
	{
		cbxGameSaveStorage.Value = _newValue;
	}

	public void RefreshCountyName()
	{
		CountyName = WorldBuilder.GetGeneratedWorldName(txtSeedInput.Text, WorldSize);
		ValidateNewRwg();
		this.OnCountyNameChanged?.Invoke();
	}

	public void ValidateNewRwg()
	{
		string name = CountyName;
		bool flag = PathAbstractions.WorldsSearchPaths.GetLocation(name).Type != PathAbstractions.EAbstractedLocationType.None;
		ValidCountyName = !flag;
		validateDataLimit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validateDataLimit()
	{
		if (cbxSaveDataLimit != null)
		{
			bool flag = !((UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType)).UsesDataLimit() || cbxSaveDataLimit.Value != SaveDataLimitType.Unlimited;
			cbxSaveDataLimit.TextColor = (flag ? Color.white : Color.red);
		}
	}

	[XuiBindEvent("OnPress", "btnManage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnManage_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DataManagement.OpenDataManagementWindow(this, OnDataManagementWindowClosed);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDataManagementWindowClosed()
	{
		updateBarValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBarValues()
	{
		if (!HasDataManagementBar)
		{
			RefreshBindings();
			return;
		}
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
		dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
		pendingBytes = WorldBuilder?.SerializedSize ?? 0;
		totalAvailableBytes = instance.TotalAvailableBytes;
		dataManagementBar.SetPendingBytes(pendingBytes);
		RefreshBindings();
	}

	[XuiBindEvent("OnPress", "btnGenerate")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateButton_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (IsGenerating)
		{
			if (WorldBuilder != null)
			{
				WorldBuilder.IsCanceled = true;
			}
		}
		else
		{
			previewWindow?.TerrainCleanup();
			prefabPreviewManagerInstance?.ClearOldPreview();
			ThreadManager.StartCoroutine(GenerateCo());
		}
	}

	public IEnumerator GenerateCo(Action<string> _onSuccess = null, Action _onFailure = null)
	{
		IsGenerating = true;
		IsCancelling = false;
		updateBarValues();
		WorldSize = cbxWorldSize.Value;
		destroyBuilder();
		UserDataStorageType storageLocation = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.UserWorldStorageType);
		WorldBuilder = new WorldBuilder(txtSeedInput.Text, WorldSize, storageLocation)
		{
			PreviewWindow = previewWindow
		};
		XUiC_WorldGenerationPreview.WorldBuilder = WorldBuilder;
		WorldBuilder.Towns = Towns;
		WorldBuilder.Wilderness = Wilderness;
		WorldBuilder.Rivers = Rivers;
		WorldBuilder.Craters = Craters;
		WorldBuilder.Canyons = Canyons;
		WorldBuilder.Lakes = Lakes;
		WorldBuilder.Plains = PlainsWeight;
		WorldBuilder.Hills = HillsWeight;
		WorldBuilder.Mountains = MountainsWeight;
		WorldBuilder.biomeLayout = BiomeLayout;
		for (int i = 0; i < 5; i++)
		{
			XUiC_RwgBiome xUiC_RwgBiome = ((i < biomeBoxes.Length) ? biomeBoxes[i] : null);
			if (xUiC_RwgBiome != null)
			{
				WorldBuilder.SetBiomeWeight((BiomeType)i, xUiC_RwgBiome.Value);
			}
		}
		if (prefabPreviewManagerInstance != null)
		{
			prefabPreviewManagerInstance.ReadyToDisplay = false;
		}
		updateBarValues();
		yield return GCUtils.WaitForIdle();
		yield return WorldBuilder.GenerateFromUI();
		if (previewWindow != null)
		{
			yield return WorldBuilder.FinishForPreview();
			if (!WorldBuilder.IsCanceled)
			{
				yield return previewWindow.ShowPreview(XUiC_WorldGenerationPreview.PreviewStep.Done);
				yield return new WaitForSeconds(2f);
			}
		}
		else
		{
			XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
			bool success = false;
			yield return WorldBuilder.SaveData(WorldBuilder.SaveDataPromptMode.OnInsufficientSpace, GetParentWindow()?.Controller ?? this, null, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				success = false;
			}, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				success = true;
			});
			if (success)
			{
				_onSuccess?.Invoke(WorldBuilder.WorldName);
			}
			else
			{
				_onFailure?.Invoke();
			}
			destroyBuilder();
		}
		XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
		if (prefabPreviewManagerInstance != null)
		{
			prefabPreviewManagerInstance.ReadyToDisplay = true;
		}
		IsCancelling = false;
		IsGenerating = false;
		StatusText = "";
		updateBarValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void destroyBuilder()
	{
		if (WorldBuilder != null)
		{
			WorldBuilder.Cleanup();
			WorldBuilder = null;
		}
	}

	public void StartClose()
	{
		if (!IsGenerating)
		{
			if (WorldBuilder == null || !WorldBuilder.IsFinished || !WorldBuilder.CanSaveData())
			{
				close();
			}
			else
			{
				xui.StartCoroutine(startCloseCo());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startCloseCo()
	{
		if (!isClosing)
		{
			isClosing = true;
			bool shouldClose = false;
			yield return WorldBuilder.SaveData(WorldBuilder.SaveDataPromptMode.On, GetParentWindow()?.Controller ?? this, [PublicizedFrom(EAccessModifier.Internal)] () =>
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
				updateBarValues();
				isClosing = false;
			}
			else
			{
				close();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void close()
	{
		xui.playerUI.windowManager.Close(windowGroup);
		xui.playerUI.windowManager.Open(lastWindowID, _bModal: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		destroyBuilder();
		PathAbstractions.CacheEnabled = false;
		CleanupPreviewManager();
		RenderSettings.fogDensity = oldFogDensity;
		SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Remove(SaveDataLimitUIHelper.OnValueChanged, new Action(SaveDataLimit_OnChanged));
	}

	public void CleanupPreviewManager()
	{
		if (prefabPreviewManagerInstance != null)
		{
			prefabPreviewManagerInstance.Cleanup();
			prefabPreviewManagerInstance = null;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
		if (IsAdvancedUi)
		{
			if (progressView != null && XUiUtils.HotkeysAllowedFor(progressView) && PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel.WasReleased)
			{
				CancelGeneration();
			}
			if (prefabPreviewManagerInstance != null && prefabPreviewManagerInstance.ReadyToDisplay)
			{
				prefabPreviewManagerInstance.Update();
			}
		}
	}

	public static void Open(XUi _xui, string _previousWindowId)
	{
		XUiC_WorldGenerationWindow childByType = _xui.GetChildByType<XUiC_WorldGenerationWindowGroup>().GetChildByType<XUiC_WorldGenerationWindow>();
		childByType.lastWindowID = _previousWindowId;
		_xui.playerUI.windowManager.Open(childByType.windowGroup, _bModal: true);
	}

	public static bool IsWindowOpen(XUi _xui)
	{
		XUiC_WorldGenerationWindow childByType = _xui.GetChildByType<XUiC_WorldGenerationWindowGroup>().GetChildByType<XUiC_WorldGenerationWindow>();
		return _xui.playerUI.windowManager.IsWindowOpen(childByType.windowGroup);
	}
}

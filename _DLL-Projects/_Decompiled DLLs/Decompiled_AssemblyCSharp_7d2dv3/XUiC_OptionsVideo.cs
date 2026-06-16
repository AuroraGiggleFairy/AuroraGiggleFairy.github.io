using System;
using Platform;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsVideo : XUiC_OptionsVideoBase
{
	public readonly struct ResolutionInfo : IComparable<ResolutionInfo>, IEquatable<ResolutionInfo>
	{
		public enum EAspectRatio
		{
			Aspect_32_9,
			Aspect_32_10,
			Aspect_24_10,
			Aspect_21_9,
			Aspect_17_9,
			Aspect_16_9,
			Aspect_5_3,
			Aspect_16_10,
			Aspect_25_16,
			Aspect_3_2,
			Aspect_4_3,
			Aspect_5_4,
			Aspect_1_1,
			Unknown
		}

		public readonly int Width;

		public readonly int Height;

		public readonly EAspectRatio AspectRatio;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string label;

		public static (EAspectRatio _aspectRatio, float _aspectRatioFactor, string _aspectRatioString) DimensionsToAspectRatio(int _width, int _height)
		{
			if (_height == 0)
			{
				return (_aspectRatio: EAspectRatio.Unknown, _aspectRatioFactor: 0f, _aspectRatioString: "n/a");
			}
			switch (1000 * _width / _height)
			{
			case 3550:
			case 3551:
			case 3552:
			case 3553:
			case 3554:
			case 3555:
			case 3556:
			case 3557:
			case 3558:
			case 3559:
			case 3560:
			case 3561:
			case 3562:
			case 3563:
			case 3564:
			case 3565:
			case 3566:
			case 3567:
			case 3568:
			case 3569:
			case 3570:
			case 3571:
			case 3572:
			case 3573:
			case 3574:
			case 3575:
			case 3576:
			case 3577:
			case 3578:
			case 3579:
			case 3580:
			case 3581:
			case 3582:
			case 3583:
			case 3584:
			case 3585:
			case 3586:
			case 3587:
			case 3588:
			case 3589:
			case 3590:
			case 3591:
			case 3592:
			case 3593:
			case 3594:
			case 3595:
			case 3596:
			case 3597:
			case 3598:
			case 3599:
			case 3600:
				return (_aspectRatio: EAspectRatio.Aspect_32_9, _aspectRatioFactor: 3.555f, _aspectRatioString: "32:9");
			case 3200:
				return (_aspectRatio: EAspectRatio.Aspect_32_10, _aspectRatioFactor: 3.2f, _aspectRatioString: "32:10");
			case 2400:
				return (_aspectRatio: EAspectRatio.Aspect_24_10, _aspectRatioFactor: 2.4f, _aspectRatioString: "24:10");
			case 2333:
			case 2370:
				return (_aspectRatio: EAspectRatio.Aspect_21_9, _aspectRatioFactor: 2.37f, _aspectRatioString: "21:9");
			case 1890:
			case 1891:
			case 1892:
			case 1893:
			case 1894:
			case 1895:
			case 1896:
			case 1897:
			case 1898:
			case 1899:
			case 1900:
				return (_aspectRatio: EAspectRatio.Aspect_17_9, _aspectRatioFactor: 2.37f, _aspectRatioString: "17:9");
			case 1770:
			case 1771:
			case 1772:
			case 1773:
			case 1774:
			case 1775:
			case 1776:
			case 1777:
			case 1778:
			case 1779:
			case 1780:
				return (_aspectRatio: EAspectRatio.Aspect_16_9, _aspectRatioFactor: 1.777f, _aspectRatioString: "16:9");
			case 1660:
			case 1661:
			case 1662:
			case 1663:
			case 1664:
			case 1665:
			case 1666:
			case 1667:
			case 1668:
			case 1669:
			case 1670:
				return (_aspectRatio: EAspectRatio.Aspect_5_3, _aspectRatioFactor: 1.666f, _aspectRatioString: "5:3");
			case 1600:
				return (_aspectRatio: EAspectRatio.Aspect_16_10, _aspectRatioFactor: 1.6f, _aspectRatioString: "16:10");
			case 1560:
			case 1561:
			case 1562:
			case 1563:
			case 1564:
			case 1565:
			case 1566:
			case 1567:
			case 1568:
			case 1569:
			case 1570:
				return (_aspectRatio: EAspectRatio.Aspect_25_16, _aspectRatioFactor: 1.562f, _aspectRatioString: "25:16");
			case 1500:
				return (_aspectRatio: EAspectRatio.Aspect_3_2, _aspectRatioFactor: 1.5f, _aspectRatioString: "3:2");
			case 1330:
			case 1331:
			case 1332:
			case 1333:
			case 1334:
			case 1335:
			case 1336:
			case 1337:
			case 1338:
			case 1339:
			case 1340:
				return (_aspectRatio: EAspectRatio.Aspect_4_3, _aspectRatioFactor: 1.333f, _aspectRatioString: "4:3");
			case 1250:
				return (_aspectRatio: EAspectRatio.Aspect_5_4, _aspectRatioFactor: 1.25f, _aspectRatioString: "5:4");
			case 1000:
				return (_aspectRatio: EAspectRatio.Aspect_1_1, _aspectRatioFactor: 1f, _aspectRatioString: "1:1");
			default:
			{
				float num = (float)_width / (float)_height;
				return (_aspectRatio: EAspectRatio.Unknown, _aspectRatioFactor: num, _aspectRatioString: num.ToCultureInvariantString("0.##") + ":1");
			}
			}
		}

		public ResolutionInfo(int _width, int _height)
		{
			Width = _width;
			if (_height > _width)
			{
				_height = _width;
			}
			Height = _height;
			(EAspectRatio, float, string) tuple = DimensionsToAspectRatio(_width, _height);
			AspectRatio = tuple.Item1;
			string item = tuple.Item3;
			label = _width + "x" + _height + " (" + item + ")";
		}

		public int CompareTo(ResolutionInfo _other)
		{
			int width = Width;
			int num = width.CompareTo(_other.Width);
			if (num == 0)
			{
				width = Height;
				return width.CompareTo(_other.Height);
			}
			return num;
		}

		public bool Equals(ResolutionInfo _other)
		{
			if (Width == _other.Width)
			{
				return Height == _other.Height;
			}
			return false;
		}

		public override string ToString()
		{
			return label;
		}
	}

	public enum UpscalerMode
	{
		Off,
		Dynamic,
		Scale,
		FSR3,
		DLSS
	}

	public enum GraphicsMode
	{
		Lowest,
		Low,
		Medium,
		High,
		Ultra,
		Custom,
		ConsolePerformance,
		ConsoleQuality
	}

	[XuiBindComponent("OptionResolution", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionResolution;

	[XuiBindComponent("Resolution", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<ResolutionInfo> comboResolution;

	[XuiBindComponent("OptionFullscreen", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionFullscreen;

	[XuiBindComponent("Fullscreen", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboFullscreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public ResolutionInfo lastResolution;

	[XuiBindComponent("OptionUpscaler", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionUpscaler;

	[XuiBindComponent("Upscaler", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboUpscaler;

	[XuiBindComponent("OptionQualityPreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefIntIndex optionQualityPreset;

	[XuiBindComponent("QualityPreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboQualityPreset;

	[XuiBindComponent("OptionViewDistance", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefIntIndex optionViewDistance;

	[XuiBindComponent("TextureQuality", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboTextureQuality;

	[XuiBindComponent("AntiAliasing", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboAntiAliasing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool changingFromCode;

	public static string ID = "";

	[XuiBindComponent("OptionDynamicMeshDistance", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefIntIndex optionDynamicMeshDistance;

	[XuiBindComponent("DynamicMeshDistance", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<int> comboDynamicMeshDistance;

	public static bool Fsr3Supported
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return FSR3.FSR3Supported();
		}
	}

	public static bool DlssSupported
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (Fsr3Supported)
			{
				return DLSS.DLSSSupported();
			}
			return false;
		}
	}

	[XuiXmlBinding("upscaler_mode")]
	public UpscalerMode SelectedUpscalerMode => (UpscalerMode)(comboUpscaler?.SelectedIndex ?? 0);

	public override bool SupportsDefaults
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return TabSelector?.SelectedTabKey != "xuiOptionsVideoQuality";
		}
	}

	[XuiXmlBinding("texture_quality_min")]
	public int TextureQualityMin
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameOptionsManager.CalcTextureQualityMin();
		}
	}

	[XuiXmlBinding("texture_quality_limited")]
	public bool TextureQualityLimited
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<string> xUiC_ComboBoxList = comboTextureQuality;
			if (xUiC_ComboBoxList == null)
			{
				return false;
			}
			return xUiC_ComboBoxList.MinIndex > 0;
		}
	}

	[XuiXmlBinding("antialiasing_mode")]
	public int AntiAliasingMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return comboAntiAliasing?.SelectedIndex ?? 0;
		}
	}

	public static event Action OnSettingsChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public void initResolutionOptions()
	{
		if (optionResolution == null || comboResolution == null || optionFullscreen == null || comboFullscreen == null)
		{
			return;
		}
		optionFullscreen.GetSettingValue = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			FullScreenMode item = PlatformApplicationManager.Application.ScreenOptions.fullScreenMode;
			comboFullscreen.SelectedIndex = ConvertFullScreenModeToIndex(item);
		};
		optionFullscreen.DiscardChanges = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			FullScreenMode item = PlatformApplicationManager.Application.ScreenOptions.fullScreenMode;
			comboFullscreen.SelectedIndex = ConvertFullScreenModeToIndex(item);
		};
		optionFullscreen.ApplyChanges = [PublicizedFrom(EAccessModifier.Internal)] (bool _) =>
		{
		};
		optionFullscreen.IsChangedDelegate = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			FullScreenMode item = PlatformApplicationManager.Application.ScreenOptions.fullScreenMode;
			return comboFullscreen.SelectedIndex != ConvertFullScreenModeToIndex(item);
		};
		optionResolution.GetSettingValue = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			comboResolution.Elements.Clear();
			Resolution[] supportedResolutions = PlatformApplicationManager.Application.SupportedResolutions;
			for (int i = 0; i < supportedResolutions.Length; i++)
			{
				Resolution resolution = supportedResolutions[i];
				ResolutionInfo item = new ResolutionInfo(resolution.width, resolution.height);
				if (!comboResolution.Elements.Contains(item))
				{
					comboResolution.Elements.Add(item);
				}
			}
			GetCurrentResolution();
		};
		optionResolution.DiscardChanges = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			comboResolution.SelectedIndex = comboResolution.Elements.IndexOf(lastResolution);
		};
		optionResolution.ApplyChanges = [PublicizedFrom(EAccessModifier.Private)] (bool _) =>
		{
			ResolutionInfo value = comboResolution.Value;
			GameOptionsManager.SetResolution(value.Width, value.Height, ConvertIndexToFullScreenMode(comboFullscreen.SelectedIndex));
			GetCurrentResolution();
		};
		optionResolution.IsChangedDelegate = [PublicizedFrom(EAccessModifier.Private)] () => !comboResolution.Value.Equals(lastResolution);
		[PublicizedFrom(EAccessModifier.Internal)]
		static int ConvertFullScreenModeToIndex(FullScreenMode _mode)
		{
			return _mode switch
			{
				FullScreenMode.FullScreenWindow => 1, 
				FullScreenMode.ExclusiveFullScreen => 2, 
				_ => 0, 
			};
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static FullScreenMode ConvertIndexToFullScreenMode(int _index)
		{
			return _index switch
			{
				1 => FullScreenMode.FullScreenWindow, 
				2 => FullScreenMode.ExclusiveFullScreen, 
				_ => FullScreenMode.Windowed, 
			};
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void GetCurrentResolution()
		{
			(int width, int height, FullScreenMode fullScreenMode) screenOptions = PlatformApplicationManager.Application.ScreenOptions;
			int item = screenOptions.width;
			int item2 = screenOptions.height;
			lastResolution = new ResolutionInfo(item, item2);
			if (!comboResolution.Elements.Contains(lastResolution))
			{
				comboResolution.Elements.Add(lastResolution);
			}
			comboResolution.Elements.Sort();
			comboResolution.SelectedIndex = comboResolution.Elements.IndexOf(lastResolution);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initUpscalerModeOptions()
	{
		if (optionUpscaler == null || comboUpscaler == null)
		{
			return;
		}
		optionUpscaler.GetSettingValue = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (!DlssSupported)
			{
				comboUpscaler.MaxIndex = 3;
			}
			if (!Fsr3Supported)
			{
				comboUpscaler.MaxIndex = 2;
			}
			int num = GamePrefUpscalerModeIndex();
			if (num == -1)
			{
				if (Fsr3Supported)
				{
					Log.Out($"Upscaler mode \"{GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode)}\" is unsupported on this platform; defaulting to \"{2}\".");
					num = 3;
					GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, 2);
				}
				else
				{
					Log.Out($"Upscaler mode \"{GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode)}\" is unsupported on this platform; defaulting to \"{4}\".");
					num = 2;
					GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, 4);
				}
			}
			comboUpscaler.SelectedIndex = num;
		};
		optionUpscaler.DiscardChanges = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			comboUpscaler.SelectedIndex = GamePrefUpscalerModeIndex();
		};
		optionUpscaler.ApplyChanges = [PublicizedFrom(EAccessModifier.Private)] (bool _) =>
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, SelectionToUpscalerMode());
		};
		optionUpscaler.IsChangedDelegate = [PublicizedFrom(EAccessModifier.Private)] () => comboUpscaler.SelectedIndex != GamePrefUpscalerModeIndex();
		[PublicizedFrom(EAccessModifier.Internal)]
		static int GamePrefUpscalerModeIndex()
		{
			return GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode) switch
			{
				0 => 0, 
				3 => 1, 
				4 => 2, 
				2 => Fsr3Supported ? 3 : (-1), 
				5 => DlssSupported ? 4 : (-1), 
				_ => -1, 
			};
		}
		[PublicizedFrom(EAccessModifier.Private)]
		int SelectionToUpscalerMode()
		{
			return comboUpscaler.SelectedIndex switch
			{
				0 => 0, 
				1 => 3, 
				2 => 4, 
				3 => Fsr3Supported ? 2 : GameOptionsPlatforms.DefaultUpscalerMode, 
				4 => (Fsr3Supported && DlssSupported) ? 5 : GameOptionsPlatforms.DefaultUpscalerMode, 
				_ => GameOptionsPlatforms.DefaultUpscalerMode, 
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initQualityPresetBasedOptions()
	{
		optionQualityPreset.MapGamePrefToListIndex = [PublicizedFrom(EAccessModifier.Internal)] (int _prefValue) => (GameOptionsPlatforms.GfxPreset)_prefValue switch
		{
			GameOptionsPlatforms.GfxPreset.Lowest => 0, 
			GameOptionsPlatforms.GfxPreset.Low => 1, 
			GameOptionsPlatforms.GfxPreset.Medium => 2, 
			GameOptionsPlatforms.GfxPreset.High => 3, 
			GameOptionsPlatforms.GfxPreset.Ultra => 4, 
			GameOptionsPlatforms.GfxPreset.ConsolePerformance => 6, 
			GameOptionsPlatforms.GfxPreset.ConsoleQuality => 7, 
			_ => 5, 
		};
		optionQualityPreset.MapListIndexToGamePref = [PublicizedFrom(EAccessModifier.Internal)] (int _index) => (GraphicsMode)_index switch
		{
			GraphicsMode.Lowest => 0, 
			GraphicsMode.Low => 1, 
			GraphicsMode.Medium => 2, 
			GraphicsMode.High => 3, 
			GraphicsMode.Ultra => 4, 
			GraphicsMode.ConsolePerformance => 6, 
			GraphicsMode.ConsoleQuality => 8, 
			_ => 5, 
		};
		optionViewDistance.MapGamePrefToListIndex = [PublicizedFrom(EAccessModifier.Internal)] (int _prefValue) => _prefValue switch
		{
			5 => 0, 
			6 => 1, 
			_ => 2, 
		};
		optionViewDistance.MapListIndexToGamePref = [PublicizedFrom(EAccessModifier.Internal)] (int _index) => _index switch
		{
			0 => 5, 
			1 => 6, 
			_ => 7, 
		};
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			comboQualityPreset.Elements.Add("ConsolePerformance");
			comboQualityPreset.Elements.Add("ConsoleQuality");
		}
		XUiC_OptionEntryAbs[] allOptions = AllOptions;
		for (int num = 0; num < allOptions.Length; num++)
		{
			if (allOptions[num] is XUiC_OptionEntryGamePrefAbs { GamePref: not null } xUiC_OptionEntryGamePrefAbs && GameOptionsManager.QualityPresets.TryGetValue(xUiC_OptionEntryGamePrefAbs.GamePref.Value, out var _))
			{
				xUiC_OptionEntryGamePrefAbs.ValueChanged += OnQualityBasedOptionChanged;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnQualityBasedOptionChanged(XUiC_OptionEntryAbs _option)
	{
		if (!changingFromCode)
		{
			comboQualityPreset.SelectedIndex = 5;
		}
	}

	[XuiBindEvent("OnValueChanged", "comboQualityPreset")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnQualityPresetChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		GameOptionsPlatforms.GfxPreset gfxPreset = (GameOptionsPlatforms.GfxPreset)optionQualityPreset.SelectedValue;
		if (gfxPreset == GameOptionsPlatforms.GfxPreset.Custom)
		{
			return;
		}
		changingFromCode = true;
		int num = (int)gfxPreset;
		XUiC_OptionEntryAbs[] allOptions = AllOptions;
		for (int i = 0; i < allOptions.Length; i++)
		{
			if (allOptions[i] is XUiC_OptionEntryGamePrefAbs { GamePref: not null } xUiC_OptionEntryGamePrefAbs && GameOptionsManager.QualityPresets.TryGetValue(xUiC_OptionEntryGamePrefAbs.GamePref.Value, out var value) && num < value.Count && value[num] != null)
			{
				xUiC_OptionEntryGamePrefAbs.SelectedValue = value[num];
			}
		}
		changingFromCode = false;
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		initResolutionOptions();
		initUpscalerModeOptions();
		initQualityPresetBasedOptions();
		optionDynamicMeshDistance.MapGamePrefToListIndex = [PublicizedFrom(EAccessModifier.Private)] (int _prefValue) => comboDynamicMeshDistance.Elements.IndexOf(_prefValue);
		optionDynamicMeshDistance.MapListIndexToGamePref = [PublicizedFrom(EAccessModifier.Private)] (int _index) => comboDynamicMeshDistance.Elements[_index];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlurEnabled, GamePrefs.GetInt(EnumGamePrefs.OptionsGfxMotionBlur) > 0);
		GameOptionsManager.ApplyAllOptions(xui.playerUI);
		QualitySettings.vSyncCount = GamePrefs.GetInt(base.VSyncCountPref);
		ReflectionManager.ApplyOptions();
		XUiC_OptionsVideo.OnSettingsChanged?.Invoke();
	}
}

using System;
using Platform;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsVideo : XUiController
{
	public struct ResolutionInfo : IComparable<ResolutionInfo>, IEquatable<ResolutionInfo>
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
		public readonly string Label;

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
			Label = _width + "x" + _height + " (" + item + ")";
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
			return Label;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGamePrefs VSyncCountPref = EnumGamePrefs.OptionsGfxVsync;

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dlssSupported;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fsr3Supported;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<ResolutionInfo> comboResolution;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboFullscreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboUpscalerMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboFSRPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboDLSSPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt comboDynamicMinFPS;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboDynamicScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVSync;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboBrightness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaultBrightness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt comboFieldOfView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaultFOV;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt comboFieldOfView3P;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaultFOV3P;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> combo3PCameraMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboDefaultCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboCameraDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaultCameraDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboQualityPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAntiAliasing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboAntiAliasingSharp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboTextureQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboTextureFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboReflectionQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboReflectedShadows;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboWaterQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboViewDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboLODDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboShadowsDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboShadowsQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboTerrainQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboObjectQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboGrassDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboOcclusion;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDymeshEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<int> comboDymeshDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDymeshHighQualityMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<int> comboDymeshMaxRegions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<int> comboDymeshMaxMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDymeshLandClaimOnly;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<int> comboDymeshLandClaimBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboBloom;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDepthOfField;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboMotionBlur;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboSSAO;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboSSReflections;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboSunShafts;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboParticles;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUIBackgroundOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUIForegroundOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUiSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboScreenBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUiFpsScaling;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public object[] previousSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_ReflectionQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_ShadowDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_ShadowQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_TerrainQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_ObjectQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_GrassDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_MotionBlur;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_SSR;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origDymeshEnabled;

	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		tabs = GetChildByType<XUiC_TabSelector>();
		tabs.OnTabChanged += TabSelector_OnTabChanged;
		fsr3Supported = FSR3.FSR3Supported();
		dlssSupported = fsr3Supported && DLSS.DLSSSupported();
		comboResolution = GetChildById("Resolution").GetChildByType<XUiC_ComboBoxList<ResolutionInfo>>();
		comboFullscreen = GetChildById("Fullscreen").GetChildByType<XUiC_ComboBoxList<string>>();
		comboUpscalerMode = GetChildById("Upscaler").GetChildByType<XUiC_ComboBoxList<string>>();
		if (!dlssSupported)
		{
			comboUpscalerMode.MaxIndex = 3;
		}
		if (!fsr3Supported)
		{
			comboUpscalerMode.MaxIndex = 2;
		}
		comboFSRPreset = GetChildById("FSRPreset").GetChildByType<XUiC_ComboBoxList<string>>();
		comboDLSSPreset = GetChildById("DLSSPreset").GetChildByType<XUiC_ComboBoxList<string>>();
		comboDynamicMinFPS = GetChildById("DyMinFPS").GetChildByType<XUiC_ComboBoxInt>();
		comboDynamicScale = GetChildById("DyScale").GetChildByType<XUiC_ComboBoxFloat>();
		comboVSync = GetChildById("VSync").GetChildByType<XUiC_ComboBoxList<string>>();
		comboBrightness = GetChildById("Brightness").GetChildByType<XUiC_ComboBoxFloat>();
		btnDefaultBrightness = GetChildById("btnDefaultBrightness").GetChildByType<XUiC_SimpleButton>();
		comboFieldOfView = GetChildById("FieldOfView").GetChildByType<XUiC_ComboBoxInt>();
		btnDefaultFOV = GetChildById("btnDefaultFOV").GetChildByType<XUiC_SimpleButton>();
		comboFieldOfView3P = GetChildById("FieldOfView3P").GetChildByType<XUiC_ComboBoxInt>();
		btnDefaultFOV3P = GetChildById("btnDefaultFOV3P").GetChildByType<XUiC_SimpleButton>();
		combo3PCameraMode = GetChildById("3PCameraMode").GetChildByType<XUiC_ComboBoxList<string>>();
		comboDefaultCamera = GetChildById("defaultCamera").GetChildByType<XUiC_ComboBoxList<string>>();
		comboCameraDistance = GetChildById("CameraDistance3P").GetChildByType<XUiC_ComboBoxFloat>();
		btnDefaultCameraDistance = GetChildById("btnDefaultCameraDistance").GetChildByType<XUiC_SimpleButton>();
		comboQualityPreset = GetChildById("QualityPreset").GetChildByType<XUiC_ComboBoxList<string>>();
		comboAntiAliasing = GetChildById("AntiAliasing").GetChildByType<XUiC_ComboBoxList<string>>();
		comboAntiAliasingSharp = GetChildById("AntiAliasingSharp").GetChildByType<XUiC_ComboBoxFloat>();
		comboTextureQuality = GetChildById("TextureQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		comboTextureFilter = GetChildById("TextureFilter").GetChildByType<XUiC_ComboBoxList<string>>();
		comboReflectionQuality = GetChildById("ReflectionQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		comboReflectedShadows = GetChildById("ReflectedShadows").GetChildByType<XUiC_ComboBoxBool>();
		comboWaterQuality = GetChildById("WaterQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		comboViewDistance = GetChildById("ViewDistance").GetChildByType<XUiC_ComboBoxList<string>>();
		comboLODDistance = GetChildById("LODDistance").GetChildByType<XUiC_ComboBoxFloat>();
		comboShadowsDistance = GetChildById("ShadowsDistance").GetChildByType<XUiC_ComboBoxList<string>>();
		comboShadowsQuality = GetChildById("ShadowsQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		comboTerrainQuality = GetChildById("TerrainQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		comboObjectQuality = GetChildById("ObjectQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		comboGrassDistance = GetChildById("GrassDistance").GetChildByType<XUiC_ComboBoxList<string>>();
		comboOcclusion = GetChildById("Occlusion").GetChildByType<XUiC_ComboBoxBool>();
		comboDymeshEnabled = GetChildById("DynamicMeshEnabled").GetChildByType<XUiC_ComboBoxBool>();
		comboDymeshDistance = GetChildById("DynamicMeshDistance").GetChildByType<XUiC_ComboBoxList<int>>();
		comboDymeshHighQualityMesh = GetChildById("DynamicMeshHighQualityMesh").GetChildByType<XUiC_ComboBoxBool>();
		comboDymeshMaxRegions = GetChildById("DynamicMeshMaxRegionLoads").GetChildByType<XUiC_ComboBoxList<int>>();
		comboDymeshMaxMesh = GetChildById("DynamicMeshMaxMeshCache").GetChildByType<XUiC_ComboBoxList<int>>();
		comboDymeshLandClaimOnly = GetChildById("DynamicMeshLandClaimOnly").GetChildByType<XUiC_ComboBoxBool>();
		comboDymeshLandClaimBuffer = GetChildById("DynamicMeshLandClaimBuffer").GetChildByType<XUiC_ComboBoxList<int>>();
		comboBloom = GetChildById("Bloom").GetChildByType<XUiC_ComboBoxBool>();
		comboDepthOfField = GetChildById("DepthOfField")?.GetChildByType<XUiC_ComboBoxBool>();
		comboMotionBlur = GetChildById("MotionBlur").GetChildByType<XUiC_ComboBoxList<string>>();
		comboSSAO = GetChildById("SSAO").GetChildByType<XUiC_ComboBoxBool>();
		comboSSReflections = GetChildById("SSReflections").GetChildByType<XUiC_ComboBoxList<string>>();
		comboSunShafts = GetChildById("SunShafts").GetChildByType<XUiC_ComboBoxBool>();
		comboParticles = GetChildById("Particles").GetChildByType<XUiC_ComboBoxFloat>();
		comboUIBackgroundOpacity = GetChildById("UIBackgroundOpacity").GetChildByType<XUiC_ComboBoxFloat>();
		comboUIForegroundOpacity = GetChildById("UIForegroundOpacity").GetChildByType<XUiC_ComboBoxFloat>();
		comboUiSize = GetChildById("UiSize").GetChildByType<XUiC_ComboBoxFloat>();
		comboScreenBounds = GetChildById("ScreenBounds").GetChildByType<XUiC_ComboBoxFloat>();
		comboUiFpsScaling = GetChildById("UiFpsScaling").GetChildByType<XUiC_ComboBoxFloat>();
		comboQualityPreset.OnValueChanged += QualityPresetChanged;
		comboAntiAliasing.OnValueChangedGeneric += AnyPresetValueChanged;
		comboAntiAliasingSharp.OnValueChangedGeneric += AnyPresetValueChanged;
		comboTextureQuality.OnValueChangedGeneric += AnyPresetValueChanged;
		comboTextureFilter.OnValueChangedGeneric += AnyPresetValueChanged;
		comboReflectionQuality.OnValueChangedGeneric += AnyPresetValueChanged;
		comboReflectedShadows.OnValueChangedGeneric += AnyPresetValueChanged;
		comboWaterQuality.OnValueChangedGeneric += AnyPresetValueChanged;
		comboViewDistance.OnValueChangedGeneric += AnyPresetValueChanged;
		comboLODDistance.OnValueChangedGeneric += AnyPresetValueChanged;
		comboShadowsDistance.OnValueChangedGeneric += AnyPresetValueChanged;
		comboShadowsQuality.OnValueChangedGeneric += AnyPresetValueChanged;
		comboTerrainQuality.OnValueChangedGeneric += AnyPresetValueChanged;
		comboObjectQuality.OnValueChangedGeneric += AnyPresetValueChanged;
		comboGrassDistance.OnValueChangedGeneric += AnyPresetValueChanged;
		comboOcclusion.OnValueChangedGeneric += AnyPresetValueChanged;
		comboDymeshEnabled.OnValueChangedGeneric += AnyPresetValueChanged;
		comboDymeshDistance.OnValueChangedGeneric += AnyPresetValueChanged;
		comboDymeshHighQualityMesh.OnValueChangedGeneric += AnyPresetValueChanged;
		comboDymeshMaxRegions.OnValueChangedGeneric += AnyPresetValueChanged;
		comboDymeshMaxMesh.OnValueChangedGeneric += AnyPresetValueChanged;
		comboDymeshLandClaimOnly.OnValueChangedGeneric += AnyPresetValueChanged;
		comboDymeshLandClaimBuffer.OnValueChangedGeneric += AnyPresetValueChanged;
		comboBloom.OnValueChangedGeneric += AnyPresetValueChanged;
		if (comboDepthOfField != null)
		{
			comboDepthOfField.OnValueChangedGeneric += AnyPresetValueChanged;
		}
		comboMotionBlur.OnValueChangedGeneric += AnyPresetValueChanged;
		comboSSAO.OnValueChangedGeneric += AnyPresetValueChanged;
		comboSSReflections.OnValueChangedGeneric += AnyPresetValueChanged;
		comboSunShafts.OnValueChangedGeneric += AnyPresetValueChanged;
		comboParticles.OnValueChangedGeneric += AnyPresetValueChanged;
		comboResolution.OnValueChangedGeneric += anyOtherValueChanged;
		comboFullscreen.OnValueChangedGeneric += anyOtherValueChanged;
		comboUpscalerMode.OnValueChangedGeneric += upscalerModeChanged;
		comboFSRPreset.OnValueChangedGeneric += upscalerPresetChanged;
		comboDLSSPreset.OnValueChangedGeneric += upscalerPresetChanged;
		comboDynamicMinFPS.OnValueChangedGeneric += anyOtherValueChanged;
		comboDynamicScale.OnValueChangedGeneric += anyOtherValueChanged;
		comboVSync.OnValueChangedGeneric += anyOtherValueChanged;
		comboBrightness.OnValueChangedGeneric += anyOtherValueChanged;
		comboFieldOfView.OnValueChangedGeneric += anyOtherValueChanged;
		comboFieldOfView3P.OnValueChangedGeneric += anyOtherValueChanged;
		combo3PCameraMode.OnValueChangedGeneric += anyOtherValueChanged;
		comboDefaultCamera.OnValueChangedGeneric += anyOtherValueChanged;
		comboCameraDistance.OnValueChangedGeneric += anyOtherValueChanged;
		comboUIBackgroundOpacity.OnValueChangedGeneric += anyOtherValueChanged;
		comboUIForegroundOpacity.OnValueChangedGeneric += anyOtherValueChanged;
		comboUiSize.OnValueChangedGeneric += anyOtherValueChanged;
		comboScreenBounds.OnValueChangedGeneric += anyOtherValueChanged;
		comboUiFpsScaling.OnValueChangedGeneric += anyOtherValueChanged;
		comboDynamicMinFPS.Min = 10L;
		comboDynamicMinFPS.Max = 60L;
		comboDynamicScale.Min = 0.20000000298023224;
		comboDynamicScale.Max = 1.0;
		comboUIBackgroundOpacity.Min = Constants.cMinGlobalBackgroundOpacity;
		comboUIBackgroundOpacity.Max = 1.0;
		comboUIForegroundOpacity.Min = Constants.cMinGlobalForegroundOpacity;
		comboUIForegroundOpacity.Max = 1.0;
		comboUiSize.Min = 0.7;
		comboUiSize.Max = 1.0;
		comboScreenBounds.Min = 0.8;
		comboScreenBounds.Max = 1.0;
		comboBrightness.Min = 0.0;
		comboBrightness.Max = 1.0;
		btnDefaultBrightness.OnPressed += BtnDefaultBrightness_OnPressed;
		comboFieldOfView.Min = Constants.cMinCameraFieldOfView;
		comboFieldOfView.Max = Constants.cMaxCameraFieldOfView;
		btnDefaultFOV.OnPressed += BtnDefaultFOV_OnPressed;
		comboFieldOfView3P.Min = Constants.cMinCameraFieldOfView;
		comboFieldOfView3P.Max = Constants.cMaxCameraFieldOfView;
		btnDefaultFOV3P.OnPressed += BtnDefaultFOV3P_OnPressed;
		comboCameraDistance.Min = 0.0;
		comboCameraDistance.Max = 1.0;
		btnDefaultCameraDistance.OnPressed += BtnDefaultCameraDistance_OnPressed;
		comboAntiAliasingSharp.Min = 0.0;
		comboAntiAliasingSharp.Max = 1.0;
		comboParticles.Min = 0.0;
		comboParticles.Max = 1.0;
		comboLODDistance.Min = 0.0;
		comboLODDistance.Max = 1.0;
		origLength_ReflectionQuality = comboReflectionQuality.Elements.Count;
		comboReflectionQuality.MaxIndex = origLength_ReflectionQuality - 1;
		comboReflectionQuality.Elements.Add("Custom");
		origLength_ShadowDistance = comboShadowsDistance.Elements.Count;
		comboShadowsDistance.MaxIndex = origLength_ShadowDistance - 1;
		comboShadowsDistance.Elements.Add("Custom");
		origLength_ShadowQuality = comboShadowsQuality.Elements.Count;
		comboShadowsQuality.MaxIndex = origLength_ShadowQuality - 1;
		comboShadowsQuality.Elements.Add("Custom");
		origLength_TerrainQuality = comboTerrainQuality.Elements.Count;
		comboTerrainQuality.MaxIndex = origLength_TerrainQuality - 1;
		comboTerrainQuality.Elements.Add("Custom");
		origLength_ObjectQuality = comboObjectQuality.Elements.Count;
		comboObjectQuality.MaxIndex = origLength_ObjectQuality - 1;
		comboObjectQuality.Elements.Add("Custom");
		origLength_GrassDistance = comboGrassDistance.Elements.Count;
		comboGrassDistance.MaxIndex = origLength_GrassDistance - 1;
		comboGrassDistance.Elements.Add("Custom");
		origLength_MotionBlur = comboMotionBlur.Elements.Count;
		comboMotionBlur.MaxIndex = origLength_MotionBlur - 1;
		comboMotionBlur.Elements.Add("Custom");
		origLength_SSR = comboSSReflections.Elements.Count;
		comboSSReflections.MaxIndex = origLength_SSR - 1;
		comboSSReflections.Elements.Add("Custom");
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			comboQualityPreset.Elements.Add("ConsolePerformance");
			comboQualityPreset.Elements.Add("ConsoleQuality");
		}
		btnBack = GetChildById("btnBack") as XUiC_SimpleButton;
		btnDefaults = GetChildById("btnDefaults") as XUiC_SimpleButton;
		btnApply = GetChildById("btnApply") as XUiC_SimpleButton;
		btnBack.OnPressed += BtnBack_OnPressed;
		btnDefaults.OnPressed += BtnDefaults_OnOnPressed;
		btnApply.OnPressed += BtnApply_OnPressed;
		RefreshApplyLabel();
		RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshApplyLabel()
	{
		InControlExtensions.SetApplyButtonString(btnApply, "xuiApply");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		RefreshApplyLabel();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TabSelector_OnTabChanged(int _i, XUiC_TabSelectorTab _tab)
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void upscalerModeChanged(XUiController _sender)
	{
		updateDynamicOptions();
		btnApply.Enabled = true;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void upscalerPresetChanged(XUiController _sender)
	{
		if (_sender as XUiC_ComboBoxList<string> == comboFSRPreset)
		{
			comboDLSSPreset.SelectedIndex = comboFSRPreset.SelectedIndex;
		}
		else
		{
			comboFSRPreset.SelectedIndex = comboDLSSPreset.SelectedIndex;
		}
		updateDynamicOptions();
		btnApply.Enabled = true;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void anyOtherValueChanged(XUiController _sender)
	{
		updateDynamicOptions();
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaultBrightness_OnPressed(XUiController _sender, int _mouseButton)
	{
		comboBrightness.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxBrightness);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaultFOV_OnPressed(XUiController _sender, int _mouseButton)
	{
		comboFieldOfView.Value = (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxFOV);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaultFOV3P_OnPressed(XUiController _sender, int _mouseButton)
	{
		comboFieldOfView3P.Value = (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxFOV3P);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaultCameraDistance_OnPressed(XUiController _sender, int _mouseButton)
	{
		comboCameraDistance.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxCameraDistance3P);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		applyChanges();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		comboUIBackgroundOpacity.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
		comboUIForegroundOpacity.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		comboUiSize.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsHudSize);
		comboScreenBounds.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsScreenBoundsValue);
		comboUiFpsScaling.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsUiFpsScaling);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GraphicsMode QualityPresetToGraphicsMode(int qualityPreset)
	{
		return qualityPreset switch
		{
			0 => GraphicsMode.Lowest, 
			1 => GraphicsMode.Low, 
			2 => GraphicsMode.Medium, 
			3 => GraphicsMode.High, 
			4 => GraphicsMode.Ultra, 
			6 => GraphicsMode.ConsolePerformance, 
			8 => GraphicsMode.ConsoleQuality, 
			_ => GraphicsMode.Custom, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GraphicsModeToQualityPreset(GraphicsMode graphicsMode)
	{
		return graphicsMode switch
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QualityPresetChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		int num = GraphicsModeToQualityPreset((GraphicsMode)comboQualityPreset.SelectedIndex);
		if (num != 5)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, num);
			GameOptionsManager.SetGraphicsQuality();
			updateGraphicOptions();
			updateDynamicOptions();
		}
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AnyPresetValueChanged(XUiController _sender)
	{
		comboQualityPreset.SelectedIndex = 5;
		GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, 5);
		btnApply.Enabled = true;
		updateGraphicsAAOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDynamicOptions()
	{
		comboAntiAliasing.Enabled = comboUpscalerMode.SelectedIndex != 3 && comboUpscalerMode.SelectedIndex != 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGraphicsAAOptions()
	{
		comboAntiAliasingSharp.Enabled = comboAntiAliasing.SelectedIndex >= 4 || comboUpscalerMode.SelectedIndex == 3 || comboUpscalerMode.SelectedIndex == 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGraphicOptions()
	{
		Resolution[] supportedResolutions = PlatformApplicationManager.Application.SupportedResolutions;
		comboResolution.Elements.Clear();
		Resolution[] array = supportedResolutions;
		for (int i = 0; i < array.Length; i++)
		{
			Resolution resolution = array[i];
			ResolutionInfo item = new ResolutionInfo(resolution.width, resolution.height);
			if (!comboResolution.Elements.Contains(item))
			{
				comboResolution.Elements.Add(item);
			}
		}
		comboResolution.Elements.Sort();
		(int width, int height, FullScreenMode fullScreenMode) screenOptions = PlatformApplicationManager.Application.ScreenOptions;
		int item2 = screenOptions.width;
		int item3 = screenOptions.height;
		ResolutionInfo item4 = new ResolutionInfo(item2, item3);
		if (!comboResolution.Elements.Contains(item4))
		{
			comboResolution.Elements.Add(item4);
			comboResolution.Elements.Sort();
		}
		comboResolution.SelectedIndex = comboResolution.Elements.IndexOf(item4);
		FullScreenMode mode = (FullScreenMode)SdPlayerPrefs.GetInt("Screenmanager Fullscreen mode", 3);
		comboFullscreen.SelectedIndex = ConvertFullScreenModeToIndex(mode);
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode) switch
		{
			0 => 0, 
			3 => 1, 
			4 => 2, 
			2 => fsr3Supported ? 3 : (-1), 
			5 => dlssSupported ? 4 : (-1), 
			_ => -1, 
		};
		if (num == -1)
		{
			if (fsr3Supported)
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
		comboUpscalerMode.SelectedIndex = num;
		comboFSRPreset.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFSRPreset);
		comboDLSSPreset.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFSRPreset);
		comboDynamicMinFPS.Value = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxDynamicMinFPS);
		comboDynamicScale.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxDynamicScale);
		comboVSync.SelectedIndex = GamePrefs.GetInt(VSyncCountPref);
		comboBrightness.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness);
		comboFieldOfView.Value = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
		comboFieldOfView3P.Value = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV3P);
		combo3PCameraMode.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfx3PCameraMode);
		comboDefaultCamera.SelectedIndex = ((!GamePrefs.GetBool(EnumGamePrefs.OptionsGfxDefaultFirstPersonCamera)) ? 1 : 0);
		comboCameraDistance.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxCameraDistance3P);
		comboDymeshEnabled.Value = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		comboDymeshDistance.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshDistance);
		comboDymeshHighQualityMesh.Value = !GamePrefs.GetBool(EnumGamePrefs.DynamicMeshUseImposters);
		comboDymeshMaxRegions.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshMaxRegionCache);
		comboDymeshMaxMesh.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshMaxItemCache);
		comboDymeshLandClaimOnly.Value = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshLandClaimOnly);
		comboDymeshLandClaimBuffer.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshLandClaimBuffer);
		comboQualityPreset.SelectedIndex = (int)QualityPresetToGraphicsMode(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset));
		comboAntiAliasing.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxAA);
		comboAntiAliasingSharp.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxAASharpness);
		updateGraphicsAAOptions();
		comboTextureQuality.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexQuality);
		comboTextureFilter.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexFilter);
		int num2 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxReflectQuality);
		if (num2 < origLength_ReflectionQuality)
		{
			comboReflectionQuality.SelectedIndex = num2;
		}
		else
		{
			comboReflectionQuality.MaxIndex = origLength_ReflectionQuality;
			comboReflectionQuality.SelectedIndex = comboReflectionQuality.Elements.Count - 1;
		}
		comboReflectedShadows.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxReflectShadows);
		comboWaterQuality.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxWaterQuality);
		comboViewDistance.SelectedIndex = ((GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance) != 5) ? ((GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance) == 6) ? 1 : 2) : 0);
		comboLODDistance.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxLODDistance);
		int num3 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		if (num3 >= origLength_ShadowDistance && num3 < 20)
		{
			num3 = 20;
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 20);
		}
		if (num3 < origLength_ShadowDistance)
		{
			comboShadowsDistance.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		}
		else
		{
			comboShadowsDistance.MaxIndex = origLength_ShadowDistance;
			comboShadowsDistance.SelectedIndex = comboShadowsDistance.Elements.Count - 1;
		}
		int num4 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowQuality);
		if (num4 < origLength_ShadowQuality)
		{
			comboShadowsQuality.SelectedIndex = num4;
		}
		else
		{
			comboShadowsQuality.MaxIndex = origLength_ShadowQuality;
			comboShadowsQuality.SelectedIndex = comboShadowsQuality.Elements.Count - 1;
		}
		int num5 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTerrainQuality);
		if (num5 < origLength_TerrainQuality)
		{
			comboTerrainQuality.SelectedIndex = num5;
		}
		else
		{
			comboTerrainQuality.MaxIndex = origLength_TerrainQuality;
			comboTerrainQuality.SelectedIndex = comboTerrainQuality.Elements.Count - 1;
		}
		int num6 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxObjQuality);
		if (num6 < origLength_ObjectQuality)
		{
			comboObjectQuality.SelectedIndex = num6;
		}
		else
		{
			comboObjectQuality.MaxIndex = origLength_ObjectQuality;
			comboObjectQuality.SelectedIndex = comboObjectQuality.Elements.Count - 1;
		}
		int num7 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxGrassDistance);
		if (num7 < origLength_GrassDistance)
		{
			comboGrassDistance.SelectedIndex = num7;
		}
		else
		{
			comboGrassDistance.MaxIndex = origLength_GrassDistance;
			comboGrassDistance.SelectedIndex = comboGrassDistance.Elements.Count - 1;
		}
		comboOcclusion.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxOcclusion);
		comboBloom.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxBloom);
		if (comboDepthOfField != null)
		{
			comboDepthOfField.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxDOF);
		}
		int num8 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxMotionBlur);
		if (num8 < origLength_MotionBlur)
		{
			comboMotionBlur.SelectedIndex = num8;
		}
		else
		{
			comboMotionBlur.MaxIndex = origLength_MotionBlur;
			comboMotionBlur.SelectedIndex = comboMotionBlur.Elements.Count - 1;
		}
		comboSSAO.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSSAO);
		int num9 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSSReflections);
		if (num9 < origLength_SSR)
		{
			comboSSReflections.SelectedIndex = num9;
		}
		else
		{
			comboSSReflections.MaxIndex = origLength_SSR;
			comboSSReflections.SelectedIndex = comboSSReflections.Elements.Count - 1;
		}
		comboSunShafts.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSunShafts);
		comboParticles.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		comboUIBackgroundOpacity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
		comboUIForegroundOpacity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		comboUiSize.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize);
		comboScreenBounds.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
		comboUiFpsScaling.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsUiFpsScaling);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		ResolutionInfo value = comboResolution.Value;
		GameOptionsManager.SetResolution(value.Width, value.Height, ConvertIndexToFullScreenMode(comboFullscreen.SelectedIndex));
		GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, comboUpscalerMode.SelectedIndex switch
		{
			0 => 0, 
			1 => 3, 
			2 => 4, 
			3 => fsr3Supported ? 2 : GameOptionsPlatforms.DefaultUpscalerMode, 
			4 => (fsr3Supported && dlssSupported) ? 5 : GameOptionsPlatforms.DefaultUpscalerMode, 
			_ => GameOptionsPlatforms.DefaultUpscalerMode, 
		});
		GamePrefs.Set(EnumGamePrefs.OptionsGfxFSRPreset, comboFSRPreset.ViewComponent.IsVisible ? comboFSRPreset.SelectedIndex : comboDLSSPreset.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicMinFPS, (int)comboDynamicMinFPS.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicScale, (float)comboDynamicScale.Value);
		GamePrefs.Set(VSyncCountPref, comboVSync.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxBrightness, (float)comboBrightness.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, (int)comboFieldOfView.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV3P, (int)comboFieldOfView3P.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfx3PCameraMode, combo3PCameraMode.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDefaultFirstPersonCamera, comboDefaultCamera.SelectedIndex == 0);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxCameraDistance3P, (float)comboCameraDistance.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, GraphicsModeToQualityPreset((GraphicsMode)comboQualityPreset.SelectedIndex));
		GamePrefs.Set(EnumGamePrefs.OptionsGfxAA, comboAntiAliasing.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxAASharpness, (float)comboAntiAliasingSharp.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxTexQuality, comboTextureQuality.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxTexFilter, comboTextureFilter.SelectedIndex);
		if (comboReflectionQuality.SelectedIndex < origLength_ReflectionQuality)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectQuality, comboReflectionQuality.SelectedIndex);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectShadows, comboReflectedShadows.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterQuality, comboWaterQuality.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxViewDistance, (comboViewDistance.SelectedIndex == 0) ? 5 : ((comboViewDistance.SelectedIndex == 1) ? 6 : 7));
		GamePrefs.Set(EnumGamePrefs.OptionsGfxLODDistance, (float)comboLODDistance.Value);
		if (comboShadowsDistance.SelectedIndex < origLength_ShadowDistance)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, comboShadowsDistance.SelectedIndex);
		}
		if (comboShadowsQuality.SelectedIndex < origLength_ShadowQuality)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, comboShadowsQuality.SelectedIndex);
		}
		if (comboTerrainQuality.SelectedIndex < origLength_TerrainQuality)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTerrainQuality, comboTerrainQuality.SelectedIndex);
		}
		if (comboObjectQuality.SelectedIndex < origLength_ObjectQuality)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxObjQuality, comboObjectQuality.SelectedIndex);
		}
		if (comboGrassDistance.SelectedIndex < origLength_GrassDistance)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxGrassDistance, comboGrassDistance.SelectedIndex);
		}
		origDymeshEnabled = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshEnabled, comboDymeshEnabled.Value);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshDistance, comboDymeshDistance.Value);
		DynamicMeshSettings.MaxViewDistance = comboDymeshDistance.Value;
		GamePrefs.Set(EnumGamePrefs.DynamicMeshUseImposters, !comboDymeshHighQualityMesh.Value);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshMaxRegionCache, comboDymeshMaxRegions.Value);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshMaxItemCache, comboDymeshMaxMesh.Value);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshLandClaimOnly, comboDymeshLandClaimOnly.Value);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshLandClaimBuffer, comboDymeshLandClaimBuffer.Value);
		if (origDymeshEnabled != comboDymeshEnabled.Value)
		{
			DynamicMeshManager.EnabledChanged(comboDymeshEnabled.Value);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsGfxOcclusion, comboOcclusion.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxBloom, comboBloom.Value);
		if (comboDepthOfField != null)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, comboDepthOfField.Value);
		}
		if (comboMotionBlur.SelectedIndex < origLength_MotionBlur)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlur, comboMotionBlur.SelectedIndex);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlurEnabled, comboMotionBlur.SelectedIndex > 0);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, comboSSAO.Value);
		if (comboSSReflections.SelectedIndex < origLength_SSR)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSReflections, comboSSReflections.SelectedIndex);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsGfxSunShafts, comboSunShafts.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterPtlLimiter, (float)comboParticles.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsBackgroundGlobalOpacity, (float)comboUIBackgroundOpacity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsForegroundGlobalOpacity, (float)comboUIForegroundOpacity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsHudSize, (float)comboUiSize.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsScreenBoundsValue, (float)comboScreenBounds.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsUiFpsScaling, (float)comboUiFpsScaling.Value);
		GamePrefs.Instance.Save();
		GameOptionsManager.ApplyAllOptions(base.xui.playerUI);
		QualitySettings.vSyncCount = GamePrefs.GetInt(VSyncCountPref);
		ReflectionManager.ApplyOptions();
		WaterSplashCubes.particleLimiter = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		XUi[] array = UnityEngine.Object.FindObjectsOfType<XUi>();
		foreach (XUi obj in array)
		{
			obj.BackgroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
			obj.ForegroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		}
		if (GameManager.Instance.World != null)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if ((bool)primaryPlayer)
			{
				primaryPlayer.UpdateCameraDistanceFromPrefs();
			}
		}
		XUiC_OptionsVideo.OnSettingsChanged?.Invoke();
		previousSettings = GamePrefs.GetSettingsCopy();
		btnApply.Enabled = false;
	}

	public static int ConvertFullScreenModeToIndex(FullScreenMode _mode)
	{
		return _mode switch
		{
			FullScreenMode.FullScreenWindow => 1, 
			FullScreenMode.ExclusiveFullScreen => 2, 
			_ => 0, 
		};
	}

	public static FullScreenMode ConvertIndexToFullScreenMode(int _index)
	{
		return _index switch
		{
			1 => FullScreenMode.FullScreenWindow, 
			2 => FullScreenMode.ExclusiveFullScreen, 
			_ => FullScreenMode.Windowed, 
		};
	}

	public override void OnOpen()
	{
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		previousSettings = GamePrefs.GetSettingsCopy();
		int minIndex = GameOptionsManager.CalcTextureQualityMin();
		comboTextureQuality.MinIndex = minIndex;
		VSyncCountPref = PlatformApplicationManager.Application.VSyncCountPref;
		updateGraphicOptions();
		updateDynamicOptions();
		bool flag = GameManager.Instance.World != null;
		comboDymeshLandClaimOnly.Enabled = !flag;
		comboViewDistance.Enabled = !flag;
		comboOcclusion.Enabled = !flag;
		comboDymeshEnabled.Enabled = !flag;
		base.OnOpen();
		btnApply.Enabled = false;
		RefreshApplyLabel();
	}

	public override void OnClose()
	{
		GamePrefs.ApplySettingsCopy(previousSettings);
		base.OnClose();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
		if (btnApply.Enabled && base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			BtnApply_OnPressed(null, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "isTabUi":
			_value = ((tabs != null && tabs.IsSelected("xuiOptionsVideoUI")) ? "true" : "false");
			return true;
		case "ui_size_limited":
		{
			float num = (float)GameOptionsManager.GetUiSizeLimit();
			float num2 = GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize);
			_value = (num2 > num).ToString();
			return true;
		}
		case "ui_size_limit":
			_value = GameOptionsManager.GetUiSizeLimit().ToCultureInvariantString();
			return true;
		case "texture_quality_limited":
		{
			XUiC_ComboBoxList<string> xUiC_ComboBoxList2 = comboTextureQuality;
			_value = (xUiC_ComboBoxList2 != null && xUiC_ComboBoxList2.MinIndex > 0).ToString();
			return true;
		}
		case "upscaler_mode_dynamic":
		{
			XUiC_ComboBoxList<string> xUiC_ComboBoxList5 = comboUpscalerMode;
			_value = (xUiC_ComboBoxList5 != null && xUiC_ComboBoxList5.SelectedIndex == 1).ToString();
			return true;
		}
		case "upscaler_mode_scale":
		{
			XUiC_ComboBoxList<string> xUiC_ComboBoxList3 = comboUpscalerMode;
			_value = (xUiC_ComboBoxList3 != null && xUiC_ComboBoxList3.SelectedIndex == 2).ToString();
			return true;
		}
		case "upscaler_mode_fsr":
		{
			XUiC_ComboBoxList<string> xUiC_ComboBoxList4 = comboUpscalerMode;
			_value = (xUiC_ComboBoxList4 != null && xUiC_ComboBoxList4.SelectedIndex == 3).ToString();
			return true;
		}
		case "upscaler_mode_dlss":
		{
			XUiC_ComboBoxList<string> xUiC_ComboBoxList = comboUpscalerMode;
			_value = (xUiC_ComboBoxList != null && xUiC_ComboBoxList.SelectedIndex == 4).ToString();
			return true;
		}
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}

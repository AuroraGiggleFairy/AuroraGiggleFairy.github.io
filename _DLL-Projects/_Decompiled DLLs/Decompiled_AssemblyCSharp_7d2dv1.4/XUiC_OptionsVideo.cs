using System;
using Platform;
using UnityEngine;
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
			case 3555:
			case 3556:
			case 3600:
				return (_aspectRatio: EAspectRatio.Aspect_32_9, _aspectRatioFactor: 3.555f, _aspectRatioString: "32:9");
			case 3200:
				return (_aspectRatio: EAspectRatio.Aspect_32_10, _aspectRatioFactor: 3.2f, _aspectRatioString: "32:10");
			case 2400:
				return (_aspectRatio: EAspectRatio.Aspect_24_10, _aspectRatioFactor: 2.4f, _aspectRatioString: "24:10");
			case 2333:
			case 2370:
				return (_aspectRatio: EAspectRatio.Aspect_21_9, _aspectRatioFactor: 2.37f, _aspectRatioString: "21:9");
			case 1896:
				return (_aspectRatio: EAspectRatio.Aspect_17_9, _aspectRatioFactor: 2.37f, _aspectRatioString: "17:9");
			case 1770:
			case 1777:
			case 1778:
				return (_aspectRatio: EAspectRatio.Aspect_16_9, _aspectRatioFactor: 1.777f, _aspectRatioString: "16:9");
			case 1666:
				return (_aspectRatio: EAspectRatio.Aspect_5_3, _aspectRatioFactor: 1.666f, _aspectRatioString: "5:3");
			case 1600:
				return (_aspectRatio: EAspectRatio.Aspect_16_10, _aspectRatioFactor: 1.6f, _aspectRatioString: "16:10");
			case 1562:
				return (_aspectRatio: EAspectRatio.Aspect_25_16, _aspectRatioFactor: 1.562f, _aspectRatioString: "25:16");
			case 1500:
				return (_aspectRatio: EAspectRatio.Aspect_3_2, _aspectRatioFactor: 1.5f, _aspectRatioString: "3:2");
			case 1333:
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

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGamePrefs VSyncCountPref = EnumGamePrefs.OptionsGfxVsync;

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<ResolutionInfo> comboResolution;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboFullscreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboDynamicMode;

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
		comboResolution = GetChildById("Resolution").GetChildByType<XUiC_ComboBoxList<ResolutionInfo>>();
		comboFullscreen = GetChildById("Fullscreen").GetChildByType<XUiC_ComboBoxList<string>>();
		comboDynamicMode = GetChildById("DyMode").GetChildByType<XUiC_ComboBoxList<string>>();
		comboDynamicMinFPS = GetChildById("DyMinFPS").GetChildByType<XUiC_ComboBoxInt>();
		comboDynamicScale = GetChildById("DyScale").GetChildByType<XUiC_ComboBoxFloat>();
		comboVSync = GetChildById("VSync").GetChildByType<XUiC_ComboBoxList<string>>();
		comboBrightness = GetChildById("Brightness").GetChildByType<XUiC_ComboBoxFloat>();
		btnDefaultBrightness = GetChildById("btnDefaultBrightness").GetChildByType<XUiC_SimpleButton>();
		comboFieldOfView = GetChildById("FieldOfView").GetChildByType<XUiC_ComboBoxInt>();
		btnDefaultFOV = GetChildById("btnDefaultFOV").GetChildByType<XUiC_SimpleButton>();
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
		comboDynamicMode.OnValueChangedGeneric += anyOtherValueChanged;
		comboDynamicMinFPS.OnValueChangedGeneric += anyOtherValueChanged;
		comboDynamicScale.OnValueChangedGeneric += anyOtherValueChanged;
		comboVSync.OnValueChangedGeneric += anyOtherValueChanged;
		comboBrightness.OnValueChangedGeneric += anyOtherValueChanged;
		comboFieldOfView.OnValueChangedGeneric += anyOtherValueChanged;
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
			comboQualityPreset.Elements.Add("ConsolePerformanceFSR");
			comboQualityPreset.Elements.Add("ConsoleQuality");
			comboQualityPreset.Elements.Add("ConsoleQualityFSR");
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
	public void TabSelector_OnTabChanged(int _i, string _s)
	{
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
	public void QualityPresetChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		if (comboQualityPreset.SelectedIndex != 5)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, comboQualityPreset.SelectedIndex);
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
		GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, comboQualityPreset.SelectedIndex);
		btnApply.Enabled = true;
		updateGraphicsAAOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDynamicOptions()
	{
		comboDynamicMinFPS.Enabled = comboDynamicMode.SelectedIndex == 1;
		comboDynamicScale.Enabled = comboDynamicMode.SelectedIndex == 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGraphicsAAOptions()
	{
		comboAntiAliasingSharp.Enabled = comboAntiAliasing.SelectedIndex >= 4;
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
		comboDynamicMode.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxDynamicMode);
		comboDynamicMinFPS.Value = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxDynamicMinFPS);
		comboDynamicScale.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxDynamicScale);
		comboVSync.SelectedIndex = GamePrefs.GetInt(VSyncCountPref);
		comboBrightness.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness);
		comboFieldOfView.Value = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
		comboDymeshEnabled.Value = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		comboDymeshDistance.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshDistance);
		comboDymeshHighQualityMesh.Value = !GamePrefs.GetBool(EnumGamePrefs.DynamicMeshUseImposters);
		comboDymeshMaxRegions.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshMaxRegionCache);
		comboDymeshMaxMesh.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshMaxItemCache);
		comboDymeshLandClaimOnly.Value = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshLandClaimOnly);
		comboDymeshLandClaimBuffer.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshLandClaimBuffer);
		comboQualityPreset.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset);
		comboAntiAliasing.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxAA);
		comboAntiAliasingSharp.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxAASharpness);
		updateGraphicsAAOptions();
		comboTextureQuality.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexQuality);
		comboTextureFilter.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexFilter);
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxReflectQuality);
		if (num < origLength_ReflectionQuality)
		{
			comboReflectionQuality.SelectedIndex = num;
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
		int num2 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		if (num2 >= origLength_ShadowDistance && num2 < 20)
		{
			num2 = 20;
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 20);
		}
		if (num2 < origLength_ShadowDistance)
		{
			comboShadowsDistance.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		}
		else
		{
			comboShadowsDistance.MaxIndex = origLength_ShadowDistance;
			comboShadowsDistance.SelectedIndex = comboShadowsDistance.Elements.Count - 1;
		}
		int num3 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowQuality);
		if (num3 < origLength_ShadowQuality)
		{
			comboShadowsQuality.SelectedIndex = num3;
		}
		else
		{
			comboShadowsQuality.MaxIndex = origLength_ShadowQuality;
			comboShadowsQuality.SelectedIndex = comboShadowsQuality.Elements.Count - 1;
		}
		int num4 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTerrainQuality);
		if (num4 < origLength_TerrainQuality)
		{
			comboTerrainQuality.SelectedIndex = num4;
		}
		else
		{
			comboTerrainQuality.MaxIndex = origLength_TerrainQuality;
			comboTerrainQuality.SelectedIndex = comboTerrainQuality.Elements.Count - 1;
		}
		int num5 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxObjQuality);
		if (num5 < origLength_ObjectQuality)
		{
			comboObjectQuality.SelectedIndex = num5;
		}
		else
		{
			comboObjectQuality.MaxIndex = origLength_ObjectQuality;
			comboObjectQuality.SelectedIndex = comboObjectQuality.Elements.Count - 1;
		}
		int num6 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxGrassDistance);
		if (num6 < origLength_GrassDistance)
		{
			comboGrassDistance.SelectedIndex = num6;
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
		int num7 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxMotionBlur);
		if (num7 < origLength_MotionBlur)
		{
			comboMotionBlur.SelectedIndex = num7;
		}
		else
		{
			comboMotionBlur.MaxIndex = origLength_MotionBlur;
			comboMotionBlur.SelectedIndex = comboMotionBlur.Elements.Count - 1;
		}
		comboSSAO.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSSAO);
		int num8 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSSReflections);
		if (num8 < origLength_SSR)
		{
			comboSSReflections.SelectedIndex = num8;
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
		if (comboAntiAliasing.SelectedIndex >= 5)
		{
			comboDynamicMode.SelectedIndex = 0;
		}
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicMode, comboDynamicMode.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicMinFPS, (int)comboDynamicMinFPS.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicScale, (float)comboDynamicScale.Value);
		GamePrefs.Set(VSyncCountPref, comboVSync.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxBrightness, (float)comboBrightness.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, (int)comboFieldOfView.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, comboQualityPreset.SelectedIndex);
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

	public override bool GetBindingValue(ref string _value, string _bindingName)
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
			XUiC_ComboBoxList<string> xUiC_ComboBoxList = comboTextureQuality;
			_value = (xUiC_ComboBoxList != null && xUiC_ComboBoxList.MinIndex > 0).ToString();
			return true;
		}
		default:
			return base.GetBindingValue(ref _value, _bindingName);
		}
	}
}

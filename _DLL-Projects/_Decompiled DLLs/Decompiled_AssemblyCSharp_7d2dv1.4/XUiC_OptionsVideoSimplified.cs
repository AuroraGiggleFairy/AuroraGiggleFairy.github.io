using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsVideoSimplified : XUiController
{
	public enum GraphicsMode
	{
		ConsolePerformance,
		ConsolePerformanceFSR,
		ConsoleQuality,
		ConsoleQualityFSR,
		ConsoleCustom
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboBrightness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaultBrightness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt comboFieldOfView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaultFOV;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboMotionBlur;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboGraphicsMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUIBackgroundOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUIForegroundOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUiSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboScreenBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public object[] previousSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public float previousRefreshRate = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGamePrefs VSyncCountPref = EnumGamePrefs.OptionsGfxVsync;

	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		tabs = GetChildByType<XUiC_TabSelector>();
		tabs.OnTabChanged += TabSelector_OnTabChanged;
		comboBrightness = GetChildById("Brightness").GetChildByType<XUiC_ComboBoxFloat>();
		comboBrightness.OnValueChangedGeneric += anyOtherValueChanged;
		btnDefaultBrightness = GetChildById("btnDefaultBrightness").GetChildByType<XUiC_SimpleButton>();
		btnDefaultBrightness.OnPressed += BtnDefaultBrightness_OnPressed;
		comboFieldOfView = GetChildById("FieldOfViewSimplified").GetChildByType<XUiC_ComboBoxInt>();
		comboFieldOfView.OnValueChangedGeneric += anyOtherValueChanged;
		comboFieldOfView.Min = Constants.cMinCameraFieldOfView;
		comboFieldOfView.Max = Constants.cMaxCameraFieldOfView;
		btnDefaultFOV = GetChildById("btnDefaultFOV").GetChildByType<XUiC_SimpleButton>();
		btnDefaultFOV.OnPressed += BtnDefaultFOV_OnPressed;
		comboMotionBlur = GetChildById("MotionBlurToggle").GetChildByType<XUiC_ComboBoxBool>();
		comboMotionBlur.OnValueChangedGeneric += AnyPresetValueChanged;
		comboGraphicsMode = GetChildById("GraphicsMode").GetChildByType<XUiC_ComboBoxList<string>>();
		comboGraphicsMode.OnValueChangedGeneric += AnyPresetValueChanged;
		comboUIBackgroundOpacity = GetChildById("UIBackgroundOpacity").GetChildByType<XUiC_ComboBoxFloat>();
		comboUIForegroundOpacity = GetChildById("UIForegroundOpacity").GetChildByType<XUiC_ComboBoxFloat>();
		comboUiSize = GetChildById("UiSize").GetChildByType<XUiC_ComboBoxFloat>();
		comboScreenBounds = GetChildById("ScreenBounds").GetChildByType<XUiC_ComboBoxFloat>();
		comboUIBackgroundOpacity.OnValueChangedGeneric += anyOtherValueChanged;
		comboUIForegroundOpacity.OnValueChangedGeneric += anyOtherValueChanged;
		comboUiSize.OnValueChangedGeneric += anyOtherValueChanged;
		comboScreenBounds.OnValueChangedGeneric += anyOtherValueChanged;
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
		btnBack = GetChildById("btnBack") as XUiC_SimpleButton;
		btnDefaults = GetChildById("btnDefaults") as XUiC_SimpleButton;
		btnApply = GetChildById("btnApply") as XUiC_SimpleButton;
		btnBack.OnPressed += BtnBack_OnPressed;
		btnDefaults.OnPressed += BtnDefaults_OnOnPressed;
		btnApply.OnPressed += BtnApply_OnPressed;
		btnApply.Text = "[action:gui:GUI Apply] " + Localization.Get("xuiApply").ToUpper();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TabSelector_OnTabChanged(int _i, string _s)
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void anyOtherValueChanged(XUiController _sender)
	{
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
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AnyPresetValueChanged(XUiController _sender)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGraphicOptions()
	{
		comboBrightness.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness);
		comboFieldOfView.Value = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
		comboMotionBlur.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxMotionBlurEnabled);
		int selectedIndex = (int)QualityPresetToGraphicsMode(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset));
		UpdateCustomModeVisibility(selectedIndex);
		comboGraphicsMode.SelectedIndex = selectedIndex;
		comboUIBackgroundOpacity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
		comboUIForegroundOpacity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		comboUiSize.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize);
		comboScreenBounds.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCustomModeVisibility(int selectedIndex)
	{
		comboGraphicsMode.MaxIndex = Math.Max(selectedIndex, 3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		GamePrefs.Set(EnumGamePrefs.OptionsGfxBrightness, (float)comboBrightness.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, (int)comboFieldOfView.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlurEnabled, comboMotionBlur.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, GraphicsModeToQualityPreset((GraphicsMode)comboGraphicsMode.SelectedIndex));
		UpdateCustomModeVisibility(comboGraphicsMode.SelectedIndex);
		GameOptionsManager.SetGraphicsQuality();
		GamePrefs.Set(EnumGamePrefs.OptionsBackgroundGlobalOpacity, (float)comboUIBackgroundOpacity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsForegroundGlobalOpacity, (float)comboUIForegroundOpacity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsHudSize, (float)comboUiSize.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsScreenBoundsValue, (float)comboScreenBounds.Value);
		GamePrefs.Instance.Save();
		GameOptionsManager.ApplyAllOptions(base.xui.playerUI);
		XUi[] array = UnityEngine.Object.FindObjectsOfType<XUi>();
		foreach (XUi obj in array)
		{
			obj.BackgroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
			obj.ForegroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		}
		XUiC_OptionsVideoSimplified.OnSettingsChanged?.Invoke();
		previousSettings = GamePrefs.GetSettingsCopy();
		btnApply.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GraphicsMode QualityPresetToGraphicsMode(int qualityPreset)
	{
		return qualityPreset switch
		{
			6 => GraphicsMode.ConsolePerformance, 
			7 => GraphicsMode.ConsolePerformanceFSR, 
			8 => GraphicsMode.ConsoleQuality, 
			9 => GraphicsMode.ConsoleQualityFSR, 
			_ => GraphicsMode.ConsoleCustom, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GraphicsModeToQualityPreset(GraphicsMode graphicsMode)
	{
		return graphicsMode switch
		{
			GraphicsMode.ConsolePerformance => 6, 
			GraphicsMode.ConsolePerformanceFSR => 7, 
			GraphicsMode.ConsoleQuality => 8, 
			GraphicsMode.ConsoleQualityFSR => 9, 
			_ => 5, 
		};
	}

	public override void OnOpen()
	{
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		previousSettings = GamePrefs.GetSettingsCopy();
		VSyncCountPref = PlatformApplicationManager.Application.VSyncCountPref;
		updateGraphicOptions();
		base.OnOpen();
		btnApply.Enabled = false;
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
		default:
			return base.GetBindingValue(ref _value, _bindingName);
		}
	}
}

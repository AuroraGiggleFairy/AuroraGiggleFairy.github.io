using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsAudio : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboOverallAudioVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDynamicMusicEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboDynamicMusicDailyTimeAllotted;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboAmbientVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboInGameMusicVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboMenuMusicVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboMumblePositionalAudioSupportEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboVoiceChatEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboVoiceOutputDevice;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboVoiceInputDevice;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboVoiceVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboSubtitlesEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboProfanityFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origOverallAudioVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origDynamicMusicEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origDynamicMusicDailyTimeAllotted;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origAmbientVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origInGameMusicVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origMenuMusicVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origMumblePositionalAudioSupportEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origVoiceChatEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origVoiceVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origSubtitlesEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origProfanityFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IPartyVoice.VoiceAudioDevice noDeviceEntry = new IPartyVoice.VoiceAudioDeviceNotFound();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IPartyVoice.VoiceAudioDevice defaultDeviceEntry = new IPartyVoice.VoiceAudioDeviceDefault();

	public bool voiceAvailable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (PlatformManager.MultiPlatform.PartyVoice != null)
			{
				return PlatformManager.MultiPlatform.PartyVoice.Status == EPartyVoiceStatus.Ok;
			}
			return false;
		}
	}

	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		comboOverallAudioVolumeLevel = GetChildById("OverallAudioVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		comboDynamicMusicEnabled = GetChildById("DynamicMusicEnabled").GetChildByType<XUiC_ComboBoxBool>();
		comboDynamicMusicDailyTimeAllotted = GetChildById("DynamicMusicDailyTimeAllotted").GetChildByType<XUiC_ComboBoxFloat>();
		comboAmbientVolumeLevel = GetChildById("AmbientVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		comboInGameMusicVolumeLevel = GetChildById("InGameMusicVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		comboMenuMusicVolumeLevel = GetChildById("MenuMusicVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		comboProfanityFilter = GetChildById("ProfanityFilter").GetChildByType<XUiC_ComboBoxBool>();
		comboOverallAudioVolumeLevel.OnValueChanged += ComboOverallAudioVolumeLevelOnOnValueChanged;
		comboDynamicMusicEnabled.OnValueChanged += ComboDynamicMusicEnabledOnOnValueChanged;
		comboDynamicMusicDailyTimeAllotted.OnValueChanged += ComboDynamicMusicDailyTimeAllottedOnOnValueChanged;
		comboAmbientVolumeLevel.OnValueChanged += ComboAmbientVolumeLevelOnOnValueChanged;
		comboInGameMusicVolumeLevel.OnValueChanged += ComboInGameMusicVolumeLevelOnOnValueChanged;
		comboMenuMusicVolumeLevel.OnValueChanged += ComboMenuMusicVolumeLevelOnOnValueChanged;
		comboProfanityFilter.OnValueChanged += ComboSubtitlesEnabledValuesChanged;
		comboOverallAudioVolumeLevel.Min = 0.0;
		comboOverallAudioVolumeLevel.Max = 1.0;
		comboAmbientVolumeLevel.Min = 0.0;
		comboAmbientVolumeLevel.Max = 1.0;
		comboDynamicMusicDailyTimeAllotted.Min = 0.0;
		comboDynamicMusicDailyTimeAllotted.Max = 1.0;
		comboInGameMusicVolumeLevel.Min = 0.0;
		comboInGameMusicVolumeLevel.Max = 1.0;
		comboMenuMusicVolumeLevel.Min = 0.0;
		comboMenuMusicVolumeLevel.Max = 1.0;
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			comboMumblePositionalAudioSupportEnabled = GetChildById("MumblePositionalAudioSupportEnabled").GetChildByType<XUiC_ComboBoxBool>();
			comboMumblePositionalAudioSupportEnabled.OnValueChanged += ComboMumblePositionalAudioSupportEnabledOnValueChanged;
		}
		if (voiceAvailable)
		{
			comboVoiceChatEnabled = GetChildById("VoiceChatEnabled").GetChildByType<XUiC_ComboBoxBool>();
			comboVoiceOutputDevice = GetChildById("VoiceOutputDevice")?.GetChildByType<XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice>>();
			comboVoiceInputDevice = GetChildById("VoiceInputDevice")?.GetChildByType<XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice>>();
			comboVoiceVolumeLevel = GetChildById("VoiceVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
			comboVoiceChatEnabled.OnValueChanged += ComboVoiceChatEnabledOnOnValueChanged;
			if (comboVoiceOutputDevice != null)
			{
				comboVoiceOutputDevice.OnValueChanged += ComboVoiceDeviceOnValueChanged;
			}
			if (comboVoiceInputDevice != null)
			{
				comboVoiceInputDevice.OnValueChanged += ComboVoiceDeviceOnValueChanged;
			}
			comboVoiceVolumeLevel.OnValueChanged += ComboVoiceVolumeLevelOnOnValueChanged;
			comboVoiceVolumeLevel.Min = 0.0;
			comboVoiceVolumeLevel.Max = 2.0;
		}
		((XUiC_SimpleButton)GetChildById("btnBack")).OnPressed += BtnBack_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnDefaults")).OnPressed += BtnDefaults_OnOnPressed;
		btnApply = (XUiC_SimpleButton)GetChildById("btnApply");
		btnApply.OnPressed += BtnApply_OnPressed;
		RegisterForInputStyleChanges();
		RefreshApplyLabel();
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboOverallAudioVolumeLevelOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsOverallAudioVolumeLevel, (float)comboOverallAudioVolumeLevel.Value);
		AudioListener.volume = GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDynamicMusicEnabledOnOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsDynamicMusicEnabled, comboDynamicMusicEnabled.Value);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDynamicMusicDailyTimeAllottedOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsDynamicMusicDailyTime, (float)comboDynamicMusicDailyTimeAllotted.Value);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboAmbientVolumeLevelOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsAmbientVolumeLevel, (float)comboAmbientVolumeLevel.Value);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboInGameMusicVolumeLevelOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsMusicVolumeLevel, (float)comboInGameMusicVolumeLevel.Value);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboMenuMusicVolumeLevelOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsMenuMusicVolumeLevel, (float)comboMenuMusicVolumeLevel.Value);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboMumblePositionalAudioSupportEnabledOnValueChanged(XUiController _sender, bool _oldvalue, bool _newvalue)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboVoiceChatEnabledOnOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsVoiceChatEnabled, comboVoiceChatEnabled.Value);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboVoiceDeviceOnValueChanged(XUiController _sender, IPartyVoice.VoiceAudioDevice _oldvalue, IPartyVoice.VoiceAudioDevice _newvalue)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboVoiceVolumeLevelOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsVoiceVolumeLevel, (float)comboVoiceVolumeLevel.Value);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboSubtitlesEnabledValuesChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		applyChanges();
		btnApply.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		GameOptionsManager.ResetGameOptions(GameOptionsManager.ResetType.Audio);
		updateOptions();
		applyChanges();
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVoiceDevices()
	{
		if (voiceAvailable && PermissionsManager.IsCommunicationAllowed())
		{
			var (devices, devices2) = PlatformManager.MultiPlatform.PartyVoice.GetDevicesList();
			SelectActiveDevice(devices, comboVoiceInputDevice, EnumGamePrefs.OptionsVoiceInputDevice);
			SelectActiveDevice(devices2, comboVoiceOutputDevice, EnumGamePrefs.OptionsVoiceOutputDevice);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void SelectActiveDevice(IList<IPartyVoice.VoiceAudioDevice> _devices, XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> _combo, EnumGamePrefs _activeDevicePref)
		{
			if (_combo != null)
			{
				string activeDevice = GamePrefs.GetString(_activeDevicePref);
				_combo.Elements.Clear();
				_combo.Elements.AddRange(_devices);
				int selectedIndex;
				if (_combo.Elements.Count == 0)
				{
					_combo.Elements.Add(noDeviceEntry);
					_combo.SelectedIndex = 0;
				}
				else if (string.IsNullOrEmpty(activeDevice) || (selectedIndex = _combo.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (IPartyVoice.VoiceAudioDevice _device) => _device.Identifier == activeDevice)) < 0)
				{
					int num = _combo.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (IPartyVoice.VoiceAudioDevice _device) => _device.IsDefault);
					if (num < 0)
					{
						_combo.Elements.Insert(0, defaultDeviceEntry);
						num = 0;
					}
					_combo.SelectedIndex = num;
				}
				else
				{
					_combo.SelectedIndex = selectedIndex;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptions()
	{
		comboOverallAudioVolumeLevel.Value = (origOverallAudioVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel));
		comboDynamicMusicEnabled.Value = (origDynamicMusicEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsDynamicMusicEnabled));
		comboDynamicMusicDailyTimeAllotted.Value = (origDynamicMusicDailyTimeAllotted = GamePrefs.GetFloat(EnumGamePrefs.OptionsDynamicMusicDailyTime));
		comboAmbientVolumeLevel.Value = (origAmbientVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsAmbientVolumeLevel));
		comboInGameMusicVolumeLevel.Value = (origInGameMusicVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel));
		comboMenuMusicVolumeLevel.Value = (origMenuMusicVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsMenuMusicVolumeLevel));
		comboProfanityFilter.Value = (origProfanityFilter = GamePrefs.GetBool(EnumGamePrefs.OptionsFilterProfanity));
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			comboMumblePositionalAudioSupportEnabled.Value = (origMumblePositionalAudioSupportEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsMumblePositionalAudioSupport));
		}
		if (voiceAvailable)
		{
			comboVoiceChatEnabled.Value = (origVoiceChatEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled));
			comboVoiceVolumeLevel.Value = (origVoiceVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsVoiceVolumeLevel));
			updateVoiceDevices();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		AudioListener.volume = GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel);
		origOverallAudioVolumeLevel = (float)comboOverallAudioVolumeLevel.Value;
		origDynamicMusicEnabled = comboDynamicMusicEnabled.Value;
		origDynamicMusicDailyTimeAllotted = (float)comboDynamicMusicDailyTimeAllotted.Value;
		origAmbientVolumeLevel = (float)comboAmbientVolumeLevel.Value;
		origInGameMusicVolumeLevel = (float)comboInGameMusicVolumeLevel.Value;
		origMenuMusicVolumeLevel = (float)comboMenuMusicVolumeLevel.Value;
		origProfanityFilter = comboProfanityFilter.Value;
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			origMumblePositionalAudioSupportEnabled = comboMumblePositionalAudioSupportEnabled.Value;
			GamePrefs.Set(EnumGamePrefs.OptionsMumblePositionalAudioSupport, origMumblePositionalAudioSupportEnabled);
		}
		if (voiceAvailable)
		{
			origVoiceChatEnabled = comboVoiceChatEnabled.Value;
			origVoiceVolumeLevel = (float)comboVoiceVolumeLevel.Value;
			if (comboVoiceInputDevice?.Value != null)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsVoiceInputDevice, comboVoiceInputDevice.Value.Identifier);
			}
			if (comboVoiceOutputDevice?.Value != null)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsVoiceOutputDevice, comboVoiceOutputDevice.Value.Identifier);
			}
			updateVoiceDevices();
		}
		GamePrefs.Instance.Save();
		if (XUiC_OptionsAudio.OnSettingsChanged != null)
		{
			XUiC_OptionsAudio.OnSettingsChanged();
		}
	}

	public override void OnOpen()
	{
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		updateOptions();
		base.OnOpen();
		btnApply.Enabled = false;
		RefreshBindings();
		RefreshApplyLabel();
	}

	public override void OnClose()
	{
		base.OnClose();
		GamePrefs.Set(EnumGamePrefs.OptionsOverallAudioVolumeLevel, origOverallAudioVolumeLevel);
		AudioListener.volume = origOverallAudioVolumeLevel;
		GamePrefs.Set(EnumGamePrefs.OptionsDynamicMusicEnabled, origDynamicMusicEnabled);
		GamePrefs.Set(EnumGamePrefs.OptionsDynamicMusicDailyTime, origDynamicMusicDailyTimeAllotted);
		GamePrefs.Set(EnumGamePrefs.OptionsAmbientVolumeLevel, origAmbientVolumeLevel);
		GamePrefs.Set(EnumGamePrefs.OptionsMusicVolumeLevel, origInGameMusicVolumeLevel);
		GamePrefs.Set(EnumGamePrefs.OptionsMenuMusicVolumeLevel, origMenuMusicVolumeLevel);
		GamePrefs.Set(EnumGamePrefs.OptionsFilterProfanity, origProfanityFilter);
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			GamePrefs.Set(EnumGamePrefs.OptionsMumblePositionalAudioSupport, origMumblePositionalAudioSupportEnabled);
		}
		if (voiceAvailable)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsVoiceChatEnabled, origVoiceChatEnabled);
			GamePrefs.Set(EnumGamePrefs.OptionsVoiceVolumeLevel, origVoiceVolumeLevel);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (btnApply.Enabled && base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			BtnApply_OnPressed(null, 0);
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "notingame":
			_value = (GameStats.GetInt(EnumGameStats.GameState) == 0).ToString();
			return true;
		case "notinlinux":
			_value = "true";
			return true;
		case "commsallowed":
			_value = PermissionsManager.IsCommunicationAllowed().ToString();
			return true;
		case "voiceavailable":
			_value = voiceAvailable.ToString();
			return true;
		default:
			return base.GetBindingValue(ref _value, _bindingName);
		}
	}
}

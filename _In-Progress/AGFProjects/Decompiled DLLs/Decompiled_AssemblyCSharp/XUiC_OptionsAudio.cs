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
	public XUiC_TabSelector tabSelector;

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
	public XUiC_ComboBoxBool comboSubtitlesEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboProfanityFilter;

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
	public bool origSubtitlesEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origProfanityFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origMumblePositionalAudioSupportEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origVoiceChatEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origVoiceVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IPartyVoice.VoiceAudioDevice noDeviceEntry = new IPartyVoice.VoiceAudioDeviceNotFound();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IPartyVoice.VoiceAudioDevice defaultDeviceEntry = new IPartyVoice.VoiceAudioDeviceDefault();

	public const string DiscordTabName = "xuiOptionsAudioDiscord";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDiscordEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDiscordDmPrivacyMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<DiscordManager.EAutoJoinVoiceMode> comboDiscordAutoJoinVoice;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDiscordVoiceButtonMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboDiscordOutputDevice;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboDiscordInputDevice;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboDiscordOutputVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboDiscordInputVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDiscordVoiceVadModeAuto;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt comboDiscordVoiceVadThreshold;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origDiscordOutputVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origDiscordInputVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origDiscordVoiceVadModeAuto;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origDiscordVoiceVadThreshold;

	public static bool VoiceAvailable
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

	public static bool IsCommunicationAllowed
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PermissionsManager.IsCommunicationAllowed();
		}
	}

	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		tabSelector = GetChildByType<XUiC_TabSelector>();
		comboOverallAudioVolumeLevel = GetChildById("OverallAudioVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		comboDynamicMusicEnabled = GetChildById("DynamicMusicEnabled").GetChildByType<XUiC_ComboBoxBool>();
		comboDynamicMusicDailyTimeAllotted = GetChildById("DynamicMusicDailyTimeAllotted").GetChildByType<XUiC_ComboBoxFloat>();
		comboAmbientVolumeLevel = GetChildById("AmbientVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		comboInGameMusicVolumeLevel = GetChildById("InGameMusicVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		comboMenuMusicVolumeLevel = GetChildById("MenuMusicVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		comboSubtitlesEnabled = GetChildById("SubtitlesEnabled").GetChildByType<XUiC_ComboBoxBool>();
		comboProfanityFilter = GetChildById("ProfanityFilter").GetChildByType<XUiC_ComboBoxBool>();
		comboOverallAudioVolumeLevel.OnValueChanged += ComboOverallAudioVolumeLevelOnOnValueChanged;
		comboDynamicMusicEnabled.OnValueChanged += ComboDynamicMusicEnabledOnOnValueChanged;
		comboDynamicMusicDailyTimeAllotted.OnValueChanged += ComboDynamicMusicDailyTimeAllottedOnOnValueChanged;
		comboAmbientVolumeLevel.OnValueChanged += ComboAmbientVolumeLevelOnOnValueChanged;
		comboInGameMusicVolumeLevel.OnValueChanged += ComboInGameMusicVolumeLevelOnOnValueChanged;
		comboMenuMusicVolumeLevel.OnValueChanged += ComboMenuMusicVolumeLevelOnOnValueChanged;
		comboSubtitlesEnabled.OnValueChanged += ComboSubtitlesEnabledOnValueChanged;
		comboProfanityFilter.OnValueChanged += ComboSubtitlesEnabledOnValueChanged;
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			comboMumblePositionalAudioSupportEnabled = GetChildById("MumblePositionalAudioSupportEnabled").GetChildByType<XUiC_ComboBoxBool>();
			comboMumblePositionalAudioSupportEnabled.OnValueChanged += ComboMumblePositionalAudioSupportEnabledOnValueChanged;
		}
		if (VoiceAvailable)
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
		}
		((XUiC_SimpleButton)GetChildById("btnBack")).OnPressed += BtnBack_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnDefaults")).OnPressed += BtnDefaults_OnOnPressed;
		btnApply = (XUiC_SimpleButton)GetChildById("btnApply");
		btnApply.OnPressed += BtnApply_OnPressed;
		RegisterForInputStyleChanges();
		RefreshApplyLabel();
		initDiscord();
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
	public void ComboSubtitlesEnabledOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		applyChanges(_fromReset: false);
		btnApply.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		GameOptionsManager.ResetGameOptions(GameOptionsManager.ResetType.Audio);
		defaultsDiscord();
		updateOptions();
		applyChanges(_fromReset: true);
		btnApply.Enabled = false;
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
		if (VoiceAvailable && IsCommunicationAllowed)
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
		comboSubtitlesEnabled.Value = (origSubtitlesEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsSubtitlesEnabled));
		comboProfanityFilter.Value = (origProfanityFilter = GamePrefs.GetBool(EnumGamePrefs.OptionsFilterProfanity));
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			comboMumblePositionalAudioSupportEnabled.Value = (origMumblePositionalAudioSupportEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsMumblePositionalAudioSupport));
		}
		if (VoiceAvailable)
		{
			comboVoiceChatEnabled.Value = (origVoiceChatEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled));
			comboVoiceVolumeLevel.Value = (origVoiceVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsVoiceVolumeLevel));
			updateVoiceDevices();
		}
		updateOptionsDiscord();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges(bool _fromReset)
	{
		AudioListener.volume = GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel);
		origOverallAudioVolumeLevel = (float)comboOverallAudioVolumeLevel.Value;
		origDynamicMusicEnabled = comboDynamicMusicEnabled.Value;
		origDynamicMusicDailyTimeAllotted = (float)comboDynamicMusicDailyTimeAllotted.Value;
		origAmbientVolumeLevel = (float)comboAmbientVolumeLevel.Value;
		origInGameMusicVolumeLevel = (float)comboInGameMusicVolumeLevel.Value;
		origMenuMusicVolumeLevel = (float)comboMenuMusicVolumeLevel.Value;
		origSubtitlesEnabled = comboSubtitlesEnabled.Value;
		origProfanityFilter = comboProfanityFilter.Value;
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			origMumblePositionalAudioSupportEnabled = comboMumblePositionalAudioSupportEnabled.Value;
			GamePrefs.Set(EnumGamePrefs.OptionsMumblePositionalAudioSupport, origMumblePositionalAudioSupportEnabled);
		}
		if (VoiceAvailable)
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
		applyChangesDiscord(_fromReset);
		GamePrefs.Instance.Save();
		if (XUiC_OptionsAudio.OnSettingsChanged != null)
		{
			XUiC_OptionsAudio.OnSettingsChanged();
		}
	}

	public override void OnOpen()
	{
		XUiC_MainMenuPlayerName.Close(base.xui);
		onOpenDiscord();
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
		GamePrefs.Set(EnumGamePrefs.OptionsSubtitlesEnabled, origSubtitlesEnabled);
		GamePrefs.Set(EnumGamePrefs.OptionsFilterProfanity, origProfanityFilter);
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			GamePrefs.Set(EnumGamePrefs.OptionsMumblePositionalAudioSupport, origMumblePositionalAudioSupportEnabled);
		}
		if (VoiceAvailable)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsVoiceChatEnabled, origVoiceChatEnabled);
			GamePrefs.Set(EnumGamePrefs.OptionsVoiceVolumeLevel, origVoiceVolumeLevel);
		}
		onCloseDiscord();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
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
		case "notingame":
			_value = (GameStats.GetInt(EnumGameStats.GameState) == 0).ToString();
			return true;
		case "notinlinux":
			_value = "true";
			return true;
		case "commsallowed":
			_value = IsCommunicationAllowed.ToString();
			return true;
		case "multiplayerallowed":
			_value = PermissionsManager.IsMultiplayerAllowed().ToString();
			return true;
		case "is_online":
			_value = (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.LoggedIn).ToString();
			return true;
		case "voiceavailable":
			_value = VoiceAvailable.ToString();
			return true;
		case "discord_enabled":
			_value = (!DiscordManager.Instance.Settings.DiscordDisabled).ToString();
			return true;
		case "discordinitialized":
			_value = DiscordManager.Instance.IsInitialized.ToString();
			return true;
		case "discord_is_ready":
			_value = DiscordManager.Instance.IsReady.ToString();
			return true;
		case "discord_supports_full_accounts":
			_value = DiscordManager.SupportsFullAccounts.ToString();
			return true;
		case "discord_supports_provisional_accounts":
			_value = DiscordManager.SupportsProvisionalAccounts.ToString();
			return true;
		case "discordaccountlinked":
			_value = (!(DiscordManager.Instance.LocalUser?.IsProvisionalAccount ?? false)).ToString();
			return true;
		case "discord_ptt":
			_value = (comboDiscordVoiceButtonMode?.Value ?? false).ToString();
			return true;
		case "discord_vad_threshold_manual":
			_value = (!DiscordManager.Instance.Settings.VoiceVadModeAuto).ToString();
			return true;
		case "discord_in_call":
			_value = (DiscordManager.Instance.ActiveVoiceLobby != null).ToString();
			return true;
		case "discord_is_speaking":
			_value = (DiscordManager.Instance.ActiveVoiceLobby?.VoiceCall.IsSpeaking ?? false).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public void SetActiveTab(string _tabKey)
	{
		tabSelector?.SelectTabByName(_tabKey);
	}

	public void OpenAtTab(string _tabName)
	{
		SetActiveTab(_tabName);
		base.xui.playerUI.windowManager.Open(ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initDiscord()
	{
		comboDiscordEnabled = GetChildById("DiscordEnabled").GetChildByType<XUiC_ComboBoxBool>();
		comboDiscordDmPrivacyMode = GetChildById("DiscordDmPrivacyMode").GetChildByType<XUiC_ComboBoxBool>();
		comboDiscordAutoJoinVoice = GetChildById("DiscordAutoJoinVoice").GetChildByType<XUiC_ComboBoxEnum<DiscordManager.EAutoJoinVoiceMode>>();
		comboDiscordVoiceButtonMode = GetChildById("DiscordVoiceButtonMode").GetChildByType<XUiC_ComboBoxBool>();
		comboDiscordOutputDevice = GetChildById("DiscordOutputDevice").GetChildByType<XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice>>();
		comboDiscordInputDevice = GetChildById("DiscordInputDevice").GetChildByType<XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice>>();
		comboDiscordOutputVolume = GetChildById("DiscordOutputVolume").GetChildByType<XUiC_ComboBoxFloat>();
		comboDiscordInputVolume = GetChildById("DiscordInputVolume").GetChildByType<XUiC_ComboBoxFloat>();
		comboDiscordVoiceVadModeAuto = GetChildById("DiscordVADThresholdAuto").GetChildByType<XUiC_ComboBoxBool>();
		comboDiscordVoiceVadThreshold = GetChildById("DiscordVADThreshold").GetChildByType<XUiC_ComboBoxInt>();
		comboDiscordEnabled.OnValueChanged += ComboDiscordEnabledOnValueChanged;
		comboDiscordDmPrivacyMode.OnValueChanged += ComboDiscordDmPrivacyModeOnValueChanged;
		comboDiscordAutoJoinVoice.OnValueChanged += comboDiscordAutoJoinVoiceOnValueChanged;
		comboDiscordVoiceButtonMode.OnValueChanged += ComboDiscordVoiceButtonModeOnValueChanged;
		comboDiscordOutputDevice.OnValueChanged += ComboDiscordOutputDeviceOnValueChanged;
		comboDiscordInputDevice.OnValueChanged += ComboDiscordInputDeviceOnValueChanged;
		comboDiscordOutputVolume.OnValueChanged += ComboDiscordOutputVolumeOnValueChanged;
		comboDiscordInputVolume.OnValueChanged += ComboDiscordInputVolumeOnValueChanged;
		comboDiscordVoiceVadModeAuto.OnValueChanged += ComboDiscordVoiceVadModeAutoOnValueChanged;
		comboDiscordVoiceVadThreshold.OnValueChanged += ComboDiscordVoiceVadThresholdOnValueChanged;
		if (GetChildById("btnDiscordInitialize") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				XUiC_DiscordLogin.Open([PublicizedFrom(EAccessModifier.Private)] () =>
				{
					OpenAtTab("xuiOptionsAudioDiscord");
				}, _showSettingsButton: false);
				DiscordManager.Instance.AuthManager.AutoLogin();
			};
		}
		if (GetChildById("btnDiscordLinkAccount") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				XUiC_DiscordLogin.Open([PublicizedFrom(EAccessModifier.Private)] () =>
				{
					OpenAtTab("xuiOptionsAudioDiscord");
				}, _showSettingsButton: false, _waitForResultToShow: false, _skipOnSuccess: false, _modal: true, (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent());
				DiscordManager.Instance.AuthManager.LoginDiscordUser();
			};
		}
		if (GetChildById("btnDiscordUnlinkAccount") is XUiC_SimpleButton xUiC_SimpleButton3)
		{
			xUiC_SimpleButton3.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				XUiC_DiscordLogin.Open([PublicizedFrom(EAccessModifier.Private)] () =>
				{
					OpenAtTab("xuiOptionsAudioDiscord");
				}, _showSettingsButton: false, _waitForResultToShow: false, _skipOnSuccess: false, _modal: true, (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent());
				DiscordManager.Instance.AuthManager.UnmergeAccount();
			};
		}
		if (GetChildById("btnDiscordManageSocialSettings") is XUiC_SimpleButton xUiC_SimpleButton4)
		{
			xUiC_SimpleButton4.OnPressed += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
			{
				DiscordManager.Instance.OpenDiscordSocialSettings();
			};
		}
		if (GetChildById("btnDiscordManageBlockedUsers") is XUiC_SimpleButton xUiC_SimpleButton5)
		{
			xUiC_SimpleButton5.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				GUIWindow window = base.xui.playerUI.windowManager.GetWindow(XUiC_DiscordBlockedUsers.ID);
				window.openWindowOnEsc = windowGroup.ID;
				base.xui.playerUI.windowManager.Open(window, _bModal: true);
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordStatusChanged(DiscordManager.EDiscordStatus _status)
	{
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordLocalUserChanged(bool _loggedIn)
	{
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordVoiceStateChanged(bool _self, ulong _userId)
	{
		if (_self)
		{
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDiscordEnabledOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDiscordDmPrivacyModeOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void comboDiscordAutoJoinVoiceOnValueChanged(XUiController _sender, DiscordManager.EAutoJoinVoiceMode _oldValue, DiscordManager.EAutoJoinVoiceMode _newValue)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDiscordVoiceButtonModeOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		btnApply.Enabled = true;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDiscordOutputDeviceOnValueChanged(XUiController _sender, IPartyVoice.VoiceAudioDevice _oldValue, IPartyVoice.VoiceAudioDevice _newValue)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDiscordInputDeviceOnValueChanged(XUiController _sender, IPartyVoice.VoiceAudioDevice _oldValue, IPartyVoice.VoiceAudioDevice _newValue)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDiscordOutputVolumeOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		DiscordManager.Instance.Settings.OutputVolume = Mathf.RoundToInt((float)(_newValue * 100.0));
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDiscordInputVolumeOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		DiscordManager.Instance.Settings.InputVolume = Mathf.RoundToInt((float)(_newValue * 100.0));
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDiscordVoiceVadModeAutoOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		DiscordManager.Instance.Settings.VoiceVadModeAuto = _newValue;
		btnApply.Enabled = true;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDiscordVoiceVadThresholdOnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		DiscordManager.Instance.Settings.VoiceVadThreshold = (int)_newValue;
		btnApply.Enabled = true;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChangesDiscord(bool _fromReset)
	{
		DiscordManager.DiscordSettings settings = DiscordManager.Instance.Settings;
		bool isInitialized = DiscordManager.Instance.IsInitialized;
		bool value = comboDiscordEnabled.Value;
		settings.DiscordDisabled = !value;
		if (isInitialized)
		{
			origDiscordOutputVolume = (float)comboDiscordOutputVolume.Value;
			origDiscordInputVolume = (float)comboDiscordInputVolume.Value;
			settings.VoiceModePtt = comboDiscordVoiceButtonMode.Value;
			settings.DmPrivacyMode = comboDiscordDmPrivacyMode.Value;
			settings.AutoJoinVoiceMode = comboDiscordAutoJoinVoice.Value;
			origDiscordVoiceVadModeAuto = comboDiscordVoiceVadModeAuto.Value;
			origDiscordVoiceVadThreshold = (int)comboDiscordVoiceVadThreshold.Value;
			if (!_fromReset)
			{
				settings.SelectedOutputDevice = comboDiscordOutputDevice.Value.Identifier;
				settings.SelectedInputDevice = comboDiscordInputDevice.Value.Identifier;
			}
		}
		if (isInitialized != value)
		{
			if (!value)
			{
				windowGroup.isEscClosable = false;
				XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiOptionsAudioDiscordDisableRestartTitle"), Localization.Get("xuiOptionsAudioDiscordDisableRestartText"), [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					windowGroup.isEscClosable = true;
				}, _openMainMenuOnClose: false, _modal: false);
			}
			updateOptionsDiscord();
			RefreshBindings();
		}
		settings.Save();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void defaultsDiscord()
	{
		DiscordManager.Instance.Settings.ResetToDefaults();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptionsDiscord()
	{
		if (IsCommunicationAllowed)
		{
			DiscordManager.DiscordSettings settings = DiscordManager.Instance.Settings;
			comboDiscordEnabled.Value = !settings.DiscordDisabled;
			comboDiscordVoiceButtonMode.Value = settings.VoiceModePtt;
			comboDiscordDmPrivacyMode.Value = settings.DmPrivacyMode;
			comboDiscordAutoJoinVoice.Value = settings.AutoJoinVoiceMode;
			comboDiscordOutputVolume.Value = (origDiscordOutputVolume = (float)settings.OutputVolume / 100f);
			comboDiscordInputVolume.Value = (origDiscordInputVolume = (float)settings.InputVolume / 100f);
			comboDiscordVoiceVadModeAuto.Value = (origDiscordVoiceVadModeAuto = settings.VoiceVadModeAuto);
			comboDiscordVoiceVadThreshold.Value = (origDiscordVoiceVadThreshold = settings.VoiceVadThreshold);
			DiscordOnAudioDevicesChanged(DiscordManager.Instance.AudioOutput);
			DiscordOnAudioDevicesChanged(DiscordManager.Instance.AudioInput);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onOpenDiscord()
	{
		DiscordManager instance = DiscordManager.Instance;
		instance.AudioDevicesChanged += DiscordOnAudioDevicesChanged;
		instance.LocalUserChanged += discordLocalUserChanged;
		instance.StatusChanged += discordStatusChanged;
		instance.VoiceStateChanged += discordVoiceStateChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onCloseDiscord()
	{
		DiscordManager instance = DiscordManager.Instance;
		DiscordManager.DiscordSettings settings = instance.Settings;
		settings.OutputVolume = Mathf.RoundToInt(origDiscordOutputVolume * 100f);
		settings.InputVolume = Mathf.RoundToInt(origDiscordInputVolume * 100f);
		settings.VoiceVadModeAuto = origDiscordVoiceVadModeAuto;
		settings.VoiceVadThreshold = origDiscordVoiceVadThreshold;
		instance.AudioDevicesChanged -= DiscordOnAudioDevicesChanged;
		instance.LocalUserChanged -= discordLocalUserChanged;
		instance.StatusChanged -= discordStatusChanged;
		instance.VoiceStateChanged -= discordVoiceStateChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DiscordOnAudioDevicesChanged(DiscordManager.AudioDeviceConfig _inOutConfig)
	{
		XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> xUiC_ComboBoxList = (_inOutConfig.IsOutput ? comboDiscordOutputDevice : comboDiscordInputDevice);
		if (xUiC_ComboBoxList == null)
		{
			return;
		}
		xUiC_ComboBoxList.Elements.Clear();
		xUiC_ComboBoxList.Elements.AddRange(_inOutConfig.CurrentAudioDevices.Values);
		int selectedIndex;
		if (xUiC_ComboBoxList.Elements.Count == 0)
		{
			xUiC_ComboBoxList.Elements.Add(noDeviceEntry);
			xUiC_ComboBoxList.SelectedIndex = 0;
		}
		else if (string.IsNullOrEmpty(_inOutConfig.ActiveAudioDevice) || (selectedIndex = xUiC_ComboBoxList.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (IPartyVoice.VoiceAudioDevice _device) => _device.Identifier == _inOutConfig.ActiveAudioDevice)) < 0)
		{
			int num = xUiC_ComboBoxList.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (IPartyVoice.VoiceAudioDevice _device) => _device.IsDefault);
			if (num < 0)
			{
				xUiC_ComboBoxList.Elements.Insert(0, defaultDeviceEntry);
				num = 0;
			}
			xUiC_ComboBoxList.SelectedIndex = num;
		}
		else
		{
			xUiC_ComboBoxList.SelectedIndex = selectedIndex;
		}
	}
}

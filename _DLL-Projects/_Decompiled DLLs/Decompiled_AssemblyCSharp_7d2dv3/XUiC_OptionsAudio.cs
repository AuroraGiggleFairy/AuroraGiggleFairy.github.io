using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsAudio : XUiC_OptionsDialogBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IPartyVoice.VoiceAudioDevice noDeviceEntry = new IPartyVoice.VoiceAudioDeviceNotFound();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IPartyVoice.VoiceAudioDevice defaultDeviceEntry = new IPartyVoice.VoiceAudioDeviceDefault();

	[XuiBindComponent("OptionVoiceOutputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionVoiceOutputDevice;

	[XuiBindComponent("VoiceOutputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboVoiceOutputDevice;

	[XuiBindComponent("OptionVoiceInputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionVoiceInputDevice;

	[XuiBindComponent("VoiceInputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboVoiceInputDevice;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastDeviceListUpdateFrame;

	public static string ID = "";

	public const string DiscordTabName = "xuiOptionsAudioDiscord";

	[XuiBindComponent("OptionDiscordOutputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionDiscordOutputDevice;

	[XuiBindComponent("DiscordOutputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboDiscordOutputDevice;

	[XuiBindComponent("OptionDiscordInputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionDiscordInputDevice;

	[XuiBindComponent("DiscordInputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboDiscordInputDevice;

	[XuiBindComponent("OptionDiscordVoiceButtonMode", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefBool optionDiscordVoiceButtonMode;

	[XuiBindComponent("btnDiscordInitialize", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordInitialize;

	[XuiBindComponent("btnDiscordLinkAccount", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordLinkAccount;

	[XuiBindComponent("btnDiscordUnlinkAccount", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordUnlinkAccount;

	[XuiBindComponent("btnDiscordManageSocialSettings", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordManageSocialSettings;

	[XuiBindComponent("btnDiscordManageBlockedUsers", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordManageBlockedUsers;

	[XuiXmlBinding("voiceavailable")]
	public bool VoiceAvailable
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

	[XuiXmlBinding("commsallowed")]
	public bool IsCommunicationAllowed
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PermissionsManager.IsCommunicationAllowed();
		}
	}

	[XuiXmlBinding("multiplayerallowed")]
	public bool MultiplayerAllowed => PermissionsManager.IsMultiplayerAllowed();

	[XuiXmlBinding("discord_ptt")]
	public bool DiscordVoiceButtonMode => optionDiscordVoiceButtonMode?.CurrentValue ?? false;

	[XuiXmlBinding("discord_enabled")]
	public bool DiscordEnabled => !DiscordManager.Instance.Settings.DiscordDisabled;

	[XuiXmlBinding("discordinitialized")]
	public bool DiscordInitialized => DiscordManager.Instance.IsInitialized;

	[XuiXmlBinding("discord_is_ready")]
	public bool DiscordReady => DiscordManager.Instance.IsReady;

	[XuiXmlBinding("discord_supports_full_accounts")]
	public bool DiscordSupportsFullAccounts => DiscordManager.SupportsFullAccounts;

	[XuiXmlBinding("discord_supports_provisional_accounts")]
	public bool DiscordSupportsProvisionalAccounts => DiscordManager.SupportsProvisionalAccounts;

	[XuiXmlBinding("discordaccountlinked")]
	public bool DiscordAccountLinked => !(DiscordManager.Instance.LocalUser?.IsProvisionalAccount ?? false);

	[XuiXmlBinding("discord_vad_threshold_manual")]
	public bool DiscordVoiceVadThresholdManual => !DiscordManager.Instance.Settings.VoiceVadModeAuto;

	[XuiXmlBinding("discord_in_call")]
	public bool DiscordInCall => DiscordManager.Instance.ActiveVoiceLobby != null;

	[XuiXmlBinding("discord_is_speaking")]
	public bool DiscordIsSpeaking => DiscordManager.Instance.ActiveVoiceLobby?.VoiceCall.IsSpeaking ?? false;

	public static event Action OnSettingsChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateVoiceDeviceList(XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> _combo, IEnumerable<IPartyVoice.VoiceAudioDevice> _devices)
	{
		if (_combo != null)
		{
			_combo.Elements.Clear();
			_combo.Elements.AddRange(_devices);
			if (_combo.Elements.Count == 0)
			{
				_combo.Elements.Add(noDeviceEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateVoiceDeviceSelection(XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> _combo, string _activeDeviceIdentifier)
	{
		if (_combo == null)
		{
			return;
		}
		int selectedIndex;
		if (_combo.Elements.Count == 1)
		{
			_combo.SelectedIndex = 0;
		}
		else if (string.IsNullOrEmpty(_activeDeviceIdentifier) || (selectedIndex = _combo.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (IPartyVoice.VoiceAudioDevice _device) => _device.Identifier == _activeDeviceIdentifier)) < 0)
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void initEosVoiceDeviceOptions()
	{
		if (optionVoiceOutputDevice == null || comboVoiceOutputDevice == null || optionVoiceInputDevice == null || comboVoiceInputDevice == null)
		{
			return;
		}
		optionVoiceOutputDevice.GetSettingValue = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			UpdateVoiceDeviceLists();
			updateVoiceDeviceSelection(comboVoiceOutputDevice, GamePrefs.GetString(EnumGamePrefs.OptionsVoiceOutputDevice));
		};
		optionVoiceOutputDevice.DiscardChanges = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			UpdateVoiceDeviceLists();
			updateVoiceDeviceSelection(comboVoiceOutputDevice, GamePrefs.GetString(EnumGamePrefs.OptionsVoiceOutputDevice));
		};
		optionVoiceOutputDevice.ApplyChanges = [PublicizedFrom(EAccessModifier.Private)] (bool _) =>
		{
			if (VoiceAvailable)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsVoiceOutputDevice, comboVoiceOutputDevice.Value.Identifier);
			}
		};
		optionVoiceOutputDevice.ResetDefaults = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			updateVoiceDeviceSelection(comboVoiceOutputDevice, (string)GamePrefs.GetDefault(EnumGamePrefs.OptionsVoiceOutputDevice));
		};
		optionVoiceOutputDevice.IsChangedDelegate = [PublicizedFrom(EAccessModifier.Private)] () => !GamePrefs.GetString(EnumGamePrefs.OptionsVoiceOutputDevice).Equals(comboVoiceOutputDevice.Value?.Identifier);
		optionVoiceOutputDevice.IsDefaultDelegate = [PublicizedFrom(EAccessModifier.Private)] () => ((string)GamePrefs.GetDefault(EnumGamePrefs.OptionsVoiceOutputDevice)).Equals(comboVoiceOutputDevice.Value?.Identifier);
		optionVoiceInputDevice.GetSettingValue = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			UpdateVoiceDeviceLists();
			updateVoiceDeviceSelection(comboVoiceInputDevice, GamePrefs.GetString(EnumGamePrefs.OptionsVoiceInputDevice));
		};
		optionVoiceInputDevice.DiscardChanges = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			UpdateVoiceDeviceLists();
			updateVoiceDeviceSelection(comboVoiceInputDevice, GamePrefs.GetString(EnumGamePrefs.OptionsVoiceInputDevice));
		};
		optionVoiceInputDevice.ApplyChanges = [PublicizedFrom(EAccessModifier.Private)] (bool _) =>
		{
			if (VoiceAvailable)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsVoiceInputDevice, comboVoiceInputDevice.Value.Identifier);
			}
		};
		optionVoiceInputDevice.ResetDefaults = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			updateVoiceDeviceSelection(comboVoiceInputDevice, (string)GamePrefs.GetDefault(EnumGamePrefs.OptionsVoiceInputDevice));
		};
		optionVoiceInputDevice.IsChangedDelegate = [PublicizedFrom(EAccessModifier.Private)] () => !GamePrefs.GetString(EnumGamePrefs.OptionsVoiceInputDevice).Equals(comboVoiceInputDevice.Value?.Identifier);
		optionVoiceInputDevice.IsDefaultDelegate = [PublicizedFrom(EAccessModifier.Private)] () => ((string)GamePrefs.GetDefault(EnumGamePrefs.OptionsVoiceInputDevice)).Equals(comboVoiceInputDevice.Value?.Identifier);
		[PublicizedFrom(EAccessModifier.Private)]
		void UpdateVoiceDeviceLists()
		{
			int frameCount = Time.frameCount;
			if (lastDeviceListUpdateFrame != frameCount)
			{
				lastDeviceListUpdateFrame = frameCount;
				if (VoiceAvailable && IsCommunicationAllowed)
				{
					var (devices, devices2) = PlatformManager.MultiPlatform.PartyVoice.GetDevicesList();
					updateVoiceDeviceList(comboVoiceOutputDevice, devices2);
					updateVoiceDeviceList(comboVoiceInputDevice, devices);
				}
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		initEosVoiceDeviceOptions();
		initDiscordVoiceDeviceOptions();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		applyDiscordDisabledChanged();
		XUiC_OptionsAudio.OnSettingsChanged?.Invoke();
	}

	public override void OnOpen()
	{
		onOpenDiscord();
		base.OnOpen();
	}

	public override void OnClose()
	{
		base.OnClose();
		onCloseDiscord();
	}

	public void SetActiveTab(string _tabKey)
	{
		TabSelector?.SelectTabByName(_tabKey);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initDiscordVoiceDeviceOptions()
	{
		if (optionDiscordOutputDevice == null || comboDiscordOutputDevice == null || optionDiscordInputDevice == null || comboDiscordInputDevice == null)
		{
			return;
		}
		optionDiscordOutputDevice.GetSettingValue = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (IsCommunicationAllowed)
			{
				updateVoiceDeviceList(comboDiscordOutputDevice, DiscordManager.Instance.AudioOutput.CurrentAudioDevices.Values);
				updateVoiceDeviceSelection(comboDiscordOutputDevice, DiscordManager.Instance.AudioOutput.ActiveAudioDevice);
			}
		};
		optionDiscordOutputDevice.DiscardChanges = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			updateVoiceDeviceList(comboDiscordOutputDevice, DiscordManager.Instance.AudioOutput.CurrentAudioDevices.Values);
			updateVoiceDeviceSelection(comboDiscordOutputDevice, DiscordManager.Instance.AudioOutput.ActiveAudioDevice);
		};
		optionDiscordOutputDevice.ApplyChanges = [PublicizedFrom(EAccessModifier.Private)] (bool _) =>
		{
			if (VoiceAvailable)
			{
				DiscordManager.Instance.Settings.SelectedOutputDevice = comboDiscordOutputDevice.Value.Identifier;
			}
		};
		optionDiscordOutputDevice.ResetDefaults = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			updateVoiceDeviceSelection(comboDiscordOutputDevice, (string)GamePrefs.GetDefault(EnumGamePrefs.DiscordSelectedOutputDevice));
		};
		optionDiscordOutputDevice.IsChangedDelegate = [PublicizedFrom(EAccessModifier.Private)] () => !GamePrefs.GetString(EnumGamePrefs.DiscordSelectedOutputDevice).Equals(comboDiscordOutputDevice.Value?.Identifier);
		optionDiscordOutputDevice.IsDefaultDelegate = [PublicizedFrom(EAccessModifier.Private)] () => ((string)GamePrefs.GetDefault(EnumGamePrefs.DiscordSelectedOutputDevice)).Equals(comboDiscordOutputDevice.Value?.Identifier);
		optionDiscordInputDevice.GetSettingValue = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (IsCommunicationAllowed)
			{
				updateVoiceDeviceList(comboDiscordInputDevice, DiscordManager.Instance.AudioInput.CurrentAudioDevices.Values);
				updateVoiceDeviceSelection(comboDiscordInputDevice, DiscordManager.Instance.AudioInput.ActiveAudioDevice);
			}
		};
		optionDiscordInputDevice.DiscardChanges = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			updateVoiceDeviceList(comboDiscordInputDevice, DiscordManager.Instance.AudioInput.CurrentAudioDevices.Values);
			updateVoiceDeviceSelection(comboDiscordInputDevice, DiscordManager.Instance.AudioInput.ActiveAudioDevice);
		};
		optionDiscordInputDevice.ApplyChanges = [PublicizedFrom(EAccessModifier.Private)] (bool _) =>
		{
			if (VoiceAvailable)
			{
				DiscordManager.Instance.Settings.SelectedInputDevice = comboDiscordInputDevice.Value.Identifier;
			}
		};
		optionDiscordInputDevice.ResetDefaults = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			updateVoiceDeviceSelection(comboDiscordInputDevice, (string)GamePrefs.GetDefault(EnumGamePrefs.DiscordSelectedInputDevice));
		};
		optionDiscordInputDevice.IsChangedDelegate = [PublicizedFrom(EAccessModifier.Private)] () => !GamePrefs.GetString(EnumGamePrefs.DiscordSelectedInputDevice).Equals(comboDiscordInputDevice.Value?.Identifier);
		optionDiscordInputDevice.IsDefaultDelegate = [PublicizedFrom(EAccessModifier.Private)] () => ((string)GamePrefs.GetDefault(EnumGamePrefs.DiscordSelectedInputDevice)).Equals(comboDiscordInputDevice.Value?.Identifier);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordVoiceDeviceListChanged(DiscordManager.AudioDeviceConfig _inOutConfig)
	{
		XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> combo = (_inOutConfig.IsOutput ? comboDiscordOutputDevice : comboDiscordInputDevice);
		updateVoiceDeviceList(combo, _inOutConfig.CurrentAudioDevices.Values);
		updateVoiceDeviceSelection(combo, _inOutConfig.ActiveAudioDevice);
	}

	[XuiBindEvent("OnPress", "btnDiscordInitialize")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordInitializeOnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_DiscordLogin.Open([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			OpenAtTab("xuiOptionsAudioDiscord");
		}, _showSettingsButton: false);
		DiscordManager.Instance.AuthManager.AutoLogin();
	}

	[XuiBindEvent("OnPress", "btnDiscordLinkAccount")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordLinkAccountOnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_DiscordLogin.Open([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			OpenAtTab("xuiOptionsAudioDiscord");
		}, _showSettingsButton: false, _waitForResultToShow: false, _skipOnSuccess: false, _modal: true, (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent());
		DiscordManager.Instance.AuthManager.LoginDiscordUser();
	}

	[XuiBindEvent("OnPress", "btnDiscordUnlinkAccount")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordUnlinkAccountOnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_DiscordLogin.Open([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			OpenAtTab("xuiOptionsAudioDiscord");
		}, _showSettingsButton: false, _waitForResultToShow: false, _skipOnSuccess: false, _modal: true, (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent());
		DiscordManager.Instance.AuthManager.UnmergeAccount();
	}

	[XuiBindEvent("OnPress", "btnDiscordManageSocialSettings")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordManageSocialSettingsOnPress(XUiController _sender, int _mouseButton)
	{
		DiscordManager.Instance.OpenDiscordSocialSettings();
	}

	[XuiBindEvent("OnPress", "btnDiscordManageBlockedUsers")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordManageBlockedUsersOnPress(XUiController _sender, int _mouseButton)
	{
		GUIWindow window = xui.playerUI.windowManager.GetWindow(XUiC_DiscordBlockedUsers.ID);
		xui.playerUI.windowManager.Open(window, _bModal: false);
	}

	public void OpenAtTab(string _tabName)
	{
		xui.playerUI.windowManager.Open(windowGroup, _bModal: true);
		SetActiveTab(_tabName);
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
	public void applyDiscordDisabledChanged()
	{
		bool isInitialized = DiscordManager.Instance.IsInitialized;
		bool flag = !DiscordManager.Instance.Settings.DiscordDisabled;
		if (isInitialized != flag)
		{
			if (!flag)
			{
				XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiOptionsAudioDiscordDisableRestartTitle"), Localization.Get("xuiOptionsAudioDiscordDisableRestartText"), null, _openMainMenuOnClose: false, _modal: false);
			}
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onOpenDiscord()
	{
		DiscordManager instance = DiscordManager.Instance;
		instance.AudioDevicesChanged += discordVoiceDeviceListChanged;
		instance.LocalUserChanged += discordLocalUserChanged;
		instance.StatusChanged += discordStatusChanged;
		instance.VoiceStateChanged += discordVoiceStateChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onCloseDiscord()
	{
		DiscordManager instance = DiscordManager.Instance;
		instance.AudioDevicesChanged -= discordVoiceDeviceListChanged;
		instance.LocalUserChanged -= discordLocalUserChanged;
		instance.StatusChanged -= discordStatusChanged;
		instance.VoiceStateChanged -= discordVoiceStateChanged;
	}
}

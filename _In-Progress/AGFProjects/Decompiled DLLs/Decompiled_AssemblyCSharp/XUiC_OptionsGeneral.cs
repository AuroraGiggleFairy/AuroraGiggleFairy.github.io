using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsGeneral : XUiController
{
	public struct LanguageInfo : IComparable<LanguageInfo>, IEquatable<LanguageInfo>, IComparable
	{
		public readonly string NameEnglish;

		public readonly string NameNative;

		public readonly string LanguageKey;

		public LanguageInfo(string _languageKey)
		{
			LanguageKey = _languageKey;
			if (_languageKey == "")
			{
				NameEnglish = null;
				NameNative = null;
			}
			else
			{
				NameEnglish = Localization.Get("languageNameEnglish", _languageKey);
				NameNative = Localization.Get("languageNameNative", _languageKey);
			}
		}

		public override string ToString()
		{
			if (!(LanguageKey == ""))
			{
				return NameEnglish + " / " + NameNative;
			}
			return "-Auto-";
		}

		public bool Equals(LanguageInfo _other)
		{
			return LanguageKey == _other.LanguageKey;
		}

		public override bool Equals(object _obj)
		{
			if (_obj is LanguageInfo other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (LanguageKey == null)
			{
				return 0;
			}
			return LanguageKey.GetHashCode();
		}

		public int CompareTo(LanguageInfo _other)
		{
			return string.Compare(NameEnglish, _other.NameEnglish, StringComparison.OrdinalIgnoreCase);
		}

		public int CompareTo(object _obj)
		{
			if (_obj == null)
			{
				return 1;
			}
			if (!(_obj is LanguageInfo other))
			{
				throw new ArgumentException("Object must be of type LanguageInfo");
			}
			return CompareTo(other);
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<LanguageInfo> comboLanguage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboUseEnglishCompass;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboTempCelsius;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDisableXmlEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboQuestsAutoShare;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboQuestsAutoAccept;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAutoParty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboTxtChat;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboCrossplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboShowConsoleButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public EUserPerms otherPerms;

	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		comboLanguage = GetChildById("Language").GetChildByType<XUiC_ComboBoxList<LanguageInfo>>();
		comboUseEnglishCompass = GetChildById("UseEnglishCompass").GetChildByType<XUiC_ComboBoxBool>();
		comboTempCelsius = GetChildById("TempCelsius").GetChildByType<XUiC_ComboBoxBool>();
		comboDisableXmlEvents = GetChildById("DisableXmlEvents").GetChildByType<XUiC_ComboBoxBool>();
		comboQuestsAutoShare = GetChildById("QuestsAutoShare").GetChildByType<XUiC_ComboBoxBool>();
		comboQuestsAutoAccept = GetChildById("QuestsAutoAccept").GetChildByType<XUiC_ComboBoxBool>();
		comboAutoParty = GetChildById("AutoParty").GetChildByType<XUiC_ComboBoxBool>();
		comboTxtChat = GetChildById("ChatComms").GetChildByType<XUiC_ComboBoxBool>();
		comboCrossplay = GetChildById("Crossplay").GetChildByType<XUiC_ComboBoxBool>();
		comboShowConsoleButton = GetChildById("ShowConsoleButton").GetChildByType<XUiC_ComboBoxBool>();
		comboLanguage.OnValueChangedGeneric += anyOtherValueChanged;
		comboUseEnglishCompass.OnValueChangedGeneric += anyOtherValueChanged;
		comboTempCelsius.OnValueChangedGeneric += anyOtherValueChanged;
		comboDisableXmlEvents.OnValueChangedGeneric += anyOtherValueChanged;
		comboQuestsAutoShare.OnValueChangedGeneric += anyOtherValueChanged;
		comboQuestsAutoAccept.OnValueChangedGeneric += anyOtherValueChanged;
		comboAutoParty.OnValueChangedGeneric += anyOtherValueChanged;
		comboTxtChat.OnValueChangedGeneric += anyOtherValueChanged;
		comboCrossplay.OnValueChangedGeneric += anyOtherValueChanged;
		comboShowConsoleButton.OnValueChangedGeneric += anyOtherValueChanged;
		((XUiC_SimpleButton)GetChildById("btnEula")).OnPressed += BtnEula_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnBugReport")).OnPressed += BtnBugReport_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnBack")).OnPressed += BtnBack_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnDefaults")).OnPressed += BtnDefaults_OnOnPressed;
		btnApply = (XUiC_SimpleButton)GetChildById("btnApply");
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
	public void anyOtherValueChanged(XUiController _sender)
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
		comboLanguage.SelectedIndex = 0;
		comboUseEnglishCompass.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections);
		comboTempCelsius.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsTempCelsius);
		comboDisableXmlEvents.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsDisableXmlEvents);
		comboQuestsAutoShare.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsQuestsAutoShare);
		comboQuestsAutoAccept.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsQuestsAutoAccept);
		comboShowConsoleButton.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsShowConsoleButton);
		comboAutoParty.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsAutoPartyWithFriends);
		comboTxtChat.Value = otherPerms.HasCommunication() && (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsChatCommunication);
		comboCrossplay.Value = otherPerms.HasCrossplay() && (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsCrossplay);
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEula_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_EulaWindow.Open(base.xui, GameManager.HasAcceptedLatestEula());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBugReport_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_BugReportWindow.Open(base.xui, _fromMainMenu: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLanguageList()
	{
		comboLanguage.Elements.Clear();
		bool flag = false;
		string[] knownLanguages = Localization.knownLanguages;
		foreach (string text in knownLanguages)
		{
			if ((flag || (flag = text.EqualsCaseInsensitive(Localization.DefaultLanguage))) && text.IndexOf(' ') < 0)
			{
				comboLanguage.Elements.Add(new LanguageInfo(text));
			}
		}
		comboLanguage.Elements.Sort();
		comboLanguage.Elements.Insert(0, new LanguageInfo(""));
		string b = GamePrefs.GetString(EnumGamePrefs.Language);
		for (int j = 0; j < comboLanguage.Elements.Count; j++)
		{
			if (comboLanguage.Elements[j].LanguageKey.EqualsCaseInsensitive(b))
			{
				comboLanguage.SelectedIndex = j;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		bool num = comboLanguage.Value.LanguageKey != GamePrefs.GetString(EnumGamePrefs.Language);
		if (num)
		{
			Log.Out("Language selection changed: " + comboLanguage.Value.LanguageKey);
			GamePrefs.Set(EnumGamePrefs.Language, comboLanguage.Value.LanguageKey);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections, comboUseEnglishCompass.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsTempCelsius, comboTempCelsius.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsDisableXmlEvents, comboDisableXmlEvents.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsQuestsAutoShare, comboQuestsAutoShare.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsQuestsAutoAccept, comboQuestsAutoAccept.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsShowConsoleButton, comboShowConsoleButton.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsAutoPartyWithFriends, comboAutoParty.Value);
		if (otherPerms.HasCommunication())
		{
			GamePrefs.Set(EnumGamePrefs.OptionsChatCommunication, comboTxtChat.Value);
		}
		if (otherPerms.HasCrossplay())
		{
			GamePrefs.Set(EnumGamePrefs.OptionsCrossplay, comboCrossplay.Value);
		}
		GamePrefs.Instance.Save();
		XUiC_OptionsGeneral.OnSettingsChanged?.Invoke();
		if (!num)
		{
			return;
		}
		if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
		{
			XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiConfirmRestartLanguageTitle"), Localization.Get("xuiConfirmRestartLanguageText"), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				Utils.RestartGame();
			}, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				base.xui.playerUI.windowManager.Open(ID, _bModal: true);
			}, _openMainMenuOnClose: false);
		}
		else if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiLanguageChangedTitle"), Localization.Get("xuiLanguageChangedText"), XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, null, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				base.xui.playerUI.windowManager.Open(ID, _bModal: true);
			}, _openMainMenuOnClose: false);
		}
	}

	public override void OnOpen()
	{
		otherPerms = PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.Platform | PermissionsManager.PermissionSources.LaunchPrefs | PermissionsManager.PermissionSources.DebugMask | PermissionsManager.PermissionSources.TitleStorage);
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		updateLanguageList();
		comboUseEnglishCompass.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections);
		comboTempCelsius.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsTempCelsius);
		comboDisableXmlEvents.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsDisableXmlEvents);
		comboQuestsAutoShare.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsQuestsAutoShare);
		comboQuestsAutoAccept.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsQuestsAutoAccept);
		comboShowConsoleButton.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsShowConsoleButton);
		comboAutoParty.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsAutoPartyWithFriends);
		comboTxtChat.Value = otherPerms.HasCommunication() && GamePrefs.GetBool(EnumGamePrefs.OptionsChatCommunication);
		comboTxtChat.Enabled = otherPerms.HasCommunication();
		comboCrossplay.Value = otherPerms.HasCrossplay() && GamePrefs.GetBool(EnumGamePrefs.OptionsCrossplay);
		comboCrossplay.Enabled = otherPerms.HasCrossplay();
		base.OnOpen();
		btnApply.Enabled = false;
		RefreshBindings();
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
		if (!(_bindingName == "crossplayTooltip"))
		{
			if (_bindingName == "bug_reporting")
			{
				_value = BacktraceUtils.BugReportFeature.ToString();
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay, PermissionsManager.PermissionSources.Platform | PermissionsManager.PermissionSources.LaunchPrefs | PermissionsManager.PermissionSources.DebugMask | PermissionsManager.PermissionSources.TitleStorage) ?? Localization.Get("xuiOptionsGeneralCrossplayTooltip");
		return true;
	}
}

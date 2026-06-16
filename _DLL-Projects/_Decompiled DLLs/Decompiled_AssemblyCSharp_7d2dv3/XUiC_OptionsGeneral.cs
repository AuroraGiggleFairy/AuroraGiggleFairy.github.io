using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsGeneral : XUiC_OptionsDialogBase
{
	public readonly struct LanguageInfo : IComparable<LanguageInfo>, IEquatable<LanguageInfo>, IComparable
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
				NameEnglish = Localization.Get("languageNameEnglish", _caseInsensitive: false, _languageKey);
				NameNative = Localization.Get("languageNameNative", _caseInsensitive: false, _languageKey);
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

	[XuiBindComponent("OptionLanguage", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionLanguage;

	[XuiBindComponent("Language", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<LanguageInfo> comboLanguage;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool languageChanged;

	[XuiBindComponent("CrosshairsColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxInt comboCrosshairsColor;

	[XuiBindComponent("CrosshairsSample", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite crosshairSample;

	[XuiBindComponent("rangedCrosshairSample", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView crosshairRangedSample;

	[PublicizedFrom(EAccessModifier.Private)]
	public CrosshairDrawer crosshairDrawer;

	[XuiBindComponent("OptionChatComms", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefBool optionChatComms;

	[XuiBindComponent("OptionCrossplay", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefBool optionCrossplay;

	public static string ID = "";

	[XuiBindComponent("btnEula", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnEula;

	[XuiBindComponent("btnBugReport", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBugReport;

	[XuiXmlBinding("crosshairs_enabled")]
	public bool CrosshairsEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GamePrefs.GetBool(EnumGamePrefs.OptionsCrosshairEnabled);
		}
	}

	public static EUserPerms UserPermissions
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.Platform | PermissionsManager.PermissionSources.LaunchPrefs | PermissionsManager.PermissionSources.DebugMask | PermissionsManager.PermissionSources.TitleStorage);
		}
	}

	[XuiXmlBinding("has_communication_perm")]
	public bool HasCommunicationPermission
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return UserPermissions.HasCommunication();
		}
	}

	[XuiXmlBinding("has_crossplay_perm")]
	public bool HasCrossplayPermission
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return UserPermissions.HasCrossplay();
		}
	}

	[XuiXmlBinding("crossplayTooltip")]
	public string CrossplayTooltip
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay, PermissionsManager.PermissionSources.Platform | PermissionsManager.PermissionSources.LaunchPrefs | PermissionsManager.PermissionSources.DebugMask | PermissionsManager.PermissionSources.TitleStorage) ?? Localization.Get("xuiOptionsGeneralCrossplayTooltip");
		}
	}

	[XuiXmlBinding("bug_reporting")]
	public bool SupportsBugReporting
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return BacktraceUtils.BugReportFeature;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initLanguageOptions()
	{
		if (optionLanguage == null || comboLanguage == null)
		{
			return;
		}
		optionLanguage.GetSettingValue = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			comboLanguage.Elements.Clear();
			bool flag = false;
			string[] knownLanguages = Localization.KnownLanguages;
			foreach (string text in knownLanguages)
			{
				if ((flag || (flag = text.EqualsCaseInsensitive("english"))) && text.IndexOf(' ') < 0)
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
		};
		optionLanguage.DiscardChanges = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			string b = GamePrefs.GetString(EnumGamePrefs.Language);
			for (int i = 0; i < comboLanguage.Elements.Count; i++)
			{
				if (comboLanguage.Elements[i].LanguageKey.EqualsCaseInsensitive(b))
				{
					comboLanguage.SelectedIndex = i;
				}
			}
		};
		optionLanguage.ApplyChanges = [PublicizedFrom(EAccessModifier.Private)] (bool _) =>
		{
			languageChanged = comboLanguage.Value.LanguageKey != GamePrefs.GetString(EnumGamePrefs.Language);
			IsDirty |= languageChanged;
			GamePrefs.Set(EnumGamePrefs.Language, comboLanguage.Value.LanguageKey);
		};
		optionLanguage.ResetDefaults = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			comboLanguage.SelectedIndex = 0;
		};
		optionLanguage.IsChangedDelegate = [PublicizedFrom(EAccessModifier.Private)] () => comboLanguage.Value.LanguageKey != GamePrefs.GetString(EnumGamePrefs.Language);
		optionLanguage.IsDefaultDelegate = [PublicizedFrom(EAccessModifier.Private)] () => comboLanguage.Value.LanguageKey.EqualsCaseInsensitive((string)GamePrefs.GetDefault(EnumGamePrefs.Language));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void languageSavedOptions()
	{
		if (!languageChanged)
		{
			return;
		}
		languageChanged = false;
		Log.Out("Language selection changed: " + GamePrefs.GetString(EnumGamePrefs.Language));
		if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
		{
			XUiC_MessageBoxWindowGroup.ShowOkCancel(xui, Localization.Get("xuiConfirmRestartLanguageTitle"), Localization.Get("xuiConfirmRestartLanguageText"), [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				Utils.RestartGame();
			}, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				xui.playerUI.windowManager.Open(windowGroup, _bModal: true);
			}, _openMainMenuOnClose: false);
		}
		else if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiLanguageChangedTitle"), Localization.Get("xuiLanguageChangedText"), [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				xui.playerUI.windowManager.Open(windowGroup, _bModal: true);
			}, _openMainMenuOnClose: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initCrosshairOptions()
	{
		comboCrosshairsColor.Min = 0L;
		comboCrosshairsColor.Max = EntityPlayerLocal.crosshairColors.Count - 1;
		crosshairDrawer = viewComponent.UiTransform.gameObject.AddComponent<CrosshairDrawer>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if ((uint)(_pref - 286) <= 3u || (uint)(_pref - 292) <= 1u)
		{
			updateCrosshairElements();
		}
	}

	[XuiBindEvent("OnTabChanged", "TabSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public new void OnTabChanged(int _tabIndex, XUiC_TabSelectorTab _tab)
	{
		updateCrosshairElements();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRangedCrosshairDrwaing()
	{
		bool flag = GamePrefs.GetBool(EnumGamePrefs.OptionsCrosshairEnabled);
		bool flag2 = GamePrefs.GetBool(EnumGamePrefs.OptionsCrosshairRangedEnabled);
		crosshairDrawer.draw = TabSelector.SelectedTabIndex == 1 && flag && flag2 && !XUiC_FullScreenCollider.IsBlocked(viewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCrosshairPosition()
	{
		Vector3 vector = xui.playerUI.uiCamera.cachedCamera.WorldToViewportPoint(crosshairRangedSample.UiTransform.position);
		vector.x *= Screen.width;
		vector.y *= Screen.height;
		vector.y = (float)Screen.height - vector.y;
		crosshairDrawer.centerX = vector.x;
		crosshairDrawer.centerY = vector.y;
		updateRangedCrosshairDrwaing();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCrosshairElements()
	{
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsCrosshairScale);
		float thickness = GamePrefs.GetFloat(EnumGamePrefs.OptionsCrosshairThickness);
		float num2 = GamePrefs.GetFloat(EnumGamePrefs.OptionsCrosshairOpacity);
		int index = GamePrefs.GetInt(EnumGamePrefs.OptionsCrosshairColor);
		Color color = EntityPlayerLocal.crosshairColors[index];
		color.a = num2;
		crosshairSample.Color = color;
		crosshairSample.Size = new Vector2i((int)(num * 100f), (int)(num * 100f));
		updateRangedCrosshairDrwaing();
		if (crosshairDrawer.draw)
		{
			crosshairDrawer.thickness = thickness;
			crosshairDrawer.length = num * (float)Screen.height * 0.025f;
			crosshairDrawer.openAreaX = (crosshairDrawer.openAreaY = (float)Screen.height * 0.02f);
			crosshairDrawer.opacity = num2;
			crosshairDrawer.color = color;
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initPermissionsBasedOptions()
	{
		if (optionChatComms != null)
		{
			optionChatComms.OverrideSettingValue = [PublicizedFrom(EAccessModifier.Internal)] (bool _, out bool _overrideValue) =>
			{
				_overrideValue = false;
				return !UserPermissions.HasCommunication();
			};
			optionChatComms.DoSaveOverride = [PublicizedFrom(EAccessModifier.Internal)] () => UserPermissions.HasCommunication();
		}
		if (optionCrossplay != null)
		{
			optionCrossplay.OverrideSettingValue = [PublicizedFrom(EAccessModifier.Internal)] (bool _, out bool _overrideValue) =>
			{
				_overrideValue = false;
				return !UserPermissions.HasCrossplay();
			};
			optionCrossplay.DoSaveOverride = [PublicizedFrom(EAccessModifier.Internal)] () => UserPermissions.HasCrossplay();
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		initLanguageOptions();
		initCrosshairOptions();
		initPermissionsBasedOptions();
	}

	[XuiBindEvent("OnPress", "btnEula")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEula_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_EulaWindow.Open(xui, GameManager.HasAcceptedLatestEula());
	}

	[XuiBindEvent("OnPress", "btnBugReport")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBugReport_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_BugReportWindow.Open();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		languageSavedOptions();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		updateCrosshairPosition();
	}

	public override void OnOpen()
	{
		updateCrosshairElements();
		base.OnOpen();
	}
}

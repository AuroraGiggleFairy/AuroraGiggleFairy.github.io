using System;
using System.Collections.Generic;
using Platform;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewContinueGameSettings : XUiController
{
	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TabSelector tabSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumGamePrefs, XUiC_GamePrefSelector> gameOptions = new EnumDictionary<EnumGamePrefs, XUiC_GamePrefSelector>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_GamePrefSelector serverEnabledSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool portValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_GamePrefSelector> tabPrefsTmp = new List<XUiC_GamePrefSelector>();

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SandboxPresetSelector sandboxPresetSelector;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SandboxSettingsDisplay sandboxDisplay;

	[XuiBindComponent("btnSandboxOptions", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSandboxOptions;

	[XuiBindComponent("sandboxPresetValues.", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollView sandboxPresetValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SandboxOptionManager sandboxManager = SandboxOptionManager.Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxValuesTemplate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxValuesTemplateDefaults;

	[XuiXmlBinding("crossplayTooltip")]
	public string CrossplayTooltip
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			string permissionDenyReason = PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay);
			if (!string.IsNullOrEmpty(permissionDenyReason))
			{
				return permissionDenyReason;
			}
			if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent() && gameOptions.Count > 0)
			{
				gameOptions[EnumGamePrefs.ServerEACPeerToPeer].CheckDefaultValue();
				return string.Format(Localization.Get("xuiOptionsGeneralCrossplayTooltipPC"), 8);
			}
			return Localization.Get("xuiOptionsGeneralCrossplayTooltip");
		}
	}

	[XuiXmlBinding("port_valid")]
	public bool PortValid
	{
		get
		{
			return portValid;
		}
		set
		{
			if (portValid != value)
			{
				portValid = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("sandbox_values_columns", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int SandboxValuesColumns
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("sandbox_values_template", false)]
	public string SandBoxValuesTemplate
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return sandBoxValuesTemplate;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == null)
			{
				sandBoxValuesTemplate = "";
			}
			else
			{
				sandBoxValuesTemplate = value.Replace("%0", "{0}").Replace("%1", "{1}");
			}
		}
	}

	[XuiXmlAttribute("sandbox_values_template_defaults", false)]
	public string SandBoxValuesTemplateDefaults
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return sandBoxValuesTemplateDefaults;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == null)
			{
				sandBoxValuesTemplateDefaults = "";
			}
			else
			{
				sandBoxValuesTemplateDefaults = value.Replace("%0", "{0}").Replace("%1", "{1}");
			}
		}
	}

	[XuiXmlBinding("sandboxvalues")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, string> SandboxValues
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
	} = new Dictionary<string, string>();

	public event Action SettingsChanged;

	public event Action OpenSandboxSettingsRequested;

	public override void Init()
	{
		base.Init();
		XUiC_GamePrefSelector[] childControllers = GetChildControllers<XUiC_GamePrefSelector>("");
		foreach (XUiC_GamePrefSelector xUiC_GamePrefSelector in childControllers)
		{
			xUiC_GamePrefSelector.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xUiC_GamePrefSelector.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(Option_OnValueChanged));
			EnumGamePrefs gamePref = xUiC_GamePrefSelector.GamePref;
			switch (gamePref)
			{
			case EnumGamePrefs.ServerEnabled:
				serverEnabledSelector = xUiC_GamePrefSelector;
				continue;
			case EnumGamePrefs.BloodMoonFrequency:
				xUiC_GamePrefSelector.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xUiC_GamePrefSelector.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(CmbBloodMoonFrequency_OnChangeHandler));
				break;
			case EnumGamePrefs.AirDropFrequency:
				xUiC_GamePrefSelector.ValuePreDisplayModifierFunc = [PublicizedFrom(EAccessModifier.Internal)] (int _n) => _n / 24;
				break;
			case EnumGamePrefs.ServerPort:
				xUiC_GamePrefSelector.ControlText.UIInput.validation = UIInput.Validation.Integer;
				xUiC_GamePrefSelector.ControlText.OnChangeHandler += TxtPort_OnChangeHandler;
				break;
			case EnumGamePrefs.ServerEACPeerToPeer:
				xUiC_GamePrefSelector.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xUiC_GamePrefSelector.OnValueChanged, (Action<XUiC_GamePrefSelector, EnumGamePrefs>)([PublicizedFrom(EAccessModifier.Private)] (XUiC_GamePrefSelector _, EnumGamePrefs _) =>
				{
					RefreshMultiplayerOptionStates(GamePrefs.GetBool(EnumGamePrefs.ServerEnabled));
				}));
				break;
			}
			gameOptions.Add(gamePref, xUiC_GamePrefSelector);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty && tabSelector != null)
		{
			XUiC_TabSelectorTab[] tabs = tabSelector.Tabs;
			foreach (XUiC_TabSelectorTab xUiC_TabSelectorTab in tabs)
			{
				xUiC_TabSelectorTab.GetChildControllers("", tabPrefsTmp);
				bool flag = false;
				foreach (XUiC_GamePrefSelector item in tabPrefsTmp)
				{
					flag |= item.ViewComponent.IsVisible && item.Enabled && !item.IsDefaultValueForGameMode();
				}
				xUiC_TabSelectorTab.TabHighlight = flag;
				tabPrefsTmp.Clear();
			}
		}
		if (handleDirtyUpdateDefault())
		{
			ThreadManager.RunTaskAfterFrames(sandboxPresetValues.ResetPosition, 3);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Option_OnValueChanged(XUiC_GamePrefSelector _uiElement, EnumGamePrefs _gamePref)
	{
		IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnSandboxOptions")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSandboxOptions_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.OpenSandboxSettingsRequested?.Invoke();
	}

	public void ResetSandboxPresetToDefault()
	{
		sandboxPresetSelector.SelectPresetByName("");
	}

	public void UpdateSandboxPresetGroups()
	{
		string text = GamePrefs.GetString(EnumGamePrefs.SandboxPreset);
		if (text == "Custom" || sandboxManager.GetPreset(text) == null)
		{
			sandboxManager.LoadOptionsFromCode(GamePrefs.GetString(EnumGamePrefs.SandboxCode), SandboxOptionManager.CustomPreset);
			sandboxPresetSelector.SelectPresetByName("Custom");
		}
		else
		{
			sandboxPresetSelector.SelectPresetByName(text);
		}
		updateSandboxData();
	}

	[XuiBindEvent("SandboxPresetSelectionChanged", "sandboxPresetSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void SandboxPresetSelector_OnValueChanged(SandboxPresetInfo _oldPreset, SandboxPresetInfo _newPreset)
	{
		updateSandboxData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSandboxData()
	{
		SandboxPresetInfo selectedPreset = sandboxPresetSelector.SelectedPreset;
		GamePrefs.Set(EnumGamePrefs.SandboxPreset, selectedPreset.InternalName);
		GamePrefs.Set(EnumGamePrefs.SandboxCode, selectedPreset.SandboxCode);
		if (selectedPreset.IsCustomPreset)
		{
			sandboxManager.LoadOptionsFromCode(selectedPreset.SandboxCode);
			sandboxDisplay.SandboxCode = selectedPreset.SandboxCode;
		}
		else
		{
			sandboxDisplay.SandboxName = selectedPreset.InternalName;
		}
		sandboxDisplay.IsDirty = true;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbBloodMoonFrequency_OnChangeHandler(XUiC_GamePrefSelector _prefSelector, EnumGamePrefs _gamePref)
	{
		gameOptions[EnumGamePrefs.BloodMoonRange].Enabled = GamePrefs.GetInt(_gamePref) != 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtPort_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		XUiC_TextInput xUiC_TextInput = (XUiC_TextInput)_sender;
		if (_text.Length < 1)
		{
			xUiC_TextInput.Text = "0";
		}
		else if (_text.Length > 1 && _text[0] == '0')
		{
			xUiC_TextInput.Text = _text.Substring(1);
		}
		validatePort(xUiC_TextInput);
		this.SettingsChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validatePort(XUiC_TextInput _txtPort)
	{
		PortValid = StringParsers.TryParseSInt32(_txtPort.Text, out var _result) && _result >= 1024 && _result < 65533;
		if (!PortValid)
		{
			_txtPort.ActiveTextColor = Color.red;
		}
	}

	public void WatchForServerEnabledChanges(bool _doWatch)
	{
		if (_doWatch)
		{
			XUiC_GamePrefSelector xUiC_GamePrefSelector = serverEnabledSelector;
			xUiC_GamePrefSelector.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xUiC_GamePrefSelector.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(CmbServerEnabled_OnChangeHandler));
		}
		else
		{
			XUiC_GamePrefSelector xUiC_GamePrefSelector2 = serverEnabledSelector;
			xUiC_GamePrefSelector2.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Remove(xUiC_GamePrefSelector2.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(CmbServerEnabled_OnChangeHandler));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbServerEnabled_OnChangeHandler(XUiC_GamePrefSelector _prefSelector, EnumGamePrefs _gamePref)
	{
		bool flag = GamePrefs.GetBool(EnumGamePrefs.ServerEnabled);
		RefreshMultiplayerOptionStates(flag);
		if (flag && (!PermissionsManager.IsMultiplayerAllowed() || !PermissionsManager.CanHostMultiplayer()))
		{
			XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.HostMultiplayer, [PublicizedFrom(EAccessModifier.Private)] (bool _result) =>
			{
				GamePrefs.Set(EnumGamePrefs.ServerEnabled, _result);
				serverEnabledSelector.SetCurrentValue();
			});
		}
	}

	public void RefreshMultiplayerOptionStates(bool _enabled)
	{
		bool enabled = PlatformManager.CrossplatformPlatform?.AntiCheatServer?.ServerEacAvailable() == true && _enabled;
		bool flag = PlatformManager.CrossplatformPlatform?.AntiCheatServer?.ServerEacEnabled() == true && _enabled;
		bool flag2 = PermissionsManager.IsCrossplayAllowed() && (flag || !Submission.Enabled) && _enabled;
		bool num = GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer);
		bool flag3 = GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay);
		bool flag4 = (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent() || !Submission.Enabled;
		GamePrefs.Set(EnumGamePrefs.ServerEACPeerToPeer, !flag4 || flag);
		gameOptions[EnumGamePrefs.ServerEACPeerToPeer].ViewComponent.IsVisible = flag4;
		gameOptions[EnumGamePrefs.ServerEACPeerToPeer].Enabled = enabled;
		if (num != GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer))
		{
			gameOptions[EnumGamePrefs.ServerEACPeerToPeer].SetCurrentValue();
		}
		gameOptions[EnumGamePrefs.ServerEACPeerToPeer].CheckDefaultValue();
		if (_enabled)
		{
			GamePrefs.Set(EnumGamePrefs.ServerAllowCrossplay, flag3 && flag2);
		}
		gameOptions[EnumGamePrefs.ServerAllowCrossplay].Enabled = flag2;
		gameOptions[EnumGamePrefs.ServerAllowCrossplay].SetCurrentValue();
		gameOptions[EnumGamePrefs.ServerAllowCrossplay].CheckDefaultValue();
		gameOptions[EnumGamePrefs.Region].Enabled = _enabled;
		gameOptions[EnumGamePrefs.ServerVisibility].Enabled = _enabled;
		gameOptions[EnumGamePrefs.ServerPassword].Enabled = _enabled;
		gameOptions[EnumGamePrefs.ServerMaxPlayerCount].Enabled = _enabled;
		gameOptions[EnumGamePrefs.Region].CheckDefaultValue();
		gameOptions[EnumGamePrefs.ServerVisibility].CheckDefaultValue();
		gameOptions[EnumGamePrefs.ServerPassword].CheckDefaultValue();
		gameOptions[EnumGamePrefs.ServerMaxPlayerCount].CheckDefaultValue();
	}

	public void ApplyMaxPlayerCountOptions()
	{
		if (!(GamePrefs.GetString(EnumGamePrefs.GameMode) != "GameMode" + EnumGameMode.Survival.ToStringCached()) && GameModeSurvival.OverrideMaxPlayerCount >= 2)
		{
			List<string> list = new List<string>();
			for (int i = 2; i <= GameModeSurvival.OverrideMaxPlayerCount; i++)
			{
				list.Add(i.ToString());
			}
			gameOptions[EnumGamePrefs.ServerMaxPlayerCount].OverrideValues(list);
		}
	}

	public void ApplyInitialServerEnabledState()
	{
		bool flag = GamePrefs.GetBool(EnumGamePrefs.ServerEnabled) && PermissionsManager.IsMultiplayerAllowed() && PermissionsManager.CanHostMultiplayer();
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, flag);
		serverEnabledSelector.SetCurrentValue();
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
		{
			serverEnabledSelector.Enabled = false;
		}
		else
		{
			WatchForServerEnabledChanges(_doWatch: true);
		}
		RefreshMultiplayerOptionStates(flag);
	}

	public void UpdateServerEnabledState()
	{
		serverEnabledSelector.SetCurrentValue();
	}

	public void UpdateCrossplayEnabledState()
	{
		gameOptions[EnumGamePrefs.ServerAllowCrossplay].SetCurrentValue();
	}

	public void ApplyCreativeModeChangeAllowed(bool _allowed)
	{
		gameOptions[EnumGamePrefs.BuildCreate].Enabled = _allowed;
	}

	public void SaveGameOptions(bool _saveAsLastUsed)
	{
		List<EnumGamePrefs> list = new List<EnumGamePrefs>(gameOptions.Count);
		gameOptions.CopyKeysTo(list);
		list.Add(EnumGamePrefs.SandboxCode);
		list.Add(EnumGamePrefs.SandboxPreset);
		GamePrefs.Instance.Save(GameIO.GetSaveGameDir() + "/gameOptions.sdf", list);
		if (_saveAsLastUsed)
		{
			GamePrefs.Instance.Save(GameIO.GetSaveGameRootDir() + "/newGameOptions.sdf", list);
		}
	}

	public void UpdateOptionVisibilityForGameMode(GameMode _gameMode)
	{
		foreach (KeyValuePair<EnumGamePrefs, XUiC_GamePrefSelector> gameOption in gameOptions)
		{
			gameOption.Value.SetCurrentGameMode(_gameMode);
		}
	}

	public void UpdateOptionValuesFromGamePrefs()
	{
		UpdateSandboxPresetGroups();
		foreach (KeyValuePair<EnumGamePrefs, XUiC_GamePrefSelector> gameOption in gameOptions)
		{
			gameOption.Value.SetCurrentValue();
		}
	}

	public void SetGamePrefsToDefaults()
	{
		foreach (KeyValuePair<EnumGamePrefs, XUiC_GamePrefSelector> gameOption in gameOptions)
		{
			GamePrefs.SetObject(gameOption.Key, GamePrefs.GetDefault(gameOption.Key));
		}
	}
}

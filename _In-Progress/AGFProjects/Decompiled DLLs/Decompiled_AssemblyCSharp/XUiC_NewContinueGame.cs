using System;
using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewContinueGame : XUiController
{
	public struct LevelInfo
	{
		public string RealName;

		public string CustName;

		public string Description;

		public GameUtils.WorldInfo WorldInfo;

		public bool IsNewRwg;

		public override string ToString()
		{
			return RealName;
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label windowheader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MultiplayerPrivilegeNotification wdwMultiplayerPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<GameMode> cbxGameMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<LevelInfo> cbxWorldName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtGameName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView txtGameNameView;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumGameMode, List<LevelInfo>> worldsPerMode = new EnumDictionary<EnumGameMode, List<LevelInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<SaveDataLimitType> cbxSaveDataLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorldGenerationWindowGroup worldGenerationControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SavegamesList savesList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDeleteSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel deleteSavePanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label deleteSaveText;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumGamePrefs, XUiC_GamePrefSelector> gameOptions = new EnumDictionary<EnumGamePrefs, XUiC_GamePrefSelector>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabsSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_GamePrefSelector serverEnabledSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isContinueGame = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dataManagementBarEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public long pendingPreviewSize;

	public static void SetIsContinueGame(XUi _xuiInstance, bool _continueGame)
	{
		_xuiInstance.FindWindowGroupByName(ID).GetChildByType<XUiC_NewContinueGame>().isContinueGame = _continueGame;
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		cbxGameMode = (XUiC_ComboBoxList<GameMode>)GetChildById("cbxGameMode");
		cbxGameMode.OnValueChanged += CbxGameMode_OnValueChanged;
		cbxWorldName = (XUiC_ComboBoxList<LevelInfo>)GetChildById("cbxWorldName");
		cbxWorldName.OnValueChanged += CbxWorldName_OnValueChanged;
		txtGameName = (XUiC_TextInput)GetChildById("txtGameName");
		txtGameName.OnChangeHandler += TxtGameName_OnChangeHandler;
		txtGameName.UIInput.onValidate = GameUtils.ValidateGameNameInput;
		txtGameNameView = txtGameName.UIInputController.ViewComponent;
		cbxSaveDataLimit = (XUiC_ComboBoxEnum<SaveDataLimitType>)GetChildById("cbxSaveDataLimit");
		SaveDataLimitUIHelper.AddComboBox(cbxSaveDataLimit);
		worldGenerationControls = GetChildByType<XUiC_WorldGenerationWindowGroup>();
		worldGenerationControls.OnCountyNameChanged += ValidateStartable;
		savesList = (XUiC_SavegamesList)GetChildById("saves");
		savesList.SelectionChanged += SavesList_OnSelectionChanged;
		savesList.OnEntryDoubleClicked += SavesList_OnEntryDoubleClicked;
		btnDeleteSave = (XUiC_SimpleButton)GetChildById("btnDeleteSave");
		btnDeleteSave.OnPressed += BtnDeleteSave_OnPressed;
		btnDeleteSave.OnHovered += BtnDeleteSave_OnHover;
		btnDeleteSave.Enabled = false;
		deleteSavePanel = (XUiV_Panel)GetChildById("deleteSavePanel").ViewComponent;
		deleteSaveText = (XUiV_Label)deleteSavePanel.Controller.GetChildById("deleteText").ViewComponent;
		XUiC_SimpleButton xUiC_SimpleButton = (XUiC_SimpleButton)deleteSavePanel.Controller.GetChildById("btnCancel");
		XUiC_SimpleButton obj = (XUiC_SimpleButton)deleteSavePanel.Controller.GetChildById("btnConfirm");
		xUiC_SimpleButton.OnPressed += BtnCancelDelete_OnPressed;
		obj.OnPressed += BtnConfirmDelete_OnPressed;
		_ = xUiC_SimpleButton.GetChildById("clickable").ViewComponent;
		_ = obj.GetChildById("clickable").ViewComponent;
		((XUiC_SimpleButton)GetChildById("btnDataManagement")).OnPressed += BtnDataManagement_OnPressed;
		windowheader = (XUiV_Label)GetChildById("windowheader").ViewComponent;
		((XUiC_SimpleButton)GetChildById("btnBack")).OnPressed += BtnBack_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnDefaults")).OnPressed += BtnDefaults_OnPressed;
		btnStart = (XUiC_SimpleButton)GetChildById("btnStart");
		btnStart.OnPressed += BtnStart_OnPressed;
		btnStart.Enabled = false;
		RefreshStartLabel();
		tabsSelector = (XUiC_TabSelector)GetChildById("tabs");
		((XUiC_SimpleButton)GetChildById("btnGenerateWorld")).OnPressed += BtnRwgPreviewerOnOnPressed;
		XUiC_GamePrefSelector[] childrenByType = GetChildrenByType<XUiC_GamePrefSelector>();
		foreach (XUiC_GamePrefSelector xUiC_GamePrefSelector in childrenByType)
		{
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
				xUiC_GamePrefSelector.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xUiC_GamePrefSelector.OnValueChanged, (Action<XUiC_GamePrefSelector, EnumGamePrefs>)([PublicizedFrom(EAccessModifier.Private)] (XUiC_GamePrefSelector _, EnumGamePrefs gamePrefs) =>
				{
					RefreshMultiplayerOptions(GamePrefs.GetBool(EnumGamePrefs.ServerEnabled));
				}));
				break;
			}
			gameOptions.Add(gamePref, xUiC_GamePrefSelector);
		}
		dataManagementBar = GetChildById("data_bar_controller") as XUiC_DataManagementBar;
		dataManagementBarEnabled = dataManagementBar != null && SaveInfoProvider.DataLimitEnabled;
		RegisterForInputStyleChanges();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshStartLabel()
	{
		InControlExtensions.SetApplyButtonString(btnStart, "xuiStart");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		RefreshStartLabel();
	}

	public override void OnOpen()
	{
		IsDirty = true;
		if (dataManagementBarEnabled)
		{
			SaveInfoProvider.Instance.SetDirty();
		}
		XUiC_GamePrefSelector xUiC_GamePrefSelector = serverEnabledSelector;
		xUiC_GamePrefSelector.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Remove(xUiC_GamePrefSelector.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(CmbServerEnabled_OnChangeHandler));
		windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
		cbxGameMode.Elements.Clear();
		createWorldList();
		GameMode[] availGameModes = GameMode.AvailGameModes;
		foreach (GameMode gameMode in availGameModes)
		{
			if (worldsPerMode.ContainsKey((EnumGameMode)gameMode.GetID()))
			{
				cbxGameMode.Elements.Add(gameMode);
			}
		}
		if (cbxGameMode.Elements.Count == 0)
		{
			Log.Error("No supported GameMode found in any world!");
		}
		if (!isContinueGame)
		{
			string modeName = GamePrefs.GetString(EnumGamePrefs.GameMode);
			int num = cbxGameMode.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (GameMode _mode) => _mode.GetTypeName().EqualsCaseInsensitive(modeName));
			if (num < 0)
			{
				num = 0;
			}
			cbxGameMode.SelectedIndex = num;
			GameModeChanged(cbxGameMode.Value);
			GamePrefs.Instance.Load(GameIO.GetSaveGameRootDir() + "/newGameOptions.sdf");
			updateGameOptionValues();
			if (GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge) == 0)
			{
				GamePrefs.Set(EnumGamePrefs.MaxChunkAge, (int)GamePrefs.GetDefault(EnumGamePrefs.MaxChunkAge));
			}
			if (GamePrefs.GetString(EnumGamePrefs.GameName).Trim().Length < 1)
			{
				GamePrefs.Set(EnumGamePrefs.GameName, "MyGame");
			}
			txtGameName.Text = GamePrefs.GetString(EnumGamePrefs.GameName).Trim();
			ValidateStartable();
		}
		windowheader.Text = (isContinueGame ? Localization.Get("xuiContinueGame") : Localization.Get("xuiNewGame"));
		base.OnOpen();
		deleteSavePanel.IsVisible = false;
		tabsSelector.ViewComponent.IsVisible = true;
		if (GamePrefs.GetString(EnumGamePrefs.GameMode) == "GameMode" + EnumGameMode.Survival.ToStringCached() && GameModeSurvival.OverrideMaxPlayerCount >= 2)
		{
			List<string> list = new List<string>();
			for (int num2 = 2; num2 <= GameModeSurvival.OverrideMaxPlayerCount; num2++)
			{
				list.Add(num2.ToString());
			}
			gameOptions[EnumGamePrefs.ServerMaxPlayerCount].OverrideValues(list);
		}
		if (isContinueGame && savesList.EntryCount > 0)
		{
			savesList.SelectedEntryIndex = 0;
			savesList.SelectedEntry.SelectCursorElement(_withDelay: true);
		}
		else if (isContinueGame)
		{
			tabsSelector.ViewComponent.IsVisible = false;
			string text = GamePrefs.GetString(EnumGamePrefs.GameMode);
			if (string.IsNullOrEmpty(text))
			{
				text = (string)GamePrefs.GetDefault(EnumGamePrefs.GameMode);
			}
			GameMode gameModeForName = GameMode.GetGameModeForName(text);
			if (gameModeForName == null)
			{
				text = (string)GamePrefs.GetDefault(EnumGamePrefs.GameMode);
				gameModeForName = GameMode.GetGameModeForName(text);
			}
			GameModeChanged(gameModeForName);
			ValidateStartable();
			GetChildById("btnBack").SelectCursorElement(_withDelay: true);
			UpdateBarSelectionValues();
		}
		else if (!isContinueGame)
		{
			updateGameOptionVisibilityForMode(cbxGameMode.Value);
			GetChildById("txtGameName").SelectCursorElement(_withDelay: true);
		}
		bool flag = GamePrefs.GetBool(EnumGamePrefs.ServerEnabled) && PermissionsManager.IsMultiplayerAllowed() && PermissionsManager.CanHostMultiplayer();
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, flag);
		serverEnabledSelector.SetCurrentValue();
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
		{
			serverEnabledSelector.Enabled = false;
		}
		else
		{
			XUiC_GamePrefSelector xUiC_GamePrefSelector2 = serverEnabledSelector;
			xUiC_GamePrefSelector2.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xUiC_GamePrefSelector2.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(CmbServerEnabled_OnChangeHandler));
		}
		RefreshMultiplayerOptions(flag);
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			gameOptions[EnumGamePrefs.BuildCreate].Enabled = !isContinueGame;
		}
		if (dataManagementBarEnabled)
		{
			dataManagementBar.SetDisplayMode((!isContinueGame) ? XUiC_DataManagementBar.DisplayMode.Preview : XUiC_DataManagementBar.DisplayMode.Selection);
			dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
			UpdateBarUsageAndAllowanceValues();
			if (!isContinueGame)
			{
				UpdateBarPendingValue();
			}
			SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Combine(SaveDataLimitUIHelper.OnValueChanged, new Action(UpdateBarPendingValue));
			worldGenerationControls.OnWorldSizeChanged += UpdateBarPendingValue;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_MultiplayerPrivilegeNotification.Close();
		SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Remove(SaveDataLimitUIHelper.OnValueChanged, new Action(UpdateBarPendingValue));
		worldGenerationControls.OnWorldSizeChanged -= UpdateBarPendingValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectWorld(string worldName)
	{
		cbxWorldName.SelectedIndex = Mathf.Max(cbxWorldName.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (LevelInfo _info) => _info.RealName.EqualsCaseInsensitive(worldName)), 0);
		GamePrefs.Set(EnumGamePrefs.GameWorld, cbxWorldName.Value.RealName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRwgPreviewerOnOnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.FindWindowGroupByName("rwgeditor").GetChildByType<XUiC_WorldGenerationWindowGroup>().LastWindowID = ID;
		base.xui.playerUI.windowManager.Open("rwgeditor", _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxGameMode_OnValueChanged(XUiController _sender, GameMode _oldValue, GameMode _newValue)
	{
		GameModeChanged(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxWorldName_OnValueChanged(XUiController _sender, LevelInfo _oldValue, LevelInfo _newValue)
	{
		IsDirty = true;
		GamePrefs.Set(EnumGamePrefs.GameWorld, _newValue.RealName);
		UpdateBarPendingValue();
		ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtGameName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		GamePrefs.Set(EnumGamePrefs.GameName, _text);
		ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createWorldList()
	{
		worldsPerMode.Clear();
		List<PathAbstractions.AbstractedLocation> availablePathsList = PathAbstractions.WorldsSearchPaths.GetAvailablePathsList();
		if (availablePathsList.Count == 0)
		{
			Log.Error("No worlds found!");
			return;
		}
		foreach (PathAbstractions.AbstractedLocation item2 in availablePathsList)
		{
			LevelInfo item = new LevelInfo
			{
				RealName = item2.Name
			};
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(item2);
			if (worldInfo == null)
			{
				item.Description = "No Description Found";
				if (!worldsPerMode.ContainsKey(EnumGameMode.Creative))
				{
					worldsPerMode.Add(EnumGameMode.Creative, new List<LevelInfo>());
				}
				worldsPerMode[EnumGameMode.Creative].Add(item);
				continue;
			}
			item.CustName = worldInfo.Name;
			item.Description = worldInfo.Description;
			item.WorldInfo = worldInfo;
			string[] modes = worldInfo.Modes;
			if (modes == null)
			{
				continue;
			}
			string[] array = modes;
			foreach (string text in array)
			{
				if (EnumUtils.TryParse<EnumGameMode>(text, out var _result, _ignoreCase: true))
				{
					if (!worldsPerMode.ContainsKey(_result))
					{
						worldsPerMode.Add(_result, new List<LevelInfo>());
					}
					worldsPerMode[_result].Add(item);
				}
				else
				{
					Log.Warning("World \"" + item.RealName + "\" has unknown game mode \"" + text + "\".");
				}
			}
		}
		if (worldsPerMode.Count == 0)
		{
			Log.Error("No world with any valid GameMode found!");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateWorlds()
	{
		string worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		EnumGameMode iD = (EnumGameMode)cbxGameMode.Value.GetID();
		cbxWorldName.Elements.Clear();
		if (worldsPerMode.ContainsKey(iD))
		{
			foreach (LevelInfo item in worldsPerMode[iD])
			{
				if (CanAddWorld(item.WorldInfo) && (item.WorldInfo == null || !item.WorldInfo.RandomGeneratedWorld))
				{
					cbxWorldName.Elements.Add(item);
				}
			}
			string text = Localization.Get("lblNewRandomWorld");
			if (text == "")
			{
				text = "New Random World";
			}
			cbxWorldName.Elements.Add(new LevelInfo
			{
				RealName = text,
				CustName = text,
				Description = "Generate New Random World",
				IsNewRwg = true
			});
			foreach (LevelInfo item2 in worldsPerMode[iD])
			{
				if (CanAddWorld(item2.WorldInfo) && item2.WorldInfo != null && item2.WorldInfo.RandomGeneratedWorld)
				{
					cbxWorldName.Elements.Add(item2);
				}
			}
		}
		int num = cbxWorldName.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (LevelInfo _info) => _info.RealName.EqualsCaseInsensitive(worldName));
		cbxWorldName.SelectedIndex = ((num >= 0) ? num : 0);
		CbxWorldName_OnValueChanged(cbxWorldName, default(LevelInfo), cbxWorldName.Value);
		[PublicizedFrom(EAccessModifier.Internal)]
		static bool CanAddWorld(GameUtils.WorldInfo _worldInfo)
		{
			if (_worldInfo != null && PlatformOptimizations.EnforceMaxWorldSizeHost)
			{
				Vector2i worldSize = _worldInfo.WorldSize;
				if (worldSize.x > PlatformOptimizations.MaxWorldSizeHost || worldSize.y > PlatformOptimizations.MaxWorldSizeHost)
				{
					return false;
				}
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnEntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && savesList.SelectedEntryIndex >= 0)
		{
			BtnStart_OnPressed(_sender, _mouseButton);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnSelectionChanged(XUiC_ListEntry<XUiC_SavegamesList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_SavegamesList.ListEntry> _newEntry)
	{
		bool flag = _newEntry != null;
		btnDeleteSave.Enabled = flag;
		tabsSelector.ViewComponent.IsVisible = flag;
		if (flag)
		{
			foreach (XUiC_GamePrefSelector value in gameOptions.Values)
			{
				GamePrefs.SetObject(value.GamePref, GamePrefs.GetDefault(value.GamePref));
			}
			XUiC_SavegamesList.ListEntry entry = _newEntry.GetEntry();
			GamePrefs.Instance.Load(GameIO.GetSaveGameDir(entry.worldName, entry.saveName) + "/gameOptions.sdf");
			GameModeChanged(entry.gameMode);
			GamePrefs.Set(EnumGamePrefs.GameName, entry.saveName);
			GamePrefs.Set(EnumGamePrefs.GameWorld, entry.worldName);
			serverEnabledSelector.SetCurrentValue();
			RefreshMultiplayerOptions(GamePrefs.GetBool(EnumGamePrefs.ServerEnabled));
		}
		UpdateBarSelectionValues();
		ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveGameOptions()
	{
		List<EnumGamePrefs> list = new List<EnumGamePrefs>(gameOptions.Count);
		gameOptions.CopyKeysTo(list);
		GamePrefs.Instance.Save(GameIO.GetSaveGameDir() + "/gameOptions.sdf", list);
		if (!isContinueGame)
		{
			GamePrefs.Instance.Save(GameIO.GetSaveGameRootDir() + "/newGameOptions.sdf", list);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteSaveGetConfirmation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnHover(XUiController _sender, bool _isOver)
	{
		if (dataManagementBarEnabled)
		{
			dataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void deleteSaveGetConfirmation()
	{
		deleteSavePanel.IsVisible = true;
		_ = (XUiC_SimpleButton)deleteSavePanel.Controller.GetChildById("btnConfirm");
		deleteSaveText.Text = string.Format(Localization.Get("xuiSavegameDeleteConfirmation"), savesList.SelectedEntry.GetEntry().saveName);
		base.xui.playerUI.CursorController.SetNavigationLockView(deleteSavePanel, deleteSavePanel.Controller.GetChildById("btnCancel").ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteSavePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		btnDeleteSave.SelectCursorElement();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteSavePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		btnDeleteSave.SelectCursorElement();
		XUiC_SavegamesList.ListEntry entry = savesList.SelectedEntry.GetEntry();
		string saveGameDir = GameIO.GetSaveGameDir(entry.worldName, entry.saveName);
		if (SdDirectory.Exists(saveGameDir))
		{
			SdDirectory.Delete(saveGameDir, recursive: true);
			SaveInfoProvider.Instance.SetDirty();
			UpdateBarUsageAndAllowanceValues();
		}
		savesList.RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGameOptionVisibilityForMode(GameMode _gameMode)
	{
		foreach (KeyValuePair<EnumGamePrefs, XUiC_GamePrefSelector> gameOption in gameOptions)
		{
			gameOption.Value.SetCurrentGameMode(_gameMode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGameOptionValues()
	{
		foreach (KeyValuePair<EnumGamePrefs, XUiC_GamePrefSelector> gameOption in gameOptions)
		{
			gameOption.Value.SetCurrentValue();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbBloodMoonFrequency_OnChangeHandler(XUiC_GamePrefSelector _prefSelector, EnumGamePrefs _gamePref)
	{
		gameOptions[EnumGamePrefs.BloodMoonRange].Enabled = GamePrefs.GetInt(_gamePref) != 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbServerEnabled_OnChangeHandler(XUiC_GamePrefSelector _prefSelector, EnumGamePrefs _gamePref)
	{
		bool flag = GamePrefs.GetBool(EnumGamePrefs.ServerEnabled);
		RefreshMultiplayerOptions(flag);
		if (flag && (!PermissionsManager.IsMultiplayerAllowed() || !PermissionsManager.CanHostMultiplayer()))
		{
			if (wdwMultiplayerPrivileges == null)
			{
				wdwMultiplayerPrivileges = XUiC_MultiplayerPrivilegeNotification.GetWindow();
			}
			wdwMultiplayerPrivileges?.ResolvePrivilegesWithDialog(EUserPerms.HostMultiplayer, [PublicizedFrom(EAccessModifier.Private)] (bool result) =>
			{
				GamePrefs.Set(EnumGamePrefs.ServerEnabled, result);
				serverEnabledSelector.SetCurrentValue();
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshMultiplayerOptions(bool enabled)
	{
		bool valueOrDefault = PlatformManager.CrossplatformPlatform?.AntiCheatServer?.ServerEacAvailable() == true;
		bool valueOrDefault2 = PlatformManager.CrossplatformPlatform?.AntiCheatServer?.ServerEacEnabled() == true;
		bool flag = PermissionsManager.IsCrossplayAllowed() && (valueOrDefault2 || !Submission.Enabled) && enabled;
		bool num = GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer);
		bool flag2 = GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay);
		bool flag3 = (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent() || !Submission.Enabled;
		GamePrefs.Set(EnumGamePrefs.ServerEACPeerToPeer, !flag3 || valueOrDefault2);
		gameOptions[EnumGamePrefs.ServerEACPeerToPeer].ViewComponent.IsVisible = flag3;
		gameOptions[EnumGamePrefs.ServerEACPeerToPeer].Enabled = valueOrDefault;
		if (num != GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer))
		{
			gameOptions[EnumGamePrefs.ServerEACPeerToPeer].SetCurrentValue();
		}
		gameOptions[EnumGamePrefs.ServerEACPeerToPeer].CheckDefaultValue();
		if (enabled)
		{
			GamePrefs.Set(EnumGamePrefs.ServerAllowCrossplay, flag2 && flag);
		}
		gameOptions[EnumGamePrefs.ServerAllowCrossplay].Enabled = flag;
		gameOptions[EnumGamePrefs.ServerAllowCrossplay].SetCurrentValue();
		gameOptions[EnumGamePrefs.ServerAllowCrossplay].CheckDefaultValue();
		gameOptions[EnumGamePrefs.Region].Enabled = enabled;
		gameOptions[EnumGamePrefs.ServerVisibility].Enabled = enabled;
		gameOptions[EnumGamePrefs.ServerPassword].Enabled = enabled;
		gameOptions[EnumGamePrefs.ServerMaxPlayerCount].Enabled = enabled;
		gameOptions[EnumGamePrefs.Region].CheckDefaultValue();
		gameOptions[EnumGamePrefs.ServerVisibility].CheckDefaultValue();
		gameOptions[EnumGamePrefs.ServerPassword].CheckDefaultValue();
		gameOptions[EnumGamePrefs.ServerMaxPlayerCount].CheckDefaultValue();
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
		ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnPressed(XUiController _sender, int _mouseButton)
	{
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, (bool)GamePrefs.GetDefault(EnumGamePrefs.ServerEnabled));
		serverEnabledSelector.SetCurrentValue();
		CmbServerEnabled_OnChangeHandler(serverEnabledSelector, EnumGamePrefs.ServerEnabled);
		cbxGameMode.Value.ResetGamePrefs();
		updateGameOptionValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDataManagement_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DataManagement.OpenDataManagementWindow(this, OnDataManagementWindowClosed);
	}

	public void OnDataManagementWindowClosed()
	{
		UpdateBarUsageAndAllowanceValues();
		if (isContinueGame)
		{
			if (dataManagementBarEnabled)
			{
				dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
				dataManagementBar.SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion.None);
			}
			savesList.RebuildList();
		}
		else
		{
			createWorldList();
			updateWorlds();
			if (dataManagementBarEnabled)
			{
				dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
				UpdateBarPendingValue();
			}
		}
		worldGenerationControls.RefreshCountyName();
		ValidateGameName();
		worldGenerationControls.RefreshBindings();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarUsageAndAllowanceValues()
	{
		if (dataManagementBarEnabled)
		{
			SaveInfoProvider instance = SaveInfoProvider.Instance;
			dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
			dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarPendingValue()
	{
		if (dataManagementBarEnabled)
		{
			pendingPreviewSize = CalculatePendingSaveSize();
			dataManagementBar.SetPendingBytes(pendingPreviewSize);
			ValidateStartable();
		}
		[PublicizedFrom(EAccessModifier.Private)]
		long CalculatePendingSaveSize()
		{
			LevelInfo value = cbxWorldName.Value;
			Vector2i worldSize = default(Vector2i);
			if (value.IsNewRwg)
			{
				worldSize.x = Math.Max(1, worldGenerationControls.WorldSize);
				worldSize.y = worldSize.x;
			}
			else
			{
				worldSize = value.WorldInfo.WorldSize;
			}
			if (SaveDataLimitUIHelper.CurrentValue == SaveDataLimitType.Unlimited)
			{
				return Math.Max(SaveDataLimitType.Short.CalculateTotalSize(worldSize), SaveInfoProvider.Instance.TotalAvailableBytes);
			}
			return SaveDataLimitUIHelper.CurrentValue.CalculateTotalSize(worldSize);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarSelectionValues()
	{
		if (dataManagementBarEnabled)
		{
			XUiC_SavegamesList.ListEntry listEntry = savesList.SelectedEntry?.GetEntry();
			if (listEntry != null && SaveInfoProvider.Instance.TryGetLocalSaveEntry(listEntry.worldName, listEntry.saveName, out var saveEntryInfo))
			{
				XUiC_DataManagementBar.BarRegion selectedByteRegion = new XUiC_DataManagementBar.BarRegion(saveEntryInfo.BarStartOffset, saveEntryInfo.SizeInfo.ReportedSize);
				dataManagementBar.SetSelectedByteRegion(selectedByteRegion);
				dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
			}
			else
			{
				dataManagementBar.SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion.None);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStart_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!btnStart.Enabled)
		{
			return;
		}
		GameManager.Instance.showOpenerMovieOnLoad = GamePrefs.GetBool(EnumGamePrefs.OptionsIntroMovieEnabled) && !isContinueGame;
		if (GamePrefs.GetBool(EnumGamePrefs.ServerEnabled))
		{
			if (wdwMultiplayerPrivileges == null)
			{
				wdwMultiplayerPrivileges = XUiC_MultiplayerPrivilegeNotification.GetWindow();
			}
			EUserPerms perms = EUserPerms.HostMultiplayer;
			if (GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay))
			{
				perms |= EUserPerms.Crossplay;
			}
			wdwMultiplayerPrivileges?.ResolvePrivilegesWithDialog(perms, [PublicizedFrom(EAccessModifier.Internal)] (bool result) =>
			{
				bool value = PermissionsManager.CanHostMultiplayer();
				GamePrefs.Set(EnumGamePrefs.ServerEnabled, value);
				serverEnabledSelector.SetCurrentValue();
				bool value2 = perms.HasCrossplay() && PermissionsManager.IsCrossplayAllowed();
				GamePrefs.Set(EnumGamePrefs.ServerAllowCrossplay, value2);
				gameOptions[EnumGamePrefs.ServerAllowCrossplay].SetCurrentValue();
				if (result)
				{
					ThreadManager.StartCoroutine(startGameCo());
				}
			});
		}
		else
		{
			ThreadManager.StartCoroutine(startGameCo());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startGameCo()
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		if (!isContinueGame)
		{
			LevelInfo value = cbxWorldName.Value;
			string worldName;
			Vector2i worldSize = default(Vector2i);
			if (value.IsNewRwg)
			{
				worldName = null;
				yield return worldGenerationControls.GenerateCo(_usePreviewer: false, [PublicizedFrom(EAccessModifier.Internal)] (string name) =>
				{
					worldName = name;
				});
				if (worldName == null)
				{
					base.xui.playerUI.windowManager.Open(windowGroup.ID, _bModal: true);
					yield break;
				}
				GamePrefs.Set(EnumGamePrefs.GameWorld, worldName);
				worldSize.x = worldGenerationControls.WorldSize;
				worldSize.y = worldSize.x;
			}
			else
			{
				worldSize = value.WorldInfo.WorldSize;
				worldName = value.WorldInfo.Name;
			}
			long saveDataSize = SaveDataLimitUIHelper.CurrentValue.CalculateTotalSize(worldSize);
			XUiC_SaveSpaceNeeded confirmationWindow = XUiC_SaveSpaceNeeded.Open(saveDataSize, new string[2]
			{
				GameIO.GetWorldDir(worldName),
				GameIO.GetSaveGameDir(worldName, GamePrefs.GetString(EnumGamePrefs.GameName))
			}, null, autoConfirm: true, canCancel: true, canDiscard: false, null, "xuiDmCreateSave", null, null, "xuiCreate");
			while (confirmationWindow.IsOpen)
			{
				yield return null;
			}
			if (confirmationWindow.Result != XUiC_SaveSpaceNeeded.ConfirmationResult.Confirmed)
			{
				base.xui.playerUI.windowManager.Open(windowGroup.ID, _bModal: true);
				yield break;
			}
			SaveDataLimit.SetLimitToPref(saveDataSize);
		}
		GamePrefs.SetPersistent(EnumGamePrefs.GameMode, _bPersistent: true);
		SaveGameOptions();
		if (PlatformOptimizations.RestartAfterRwg)
		{
			yield return PlatformApplicationManager.CheckRestartCoroutine(loadSaveGame: true);
		}
		bool offline = !GamePrefs.GetBool(EnumGamePrefs.ServerEnabled);
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), offline);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			((XUiC_MessageBoxWindowGroup)((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller).ShowNetworkError(networkConnectionError);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GameModeChanged(GameMode _newMode)
	{
		GamePrefs.Set(EnumGamePrefs.GameMode, _newMode.GetTypeName());
		updateWorlds();
		updateGameOptionVisibilityForMode(_newMode);
		updateGameOptionValues();
		ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateStartable()
	{
		bool flag;
		if (isContinueGame)
		{
			flag = savesList.SelectedEntryIndex >= 0;
			if (flag)
			{
				XUiC_SavegamesList.ListEntry entry = savesList.SelectedEntry.GetEntry();
				flag = entry.versionComparison != VersionInformation.EVersionComparisonResult.DifferentMajor;
				if (flag && entry.AbstractedLocation.Type == PathAbstractions.EAbstractedLocationType.None)
				{
					flag = false;
				}
			}
			tabsSelector.ViewComponent.IsVisible = flag;
		}
		else
		{
			flag = ValidateGameName();
			if (!cbxWorldName.Value.IsNewRwg && cbxWorldName.Value.WorldInfo != null)
			{
				GameUtils.WorldInfo worldInfo = cbxWorldName.Value.WorldInfo;
				bool flag2 = worldInfo.GameVersionCreated.IsValid && !worldInfo.GameVersionCreated.EqualsMajor(Constants.cVersionInformation);
				flag = flag && !flag2;
			}
			if (dataManagementBarEnabled)
			{
				flag &= SaveInfoProvider.Instance.TotalAvailableBytes >= pendingPreviewSize;
			}
		}
		flag &= ValidatePort();
		btnStart.Enabled = flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ValidateGameName()
	{
		bool isNewRwg = cbxWorldName.Value.IsNewRwg;
		string text = txtGameName.Text;
		string text2 = (isNewRwg ? worldGenerationControls.CountyNameLabel.Text : cbxWorldName.Value.RealName);
		bool flag = GameUtils.ValidateGameName(text);
		bool flag2 = text.Trim().Length > 0;
		bool flag3 = !text.EqualsCaseInsensitive("Region");
		bool flag4 = !SdFile.Exists(GameIO.GetSaveGameDir(text2, text) + "/main.ttw");
		bool flag5 = PathAbstractions.WorldsSearchPaths.GetLocation(text2, text2, text).Type != PathAbstractions.EAbstractedLocationType.None;
		bool validCountyName = worldGenerationControls.ValidCountyName;
		bool flag6 = ((!isNewRwg) ? flag5 : validCountyName);
		bool flag7 = flag && flag2 && flag3 && flag4 && flag6;
		txtGameName.ActiveTextColor = (flag7 ? Color.white : Color.red);
		if (!flag2)
		{
			txtGameNameView.ToolTip = Localization.Get("mmLblNameErrorEmpty");
		}
		else if (!flag)
		{
			txtGameNameView.ToolTip = Localization.Get("mmLblNameErrorInvalid");
		}
		else if (!flag3)
		{
			txtGameNameView.ToolTip = Localization.Get("mmLblNameErrorDefault");
		}
		else if (!flag4)
		{
			txtGameNameView.ToolTip = Localization.Get("mmLblNameErrorExists");
		}
		else if (!flag5)
		{
			txtGameNameView.ToolTip = Localization.Get("mmLblNameErrorNoWorld");
		}
		else
		{
			txtGameNameView.ToolTip = "";
		}
		return flag7;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ValidatePort()
	{
		XUiC_TextInput controlText = gameOptions[EnumGamePrefs.ServerPort].ControlText;
		int num;
		if (StringParsers.TryParseSInt32(controlText.Text, out var _result) && _result >= 1024)
		{
			num = ((_result < 65533) ? 1 : 0);
			if (num != 0)
			{
				goto IL_0046;
			}
		}
		else
		{
			num = 0;
		}
		controlText.ActiveTextColor = Color.red;
		goto IL_0046;
		IL_0046:
		return (byte)num != 0;
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.windowManager.IsKeyShortcutsAllowed() && !XUiC_DataManagement.IsWindowOpen(base.xui))
		{
			if (isContinueGame && !deleteSavePanel.IsVisible && Input.GetKeyUp(KeyCode.Delete) && savesList.SelectedEntry?.GetEntry() != null)
			{
				deleteSaveGetConfirmation();
			}
			if (base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
			{
				BtnStart_OnPressed(null, 0);
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
		DoLoadSaveGameAutomation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoLoadSaveGameAutomation()
	{
		EPlatformLoadSaveGameState loadSaveGameState = PlatformApplicationManager.GetLoadSaveGameState();
		switch (loadSaveGameState)
		{
		case EPlatformLoadSaveGameState.NewGameSelect:
			if (!isContinueGame)
			{
				string text3 = GamePrefs.GetString(EnumGamePrefs.GameWorld);
				if (!cbxWorldName.Value.RealName.EqualsCaseInsensitive(text3))
				{
					SelectWorld(text3);
				}
				if (!cbxWorldName.Value.RealName.EqualsCaseInsensitive(text3))
				{
					PlatformApplicationManager.SetFailedLoadSaveGame();
				}
				else
				{
					PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
				}
			}
			break;
		case EPlatformLoadSaveGameState.ContinueGameSelect:
			if (isContinueGame)
			{
				string text = GamePrefs.GetString(EnumGamePrefs.GameWorld);
				string text2 = GamePrefs.GetString(EnumGamePrefs.GameName);
				XUiC_SavegamesList.ListEntry listEntry = savesList.SelectedEntry?.GetEntry();
				if (listEntry == null || !listEntry.worldName.EqualsCaseInsensitive(text) || !listEntry.saveName.EqualsCaseInsensitive(text2))
				{
					savesList.SelectEntry(text, text2);
				}
				listEntry = savesList.SelectedEntry?.GetEntry();
				if (listEntry == null || !listEntry.worldName.EqualsCaseInsensitive(text) || !listEntry.saveName.EqualsCaseInsensitive(text2))
				{
					PlatformApplicationManager.SetFailedLoadSaveGame();
				}
				else
				{
					PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
				}
			}
			break;
		case EPlatformLoadSaveGameState.NewGamePlay:
		case EPlatformLoadSaveGameState.ContinueGamePlay:
			if (!btnStart.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				break;
			}
			BtnStart_OnPressed(btnStart, -1);
			PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			break;
		case EPlatformLoadSaveGameState.ContinueGameOpen:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "isnewgame":
			_value = (!isContinueGame).ToString();
			return true;
		case "iscontinuegame":
			_value = isContinueGame.ToString();
			return true;
		case "isgenerateworld":
			_value = (!isContinueGame && cbxWorldName.Value.IsNewRwg).ToString();
			return true;
		case "isnotgenerateworld":
			_value = (!isContinueGame && !cbxWorldName.Value.IsNewRwg).ToString();
			return true;
		case "mapsize":
			if (!isContinueGame && cbxWorldName.Value.WorldInfo != null)
			{
				GameUtils.WorldInfo worldInfo2 = cbxWorldName.Value.WorldInfo;
				_value = worldInfo2.WorldSize.x + " x " + worldInfo2.WorldSize.y;
			}
			else
			{
				_value = "";
			}
			return true;
		case "differentworldversion":
			if (!isContinueGame && cbxWorldName.Value.WorldInfo != null)
			{
				GameUtils.WorldInfo worldInfo = cbxWorldName.Value.WorldInfo;
				_value = (worldInfo.GameVersionCreated.IsValid && !worldInfo.GameVersionCreated.EqualsMajor(Constants.cVersionInformation)).ToString();
			}
			else
			{
				_value = "false";
			}
			return true;
		case "worldgameversion":
			if (!isContinueGame && cbxWorldName.Value.WorldInfo != null)
			{
				GameUtils.WorldInfo worldInfo3 = cbxWorldName.Value.WorldInfo;
				object obj;
				if (!worldInfo3.GameVersionCreated.IsValid)
				{
					obj = "";
				}
				else
				{
					string text = worldInfo3.GameVersionCreated.ReleaseType.ToStringCached();
					int major = worldInfo3.GameVersionCreated.Major;
					obj = text + " " + major;
				}
				_value = (string)obj;
			}
			else
			{
				_value = "false";
			}
			return true;
		case "false":
			_value = "false";
			return true;
		case "showbar":
			_value = dataManagementBarEnabled.ToString();
			return true;
		case "crossplayTooltip":
		{
			string permissionDenyReason = PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay);
			if (!string.IsNullOrEmpty(permissionDenyReason))
			{
				_value = permissionDenyReason;
			}
			else if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent() && gameOptions.Count > 0)
			{
				gameOptions[EnumGamePrefs.ServerEACPeerToPeer].CheckDefaultValue();
				_value = string.Format(Localization.Get("xuiOptionsGeneralCrossplayTooltipPC"), 8);
			}
			else
			{
				_value = Localization.Get("xuiOptionsGeneralCrossplayTooltip");
			}
			return true;
		}
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}

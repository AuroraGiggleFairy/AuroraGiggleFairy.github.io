using System.Collections;
using Platform;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ContinueGame : XUiC_NewContinueBase
{
	public static string ID = "";

	[XuiBindComponent("saves", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SavegamesList savesList;

	[XuiBindComponent("btnDeleteSave", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDeleteSave;

	public override bool PlayIntroMovie
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	public override bool AllowChangingCreativeMode
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return !(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent();
		}
	}

	[XuiXmlBinding("iscontinuegame")]
	public bool IsContinueGame
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return true;
		}
	}

	[XuiXmlBinding("saveselected")]
	public bool SaveSelected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return savesList?.SelectedEntryData != null;
		}
	}

	[XuiXmlBinding("saveplayable")]
	public bool SavePlayable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return savesList?.SelectedEntryData?.Playable == true;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	public override void OnOpen()
	{
		IsDirty = true;
		Settings.WatchForServerEnabledChanges(_doWatch: false);
		base.OnOpen();
		if (savesList.EntryCount > 0)
		{
			savesList.SelectedEntryIndex = 0;
			savesList.SelectCursorElementForSelectedEntry();
			validateStartable();
		}
		else
		{
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
			gameModeChanged(gameModeForName);
			validateStartable();
			GetChildById("btnBack").SelectCursorElement(_withDelay: true);
			updateBarSelectionValues();
		}
		Settings.ApplyInitialServerEnabledState();
		if (base.DataManagementBarEnabled)
		{
			SaveInfoProvider.Instance.SetDirty();
			DataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			DataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
			updateBarUsageAndAllowanceValues();
		}
		GamePrefs.SetObject(EnumGamePrefs.ResetUnprotectedChunks, GamePrefs.GetDefault(EnumGamePrefs.ResetUnprotectedChunks));
	}

	[XuiBindEvent("ListEntryDoubleClicked", "savesList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnEntryDoubleClicked(XUiC_List<XUiC_SavegamesList.ListEntry> _list, XUiC_SavegamesList.ListEntry _entry)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && savesList.SelectedEntryIndex >= 0)
		{
			BtnStart_OnPressed(this, -1);
		}
	}

	[XuiBindEvent("SelectionChanged", "savesList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnSelectionChanged(XUiC_List<XUiC_SavegamesList.ListEntry> _list, XUiC_SavegamesList.ListEntry _previousEntry, XUiC_SavegamesList.ListEntry _newEntry)
	{
		IsDirty = true;
		if (_newEntry != null)
		{
			Settings.SetGamePrefsToDefaults();
			GamePrefs.Instance.Load(_newEntry.GetSaveDir() + "/gameOptions.sdf");
			GamePrefs.SetObject(EnumGamePrefs.ResetUnprotectedChunks, GamePrefs.GetDefault(EnumGamePrefs.ResetUnprotectedChunks));
			gameModeChanged(_newEntry.GameMode);
			GamePrefs.Set(EnumGamePrefs.GameName, _newEntry.SaveName);
			GamePrefs.Set(EnumGamePrefs.GameWorld, _newEntry.WorldName);
			GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, (int)_newEntry.StorageType);
			Settings.RefreshMultiplayerOptionStates(GamePrefs.GetBool(EnumGamePrefs.ServerEnabled));
		}
		updateBarSelectionValues();
		validateStartable();
	}

	[XuiBindEvent("OnPress", "btnDeleteSave")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteSaveWithConfirmation();
	}

	[XuiBindEvent("OnHover", "btnDeleteSave")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnHover(XUiController _sender, bool _isOver)
	{
		if (base.DataManagementBarEnabled)
		{
			DataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void deleteSaveWithConfirmation()
	{
		XUiC_SavegamesList.ListEntry saveEntry = savesList.SelectedEntryData;
		XUiC_MessageBoxWindowGroup.ShowCustom(xui, Localization.Get("xuiDeleteSaveGame"), string.Format(Localization.Get("xuiSavegameDeleteConfirmation"), saveEntry.SaveName), [PublicizedFrom(EAccessModifier.Internal)] (XUiC_MessageBoxWindowGroup _box) =>
		{
			_box.Buttons[0].DefaultConfirm("btnConfirm", [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				btnDeleteSave.SelectCursorElement();
				string saveDir = saveEntry.GetSaveDir();
				if (SdDirectory.Exists(saveDir))
				{
					SdDirectory.Delete(saveDir, recursive: true);
					SaveDataUtils.SaveDataManager.CommitSync();
					SaveInfoProvider.Instance.SetDirty();
					updateBarUsageAndAllowanceValues();
				}
				savesList.RebuildList();
			}, _enabled: true, 0f, 1.5f);
			_box.Buttons[2].DefaultCancel("xuiCancel", [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				btnDeleteSave.SelectCursorElement();
			});
		}, _openMainMenuOnClose: false, _modal: false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnOpenSandboxSettingsRequested()
	{
		XUiC_SavegamesList.ListEntry selectedEntryData = savesList.SelectedEntryData;
		SandboxOptionManager.Current.SetWorldAndGame(selectedEntryData.WorldName, selectedEntryData.SaveName);
		base.OnOpenSandboxSettingsRequested();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDataManagementWindowClosed()
	{
		updateBarUsageAndAllowanceValues();
		if (base.DataManagementBarEnabled)
		{
			DataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			DataManagementBar.SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion.None);
		}
		XUiC_SavegamesList.ListEntry selectedEntryData = savesList.SelectedEntryData;
		string worldName = null;
		string saveName = null;
		UserDataStorageType storageType = UserDataStorageType.DeviceLocal;
		if (selectedEntryData != null)
		{
			worldName = selectedEntryData.WorldName;
			saveName = selectedEntryData.SaveName;
			storageType = selectedEntryData.StorageType;
		}
		savesList.RebuildList();
		savesList.SelectEntry(worldName, saveName, storageType);
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBarSelectionValues()
	{
		if (base.DataManagementBarEnabled)
		{
			XUiC_SavegamesList.ListEntry selectedEntryData = savesList.SelectedEntryData;
			if (selectedEntryData != null && SaveInfoProvider.Instance.TryGetLocalSaveEntry(selectedEntryData.WorldName, selectedEntryData.SaveName, selectedEntryData.StorageType, out var saveEntryInfo) && selectedEntryData.StorageType.UsesDataLimit())
			{
				XUiC_DataManagementBar.BarRegion selectedByteRegion = new XUiC_DataManagementBar.BarRegion(saveEntryInfo.BarStartOffset, saveEntryInfo.SizeInfo.ReportedSize);
				DataManagementBar.SetSelectedByteRegion(selectedByteRegion);
				DataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
			}
			else
			{
				DataManagementBar.SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion.None);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator startGameCo()
	{
		XUiC_SavegamesList.ListEntry selectedSave = savesList.SelectedEntryData;
		if (selectedSave.SaveVersion.ReleaseType != Constants.cVersionInformation.ReleaseType || selectedSave.SaveVersion.Major != Constants.cVersionInformation.Major)
		{
			bool? selection = null;
			XUiC_MessageBoxWindowGroup.ShowOkCancel(xui, Localization.Get("xuiSaveDifferentGameVersion"), string.Format(Localization.Get("xuiSaveDifferentGameVersionText"), selectedSave.SaveName, selectedSave.SaveVersion.ShortString, Constants.cVersionInformation.ShortString), [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				selection = true;
			}, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				selection = false;
			}, _openMainMenuOnClose: false, _modal: false);
			while (!selection.HasValue)
			{
				yield return null;
			}
			if (!selection.Value)
			{
				yield break;
			}
		}
		SaveInfoProvider.SaveEntryInfo saveEntryInfo;
		if (selectedSave.WorldName.Equals("Playtesting") || selectedSave.WorldName.Equals("Empty"))
		{
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, 4);
		}
		else if (SaveInfoProvider.Instance.TryGetLocalSaveEntry(selectedSave.WorldName, selectedSave.SaveName, selectedSave.StorageType, out saveEntryInfo) && saveEntryInfo.WorldEntry.Location.Type != PathAbstractions.EAbstractedLocationType.None)
		{
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, (int)saveEntryInfo.WorldEntry.Location.Type);
			GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)saveEntryInfo.WorldEntry.Location.StorageType);
		}
		else
		{
			PathAbstractions.AbstractedLocation? worldLocation = null;
			XUiC_WorldSelectionPopup.Open(xui, "xuiWorldConflict", selectedSave.WorldName, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				worldLocation = PathAbstractions.AbstractedLocation.None;
			}, [PublicizedFrom(EAccessModifier.Internal)] (PathAbstractions.AbstractedLocation _location) =>
			{
				worldLocation = _location;
			});
			while (!worldLocation.HasValue)
			{
				yield return null;
			}
			if (worldLocation.Value.Type == PathAbstractions.EAbstractedLocationType.None)
			{
				yield break;
			}
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, (int)worldLocation.Value.Type);
			GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)worldLocation.Value.StorageType);
		}
		if (GamePrefs.GetInt(EnumGamePrefs.ResetUnprotectedChunks) != 0)
		{
			bool? resetConfirm = null;
			XUiC_MessageBoxWindowGroup.ShowOkCancel(xui, Localization.Get("goResetUnprotectedChunksMode"), Localization.Get("goResetUnprotectedChunksModeConfirmation"), [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				resetConfirm = true;
			}, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				resetConfirm = false;
			}, _openMainMenuOnClose: false, _modal: false);
			while (!resetConfirm.HasValue)
			{
				yield return null;
			}
			if (!resetConfirm.Value)
			{
				yield break;
			}
		}
		GamePrefs.SetPersistent(EnumGamePrefs.GameMode, _bPersistent: true);
		Settings.SaveGameOptions(_saveAsLastUsed: false);
		if (PlatformOptimizations.RestartAfterRwg)
		{
			yield return PlatformApplicationManager.CheckRestartCoroutine(loadSaveGame: true);
		}
		xui.playerUI.windowManager.Close(windowGroup);
		bool offline = !GamePrefs.GetBool(EnumGamePrefs.ServerEnabled);
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), offline);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			XUiC_MessageBoxWindowGroup.ShowNetworkError(xui, networkConnectionError);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameModeChanged(GameMode _newMode)
	{
		GamePrefs.Set(EnumGamePrefs.GameMode, _newMode.GetTypeName());
		Settings.UpdateOptionVisibilityForGameMode(_newMode);
		Settings.UpdateOptionValuesFromGamePrefs();
		validateStartable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void validateStartable()
	{
		bool flag = savesList.SelectedEntryIndex >= 0;
		if (flag)
		{
			XUiC_SavegamesList.ListEntry selectedEntryData = savesList.SelectedEntryData;
			flag = selectedEntryData.Playable;
			if (flag && !selectedEntryData.WorldExists)
			{
				flag = false;
			}
		}
		flag &= Settings.PortValid;
		base.ValidStartableState = flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateInput()
	{
		if (XUiUtils.HotkeysAllowedFor(viewComponent ?? children[0].ViewComponent))
		{
			if (Input.GetKeyUp(KeyCode.Delete) && savesList.SelectedEntryData != null)
			{
				deleteSaveWithConfirmation();
			}
			if (xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
			{
				BtnStart_OnPressed(null, 0);
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		doLoadSaveGameAutomation();
		updateInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doLoadSaveGameAutomation()
	{
		EPlatformLoadSaveGameState loadSaveGameState = PlatformApplicationManager.GetLoadSaveGameState();
		switch (loadSaveGameState)
		{
		case EPlatformLoadSaveGameState.ContinueGameSelect:
		{
			string text = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			string text2 = GamePrefs.GetString(EnumGamePrefs.GameName);
			UserDataStorageType storageType = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
			XUiC_SavegamesList.ListEntry selectedEntryData = savesList.SelectedEntryData;
			if (selectedEntryData == null || !selectedEntryData.WorldName.EqualsCaseInsensitive(text) || !selectedEntryData.SaveName.EqualsCaseInsensitive(text2))
			{
				savesList.SelectEntry(text, text2, storageType);
			}
			selectedEntryData = savesList.SelectedEntryData;
			if (selectedEntryData == null || !selectedEntryData.WorldName.EqualsCaseInsensitive(text) || !selectedEntryData.SaveName.EqualsCaseInsensitive(text2))
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
			}
			else
			{
				PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			}
			break;
		}
		case EPlatformLoadSaveGameState.NewGamePlay:
		case EPlatformLoadSaveGameState.ContinueGamePlay:
			if (!BtnStart.ViewComponent.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				break;
			}
			BtnStart_OnPressed(this, -1);
			PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			break;
		case EPlatformLoadSaveGameState.NewGameSelect:
		case EPlatformLoadSaveGameState.ContinueGameOpen:
			break;
		}
	}
}

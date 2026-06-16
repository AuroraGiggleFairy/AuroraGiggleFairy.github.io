using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DataManagement : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum DeletionMode
	{
		None,
		World,
		Save,
		Player,
		BlockedPlayers
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string id = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.PlayerEntryInfoPlatformDataResolver playerEntryResolver;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DMWorldList worldList;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DMSavegamesList savesList;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DMPlayersList playersList;

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent("btnDeleteWorld", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDeleteWorld;

	[XuiBindComponent("btnDeleteSave", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDeleteSave;

	[XuiBindComponent("btnCopySave", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCopySave;

	[XuiBindComponent("btnManageSave", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnManageSave;

	[XuiBindComponent("btnDeletePlayer", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDeletePlayer;

	[XuiBindComponent("btnDeleteBlockedPlayers", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDeleteBlockedPlayers;

	[XuiBindComponent("btnMoveWorld", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnMoveWorld;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool worldDeletable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveDeletable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveManageable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveCopyable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveVersionValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerDeletable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBlockedPlayersInBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserDataManagement.WorldMove worldMove;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView previousLockView;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onClosedCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int commitTimeoutSeconds = 30;

	[PublicizedFrom(EAccessModifier.Private)]
	public int commitToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool waitingForCommit;

	[PublicizedFrom(EAccessModifier.Private)]
	public int commitDeadline = int.MaxValue;

	[XuiXmlBinding("showbar")]
	public bool DataManagementBarEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (dataManagementBar != null)
			{
				return SaveInfoProvider.DataLimitEnabled;
			}
			return false;
		}
	}

	[XuiXmlBinding("savedeletable")]
	public bool SaveDeletable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return saveDeletable;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			saveDeletable = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("worlddeletable")]
	public bool WorldDeletable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return worldDeletable;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			worldDeletable = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("playerdeletable")]
	public bool PlayerDeletable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return playerDeletable;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			playerDeletable = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("worldmoveable")]
	public bool WorldMoveable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return worldMove.IsReady;
		}
	}

	[XuiXmlBinding("worldmovelabel")]
	public string WorldMoveLabel
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!worldMove.IsReady)
			{
				return string.Empty;
			}
			return storageTargetToBtnLabel(worldMove.moveToStorageType);
		}
	}

	[XuiXmlBinding("savecopyable")]
	public bool SaveCopyable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return saveCopyable;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			saveCopyable = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("savemanageable")]
	public bool SaveManageable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return saveManageable;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			saveManageable = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("blockedplayerscount")]
	public int BlockedPlayersCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return playersList?.BlockedPlayerCount ?? 0;
		}
	}

	public override void Init()
	{
		base.Init();
		id = base.WindowGroup.Id;
		IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (DataManagementBarEnabled)
		{
			dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			dataManagementBar.SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion.None);
		}
		refresh();
		string worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		string saveName = GamePrefs.GetString(EnumGamePrefs.GameName);
		UserDataStorageType storage = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		if (!SaveInfoProvider.Instance.TryGetLocalSaveEntry(worldName, saveName, storage, out var saveEntryInfo) || !worldList.SelectByKey(saveEntryInfo.WorldEntry.WorldKey))
		{
			worldList.SelectedEntryIndex = -1;
			savesList.SelectedEntryIndex = -1;
			savesList.SetWorldFilter(string.Empty);
			playersList.ClearList(_resetFilter: true);
		}
		else if (!savesList.SelectByName(GamePrefs.GetString(EnumGamePrefs.GameName)))
		{
			savesList.SelectedEntryIndex = -1;
			playersList.ClearList(_resetFilter: true);
		}
		playersList.SelectedEntryIndex = -1;
		if (DataManagementBarEnabled)
		{
			dataManagementBar.SetSelectionDepth((savesList.SelectedEntryIndex != -1) ? XUiC_DataManagementBar.SelectionDepth.Secondary : XUiC_DataManagementBar.SelectionDepth.Primary);
		}
		refreshButtonStates();
		updateBarSelectionValues();
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		waitingForCommit = false;
		commitDeadline = int.MaxValue;
		worldList.ClearList();
		savesList.ClearList();
		playersList.ClearList(_resetFilter: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refresh()
	{
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		worldList.RebuildList(instance.WorldEntryInfos);
		savesList.RebuildList(instance.SaveEntryInfos);
		if (DataManagementBarEnabled)
		{
			dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
			dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (waitingForCommit)
		{
			if (!SaveDataUtils.SaveDataManager.IsCommitPending(commitToken))
			{
				waitingForCommit = false;
				XUiC_MessageBoxWindowGroup.Close(xui);
			}
			else if ((int)Time.unscaledTimeAsDouble >= commitDeadline)
			{
				commitDeadline = int.MaxValue;
				XUiC_MessageBoxWindowGroup.Close(xui);
				XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiDmCommitTitle"), Localization.Get("xuiPleaseWait"), [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					waitingForCommit = false;
				}, _openMainMenuOnClose: false, _modal: false);
			}
		}
		if (XUiUtils.HotkeysAllowedFor(viewComponent) && xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased && (!waitingForCommit || commitDeadline >= int.MaxValue))
		{
			ThreadManager.RunTaskAfterFrames(closeDataManagementWindow);
			return;
		}
		if (playerEntryResolver != null && playerEntryResolver.IsComplete)
		{
			playersList.RebuildList(playerEntryResolver.pendingPlayerEntries);
			playerEntryResolver = null;
			IsDirty = true;
		}
		handleDirtyUpdateDefault();
	}

	[XuiBindEvent("SelectionChanged", "worldList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_SelectionChanged(XUiC_List<XUiC_DMWorldList.ListEntry> _list, XUiC_DMWorldList.ListEntry _previousEntry, XUiC_DMWorldList.ListEntry _newEntry)
	{
		string worldFilter = "";
		if (_newEntry != null)
		{
			worldFilter = _newEntry.Key;
		}
		savesList.SetWorldFilter(worldFilter);
		savesList.ClearSelection();
		playersList.ClearSelection();
		playersList.ClearList(_resetFilter: true);
		refreshButtonStates();
		updateBarSelectionValues();
	}

	[XuiBindEvent("SelectionChanged", "savesList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_SelectionChanged(XUiC_List<XUiC_DMSavegamesList.ListEntry> _list, XUiC_DMSavegamesList.ListEntry _previousEntry, XUiC_DMSavegamesList.ListEntry _newEntry)
	{
		if (_newEntry != null)
		{
			IEnumerable<SaveInfoProvider.PlayerEntryInfo> playerEntries = SaveInfoProvider.Instance.SaveEntryInfos.Where([PublicizedFrom(EAccessModifier.Internal)] (SaveInfoProvider.SaveEntryInfo _entry) => _entry.WorldEntry.WorldKey.Equals(_newEntry.WorldKey) && _entry.Name.Equals(_newEntry.SaveName)).SelectMany([PublicizedFrom(EAccessModifier.Internal)] (SaveInfoProvider.SaveEntryInfo _entry) => _entry.PlayerEntryInfos);
			playerEntryResolver = SaveInfoProvider.PlayerEntryInfoPlatformDataResolver.StartNew(playerEntries);
			if (playerEntryResolver.IsComplete)
			{
				playersList.RebuildList(playerEntryResolver.pendingPlayerEntries);
				playerEntryResolver = null;
			}
			else
			{
				playersList.ShowLoading();
				playersList.ClearList();
			}
		}
		playersList.ClearSelection();
		refreshButtonStates();
		updateBarSelectionValues();
	}

	[XuiBindEvent("SelectionChanged", "playersList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayersList_SelectionChanged(XUiC_List<XUiC_DMPlayersList.ListEntry> _list, XUiC_DMPlayersList.ListEntry _previousEntry, XUiC_DMPlayersList.ListEntry _newEntry)
	{
		refreshButtonStates();
		updateBarSelectionValues();
	}

	[XuiBindEvent("OnEntryHovered", "worldList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_OnChildElementHovered(XUiC_DMWorldList.ListEntry _sender, bool _isOver)
	{
		if (!DataManagementBarEnabled)
		{
			return;
		}
		if (_isOver)
		{
			SaveInfoProvider.WorldEntryInfo worldEntryInfo = _sender?.WorldEntryInfo;
			if (worldEntryInfo != null)
			{
				long num = worldEntryInfo.SaveDataSizeForLimit;
				if (worldEntryInfo.UsesDataLimit)
				{
					num += worldEntryInfo.WorldDataSize;
				}
				XUiC_DataManagementBar.BarRegion hoveredByteRegion = new XUiC_DataManagementBar.BarRegion(worldEntryInfo.BarStartOffset, num);
				dataManagementBar.SetHoveredByteRegion(hoveredByteRegion);
				goto IL_0060;
			}
		}
		dataManagementBar.SetHoveredByteRegion(XUiC_DataManagementBar.BarRegion.None);
		goto IL_0060;
		IL_0060:
		dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
	}

	[XuiBindEvent("OnEntryHovered", "savesList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnChildElementHovered(XUiC_DMSavegamesList.ListEntry _sender, bool _isOver)
	{
		if (!DataManagementBarEnabled)
		{
			return;
		}
		if (_isOver)
		{
			SaveInfoProvider.SaveEntryInfo saveEntryInfo = _sender?.SaveEntryInfo;
			if (saveEntryInfo != null)
			{
				SaveInfoProvider.SaveSizeInfo sizeInfo = saveEntryInfo.SizeInfo;
				if (sizeInfo.UsesDataLimit)
				{
					XUiC_DataManagementBar.BarRegion hoveredByteRegion = new XUiC_DataManagementBar.BarRegion(saveEntryInfo.BarStartOffset, saveEntryInfo.SizeInfo.ReportedSize);
					dataManagementBar.SetHoveredByteRegion(hoveredByteRegion);
					goto IL_0062;
				}
			}
		}
		dataManagementBar.SetHoveredByteRegion(XUiC_DataManagementBar.BarRegion.None);
		goto IL_0062;
		IL_0062:
		dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Secondary);
	}

	[XuiBindEvent("OnEntryHovered", "playersList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayersList_OnChildElementHovered(XUiC_DMPlayersList.ListEntry _sender, bool _isOver)
	{
		if (!DataManagementBarEnabled)
		{
			return;
		}
		if (_isOver)
		{
			SaveInfoProvider.PlayerEntryInfo playerEntryInfo = _sender?.PlayerEntryInfo;
			if (playerEntryInfo != null)
			{
				XUiC_DataManagementBar.BarRegion hoveredByteRegion = new XUiC_DataManagementBar.BarRegion(playerEntryInfo.BarStartOffset, playerEntryInfo.Size);
				dataManagementBar.SetHoveredByteRegion(hoveredByteRegion);
				goto IL_004d;
			}
		}
		dataManagementBar.SetHoveredByteRegion(XUiC_DataManagementBar.BarRegion.None);
		goto IL_004d;
		IL_004d:
		dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Tertiary);
	}

	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		closeDataManagementWindow();
	}

	[XuiBindEvent("OnPress", "btnDeleteWorld")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteWorld_OnPressed(XUiController _sender, int _mouseButton)
	{
		int num = savesList.GetSavesInWorld(worldList.SelectedEntryData.Key).Count();
		XUiC_MessageBoxWindowGroup.ShowConfirmCancel(xui, Localization.Get("xuiWorldDelete"), (num > 0) ? string.Format(Localization.Get("xuiWorldDeleteConfirmation"), num) : Localization.Get("xuiWorldDeleteConfirmationNoSaves"), [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			processDeletionConfirmationResult(DeletionMode.World, XUiC_ConfirmationPrompt.Result.Confirmed);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			processDeletionConfirmationResult(DeletionMode.World, XUiC_ConfirmationPrompt.Result.Cancelled);
		}, _openMainMenuOnClose: false, _modal: false);
		dataManagementBar.SetDeleteWindowDisplayed(_displayed: true);
	}

	[XuiBindEvent("OnHover", "btnDeleteWorld")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteWorld_OnHover(XUiController _sender, bool _isOver)
	{
		if (DataManagementBarEnabled)
		{
			dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
			dataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[XuiBindEvent("OnPress", "btnDeleteSave")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		bool flag = worldList.SelectedEntryData.Type == SaveInfoProvider.RemoteWorldsType;
		XUiC_MessageBoxWindowGroup.ShowConfirmCancel(xui, Localization.Get(flag ? "xuiDmDeleteRemoteSave" : "xuiDeleteSaveGame"), Localization.Get(flag ? "xuiDmDeleteRemoteSaveConfirmation" : "xuiDeleteSaveGame"), [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			processDeletionConfirmationResult(DeletionMode.Save, XUiC_ConfirmationPrompt.Result.Confirmed);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			processDeletionConfirmationResult(DeletionMode.Save, XUiC_ConfirmationPrompt.Result.Cancelled);
		}, _openMainMenuOnClose: false, _modal: false);
		dataManagementBar.SetDeleteWindowDisplayed(_displayed: true);
	}

	[XuiBindEvent("OnHover", "btnDeleteSave")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnHover(XUiController _sender, bool _isOver)
	{
		if (DataManagementBarEnabled)
		{
			dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Secondary);
			dataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[XuiBindEvent("OnPress", "btnDeletePlayer")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeletePlayer_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_MessageBoxWindowGroup.ShowConfirmCancel(xui, Localization.Get("xuiDmDeletePlayer"), Localization.Get("xuiDmDeletePlayerConfirmation"), [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			processDeletionConfirmationResult(DeletionMode.Player, XUiC_ConfirmationPrompt.Result.Confirmed);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			processDeletionConfirmationResult(DeletionMode.Player, XUiC_ConfirmationPrompt.Result.Cancelled);
		}, _openMainMenuOnClose: false, _modal: false);
		dataManagementBar.SetDeleteWindowDisplayed(_displayed: true);
	}

	[XuiBindEvent("OnHover", "btnDeletePlayer")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeletePlayer_OnHover(XUiController _sender, bool _isOver)
	{
		if (DataManagementBarEnabled)
		{
			dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Tertiary);
			dataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[XuiBindEvent("OnPress", "btnDeleteBlockedPlayers")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteBlockedPlayers_OnPressed(XUiController _sender, int _mouseButton)
	{
		playersList.ClearSelection();
		showBlockedPlayersInBar = true;
		updateBarSelectionValues();
		XUiC_MessageBoxWindowGroup.ShowConfirmCancel(xui, Localization.Get("xuiDmDeleteBlockedPlayers"), Localization.Get("xuiDmDeletePlayerConfirmation"), [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			processDeletionConfirmationResult(DeletionMode.BlockedPlayers, XUiC_ConfirmationPrompt.Result.Confirmed);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			processDeletionConfirmationResult(DeletionMode.BlockedPlayers, XUiC_ConfirmationPrompt.Result.Cancelled);
		}, _openMainMenuOnClose: false, _modal: false);
	}

	[XuiBindEvent("OnHover", "btnDeleteBlockedPlayers")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteBlockedPlayers_OnHover(XUiController _sender, bool _isOver)
	{
		if (_isOver != showBlockedPlayersInBar)
		{
			showBlockedPlayersInBar = _isOver;
			updateBarSelectionValues();
		}
		if (DataManagementBarEnabled)
		{
			dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Tertiary);
			dataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void processDeletionConfirmationResult(DeletionMode _deletionMode, XUiC_ConfirmationPrompt.Result _result)
	{
		if (_result == XUiC_ConfirmationPrompt.Result.Confirmed)
		{
			processDeletion(_deletionMode);
		}
		else
		{
			cancelDeletion(_deletionMode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cancelDeletion(DeletionMode _deletionMode)
	{
		XUiController xUiController = _deletionMode switch
		{
			DeletionMode.World => btnDeleteWorld, 
			DeletionMode.Save => btnDeleteSave, 
			DeletionMode.Player => btnDeletePlayer, 
			_ => null, 
		};
		dataManagementBar.SetDeleteWindowDisplayed(_displayed: false);
		xui.playerUI.CursorController.SetNavigationLockView(viewComponent, xUiController?.ViewComponent);
		showBlockedPlayersInBar = false;
		updateBarSelectionValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void processDeletion(DeletionMode _deletionMode)
	{
		int num = worldList.SelectedEntryIndex;
		int num2 = savesList.SelectedEntryIndex;
		int num3 = playersList.SelectedEntryIndex;
		XUiController xUiController = null;
		switch (_deletionMode)
		{
		case DeletionMode.World:
			try
			{
				XUiC_DMWorldList.ListEntry selectedEntryData2 = worldList.SelectedEntryData;
				if (selectedEntryData2 == null)
				{
					throw new NotSupportedException("Failed to retrieve selected world.");
				}
				if (!selectedEntryData2.Deletable)
				{
					throw new NotSupportedException("Tried to delete non-generated world.");
				}
				if (selectedEntryData2.Location == PathAbstractions.AbstractedLocation.None)
				{
					throw new NotSupportedException("Tried to delete world entry with location == none.");
				}
				GameUtils.DeleteWorld(selectedEntryData2.Location);
				if (GamePrefs.GetString(EnumGamePrefs.GameWorld).EqualsCaseInsensitive(selectedEntryData2.Location.Name))
				{
					GamePrefs.Set(EnumGamePrefs.GameWorld, "Navezgane");
				}
				commitToken = SaveDataUtils.SaveDataManager.CommitAsync();
				num = Mathf.Clamp(num, -1, worldList.EntryCount - 1);
				num2 = -1;
				num3 = -1;
			}
			catch (Exception ex5)
			{
				Log.Error("Error occurred while deleting world: \"" + ex5.Message + "\"");
			}
			xUiController = btnDeleteWorld;
			break;
		case DeletionMode.Save:
			try
			{
				SdDirectory.Delete((savesList.SelectedEntryData ?? throw new NotSupportedException("Failed to retrieve selected save.")).SaveDirectory, recursive: true);
				commitToken = SaveDataUtils.SaveDataManager.CommitAsync();
				num2 = Mathf.Clamp(num2, -1, savesList.EntryCount - 1);
				num3 = -1;
			}
			catch (Exception ex3)
			{
				Log.Error("Error occurred while deleting save: \"" + ex3.Message + "\"");
			}
			xUiController = btnDeleteSave;
			break;
		case DeletionMode.Player:
			try
			{
				XUiC_DMPlayersList.ListEntry selectedEntryData = playersList.SelectedEntryData;
				if (selectedEntryData == null)
				{
					throw new Exception("Failed to retrieve selected player entry.");
				}
				string text2 = (savesList.SelectedEntryData ?? throw new Exception("Failed to retrieve selected save entry.")).SaveDirectory + "/Player";
				if (!SdDirectory.Exists(text2))
				{
					throw new Exception("Player save data directory not found at expected path: " + text2 + ".");
				}
				SdFileSystemInfo[] fileSystemInfos = new SdDirectoryInfo(text2).GetFileSystemInfos(selectedEntryData.ID + "*");
				foreach (SdFileSystemInfo sdFileSystemInfo in fileSystemInfos)
				{
					if (string.IsNullOrEmpty(sdFileSystemInfo.Extension) && SdDirectory.Exists(sdFileSystemInfo.FullName))
					{
						SdDirectory.Delete(sdFileSystemInfo.FullName, recursive: true);
					}
					else
					{
						SdFile.Delete(sdFileSystemInfo.FullName);
					}
				}
				commitToken = SaveDataUtils.SaveDataManager.CommitAsync();
				num3 = Mathf.Clamp(num3, -1, playersList.EntryCount - 1);
			}
			catch (Exception ex4)
			{
				Log.Error("Error occurred while deleting player: \"" + ex4.Message + "\"");
			}
			xUiController = btnDeletePlayer;
			break;
		case DeletionMode.BlockedPlayers:
			try
			{
				string text = (savesList.SelectedEntryData ?? throw new Exception("Failed to retrieve selected save entry.")).SaveDirectory + "/Player";
				if (!SdDirectory.Exists(text))
				{
					throw new Exception("Player save data directory not found at expected path: " + text + ".");
				}
				SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(text);
				foreach (SaveInfoProvider.PlayerEntryInfo blockedPlayer in playersList.BlockedPlayers)
				{
					try
					{
						SdFileSystemInfo[] fileSystemInfos = sdDirectoryInfo.GetFileSystemInfos(blockedPlayer.Id + "*");
						for (int i = 0; i < fileSystemInfos.Length; i++)
						{
							SdFile.Delete(fileSystemInfos[i].FullName);
						}
					}
					catch (Exception ex)
					{
						Log.Error("Error occurred while deleting blocked player save files: " + ex.Message);
					}
				}
				commitToken = SaveDataUtils.SaveDataManager.CommitAsync();
			}
			catch (Exception ex2)
			{
				Log.Error("Error occurred while deleting blocked players: \"" + ex2.Message + "\"");
			}
			showBlockedPlayersInBar = false;
			break;
		default:
			UnityEngine.Debug.LogError("Error in internal logic: invalid deletion mode. No data will be deleted.");
			break;
		}
		waitForCommitIfPending();
		SaveInfoProvider.Instance.SetDirty();
		refresh();
		worldList.SelectedEntryIndex = num;
		savesList.SelectedEntryIndex = num2;
		playersList.SelectedEntryIndex = num3;
		dataManagementBar.SetDeleteWindowDisplayed(_displayed: false);
		xui.playerUI.CursorController.SetNavigationLockView(viewComponent, xUiController?.ViewComponent);
	}

	[Conditional("DELETION_LOG_ENABLED")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void logDeletion(string _v)
	{
		UnityEngine.Debug.Log(_v);
	}

	[XuiBindEvent("OnHover", "btnMoveWorld")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMoveWorld_OnHover(XUiController _sender, bool _isOver)
	{
		setWorldMoveBarPreviewActive(_isOver);
	}

	[XuiBindEvent("OnPress", "btnMoveWorld")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMoveWorld_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!worldMove.IsReady)
		{
			throw new Exception("Can not move world. No world selected for moving");
		}
		setWorldMoveBarPreviewActive(_isActive: true);
		int countOfAssociatedSavesInSameStorage = worldMove.CountOfAssociatedSavesInSameStorage;
		XUiC_MessageBoxWindowGroup.ShowConfirmCancel(xui, Localization.Get($"xuiDmMoveWorldTitleTo{worldMove.moveToStorageType}"), (countOfAssociatedSavesInSameStorage > 0) ? string.Format(Localization.Get($"xuiDmMoveWorldTo{worldMove.moveToStorageType}"), countOfAssociatedSavesInSameStorage) : Localization.Get("xuiDmMoveWorldNoSaves"), OnConfirmationResult, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			setWorldMoveBarPreviewActive(_isActive: false);
		}, _openMainMenuOnClose: false, _modal: false);
		[PublicizedFrom(EAccessModifier.Private)]
		void OnConfirmationResult()
		{
			setWorldMoveBarPreviewActive(_isActive: false);
			UserDataManagement.Result result = worldMove.PerformMove();
			switch (result)
			{
			case UserDataManagement.Result.TargetAlreadyExists:
				XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiDmWorldMoveErrorTitle"), Localization.Get("xuiDmWorldMoveErrorExists"), null, _openMainMenuOnClose: false, _modal: false);
				break;
			case UserDataManagement.Result.Exception:
				commitToken = SaveDataUtils.SaveDataManager.CommitAsync();
				XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiDmWorldMoveErrorTitle"), Localization.Get("xuiDmWorldMoveErrorFailed"), null, _openMainMenuOnClose: false, _modal: false);
				break;
			case UserDataManagement.Result.FailedToMoveSaves:
				commitToken = SaveDataUtils.SaveDataManager.CommitAsync();
				XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiDmSaveMoveErrorTitle"), Localization.Get("xuiDmSaveMoveErrorMultiple"), null, _openMainMenuOnClose: false, _modal: false);
				break;
			case UserDataManagement.Result.Success:
				commitToken = SaveDataUtils.SaveDataManager.CommitAsync();
				break;
			default:
				Log.Error($"Unhandled world move result type {result}");
				break;
			}
			string name = worldMove.worldInfo.Name;
			UserDataStorageType userStorageType = ((result == UserDataManagement.Result.Success) ? worldMove.moveToStorageType : worldMove.worldInfo.Location.StorageType);
			waitForCommitIfPending();
			refresh();
			string worldEntryKey = SaveInfoProvider.GetWorldEntryKey(name, userStorageType);
			worldList.SelectByKey(worldEntryKey);
			refreshButtonStates();
			updateBarSelectionValues();
		}
	}

	[XuiBindEvent("OnPress", "btnCopySave")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCopySave_OnPressed(XUiController _sender, int _mouseButton)
	{
		openSaveManagementWindow(XUiC_SaveManagementPrompt.SaveManagementMode.Copy);
	}

	[XuiBindEvent("OnPress", "btnManageSave")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnManageSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		openSaveManagementWindow(XUiC_SaveManagementPrompt.SaveManagementMode.Manage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openSaveManagementWindow(XUiC_SaveManagementPrompt.SaveManagementMode _mode)
	{
		XUiC_DMSavegamesList.ListEntry listEntry = savesList?.SelectedEntryData;
		if (listEntry != null)
		{
			SaveInfoProvider.SaveEntryInfo saveEntryInfo = listEntry.SaveEntryInfo;
			SaveInfoProvider.WorldEntryInfo worldEntry = saveEntryInfo.WorldEntry;
			XUiC_SaveManagementPrompt.Show(xui, dataManagementBar, _mode, saveEntryInfo, worldEntry, OnSaveCopyCompleted, OnSaveMoveCompleted, OnSaveApplyCompleted);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSaveMoveCompleted(UserDataManagement.Result _result, string _targetSaveName, string _worldName, UserDataStorageType _expectedWorldStorage)
	{
		commitToken = SaveDataUtils.SaveDataManager.CommitAsync();
		switch (_result)
		{
		case UserDataManagement.Result.TargetAlreadyExists:
			XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiDmSaveMoveErrorTitle"), Localization.Get("xuiDmSaveManageErrorExists"), null, _openMainMenuOnClose: false, _modal: false);
			break;
		case UserDataManagement.Result.Exception:
			XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiDmSaveMoveErrorTitle"), Localization.Get("xuiDmSaveManageErrorFailed"), null, _openMainMenuOnClose: false, _modal: false);
			break;
		}
		waitForCommitIfPending();
		refresh();
		string worldEntryKey = SaveInfoProvider.GetWorldEntryKey(_worldName, _expectedWorldStorage);
		if (worldList.SelectByKey(worldEntryKey))
		{
			savesList.SelectByName(_targetSaveName);
		}
		refreshButtonStates();
		updateBarSelectionValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSaveApplyCompleted()
	{
		commitToken = SaveDataUtils.SaveDataManager.CommitAsync();
		waitForCommitIfPending();
		int selectedEntryIndex = worldList.SelectedEntryIndex;
		int selectedEntryIndex2 = savesList.SelectedEntryIndex;
		refresh();
		worldList.SelectedEntryIndex = selectedEntryIndex;
		savesList.SelectedEntryIndex = selectedEntryIndex2;
		refreshButtonStates();
		updateBarSelectionValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSaveCopyCompleted(UserDataManagement.Result _result, string _targetSaveName, string _worldName, UserDataStorageType _expectedWorldStorage)
	{
		commitToken = SaveDataUtils.SaveDataManager.CommitAsync();
		switch (_result)
		{
		case UserDataManagement.Result.TargetAlreadyExists:
			XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiDmSaveCopyErrorTitle"), Localization.Get("xuiDmSaveManageErrorExists"), null, _openMainMenuOnClose: false, _modal: false);
			break;
		case UserDataManagement.Result.Exception:
			XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiDmSaveCopyErrorTitle"), Localization.Get("xuiDmSaveManageErrorFailed"), null, _openMainMenuOnClose: false, _modal: false);
			break;
		}
		waitForCommitIfPending();
		refresh();
		string worldEntryKey = SaveInfoProvider.GetWorldEntryKey(_worldName, _expectedWorldStorage);
		if (worldList.SelectByKey(worldEntryKey))
		{
			savesList.SelectByName(_targetSaveName);
		}
		refreshButtonStates();
		updateBarSelectionValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waitForCommitIfPending()
	{
		if (SaveDataUtils.SaveDataManager.IsCommitPending(commitToken))
		{
			waitingForCommit = true;
			commitDeadline = (int)Time.unscaledTimeAsDouble + commitTimeoutSeconds;
			XUiC_MessageBoxWindowGroup.ShowCustom(xui, Localization.Get("xuiDmCommitTitle"), Localization.Get("xuiPleaseWait"), null, _openMainMenuOnClose: false, _modal: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setWorldMoveBarPreviewActive(bool _isActive)
	{
		if (DataManagementBarEnabled && worldMove.IsReady)
		{
			if (worldMove.moveToStorageType.UsesDataLimit())
			{
				long worldDataSize = worldMove.worldInfo.WorldDataSize;
				setBarMovePreviewModeActive(_isActive, worldDataSize);
			}
			else
			{
				dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
				dataManagementBar.SetDeleteHovered(_isActive);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setBarMovePreviewModeActive(bool _isActive, long _pendingBytes)
	{
		if (_isActive && _pendingBytes > 0)
		{
			dataManagementBar.SetPendingBytes(_pendingBytes);
			dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
		}
		else
		{
			dataManagementBar.SetPendingBytes(0L);
			dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public UserDataStorageType getStorageTypeToMoveTo(UserDataStorageType _moveFromStorageType)
	{
		return _moveFromStorageType switch
		{
			UserDataStorageType.Roaming => UserDataStorageType.DeviceLocal, 
			UserDataStorageType.DeviceLocal => UserDataStorageType.Roaming, 
			_ => throw new Exception($"Unhandled move from storage type {_moveFromStorageType}"), 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshButtonStates()
	{
		XUiC_DMWorldList.ListEntry listEntry = worldList?.SelectedEntryData;
		if (listEntry != null)
		{
			WorldDeletable = listEntry.Deletable && !SaveInfoProvider.Instance.IsDirectoryProtected(listEntry.Location.FullPath);
			if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional && listEntry.Moveable)
			{
				worldMove = new UserDataManagement.WorldMove(listEntry.WorldEntryInfo, getStorageTypeToMoveTo(listEntry.WorldEntryInfo.Location.StorageType));
			}
			else
			{
				worldMove = default(UserDataManagement.WorldMove);
			}
		}
		else
		{
			WorldDeletable = false;
			worldMove = default(UserDataManagement.WorldMove);
		}
		XUiC_DMSavegamesList.ListEntry listEntry2 = savesList?.SelectedEntryData;
		if (listEntry2 != null)
		{
			SaveDeletable = !SaveInfoProvider.Instance.IsDirectoryProtected(listEntry2.SaveDirectory);
			bool flag = worldList.SelectedEntryData?.Type != SaveInfoProvider.RemoteWorldsType;
			SaveManageable = flag || PlatformManager.MultiPlatform.UserDataRoaming.IsSupported;
			SaveCopyable = flag;
		}
		else
		{
			SaveDeletable = false;
			SaveManageable = false;
			SaveCopyable = false;
		}
		PlayerDeletable = SaveDeletable && playersList.SelectedEntryIndex >= 0;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBarSelectionValues()
	{
		if (!DataManagementBarEnabled)
		{
			return;
		}
		XUiC_DataManagementBar.BarRegion primaryRegion = XUiC_DataManagementBar.BarRegion.None;
		XUiC_DataManagementBar.BarRegion secondaryRegion = XUiC_DataManagementBar.BarRegion.None;
		XUiC_DataManagementBar.BarRegion tertiaryRegion = XUiC_DataManagementBar.BarRegion.None;
		XUiC_DMWorldList.ListEntry selectedEntryData = worldList.SelectedEntryData;
		if (selectedEntryData != null)
		{
			long size = (selectedEntryData.UsesDataLimit ? (selectedEntryData.WorldDataSize + selectedEntryData.SaveDataSizeForLimit) : selectedEntryData.SaveDataSizeForLimit);
			primaryRegion = new XUiC_DataManagementBar.BarRegion(selectedEntryData.WorldEntryInfo.BarStartOffset, size);
			XUiC_DMSavegamesList.ListEntry selectedEntryData2 = savesList.SelectedEntryData;
			if (selectedEntryData2 != null && selectedEntryData2.SaveEntryInfo.SizeInfo.UsesDataLimit)
			{
				secondaryRegion = new XUiC_DataManagementBar.BarRegion(selectedEntryData2.SaveEntryInfo.BarStartOffset, selectedEntryData2.SaveEntryInfo.SizeInfo.ReportedSize);
				if (showBlockedPlayersInBar)
				{
					long num = 0L;
					foreach (SaveInfoProvider.PlayerEntryInfo blockedPlayer in playersList.BlockedPlayers)
					{
						num += blockedPlayer.Size;
					}
					tertiaryRegion = new XUiC_DataManagementBar.BarRegion(secondaryRegion.End - num, num);
				}
				else
				{
					XUiC_DMPlayersList.ListEntry selectedEntryData3 = playersList.SelectedEntryData;
					if (selectedEntryData3 != null)
					{
						tertiaryRegion = new XUiC_DataManagementBar.BarRegion(selectedEntryData3.PlayerEntryInfo.BarStartOffset, selectedEntryData3.PlayerEntryInfo.Size);
					}
				}
			}
		}
		dataManagementBar.SetSelectedByteRegion(primaryRegion, secondaryRegion, tertiaryRegion);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string storageTargetToBtnLabel(UserDataStorageType _storageTarget)
	{
		return _storageTarget switch
		{
			UserDataStorageType.Roaming => Localization.Get("btnMoveToRoaming"), 
			UserDataStorageType.DeviceLocal => Localization.Get("btnMoveToDeviceLocal"), 
			_ => throw new Exception($"Unhandled storage target {_storageTarget}"), 
		};
	}

	public static void OpenDataManagementWindow(XUiController _parentController, Action _onClosedCallback = null)
	{
		GUIWindowManager windowManager = _parentController.xui.playerUI.windowManager;
		windowManager.Open(id, _bModal: false);
		XUiC_DataManagement xUiC_DataManagement = ((XUiWindowGroup)windowManager.GetWindow(id))?.Controller?.GetChildByType<XUiC_DataManagement>();
		if (xUiC_DataManagement == null)
		{
			UnityEngine.Debug.LogError("Failed to retrieve reference to XUiC_DataManagement instance.");
		}
		else
		{
			xUiC_DataManagement.onClosedCallback = _onClosedCallback;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeDataManagementWindow()
	{
		xui.playerUI.windowManager.Close(windowGroup);
		onClosedCallback?.Invoke();
	}

	public static bool IsWindowOpen(XUi _xui)
	{
		return _xui.playerUI.windowManager.IsWindowOpen(id);
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DMWorldList worldList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DMSavegamesList savesList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DMPlayersList playersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.PlayerEntryInfoPlatformDataResolver playerEntryResolver;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDeleteWorld;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDeleteSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnArchiveSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDeletePlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDeleteBlockedPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ConfirmationPrompt confirmationPrompt;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public string invalidFontColor = "255,0,0";

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultFontColor = "255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool worldDeletable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveDeletable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool archiveButtonVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.SaveSizeInfo archivableSizeInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveVersionValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerDeletable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBlockedPlayersInBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView previousLockView;

	[PublicizedFrom(EAccessModifier.Private)]
	public ParentControllerState m_parentControllerState;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onClosedCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dataManagementBarEnabled;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		worldList = GetChildById("worlds") as XUiC_DMWorldList;
		worldList.SelectionChanged += WorldList_SelectionChanged;
		worldList.OnEntryClicked += WorldList_OnEntryClicked;
		worldList.OnChildElementHovered += WorldList_OnChildElementHovered;
		savesList = GetChildById("saves") as XUiC_DMSavegamesList;
		savesList.SelectionChanged += SavesList_SelectionChanged;
		savesList.OnEntryClicked += SavesList_OnEntryClicked;
		savesList.OnChildElementHovered += SavesList_OnChildElementHovered;
		playersList = GetChildById("players") as XUiC_DMPlayersList;
		playersList.SelectionChanged += PlayersList_SelectionChanged;
		playersList.OnChildElementHovered += PlayersList_OnChildElementHovered;
		if (GetChildById("btnBack") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnBack_OnPressed;
		}
		btnDeleteWorld = (XUiC_SimpleButton)GetChildById("btnDeleteWorld");
		btnDeleteSave = (XUiC_SimpleButton)GetChildById("btnDeleteSave");
		btnArchiveSave = (XUiC_SimpleButton)GetChildById("btnArchiveSave");
		btnDeletePlayer = (XUiC_SimpleButton)GetChildById("btnDeletePlayer");
		btnDeleteBlockedPlayers = (XUiC_SimpleButton)GetChildById("btnDeleteBlockedPlayers");
		btnDeleteWorld.OnPressed += BtnDeleteWorld_OnPressed;
		btnDeleteWorld.OnHovered += BtnDeleteWorld_OnHover;
		btnDeleteSave.OnPressed += BtnDeleteSave_OnPressed;
		btnDeleteSave.OnHovered += BtnDeleteSave_OnHover;
		btnArchiveSave.OnPressed += BtnArchiveSave_OnPressed;
		btnArchiveSave.OnHovered += BtnArchiveSave_OnHover;
		btnDeletePlayer.OnPressed += BtnDeletePlayer_OnPressed;
		btnDeletePlayer.OnHovered += BtnDeletePlayer_OnHover;
		btnDeleteBlockedPlayers.OnPressed += BtnDeleteBlockedPlayers_OnPressed;
		btnDeleteBlockedPlayers.OnHovered += BtnDeleteBlockedPlayers_OnHover;
		confirmationPrompt = GetChildById("confirmation_prompt_controller") as XUiC_ConfirmationPrompt;
		dataManagementBar = GetChildById("data_bar_controller") as XUiC_DataManagementBar;
		dataManagementBarEnabled = dataManagementBar != null && SaveInfoProvider.DataLimitEnabled;
		IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		windowGroup.openWindowOnEsc = XUiC_EditingTools.ID;
		if (dataManagementBarEnabled)
		{
			dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			dataManagementBar.SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion.None);
		}
		Refresh();
		if (!worldList.SelectByKey(SaveInfoProvider.GetWorldEntryKey(GamePrefs.GetString(EnumGamePrefs.GameWorld), "Local")))
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
		if (dataManagementBarEnabled)
		{
			dataManagementBar.SetSelectionDepth((savesList.SelectedEntryIndex != -1) ? XUiC_DataManagementBar.SelectionDepth.Secondary : XUiC_DataManagementBar.SelectionDepth.Primary);
		}
		RefreshButtonStates();
		UpdateBarSelectionValues();
		previousLockView = base.xui.playerUI.CursorController.lockNavigationToView;
		base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent);
		base.WindowGroup.isEscClosable = false;
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		worldList.ClearList();
		savesList.ClearList();
		playersList.ClearList(_resetFilter: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Refresh()
	{
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		worldList.RebuildList(instance.WorldEntryInfos);
		savesList.RebuildList(instance.SaveEntryInfos);
		if (dataManagementBarEnabled)
		{
			dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
			dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.playerInput != null && base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			if (!confirmationPrompt.IsVisible)
			{
				CloseDataManagementWindow();
				return;
			}
			confirmationPrompt.Cancel();
		}
		if (playerEntryResolver != null && playerEntryResolver.IsComplete)
		{
			playersList.RebuildList(playerEntryResolver.pendingPlayerEntries);
			playerEntryResolver = null;
			IsDirty = true;
		}
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_SelectionChanged(XUiC_ListEntry<XUiC_DMWorldList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_DMWorldList.ListEntry> _newEntry)
	{
		string worldFilter = "";
		if (_newEntry != null)
		{
			worldFilter = _newEntry.GetEntry()?.Key;
		}
		savesList.SetWorldFilter(worldFilter);
		savesList.ClearSelection();
		playersList.ClearSelection();
		playersList.ClearList(_resetFilter: true);
		RefreshButtonStates();
		UpdateBarSelectionValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_OnEntryClicked(XUiController _sender, int _mouseButton)
	{
		savesList.ClearSelection();
		playersList.ClearSelection();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_SelectionChanged(XUiC_ListEntry<XUiC_DMSavegamesList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_DMSavegamesList.ListEntry> _newEntry)
	{
		XUiC_DMSavegamesList.ListEntry saveEntry = _newEntry?.GetEntry();
		if (saveEntry != null)
		{
			IEnumerable<SaveInfoProvider.PlayerEntryInfo> playerEntries = SaveInfoProvider.Instance.PlayerEntryInfos.Where([PublicizedFrom(EAccessModifier.Internal)] (SaveInfoProvider.PlayerEntryInfo entry) => entry.SaveEntry.WorldEntry.WorldKey.Equals(saveEntry.worldKey) && entry.SaveEntry.Name.Equals(saveEntry.saveName));
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
		RefreshButtonStates();
		UpdateBarSelectionValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnEntryClicked(XUiController _sender, int _mouseButton)
	{
		playersList.ClearSelection();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayersList_SelectionChanged(XUiC_ListEntry<XUiC_DMPlayersList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_DMPlayersList.ListEntry> _newEntry)
	{
		RefreshButtonStates();
		UpdateBarSelectionValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_OnChildElementHovered(XUiController _sender, bool _isOver)
	{
		if (!dataManagementBarEnabled)
		{
			return;
		}
		if (_sender is XUiC_ListEntry<XUiC_DMWorldList.ListEntry> xUiC_ListEntry)
		{
			if (_isOver)
			{
				SaveInfoProvider.WorldEntryInfo worldEntryInfo = xUiC_ListEntry.GetEntry()?.WorldEntryInfo;
				if (worldEntryInfo != null)
				{
					long size = (worldEntryInfo.Deletable ? (worldEntryInfo.SaveDataSize + worldEntryInfo.WorldDataSize) : worldEntryInfo.SaveDataSize);
					XUiC_DataManagementBar.BarRegion hoveredByteRegion = new XUiC_DataManagementBar.BarRegion(worldEntryInfo.BarStartOffset, size);
					dataManagementBar.SetHoveredByteRegion(hoveredByteRegion);
					goto IL_0076;
				}
			}
			dataManagementBar.SetHoveredByteRegion(XUiC_DataManagementBar.BarRegion.None);
		}
		goto IL_0076;
		IL_0076:
		dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnChildElementHovered(XUiController _sender, bool _isOver)
	{
		if (!dataManagementBarEnabled)
		{
			return;
		}
		if (_sender is XUiC_ListEntry<XUiC_DMSavegamesList.ListEntry> xUiC_ListEntry)
		{
			if (_isOver)
			{
				SaveInfoProvider.SaveEntryInfo saveEntryInfo = xUiC_ListEntry.GetEntry()?.saveEntryInfo;
				if (saveEntryInfo != null)
				{
					XUiC_DataManagementBar.BarRegion hoveredByteRegion = new XUiC_DataManagementBar.BarRegion(saveEntryInfo.BarStartOffset, saveEntryInfo.SizeInfo.ReportedSize);
					dataManagementBar.SetHoveredByteRegion(hoveredByteRegion);
					goto IL_0062;
				}
			}
			dataManagementBar.SetHoveredByteRegion(XUiC_DataManagementBar.BarRegion.None);
		}
		goto IL_0062;
		IL_0062:
		dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Secondary);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayersList_OnChildElementHovered(XUiController _sender, bool _isOver)
	{
		if (!dataManagementBarEnabled)
		{
			return;
		}
		if (_sender is XUiC_ListEntry<XUiC_DMPlayersList.ListEntry> xUiC_ListEntry)
		{
			if (_isOver)
			{
				SaveInfoProvider.PlayerEntryInfo playerEntryInfo = xUiC_ListEntry.GetEntry()?.playerEntryInfo;
				if (playerEntryInfo != null)
				{
					XUiC_DataManagementBar.BarRegion hoveredByteRegion = new XUiC_DataManagementBar.BarRegion(playerEntryInfo.BarStartOffset, playerEntryInfo.Size);
					dataManagementBar.SetHoveredByteRegion(hoveredByteRegion);
					goto IL_005d;
				}
			}
			dataManagementBar.SetHoveredByteRegion(XUiC_DataManagementBar.BarRegion.None);
		}
		goto IL_005d;
		IL_005d:
		dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Tertiary);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		CloseDataManagementWindow();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteWorld_OnPressed(XUiController _sender, int _mouseButton)
	{
		int num = savesList.GetSavesInWorld(worldList.SelectedEntry?.GetEntry().Key).Count();
		confirmationPrompt.ShowPrompt(Localization.Get("xuiWorldDelete"), Localization.Get((num > 0) ? string.Format(Localization.Get("xuiWorldDeleteConfirmation"), num) : Localization.Get("xuiWorldDeleteConfirmationNoSaves")), Localization.Get("xuiCancel"), Localization.Get("btnConfirm"), OnDeleteWorldPromptConfirmationResult);
		dataManagementBar.SetDeleteWindowDisplayed(displayed: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteWorld_OnHover(XUiController _sender, bool _isOver)
	{
		if (dataManagementBarEnabled)
		{
			dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
			dataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		bool flag = worldList.SelectedEntry?.GetEntry().Type == SaveInfoProvider.RemoteWorldsType;
		confirmationPrompt.ShowPrompt(Localization.Get(flag ? "xuiDmDeleteRemoteSave" : "xuiDeleteSaveGame"), Localization.Get(flag ? "xuiDmDeleteRemoteSaveConfirmation" : "xuiDeleteSaveGame"), Localization.Get("xuiCancel"), Localization.Get("btnConfirm"), OnDeleteSavePromptConfirmationResult);
		dataManagementBar.SetDeleteWindowDisplayed(displayed: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnHover(XUiController _sender, bool _isOver)
	{
		if (dataManagementBarEnabled)
		{
			dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Secondary);
			dataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnArchiveSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DMSavegamesList.ListEntry listEntry = savesList?.SelectedEntry?.GetEntry();
		if (listEntry == null)
		{
			return;
		}
		int selectedEntryIndex = worldList.SelectedEntryIndex;
		int selectedEntryIndex2 = savesList.SelectedEntryIndex;
		string text = Path.Combine(listEntry.saveDirectory, "archived.flag");
		if (listEntry.saveEntryInfo.SizeInfo.IsArchived)
		{
			long num = archivableSizeInfo.BytesReserved - archivableSizeInfo.BytesOnDisk;
			if (SaveInfoProvider.Instance.TotalAvailableBytes < num)
			{
				confirmationPrompt.ShowPrompt(Localization.Get("xuiDmRestoreFailureTitle"), Localization.Get("xuiDmRestoreFailureBody"), Localization.Get("xuiOk"), string.Empty, OnRestoreFailurePromptConfirmationResult);
				return;
			}
			Log.Out("Unarchiving save by deleting: " + text);
			SdFile.Delete(text);
		}
		else
		{
			Log.Out("Archiving save by creating: " + text);
			using (SdFile.Create(text))
			{
			}
		}
		SaveInfoProvider.Instance.SetDirty();
		Refresh();
		worldList.SelectedEntryIndex = selectedEntryIndex;
		savesList.SelectedEntryIndex = selectedEntryIndex2;
		SetBarArchivePreviewModeActive(_isActive: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDeleteWorldPromptConfirmationResult(XUiC_ConfirmationPrompt.Result result)
	{
		ProcessDeletionConfirmationResult(DeletionMode.World, result);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDeleteSavePromptConfirmationResult(XUiC_ConfirmationPrompt.Result result)
	{
		ProcessDeletionConfirmationResult(DeletionMode.Save, result);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDeletePlayerPromptConfirmationResult(XUiC_ConfirmationPrompt.Result result)
	{
		ProcessDeletionConfirmationResult(DeletionMode.Player, result);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDeleteBlockedPlayersPromptConfirmationResult(XUiC_ConfirmationPrompt.Result result)
	{
		ProcessDeletionConfirmationResult(DeletionMode.BlockedPlayers, result);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRestoreFailurePromptConfirmationResult(XUiC_ConfirmationPrompt.Result result)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			SetBarArchivePreviewModeActive(_isActive: false);
		}
		base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent, btnArchiveSave.ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnArchiveSave_OnHover(XUiController _sender, bool _isOver)
	{
		if (!confirmationPrompt.IsVisible)
		{
			SetBarArchivePreviewModeActive(_isOver);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBarArchivePreviewModeActive(bool _isActive)
	{
		if (!dataManagementBarEnabled)
		{
			return;
		}
		dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Secondary);
		XUiC_DMSavegamesList.ListEntry listEntry = savesList.SelectedEntry?.GetEntry();
		if (_isActive && archiveButtonVisible && listEntry != null)
		{
			long num = archivableSizeInfo.BytesReserved - archivableSizeInfo.BytesOnDisk;
			if (archivableSizeInfo.IsArchived)
			{
				dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
				dataManagementBar.SetPendingBytes(num);
				return;
			}
			dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			long offset = listEntry.saveEntryInfo.BarStartOffset + listEntry.saveEntryInfo.SizeInfo.BytesOnDisk;
			XUiC_DataManagementBar.BarRegion archivePreviewRegion = new XUiC_DataManagementBar.BarRegion(offset, num);
			dataManagementBar.SetArchivePreviewRegion(archivePreviewRegion);
		}
		else
		{
			dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeletePlayer_OnPressed(XUiController _sender, int _mouseButton)
	{
		confirmationPrompt.ShowPrompt(Localization.Get("xuiDmDeletePlayer"), Localization.Get("xuiDmDeletePlayerConfirmation"), Localization.Get("xuiCancel"), Localization.Get("btnConfirm"), OnDeletePlayerPromptConfirmationResult);
		dataManagementBar.SetDeleteWindowDisplayed(displayed: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeletePlayer_OnHover(XUiController _sender, bool _isOver)
	{
		if (dataManagementBarEnabled)
		{
			dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Tertiary);
			dataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteBlockedPlayers_OnPressed(XUiController _sender, int _mouseButton)
	{
		playersList.ClearSelection();
		showBlockedPlayersInBar = true;
		UpdateBarSelectionValues();
		confirmationPrompt.ShowPrompt(Localization.Get("xuiDmDeleteBlockedPlayers"), Localization.Get("xuiDmDeletePlayerConfirmation"), Localization.Get("xuiCancel"), Localization.Get("btnConfirm"), OnDeleteBlockedPlayersPromptConfirmationResult);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteBlockedPlayers_OnHover(XUiController _sender, bool _isOver)
	{
		if (!confirmationPrompt.IsVisible && _isOver != showBlockedPlayersInBar)
		{
			showBlockedPlayersInBar = _isOver;
			UpdateBarSelectionValues();
		}
		if (dataManagementBarEnabled)
		{
			dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Tertiary);
			dataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessDeletionConfirmationResult(DeletionMode deletionMode, XUiC_ConfirmationPrompt.Result result)
	{
		if (result == XUiC_ConfirmationPrompt.Result.Confirmed)
		{
			ProcessDeletion(deletionMode);
		}
		else
		{
			CancelDeletion(deletionMode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CancelDeletion(DeletionMode deletionMode)
	{
		XUiController xUiController = deletionMode switch
		{
			DeletionMode.World => btnDeleteWorld, 
			DeletionMode.Save => btnDeleteSave, 
			DeletionMode.Player => btnDeletePlayer, 
			_ => null, 
		};
		dataManagementBar.SetDeleteWindowDisplayed(displayed: false);
		base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent, xUiController?.ViewComponent);
		showBlockedPlayersInBar = false;
		UpdateBarSelectionValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessDeletion(DeletionMode deletionMode)
	{
		int num = worldList.SelectedEntryIndex;
		int num2 = savesList.SelectedEntryIndex;
		int num3 = playersList.SelectedEntryIndex;
		XUiController xUiController = null;
		switch (deletionMode)
		{
		case DeletionMode.World:
			try
			{
				XUiC_DMWorldList.ListEntry listEntry2 = worldList.SelectedEntry?.GetEntry();
				if (listEntry2 == null)
				{
					throw new NotSupportedException("Failed to retrieve selected world.");
				}
				if (!listEntry2.Deletable)
				{
					throw new NotSupportedException("Tried to delete non-generated world.");
				}
				if (listEntry2.Location == PathAbstractions.AbstractedLocation.None)
				{
					throw new NotSupportedException("Tried to delete world entry with location == none.");
				}
				GameUtils.DeleteWorld(listEntry2.Location);
				if (GamePrefs.GetString(EnumGamePrefs.GameWorld).EqualsCaseInsensitive(listEntry2.Location.Name))
				{
					GamePrefs.Set(EnumGamePrefs.GameWorld, "Navezgane");
				}
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
				SdDirectory.Delete((savesList.SelectedEntry?.GetEntry() ?? throw new NotSupportedException("Failed to retrieve selected save.")).saveDirectory, recursive: true);
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
				XUiC_DMPlayersList.ListEntry listEntry = playersList.SelectedEntry?.GetEntry();
				if (listEntry == null)
				{
					throw new Exception("Failed to retrieve selected player entry.");
				}
				string text2 = (savesList.SelectedEntry?.GetEntry() ?? throw new Exception("Failed to retrieve selected save entry.")).saveDirectory + "/Player";
				if (!SdDirectory.Exists(text2))
				{
					throw new Exception("Player save data directory not found at expected path: " + text2 + ".");
				}
				SdFileSystemInfo[] fileSystemInfos = new SdDirectoryInfo(text2).GetFileSystemInfos(listEntry.id + "*");
				foreach (SdFileSystemInfo sdFileSystemInfo in fileSystemInfos)
				{
					if (string.IsNullOrEmpty(sdFileSystemInfo.Extension) && SdDirectory.Exists(sdFileSystemInfo.FullName))
					{
						SdDirectory.Delete(sdFileSystemInfo.FullName);
					}
					else
					{
						SdFile.Delete(sdFileSystemInfo.FullName);
					}
				}
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
				string text = (savesList.SelectedEntry?.GetEntry() ?? throw new Exception("Failed to retrieve selected save entry.")).saveDirectory + "/Player";
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
		SaveInfoProvider.Instance.SetDirty();
		Refresh();
		worldList.SelectedEntryIndex = num;
		savesList.SelectedEntryIndex = num2;
		playersList.SelectedEntryIndex = num3;
		dataManagementBar.SetDeleteWindowDisplayed(displayed: false);
		base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent, xUiController?.ViewComponent);
	}

	[Conditional("DELETION_LOG_ENABLED")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void LogDeletion(string v)
	{
		UnityEngine.Debug.Log(v);
	}

	public static int BytesToMebibytes(long bytes)
	{
		return Mathf.CeilToInt((float)bytes / 1024f / 1024f);
	}

	public static string FormatMemoryString(long bytes)
	{
		return BytesToMebibytes(bytes) + " MB";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshButtonStates()
	{
		XUiC_DMWorldList.ListEntry listEntry = worldList?.SelectedEntry?.GetEntry();
		worldDeletable = listEntry != null && listEntry.Deletable && !SaveInfoProvider.Instance.IsDirectoryProtected(listEntry.Location.FullPath);
		XUiC_DMSavegamesList.ListEntry listEntry2 = savesList?.SelectedEntry?.GetEntry();
		if (listEntry2 != null)
		{
			saveDeletable = !SaveInfoProvider.Instance.IsDirectoryProtected(listEntry2.saveDirectory);
			archiveButtonVisible = saveDeletable && listEntry2.saveEntryInfo.SizeInfo.Archivable;
			archivableSizeInfo = listEntry2.saveEntryInfo.SizeInfo;
		}
		else
		{
			saveDeletable = false;
			archiveButtonVisible = false;
		}
		playerDeletable = saveDeletable && playersList.SelectedEntryIndex >= 0;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarSelectionValues()
	{
		if (!dataManagementBarEnabled)
		{
			return;
		}
		XUiC_DataManagementBar.BarRegion primaryRegion = XUiC_DataManagementBar.BarRegion.None;
		XUiC_DataManagementBar.BarRegion secondaryRegion = XUiC_DataManagementBar.BarRegion.None;
		XUiC_DataManagementBar.BarRegion tertiaryRegion = XUiC_DataManagementBar.BarRegion.None;
		XUiC_DMWorldList.ListEntry listEntry = worldList.SelectedEntry?.GetEntry();
		if (listEntry != null)
		{
			long size = (listEntry.Deletable ? (listEntry.SaveDataSize + listEntry.WorldDataSize) : listEntry.SaveDataSize);
			primaryRegion = new XUiC_DataManagementBar.BarRegion(listEntry.WorldEntryInfo.BarStartOffset, size);
			XUiC_DMSavegamesList.ListEntry listEntry2 = savesList.SelectedEntry?.GetEntry();
			if (listEntry2 != null)
			{
				secondaryRegion = new XUiC_DataManagementBar.BarRegion(listEntry2.saveEntryInfo.BarStartOffset, listEntry2.saveEntryInfo.SizeInfo.ReportedSize);
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
					XUiC_DMPlayersList.ListEntry listEntry3 = playersList.SelectedEntry?.GetEntry();
					if (listEntry3 != null)
					{
						tertiaryRegion = new XUiC_DataManagementBar.BarRegion(listEntry3.playerEntryInfo.BarStartOffset, listEntry3.playerEntryInfo.Size);
					}
				}
			}
		}
		dataManagementBar.SetSelectedByteRegion(primaryRegion, secondaryRegion, tertiaryRegion);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (!(_name == "default_font_color"))
		{
			if (_name == "invalid_font_color")
			{
				invalidFontColor = _value;
				return true;
			}
			return base.ParseAttribute(_name, _value, _parent);
		}
		defaultFontColor = _value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "showbar":
			_value = dataManagementBarEnabled.ToString();
			return true;
		case "false":
			_value = "false";
			return true;
		case "saveversioncolor":
			_value = (saveVersionValid ? defaultFontColor : invalidFontColor);
			return true;
		case "savedeletable":
			_value = saveDeletable.ToString();
			return true;
		case "worlddeletable":
			_value = worldDeletable.ToString();
			return true;
		case "playerdeletable":
			_value = playerDeletable.ToString();
			return true;
		case "archivelabel":
			if (archiveButtonVisible)
			{
				long num = archivableSizeInfo.BytesReserved - archivableSizeInfo.BytesOnDisk;
				long num2 = BytesToMebibytes(archivableSizeInfo.BytesReserved) - BytesToMebibytes(archivableSizeInfo.BytesOnDisk);
				if (archivableSizeInfo.IsArchived)
				{
					if (SaveInfoProvider.Instance.TotalAvailableBytes >= num)
					{
						_value = string.Format("{0} ({1} MB)", Localization.Get("xuiDmRestoreLabel"), num2);
					}
					else
					{
						_value = string.Format("{0} ([ff9999]{1} MB[-])", Localization.Get("xuiDmRestoreLabel"), num2);
					}
				}
				else
				{
					_value = string.Format("{0} ({1} MB)", Localization.Get("xuiDmArchiveLabel"), num2);
				}
			}
			else
			{
				_value = string.Empty;
			}
			return true;
		case "savearchivable":
			_value = archiveButtonVisible.ToString();
			return true;
		case "blockedplayers":
		{
			XUiC_DMPlayersList xUiC_DMPlayersList = playersList;
			if (xUiC_DMPlayersList != null && xUiC_DMPlayersList.HasBlockedPlayers)
			{
				_value = string.Format(Localization.Get("xuiDmBlockedPlayers"), playersList.BlockedPlayerCount);
			}
			else
			{
				_value = string.Empty;
			}
			return true;
		}
		case "hasblockedplayers":
			_value = playersList?.HasBlockedPlayers.ToString() ?? string.Empty;
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public static void OpenDataManagementWindow(XUiController parentController, Action onClosedCallback = null)
	{
		GUIWindowManager windowManager = parentController.xui.playerUI.windowManager;
		windowManager.Open(ID, _bModal: true, _bIsNotEscClosable: false, _bCloseAllOpenWindows: false);
		XUiC_DataManagement xUiC_DataManagement = ((XUiWindowGroup)windowManager.GetWindow(ID))?.Controller?.GetChildByType<XUiC_DataManagement>();
		if (xUiC_DataManagement == null)
		{
			UnityEngine.Debug.LogError("Failed to retrieve reference to XUiC_DataManagement instance.");
			return;
		}
		xUiC_DataManagement.SetParentController(parentController);
		xUiC_DataManagement.onClosedCallback = onClosedCallback;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetParentController(XUiController parentController)
	{
		m_parentControllerState = new ParentControllerState(parentController);
		m_parentControllerState.Hide();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseDataManagementWindow()
	{
		base.xui.playerUI.CursorController.SetNavigationLockView(previousLockView);
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		m_parentControllerState.Restore();
		onClosedCallback?.Invoke();
	}

	public static bool IsWindowOpen(XUi xui)
	{
		return xui.playerUI.windowManager.IsWindowOpen(ID);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CreateWorld : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct WorldSavePair
	{
		public string worldName;

		public string saveName;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MinWorldSize = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleSelectWorld;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorldList worldList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SavegamesList savesList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleNewWorld;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtNameInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<int> cmbSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel deleteSavePanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label deleteSaveText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label deleteHeaderText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label worldLastPlayedLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label worldVersionLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label worldMemoryLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label saveLastPlayedLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label saveVersionLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label saveMemoryLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public string invalidFontColor = "255,0,0";

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultFontColor = "255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool worlddeletable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool savedeletable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveVersionValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool worldVersionValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, long> worldDataSizeCache = new Dictionary<string, long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<WorldSavePair, long> saveSizeCache = new Dictionary<WorldSavePair, long>();

	public bool IsCustomSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (cmbSize != null)
			{
				return cmbSize.Value == -1;
			}
			return false;
		}
	}

	public int NewWorldSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!IsCustomSize)
			{
				return cmbSize?.Value ?? (-1);
			}
			if (txtSize != null && int.TryParse(txtSize.Text, out var result))
			{
				return result;
			}
			return -1;
		}
	}

	public bool CustomSizeValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (txtSize != null && int.TryParse(txtSize.Text, out var result) && result >= 1024)
			{
				return result % 1024 == 0;
			}
			return false;
		}
	}

	public bool CustomNameValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (txtNameInput != null && txtNameInput.Text.Trim().Length > 0)
			{
				return !SdDirectory.Exists(GameIO.GetGameDir("Data/Worlds") + "/" + txtNameInput.Text);
			}
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		toggleSelectWorld = GetChildById("toggleSelectWorld").GetChildByType<XUiC_ToggleButton>();
		toggleSelectWorld.OnValueChanged += ToggleSelectWorld_OnValueChanged;
		worldList = GetChildById("worlds") as XUiC_WorldList;
		worldList.SelectionChanged += WorldList_SelectionChanged;
		worldList.OnEntryDoubleClicked += WorldList_OnEntryDoubleClicked;
		worldList.OnEntryClicked += WorldList_OnEntryClicked;
		savesList = GetChildById("saves") as XUiC_SavegamesList;
		savesList.SelectionChanged += SavesList_SelectionChanged;
		toggleNewWorld = GetChildById("toggleNewWorld")?.GetChildByType<XUiC_ToggleButton>();
		txtNameInput = GetChildById("nameInput") as XUiC_TextInput;
		txtSize = GetChildById("txtSize") as XUiC_TextInput;
		cmbSize = GetChildById("cmbSize").GetChildByType<XUiC_ComboBoxList<int>>();
		toggleNewWorld.OnValueChanged += ToggleNewWorld_OnValueChanged;
		txtNameInput.OnChangeHandler += TxtInput_OnChangeHandler;
		txtSize.OnChangeHandler += TxtInput_OnChangeHandler;
		cmbSize.OnValueChanged += CmbSize_OnValueChanged;
		txtSize.UIInputController.OnScroll += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, float _args) =>
		{
			cmbSize.ScrollEvent(_sender, _args);
		};
		if (GetChildById("btnBack") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnBack_OnPressed;
		}
		if (GetChildById("btnDeleteWorld") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += BtnDeleteWorld_OnPressed;
		}
		if (GetChildById("btnDeleteSave") is XUiC_SimpleButton xUiC_SimpleButton3)
		{
			xUiC_SimpleButton3.OnPressed += BtnDeleteSave_OnPressed;
		}
		if (GetChildById("btnStart") is XUiC_SimpleButton xUiC_SimpleButton4)
		{
			xUiC_SimpleButton4.OnPressed += BtnStart_OnPressed;
		}
		deleteSavePanel = (XUiV_Panel)GetChildById("deleteSavePanel").ViewComponent;
		XUiC_SimpleButton xUiC_SimpleButton5 = (XUiC_SimpleButton)deleteSavePanel.Controller.GetChildById("btnCancel");
		XUiC_SimpleButton obj = (XUiC_SimpleButton)deleteSavePanel.Controller.GetChildById("btnConfirm");
		xUiC_SimpleButton5.OnPressed += BtnCancelDelete_OnPressed;
		obj.OnPressed += BtnConfirmDelete_OnPressed;
		deleteSaveText = (XUiV_Label)deleteSavePanel.Controller.GetChildById("deleteText").ViewComponent;
		deleteHeaderText = (XUiV_Label)deleteSavePanel.Controller.GetChildById("headerText").ViewComponent;
		XUiV_Panel xUiV_Panel = (XUiV_Panel)GetChildById("worldInfo").ViewComponent;
		worldLastPlayedLabel = (XUiV_Label)xUiV_Panel.Controller.GetChildById("worldLastPlayedLabel").ViewComponent;
		worldVersionLabel = (XUiV_Label)xUiV_Panel.Controller.GetChildById("worldVersionLabel").ViewComponent;
		worldMemoryLabel = (XUiV_Label)xUiV_Panel.Controller.GetChildById("worldMemoryLabel").ViewComponent;
		XUiV_Panel xUiV_Panel2 = (XUiV_Panel)GetChildById("saveInfo").ViewComponent;
		saveLastPlayedLabel = (XUiV_Label)xUiV_Panel2.Controller.GetChildById("saveLastPlayedLabel").ViewComponent;
		saveVersionLabel = (XUiV_Label)xUiV_Panel2.Controller.GetChildById("saveVersionLabel").ViewComponent;
		saveMemoryLabel = (XUiV_Label)xUiV_Panel2.Controller.GetChildById("saveMemoryLabel").ViewComponent;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		worldDataSizeCache.Clear();
		saveSizeCache.Clear();
		windowGroup.openWindowOnEsc = XUiC_EditingTools.ID;
		worldList.RebuildList();
		txtNameInput.Text = GamePrefs.GetString(EnumGamePrefs.CreateLevelName);
		if (int.TryParse(GamePrefs.GetString(EnumGamePrefs.CreateLevelDim), out var result))
		{
			if (cmbSize.Elements.Contains(result))
			{
				cmbSize.Value = result;
			}
			else
			{
				cmbSize.SelectedIndex = 0;
				txtSize.Text = result.ToString();
			}
		}
		else
		{
			txtSize.Text = "";
		}
		deleteSavePanel.IsVisible = false;
		toggleSelectWorld.Value = true;
		toggleNewWorld.Value = false;
		if (string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.GameWorld)) || !worldList.SelectByName(GamePrefs.GetString(EnumGamePrefs.GameWorld)))
		{
			worldList.SelectedEntryIndex = 0;
		}
		RefreshButtonStates();
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		worldDataSizeCache.Clear();
		saveSizeCache.Clear();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleSelectWorld_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			toggleNewWorld.Value = false;
		}
		else
		{
			toggleSelectWorld.Value = true;
		}
		RefreshButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_SelectionChanged(XUiC_ListEntry<XUiC_WorldList.WorldListEntry> _previousEntry, XUiC_ListEntry<XUiC_WorldList.WorldListEntry> _newEntry)
	{
		string worldFilter = "";
		if (_newEntry != null)
		{
			toggleSelectWorld.Value = true;
			toggleNewWorld.Value = false;
			worldFilter = _newEntry.GetEntry()?.Location.Name;
		}
		savesList.SetWorldFilter(worldFilter);
		savesList.RebuildList();
		RefreshInfoLabels();
		RefreshButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_OnEntryClicked(XUiController _sender, int _mouseButton)
	{
		savesList.ClearSelection();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_SelectionChanged(XUiC_ListEntry<XUiC_SavegamesList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_SavegamesList.ListEntry> _newEntry)
	{
		RefreshInfoLabels();
		RefreshButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_OnEntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		toggleSelectWorld.Value = true;
		toggleNewWorld.Value = false;
		if (worldList.SelectedEntryIndex >= 0)
		{
			BtnStart_OnPressed(_sender, _mouseButton);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		worldList.ClearSelection();
		toggleSelectWorld.Value = false;
		toggleNewWorld.Value = true;
		RefreshButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleNewWorld_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			worldList.ClearSelection();
			toggleSelectWorld.Value = false;
		}
		else
		{
			toggleNewWorld.Value = true;
		}
		RefreshButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbSize_OnValueChanged(XUiController _sender, int _oldvalue, int _newvalue)
	{
		RefreshButtonStates();
		if (_newvalue < 0 && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			txtSize.SetSelected(_selected: true, _delayed: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_EditingTools.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteWorld_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteWorldGetConfirmation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void deleteWorldGetConfirmation()
	{
		deleteSavePanel.IsVisible = true;
		deleteHeaderText.Text = Localization.Get("xuiWorldDelete");
		int num = savesList.GetSavesInWorld(worldList.SelectedEntry?.GetEntry().Location.Name).Count();
		if (num > 0)
		{
			deleteSaveText.Text = string.Format(Localization.Get("xuiWorldDeleteConfirmation"), num);
		}
		else
		{
			deleteSaveText.Text = Localization.Get("xuiWorldDeleteConfirmationNoSaves");
		}
		base.xui.playerUI.CursorController.SetNavigationLockView(deleteSavePanel, deleteSavePanel.Controller.GetChildById("btnCancel").ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteSaveGetConfirmation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void deleteSaveGetConfirmation()
	{
		deleteSavePanel.IsVisible = true;
		deleteHeaderText.Text = Localization.Get("xuiDeleteSaveGame");
		deleteSaveText.Text = Localization.Get("xuiDeleteSaveGame");
		base.xui.playerUI.CursorController.SetNavigationLockView(deleteSavePanel, deleteSavePanel.Controller.GetChildById("btnCancel").ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteSavePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		if (savesList.SelectedEntry != null)
		{
			GetChildById("btnDeleteSave").SelectCursorElement();
		}
		else
		{
			GetChildById("btnDeleteWorld").SelectCursorElement();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteSavePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		XUiC_SavegamesList.ListEntry listEntry = savesList.SelectedEntry?.GetEntry();
		if (listEntry != null)
		{
			string saveGameDir = GameIO.GetSaveGameDir(listEntry.worldName, listEntry.saveName);
			if (SdDirectory.Exists(saveGameDir))
			{
				SdDirectory.Delete(saveGameDir, recursive: true);
			}
			savesList.RebuildList();
			GetChildById("btnDeleteSave").SelectCursorElement();
			return;
		}
		XUiC_WorldList.WorldListEntry entry = worldList.SelectedEntry.GetEntry();
		if (!entry.GeneratedWorld)
		{
			Log.Warning("Tried to delete non-generated world");
			return;
		}
		try
		{
			GameUtils.DeleteWorld(entry.Location);
		}
		catch (Exception ex)
		{
			Log.Error("Error occurred while deleting world: \"" + ex.Message + "\"");
		}
		worldList.RebuildList();
		worldList.SelectedEntryIndex = 0;
		GetChildById("btnDeleteWorld").SelectCursorElement();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStart_OnPressed(XUiController _sender, int _mouseButton)
	{
		new GameModeEditWorld().ResetGamePrefs();
		if (toggleNewWorld.Value)
		{
			startWithNewWorld();
		}
		else
		{
			startWithExistingWorld();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startWithNewWorld()
	{
		string text = txtNameInput.Text.Trim();
		int newWorldSize = NewWorldSize;
		GamePrefs.Set(EnumGamePrefs.GameWorld, text);
		GamePrefs.Set(EnumGamePrefs.CreateLevelName, text);
		GamePrefs.Set(EnumGamePrefs.CreateLevelDim, newWorldSize.ToString());
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		MicroStopwatch microStopwatch2 = new MicroStopwatch(_bStart: true);
		WorldStaticData.Cleanup(null);
		Log.Out($"WSD.Cleanup took {microStopwatch2.ElapsedMilliseconds} ms");
		microStopwatch2.ResetAndRestart();
		WorldStaticData.Reset(null);
		Log.Out($"WSD.Reset took {microStopwatch2.ElapsedMilliseconds} ms");
		microStopwatch2.ResetAndRestart();
		GameUtils.CreateEmptyFlatLevel(text, newWorldSize);
		Log.Out($"Creating empty world took {microStopwatch.ElapsedMilliseconds} ms");
		startEditor();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startWithExistingWorld()
	{
		PathAbstractions.AbstractedLocation selectedWorld = worldList.SelectedEntry.GetEntry().Location;
		GamePrefs.Set(EnumGamePrefs.GameWorld, selectedWorld.Name);
		if (CanSaveWorldIn(selectedWorld))
		{
			startEditor();
			return;
		}
		string text = selectedWorld.Name + "_UserCopy";
		PathAbstractions.AbstractedLocation newLocation = LocationForNewWorld(text);
		int num = 0;
		while (newLocation.Exists())
		{
			text = selectedWorld.Name + "_UserCopy" + ++num;
			newLocation = LocationForNewWorld(text);
		}
		GamePrefs.Set(EnumGamePrefs.GameWorld, text);
		XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiCreateWorldCanNotEditWorldInGameFolderTitle"), string.Format(Localization.Get("xuiCreateWorldCanNotEditWorldInGameFolderText"), GameIO.GetOsStylePath(newLocation.FullPath)), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Log.Out("Will copy the world from '" + selectedWorld.FullPath + "' to '" + newLocation.FullPath + "' for editing.");
			GameIO.CopyDirectory(selectedWorld.FullPath, newLocation.FullPath);
			startEditor();
		}, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			base.xui.playerUI.windowManager.Open(windowGroup.ID, _bModal: true);
		}, _openMainMenuOnClose: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startEditor()
	{
		GamePrefs.Set(EnumGamePrefs.GameMode, GameModeEditWorld.TypeName);
		GamePrefs.Set(EnumGamePrefs.GameName, "WorldEditor");
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), _offline: false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			(((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller as XUiC_MessageBoxWindowGroup).ShowNetworkError(networkConnectionError);
		}
	}

	public static PathAbstractions.AbstractedLocation LocationForNewWorld(string _name)
	{
		PathAbstractions.EAbstractedLocationType locationType = PathAbstractions.EAbstractedLocationType.UserDataPath;
		return PathAbstractions.WorldsSearchPaths.BuildLocation(locationType, (string)null, _name, (Mod)null, (string)null, (string)null).Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CanSaveWorldIn(PathAbstractions.AbstractedLocation _location)
	{
		return _location.Type != PathAbstractions.EAbstractedLocationType.GameData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshInfoLabels()
	{
		XUiC_WorldList.WorldListEntry worldListEntry = worldList.SelectedEntry?.GetEntry();
		XUiC_SavegamesList.ListEntry listEntry = savesList.SelectedEntry?.GetEntry();
		worldLastPlayedLabel.Text = "--";
		worldVersionLabel.Text = "--";
		worldMemoryLabel.Text = "--";
		saveLastPlayedLabel.Text = "--";
		saveVersionLabel.Text = "--";
		saveMemoryLabel.Text = "--";
		saveVersionValid = true;
		worldVersionValid = true;
		if (worldListEntry == null)
		{
			return;
		}
		if (worldListEntry.GeneratedWorld)
		{
			if (2 != worldListEntry.Version.Major)
			{
				worldVersionValid = false;
			}
			worldVersionLabel.Text = worldListEntry.Version.LongStringNoBuild;
		}
		else
		{
			worldVersionLabel.Text = VersionInformation.EGameReleaseType.V.ToString() + " " + 2;
		}
		worldMemoryLabel.Text = FormatMemoryString(GetWorldMemory(worldListEntry));
		IEnumerator<XUiC_SavegamesList.ListEntry> enumerator = savesList.GetSavesInWorld(worldListEntry.Location.Name).GetEnumerator();
		enumerator.MoveNext();
		if (enumerator.Current != null)
		{
			XUiV_Label xUiV_Label = worldLastPlayedLabel;
			DateTime lastSaved = enumerator.Current.lastSaved;
			xUiV_Label.Text = lastSaved.ToString() ?? "";
		}
		if (listEntry != null)
		{
			XUiV_Label xUiV_Label2 = saveLastPlayedLabel;
			DateTime lastSaved = listEntry.lastSaved;
			xUiV_Label2.Text = lastSaved.ToString() ?? "";
			VersionInformation gameVersion = listEntry.worldState.gameVersion;
			saveVersionLabel.Text = gameVersion.LongStringNoBuild;
			if (gameVersion.Major != 2)
			{
				saveVersionValid = false;
			}
			saveMemoryLabel.Text = FormatMemoryString(saveSizeCache[new WorldSavePair
			{
				worldName = listEntry.worldName,
				saveName = listEntry.saveName
			}]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string FormatMemoryString(long bytes)
	{
		return bytes / 1024 / 1024 + " MB";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshButtonStates()
	{
		int selectedEntryIndex = worldList.SelectedEntryIndex;
		int selectedEntryIndex2 = savesList.SelectedEntryIndex;
		if (toggleSelectWorld.Value)
		{
			startable = selectedEntryIndex >= 0 && selectedEntryIndex2 < 0;
		}
		else
		{
			startable = CustomNameValid && (!IsCustomSize || CustomSizeValid);
		}
		worlddeletable = selectedEntryIndex >= 0 && worldList.SelectedEntry.GetEntry().GeneratedWorld && toggleSelectWorld.Value;
		savedeletable = selectedEntryIndex2 >= 0;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long GetWorldMemory(XUiC_WorldList.WorldListEntry _worldEntry)
	{
		long num = 0L;
		string name = _worldEntry.Location.Name;
		if (!worldDataSizeCache.ContainsKey(name))
		{
			worldDataSizeCache.Add(name, GameIO.GetDirectorySize(_worldEntry.Location.FullPath));
		}
		num += worldDataSizeCache[name];
		foreach (XUiC_SavegamesList.ListEntry item in savesList.GetSavesInWorld(name))
		{
			WorldSavePair key = new WorldSavePair
			{
				worldName = name,
				saveName = item.saveName
			};
			if (!saveSizeCache.ContainsKey(key))
			{
				saveSizeCache.Add(key, GameIO.GetDirectorySize(GameIO.GetSaveGameDir(item.worldName, item.saveName)));
			}
			num += saveSizeCache[key];
		}
		return num;
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
		case "false":
			_value = "false";
			return true;
		case "iscustomsize":
			_value = IsCustomSize.ToString();
			return true;
		case "customsizecolor":
			_value = (CustomSizeValid ? defaultFontColor : invalidFontColor);
			return true;
		case "customsizewarning":
			_value = ((IsCustomSize && !CustomSizeValid) ? string.Format(Localization.Get("xuiCreateWorldSizeInvalidTooltip"), 1024, 1024) : "");
			return true;
		case "customnamecolor":
			_value = (CustomNameValid ? defaultFontColor : invalidFontColor);
			return true;
		case "customnamewarning":
			_value = ((!CustomNameValid) ? Localization.Get("xuiCreateWorldNameInvalidTooltip") : "");
			return true;
		case "worldversioncolor":
			_value = (worldVersionValid ? defaultFontColor : invalidFontColor);
			return true;
		case "saveversioncolor":
			_value = (saveVersionValid ? defaultFontColor : invalidFontColor);
			return true;
		case "startable":
			_value = startable.ToString();
			return true;
		case "savedeletable":
			_value = savedeletable.ToString();
			return true;
		case "worlddeletable":
			_value = worlddeletable.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}

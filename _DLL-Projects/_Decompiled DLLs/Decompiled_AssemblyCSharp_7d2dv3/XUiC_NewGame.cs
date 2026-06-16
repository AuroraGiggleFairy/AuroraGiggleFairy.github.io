using System;
using System.Collections;
using System.Collections.Generic;
using Platform;
using SandboxOptions;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewGame : XUiC_NewContinueBase
{
	public enum EWorldType
	{
		Handmade,
		ExistingRandom,
		CreateRandom
	}

	public struct GameWorldInfo
	{
		public string RealName;

		public string CustName;

		public string Description;

		public GameUtils.WorldInfo WorldInfo;

		public PathAbstractions.AbstractedLocation Location;

		public bool IsRandomWorld => WorldInfo?.RandomGeneratedWorld ?? false;

		public override string ToString()
		{
			if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional && Location.Type == PathAbstractions.EAbstractedLocationType.UserDataPath)
			{
				return RealName + " [808080][i](" + Location.StorageType.LocalizedName() + ")[/i][-]";
			}
			return RealName;
		}
	}

	public static string ID = "";

	[XuiBindComponent("cbxGameMode", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<GameMode> cbxGameMode;

	[XuiBindComponent("cbxWorldType", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<EWorldType> cbxWorldType;

	[XuiBindComponent("cbxWorldName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<GameWorldInfo> cbxWorldName;

	[XuiBindComponent("txtGameName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtGameName;

	[XuiBindComponent("btnDefaults", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDefaults;

	[XuiBindComponent("btnGenerateWorld", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnGenerateWorld;

	[XuiBindComponent("cbxSaveDataLimit", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<SaveDataLimitType> cbxSaveDataLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public string saveDataLimitTooltipKey = string.Empty;

	[XuiBindComponent("cbxGameSaveStorage", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<UserDataStorageType> cbxGameSaveStorage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string saveStorageTooltipKey = string.Empty;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationWindow worldGenerationControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumGameMode, List<GameWorldInfo>> worldsPerMode = new EnumDictionary<EnumGameMode, List<GameWorldInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public long pendingPreviewSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public string gameNameTooltip = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validGameName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmOnOpen = new ProfilerMarker("OnOpen");

	[XuiXmlBinding("gamenametooltip")]
	public string GameNameTooltip
	{
		get
		{
			return gameNameTooltip;
		}
		set
		{
			if (!(gameNameTooltip == value))
			{
				gameNameTooltip = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("gamenamevalid")]
	public bool ValidGameName
	{
		get
		{
			return validGameName;
		}
		set
		{
			if (validGameName != value)
			{
				validGameName = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("countynamevalid")]
	public bool CountyNameValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return worldGenerationControls?.ValidCountyName ?? true;
		}
	}

	public GameMode SelectedGameMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxGameMode?.Value;
		}
	}

	[XuiXmlBinding("iscontinuegame")]
	public bool IsContinueGame
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return false;
		}
	}

	[XuiXmlBinding("worldtype")]
	public EWorldType WorldType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxWorldType?.Value ?? EWorldType.Handmade;
		}
	}

	[XuiXmlBinding("saveStorageTooltip")]
	public string SaveStorageTooltipKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return saveStorageTooltipKey;
		}
	}

	[XuiXmlBinding("saveDataLimitTooltip")]
	public string SaveDataLimitTooltipKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return saveDataLimitTooltipKey;
		}
	}

	public GameWorldInfo? SelectedWorld
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxWorldName?.Value ?? default(GameWorldInfo);
		}
	}

	public GameUtils.WorldInfo SelectedWorldInfo
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SelectedWorld?.WorldInfo;
		}
	}

	[XuiXmlBinding("worldgameversion")]
	public string WorldGameVersion
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			GameUtils.WorldInfo selectedWorldInfo = SelectedWorldInfo;
			if (selectedWorldInfo == null)
			{
				return "";
			}
			if (!selectedWorldInfo.GameVersionCreated.IsValid)
			{
				return "";
			}
			string text = selectedWorldInfo.GameVersionCreated.ReleaseType.ToStringCached();
			int major = selectedWorldInfo.GameVersionCreated.Major;
			return text + " " + major;
		}
	}

	[XuiXmlBinding("differentworldversion")]
	public bool DifferentWorldVersion
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			GameUtils.WorldInfo selectedWorldInfo = SelectedWorldInfo;
			if (selectedWorldInfo == null)
			{
				return false;
			}
			if (selectedWorldInfo.GameVersionCreated.IsValid)
			{
				return !selectedWorldInfo.GameVersionCreated.EqualsMajor(Constants.cVersionInformation);
			}
			return false;
		}
	}

	[XuiXmlBinding("mapsize")]
	public string MapSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			GameUtils.WorldInfo selectedWorldInfo = SelectedWorldInfo;
			if (selectedWorldInfo == null)
			{
				return "";
			}
			return $"{selectedWorldInfo.WorldSize.x} x {selectedWorldInfo.WorldSize.y}";
		}
	}

	[XuiXmlBinding("isgenerateworld")]
	public bool IsGenerateWorld
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return WorldType == EWorldType.CreateRandom;
		}
	}

	[XuiXmlBinding("worldstorageusesdatalimit")]
	public bool WorldStorageUsesDataLimit
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SelectedWorld?.Location.StorageType.UsesDataLimit() ?? false;
		}
	}

	public override bool PlayIntroMovie
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GamePrefs.GetBool(EnumGamePrefs.OptionsIntroMovieEnabled);
		}
	}

	public override bool AllowChangingCreativeMode
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		txtGameName.UIInput.onValidate = GameUtils.ValidateGameNameInput;
		SaveDataLimitUIHelper.AddComboBox(cbxSaveDataLimit);
	}

	public override void OnOpen()
	{
		using (pmOnOpen.Auto())
		{
			IsDirty = true;
			Settings.WatchForServerEnabledChanges(_doWatch: false);
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
			string modeName = GamePrefs.GetString(EnumGamePrefs.GameMode);
			int num = cbxGameMode.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (GameMode _mode) => _mode.GetTypeName().EqualsCaseInsensitive(modeName));
			if (num < 0)
			{
				num = 0;
			}
			cbxGameMode.SelectedIndex = num;
			gameModeChanged(SelectedGameMode);
			GamePrefs.Instance.Load(GameIO.GetSaveGameRootDir() + "/newGameOptions.sdf");
			Settings.UpdateOptionValuesFromGamePrefs();
			Settings.ResetSandboxPresetToDefault();
			if (GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge) == 0)
			{
				GamePrefs.Set(EnumGamePrefs.MaxChunkAge, (int)GamePrefs.GetDefault(EnumGamePrefs.MaxChunkAge));
			}
			if (GamePrefs.GetString(EnumGamePrefs.GameName).Trim().Length < 1)
			{
				GamePrefs.Set(EnumGamePrefs.GameName, "MyGame");
			}
			txtGameName.Text = GamePrefs.GetString(EnumGamePrefs.GameName).Trim();
			validateStartable();
			base.OnOpen();
			Settings.ApplyMaxPlayerCountOptions();
			Settings.UpdateOptionVisibilityForGameMode(SelectedGameMode);
			GetChildById("txtGameName").SelectCursorElement(_withDelay: true);
			Settings.ApplyInitialServerEnabledState();
			if (base.DataManagementBarEnabled)
			{
				SaveInfoProvider.Instance.SetDirty();
				DataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
				DataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
				updateBarUsageAndAllowanceValues();
				updateBarPendingValue();
				SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Combine(SaveDataLimitUIHelper.OnValueChanged, new Action(CbxSaveDataLimit_OnValueChanged));
				worldGenerationControls.OnWorldSizeChanged += updateBarPendingValue;
			}
			cbxGameSaveStorage.Value = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Remove(SaveDataLimitUIHelper.OnValueChanged, new Action(CbxSaveDataLimit_OnValueChanged));
		worldGenerationControls.OnWorldSizeChanged -= updateBarPendingValue;
	}

	[XuiBindEvent("OnPress", "btnGenerateWorld")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRwgPreviewerOnOnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_WorldGenerationWindow.Open(xui, ID);
	}

	[XuiBindEvent("OnValueChanged", "cbxGameMode")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxGameMode_OnValueChanged(XUiController _sender, GameMode _oldValue, GameMode _newValue)
	{
		gameModeChanged(_newValue);
	}

	[XuiBindEvent("OnValueChanged", "cbxWorldType")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxWorldType_OnValueChanged(XUiController _sender, EWorldType _oldValue, EWorldType _newValue)
	{
		IsDirty = true;
		updateWorlds();
		validateStartable();
	}

	[XuiBindEvent("OnValueChanged", "cbxWorldName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxWorldName_OnValueChanged(XUiController _sender, GameWorldInfo _oldValue, GameWorldInfo _newValue)
	{
		IsDirty = true;
		GamePrefs.Set(EnumGamePrefs.GameWorld, _newValue.RealName);
		if (IsGenerateWorld)
		{
			GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType));
			worldGenerationControls.ValidateNewRwg();
		}
		else
		{
			GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)_newValue.Location.StorageType);
		}
		updateBarPendingValue();
		validateStartable();
	}

	[XuiBindEvent("OnChangeHandler", "txtGameName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtGameName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		GamePrefs.Set(EnumGamePrefs.GameName, _text);
		validateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxSaveDataLimit_OnValueChanged()
	{
		IsDirty = true;
		updateBarPendingValue();
	}

	[XuiBindEvent("OnValueChanged", "cbxGameSaveStorage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxGameSaveStorage_OnValueChanged(XUiController _sender, UserDataStorageType _oldValue, UserDataStorageType _newValue)
	{
		IsDirty = true;
		GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, (int)_newValue);
		worldGenerationControls.SetGameSaveStorageValue(_newValue);
		updateBarPendingValue();
		validateStartable();
	}

	[XuiBindEvent("OnGameSaveStorageChanged", "worldGenerationControls")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void worldGenerationGameSaveStorageChanged(UserDataStorageType _newValue)
	{
		IsDirty = true;
		cbxGameSaveStorage.Value = _newValue;
		updateBarPendingValue();
		validateStartable();
	}

	[XuiBindEvent("OnWorldStorageChanged", "worldGenerationControls")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void worldGenerationWorldSaveStorageChanged()
	{
		updateBarPendingValue();
		validateStartable();
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
			GameWorldInfo item = new GameWorldInfo
			{
				RealName = item2.Name,
				Location = item2
			};
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(item2);
			if (worldInfo == null)
			{
				item.Description = "No Description Found";
				if (!worldsPerMode.ContainsKey(EnumGameMode.Creative))
				{
					worldsPerMode.Add(EnumGameMode.Creative, new List<GameWorldInfo>());
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
						worldsPerMode.Add(_result, new List<GameWorldInfo>());
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
		EnumGameMode iD = (EnumGameMode)SelectedGameMode.GetID();
		cbxWorldName.Elements.Clear();
		if (WorldType != EWorldType.CreateRandom && worldsPerMode.TryGetValue(iD, out var value))
		{
			foreach (GameWorldInfo item in value)
			{
				if (CanAddWorld(item))
				{
					cbxWorldName.Elements.Add(item);
				}
			}
		}
		cbxWorldName.SelectedIndex = 0;
		CbxWorldName_OnValueChanged(cbxWorldName, default(GameWorldInfo), SelectedWorld.Value);
		[PublicizedFrom(EAccessModifier.Private)]
		bool CanAddWorld(GameWorldInfo _worldInfo)
		{
			if (_worldInfo.WorldInfo != null && PlatformOptimizations.EnforceMaxWorldSizeHost)
			{
				Vector2i worldSize = _worldInfo.WorldInfo.WorldSize;
				if (worldSize.x > PlatformOptimizations.MaxWorldSizeHost || worldSize.y > PlatformOptimizations.MaxWorldSizeHost)
				{
					return false;
				}
			}
			switch (WorldType)
			{
			case EWorldType.Handmade:
				if (_worldInfo.IsRandomWorld)
				{
					return false;
				}
				break;
			case EWorldType.ExistingRandom:
				if (!_worldInfo.IsRandomWorld)
				{
					return false;
				}
				break;
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectWorld(string _worldName)
	{
		int num = cbxWorldName.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (GameWorldInfo _info) => _info.RealName.EqualsCaseInsensitive(_worldName));
		if (num < 0)
		{
			EnumGameMode iD = (EnumGameMode)SelectedGameMode.GetID();
			if (worldsPerMode.TryGetValue(iD, out var value))
			{
				num = value.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (GameWorldInfo _info) => _info.RealName.EqualsCaseInsensitive(_worldName));
				if (num >= 0)
				{
					GameWorldInfo gameWorldInfo = value[num];
					cbxWorldType.Value = (gameWorldInfo.IsRandomWorld ? EWorldType.ExistingRandom : EWorldType.Handmade);
					CbxWorldType_OnValueChanged(cbxWorldType, EWorldType.Handmade, cbxWorldType.Value);
				}
			}
			num = cbxWorldName.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (GameWorldInfo _info) => _info.RealName.EqualsCaseInsensitive(_worldName));
		}
		cbxWorldName.SelectedIndex = num;
		CbxWorldName_OnValueChanged(cbxWorldName, default(GameWorldInfo), SelectedWorld.Value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnOpenSandboxSettingsRequested()
	{
		SandboxOptionManager.Current.SetWorldAndGame("", "");
		base.OnOpenSandboxSettingsRequested();
	}

	[XuiBindEvent("OnPress", "btnDefaults")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnPressed(XUiController _sender, int _mouseButton)
	{
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, (bool)GamePrefs.GetDefault(EnumGamePrefs.ServerEnabled));
		SelectedGameMode.ResetGamePrefs();
		Settings.UpdateOptionValuesFromGamePrefs();
		Settings.UpdateServerEnabledState();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDataManagementWindowClosed()
	{
		updateBarUsageAndAllowanceValues();
		string worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		createWorldList();
		updateWorlds();
		selectWorld(worldName);
		if (base.DataManagementBarEnabled)
		{
			DataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
			updateBarPendingValue();
		}
		worldGenerationControls.RefreshCountyName();
		validateStartable();
		worldGenerationControls.RefreshBindings();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBarPendingValue()
	{
		if (base.DataManagementBarEnabled)
		{
			UserDataStorageType storage = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
			pendingPreviewSize = (storage.UsesDataLimit() ? CalculatePendingSaveSize() : 0);
			DataManagementBar.SetPendingBytes(pendingPreviewSize);
			validateStartable();
		}
		[PublicizedFrom(EAccessModifier.Private)]
		long CalculatePendingSaveSize()
		{
			Vector2i worldSize = default(Vector2i);
			if (IsGenerateWorld)
			{
				worldSize.y = (worldSize.x = Math.Max(1, worldGenerationControls.WorldSize));
			}
			else
			{
				worldSize = SelectedWorldInfo.WorldSize;
			}
			if (SaveDataLimitUIHelper.CurrentValue == SaveDataLimitType.Unlimited)
			{
				return Math.Max(SaveDataLimitType.Short.CalculateTotalSize(worldSize), SaveInfoProvider.Instance.TotalAvailableBytes);
			}
			return SaveDataLimitUIHelper.CurrentValue.CalculateTotalSize(worldSize);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator startGameCo()
	{
		string worldName;
		Vector2i worldSize = default(Vector2i);
		if (IsGenerateWorld)
		{
			worldName = null;
			yield return worldGenerationControls.GenerateCo([PublicizedFrom(EAccessModifier.Internal)] (string _name) =>
			{
				worldName = _name;
			});
			if (worldName == null)
			{
				xui.playerUI.windowManager.Open(windowGroup, _bModal: true);
				yield break;
			}
			GamePrefs.Set(EnumGamePrefs.GameWorld, worldName);
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, 2);
			worldSize.y = (worldSize.x = worldGenerationControls.WorldSize);
		}
		else
		{
			worldSize = SelectedWorldInfo.WorldSize;
			worldName = SelectedWorldInfo.Name;
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, (int)(SelectedWorld?.Location.Type).Value);
		}
		PathAbstractions.AbstractedLocation abstractedLocation = PathAbstractions.Contextual.FindActiveWorldLocation();
		UserDataStorageType storageType = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		long saveDataSize = SaveDataLimitUIHelper.CurrentValue.CalculateTotalSize(worldSize);
		XUiC_SaveSpaceNeeded confirmationWindow = XUiC_SaveSpaceNeeded.Open(saveDataSize, new string[2]
		{
			abstractedLocation.FullPath,
			GameIO.GetSaveGameDir(worldName, GamePrefs.GetString(EnumGamePrefs.GameName), (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType))
		}, storageType, _onlyShowOnInsufficientSpace: true, _canCancel: true, _canDiscard: false, null, "xuiDmCreateSave", null, null, "xuiCreate");
		if (confirmationWindow != null)
		{
			while (confirmationWindow.IsOpen || confirmationWindow.Result == XUiC_SaveSpaceNeeded.ConfirmationResult.Pending)
			{
				yield return null;
			}
			if (confirmationWindow.Result != XUiC_SaveSpaceNeeded.ConfirmationResult.Confirmed)
			{
				xui.playerUI.windowManager.Open(windowGroup, _bModal: true);
				yield break;
			}
		}
		SaveDataLimit.SetLimitToPref(saveDataSize);
		GamePrefs.SetPersistent(EnumGamePrefs.GameMode, _bPersistent: true);
		Settings.SaveGameOptions(_saveAsLastUsed: true);
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
		string worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		updateWorlds();
		selectWorld(worldName);
		Settings.UpdateOptionVisibilityForGameMode(_newMode);
		Settings.UpdateOptionValuesFromGamePrefs();
		Settings.ResetSandboxPresetToDefault();
		validateStartable();
	}

	[XuiBindEvent("OnCountyNameChanged", "worldGenerationControls")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void validateStartable()
	{
		bool flag = validateWorld(out var _worldName);
		flag &= validateGameName(_worldName);
		flag &= validateStorage();
		if (base.DataManagementBarEnabled)
		{
			flag &= SaveInfoProvider.Instance.TotalAvailableBytes >= pendingPreviewSize;
		}
		flag &= Settings.PortValid;
		base.ValidStartableState = flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateGameName(string _worldName)
	{
		string text = txtGameName.Text;
		UserDataStorageType storage = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		bool flag = GameUtils.ValidateGameName(text);
		bool flag2 = text.Trim().Length > 0;
		bool flag3 = !text.EqualsCaseInsensitive("Region");
		bool flag4 = !SdFile.Exists(GameIO.GetSaveGameDir(_worldName, text, storage) + "/main.ttw");
		ValidGameName = flag && flag2 && flag3 && flag4;
		if (!flag2)
		{
			GameNameTooltip = Localization.Get("mmLblNameErrorEmpty");
		}
		else if (!flag)
		{
			GameNameTooltip = Localization.Get("mmLblNameErrorInvalid");
		}
		else if (!flag3)
		{
			GameNameTooltip = Localization.Get("mmLblNameErrorDefault");
		}
		else if (!flag4)
		{
			GameNameTooltip = Localization.Get("mmLblNameErrorExists");
		}
		else
		{
			GameNameTooltip = "";
		}
		return ValidGameName;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateWorld(out string _worldName)
	{
		GameWorldInfo value = SelectedWorld.Value;
		bool flag = true;
		bool flag2;
		if (!IsGenerateWorld)
		{
			flag2 = value.Location.Type != PathAbstractions.EAbstractedLocationType.None;
			if (SelectedWorldInfo != null)
			{
				bool flag3 = SelectedWorldInfo.GameVersionCreated.IsValid && !SelectedWorldInfo.GameVersionCreated.EqualsMajor(Constants.cVersionInformation);
				flag = flag && !flag3;
			}
			_worldName = value.RealName;
		}
		else
		{
			flag2 = worldGenerationControls.ValidCountyName;
			_worldName = worldGenerationControls.CountyName;
		}
		return flag2 && flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateStorage()
	{
		UserDataStorageType userDataStorageType = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		GameWorldInfo value = SelectedWorld.Value;
		int num;
		if (!IsGenerateWorld && value.Location.Type != PathAbstractions.EAbstractedLocationType.GameData && value.Location.StorageType == UserDataStorageType.DeviceLocal)
		{
			num = ((userDataStorageType == UserDataStorageType.DeviceLocal) ? 1 : 0);
			if (num == 0)
			{
				cbxGameSaveStorage.TextColor = Color.red;
				saveStorageTooltipKey = "mmLblNameErrorSaveStorageInvalid";
				goto IL_007f;
			}
		}
		else
		{
			num = 1;
		}
		cbxGameSaveStorage.TextColor = Color.white;
		saveStorageTooltipKey = string.Empty;
		goto IL_007f;
		IL_007f:
		bool flag = !userDataStorageType.UsesDataLimit() || cbxSaveDataLimit.Value != SaveDataLimitType.Unlimited;
		if (flag)
		{
			cbxSaveDataLimit.TextColor = Color.white;
			saveDataLimitTooltipKey = string.Empty;
		}
		else
		{
			cbxSaveDataLimit.TextColor = Color.red;
			saveDataLimitTooltipKey = "mmLblNameErrorSaveDataLimitNoUnlimited";
		}
		return (byte)((uint)num & (flag ? 1u : 0u)) != 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateInput()
	{
		if (XUiUtils.HotkeysAllowedFor(viewComponent ?? children[0].ViewComponent) && xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			BtnStart_OnPressed(null, 0);
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
		case EPlatformLoadSaveGameState.NewGameSelect:
		{
			string text = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			if (!SelectedWorld.Value.RealName.EqualsCaseInsensitive(text))
			{
				selectWorld(text);
			}
			if (!SelectedWorld.Value.RealName.EqualsCaseInsensitive(text))
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
		case EPlatformLoadSaveGameState.ContinueGameOpen:
		case EPlatformLoadSaveGameState.ContinueGameSelect:
			break;
		}
	}
}

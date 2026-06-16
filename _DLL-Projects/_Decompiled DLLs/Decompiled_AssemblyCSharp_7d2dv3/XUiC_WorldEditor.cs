using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldEditor : XUiC_EditingToolsDialogBase
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public const UserDataStorageType AllowedUserDataStorageType = UserDataStorageType.DeviceLocal;

	[XuiBindComponent("btnDataManagement", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDataManagement;

	[XuiBindComponent("btnStart", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnStart;

	[XuiBindComponent("btnCreate", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCreate;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldList worldList;

	[XuiXmlBinding("startable")]
	public bool Startable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_WorldList xUiC_WorldList = worldList;
			if (xUiC_WorldList == null)
			{
				return false;
			}
			return xUiC_WorldList.SelectedEntryIndex >= 0;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		worldList.SetUserDataStorageTypeFilter(default(UserDataStorageType));
		worldList.RebuildList();
		if (string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.GameWorld)) || !worldList.SelectByName(GamePrefs.GetString(EnumGamePrefs.GameWorld)))
		{
			worldList.SelectedEntryIndex = 0;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(viewComponent ?? children[0].ViewComponent) && xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
		{
			BtnStart_OnPressed(this, 0);
		}
	}

	[XuiBindEvent("SelectionChanged", "worldList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_SelectionChanged(XUiC_List<XUiC_WorldList.WorldListEntry> _list, XUiC_WorldList.WorldListEntry _previousEntry, XUiC_WorldList.WorldListEntry _newEntry)
	{
		IsDirty = true;
	}

	[XuiBindEvent("ListEntryDoubleClicked", "worldList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_OnEntryDoubleClicked(XUiC_List<XUiC_WorldList.WorldListEntry> _list, XUiC_WorldList.WorldListEntry _entry)
	{
		if (worldList.SelectedEntryIndex >= 0)
		{
			BtnStart_OnPressed(this, -1);
		}
	}

	[XuiBindEvent("OnPress", "btnDataManagement")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDataManagement_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DataManagement.OpenDataManagementWindow(this, OnDataManagementWindowClosed);
	}

	[XuiBindEvent("OnPress", "btnCreate")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCreateManagement_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_WorldEditorCreateWorld.Open(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDataManagementWindowClosed()
	{
		worldList.RebuildList();
	}

	[XuiBindEvent("OnPress", "btnStart")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStart_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!btnStart.ViewComponent.Enabled)
		{
			return;
		}
		new GameModeEditWorld().ResetGamePrefs();
		PathAbstractions.AbstractedLocation selectedWorld = worldList.SelectedEntryData.Location;
		GamePrefs.Set(EnumGamePrefs.GameWorld, selectedWorld.Name);
		if (canSaveWorldIn(selectedWorld))
		{
			StartEditor();
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
		XUiC_MessageBoxWindowGroup.ShowOkCancel(xui, Localization.Get("xuiCreateWorldCanNotEditWorldInGameFolderTitle"), string.Format(Localization.Get("xuiCreateWorldCanNotEditWorldInGameFolderText"), GameIO.GetOsStylePath(newLocation.FullPath)), [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			Log.Out("Will copy the world from '" + selectedWorld.FullPath + "' to '" + newLocation.FullPath + "' for editing.");
			GameIO.CopyDirectory(selectedWorld.FullPath, newLocation.FullPath);
			StartEditor();
		}, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			xui.playerUI.windowManager.Open(windowGroup, _bModal: true);
		}, _openMainMenuOnClose: false);
	}

	public void StartEditor()
	{
		GamePrefs.Set(EnumGamePrefs.GameMode, GameModeEditWorld.TypeName);
		GamePrefs.Set(EnumGamePrefs.GameName, "WorldEditor");
		GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, 0);
		xui.playerUI.windowManager.Close(windowGroup);
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), _offline: false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			XUiC_MessageBoxWindowGroup.ShowNetworkError(xui, networkConnectionError);
		}
	}

	public static PathAbstractions.AbstractedLocation LocationForNewWorld(string _name)
	{
		PathAbstractions.EAbstractedLocationType locationType = PathAbstractions.EAbstractedLocationType.UserDataPath;
		return PathAbstractions.WorldsSearchPaths.BuildLocation(locationType, (string)null, _name, (Mod)null, (UserDataStorageType?)UserDataStorageType.DeviceLocal).Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool canSaveWorldIn(PathAbstractions.AbstractedLocation _location)
	{
		return _location.Type != PathAbstractions.EAbstractedLocationType.GameData;
	}
}

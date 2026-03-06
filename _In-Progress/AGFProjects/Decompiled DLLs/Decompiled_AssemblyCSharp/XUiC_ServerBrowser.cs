using System.Collections;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowser : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnConnect;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingEacNeeded;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServersList serversList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerFilters serverFilters;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerInfo serverInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MultiplayerPrivilegeNotification wdwMultiplayerPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserDirectConnect directConnect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel searchErrorPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label searchErrorLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool maxSearchResultsReached;

	[PublicizedFrom(EAccessModifier.Private)]
	public int searchResultLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingFilters;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool filtersHiddenRefreshRequired;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector browserTabSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector filtersTabSelector;

	public bool ShowingFilters
	{
		get
		{
			return showingFilters;
		}
		set
		{
			filtersTabSelector.isActiveTabSelector = value;
			browserTabSelector.isActiveTabSelector = !value;
			if (value != showingFilters)
			{
				showingFilters = value;
				if (!showingFilters)
				{
					filtersHiddenRefreshRequired = true;
				}
				IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		((XUiC_SimpleButton)GetChildById("btnBack")).OnPressed += BtnBack_OnPressed;
		foreach (XUiC_ServersList.EnumServerLists list in EnumUtils.Values<XUiC_ServersList.EnumServerLists>())
		{
			string id = "type" + list.ToStringCached();
			XUiController xUiController = GetChildById(id)?.GetChildById("button");
			if (xUiController != null)
			{
				xUiController.OnPress += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _sender, int _args) =>
				{
					serversList.SetServerTypeFilter(list);
					IsDirty = true;
				};
			}
		}
		btnConnect = (XUiC_SimpleButton)GetChildById("btnConnect");
		btnConnect.OnPressed += BtnConnect_OnPressed;
		RefreshConnectLabel();
		XUiController[] childrenById = GetChildrenById("btnDirectConnect");
		for (int num = 0; num < childrenById.Length; num++)
		{
			((XUiC_SimpleButton)childrenById[num]).OnPressed += BtnDirectConnect_OnPressed;
		}
		GetChildById("btnShowFilters").OnPress += ShowFiltersButton_OnPress;
		((XUiC_SimpleButton)GetChildById("pnlEacNeeded").GetChildById("btnOk")).OnPressed += BtnEacNeededOk_OnPressed;
		XUiController childById = GetChildById("pnlEacNeeded").GetChildById("outclick");
		childById.ViewComponent.IsNavigatable = false;
		childById.OnPress += BtnEacNeededOk_OnPressed;
		browserTabSelector = GetChildByType<XUiC_TabSelector>();
		serversList = (XUiC_ServersList)GetChildById("servers");
		serversList.OnEntryDoubleClicked += ServersList_OnEntryDoubleClicked;
		serversList.SelectionChanged += ServersList_OnSelectionChanged;
		serversList.CountsChanged += ServersList_OnCountsChanged;
		serverInfo = (XUiC_ServerInfo)GetChildById("serverinfo");
		serverFilters = (XUiC_ServerFilters)GetChildById("serverfilters");
		serverFilters.OnFilterChanged += OnFilterValueChanged;
		serverFilters.SetServersList(serversList);
		filtersTabSelector = serverFilters.GetChildByType<XUiC_TabSelector>();
		directConnect = (XUiC_ServerBrowserDirectConnect)GetChildById("pnlDirectConnect");
		searchErrorPanel = (XUiV_Panel)GetChildById("searchErrorPanel").ViewComponent;
		searchErrorLabel = (XUiV_Label)GetChildById("searchErrorPanel").GetChildById("lblErrorMessage").ViewComponent;
		((XUiC_SimpleButton)GetChildById("searchErrorPanel").GetChildById("btnOk")).OnPressed += BtnSearchErrorPanel_OnPressed;
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			SetServerListType(XUiC_ServersList.EnumServerLists.Peer);
		}
		else
		{
			SetServerListType(XUiC_ServersList.EnumServerLists.Dedicated);
		}
		RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshConnectLabel()
	{
		InControlExtensions.SetApplyButtonString(btnConnect, "xuiServerBrowserConnect");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		RefreshConnectLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetServerListType(XUiC_ServersList.EnumServerLists _type)
	{
		serversList.SetServerTypeFilter(_type);
		serverInfo.InitializeForListFilter(_type);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDirectConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		directConnect.Show(_sender);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!btnConnect.Enabled)
		{
			return;
		}
		if (wdwMultiplayerPrivileges == null)
		{
			wdwMultiplayerPrivileges = XUiC_MultiplayerPrivilegeNotification.GetWindow();
		}
		GameServerInfo gsi = serversList?.SelectedEntry?.GetEntry()?.gameServerInfo;
		if (gsi == null)
		{
			return;
		}
		EUserPerms permissions = ((!gsi.AllowsCrossplay) ? EUserPerms.Multiplayer : (EUserPerms.Multiplayer | EUserPerms.Crossplay));
		wdwMultiplayerPrivileges?.ResolvePrivilegesWithDialog(permissions, [PublicizedFrom(EAccessModifier.Internal)] (bool result) =>
		{
			if (result)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo = gsi;
				IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
				if ((antiCheatClient == null || !antiCheatClient.ClientAntiCheatEnabled()) && gsi.EACEnabled)
				{
					showingEacNeeded = true;
					IsDirty = true;
				}
				else if (gsi.GetValue(GameInfoBool.IsPasswordProtected))
				{
					XUiC_ServerPasswordWindow.OpenPasswordWindow(base.xui, _badPassword: false, ServerInfoCache.Instance.GetPassword(gsi), _modal: false, connectToServer, [PublicizedFrom(EAccessModifier.Internal)] () =>
					{
					});
				}
				else
				{
					connectToServer("");
				}
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void connectToServer(string _password)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo != null)
		{
			ServerInfoCache.Instance.SavePassword(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo, _password);
		}
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEacNeededOk_OnPressed(XUiController _xUiController, int _mouseButton)
	{
		showingEacNeeded = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		btnConnect.SelectCursorElement(_withDelay: true);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSearchErrorPanel_OnPressed(XUiController _xUiController, int _mouseButton)
	{
		searchErrorPanel.IsVisible = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFilterValueChanged(IServerBrowserFilterControl _sender)
	{
		serversList.SetFilter(_sender.GetFilter());
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool anyFilterSet()
	{
		if (serversList != null)
		{
			return serversList.GetActiveFilterCount() > 0;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		showFilters();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showFilters()
	{
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			ServerListManager.Instance.StopSearch();
		}
		ShowingFilters = true;
		serverFilters.GetChildByType<XUiC_TabSelector>().SelectedTabIndex = 0;
		base.xui.playerUI.CursorController.SetNavigationLockView(serverFilters.ViewComponent, serverFilters.GetInitialSelectedElement());
	}

	public override void OnOpen()
	{
		IsDirty = true;
		base.OnOpen();
		windowGroup.isEscClosable = false;
		ServerListManager.Instance.RegisterGameServerFoundCallback(onGameServerFoundCallback, maxResultsCallback, serverSearchErrorCallback);
		btnConnect.Enabled = false;
		if (directConnect != null)
		{
			directConnect.ViewComponent.IsVisible = false;
		}
		searchErrorPanel.IsVisible = false;
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			showFilters();
		}
		else
		{
			ServerListManager.Instance.StartSearch(null);
			ShowingFilters = false;
			GetChildById("btnBack").SelectCursorElement();
		}
		showingEacNeeded = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_MultiplayerPrivilegeNotification.Close();
		ServerListManager.Instance.Disconnect();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!PermissionsManager.IsMultiplayerAllowed() && base.xui.playerUI.windowManager.IsWindowOpen(windowGroup.ID))
		{
			closeBrowser();
		}
		if (base.xui.playerUI.playerInput != null)
		{
			if (base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed && !base.xui.playerUI.windowManager.IsInputActive())
			{
				handleBackOrEscape();
			}
			if (base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
			{
				if (ShowingFilters)
				{
					serverFilters.StartShortcutPressed();
				}
				else if (btnConnect.Enabled)
				{
					BtnConnect_OnPressed(null, 0);
				}
			}
		}
		if (!IsDirty)
		{
			return;
		}
		IsDirty = false;
		RefreshBindings();
		if (showingEacNeeded)
		{
			base.xui.playerUI.CursorController.SetNavigationLockView(GetChildById("pnlEacNeeded").ViewComponent, GetChildById("pnlEacNeeded").GetChildById("btnOk").ViewComponent);
		}
		if (filtersHiddenRefreshRequired)
		{
			filtersHiddenRefreshRequired = false;
			if (ServerListManager.Instance.IsPrefilteredSearch)
			{
				GetChildById("btnBack").SelectCursorElement(_withDelay: true);
			}
			else
			{
				GetChildById("btnShowFilters").SelectCursorElement(_withDelay: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "filtersbuttonselected":
			_value = anyFilterSet().ToString();
			return true;
		case "eacneededvisible":
			_value = showingEacNeeded.ToString();
			return true;
		case "typededicatedselected":
			_value = (serversList != null && serversList.CurrentServerTypeList == XUiC_ServersList.EnumServerLists.Dedicated).ToString();
			return true;
		case "typededicatedcount":
			_value = ((serversList == null) ? "" : serversList.GetServerCount(XUiC_ServersList.EnumServerLists.Dedicated).ToString());
			return true;
		case "typepeerselected":
			_value = (serversList != null && serversList.CurrentServerTypeList == XUiC_ServersList.EnumServerLists.Peer).ToString();
			return true;
		case "typepeercount":
			_value = ((serversList == null) ? "" : serversList.GetServerCount(XUiC_ServersList.EnumServerLists.Peer).ToString());
			return true;
		case "typefriendsselected":
			_value = (serversList != null && serversList.CurrentServerTypeList == XUiC_ServersList.EnumServerLists.Friends).ToString();
			return true;
		case "typefriendscount":
			_value = ((serversList == null) ? "" : serversList.GetServerCount(XUiC_ServersList.EnumServerLists.Friends).ToString());
			return true;
		case "typehistoryselected":
			_value = (serversList != null && serversList.CurrentServerTypeList == XUiC_ServersList.EnumServerLists.History).ToString();
			return true;
		case "typehistorycount":
			_value = ((serversList == null) ? "" : serversList.GetServerCount(XUiC_ServersList.EnumServerLists.History).ToString());
			return true;
		case "typelanselected":
			_value = (serversList != null && serversList.CurrentServerTypeList == XUiC_ServersList.EnumServerLists.LAN).ToString();
			return true;
		case "typelancount":
			_value = ((serversList == null) ? "" : serversList.GetServerCount(XUiC_ServersList.EnumServerLists.LAN).ToString());
			return true;
		case "filtersvisible":
			_value = ShowingFilters.ToString();
			return true;
		case "isprefilteredsearch":
			_value = ServerListManager.Instance.IsPrefilteredSearch.ToString();
			return true;
		case "limitwarningvisible":
			_value = maxSearchResultsReached.ToString();
			return true;
		case "resultlimit":
			_value = searchResultLimit.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onGameServerDetailsCallback(bool _success, string _message, GameServerInfo _info)
	{
		if (_success && serversList.SelectedEntryIndex >= 0 && _info.Equals(serversList.SelectedEntry.GetEntry().gameServerInfo))
		{
			serverInfo.SetServerInfo(_info);
			serversList.SelectedEntry.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onGameServerFoundCallback(IPlatform _owner, GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		if (!(_gameServerInfo.AllowsCrossplay ? PermissionsManager.IsCrossplayAllowed() : _gameServerInfo.PlayGroup.IsCurrent()) || (Submission.Enabled && !_gameServerInfo.IsCompatibleVersion))
		{
			return;
		}
		if (_gameServerInfo.IsDedicated)
		{
			serversList.AddGameServer(_gameServerInfo, _source);
			IsDirty = true;
			return;
		}
		string value = _gameServerInfo.GetValue(GameInfoString.CombinedPrimaryId);
		string value2 = _gameServerInfo.GetValue(GameInfoString.CombinedNativeId);
		if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value2))
		{
			serversList.AddGameServer(_gameServerInfo, _source);
			IsDirty = true;
		}
		else
		{
			PlatformUserIdentifierAbs hostPrimaryId = PlatformUserIdentifierAbs.FromCombinedString(value);
			PlatformUserIdentifierAbs hostNativeId = PlatformUserIdentifierAbs.FromCombinedString(value2);
			ThreadManager.StartCoroutine(ResolveBlocksAndAddServerCoroutine(hostPrimaryId, hostNativeId, _gameServerInfo, _source));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ResolveBlocksAndAddServerCoroutine(PlatformUserIdentifierAbs _hostPrimaryId, PlatformUserIdentifierAbs _hostNativeId, GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		IPlatformUserData userData = PlatformUserManager.GetOrCreate(_hostPrimaryId);
		userData.NativeId = _hostNativeId;
		userData.MarkBlockedStateChanged();
		yield return PlatformUserManager.ResolveUserBlockedCoroutine(userData);
		if (!userData.Blocked[EBlockType.Play].IsBlocked())
		{
			serversList.AddGameServer(_gameServerInfo, _source);
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void maxResultsCallback(IPlatform _sourcePlatform, bool _maxReached, int _limit)
	{
		maxSearchResultsReached = _maxReached;
		searchResultLimit = _limit;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serverSearchErrorCallback(string _message)
	{
		searchErrorPanel.IsVisible = true;
		searchErrorLabel.Text = _message;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnEntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && serversList.SelectedEntryIndex >= 0)
		{
			BtnConnect_OnPressed(_sender, _mouseButton);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnSelectionChanged(XUiC_ListEntry<XUiC_ServersList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_ServersList.ListEntry> _newEntry)
	{
		bool flag = _newEntry != null;
		GameServerInfo gameServerInfo = (flag ? _newEntry.GetEntry().gameServerInfo : null);
		btnConnect.Enabled = flag && gameServerInfo.IsCompatibleVersion;
		serverInfo.SetServerInfo(gameServerInfo);
		if (flag)
		{
			ServerInformationTcpClient.RequestRules(gameServerInfo, _ignoreTimeouts: false, onGameServerDetailsCallback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnCountsChanged()
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		handleBackOrEscape();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleBackOrEscape()
	{
		if (base.xui.playerUI.windowManager.IsWindowOpen(XUiC_ServerPasswordWindow.ID))
		{
			return;
		}
		if (directConnect != null && directConnect.ViewComponent.IsVisible)
		{
			directConnect.Hide();
		}
		else if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			if (ShowingFilters)
			{
				closeBrowser();
				return;
			}
			maxSearchResultsReached = false;
			serversList.RebuildList();
			showFilters();
			btnConnect.Enabled = false;
		}
		else if (ShowingFilters)
		{
			serverFilters.closeFilters();
			GetChildById("btnShowFilters").SelectCursorElement(_withDelay: true);
		}
		else
		{
			closeBrowser();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeBrowser()
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
	}
}

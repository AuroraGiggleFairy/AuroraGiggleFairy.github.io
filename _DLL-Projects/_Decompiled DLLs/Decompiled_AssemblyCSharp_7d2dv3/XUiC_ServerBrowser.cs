using System.Collections;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowser : XUiC_PlayGameDialogBase
{
	public static string ID = "";

	[XuiBindComponent("btnConnect", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConnect;

	[XuiBindComponent("btnDirectConnect", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button[] btnDirectConnect;

	[XuiBindComponent("btnShowFilters", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnShowFilters;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ServersList serversList;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ServerFilters serverFilters;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ServerInfo serverInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool maxSearchResultsReached;

	[PublicizedFrom(EAccessModifier.Private)]
	public int searchResultLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingFilters;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool filtersHiddenRefreshRequired;

	[XuiXmlBinding("filtersvisible")]
	public bool ShowingFilters
	{
		get
		{
			return showingFilters;
		}
		set
		{
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

	[XuiXmlBinding("filtersbuttonselected")]
	public bool HasActiveFilters
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return anyFilterSet();
		}
	}

	[XuiXmlBinding("selectedlisttype")]
	public XUiC_ServersList.EnumServerLists SelectedListType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return serversList?.CurrentServerTypeList ?? XUiC_ServersList.EnumServerLists.Dedicated;
		}
	}

	[XuiXmlBinding("typededicatedcount")]
	public int ServersCountDedicated
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return serversList?.GetServerCount(XUiC_ServersList.EnumServerLists.Dedicated) ?? 0;
		}
	}

	[XuiXmlBinding("typepeercount")]
	public int ServersCountPeer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return serversList?.GetServerCount(XUiC_ServersList.EnumServerLists.Peer) ?? 0;
		}
	}

	[XuiXmlBinding("typefriendscount")]
	public int ServersCountFriends
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return serversList?.GetServerCount(XUiC_ServersList.EnumServerLists.Friends) ?? 0;
		}
	}

	[XuiXmlBinding("typehistorycount")]
	public int ServersCountHistory
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return serversList?.GetServerCount(XUiC_ServersList.EnumServerLists.History) ?? 0;
		}
	}

	[XuiXmlBinding("typelancount")]
	public int ServersCountLan
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return serversList?.GetServerCount(XUiC_ServersList.EnumServerLists.LAN) ?? 0;
		}
	}

	[XuiXmlBinding("isprefilteredsearch")]
	public bool IsPrefilteredSearch
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return ServerListManager.Instance.IsPrefilteredSearch;
		}
	}

	[XuiXmlBinding("limitwarningvisible")]
	public bool SearchLimitReached
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return maxSearchResultsReached;
		}
	}

	[XuiXmlBinding("resultlimit")]
	public int SearchLimit
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return searchResultLimit;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		foreach (XUiC_ServersList.EnumServerLists list in EnumUtils.Values<XUiC_ServersList.EnumServerLists>())
		{
			string id = "type" + list.ToStringCached();
			if (TryGetChildController<XUiController>(id, out var _child) && _child.TryGetChildController<XUiC_Button>("", out var _child2))
			{
				_child2.OnPress += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
				{
					serversList.SetServerTypeFilter(list);
					IsDirty = true;
				};
			}
		}
		serverFilters.SetServersList(serversList);
		setServerListType((!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent()) ? XUiC_ServersList.EnumServerLists.Dedicated : XUiC_ServersList.EnumServerLists.Peer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setServerListType(XUiC_ServersList.EnumServerLists _type)
	{
		serversList.SetServerTypeFilter(_type);
		serverInfo.InitializeForListFilter(_type);
		IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnDirectConnect")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDirectConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_ServerBrowserDirectConnect.Open(xui, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			xui.playerUI.windowManager.Close(windowGroup);
		}, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			_sender.SelectCursorElement(_withDelay: true);
		});
	}

	[XuiBindEvent("OnPress", "btnConnect")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!btnConnect.ViewComponent.Enabled)
		{
			return;
		}
		GameServerInfo gsi = serversList?.SelectedEntryData?.GameServerInfo;
		if (gsi == null)
		{
			return;
		}
		XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog((!gsi.AllowsCrossplay) ? EUserPerms.Multiplayer : (EUserPerms.Multiplayer | EUserPerms.Crossplay), [PublicizedFrom(EAccessModifier.Internal)] (bool _result) =>
		{
			if (_result)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo = gsi;
				IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
				if ((antiCheatClient == null || !antiCheatClient.ClientAntiCheatEnabled()) && gsi.EACEnabled)
				{
					XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiServerEacNeeded"), Localization.Get("xuiServerEacNeededText"), [PublicizedFrom(EAccessModifier.Internal)] () =>
					{
						btnConnect.SelectCursorElement(_withDelay: true);
					}, _openMainMenuOnClose: false, _modal: false, _confirmOnOutsideClick: true);
				}
				else if (gsi.GetValue(GameInfoBool.IsPasswordProtected))
				{
					XUiC_ServerPasswordWindow.OpenPasswordWindow(xui, _badPassword: false, ServerInfoCache.Instance.GetPassword(gsi), _modal: false, connectToServer, [PublicizedFrom(EAccessModifier.Internal)] () =>
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
		xui.playerUI.windowManager.Close(windowGroup);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
	}

	[XuiBindEvent("OnFilterChanged", "serverFilters")]
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

	[XuiBindEvent("OnPress", "btnShowFilters")]
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
		if (!ServerListManager.Instance.IsPrefilteredSearch)
		{
			xui.playerUI.CursorController.SetNavigationLockView(serverFilters.ViewComponent, serverFilters.GetInitialSelectedElement());
		}
	}

	public override void OnOpen()
	{
		IsDirty = true;
		base.OnOpen();
		windowGroup.isEscClosable = false;
		ServerListManager.Instance.RegisterGameServerFoundCallback(onGameServerFoundCallback, maxResultsCallback, serverSearchErrorCallback);
		btnConnect.ViewComponent.Enabled = false;
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			showFilters();
			return;
		}
		ServerListManager.Instance.StartSearch(null);
		ShowingFilters = false;
		GetChildById("btnBack").SelectCursorElement();
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
		if (!PermissionsManager.IsMultiplayerAllowed() && xui.playerUI.windowManager.IsWindowOpen(windowGroup.Id))
		{
			CloseBrowser();
		}
		if (xui.playerUI.playerInput != null && XUiUtils.HotkeysAllowedFor(children[0].ViewComponent) && xui.playerUI.playerInput.GUIActions.Apply.WasPressed && btnConnect.ViewComponent.Enabled)
		{
			BtnConnect_OnPressed(null, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void close()
	{
		handleBackOrEscape();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onDirtyUpdate()
	{
		base.onDirtyUpdate();
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void onGameServerDetailsCallback(bool _success, string _message, GameServerInfo _info)
	{
		if (_success)
		{
			serversList.SetAllChildrenDirty();
			if (_info.Equals(serversList.SelectedEntryData?.GameServerInfo))
			{
				serverInfo.SetServerInfo(_info);
			}
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
			ThreadManager.StartCoroutine(resolveBlocksAndAddServerCoroutine(hostPrimaryId, hostNativeId, _gameServerInfo, _source));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator resolveBlocksAndAddServerCoroutine(PlatformUserIdentifierAbs _hostPrimaryId, PlatformUserIdentifierAbs _hostNativeId, GameServerInfo _gameServerInfo, EServerRelationType _source)
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
		XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiServerBrowserSearchErrorTitle"), _message, null, _openMainMenuOnClose: false, _modal: false, _confirmOnOutsideClick: true);
	}

	[XuiBindEvent("ListEntryDoubleClicked", "serversList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnEntryDoubleClicked(XUiC_List<XUiC_ServersList.ListEntry> _list, XUiC_ServersList.ListEntry _entry)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && serversList.SelectedEntryIndex >= 0)
		{
			BtnConnect_OnPressed(this, -1);
		}
	}

	[XuiBindEvent("SelectionChanged", "serversList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnSelectionChanged(XUiC_List<XUiC_ServersList.ListEntry> _list, XUiC_ServersList.ListEntry _previousEntry, XUiC_ServersList.ListEntry _newEntry)
	{
		bool flag = _newEntry != null;
		GameServerInfo gameServerInfo = (flag ? _newEntry.GameServerInfo : null);
		btnConnect.ViewComponent.Enabled = flag && gameServerInfo.IsCompatibleVersion;
		serverInfo.SetServerInfo(gameServerInfo);
		if (flag)
		{
			ServerInformationTcpClient.RequestRules(gameServerInfo, _ignoreTimeouts: false, onGameServerDetailsCallback);
		}
	}

	[XuiBindEvent("CountsChanged", "serversList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnCountsChanged()
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleBackOrEscape()
	{
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			if (ShowingFilters)
			{
				CloseBrowser();
				return;
			}
			maxSearchResultsReached = false;
			serversList.RebuildList();
			showFilters();
			btnConnect.ViewComponent.Enabled = false;
		}
		else if (ShowingFilters)
		{
			serverFilters.CloseFilters();
			GetChildById("btnShowFilters").SelectCursorElement(_withDelay: true);
		}
		else
		{
			CloseBrowser();
		}
	}

	public void CloseBrowser()
	{
		xui.playerUI.windowManager.Close(windowGroup);
		xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
	}
}

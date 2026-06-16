using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL;

public class User : IUserClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly EnumDictionary<XblPermission, EBlockType> xblPermissionToBlockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XblPermission[] userBlockedPermissions;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XblAnonymousUserType[] userBlockedAnonymousTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform m_owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatformApi m_api;

	[PublicizedFrom(EAccessModifier.Private)]
	public IApplicationStateController m_appState;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUserHandle m_userHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_userHandleLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<XUserHandle> m_userHandleReady;

	[PublicizedFrom(EAccessModifier.Private)]
	public Unity.XGamingRuntime.XblContextHandle m_contextHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUserLocalId m_userLocalId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong m_userXuid;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserIdentifierXbl m_platformUserId;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_loginLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_loginDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public EUserStatus m_loginUserStatus;

	[PublicizedFrom(EAccessModifier.Private)]
	public LoginUserCallback m_loginUserCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public EApiStatusReason m_loginUserCallbackReason;

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_loginUserCallbackReasonAdditional;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserPrivilegeHelper m_privilegeHelper;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_activityLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerInfo m_activityLastServerInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isActivityActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_shouldActivityBeActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_shouldRetryDeleteActivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_currentlyDeletingActivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<ulong, IPlatformUserBlockedResults> userBlockedXuidToResultsTemp = new Dictionary<ulong, IPlatformUserBlockedResults>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IPlatformUserBlockedResults> userBlockedAnonymousResultsTemp = new List<IPlatformUserBlockedResults>();

	public string SandboxName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<IPlatform> m_userLoggedIn;

	[PublicizedFrom(EAccessModifier.Private)]
	public IdProviderGameCore m_idProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public FriendsListXbl friendsList;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string eosRelyingPartyUrl = "https://eos.epicgames.com";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string eosRelyingPartyHttpMethod = "GET";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XUserGetTokenAndSignatureUtf16HttpHeader[] eosRelyingPartyRequestHeaders;

	public bool IsMultiplayerActivityActive
	{
		get
		{
			lock (m_activityLock)
			{
				return m_isActivityActive;
			}
		}
	}

	public XUserLocalId LocalID => m_userLocalId;

	public ulong Xuid => m_userXuid;

	public Unity.XGamingRuntime.XblContextHandle XblContextHandle => m_contextHandle;

	public XUserHandle UserHandle => m_userHandle;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EUserStatus UserStatus
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = EUserStatus.NotAttempted;

	public PlatformUserIdentifierAbs PlatformUserId => m_idProvider.Id;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string GamerTag
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SocialManagerXbl SocialManager
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public MultiplayerActivityQueryManager MultiplayerActivityQueryManager
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XblSandboxHelper SandboxHelper
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = new XblSandboxHelper();

	public EUserPerms Permissions
	{
		get
		{
			if (m_privilegeHelper == null)
			{
				return (EUserPerms)0;
			}
			EUserPerms eUserPerms = (EUserPerms)0;
			if (m_privilegeHelper.MultiplayerAllowed.Has())
			{
				eUserPerms |= EUserPerms.Multiplayer | EUserPerms.HostMultiplayer;
			}
			if (m_privilegeHelper.CommunicationAllowed.Has())
			{
				eUserPerms |= EUserPerms.Communication;
			}
			if (m_privilegeHelper.CrossPlayAllowed.Has())
			{
				eUserPerms |= EUserPerms.Crossplay;
			}
			return eUserPerms;
		}
	}

	public event Action<XUserHandle> UserHandleReady
	{
		add
		{
			lock (m_userHandleLock)
			{
				m_userHandleReady = (Action<XUserHandle>)Delegate.Combine(m_userHandleReady, value);
				if (m_userHandle != null)
				{
					value(m_userHandle);
				}
			}
		}
		remove
		{
			lock (m_userHandleLock)
			{
				m_userHandleReady = (Action<XUserHandle>)Delegate.Remove(m_userHandleReady, value);
			}
		}
	}

	public event Action<IPlatform> UserLoggedIn
	{
		add
		{
			lock (m_loginLock)
			{
				m_userLoggedIn = (Action<IPlatform>)Delegate.Combine(m_userLoggedIn, value);
				if (UserStatus == EUserStatus.LoggedIn)
				{
					value(m_owner);
				}
			}
		}
		remove
		{
			lock (m_loginLock)
			{
				m_userLoggedIn = (Action<IPlatform>)Delegate.Remove(m_userLoggedIn, value);
			}
		}
	}

	public event UserBlocksChangedCallback UserBlocksChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	static User()
	{
		eosRelyingPartyRequestHeaders = new XUserGetTokenAndSignatureUtf16HttpHeader[1]
		{
			new XUserGetTokenAndSignatureUtf16HttpHeader
			{
				Name = "X-XBL-Contract-Version",
				Value = "2"
			}
		};
		Dictionary<EBlockType, XblPermission> dictionary = EnumUtils.Values<EBlockType>().ToDictionary([PublicizedFrom(EAccessModifier.Internal)] (EBlockType blockType) => blockType, [PublicizedFrom(EAccessModifier.Internal)] (EBlockType blockType) => blockType switch
		{
			EBlockType.TextChat => XblPermission.CommunicateUsingText, 
			EBlockType.VoiceChat => XblPermission.CommunicateUsingVoice, 
			EBlockType.Play => XblPermission.PlayMultiplayer, 
			_ => throw new NotImplementedException(string.Format("Mapping from {0}.{1} to {2} not implemented!", "EBlockType", blockType, "XblPermission")), 
		});
		userBlockedPermissions = dictionary.Values.ToArray();
		xblPermissionToBlockType = new EnumDictionary<XblPermission, EBlockType>();
		foreach (var (value, key) in dictionary)
		{
			xblPermissionToBlockType.Add(key, value);
		}
		userBlockedAnonymousTypes = new XblAnonymousUserType[2]
		{
			XblAnonymousUserType.CrossNetworkFriend,
			XblAnonymousUserType.CrossNetworkUser
		};
	}

	public void Init(IPlatform _owner)
	{
		m_owner = _owner;
		m_api = _owner.Api;
		m_appState = _owner.ApplicationState;
		m_api.ClientApiInitialized += OnClientApiInitialized;
		m_idProvider = new IdProviderGameCore(this, PlatformManager.CrossplatformPlatform?.User);
		XblXuidMapper.XuidMapped += OnXuidMapped;
		UserLoggedIn += SetNameOnPlayerLogin;
		m_appState.OnNetworkStateChanged += OnNetworkStateChanged;
	}

	public void Destroy()
	{
		m_appState.OnNetworkStateChanged -= OnNetworkStateChanged;
		if (m_userHandle != null)
		{
			SDK.XUserCloseHandle(m_userHandle);
			XblHelpers.LogHR(0, "Close User Handle.");
			m_userHandle = null;
			m_userLocalId = null;
		}
		m_loginUserCallback = null;
		m_api.ClientApiInitialized -= OnClientApiInitialized;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetNameOnPlayerLogin(IPlatform platform)
	{
		if (Unity.XGamingRuntime.Interop.HR.FAILED(SDK.XUserGetGamertag(UserHandle, XUserGamertagComponent.Classic, out var gamertag)))
		{
			Log.Error("[XBL] Failed to get player's gamertag");
			return;
		}
		GamePrefs.Set(EnumGamePrefs.PlayerName, gamertag);
		GamerTag = gamertag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClientApiInitialized()
	{
		SandboxHelper.RefreshSandboxId();
		SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserSilently, XUserAddAsyncSilentCompletionRoutine);
		[PublicizedFrom(EAccessModifier.Private)]
		void DoLoginUserCallback(EUserStatus userStatus, EApiStatusReason reason, string reasonAdditional)
		{
			lock (m_loginLock)
			{
				UserStatus = userStatus;
				m_loginUserStatus = userStatus;
				m_loginUserCallbackReason = reason;
				m_loginUserCallbackReasonAdditional = reasonAdditional;
				m_loginDone = true;
				XblHelpers.LogHR((userStatus != EUserStatus.LoggedIn) ? (-2147467259) : 0, $"Initial Login Callback Done. User Status: {userStatus}, Reason: {reason}, Additional: '{reasonAdditional}'.");
				m_loginUserCallback?.Invoke(m_owner, reason, reasonAdditional);
				m_loginUserCallback = null;
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void GetXuid()
		{
			int num = SDK.XUserGetId(m_userHandle, out m_userXuid);
			XblHelpers.LogHR(num, "XUserGetId");
			if (Unity.XGamingRuntime.Interop.HR.FAILED(num))
			{
				if (num == -1994108670)
				{
					ResolveXuidIssue();
				}
				else
				{
					DoLoginUserCallback(EUserStatus.PermanentError, EApiStatusReason.Other, "Could not obtain User's Xuid.");
				}
			}
			else
			{
				PostLogin();
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void PostLogin()
		{
			int hr = SDK.XUserGetLocalId(m_userHandle, out m_userLocalId);
			XblHelpers.LogHR(hr, "XUserGetLocalId");
			if (Unity.XGamingRuntime.Interop.HR.FAILED(hr))
			{
				DoLoginUserCallback(EUserStatus.PermanentError, EApiStatusReason.Other, "Could not obtain User's Local ID.");
			}
			else
			{
				int hr2 = SDK.XBL.XblContextCreateHandle(m_userHandle, out m_contextHandle);
				XblHelpers.LogHR(hr2, "Create Xbox Live Context Handle");
				if (Unity.XGamingRuntime.Interop.HR.FAILED(hr2))
				{
					DoLoginUserCallback(EUserStatus.PermanentError, EApiStatusReason.Other, "Could not obtain Xbox Live Context Handle.");
				}
				else
				{
					MultiplayerActivityQueryManager = new MultiplayerActivityQueryManager(m_contextHandle);
					DoLoginUserCallback(EUserStatus.LoggedIn, EApiStatusReason.Ok, null);
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void ResolveXuidIssue()
		{
			SDK.XUserResolveIssueWithUiUtf16Async(m_userHandle, null, ResolveXuidIssueCompletionRoutine);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void ResolveXuidIssueCompletionRoutine(int hrResolve)
		{
			XblHelpers.LogHR(hrResolve, "XUserResolveIssueWithUiUtf16Async");
			GetXuid();
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void XUserAddAsyncAllowingUICompletionRoutine(int hr, XUserHandle userHandle)
		{
			XblHelpers.LogHR(hr, "XUserAddAsync: AddDefaultUserAllowingUI");
			if (Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
			{
				XUserAddSucceeded(userHandle);
			}
			else
			{
				UserStatus = EUserStatus.TemporaryError;
				SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserSilently, XUserAddAsyncSilentCompletionRoutine);
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void XUserAddAsyncSilentCompletionRoutine(int hr, XUserHandle userHandle)
		{
			XblHelpers.LogHR(hr, "XUserAddAsync: AddDefaultUserSilently");
			if (Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
			{
				XUserAddSucceeded(userHandle);
			}
			else if (hr == -1994108666)
			{
				UserStatus = EUserStatus.TemporaryError;
				SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, XUserAddAsyncAllowingUICompletionRoutine);
			}
			else
			{
				UserStatus = EUserStatus.TemporaryError;
				SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserSilently, XUserAddAsyncSilentCompletionRoutine);
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void XUserAddSucceeded(XUserHandle userHandle)
		{
			lock (m_userHandleLock)
			{
				m_userHandle = userHandle;
				m_userHandleReady?.Invoke(userHandle);
			}
			m_privilegeHelper = new UserPrivilegeHelper(m_userHandle);
			m_privilegeHelper.AllAllowed.ResolveSilent();
			SocialManager = new SocialManagerXbl(m_userHandle);
			friendsList = new FriendsListXbl(SocialManager);
			GetXuid();
		}
	}

	public void Login(LoginUserCallback _delegate)
	{
		lock (m_loginLock)
		{
			if (m_loginDone)
			{
				UserStatus = m_loginUserStatus;
				XblHelpers.LogHR((m_loginUserStatus != EUserStatus.LoggedIn) ? (-2147467259) : 0, $"Login with cached initial results. User Status: {m_loginUserStatus}, Reason: {m_loginUserCallbackReason}, Additional: '{m_loginUserCallbackReasonAdditional}'.");
				Callback(m_owner, m_loginUserCallbackReason, m_loginUserCallbackReasonAdditional);
			}
			else
			{
				m_loginUserCallback = Callback;
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void Callback(IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText)
		{
			_delegate(_platform, _statusReason, _statusReasonAdditionalText);
			if (UserStatus == EUserStatus.LoggedIn)
			{
				m_userLoggedIn?.Invoke(m_owner);
			}
		}
	}

	public void PlayOffline(LoginUserCallback _delegate)
	{
		if (m_idProvider.LoadOfflineId())
		{
			UserStatus = EUserStatus.OfflineMode;
			_delegate(m_owner, EApiStatusReason.Ok, null);
			m_userLoggedIn?.Invoke(m_owner);
		}
		else
		{
			UserStatus = EUserStatus.TemporaryError;
			_delegate(m_owner, EApiStatusReason.NoOnlineStart, null);
		}
	}

	public Action IsConnectToServerFromCommandline()
	{
		return null;
	}

	public void StartAdvertisePlaying(GameServerInfo _serverInfo)
	{
		if (_serverInfo == null)
		{
			return;
		}
		lock (m_activityLock)
		{
			m_shouldActivityBeActive = true;
			if (_serverInfo != m_activityLastServerInfo)
			{
				if (m_activityLastServerInfo != null)
				{
					UnregisterGameServerInfoEvents(m_activityLastServerInfo);
				}
				m_activityLastServerInfo = _serverInfo;
				RegisterGameServerInfoEvents(_serverInfo);
			}
			SetActivity(_serverInfo);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RegisterGameServerInfoEvents(GameServerInfo _serverInfo)
	{
		_serverInfo.OnChangedString += OnServerInfoChangedString;
		_serverInfo.OnChangedInt += OnServerInfoChangedInt;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnregisterGameServerInfoEvents(GameServerInfo _serverInfo)
	{
		_serverInfo.OnChangedInt -= OnServerInfoChangedInt;
		_serverInfo.OnChangedString -= OnServerInfoChangedString;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnServerInfoChangedString(GameServerInfo _serverInfo, GameInfoString _key)
	{
		if (_key == GameInfoString.UniqueId)
		{
			SetActivity(_serverInfo);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnServerInfoChangedInt(GameServerInfo _serverInfo, GameInfoInt _key)
	{
		if (_key == GameInfoInt.CurrentPlayers || _key == GameInfoInt.ServerVisibility || _key == GameInfoInt.MaxPlayers)
		{
			SetActivity(_serverInfo);
		}
	}

	public void StopAdvertisePlaying()
	{
		lock (m_activityLock)
		{
			m_shouldActivityBeActive = false;
			if (m_activityLastServerInfo != null)
			{
				UnregisterGameServerInfoEvents(m_activityLastServerInfo);
				m_activityLastServerInfo = null;
			}
			DeleteActivity();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DeleteActivity()
	{
		lock (m_activityLock)
		{
			if (!m_isActivityActive || m_shouldActivityBeActive)
			{
				m_shouldRetryDeleteActivity = false;
				return;
			}
			if (m_currentlyDeletingActivity)
			{
				m_shouldRetryDeleteActivity = true;
				return;
			}
			m_shouldRetryDeleteActivity = false;
			m_currentlyDeletingActivity = true;
			SDK.XBL.XblMultiplayerActivityDeleteActivityAsync(XblContextHandle, CompletionRoutine);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void CompletionRoutine(int _hresult)
		{
			lock (m_activityLock)
			{
				m_currentlyDeletingActivity = false;
				if (Unity.XGamingRuntime.Interop.HR.SUCCEEDED(_hresult))
				{
					if (m_isActivityActive)
					{
						Log.Out("[XBL] Activity Deleted");
						m_isActivityActive = false;
					}
					m_shouldRetryDeleteActivity = false;
					if (m_shouldActivityBeActive)
					{
						Log.Out("[XBL] Setting activity since it should be active.");
						SetActivity(m_activityLastServerInfo);
					}
				}
				else
				{
					XblHelpers.LogHR(_hresult, "Delete Activity", failWarn: true);
					if (m_isActivityActive && !m_shouldActivityBeActive)
					{
						if (m_shouldRetryDeleteActivity)
						{
							Log.Warning("[XBL] Failed to delete activity. Will retry now because a change (e.g. Network State) happened while deleting the activity.");
							m_shouldRetryDeleteActivity = false;
							SDK.XBL.XblMultiplayerActivityDeleteActivityAsync(XblContextHandle, CompletionRoutine);
						}
						else
						{
							Log.Warning("[XBL] Failed to delete activity. Will retry when a change occurs (e.g. Network State).");
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnNetworkStateChanged(bool state)
	{
		lock (m_activityLock)
		{
			if (m_isActivityActive && !m_shouldActivityBeActive)
			{
				DeleteActivity();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetActivity(GameServerInfo _serverInfo)
	{
		if (_serverInfo == null)
		{
			return;
		}
		string value = _serverInfo.GetValue(GameInfoString.UniqueId);
		int value2 = _serverInfo.GetValue(GameInfoInt.CurrentPlayers);
		int value3 = _serverInfo.GetValue(GameInfoInt.ServerVisibility);
		int value4 = _serverInfo.GetValue(GameInfoInt.MaxPlayers);
		if (value4 < 2 || string.IsNullOrEmpty(value))
		{
			return;
		}
		XblMultiplayerActivityInfo xblMultiplayerActivityInfo = new XblMultiplayerActivityInfo();
		xblMultiplayerActivityInfo.ConnectionString = value;
		xblMultiplayerActivityInfo.CurrentPlayers = (uint)value2;
		xblMultiplayerActivityInfo.GroupId = "Dummy";
		XblMultiplayerActivityInfo xblMultiplayerActivityInfo2 = xblMultiplayerActivityInfo;
		xblMultiplayerActivityInfo2.JoinRestriction = value3 switch
		{
			2 => XblMultiplayerActivityJoinRestriction.Public, 
			1 => XblMultiplayerActivityJoinRestriction.Followed, 
			_ => XblMultiplayerActivityJoinRestriction.InviteOnly, 
		};
		xblMultiplayerActivityInfo.MaxPlayers = (uint)value4;
		xblMultiplayerActivityInfo.Platform = XblMultiplayerActivityPlatform.All;
		xblMultiplayerActivityInfo.Xuid = m_idProvider.Id.Xuid;
		XblMultiplayerActivityInfo activityInfo = xblMultiplayerActivityInfo;
		lock (m_activityLock)
		{
			if (m_shouldActivityBeActive)
			{
				SDK.XBL.XblMultiplayerActivitySetActivityAsync(XblContextHandle, activityInfo, allowCrossPlatformJoin: true, CompletionRoutine);
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void CompletionRoutine(int _hresult)
		{
			lock (m_activityLock)
			{
				if (Unity.XGamingRuntime.Interop.HR.SUCCEEDED(_hresult))
				{
					if (!m_isActivityActive)
					{
						Log.Out("[XBL] Activity Created");
						m_isActivityActive = true;
					}
					if (!m_shouldActivityBeActive)
					{
						Log.Out("[XBL] Deleting activity since it should not be active.");
						DeleteActivity();
					}
				}
				else
				{
					XblHelpers.LogHR(_hresult, "Set Activity");
				}
			}
		}
	}

	public void GetLoginTicket(Action<bool, byte[], string> _callback)
	{
		if (m_userHandle == null)
		{
			Log.Error("[XBL] Attempting to retrieve XSTS token before acquiring XUserHandle");
			_callback(arg1: false, null, null);
		}
		else
		{
			SDK.XUserGetTokenAndSignatureUtf16Async(m_userHandle, XUserGetTokenAndSignatureOptions.None, "GET", "https://eos.epicgames.com", eosRelyingPartyRequestHeaders, null, CompletionRoutine);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void CompletionRoutine(int _hresult, XUserGetTokenAndSignatureUtf16Data _signature)
		{
			ThreadManager.AddSingleTaskMainThread("GameCoreLoginTicketGet", [PublicizedFrom(EAccessModifier.Internal)] (object obj) =>
			{
				XblHelpers.LogHR(_hresult, "GetToken");
				if (Unity.XGamingRuntime.Interop.HR.FAILED(_hresult))
				{
					Log.Error("[XBL] Retrieving XSTS token failed");
					_callback(arg1: false, null, null);
				}
				else
				{
					Log.Out("[XBL] XSTS token retrieved");
					_callback(arg1: true, null, _signature.Token);
				}
			});
		}
	}

	public string GetFriendName(PlatformUserIdentifierAbs _playerId)
	{
		return null;
	}

	public bool IsFriend(PlatformUserIdentifierAbs _playerId)
	{
		ulong xuid = XblXuidMapper.GetXuid(_playerId);
		if (xuid == 0L)
		{
			return false;
		}
		return friendsList?.IsFriend(xuid) ?? false;
	}

	public bool CanShowProfile(PlatformUserIdentifierAbs _playerId)
	{
		return XblXuidMapper.GetXuid(_playerId) != 0;
	}

	public void ShowProfile(PlatformUserIdentifierAbs _playerId)
	{
		ulong xuid = XblXuidMapper.GetXuid(_playerId);
		if (xuid == 0L)
		{
			return;
		}
		SDK.XGameUiShowPlayerProfileCardAsync(m_userHandle, xuid, [PublicizedFrom(EAccessModifier.Internal)] (int hr) =>
		{
			XblHelpers.LogHR(0, "XGameUiShowPlayerProfileCardAsync");
			if (Unity.XGamingRuntime.Interop.HR.FAILED(hr))
			{
				Log.Error("XBL-GXDK: Showing Player Profile Failed.");
			}
			else
			{
				Log.Out("XBL-GXDK: Showing Player Profile Succeeded.");
			}
		});
	}

	public string GetPermissionDenyReason(EUserPerms _perms)
	{
		return null;
	}

	public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
	{
		Log.Out(string.Format("[XBL] {0}({1}: [{2}], {3}: {4})", "ResolvePermissions", "_perms", _perms, "_canPrompt", _canPrompt));
		if (m_privilegeHelper != null)
		{
			yield return m_privilegeHelper.ResolvePermissions(_perms, _canPrompt, _cancellationToken);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnXuidMapped(IReadOnlyCollection<PlatformUserIdentifierAbs> userIds, ulong xuid)
	{
		this.UserBlocksChanged?.Invoke(userIds);
	}

	public IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results)
	{
		userBlockedXuidToResultsTemp.Clear();
		userBlockedAnonymousResultsTemp.Clear();
		PlatformUserIdentifierAbs platformUserId = PlatformUserId;
		foreach (IPlatformUserBlockedResults _result in _results)
		{
			PlatformUserIdentifierAbs nativeId = _result.User.NativeId;
			if (!object.Equals(platformUserId, nativeId))
			{
				ulong xuid;
				if (!(nativeId is UserIdentifierXbl userIdentifierXbl) || (xuid = userIdentifierXbl.Xuid) == 0L)
				{
					userBlockedAnonymousResultsTemp.Add(_result);
				}
				else
				{
					userBlockedXuidToResultsTemp[xuid] = _result;
				}
			}
		}
		bool running = true;
		SDK.XBL.XblPrivacyBatchCheckPermissionAsync(XblContextHandle, userBlockedPermissions, userBlockedXuidToResultsTemp.Keys.ToArray(), (userBlockedAnonymousResultsTemp.Count > 0) ? userBlockedAnonymousTypes : Array.Empty<XblAnonymousUserType>(), CompletionRoutine);
		while (running)
		{
			yield return null;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void CompletionRoutine(int hr, XblPermissionCheckResult[] _permissionResults)
		{
			try
			{
				XblHelpers.LogHR(hr, "XblPrivacyBatchCheckPermissionAsync");
				if (Unity.XGamingRuntime.Interop.HR.FAILED(hr))
				{
					HandleFailure();
				}
				else
				{
					HandleSuccess(_permissionResults);
				}
			}
			finally
			{
				running = false;
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void HandleFailure()
		{
			foreach (IPlatformUserBlockedResults _result2 in _results)
			{
				_result2.Error();
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void HandleSuccess(XblPermissionCheckResult[] _permissionResults)
		{
			foreach (XblPermissionCheckResult xblPermissionCheckResult in _permissionResults)
			{
				if (!xblPermissionCheckResult.IsAllowed && xblPermissionToBlockType.TryGetValue(xblPermissionCheckResult.PermissionRequested, out var value))
				{
					switch (xblPermissionCheckResult.TargetUserType)
					{
					case XblAnonymousUserType.Unknown:
						userBlockedXuidToResultsTemp[xblPermissionCheckResult.TargetXuid].Block(value);
						break;
					case XblAnonymousUserType.CrossNetworkUser:
					case XblAnonymousUserType.CrossNetworkFriend:
						foreach (IPlatformUserBlockedResults item in userBlockedAnonymousResultsTemp)
						{
							item.Block(value);
						}
						break;
					default:
						throw new NotImplementedException(string.Format("No handling for {0}.{1}.", "XblAnonymousUserType", xblPermissionCheckResult.TargetUserType));
					}
				}
			}
		}
	}

	public EMatchmakingGroup GetMatchmakingGroup()
	{
		string sandboxId = SandboxHelper.SandboxId;
		if (sandboxId == null)
		{
			EMatchmakingGroup eMatchmakingGroup = EMatchmakingGroup.Retail;
			Log.Warning(string.Format("[XBL] {0} no sandbox id. Defaulting to {1}", "GetMatchmakingGroup", eMatchmakingGroup));
			return eMatchmakingGroup;
		}
		return XblSandboxHelper.SandboxIdToMatchmakingGroup(sandboxId);
	}
}

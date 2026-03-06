using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.EOS;
using Unity.XGamingRuntime;

namespace Platform.XBL;

public class User : XblUser, IUserClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string xblMappingsPrefName = "XblMappings";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string eosRelyingPartyUrl = "https://eos.epicgames.com/";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string eosRelyingPartyHttpMethod = "GET";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XUserGetTokenAndSignatureUtf16HttpHeader[] eosRelyingPartyRequestHeaders;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly EnumDictionary<XblPermission, EBlockType> xblPermissionToBlockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XblPermission[] userBlockedPermissions;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XblAnonymousUserType[] userBlockedAnonymousTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserPrivilegeHelper privilegeHelper;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public EUserStatus loginActualUserStatus = EUserStatus.NotAttempted;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<ulong, IPlatformUserBlockedResults> userBlockedXuidToResultsTemp = new Dictionary<ulong, IPlatformUserBlockedResults>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IPlatformUserBlockedResults> userBlockedAnonymousResultsTemp = new List<IPlatformUserBlockedResults>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<IPlatform> userLoggedIn;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong userXuid;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserIdentifierXbl userIdentifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public LoginUserCallback loginUserCallback;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SocialManagerXbl SocialManager
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public FriendsListXbl FriendsList
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

	[field: PublicizedFrom(EAccessModifier.Private)]
	public MultiplayerActivityQueryManager MultiplayerActivityQueryManager
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EUserStatus UserStatus
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = EUserStatus.NotAttempted;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUserHandle GdkUserHandle
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XblContextHandle XblContextHandle
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public PlatformUserIdentifierAbs PlatformUserId => userIdentifier;

	public EUserPerms Permissions
	{
		get
		{
			if (privilegeHelper == null)
			{
				return (EUserPerms)0;
			}
			EUserPerms eUserPerms = (EUserPerms)0;
			if (privilegeHelper.MultiplayerAllowed.Has())
			{
				eUserPerms |= EUserPerms.Multiplayer | EUserPerms.HostMultiplayer;
			}
			if (privilegeHelper.CommunicationAllowed.Has())
			{
				eUserPerms |= EUserPerms.Communication;
			}
			if (privilegeHelper.CrossPlayAllowed.Has())
			{
				eUserPerms |= EUserPerms.Crossplay;
			}
			return eUserPerms;
		}
	}

	public event Action<IPlatform> UserLoggedIn
	{
		add
		{
			lock (this)
			{
				userLoggedIn = (Action<IPlatform>)Delegate.Combine(userLoggedIn, value);
				if (UserStatus == EUserStatus.LoggedIn)
				{
					value(owner);
				}
			}
		}
		remove
		{
			lock (this)
			{
				userLoggedIn = (Action<IPlatform>)Delegate.Remove(userLoggedIn, value);
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
		owner = _owner;
		owner.Api.ClientApiInitialized += apiInitialized;
		if (!GameManager.IsDedicatedServer)
		{
			PlatformManager.CrossplatformPlatform.User.UserLoggedIn += CrossLoginDone;
			XblXuidMapper.XuidMapped += OnXuidMapped;
		}
	}

	public void Login(LoginUserCallback _delegate)
	{
		if (loginActualUserStatus == EUserStatus.LoggedIn)
		{
			Log.Out("[XBL] Already logged in.");
			UserStatus = EUserStatus.LoggedIn;
			userLoggedIn?.Invoke(owner);
			_delegate?.Invoke(owner, EApiStatusReason.Ok, null);
		}
		else
		{
			Log.Out("[XBL] Login");
			loginUserCallback = _delegate;
			SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, AddUserComplete);
		}
	}

	public void PlayOffline(LoginUserCallback _delegate)
	{
		if (UserStatus != EUserStatus.LoggedIn)
		{
			throw new Exception("Can not explicitly set XBL to offline mode");
		}
		UserStatus = EUserStatus.OfflineMode;
		userLoggedIn?.Invoke(owner);
		_delegate(owner, EApiStatusReason.Ok, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAdvertisment(GameServerInfo _serverInfo)
	{
		string value = _serverInfo.GetValue(GameInfoString.UniqueId);
		uint value2 = (uint)_serverInfo.GetValue(GameInfoInt.CurrentPlayers);
		int value3 = _serverInfo.GetValue(GameInfoInt.ServerVisibility);
		uint value4 = (uint)_serverInfo.GetValue(GameInfoInt.MaxPlayers);
		if (value4 >= 2 && !string.IsNullOrEmpty(value))
		{
			XblContextHandle xblContextHandle = XblContextHandle;
			XblMultiplayerActivityInfo xblMultiplayerActivityInfo = new XblMultiplayerActivityInfo();
			xblMultiplayerActivityInfo.ConnectionString = value;
			xblMultiplayerActivityInfo.CurrentPlayers = value2;
			xblMultiplayerActivityInfo.GroupId = "Dummy";
			XblMultiplayerActivityInfo xblMultiplayerActivityInfo2 = xblMultiplayerActivityInfo;
			xblMultiplayerActivityInfo2.JoinRestriction = value3 switch
			{
				2 => XblMultiplayerActivityJoinRestriction.Public, 
				1 => XblMultiplayerActivityJoinRestriction.Followed, 
				_ => XblMultiplayerActivityJoinRestriction.InviteOnly, 
			};
			xblMultiplayerActivityInfo.MaxPlayers = value4;
			xblMultiplayerActivityInfo.Platform = XblMultiplayerActivityPlatform.All;
			xblMultiplayerActivityInfo.Xuid = userXuid;
			SDK.XBL.XblMultiplayerActivitySetActivityAsync(xblContextHandle, xblMultiplayerActivityInfo, allowCrossPlatformJoin: true, [PublicizedFrom(EAccessModifier.Internal)] (int _hresult) =>
			{
				XblHelpers.Succeeded(_hresult, "Set Activity");
			});
		}
	}

	public void StartAdvertisePlaying(GameServerInfo _serverInfo)
	{
		_serverInfo.OnChangedString += [PublicizedFrom(EAccessModifier.Internal)] (GameServerInfo _info, GameInfoString _key) =>
		{
			if (_key == GameInfoString.UniqueId)
			{
				updateAdvertisment(_serverInfo);
			}
		};
		_serverInfo.OnChangedInt += [PublicizedFrom(EAccessModifier.Internal)] (GameServerInfo _info, GameInfoInt _key) =>
		{
			if (_key == GameInfoInt.CurrentPlayers || _key == GameInfoInt.ServerVisibility || _key == GameInfoInt.MaxPlayers)
			{
				updateAdvertisment(_serverInfo);
			}
		};
	}

	public void StopAdvertisePlaying()
	{
		SDK.XBL.XblMultiplayerActivityDeleteActivityAsync(XblContextHandle, [PublicizedFrom(EAccessModifier.Internal)] (int _hresult) =>
		{
			if (XblHelpers.Succeeded(_hresult, "Delete Activity"))
			{
				Log.Out("[XBL] Activity cleared");
			}
		});
	}

	public void GetLoginTicket(Action<bool, byte[], string> _callback)
	{
		SDK.XUserGetTokenAndSignatureUtf16Async(GdkUserHandle, XUserGetTokenAndSignatureOptions.None, "GET", "https://eos.epicgames.com/", eosRelyingPartyRequestHeaders, null, CompletionRoutine);
		[PublicizedFrom(EAccessModifier.Internal)]
		void CompletionRoutine(int _hresult, XUserGetTokenAndSignatureUtf16Data _signature)
		{
			if (!XblHelpers.Succeeded(_hresult, "GetToken"))
			{
				Log.Error("[XBL] Retrieving XSTS token failed");
				_callback(arg1: false, null, null);
			}
			else
			{
				Log.Out("[XBL] XSTS token retrieved");
				_callback(arg1: true, null, _signature.Token);
			}
		}
	}

	public string GetFriendName(PlatformUserIdentifierAbs _playerId)
	{
		throw new NotImplementedException();
	}

	public bool IsFriend(PlatformUserIdentifierAbs _playerId)
	{
		if (_playerId == null)
		{
			return false;
		}
		ulong xuid = XblXuidMapper.GetXuid(_playerId);
		if (xuid == 0L)
		{
			return false;
		}
		return FriendsList?.IsFriend(xuid) ?? false;
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
		SDK.XGameUiShowPlayerProfileCardAsync(GdkUserHandle, xuid, [PublicizedFrom(EAccessModifier.Internal)] (int hr) =>
		{
			if (!XblHelpers.Succeeded(hr, "XGameUiShowPlayerProfileCardAsync"))
			{
				Log.Error("[XBL] Showing Player Profile Failed.");
			}
			else
			{
				Log.Out("[XBL] Showing Player Profile Succeeded.");
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
		if (privilegeHelper != null)
		{
			yield return privilegeHelper.ResolvePermissions(_perms, _canPrompt, _cancellationToken);
		}
	}

	public void UserAdded(PlatformUserIdentifierAbs _userId, bool _isPrimary)
	{
		if (!_isPrimary)
		{
			XblXuidMapper.GetXuid(_userId);
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
				if (XblHelpers.Succeeded(hr, "XblPrivacyBatchCheckPermissionAsync"))
				{
					HandleSuccess(_permissionResults);
				}
				else
				{
					HandleFailure();
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

	public void Destroy()
	{
		SDK.XBL.XblMultiplayerActivityDeleteActivityAsync(XblContextHandle, [PublicizedFrom(EAccessModifier.Internal)] (int _hresult) =>
		{
			if (XblHelpers.Succeeded(_hresult, "Delete Activity"))
			{
				Log.Out("[XBL] Activity deleted");
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void apiInitialized()
	{
		SandboxHelper.RefreshSandboxId();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddUserComplete(int _hresult, XUserHandle _userHandle)
	{
		if (!XblHelpers.Succeeded(_hresult, "Sign in"))
		{
			DoLoginUserCallback(EUserStatus.TemporaryError, EApiStatusReason.Unknown, $"Error code: 0x{_hresult:X8}");
			return;
		}
		GdkUserHandle = _userHandle;
		privilegeHelper = new UserPrivilegeHelper(GdkUserHandle);
		privilegeHelper.AllAllowed.ResolveSilent();
		SocialManager = new SocialManagerXbl(GdkUserHandle);
		FriendsList = new FriendsListXbl(SocialManager);
		if (!XblHelpers.Succeeded(SDK.XUserGetGamertag(GdkUserHandle, XUserGamertagComponent.Classic, out var gamertag), "Get gamertag"))
		{
			DoLoginUserCallback(EUserStatus.TemporaryError, EApiStatusReason.NoFriendsName, $"Error code: 0x{_hresult:X8}");
			return;
		}
		if (!XblHelpers.Succeeded(SDK.XUserGetId(GdkUserHandle, out var userId), "Get user id"))
		{
			DoLoginUserCallback(EUserStatus.TemporaryError, EApiStatusReason.Unknown, $"Error code: 0x{_hresult:X8}");
			return;
		}
		Log.Out($"[XBL] Signed in, id: {userId} gamertag: {gamertag}");
		if (!XblHelpers.Succeeded(SDK.XBL.XblContextCreateHandle(GdkUserHandle, out var context), "Create Xbox Live context"))
		{
			DoLoginUserCallback(EUserStatus.TemporaryError, EApiStatusReason.Unknown, $"Error code: 0x{_hresult:X8}");
			return;
		}
		XblContextHandle = context;
		MultiplayerActivityQueryManager = new MultiplayerActivityQueryManager(XblContextHandle);
		GamePrefs.Set(EnumGamePrefs.PlayerName, gamertag);
		userXuid = userId;
		Dictionary<ulong, UserIdentifierXbl> dictionary = loadUserMappings();
		if (dictionary != null && dictionary.TryGetValue(userId, out var value))
		{
			userIdentifier = value;
			XblXuidMapper.SetXuid(value, userXuid);
		}
		DoLoginUserCallback(EUserStatus.LoggedIn, EApiStatusReason.Ok, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoLoginUserCallback(EUserStatus userStatus, EApiStatusReason reason, string reasonAdditional)
	{
		UserStatus = (loginActualUserStatus = userStatus);
		if (userStatus == EUserStatus.LoggedIn)
		{
			userLoggedIn?.Invoke(owner);
		}
		loginUserCallback?.Invoke(owner, reason, reasonAdditional);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CrossLoginDone(IPlatform _sender)
	{
		if (userIdentifier == null)
		{
			PlatformUserIdentifierAbs nativePlatformUserId = ((Platform.EOS.User)_sender.User).NativePlatformUserId;
			if (nativePlatformUserId.PlatformIdentifier != EPlatformIdentifier.XBL)
			{
				Log.Error("[XBL] EOS detected different native platform: " + nativePlatformUserId.PlatformIdentifierString);
				return;
			}
			userIdentifier = (UserIdentifierXbl)nativePlatformUserId;
			XblXuidMapper.SetXuid(userIdentifier, userXuid);
			saveUserMapping();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<ulong, UserIdentifierXbl> loadUserMappings()
	{
		if (!SdPlayerPrefs.HasKey("XblMappings"))
		{
			Log.Warning("[XBL] No XUID -> PXUID mappings found");
			return null;
		}
		Dictionary<ulong, UserIdentifierXbl> dictionary = new Dictionary<ulong, UserIdentifierXbl>();
		string[] array = SdPlayerPrefs.GetString("XblMappings").Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length == 0)
			{
				continue;
			}
			string[] array2 = array[i].Split('=');
			if (array2.Length != 2)
			{
				Log.Warning("[XBL] Malformed user mapping entry: '" + array[i] + "'");
				continue;
			}
			if (!ulong.TryParse(array2[0], out var result))
			{
				Log.Warning("[XBL] Malformed user identifier entry: '" + array2[0] + "'");
				continue;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromCombinedString(array2[1]);
			if (platformUserIdentifierAbs == null)
			{
				Log.Warning("[XBL] Malformed user identifier XBL mapping entry: '" + array2[1] + "'");
			}
			else if (platformUserIdentifierAbs.PlatformIdentifier != EPlatformIdentifier.XBL)
			{
				Log.Warning("[XBL] Stored user identifier XBL mapping not an XBL identifier: '" + array2[1] + "'");
			}
			else
			{
				dictionary.Add(result, (UserIdentifierXbl)platformUserIdentifierAbs);
			}
		}
		return dictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveUserMapping()
	{
		Dictionary<ulong, UserIdentifierXbl> obj = loadUserMappings() ?? new Dictionary<ulong, UserIdentifierXbl>();
		obj[userXuid] = userIdentifier;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<ulong, UserIdentifierXbl> item in obj)
		{
			stringBuilder.Append(item.Key + "=" + item.Value.CombinedString + ";");
		}
		SdPlayerPrefs.SetString("XblMappings", stringBuilder.ToString());
		SdPlayerPrefs.Save();
	}
}

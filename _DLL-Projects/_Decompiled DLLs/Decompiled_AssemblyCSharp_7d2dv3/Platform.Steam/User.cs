using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;
using Steamworks;

namespace Platform.Steam;

public class User : IUserClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<IPlatform> userLoggedIn;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<GameOverlayActivated_t> m_gameOverlayActivated;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserIdentifierSteam platformUserId;

	[PublicizedFrom(EAccessModifier.Private)]
	public CallResult<EncryptedAppTicketResponse_t> requestEncryptedAppTicketCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<bool, byte[], string> encryptedAppTicketCallback;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EUserStatus UserStatus
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = EUserStatus.NotAttempted;

	public PlatformUserIdentifierAbs PlatformUserId => platformUserId;

	public EUserPerms Permissions => EUserPerms.All;

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

	public event UserBlocksChangedCallback UserBlocksChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (!GameManager.IsDedicatedServer && m_gameOverlayActivated == null)
			{
				m_gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(GameOverlayActivated);
			}
		};
	}

	public void Login(LoginUserCallback _delegate)
	{
		if (UserStatus == EUserStatus.LoggedIn)
		{
			Log.Out("[Steamworks.NET] Already logged in");
			_delegate(owner, EApiStatusReason.Ok, null);
			return;
		}
		if (owner.Api.ClientApiStatus == EApiStatus.PermanentError)
		{
			Log.Out("[Steamworks.NET] API could not be loaded.");
			UserStatus = EUserStatus.PermanentError;
			_delegate(owner, EApiStatusReason.ApiNotLoadable, null);
			return;
		}
		if (owner.Api.ClientApiStatus == EApiStatus.TemporaryError)
		{
			owner.Api.InitClientApis();
			if (owner.Api.ClientApiStatus == EApiStatus.TemporaryError)
			{
				Log.Out("[Steamworks.NET] API could not be initialized - probably Steam not running.");
				UserStatus = EUserStatus.TemporaryError;
				_delegate(owner, EApiStatusReason.SteamNotRunning, null);
				return;
			}
		}
		if (!SteamApps.BIsSubscribedApp((AppId_t)251570u))
		{
			Log.Out("[Steamworks.NET] User not licensed for app.");
			UserStatus = EUserStatus.PermanentError;
			_delegate(owner, EApiStatusReason.NoLicense, null);
			return;
		}
		string personaName = SteamFriends.GetPersonaName();
		if (string.IsNullOrEmpty(personaName))
		{
			Log.Out("[Steamworks.NET] Username not found.");
			UserStatus = EUserStatus.TemporaryError;
			_delegate(owner, EApiStatusReason.NoFriendsName, null);
			return;
		}
		GamePrefs.Set(EnumGamePrefs.PlayerName, personaName);
		platformUserId = new UserIdentifierSteam(SteamUser.GetSteamID());
		if (!SteamUser.BLoggedOn())
		{
			UserStatus = EUserStatus.OfflineMode;
			Log.Out("[Steamworks.NET] User not logged in.");
			_delegate(owner, EApiStatusReason.NotLoggedOn, null);
		}
		else
		{
			Log.Out("[Steamworks.NET] Login ok.");
			UserStatus = EUserStatus.LoggedIn;
			userLoggedIn?.Invoke(owner);
			_delegate(owner, EApiStatusReason.Ok, null);
		}
	}

	public void PlayOffline(LoginUserCallback _delegate)
	{
		if (UserStatus != EUserStatus.OfflineMode && UserStatus != EUserStatus.LoggedIn)
		{
			throw new Exception("Can not explicitly set Steam to offline mode");
		}
		UserStatus = EUserStatus.OfflineMode;
		userLoggedIn?.Invoke(owner);
		_delegate(owner, EApiStatusReason.Ok, null);
	}

	public void StartAdvertisePlaying(GameServerInfo _serverInfo)
	{
	}

	public void StopAdvertisePlaying()
	{
	}

	public void GetLoginTicket(Action<bool, byte[], string> _callback)
	{
		if (requestEncryptedAppTicketCallback == null)
		{
			requestEncryptedAppTicketCallback = CallResult<EncryptedAppTicketResponse_t>.Create(EncryptedAppTicketCallback);
		}
		encryptedAppTicketCallback = _callback;
		SteamAPICall_t hAPICall = SteamUser.RequestEncryptedAppTicket(null, 0);
		requestEncryptedAppTicketCallback.Set(hAPICall);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EncryptedAppTicketCallback(EncryptedAppTicketResponse_t _result, bool _ioFailure)
	{
		if (_ioFailure || _result.m_eResult != EResult.k_EResultOK)
		{
			Callback(null, "[Steamworks.NET] RequestEncryptedAppTicket failed (result=" + _result.m_eResult.ToStringCached() + ")");
			return;
		}
		SteamUser.GetEncryptedAppTicket(null, 0, out var pcbTicket);
		if (pcbTicket == 0 || pcbTicket > 1024)
		{
			Callback(null, $"[Steamworks.NET] Fetching encrypted app ticket size: {pcbTicket}");
			return;
		}
		byte[] array = new byte[pcbTicket];
		if (!SteamUser.GetEncryptedAppTicket(array, (int)pcbTicket, out var pcbTicket2))
		{
			Callback(null, "[Steamworks.NET] Failed fetching encrypted app ticket");
		}
		else if (pcbTicket2 != pcbTicket)
		{
			Callback(null, $"[Steamworks.NET] Ticket size expected {pcbTicket} does not match ticket size received {pcbTicket2}");
		}
		else
		{
			Callback(array, null);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void Callback(byte[] _ticket, string _message)
		{
			if (_message != null)
			{
				Log.Error(_message);
			}
			encryptedAppTicketCallback?.Invoke(_message == null, _ticket, null);
			encryptedAppTicketCallback = null;
		}
	}

	public string GetFriendName(PlatformUserIdentifierAbs _playerId)
	{
		if (!(_playerId is UserIdentifierSteam userIdentifierSteam))
		{
			return null;
		}
		return SteamFriends.GetFriendPersonaName(new CSteamID(userIdentifierSteam.SteamId));
	}

	public bool IsFriend(PlatformUserIdentifierAbs _playerId)
	{
		if (!(_playerId is UserIdentifierSteam userIdentifierSteam))
		{
			return false;
		}
		if (owner.Api.ClientApiStatus != EApiStatus.Ok)
		{
			return false;
		}
		return SteamFriends.GetFriendRelationship(new CSteamID(userIdentifierSteam.SteamId)) == EFriendRelationship.k_EFriendRelationshipFriend;
	}

	public string GetPermissionDenyReason(EUserPerms _perms)
	{
		return null;
	}

	public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
	{
		return Enumerable.Empty<object>().GetEnumerator();
	}

	public void UserAdded(PlatformUserIdentifierAbs _userId, bool _isPrimary)
	{
	}

	public IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results)
	{
		return Enumerable.Empty<object>().GetEnumerator();
	}

	public EMatchmakingGroup GetMatchmakingGroup()
	{
		return EMatchmakingGroup.Retail;
	}

	public void Destroy()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GameOverlayActivated(GameOverlayActivated_t _val)
	{
		InputManager.Enabled = _val.m_bActive == 0;
	}
}

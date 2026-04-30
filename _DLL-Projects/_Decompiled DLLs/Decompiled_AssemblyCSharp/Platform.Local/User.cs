using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Local;

public class User : IUserClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserIdentifierLocal platformUserId;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EUserStatus UserStatus
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public PlatformUserIdentifierAbs PlatformUserId => platformUserId;

	public EUserPerms Permissions => EUserPerms.All;

	public event Action<IPlatform> UserLoggedIn
	{
		add
		{
			lock (this)
			{
				value(owner);
			}
		}
		remove
		{
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
		GamePrefs.OnGamePrefChanged += [PublicizedFrom(EAccessModifier.Private)] (EnumGamePrefs _pref) =>
		{
			if (_pref == EnumGamePrefs.PlayerName)
			{
				platformUserId = new UserIdentifierLocal(GamePrefs.GetString(EnumGamePrefs.PlayerName));
			}
		};
	}

	public void Login(LoginUserCallback _delegate)
	{
		platformUserId = new UserIdentifierLocal(GamePrefs.GetString(EnumGamePrefs.PlayerName));
		_delegate(owner, EApiStatusReason.Ok, null);
	}

	public void PlayOffline(LoginUserCallback _delegate)
	{
		UserStatus = EUserStatus.OfflineMode;
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
		throw new NotImplementedException();
	}

	public string GetFriendName(PlatformUserIdentifierAbs _playerId)
	{
		return null;
	}

	public bool IsFriend(PlatformUserIdentifierAbs _playerId)
	{
		return true;
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

	public void Destroy()
	{
	}
}

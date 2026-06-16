using System;
using System.Collections;
using System.Collections.Generic;

namespace Platform;

public interface IUserClient
{
	EUserStatus UserStatus { get; }

	PlatformUserIdentifierAbs PlatformUserId { get; }

	EUserPerms Permissions { get; }

	event Action<IPlatform> UserLoggedIn;

	event UserBlocksChangedCallback UserBlocksChanged;

	void Init(IPlatform _owner);

	void Login(LoginUserCallback _delegate);

	void PlayOffline(LoginUserCallback _delegate);

	EMatchmakingGroup GetMatchmakingGroup()
	{
		return EMatchmakingGroup.Dev;
	}

	void StartAdvertisePlaying(GameServerInfo _serverInfo);

	void StopAdvertisePlaying();

	void GetLoginTicket(Action<bool, byte[], string> _callback);

	string GetFriendName(PlatformUserIdentifierAbs _playerId);

	bool IsFriend(PlatformUserIdentifierAbs _playerId);

	bool CanShowProfile(PlatformUserIdentifierAbs _playerId)
	{
		return false;
	}

	void ShowProfile(PlatformUserIdentifierAbs _playerId)
	{
	}

	string GetPermissionDenyReason(EUserPerms _perms);

	IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken cancellationToken = null);

	IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results);

	void Destroy();
}

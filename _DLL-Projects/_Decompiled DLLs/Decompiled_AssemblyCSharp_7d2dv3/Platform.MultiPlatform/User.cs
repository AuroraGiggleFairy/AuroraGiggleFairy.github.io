using System;
using System.Collections;
using System.Collections.Generic;

namespace Platform.MultiPlatform;

public class User : IUserClient
{
	public EUserStatus UserStatus
	{
		get
		{
			if (PlatformManager.CrossplatformPlatform?.User == null)
			{
				return PlatformManager.NativePlatform.User.UserStatus;
			}
			return PlatformManager.CrossplatformPlatform.User.UserStatus;
		}
	}

	public PlatformUserIdentifierAbs PlatformUserId => PlatformManager.CrossplatformPlatform?.User?.PlatformUserId ?? PlatformManager.NativePlatform.User.PlatformUserId;

	public EUserPerms Permissions
	{
		get
		{
			if (GameManager.IsDedicatedServer)
			{
				return EUserPerms.All;
			}
			return PlatformManager.NativePlatform.User.Permissions & (PlatformManager.CrossplatformPlatform?.User?.Permissions ?? EUserPerms.All);
		}
	}

	public event Action<IPlatform> UserLoggedIn
	{
		add
		{
			lock (this)
			{
				PlatformManager.NativePlatform.User.UserLoggedIn += value;
				if (PlatformManager.CrossplatformPlatform?.User != null)
				{
					PlatformManager.CrossplatformPlatform.User.UserLoggedIn += value;
				}
			}
		}
		remove
		{
			lock (this)
			{
				PlatformManager.NativePlatform.User.UserLoggedIn -= value;
				if (PlatformManager.CrossplatformPlatform?.User != null)
				{
					PlatformManager.CrossplatformPlatform.User.UserLoggedIn -= value;
				}
			}
		}
	}

	public event UserBlocksChangedCallback UserBlocksChanged
	{
		add
		{
			PlatformManager.NativePlatform.User.UserBlocksChanged += value;
			IUserClient userClient = PlatformManager.CrossplatformPlatform?.User;
			if (userClient != null)
			{
				userClient.UserBlocksChanged += value;
			}
		}
		remove
		{
			PlatformManager.NativePlatform.User.UserBlocksChanged -= value;
			IUserClient userClient = PlatformManager.CrossplatformPlatform?.User;
			if (userClient != null)
			{
				userClient.UserBlocksChanged -= value;
			}
		}
	}

	public void Init(IPlatform _owner)
	{
	}

	public void Login(LoginUserCallback _delegate)
	{
		PlatformManager.NativePlatform.User.Login([PublicizedFrom(EAccessModifier.Internal)] (IPlatform _nativePlatform, EApiStatusReason _nativeReason, string _statusReasonAdditionalText) =>
		{
			if (_nativePlatform.Api.ClientApiStatus != EApiStatus.Ok || _nativePlatform.User.UserStatus != EUserStatus.LoggedIn)
			{
				_delegate(_nativePlatform, _nativeReason, _statusReasonAdditionalText);
			}
			else if (_nativeReason != EApiStatusReason.Ok)
			{
				_delegate(_nativePlatform, _nativeReason, _statusReasonAdditionalText);
			}
			else if (PlatformManager.CrossplatformPlatform?.User == null)
			{
				_delegate(_nativePlatform, _nativeReason, _statusReasonAdditionalText);
			}
			else
			{
				PlatformManager.CrossplatformPlatform.User.Login(_delegate);
			}
		});
	}

	public void PlayOffline(LoginUserCallback _delegate)
	{
		PlatformManager.NativePlatform.User.PlayOffline([PublicizedFrom(EAccessModifier.Internal)] (IPlatform _nativePlatform, EApiStatusReason _nativeReason, string _statusReasonAdditionalText) =>
		{
			if (_nativePlatform.Api.ClientApiStatus != EApiStatus.Ok || _nativePlatform.User.UserStatus != EUserStatus.OfflineMode)
			{
				_delegate(_nativePlatform, _nativeReason, _statusReasonAdditionalText);
			}
			else if (PlatformManager.CrossplatformPlatform?.User == null)
			{
				_delegate(_nativePlatform, _nativeReason, _statusReasonAdditionalText);
			}
			else
			{
				PlatformManager.CrossplatformPlatform.User.PlayOffline(_delegate);
			}
		});
	}

	public void StartAdvertisePlaying(GameServerInfo _serverInfo)
	{
		PlatformManager.CrossplatformPlatform?.User?.StartAdvertisePlaying(_serverInfo);
		PlatformManager.NativePlatform.User.StartAdvertisePlaying(_serverInfo);
	}

	public void StopAdvertisePlaying()
	{
		PlatformManager.CrossplatformPlatform?.User?.StopAdvertisePlaying();
		PlatformManager.NativePlatform.User.StopAdvertisePlaying();
	}

	public void GetLoginTicket(Action<bool, byte[], string> _callback)
	{
		throw new NotImplementedException();
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
		IPlatform platform = PlatformManager.InstanceForPlatformIdentifier(_playerId.PlatformIdentifier);
		if (platform == null)
		{
			return false;
		}
		return platform.User?.IsFriend(_playerId) ?? false;
	}

	public bool CanShowProfile(PlatformUserIdentifierAbs _playerId)
	{
		if (!PlatformManager.NativePlatform.User.CanShowProfile(_playerId))
		{
			return PlatformManager.CrossplatformPlatform?.User?.CanShowProfile(_playerId) == true;
		}
		return true;
	}

	public void ShowProfile(PlatformUserIdentifierAbs _playerId)
	{
		if (PlatformManager.NativePlatform.User.CanShowProfile(_playerId))
		{
			PlatformManager.NativePlatform.User.ShowProfile(_playerId);
		}
		else if (PlatformManager.CrossplatformPlatform?.User?.CanShowProfile(_playerId) == true)
		{
			PlatformManager.CrossplatformPlatform.User.ShowProfile(_playerId);
		}
	}

	public string GetPermissionDenyReason(EUserPerms _perms)
	{
		string text = PlatformManager.CrossplatformPlatform?.User?.GetPermissionDenyReason(_perms);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		string permissionDenyReason = PlatformManager.NativePlatform.User.GetPermissionDenyReason(_perms);
		if (!string.IsNullOrEmpty(permissionDenyReason))
		{
			return permissionDenyReason;
		}
		return null;
	}

	public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
	{
		if (_canPrompt && UserStatus != EUserStatus.LoggedIn)
		{
			Log.Out("[MultiPlatform] ResolvePermissions: Attempting Login as we're allowed to prompt.");
			bool loginAttemptDone = false;
			Login([PublicizedFrom(EAccessModifier.Internal)] (IPlatform platform, EApiStatusReason reason, string text) =>
			{
				CoroutineCancellationToken coroutineCancellationToken = _cancellationToken;
				if (coroutineCancellationToken == null || !coroutineCancellationToken.IsCancelled())
				{
					loginAttemptDone = true;
					EUserStatus userStatus = UserStatus;
					((userStatus == EUserStatus.LoggedIn) ? new Action<string>(Log.Out) : new Action<string>(Log.Warning))(string.Format("[MultiPlatform] {0}: Login Attempt Completed. Status: {1}, Platform: {2}, Reason: {3}, Additional Reason: '{4}'.", "ResolvePermissions", userStatus, platform, reason, text));
				}
			});
			while (!loginAttemptDone)
			{
				yield return null;
				if (_cancellationToken?.IsCancelled() ?? false)
				{
					yield break;
				}
			}
		}
		yield return PlatformManager.NativePlatform.User.ResolvePermissions(_perms, _canPrompt, _cancellationToken);
		_perms &= PlatformManager.NativePlatform.User.Permissions;
		if (_perms != 0)
		{
			yield return PlatformManager.CrossplatformPlatform?.User?.ResolvePermissions(_perms, _canPrompt, _cancellationToken);
		}
	}

	public IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results)
	{
		if (GameManager.IsDedicatedServer)
		{
			yield break;
		}
		if (!Permissions.HasCommunication())
		{
			PlatformUserIdentifierAbs platformUserId = PlatformUserId;
			foreach (IPlatformUserBlockedResults _result in _results)
			{
				if (!object.Equals(platformUserId, _result.User.PrimaryId))
				{
					_result.Block(EBlockType.TextChat);
					_result.Block(EBlockType.VoiceChat);
				}
			}
		}
		yield return PlatformManager.NativePlatform.User.ResolveUserBlocks(_results);
		yield return PlatformManager.CrossplatformPlatform?.User?.ResolveUserBlocks(_results);
	}

	public EMatchmakingGroup GetMatchmakingGroup()
	{
		return PlatformManager.NativePlatform.User?.GetMatchmakingGroup() ?? EMatchmakingGroup.Dev;
	}

	public void Destroy()
	{
	}
}

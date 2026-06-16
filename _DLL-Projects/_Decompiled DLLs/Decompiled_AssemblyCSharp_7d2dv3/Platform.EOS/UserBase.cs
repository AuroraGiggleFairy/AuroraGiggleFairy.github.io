using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace Platform.EOS;

public abstract class UserBase : IUserClient
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public IPlatform Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong notifyAuthExpirationHandle;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Action<IPlatform> userLoggedIn;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UserIdentifierEos PlatformUserIdEos;

	public ConnectInterface ConnectInterface
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return ((Api)Owner.Api).ConnectInterface;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EUserStatus UserStatus
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	} = EUserStatus.NotAttempted;

	public PlatformUserIdentifierAbs PlatformUserId => PlatformUserIdEos;

	public abstract EUserPerms Permissions { get; }

	public event Action<IPlatform> UserLoggedIn
	{
		add
		{
			lock (this)
			{
				userLoggedIn = (Action<IPlatform>)Delegate.Combine(userLoggedIn, value);
				if (UserStatus == EUserStatus.LoggedIn)
				{
					value(Owner);
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

	public virtual void Init(IPlatform _owner)
	{
		Owner = _owner;
		Owner.Api.ClientApiInitialized += apiInitialized;
	}

	public virtual void Login(LoginUserCallback _delegate)
	{
	}

	public virtual void PlayOffline(LoginUserCallback _delegate)
	{
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
		throw new NotImplementedException();
	}

	public bool IsFriend(PlatformUserIdentifierAbs _playerId)
	{
		return false;
	}

	public virtual string GetPermissionDenyReason(EUserPerms _perms)
	{
		return null;
	}

	public virtual IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
	{
		yield break;
	}

	public IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results)
	{
		return Enumerable.Empty<object>().GetEnumerator();
	}

	public virtual void Destroy()
	{
		EosHelpers.AssertMainThread("Usr.Destroy");
		removeNotifications();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void apiInitialized()
	{
		EosHelpers.AssertMainThread("Usr.Init");
		addNotifications();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addNotifications()
	{
		if (ConnectInterface == null)
		{
			return;
		}
		EosHelpers.AssertMainThread("Usr.AddNtfs");
		AddNotifyAuthExpirationOptions options = default(AddNotifyAuthExpirationOptions);
		lock (AntiCheatCommon.LockObject)
		{
			notifyAuthExpirationHandle = ConnectInterface.AddNotifyAuthExpiration(ref options, null, OnAuthExpiration);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeNotifications()
	{
		if (ConnectInterface == null)
		{
			return;
		}
		EosHelpers.AssertMainThread("Usr.RemNtfs");
		if (notifyAuthExpirationHandle != 0L)
		{
			lock (AntiCheatCommon.LockObject)
			{
				ConnectInterface.RemoveNotifyAuthExpiration(notifyAuthExpirationHandle);
			}
			notifyAuthExpirationHandle = 0uL;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAuthExpiration(ref AuthExpirationCallbackInfo _data)
	{
		Log.Out("[EOS] Refreshing Login");
		startLogin(null, _refreshing: true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void startLogin(LoginUserCallback _delegate, bool _refreshing = false);

	[PublicizedFrom(EAccessModifier.Protected)]
	public void connectLogin(byte[] _byteTicket, string _stringTicket, ExternalCredentialType _externalType, LoginUserCallback _callback, bool _refreshing, string _displayName = null)
	{
		EosHelpers.AssertMainThread("Usr.Log");
		Utf8String token = ((_byteTicket != null) ? Common.ToString(new ArraySegment<byte>(_byteTicket)) : ((_stringTicket != null) ? new Utf8String(_stringTicket) : null));
		LoginOptions options = new LoginOptions
		{
			Credentials = new Credentials
			{
				Token = token,
				Type = _externalType
			},
			UserLoginInfo = null
		};
		if (_displayName != null)
		{
			options.UserLoginInfo = new UserLoginInfo
			{
				DisplayName = _displayName
			};
		}
		lock (AntiCheatCommon.LockObject)
		{
			ConnectInterface.Login(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref LoginCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode == Result.Success)
				{
					if (!_refreshing)
					{
						Log.Out("[EOS] Login succeeded, PUID: " + _callbackData.LocalUserId);
						eosLoggedIn(_callbackData.LocalUserId, _callback);
					}
					else
					{
						Log.Out("[EOS] Login refreshed");
					}
				}
				else if (Common.IsOperationComplete(_callbackData.ResultCode))
				{
					if (_callbackData.ResultCode == Result.InvalidUser)
					{
						if (!_refreshing)
						{
							connectCreateUser(_callbackData.ContinuanceToken, _callback);
						}
						else
						{
							Log.Error("[EOS] Login refresh failed, invalid user");
						}
					}
					else
					{
						Log.Warning(string.Format("[EOS] Login {0}failed: {1}", _refreshing ? "refresh " : "", _callbackData.ResultCode));
						switch (_callbackData.ResultCode)
						{
						case Result.ConnectExternalServiceUnavailable:
							UserStatus = EUserStatus.OfflineMode;
							_callback?.Invoke(Owner, EApiStatusReason.ExternalAuthUnavailable, PlatformManager.NativePlatform.PlatformDisplayName);
							break;
						case Result.UnexpectedError:
							UserStatus = EUserStatus.OfflineMode;
							_callback?.Invoke(Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
							break;
						case Result.UnrecognizedResponse:
							UserStatus = EUserStatus.OfflineMode;
							_callback?.Invoke(Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
							break;
						default:
							UserStatus = EUserStatus.TemporaryError;
							_callback?.Invoke(Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
							break;
						}
					}
				}
				else
				{
					Log.Error("[EOS] Login " + (_refreshing ? "refresh " : "") + "error: " + _callbackData.ResultCode);
					UserStatus = EUserStatus.PermanentError;
					_callback?.Invoke(Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
				}
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void connectCreateUser(ContinuanceToken _continuanceToken, LoginUserCallback _callback);

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void eosLoggedIn(ProductUserId _puid, LoginUserCallback _callback)
	{
		PlatformUserIdEos = new UserIdentifierEos(_puid);
		if (EosHelpers.UserAccountState != EUserAccountState.NewUser)
		{
			EosHelpers.UserAccountState = EUserAccountState.ReturningUser;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void eosLoginDone(LoginUserCallback _callback)
	{
		UserStatus = EUserStatus.LoggedIn;
		userLoggedIn?.Invoke(Owner);
		_callback?.Invoke(Owner, EApiStatusReason.Ok, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UserBase()
	{
	}
}

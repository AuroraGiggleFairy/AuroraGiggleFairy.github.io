using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using UnityEngine;

namespace Platform.EOS;

public class User : IUserClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string eosMappingsPrefName = "EosMappings";

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public IApplicationStateController nativeApplicationStateController;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExternalCredentialType externalCredentialType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasSuspended;

	[PublicizedFrom(EAccessModifier.Private)]
	public int resumeCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldRefreshLoginOnResume;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong notifyAuthExpirationHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<IPlatform> userLoggedIn;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserIdentifierEos platformUserId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerHasSanctions;

	[PublicizedFrom(EAccessModifier.Private)]
	public string reasonForPermissions;

	public ConnectInterface connectInterface
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return ((Api)owner.Api).ConnectInterface;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EUserStatus UserStatus
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = EUserStatus.NotAttempted;

	public PlatformUserIdentifierAbs PlatformUserId => platformUserId;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs NativePlatformUserId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public EUserPerms Permissions
	{
		get
		{
			if (playerHasSanctions)
			{
				return EUserPerms.Multiplayer | EUserPerms.Communication | EUserPerms.Crossplay;
			}
			return EUserPerms.All;
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
		owner.Api.ClientApiInitialized += apiInitialized;
		EPlatformIdentifier platformIdentifier = PlatformManager.NativePlatform.PlatformIdentifier;
		externalCredentialType = platformIdentifier switch
		{
			EPlatformIdentifier.Steam => ExternalCredentialType.SteamAppTicket, 
			EPlatformIdentifier.XBL => ExternalCredentialType.XblXstsToken, 
			EPlatformIdentifier.PSN => ExternalCredentialType.PsnIdToken, 
			_ => throw new Exception("[EOS] Can not run EOS with the " + platformIdentifier.ToStringCached() + " platform"), 
		};
		nativeApplicationStateController = PlatformManager.NativePlatform.ApplicationState;
		if (nativeApplicationStateController != null)
		{
			nativeApplicationStateController.OnApplicationStateChanged += OnApplicationStateChanged;
		}
	}

	public void Login(LoginUserCallback _delegate)
	{
		if (UserStatus == EUserStatus.LoggedIn)
		{
			Log.Out("[EOS] Login already done.");
			eosLoginDone(_delegate);
			return;
		}
		Log.Out("[EOS] Login");
		EosHelpers.TestEosConnection([PublicizedFrom(EAccessModifier.Internal)] (bool _success) =>
		{
			if (_success)
			{
				if (PlatformManager.NativePlatform.User.UserStatus == EUserStatus.LoggedIn)
				{
					FetchTicket(_delegate);
				}
				else
				{
					UserStatus = EUserStatus.OfflineMode;
					_delegate(owner, EApiStatusReason.Other, "User offline");
				}
			}
			else
			{
				UserStatus = EUserStatus.OfflineMode;
				_delegate(owner, EApiStatusReason.Other, "No connection to EOS backend");
			}
		});
	}

	public void PlayOffline(LoginUserCallback _delegate)
	{
		UserStatus = EUserStatus.NotAttempted;
		Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos> dictionary = loadUserMappings();
		if (dictionary == null)
		{
			_delegate(owner, EApiStatusReason.NoOnlineStart, null);
			return;
		}
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformManager.NativePlatform.User.PlatformUserId;
		if (platformUserIdentifierAbs == null)
		{
			Log.Warning("[EOS] No native platform user logged in, can not proceed in offline mode");
			_delegate(owner, EApiStatusReason.Other, "Not logged in to native platform");
			return;
		}
		if (!dictionary.TryGetValue(platformUserIdentifierAbs, out var value))
		{
			Log.Warning("[EOS] No mapping for the logged in user: " + platformUserIdentifierAbs.CombinedString);
			_delegate(owner, EApiStatusReason.NoOnlineStart, null);
			return;
		}
		platformUserId = value;
		UserStatus = EUserStatus.OfflineMode;
		userLoggedIn?.Invoke(owner);
		_delegate(owner, EApiStatusReason.NotLoggedOn, null);
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

	public string GetPermissionDenyReason(EUserPerms _perms)
	{
		EUserPerms eUserPerms = ~Permissions & _perms;
		if (eUserPerms.HasFlag(EUserPerms.HostMultiplayer))
		{
			return reasonForPermissions;
		}
		return null;
	}

	public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
	{
		Log.Out(string.Format("[EOS] {0}({1}: [{2}], {3}: {4})", "ResolvePermissions", "_perms", _perms, "_canPrompt", _canPrompt));
		if (UserStatus != EUserStatus.LoggedIn)
		{
			yield break;
		}
		if (((Api)owner.Api).SanctionsInterface == null || ((Api)owner.Api).eosSanctionsCheck == null)
		{
			Log.Out($"[EOS] ResolvePermissions not possible: eosSanctionsCheck: {((Api)owner.Api).eosSanctionsCheck != null}, SanctionsInterface: {((Api)owner.Api).SanctionsInterface != null}");
			playerHasSanctions = true;
		}
		else
		{
			if (!_perms.HasHostMultiplayer())
			{
				yield break;
			}
			bool connectionTestComplete = false;
			bool connectionTestSuccess = false;
			EosHelpers.TestEosConnection([PublicizedFrom(EAccessModifier.Internal)] (bool isConnected) =>
			{
				connectionTestComplete = true;
				connectionTestSuccess = isConnected;
			});
			while (!connectionTestComplete)
			{
				yield return null;
				if (_cancellationToken?.IsCancelled() ?? false)
				{
					yield break;
				}
			}
			if (!connectionTestSuccess)
			{
				Log.Out("[EOS] Could not check sanctions as the connection test failed");
				playerHasSanctions = true;
				reasonForPermissions = Localization.Get("permissionsSanction_error");
				yield break;
			}
			yield return (owner.Api as Api).eosSanctionsCheck.CheckSanctionsEnumerator((owner.Api as Api).SanctionsInterface, platformUserId.ProductUserId, platformUserId.ProductUserId, [PublicizedFrom(EAccessModifier.Private)] (SanctionsCheckResult checkResult) =>
			{
				if (checkResult.Success)
				{
					Log.Out($"[EOS] CheckSanctionsEnumerator: hasSanctions {checkResult.HasActiveSanctions}");
					if (checkResult.HasActiveSanctions)
					{
						playerHasSanctions = true;
						reasonForPermissions = checkResult.ReasonForSanction;
					}
					else
					{
						playerHasSanctions = false;
						reasonForPermissions = null;
					}
				}
				else
				{
					playerHasSanctions = true;
					reasonForPermissions = Localization.Get("permissionsSanction_error");
				}
			}, _cancellationToken);
		}
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
		EosHelpers.AssertMainThread("Usr.Destroy");
		RemoveNotifications();
		if (nativeApplicationStateController != null)
		{
			nativeApplicationStateController.OnApplicationStateChanged -= OnApplicationStateChanged;
			nativeApplicationStateController = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void apiInitialized()
	{
		EosHelpers.AssertMainThread("Usr.Init");
		AddNotifications();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationStateChanged(ApplicationState newState)
	{
		bool flag = newState == ApplicationState.Suspended;
		if (wasSuspended != flag)
		{
			wasSuspended = flag;
			if (flag)
			{
				OnSuspend();
				return;
			}
			resumeCount++;
			OnResume();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSuspend()
	{
		shouldRefreshLoginOnResume = shouldRefreshLoginOnResume || UserStatus == EUserStatus.LoggedIn;
		Log.Out($"[EOS] User.OnSuspend() shouldRefreshLoginOnResume: {shouldRefreshLoginOnResume}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnResume()
	{
		Log.Out($"[EOS] User.OnResume() shouldRefreshLoginOnResume: {shouldRefreshLoginOnResume}");
		if (shouldRefreshLoginOnResume)
		{
			ThreadManager.StartCoroutine(OnResumeRefreshLoginCoroutine());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator OnResumeRefreshLoginCoroutine()
	{
		int initialResumeCount = resumeCount;
		yield return RefreshLoginCoroutine();
		if (ShouldExitCoroutine(attemptedLogin: true))
		{
			yield break;
		}
		if (nativeApplicationStateController != null)
		{
			Log.Out("[EOS] Waiting for network to be ready...");
			do
			{
				yield return new WaitForSecondsRealtime(0.25f);
				if (ShouldExitCoroutine(attemptedLogin: false))
				{
					yield break;
				}
			}
			while (!nativeApplicationStateController.NetworkConnectionState);
			Log.Out("[EOS] Network is ready. Trying to refresh login...");
			yield return RefreshLoginCoroutine();
			if (ShouldExitCoroutine(attemptedLogin: true))
			{
				yield break;
			}
		}
		Log.Out("[EOS] Waiting for EOS to be reachable...");
		bool eosReachable = false;
		bool eosReachableFirstCheck = true;
		float waitTime = 2f;
		while (!eosReachable)
		{
			if (eosReachableFirstCheck)
			{
				eosReachableFirstCheck = false;
			}
			else
			{
				Log.Out($"[EOS] No connection to EOS. Will retry in {waitTime} s");
				yield return new WaitForSecondsRealtime(waitTime);
				waitTime = Math.Min(waitTime * 2f, 60f);
			}
			if (ShouldExitCoroutine(attemptedLogin: false))
			{
				yield break;
			}
			Log.Out("[EOS] Testing connecting to EOS...");
			bool eosTestComplete = false;
			EosHelpers.TestEosConnection([PublicizedFrom(EAccessModifier.Internal)] (bool success) =>
			{
				eosReachable = success;
				eosTestComplete = true;
			});
			while (!eosTestComplete)
			{
				yield return new WaitForSecondsRealtime(0.25f);
				if (ShouldExitCoroutine(attemptedLogin: false))
				{
					yield break;
				}
			}
		}
		Log.Out("[EOS] EOS is reachable so we can try refresh the login now.");
		yield return RefreshLoginCoroutine();
		if (UserStatus == EUserStatus.LoggedIn)
		{
			Log.Warning("[EOS] Refresh login on resume has failed. User will have to trigger a login through other means.");
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool ShouldExitCoroutine(bool attemptedLogin)
		{
			if (initialResumeCount != resumeCount)
			{
				Log.Out("[EOS] Another resume is in progress. Exiting.");
				return true;
			}
			if (!shouldRefreshLoginOnResume)
			{
				Log.Out("[EOS] Refresh login on resume is no longer needed. Exiting.");
				return true;
			}
			if (UserStatus == EUserStatus.LoggedIn)
			{
				shouldRefreshLoginOnResume = false;
				if (!attemptedLogin)
				{
					Log.Out("[EOS] User logged in through other means. Exiting.");
				}
				else
				{
					Log.Out("[EOS] User successfully logged in on resume.");
				}
				return true;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddNotifications()
	{
		if (connectInterface == null)
		{
			return;
		}
		EosHelpers.AssertMainThread("Usr.AddNtfs");
		AddNotifyAuthExpirationOptions options = default(AddNotifyAuthExpirationOptions);
		lock (AntiCheatCommon.LockObject)
		{
			notifyAuthExpirationHandle = connectInterface.AddNotifyAuthExpiration(ref options, null, OnAuthExpiration);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveNotifications()
	{
		if (connectInterface == null)
		{
			return;
		}
		EosHelpers.AssertMainThread("Usr.RemNtfs");
		if (notifyAuthExpirationHandle != 0L)
		{
			lock (AntiCheatCommon.LockObject)
			{
				connectInterface.RemoveNotifyAuthExpiration(notifyAuthExpirationHandle);
			}
			notifyAuthExpirationHandle = 0uL;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAuthExpiration(ref AuthExpirationCallbackInfo _data)
	{
		RefreshLogin();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshLogin()
	{
		Log.Out("[EOS] Refreshing Login");
		FetchTicket(null, _refreshing: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator RefreshLoginCoroutine()
	{
		bool done = false;
		Log.Out("[EOS] Refreshing Login");
		FetchTicket([PublicizedFrom(EAccessModifier.Internal)] (IPlatform _, EApiStatusReason _, string _) =>
		{
			done = true;
		}, _refreshing: true);
		while (!done)
		{
			yield return new WaitForSecondsRealtime(0.25f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FetchTicket(LoginUserCallback _delegate, bool _refreshing = false)
	{
		PlatformManager.NativePlatform.User.GetLoginTicket([PublicizedFrom(EAccessModifier.Internal)] (bool _success, byte[] _byteTicket, string _stringTicket) =>
		{
			if (_success)
			{
				ConnectLogin(_byteTicket, _stringTicket, externalCredentialType, _delegate, _refreshing);
			}
			else
			{
				Log.Error("[EOS] Failed fetching login ticket from native platform");
				UserStatus = EUserStatus.TemporaryError;
				_delegate?.Invoke(owner, EApiStatusReason.NoLoginTicket, null);
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectLogin(byte[] _byteTicket, string _stringTicket, ExternalCredentialType _externalType, LoginUserCallback _callback, bool _refreshing)
	{
		EosHelpers.AssertMainThread("Usr.Log");
		Utf8String token = ((_byteTicket != null) ? Common.ToString(new ArraySegment<byte>(_byteTicket)) : new Utf8String(_stringTicket));
		LoginOptions options = new LoginOptions
		{
			Credentials = new Credentials
			{
				Token = token,
				Type = _externalType
			},
			UserLoginInfo = null
		};
		lock (AntiCheatCommon.LockObject)
		{
			connectInterface.Login(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref LoginCallbackInfo _callbackData) =>
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
							ConnectCreateUser(_callbackData.ContinuanceToken, _callback);
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
							_callback?.Invoke(owner, EApiStatusReason.ExternalAuthUnavailable, PlatformManager.NativePlatform.PlatformDisplayName);
							break;
						case Result.UnexpectedError:
							UserStatus = EUserStatus.OfflineMode;
							_callback?.Invoke(owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
							break;
						case Result.UnrecognizedResponse:
							UserStatus = EUserStatus.OfflineMode;
							_callback?.Invoke(owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
							break;
						default:
							UserStatus = EUserStatus.TemporaryError;
							_callback?.Invoke(owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
							break;
						}
					}
				}
				else
				{
					Log.Error("[EOS] Login " + (_refreshing ? "refresh " : "") + "error: " + _callbackData.ResultCode);
					UserStatus = EUserStatus.PermanentError;
					_callback?.Invoke(owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
				}
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectCreateUser(ContinuanceToken _continuanceToken, LoginUserCallback _callback)
	{
		EosHelpers.AssertMainThread("Usr.Create");
		Log.Out("[EOS] Creating account");
		CreateUserOptions options = new CreateUserOptions
		{
			ContinuanceToken = _continuanceToken
		};
		lock (AntiCheatCommon.LockObject)
		{
			connectInterface.CreateUser(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref CreateUserCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode == Result.Success)
				{
					Log.Out("[EOS] CreateUser succeeded, PUID: " + _callbackData.LocalUserId);
					SyncExternalAccountInfo(_callbackData.LocalUserId, _callback);
				}
				else if (Common.IsOperationComplete(_callbackData.ResultCode))
				{
					Log.Warning("[EOS] CreateUser failed: " + _callbackData.ResultCode);
					UserStatus = EUserStatus.TemporaryError;
					_callback?.Invoke(owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
				}
				else
				{
					Log.Error("[EOS] CreateUser error: " + _callbackData.ResultCode);
					UserStatus = EUserStatus.PermanentError;
					_callback?.Invoke(owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
				}
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SyncExternalAccountInfo(ProductUserId _puid, LoginUserCallback _callback)
	{
		if (PlatformManager.NativePlatform.PlatformIdentifier == EPlatformIdentifier.XBL)
		{
			Log.Out("[EOS] EnsureAccountInfo required for this platform, starting additional login");
			PlatformManager.NativePlatform.User.GetLoginTicket([PublicizedFrom(EAccessModifier.Internal)] (bool _success, byte[] _byteTicket, string _stringTicket) =>
			{
				Utf8String token = ((_byteTicket != null) ? Common.ToString(new ArraySegment<byte>(_byteTicket)) : new Utf8String(_stringTicket));
				LoginOptions options = new LoginOptions
				{
					Credentials = new Credentials
					{
						Token = token,
						Type = externalCredentialType
					},
					UserLoginInfo = null
				};
				lock (AntiCheatCommon.LockObject)
				{
					connectInterface.Login(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref LoginCallbackInfo _callbackData) =>
					{
						if (_callbackData.ResultCode == Result.Success)
						{
							Log.Out("[EOS] ensure account info succeeded, PUID: " + _callbackData.LocalUserId);
							eosLoggedIn(_callbackData.LocalUserId, _callback);
						}
						else if (Common.IsOperationComplete(_callbackData.ResultCode))
						{
							Log.Warning("[EOS] ensure account info failed: " + _callbackData.ResultCode);
							UserStatus = EUserStatus.TemporaryError;
							_callback?.Invoke(owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
						}
						else
						{
							Log.Error("[EOS] ensure account info error: " + _callbackData.ResultCode);
							UserStatus = EUserStatus.PermanentError;
							_callback?.Invoke(owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
						}
					});
				}
			});
		}
		else
		{
			eosLoggedIn(_puid, _callback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void eosLoggedIn(ProductUserId _puid, LoginUserCallback _callback)
	{
		platformUserId = new UserIdentifierEos(_puid);
		GetNativePlatformUserIdentifier(_callback);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetNativePlatformUserIdentifier(LoginUserCallback _callback)
	{
		Log.Out("[EOS] Getting native user for " + platformUserId.ReadablePlatformUserIdentifier);
		IdToken value = new IdToken
		{
			JsonWebToken = owner.AuthenticationClient.GetAuthTicket(),
			ProductUserId = platformUserId.ProductUserId
		};
		VerifyIdTokenOptions options = new VerifyIdTokenOptions
		{
			IdToken = value
		};
		lock (AntiCheatCommon.LockObject)
		{
			connectInterface.VerifyIdToken(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref VerifyIdTokenCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode != Result.Success)
				{
					Log.Error("[EOS] VerifyIdToken failed: " + _callbackData.ResultCode.ToStringCached());
					UserStatus = EUserStatus.TemporaryError;
					_callback(owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
				}
				else if (!_callbackData.IsAccountInfoPresent)
				{
					Log.Error("[EOS] VerifyIdToken failed: No account info");
					UserStatus = EUserStatus.TemporaryError;
					_callback(owner, EApiStatusReason.Unknown, "NoAccountInfo");
				}
				else
				{
					_ = (string)_callbackData.Platform;
					string text = _callbackData.AccountId;
					_ = (string)_callbackData.DeviceType;
					ExternalAccountType accountIdType = _callbackData.AccountIdType;
					ProductUserId productUserId = _callbackData.ProductUserId;
					if (!EosHelpers.AccountTypeMappings.TryGetValue(accountIdType, out var value2))
					{
						Log.Error("[EOS] VerifyIdToken failed: Unsupported account type: " + accountIdType);
						UserStatus = EUserStatus.TemporaryError;
						_callback(owner, EApiStatusReason.Unknown, "UnsupportedAccountType");
					}
					else
					{
						string text2 = productUserId.ToString();
						if (text2 != platformUserId.ProductUserIdString)
						{
							Log.Error("[EOS] VerifyIdToken failed: PUID mismatch: " + text2);
							UserStatus = EUserStatus.TemporaryError;
							_callback(owner, EApiStatusReason.Unknown, "PUID mismatch");
						}
						else
						{
							PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromPlatformAndId(value2.ToStringCached(), text);
							if (platformUserIdentifierAbs == null)
							{
								Log.Error("[EOS] VerifyIdToken failed: Could not create user identifier from platform/accountid: " + value2.ToStringCached() + "/" + text);
								UserStatus = EUserStatus.TemporaryError;
								_callback(owner, EApiStatusReason.Unknown, "NoUserId");
							}
							else
							{
								NativePlatformUserId = platformUserIdentifierAbs;
								eosLoginDone(_callback);
							}
						}
					}
				}
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void eosLoginDone(LoginUserCallback _callback)
	{
		UserStatus = EUserStatus.LoggedIn;
		userLoggedIn?.Invoke(owner);
		_callback?.Invoke(owner, EApiStatusReason.Ok, null);
		saveUserMapping();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos> loadUserMappings()
	{
		if (!SdPlayerPrefs.HasKey("EosMappings"))
		{
			Log.Warning("[EOS] No platform -> EOS mappings found");
			return null;
		}
		Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos> dictionary = new Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos>();
		string[] array = SdPlayerPrefs.GetString("EosMappings").Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length == 0)
			{
				continue;
			}
			string[] array2 = array[i].Split('=');
			if (array2.Length != 2)
			{
				Log.Warning("[EOS] Malformed user mapping entry: '" + array[i] + "'");
				continue;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromCombinedString(array2[0]);
			if (platformUserIdentifierAbs == null)
			{
				Log.Warning("[EOS] Malformed user identifier entry: '" + array2[0] + "'");
				continue;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromCombinedString(array2[1]);
			if (platformUserIdentifierAbs2 == null)
			{
				Log.Warning("[EOS] Malformed user identifier EOS mapping entry: '" + array2[1] + "'");
				continue;
			}
			if (platformUserIdentifierAbs2.PlatformIdentifier != EPlatformIdentifier.EOS)
			{
				Log.Warning("[EOS] Stored user identifier EOS mapping not an EOS identifier: '" + array2[1] + "'");
				continue;
			}
			if (dictionary.ContainsKey(platformUserIdentifierAbs))
			{
				Log.Warning("[EOS] User identifier found multiple times: " + array2[0]);
			}
			dictionary[platformUserIdentifierAbs] = (UserIdentifierEos)platformUserIdentifierAbs2;
		}
		return dictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveUserMapping()
	{
		Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos> obj = loadUserMappings() ?? new Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos>();
		obj[PlatformManager.NativePlatform.User.PlatformUserId] = platformUserId;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<PlatformUserIdentifierAbs, UserIdentifierEos> item in obj)
		{
			stringBuilder.Append(item.Key.CombinedString + "=" + item.Value.CombinedString + ";");
		}
		SdPlayerPrefs.SetString("EosMappings", stringBuilder.ToString());
		SdPlayerPrefs.Save();
	}
}

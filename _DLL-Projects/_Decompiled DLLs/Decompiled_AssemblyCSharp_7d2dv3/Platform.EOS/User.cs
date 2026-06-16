using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using UnityEngine;

namespace Platform.EOS;

public class User : UserBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string EosMappingsPrefName = "EosMappings";

	[PublicizedFrom(EAccessModifier.Private)]
	public IApplicationStateController nativeApplicationStateController;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExternalCredentialType externalCredentialType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerHasSanctions;

	[PublicizedFrom(EAccessModifier.Private)]
	public string reasonForPermissions;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasSuspended;

	[PublicizedFrom(EAccessModifier.Private)]
	public int resumeCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldRefreshLoginOnResume;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs NativePlatformUserId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override EUserPerms Permissions
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

	public override void Init(IPlatform _owner)
	{
		base.Init(_owner);
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

	public override void Destroy()
	{
		base.Destroy();
		if (nativeApplicationStateController != null)
		{
			nativeApplicationStateController.OnApplicationStateChanged -= OnApplicationStateChanged;
			nativeApplicationStateController = null;
		}
	}

	public override void Login(LoginUserCallback _delegate)
	{
		if (base.UserStatus == EUserStatus.LoggedIn)
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
					startLogin(_delegate);
				}
				else
				{
					base.UserStatus = EUserStatus.OfflineMode;
					_delegate(Owner, EApiStatusReason.Other, "User offline");
				}
			}
			else
			{
				base.UserStatus = EUserStatus.OfflineMode;
				_delegate(Owner, EApiStatusReason.Other, "No connection to EOS backend");
			}
		});
	}

	public override void PlayOffline(LoginUserCallback _delegate)
	{
		base.UserStatus = EUserStatus.NotAttempted;
		Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos> dictionary = loadUserMappings();
		if (dictionary == null)
		{
			_delegate(Owner, EApiStatusReason.NoOnlineStart, null);
			return;
		}
		PlatformUserIdentifierAbs platformUserId = PlatformManager.NativePlatform.User.PlatformUserId;
		if (platformUserId == null)
		{
			Log.Warning("[EOS] No native platform user logged in, can not proceed in offline mode");
			_delegate(Owner, EApiStatusReason.Other, "Not logged in to native platform");
			return;
		}
		if (!dictionary.TryGetValue(platformUserId, out var value))
		{
			Log.Warning("[EOS] No mapping for the logged in user: " + platformUserId.CombinedString);
			_delegate(Owner, EApiStatusReason.NoOnlineStart, null);
			return;
		}
		PlatformUserIdEos = value;
		base.UserStatus = EUserStatus.OfflineMode;
		userLoggedIn?.Invoke(Owner);
		_delegate(Owner, EApiStatusReason.NotLoggedOn, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void startLogin(LoginUserCallback _delegate, bool _refreshing = false)
	{
		fetchTicket(_delegate, _refreshing);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void fetchTicket(LoginUserCallback _delegate, bool _refreshing = false)
	{
		PlatformManager.NativePlatform.User.GetLoginTicket([PublicizedFrom(EAccessModifier.Internal)] (bool _success, byte[] _byteTicket, string _stringTicket) =>
		{
			if (_success)
			{
				connectLogin(_byteTicket, _stringTicket, externalCredentialType, _delegate, _refreshing);
			}
			else
			{
				Log.Error("[EOS] Failed fetching login ticket from native platform");
				base.UserStatus = EUserStatus.TemporaryError;
				_delegate?.Invoke(Owner, EApiStatusReason.NoLoginTicket, null);
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void connectCreateUser(ContinuanceToken _continuanceToken, LoginUserCallback _callback)
	{
		EosHelpers.AssertMainThread("Usr.Create");
		Log.Out("[EOS] Creating account");
		EosHelpers.UserAccountState = EUserAccountState.NewUser;
		CreateUserOptions options = new CreateUserOptions
		{
			ContinuanceToken = _continuanceToken
		};
		lock (AntiCheatCommon.LockObject)
		{
			base.ConnectInterface.CreateUser(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref CreateUserCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode == Result.Success)
				{
					Log.Out("[EOS] CreateUser succeeded, PUID: " + _callbackData.LocalUserId);
					syncExternalAccountInfo(_callbackData.LocalUserId, _callback);
				}
				else if (Common.IsOperationComplete(_callbackData.ResultCode))
				{
					Log.Warning("[EOS] CreateUser failed: " + _callbackData.ResultCode);
					base.UserStatus = EUserStatus.TemporaryError;
					_callback?.Invoke(Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
				}
				else
				{
					Log.Error("[EOS] CreateUser error: " + _callbackData.ResultCode);
					base.UserStatus = EUserStatus.PermanentError;
					_callback?.Invoke(Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
				}
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void syncExternalAccountInfo(ProductUserId _puid, LoginUserCallback _callback)
	{
		if (PlatformManager.NativePlatform.PlatformIdentifier == EPlatformIdentifier.XBL)
		{
			Log.Out("[EOS] EnsureAccountInfo required for this platform, starting additional login");
			PlatformManager.NativePlatform.User.GetLoginTicket([PublicizedFrom(EAccessModifier.Internal)] (bool _, byte[] _byteTicket, string _stringTicket) =>
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
					base.ConnectInterface.Login(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref LoginCallbackInfo _callbackData) =>
					{
						if (_callbackData.ResultCode == Result.Success)
						{
							Log.Out("[EOS] ensure account info succeeded, PUID: " + _callbackData.LocalUserId);
							eosLoggedIn(_callbackData.LocalUserId, _callback);
						}
						else if (Common.IsOperationComplete(_callbackData.ResultCode))
						{
							Log.Warning("[EOS] ensure account info failed: " + _callbackData.ResultCode);
							base.UserStatus = EUserStatus.TemporaryError;
							_callback?.Invoke(Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
						}
						else
						{
							Log.Error("[EOS] ensure account info error: " + _callbackData.ResultCode);
							base.UserStatus = EUserStatus.PermanentError;
							_callback?.Invoke(Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void eosLoggedIn(ProductUserId _puid, LoginUserCallback _callback)
	{
		base.eosLoggedIn(_puid, _callback);
		getNativePlatformUserIdentifier(_callback);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getNativePlatformUserIdentifier(LoginUserCallback _callback)
	{
		Log.Out("[EOS] Getting native user for " + PlatformUserIdEos.ReadablePlatformUserIdentifier);
		IdToken value = new IdToken
		{
			JsonWebToken = Owner.AuthenticationClient.GetAuthTicket(),
			ProductUserId = PlatformUserIdEos.ProductUserId
		};
		VerifyIdTokenOptions options = new VerifyIdTokenOptions
		{
			IdToken = value
		};
		lock (AntiCheatCommon.LockObject)
		{
			base.ConnectInterface.VerifyIdToken(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref VerifyIdTokenCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode != Result.Success)
				{
					Log.Error("[EOS] VerifyIdToken failed: " + _callbackData.ResultCode.ToStringCached());
					base.UserStatus = EUserStatus.TemporaryError;
					_callback(Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached());
				}
				else if (!_callbackData.IsAccountInfoPresent)
				{
					Log.Error("[EOS] VerifyIdToken failed: No account info");
					base.UserStatus = EUserStatus.TemporaryError;
					_callback(Owner, EApiStatusReason.Unknown, "NoAccountInfo");
				}
				else
				{
					string text = _callbackData.AccountId;
					ExternalAccountType accountIdType = _callbackData.AccountIdType;
					ProductUserId productUserId = _callbackData.ProductUserId;
					if (!EosHelpers.AccountTypeMappings.TryGetValue(accountIdType, out var value2))
					{
						Log.Error("[EOS] VerifyIdToken failed: Unsupported account type: " + accountIdType);
						base.UserStatus = EUserStatus.TemporaryError;
						_callback(Owner, EApiStatusReason.Unknown, "UnsupportedAccountType");
					}
					else
					{
						string text2 = productUserId.ToString();
						if (text2 != PlatformUserIdEos.ProductUserIdString)
						{
							Log.Error("[EOS] VerifyIdToken failed: PUID mismatch: " + text2);
							base.UserStatus = EUserStatus.TemporaryError;
							_callback(Owner, EApiStatusReason.Unknown, "PUID mismatch");
						}
						else
						{
							PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromPlatformAndId(value2.ToStringCached(), text);
							if (platformUserIdentifierAbs == null)
							{
								Log.Error("[EOS] VerifyIdToken failed: Could not create user identifier from platform/accountid: " + value2.ToStringCached() + "/" + text);
								base.UserStatus = EUserStatus.TemporaryError;
								_callback(Owner, EApiStatusReason.Unknown, "NoUserId");
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void eosLoginDone(LoginUserCallback _callback)
	{
		base.eosLoginDone(_callback);
		saveUserMapping();
	}

	public override string GetPermissionDenyReason(EUserPerms _perms)
	{
		EUserPerms eUserPerms = ~Permissions & _perms;
		if (eUserPerms.HasFlag(EUserPerms.HostMultiplayer))
		{
			return reasonForPermissions;
		}
		return null;
	}

	public override IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
	{
		Log.Out(string.Format("[EOS] {0}({1}: [{2}], {3}: {4})", "ResolvePermissions", "_perms", _perms, "_canPrompt", _canPrompt));
		if (base.UserStatus != EUserStatus.LoggedIn)
		{
			yield break;
		}
		if (((Api)Owner.Api).SanctionsInterface == null || ((Api)Owner.Api).eosSanctionsCheck == null)
		{
			Log.Out($"[EOS] ResolvePermissions not possible: eosSanctionsCheck: {((Api)Owner.Api).eosSanctionsCheck != null}, SanctionsInterface: {((Api)Owner.Api).SanctionsInterface != null}");
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
			EosHelpers.TestEosConnection([PublicizedFrom(EAccessModifier.Internal)] (bool _isConnected) =>
			{
				connectionTestComplete = true;
				connectionTestSuccess = _isConnected;
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
			yield return ((Api)Owner.Api).eosSanctionsCheck.CheckSanctionsEnumerator((Owner.Api as Api).SanctionsInterface, PlatformUserIdEos.ProductUserId, PlatformUserIdEos.ProductUserId, [PublicizedFrom(EAccessModifier.Private)] (SanctionsCheckResult _checkResult) =>
			{
				if (_checkResult.Success)
				{
					Log.Out($"[EOS] CheckSanctionsEnumerator: hasSanctions {_checkResult.HasActiveSanctions}");
					if (_checkResult.HasActiveSanctions)
					{
						playerHasSanctions = true;
						reasonForPermissions = _checkResult.ReasonForSanction;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationStateChanged(ApplicationState _newState)
	{
		bool flag = _newState == ApplicationState.Suspended;
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
		shouldRefreshLoginOnResume = shouldRefreshLoginOnResume || base.UserStatus == EUserStatus.LoggedIn;
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
		yield return refreshLoginCoroutine();
		if (ShouldExitCoroutine(_attemptedLogin: true))
		{
			yield break;
		}
		if (nativeApplicationStateController != null)
		{
			Log.Out("[EOS] Waiting for network to be ready...");
			do
			{
				yield return new WaitForSecondsRealtime(0.25f);
				if (ShouldExitCoroutine(_attemptedLogin: false))
				{
					yield break;
				}
			}
			while (!nativeApplicationStateController.NetworkConnectionState);
			Log.Out("[EOS] Network is ready. Trying to refresh login...");
			yield return refreshLoginCoroutine();
			if (ShouldExitCoroutine(_attemptedLogin: true))
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
			if (ShouldExitCoroutine(_attemptedLogin: false))
			{
				yield break;
			}
			Log.Out("[EOS] Testing connecting to EOS...");
			bool eosTestComplete = false;
			EosHelpers.TestEosConnection([PublicizedFrom(EAccessModifier.Internal)] (bool _success) =>
			{
				eosReachable = _success;
				eosTestComplete = true;
			});
			while (!eosTestComplete)
			{
				yield return new WaitForSecondsRealtime(0.25f);
				if (ShouldExitCoroutine(_attemptedLogin: false))
				{
					yield break;
				}
			}
		}
		Log.Out("[EOS] EOS is reachable so we can try refresh the login now.");
		yield return refreshLoginCoroutine();
		if (base.UserStatus == EUserStatus.LoggedIn)
		{
			Log.Warning("[EOS] Refresh login on resume has failed. User will have to trigger a login through other means.");
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool ShouldExitCoroutine(bool _attemptedLogin)
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
			if (base.UserStatus == EUserStatus.LoggedIn)
			{
				shouldRefreshLoginOnResume = false;
				if (!_attemptedLogin)
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
	public IEnumerator refreshLoginCoroutine()
	{
		bool done = false;
		Log.Out("[EOS] Refreshing Login");
		fetchTicket([PublicizedFrom(EAccessModifier.Internal)] (IPlatform _, EApiStatusReason _, string _) =>
		{
			done = true;
		}, _refreshing: true);
		while (!done)
		{
			yield return new WaitForSecondsRealtime(0.25f);
		}
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
		obj[PlatformManager.NativePlatform.User.PlatformUserId] = PlatformUserIdEos;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<PlatformUserIdentifierAbs, UserIdentifierEos> item in obj)
		{
			stringBuilder.Append(item.Key.CombinedString + "=" + item.Value.CombinedString + ";");
		}
		SdPlayerPrefs.SetString("EosMappings", stringBuilder.ToString());
		SdPlayerPrefs.Save();
	}
}

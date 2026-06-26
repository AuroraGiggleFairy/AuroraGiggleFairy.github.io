using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Sanctions;

namespace Platform.EOS;

public class AuthServer : IAuthenticationServer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthenticationSuccessfulCallbackDelegate authSuccessfulDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public KickPlayerDelegate kickPlayerDelegate;

	public ConnectInterface connectInterface
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return ((Api)owner.Api).ConnectInterface;
		}
	}

	public SanctionsInterface sanctionsInterface
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return ((Api)owner.Api).SanctionsInterface;
		}
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
	}

	public EBeginUserAuthenticationResult AuthenticateUser(ClientInfo _cInfo)
	{
		UserIdentifierEos identifierEos = (UserIdentifierEos)_cInfo.CrossplatformId;
		Log.Out("[EOS] Verifying token for " + identifierEos.ProductUserIdString);
		EosHelpers.AssertMainThread("ASe.Auth");
		IdToken value = new IdToken
		{
			JsonWebToken = identifierEos.Ticket,
			ProductUserId = identifierEos.ProductUserId
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
					KickPlayerDelegate obj = kickPlayerDelegate;
					ClientInfo cInfo = _cInfo;
					string customReason = _callbackData.ResultCode.ToStringCached();
					obj(cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 50, default(DateTime), customReason));
				}
				else if (!_callbackData.IsAccountInfoPresent)
				{
					Log.Error("[EOS] VerifyIdToken failed: No account info");
					KickPlayerDelegate obj2 = kickPlayerDelegate;
					ClientInfo cInfo2 = _cInfo;
					string customReason = _callbackData.ResultCode.ToStringCached();
					obj2(cInfo2, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 50, default(DateTime), customReason));
				}
				else
				{
					string text = _callbackData.Platform;
					string text2 = _callbackData.AccountId;
					string text3 = _callbackData.DeviceType;
					ExternalAccountType accountIdType = _callbackData.AccountIdType;
					ProductUserId productUserId = _callbackData.ProductUserId;
					if (!EosHelpers.AccountTypeMappings.TryGetValue(accountIdType, out var value2))
					{
						KickPlayerDelegate obj3 = kickPlayerDelegate;
						ClientInfo cInfo3 = _cInfo;
						string customReason = "UnsupportedAccountType " + accountIdType.ToStringCached();
						obj3(cInfo3, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 50, default(DateTime), customReason));
					}
					else if (value2 != _cInfo.PlatformId.PlatformIdentifier)
					{
						KickPlayerDelegate obj4 = kickPlayerDelegate;
						ClientInfo cInfo4 = _cInfo;
						string customReason = "PlatformIdentifierMismatch (" + value2.ToStringCached() + " vs " + _cInfo.PlatformId.PlatformIdentifier.ToStringCached() + ")";
						obj4(cInfo4, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 50, default(DateTime), customReason));
					}
					else if (text2 != _cInfo.PlatformId.ReadablePlatformUserIdentifier)
					{
						KickPlayerDelegate obj5 = kickPlayerDelegate;
						ClientInfo cInfo5 = _cInfo;
						string customReason = "AccountIdMismatch (" + text2 + ")";
						obj5(cInfo5, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 50, default(DateTime), customReason));
					}
					else
					{
						string text4 = productUserId.ToString();
						if (text4 != identifierEos.ProductUserIdString)
						{
							KickPlayerDelegate obj6 = kickPlayerDelegate;
							ClientInfo cInfo6 = _cInfo;
							string customReason = "PuidMismatch (" + text4 + ")";
							obj6(cInfo6, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 50, default(DateTime), customReason));
						}
						else
						{
							Log.Out($"[EOS] Device={text3}, Platform={text}, AccType={accountIdType}, AccId={text2}, PUID={productUserId}");
							_cInfo.device = EosHelpers.GetDeviceTypeFromPlatform(text);
							_cInfo.requiresAntiCheat = _cInfo.device.RequiresAntiCheat();
							EPlayGroup ePlayGroup = _cInfo.device.ToPlayGroup();
							if (ePlayGroup != EPlayGroupExtensions.Current && !SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.AllowsCrossplay)
							{
								KickPlayerDelegate obj7 = kickPlayerDelegate;
								ClientInfo cInfo7 = _cInfo;
								string customReason = $"NoCrossplay {EPlayGroupExtensions.Current} <-> {ePlayGroup}";
								obj7(cInfo7, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 50, default(DateTime), customReason));
							}
							else
							{
								ProductUserId localUser = null;
								if (!GameManager.IsDedicatedServer)
								{
									localUser = (owner.User.PlatformUserId as UserIdentifierEos)?.ProductUserId;
								}
								if (GameManager.IsDedicatedServer && GamePrefs.GetBool(EnumGamePrefs.IgnoreEOSSanctions))
								{
									authSuccessfulDelegate(_cInfo);
									((SessionsHost)owner.ServerListAnnouncer).RegisterUser(_cInfo);
								}
								else
								{
									((Api)owner.Api).eosSanctionsCheck.CheckSanctions(sanctionsInterface, identifierEos.ProductUserId, localUser, [PublicizedFrom(EAccessModifier.Internal)] (SanctionsCheckResult result) =>
									{
										if (!result.Success || result.HasActiveSanctions)
										{
											kickPlayerDelegate(_cInfo, result.KickReason);
										}
										else
										{
											authSuccessfulDelegate(_cInfo);
											((SessionsHost)owner.ServerListAnnouncer).RegisterUser(_cInfo);
										}
									});
								}
							}
						}
					}
				}
			});
		}
		return EBeginUserAuthenticationResult.Ok;
	}

	public void RemoveUser(ClientInfo _cInfo)
	{
		((SessionsHost)owner.ServerListAnnouncer).UnregisterUser(_cInfo);
	}

	public void StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate)
	{
		authSuccessfulDelegate = _authSuccessfulDelegate;
		kickPlayerDelegate = _kickPlayerDelegate;
	}

	public void StartServerSteamGroups(SteamGroupStatusResponse _groupStatusResponseDelegate)
	{
		throw new NotImplementedException();
	}

	public void StopServer()
	{
		authSuccessfulDelegate = null;
		kickPlayerDelegate = null;
	}

	public bool RequestUserInGroupStatus(ClientInfo _cInfo, string _steamIdGroup)
	{
		throw new NotImplementedException();
	}
}

using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace Platform.EOS;

public class AuthClient : IAuthenticationClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	public ConnectInterface connectInterface
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return ((Api)owner.Api).ConnectInterface;
		}
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
	}

	public string GetAuthTicket()
	{
		EosHelpers.AssertMainThread("ACl.Get");
		CopyIdTokenOptions options = new CopyIdTokenOptions
		{
			LocalUserId = ((UserIdentifierEos)owner.User.PlatformUserId).ProductUserId
		};
		Result result;
		IdToken? outIdToken;
		lock (AntiCheatCommon.LockObject)
		{
			result = connectInterface.CopyIdToken(ref options, out outIdToken);
		}
		Log.Out($"[EOS] CopyIdToken result: {result}");
		return outIdToken?.JsonWebToken;
	}

	public void AuthenticateServer(ClientAuthenticateServerContext _context)
	{
		EosHelpers.AssertMainThread("ACl.Auth");
		if (PermissionsManager.IsCrossplayAllowed())
		{
			_context.Success();
			return;
		}
		if (_context.GameServerInfo.AllowsCrossplay)
		{
			Log.Error("[EOS] [ACl.Auth] Cannot join server that has crossplay when we do not have crossplay permissions.");
			_context.DisconnectNoCrossplay();
			return;
		}
		if (EPlayGroupExtensions.Current == EPlayGroup.Standalone && (_context.GameServerInfo.PlayGroup == EPlayGroup.Standalone || _context.GameServerInfo.IsDedicated))
		{
			_context.Success();
			return;
		}
		PlatformUserIdentifierAbs crossplatformUserId = _context.CrossplatformUserId;
		UserIdentifierEos identifierEos = crossplatformUserId as UserIdentifierEos;
		if (identifierEos == null)
		{
			Log.Warning($"[EOS] [ACl.Auth] Expected EOS Crossplatform ID? But got: {_context.CrossplatformUserId}");
			_context.DisconnectNoCrossplay();
			return;
		}
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
			connectInterface.VerifyIdToken(ref options, null, VerifyIdTokenCallback);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void VerifyIdTokenCallback(ref VerifyIdTokenCallbackInfo _callbackData)
		{
			if (_callbackData.ResultCode != Result.Success)
			{
				Log.Error("[EOS] [ACl.Auth] VerifyIdToken failed: " + _callbackData.ResultCode.ToStringCached());
				_context.DisconnectNoCrossplay();
			}
			else if (!_callbackData.IsAccountInfoPresent)
			{
				Log.Error("[EOS] [ACl.Auth] VerifyIdToken failed: No account info");
				_context.DisconnectNoCrossplay();
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
					Log.Error("[EOS] [ACl.Auth] Unsupported Account Type: " + accountIdType.ToStringCached());
					_context.DisconnectNoCrossplay();
				}
				else if (value2 != _context.PlatformUserId.PlatformIdentifier)
				{
					Log.Error($"[EOS] [ACl.Auth] Platform Identifier Mismatch. Expected: {value2} Got: {_context.PlatformUserId.PlatformIdentifier}");
					_context.DisconnectNoCrossplay();
				}
				else if (text2 != _context.PlatformUserId.ReadablePlatformUserIdentifier)
				{
					Log.Error("[EOS] [ACl.Auth] Account Id Mismatch. Expected: " + text2 + " Got: " + _context.PlatformUserId.ReadablePlatformUserIdentifier);
					_context.DisconnectNoCrossplay();
				}
				else
				{
					string text4 = productUserId.ToString();
					if (text4 != identifierEos.ProductUserIdString)
					{
						Log.Error("[EOS] [ACl.Auth] PUID Mismatch. Expected: " + text4 + " Got: " + identifierEos.ProductUserIdString);
						_context.DisconnectNoCrossplay();
					}
					else
					{
						Log.Out($"[EOS] [ACl.Auth] Device={text3}, Platform={text}, AccType={accountIdType}, AccId={text2}, PUID={productUserId}");
						EPlayGroup ePlayGroup = EosHelpers.GetDeviceTypeFromPlatform(text).ToPlayGroup();
						if (ePlayGroup != _context.GameServerInfo.PlayGroup)
						{
							Log.Error($"[EOS] [ACl.Auth] Play Group Mismatch. Expected: {ePlayGroup} Got: {_context.GameServerInfo.PlayGroup}.");
							_context.DisconnectNoCrossplay();
						}
						if (ePlayGroup != EPlayGroupExtensions.Current && !PermissionsManager.IsCrossplayAllowed())
						{
							Log.Error($"[EOS] [ACl.Auth] No Crossplay Between {EPlayGroupExtensions.Current} and {ePlayGroup}.");
							_context.DisconnectNoCrossplay(ePlayGroup);
						}
						else
						{
							_context.Success();
						}
					}
				}
			}
		}
	}

	public void Destroy()
	{
	}
}

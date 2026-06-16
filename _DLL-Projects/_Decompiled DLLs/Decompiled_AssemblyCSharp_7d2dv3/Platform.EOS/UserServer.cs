using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace Platform.EOS;

public class UserServer : UserBase
{
	public override EUserPerms Permissions => EUserPerms.All;

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
				startLogin(_delegate);
			}
			else
			{
				base.UserStatus = EUserStatus.OfflineMode;
				_delegate(Owner, EApiStatusReason.Other, "No connection to EOS backend");
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void startLogin(LoginUserCallback _delegate, bool _refreshing = false)
	{
		fetchDeviceId(_delegate, _refreshing);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void fetchDeviceId(LoginUserCallback _delegate, bool _refreshing)
	{
		Log.Warning($"[EOS] [DEVICEID] Fetching device id from native platform, with delegate={_delegate != null}, refreshing={_refreshing}");
		CreateDeviceIdOptions options = new CreateDeviceIdOptions
		{
			DeviceModel = "DedicatedServer"
		};
		base.ConnectInterface.CreateDeviceId(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref CreateDeviceIdCallbackInfo _result) =>
		{
			if (_result.ResultCode == Result.Success || _result.ResultCode == Result.DuplicateNotAllowed)
			{
				connectLogin(null, null, ExternalCredentialType.DeviceidAccessToken, _delegate, _refreshing, "DedicatedServer");
			}
			else
			{
				Log.Error("[EOS] [DEVICEID] Failed creating DeviceId: " + _result.ResultCode.ToStringCached());
				base.UserStatus = EUserStatus.TemporaryError;
				_delegate?.Invoke(Owner, EApiStatusReason.NoLoginTicket, null);
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void connectCreateUser(ContinuanceToken _continuanceToken, LoginUserCallback _callback)
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void eosLoggedIn(ProductUserId _puid, LoginUserCallback _callback)
	{
		base.eosLoggedIn(_puid, _callback);
		eosLoginDone(_callback);
	}
}

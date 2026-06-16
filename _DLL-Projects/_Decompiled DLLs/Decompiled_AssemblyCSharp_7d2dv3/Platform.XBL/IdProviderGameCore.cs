using Platform.EOS;
using Platform.Shared;

namespace Platform.XBL;

public class IdProviderGameCore
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly User m_nativeUser;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public UserIdentifierXbl Id
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public IdProviderGameCore(User _nativeUser, IUserClient _crossplatformUser)
	{
		m_nativeUser = _nativeUser;
		if (_crossplatformUser != null)
		{
			_crossplatformUser.UserLoggedIn += CrossplatformLogin;
		}
		else
		{
			m_nativeUser.UserLoggedIn += NativeLogin;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NativeLogin(IPlatform _platform)
	{
		Id = new UserIdentifierXbl(m_nativeUser.LocalID.Value.ToString());
		Log.Out("[XBL] Initializing user id with Local ID");
		XblXuidMapper.SetXuid(Id, m_nativeUser.Xuid);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CrossplatformLogin(IPlatform _platform)
	{
		PlatformUserIdentifierAbs nativePlatformUserId = ((Platform.EOS.User)_platform.User).NativePlatformUserId;
		if (nativePlatformUserId != null)
		{
			if (!(nativePlatformUserId is UserIdentifierXbl userIdentifierXbl))
			{
				Log.Error($"[XBL] Got different native platform id from EOS: {nativePlatformUserId.PlatformIdentifier}");
				return;
			}
			Id = userIdentifierXbl;
			Log.Out("[XBL] Initializing user id with PXUID " + userIdentifierXbl.CombinedString);
			PlatformIdCache.SetCachedId(Id);
			XblXuidMapper.SetXuid(Id, m_nativeUser.Xuid);
		}
	}

	public bool LoadOfflineId()
	{
		if (PlatformIdCache.TryGetCachedId<UserIdentifierXbl>(out var _platformUserIdentifier))
		{
			Id = _platformUserIdentifier;
			Log.Out("[XBL] Retrieved offline user id: " + _platformUserIdentifier.CombinedString);
			return true;
		}
		return false;
	}
}

namespace Platform;

public class UserDataRoamingMultiPlatform : UserDataRoamingAbs
{
	public override SaveRoamingMode SaveRoamingMode
	{
		get
		{
			if (GameManager.IsDedicatedServer)
			{
				return SaveRoamingMode.None;
			}
			return PlatformManager.NativePlatform.UserDataRoaming?.SaveRoamingMode ?? SaveRoamingMode.None;
		}
	}

	public override UserDataStorageType DefaultSaveStorage
	{
		get
		{
			if (GameManager.IsDedicatedServer)
			{
				return UserDataStorageType.DeviceLocal;
			}
			return PlatformManager.NativePlatform.UserDataRoaming?.DefaultSaveStorage ?? UserDataStorageType.DeviceLocal;
		}
	}
}

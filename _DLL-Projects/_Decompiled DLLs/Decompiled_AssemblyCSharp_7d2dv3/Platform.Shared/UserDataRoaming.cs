namespace Platform.Shared;

public class UserDataRoaming : UserDataRoamingAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SaveRoamingMode mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserDataStorageType storageType;

	public override SaveRoamingMode SaveRoamingMode => mode;

	public override UserDataStorageType DefaultSaveStorage => storageType;

	public static UserDataRoaming OptionalSaveRoaming => new UserDataRoaming(SaveRoamingMode.Optional, UserDataStorageType.Roaming);

	public static UserDataRoaming ForcedSaveRoaming => new UserDataRoaming(SaveRoamingMode.Forced, UserDataStorageType.Roaming);

	public static UserDataRoaming NoSaveRoaming => new UserDataRoaming(SaveRoamingMode.None, UserDataStorageType.DeviceLocal);

	[PublicizedFrom(EAccessModifier.Private)]
	public UserDataRoaming(SaveRoamingMode mode, UserDataStorageType storageType)
	{
		this.mode = mode;
		this.storageType = storageType;
	}
}

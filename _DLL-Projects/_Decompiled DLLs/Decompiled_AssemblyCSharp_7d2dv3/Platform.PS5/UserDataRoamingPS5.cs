namespace Platform.PS5;

public class UserDataRoamingPS5 : UserDataRoamingAbs
{
	public override SaveRoamingMode SaveRoamingMode => SaveRoamingMode.Forced;

	public override UserDataStorageType DefaultSaveStorage => UserDataStorageType.Roaming;
}

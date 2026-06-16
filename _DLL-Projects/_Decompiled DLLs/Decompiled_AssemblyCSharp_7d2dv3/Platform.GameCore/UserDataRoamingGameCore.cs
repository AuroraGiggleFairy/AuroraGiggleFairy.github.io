namespace Platform.GameCore;

public class UserDataRoamingGameCore : UserDataRoamingAbs
{
	public override SaveRoamingMode SaveRoamingMode => SaveRoamingMode.Forced;

	public override UserDataStorageType DefaultSaveStorage => UserDataStorageType.Roaming;
}

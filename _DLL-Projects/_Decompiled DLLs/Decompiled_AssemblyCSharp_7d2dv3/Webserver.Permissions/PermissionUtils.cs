namespace Webserver.Permissions;

public static class PermissionUtils
{
	public static bool CanViewAllPlayers(int _permissionLevel)
	{
		return AdminWebModules.Instance.ModuleAllowedWithLevel("webapi.viewallplayers", _permissionLevel);
	}

	public static bool CanViewAllClaims(int _permissionLevel)
	{
		return AdminWebModules.Instance.ModuleAllowedWithLevel("webapi.viewallclaims", _permissionLevel);
	}
}

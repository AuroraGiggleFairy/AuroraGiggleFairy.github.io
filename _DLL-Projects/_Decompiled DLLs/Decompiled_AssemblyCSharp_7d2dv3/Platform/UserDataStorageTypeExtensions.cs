namespace Platform;

public static class UserDataStorageTypeExtensions
{
	public static bool UsesDataLimit(this UserDataStorageType storage)
	{
		return storage == UserDataStorageType.Roaming;
	}

	public static string LocalizedName(this UserDataStorageType storage)
	{
		return Localization.Get("xuiStorageTypeOptions" + storage.ToStringCached());
	}
}

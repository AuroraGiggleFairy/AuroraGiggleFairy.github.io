namespace Platform;

public abstract class UserDataRoamingAbs : IUserDataRoaming
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public IPlatform platform;

	public bool IsSupported => platform.SaveGameProvider != null;

	public abstract SaveRoamingMode SaveRoamingMode { get; }

	public bool SaveRoamingEnabled
	{
		get
		{
			if (IsSupported)
			{
				if (SaveRoamingMode != SaveRoamingMode.Optional)
				{
					return SaveRoamingMode == SaveRoamingMode.Forced;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsRoamingOptional => SaveRoamingMode == SaveRoamingMode.Optional;

	public abstract UserDataStorageType DefaultSaveStorage { get; }

	public void Init(IPlatform platform)
	{
		this.platform = platform;
	}

	public void ValidateRoamingMode()
	{
		ValidateStoragePref(EnumGamePrefs.GameSaveStorageType);
		ValidateStoragePref(EnumGamePrefs.UserWorldStorageType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateStoragePref(EnumGamePrefs pref)
	{
		UserDataStorageType userDataStorageType = (UserDataStorageType)GamePrefs.GetInt(pref);
		UserDataStorageType userDataStorageType2 = userDataStorageType;
		switch (SaveRoamingMode)
		{
		case SaveRoamingMode.None:
			userDataStorageType2 = UserDataStorageType.DeviceLocal;
			break;
		case SaveRoamingMode.Forced:
			userDataStorageType2 = UserDataStorageType.Roaming;
			break;
		}
		if (userDataStorageType != userDataStorageType2)
		{
			Log.Out($"UserDataRoaming invalid storage pref {userDataStorageType} configured for {pref.ToStringCached()}. Platform roaming mode is {SaveRoamingMode}. Changing to {userDataStorageType2}");
			GamePrefs.Set(pref, (int)userDataStorageType2);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UserDataRoamingAbs()
	{
	}
}

public static class SdPlayerPrefs
{
	public static bool CanLoad
	{
		[PublicizedFrom(EAccessModifier.Internal)]
		get
		{
			return SaveDataUtils.SaveDataPrefs.CanLoad;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static SdPlayerPrefs()
	{
	}

	public static float GetFloat(string key)
	{
		return SaveDataUtils.SaveDataPrefs.GetFloat(key);
	}

	public static float GetFloat(string key, float defaultValue)
	{
		return SaveDataUtils.SaveDataPrefs.GetFloat(key, defaultValue);
	}

	public static void SetFloat(string key, float value)
	{
		SaveDataUtils.SaveDataPrefs.SetFloat(key, value);
	}

	public static int GetInt(string key)
	{
		return SaveDataUtils.SaveDataPrefs.GetInt(key);
	}

	public static int GetInt(string key, int defaultValue)
	{
		return SaveDataUtils.SaveDataPrefs.GetInt(key, defaultValue);
	}

	public static void SetInt(string key, int value)
	{
		SaveDataUtils.SaveDataPrefs.SetInt(key, value);
	}

	public static string GetString(string key)
	{
		return SaveDataUtils.SaveDataPrefs.GetString(key);
	}

	public static string GetString(string key, string defaultValue)
	{
		return SaveDataUtils.SaveDataPrefs.GetString(key, defaultValue);
	}

	public static void SetString(string key, string value)
	{
		SaveDataUtils.SaveDataPrefs.SetString(key, value);
	}

	public static bool HasKey(string key)
	{
		return SaveDataUtils.SaveDataPrefs.HasKey(key);
	}

	public static void DeleteKey(string key)
	{
		SaveDataUtils.SaveDataPrefs.DeleteKey(key);
	}

	public static void DeleteAll()
	{
		SaveDataUtils.SaveDataPrefs.DeleteAll();
	}

	public static void Save()
	{
		SaveDataUtils.SaveDataPrefs.Save();
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void Load()
	{
		SaveDataUtils.SaveDataPrefs.Load();
	}
}

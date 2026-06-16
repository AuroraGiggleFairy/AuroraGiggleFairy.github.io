public static class SaveDataLimit
{
	public const int SAVE_DATA_LIMIT_DISABLED = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SAVE_DATA_LIMIT_PREF_BYTES_PER = 1048576;

	public static void SetLimitToPref(long limit)
	{
		GamePrefs.Set(EnumGamePrefs.SaveDataLimit, ToPrefValue(limit));
	}

	public static long GetLimitFromPref()
	{
		return FromPrefValue(GamePrefs.GetInt(EnumGamePrefs.SaveDataLimit));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int ToPrefValue(long limit)
	{
		if (limit >= 0)
		{
			return (int)((limit + 1048576 - 1) / 1048576);
		}
		if (PlatformOptimizations.LimitedSaveData)
		{
			Log.Warning(string.Format("[{0}] Expected finite save data limit for {1}, but was: {2}", "SaveDataLimit", "ToPrefValue", limit));
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static long FromPrefValue(int limit)
	{
		if (limit >= 0)
		{
			return limit * 1048576;
		}
		if (PlatformOptimizations.LimitedSaveData)
		{
			Log.Warning(string.Format("[{0}] Expected finite save data limit for {1}, but was: {2}", "SaveDataLimit", "FromPrefValue", limit));
		}
		return -1L;
	}
}

using System;

public sealed class SaveDataPrefsUninitialized : ISaveDataPrefs
{
	public static readonly SaveDataPrefsUninitialized INSTANCE = new SaveDataPrefsUninitialized();

	[PublicizedFrom(EAccessModifier.Private)]
	public const string ERROR_MESSAGE = "SdPlayerPrefs is being accessed while the pref instance is not initialized.";

	public bool CanLoad
	{
		get
		{
			throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataPrefsUninitialized()
	{
	}

	public float GetFloat(string key, float defaultValue)
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}

	public void SetFloat(string key, float value)
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}

	public int GetInt(string key, int defaultValue)
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}

	public void SetInt(string key, int value)
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}

	public string GetString(string key, string defaultValue)
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}

	public void SetString(string key, string value)
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}

	public bool HasKey(string key)
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}

	public void DeleteKey(string key)
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}

	public void DeleteAll()
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}

	public void Save()
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}

	public void Load()
	{
		throw new NotSupportedException("SdPlayerPrefs is being accessed while the pref instance is not initialized.");
	}
}

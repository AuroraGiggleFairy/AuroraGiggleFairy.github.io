using System;
using System.Collections.Generic;
using System.IO;

public class PrefVersionStore
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string ext = ".cfg";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string saveDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, List<(string, PrefType)>> prefs = new Dictionary<string, List<(string, PrefType)>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, ISaveDataPrefs> idToFile = new Dictionary<string, ISaveDataPrefs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, ISaveDataPrefs> prefNameToFile = new Dictionary<string, ISaveDataPrefs>();

	public PrefVersionStore(string saveDir)
	{
		this.saveDir = saveDir;
	}

	public void AddPref(string id, string prefName, PrefType type)
	{
		if (prefNameToFile.ContainsKey(prefName))
		{
			throw new ArgumentException(prefName + " already exists in another group");
		}
		if (!idToFile.TryGetValue(id, out var value))
		{
			value = new SaveDataPrefsFile(Path.Combine(saveDir, id) + ".cfg");
			idToFile.Add(id, value);
		}
		if (!prefs.TryGetValue(id, out var value2))
		{
			value2 = new List<(string, PrefType)>();
			prefs.Add(id, value2);
		}
		value2.Add((prefName, type));
		prefNameToFile[prefName] = value;
	}

	public void Save()
	{
		foreach (var (text2, list2) in prefs)
		{
			if (!idToFile.TryGetValue(text2, out var value))
			{
				Log.Error("Expected to have pref file for " + text2);
				continue;
			}
			foreach (var item in list2)
			{
				switch (item.Item2)
				{
				case PrefType.Float:
					value.SetFloat(item.Item1, SdPlayerPrefs.GetFloat(item.Item1));
					break;
				case PrefType.Int:
					value.SetInt(item.Item1, SdPlayerPrefs.GetInt(item.Item1));
					break;
				case PrefType.String:
					value.SetString(item.Item1, SdPlayerPrefs.GetString(item.Item1));
					break;
				}
			}
			value.Save();
		}
	}

	public void Apply()
	{
		foreach (var (text2, list2) in prefs)
		{
			if (!idToFile.TryGetValue(text2, out var value))
			{
				Log.Error("Expected to have pref file for " + text2);
				continue;
			}
			foreach (var item in list2)
			{
				if (value.HasKey(item.Item1))
				{
					switch (item.Item2)
					{
					case PrefType.Float:
						SdPlayerPrefs.SetFloat(item.Item1, value.GetFloat(item.Item1));
						break;
					case PrefType.Int:
						SdPlayerPrefs.SetInt(item.Item1, value.GetInt(item.Item1));
						break;
					case PrefType.String:
						SdPlayerPrefs.SetString(item.Item1, value.GetString(item.Item1));
						break;
					}
				}
			}
		}
	}

	public bool HasKey(string key)
	{
		return prefNameToFile.ContainsKey(key);
	}

	public bool TryGetFloat(string key, out float value)
	{
		if (prefNameToFile.TryGetValue(key, out var value2) && value2.HasKey(key))
		{
			value = value2.GetFloat(key);
			return true;
		}
		value = 0f;
		return false;
	}

	public bool TryGetInt(string key, out int value)
	{
		if (prefNameToFile.TryGetValue(key, out var value2) && value2.HasKey(key))
		{
			value = value2.GetInt(key);
			return true;
		}
		value = 0;
		return false;
	}

	public bool TryGetString(string key, out string value)
	{
		if (prefNameToFile.TryGetValue(key, out var value2) && value2.HasKey(key))
		{
			value = value2.GetString(key);
			return true;
		}
		value = null;
		return false;
	}

	public bool TryGetGamePref(EnumGamePrefs enumGamePref, out object value)
	{
		GamePrefs.EnumType? prefType = GamePrefs.GetPrefType(enumGamePref);
		if (!prefType.HasValue)
		{
			Log.Error($"Unknown enum type for {enumGamePref}");
			value = null;
			return false;
		}
		string key = enumGamePref.ToStringCached();
		switch (prefType.Value)
		{
		case GamePrefs.EnumType.Float:
		{
			if (TryGetFloat(key, out var value3))
			{
				value = value3;
				return true;
			}
			break;
		}
		case GamePrefs.EnumType.Int:
		{
			if (TryGetInt(key, out var value5))
			{
				value = value5;
				return true;
			}
			break;
		}
		case GamePrefs.EnumType.String:
		{
			if (TryGetString(key, out var value6))
			{
				value = value6;
				return true;
			}
			break;
		}
		case GamePrefs.EnumType.Binary:
		{
			if (TryGetString(key, out var value4))
			{
				value = Utils.FromBase64(value4);
				return true;
			}
			break;
		}
		case GamePrefs.EnumType.Bool:
		{
			if (TryGetInt(key, out var value2))
			{
				value = value2 != 0;
				return true;
			}
			break;
		}
		}
		value = null;
		return false;
	}
}

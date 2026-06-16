using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Noemax.GZip;
using Platform;
using UnityEngine;

public static class Localization
{
	public const string DefaultLanguage = "english";

	public const string HeaderKey = "KEY";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string KeepLoadedColumnName = "KeepLoaded";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string UsedInMainMenuColumnName = "UsedInMainMenu";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] ignoredMetaColumnNames = new string[4] { "File", "Type", "NoTranslate", "Context / Alternate Text" };

	public const string MainLocalizationFilename = "Localization.csv";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> allLanguages = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, int> languageToColumnIndex = new CaseInsensitiveStringDictionary<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string[]> mDictionary;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string[]> mDictionaryCaseInsensitive;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, bool[]> patchedCells = new Dictionary<string, bool[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static int defaultLanguageIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int currentLanguageIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string platformLanguage;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex languageHeaderMatcher = new Regex("^[A-Za-z]+$");

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool LocalizationChecks
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static Dictionary<string, string[]> Dictionary
	{
		get
		{
			checkLoaded();
			return mDictionary;
		}
	}

	public static string[] KnownLanguages
	{
		get
		{
			checkLoaded(_throwExc: true);
			return allLanguages.ToArray();
		}
	}

	public static string ActiveLanguage
	{
		get
		{
			checkLoaded(_throwExc: true);
			if (currentLanguageIndex >= 0)
			{
				return allLanguages[currentLanguageIndex];
			}
			if (allLanguages.ContainsCaseInsensitive("english"))
			{
				return "english";
			}
			return "";
		}
		set
		{
			Debug.Log("Language selected: " + value);
		}
	}

	public static string RequestedLanguage
	{
		get
		{
			string launchArgument = GameUtils.GetLaunchArgument("language");
			if (!string.IsNullOrEmpty(launchArgument))
			{
				return launchArgument;
			}
			if (!string.IsNullOrEmpty(platformLanguage))
			{
				return platformLanguage;
			}
			return "english";
		}
	}

	public static int TotalKeys
	{
		get
		{
			checkLoaded();
			return mDictionary.Count;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static byte[] PatchedData
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static event Action<string> LanguageSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public static void checkLoaded(bool _throwExc = false)
	{
		if (mDictionary == null)
		{
			LoadAndSelectLanguage();
		}
		if (mDictionary == null && _throwExc)
		{
			throw new Exception("Localization could not be loaded");
		}
	}

	public static void Init()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		string launchArgument = GameUtils.GetLaunchArgument("language");
		if (!string.IsNullOrEmpty(launchArgument))
		{
			Log.Out("Localization language from command line: " + launchArgument);
		}
		else
		{
			string text = GamePrefs.GetString(EnumGamePrefs.Language);
			if (!string.IsNullOrEmpty(text))
			{
				platformLanguage = text;
				Log.Out("Localization language from prefs: " + text);
			}
			else
			{
				PlatformManager.NativePlatform.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					platformLanguage = PlatformManager.NativePlatform.Utils?.GetAppLanguage() ?? "english";
					LoadAndSelectLanguage();
					Log.Out("Localization language from platform: " + platformLanguage);
				};
			}
		}
		LoadAndSelectLanguage();
		LocalizationChecks = GameUtils.GetLaunchArgument("localizationchecks") != null;
	}

	public static bool ReloadBaseLocalization()
	{
		return LoadAndSelectLanguage(_forceReload: true);
	}

	public static bool LoadAndSelectLanguage(bool _forceReload = false)
	{
		if (_forceReload)
		{
			mDictionary = null;
			mDictionaryCaseInsensitive = null;
		}
		if (mDictionary == null && !loadBaseDictionaries())
		{
			return false;
		}
		return selectLanguage();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool loadBaseDictionaries()
	{
		allLanguages.Clear();
		patchedCells.Clear();
		mDictionary = new Dictionary<string, string[]>();
		mDictionaryCaseInsensitive = new CaseInsensitiveStringDictionary<string[]>();
		if (!loadCsv(GameIO.GetGameDir("Data/Config") + "/Localization.csv"))
		{
			mDictionary = null;
			mDictionaryCaseInsensitive = null;
			return false;
		}
		updateLanguages();
		WriteCsv();
		return true;
	}

	public static bool LoadPatchDictionaries(string _modName, string _folder, bool _loadingInGame)
	{
		checkLoaded(_throwExc: true);
		string text = _folder + "/Localization.csv";
		if (SdFile.Exists(text))
		{
			Log.Out("[MODS] Loading localization from mod: " + _modName);
			if (!loadCsv(text, _patch: true))
			{
				Log.Error("[MODS] Could not load localization from " + text);
			}
		}
		updateLanguages();
		selectLanguage();
		return true;
	}

	public static bool LoadServerPatchDictionary(byte[] _data)
	{
		checkLoaded(_throwExc: true);
		if (!loadCsv(_data, _patch: true, _serverData: true))
		{
			Log.Error("Could not load localization from server!");
		}
		updateLanguages();
		selectLanguage();
		return true;
	}

	public static void UnloadMetaData()
	{
		mDictionary.TryGetValue("KEY", out var value);
		if (value == null)
		{
			Log.Error("Failed unloading Localization metadata columns, could not find header entry!");
			return;
		}
		int[] array = new int[ignoredMetaColumnNames.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = -1;
		}
		for (int j = 0; j < ignoredMetaColumnNames.Length; j++)
		{
			string b = ignoredMetaColumnNames[j];
			for (int k = 0; k < value.Length; k++)
			{
				if (value[k].EqualsCaseInsensitive(b))
				{
					array[j] = k;
					break;
				}
			}
		}
		foreach (var (a, array3) in mDictionary)
		{
			if (a.EqualsCaseInsensitive("KEY"))
			{
				continue;
			}
			foreach (int num in array)
			{
				if (num >= 0 && num < array3.Length)
				{
					array3[num] = null;
				}
			}
		}
	}

	public static void UnloadUnusedLanguages()
	{
		mDictionary.TryGetValue("KEY", out var value);
		if (value == null)
		{
			Log.Error("Failed unloading Localization metadata columns, could not find header entry!");
			return;
		}
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i].EqualsCaseInsensitive("UsedInMainMenu"))
			{
				num = i;
			}
			if (value[i].EqualsCaseInsensitive("KeepLoaded"))
			{
				num2 = i;
			}
		}
		foreach (var (a, array2) in mDictionary)
		{
			if (a.EqualsCaseInsensitive("KEY") || (num2 >= 0 && num2 < array2.Length && !string.IsNullOrEmpty(array2[num2])))
			{
				continue;
			}
			for (int j = 0; j < array2.Length; j++)
			{
				if (j != num && j != currentLanguageIndex && (j != defaultLanguageIndex || currentLanguageIndex >= 0) && !string.IsNullOrEmpty(array2[currentLanguageIndex]))
				{
					array2[j] = null;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static (int defaultLanguage, int userLanguage) findUserLanguageColumns(string[] _headerColumns, string _userLanguage)
	{
		if (_headerColumns == null || _headerColumns.Length == 0)
		{
			return (defaultLanguage: -1, userLanguage: -1);
		}
		int item = -1;
		int item2 = -1;
		for (int i = 0; i < _headerColumns.Length; i++)
		{
			if (_headerColumns[i].EqualsCaseInsensitive("english"))
			{
				item2 = i;
			}
			if (_headerColumns[i].EqualsCaseInsensitive(_userLanguage))
			{
				item = i;
			}
		}
		return (defaultLanguage: item2, userLanguage: item);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool selectLanguage()
	{
		if (mDictionary == null)
		{
			return false;
		}
		mDictionary.TryGetValue("KEY", out var value);
		(defaultLanguageIndex, currentLanguageIndex) = findUserLanguageColumns(value, RequestedLanguage);
		UIRoot.Broadcast("OnLocalize");
		Localization.LanguageSelected?.Invoke(ActiveLanguage);
		return currentLanguageIndex >= 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateLanguages()
	{
		UnloadMetaData();
		allLanguages.Clear();
		languageToColumnIndex.Clear();
		if (mDictionary != null)
		{
			for (int i = 0; i < mDictionary["KEY"].Length; i++)
			{
				string text = mDictionary["KEY"][i];
				allLanguages.Add(text);
				languageToColumnIndex[text] = i;
			}
		}
	}

	public static void WriteCsv()
	{
		new MicroStopwatch(_bStart: true);
		using PooledMemoryStream pooledMemoryStream = MemoryPools.poolMS.AllocSync(_bReset: true);
		using (TextWriter textWriter = new StreamWriter(pooledMemoryStream, Encoding.UTF8, 1024, leaveOpen: true))
		{
			textWriter.Write("KEY");
			string[] value = mDictionary["KEY"];
			for (int i = 0; i < value.Length; i++)
			{
				textWriter.Write(",");
				textWriter.Write(value[i]);
			}
			textWriter.WriteLine();
			int num = value.Length;
			foreach (KeyValuePair<string, bool[]> patchedCell in patchedCells)
			{
				if (!mDictionaryCaseInsensitive.TryGetValue(patchedCell.Key, out value))
				{
					continue;
				}
				textWriter.Write(patchedCell.Key);
				int j;
				for (j = 0; j < value.Length; j++)
				{
					textWriter.Write(',');
					if (!patchedCell.Value[j])
					{
						continue;
					}
					string text = value[j] ?? "";
					text = text.Replace("\n", "\\n");
					int num2;
					if (text.IndexOf('"') < 0)
					{
						num2 = ((text.IndexOf(',') >= 0) ? 1 : 0);
						if (num2 == 0)
						{
							goto IL_012a;
						}
					}
					else
					{
						num2 = 1;
					}
					textWriter.Write('"');
					text = text.Replace("\"", "\"\"");
					goto IL_012a;
					IL_012a:
					textWriter.Write(text);
					if (num2 != 0)
					{
						textWriter.Write('"');
					}
				}
				for (; j < num; j++)
				{
					textWriter.Write(',');
				}
				textWriter.WriteLine();
			}
		}
		pooledMemoryStream.Position = 0L;
		using PooledMemoryStream pooledMemoryStream2 = MemoryPools.poolMS.AllocSync(_bReset: true);
		using DeflateOutputStream destination = new DeflateOutputStream(pooledMemoryStream2, 3);
		StreamUtils.StreamCopy(pooledMemoryStream, destination);
		PatchedData = pooledMemoryStream2.ToArray();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool getLanguageEntry(string[] _entry, int _languageColumn, out string _result, string _localizationCheckPrefix = "L_")
	{
		if (_languageColumn < 0 || _languageColumn >= _entry.Length)
		{
			_result = null;
			return false;
		}
		string text = _entry[_languageColumn];
		if (string.IsNullOrEmpty(text))
		{
			_result = null;
			return false;
		}
		if (!LocalizationChecks)
		{
			_result = text;
		}
		else
		{
			_result = _localizationCheckPrefix + text;
		}
		return true;
	}

	public static string Get(string _key, bool _caseInsensitive = false, string _languageName = null)
	{
		checkLoaded();
		if (string.IsNullOrEmpty(_key))
		{
			return "";
		}
		int value = -1;
		if (!string.IsNullOrEmpty(_languageName) && !languageToColumnIndex.TryGetValue(_languageName, out value))
		{
			Log.Warning("[Localization] Requested '" + _key + "' for non-existing language '" + _languageName + "'");
			return _key;
		}
		if ((_caseInsensitive ? mDictionaryCaseInsensitive : mDictionary).TryGetValue(_key, out var value2))
		{
			string _result2;
			if (value < 0)
			{
				if (getLanguageEntry(value2, currentLanguageIndex, out var _result) || getLanguageEntry(value2, defaultLanguageIndex, out _result, "LE_"))
				{
					return _result;
				}
			}
			else if (getLanguageEntry(value2, value, out _result2))
			{
				return _result2;
			}
		}
		if (LocalizationChecks)
		{
			return "UL_" + _key;
		}
		return _key;
	}

	public static bool Exists(string _key, bool _caseInsensitive = false)
	{
		checkLoaded();
		if (string.IsNullOrEmpty(_key))
		{
			return false;
		}
		if (!_caseInsensitive)
		{
			return mDictionary.ContainsKey(_key);
		}
		return mDictionaryCaseInsensitive.ContainsKey(_key);
	}

	public static bool TryGet(string _key, out string _localizedString)
	{
		if (!Exists(_key))
		{
			_localizedString = _key;
			return false;
		}
		_localizedString = Get(_key);
		return true;
	}

	public static string FormatListAnd(params object[] _items)
	{
		return formatListX("listAndTwo", "listAndStart", "listAndMiddle", "listAndEnd", _items);
	}

	public static string FormatListOr(params object[] _items)
	{
		return formatListX("listOrTwo", "listOrStart", "listOrMiddle", "listOrEnd", _items);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string formatListX(string _listXTwo, string _listXStart, string _listXMiddle, string _listXEnd, object[] _items)
	{
		int num = _items.Length;
		if (num > 0)
		{
			return num switch
			{
				1 => _items[0].ToString(), 
				2 => string.Format(Get(_listXTwo), _items), 
				_ => FormatThreeOrMore(), 
			};
		}
		return string.Empty;
		[PublicizedFrom(EAccessModifier.Internal)]
		string FormatThreeOrMore()
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder.AppendFormat(Get(_listXStart), _items[0], _items[1]);
			for (int i = 2; i < _items.Length - 1; i++)
			{
				stringBuilder2.AppendFormat(Get(_listXMiddle), stringBuilder, _items[i]);
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder stringBuilder4 = stringBuilder;
				stringBuilder = stringBuilder3;
				stringBuilder2 = stringBuilder4;
				stringBuilder2.Clear();
			}
			stringBuilder2.AppendFormat(Get(_listXEnd), stringBuilder, _items[^1]);
			return stringBuilder2.ToString();
		}
	}

	public static IEnumerable<KeyValuePair<string, string[]>> EnumerateAll()
	{
		foreach (KeyValuePair<string, string[]> item in mDictionary)
		{
			yield return item;
		}
	}

	public static string GetCurrentLocale()
	{
		return ActiveLanguage switch
		{
			"english" => "en-US", 
			"german" => "de-DE", 
			"spanish" => "es-ES", 
			"french" => "fr-FR", 
			"italian" => "it-IT", 
			"japanese" => "ja-JP", 
			"koreana" => "ko-KR", 
			"polish" => "pl-PL", 
			"brazilian" => "pt-BR", 
			"russian" => "ru-RU", 
			"turkish" => "tr-TR", 
			"schinese" => "zh-Hans", 
			"tchinese" => "zh-Hant", 
			_ => throw new Exception("No language code mapping found for Language \"" + ActiveLanguage + "\"."), 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool loadCsv(string _filename, bool _patch = false)
	{
		return loadCsv(SdFile.ReadAllBytes(_filename), _patch);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool loadCsv(byte[] _asset, bool _patch = false, bool _serverData = false)
	{
		ByteReader byteReader = new ByteReader(_asset);
		BetterList<string> betterList = byteReader.ReadCSV();
		if (betterList.size < 2)
		{
			return false;
		}
		betterList.buffer[0] = "KEY";
		if (!string.Equals(betterList.buffer[0], "KEY", StringComparison.OrdinalIgnoreCase))
		{
			Log.Error("Invalid localization CSV file. The first value is expected to be 'KEY', followed by language columns.\nInstead found '" + betterList.buffer[0] + "'");
			return false;
		}
		int[] array = null;
		int newLength = 0;
		int origColIndexUsedInMainMenu = -1;
		if (!_patch)
		{
			mDictionary.Clear();
			mDictionaryCaseInsensitive.Clear();
		}
		else
		{
			string[] array2 = mDictionary["KEY"];
			newLength = array2.Length;
			array = new int[betterList.size];
			for (int i = 1; i < betterList.size; i++)
			{
				string text = betterList.buffer[i];
				if (!languageHeaderMatcher.IsMatch(text) && !text.StartsWith("context", StringComparison.OrdinalIgnoreCase))
				{
					Log.Error($"Invalid localization CSV file. The first row has to contain the column definition header. Column {i + 1} is not a valid column definition (must be a non-empty string consisting of latin characters only): '{text}'");
					return false;
				}
				int j;
				for (j = 0; j < array2.Length; j++)
				{
					if (text.EqualsCaseInsensitive(array2[j]))
					{
						array[i] = j;
						break;
					}
				}
				if (j >= array2.Length)
				{
					array[i] = newLength++;
				}
			}
			for (int k = 0; k < array2.Length; k++)
			{
				if (array2[k].EqualsCaseInsensitive("UsedInMainMenu"))
				{
					origColIndexUsedInMainMenu = k;
					break;
				}
			}
		}
		while (betterList != null)
		{
			addCsv(betterList, array, newLength, origColIndexUsedInMainMenu, _serverData);
			betterList = byteReader.ReadCSV();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addCsv(BetterList<string> _temp, int[] _colTranslationTable, int _newLength, int _origColIndexUsedInMainMenu, bool _serverData = false)
	{
		if (_temp.size < 2)
		{
			return;
		}
		string text = _temp.buffer[0];
		if (string.IsNullOrEmpty(text))
		{
			Log.Warning("Localization: Entry missing a key! Please check Localization file. Skipping entry...");
			return;
		}
		if (_colTranslationTable == null)
		{
			string[] array = new string[_temp.size - 1];
			for (int i = 1; i < _temp.size; i++)
			{
				array[i - 1] = _temp.buffer[i];
			}
			if (mDictionary.TryAdd(text, array))
			{
				mDictionaryCaseInsensitive.Add(text, array);
			}
			else
			{
				Log.Warning("Localization: Duplicate key \"" + text + "\" found! Please check Localization file. Skipping entry...");
			}
			return;
		}
		if (mDictionaryCaseInsensitive.TryGetValue(text, out var value))
		{
			if (_serverData && _origColIndexUsedInMainMenu >= 0 && !string.IsNullOrEmpty(value[_origColIndexUsedInMainMenu]))
			{
				return;
			}
			if (value.Length < _newLength)
			{
				Array.Resize(ref value, _newLength);
			}
			if (patchedCells.TryGetValue(text, out var value2))
			{
				if (value2.Length < _newLength)
				{
					Array.Resize(ref value2, _newLength);
				}
			}
			else
			{
				value2 = new bool[_newLength];
			}
			for (int j = 1; j < _temp.size && j < _colTranslationTable.Length; j++)
			{
				if (!string.IsNullOrEmpty(_temp.buffer[j]))
				{
					int num = _colTranslationTable[j];
					value[num] = _temp.buffer[j];
					value2[num] = true;
				}
			}
			if (_origColIndexUsedInMainMenu < 0 || string.IsNullOrEmpty(value[_origColIndexUsedInMainMenu]))
			{
				patchedCells[text] = value2;
			}
		}
		else
		{
			value = new string[_newLength];
			bool[] array2 = new bool[_newLength];
			for (int k = 1; k < _temp.size && k < _colTranslationTable.Length; k++)
			{
				int num2 = _colTranslationTable[k];
				value[num2] = _temp.buffer[k];
				array2[num2] = true;
			}
			patchedCells.Add(text, array2);
		}
		mDictionary[text] = value;
		mDictionaryCaseInsensitive[text] = value;
	}

	public static int EstimateOwnedBytes()
	{
		int num = 0;
		foreach (KeyValuePair<string, string[]> item in mDictionary)
		{
			item.Deconstruct(out var key, out var value);
			string stringVal = key;
			string[] array = value;
			num += MemoryTracker.GetSize(stringVal);
			num += MemoryTracker.GetSize(array);
			value = array;
			foreach (string stringVal2 in value)
			{
				num += MemoryTracker.GetSize(stringVal2);
			}
		}
		return num;
	}
}

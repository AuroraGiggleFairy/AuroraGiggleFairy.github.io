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
	public static readonly string DefaultLanguage = "english";

	public static readonly string HeaderKey = "KEY";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string UsedInMainMenuKey = "UsedInMainMenu";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string MainLocalizationFilename = "Localization.txt";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool localizationChecks;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> AllLanguages = new List<string>();

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
	public static byte[] patchedData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string platformLanguage;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex languageHeaderMatcher = new Regex("^[A-Za-z]+$");

	public static bool LocalizationChecks => localizationChecks;

	public static Dictionary<string, string[]> dictionary
	{
		get
		{
			CheckLoaded();
			return mDictionary;
		}
	}

	public static string[] knownLanguages
	{
		get
		{
			CheckLoaded(_throwExc: true);
			return AllLanguages.ToArray();
		}
	}

	public static string language
	{
		get
		{
			CheckLoaded(_throwExc: true);
			if (currentLanguageIndex >= 0)
			{
				return AllLanguages[currentLanguageIndex];
			}
			if (AllLanguages.ContainsCaseInsensitive(DefaultLanguage))
			{
				return DefaultLanguage;
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
			return DefaultLanguage;
		}
	}

	public static int TotalKeys
	{
		get
		{
			CheckLoaded();
			return mDictionary.Count;
		}
	}

	public static byte[] PatchedData => patchedData;

	public static event Action<string> LanguageSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CheckLoaded(bool _throwExc = false)
	{
		if (mDictionary == null)
		{
			LoadAndSelectLanguage(DefaultLanguage);
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
			LoadAndSelectLanguage(launchArgument);
			Log.Out("Localization language from command line: " + language);
		}
		else
		{
			string value = GamePrefs.GetString(EnumGamePrefs.Language);
			if (!string.IsNullOrEmpty(value))
			{
				platformLanguage = value;
				LoadAndSelectLanguage(value);
				Log.Out("Localization language from prefs: " + language);
			}
			else
			{
				LoadAndSelectLanguage(DefaultLanguage);
				PlatformManager.NativePlatform.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					platformLanguage = PlatformManager.NativePlatform.Utils?.GetAppLanguage() ?? DefaultLanguage;
					LoadAndSelectLanguage(platformLanguage);
					Log.Out("Localization language from platform: " + language);
				};
			}
		}
		localizationChecks = GameUtils.GetLaunchArgument("localizationchecks") != null;
	}

	public static bool ReloadBaseLocalization()
	{
		return LoadAndSelectLanguage(language, _forceReload: true);
	}

	public static bool LoadAndSelectLanguage(string _language, bool _forceReload = false)
	{
		if (_forceReload)
		{
			mDictionary = null;
			mDictionaryCaseInsensitive = null;
		}
		if (mDictionary == null && !LoadBaseDictionaries())
		{
			return false;
		}
		return SelectLanguage(_language);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool LoadBaseDictionaries()
	{
		AllLanguages.Clear();
		patchedCells.Clear();
		mDictionary = new Dictionary<string, string[]>();
		mDictionaryCaseInsensitive = new CaseInsensitiveStringDictionary<string[]>();
		if (!LoadCsv(GameIO.GetGameDir("Data/Config") + "/Localization.txt"))
		{
			mDictionary = null;
			mDictionaryCaseInsensitive = null;
			return false;
		}
		UpdateLanguages();
		WriteCsv();
		return true;
	}

	public static bool LoadPatchDictionaries(string _modName, string _folder, bool _loadingInGame)
	{
		CheckLoaded(_throwExc: true);
		string text = _folder + "/Localization.txt";
		if (SdFile.Exists(text))
		{
			Log.Out("[MODS] Loading localization from mod: " + _modName);
			if (!LoadCsv(text, _patch: true))
			{
				Log.Error("[MODS] Could not load localization from " + text);
			}
		}
		UpdateLanguages();
		SelectLanguage(RequestedLanguage);
		return true;
	}

	public static bool LoadServerPatchDictionary(byte[] _data)
	{
		CheckLoaded(_throwExc: true);
		if (!LoadCsv(_data, _patch: true, _serverData: true))
		{
			Log.Error("Could not load localization from server!");
		}
		UpdateLanguages();
		SelectLanguage(RequestedLanguage);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SelectLanguage(string _language)
	{
		if (mDictionary == null)
		{
			return false;
		}
		currentLanguageIndex = -1;
		defaultLanguageIndex = -1;
		if (mDictionary.Count > 0 && mDictionary.TryGetValue(HeaderKey, out var value))
		{
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i].EqualsCaseInsensitive(DefaultLanguage))
				{
					defaultLanguageIndex = i;
				}
				if (value[i].EqualsCaseInsensitive(_language))
				{
					currentLanguageIndex = i;
				}
			}
		}
		UIRoot.Broadcast("OnLocalize");
		Localization.LanguageSelected?.Invoke(_language);
		return currentLanguageIndex >= 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateLanguages()
	{
		AllLanguages.Clear();
		languageToColumnIndex.Clear();
		if (mDictionary != null)
		{
			for (int i = 0; i < mDictionary[HeaderKey].Length; i++)
			{
				string text = mDictionary[HeaderKey][i];
				AllLanguages.Add(text);
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
			textWriter.Write(HeaderKey);
			string[] value = mDictionary[HeaderKey];
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
		patchedData = pooledMemoryStream2.ToArray();
	}

	public static string Get(string _key, bool _caseInsensitive = false)
	{
		CheckLoaded();
		if (string.IsNullOrEmpty(_key))
		{
			return "";
		}
		if ((_caseInsensitive ? mDictionaryCaseInsensitive : mDictionary).TryGetValue(_key, out var value))
		{
			if (currentLanguageIndex >= 0 && currentLanguageIndex < value.Length && !string.IsNullOrEmpty(value[currentLanguageIndex]))
			{
				if (!localizationChecks)
				{
					return value[currentLanguageIndex];
				}
				return "L_" + value[currentLanguageIndex];
			}
			if (defaultLanguageIndex >= 0 && defaultLanguageIndex < value.Length && !string.IsNullOrEmpty(value[defaultLanguageIndex]))
			{
				if (!localizationChecks)
				{
					return value[defaultLanguageIndex];
				}
				return "LE_" + value[defaultLanguageIndex];
			}
		}
		if (localizationChecks)
		{
			return "UL_" + _key;
		}
		return _key;
	}

	public static string Get(string _key, string _languageName, bool _caseInsensitive = false)
	{
		CheckLoaded();
		if (string.IsNullOrEmpty(_key))
		{
			return "";
		}
		if (string.IsNullOrEmpty(_languageName))
		{
			return Get(_key);
		}
		if (!languageToColumnIndex.TryGetValue(_languageName, out var value))
		{
			Log.Warning("[Localization] Requested '" + _key + "' for non-existing language '" + _languageName + "'");
			return _key;
		}
		if ((_caseInsensitive ? mDictionaryCaseInsensitive : mDictionary).TryGetValue(_key, out var value2) && value < value2.Length && !string.IsNullOrEmpty(value2[value]))
		{
			if (!localizationChecks)
			{
				return value2[value];
			}
			return "L_" + value2[value];
		}
		if (localizationChecks)
		{
			return "UL_" + _key;
		}
		return _key;
	}

	public static bool Exists(string _key, bool _caseInsensitive = false)
	{
		CheckLoaded();
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

	public static string FormatListAnd(params object[] items)
	{
		return FormatListX("listAndTwo", "listAndStart", "listAndMiddle", "listAndEnd", items);
	}

	public static string FormatListOr(params object[] items)
	{
		return FormatListX("listOrTwo", "listOrStart", "listOrMiddle", "listOrEnd", items);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string FormatListX(string listXTwo, string listXStart, string listXMiddle, string listXEnd, object[] items)
	{
		int num = items.Length;
		if (num > 0)
		{
			return num switch
			{
				1 => items[0].ToString(), 
				2 => string.Format(Get(listXTwo), items), 
				_ => FormatThreeOrMore(), 
			};
		}
		return string.Empty;
		[PublicizedFrom(EAccessModifier.Internal)]
		string FormatThreeOrMore()
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder.AppendFormat(Get(listXStart), items[0], items[1]);
			for (int i = 2; i < items.Length - 1; i++)
			{
				stringBuilder2.AppendFormat(Get(listXMiddle), stringBuilder, items[i]);
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder stringBuilder4 = stringBuilder;
				stringBuilder = stringBuilder3;
				stringBuilder2 = stringBuilder4;
				stringBuilder2.Clear();
			}
			stringBuilder2.AppendFormat(Get(listXEnd), stringBuilder, items[^1]);
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
		return language switch
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
			_ => throw new Exception("No language code mapping found for Language \"" + language + "\"."), 
		};
	}

	public static bool LoadCsv(string _filename, bool _patch = false)
	{
		return LoadCsv(SdFile.ReadAllBytes(_filename), _patch);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool LoadCsv(byte[] _asset, bool _patch = false, bool _serverData = false)
	{
		ByteReader byteReader = new ByteReader(_asset);
		BetterList<string> betterList = byteReader.ReadCSV();
		if (betterList.size < 2)
		{
			return false;
		}
		betterList.buffer[0] = HeaderKey;
		if (!string.Equals(betterList.buffer[0], HeaderKey, StringComparison.OrdinalIgnoreCase))
		{
			Log.Error("Invalid localization CSV file. The first value is expected to be '" + HeaderKey + "', followed by language columns.\nInstead found '" + betterList.buffer[0] + "'");
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
			string[] array2 = mDictionary[HeaderKey];
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
				if (array2[k].EqualsCaseInsensitive(UsedInMainMenuKey))
				{
					origColIndexUsedInMainMenu = k;
					break;
				}
			}
		}
		while (betterList != null)
		{
			AddCsv(betterList, array, newLength, origColIndexUsedInMainMenu, _serverData);
			betterList = byteReader.ReadCSV();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddCsv(BetterList<string> _temp, int[] _colTranslationTable, int _newLength, int _origColIndexUsedInMainMenu, bool _serverData = false)
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
			if (!mDictionary.ContainsKey(text))
			{
				mDictionary.Add(text, array);
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
}

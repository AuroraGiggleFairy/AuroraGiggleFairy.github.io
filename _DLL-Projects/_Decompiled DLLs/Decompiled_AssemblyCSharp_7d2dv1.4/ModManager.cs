using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class ModManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class AtlasManagerEntry
	{
		public readonly MultiSourceAtlasManager Manager;

		public readonly bool CreatedByMod;

		public readonly Shader Shader;

		public readonly Action<UIAtlas, bool> OnNewAtlasLoaded;

		public AtlasManagerEntry(MultiSourceAtlasManager _manager, bool _createdByMod, Shader _shader, Action<UIAtlas, bool> _onNewAtlasLoaded)
		{
			Manager = _manager;
			CreatedByMod = _createdByMod;
			Shader = _shader;
			OnNewAtlasLoaded = _onNewAtlasLoaded;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string ModsBasePathLegacy = ((Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXServer) ? (Application.dataPath + "/../../Mods") : (Application.dataPath + "/../Mods"));

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly DictionaryList<string, Mod> loadedMods = new DictionaryList<string, Mod>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Mod> failedMods = new List<Mod>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, AtlasManagerEntry> atlasManagers = new CaseInsensitiveStringDictionary<AtlasManagerEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject atlasesParentGo;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Shader defaultShader;

	public static string ModsBasePath
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameIO.GetUserGameDataDir() + "/Mods";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void initModManager()
	{
	}

	public static void LoadMods()
	{
		initModManager();
		bool num = loadModsFromFolder(ModsBasePath);
		bool flag = GameIO.PathsEquals(ModsBasePath, ModsBasePathLegacy, _ignoreCase: true) || loadModsFromFolder(ModsBasePathLegacy);
		if (!num && !flag)
		{
			Log.Out("[MODS] No mods folder found");
			return;
		}
		int num2 = loadedMods.list.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (Mod _mod) => _mod.Name == "TFP_Harmony");
		if (num2 >= 0)
		{
			Mod item = loadedMods.list[num2];
			loadedMods.list.RemoveAt(num2);
			loadedMods.list.Insert(0, item);
		}
		Log.Out("[MODS] Initializing mod code");
		foreach (Mod item2 in loadedMods.list)
		{
			item2.InitModCode();
		}
		Log.Out("[MODS] Loading done");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool loadModsFromFolder(string _folder)
	{
		if (!SdDirectory.Exists(_folder))
		{
			return false;
		}
		Log.Out("[MODS] Start loading from: '" + _folder + "'");
		string[] directories = SdDirectory.GetDirectories(_folder);
		Array.Sort(directories);
		string[] array = directories;
		foreach (string path in array)
		{
			Log.Out("[MODS]   Trying to load from folder: '" + Path.GetFileName(path) + "'");
			try
			{
				Mod mod = Mod.LoadDefinitionFromFolder(path);
				if (mod != null)
				{
					if (!mod.LoadMod())
					{
						failedMods.Add(mod);
					}
					else
					{
						loadedMods.Add(mod.Name, mod);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error("[MODS]     Failed loading mod from folder: '" + Path.GetFileName(path) + "'");
				Log.Exception(e);
			}
		}
		return true;
	}

	public static bool ModLoaded(string _modName)
	{
		return loadedMods.dict.ContainsKey(_modName);
	}

	public static Mod GetMod(string _modName, bool _onlyLoaded = false)
	{
		if (!ModLoaded(_modName))
		{
			return null;
		}
		return loadedMods.dict[_modName];
	}

	public static List<Mod> GetLoadedMods()
	{
		return loadedMods.list;
	}

	public static List<Mod> GetFailedMods(Mod.EModLoadState? _failureReason = null)
	{
		if (!_failureReason.HasValue)
		{
			return failedMods;
		}
		List<Mod> list = new List<Mod>();
		foreach (Mod failedMod in failedMods)
		{
			if (failedMod.LoadState == _failureReason.Value)
			{
				list.Add(failedMod);
			}
		}
		return list;
	}

	public static List<Assembly> GetLoadedAssemblies()
	{
		List<Assembly> list = new List<Assembly>();
		for (int i = 0; i < loadedMods.Count; i++)
		{
			Mod mod = loadedMods.list[i];
			list.AddRange(mod.AllAssemblies);
		}
		return list;
	}

	public static Mod GetModForAssembly(Assembly _asm)
	{
		for (int i = 0; i < loadedMods.Count; i++)
		{
			Mod mod = loadedMods.list[i];
			if (mod.ContainsAssembly(_asm))
			{
				return mod;
			}
		}
		return null;
	}

	public static bool AnyConfigModActive()
	{
		for (int i = 0; i < loadedMods.Count; i++)
		{
			if (loadedMods.list[i].GameConfigMod)
			{
				return true;
			}
		}
		return false;
	}

	public static string PatchModPathString(string _pathString)
	{
		if (_pathString.IndexOf('@') < 0)
		{
			return null;
		}
		int num = _pathString.IndexOf("@modfolder(", StringComparison.OrdinalIgnoreCase);
		if (num < 0)
		{
			return null;
		}
		int num2 = _pathString.IndexOf("):", StringComparison.Ordinal);
		int num3 = num + "@modfolder(".Length;
		string text = _pathString.Substring(num3, num2 - num3);
		string text2 = _pathString.Substring(0, num);
		int i;
		for (i = num2 + 2; _pathString[i] == '/'; i++)
		{
		}
		string text3 = _pathString.Substring(i);
		Mod mod = GetMod(text, _onlyLoaded: true);
		if (mod != null)
		{
			_pathString = text2 + mod.Path + "/" + text3;
			return _pathString;
		}
		Log.Error("[MODS] Mod reference for a mod that is not loaded: '" + text + "'");
		return null;
	}

	public static IEnumerator LoadPatchStuff(bool _isLoadingInGame)
	{
		yield return LoadUiAtlases(_isLoadingInGame);
		yield return LoadLocalizations(_isLoadingInGame);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadUiAtlases(bool _isLoadingInGame)
	{
		if (GameManager.IsDedicatedServer || _isLoadingInGame)
		{
			yield break;
		}
		for (int i = 0; i < loadedMods.Count; i++)
		{
			Mod mod = loadedMods.list[i];
			string path = mod.Path + "/UIAtlases";
			if (!SdDirectory.Exists(path))
			{
				continue;
			}
			string[] array = null;
			try
			{
				array = SdDirectory.GetDirectories(path);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			if (array == null)
			{
				continue;
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				string fileName = Path.GetFileName(text);
				if (!atlasManagers.TryGetValue(fileName, out var ame))
				{
					Log.Out("[MODS] Creating new atlas '" + fileName + "' for mod '" + mod.Name + "'");
					RegisterAtlasManager(MultiSourceAtlasManager.Create(atlasesParentGo, fileName), _createdByMod: true, defaultShader);
					ame = atlasManagers[fileName];
				}
				yield return UIAtlasFromFolder.CreateUiAtlasFromFolder(text, ame.Shader, [PublicizedFrom(EAccessModifier.Internal)] (UIAtlas _atlas) =>
				{
					_atlas.transform.parent = ame.Manager.transform;
					ame.Manager.AddAtlas(_atlas, _isLoadingInGame);
					ame.OnNewAtlasLoaded?.Invoke(_atlas, _isLoadingInGame);
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadLocalizations(bool _isLoadingInGame)
	{
		if (_isLoadingInGame)
		{
			yield break;
		}
		for (int i = 0; i < loadedMods.Count; i++)
		{
			Mod mod = loadedMods.list[i];
			string text = mod.Path + "/Config";
			if (SdDirectory.Exists(text))
			{
				try
				{
					Localization.LoadPatchDictionaries(mod.Name, text, _isLoadingInGame);
				}
				catch (Exception e)
				{
					Log.Error("[MODS] Failed loading localization from mod: '" + mod.Name + "'");
					Log.Exception(e);
				}
			}
		}
		Localization.WriteCsv();
	}

	public static void ModAtlasesDefaults(GameObject _parentGo, Shader _defaultShader)
	{
		atlasesParentGo = _parentGo;
		defaultShader = _defaultShader;
	}

	public static void RegisterAtlasManager(MultiSourceAtlasManager _atlasManager, bool _createdByMod, Shader _shader, Action<UIAtlas, bool> _onNewAtlasLoaded = null)
	{
		atlasManagers.Add(_atlasManager.name, new AtlasManagerEntry(_atlasManager, _createdByMod, _shader, _onNewAtlasLoaded));
	}

	public static MultiSourceAtlasManager GetAtlasManager(string _name)
	{
		if (!atlasManagers.TryGetValue(_name, out var value))
		{
			return null;
		}
		return value.Manager;
	}

	public static void GameEnded()
	{
		foreach (KeyValuePair<string, AtlasManagerEntry> atlasManager in atlasManagers)
		{
			atlasManager.Value.Manager.CleanupAfterGame();
		}
		Localization.ReloadBaseLocalization();
		ThreadManager.RunCoroutineSync(LoadLocalizations(_isLoadingInGame: false));
	}
}

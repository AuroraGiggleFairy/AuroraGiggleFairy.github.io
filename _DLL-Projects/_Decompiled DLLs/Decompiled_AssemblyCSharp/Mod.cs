using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using JetBrains.Annotations;
using Platform;
using UnityEngine;

public class Mod
{
	public enum EModLoadState
	{
		LoadNotRequested,
		Success,
		NotAntiCheatCompatible,
		SkippedDueToAntiCheat,
		DuplicateModName,
		FailedLoadingAssembly,
		Failed
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Assembly> allAssemblies = new List<Assembly>();

	public readonly ReadOnlyCollection<Assembly> AllAssemblies;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex nameValidationRegex = new Regex("^[0-9a-zA-Z_\\-]+$", RegexOptions.Compiled);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EModLoadState LoadState
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Path
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string FolderName
	{
		[UsedImplicitly]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string DisplayName
	{
		[UsedImplicitly]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Description
	{
		[UsedImplicitly]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Author
	{
		[UsedImplicitly]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Version Version
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string VersionString
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Website
	{
		[UsedImplicitly]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool SkipLoadingWithAntiCheat
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AntiCheatCompatible
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool GameConfigMod
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Mod()
	{
		AllAssemblies = new ReadOnlyCollection<Assembly>(allAssemblies);
	}

	public bool LoadMod()
	{
		LoadState = EModLoadState.Failed;
		if (ModManager.ModLoaded(Name))
		{
			Log.Warning("[MODS]     Mod with same name (" + Name + ") already loaded, ignoring");
			LoadState = EModLoadState.DuplicateModName;
			return false;
		}
		EModLoadState eModLoadState = LoadAssemblies();
		if (eModLoadState != EModLoadState.Success)
		{
			LoadState = eModLoadState;
			return false;
		}
		DetectContents();
		Log.Out("[MODS]     Loaded Mod: " + Name + " (" + (VersionString ?? "<unknown version>") + ")");
		LoadState = EModLoadState.Success;
		return LoadState == EModLoadState.Success;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EModLoadState LoadAssemblies()
	{
		string[] files = SdDirectory.GetFiles(Path);
		if (files.Length == 0)
		{
			return EModLoadState.Success;
		}
		string[] array = files;
		foreach (string text in array)
		{
			if (!text.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			if (!GameManager.IsDedicatedServer)
			{
				IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
				if (antiCheatClient != null && antiCheatClient.ClientAntiCheatEnabled())
				{
					if (SkipLoadingWithAntiCheat)
					{
						Log.Out("[MODS]     AntiCheat enabled, mod skipped because it is set not to load");
						return EModLoadState.SkippedDueToAntiCheat;
					}
					if (!AntiCheatCompatible)
					{
						Log.Warning("[MODS]     Mod contains custom code, AntiCheat needs to be disabled to load it!");
						return EModLoadState.NotAntiCheatCompatible;
					}
				}
			}
			try
			{
				Assembly assembly = loadAssembly(text);
				allAssemblies.Add(assembly);
				Log.Out("[MODS]     Loaded assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(assembly, text));
			}
			catch (Exception e)
			{
				Log.Error("[MODS]     Failed loading DLL " + text);
				Log.Exception(e);
				return EModLoadState.FailedLoadingAssembly;
			}
		}
		return EModLoadState.Success;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Assembly loadAssembly(string _path)
	{
		if (GameManager.IsDedicatedServer)
		{
			RuntimePlatform platform = Application.platform;
			if (platform == RuntimePlatform.WindowsServer || platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
			{
				return Assembly.Load(SdFile.ReadAllBytes(_path));
			}
		}
		return Assembly.LoadFrom(_path);
	}

	public bool InitModCode()
	{
		if (allAssemblies.Count > 0)
		{
			Log.Out("[MODS]   Initializing mod " + Name);
			bool flag = false;
			Type typeFromHandle = typeof(IModApi);
			foreach (Assembly allAssembly in allAssemblies)
			{
				try
				{
					Type[] types = allAssembly.GetTypes();
					foreach (Type type in types)
					{
						if (typeFromHandle.IsAssignableFrom(type))
						{
							Log.Out("[MODS]     Found ModAPI in assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(allAssembly) + ", creating instance");
							IModApi modApi = (IModApi)Activator.CreateInstance(type);
							try
							{
								modApi.InitMod(this);
								Log.Out("[MODS]     Initialized code in mod '" + Name + "' in assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(allAssembly));
							}
							catch (Exception e)
							{
								Log.Error("[MODS]     Failed initializing ModAPI instance on mod '" + Name + "' in assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(allAssembly));
								Log.Exception(e);
							}
							flag = true;
						}
					}
				}
				catch (ReflectionTypeLoadException)
				{
					Log.Warning("[MODS]     Failed iterating types in assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(allAssembly));
				}
				catch (Exception e2)
				{
					Log.Error("[MODS]     Failed creating ModAPI instance from assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(allAssembly));
					Log.Exception(e2);
					return false;
				}
			}
			if (!flag)
			{
				Log.Out("[MODS]     No ModAPI found in mod DLLs");
			}
		}
		return true;
	}

	public bool ContainsAssembly(Assembly _assembly)
	{
		return allAssemblies.Contains(_assembly);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DetectContents()
	{
		string path = Path + "/Config";
		if (!SdDirectory.Exists(path))
		{
			return;
		}
		string[] fileSystemEntries = SdDirectory.GetFileSystemEntries(path);
		for (int i = 0; i < fileSystemEntries.Length; i++)
		{
			string fileName = System.IO.Path.GetFileName(fileSystemEntries[i]);
			if (!fileName.EqualsCaseInsensitive("XUi_Menu") && !fileName.EqualsCaseInsensitive("loadingscreen.xml") && !fileName.EqualsCaseInsensitive("Localization.txt"))
			{
				GameConfigMod = true;
				break;
			}
		}
	}

	public static Mod LoadDefinitionFromFolder(string _path)
	{
		string text = _path + "/ModInfo.xml";
		string fileName = System.IO.Path.GetFileName(_path);
		if (!SdFile.Exists(text))
		{
			Log.Warning("[MODS]     Folder " + fileName + " does not contain a ModInfo.xml, ignoring");
			return null;
		}
		XmlFile xmlFile = new XmlFile(_path, "ModInfo.xml");
		XElement root = xmlFile.XmlDoc.Root;
		if (root == null)
		{
			Log.Error("[MODS]     " + fileName + "/ModInfo.xml does not have a root element, ignoring");
			return null;
		}
		Mod mod = ((root.Element("ModInfo") != null) ? parseModInfoV1(_path, fileName, text, xmlFile) : parseModInfoV2(_path, fileName, root));
		if (mod == null)
		{
			Log.Error("[MODS]     Could not parse " + fileName + "/ModInfo.xml, ignoring");
			return null;
		}
		return mod;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mod parseModInfoV2(string _modPath, string _folderName, XElement _xmlRoot)
	{
		string elementAttributeValue = getElementAttributeValue(_folderName, _xmlRoot, "Name");
		if (elementAttributeValue == null)
		{
			return null;
		}
		if (elementAttributeValue.Length == 0)
		{
			Log.Error("[MODS]     " + _folderName + "/ModInfo.xml does not specify a non-empty Name, ignoring");
			return null;
		}
		if (!nameValidationRegex.IsMatch(elementAttributeValue))
		{
			Log.Error($"[MODS]     {_folderName}/ModInfo.xml does not define a valid non-empty Name ({nameValidationRegex}), ignoring");
			return null;
		}
		Version result = null;
		string text = getElementAttributeValue(_folderName, _xmlRoot, "Version");
		if (text != null)
		{
			if (text.Length == 0)
			{
				text = null;
			}
			else
			{
				Version.TryParse(text, out result);
			}
		}
		if (result == null)
		{
			Log.Warning("[MODS]     " + _folderName + "/ModInfo.xml does not define a valid Version. Please consider updating it for future compatibility.");
		}
		string elementAttributeValue2 = getElementAttributeValue(_folderName, _xmlRoot, "DisplayName", _logNonExisting: false);
		if (string.IsNullOrEmpty(elementAttributeValue2))
		{
			Log.Error("[MODS]     " + _folderName + "/ModInfo.xml does not define a non-empty DisplayName, ignoring");
			return null;
		}
		string elementAttributeValue3 = getElementAttributeValue(_folderName, _xmlRoot, "Description", _logNonExisting: false);
		string elementAttributeValue4 = getElementAttributeValue(_folderName, _xmlRoot, "Author", _logNonExisting: false);
		string elementAttributeValue5 = getElementAttributeValue(_folderName, _xmlRoot, "Website", _logNonExisting: false);
		string elementAttributeValue6 = getElementAttributeValue(_folderName, _xmlRoot, "SkipWithAntiCheat", _logNonExisting: false);
		bool _result = false;
		if (!string.IsNullOrEmpty(elementAttributeValue6) && !StringParsers.TryParseBool(elementAttributeValue6, out _result))
		{
			Log.Warning("[MODS]     " + _folderName + "/ModInfo.xml does have a SkipWithAntiCheat, but its value is not a valid boolean. Assuming 'false'");
			_result = false;
		}
		return new Mod
		{
			Path = _modPath,
			FolderName = _folderName,
			Name = elementAttributeValue,
			DisplayName = elementAttributeValue2,
			Description = elementAttributeValue3,
			Author = elementAttributeValue4,
			Version = result,
			VersionString = text,
			Website = elementAttributeValue5,
			SkipLoadingWithAntiCheat = _result
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getElementAttributeValue(string _folderName, XElement _xmlParent, string _elementName, bool _logNonExisting = true)
	{
		List<XElement> list = _xmlParent.Elements(_elementName).ToList();
		if (list.Count != 1)
		{
			if (_logNonExisting)
			{
				Log.Error("[MODS] " + _folderName + "/ModInfo.xml does not have exactly one '" + _elementName + "' element, ignoring");
			}
			return null;
		}
		XAttribute xAttribute = list[0].Attribute("value");
		if (xAttribute == null)
		{
			Log.Error("[MODS] " + _folderName + "/ModInfo.xml '" + _elementName + "' element does not have a 'value' attribute, ignoring");
			return null;
		}
		return xAttribute.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mod parseModInfoV1(string _modPath, string _folderName, string _modInfoFilename, XmlFile _xml)
	{
		Log.Error("[MODS]     " + _folderName + "/ModInfo.xml in legacy format. V2 required to load mod");
		return null;
	}

	public override string ToString()
	{
		return DisplayName;
	}
}

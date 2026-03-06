using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class PathAbstractions
{
	public enum EAbstractedLocationType
	{
		HostSave,
		LocalSave,
		UserDataPath,
		Mods,
		GameData,
		None
	}

	public readonly struct AbstractedLocation : IEquatable<AbstractedLocation>, IComparable<AbstractedLocation>, IComparable
	{
		public static readonly AbstractedLocation None = new AbstractedLocation(EAbstractedLocationType.None, null, null, null, _isFolder: false);

		public readonly EAbstractedLocationType Type;

		public readonly string Name;

		public readonly string Folder;

		public readonly string RelativePath;

		public readonly string FileNameNoExtension;

		public readonly string Extension;

		public readonly bool IsFolder;

		public readonly Mod ContainingMod;

		public string FullPath => Folder + "/" + FileNameNoExtension + Extension;

		public string FullPathNoExtension => Folder + "/" + FileNameNoExtension;

		public AbstractedLocation(EAbstractedLocationType _type, string _name, string _fullPath, string _relativePath, bool _isFolder, Mod _containingMod = null)
		{
			_fullPath = _fullPath?.Replace("\\", "/");
			Type = _type;
			Name = Path.GetFileName(_name);
			Folder = Path.GetDirectoryName(_fullPath)?.Replace("\\", "/");
			RelativePath = _relativePath;
			FileNameNoExtension = Path.GetFileNameWithoutExtension(_fullPath);
			Extension = Path.GetExtension(_fullPath);
			Extension = (string.IsNullOrEmpty(Extension) ? null : Extension);
			IsFolder = _isFolder;
			ContainingMod = _containingMod;
		}

		public AbstractedLocation(EAbstractedLocationType _type, string _name, string _folder, string _relativePath, string _fileNameNoExtension, string _extension, bool _isFolder, Mod _containingMod = null)
		{
			Type = _type;
			Name = _name;
			Folder = _folder?.Replace("\\", "/");
			RelativePath = _relativePath;
			FileNameNoExtension = _fileNameNoExtension;
			Extension = _extension;
			IsFolder = _isFolder;
			ContainingMod = _containingMod;
		}

		public bool Exists()
		{
			if (Type == EAbstractedLocationType.None)
			{
				return false;
			}
			if (!IsFolder)
			{
				return SdFile.Exists(FullPath);
			}
			return SdDirectory.Exists(FullPath);
		}

		public bool Equals(AbstractedLocation _other)
		{
			if (Type != _other.Type)
			{
				return false;
			}
			if (Type == EAbstractedLocationType.None)
			{
				return true;
			}
			if (IsFolder != _other.IsFolder)
			{
				return false;
			}
			if (string.Equals(Name, _other.Name))
			{
				return GameIO.PathsEquals(FullPath, _other.FullPath, _ignoreCase: true);
			}
			return false;
		}

		public override bool Equals(object _obj)
		{
			if (_obj == null)
			{
				return false;
			}
			if (_obj is AbstractedLocation other)
			{
				return Equals(other);
			}
			return false;
		}

		public static bool operator ==(AbstractedLocation _a, AbstractedLocation _b)
		{
			return _a.Equals(_b);
		}

		public static bool operator !=(AbstractedLocation _a, AbstractedLocation _b)
		{
			return !(_a == _b);
		}

		public override int GetHashCode()
		{
			return (int)(((uint)(((Name != null) ? Name.GetHashCode() : 0) * 397) ^ (uint)Type) * 397) ^ ((FullPath != null) ? FullPath.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return Name + " (src: " + Type.ToStringCached() + ")";
		}

		public int CompareTo(AbstractedLocation _other)
		{
			int num = string.Compare(Name, _other.Name, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			int num2 = Type.CompareTo(_other.Type);
			if (num2 != 0)
			{
				return num2;
			}
			int num3 = string.Compare(FileNameNoExtension, _other.FileNameNoExtension, StringComparison.OrdinalIgnoreCase);
			if (num3 != 0)
			{
				return num3;
			}
			int num4 = string.Compare(Extension, _other.Extension, StringComparison.OrdinalIgnoreCase);
			if (num4 != 0)
			{
				return num4;
			}
			return string.Compare(Folder, _other.Folder, StringComparison.OrdinalIgnoreCase);
		}

		public int CompareTo(object _obj)
		{
			if (_obj == null)
			{
				return 1;
			}
			if (!(_obj is AbstractedLocation other))
			{
				throw new ArgumentException("Object must be of type AbstractedLocation");
			}
			return CompareTo(other);
		}
	}

	public class SearchDefinition
	{
		public readonly bool IsFolder;

		public readonly string Extension;

		public readonly bool RemoveExtension;

		public readonly bool Recursive;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<SearchPath> paths;

		public SearchDefinition(bool _isFolder, string _extension, bool _removeExtension, bool _recursive, params SearchPath[] _paths)
		{
			IsFolder = _isFolder;
			Extension = _extension;
			RemoveExtension = _removeExtension;
			Recursive = _recursive;
			if (IsFolder && Recursive)
			{
				throw new Exception("SearchDefinition can not be set to target folders and search recursively at the same time!");
			}
			paths = new List<SearchPath>(_paths);
			for (int i = 0; i < _paths.Length; i++)
			{
				_paths[i].SetOwner(this);
			}
			allSearchDefs.Add(this);
		}

		public AbstractedLocation GetLocation(string _name, string _worldName = null, string _gameName = null, EAbstractedLocationType? _onlyLocationType = null)
		{
			foreach (SearchPath path in paths)
			{
				if (path.CanMatch && (!_onlyLocationType.HasValue || path.CanHandleLocationType(_onlyLocationType.Value)))
				{
					AbstractedLocation location = path.GetLocation(_name, _worldName, _gameName);
					if (location.Type != EAbstractedLocationType.None)
					{
						return location;
					}
				}
			}
			return new AbstractedLocation(EAbstractedLocationType.None, _name, null, null, IsFolder);
		}

		public List<AbstractedLocation> GetAvailablePathsList(Regex _nameMatch = null, string _worldName = null, string _gameName = null, bool _ignoreDuplicateNames = false, EAbstractedLocationType? _onlyLocationType = null)
		{
			List<AbstractedLocation> _resultList = new List<AbstractedLocation>();
			GetAvailablePathsList(ref _resultList, _nameMatch, _worldName, _gameName, _ignoreDuplicateNames, _onlyLocationType);
			return _resultList;
		}

		public void GetAvailablePathsList(ref List<AbstractedLocation> _resultList, Regex _nameMatch = null, string _worldName = null, string _gameName = null, bool _ignoreDuplicateNames = false, EAbstractedLocationType? _onlyLocationType = null)
		{
			if (_resultList == null)
			{
				_resultList = new List<AbstractedLocation>();
			}
			foreach (SearchPath path in paths)
			{
				if (path.CanMatch && (!_onlyLocationType.HasValue || path.CanHandleLocationType(_onlyLocationType.Value)))
				{
					path.GetAvailablePathsList(_resultList, _nameMatch, _worldName, _gameName, _ignoreDuplicateNames);
				}
			}
		}

		public AbstractedLocation? BuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, string _modName = null, string _worldName = null, string _gameName = null)
		{
			Mod mod = null;
			if (_locationType == EAbstractedLocationType.Mods)
			{
				if (string.IsNullOrEmpty(_modName))
				{
					throw new ArgumentException("BuildLocation for LocationType==Mods requires a Mod name", "_modName");
				}
				mod = ModManager.GetMod(_modName, _onlyLoaded: true);
				if (mod == null)
				{
					throw new ArgumentException("BuildLocation for LocationType==Mods requires the name of a loaded Mod", "_modName");
				}
			}
			return BuildLocation(_locationType, _subFolder, _elementName, mod, _worldName, _gameName);
		}

		public AbstractedLocation? BuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod = null, string _worldName = null, string _gameName = null)
		{
			foreach (SearchPath path in paths)
			{
				if (path.CanHandleLocationType(_locationType))
				{
					return path.TryBuildLocation(_locationType, _subFolder, _elementName, _mod, _worldName, _gameName);
				}
			}
			return null;
		}

		public string GetBasePath(EAbstractedLocationType _locationType, string _modName = null, string _worldName = null, string _gameName = null)
		{
			Mod mod = null;
			if (_locationType == EAbstractedLocationType.Mods)
			{
				if (string.IsNullOrEmpty(_modName))
				{
					throw new ArgumentException("GetBasePath for LocationType==Mods requires a Mod name", "_modName");
				}
				mod = ModManager.GetMod(_modName, _onlyLoaded: true);
				if (mod == null)
				{
					throw new ArgumentException("GetBasePath for LocationType==Mods requires the name of a loaded Mod", "_modName");
				}
			}
			return GetBasePath(_locationType, mod, _worldName, _gameName);
		}

		public string GetBasePath(EAbstractedLocationType _locationType, Mod _mod = null, string _worldName = null, string _gameName = null)
		{
			foreach (SearchPath path in paths)
			{
				if (path.CanHandleLocationType(_locationType))
				{
					return path.TryGetBasePath(_locationType, _mod, _worldName, _gameName);
				}
			}
			return null;
		}

		public void InvalidateCache()
		{
			foreach (SearchPath path in paths)
			{
				path.InvalidateCache();
			}
		}
	}

	public abstract class SearchPath
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public SearchDefinition Owner;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string RelativePath;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly bool PathIsTarget;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly Dictionary<string, IList<AbstractedLocation>> locationsCache = new CaseInsensitiveStringDictionary<IList<AbstractedLocation>>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool locationsCachePopulated;

		public virtual bool CanMatch => true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public SearchPath(string _relativePath, bool _pathIsTarget)
		{
			RelativePath = _relativePath;
			PathIsTarget = _pathIsTarget;
		}

		public void SetOwner(SearchDefinition _owner)
		{
			Owner = _owner;
		}

		public abstract AbstractedLocation GetLocation(string _name, string _worldName, string _gameName);

		public abstract void GetAvailablePathsList(List<AbstractedLocation> _targetList, Regex _nameMatch, string _worldName, string _gameName, bool _ignoreDuplicateNames);

		[PublicizedFrom(EAccessModifier.Protected)]
		public AbstractedLocation getLocationSingleBase(EAbstractedLocationType _locationType, string _basePath, string _name, string _worldName, string _gameName, Mod _containingMod, string _subfolder = null)
		{
			UseCache(_worldName, _gameName);
			if (!SdDirectory.Exists(_basePath))
			{
				return AbstractedLocation.None;
			}
			string text = _basePath + "/" + _name + Owner.Extension;
			if (Owner.IsFolder)
			{
				if (SdDirectory.Exists(text))
				{
					return new AbstractedLocation(_locationType, _name, text, _subfolder, Owner.IsFolder, _containingMod);
				}
			}
			else
			{
				if (SdFile.Exists(text))
				{
					string name = (Owner.RemoveExtension ? GameIO.RemoveExtension(_name, Owner.Extension) : _name);
					return new AbstractedLocation(_locationType, name, text, _subfolder, Owner.IsFolder, _containingMod);
				}
				if (Owner.Recursive)
				{
					string[] directories = SdDirectory.GetDirectories(_basePath);
					foreach (string text2 in directories)
					{
						AbstractedLocation locationSingleBase = getLocationSingleBase(_locationType, text2, _name, _worldName, _gameName, _containingMod, Path.GetFileName(text2));
						if (!locationSingleBase.Equals(AbstractedLocation.None))
						{
							return locationSingleBase;
						}
					}
				}
			}
			return AbstractedLocation.None;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void getAvailablePathsSingleBase(List<AbstractedLocation> _targetList, EAbstractedLocationType _locationType, string _basePath, Regex _nameMatch, string _worldName, string _gameName, bool _ignoreDuplicateNames, Mod _containingMod, string _subfolder = null)
		{
			if (!SdDirectory.Exists(_basePath))
			{
				return;
			}
			SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(_basePath);
			SdFileSystemInfo[] array;
			SdFileSystemInfo[] directories;
			if (Owner.IsFolder)
			{
				directories = sdDirectoryInfo.GetDirectories();
				array = directories;
			}
			else
			{
				directories = sdDirectoryInfo.GetFiles("*" + Owner.Extension, SearchOption.TopDirectoryOnly);
				array = directories;
			}
			directories = array;
			foreach (SdFileSystemInfo sdFileSystemInfo in directories)
			{
				if (Owner.Extension == null || sdFileSystemInfo.Name.EndsWith(Owner.Extension, StringComparison.Ordinal))
				{
					string filename = (Owner.RemoveExtension ? GameIO.RemoveExtension(sdFileSystemInfo.Name, Owner.Extension) : sdFileSystemInfo.Name);
					if ((_nameMatch == null || _nameMatch.IsMatch(filename)) && (!_ignoreDuplicateNames || !_targetList.Exists([PublicizedFrom(EAccessModifier.Internal)] (AbstractedLocation _location) => _location.Name.Equals(filename))))
					{
						_targetList.Add(new AbstractedLocation(_locationType, filename, sdFileSystemInfo.FullName, _subfolder, Owner.IsFolder, _containingMod));
					}
				}
			}
			if (!Owner.IsFolder && Owner.Recursive)
			{
				string[] directories2 = SdDirectory.GetDirectories(_basePath);
				foreach (string text in directories2)
				{
					getAvailablePathsSingleBase(_targetList, _locationType, text, _nameMatch, _worldName, _gameName, _ignoreDuplicateNames, _containingMod, Path.GetFileName(text));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool UseCache(string _worldName, string _gameName)
		{
			int num;
			if (CacheEnabled && string.IsNullOrEmpty(_worldName))
			{
				num = (string.IsNullOrEmpty(_gameName) ? 1 : 0);
				if (num != 0 && !locationsCachePopulated)
				{
					PopulateCache();
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public AbstractedLocation GetCachedLocation(string _name, bool _ignoreName = false)
		{
			if (locationsCache.Count == 0)
			{
				return AbstractedLocation.None;
			}
			if (_ignoreName)
			{
				foreach (KeyValuePair<string, IList<AbstractedLocation>> item in locationsCache)
				{
					if (item.Value.Count != 0)
					{
						return item.Value[0];
					}
				}
				return AbstractedLocation.None;
			}
			if (locationsCache.TryGetValue(Owner.RemoveExtension ? _name : (_name + Owner.Extension), out var value))
			{
				if (value.Count <= 0)
				{
					return AbstractedLocation.None;
				}
				return value[0];
			}
			return AbstractedLocation.None;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void GetCachedPathList(List<AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (locationsCache.Count == 0)
			{
				return;
			}
			foreach (KeyValuePair<string, IList<AbstractedLocation>> kvp in locationsCache)
			{
				if ((_nameMatch == null || _nameMatch.IsMatch(kvp.Key)) && (!_ignoreDuplicateNames || !_targetList.Exists([PublicizedFrom(EAccessModifier.Internal)] (AbstractedLocation _location) => _location.Name.Equals(kvp.Key))))
				{
					int num = (_ignoreDuplicateNames ? Mathf.Min(1, kvp.Value.Count) : kvp.Value.Count);
					for (int num2 = 0; num2 < num; num2++)
					{
						_targetList.Add(kvp.Value[num2]);
					}
				}
			}
		}

		public void InvalidateCache()
		{
			locationsCache.Clear();
			locationsCachePopulated = false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void PopulateCache();

		public abstract AbstractedLocation? TryBuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, string _worldName, string _gameName);

		public abstract string TryGetBasePath(EAbstractedLocationType _locationType, Mod _mod, string _worldName, string _gameName);

		public abstract bool CanHandleLocationType(EAbstractedLocationType _locationType);
	}

	public class SearchPathBasic : SearchPath
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EAbstractedLocationType locationType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<string> basePath;

		public SearchPathBasic(EAbstractedLocationType _locationType, Func<string> _basePath, string _relativePath, bool _pathIsTarget = false)
			: base(_relativePath, _pathIsTarget)
		{
			locationType = _locationType;
			basePath = _basePath;
		}

		public override AbstractedLocation GetLocation(string _name, string _worldName, string _gameName)
		{
			if (UseCache(_worldName, _gameName))
			{
				return GetCachedLocation(_name);
			}
			AbstractedLocation locationSingleBase = getLocationSingleBase(locationType, basePath() + "/" + RelativePath, _name, _worldName, _gameName, null);
			if (!locationSingleBase.Equals(AbstractedLocation.None))
			{
				return locationSingleBase;
			}
			return AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<AbstractedLocation> _targetList, Regex _nameMatch, string _worldName, string _gameName, bool _ignoreDuplicateNames)
		{
			if (UseCache(_worldName, _gameName))
			{
				GetCachedPathList(_targetList, _nameMatch, _ignoreDuplicateNames);
			}
			else
			{
				getAvailablePathsSingleBase(_targetList, locationType, basePath() + "/" + RelativePath, _nameMatch, _worldName, _gameName, _ignoreDuplicateNames, null);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			List<AbstractedLocation> list = new List<AbstractedLocation>();
			getAvailablePathsSingleBase(list, locationType, basePath() + "/" + RelativePath, null, null, null, _ignoreDuplicateNames: false, null);
			locationsCache.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				AbstractedLocation item = list[i];
				if (!locationsCache.TryGetValue(item.Name, out var value))
				{
					value = new List<AbstractedLocation>();
					locationsCache[item.Name] = value;
				}
				value.Add(item);
			}
			locationsCachePopulated = true;
		}

		public override AbstractedLocation? TryBuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, string _worldName, string _gameName)
		{
			return new AbstractedLocation(_locationType, _elementName, basePath() + "/" + RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, Owner.Extension, Owner.IsFolder);
		}

		public override string TryGetBasePath(EAbstractedLocationType _locationType, Mod _mod, string _worldName, string _gameName)
		{
			return basePath() + "/" + RelativePath;
		}

		public override bool CanHandleLocationType(EAbstractedLocationType _locationType)
		{
			return _locationType == locationType;
		}
	}

	public class SearchPathSaves : SearchPath
	{
		public override bool CanMatch => SingletonMonoBehaviour<ConnectionManager>.Instance != null;

		public SearchPathSaves(string _relativePath, bool _pathIsTarget = false)
			: base(_relativePath, _pathIsTarget)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public (string, EAbstractedLocationType) GetSaveFolder(string _worldName, string _gameName)
		{
			if (!string.IsNullOrEmpty(_worldName) && !string.IsNullOrEmpty(_gameName))
			{
				return (GameIO.GetSaveGameDir(_worldName, _gameName), EAbstractedLocationType.HostSave);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (GameIO.GetSaveGameDir(), EAbstractedLocationType.HostSave);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				return (GameIO.GetSaveGameLocalDir(), EAbstractedLocationType.LocalSave);
			}
			return (null, EAbstractedLocationType.None);
		}

		public override AbstractedLocation GetLocation(string _name, string _worldName, string _gameName)
		{
			if (UseCache(_worldName, _gameName))
			{
				return GetCachedLocation(_name, _ignoreName: true);
			}
			var (text, type) = GetSaveFolder(_worldName, _gameName);
			if (text == null)
			{
				return AbstractedLocation.None;
			}
			string text2 = text + "/" + RelativePath;
			if ((Owner.IsFolder && SdDirectory.Exists(text2)) || (!Owner.IsFolder && SdFile.Exists(text2)))
			{
				return new AbstractedLocation(type, RelativePath, text2, null, Owner.IsFolder);
			}
			return AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<AbstractedLocation> _targetList, Regex _nameMatch, string _worldName, string _gameName, bool _ignoreDuplicateNames)
		{
			if (UseCache(_worldName, _gameName))
			{
				GetCachedPathList(_targetList, null, _ignoreDuplicateNames);
				return;
			}
			var (text, type) = GetSaveFolder(_worldName, _gameName);
			if (text != null)
			{
				string text2 = text + "/" + RelativePath;
				if ((Owner.IsFolder && SdDirectory.Exists(text2)) || (!Owner.IsFolder && SdFile.Exists(text2)))
				{
					_targetList.Add(new AbstractedLocation(type, RelativePath, text2, null, Owner.IsFolder));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			var (text, type) = GetSaveFolder(null, null);
			locationsCache.Clear();
			locationsCachePopulated = true;
			if (text != null)
			{
				string text2 = text + "/" + RelativePath;
				if ((Owner.IsFolder && SdDirectory.Exists(text2)) || (!Owner.IsFolder && SdFile.Exists(text2)))
				{
					AbstractedLocation item = new AbstractedLocation(type, RelativePath, text2, null, Owner.IsFolder);
					List<AbstractedLocation> list = new List<AbstractedLocation>();
					locationsCache[item.Name] = list;
					list.Add(item);
				}
			}
		}

		public override AbstractedLocation? TryBuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, string _worldName, string _gameName)
		{
			var (text, type) = GetSaveFolder(_worldName, _gameName);
			if (text == null)
			{
				return null;
			}
			return new AbstractedLocation(type, _elementName, text + "/" + RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, Owner.Extension, Owner.IsFolder);
		}

		public override string TryGetBasePath(EAbstractedLocationType _locationType, Mod _mod, string _worldName, string _gameName)
		{
			string item = GetSaveFolder(_worldName, _gameName).Item1;
			if (item == null)
			{
				return null;
			}
			return item + "/" + RelativePath;
		}

		public override bool CanHandleLocationType(EAbstractedLocationType _locationType)
		{
			if (_locationType != EAbstractedLocationType.HostSave)
			{
				return _locationType == EAbstractedLocationType.LocalSave;
			}
			return true;
		}
	}

	public class SearchPathMods : SearchPath
	{
		public override bool CanMatch => SingletonMonoBehaviour<ConnectionManager>.Instance != null;

		public SearchPathMods(string _relativePath, bool _pathIsTarget = false)
			: base(_relativePath, _pathIsTarget)
		{
		}

		public override AbstractedLocation GetLocation(string _name, string _worldName, string _gameName)
		{
			if (UseCache(_worldName, _gameName))
			{
				return GetCachedLocation(_name);
			}
			foreach (Mod loadedMod in ModManager.GetLoadedMods())
			{
				AbstractedLocation locationSingleBase = getLocationSingleBase(EAbstractedLocationType.Mods, loadedMod.Path + "/" + RelativePath, _name, _worldName, _gameName, loadedMod);
				if (!locationSingleBase.Equals(AbstractedLocation.None))
				{
					return locationSingleBase;
				}
			}
			return AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<AbstractedLocation> _targetList, Regex _nameMatch, string _worldName, string _gameName, bool _ignoreDuplicateNames)
		{
			if (UseCache(_worldName, _gameName))
			{
				GetCachedPathList(_targetList, _nameMatch, _ignoreDuplicateNames);
				return;
			}
			foreach (Mod loadedMod in ModManager.GetLoadedMods())
			{
				getAvailablePathsSingleBase(_targetList, EAbstractedLocationType.Mods, loadedMod.Path + "/" + RelativePath, _nameMatch, _worldName, _gameName, _ignoreDuplicateNames, loadedMod);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			List<AbstractedLocation> list = new List<AbstractedLocation>();
			foreach (Mod loadedMod in ModManager.GetLoadedMods())
			{
				getAvailablePathsSingleBase(list, EAbstractedLocationType.Mods, loadedMod.Path + "/" + RelativePath, null, null, null, _ignoreDuplicateNames: false, loadedMod);
			}
			locationsCache.Clear();
			foreach (AbstractedLocation item in list)
			{
				if (!locationsCache.TryGetValue(item.Name, out var value))
				{
					value = new List<AbstractedLocation>();
					locationsCache[item.Name] = value;
				}
				value.Add(item);
			}
			locationsCachePopulated = true;
		}

		public override AbstractedLocation? TryBuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, string _worldName, string _gameName)
		{
			if (_mod == null)
			{
				throw new ArgumentException("BuildLocation for LocationType==Mods requires a loaded Mod", "_mod");
			}
			return new AbstractedLocation(_locationType, _elementName, _mod.Path + "/" + RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, Owner.Extension, Owner.IsFolder, _mod);
		}

		public override string TryGetBasePath(EAbstractedLocationType _locationType, Mod _mod, string _worldName, string _gameName)
		{
			if (_mod == null)
			{
				throw new ArgumentException("GetBasePath for LocationType==Mods requires a loaded Mod", "_mod");
			}
			return _mod.Path + "/" + RelativePath;
		}

		public override bool CanHandleLocationType(EAbstractedLocationType _locationType)
		{
			return _locationType == EAbstractedLocationType.Mods;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SearchDefinition> allSearchDefs = new List<SearchDefinition>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Func<string> userDataPath = GameIO.GetUserGameDataDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Func<string> gameDataPath = GameIO.GetApplicationPath;

	public static readonly SearchDefinition WorldsSearchPaths = new SearchDefinition(true, null, false, false, new SearchPathSaves("World", _pathIsTarget: true), new SearchPathBasic(EAbstractedLocationType.UserDataPath, userDataPath, "GeneratedWorlds"), new SearchPathMods("Worlds"), new SearchPathBasic(EAbstractedLocationType.GameData, gameDataPath, "Data/Worlds"));

	public static readonly SearchDefinition PrefabsSearchPaths = new SearchDefinition(false, ".tts", true, true, new SearchPathBasic(EAbstractedLocationType.UserDataPath, userDataPath, "LocalPrefabs"), new SearchPathMods("Prefabs"), new SearchPathBasic(EAbstractedLocationType.GameData, gameDataPath, "Data/Prefabs"));

	public static readonly SearchDefinition PrefabImpostersSearchPaths = new SearchDefinition(false, ".mesh", false, true, new SearchPathBasic(EAbstractedLocationType.UserDataPath, userDataPath, "LocalPrefabs"), new SearchPathMods("Prefabs"), new SearchPathBasic(EAbstractedLocationType.GameData, gameDataPath, "Data/Prefabs"));

	public static readonly SearchDefinition RwgStampsSearchPaths = new SearchDefinition(false, "", true, true, new SearchPathBasic(EAbstractedLocationType.UserDataPath, userDataPath, "LocalStamps"), new SearchPathMods("Stamps"), new SearchPathBasic(EAbstractedLocationType.GameData, gameDataPath, "Data/Stamps"));

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool cacheEnabled;

	public static bool CacheEnabled
	{
		get
		{
			return cacheEnabled;
		}
		set
		{
			cacheEnabled = value;
			if (!value)
			{
				InvalidateCaches();
			}
		}
	}

	public static void InvalidateCaches()
	{
		foreach (SearchDefinition allSearchDef in allSearchDefs)
		{
			allSearchDef.InvalidateCache();
		}
	}
}

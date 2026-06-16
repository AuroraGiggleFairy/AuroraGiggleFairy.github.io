using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Platform;

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

		public readonly UserDataStorageType StorageType;

		public string FullPath => Folder + "/" + FileNameNoExtension + Extension;

		public string FullPathNoExtension => Folder + "/" + FileNameNoExtension;

		public AbstractedLocation(EAbstractedLocationType _type, string _name, string _fullPath, string _relativePath, bool _isFolder, Mod _containingMod = null)
		{
			_fullPath = ((_fullPath != null) ? Path.GetFullPath(_fullPath).Replace("\\", "/") : null);
			Type = _type;
			Name = Path.GetFileName(_name);
			Folder = Path.GetDirectoryName(_fullPath)?.Replace("\\", "/");
			RelativePath = _relativePath;
			FileNameNoExtension = Path.GetFileNameWithoutExtension(_fullPath);
			Extension = Path.GetExtension(_fullPath);
			Extension = (string.IsNullOrEmpty(Extension) ? null : Extension);
			IsFolder = _isFolder;
			ContainingMod = _containingMod;
			StorageType = ((Folder != null && GameIO.IsRoamingUserDataPath(Folder)) ? UserDataStorageType.Roaming : UserDataStorageType.DeviceLocal);
		}

		public AbstractedLocation(EAbstractedLocationType _type, string _name, string _folder, string _relativePath, string _fileNameNoExtension, string _extension, bool _isFolder, Mod _containingMod = null)
		{
			Type = _type;
			Name = _name;
			Folder = ((_folder != null) ? Path.GetFullPath(_folder).Replace("\\", "/") : null);
			RelativePath = _relativePath;
			FileNameNoExtension = _fileNameNoExtension;
			Extension = _extension;
			IsFolder = _isFolder;
			ContainingMod = _containingMod;
			StorageType = ((Folder != null && GameIO.IsRoamingUserDataPath(Folder)) ? UserDataStorageType.Roaming : UserDataStorageType.DeviceLocal);
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
			return Name + " (src: " + Type.ToStringCached() + ", " + StorageType.ToStringCached() + ")";
		}

		public string GetLocationTypeDisplayString()
		{
			switch (Type)
			{
			case EAbstractedLocationType.Mods:
				return "Mod: " + ContainingMod.Name;
			case EAbstractedLocationType.UserDataPath:
				if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional)
				{
					return "User Data: " + StorageType.LocalizedName();
				}
				return "User Data";
			case EAbstractedLocationType.GameData:
				return "Vanilla";
			default:
				return Type.ToStringCached();
			}
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

		public AbstractedLocation GetLocation(string _name, EAbstractedLocationType? _onlyLocationType = null, UserDataStorageType? _userDataHint = null)
		{
			foreach (SearchPath path in paths)
			{
				if (path.CanMatch && (!_onlyLocationType.HasValue || path.CanHandleLocationType(_onlyLocationType.Value)))
				{
					AbstractedLocation location = path.GetLocation(_name, _userDataHint);
					if (location.Type != EAbstractedLocationType.None)
					{
						return location;
					}
				}
			}
			return new AbstractedLocation(EAbstractedLocationType.None, _name, null, null, IsFolder);
		}

		public List<AbstractedLocation> GetAvailablePathsList(Regex _nameMatch = null, bool _ignoreDuplicateNames = false, EAbstractedLocationType? _onlyLocationType = null)
		{
			List<AbstractedLocation> _resultList = new List<AbstractedLocation>();
			GetAvailablePathsList(ref _resultList, _nameMatch, _ignoreDuplicateNames, _onlyLocationType);
			return _resultList;
		}

		public void GetAvailablePathsList(ref List<AbstractedLocation> _resultList, Regex _nameMatch = null, bool _ignoreDuplicateNames = false, EAbstractedLocationType? _onlyLocationType = null)
		{
			if (_resultList == null)
			{
				_resultList = new List<AbstractedLocation>();
			}
			foreach (SearchPath path in paths)
			{
				if (path.CanMatch && (!_onlyLocationType.HasValue || path.CanHandleLocationType(_onlyLocationType.Value)))
				{
					path.GetAvailablePathsList(_resultList, _nameMatch, _ignoreDuplicateNames);
				}
			}
		}

		public AbstractedLocation? BuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, string _modName = null, UserDataStorageType? _userStorageHint = null)
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
			return BuildLocation(_locationType, _subFolder, _elementName, mod, _userStorageHint);
		}

		public AbstractedLocation? BuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod = null, UserDataStorageType? _userStorageHint = null)
		{
			foreach (SearchPath path in paths)
			{
				if (path.CanHandleLocationType(_locationType))
				{
					return path.TryBuildLocation(_locationType, _subFolder, _elementName, _mod, _userStorageHint);
				}
			}
			return null;
		}

		public string GetBasePath(EAbstractedLocationType _locationType, string _modName = null)
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
			return GetBasePath(_locationType, mod);
		}

		public string GetBasePath(EAbstractedLocationType _locationType, Mod _mod = null)
		{
			foreach (SearchPath path in paths)
			{
				if (path.CanHandleLocationType(_locationType))
				{
					return path.TryGetBasePath(_locationType, _mod);
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
		public readonly Dictionary<string, IList<AbstractedLocation>> locationsCache = new CaseInsensitiveStringDictionary<IList<AbstractedLocation>>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool locationsCachePopulated;

		public virtual bool CanMatch => true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public SearchPath(string _relativePath)
		{
			RelativePath = _relativePath;
		}

		public void SetOwner(SearchDefinition _owner)
		{
			Owner = _owner;
		}

		public abstract AbstractedLocation GetLocation(string _name, UserDataStorageType? _userDataHint);

		public abstract void GetAvailablePathsList(List<AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames);

		[PublicizedFrom(EAccessModifier.Protected)]
		public AbstractedLocation getLocationSingleBase(EAbstractedLocationType _locationType, string _basePath, string _name, Mod _containingMod, string _subfolder = null)
		{
			UseCache();
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
						AbstractedLocation locationSingleBase = getLocationSingleBase(_locationType, text2, _name, _containingMod, Path.GetFileName(text2));
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
		public void getAvailablePathsSingleBase(List<AbstractedLocation> _targetList, EAbstractedLocationType _locationType, string _basePath, Regex _nameMatch, bool _ignoreDuplicateNames, Mod _containingMod, string _subfolder = null)
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
					getAvailablePathsSingleBase(_targetList, _locationType, text, _nameMatch, _ignoreDuplicateNames, _containingMod, Path.GetFileName(text));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool UseCache()
		{
			bool cacheEnabled = CacheEnabled;
			if (cacheEnabled && !locationsCachePopulated)
			{
				PopulateCache();
			}
			return cacheEnabled;
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
			foreach (KeyValuePair<string, IList<AbstractedLocation>> item in locationsCache)
			{
				foreach (AbstractedLocation loc in item.Value)
				{
					if ((_nameMatch == null || _nameMatch.IsMatch(loc.Name)) && (!_ignoreDuplicateNames || !_targetList.Exists([PublicizedFrom(EAccessModifier.Internal)] (AbstractedLocation location) => location.Name.Equals(loc.Name))))
					{
						_targetList.Add(loc);
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

		public abstract AbstractedLocation? TryBuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, UserDataStorageType? _userStorageHint);

		public abstract string TryGetBasePath(EAbstractedLocationType _locationType, Mod _mod);

		public abstract bool CanHandleLocationType(EAbstractedLocationType _locationType);
	}

	public class SearchPathBasic : SearchPath
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EAbstractedLocationType locationType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<string> basePath;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<bool> canMatch;

		public override bool CanMatch => canMatch?.Invoke() ?? base.CanMatch;

		public SearchPathBasic(EAbstractedLocationType _locationType, Func<string> _basePath, string _relativePath, Func<bool> _canMatch = null)
			: base(_relativePath)
		{
			locationType = _locationType;
			basePath = _basePath;
			canMatch = _canMatch;
		}

		public override AbstractedLocation GetLocation(string _name, UserDataStorageType? _userDataHint)
		{
			if (UseCache())
			{
				return GetCachedLocation(_name);
			}
			AbstractedLocation locationSingleBase = getLocationSingleBase(locationType, basePath() + "/" + RelativePath, _name, null);
			if (!locationSingleBase.Equals(AbstractedLocation.None))
			{
				return locationSingleBase;
			}
			return AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (UseCache())
			{
				GetCachedPathList(_targetList, _nameMatch, _ignoreDuplicateNames);
			}
			else
			{
				getAvailablePathsSingleBase(_targetList, locationType, basePath() + "/" + RelativePath, _nameMatch, _ignoreDuplicateNames, null);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			List<AbstractedLocation> list = new List<AbstractedLocation>();
			getAvailablePathsSingleBase(list, locationType, basePath() + "/" + RelativePath, null, _ignoreDuplicateNames: false, null);
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

		public override AbstractedLocation? TryBuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, UserDataStorageType? _userStorageHint = null)
		{
			return new AbstractedLocation(_locationType, _elementName, basePath() + "/" + RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, Owner.Extension, Owner.IsFolder);
		}

		public override string TryGetBasePath(EAbstractedLocationType _locationType, Mod _mod)
		{
			return basePath() + "/" + RelativePath;
		}

		public override bool CanHandleLocationType(EAbstractedLocationType _locationType)
		{
			return _locationType == locationType;
		}
	}

	public class SearchPathUserData : SearchPath
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<UserDataStorageType> getStorageTarget;

		public SearchPathUserData(string _relativePath, Func<UserDataStorageType> _getStorageTarget)
			: base(_relativePath)
		{
			getStorageTarget = _getStorageTarget;
		}

		public override AbstractedLocation GetLocation(string _name, UserDataStorageType? _userDataHint)
		{
			UserDataStorageType userDataStorageType = (_userDataHint.HasValue ? _userDataHint.Value : getStorageTarget());
			if (UseCache())
			{
				return GetCachedLocation(GetCacheLookupName(_name, userDataStorageType));
			}
			string basePath = Path.Combine(GameIO.GetUserGameDataDir(userDataStorageType), RelativePath);
			return getLocationSingleBase(EAbstractedLocationType.UserDataPath, basePath, _name, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CollectLocations(List<AbstractedLocation> _list, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			string basePath = Path.Combine(GameIO.GetUserGameDataDir(UserDataStorageType.DeviceLocal), RelativePath);
			getAvailablePathsSingleBase(_list, EAbstractedLocationType.UserDataPath, basePath, _nameMatch, _ignoreDuplicateNames, null);
			if (PlatformManager.MultiPlatform.UserDataRoaming.IsSupported)
			{
				string basePath2 = Path.Combine(GameIO.GetUserGameDataDir(UserDataStorageType.Roaming), RelativePath);
				getAvailablePathsSingleBase(_list, EAbstractedLocationType.UserDataPath, basePath2, _nameMatch, _ignoreDuplicateNames, null);
			}
		}

		public override void GetAvailablePathsList(List<AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (UseCache())
			{
				GetCachedPathList(_targetList, _nameMatch, _ignoreDuplicateNames);
			}
			else
			{
				CollectLocations(_targetList, _nameMatch, _ignoreDuplicateNames);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string GetCacheLookupName(string _name, UserDataStorageType _storageType)
		{
			return $"{_name}_{_storageType}";
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			List<AbstractedLocation> list = new List<AbstractedLocation>();
			CollectLocations(list, null, _ignoreDuplicateNames: false);
			locationsCache.Clear();
			foreach (AbstractedLocation item in list)
			{
				string cacheLookupName = GetCacheLookupName(item.Name, item.StorageType);
				if (!locationsCache.TryGetValue(cacheLookupName, out var value))
				{
					value = new List<AbstractedLocation>();
					locationsCache.Add(cacheLookupName, value);
				}
				value.Add(item);
			}
			locationsCachePopulated = true;
		}

		public override AbstractedLocation? TryBuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, UserDataStorageType? _userStorageHint = null)
		{
			string userGameDataDir = GameIO.GetUserGameDataDir(_userStorageHint.HasValue ? _userStorageHint.Value : getStorageTarget());
			return new AbstractedLocation(_locationType, _elementName, userGameDataDir + "/" + RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, Owner.Extension, Owner.IsFolder);
		}

		public override string TryGetBasePath(EAbstractedLocationType _locationType, Mod _mod)
		{
			return GameIO.GetUserGameDataDir(getStorageTarget()) + "/" + RelativePath;
		}

		public override bool CanHandleLocationType(EAbstractedLocationType _locationType)
		{
			return _locationType == EAbstractedLocationType.UserDataPath;
		}
	}

	public class SearchPathSaves : SearchPath
	{
		public override bool CanMatch => SingletonMonoBehaviour<ConnectionManager>.Instance != null;

		public SearchPathSaves(string _relativePath)
			: base(_relativePath)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public (string, EAbstractedLocationType) GetSaveFolder()
		{
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

		public override AbstractedLocation GetLocation(string _name, UserDataStorageType? _userDataHint)
		{
			if (UseCache())
			{
				return GetCachedLocation(_name, _ignoreName: true);
			}
			var (text, type) = GetSaveFolder();
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

		public override void GetAvailablePathsList(List<AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (UseCache())
			{
				GetCachedPathList(_targetList, null, _ignoreDuplicateNames);
				return;
			}
			var (text, type) = GetSaveFolder();
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
			var (text, type) = GetSaveFolder();
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

		public override AbstractedLocation? TryBuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, UserDataStorageType? _userStorageHint = null)
		{
			var (text, type) = GetSaveFolder();
			if (text == null)
			{
				return null;
			}
			return new AbstractedLocation(type, _elementName, text + "/" + RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, Owner.Extension, Owner.IsFolder);
		}

		public override string TryGetBasePath(EAbstractedLocationType _locationType, Mod _mod)
		{
			string item = GetSaveFolder().Item1;
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

		public SearchPathMods(string _relativePath)
			: base(_relativePath)
		{
		}

		public override AbstractedLocation GetLocation(string _name, UserDataStorageType? _userDataHint)
		{
			if (UseCache())
			{
				return GetCachedLocation(_name);
			}
			foreach (Mod loadedMod in ModManager.GetLoadedMods())
			{
				AbstractedLocation locationSingleBase = getLocationSingleBase(EAbstractedLocationType.Mods, loadedMod.Path + "/" + RelativePath, _name, loadedMod);
				if (!locationSingleBase.Equals(AbstractedLocation.None))
				{
					return locationSingleBase;
				}
			}
			return AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (UseCache())
			{
				GetCachedPathList(_targetList, _nameMatch, _ignoreDuplicateNames);
				return;
			}
			foreach (Mod loadedMod in ModManager.GetLoadedMods())
			{
				getAvailablePathsSingleBase(_targetList, EAbstractedLocationType.Mods, loadedMod.Path + "/" + RelativePath, _nameMatch, _ignoreDuplicateNames, loadedMod);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			List<AbstractedLocation> list = new List<AbstractedLocation>();
			foreach (Mod loadedMod in ModManager.GetLoadedMods())
			{
				getAvailablePathsSingleBase(list, EAbstractedLocationType.Mods, loadedMod.Path + "/" + RelativePath, null, _ignoreDuplicateNames: false, loadedMod);
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

		public override AbstractedLocation? TryBuildLocation(EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, UserDataStorageType? _userStorageHint = null)
		{
			if (_mod == null)
			{
				throw new ArgumentException("BuildLocation for LocationType==Mods requires a loaded Mod", "_mod");
			}
			return new AbstractedLocation(_locationType, _elementName, _mod.Path + "/" + RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, Owner.Extension, Owner.IsFolder, _mod);
		}

		public override string TryGetBasePath(EAbstractedLocationType _locationType, Mod _mod)
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

	public static class Contextual
	{
		public static AbstractedLocation FindDownloadedRemoteWorld(string _saveDir)
		{
			string text = Path.Combine(_saveDir, "World");
			if (!SdDirectory.Exists(text))
			{
				return AbstractedLocation.None;
			}
			return new AbstractedLocation(EAbstractedLocationType.LocalSave, "World", text, null, _isFolder: true);
		}

		public static void FindActiveWorld(out string _name, out AbstractedLocation _location)
		{
			_name = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			EAbstractedLocationType locationTypeHint = (EAbstractedLocationType)GamePrefs.GetInt(EnumGamePrefs.GameWorldLocationType);
			UserDataStorageType storageTypeHint = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.UserWorldStorageType);
			_location = FindWorld(_name, locationTypeHint, storageTypeHint);
		}

		public static AbstractedLocation FindActiveWorldLocation()
		{
			return FindWorld(GamePrefs.GetString(EnumGamePrefs.GameWorld), (EAbstractedLocationType)GamePrefs.GetInt(EnumGamePrefs.GameWorldLocationType), (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.UserWorldStorageType));
		}

		public static bool DoesWorldExist(string _name)
		{
			UserDataStorageType? userDataHint;
			if (PlatformManager.MultiPlatform.UserDataRoaming.SaveRoamingEnabled)
			{
				SearchDefinition worldsSearchPaths = WorldsSearchPaths;
				userDataHint = UserDataStorageType.Roaming;
				if (worldsSearchPaths.GetLocation(_name, null, userDataHint).Exists())
				{
					return true;
				}
			}
			SearchDefinition worldsSearchPaths2 = WorldsSearchPaths;
			userDataHint = UserDataStorageType.DeviceLocal;
			return worldsSearchPaths2.GetLocation(_name, null, userDataHint).Exists();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static AbstractedLocation FindWorld(string _name, EAbstractedLocationType _locationTypeHint, UserDataStorageType _storageTypeHint)
		{
			if (_locationTypeHint == EAbstractedLocationType.None)
			{
				SearchDefinition worldsSearchPaths = WorldsSearchPaths;
				UserDataStorageType? userDataHint = _storageTypeHint;
				return worldsSearchPaths.GetLocation(_name, null, userDataHint);
			}
			return WorldsSearchPaths.GetLocation(_name, _locationTypeHint, _storageTypeHint);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SearchDefinition> allSearchDefs = new List<SearchDefinition>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Func<string> deviceLocalUserDataPath = GameIO.GetDeviceLocalUserGameDataDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Func<string> gameDataPath = GameIO.GetApplicationPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Func<UserDataStorageType> getWorldsStorageType = [PublicizedFrom(EAccessModifier.Internal)] () => (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.UserWorldStorageType);

	public static readonly SearchDefinition WorldsSearchPaths = new SearchDefinition(true, null, false, false, new SearchPathSaves("World"), new SearchPathUserData("GeneratedWorlds", getWorldsStorageType), new SearchPathMods("Worlds"), new SearchPathBasic(EAbstractedLocationType.GameData, gameDataPath, "Data/Worlds"));

	public static readonly SearchDefinition PrefabsSearchPaths = new SearchDefinition(false, ".tts", true, true, new SearchPathBasic(EAbstractedLocationType.UserDataPath, deviceLocalUserDataPath, "LocalPrefabs"), new SearchPathMods("Prefabs"), new SearchPathBasic(EAbstractedLocationType.GameData, gameDataPath, "Data/Prefabs"));

	public static readonly SearchDefinition PrefabImpostersSearchPaths = new SearchDefinition(false, ".mesh", false, true, new SearchPathBasic(EAbstractedLocationType.UserDataPath, deviceLocalUserDataPath, "LocalPrefabs"), new SearchPathMods("Prefabs"), new SearchPathBasic(EAbstractedLocationType.GameData, gameDataPath, "Data/Prefabs"));

	public static readonly SearchDefinition RwgStampsSearchPaths = new SearchDefinition(false, "", true, true, new SearchPathBasic(EAbstractedLocationType.UserDataPath, deviceLocalUserDataPath, "LocalStamps"), new SearchPathMods("Stamps"), new SearchPathBasic(EAbstractedLocationType.GameData, gameDataPath, "Data/Stamps"));

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

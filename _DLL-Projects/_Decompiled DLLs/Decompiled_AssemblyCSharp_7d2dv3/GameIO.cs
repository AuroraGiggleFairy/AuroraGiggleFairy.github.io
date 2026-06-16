using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Platform;
using UnityEngine;

public static class GameIO
{
	public delegate void FoundSave(UserDataStorageType storage, string saveName, string worldName, DateTime lastSaved, WorldState worldState, bool isArchived);

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_ApplicationScratchPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string XB1ScratchPath = "D:";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PS4ScratchPath = "/hostapp";

	[PublicizedFrom(EAccessModifier.Private)]
	public static RuntimePlatform m_UnityRuntimePlatform;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_UnityDataPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_ApplicationPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] pathTrimCharacters;

	public static readonly char[] ResourcePathSeparators;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_lastDeviceLocalRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<(UserDataStorageType, string), string> m_cachedUserDataPaths;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Regex m_isRoamingPathRegex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex linuxOsReleaseMatcher;

	[PublicizedFrom(EAccessModifier.Private)]
	static GameIO()
	{
		pathTrimCharacters = new char[2] { '/', '\\' };
		ResourcePathSeparators = new char[3] { '/', '\\', '?' };
		linuxOsReleaseMatcher = new Regex("^ID=['\"]?([^'\"]+)['\"]?$");
		m_UnityDataPath = Application.dataPath;
		m_UnityRuntimePlatform = Application.platform;
	}

	public static SdFileInfo[] GetDirectory(string _path, string _pattern)
	{
		if (!SdDirectory.Exists(_path))
		{
			return Array.Empty<SdFileInfo>();
		}
		return new SdDirectoryInfo(_path).GetFiles(_pattern);
	}

	public static long FileSize(string _filePath)
	{
		SdFileInfo sdFileInfo = new SdFileInfo(_filePath);
		if (!sdFileInfo.Exists)
		{
			return -1L;
		}
		return sdFileInfo.Length;
	}

	public static string GetNormalizedPath(string _path)
	{
		return Path.GetFullPath(_path).TrimEnd(pathTrimCharacters);
	}

	public static bool PathsEquals(string _path1, string _path2, bool _ignoreCase)
	{
		return string.Equals(GetNormalizedPath(_path1), GetNormalizedPath(_path2), _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	public static string GetFileExtension(string _filename)
	{
		int startIndex;
		if (_filename.Length > 4 && (startIndex = _filename.LastIndexOf('.')) > 0)
		{
			return _filename.Substring(startIndex);
		}
		return _filename;
	}

	public static string RemoveFileExtension(string _filename)
	{
		int length;
		if (_filename.Length > 4 && (length = _filename.LastIndexOf('.')) > 0)
		{
			return _filename.Substring(0, length);
		}
		return _filename;
	}

	public static string RemoveExtension(string _filename, string _extension)
	{
		if (_filename.Length > _extension.Length && _filename.EndsWith(_extension, StringComparison.InvariantCultureIgnoreCase))
		{
			return _filename.Substring(0, _filename.Length - _extension.Length);
		}
		return _filename;
	}

	public static string GetFilenameFromPath(string _filepath)
	{
		int num = _filepath.LastIndexOfAny(ResourcePathSeparators);
		if (num >= 0 && num < _filepath.Length)
		{
			_filepath = _filepath.Substring(num + 1);
		}
		return _filepath;
	}

	public static string GetFilenameFromPathWithoutExtension(string _filepath)
	{
		int num = _filepath.LastIndexOfAny(ResourcePathSeparators);
		int num2 = _filepath.LastIndexOf('.');
		if (num >= 0 && num2 < num)
		{
			num2 = -1;
		}
		if (num >= 0 && num2 >= 0)
		{
			return _filepath.Substring(num + 1, num2 - num - 1);
		}
		if (num >= 0)
		{
			return _filepath.Substring(num + 1);
		}
		if (num2 >= 0)
		{
			return _filepath.Substring(0, num2);
		}
		return _filepath;
	}

	public static string GetDirectoryFromPath(string _filepath)
	{
		int num = _filepath.LastIndexOf('/');
		if (num > 0 && num < _filepath.Length)
		{
			_filepath = _filepath.Substring(0, num);
		}
		return _filepath;
	}

	public static long GetDirectorySize(string _filepath, bool recursive = true)
	{
		return GetDirectorySize(new SdDirectoryInfo(_filepath), recursive);
	}

	public static long GetDirectorySize(SdDirectoryInfo directoryInfo, bool recursive = true)
	{
		long num = 0L;
		if (directoryInfo == null || !directoryInfo.Exists)
		{
			return num;
		}
		SdFileInfo[] files = directoryInfo.GetFiles();
		foreach (SdFileInfo sdFileInfo in files)
		{
			num += sdFileInfo.Length;
		}
		if (recursive)
		{
			SdDirectoryInfo[] directories = directoryInfo.GetDirectories();
			foreach (SdDirectoryInfo directoryInfo2 in directories)
			{
				num += GetDirectorySize(directoryInfo2, recursive);
			}
		}
		return num;
	}

	public static string GetGameDir(string _relDir)
	{
		return GetApplicationPath() + "/" + _relDir;
	}

	public static string GetApplicationPath()
	{
		if (m_ApplicationPath == null)
		{
			string text = m_UnityDataPath;
			switch (m_UnityRuntimePlatform)
			{
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsServer:
			case RuntimePlatform.OSXServer:
				text += "/..";
				break;
			default:
				text += "/..";
				break;
			case RuntimePlatform.PS4:
			case RuntimePlatform.PS5:
				break;
			}
			m_ApplicationPath = text;
		}
		return m_ApplicationPath;
	}

	public static string GetGamePath()
	{
		if (m_UnityRuntimePlatform != RuntimePlatform.OSXPlayer && m_UnityRuntimePlatform != RuntimePlatform.OSXServer)
		{
			return m_UnityDataPath + "/..";
		}
		return m_UnityDataPath + "/../..";
	}

	public static string GetGameExecutablePath()
	{
		return GetGamePath() + "/" + GetGameExecutableName();
	}

	public static string GetGameExecutableName()
	{
		return m_UnityRuntimePlatform switch
		{
			RuntimePlatform.OSXEditor => "7DaysToDie.app", 
			RuntimePlatform.OSXPlayer => "7DaysToDie.app", 
			RuntimePlatform.WindowsPlayer => "7DaysToDie.exe", 
			RuntimePlatform.WindowsEditor => "7DaysToDie.exe", 
			RuntimePlatform.LinuxPlayer => "7DaysToDie.x86_64", 
			RuntimePlatform.LinuxEditor => "7DaysToDie.x86_64", 
			RuntimePlatform.XboxOne => throw new ArgumentException("Platform " + m_UnityRuntimePlatform.ToStringCached() + " currently not supported", "m_UnityRuntimePlatform"), 
			RuntimePlatform.GameCoreXboxSeries => throw new ArgumentException("Platform " + m_UnityRuntimePlatform.ToStringCached() + " currently not supported", "m_UnityRuntimePlatform"), 
			RuntimePlatform.GameCoreXboxOne => throw new ArgumentException("Platform " + m_UnityRuntimePlatform.ToStringCached() + " currently not supported", "m_UnityRuntimePlatform"), 
			RuntimePlatform.PS5 => throw new ArgumentException("Platform " + m_UnityRuntimePlatform.ToStringCached() + " currently not supported", "m_UnityRuntimePlatform"), 
			_ => throw new ArgumentOutOfRangeException("m_UnityRuntimePlatform"), 
		};
	}

	public static string GetLauncherExecutablePath()
	{
		return GetGamePath() + "/" + GetLauncherExecutableName();
	}

	public static string GetLauncherExecutableName()
	{
		return m_UnityRuntimePlatform switch
		{
			RuntimePlatform.OSXEditor => "7dLauncher.app", 
			RuntimePlatform.OSXPlayer => "7dLauncher.app", 
			RuntimePlatform.WindowsPlayer => "7dLauncher.exe", 
			RuntimePlatform.WindowsEditor => "7dLauncher.exe", 
			RuntimePlatform.LinuxPlayer => "7DaysToDie.sh", 
			RuntimePlatform.LinuxEditor => "7DaysToDie.sh", 
			RuntimePlatform.XboxOne => throw new ArgumentException("Platform " + m_UnityRuntimePlatform.ToStringCached() + " currently not supported", "m_UnityRuntimePlatform"), 
			RuntimePlatform.GameCoreXboxSeries => throw new ArgumentException("Platform " + m_UnityRuntimePlatform.ToStringCached() + " currently not supported", "m_UnityRuntimePlatform"), 
			RuntimePlatform.GameCoreXboxOne => throw new ArgumentException("Platform " + m_UnityRuntimePlatform.ToStringCached() + " currently not supported", "m_UnityRuntimePlatform"), 
			RuntimePlatform.PS5 => throw new ArgumentException("Platform " + m_UnityRuntimePlatform.ToStringCached() + " currently not supported", "m_UnityRuntimePlatform"), 
			_ => throw new ArgumentOutOfRangeException("m_UnityRuntimePlatform"), 
		};
	}

	public static string GetApplicationScratchPath()
	{
		if (m_ApplicationScratchPath == null)
		{
			switch (m_UnityRuntimePlatform)
			{
			case RuntimePlatform.XboxOne:
				m_ApplicationScratchPath = "D:";
				break;
			case RuntimePlatform.GameCoreXboxSeries:
			case RuntimePlatform.GameCoreXboxOne:
				m_ApplicationScratchPath = "D:";
				break;
			case RuntimePlatform.PS4:
			case RuntimePlatform.PS5:
				m_ApplicationScratchPath = "/hostapp";
				break;
			default:
				m_ApplicationScratchPath = GetApplicationPath();
				break;
			}
		}
		return m_ApplicationScratchPath;
	}

	public static string GetApplicationTempPath()
	{
		return PlatformApplicationManager.Application.temporaryCachePath;
	}

	public static IEnumerator PrecacheFile(string _path, int _doYieldEveryMs = -1, Action<float, long, long> _statusUpdateHandler = null)
	{
		if (!SdFile.Exists(_path))
		{
			Log.Error("File does not exist: " + _path);
			yield break;
		}
		Stream fs;
		try
		{
			fs = SdFile.OpenRead(_path);
		}
		catch (Exception e)
		{
			Log.Error("Precaching file failed");
			Log.Exception(e);
			yield break;
		}
		byte[] buf = new byte[16384];
		MicroStopwatch msw = new MicroStopwatch();
		int num;
		do
		{
			if (_doYieldEveryMs > 0 && msw.ElapsedMilliseconds >= _doYieldEveryMs)
			{
				_statusUpdateHandler?.Invoke((float)fs.Position / (float)fs.Length, fs.Position, fs.Length);
				yield return null;
				msw.ResetAndRestart();
			}
			try
			{
				num = fs.Read(buf, 0, buf.Length);
			}
			catch (Exception e2)
			{
				Log.Error("Precaching file failed");
				Log.Exception(e2);
				try
				{
					fs.Dispose();
					yield break;
				}
				catch (Exception e3)
				{
					Log.Error("Failed disposing filestream");
					Log.Exception(e3);
					yield break;
				}
			}
		}
		while (num > 0);
		fs.Dispose();
	}

	public static string GetDocumentPath()
	{
		switch (m_UnityRuntimePlatform)
		{
		case RuntimePlatform.LinuxPlayer:
		case RuntimePlatform.LinuxEditor:
		case RuntimePlatform.LinuxServer:
			return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
		case RuntimePlatform.WindowsServer:
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
		case RuntimePlatform.OSXServer:
			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Library/Application Support";
		case RuntimePlatform.XboxOne:
			UnityEngine.Debug.LogWarning("XboxOne: Platform Document Path is currently not set");
			return null;
		case RuntimePlatform.GameCoreXboxSeries:
		case RuntimePlatform.GameCoreXboxOne:
			return Application.persistentDataPath;
		case RuntimePlatform.PS4:
			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		case RuntimePlatform.PS5:
			return "/download0";
		default:
			return null;
		}
	}

	public static string GetDefaultPersistentDataPath()
	{
		return GetDocumentPath() + "/" + "7 Days To Die".Replace(" ", "");
	}

	public static void InitializeUserDataPaths(string _rootPathOverride = null)
	{
		string text = GetDefaultPersistentDataPath();
		if (!string.IsNullOrEmpty(_rootPathOverride))
		{
			text = MakeAbsolutePath(_rootPathOverride);
			Log.Out("Overriding default user data path to " + text);
		}
		if (m_lastDeviceLocalRoot == null || !m_lastDeviceLocalRoot.Equals(text))
		{
			m_lastDeviceLocalRoot = text;
			text = Path.GetFullPath(text);
			string text2 = text + "Roaming";
			string text3 = Regex.Replace(text2, "[\\\\/]", "[\\\\/]", RegexOptions.Singleline | RegexOptions.CultureInvariant);
			m_isRoamingPathRegex = new Regex("^(?:" + text3 + ")", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
			m_cachedUserDataPaths = new Dictionary<(UserDataStorageType, string), string>();
			PopulateCache(UserDataStorageType.DeviceLocal, text);
			PopulateCache(UserDataStorageType.Roaming, text2);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void PopulateCache(UserDataStorageType _storage, string _rootPath)
		{
			m_cachedUserDataPaths.Add((_storage, ""), _rootPath);
			m_cachedUserDataPaths.Add((_storage, "Saves"), Path.Combine(_rootPath, "Saves"));
			m_cachedUserDataPaths.Add((_storage, "SavesLocal"), Path.Combine(_rootPath, "SavesLocal"));
		}
	}

	public static bool IsRoamingUserDataPath(string _path)
	{
		if (m_isRoamingPathRegex == null)
		{
			throw new Exception("IsRoamingUserDataPath used before InitializeUserDataPaths");
		}
		return m_isRoamingPathRegex.IsMatch(_path);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetCachedUserDataPath(UserDataStorageType _storage, string _folder = "")
	{
		if (m_cachedUserDataPaths == null)
		{
			throw new Exception("GetCachedUserDataPath used before InitializeUserDataPaths");
		}
		return m_cachedUserDataPaths[(_storage, _folder)];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetDefaultUserGameDataPath(string _folder = "")
	{
		if (PlatformManager.MultiPlatform == null)
		{
			string cachedUserDataPath = GetCachedUserDataPath(UserDataStorageType.DeviceLocal, _folder);
			Log.Error("Trying to access default save storage before native platform initialized. Falling back to " + cachedUserDataPath);
			return cachedUserDataPath;
		}
		return GetUserGameDataPath(PlatformManager.MultiPlatform.UserDataRoaming.DefaultSaveStorage, _folder);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetUserGameDataPath(UserDataStorageType _storage, string _folder = "")
	{
		switch (_storage)
		{
		case UserDataStorageType.DeviceLocal:
			return GetCachedUserDataPath(_storage, _folder);
		case UserDataStorageType.Roaming:
			if (!PlatformManager.MultiPlatform.UserDataRoaming.IsSupported)
			{
				string cachedUserDataPath = GetCachedUserDataPath(UserDataStorageType.DeviceLocal, _folder);
				Log.Error("Platform does not support save roaming. Falling back to " + cachedUserDataPath);
				return cachedUserDataPath;
			}
			return GetCachedUserDataPath(_storage, _folder);
		default:
			throw new Exception($"Unknown user data storage type {_storage}");
		}
	}

	public static string GetDeviceLocalUserGameDataDir()
	{
		return GetUserGameDataPath(UserDataStorageType.DeviceLocal);
	}

	public static string GetRoamingUserGameDataDir()
	{
		return GetUserGameDataPath(UserDataStorageType.Roaming);
	}

	public static string GetUserGameDataDir()
	{
		return GetDefaultUserGameDataPath();
	}

	public static string GetUserGameDataDir(UserDataStorageType _storage)
	{
		return GetUserGameDataPath(_storage);
	}

	public static string GetSaveGameRootDir()
	{
		return GetDefaultUserGameDataPath("Saves");
	}

	public static string GetSaveGameRootDir(UserDataStorageType _storage)
	{
		return GetUserGameDataPath(_storage, "Saves");
	}

	public static string GetSaveGameDir(string _worldName, UserDataStorageType _storage)
	{
		return GetSaveGameRootDir(_storage) + "/" + _worldName;
	}

	public static string GetSaveGameDir(string _worldName, string _gameName, UserDataStorageType _storage)
	{
		return GetSaveGameDir(_worldName, _storage) + "/" + _gameName;
	}

	public static string GetSaveGameDir()
	{
		return GetSaveGameDir(GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameName), (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType));
	}

	public static string GetSaveGameLocalRootDir(UserDataStorageType _storage)
	{
		return GetUserGameDataPath(_storage, "SavesLocal");
	}

	public static void SetSaveGameLocalGuid(string _guid)
	{
		GamePrefs.Set(EnumGamePrefs.GameGuidClient, _guid);
		if (!string.IsNullOrEmpty(_guid))
		{
			if (!PlatformManager.MultiPlatform.UserDataRoaming.SaveRoamingEnabled)
			{
				GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, 0);
			}
			else if (SdDirectory.Exists(GetSaveGameLocalDir(UserDataStorageType.DeviceLocal, _guid)))
			{
				GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, 0);
			}
			else
			{
				GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, 1);
			}
		}
	}

	public static string GetSaveGameLocalDir()
	{
		string text = GamePrefs.GetString(EnumGamePrefs.GameGuidClient);
		if (string.IsNullOrEmpty(text))
		{
			throw new Exception("Accessing GetSaveGameLocalDir while GameGuidClient is not yet set!");
		}
		return GetSaveGameLocalDir((UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType), text);
	}

	public static string GetSaveGameLocalDir(UserDataStorageType _storage, string _guid)
	{
		return GetSaveGameLocalRootDir(_storage) + "/" + _guid;
	}

	public static string GetPlayerDataDir()
	{
		return Path.Combine(GetSaveGameDir(), "Player");
	}

	public static string GetPlayerDataLocalDir()
	{
		return Path.Combine(GetSaveGameLocalDir(), "Player");
	}

	public static int GetPlayerSaves(FoundSave _foundSave = null, bool includeArchived = false)
	{
		int num = 0;
		if (PlatformManager.MultiPlatform.UserDataRoaming.SaveRoamingEnabled)
		{
			num += SearchSaveDir(UserDataStorageType.Roaming);
		}
		return num + SearchSaveDir(UserDataStorageType.DeviceLocal);
		[PublicizedFrom(EAccessModifier.Internal)]
		int SearchSaveDir(UserDataStorageType storage)
		{
			int num2 = 0;
			string saveGameRootDir = GetSaveGameRootDir(storage);
			if (!SdDirectory.Exists(saveGameRootDir))
			{
				return 0;
			}
			SdFileSystemInfo[] directories = new SdDirectoryInfo(saveGameRootDir).GetDirectories();
			directories = directories;
			for (int i = 0; i < directories.Length; i++)
			{
				SdDirectoryInfo sdDirectoryInfo = (SdDirectoryInfo)directories[i];
				string fullName = sdDirectoryInfo.FullName;
				if (SdDirectory.Exists(fullName))
				{
					SdFileSystemInfo[] directories2 = new SdDirectoryInfo(fullName).GetDirectories();
					directories2 = directories2;
					for (int j = 0; j < directories2.Length; j++)
					{
						SdDirectoryInfo sdDirectoryInfo2 = (SdDirectoryInfo)directories2[j];
						if (!sdDirectoryInfo2.Name.Contains("#"))
						{
							bool flag = SdFile.Exists(Path.Combine(sdDirectoryInfo2.FullName, "archived.flag"));
							if (!(!includeArchived && flag))
							{
								string text = sdDirectoryInfo2.FullName + "/main.ttw";
								if (SdFile.Exists(text))
								{
									try
									{
										WorldState worldState = new WorldState();
										worldState.Load(text, _warnOnDifferentVersion: false);
										if (worldState.gameVersion != null)
										{
											_foundSave?.Invoke(storage, sdDirectoryInfo2.Name, sdDirectoryInfo.Name, SdFile.GetLastWriteTime(text), worldState, flag);
											num2++;
										}
									}
									catch (Exception ex)
									{
										Log.Warning("Error reading header of level '" + text + "'. Ignoring. Msg: " + ex.Message);
									}
								}
							}
						}
					}
				}
			}
			return num2;
		}
	}

	public static string GetSaveGameRegionDir()
	{
		return Path.Combine(GetSaveGameDir(), "Region");
	}

	public static bool IsWorldGenerated(string _worldName, UserDataStorageType _storage)
	{
		return SdDirectory.Exists(Path.Combine(GetUserGameDataDir(_storage), "GeneratedWorlds", _worldName));
	}

	public static bool IsAbsolutePath(string _path)
	{
		switch (m_UnityRuntimePlatform)
		{
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
		case RuntimePlatform.IPhonePlayer:
		case RuntimePlatform.Android:
		case RuntimePlatform.LinuxPlayer:
		case RuntimePlatform.LinuxEditor:
		case RuntimePlatform.PS4:
		case RuntimePlatform.PS5:
		case RuntimePlatform.LinuxServer:
		case RuntimePlatform.OSXServer:
			if (_path[0] != '/' && _path[0] != '\\' && !_path.StartsWith("~/"))
			{
				return _path.StartsWith("~\\");
			}
			return true;
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
		case RuntimePlatform.XboxOne:
		case RuntimePlatform.GameCoreXboxSeries:
		case RuntimePlatform.GameCoreXboxOne:
		case RuntimePlatform.WindowsServer:
			if (_path[1] == ':' && (_path[2] == '/' || _path[2] == '\\'))
			{
				if (_path[0] < 'A' || _path[0] > 'Z')
				{
					if (_path[0] >= 'a')
					{
						return _path[0] <= 'z';
					}
					return false;
				}
				return true;
			}
			return false;
		default:
			throw new ArgumentOutOfRangeException("_path", _path, "Unsupported platform");
		}
	}

	public static string MakeAbsolutePath(string _path)
	{
		if (IsAbsolutePath(_path))
		{
			return _path;
		}
		return GetGamePath() + "/" + _path;
	}

	public static string GetOsStylePath(string _path)
	{
		if (m_UnityRuntimePlatform != RuntimePlatform.WindowsPlayer && m_UnityRuntimePlatform != RuntimePlatform.WindowsServer)
		{
			return _path.Replace("\\", "/");
		}
		return _path.Replace("/", "\\");
	}

	public static void CopyDirectory(string _sourceDirectory, string _targetDirectory)
	{
		SdDirectoryInfo source = new SdDirectoryInfo(_sourceDirectory);
		SdDirectoryInfo target = new SdDirectoryInfo(_targetDirectory);
		CopyAll(source, target);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CopyAll(SdDirectoryInfo _source, SdDirectoryInfo _target)
	{
		SdDirectory.CreateDirectory(_target.FullName);
		SdFileInfo[] files = _source.GetFiles();
		foreach (SdFileInfo sdFileInfo in files)
		{
			sdFileInfo.CopyTo(Path.Combine(_target.FullName, sdFileInfo.Name), overwrite: true);
		}
		SdDirectoryInfo[] directories = _source.GetDirectories();
		foreach (SdDirectoryInfo sdDirectoryInfo in directories)
		{
			SdDirectoryInfo target = _target.CreateSubdirectory(sdDirectoryInfo.Name);
			CopyAll(sdDirectoryInfo, target);
		}
	}

	public static void SafeDirectoryCopy(string _sourceDirectory, string _targetDirectory)
	{
		try
		{
			CopyDirectory(_sourceDirectory, _targetDirectory);
		}
		catch
		{
			try
			{
				if (SdDirectory.Exists(_targetDirectory))
				{
					SdDirectory.Delete(_targetDirectory, recursive: true);
				}
			}
			catch (Exception e)
			{
				Log.Error("Failed to cleanup target path " + _targetDirectory + " after failed directory move");
				Log.Exception(e);
			}
			throw;
		}
	}

	public static void SafeDirectoryMove(string _sourceDirectory, string _targetDirectory)
	{
		SafeDirectoryCopy(_sourceDirectory, _targetDirectory);
		try
		{
			SdDirectory.Delete(_sourceDirectory, recursive: true);
		}
		catch (Exception e)
		{
			Log.Error("Could not remove source directory " + _sourceDirectory);
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OpenExplorerOnLinux(string _path)
	{
		_path = _path.Replace("\\", "/");
		if (SdFile.Exists(_path))
		{
			_path = Path.GetDirectoryName(_path);
		}
		if (_path.IndexOf(' ') >= 0)
		{
			_path = "\"" + _path + "\"";
		}
		try
		{
			Process.Start("xdg-open", _path);
		}
		catch (Exception e)
		{
			Log.Error("Failed opening file browser:");
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OpenExplorerOnMac(string _path)
	{
		_path = _path.Replace("\\", "/");
		bool flag = SdDirectory.Exists(_path);
		if (!_path.StartsWith("\""))
		{
			_path = "\"" + _path;
		}
		if (!_path.EndsWith("\""))
		{
			_path += "\"";
		}
		try
		{
			Process.Start("open", (flag ? "" : "-R ") + _path);
		}
		catch (Exception e)
		{
			Log.Error("Failed opening Finder:");
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OpenExplorerOnWin(string _path)
	{
		_path = _path.Replace("/", "\\");
		bool flag = SdDirectory.Exists(_path);
		try
		{
			Process.Start("explorer.exe", (flag ? "/root,\"" : "/select,\"") + _path + "\"");
		}
		catch (Exception e)
		{
			Log.Error("Failed opening Explorer:");
			Log.Exception(e);
		}
	}

	public static void OpenExplorer(string _path)
	{
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
		case RuntimePlatform.WindowsServer:
			OpenExplorerOnWin(_path);
			break;
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
		case RuntimePlatform.OSXServer:
			OpenExplorerOnMac(_path);
			break;
		case RuntimePlatform.LinuxPlayer:
		case RuntimePlatform.LinuxEditor:
		case RuntimePlatform.LinuxServer:
			OpenExplorerOnLinux(_path);
			break;
		default:
			Log.Error("Failed opening file browser: Unsupported OS");
			break;
		}
	}

	public static string IsRunningAsSnap()
	{
		if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.Linux)
		{
			return null;
		}
		string environmentVariable = Environment.GetEnvironmentVariable("SNAP_NAME");
		if (environmentVariable == null)
		{
			Log.Out("Snap detection: Not running as Snap (no SNAP_NAME environment variable)");
			return null;
		}
		Log.Out("Snap detection: Running as Snap (Snap package: '" + environmentVariable + "')");
		return environmentVariable;
	}

	public static bool IsRunningInSteamRuntime()
	{
		if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.Linux)
		{
			return false;
		}
		string text = "/etc/os-release";
		if (!SdFile.Exists(text))
		{
			Log.Out("SteamRuntime detection: Linux OS file " + text + " does not exist");
			return false;
		}
		string[] array = SdFile.ReadAllLines(text);
		foreach (string input in array)
		{
			Match match = linuxOsReleaseMatcher.Match(input);
			if (match.Success)
			{
				string value = match.Groups[1].Value;
				bool flag = value.EqualsCaseInsensitive("steamrt");
				Log.Out($"SteamRuntime detection: OS ID='{value}', is SteamRT={flag}");
				return flag;
			}
		}
		Log.Out("SteamRuntime detection: No ID line matched");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static string GetPostTerminationAccessiblePath()
	{
		string text = GetApplicationScratchPath() + "/" + "7 Days To Die".Replace(" ", "");
		if (!text.EndsWith('/') && !text.EndsWith('\\'))
		{
			text += "/";
		}
		return text;
	}
}

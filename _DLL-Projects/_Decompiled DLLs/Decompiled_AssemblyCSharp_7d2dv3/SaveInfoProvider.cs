using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Platform;
using Platform.XBL.Save.Storage;
using UnityEngine;

public class SaveInfoProvider
{
	public struct SaveSizeInfo
	{
		public UserDataStorageType StorageType;

		public long BytesOnDisk;

		public long BytesReserved;

		public bool IsArchived;

		public long ReportedSize
		{
			get
			{
				if (!IsArchived && UsesDataLimit)
				{
					return MaxSize;
				}
				return BytesOnDisk;
			}
		}

		public long MaxSize => Math.Max(BytesOnDisk, BytesReserved);

		public bool UsesDataLimit => StorageType.UsesDataLimit();

		public bool Archivable
		{
			get
			{
				if (UsesDataLimit)
				{
					return BytesReserved >= BytesOnDisk;
				}
				return false;
			}
		}
	}

	public class WorldEntryInfo : IComparable
	{
		public string WorldKey;

		public string Name;

		public string Type;

		public PathAbstractions.AbstractedLocation Location;

		public bool Deletable;

		public bool Moveable;

		public long WorldDataSize;

		public VersionInformation Version;

		public Vector2i? WorldSize;

		public long SaveDataSizeForLimit;

		public long SaveDataSizeTotal;

		public int SaveDataCount;

		public long BarStartOffset;

		public bool HideIfEmpty;

		public readonly List<SaveEntryInfo> SaveEntryInfos = new List<SaveEntryInfo>();

		public bool UsesDataLimit => Location.StorageType.UsesDataLimit();

		public string DisplayName
		{
			get
			{
				if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional && Location.Type == PathAbstractions.EAbstractedLocationType.UserDataPath)
				{
					return Name + " [808080][i](" + Location.StorageType.LocalizedName() + ")[/i][-]";
				}
				return Name;
			}
		}

		public bool ShouldBeMovedWithSave(UserDataStorageType targetSaveStorage)
		{
			if (targetSaveStorage.UsesDataLimit() && Moveable)
			{
				return targetSaveStorage != Location.StorageType;
			}
			return false;
		}

		public int CompareTo(object obj)
		{
			if (obj is WorldEntryInfo worldEntryInfo)
			{
				return string.Compare(WorldKey, worldEntryInfo.WorldKey, StringComparison.OrdinalIgnoreCase);
			}
			return 1;
		}
	}

	public class SaveEntryInfo : IComparable
	{
		public string Name;

		public UserDataStorageType StorageType;

		public string SaveDir;

		public SaveSizeInfo SizeInfo;

		public long BarStartOffset;

		public WorldEntryInfo WorldEntry;

		public DateTime LastSaved;

		public VersionInformation Version;

		public readonly List<PlayerEntryInfo> PlayerEntryInfos = new List<PlayerEntryInfo>();

		public string DisplayName
		{
			get
			{
				if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional)
				{
					return Name + " [808080][i](" + StorageType.LocalizedName() + ")[/i][-]";
				}
				return Name;
			}
		}

		public int CompareTo(object obj)
		{
			if (obj is SaveEntryInfo saveEntryInfo)
			{
				int num = saveEntryInfo.LastSaved.CompareTo(LastSaved);
				if (num == 0)
				{
					return saveEntryInfo.Name.CompareTo(Name);
				}
				return num;
			}
			return 1;
		}
	}

	public class PlayerEntryInfo : IComparable
	{
		public string Id;

		public string CachedName;

		public PlatformUserIdentifierAbs PrimaryUserId;

		public PlatformUserIdentifierAbs NativeUserId;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatformUserData platformUserData;

		public long Size;

		public long BarStartOffset;

		public SaveEntryInfo SaveEntry;

		public DateTime LastPlayed;

		public int PlayerLevel;

		public float DistanceWalked;

		public string PlatformName => (NativeUserId ?? PrimaryUserId)?.PlatformIdentifierString ?? "-";

		public IPlatformUserData PlatformUserData
		{
			get
			{
				if (platformUserData == null && PrimaryUserId != null)
				{
					IPlatformUserData orCreate = PlatformUserManager.GetOrCreate(PrimaryUserId);
					if (NativeUserId != null)
					{
						orCreate.NativeId = NativeUserId;
					}
					platformUserData = orCreate;
				}
				return platformUserData;
			}
		}

		public int CompareTo(object obj)
		{
			if (obj is PlayerEntryInfo playerEntryInfo)
			{
				int num = playerEntryInfo.LastPlayed.CompareTo(LastPlayed);
				if (num == 0)
				{
					return playerEntryInfo.CachedName.CompareTo(CachedName);
				}
				return num;
			}
			return 1;
		}
	}

	public class PlayerEntryInfoPlatformDataResolver
	{
		public readonly List<PlayerEntryInfo> pendingPlayerEntries;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool IsComplete
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerEntryInfoPlatformDataResolver(List<PlayerEntryInfo> playerEntries)
		{
			pendingPlayerEntries = playerEntries;
		}

		public static PlayerEntryInfoPlatformDataResolver StartNew(IEnumerable<PlayerEntryInfo> playerEntries)
		{
			List<PlayerEntryInfo> list = new List<PlayerEntryInfo>();
			List<IPlatformUserData> list2 = null;
			foreach (PlayerEntryInfo playerEntry in playerEntries)
			{
				list.Add(playerEntry);
				IPlatformUserData platformUserData = playerEntry.PlatformUserData;
				if (platformUserData != null)
				{
					if (list2 == null)
					{
						list2 = new List<IPlatformUserData>();
					}
					list2.Add(platformUserData);
				}
			}
			PlayerEntryInfoPlatformDataResolver playerEntryInfoPlatformDataResolver = new PlayerEntryInfoPlatformDataResolver(list);
			if (list2 == null)
			{
				playerEntryInfoPlatformDataResolver.IsComplete = true;
				return playerEntryInfoPlatformDataResolver;
			}
			if (!PlatformUserManager.AreUsersPendingResolve(list2))
			{
				playerEntryInfoPlatformDataResolver.IsComplete = true;
				return playerEntryInfoPlatformDataResolver;
			}
			ThreadManager.StartCoroutine(playerEntryInfoPlatformDataResolver.ResolveUserData(list2));
			return playerEntryInfoPlatformDataResolver;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator ResolveUserData(List<IPlatformUserData> resolvingPlatformData)
		{
			yield return PlatformUserManager.ResolveUsersDetailsCoroutine(resolvingPlatformData);
			yield return PlatformUserManager.ResolveUserBlocksCoroutine(resolvingPlatformData);
			IsComplete = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const long BytesPerMB = 1048576L;

	public static readonly string RemoteWorldsLabel = "[" + Localization.Get("xuiDmRemoteWorlds") + "] ";

	public static readonly string RemoteWorldsType = Localization.Get("xuiDmRemote");

	public static readonly string DeletedWorldsType = Localization.Get("xuiDmDeleted");

	public static readonly string ConflictedWorldsType = Localization.Get("xuiDmConflicted");

	public static readonly List<string> HideableWorlds = new List<string> { "Empty", "Playtesting" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveInfoProvider instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, WorldEntryInfo> worldEntryInfosByWorldKey = new Dictionary<string, WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, WorldEntryInfo> deletedWorlds = new Dictionary<string, WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, WorldEntryInfo> conflictedWorlds = new Dictionary<string, WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveEntryInfo> saveEntryInfosBySaveKey = new Dictionary<string, SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveEntryInfo> remoteSaveEntryInfosByGuid = new Dictionary<string, SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<WorldEntryInfo> worldEntryInfos = new List<WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveEntryInfo> saveEntryInfos = new List<SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<string> protectedDirectories = new HashSet<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public long localSavesSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public long remoteSavesSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public long worldsSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public long totalUsedBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long totalAllowanceBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, PlayerEntryInfo> fileNameKeysToPlayerInfos = new Dictionary<string, PlayerEntryInfo>();

	public static SaveInfoProvider Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new SaveInfoProvider();
			}
			return instance;
		}
	}

	public static bool DataLimitEnabled => SaveDataUtils.SaveDataManager.ShouldLimitSize();

	public IReadOnlyCollection<WorldEntryInfo> WorldEntryInfos
	{
		get
		{
			RefreshIfDirty();
			return worldEntryInfos;
		}
	}

	public IReadOnlyCollection<SaveEntryInfo> SaveEntryInfos
	{
		get
		{
			RefreshIfDirty();
			return saveEntryInfos;
		}
	}

	public long TotalUsedBytes
	{
		get
		{
			RefreshIfDirty();
			return totalUsedBytes;
		}
	}

	public long TotalAllowanceBytes
	{
		get
		{
			RefreshIfDirty();
			return totalAllowanceBytes;
		}
	}

	public long TotalAvailableBytes
	{
		get
		{
			RefreshIfDirty();
			return totalAllowanceBytes - totalUsedBytes;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetWorldEntryKey(string worldName, string fullPath)
	{
		return worldName.ToLowerInvariant() + "_" + GameIO.GetNormalizedPath(fullPath);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetWorldEntryKey(string key)
	{
		return key.ToLowerInvariant();
	}

	public static string GetWorldEntryKey(string worldName, UserDataStorageType userStorageType)
	{
		if (string.IsNullOrEmpty(worldName))
		{
			return null;
		}
		PathAbstractions.SearchDefinition worldsSearchPaths = PathAbstractions.WorldsSearchPaths;
		UserDataStorageType? userDataHint = userStorageType;
		PathAbstractions.AbstractedLocation location = worldsSearchPaths.GetLocation(worldName, null, userDataHint);
		if (location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			return GetWorldEntryKey(worldName);
		}
		return GetWorldEntryKey(worldName, location.FullPath);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalSaveEntryKey(string worldName, string saveName, UserDataStorageType saveStorage)
	{
		return $"{worldName}/{saveName}_{saveStorage}".ToLowerInvariant();
	}

	public void SetDirty()
	{
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static long GetPlatformReservedSizeBytes(SaveDataSizes sizes)
	{
		if (PlatformManager.NativePlatform.PlatformIdentifier == EPlatformIdentifier.XBL)
		{
			long num = ((LaunchPrefs.GameCoreSaveStorageProvider.Value == SaveStorageProvider.Files) ? 32 : 16);
			return 5242880 + num * 1048576;
		}
		return 5242880L;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshIfDirty()
	{
		if (!isDirty)
		{
			return;
		}
		isDirty = false;
		localSavesSum = 0L;
		remoteSavesSum = 0L;
		worldsSum = 0L;
		worldEntryInfosByWorldKey.Clear();
		deletedWorlds.Clear();
		conflictedWorlds.Clear();
		saveEntryInfosBySaveKey.Clear();
		remoteSaveEntryInfosByGuid.Clear();
		worldEntryInfos.Clear();
		saveEntryInfos.Clear();
		ProcessLocalWorlds();
		ProcessLocalWorldSaves();
		ProcessRemoteWorldSaves();
		worldEntryInfos.AddRange(worldEntryInfosByWorldKey.Values);
		worldEntryInfos.AddRange(deletedWorlds.Values);
		worldEntryInfos.AddRange(conflictedWorlds.Values);
		totalUsedBytes = localSavesSum + remoteSavesSum + worldsSum;
		long num = 0L;
		worldEntryInfos.Sort();
		foreach (WorldEntryInfo worldEntryInfo in worldEntryInfos)
		{
			worldEntryInfo.BarStartOffset = num;
			if (worldEntryInfo.UsesDataLimit)
			{
				num += worldEntryInfo.WorldDataSize;
			}
			worldEntryInfo.SaveEntryInfos.Sort();
			foreach (SaveEntryInfo saveEntryInfo in worldEntryInfo.SaveEntryInfos)
			{
				saveEntryInfo.PlayerEntryInfos.Sort();
				if (saveEntryInfo.SizeInfo.UsesDataLimit)
				{
					saveEntryInfo.BarStartOffset = num;
					num += saveEntryInfo.SizeInfo.ReportedSize;
					long num2 = num;
					for (int num3 = saveEntryInfo.PlayerEntryInfos.Count - 1; num3 >= 0; num3--)
					{
						PlayerEntryInfo playerEntryInfo = saveEntryInfo.PlayerEntryInfos[num3];
						num2 = (playerEntryInfo.BarStartOffset = num2 - playerEntryInfo.Size);
					}
				}
			}
		}
		if (SaveDataUtils.SaveDataManager.ShouldLimitSize())
		{
			SaveDataUtils.SaveDataManager.UpdateSizes();
			SaveDataSizes sizes = SaveDataUtils.SaveDataManager.GetSizes();
			long platformReservedSizeBytes = GetPlatformReservedSizeBytes(sizes);
			totalAllowanceBytes = sizes.Total - platformReservedSizeBytes;
		}
		else
		{
			totalAllowanceBytes = -1L;
		}
	}

	public void ClearResources()
	{
		worldEntryInfosByWorldKey.Clear();
		deletedWorlds.Clear();
		conflictedWorlds.Clear();
		saveEntryInfosBySaveKey.Clear();
		remoteSaveEntryInfosByGuid.Clear();
		worldEntryInfos.Clear();
		saveEntryInfos.Clear();
		protectedDirectories.Clear();
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessLocalWorlds()
	{
		foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.WorldsSearchPaths.GetAvailablePathsList())
		{
			string type = availablePaths.Type switch
			{
				PathAbstractions.EAbstractedLocationType.Mods => Localization.Get("xuiDmMod") + ": " + availablePaths.ContainingMod.Name, 
				PathAbstractions.EAbstractedLocationType.GameData => Localization.Get("xuiDmBuiltIn"), 
				_ => Localization.Get("xuiDmGenerated"), 
			};
			bool flag = GameIO.IsWorldGenerated(availablePaths.Name, availablePaths.StorageType);
			WorldEntryInfo worldEntryInfo = new WorldEntryInfo
			{
				WorldKey = GetWorldEntryKey(availablePaths.Name, availablePaths.FullPath),
				Name = availablePaths.Name,
				Type = type,
				Location = availablePaths,
				Deletable = flag,
				Moveable = flag,
				WorldDataSize = GameIO.GetDirectorySize(availablePaths.FullPath),
				Version = null,
				HideIfEmpty = (!flag || HideableWorlds.ContainsCaseInsensitive(availablePaths.Name))
			};
			if (worldEntryInfo.UsesDataLimit)
			{
				worldsSum += worldEntryInfo.WorldDataSize;
			}
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(availablePaths);
			if (worldInfo != null)
			{
				worldEntryInfo.Version = worldInfo.GameVersionCreated;
				worldEntryInfo.WorldSize = worldInfo.WorldSize;
			}
			worldEntryInfosByWorldKey[worldEntryInfo.WorldKey] = worldEntryInfo;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessLocalWorldSaves()
	{
		if (PlatformManager.MultiPlatform.UserDataRoaming.SaveRoamingEnabled)
		{
			ProcessLocalWorldSaves(UserDataStorageType.Roaming);
		}
		ProcessLocalWorldSaves(UserDataStorageType.DeviceLocal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessLocalWorldSaves(UserDataStorageType storageType)
	{
		string saveGameRootDir = GameIO.GetSaveGameRootDir(storageType);
		if (!SdDirectory.Exists(saveGameRootDir))
		{
			return;
		}
		SdFileSystemInfo[] directories = new SdDirectoryInfo(saveGameRootDir).GetDirectories();
		directories = directories;
		for (int i = 0; i < directories.Length; i++)
		{
			SdDirectoryInfo sdDirectoryInfo = (SdDirectoryInfo)directories[i];
			if (!SdDirectory.Exists(sdDirectoryInfo.FullName))
			{
				continue;
			}
			SdDirectoryInfo sdDirectoryInfo2 = new SdDirectoryInfo(sdDirectoryInfo.FullName);
			SdFileSystemInfo[] directories2 = sdDirectoryInfo2.GetDirectories();
			SdFileSystemInfo[] array = directories2;
			if (array.Length == 0 && sdDirectoryInfo2.GetFiles().Length == 0)
			{
				SdDirectory.Delete(sdDirectoryInfo.FullName);
				continue;
			}
			string name = sdDirectoryInfo.Name;
			directories2 = array;
			for (int j = 0; j < directories2.Length; j++)
			{
				SdDirectoryInfo curSaveFolder = (SdDirectoryInfo)directories2[j];
				WorldEntryInfo value = null;
				int num = 0;
				int num2 = 0;
				UserDataStorageType value2 = UserDataStorageType.DeviceLocal;
				foreach (WorldEntryInfo value3 in worldEntryInfosByWorldKey.Values)
				{
					if (!value3.Type.Equals(RemoteWorldsType) && value3.Name.Equals(name))
					{
						num++;
						if (value3.Location.Type == PathAbstractions.EAbstractedLocationType.UserDataPath)
						{
							num2++;
							value2 = value3.Location.StorageType;
						}
						value = value3;
					}
				}
				if (num > 1)
				{
					string worldEntryKey = GetWorldEntryKey(name + "_" + ConflictedWorldsType);
					if (!conflictedWorlds.TryGetValue(worldEntryKey, out value))
					{
						PathAbstractions.AbstractedLocation location = PathAbstractions.AbstractedLocation.None;
						if (num2 <= 1)
						{
							PathAbstractions.SearchDefinition worldsSearchPaths = PathAbstractions.WorldsSearchPaths;
							UserDataStorageType? userDataHint = value2;
							location = worldsSearchPaths.GetLocation(name, null, userDataHint);
						}
						value = new WorldEntryInfo
						{
							WorldKey = worldEntryKey,
							Name = name,
							Type = ConflictedWorldsType,
							Location = location,
							Deletable = false,
							Moveable = false,
							WorldDataSize = 0L,
							Version = null,
							HideIfEmpty = true
						};
						conflictedWorlds.Add(value.WorldKey, value);
					}
				}
				if (value == null)
				{
					string worldEntryKey2 = GetWorldEntryKey(sdDirectoryInfo.Name + "_" + DeletedWorldsType);
					if (!deletedWorlds.TryGetValue(worldEntryKey2, out value))
					{
						value = new WorldEntryInfo
						{
							WorldKey = worldEntryKey2,
							Name = sdDirectoryInfo.Name,
							Type = DeletedWorldsType,
							Location = PathAbstractions.AbstractedLocation.None,
							Deletable = false,
							Moveable = false,
							WorldDataSize = 0L,
							Version = null,
							HideIfEmpty = true
						};
						deletedWorlds.Add(value.WorldKey, value);
					}
				}
				ProcessSaveEntry(value, curSaveFolder, storageType);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessSaveEntry(WorldEntryInfo worldEntryInfo, SdDirectoryInfo curSaveFolder, UserDataStorageType storageType)
	{
		string text = curSaveFolder.FullName + "/main.ttw";
		SaveEntryInfo saveEntryInfo = new SaveEntryInfo
		{
			Name = curSaveFolder.Name,
			WorldEntry = worldEntryInfo,
			SaveDir = curSaveFolder.FullName,
			StorageType = storageType,
			Version = null
		};
		saveEntryInfo.SizeInfo.BytesOnDisk = GameIO.GetDirectorySize(curSaveFolder);
		saveEntryInfo.SizeInfo.IsArchived = DataLimitEnabled && SdFile.Exists(Path.Combine(curSaveFolder.FullName, "archived.flag"));
		saveEntryInfo.SizeInfo.StorageType = storageType;
		if (SdFile.Exists(text))
		{
			saveEntryInfo.LastSaved = SdFile.GetLastWriteTime(text);
			try
			{
				WorldState worldState = new WorldState();
				worldState.Load(text, _warnOnDifferentVersion: false);
				saveEntryInfo.Version = worldState.gameVersion;
				saveEntryInfo.SizeInfo.BytesReserved = worldState.saveDataLimit;
				if (DataLimitEnabled && saveEntryInfo.SizeInfo.BytesReserved != -1 && saveEntryInfo.SizeInfo.BytesOnDisk > saveEntryInfo.SizeInfo.BytesReserved)
				{
					Debug.LogError($"Directory size of save \"{curSaveFolder.FullName}\" exceeds serialized save data limit of {worldState.saveDataLimit}.");
				}
			}
			catch (Exception ex)
			{
				Log.Warning("Error reading header of level '" + text + "'. Msg: " + ex.Message);
			}
		}
		else
		{
			if (curSaveFolder.Name != "WorldEditor" && curSaveFolder.Name != "PrefabEditor")
			{
				Log.Warning($"Could not find main ttw file for save in directory: {curSaveFolder}");
			}
			saveEntryInfo.LastSaved = curSaveFolder.LastWriteTime;
		}
		saveEntryInfos.Add(saveEntryInfo);
		string localSaveEntryKey = GetLocalSaveEntryKey(saveEntryInfo.WorldEntry.Name, saveEntryInfo.Name, saveEntryInfo.StorageType);
		saveEntryInfosBySaveKey[localSaveEntryKey] = saveEntryInfo;
		worldEntryInfo.SaveEntryInfos.Add(saveEntryInfo);
		worldEntryInfo.SaveDataCount++;
		worldEntryInfo.SaveDataSizeTotal += saveEntryInfo.SizeInfo.ReportedSize;
		if (saveEntryInfo.SizeInfo.UsesDataLimit)
		{
			worldEntryInfo.SaveDataSizeForLimit += saveEntryInfo.SizeInfo.ReportedSize;
			localSavesSum += saveEntryInfo.SizeInfo.ReportedSize;
		}
		ProcessPlayerEntries(saveEntryInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessPlayerEntries(SaveEntryInfo saveEntryInfo)
	{
		string text = saveEntryInfo.SaveDir + "/Player";
		if (!SdDirectory.Exists(text))
		{
			return;
		}
		fileNameKeysToPlayerInfos.Clear();
		SdFileInfo[] files = new SdDirectoryInfo(text).GetFiles();
		foreach (SdFileInfo sdFileInfo in files)
		{
			int length;
			string text2;
			string a;
			if ((length = sdFileInfo.Name.IndexOf('.')) != -1)
			{
				text2 = sdFileInfo.Name.Substring(0, length);
				int num = sdFileInfo.Name.LastIndexOf('.') + 1;
				a = sdFileInfo.Name.Substring(num, sdFileInfo.Name.Length - num);
			}
			else
			{
				Debug.LogError("Encountered player save file with no extension.");
				text2 = sdFileInfo.Name;
				a = string.Empty;
			}
			DateTime lastWriteTime = SdFile.GetLastWriteTime(sdFileInfo.FullName);
			if (!fileNameKeysToPlayerInfos.TryGetValue(text2, out var value))
			{
				value = new PlayerEntryInfo
				{
					Id = text2,
					LastPlayed = lastWriteTime,
					SaveEntry = saveEntryInfo
				};
				if (PlatformUserIdentifierAbs.TryFromCombinedString(text2, out var _userIdentifier))
				{
					value.PrimaryUserId = _userIdentifier;
					value.CachedName = _userIdentifier.ReadablePlatformUserIdentifier;
				}
				else
				{
					Log.Error("Could not associate player save file \"" + sdFileInfo.FullName + "\" with a player id. Combined id string: " + text2);
					value.CachedName = text2;
				}
				saveEntryInfo.PlayerEntryInfos.Add(value);
			}
			else if (lastWriteTime > value.LastPlayed)
			{
				value.LastPlayed = lastWriteTime;
			}
			value.Size += sdFileInfo.Length;
			if (string.Equals(a, "meta", StringComparison.InvariantCultureIgnoreCase) && PlayerMetaInfo.TryRead(sdFileInfo.FullName, out var _playerMetaInfo))
			{
				if (_playerMetaInfo.NativeId != null)
				{
					value.NativeUserId = _playerMetaInfo.NativeId;
				}
				if (_playerMetaInfo.Name != null)
				{
					value.CachedName = _playerMetaInfo.Name;
				}
				value.PlayerLevel = _playerMetaInfo.Level;
				value.DistanceWalked = _playerMetaInfo.DistanceWalked;
			}
			fileNameKeysToPlayerInfos[text2] = value;
		}
		foreach (KeyValuePair<string, PlayerEntryInfo> fileNameKeysToPlayerInfo in fileNameKeysToPlayerInfos)
		{
			long directorySize = GameIO.GetDirectorySize(new SdDirectoryInfo(Path.Combine(text, fileNameKeysToPlayerInfo.Key)));
			fileNameKeysToPlayerInfo.Value.Size += directorySize;
		}
		fileNameKeysToPlayerInfos.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessRemoteWorldSaves()
	{
		if (PlatformManager.MultiPlatform.UserDataRoaming.SaveRoamingEnabled)
		{
			ProcessRemoteWorldSaves(UserDataStorageType.Roaming);
		}
		ProcessRemoteWorldSaves(UserDataStorageType.DeviceLocal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessRemoteWorldSaves(UserDataStorageType storageType)
	{
		string saveGameLocalRootDir = GameIO.GetSaveGameLocalRootDir(storageType);
		if (!SdDirectory.Exists(saveGameLocalRootDir))
		{
			return;
		}
		SdFileSystemInfo[] directories = new SdDirectoryInfo(saveGameLocalRootDir).GetDirectories();
		directories = directories;
		for (int i = 0; i < directories.Length; i++)
		{
			SdDirectoryInfo sdDirectoryInfo = (SdDirectoryInfo)directories[i];
			string text = sdDirectoryInfo.FullName + "/RemoteWorldInfo.xml";
			SaveEntryInfo saveEntryInfo = new SaveEntryInfo
			{
				SaveDir = sdDirectoryInfo.FullName,
				StorageType = storageType
			};
			saveEntryInfo.SizeInfo.BytesOnDisk = GameIO.GetDirectorySize(sdDirectoryInfo);
			saveEntryInfo.SizeInfo.IsArchived = DataLimitEnabled && SdFile.Exists(Path.Combine(sdDirectoryInfo.FullName, "archived.flag"));
			saveEntryInfo.SizeInfo.StorageType = storageType;
			PathAbstractions.AbstractedLocation location = PathAbstractions.Contextual.FindDownloadedRemoteWorld(sdDirectoryInfo.FullName);
			WorldEntryInfo value;
			if (RemoteWorldInfo.TryRead(text, out var _remoteWorldInfo))
			{
				string text2 = ((location.Type != PathAbstractions.EAbstractedLocationType.None) ? GetWorldEntryKey(_remoteWorldInfo.WorldName, location.FullPath) : GetWorldEntryKey(_remoteWorldInfo.WorldName));
				if (!worldEntryInfosByWorldKey.TryGetValue(text2, out value))
				{
					value = new WorldEntryInfo
					{
						WorldKey = text2,
						Name = _remoteWorldInfo.WorldName,
						Type = RemoteWorldsType,
						Location = location,
						Deletable = false,
						Moveable = false,
						WorldDataSize = 0L,
						Version = _remoteWorldInfo.GameVersion
					};
					worldEntryInfosByWorldKey[text2] = value;
				}
				saveEntryInfo.SizeInfo.BytesReserved = _remoteWorldInfo.SaveSize;
				if (DataLimitEnabled && saveEntryInfo.SizeInfo.BytesReserved != -1 && saveEntryInfo.SizeInfo.BytesOnDisk > saveEntryInfo.SizeInfo.BytesReserved)
				{
					Debug.LogError($"Directory size of save \"{sdDirectoryInfo.FullName}\" exceeds serialized save data size of {_remoteWorldInfo.SaveSize}.");
				}
				saveEntryInfo.Name = _remoteWorldInfo.GameName;
				saveEntryInfo.WorldEntry = value;
				saveEntryInfo.LastSaved = SdFile.GetLastWriteTime(text);
				saveEntryInfo.Version = _remoteWorldInfo.GameVersion;
			}
			else
			{
				string worldEntryKey = GetWorldEntryKey(RemoteWorldsLabel);
				if (!worldEntryInfosByWorldKey.TryGetValue(worldEntryKey, out value))
				{
					value = new WorldEntryInfo
					{
						WorldKey = worldEntryKey,
						Name = RemoteWorldsLabel,
						Type = RemoteWorldsType,
						Location = location,
						Deletable = false,
						Moveable = false,
						WorldDataSize = 0L,
						Version = null
					};
					worldEntryInfosByWorldKey[worldEntryKey] = value;
				}
				saveEntryInfo.Name = sdDirectoryInfo.Name;
				saveEntryInfo.WorldEntry = value;
				saveEntryInfo.LastSaved = sdDirectoryInfo.LastWriteTime;
				saveEntryInfo.Version = null;
			}
			saveEntryInfos.Add(saveEntryInfo);
			remoteSaveEntryInfosByGuid[sdDirectoryInfo.Name] = saveEntryInfo;
			value.SaveEntryInfos.Add(saveEntryInfo);
			value.SaveDataCount++;
			value.SaveDataSizeTotal += saveEntryInfo.SizeInfo.ReportedSize;
			if (saveEntryInfo.SizeInfo.UsesDataLimit)
			{
				value.SaveDataSizeForLimit += saveEntryInfo.SizeInfo.ReportedSize;
				remoteSavesSum += saveEntryInfo.SizeInfo.ReportedSize;
			}
		}
	}

	public bool TryGetLocalSaveEntry(string worldName, string saveName, UserDataStorageType storage, out SaveEntryInfo saveEntryInfo)
	{
		RefreshIfDirty();
		string localSaveEntryKey = GetLocalSaveEntryKey(worldName, saveName, storage);
		return saveEntryInfosBySaveKey.TryGetValue(localSaveEntryKey, out saveEntryInfo);
	}

	public bool TryGetRemoteSaveEntry(string guid, out SaveEntryInfo saveEntryInfo)
	{
		RefreshIfDirty();
		return remoteSaveEntryInfosByGuid.TryGetValue(guid, out saveEntryInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string NormalizePath(string path)
	{
		return Path.GetFullPath(path);
	}

	public void SetDirectoryProtected(string path, bool isProtected)
	{
		if (isProtected)
		{
			protectedDirectories.Add(NormalizePath(path));
		}
		else
		{
			protectedDirectories.Remove(NormalizePath(path));
		}
	}

	public bool IsDirectoryProtected(string path)
	{
		return protectedDirectories.Contains(NormalizePath(path));
	}
}

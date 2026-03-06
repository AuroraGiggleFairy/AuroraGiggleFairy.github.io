using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Platform;
using UnityEngine;

public class SaveInfoProvider
{
	public struct SaveSizeInfo
	{
		public long BytesOnDisk;

		public long BytesReserved;

		public bool IsArchived;

		public long ReportedSize
		{
			get
			{
				if (!IsArchived)
				{
					return MaxSize;
				}
				return BytesOnDisk;
			}
		}

		public long MaxSize => Math.Max(BytesOnDisk, BytesReserved);

		public bool Archivable => BytesReserved >= BytesOnDisk;
	}

	public class WorldEntryInfo : IComparable
	{
		public string WorldKey;

		public string Name;

		public string Type;

		public PathAbstractions.AbstractedLocation Location;

		public bool Deletable;

		public long WorldDataSize;

		public VersionInformation Version;

		public long SaveDataSize;

		public int SaveDataCount;

		public long BarStartOffset;

		public bool HideIfEmpty;

		public readonly List<SaveEntryInfo> SaveEntryInfos = new List<SaveEntryInfo>();

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

		public string SaveDir;

		public long Size;

		public SaveSizeInfo SizeInfo;

		public long BarStartOffset;

		public WorldEntryInfo WorldEntry;

		public DateTime LastSaved;

		public VersionInformation Version;

		public readonly List<PlayerEntryInfo> PlayerEntryInfos = new List<PlayerEntryInfo>();

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

	public const string cLocalWorldsKey = "Local";

	public static readonly string RemoteWorldsLabel = "[" + Localization.Get("xuiDmRemoteWorlds") + "] ";

	public static readonly string RemoteWorldsType = Localization.Get("xuiDmRemote");

	public static readonly string DeletedWorldsType = Localization.Get("xuiDmDeleted");

	public static readonly List<string> HideableWorlds = new List<string> { "Empty", "Playtesting" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveInfoProvider instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, WorldEntryInfo> worldEntryInfosByWorldKey = new Dictionary<string, WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveEntryInfo> saveEntryInfosBySaveKey = new Dictionary<string, SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveEntryInfo> remoteSaveEntryInfosByGuid = new Dictionary<string, SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<WorldEntryInfo> worldEntryInfos = new List<WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveEntryInfo> saveEntryInfos = new List<SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlayerEntryInfo> playerEntryInfos = new List<PlayerEntryInfo>();

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

	public ReadOnlyCollection<WorldEntryInfo> WorldEntryInfos
	{
		get
		{
			RefreshIfDirty();
			return worldEntryInfos.AsReadOnly();
		}
	}

	public ReadOnlyCollection<SaveEntryInfo> SaveEntryInfos
	{
		get
		{
			RefreshIfDirty();
			return saveEntryInfos.AsReadOnly();
		}
	}

	public ReadOnlyCollection<PlayerEntryInfo> PlayerEntryInfos
	{
		get
		{
			RefreshIfDirty();
			return playerEntryInfos.AsReadOnly();
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

	public static string GetWorldEntryKey(string worldName, string worldType)
	{
		return (worldName + worldType).ToLowerInvariant();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetSaveEntryKey(string worldKey, string saveName)
	{
		return (worldKey + "/" + saveName).ToLowerInvariant();
	}

	public void SetDirty()
	{
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static long GetPlatformReservedSizeBytes(SaveDataSizes sizes)
	{
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
		saveEntryInfosBySaveKey.Clear();
		remoteSaveEntryInfosByGuid.Clear();
		worldEntryInfos.Clear();
		saveEntryInfos.Clear();
		playerEntryInfos.Clear();
		ProcessLocalWorlds();
		ProcessLocalWorldSaves();
		ProcessRemoteWorldSaves();
		worldEntryInfos.AddRange(worldEntryInfosByWorldKey.Values);
		totalUsedBytes = localSavesSum + remoteSavesSum + worldsSum;
		long num = 0L;
		worldEntryInfos.Sort();
		foreach (WorldEntryInfo worldEntryInfo in worldEntryInfos)
		{
			worldEntryInfo.BarStartOffset = num;
			if (worldEntryInfo.Deletable)
			{
				num += worldEntryInfo.WorldDataSize;
			}
			worldEntryInfo.SaveEntryInfos.Sort();
			foreach (SaveEntryInfo saveEntryInfo in worldEntryInfo.SaveEntryInfos)
			{
				saveEntryInfo.BarStartOffset = num;
				num += saveEntryInfo.SizeInfo.ReportedSize;
				long num2 = num;
				saveEntryInfo.PlayerEntryInfos.Sort();
				for (int num3 = saveEntryInfo.PlayerEntryInfos.Count - 1; num3 >= 0; num3--)
				{
					PlayerEntryInfo playerEntryInfo = saveEntryInfo.PlayerEntryInfos[num3];
					num2 = (playerEntryInfo.BarStartOffset = num2 - playerEntryInfo.Size);
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
		saveEntryInfosBySaveKey.Clear();
		remoteSaveEntryInfosByGuid.Clear();
		worldEntryInfos.Clear();
		saveEntryInfos.Clear();
		playerEntryInfos.Clear();
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
			bool flag = GameIO.IsWorldGenerated(availablePaths.Name);
			WorldEntryInfo worldEntryInfo = new WorldEntryInfo
			{
				WorldKey = GetWorldEntryKey(availablePaths.Name, "Local"),
				Name = availablePaths.Name,
				Type = type,
				Location = availablePaths,
				Deletable = flag,
				WorldDataSize = GameIO.GetDirectorySize(availablePaths.FullPath),
				Version = null,
				HideIfEmpty = (!flag || HideableWorlds.ContainsCaseInsensitive(availablePaths.Name))
			};
			if (worldEntryInfo.Deletable)
			{
				worldsSum += worldEntryInfo.WorldDataSize;
			}
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(availablePaths);
			if (worldInfo != null)
			{
				worldEntryInfo.Version = worldInfo.GameVersionCreated;
			}
			worldEntryInfosByWorldKey[worldEntryInfo.WorldKey] = worldEntryInfo;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessLocalWorldSaves()
	{
		string saveGameRootDir = GameIO.GetSaveGameRootDir();
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
			if (!worldEntryInfosByWorldKey.TryGetValue(GetWorldEntryKey(sdDirectoryInfo.Name, "Local"), out var value))
			{
				value = new WorldEntryInfo
				{
					WorldKey = GetWorldEntryKey(sdDirectoryInfo.Name, DeletedWorldsType),
					Name = sdDirectoryInfo.Name,
					Type = DeletedWorldsType,
					Location = PathAbstractions.AbstractedLocation.None,
					Deletable = false,
					WorldDataSize = 0L,
					Version = null,
					HideIfEmpty = true
				};
				worldEntryInfosByWorldKey[value.WorldKey] = value;
			}
			directories2 = array;
			for (int j = 0; j < directories2.Length; j++)
			{
				SdDirectoryInfo curSaveFolder = (SdDirectoryInfo)directories2[j];
				ProcessSaveEntry(value, curSaveFolder);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessSaveEntry(WorldEntryInfo worldEntryInfo, SdDirectoryInfo curSaveFolder)
	{
		string text = curSaveFolder.FullName + "/main.ttw";
		SaveEntryInfo saveEntryInfo = new SaveEntryInfo
		{
			Name = curSaveFolder.Name,
			WorldEntry = worldEntryInfo,
			SaveDir = curSaveFolder.FullName,
			Version = null
		};
		saveEntryInfo.SizeInfo.BytesOnDisk = GameIO.GetDirectorySize(curSaveFolder);
		saveEntryInfo.SizeInfo.IsArchived = DataLimitEnabled && SdFile.Exists(Path.Combine(curSaveFolder.FullName, "archived.flag"));
		if (SdFile.Exists(text))
		{
			saveEntryInfo.LastSaved = SdFile.GetLastWriteTime(text);
			try
			{
				WorldState worldState = new WorldState();
				worldState.Load(text, _warnOnDifferentVersion: false);
				saveEntryInfo.Version = worldState.gameVersion;
				if (DataLimitEnabled && worldState.saveDataLimit != -1)
				{
					saveEntryInfo.SizeInfo.BytesReserved = worldState.saveDataLimit;
					if (saveEntryInfo.SizeInfo.BytesOnDisk > saveEntryInfo.SizeInfo.BytesReserved)
					{
						Debug.LogError($"Directory size of save \"{curSaveFolder.FullName}\" exceeds serialized save data limit of {worldState.saveDataLimit}.");
					}
				}
				else
				{
					saveEntryInfo.SizeInfo.BytesReserved = -1L;
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
		string saveEntryKey = GetSaveEntryKey(saveEntryInfo.WorldEntry.WorldKey, saveEntryInfo.Name);
		saveEntryInfosBySaveKey[saveEntryKey] = saveEntryInfo;
		worldEntryInfo.SaveEntryInfos.Add(saveEntryInfo);
		worldEntryInfo.SaveDataCount++;
		worldEntryInfo.SaveDataSize += saveEntryInfo.SizeInfo.ReportedSize;
		localSavesSum += saveEntryInfo.SizeInfo.ReportedSize;
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
			if (string.Equals(a, "meta", StringComparison.InvariantCultureIgnoreCase) && PlayerMetaInfo.TryRead(sdFileInfo.FullName, out var playerMetaInfo))
			{
				if (playerMetaInfo.nativeId != null)
				{
					value.NativeUserId = playerMetaInfo.nativeId;
				}
				if (playerMetaInfo.name != null)
				{
					value.CachedName = playerMetaInfo.name;
				}
				value.PlayerLevel = playerMetaInfo.level;
				value.DistanceWalked = playerMetaInfo.distanceWalked;
			}
			fileNameKeysToPlayerInfos[text2] = value;
		}
		foreach (KeyValuePair<string, PlayerEntryInfo> fileNameKeysToPlayerInfo in fileNameKeysToPlayerInfos)
		{
			long directorySize = GameIO.GetDirectorySize(new SdDirectoryInfo(Path.Combine(text, fileNameKeysToPlayerInfo.Key)));
			fileNameKeysToPlayerInfo.Value.Size += directorySize;
			playerEntryInfos.Add(fileNameKeysToPlayerInfo.Value);
		}
		fileNameKeysToPlayerInfos.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessRemoteWorldSaves()
	{
		string saveGameLocalRootDir = GameIO.GetSaveGameLocalRootDir();
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
				SaveDir = sdDirectoryInfo.FullName
			};
			saveEntryInfo.SizeInfo.BytesOnDisk = GameIO.GetDirectorySize(sdDirectoryInfo);
			saveEntryInfo.SizeInfo.IsArchived = DataLimitEnabled && SdFile.Exists(Path.Combine(sdDirectoryInfo.FullName, "archived.flag"));
			WorldEntryInfo value;
			if (RemoteWorldInfo.TryRead(text, out var remoteWorldInfo))
			{
				string worldEntryKey = GetWorldEntryKey(remoteWorldInfo.worldName, RemoteWorldsType);
				if (!worldEntryInfosByWorldKey.TryGetValue(worldEntryKey, out value))
				{
					value = new WorldEntryInfo
					{
						WorldKey = worldEntryKey,
						Name = remoteWorldInfo.worldName,
						Type = RemoteWorldsType,
						Location = PathAbstractions.AbstractedLocation.None,
						Deletable = false,
						WorldDataSize = 0L,
						Version = remoteWorldInfo.gameVersion
					};
					worldEntryInfosByWorldKey[worldEntryKey] = value;
				}
				if (DataLimitEnabled && remoteWorldInfo.saveSize != -1)
				{
					saveEntryInfo.SizeInfo.BytesReserved = remoteWorldInfo.saveSize;
					if (saveEntryInfo.SizeInfo.BytesOnDisk > saveEntryInfo.SizeInfo.BytesReserved)
					{
						Debug.LogError($"Directory size of save \"{sdDirectoryInfo.FullName}\" exceeds serialized save data size of {remoteWorldInfo.saveSize}.");
					}
				}
				else
				{
					saveEntryInfo.SizeInfo.BytesReserved = -1L;
				}
				saveEntryInfo.Name = remoteWorldInfo.gameName;
				saveEntryInfo.WorldEntry = value;
				saveEntryInfo.LastSaved = SdFile.GetLastWriteTime(text);
				saveEntryInfo.Version = remoteWorldInfo.gameVersion;
			}
			else
			{
				string worldEntryKey2 = GetWorldEntryKey(RemoteWorldsLabel, RemoteWorldsType);
				if (!worldEntryInfosByWorldKey.TryGetValue(worldEntryKey2, out value))
				{
					value = new WorldEntryInfo
					{
						WorldKey = worldEntryKey2,
						Name = RemoteWorldsLabel,
						Type = RemoteWorldsType,
						Location = PathAbstractions.AbstractedLocation.None,
						Deletable = false,
						WorldDataSize = 0L,
						Version = null
					};
					worldEntryInfosByWorldKey[worldEntryKey2] = value;
				}
				saveEntryInfo.Name = sdDirectoryInfo.Name;
				saveEntryInfo.WorldEntry = value;
				saveEntryInfo.LastSaved = sdDirectoryInfo.LastWriteTime;
				saveEntryInfo.Version = null;
			}
			saveEntryInfos.Add(saveEntryInfo);
			string saveEntryKey = GetSaveEntryKey(saveEntryInfo.WorldEntry.WorldKey, saveEntryInfo.Name);
			saveEntryInfosBySaveKey[saveEntryKey] = saveEntryInfo;
			remoteSaveEntryInfosByGuid[sdDirectoryInfo.Name] = saveEntryInfo;
			value.SaveEntryInfos.Add(saveEntryInfo);
			value.SaveDataCount++;
			value.SaveDataSize += saveEntryInfo.SizeInfo.ReportedSize;
			remoteSavesSum += saveEntryInfo.SizeInfo.ReportedSize;
		}
	}

	public bool TryGetLocalSaveEntry(string worldName, string saveName, out SaveEntryInfo saveEntryInfo)
	{
		RefreshIfDirty();
		string saveEntryKey = GetSaveEntryKey(GetWorldEntryKey(worldName, "Local"), saveName);
		return saveEntryInfosBySaveKey.TryGetValue(saveEntryKey, out saveEntryInfo);
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

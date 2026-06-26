using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public interface ISaveDataManager
{
	event Action CommitStarted;

	event Action CommitFinished;

	void Init();

	void Cleanup();

	SaveDataWriteMode GetWriteMode();

	void SetWriteMode(SaveDataWriteMode writeMode);

	void RegisterRegionFileManager(RegionFileManager regionFileManager);

	void DeregisterRegionFileManager(RegionFileManager regionFileManager);

	void CommitAsync();

	void CommitSync();

	IEnumerator CommitCoroutine();

	bool ShouldLimitSize();

	void UpdateSizes();

	SaveDataSizes GetSizes();

	Stream ManagedFileOpen(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share);

	void ManagedFileDelete(SaveDataManagedPath path);

	bool ManagedFileExists(SaveDataManagedPath path);

	DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path);

	SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path);

	DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path);

	bool ManagedDirectoryExists(SaveDataManagedPath path);

	IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption);

	IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption);

	IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption);

	void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive);

	long ManagedFileInfoLength(SaveDataManagedPath path);

	IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption);

	IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption);

	IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption);
}

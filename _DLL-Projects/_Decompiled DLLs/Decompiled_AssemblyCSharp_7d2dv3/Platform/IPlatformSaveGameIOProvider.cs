using System;
using System.Collections.Generic;
using System.IO;

namespace Platform;

public interface IPlatformSaveGameIOProvider
{
	void ManagedFileRead(SaveDataManagedPath path, Stream dest);

	void ManagedFileWrite(SaveDataManagedPath path, Stream src);

	void ManagedFileCopy(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName, bool overwrite = false);

	void ManagedFileDelete(SaveDataManagedPath path);

	bool ManagedFileExists(SaveDataManagedPath path);

	DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path);

	void ManagedFileMove(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName);

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

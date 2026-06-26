using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SaveDataManager_Placeholder : ISaveDataManager
{
	public static readonly SaveDataManager_Placeholder Instance = new SaveDataManager_Placeholder();

	public event Action CommitStarted
	{
		add
		{
		}
		remove
		{
		}
	}

	public event Action CommitFinished
	{
		add
		{
		}
		remove
		{
		}
	}

	public void Init()
	{
	}

	public SaveDataWriteMode GetWriteMode()
	{
		return SaveDataWriteMode.None;
	}

	public void SetWriteMode(SaveDataWriteMode writeMode)
	{
	}

	public void Cleanup()
	{
	}

	public void RegisterRegionFileManager(RegionFileManager regionFileManager)
	{
	}

	public void DeregisterRegionFileManager(RegionFileManager regionFileManager)
	{
	}

	public void CommitAsync()
	{
	}

	public void CommitSync()
	{
	}

	public IEnumerator CommitCoroutine()
	{
		yield break;
	}

	public virtual bool ShouldLimitSize()
	{
		return false;
	}

	public virtual void UpdateSizes()
	{
	}

	public virtual SaveDataSizes GetSizes()
	{
		return default(SaveDataSizes);
	}

	public virtual Stream ManagedFileOpen(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
	{
		return File.Open(path.GetOriginalPath(), mode, access, share);
	}

	public virtual void ManagedFileDelete(SaveDataManagedPath path)
	{
		File.Delete(path.GetOriginalPath());
	}

	public virtual bool ManagedFileExists(SaveDataManagedPath path)
	{
		return File.Exists(path.GetOriginalPath());
	}

	public virtual DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return File.GetLastWriteTimeUtc(path.GetOriginalPath());
	}

	public virtual SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
	{
		return new SdDirectoryInfo(Directory.CreateDirectory(path.GetOriginalPath()));
	}

	public virtual DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return Directory.GetLastWriteTimeUtc(path.GetOriginalPath());
	}

	public virtual bool ManagedDirectoryExists(SaveDataManagedPath path)
	{
		return Directory.Exists(path.GetOriginalPath());
	}

	public virtual IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return ConvertPathsToManagedPaths(Directory.EnumerateDirectories(path.GetOriginalPath(), searchPattern, searchOption));
	}

	public virtual IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return ConvertPathsToManagedPaths(Directory.EnumerateFiles(path.GetOriginalPath(), searchPattern, searchOption));
	}

	public virtual IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return ConvertPathsToManagedPaths(Directory.EnumerateFileSystemEntries(path.GetOriginalPath(), searchPattern, searchOption));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<SaveDataManagedPath> ConvertPathsToManagedPaths(IEnumerable<string> paths)
	{
		foreach (string path in paths)
		{
			if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
			{
				yield return managedPath;
			}
		}
	}

	public virtual void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
	{
		Directory.Delete(path.GetOriginalPath(), recursive);
	}

	public long ManagedFileInfoLength(SaveDataManagedPath path)
	{
		return new FileInfo(path.GetOriginalPath()).Length;
	}

	public virtual IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return from x in new DirectoryInfo(path.GetOriginalPath()).EnumerateDirectories(searchPattern, searchOption)
			select new SdDirectoryInfo(x);
	}

	public virtual IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return from x in new DirectoryInfo(path.GetOriginalPath()).EnumerateFiles(searchPattern, searchOption)
			select new SdFileInfo(x);
	}

	public virtual IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return new DirectoryInfo(path.GetOriginalPath()).EnumerateFileSystemInfos(searchPattern, searchOption).Select(SdDirectoryInfo.WrapFileSystemInfo);
	}
}

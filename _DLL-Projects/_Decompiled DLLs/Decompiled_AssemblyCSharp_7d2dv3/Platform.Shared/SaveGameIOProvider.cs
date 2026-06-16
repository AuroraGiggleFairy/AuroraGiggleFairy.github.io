using System;
using System.Collections.Generic;
using System.IO;

namespace Platform.Shared;

public abstract class SaveGameIOProvider : IPlatformSaveGameIOProvider
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract string GetPath(SaveDataManagedPath path);

	public void ManagedFileRead(SaveDataManagedPath path, Stream dest)
	{
		using FileStream source = File.OpenRead(GetPath(path));
		StreamUtils.StreamCopy(source, dest);
	}

	public void ManagedFileWrite(SaveDataManagedPath path, Stream src)
	{
		using FileStream destination = File.Open(GetPath(path), FileMode.Create, FileAccess.Write, FileShare.Read);
		StreamUtils.StreamCopy(src, destination);
	}

	public void ManagedFileCopy(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName, bool overwrite = false)
	{
		File.Copy(GetPath(sourceFileName), GetPath(destFileName), overwrite);
	}

	public void ManagedFileDelete(SaveDataManagedPath path)
	{
		File.Delete(GetPath(path));
	}

	public bool ManagedFileExists(SaveDataManagedPath path)
	{
		return File.Exists(GetPath(path));
	}

	public DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return File.GetLastWriteTimeUtc(GetPath(path));
	}

	public void ManagedFileMove(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName)
	{
		File.Move(GetPath(sourceFileName), GetPath(destFileName));
	}

	public SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
	{
		return new SdDirectoryInfo(Directory.CreateDirectory(GetPath(path)));
	}

	public DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return Directory.GetLastWriteTimeUtc(GetPath(path));
	}

	public bool ManagedDirectoryExists(SaveDataManagedPath path)
	{
		return Directory.Exists(GetPath(path));
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		string path2 = GetPath(path);
		return SaveGameProviderHelper.GetManagedPathsFromBaseAndSubPaths(path, path2, Directory.EnumerateDirectories(path2, searchPattern, searchOption));
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		string path2 = GetPath(path);
		return SaveGameProviderHelper.GetManagedPathsFromBaseAndSubPaths(path, path2, Directory.EnumerateFiles(path2, searchPattern, searchOption));
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		string path2 = GetPath(path);
		return SaveGameProviderHelper.GetManagedPathsFromBaseAndSubPaths(path, path2, Directory.EnumerateFileSystemEntries(path2, searchPattern, searchOption));
	}

	public void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
	{
		Directory.Delete(GetPath(path), recursive);
	}

	public long ManagedFileInfoLength(SaveDataManagedPath path)
	{
		return new FileInfo(GetPath(path)).Length;
	}

	public IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		string basePath = GetPath(path);
		DirectoryInfo directoryInfo = new DirectoryInfo(basePath);
		foreach (DirectoryInfo item in directoryInfo.EnumerateDirectories(searchPattern, searchOption))
		{
			string fullName = item.FullName;
			SaveDataManagedPath managedPathFromBaseAndSubPath = SaveGameProviderHelper.GetManagedPathFromBaseAndSubPath(path, basePath, fullName);
			yield return new SdDirectoryInfo(managedPathFromBaseAndSubPath);
		}
	}

	public IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		string basePath = GetPath(path);
		DirectoryInfo directoryInfo = new DirectoryInfo(basePath);
		foreach (FileInfo item in directoryInfo.EnumerateFiles(searchPattern, searchOption))
		{
			string fullName = item.FullName;
			SaveDataManagedPath managedPathFromBaseAndSubPath = SaveGameProviderHelper.GetManagedPathFromBaseAndSubPath(path, basePath, fullName);
			yield return new SdFileInfo(managedPathFromBaseAndSubPath);
		}
	}

	public IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		string basePath = GetPath(path);
		DirectoryInfo directoryInfo = new DirectoryInfo(basePath);
		foreach (FileSystemInfo item in directoryInfo.EnumerateFileSystemInfos(searchPattern, searchOption))
		{
			string fullName = item.FullName;
			SaveDataManagedPath managedPathFromBaseAndSubPath = SaveGameProviderHelper.GetManagedPathFromBaseAndSubPath(path, basePath, fullName);
			SdFileSystemInfo sdFileSystemInfo;
			if (!(item is FileInfo))
			{
				if (!(item is DirectoryInfo))
				{
					throw new NotImplementedException("Unsupported implementation of FileSystemInfo: " + item.GetType().FullName + ".");
				}
				sdFileSystemInfo = new SdDirectoryInfo(managedPathFromBaseAndSubPath);
			}
			else
			{
				sdFileSystemInfo = new SdFileInfo(managedPathFromBaseAndSubPath);
			}
			yield return sdFileSystemInfo;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public SaveGameIOProvider()
	{
	}
}

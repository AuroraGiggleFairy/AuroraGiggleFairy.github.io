using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class SdDirectory
{
	public static SdDirectoryInfo CreateDirectory(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedCreateDirectory(managedPath);
		}
		return new SdDirectoryInfo(Directory.CreateDirectory(path));
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static SdDirectoryInfo ManagedCreateDirectory(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryCreateDirectory(path);
	}

	public static DateTime GetLastWriteTime(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedGetLastWriteTime(managedPath);
		}
		return Directory.GetLastWriteTime(path);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime ManagedGetLastWriteTime(SaveDataManagedPath path)
	{
		return ManagedGetLastWriteTimeUtc(path).ToLocalTime();
	}

	public static DateTime GetLastWriteTimeUtc(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedGetLastWriteTimeUtc(managedPath);
		}
		return Directory.GetLastWriteTimeUtc(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static DateTime ManagedGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryGetLastWriteTimeUtc(path);
	}

	public static bool Exists(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedExists(managedPath);
		}
		return Directory.Exists(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool ManagedExists(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryExists(path);
	}

	public static string[] GetFiles(string path)
	{
		return EnumerateFiles(path).ToArray();
	}

	public static string[] GetFiles(string path, string searchPattern)
	{
		return EnumerateFiles(path, searchPattern).ToArray();
	}

	public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
	{
		return EnumerateFiles(path, searchPattern, searchOption).ToArray();
	}

	public static string[] GetDirectories(string path)
	{
		return EnumerateDirectories(path).ToArray();
	}

	public static string[] GetDirectories(string path, string searchPattern)
	{
		return EnumerateDirectories(path, searchPattern).ToArray();
	}

	public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
	{
		return EnumerateDirectories(path, searchPattern, searchOption).ToArray();
	}

	public static string[] GetFileSystemEntries(string path)
	{
		return EnumerateFileSystemEntries(path).ToArray();
	}

	public static string[] GetFileSystemEntries(string path, string searchPattern)
	{
		return EnumerateFileSystemEntries(path, searchPattern).ToArray();
	}

	public static string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
	{
		return EnumerateFileSystemEntries(path, searchPattern, searchOption).ToArray();
	}

	public static IEnumerable<string> EnumerateDirectories(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return from x in ManagedEnumerateDirectories(managedPath, "*", SearchOption.TopDirectoryOnly)
				select x.GetOriginalPath();
		}
		return Directory.EnumerateDirectories(path);
	}

	public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return from x in ManagedEnumerateDirectories(managedPath, searchPattern, SearchOption.TopDirectoryOnly)
				select x.GetOriginalPath();
		}
		return Directory.EnumerateDirectories(path, searchPattern);
	}

	public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return from x in ManagedEnumerateDirectories(managedPath, searchPattern, searchOption)
				select x.GetOriginalPath();
		}
		return Directory.EnumerateDirectories(path, searchPattern, searchOption);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<SaveDataManagedPath> ManagedEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryEnumerateDirectories(path, searchPattern, searchOption);
	}

	public static IEnumerable<string> EnumerateFiles(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return from x in ManagedEnumerateFiles(managedPath, "*", SearchOption.TopDirectoryOnly)
				select x.GetOriginalPath();
		}
		return Directory.EnumerateFiles(path);
	}

	public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return from x in ManagedEnumerateFiles(managedPath, searchPattern, SearchOption.TopDirectoryOnly)
				select x.GetOriginalPath();
		}
		return Directory.EnumerateFiles(path, searchPattern);
	}

	public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return from x in ManagedEnumerateFiles(managedPath, searchPattern, searchOption)
				select x.GetOriginalPath();
		}
		return Directory.EnumerateFiles(path, searchPattern, searchOption);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<SaveDataManagedPath> ManagedEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryEnumerateFiles(path, searchPattern, searchOption);
	}

	public static IEnumerable<string> EnumerateFileSystemEntries(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return from x in ManagedEnumerateFileSystemEntries(managedPath, "*", SearchOption.TopDirectoryOnly)
				select x.GetOriginalPath();
		}
		return Directory.EnumerateFileSystemEntries(path);
	}

	public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return from x in ManagedEnumerateFileSystemEntries(managedPath, searchPattern, SearchOption.TopDirectoryOnly)
				select x.GetOriginalPath();
		}
		return Directory.EnumerateFileSystemEntries(path, searchPattern);
	}

	public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return from x in ManagedEnumerateFileSystemEntries(managedPath, searchPattern, searchOption)
				select x.GetOriginalPath();
		}
		return Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<SaveDataManagedPath> ManagedEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryEnumerateFileSystemEntries(path, searchPattern, searchOption);
	}

	public static string[] GetLogicalDrives()
	{
		return Directory.GetLogicalDrives();
	}

	public static string GetDirectoryRoot(string path)
	{
		return Directory.GetDirectoryRoot(path);
	}

	public static string GetCurrentDirectory()
	{
		return Directory.GetCurrentDirectory();
	}

	public static void SetCurrentDirectory(string path)
	{
		Directory.SetCurrentDirectory(path);
	}

	public static void Delete(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedDelete(managedPath, recursive: false);
		}
		else
		{
			Directory.Delete(path);
		}
	}

	public static void Delete(string path, bool recursive)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedDelete(managedPath, recursive);
		}
		else
		{
			Directory.Delete(path, recursive);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedDelete(SaveDataManagedPath path, bool recursive)
	{
		SaveDataUtils.SaveDataManager.ManagedDirectoryDelete(path, recursive);
	}
}

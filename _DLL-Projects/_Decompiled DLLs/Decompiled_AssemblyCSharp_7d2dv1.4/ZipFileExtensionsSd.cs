using System;
using System.IO;
using System.IO.Compression;

public static class ZipFileExtensionsSd
{
	public static void CreateFromDirectory(this ZipArchive archiveZip, string saveDir)
	{
		Log.Out("[BACKTRACE] CreateFromDirectory Path: " + saveDir);
		archiveZip.CreateFromDirectory(saveDir, CompressionLevel.Optimal, includeBaseDirectory: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateFromDirectory(this ZipArchive destination, string sourceDirectoryName, CompressionLevel compressionLevel, bool includeBaseDirectory)
	{
		bool flag = true;
		SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(sourceDirectoryName);
		string fullName = sdDirectoryInfo.FullName;
		if (includeBaseDirectory && sdDirectoryInfo.Parent != null)
		{
			fullName = sdDirectoryInfo.Parent.FullName;
		}
		Log.Out("[BACKTRACE] Checking Path: " + fullName);
		foreach (SdFileSystemInfo item in sdDirectoryInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
		{
			Log.Out("[BACKTRACE] Adding Path: " + item.FullName);
			flag = false;
			int length = item.FullName.Length - fullName.Length;
			string text = EntryFromPath(item.FullName, fullName.Length, length);
			if (item is SdFileInfo)
			{
				destination.CreateEntryFromFile(item, text, compressionLevel);
			}
			else if (item is SdDirectoryInfo sdDirectoryInfo2 && sdDirectoryInfo2.IsDirEmpty())
			{
				destination.CreateEntry(text + "/");
			}
		}
		if (includeBaseDirectory && flag)
		{
			string text2 = EntryFromPath(sdDirectoryInfo.Name, 0, sdDirectoryInfo.Name.Length);
			destination.CreateEntry(text2 + "/");
		}
	}

	public static ZipArchiveEntry CreateEntryFromFile(this ZipArchive destination, SdFileSystemInfo directoryInfo, string entryName, CompressionLevel compressionLevel)
	{
		string fullName = directoryInfo.FullName;
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (fullName == null)
		{
			throw new ArgumentNullException("sourceFileName");
		}
		if (entryName == null)
		{
			throw new ArgumentNullException("entryName");
		}
		using Stream stream = SdFile.Open(fullName, FileMode.Open, FileAccess.Read, FileShare.Read);
		ZipArchiveEntry zipArchiveEntry = destination.CreateEntry(entryName, compressionLevel);
		DateTime dateTime = SdFile.GetLastWriteTime(fullName);
		if (dateTime.Year < 1980 || dateTime.Year > 2107)
		{
			dateTime = new DateTime(1980, 1, 1, 0, 0, 0);
		}
		zipArchiveEntry.LastWriteTime = dateTime;
		using (Stream destination2 = zipArchiveEntry.Open())
		{
			stream.CopyTo(destination2);
		}
		return zipArchiveEntry;
	}

	public static void AddSearchPattern(this ZipArchive destination, SdDirectoryInfo directoryInfo, string pattern, SearchOption options)
	{
		string fullName = directoryInfo.FullName;
		Log.Out("[BACKTRACE] Directory full name: " + directoryInfo.FullName + ", Pattern: " + pattern);
		foreach (SdFileSystemInfo item in directoryInfo.EnumerateFileSystemInfos(pattern, options))
		{
			Log.Out("[BACKTRACE] Found full name: " + item.FullName);
			int length = item.FullName.Length - fullName.Length;
			string text = EntryFromPath(item.FullName, fullName.Length, length);
			if (item is SdFileInfo)
			{
				destination.CreateEntryFromFile(item, text, CompressionLevel.Optimal);
			}
			else if (item is SdDirectoryInfo sdDirectoryInfo && sdDirectoryInfo.IsDirEmpty())
			{
				char directorySeparatorChar = Path.DirectorySeparatorChar;
				destination.CreateEntry(text + directorySeparatorChar, CompressionLevel.Optimal);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string EntryFromPath(string entry, int offset, int length)
	{
		while (length > 0 && (entry[offset] == Path.DirectorySeparatorChar || entry[offset] == Path.AltDirectorySeparatorChar))
		{
			offset++;
			length--;
		}
		if (length == 0)
		{
			return string.Empty;
		}
		char[] array = entry.ToCharArray(offset, length);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == Path.DirectorySeparatorChar || array[i] == Path.AltDirectorySeparatorChar)
			{
				array[i] = Path.DirectorySeparatorChar;
			}
		}
		return new string(array);
	}
}

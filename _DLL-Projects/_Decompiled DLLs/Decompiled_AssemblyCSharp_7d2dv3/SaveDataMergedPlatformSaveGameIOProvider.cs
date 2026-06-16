using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Platform;

public class SaveDataMergedPlatformSaveGameIOProvider : IPlatformSaveGameProvider, IPlatformSaveGameIOProvider
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string CACHE_OWNER_FILENAME = "CacheOwner.txt";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string SAVE_OWNER_FILENAME = "SaveOwner.txt";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex s_cachedManagedPathRegex = CreateCachedManagedPathRegex();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IPlatformSaveGameProvider m_main;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IPlatformSaveGameIOProvider m_cache;

	public ESaveGameProviderStatus Status => m_main.Status;

	public IPlatformSaveGameIOProvider Cache => null;

	public event Action Initialized
	{
		add
		{
			m_main.Initialized += value;
		}
		remove
		{
			m_main.Initialized -= value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string text)
	{
		Log.Out("[SaveDataMergedPlatformSaveGameIOProvider] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Regex CreateCachedManagedPathRegex()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("^(?:");
		stringBuilder.Append("(?:");
		stringBuilder.Append("(?:");
		stringBuilder.Append("(?:");
		stringBuilder.Append("Saves");
		stringBuilder.Append("[/]");
		stringBuilder.Append("[^/]+");
		stringBuilder.Append("[/]");
		stringBuilder.Append("[^/]+");
		stringBuilder.Append(')');
		stringBuilder.Append('|');
		stringBuilder.Append("(?:");
		stringBuilder.Append("SavesLocal");
		stringBuilder.Append("[/]");
		stringBuilder.Append("[^/]+");
		stringBuilder.Append(')');
		stringBuilder.Append(')');
		stringBuilder.Append("[/]");
		stringBuilder.Append("DynamicMeshes");
		stringBuilder.Append(')');
		stringBuilder.Append('|');
		stringBuilder.Append(Regex.Escape("CacheOwner.txt"));
		stringBuilder.Append('|');
		stringBuilder.Append(Regex.Escape("GeneratedWorlds"));
		stringBuilder.Append('/');
		stringBuilder.Append("[^/]+?");
		stringBuilder.Append('/');
		stringBuilder.Append("(?:");
		stringBuilder.Append(Regex.Escape("checksums.txt"));
		stringBuilder.Append('|');
		stringBuilder.Append("[^/]+?(?:");
		stringBuilder.Append(Regex.Escape("_processed"));
		stringBuilder.Append('|');
		stringBuilder.Append(Regex.Escape("_half"));
		stringBuilder.Append(")\\.(?:");
		stringBuilder.Append(Regex.Escape("raw"));
		stringBuilder.Append('|');
		stringBuilder.Append(Regex.Escape("png"));
		stringBuilder.Append('|');
		stringBuilder.Append(Regex.Escape("tga"));
		stringBuilder.Append(')');
		stringBuilder.Append(')');
		stringBuilder.Append(')');
		stringBuilder.Append("(?:");
		stringBuilder.Append(Regex.Escape(".bup"));
		stringBuilder.Append(")*");
		stringBuilder.Append("(?:/|$)");
		return new Regex(stringBuilder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
	}

	public SaveDataMergedPlatformSaveGameIOProvider(IPlatformSaveGameProvider main)
	{
		m_main = main;
		m_cache = main.Cache;
		try
		{
			InitialSaveCacheSync();
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsCached(SaveDataManagedPath path)
	{
		if (m_cache != null)
		{
			return s_cachedManagedPathRegex.IsMatch(path.PathRelativeToRoot);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatformSaveGameIOProvider GetIOProvider(SaveDataManagedPath path)
	{
		if (!IsCached(path))
		{
			return m_main;
		}
		return m_cache;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SyncParentDirectories(SaveDataManagedPath path)
	{
		if (IsCached(path) && !m_cache.ManagedFileExists(path) && !m_cache.ManagedDirectoryExists(path) && path.TryGetParentPath(out var parentPath) && parentPath.PathRelativeToRoot.Length > 0 && !m_cache.ManagedDirectoryExists(parentPath) && m_main.ManagedDirectoryExists(parentPath))
		{
			m_cache.ManagedDirectoryCreateDirectory(parentPath);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitialSaveCacheSync()
	{
		if (m_cache == null)
		{
			return;
		}
		using MemoryStream buffer = new MemoryStream();
		InitEnsureSameCacheUser(buffer);
		InitMoveToCache(buffer);
		InitRemoveInvalidCachedFiles();
		InitRemoveCachedGeneratedWorlds();
		InitRemoveEmptyCacheDirectories();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitEnsureSameCacheUser(MemoryStream buffer)
	{
		SaveDataManagedPath cacheOwnerPath = new SaveDataManagedPath("CacheOwner.txt");
		SaveDataManagedPath saveOwnerPath = new SaveDataManagedPath("SaveOwner.txt");
		if (!m_main.ManagedFileExists(saveOwnerPath))
		{
			LogInfo("Cache needs a reset because the save owner is missing.");
			ResetCache();
			return;
		}
		if (!m_cache.ManagedFileExists(cacheOwnerPath))
		{
			LogInfo("Cache needs a reset because the cache owner is missing.");
			ResetCache();
			return;
		}
		buffer.Position = 0L;
		buffer.SetLength(0L);
		m_main.ManagedFileRead(saveOwnerPath, buffer);
		buffer.Position = 0L;
		string text;
		using (StreamReader streamReader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true))
		{
			text = streamReader.ReadToEnd();
		}
		buffer.Position = 0L;
		buffer.SetLength(0L);
		m_cache.ManagedFileRead(cacheOwnerPath, buffer);
		buffer.Position = 0L;
		string text2;
		using (StreamReader streamReader2 = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true))
		{
			text2 = streamReader2.ReadToEnd();
		}
		if (!(text == text2))
		{
			LogInfo("Cache needs a reset because the save owner '" + text + "' does not match the cache owner '" + text2 + "'.");
			ResetCache();
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void ResetCache()
		{
			foreach (SaveDataManagedPath item in m_cache.ManagedDirectoryEnumerateDirectories(SaveDataManagedPath.RootPath, "*", SearchOption.TopDirectoryOnly))
			{
				m_cache.ManagedDirectoryDelete(item, recursive: true);
			}
			foreach (SaveDataManagedPath item2 in m_cache.ManagedDirectoryEnumerateFiles(SaveDataManagedPath.RootPath, "*", SearchOption.TopDirectoryOnly))
			{
				m_cache.ManagedFileDelete(item2);
			}
			buffer.Position = 0L;
			buffer.SetLength(0L);
			if (!m_main.ManagedFileExists(saveOwnerPath))
			{
				using (StreamWriter streamWriter = new StreamWriter(buffer, Encoding.UTF8, 1024, leaveOpen: true))
				{
					streamWriter.Write(Guid.NewGuid());
				}
				buffer.Position = 0L;
				m_main.ManagedFileWrite(saveOwnerPath, buffer);
			}
			else
			{
				m_main.ManagedFileRead(saveOwnerPath, buffer);
			}
			buffer.Position = 0L;
			m_cache.ManagedFileWrite(cacheOwnerPath, buffer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitMoveToCache(MemoryStream buffer)
	{
		foreach (SaveDataManagedPath item in m_main.ManagedDirectoryEnumerateFiles(SaveDataManagedPath.RootPath, "*", SearchOption.AllDirectories))
		{
			if (IsCached(item))
			{
				if (item.TryGetParentPath(out var parentPath))
				{
					m_cache.ManagedDirectoryCreateDirectory(parentPath);
				}
				buffer.Position = 0L;
				buffer.SetLength(0L);
				m_main.ManagedFileRead(item, buffer);
				buffer.Position = 0L;
				m_cache.ManagedFileWrite(item, buffer);
				m_main.ManagedFileDelete(item);
				LogInfo($"Moved from save data to cache: {item}");
			}
		}
		foreach (SaveDataManagedPath item2 in from p in m_main.ManagedDirectoryEnumerateDirectories(SaveDataManagedPath.RootPath, "*", SearchOption.AllDirectories)
			orderby p.PathRelativeToRoot.Count([PublicizedFrom(EAccessModifier.Internal)] (char c) => c == '/') descending
			select p)
		{
			if (IsCached(item2) && m_main.ManagedDirectoryExists(item2) && !m_main.ManagedDirectoryEnumerateFileSystemEntries(item2, "*", SearchOption.TopDirectoryOnly).Any())
			{
				m_main.ManagedDirectoryDelete(item2, recursive: false);
				LogInfo($"Deleted cache directory remaining in main: {item2}");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRemoveInvalidCachedFiles()
	{
		foreach (SaveDataManagedPath item in m_cache.ManagedDirectoryEnumerateFiles(SaveDataManagedPath.RootPath, "*", SearchOption.AllDirectories))
		{
			if (!IsCached(item))
			{
				m_cache.ManagedFileDelete(item);
				LogInfo($"Deleted invalid cached file: {item}");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRemoveCachedGeneratedWorlds()
	{
		SaveDataManagedPath path = new SaveDataManagedPath("GeneratedWorlds");
		if (!m_cache.ManagedDirectoryExists(path))
		{
			return;
		}
		foreach (SaveDataManagedPath item in m_cache.ManagedDirectoryEnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
		{
			if (!m_main.ManagedDirectoryExists(item))
			{
				m_cache.ManagedDirectoryDelete(item, recursive: true);
				LogInfo($"Delete cache for non-existent generated world: {item}");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRemoveEmptyCacheDirectories()
	{
		foreach (SaveDataManagedPath item in from p in m_cache.ManagedDirectoryEnumerateDirectories(SaveDataManagedPath.RootPath, "*", SearchOption.AllDirectories)
			orderby p.PathRelativeToRoot.Count([PublicizedFrom(EAccessModifier.Internal)] (char c) => c == '/') descending
			select p)
		{
			if (m_cache.ManagedDirectoryExists(item) && !m_cache.ManagedDirectoryEnumerateFileSystemEntries(item, "*", SearchOption.TopDirectoryOnly).Any())
			{
				m_cache.ManagedDirectoryDelete(item, recursive: false);
				LogInfo($"Deleted empty cache directory: {item}");
			}
		}
	}

	public void Init(IPlatform _owner)
	{
		m_main.Init(_owner);
	}

	public void Destroy()
	{
		m_main.Destroy();
	}

	public bool ShouldBackup()
	{
		return m_main.ShouldBackup();
	}

	public bool ShouldCommit()
	{
		return m_main.ShouldCommit();
	}

	public double GetCommitProgress()
	{
		return m_main.GetCommitProgress();
	}

	public void Flush(bool waitForFlush)
	{
		m_main.Flush(waitForFlush);
	}

	public bool ShouldLimitSize()
	{
		return m_main.ShouldLimitSize();
	}

	public void UpdateSizes()
	{
		m_main.UpdateSizes();
	}

	public SaveDataSizes GetSizes()
	{
		return m_main.GetSizes();
	}

	public void ManagedFileRead(SaveDataManagedPath path, Stream dest)
	{
		GetIOProvider(path).ManagedFileRead(path, dest);
	}

	public void ManagedFileWrite(SaveDataManagedPath path, Stream src)
	{
		SyncParentDirectories(path);
		GetIOProvider(path).ManagedFileWrite(path, src);
	}

	public void ManagedFileCopy(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName, bool overwrite = false)
	{
		IPlatformSaveGameIOProvider iOProvider = GetIOProvider(sourceFileName);
		IPlatformSaveGameIOProvider iOProvider2 = GetIOProvider(destFileName);
		if (iOProvider == iOProvider2)
		{
			iOProvider.ManagedFileCopy(sourceFileName, destFileName, overwrite);
			return;
		}
		if (!overwrite && iOProvider2.ManagedFileExists(destFileName))
		{
			throw new IOException($"File already exists at '{destFileName}'.");
		}
		SyncParentDirectories(destFileName);
		using MemoryStream memoryStream = new MemoryStream();
		iOProvider.ManagedFileRead(sourceFileName, memoryStream);
		memoryStream.Position = 0L;
		iOProvider2.ManagedFileWrite(destFileName, memoryStream);
	}

	public void ManagedFileDelete(SaveDataManagedPath path)
	{
		GetIOProvider(path).ManagedFileDelete(path);
	}

	public bool ManagedFileExists(SaveDataManagedPath path)
	{
		return GetIOProvider(path).ManagedFileExists(path);
	}

	public DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return GetIOProvider(path).ManagedFileGetLastWriteTimeUtc(path);
	}

	public void ManagedFileMove(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName)
	{
		IPlatformSaveGameIOProvider iOProvider = GetIOProvider(sourceFileName);
		IPlatformSaveGameIOProvider iOProvider2 = GetIOProvider(destFileName);
		if (iOProvider == iOProvider2)
		{
			iOProvider.ManagedFileMove(sourceFileName, destFileName);
			return;
		}
		if (iOProvider2.ManagedFileExists(destFileName))
		{
			throw new IOException($"File already exists at '{destFileName}'.");
		}
		SyncParentDirectories(destFileName);
		using (MemoryStream memoryStream = new MemoryStream())
		{
			iOProvider.ManagedFileRead(sourceFileName, memoryStream);
			memoryStream.Position = 0L;
			iOProvider2.ManagedFileWrite(destFileName, memoryStream);
		}
		iOProvider.ManagedFileDelete(sourceFileName);
	}

	public SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
	{
		return GetIOProvider(path).ManagedDirectoryCreateDirectory(path);
	}

	public DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return GetIOProvider(path).ManagedDirectoryGetLastWriteTimeUtc(path);
	}

	public bool ManagedDirectoryExists(SaveDataManagedPath path)
	{
		return GetIOProvider(path).ManagedDirectoryExists(path);
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return ManagedDirectoryEnumerateInternal(path, [PublicizedFrom(EAccessModifier.Internal)] (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryEnumerateDirectories(path, searchPattern, searchOption));
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return ManagedDirectoryEnumerateInternal(path, [PublicizedFrom(EAccessModifier.Internal)] (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryEnumerateFiles(path, searchPattern, searchOption));
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return ManagedDirectoryEnumerateInternal(path, [PublicizedFrom(EAccessModifier.Internal)] (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryEnumerateFileSystemEntries(path, searchPattern, searchOption));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateInternal(SaveDataManagedPath path, Func<IPlatformSaveGameIOProvider, IEnumerable<SaveDataManagedPath>> getEnumerable)
	{
		if (m_cache == null)
		{
			return getEnumerable(m_main);
		}
		if (IsCached(path))
		{
			return getEnumerable(m_cache).Where(IsCached);
		}
		if (!m_cache.ManagedDirectoryExists(path))
		{
			return from p in getEnumerable(m_main)
				where !IsCached(p)
				select p;
		}
		if (!m_main.ManagedDirectoryExists(path))
		{
			throw new IOException($"Directory does not exist at '{path}'.");
		}
		return EnumerateBoth();
		[PublicizedFrom(EAccessModifier.Internal)]
		IEnumerable<SaveDataManagedPath> EnumerateBoth()
		{
			using IEnumerator<SaveDataManagedPath> mainPaths = getEnumerable(m_main).GetEnumerator();
			using IEnumerator<SaveDataManagedPath> cachePaths = getEnumerable(m_cache).GetEnumerator();
			HashSet<SaveDataManagedPath> seenPaths = new HashSet<SaveDataManagedPath>();
			bool mainHasNext = mainPaths.MoveNext();
			bool cacheHasNext = cachePaths.MoveNext();
			while (mainHasNext && cacheHasNext)
			{
				SaveDataManagedPath current = mainPaths.Current;
				SaveDataManagedPath current2 = cachePaths.Current;
				if (current <= current2)
				{
					if (!IsCached(current) && seenPaths.Add(current))
					{
						yield return current;
					}
					mainHasNext = mainPaths.MoveNext();
				}
				else
				{
					if (IsCached(current2) && seenPaths.Add(current2))
					{
						yield return current2;
					}
					cacheHasNext = cachePaths.MoveNext();
				}
			}
			while (mainHasNext)
			{
				SaveDataManagedPath current3 = mainPaths.Current;
				if (!IsCached(current3) && seenPaths.Add(current3))
				{
					yield return current3;
				}
				mainHasNext = mainPaths.MoveNext();
			}
			while (cacheHasNext)
			{
				SaveDataManagedPath current4 = cachePaths.Current;
				if (IsCached(current4) && seenPaths.Add(current4))
				{
					yield return current4;
				}
				cacheHasNext = cachePaths.MoveNext();
			}
		}
	}

	public void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
	{
		if (IsCached(path))
		{
			m_cache.ManagedDirectoryDelete(path, recursive);
			return;
		}
		m_main.ManagedDirectoryDelete(path, recursive);
		if (m_cache != null && m_cache.ManagedDirectoryExists(path))
		{
			m_cache.ManagedDirectoryDelete(path, recursive);
		}
	}

	public long ManagedFileInfoLength(SaveDataManagedPath path)
	{
		if (IsCached(path))
		{
			return 0L;
		}
		return m_main.ManagedFileInfoLength(path);
	}

	public IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return ManagedDirectoryInfoEnumerateInternal(path, [PublicizedFrom(EAccessModifier.Internal)] (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryInfoEnumerateDirectories(path, searchPattern, searchOption));
	}

	public IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return ManagedDirectoryInfoEnumerateInternal(path, [PublicizedFrom(EAccessModifier.Internal)] (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryInfoEnumerateFiles(path, searchPattern, searchOption));
	}

	public IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return ManagedDirectoryInfoEnumerateInternal(path, [PublicizedFrom(EAccessModifier.Internal)] (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryInfoEnumerateFileSystemInfos(path, searchPattern, searchOption));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<T> ManagedDirectoryInfoEnumerateInternal<T>(SaveDataManagedPath path, Func<IPlatformSaveGameIOProvider, IEnumerable<T>> getEnumerable) where T : SdFileSystemInfo
	{
		if (m_cache == null)
		{
			return getEnumerable(m_main);
		}
		if (IsCached(path))
		{
			return from info in getEnumerable(m_cache)
				where IsCached(info.ManagedPath)
				select info;
		}
		if (!m_cache.ManagedDirectoryExists(path))
		{
			return from info in getEnumerable(m_main)
				where !IsCached(info.ManagedPath)
				select info;
		}
		if (!m_main.ManagedDirectoryExists(path))
		{
			throw new IOException($"Directory does not exist at '{path}'.");
		}
		return EnumerateBoth();
		[PublicizedFrom(EAccessModifier.Internal)]
		IEnumerable<T> EnumerateBoth()
		{
			using IEnumerator<T> mainPaths = getEnumerable(m_main).GetEnumerator();
			using IEnumerator<T> cachePaths = getEnumerable(m_cache).GetEnumerator();
			HashSet<SaveDataManagedPath> seenPaths = new HashSet<SaveDataManagedPath>();
			bool mainHasNext = mainPaths.MoveNext();
			bool cacheHasNext = cachePaths.MoveNext();
			while (mainHasNext && cacheHasNext)
			{
				T current = mainPaths.Current;
				SaveDataManagedPath managedPath = current.ManagedPath;
				T current2 = cachePaths.Current;
				SaveDataManagedPath managedPath2 = current2.ManagedPath;
				if (managedPath <= managedPath2)
				{
					if (!IsCached(managedPath) && seenPaths.Add(managedPath))
					{
						yield return current;
					}
					mainHasNext = mainPaths.MoveNext();
				}
				else
				{
					if (IsCached(managedPath2) && seenPaths.Add(managedPath2))
					{
						yield return current2;
					}
					cacheHasNext = cachePaths.MoveNext();
				}
			}
			while (mainHasNext)
			{
				T current3 = mainPaths.Current;
				SaveDataManagedPath managedPath3 = current3.ManagedPath;
				if (!IsCached(managedPath3) && seenPaths.Add(managedPath3))
				{
					yield return current3;
				}
				mainHasNext = mainPaths.MoveNext();
			}
			while (cacheHasNext)
			{
				T current4 = cachePaths.Current;
				SaveDataManagedPath managedPath4 = current4.ManagedPath;
				if (IsCached(managedPath4) && seenPaths.Add(managedPath4))
				{
					yield return current4;
				}
				cacheHasNext = cachePaths.MoveNext();
			}
		}
	}
}

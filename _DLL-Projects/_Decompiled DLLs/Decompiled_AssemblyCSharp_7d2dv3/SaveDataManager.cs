using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Platform;
using Unity.Profiling;

public class SaveDataManager : SaveDataManagerBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class CachedStream : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public class VirtualFileStream : MemoryStream
		{
			[field: PublicizedFrom(EAccessModifier.Private)]
			public bool ReadEnabled { get; set; }

			[field: PublicizedFrom(EAccessModifier.Private)]
			public bool WriteEnabled { get; set; }

			public override bool CanRead
			{
				get
				{
					if (ReadEnabled)
					{
						return base.CanRead;
					}
					return false;
				}
			}

			public override bool CanWrite
			{
				get
				{
					if (WriteEnabled)
					{
						return base.CanWrite;
					}
					return false;
				}
			}

			public VirtualFileStream()
			{
				ReadEnabled = true;
				WriteEnabled = true;
			}

			public VirtualFileStream(int capacity)
				: base(capacity)
			{
				ReadEnabled = true;
				WriteEnabled = true;
			}

			public VirtualFileStream(long capacity)
				: base((int)((capacity < int.MaxValue) ? capacity : int.MaxValue))
			{
				ReadEnabled = true;
				WriteEnabled = true;
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (!CanRead)
				{
					throw new NotSupportedException("Stream does not support reading.");
				}
				return base.Read(buffer, offset, count);
			}

			public override int ReadByte()
			{
				if (!CanRead)
				{
					throw new NotSupportedException("Stream does not support reading.");
				}
				return base.ReadByte();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime lastAccessTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime lastWriteTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IPlatformSaveGameProvider platformSaveGameProvider;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly SaveDataManagedPath backupPath;

		[PublicizedFrom(EAccessModifier.Private)]
		public SpinLock sl = new SpinLock(enableThreadOwnerTracking: true);

		[PublicizedFrom(EAccessModifier.Private)]
		public SpinLock commitLock = new SpinLock(enableThreadOwnerTracking: true);

		[PublicizedFrom(EAccessModifier.Private)]
		public VirtualFileStream workingCopy;

		[PublicizedFrom(EAccessModifier.Private)]
		public VirtualFileStream commitCopy;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isReading;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isWriting;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object lockObj = new object();

		[field: PublicizedFrom(EAccessModifier.Private)]
		public SaveDataManagedPath Path { get; }

		public long PendingSize
		{
			get
			{
				if (!HasPendingChanges)
				{
					return 0L;
				}
				return commitCopy?.Length ?? 0;
			}
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool HasPendingChanges
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool IsLocked => sl.IsHeld;

		public DateTime LastAccessTime
		{
			get
			{
				if (!IsLocked)
				{
					return lastAccessTime;
				}
				return DateTime.Now;
			}
		}

		public DateTime LastWriteTime => lastWriteTime;

		public bool ExistsVirtual
		{
			get
			{
				lock (lockObj)
				{
					return workingCopy != null;
				}
			}
		}

		public CachedStream(SaveDataManagedPath path, IPlatformSaveGameProvider platformSaveGameProvider)
		{
			Path = path;
			backupPath = SaveDataUtils.GetBackupPath(path);
			this.platformSaveGameProvider = platformSaveGameProvider;
			lastAccessTime = DateTime.MinValue;
			lastWriteTime = platformSaveGameProvider.ManagedFileGetLastWriteTimeUtc(path);
			if (platformSaveGameProvider.ShouldBackup() && platformSaveGameProvider.ManagedFileExists(backupPath))
			{
				if (platformSaveGameProvider.ManagedDirectoryExists(Path))
				{
					Log.Warning($"Could not restore to \"{Path}\" using \"{backupPath}\" because the former is a directory.");
					return;
				}
				platformSaveGameProvider.ManagedFileDelete(Path);
				platformSaveGameProvider.ManagedFileMove(backupPath, Path);
			}
		}

		public void EnterLock(FileMode mode, FileAccess access, FileShare share, out Stream stream)
		{
			bool lockTaken = false;
			stream = null;
			try
			{
				sl.Enter(ref lockTaken);
				if (!lockTaken)
				{
					Log.Error("[SaveDataManager] Spin lock should have blocked till it was acquired or thrown an exception.");
					return;
				}
				lock (lockObj)
				{
					bool flag = workingCopy != null;
					bool flag2 = platformSaveGameProvider.ManagedFileExists(Path);
					bool flag3 = flag || flag2;
					if (mode == FileMode.Create)
					{
						mode = ((!flag3) ? FileMode.CreateNew : FileMode.Truncate);
					}
					isReading = (access & FileAccess.Read) != 0;
					isWriting = (access & FileAccess.Write) != 0;
					if (mode == FileMode.Open || mode == FileMode.OpenOrCreate || mode == FileMode.Append)
					{
						if (!flag)
						{
							VirtualFileStream dest;
							if (mode != FileMode.Open && !flag2)
							{
								dest = new VirtualFileStream();
								lastWriteTime = DateTime.UtcNow;
							}
							else
							{
								dest = new VirtualFileStream();
								platformSaveGameProvider.ManagedFileRead(Path, dest);
							}
							workingCopy = dest;
						}
						SeekOrigin origin = ((mode == FileMode.Append) ? SeekOrigin.End : SeekOrigin.Begin);
						workingCopy.Seek(0L, origin);
					}
					else
					{
						switch (mode)
						{
						case FileMode.Truncate:
							if (flag)
							{
								workingCopy.SetLength(0L);
								lastWriteTime = DateTime.UtcNow;
								break;
							}
							if (flag2)
							{
								workingCopy = new VirtualFileStream();
								lastWriteTime = DateTime.UtcNow;
								break;
							}
							throw new IOException($"File does not exist at {Path}");
						case FileMode.CreateNew:
							if (flag3)
							{
								throw new IOException($"File already exists at {Path}");
							}
							workingCopy = new VirtualFileStream();
							lastWriteTime = DateTime.UtcNow;
							break;
						default:
							throw new NotImplementedException($"Unhandled mode: {mode}");
						}
					}
					workingCopy.ReadEnabled = isReading;
					workingCopy.WriteEnabled = isWriting;
					stream = workingCopy;
				}
			}
			finally
			{
				if (stream == null && lockTaken)
				{
					sl.Exit();
				}
			}
		}

		public void ExitLock()
		{
			bool lockTaken = false;
			EnterCommitLock(ref lockTaken);
			if (lockTaken)
			{
				lock (lockObj)
				{
					workingCopy.ReadEnabled = true;
					workingCopy.WriteEnabled = true;
					if (isWriting)
					{
						if (commitCopy == null)
						{
							commitCopy = new VirtualFileStream(workingCopy.Length);
						}
						else
						{
							commitCopy.Seek(0L, SeekOrigin.Begin);
							commitCopy.SetLength(0L);
						}
						workingCopy.Position = 0L;
						StreamUtils.StreamCopy(workingCopy, commitCopy);
						HasPendingChanges = true;
						lastWriteTime = DateTime.UtcNow;
					}
					isReading = false;
					isWriting = false;
					lastAccessTime = DateTime.Now;
					sl.Exit();
				}
			}
			else
			{
				Log.Error($"[SaveDataManager] Failed to take commit lock on cached stream for path: {Path}");
			}
			ExitCommitLock();
		}

		public void EnterCommitLock(ref bool lockTaken)
		{
			commitLock.Enter(ref lockTaken);
		}

		public void ExitCommitLock()
		{
			commitLock.Exit();
		}

		public void Commit()
		{
			lock (lockObj)
			{
				if (commitCopy == null)
				{
					Log.Error("CachedStream commit failed: commitCopy is null.");
					return;
				}
				if (platformSaveGameProvider.ShouldBackup() && platformSaveGameProvider.ManagedFileExists(Path))
				{
					if (platformSaveGameProvider.ManagedDirectoryExists(backupPath))
					{
						Log.Warning($"Could not create backup of \"{Path}\" to \"{backupPath}\" because the latter is a directory.");
					}
					else
					{
						platformSaveGameProvider.ManagedFileDelete(backupPath);
						platformSaveGameProvider.ManagedFileMove(Path, backupPath);
					}
				}
				commitCopy.Position = 0L;
				platformSaveGameProvider.ManagedFileWrite(Path, commitCopy);
				if (platformSaveGameProvider.ShouldBackup() && platformSaveGameProvider.ManagedFileExists(backupPath))
				{
					platformSaveGameProvider.ManagedFileDelete(backupPath);
				}
				HasPendingChanges = false;
			}
		}

		public void Dispose()
		{
			lock (lockObj)
			{
				workingCopy?.Dispose();
				commitCopy?.Dispose();
				workingCopy = null;
				commitCopy = null;
				HasPendingChanges = false;
			}
		}
	}

	public class SlotSizeData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<SaveDataManagedPath, long> priorityFileSizes = new Dictionary<SaveDataManagedPath, long>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<SaveDataManagedPath, long> otherFileSizes = new Dictionary<SaveDataManagedPath, long>();

		[PublicizedFrom(EAccessModifier.Private)]
		public (SaveDataManagedPath path, long size) largestPriorityFile;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly SaveDataSlot saveDataSlot;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public long TotalFileSize
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public long LargestPriorityFileSize => largestPriorityFile.size;

		public int FileCount => otherFileSizes.Count + priorityFileSizes.Count;

		public SlotSizeData(SaveDataSlot saveDataSlot)
		{
			this.saveDataSlot = saveDataSlot;
		}

		public void SetFileSize(SaveDataManagedPath managedPath, long pendingSize)
		{
			long num = pendingSize;
			if (TryGetFileSize(managedPath, out var trackedSize, out var isPriorityFile))
			{
				num -= trackedSize;
			}
			else
			{
				isPriorityFile = IsPriorityFilePath(managedPath);
			}
			TotalFileSize += num;
			if (isPriorityFile)
			{
				priorityFileSizes[managedPath] = pendingSize;
				UpdateLargestPriorityFileSize(managedPath, pendingSize, num);
			}
			else
			{
				otherFileSizes[managedPath] = pendingSize;
			}
		}

		public bool TryGetFileSize(SaveDataManagedPath managedPath, out long trackedSize)
		{
			bool isPriorityFile;
			return TryGetFileSize(managedPath, out trackedSize, out isPriorityFile);
		}

		public bool TryGetFileSize(SaveDataManagedPath managedPath, out long trackedSize, out bool isPriorityFile)
		{
			isPriorityFile = priorityFileSizes.TryGetValue(managedPath, out trackedSize);
			if (!isPriorityFile)
			{
				return otherFileSizes.TryGetValue(managedPath, out trackedSize);
			}
			return true;
		}

		public void RemoveFileSize(SaveDataManagedPath managedPath)
		{
			if (TryGetFileSize(managedPath, out var trackedSize, out var isPriorityFile))
			{
				TotalFileSize -= trackedSize;
				if (isPriorityFile)
				{
					priorityFileSizes.Remove(managedPath);
					UpdateLargestPriorityFileSize(managedPath, 0L, -trackedSize);
				}
				else
				{
					otherFileSizes.Remove(managedPath);
				}
			}
		}

		public void RemoveFileSizes(SaveDataManagedPath parentManagedPath)
		{
			List<SaveDataManagedPath> managedPaths = priorityFileSizes.Keys.ToList();
			RemoveFileSizesInternal(managedPaths);
			List<SaveDataManagedPath> managedPaths2 = otherFileSizes.Keys.ToList();
			RemoveFileSizesInternal(managedPaths2);
			[PublicizedFrom(EAccessModifier.Private)]
			void RemoveFileSizesInternal(List<SaveDataManagedPath> list)
			{
				foreach (SaveDataManagedPath item in list)
				{
					if (parentManagedPath.IsParentOf(item))
					{
						RemoveFileSize(item);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateLargestPriorityFileSize(SaveDataManagedPath managedPath, long pendingSize, long delta)
		{
			if (delta == 0L)
			{
				return;
			}
			if (pendingSize > largestPriorityFile.size)
			{
				largestPriorityFile = (path: managedPath, size: pendingSize);
			}
			else
			{
				if (delta >= 0 || !(largestPriorityFile.path == managedPath))
				{
					return;
				}
				largestPriorityFile = default((SaveDataManagedPath, long));
				foreach (KeyValuePair<SaveDataManagedPath, long> priorityFileSize in priorityFileSizes)
				{
					if (priorityFileSize.Value > largestPriorityFile.size)
					{
						largestPriorityFile = (path: priorityFileSize.Key, size: pendingSize);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int writerThreadTerminationTimeout = 420;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IPlatformSaveGameProvider platformSaveGameProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object mapLockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object commitLockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<SaveDataManagedPath, CachedStream> pathToCacheMap = new Dictionary<SaveDataManagedPath, CachedStream>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<CachedStream> priorityCachedStreams = new HashSet<CachedStream>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Stream, CachedStream> streamToCacheMap = new Dictionary<Stream, CachedStream>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan commitInterval = TimeSpan.FromSeconds(30.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int writeTaskDelay = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int memoryCopyTimeout = 30000;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CommitPendingChangesMarker = new ProfilerMarker("SaveDataManager.CommitPendingChanges");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_InitRestoreBackupsMarker = new ProfilerMarker("SaveDataManager.InitRestoreBackups");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_InitFileSizeTrackingMarker = new ProfilerMarker("SaveDataManager.InitFileSizeTracking");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_CachedStreamCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Cached Streams", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_StreamOperationCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Stream Operations", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo writerThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime lastCommitTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int explicitCommitNeeded;

	[PublicizedFrom(EAccessModifier.Private)]
	public int explicitCommitCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<CachedStream> commitList = new List<CachedStream>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<CachedStream> closeList = new List<CachedStream>();

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataWriteMode writeMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cleanupComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<SaveDataSlot, SlotSizeData> fileSizesBySlot = new Dictionary<SaveDataSlot, SlotSizeData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public object regionFileLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileManager activeRegionFileManager;

	public override bool AppliesSaveSizeLimit => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Stream GetStream(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
	{
		if ((access == FileAccess.Read && (mode == FileMode.CreateNew || mode == FileMode.Create || mode == FileMode.Truncate || mode == FileMode.Append)) || (access == FileAccess.ReadWrite && mode == FileMode.Append))
		{
			throw new ArgumentException(string.Format("Combining {0}: {1} with {2}: {3} is invalid.", "FileMode", mode, "FileAccess", access));
		}
		if (cleanupComplete)
		{
			throw new InvalidOperationException("Attempting to get stream from Save Data Manager after Cleanup has been called.");
		}
		int num = path.PathRelativeToRoot.LastIndexOf('/');
		SaveDataManagedPath path2 = new SaveDataManagedPath((num >= 0) ? path.PathRelativeToRoot.Substring(0, num) : string.Empty);
		if (!ManagedDirectoryExists(path2))
		{
			throw new IOException(SpanUtils.Concat("Parent of ", path.PathRelativeToRoot, " does not exist."));
		}
		CachedStream value;
		lock (mapLockObj)
		{
			if (!pathToCacheMap.TryGetValue(path, out value))
			{
				value = new CachedStream(path, platformSaveGameProvider);
				pathToCacheMap[path] = value;
			}
			s_CachedStreamCount.Value = pathToCacheMap.Count;
		}
		value.EnterLock(mode, access, share, out var stream);
		lock (mapLockObj)
		{
			streamToCacheMap[stream] = value;
			ProfilerCounterValue<int> profilerCounterValue = s_StreamOperationCount;
			profilerCounterValue.Value++;
			return stream;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsPriorityFilePath(SaveDataManagedPath path)
	{
		if (path.Type == SaveDataType.Saves)
		{
			return path.PathRelativeToSlot.AsSpan().IndexOf("Region") == 0;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ReturnStream(Stream stream)
	{
		if (cleanupComplete)
		{
			throw new InvalidOperationException("Attempting to return stream to Save Data Manager after Cleanup has been called.");
		}
		CachedStream cachedStream;
		lock (mapLockObj)
		{
			cachedStream = streamToCacheMap[stream];
			cachedStream.ExitLock();
			streamToCacheMap.Remove(stream);
			long trackedSize = default(long);
			bool isPriorityFile = default(bool);
			if (cachedStream.HasPendingChanges && TryGetTrackedSize(cachedStream.Path, out trackedSize, out isPriorityFile) && isPriorityFile && trackedSize > cachedStream.PendingSize)
			{
				priorityCachedStreams.Add(cachedStream);
			}
			else
			{
				priorityCachedStreams.Remove(cachedStream);
			}
		}
		if (writeMode != SaveDataWriteMode.Immediate)
		{
			return;
		}
		if (cachedStream.HasPendingChanges)
		{
			cachedStream.Commit();
		}
		lock (mapLockObj)
		{
			if (!cachedStream.IsLocked)
			{
				RemoveAndDispose(cachedStream);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveAndDispose(CachedStream cachedStream)
	{
		lock (mapLockObj)
		{
			pathToCacheMap.Remove(cachedStream.Path);
			priorityCachedStreams.Remove(cachedStream);
			cachedStream.Dispose();
		}
	}

	public override bool ShouldLimitSize()
	{
		return platformSaveGameProvider.ShouldLimitSize();
	}

	public override void UpdateSizes()
	{
		platformSaveGameProvider.UpdateSizes();
	}

	public override SaveDataSizes GetSizes()
	{
		return platformSaveGameProvider.GetSizes();
	}

	public override void ManagedFileDelete(SaveDataManagedPath path)
	{
		lock (mapLockObj)
		{
			CachedStream value;
			bool num = pathToCacheMap.TryGetValue(path, out value);
			if (num && value.IsLocked)
			{
				throw new IOException($"Cached Stream for '{path}' can not be deleted because it is current locked (stream is loaned out and not returned yet).");
			}
			if (platformSaveGameProvider.ShouldBackup())
			{
				SaveDataManagedPath backupPath = SaveDataUtils.GetBackupPath(path);
				if (platformSaveGameProvider.ManagedFileExists(backupPath))
				{
					platformSaveGameProvider.ManagedFileDelete(backupPath);
				}
			}
			platformSaveGameProvider.ManagedFileDelete(path);
			if (num)
			{
				RemoveAndDispose(value);
			}
			if (fileSizesBySlot.TryGetValue(path.Slot, out var value2))
			{
				value2.RemoveFileSize(path);
			}
		}
	}

	public override bool ManagedFileExists(SaveDataManagedPath path)
	{
		lock (mapLockObj)
		{
			if (pathToCacheMap.TryGetValue(path, out var value) && value.ExistsVirtual)
			{
				return true;
			}
		}
		return platformSaveGameProvider.ManagedFileExists(path);
	}

	public override DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		lock (mapLockObj)
		{
			if (pathToCacheMap.TryGetValue(path, out var value))
			{
				return value.LastWriteTime;
			}
		}
		return platformSaveGameProvider.ManagedFileGetLastWriteTimeUtc(path);
	}

	public override SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
	{
		return platformSaveGameProvider.ManagedDirectoryCreateDirectory(path);
	}

	public override DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return platformSaveGameProvider.ManagedDirectoryGetLastWriteTimeUtc(path);
	}

	public override bool ManagedDirectoryExists(SaveDataManagedPath path)
	{
		return platformSaveGameProvider.ManagedDirectoryExists(path);
	}

	public override IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return platformSaveGameProvider.ManagedDirectoryEnumerateDirectories(path, searchPattern, searchOption);
	}

	public override IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		HashSet<SaveDataManagedPath> cachedPaths = new HashSet<SaveDataManagedPath>();
		lock (mapLockObj)
		{
			foreach (var (saveDataManagedPath2, cachedStream2) in pathToCacheMap)
			{
				if (cachedStream2.ExistsVirtual && PathSearchMatches(path, saveDataManagedPath2, searchPattern, searchOption))
				{
					cachedPaths.Add(saveDataManagedPath2);
				}
			}
		}
		foreach (SaveDataManagedPath platformPath in platformSaveGameProvider.ManagedDirectoryEnumerateFiles(path, searchPattern, searchOption))
		{
			yield return platformPath;
			cachedPaths.Remove(platformPath);
		}
		foreach (SaveDataManagedPath item in cachedPaths)
		{
			yield return item;
		}
	}

	public override IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		HashSet<SaveDataManagedPath> cachedPaths = new HashSet<SaveDataManagedPath>();
		lock (mapLockObj)
		{
			foreach (var (saveDataManagedPath2, cachedStream2) in pathToCacheMap)
			{
				if (cachedStream2.ExistsVirtual && PathSearchMatches(path, saveDataManagedPath2, searchPattern, searchOption))
				{
					cachedPaths.Add(saveDataManagedPath2);
				}
			}
		}
		foreach (SaveDataManagedPath platformPath in platformSaveGameProvider.ManagedDirectoryEnumerateFileSystemEntries(path, searchPattern, searchOption))
		{
			yield return platformPath;
			cachedPaths.Remove(platformPath);
		}
		foreach (SaveDataManagedPath item in cachedPaths)
		{
			yield return item;
		}
	}

	public override void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
	{
		lock (mapLockObj)
		{
			List<KeyValuePair<SaveDataManagedPath, CachedStream>> list = pathToCacheMap.Where([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<SaveDataManagedPath, CachedStream> kv) => path.IsParentOf(kv.Key)).ToList();
			SaveDataManagedPath key;
			CachedStream value;
			foreach (KeyValuePair<SaveDataManagedPath, CachedStream> item in list)
			{
				item.Deconstruct(out key, out value);
				SaveDataManagedPath arg = key;
				if (value.IsLocked)
				{
					throw new IOException($"Can not delete directory at '{path}' because Cached Stream for '{arg}' can not be deleted because it is current locked (stream is loaned out and not returned yet).");
				}
			}
			platformSaveGameProvider.ManagedDirectoryDelete(path, recursive);
			foreach (KeyValuePair<SaveDataManagedPath, CachedStream> item2 in list)
			{
				item2.Deconstruct(out key, out value);
				CachedStream cachedStream = value;
				RemoveAndDispose(cachedStream);
			}
			foreach (SlotSizeData value2 in fileSizesBySlot.Values)
			{
				value2.RemoveFileSizes(path);
			}
		}
	}

	public override long ManagedFileInfoLength(SaveDataManagedPath path)
	{
		lock (mapLockObj)
		{
			if (pathToCacheMap.TryGetValue(path, out var value) && value.HasPendingChanges)
			{
				return value.PendingSize;
			}
		}
		return platformSaveGameProvider.ManagedFileInfoLength(path);
	}

	public override IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return platformSaveGameProvider.ManagedDirectoryInfoEnumerateDirectories(path, searchPattern, searchOption);
	}

	public override IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		HashSet<SaveDataManagedPath> cachedPaths = new HashSet<SaveDataManagedPath>();
		lock (mapLockObj)
		{
			foreach (var (saveDataManagedPath2, cachedStream2) in pathToCacheMap)
			{
				if (cachedStream2.ExistsVirtual && PathSearchMatches(path, saveDataManagedPath2, searchPattern, searchOption))
				{
					cachedPaths.Add(saveDataManagedPath2);
				}
			}
		}
		foreach (SdFileInfo fileInfo in platformSaveGameProvider.ManagedDirectoryInfoEnumerateFiles(path, searchPattern, searchOption))
		{
			yield return fileInfo;
			cachedPaths.Remove(fileInfo.ManagedPath);
		}
		foreach (SaveDataManagedPath item in cachedPaths)
		{
			yield return new SdFileInfo(item);
		}
	}

	public override IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		HashSet<SaveDataManagedPath> cachedPaths = new HashSet<SaveDataManagedPath>();
		lock (mapLockObj)
		{
			foreach (var (saveDataManagedPath2, cachedStream2) in pathToCacheMap)
			{
				if (cachedStream2.ExistsVirtual && PathSearchMatches(path, saveDataManagedPath2, searchPattern, searchOption))
				{
					cachedPaths.Add(saveDataManagedPath2);
				}
			}
		}
		foreach (SdFileSystemInfo fileInfo in platformSaveGameProvider.ManagedDirectoryInfoEnumerateFileSystemInfos(path, searchPattern, searchOption))
		{
			yield return fileInfo;
			cachedPaths.Remove(fileInfo.ManagedPath);
		}
		foreach (SaveDataManagedPath item in cachedPaths)
		{
			yield return new SdFileInfo(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool PathSearchMatches(SaveDataManagedPath parent, SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		if (!parent.IsParentOf(path))
		{
			return false;
		}
		if (searchOption == SearchOption.TopDirectoryOnly)
		{
			int startIndex = ((parent.PathRelativeToRoot.Length > 0) ? (parent.PathRelativeToRoot.Length + 1) : 0);
			if (path.PathRelativeToRoot.IndexOf('/', startIndex) >= 0)
			{
				return false;
			}
		}
		if (!MatchesSearchPattern(path, searchPattern))
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool MatchesSearchPattern(SaveDataManagedPath path, string searchPattern)
	{
		if (searchPattern == "*")
		{
			return true;
		}
		StringSpan text = path.PathRelativeToRoot;
		int num = text.LastIndexOf('/');
		if (num >= 0 && num < text.Length - 1)
		{
			text = text.Slice(num + 1);
		}
		return MatchWildcard(text, searchPattern);
		[PublicizedFrom(EAccessModifier.Internal)]
		static bool MatchWildcard(StringSpan stringSpan, string pattern)
		{
			int num2 = 0;
			int i = 0;
			int num3 = -1;
			int num4 = -1;
			while (num2 < stringSpan.Length)
			{
				if (i < pattern.Length && (pattern[i] == '?' || char.ToLowerInvariant(pattern[i]) == char.ToLowerInvariant(stringSpan[num2])))
				{
					num2++;
					i++;
				}
				else if (i < pattern.Length && pattern[i] == '*')
				{
					num4 = i++;
					num3 = num2;
				}
				else
				{
					if (num4 < 0)
					{
						return false;
					}
					i = num4 + 1;
					num2 = ++num3;
				}
			}
			for (; i < pattern.Length && pattern[i] == '*'; i++)
			{
			}
			return i == pattern.Length;
		}
	}

	public SaveDataManager(IPlatformSaveGameProvider platformSaveGameProvider)
	{
		this.platformSaveGameProvider = new SaveDataMergedPlatformSaveGameIOProvider(platformSaveGameProvider);
	}

	public override void Init()
	{
		SetWriteMode(SaveDataWriteMode.Deferred);
		InitRestoreBackups();
		InitFileSizeTracking();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRestoreBackups()
	{
		if (!platformSaveGameProvider.ShouldBackup())
		{
			return;
		}
		using (s_InitRestoreBackupsMarker.Auto())
		{
			SaveDataManagedPath path = SaveDataType.User.GetPath();
			if (!ManagedDirectoryExists(path))
			{
				return;
			}
			foreach (SdFileInfo item in ManagedDirectoryInfoEnumerateFiles(path, "*.bup", SearchOption.AllDirectories))
			{
				SaveDataManagedPath managedPath = item.ManagedPath;
				SaveDataManagedPath restorePath = SaveDataUtils.GetRestorePath(managedPath);
				if (platformSaveGameProvider.ManagedDirectoryExists(restorePath))
				{
					Log.Warning($"Could not restore to \"{restorePath}\" using \"{managedPath}\" because the former is a directory.");
					continue;
				}
				platformSaveGameProvider.ManagedFileDelete(restorePath);
				platformSaveGameProvider.ManagedFileMove(managedPath, restorePath);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitFileSizeTracking()
	{
		using (s_InitFileSizeTrackingMarker.Auto())
		{
			fileSizesBySlot.Clear();
			SaveDataManagedPath path = SaveDataType.User.GetPath();
			if (!ManagedDirectoryExists(path))
			{
				return;
			}
			foreach (SdFileInfo item in ManagedDirectoryInfoEnumerateFiles(path, "*", SearchOption.AllDirectories))
			{
				SaveDataManagedPath managedPath = item.ManagedPath;
				if (!fileSizesBySlot.TryGetValue(managedPath.Slot, out var value))
				{
					SaveDataSlot simpleSlot = managedPath.Slot.GetSimpleSlot();
					value = new SlotSizeData(simpleSlot);
					fileSizesBySlot[simpleSlot] = value;
				}
				long length = item.Length;
				value.SetFileSize(managedPath, length);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetTrackedSize(SaveDataManagedPath path, out long trackedSize, out bool isPriorityFile)
	{
		if (fileSizesBySlot.TryGetValue(path.Slot, out var value) && value.TryGetFileSize(path, out trackedSize, out isPriorityFile))
		{
			return true;
		}
		trackedSize = 0L;
		isPriorityFile = false;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int WriteTask(ThreadManager.ThreadInfo _threadInfo)
	{
		try
		{
			if (_threadInfo.TerminationRequested())
			{
				return -1;
			}
			int num = explicitCommitNeeded;
			TimeSpan timeSpan = DateTime.Now - lastCommitTime;
			if (num != explicitCommitCount || timeSpan > commitInterval)
			{
				if (CommitPendingChanges())
				{
					explicitCommitCount = num;
				}
				lastCommitTime = DateTime.Now;
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return 5;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CommitPendingChanges(bool force = false)
	{
		if (!platformSaveGameProvider.ShouldCommit())
		{
			if (!force)
			{
				return false;
			}
			Log.Out("[SaveDataManager] Forcing CommitPendingChanges (platform currently suggests to not commit).");
		}
		bool flag = false;
		try
		{
			lock (commitLockObj)
			{
				HashSet<CachedStream> hashSet = new HashSet<CachedStream>();
				CachedStream commitTarget;
				bool isPriorityStream;
				while (TryGetNextStreamToCommit(hashSet, out commitTarget, out isPriorityStream))
				{
					bool lockTaken = true;
					try
					{
						if (!flag)
						{
							flag = true;
							OnCommitStarted();
						}
						long pendingSize = commitTarget.PendingSize;
						long num = pendingSize;
						long trackedSize;
						if (!fileSizesBySlot.TryGetValue(commitTarget.Path.Slot, out var value))
						{
							SaveDataSlot simpleSlot = commitTarget.Path.Slot.GetSimpleSlot();
							value = new SlotSizeData(simpleSlot);
							fileSizesBySlot[simpleSlot] = value;
						}
						else if (value.TryGetFileSize(commitTarget.Path, out trackedSize))
						{
							num -= trackedSize;
						}
						if (!isPriorityStream && pendingSize > 0)
						{
							lock (regionFileLock)
							{
								if (activeRegionFileManager != null && activeRegionFileManager.MaxBytes > 0 && commitTarget.Path.Slot == activeRegionFileManager.SaveDataSlot)
								{
									long num2 = value.TotalFileSize + value.LargestPriorityFileSize + pendingSize - activeRegionFileManager.MaxBytes;
									if (num2 > 0)
									{
										commitTarget.ExitCommitLock();
										lockTaken = false;
										if (!activeRegionFileManager.MakeRoom(num2))
										{
											Log.Error($"[SaveDataManager] SaveDataLimit will be exceeded! Failed to clear enough space for file: {commitTarget.Path}");
											commitTarget.EnterCommitLock(ref lockTaken);
											if (lockTaken)
											{
												goto end_IL_00e0;
											}
											Log.Error($"[SaveDataManager] Failed to take commit lock on cached stream for path: {commitTarget.Path}");
										}
										continue;
									}
									goto end_IL_00e0;
								}
								end_IL_00e0:;
							}
						}
						value.SetFileSize(commitTarget.Path, pendingSize);
						commitTarget.Commit();
					}
					catch (Exception e)
					{
						hashSet.Add(commitTarget);
						Log.Error($"[SaveDataManager] Failed to commit cached stream '{commitTarget.Path}'.");
						Log.Exception(e);
					}
					finally
					{
						if (lockTaken)
						{
							commitTarget.ExitCommitLock();
						}
						lock (mapLockObj)
						{
							if (!commitTarget.HasPendingChanges)
							{
								priorityCachedStreams.Remove(commitTarget);
							}
						}
					}
				}
				CloseDormantStreams();
			}
		}
		finally
		{
			try
			{
				platformSaveGameProvider?.Flush(waitForFlush: true);
			}
			catch (Exception arg)
			{
				Log.Error($"Error while flushing platform save game provider: {arg}");
			}
			if (flag)
			{
				OnCommitFinished();
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetNextStreamToCommit(HashSet<CachedStream> ignored, out CachedStream commitTarget, out bool isPriorityStream)
	{
		lock (mapLockObj)
		{
			if (TryGetNextStreamInternal(out commitTarget, out isPriorityStream))
			{
				bool lockTaken = false;
				commitTarget.EnterCommitLock(ref lockTaken);
				if (lockTaken)
				{
					return true;
				}
				commitTarget = null;
				isPriorityStream = false;
				Log.Error($"[SaveDataManager] Failed to take commit lock on cached stream for path: {commitTarget.Path}");
			}
			return false;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		bool TryGetNextStreamInternal(out CachedStream reference, out bool reference2)
		{
			reference = priorityCachedStreams.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (CachedStream stream) => !ignored.Contains(stream));
			if (reference != null)
			{
				reference2 = true;
				return true;
			}
			reference2 = false;
			foreach (CachedStream value in pathToCacheMap.Values)
			{
				if (value.HasPendingChanges && !ignored.Contains(value))
				{
					reference = value;
					return true;
				}
			}
			reference = null;
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseDormantStreams()
	{
		lock (mapLockObj)
		{
			closeList.Clear();
			DateTime now = DateTime.Now;
			foreach (CachedStream value in pathToCacheMap.Values)
			{
				if (!value.HasPendingChanges && (now - value.LastAccessTime).TotalMilliseconds > 30000.0)
				{
					closeList.Add(value);
				}
			}
			foreach (CachedStream close in closeList)
			{
				RemoveAndDispose(close);
			}
			s_CachedStreamCount.Value = pathToCacheMap.Count;
		}
	}

	public override void RegisterRegionFileManager(RegionFileManager regionFileManager)
	{
		lock (regionFileLock)
		{
			if (activeRegionFileManager != null)
			{
				Log.Warning("[SaveDataManager] Attempting to register a RegionFileManager when one is already registered. The existing reference will be overridden.");
			}
			activeRegionFileManager = regionFileManager;
		}
	}

	public override void DeregisterRegionFileManager(RegionFileManager regionFileManager)
	{
		lock (regionFileLock)
		{
			if (activeRegionFileManager != regionFileManager)
			{
				Log.Error("[SaveDataManager] Attempting to deregister a RegionFileManager which is not currently registered.");
			}
			else
			{
				activeRegionFileManager = null;
			}
		}
	}

	public override SaveDataWriteMode GetWriteMode()
	{
		return writeMode;
	}

	public override void SetWriteMode(SaveDataWriteMode writeMode)
	{
		if (this.writeMode == writeMode)
		{
			return;
		}
		lock (commitLockObj)
		{
			lock (mapLockObj)
			{
				if (writeMode == SaveDataWriteMode.Deferred)
				{
					lastCommitTime = DateTime.Now;
					writerThreadInfo = ThreadManager.StartThread("SaveDataManager_WriterThread", null, WriteTask, null);
				}
				else
				{
					writerThreadInfo?.WaitForEnd(420);
					writerThreadInfo = null;
					CommitPendingChanges(force: true);
				}
				this.writeMode = writeMode;
			}
		}
	}

	public override void Cleanup()
	{
		SetWriteMode(SaveDataWriteMode.None);
		cleanupComplete = true;
	}

	public override int CommitAsync()
	{
		if (writeMode != SaveDataWriteMode.Deferred)
		{
			return explicitCommitCount;
		}
		return ++explicitCommitNeeded;
	}

	public override bool IsCommitPending(int token)
	{
		return token > explicitCommitCount;
	}

	public override void CommitSync()
	{
		if (writeMode == SaveDataWriteMode.Deferred)
		{
			int token = CommitAsync();
			while (IsCommitPending(token) && writeMode == SaveDataWriteMode.Deferred)
			{
				Thread.Sleep(5);
			}
		}
	}

	public override IEnumerator CommitCoroutine(Action<double> progressFeedback = null)
	{
		if (writeMode == SaveDataWriteMode.Deferred)
		{
			int token = CommitAsync();
			while (IsCommitPending(token) && writeMode == SaveDataWriteMode.Deferred)
			{
				progressFeedback?.Invoke(platformSaveGameProvider.GetCommitProgress());
				yield return null;
			}
		}
	}

	[Conditional("DEBUG_SAVE_DATA_MANAGER")]
	[PublicizedFrom(EAccessModifier.Private)]
	public new static void DebugLog(string text)
	{
		Log.Out("[SaveDataManager] " + text);
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Platform.Shared;
using Platform.XBL.Save;
using Platform.XBL.Save.Storage;
using Unity.XGamingRuntime;

namespace Platform.XBL;

public class SaveGameProviderGameCore : IPlatformSaveGameProvider, IPlatformSaveGameIOProvider
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct OperationScope : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly SaveGameProviderGameCore m_provider;

		public OperationScope(SaveGameProviderGameCore provider)
		{
			m_provider = provider;
			Interlocked.Increment(ref m_provider.m_operations);
		}

		public void Dispose()
		{
			Interlocked.Decrement(ref m_provider.m_operations);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SingleThreadTaskScheduler m_taskScheduler;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action m_initializedDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_initializedDelegateLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform m_owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public IApplicationStateController m_appState;

	[PublicizedFrom(EAccessModifier.Private)]
	public User m_userClient;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_maxStorageSizeBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public ISaveStorageProvider m_saveStorageProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_suspended;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_paused;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_operations;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveContainer m_rootSaveContainer;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ESaveGameProviderStatus Status
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IPlatformSaveGameIOProvider Cache
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public event Action Initialized
	{
		add
		{
			lock (m_initializedDelegateLock)
			{
				m_initializedDelegate = (Action)Delegate.Combine(m_initializedDelegate, value);
				if (Status == ESaveGameProviderStatus.Ok)
				{
					value();
				}
			}
		}
		remove
		{
			lock (m_initializedDelegateLock)
			{
				m_initializedDelegate = (Action)Delegate.Remove(m_initializedDelegate, value);
			}
		}
	}

	public SaveGameProviderGameCore(long maxStorageSizeBytes = 1073741824L)
	{
		m_maxStorageSizeBytes = maxStorageSizeBytes;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnUserHandleReady(XUserHandle userHandle)
	{
		if (userHandle == null)
		{
			Log.Error("[XBL] SaveGameProviderGameCore.OnUserHandleReady: Attempting to retrieve XSTS token before acquiring XUserHandle");
			return;
		}
		if (userHandle.IsInvalid)
		{
			Log.Error("[XBL] SaveGameProviderGameCore.OnUserHandleReady: m_userHandle.IsInvalid is true");
		}
		if (userHandle.IsClosed)
		{
			Log.Error("[XBL] SaveGameProviderGameCore.OnUserHandleReady: m_userHandle.IsClosed is true");
		}
		InitializeSaveStorageProvider();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeSaveStorageProvider()
	{
		SaveStorageProvider value = LaunchPrefs.GameCoreSaveStorageProvider.Value;
		LogInfo($"[GDK] Initializing Save Storage Provider '{value}'.");
		m_saveStorageProvider = value.Create();
		m_saveStorageProvider.InitializeAsync(m_owner, m_maxStorageSizeBytes, m_taskScheduler, OnGameSaveProviderStatusChanged);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameSaveProviderStatusChanged(ESaveGameProviderStatus status)
	{
		if (status == ESaveGameProviderStatus.Ok && m_rootSaveContainer == null)
		{
			OnGameSaveProviderReady();
		}
		lock (m_initializedDelegateLock)
		{
			bool num = status == ESaveGameProviderStatus.Ok && Status != ESaveGameProviderStatus.Ok;
			Status = status;
			if (num)
			{
				m_initializedDelegate?.Invoke();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameSaveProviderReady()
	{
		m_rootSaveContainer = new SaveContainer(m_saveStorageProvider.RootSaveStorageContainer, m_saveStorageProvider.SizeTracker);
		ResumeIO();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationStateChanged(ApplicationState newstate)
	{
		bool flag = newstate == ApplicationState.Suspended;
		if (flag != m_suspended)
		{
			m_suspended = flag;
			if (flag)
			{
				OnSuspend();
			}
			else
			{
				OnResume();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSuspend()
	{
		PauseIO();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnResume()
	{
		if (!Status.IsTerminal())
		{
			LogWarning("Status was not terminal on resume? Did sleep happen during loading?");
		}
		m_rootSaveContainer?.Dispose();
		m_rootSaveContainer = null;
		m_saveStorageProvider?.Dispose();
		m_saveStorageProvider = null;
		Status = ESaveGameProviderStatus.Uninitialized;
		InitializeSaveStorageProvider();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PauseIO()
	{
		if (m_paused)
		{
			LogError("PauseIO should only be called when IO is not paused.");
			return;
		}
		m_paused = true;
		while (m_paused && m_operations > 0)
		{
			Thread.Sleep(5);
		}
		Flush(waitForFlush: true);
		if (!m_paused)
		{
			LogWarning("Was un-paused before we finished waiting on IO to pause.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResumeIO()
	{
		if (!m_paused)
		{
			LogError("ResumeIO should only be called when IO is paused.");
		}
		else
		{
			m_paused = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public OperationScope CreateOperationScope(string debugIdentifier)
	{
		if (m_paused)
		{
			while (m_paused)
			{
				Thread.Sleep(50);
			}
		}
		return new OperationScope(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetSaveContainerAndRelativePath(SaveDataManagedPath path, out SaveContainer saveContainer, out string relativePath)
	{
		saveContainer = m_rootSaveContainer;
		relativePath = path.PathRelativeToRoot;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string text)
	{
		Log.Out("[XBL: SaveGameProvider] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogWarning(string text)
	{
		Log.Warning("[XBL: SaveGameProvider] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogError(string text)
	{
		Log.Error("[XBL: SaveGameProvider] " + text);
	}

	[Conditional("DEBUG_SAVE_DATA_MANAGER")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogTrace(string text)
	{
		Log.Out("[XBL: SaveGameProvider] " + text);
	}

	public void Init(IPlatform _owner)
	{
		PauseIO();
		m_taskScheduler = new SingleThreadTaskScheduler("GameCore", "Game Core Save Tasks");
		m_owner = _owner;
		m_appState = _owner.ApplicationState;
		m_appState.OnApplicationStateChanged += OnApplicationStateChanged;
		m_userClient = (User)m_owner.User;
		m_userClient.UserHandleReady += OnUserHandleReady;
		Cache = new SaveGameIOProviderFixedRoot(GameIO.GetNormalizedPath(GameIO.GetDeviceLocalUserGameDataDir()) + "Cache");
	}

	public void Destroy()
	{
		if (m_rootSaveContainer != null)
		{
			m_rootSaveContainer.Dispose();
			m_rootSaveContainer = null;
		}
		m_saveStorageProvider?.Dispose();
		m_saveStorageProvider = null;
		if (m_userClient != null)
		{
			m_userClient.UserHandleReady -= OnUserHandleReady;
			m_userClient = null;
		}
		if (m_appState != null)
		{
			m_appState.OnApplicationStateChanged -= OnApplicationStateChanged;
			m_appState = null;
		}
		m_owner = null;
		m_taskScheduler?.Dispose();
		m_taskScheduler = null;
	}

	public bool ShouldBackup()
	{
		return false;
	}

	public bool ShouldCommit()
	{
		return !m_paused;
	}

	public double GetCommitProgress()
	{
		return 1.0;
	}

	public void Flush(bool waitForFlush)
	{
		m_saveStorageProvider?.Flush(waitForFlush);
	}

	public bool ShouldLimitSize()
	{
		return true;
	}

	public void UpdateSizes()
	{
		using (CreateOperationScope("UpdateSizes"))
		{
			m_saveStorageProvider.SizeTracker.RefreshSync();
		}
	}

	public SaveDataSizes GetSizes()
	{
		using (CreateOperationScope("GetSizes"))
		{
			return m_saveStorageProvider.SizeTracker.Sizes;
		}
	}

	public void ManagedFileRead(SaveDataManagedPath path, Stream dest)
	{
		using (CreateOperationScope("ManagedFileRead"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			saveContainer.FileRead(relativePath, dest);
		}
	}

	public void ManagedFileWrite(SaveDataManagedPath path, Stream src)
	{
		using (CreateOperationScope("ManagedFileWrite"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			saveContainer.FileWrite(relativePath, src);
		}
	}

	public void ManagedFileCopy(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName, bool overwrite = false)
	{
		using (CreateOperationScope("ManagedFileCopy"))
		{
			GetSaveContainerAndRelativePath(destFileName, out var saveContainer, out var relativePath);
			if (!overwrite && saveContainer.FileExists(relativePath))
			{
				throw new IOException($"Destination '{destFileName}' already exists.");
			}
			GetSaveContainerAndRelativePath(sourceFileName, out var saveContainer2, out var relativePath2);
			using MemoryStream memoryStream = new MemoryStream();
			saveContainer2.FileRead(relativePath2, memoryStream);
			memoryStream.Position = 0L;
			saveContainer.FileWrite(relativePath, memoryStream);
		}
	}

	public void ManagedFileDelete(SaveDataManagedPath path)
	{
		using (CreateOperationScope("ManagedFileDelete"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			saveContainer.FileDelete(relativePath);
		}
	}

	public bool ManagedFileExists(SaveDataManagedPath path)
	{
		using (CreateOperationScope("ManagedFileExists"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			return saveContainer.FileExists(relativePath);
		}
	}

	public DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		using (CreateOperationScope("ManagedFileGetLastWriteTimeUtc"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			return saveContainer.GetLastWriteTimeUtc(relativePath);
		}
	}

	public void ManagedFileMove(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName)
	{
		using (CreateOperationScope("ManagedFileMove"))
		{
			GetSaveContainerAndRelativePath(destFileName, out var saveContainer, out var relativePath);
			if (saveContainer.FileExists(relativePath))
			{
				throw new IOException($"Destination '{destFileName}' already exists.");
			}
			GetSaveContainerAndRelativePath(sourceFileName, out var saveContainer2, out var relativePath2);
			if (saveContainer2 != saveContainer)
			{
				ManagedFileCopy(sourceFileName, destFileName);
				ManagedFileDelete(sourceFileName);
			}
			else
			{
				saveContainer2.FileMove(relativePath2, relativePath);
			}
		}
	}

	public SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
	{
		using (CreateOperationScope("ManagedDirectoryCreateDirectory"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			saveContainer.DirectoryCreate(relativePath);
			return new SdDirectoryInfo(path.GetOriginalPath());
		}
	}

	public DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		using (CreateOperationScope("ManagedDirectoryGetLastWriteTimeUtc"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			return saveContainer.GetLastWriteTimeUtc(relativePath);
		}
	}

	public bool ManagedDirectoryExists(SaveDataManagedPath path)
	{
		using (CreateOperationScope("ManagedDirectoryExists"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			return saveContainer.DirectoryExists(relativePath);
		}
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		using (CreateOperationScope("ManagedDirectoryEnumerateDirectories"))
		{
			return ManagedDirectoryEnumerateInternal(path, searchPattern, searchOption, includeDirectories: true, includeFiles: false).ToArray();
		}
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		using (CreateOperationScope("ManagedDirectoryEnumerateFiles"))
		{
			return ManagedDirectoryEnumerateInternal(path, searchPattern, searchOption, includeDirectories: false, includeFiles: true).ToArray();
		}
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		using (CreateOperationScope("ManagedDirectoryEnumerateFileSystemEntries"))
		{
			return ManagedDirectoryEnumerateInternal(path, searchPattern, searchOption, includeDirectories: true, includeFiles: true).ToArray();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateInternal(SaveDataManagedPath path, string searchPattern, SearchOption searchOption, bool includeDirectories, bool includeFiles)
	{
		GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
		IEnumerable<string> subPaths = saveContainer.DirectoryEnumerate(relativePath, searchPattern, searchOption == SearchOption.AllDirectories, includeDirectories, includeFiles);
		return SaveGameProviderHelper.GetManagedPathsFromBaseAndSubPaths(path, relativePath, subPaths);
	}

	public void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
	{
		using (CreateOperationScope("ManagedDirectoryDelete"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			saveContainer.DirectoryDelete(relativePath, recursive);
		}
	}

	public long ManagedFileInfoLength(SaveDataManagedPath path)
	{
		using (CreateOperationScope("ManagedFileInfoLength"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			return saveContainer.FileLength(relativePath);
		}
	}

	public IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		using (CreateOperationScope("ManagedDirectoryInfoEnumerateDirectories"))
		{
			return (from managedPath in ManagedDirectoryEnumerateInternal(path, searchPattern, searchOption, includeDirectories: true, includeFiles: false)
				select new SdDirectoryInfo(managedPath)).ToArray();
		}
	}

	public IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		using (CreateOperationScope("ManagedDirectoryInfoEnumerateFiles"))
		{
			return (from managedPath in ManagedDirectoryEnumerateInternal(path, searchPattern, searchOption, includeDirectories: false, includeFiles: true)
				select new SdFileInfo(managedPath)).ToArray();
		}
	}

	public IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		using (CreateOperationScope("ManagedDirectoryInfoEnumerateFileSystemInfos"))
		{
			GetSaveContainerAndRelativePath(path, out var saveContainer, out var relativePath);
			return saveContainer.DirectoryEnumerate(relativePath, searchPattern, searchOption == SearchOption.AllDirectories).Select([PublicizedFrom(EAccessModifier.Internal)] (PathEnumerationInfo x) =>
			{
				SaveDataManagedPath managedPathFromBaseAndSubPath = SaveGameProviderHelper.GetManagedPathFromBaseAndSubPath(path, relativePath, x.RelativePath);
				return (!x.IsDirectory) ? ((SdFileSystemInfo)new SdFileInfo(managedPathFromBaseAndSubPath)) : ((SdFileSystemInfo)new SdDirectoryInfo(managedPathFromBaseAndSubPath));
			}).ToArray();
		}
	}
}

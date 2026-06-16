using System;
using System.IO;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL.Save.Storage.Files;

public sealed class SaveStorageStorageProviderFiles : ISaveStorageProvider, IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XblPlatformApi m_api;

	[PublicizedFrom(EAccessModifier.Private)]
	public User m_userClient;

	[PublicizedFrom(EAccessModifier.Private)]
	public SingleThreadTaskScheduler m_taskScheduler;

	[PublicizedFrom(EAccessModifier.Private)]
	public OnGameSaveProviderStatusChanged m_statusChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_gameSaveDirectory;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_maxSizeBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public SizeTracker m_sizeTracker;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveStorageStorageContainerFiles m_rootSaveStorageContainer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isDisposed;

	public SizeTracker SizeTracker => m_sizeTracker;

	public ISaveStorageContainer RootSaveStorageContainer => m_rootSaveStorageContainer;

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string text)
	{
		Log.Out("[XBL: SaveStorageStorageProviderFiles] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogError(string text)
	{
		Log.Error("[XBL: SaveStorageStorageProviderFiles] " + text);
	}

	public void Dispose()
	{
		m_rootSaveStorageContainer?.Dispose();
		m_rootSaveStorageContainer = null;
		m_sizeTracker?.Dispose();
		m_sizeTracker = null;
		m_gameSaveDirectory = null;
		m_statusChanged = null;
		m_taskScheduler = null;
		m_userClient = null;
		m_api = null;
		m_isDisposed = true;
	}

	public void InitializeAsync(IPlatform owner, long maxSizeBytes, SingleThreadTaskScheduler taskScheduler, OnGameSaveProviderStatusChanged statusChanged)
	{
		m_api = (XblPlatformApi)owner.Api;
		m_userClient = (User)owner.User;
		m_maxSizeBytes = maxSizeBytes;
		m_taskScheduler = taskScheduler;
		m_statusChanged = statusChanged;
		SDK.XGameSaveFilesGetFolderWithUiAsync(m_userClient.UserHandle, m_api.SCID, OnGameSaveFilesGetFolderWithUiCompleted);
	}

	public void Flush(bool waitForFlush)
	{
		m_rootSaveStorageContainer?.Flush(waitForFlush);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetRemainingQuotaAsync(SizeTrackerGetRemainingQuotaCompleted completionRoutine)
	{
		m_taskScheduler.ExecuteNoWait([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			ulong remainingQuota;
			int hresult = SDK.XGameSaveFilesGetRemainingQuota(m_userClient.UserHandle, m_api.SCID, out remainingQuota);
			completionRoutine(hresult, (long)remainingQuota);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameSaveFilesGetFolderWithUiCompleted(int hr, string folderResult)
	{
		XblHelpers.LogHR(hr, "Initialize Game Saves (Files).");
		if (m_isDisposed)
		{
			LogInfo("Disposed before XGameSaveFilesGetFolderWithUiAsync completed, cancelling setup");
		}
		else if (Unity.XGamingRuntime.Interop.HR.FAILED(hr))
		{
			if (hr == -2138898428)
			{
				m_statusChanged(ESaveGameProviderStatus.TemporaryError);
				LogInfo("User cancelled sync, retrying...");
				SDK.XGameSaveFilesGetFolderWithUiAsync(m_userClient.UserHandle, m_api.SCID, OnGameSaveFilesGetFolderWithUiCompleted);
			}
			else
			{
				LogError("Could not Initialize. This is a FATAL error!");
				m_statusChanged(ESaveGameProviderStatus.PermanentError);
			}
		}
		else
		{
			m_gameSaveDirectory = Path.GetFullPath(folderResult);
			LogInfo("Game Save Files Directory: " + m_gameSaveDirectory);
			m_taskScheduler.ExecuteNoWait(OnGameSaveDirectoryReady);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameSaveDirectoryReady()
	{
		m_sizeTracker = new SizeTracker(m_maxSizeBytes, 0L, GetRemainingQuotaAsync, shouldUpdateSizesOnEstimate: false);
		m_sizeTracker.RefreshSync();
		m_rootSaveStorageContainer = new SaveStorageStorageContainerFiles(m_gameSaveDirectory, "root");
		m_statusChanged(ESaveGameProviderStatus.Ok);
	}
}

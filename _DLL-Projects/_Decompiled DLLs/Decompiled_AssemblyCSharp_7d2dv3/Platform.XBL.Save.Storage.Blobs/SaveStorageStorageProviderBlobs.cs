using System;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL.Save.Storage.Blobs;

public sealed class SaveStorageStorageProviderBlobs : ISaveStorageProvider, IDisposable
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
	public XGameSaveProviderHandle m_gameSaveProviderHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_maxSizeBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public SizeTracker m_sizeTracker;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveStorageStorageContainerBlobs m_rootSaveStorageContainer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isDisposed;

	public SizeTracker SizeTracker => m_sizeTracker;

	public ISaveStorageContainer RootSaveStorageContainer => m_rootSaveStorageContainer;

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string text)
	{
		Log.Out("[XBL: SaveStorageStorageProviderBlobs] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogError(string text)
	{
		Log.Error("[XBL: SaveStorageStorageProviderBlobs] " + text);
	}

	public void Dispose()
	{
		m_rootSaveStorageContainer?.Dispose();
		m_rootSaveStorageContainer = null;
		m_sizeTracker?.Dispose();
		m_sizeTracker = null;
		if (m_gameSaveProviderHandle != null)
		{
			SDK.XGameSaveCloseProvider(m_gameSaveProviderHandle);
			XblHelpers.LogHR(0, "Uninitialize Game Saves.");
			m_gameSaveProviderHandle = null;
		}
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
		SDK.XGameSaveInitializeProviderAsync(m_userClient.UserHandle, m_api.SCID, syncOnDemand: false, OnXGameSaveInitializeProviderCompleted);
	}

	public void Flush(bool waitForFlush)
	{
		m_rootSaveStorageContainer?.Flush(waitForFlush);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetRemainingQuotaAsync(SizeTrackerGetRemainingQuotaCompleted completionRoutine)
	{
		m_taskScheduler?.ExecuteNoWait([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			SDK.XGameSaveGetRemainingQuotaAsync(m_gameSaveProviderHandle, [PublicizedFrom(EAccessModifier.Internal)] (int hr, long remaining) =>
			{
				m_taskScheduler?.ExecuteNoWait([PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					if (m_rootSaveStorageContainer != null)
					{
						remaining = Math.Max(0L, remaining - m_rootSaveStorageContainer.GetQueuedUsed());
					}
					completionRoutine(hr, remaining);
				});
			});
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnXGameSaveInitializeProviderCompleted(int hr, XGameSaveProviderHandle gameSaveProviderHandle)
	{
		XblHelpers.LogHR(hr, "Initialize Game Saves (Blobs).");
		if (m_isDisposed)
		{
			LogInfo("Disposed before XGameSaveInitializeProviderAsync completed, cancelling setup");
			if (Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
			{
				SDK.XGameSaveCloseProvider(gameSaveProviderHandle);
			}
		}
		else if (Unity.XGamingRuntime.Interop.HR.FAILED(hr))
		{
			if (hr == -2138898428)
			{
				m_statusChanged(ESaveGameProviderStatus.TemporaryError);
				LogInfo("User cancelled sync, retrying...");
				SDK.XGameSaveInitializeProviderAsync(m_userClient.UserHandle, m_api.SCID, syncOnDemand: false, OnXGameSaveInitializeProviderCompleted);
			}
			else
			{
				LogError("Could not Initialize. This is a FATAL error!");
				m_statusChanged(ESaveGameProviderStatus.PermanentError);
			}
		}
		else
		{
			m_gameSaveProviderHandle = gameSaveProviderHandle;
			m_taskScheduler.ExecuteNoWait(OnXGameSaveProviderHandleReady);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnXGameSaveProviderHandleReady()
	{
		ulong num = 0uL;
		if (Unity.XGamingRuntime.Interop.HR.SUCCEEDED(SDK.XGameSaveEnumerateContainerInfo(m_gameSaveProviderHandle, out var localInfos)))
		{
			XGameSaveContainerInfo[] array = localInfos;
			foreach (XGameSaveContainerInfo xGameSaveContainerInfo in array)
			{
				LogInfo($"Container '{xGameSaveContainerInfo.Name}' (display name '{xGameSaveContainerInfo.DisplayName}') has {xGameSaveContainerInfo.BlobCount} blobs. NeedsSync:{xGameSaveContainerInfo.NeedsSync} TotalSize:{xGameSaveContainerInfo.TotalSize} LastModifiedTime:{xGameSaveContainerInfo.LastModifiedTime}");
				num += xGameSaveContainerInfo.TotalSize;
			}
		}
		m_sizeTracker = new SizeTracker(m_maxSizeBytes, (long)num, GetRemainingQuotaAsync, shouldUpdateSizesOnEstimate: true);
		m_sizeTracker.RefreshAsync();
		m_rootSaveStorageContainer = new SaveStorageStorageContainerBlobs("root", m_gameSaveProviderHandle, m_taskScheduler);
		m_statusChanged(ESaveGameProviderStatus.Ok);
	}
}

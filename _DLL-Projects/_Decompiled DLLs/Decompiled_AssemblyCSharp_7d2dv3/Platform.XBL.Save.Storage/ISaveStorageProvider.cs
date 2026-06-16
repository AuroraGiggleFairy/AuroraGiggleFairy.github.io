using System;

namespace Platform.XBL.Save.Storage;

public interface ISaveStorageProvider : IDisposable
{
	SizeTracker SizeTracker { get; }

	ISaveStorageContainer RootSaveStorageContainer { get; }

	void InitializeAsync(IPlatform owner, long maxSizeBytes, SingleThreadTaskScheduler taskScheduler, OnGameSaveProviderStatusChanged statusChanged);

	void Flush(bool waitForFlush);
}

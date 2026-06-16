using System;

namespace Platform;

public interface IPlatformSaveGameProvider : IPlatformSaveGameIOProvider
{
	ESaveGameProviderStatus Status { get; }

	IPlatformSaveGameIOProvider Cache { get; }

	event Action Initialized;

	void Init(IPlatform _owner);

	void Destroy();

	bool ShouldBackup();

	bool ShouldCommit();

	double GetCommitProgress();

	void Flush(bool waitForFlush);

	bool ShouldLimitSize();

	void UpdateSizes();

	SaveDataSizes GetSizes();
}

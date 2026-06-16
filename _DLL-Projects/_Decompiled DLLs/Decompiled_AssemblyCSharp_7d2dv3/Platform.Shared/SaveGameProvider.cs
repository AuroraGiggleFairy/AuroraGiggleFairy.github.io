using System;
using System.IO;

namespace Platform.Shared;

public class SaveGameProvider : SaveGameIOProviderFixedRoot, IPlatformSaveGameProvider, IPlatformSaveGameIOProvider
{
	[PublicizedFrom(EAccessModifier.Private)]
	public long m_maxStorageSizeBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataSizes m_currentSize;

	public ESaveGameProviderStatus Status => ESaveGameProviderStatus.Ok;

	public IPlatformSaveGameIOProvider Cache => null;

	public event Action Initialized
	{
		add
		{
			value();
		}
		remove
		{
		}
	}

	public SaveGameProvider()
		: base(null)
	{
	}

	public SaveGameProvider(string rootPath, long maxStorageSizeBytes = 0L)
		: base(rootPath)
	{
		m_maxStorageSizeBytes = maxStorageSizeBytes;
		UpdateSizes();
	}

	public void Init(IPlatform _owner)
	{
	}

	public void Destroy()
	{
	}

	public bool ShouldBackup()
	{
		return true;
	}

	public bool ShouldCommit()
	{
		return true;
	}

	public double GetCommitProgress()
	{
		return 1.0;
	}

	public void Flush(bool waitForFlush)
	{
	}

	public bool ShouldLimitSize()
	{
		if (!string.IsNullOrEmpty(m_rootPath))
		{
			return m_maxStorageSizeBytes > 0;
		}
		return false;
	}

	public void UpdateSizes()
	{
		if (!ShouldLimitSize())
		{
			return;
		}
		long num = 0L;
		foreach (string item in Directory.EnumerateFiles(m_rootPath, "*", SearchOption.AllDirectories))
		{
			FileInfo fileInfo = new FileInfo(item);
			num += fileInfo.Length;
		}
		m_currentSize = new SaveDataSizes(m_maxStorageSizeBytes, m_maxStorageSizeBytes - num);
	}

	public SaveDataSizes GetSizes()
	{
		return m_currentSize;
	}
}

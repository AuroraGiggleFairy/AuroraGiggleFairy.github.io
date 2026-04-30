using System;
using System.IO;

public abstract class SdFileSystemInfo
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FileSystemInfo m_fileSystemInfo;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsManaged
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SaveDataManagedPath ManagedPath
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public virtual string FullName => m_fileSystemInfo.FullName;

	public string Extension => m_fileSystemInfo.Extension;

	public abstract string Name { get; }

	public abstract bool Exists { get; }

	public DateTime LastWriteTime => LastWriteTimeUtc.ToLocalTime();

	public DateTime LastWriteTimeUtc
	{
		get
		{
			if (IsManaged)
			{
				if (m_fileSystemInfo is DirectoryInfo)
				{
					return SdDirectory.ManagedGetLastWriteTimeUtc(ManagedPath);
				}
				return SdFile.ManagedGetLastWriteTimeUtc(ManagedPath);
			}
			return m_fileSystemInfo.LastWriteTimeUtc;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public SdFileSystemInfo(FileSystemInfo fileSystemInfo)
	{
		Reinitialize(fileSystemInfo);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public SdFileSystemInfo(FileSystemInfo fileSystemInfo, SaveDataManagedPath managedPath)
	{
		m_fileSystemInfo = fileSystemInfo;
		IsManaged = true;
		ManagedPath = managedPath;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Reinitialize(FileSystemInfo fileSystemInfo)
	{
		m_fileSystemInfo = fileSystemInfo;
		IsManaged = SaveDataUtils.TryGetManagedPath(fileSystemInfo.FullName, out var managedPath);
		ManagedPath = managedPath;
	}

	public abstract void Delete();

	public void Refresh()
	{
		m_fileSystemInfo.Refresh();
		Reinitialize(m_fileSystemInfo);
	}
}

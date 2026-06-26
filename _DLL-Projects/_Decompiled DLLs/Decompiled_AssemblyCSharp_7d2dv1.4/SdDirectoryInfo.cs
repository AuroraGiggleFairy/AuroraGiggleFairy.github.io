using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public sealed class SdDirectoryInfo : SdFileSystemInfo
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DirectoryInfo m_directoryInfo;

	public override string Name => m_directoryInfo.Name;

	public override string FullName => m_directoryInfo.FullName;

	public SdDirectoryInfo Parent => new SdDirectoryInfo(m_directoryInfo.Parent);

	public override bool Exists
	{
		get
		{
			if (!base.IsManaged)
			{
				return m_directoryInfo.Exists;
			}
			return SdDirectory.ManagedExists(base.ManagedPath);
		}
	}

	public SdDirectoryInfo Root => new SdDirectoryInfo(m_directoryInfo.Root);

	public SdDirectoryInfo(string path)
		: this(new DirectoryInfo(path))
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public SdDirectoryInfo(SaveDataManagedPath saveDataManagedPath)
		: this(new DirectoryInfo(saveDataManagedPath.GetOriginalPath()), saveDataManagedPath)
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public SdDirectoryInfo(DirectoryInfo directoryInfo)
		: base(directoryInfo)
	{
		m_directoryInfo = directoryInfo;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SdDirectoryInfo(DirectoryInfo directoryInfo, SaveDataManagedPath saveDataManagedPath)
		: base(directoryInfo, saveDataManagedPath)
	{
		m_directoryInfo = directoryInfo;
	}

	public SdDirectoryInfo CreateSubdirectory(string path)
	{
		if (base.IsManaged)
		{
			return ManagedCreateSubdirectory(path);
		}
		return new SdDirectoryInfo(m_directoryInfo.CreateSubdirectory(path));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SdDirectoryInfo ManagedCreateSubdirectory(string path)
	{
		return SdDirectory.CreateDirectory(Path.Combine(FullName, path));
	}

	public void Create()
	{
		if (base.IsManaged)
		{
			ManagedCreateDirectory();
		}
		else
		{
			m_directoryInfo.Create();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ManagedCreateDirectory()
	{
		SdDirectory.ManagedCreateDirectory(base.ManagedPath);
	}

	public SdFileInfo[] GetFiles(string searchPattern)
	{
		return EnumerateFiles(searchPattern).ToArray();
	}

	public SdFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
	{
		return EnumerateFiles(searchPattern, searchOption).ToArray();
	}

	public SdFileInfo[] GetFiles()
	{
		return EnumerateFiles().ToArray();
	}

	public SdDirectoryInfo[] GetDirectories()
	{
		return EnumerateDirectories().ToArray();
	}

	public SdDirectoryInfo[] GetDirectories(string searchPattern)
	{
		return EnumerateDirectories(searchPattern).ToArray();
	}

	public SdDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
	{
		return EnumerateDirectories(searchPattern, searchOption).ToArray();
	}

	public SdFileSystemInfo[] GetFileSystemInfos()
	{
		return EnumerateFileSystemInfos().ToArray();
	}

	public SdFileSystemInfo[] GetFileSystemInfos(string searchPattern)
	{
		return EnumerateFileSystemInfos(searchPattern).ToArray();
	}

	public SdFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
	{
		return EnumerateFileSystemInfos(searchPattern, searchOption).ToArray();
	}

	public IEnumerable<SdDirectoryInfo> EnumerateDirectories()
	{
		if (base.IsManaged)
		{
			return ManagedEnumerateDirectories("*", SearchOption.TopDirectoryOnly);
		}
		return from x in m_directoryInfo.EnumerateDirectories()
			select new SdDirectoryInfo(x);
	}

	public IEnumerable<SdDirectoryInfo> EnumerateDirectories(string searchPattern)
	{
		if (base.IsManaged)
		{
			return ManagedEnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);
		}
		return from x in m_directoryInfo.EnumerateDirectories(searchPattern)
			select new SdDirectoryInfo(x);
	}

	public IEnumerable<SdDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
	{
		if (base.IsManaged)
		{
			return ManagedEnumerateDirectories(searchPattern, searchOption);
		}
		return from x in m_directoryInfo.EnumerateDirectories(searchPattern, searchOption)
			select new SdDirectoryInfo(x);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<SdDirectoryInfo> ManagedEnumerateDirectories(string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryInfoEnumerateDirectories(base.ManagedPath, searchPattern, searchOption);
	}

	public IEnumerable<SdFileInfo> EnumerateFiles()
	{
		if (base.IsManaged)
		{
			return ManagedEnumerateFiles("*", SearchOption.TopDirectoryOnly);
		}
		return from x in m_directoryInfo.EnumerateFiles()
			select new SdFileInfo(x);
	}

	public IEnumerable<SdFileInfo> EnumerateFiles(string searchPattern)
	{
		if (base.IsManaged)
		{
			return ManagedEnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
		}
		return from x in m_directoryInfo.EnumerateFiles(searchPattern)
			select new SdFileInfo(x);
	}

	public IEnumerable<SdFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
	{
		if (base.IsManaged)
		{
			return ManagedEnumerateFiles(searchPattern, searchOption);
		}
		return from x in m_directoryInfo.EnumerateFiles(searchPattern, searchOption)
			select new SdFileInfo(x);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<SdFileInfo> ManagedEnumerateFiles(string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryInfoEnumerateFiles(base.ManagedPath, searchPattern, searchOption);
	}

	public IEnumerable<SdFileSystemInfo> EnumerateFileSystemInfos()
	{
		if (base.IsManaged)
		{
			return ManagedEnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
		}
		return m_directoryInfo.EnumerateFileSystemInfos().Select(WrapFileSystemInfo);
	}

	public IEnumerable<SdFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
	{
		if (base.IsManaged)
		{
			return ManagedEnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
		}
		return m_directoryInfo.EnumerateFileSystemInfos(searchPattern).Select(WrapFileSystemInfo);
	}

	public IEnumerable<SdFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
	{
		if (base.IsManaged)
		{
			return ManagedEnumerateFileSystemInfos(searchPattern, searchOption);
		}
		return m_directoryInfo.EnumerateFileSystemInfos(searchPattern, searchOption).Select(WrapFileSystemInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<SdFileSystemInfo> ManagedEnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryInfoEnumerateFileSystemInfos(base.ManagedPath, searchPattern, searchOption);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static SdFileSystemInfo WrapFileSystemInfo(FileSystemInfo fileSystemInfo)
	{
		if (!(fileSystemInfo is FileInfo fileInfo))
		{
			if (fileSystemInfo is DirectoryInfo directoryInfo)
			{
				return new SdDirectoryInfo(directoryInfo);
			}
			throw new NotImplementedException("Unsupported implementation of FileSystemInfo: " + fileSystemInfo.GetType().FullName + ".");
		}
		return new SdFileInfo(fileInfo);
	}

	public bool IsDirEmpty()
	{
		using IEnumerator<SdFileSystemInfo> enumerator = EnumerateFileSystemInfos("*", SearchOption.AllDirectories).GetEnumerator();
		return !enumerator.MoveNext();
	}

	public override void Delete()
	{
		if (base.IsManaged)
		{
			ManagedDelete(recursive: false);
		}
		else
		{
			m_directoryInfo.Delete();
		}
	}

	public void Delete(bool recursive)
	{
		if (base.IsManaged)
		{
			ManagedDelete(recursive);
		}
		else
		{
			m_directoryInfo.Delete(recursive);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ManagedDelete(bool recursive)
	{
		SdDirectory.ManagedDelete(base.ManagedPath, recursive);
	}

	public override string ToString()
	{
		return m_directoryInfo.ToString();
	}
}

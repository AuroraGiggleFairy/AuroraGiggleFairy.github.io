using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.XBL.Save;

public sealed class PathEnumerationInfo
{
	public readonly string RelativePath;

	public readonly bool IsDirectory;

	public readonly bool IsFile;

	public PathEnumerationInfo(string relativePath, bool isDirectory, bool isFile)
	{
		RelativePath = relativePath;
		IsDirectory = isDirectory;
		IsFile = isFile;
	}

	public override string ToString()
	{
		return string.Format("{0}[{1}=\"{2}\", {3}={4}, {5}={6}]", "PathEnumerationInfo", "RelativePath", RelativePath, "IsDirectory", IsDirectory, "IsFile", IsFile);
	}

	public void UsedOnlyForAOTCodeGeneration()
	{
		IEnumerable<SaveDataManagedPath> source = from _ in Enumerable.Empty<PathEnumerationInfo>()
			select (SaveDataManagedPath)null;
		source.Select([PublicizedFrom(EAccessModifier.Internal)] (SaveDataManagedPath _) => (SdFileSystemInfo)null);
		source.Select([PublicizedFrom(EAccessModifier.Internal)] (SaveDataManagedPath _) => (SdDirectoryInfo)null);
		source.Select([PublicizedFrom(EAccessModifier.Internal)] (SaveDataManagedPath _) => (SdFileInfo)null);
		throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
	}
}

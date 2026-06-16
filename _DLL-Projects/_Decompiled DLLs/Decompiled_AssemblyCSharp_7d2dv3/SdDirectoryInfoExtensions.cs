using System.Collections.Generic;
using System.IO;

public static class SdDirectoryInfoExtensions
{
	public static bool IsDirEmpty(this SdDirectoryInfo possiblyEmptyDir)
	{
		using IEnumerator<SdFileSystemInfo> enumerator = possiblyEmptyDir.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).GetEnumerator();
		return !enumerator.MoveNext();
	}
}

using System;
using System.Collections.Generic;

namespace Platform.Shared;

public static class SaveGameProviderHelper
{
	public static IEnumerable<SaveDataManagedPath> GetManagedPathsFromBaseAndSubPaths(SaveDataManagedPath path, string basePath, IEnumerable<string> subPaths)
	{
		foreach (string subPath in subPaths)
		{
			yield return GetManagedPathFromBaseAndSubPath(path, basePath, subPath);
		}
	}

	public static SaveDataManagedPath GetManagedPathFromBaseAndSubPath(SaveDataManagedPath path, string basePath, string subPath)
	{
		ReadOnlySpan<char> readOnlySpan = subPath.AsSpan(basePath.Length).TrimStart("\\/");
		return path.GetChildPath(readOnlySpan);
	}
}

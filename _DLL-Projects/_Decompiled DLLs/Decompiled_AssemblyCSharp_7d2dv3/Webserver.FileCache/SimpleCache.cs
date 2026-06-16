using System;
using System.Collections.Generic;
using System.IO;

namespace Webserver.FileCache;

public class SimpleCache : AbstractCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, byte[]> fileCache = new Dictionary<string, byte[]>();

	public override byte[] GetFileContent(string _filename)
	{
		try
		{
			lock (fileCache)
			{
				if (fileCache.TryGetValue(_filename, out var value))
				{
					return value;
				}
				if (!File.Exists(_filename))
				{
					return null;
				}
				byte[] array = File.ReadAllBytes(_filename);
				fileCache.Add(_filename, array);
				return array;
			}
		}
		catch (Exception arg)
		{
			Log.Out($"Error in SimpleCache.GetFileContent: {arg}");
		}
		return null;
	}

	public override (int, int) Invalidate()
	{
		(int, int) result = (0, 0);
		lock (fileCache)
		{
			result.Item1 = fileCache.Count;
			foreach (var (_, array2) in fileCache)
			{
				result.Item2 += array2.Length;
			}
			fileCache.Clear();
			return result;
		}
	}
}

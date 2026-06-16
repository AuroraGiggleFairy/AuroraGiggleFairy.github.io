using System;
using System.IO;

namespace Webserver.FileCache;

public class DirectAccess : AbstractCache
{
	public override byte[] GetFileContent(string _filename)
	{
		try
		{
			return File.Exists(_filename) ? File.ReadAllBytes(_filename) : null;
		}
		catch (Exception arg)
		{
			Log.Out($"Error in DirectAccess.GetFileContent: {arg}");
		}
		return null;
	}

	public override (int, int) Invalidate()
	{
		return (0, 0);
	}
}

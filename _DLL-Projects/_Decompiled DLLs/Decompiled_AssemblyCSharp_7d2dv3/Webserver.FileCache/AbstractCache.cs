using System.Collections.Generic;

namespace Webserver.FileCache;

public abstract class AbstractCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<AbstractCache> caches = new List<AbstractCache>();

	public abstract byte[] GetFileContent(string _filename);

	public abstract (int filesDropped, int bytesDropped) Invalidate();

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbstractCache()
	{
		caches.Add(this);
	}

	public static (int, int) InvalidateAllCaches()
	{
		int num = 0;
		int num2 = 0;
		foreach (AbstractCache cache in caches)
		{
			(int filesDropped, int bytesDropped) tuple = cache.Invalidate();
			int item = tuple.filesDropped;
			int item2 = tuple.bytesDropped;
			num += item;
			num2 += item2;
		}
		return (num, num2);
	}
}

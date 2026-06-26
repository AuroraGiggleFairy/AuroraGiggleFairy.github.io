using System;

public class FileBackedDecoOccupiedMap : IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FileBackedArray<EnumDecoOccupied> occupiedMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public int width;

	[PublicizedFrom(EAccessModifier.Private)]
	public int height;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cacheLength;

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArrayHandle cacheHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public Memory<EnumDecoOccupied> cache;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cacheStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cacheEnd;

	public FileBackedDecoOccupiedMap(int _worldWidth, int _worldHeight)
	{
		width = _worldWidth;
		height = _worldHeight;
		heightHalf = height / 2;
		occupiedMap = new FileBackedArray<EnumDecoOccupied>(width * height);
		cacheLength = width * 128;
		cacheEnd = cacheLength;
		cacheHandle = occupiedMap.GetMemory(0, cacheLength, out cache);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetDecoChunkRowCacheStart(int offset)
	{
		return offset / cacheLength * cacheLength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Cache(int offset)
	{
		if (offset >= cacheEnd || offset < cacheStart)
		{
			cacheStart = GetDecoChunkRowCacheStart(offset);
			cacheEnd = cacheStart + cacheLength;
			cacheHandle.Dispose();
			cacheHandle = occupiedMap.GetMemory(cacheStart, cacheLength, out cache);
		}
	}

	public EnumDecoOccupied Get(int _offs)
	{
		Cache(_offs);
		return cache.Span[_offs - cacheStart];
	}

	public void CopyDecoChunkRow(int row, EnumDecoOccupied[] from)
	{
		int num = heightHalf / 128;
		int start = (row + num) * 128 * width;
		Span<EnumDecoOccupied> span;
		using (occupiedMap.GetSpan(start, cacheLength, out span))
		{
			from.AsSpan(start, cacheLength).CopyTo(span);
		}
	}

	public void Dispose()
	{
		cacheHandle.Dispose();
		cacheHandle = null;
		occupiedMap.Dispose();
		occupiedMap = null;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

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
	public ReadOnlyMemory<EnumDecoOccupied> cache;

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
			cacheHandle?.Dispose();
			cacheHandle = occupiedMap.GetReadOnlyMemory(cacheStart, cacheLength, out cache);
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
		cacheHandle?.Dispose();
		cacheHandle = null;
		occupiedMap?.Dispose();
		occupiedMap = null;
	}

	public void SaveAsTexture(string path, bool includeFlatAreas = false, List<FlatArea> flatAreas = null)
	{
		Color32[] array = new Color32[occupiedMap.Length];
		for (int i = 0; i < occupiedMap.Length; i++)
		{
			Color color = Color.black;
			switch (Get(i))
			{
			case EnumDecoOccupied.SmallSlope:
				color = Color.blue;
				break;
			case EnumDecoOccupied.Stop_BigDeco:
				color = Color.gray;
				break;
			case EnumDecoOccupied.Perimeter:
				color = Color.red;
				break;
			case EnumDecoOccupied.Stop_AnyDeco:
				color = Color.cyan;
				break;
			case EnumDecoOccupied.Deco:
				color = Color.green;
				break;
			case EnumDecoOccupied.POI:
				color = Color.magenta;
				break;
			case EnumDecoOccupied.BigSlope:
				color = Color.yellow;
				break;
			case EnumDecoOccupied.NoneAllowed:
				color = Color.white;
				break;
			}
			array[i] = color;
		}
		if (includeFlatAreas)
		{
			if (flatAreas == null || flatAreas.Count == 0)
			{
				flatAreas = GameManager.Instance.World.FlatAreaManager?.GetAllFlatAreas();
			}
			if (flatAreas != null)
			{
				foreach (FlatArea flatArea in flatAreas)
				{
					foreach (Vector2i position in flatArea.GetPositions())
					{
						Color color2 = ((position.x == flatArea.position.x && position.y == flatArea.position.z) ? Color.red : ((flatArea.size != 16) ? new Color(0.5f, 0.5f, 0.5f, 1f) : new Color(0.75f, 0.75f, 0.75f, 1f)));
						int num = DecoManager.CheckPosition(width, height, position.x, position.y);
						array[num] = color2;
					}
				}
			}
		}
		Texture2D texture2D = new Texture2D(width, height);
		texture2D.SetPixels32(array);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, path);
		Log.Out("Saved deco texture to {0}", path);
		UnityEngine.Object.Destroy(texture2D);
	}
}

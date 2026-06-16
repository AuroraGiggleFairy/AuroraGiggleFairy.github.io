using System;
using System.IO;
using UnityEngine;
using Webserver.FileCache;

namespace MapRendering;

public class MapTileCache : AbstractCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class CurrentZoomFile
	{
		public string filename;

		public byte[] pngData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] transparentTile;

	[PublicizedFrom(EAccessModifier.Private)]
	public CurrentZoomFile[] cache;

	public MapTileCache(int _tileSize)
	{
		Texture2D texture2D = new Texture2D(_tileSize, _tileSize);
		Color color = new Color(0f, 0f, 0f, 0f);
		for (int i = 0; i < _tileSize; i++)
		{
			for (int j = 0; j < _tileSize; j++)
			{
				texture2D.SetPixel(i, j, color);
			}
		}
		transparentTile = texture2D.EncodeToPNG();
		UnityEngine.Object.Destroy(texture2D);
	}

	public void SetZoomCount(int _count)
	{
		cache = new CurrentZoomFile[_count];
		for (int i = 0; i < cache.Length; i++)
		{
			cache[i] = new CurrentZoomFile();
		}
	}

	public byte[] LoadTile(int _zoomlevel, string _filename)
	{
		try
		{
			lock (cache)
			{
				CurrentZoomFile currentZoomFile = cache[_zoomlevel];
				if (currentZoomFile.filename != null && currentZoomFile.filename.Equals(_filename))
				{
					return currentZoomFile.pngData;
				}
				currentZoomFile.filename = _filename;
				if (!File.Exists(_filename))
				{
					currentZoomFile.pngData = null;
					return null;
				}
				currentZoomFile.pngData = ReadAllBytes(_filename);
				return currentZoomFile.pngData;
			}
		}
		catch (Exception arg)
		{
			Log.Warning($"Error in MapTileCache.LoadTile: {arg}");
		}
		return null;
	}

	public void SaveTile(int _zoomlevel, byte[] _contentPng)
	{
		try
		{
			lock (cache)
			{
				CurrentZoomFile currentZoomFile = cache[_zoomlevel];
				string filename = currentZoomFile.filename;
				if (string.IsNullOrEmpty(filename))
				{
					return;
				}
				currentZoomFile.pngData = _contentPng;
				using Stream stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096);
				stream.Write(_contentPng, 0, _contentPng.Length);
			}
		}
		catch (Exception arg)
		{
			Log.Warning($"Error in MapTileCache.SaveTile: {arg}");
		}
	}

	public void ResetTile(int _zoomlevel)
	{
		try
		{
			lock (cache)
			{
				cache[_zoomlevel].filename = null;
				cache[_zoomlevel].pngData = null;
			}
		}
		catch (Exception arg)
		{
			Log.Warning($"Error in MapTileCache.ResetTile: {arg}");
		}
	}

	public override byte[] GetFileContent(string _filename)
	{
		try
		{
			lock (cache)
			{
				CurrentZoomFile[] array = cache;
				foreach (CurrentZoomFile currentZoomFile in array)
				{
					if (currentZoomFile.filename != null && currentZoomFile.filename.Equals(_filename))
					{
						return currentZoomFile.pngData;
					}
				}
				return (!File.Exists(_filename)) ? transparentTile : ReadAllBytes(_filename);
			}
		}
		catch (Exception arg)
		{
			Log.Warning($"Error in MapTileCache.GetFileContent: {arg}");
		}
		return null;
	}

	public override (int, int) Invalidate()
	{
		return (0, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] ReadAllBytes(string _path)
	{
		using FileStream fileStream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
		int num = 0;
		int num2 = (int)fileStream.Length;
		byte[] array = new byte[num2];
		while (num2 > 0)
		{
			int num3 = fileStream.Read(array, num, num2);
			if (num3 == 0)
			{
				throw new IOException("Unexpected end of stream");
			}
			num += num3;
			num2 -= num3;
		}
		return array;
	}
}

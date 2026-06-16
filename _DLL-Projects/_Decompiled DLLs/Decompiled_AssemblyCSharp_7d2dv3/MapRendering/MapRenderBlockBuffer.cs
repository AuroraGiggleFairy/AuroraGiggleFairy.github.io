using System;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace MapRendering;

public class MapRenderBlockBuffer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Texture2D blockMap = new Texture2D(Constants.MapBlockSize, Constants.MapBlockSize, Constants.DefaultTextureFormat, mipChain: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MapTileCache cache;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly NativeArray<int> emptyImageData;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Texture2D zoomBuffer = new Texture2D(Constants.MapBlockSize / 2, Constants.MapBlockSize / 2, Constants.DefaultTextureFormat, mipChain: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int zoomLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string folderBase;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i currentBlockMapPos = new Vector2i(int.MinValue, int.MinValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentBlockMapFolder = string.Empty;

	public TextureFormat FormatSelf => blockMap.format;

	public MapRenderBlockBuffer(int _level, MapTileCache _cache)
	{
		zoomLevel = _level;
		cache = _cache;
		folderBase = $"{Constants.MapDirectory}/{zoomLevel}/";
		Color color = new Color(0f, 0f, 0f, 0f);
		for (int i = 0; i < Constants.MapBlockSize; i++)
		{
			for (int j = 0; j < Constants.MapBlockSize; j++)
			{
				blockMap.SetPixel(i, j, color);
			}
		}
		NativeArray<int> rawTextureData = blockMap.GetRawTextureData<int>();
		emptyImageData = new NativeArray<int>(rawTextureData.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		rawTextureData.CopyTo(emptyImageData);
	}

	public void ResetBlock()
	{
		currentBlockMapFolder = string.Empty;
		currentBlockMapPos = new Vector2i(int.MinValue, int.MinValue);
		cache.ResetTile(zoomLevel);
	}

	public void SaveBlock()
	{
		try
		{
			saveTextureToFile();
		}
		catch (Exception arg)
		{
			Log.Warning($"Exception in MapRenderBlockBuffer.SaveBlock(): {arg}");
		}
	}

	public bool LoadBlock(Vector2i _block)
	{
		lock (blockMap)
		{
			if (currentBlockMapPos != _block)
			{
				string text;
				if (currentBlockMapPos.x != _block.x)
				{
					text = $"{folderBase}{_block.x}/";
					Directory.CreateDirectory(text);
				}
				else
				{
					text = currentBlockMapFolder;
				}
				string fileName = $"{text}{_block.y}.png";
				SaveBlock();
				loadTextureFromFile(fileName);
				currentBlockMapFolder = text;
				currentBlockMapPos = _block;
				return true;
			}
		}
		return false;
	}

	public void SetPart(Vector2i _offset, int _partSize, Color32[] _pixels)
	{
		if (_offset.x + _partSize > Constants.MapBlockSize || _offset.y + _partSize > Constants.MapBlockSize)
		{
			Log.Error($"MapBlockBuffer[{zoomLevel}].SetPart ({_offset}, {_partSize}, {_pixels.Length}) has blockMap.size ({Constants.MapBlockSize}/{Constants.MapBlockSize})");
		}
		else
		{
			blockMap.SetPixels32(_offset.x, _offset.y, _partSize, _partSize, _pixels);
		}
	}

	public Color32[] GetHalfScaled()
	{
		zoomBuffer.Reinitialize(Constants.MapBlockSize, Constants.MapBlockSize);
		if (blockMap.format == zoomBuffer.format)
		{
			NativeArray<byte> rawTextureData = blockMap.GetRawTextureData<byte>();
			NativeArray<byte> rawTextureData2 = zoomBuffer.GetRawTextureData<byte>();
			rawTextureData.CopyTo(rawTextureData2);
		}
		else
		{
			zoomBuffer.SetPixels32(blockMap.GetPixels32());
		}
		TextureScale.Point(zoomBuffer, Constants.MapBlockSize / 2, Constants.MapBlockSize / 2);
		return zoomBuffer.GetPixels32();
	}

	public void SetPartNative(Vector2i _offset, int _partSize, NativeArray<int> _pixels)
	{
		if (_offset.x + _partSize > Constants.MapBlockSize || _offset.y + _partSize > Constants.MapBlockSize)
		{
			Log.Error($"MapBlockBuffer[{zoomLevel}].SetPart ({_offset}, {_partSize}, {_pixels.Length}) has blockMap.size ({Constants.MapBlockSize}/{Constants.MapBlockSize})");
			return;
		}
		NativeArray<int> rawTextureData = blockMap.GetRawTextureData<int>();
		for (int i = 0; i < _partSize; i++)
		{
			int num = _partSize * i;
			int num2 = blockMap.width * (_offset.y + i) + _offset.x;
			for (int j = 0; j < _partSize; j++)
			{
				rawTextureData[num2 + j] = _pixels[num + j];
			}
		}
	}

	public NativeArray<int> GetHalfScaledNative()
	{
		if (zoomBuffer.format != blockMap.format || zoomBuffer.height != Constants.MapBlockSize / 2 || zoomBuffer.width != Constants.MapBlockSize / 2)
		{
			zoomBuffer.Reinitialize(Constants.MapBlockSize / 2, Constants.MapBlockSize / 2, blockMap.format, hasMipMap: false);
		}
		ScaleNative(blockMap, zoomBuffer);
		return zoomBuffer.GetRawTextureData<int>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ScaleNative(Texture2D _sourceTex, Texture2D _targetTex)
	{
		NativeArray<int> rawTextureData = _sourceTex.GetRawTextureData<int>();
		NativeArray<int> rawTextureData2 = _targetTex.GetRawTextureData<int>();
		int width = _sourceTex.width;
		int height = _sourceTex.height;
		int width2 = _targetTex.width;
		int height2 = _targetTex.height;
		float num = (float)width / (float)width2;
		float num2 = (float)height / (float)height2;
		for (int i = 0; i < height2; i++)
		{
			int num3 = (int)(num2 * (float)i) * width;
			int num4 = i * width2;
			for (int j = 0; j < width2; j++)
			{
				rawTextureData2[num4 + j] = rawTextureData[(int)((float)num3 + num * (float)j)];
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadTextureFromFile(string _fileName)
	{
		byte[] array = cache.LoadTile(zoomLevel, _fileName);
		if (array == null || !blockMap.LoadImage(array) || blockMap.height != Constants.MapBlockSize || blockMap.width != Constants.MapBlockSize)
		{
			if (array != null)
			{
				Log.Error("Map image tile " + _fileName + " has been corrupted, recreating tile");
			}
			if (blockMap.format != Constants.DefaultTextureFormat || blockMap.height != Constants.MapBlockSize || blockMap.width != Constants.MapBlockSize)
			{
				blockMap.Reinitialize(Constants.MapBlockSize, Constants.MapBlockSize, Constants.DefaultTextureFormat, hasMipMap: false);
			}
			blockMap.LoadRawTextureData(emptyImageData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveTextureToFile()
	{
		byte[] contentPng = blockMap.EncodeToPNG();
		cache.SaveTile(zoomLevel, contentPng);
	}
}

using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class HeightMapUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cGameHeightToU16Scale = 257f;

	public static float[,] ConvertDTMToHeightData(string levelName)
	{
		Texture2D texture2D = Resources.Load("Data/Worlds/" + levelName + "/dtm", typeof(Texture2D)) as Texture2D;
		if (texture2D == null)
		{
			return ConvertDTMToHeightDataExternal(levelName);
		}
		float[,] result = ConvertDTMToHeightData(texture2D);
		Resources.UnloadAsset(texture2D);
		return result;
	}

	public static float[,] ConvertDTMToHeightDataExternal(string levelName, bool loadPNG = true)
	{
		PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(levelName);
		if (location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			throw new FileNotFoundException();
		}
		string text = location.FullPath + "/dtm";
		Texture2D texture2D;
		if (loadPNG && SdFile.Exists(text + ".png"))
		{
			texture2D = TextureUtils.LoadTexture(text + ".png");
		}
		else
		{
			if (!SdFile.Exists(text + ".tga"))
			{
				throw new FileNotFoundException();
			}
			texture2D = TextureUtils.LoadTexture(text + ".tga");
		}
		float[,] result = ConvertDTMToHeightData(texture2D);
		UnityEngine.Object.Destroy(texture2D);
		return result;
	}

	public static float[,] ConvertDTMToHeightData(Color[] dtmPixs, int dtmSize, bool _bFlip = false)
	{
		float[,] array = new float[dtmSize, dtmSize];
		if (!_bFlip)
		{
			for (int i = 0; i < dtmSize; i++)
			{
				for (int j = 0; j < dtmSize; j++)
				{
					array[i, j] = dtmPixs[i + j * dtmSize].r * 255f;
				}
			}
		}
		else
		{
			for (int k = 0; k < dtmSize; k++)
			{
				for (int l = 0; l < dtmSize; l++)
				{
					array[k, dtmSize - l - 1] = dtmPixs[k + l * dtmSize].r * 255f;
				}
			}
		}
		return array;
	}

	public static float[,] ConvertDTMToHeightData(Texture2D dtm, bool _bFlip = false)
	{
		Color[] pixels = dtm.GetPixels();
		float[,] array = new float[dtm.width, dtm.height];
		if (!_bFlip)
		{
			for (int i = 0; i < dtm.width; i++)
			{
				for (int j = 0; j < dtm.height; j++)
				{
					array[i, j] = pixels[i + j * dtm.width].r * 255f;
				}
			}
		}
		else
		{
			int height = dtm.height;
			int width = dtm.width;
			for (int k = 0; k < width; k++)
			{
				for (int l = 0; l < height; l++)
				{
					array[k, height - l - 1] = pixels[k + l * dtm.width].r * 255f;
				}
			}
		}
		return array;
	}

	public static float[,] ConvertDTMToTerrainStampData(Texture2D dtm)
	{
		Color[] pixels = dtm.GetPixels();
		float[,] array = new float[dtm.width, dtm.height];
		for (int i = 0; i < dtm.width; i++)
		{
			for (int j = 0; j < dtm.height; j++)
			{
				array[i, j] = pixels[i + j * dtm.width].r;
			}
		}
		return array;
	}

	public static float[,] ConvertDTMToHeightData(Color32[] dtmPixs, int w, int h, bool _bFlip = false)
	{
		float[,] array = new float[w, h];
		if (!_bFlip)
		{
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					array[i, j] = (int)dtmPixs[i + j * w].r;
					if (dtmPixs[i + j * w].r != dtmPixs[i + j * w].g)
					{
						array[i, j] += (float)(int)dtmPixs[i + j * w].g / 255f;
					}
				}
			}
		}
		else
		{
			for (int k = 0; k < w; k++)
			{
				for (int l = 0; l < h; l++)
				{
					array[k, h - l - 1] = (int)dtmPixs[k + l * w].r;
					if (dtmPixs[k + l * w].r != dtmPixs[k + l * w].g)
					{
						array[k, h - l - 1] += (float)(int)dtmPixs[k + l * w].g / 255f;
					}
				}
			}
		}
		return array;
	}

	public static float[,] LoadRAWToHeightData(string _filePath)
	{
		ushort[] array = LoadHeightMapRAW(_filePath);
		int num = (int)Mathf.Sqrt(array.Length);
		int num2 = num;
		float[,] array2 = new float[num, num2];
		for (int num3 = num - 1; num3 >= 0; num3--)
		{
			for (int num4 = num2 - 1; num4 >= 0; num4--)
			{
				ushort num5 = array[num3 + num4 * num];
				array2[num3, num4] = (float)(int)num5 / 65280f * 255f;
			}
		}
		return array2;
	}

	public static ushort[] LoadHeightMapRAW(string _filePath)
	{
		using BufferedStream bufferedStream = new BufferedStream(SdFile.OpenRead(_filePath));
		int num = (int)Mathf.Sqrt(bufferedStream.Length);
		byte[] array = new byte[8192];
		ushort[] array2 = new ushort[bufferedStream.Length / 2];
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num4 < bufferedStream.Length)
		{
			int num5 = bufferedStream.Read(array, 0, array.Length);
			num4 += num5;
			int i = 0;
			int num6 = num2 + num3 * num;
			for (; i < num5; i += 2)
			{
				byte b = array[i];
				byte b2 = array[i + 1];
				array2[num6++] = (ushort)(b2 * 256 + b);
				num2++;
				if (num2 >= num)
				{
					num2 = 0;
					num3++;
					num6 = num2 + num3 * num;
				}
			}
		}
		return array2;
	}

	public static IBackedArray<ushort> LoadHeightMapRAW(string _filePath, int w, int h, float _fac = 1f, int _clampHeight = 1)
	{
		_clampHeight *= 256;
		if (_clampHeight <= 0)
		{
			_clampHeight = int.MaxValue;
		}
		ushort num = (ushort)_clampHeight;
		using Stream stream = SdFile.OpenRead(_filePath);
		int num2 = (int)stream.Length / 2 * 2;
		IBackedArray<ushort> backedArray = BackedArrays.Create<ushort>(w * h);
		int num3 = 0;
		while (num3 < num2)
		{
			int start = num3 / 2;
			int num4 = Math.Min(num2 - num3, 1048576);
			int length = num4 / 2;
			Span<ushort> span;
			using (backedArray.GetSpan(start, length, out span))
			{
				Span<byte> span2 = MemoryMarshal.Cast<ushort, byte>(span);
				int i;
				int num5;
				for (i = 0; i < num4; i += num5)
				{
					num5 = stream.Read(span2.Slice(i, num4 - i));
					if (num5 <= 0)
					{
						throw new IOException($"Unexpected end of stream. Expected {num2} bytes but only read {num3} bytes.");
					}
				}
				int num6 = i / 2;
				for (int j = 0; j < num6; j++)
				{
					if (span[j] > _clampHeight)
					{
						span[j] = num;
					}
				}
				num3 += i;
			}
		}
		return backedArray;
	}

	public static float[,] LoadHeightMapRAWAsUnityHeightMap(string _filePath, int w, int h, float _fac = 1f)
	{
		using BufferedStream bufferedStream = new BufferedStream(SdFile.OpenRead(_filePath));
		byte[] array = new byte[8192];
		float[,] array2 = new float[h, w];
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num3 < bufferedStream.Length)
		{
			int num4 = bufferedStream.Read(array, 0, array.Length);
			num3 += num4;
			for (int i = 0; i < num4; i += 2)
			{
				byte b = array[i];
				byte b2 = array[i + 1];
				array2[num2, num] = (float)(b2 * 256 + b) / 65535f;
				num++;
				if (num >= w)
				{
					num = 0;
					num2++;
				}
			}
		}
		return array2;
	}

	public static float[,] LoadHeightMapRAWAsStampData(string _filePath, float multiplier = 1f)
	{
		using BufferedStream bufferedStream = new BufferedStream(SdFile.OpenRead(_filePath));
		byte[] array = new byte[8192];
		int num = (int)Mathf.Sqrt(bufferedStream.Length / 2);
		float[,] array2 = new float[num, num];
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num4 < bufferedStream.Length)
		{
			int num5 = bufferedStream.Read(array, 0, array.Length);
			num4 += num5;
			for (int i = 0; i < num5; i += 2)
			{
				byte b = array[i];
				byte b2 = array[i + 1];
				array2[num3, num2] = (float)(b2 * 256 + b) / 65535f * multiplier;
				num2++;
				if (num2 >= num)
				{
					num2 = 0;
					num3++;
				}
			}
		}
		return array2;
	}

	public static void SaveHeightMapRAW(string _filePath, int w, int h, float[] _data)
	{
		using BufferedStream bufferedStream = new BufferedStream(SdFile.OpenWrite(_filePath));
		int num = w * 2;
		byte[] array = new byte[num];
		for (int i = 0; i < h; i++)
		{
			int num2 = i * w;
			for (int j = 0; j < w; j++)
			{
				uint num3 = (uint)(_data[num2 + j] * 257f);
				array[2 * j] = (byte)num3;
				array[2 * j + 1] = (byte)(num3 >> 8);
			}
			bufferedStream.Write(array, 0, num);
		}
	}

	public static void SaveHeightMapRAW(string _filePath, int w, int h, IBackedArray<ushort> _data)
	{
		using Stream stream = SdFile.OpenWrite(_filePath);
		int num = w * h;
		int num2 = 0;
		while (num2 < num)
		{
			int num3 = Math.Min(524288, num - num2);
			ReadOnlySpan<ushort> span;
			using (_data.GetReadOnlySpan(num2, num3, out span))
			{
				ReadOnlySpan<byte> buffer = MemoryMarshal.Cast<ushort, byte>(span);
				stream.Write(buffer);
				num2 += num3;
			}
		}
	}

	public static void SaveHeightMapRAW(string _filePath, int w, int h, float[,] _data)
	{
		using BufferedStream bufferedStream = new BufferedStream(SdFile.OpenWrite(_filePath));
		int num = w * 2;
		byte[] array = new byte[num];
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				uint num2 = (uint)(_data[j, i] * 257f);
				array[2 * j] = (byte)num2;
				array[2 * j + 1] = (byte)(num2 >> 8);
			}
			bufferedStream.Write(array, 0, num);
		}
	}

	public static void SaveHeightMapRAW(Stream _stream, float[] _data, float _offset)
	{
		for (int i = 0; i < _data.Length; i++)
		{
			int v = (int)((_data[i] + _offset) * 257f);
			v = Utils.FastClamp(v, 0, 65535);
			_stream.WriteByte((byte)v);
			_stream.WriteByte((byte)(v >> 8));
		}
	}

	public static float[,] SmoothTerrain(int Passes, float[,] HeightData)
	{
		int length = HeightData.GetLength(0);
		int length2 = HeightData.GetLength(1);
		float[,] array = new float[length, length2];
		while (Passes > 0)
		{
			Passes--;
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					int num = 0;
					float num2 = 0f;
					if (i - 1 > 0)
					{
						num2 += HeightData[i - 1, j];
						num++;
						if (j - 1 > 0)
						{
							num2 += HeightData[i - 1, j - 1];
							num++;
						}
						if (j + 1 < length2)
						{
							num2 += HeightData[i - 1, j + 1];
							num++;
						}
					}
					if (i + 1 < length)
					{
						num2 += HeightData[i + 1, j];
						num++;
						if (j - 1 > 0)
						{
							num2 += HeightData[i + 1, j - 1];
							num++;
						}
						if (j + 1 < length2)
						{
							num2 += HeightData[i + 1, j + 1];
							num++;
						}
					}
					if (j - 1 > 0)
					{
						num2 += HeightData[i, j - 1];
						num++;
					}
					if (j + 1 < length2)
					{
						num2 += HeightData[i, j + 1];
						num++;
					}
					array[i, j] = (HeightData[i, j] + num2 / (float)num) * 0.5f;
				}
			}
			float[,] array2 = array;
			array = HeightData;
			HeightData = array2;
		}
		return HeightData;
	}

	public static float[,][,] ConvertAndSliceUnityHeightmap(ushort[] _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceAtPix, int _resStep)
	{
		int num = _heightMapWidth / _sliceAtPix;
		int num2 = _heightMapHeight / _sliceAtPix;
		float[,][,] array = new float[num, num2][,];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float[,] array2 = new float[_sliceAtPix / _resStep + 1, _sliceAtPix / _resStep + 1];
				for (int num3 = _sliceAtPix; num3 >= 0; num3 -= _resStep)
				{
					for (int num4 = _sliceAtPix; num4 >= 0; num4 -= _resStep)
					{
						int num5 = j * _sliceAtPix + num3;
						int num6 = i * _sliceAtPix + num4;
						if (num5 >= _heightMapWidth)
						{
							num5 = _heightMapWidth - 1;
						}
						if (num6 >= _heightMapHeight)
						{
							num6 = _heightMapHeight - 1;
						}
						ushort num7 = _rawHeightMap[num5 + num6 * _heightMapWidth];
						array2[num4 / _resStep, num3 / _resStep] = (float)(int)num7 / 65280f;
					}
				}
				array[j, i] = array2;
			}
		}
		return array;
	}

	public static float[,][,] ConvertAndSliceUnityHeightmapQuartered(IBackedArray<ushort> _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceAtPix)
	{
		int num = _heightMapWidth / _sliceAtPix;
		int num2 = _heightMapHeight / _sliceAtPix;
		float[,][,] array = new float[num, num2][,];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				array[j, i] = ConvertUnityHeightmapSliceQuartered(_rawHeightMap, _heightMapWidth, _heightMapHeight, j, i, _sliceAtPix);
			}
		}
		return array;
	}

	public unsafe static float[,] ConvertUnityHeightmapSliceQuartered(IBackedArray<ushort> _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceX, int _sliceZ, int _sliceAtPix)
	{
		float[,] array = new float[_sliceAtPix / 2 + 1, _sliceAtPix / 2 + 1];
		fixed (float* pointer = array)
		{
			Span<float> tempHeightMapData = new Span<float>(pointer, array.GetLength(0) * array.GetLength(1));
			ConvertUnityHeightmapSliceQuartered(_rawHeightMap, _heightMapWidth, _heightMapHeight, _sliceX, _sliceZ, _sliceAtPix, tempHeightMapData);
		}
		return array;
	}

	public static TileFile<float> ConvertAndSliceUnityHeightmapQuarteredToFile(IBackedArray<ushort> _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceAtPix)
	{
		int num = _heightMapWidth / _sliceAtPix;
		int num2 = _heightMapHeight / _sliceAtPix;
		int num3 = _sliceAtPix / 2 + 1;
		FileBackedArray<float> fileBackedArray = new FileBackedArray<float>(num * num2 * num3 * num3);
		int num4 = num3 * num3;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int start = i * num4 * num + j * num4;
				Span<float> span;
				using (fileBackedArray.GetSpan(start, num4, out span))
				{
					ConvertUnityHeightmapSliceQuartered(_rawHeightMap, _heightMapWidth, _heightMapHeight, j, i, _sliceAtPix, span);
				}
			}
		}
		return new TileFile<float>(fileBackedArray, num3, num, num2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ConvertUnityHeightmapSliceQuartered(IBackedArray<ushort> _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceX, int _sliceZ, int _sliceAtPix, Span<float> tempHeightMapData)
	{
		int num = _heightMapWidth - 1;
		int num2 = _heightMapHeight - 1;
		int num3 = Math.Max((_sliceZ * _sliceAtPix - 1) * _heightMapWidth, 0);
		int length = Math.Min(((_sliceZ + 1) * _sliceAtPix + 2) * _heightMapWidth, _rawHeightMap.Length) - num3;
		ReadOnlySpan<ushort> span;
		using (_rawHeightMap.GetReadOnlySpan(num3, length, out span))
		{
			int num4 = _sliceAtPix / 2 + 1;
			for (int i = 0; i <= _sliceAtPix; i += 2)
			{
				int num5 = _sliceZ * _sliceAtPix + i;
				if (num5 >= num2)
				{
					num5 = _heightMapHeight - 2;
				}
				else if (num5 < 1)
				{
					num5 = 1;
				}
				int num6 = num5 * _heightMapWidth;
				for (int j = 0; j <= _sliceAtPix; j += 2)
				{
					int num7 = _sliceX * _sliceAtPix + j;
					if (num7 >= num)
					{
						num7 = _heightMapWidth - 2;
					}
					else if (num7 < 1)
					{
						num7 = 1;
					}
					int num8 = num7 + num6 - num3;
					uint num9 = span[num8 - _heightMapWidth];
					ushort num10 = span[num8 - 1];
					uint num11 = span[num8];
					uint num12 = span[num8 + 1];
					uint num13 = span[num8 + _heightMapWidth];
					ushort num14 = (ushort)((num10 + num11 + num12 + num9 + num13) / 5);
					int num15 = i / 2;
					int num16 = j / 2;
					tempHeightMapData[num15 * num4 + num16] = (float)(int)num14 / 65280f;
				}
			}
		}
	}

	public static Color32[,][] ConvertAndSliceSplatmap(Color32[] _splat, int _splatMapWidth, int _splatMapHeight, int _overlapBorderPix, int _sliceAtWidth, int _resStep)
	{
		int num = _splatMapWidth / _sliceAtWidth;
		int num2 = _splatMapHeight / _sliceAtWidth;
		Color32[,][] array = new Color32[num, num2][];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num3 = _sliceAtWidth / _resStep + 2 * _overlapBorderPix;
				int num4 = _sliceAtWidth / _resStep + 2 * _overlapBorderPix;
				Color32[] array2 = new Color32[num3 * num4];
				for (int k = -_resStep * _overlapBorderPix; k < _sliceAtWidth + _overlapBorderPix; k += _resStep)
				{
					for (int l = -_resStep * _overlapBorderPix; l < _sliceAtWidth + _overlapBorderPix; l += _resStep)
					{
						int num5 = j * _sliceAtWidth + k;
						int num6 = i * _sliceAtWidth + l;
						if (num5 < 0)
						{
							num5 += _splatMapHeight;
						}
						else if (num5 >= _splatMapWidth)
						{
							num5 -= _splatMapWidth;
						}
						if (num6 < 0)
						{
							num6 += _splatMapHeight;
						}
						else if (num6 >= _splatMapHeight)
						{
							num6 -= _splatMapHeight;
						}
						Color32 color = _splat[num5 + num6 * _splatMapWidth];
						array2[k / _resStep + _overlapBorderPix + (l / _resStep + _overlapBorderPix) * num3] = color;
					}
				}
				array[j, i] = array2;
			}
		}
		return array;
	}
}

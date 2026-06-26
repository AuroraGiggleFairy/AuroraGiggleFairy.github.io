using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public struct BiomeImageLoader
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct BiomePixel
	{
		public byte c1;

		public byte c2;

		public byte c3;

		public byte c4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate uint PixelToBiomeValue(BiomePixel pix);

	[PublicizedFrom(EAccessModifier.Private)]
	public static PixelToBiomeValue fromARGB32 = BiomeValueFromARGB32;

	[PublicizedFrom(EAccessModifier.Private)]
	public static PixelToBiomeValue fromRGBA32 = BiomeValueFromRGBA32;

	[PublicizedFrom(EAccessModifier.Private)]
	public PixelToBiomeValue toBiomeValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D biomesTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, BiomeDefinition> biomeDefinitions;

	public GridCompressedData<byte> biomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isError;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint lastBiomeValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte biomeId;

	[PublicizedFrom(EAccessModifier.Private)]
	public static uint BiomeValueFromARGB32(BiomePixel _argb)
	{
		int num = _argb.c2 << 16;
		uint num2 = (uint)(_argb.c3 << 8);
		uint c = _argb.c4;
		return (uint)num | num2 | c;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static uint BiomeValueFromRGBA32(BiomePixel _rgba)
	{
		int num = _rgba.c1 << 16;
		uint num2 = (uint)(_rgba.c2 << 8);
		uint c = _rgba.c3;
		return (uint)num | num2 | c;
	}

	public BiomeImageLoader(Texture2D _biomesTex, Dictionary<uint, BiomeDefinition> _biomeDefinitions)
	{
		if (_biomesTex.format == TextureFormat.ARGB32)
		{
			toBiomeValue = fromARGB32;
		}
		else
		{
			if (_biomesTex.format != TextureFormat.RGBA32)
			{
				throw new Exception($"Unsupported biome texture format: {_biomesTex.format}");
			}
			toBiomeValue = fromRGBA32;
		}
		biomesTex = _biomesTex;
		biomeDefinitions = _biomeDefinitions;
		int num = 16;
		biomeMap = new GridCompressedData<byte>(biomesTex.width, biomesTex.height, num, num);
		isError = false;
		lastBiomeValue = 0u;
		biomeId = byte.MaxValue;
	}

	public IEnumerator Load()
	{
		isError = false;
		lastBiomeValue = 0u;
		biomeId = byte.MaxValue;
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		NativeArray<BiomePixel> biomePixs = biomesTex.GetPixelData<BiomePixel>(0);
		int blockSize = biomeMap.cellSizeX;
		int blockIndex = 0;
		for (int blockY = 0; blockY < biomeMap.heightCells; blockY++)
		{
			for (int blockX = 0; blockX < biomeMap.widthCells; blockX++)
			{
				int num = blockY * biomeMap.cellSizeY;
				int num2 = blockX * biomeMap.cellSizeX;
				int num3 = num2 + num * biomesTex.width;
				BiomePixel pix = biomePixs[num2 + num * biomesTex.width];
				uint iBiomeValue = toBiomeValue(pix);
				biomeMap.SetSameValue(blockIndex, GetBiomeId(iBiomeValue));
				for (int i = 0; i < blockSize; i++)
				{
					for (int j = 0; j < blockSize; j++)
					{
						num3 = num2 + j + (num + i) * biomesTex.width;
						iBiomeValue = toBiomeValue(biomePixs[num3]);
						biomeMap.SetValue(blockIndex, j, i, GetBiomeId(iBiomeValue));
					}
				}
				if (num3 % 8192 == 0 && msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
				blockIndex++;
			}
		}
		biomePixs.Dispose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte GetBiomeId(uint iBiomeValue)
	{
		if (lastBiomeValue != iBiomeValue)
		{
			lastBiomeValue = iBiomeValue;
			if (biomeDefinitions.TryGetValue(iBiomeValue, out var value))
			{
				biomeId = value.m_Id;
			}
		}
		return biomeId;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 BiomeIdToColor32(uint iBiomeValue)
	{
		byte r = (byte)((iBiomeValue >> 16) & 0xFF);
		byte g = (byte)((iBiomeValue >> 8) & 0xFF);
		byte b = (byte)(iBiomeValue & 0xFF);
		return new Color32(r, g, b, 0);
	}
}

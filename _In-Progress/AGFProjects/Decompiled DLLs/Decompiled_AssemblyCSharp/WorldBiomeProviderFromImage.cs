using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;

public class WorldBiomeProviderFromImage : IBiomeProvider
{
	[PublicizedFrom(EAccessModifier.Private)]
	public GridCompressedData<byte> m_BiomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBiomes m_Biomes;

	[PublicizedFrom(EAccessModifier.Private)]
	public PerlinNoise noiseGen;

	[PublicizedFrom(EAccessModifier.Private)]
	public string worldName;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomeMapWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomesMapWidthHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomeMapHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomesMapHeightHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomesScaleDiv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int radiationMapSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int radiationMapScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] radiationMapSmall;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRadiationTileSize = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public int radiationTilesX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int radiationTilesZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] splatMapMaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cntSplatChannels;

	[PublicizedFrom(EAccessModifier.Private)]
	public int splatScaleDiv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int splatW;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldSizeHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue bvReturn = BlockValue.Air;

	public WorldBiomeProviderFromImage(string _levelName, WorldBiomes _biomes, int _worldSize = 4096)
	{
		worldName = _levelName;
		m_Biomes = _biomes;
		worldSize = _worldSize;
		worldSizeHalf = _worldSize / 2;
	}

	public IEnumerator InitData()
	{
		PathAbstractions.AbstractedLocation worldLocation = PathAbstractions.WorldsSearchPaths.GetLocation(worldName);
		string text = worldLocation.FullPath + "/biomes";
		Texture2D biomesTex = ((!SdFile.Exists(text + ".tga")) ? TextureUtils.LoadTexture(text + ".png") : TextureUtils.LoadTexture(text + ".tga"));
		yield return null;
		biomeMapWidth = biomesTex.width;
		biomeMapHeight = biomesTex.height;
		biomesMapWidthHalf = biomesTex.width / 2;
		biomesMapHeightHalf = biomesTex.height / 2;
		Log.Out("Biomes image size w= " + biomesTex.width + ", h = " + biomesTex.height);
		yield return null;
		BiomeImageLoader biomesLoader = new BiomeImageLoader(biomesTex, m_Biomes.GetBiomeMap());
		yield return biomesLoader.Load();
		m_BiomeMap = biomesLoader.biomeMap;
		yield return null;
		biomesScaleDiv = worldSize / biomesTex.width;
		UnityEngine.Object.Destroy(biomesTex);
		noiseGen = new PerlinNoise(worldName.GetStableHashCode());
		yield return null;
		Texture2D texture2D = null;
		text = worldLocation.FullPath + "/radiation";
		if (SdFile.Exists(text + ".png"))
		{
			texture2D = TextureUtils.LoadTexture(text + ".png");
		}
		else if (SdFile.Exists(text + ".tga"))
		{
			texture2D = TextureUtils.LoadTexture(text + ".tga");
		}
		if (!(texture2D != null))
		{
			yield break;
		}
		radiationMapSize = texture2D.width;
		radiationMapScale = worldSize / radiationMapSize;
		if (texture2D.width <= 512 && texture2D.height <= 512)
		{
			radiationMapSmall = new byte[texture2D.width * texture2D.height];
			if (texture2D.format == TextureFormat.RGBA32)
			{
				using NativeArray<Color32> nativeArray = texture2D.GetPixelData<Color32>(0);
				for (int i = 0; i < nativeArray.Length; i++)
				{
					radiationMapSmall[i] = nativeArray[i].r;
				}
			}
			else
			{
				using NativeArray<TextureUtils.ColorARGB32> nativeArray2 = texture2D.GetPixelData<TextureUtils.ColorARGB32>(0);
				for (int j = 0; j < nativeArray2.Length; j++)
				{
					radiationMapSmall[j] = nativeArray2[j].r;
				}
			}
		}
		else
		{
			Log.Out("Radiation ignored {0}", radiationMapSize);
		}
		UnityEngine.Object.Destroy(texture2D);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FillRadiationResult<T>(NativeArray<T> radPixs, byte[,][,] result, Func<T, byte> processColor) where T : struct
	{
		for (int i = 0; i < result.GetLength(0); i++)
		{
			for (int j = 0; j < result.GetLength(1); j++)
			{
				result[j, i] = new byte[512, 512];
				for (int k = 0; k < 512; k++)
				{
					for (int l = 0; l < 512; l++)
					{
						int index = (i * 512 + k) * radiationMapSize + (j * 512 + l);
						T arg = radPixs[index];
						byte b = processColor(arg);
						result[j, i][l, k] = b;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FillRadiationFileBackedArray<T>(NativeArray<T> radPixs, FileBackedArray<byte> fba, Func<T, byte> processColor) where T : struct
	{
		for (int i = 0; i < radiationTilesZ; i++)
		{
			for (int j = 0; j < radiationTilesX; j++)
			{
				int start = i * 512 * 512 * radiationTilesX + j * 512 * 512;
				int num = 0;
				Span<byte> span;
				using (fba.GetSpan(start, 262144, out span))
				{
					for (int k = 0; k < 512; k++)
					{
						for (int l = 0; l < 512; l++)
						{
							int index = (i * 512 + l) * radiationMapSize + (j * 512 + k);
							T arg = radPixs[index];
							byte b = processColor(arg);
							span[num] = b;
							num++;
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte ProcessColor(Color32 pixel)
	{
		byte result = 0;
		if (pixel.g > 0)
		{
			result = 1;
		}
		if (pixel.b > 0)
		{
			result = 2;
		}
		if (pixel.r > 0)
		{
			result = 3;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte ProcessColor(TextureUtils.ColorARGB32 pixel)
	{
		byte result = 0;
		if (pixel.g > 0)
		{
			result = 1;
		}
		if (pixel.b > 0)
		{
			result = 2;
		}
		if (pixel.r > 0)
		{
			result = 3;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RadiationTileArrayFileFromTexture(FileBackedArray<byte> fba, Texture2D radiationTexture)
	{
		if (radiationTexture.format == TextureFormat.RGBA32)
		{
			using (NativeArray<Color32> radPixs = radiationTexture.GetPixelData<Color32>(0))
			{
				FillRadiationFileBackedArray(radPixs, fba, ProcessColor);
				return;
			}
		}
		using NativeArray<TextureUtils.ColorARGB32> radPixs2 = radiationTexture.GetPixelData<TextureUtils.ColorARGB32>(0);
		FillRadiationFileBackedArray(radPixs2, fba, ProcessColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[,][,] RadiationTileArrayFromTexture(Texture2D radiationTexture)
	{
		int num = radiationTexture.width / 512;
		int num2 = radiationTexture.height / 512;
		byte[,][,] result = new byte[num, num2][,];
		if (radiationTexture.format == TextureFormat.RGBA32)
		{
			using NativeArray<Color32> radPixs = radiationTexture.GetPixelData<Color32>(0);
			FillRadiationResult(radPixs, result, ProcessColor);
		}
		else
		{
			using NativeArray<TextureUtils.ColorARGB32> radPixs2 = radiationTexture.GetPixelData<TextureUtils.ColorARGB32>(0);
			FillRadiationResult(radPixs2, result, ProcessColor);
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void worldCoordsToTileCoords(int worldX, int worldZ, TileAreaConfig tileAreaConfig, out int tileX, out int tileZ, out int posX, out int posZ)
	{
		tileX = (worldX + worldSizeHalf) / 512 + tileAreaConfig.tileStart.x;
		tileZ = (worldZ + worldSizeHalf) / 512 + tileAreaConfig.tileStart.y;
		posX = worldX - tileX * 512;
		posZ = worldZ - tileZ * 512;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileArea<byte[,]> LoadRadiationMap(Texture2D radiationTex, TileAreaConfig tileAreaConfig)
	{
		byte[,][,] data = RadiationTileArrayFromTexture(radiationTex);
		return new TileArea<byte[,]>(tileAreaConfig, data);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileArea<byte[,]> LoadRadiationMapToFile(Texture2D radiationTex, TileAreaConfig tileAreaConfig)
	{
		FileBackedArray<byte> fileBackedArray = new FileBackedArray<byte>(radiationTex.width * radiationTex.height);
		RadiationTileArrayFileFromTexture(fileBackedArray, radiationTex);
		TileFile<byte> tileFile = new TileFile<byte>(fileBackedArray, 512, radiationTilesX, radiationTilesZ);
		return new TileAreaCache<byte>(tileAreaConfig, tileFile, 9);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadSplatMaps(string _levelName, int _worldWidth)
	{
		string fullPath = PathAbstractions.WorldsSearchPaths.GetLocation(_levelName).FullPath;
		string text = fullPath + "/splat1.png";
		if (!SdFile.Exists(text))
		{
			return;
		}
		Texture2D texture2D = TextureUtils.LoadTexture(text);
		Color32[] pixels = texture2D.GetPixels32();
		Color32[] array = null;
		Color32[] array2 = null;
		splatW = texture2D.width;
		_ = texture2D.height;
		splatScaleDiv = _worldWidth / splatW;
		if (Application.isEditor)
		{
			UnityEngine.Object.DestroyImmediate(texture2D);
		}
		else
		{
			UnityEngine.Object.Destroy(texture2D);
		}
		cntSplatChannels += 4;
		string text2 = fullPath + "/splat2.png";
		if (SdFile.Exists(text2))
		{
			Texture2D texture2D2 = TextureUtils.LoadTexture(text2);
			array = texture2D2.GetPixels32();
			if (Application.isEditor)
			{
				UnityEngine.Object.DestroyImmediate(texture2D2);
			}
			else
			{
				UnityEngine.Object.Destroy(texture2D2);
			}
			cntSplatChannels += 4;
		}
		string text3 = fullPath + "/splat3.png";
		if (SdFile.Exists(text3))
		{
			Texture2D texture2D3 = TextureUtils.LoadTexture(text3);
			array2 = texture2D3.GetPixels32();
			if (Application.isEditor)
			{
				UnityEngine.Object.DestroyImmediate(texture2D3);
			}
			else
			{
				UnityEngine.Object.Destroy(texture2D3);
			}
			cntSplatChannels += 4;
		}
		splatMapMaxValue = new byte[pixels.Length];
		Color32 color = new Color32(0, 0, 0, 0);
		Color32 color2 = new Color32(0, 0, 0, 0);
		Color32 color3 = new Color32(0, 0, 0, 0);
		for (int num = pixels.Length - 1; num >= 0; num--)
		{
			color = pixels[num];
			if (cntSplatChannels > 4)
			{
				color2 = array[num];
			}
			if (cntSplatChannels > 8)
			{
				color3 = array2[num];
			}
			int num2 = 0;
			if (color.r >= color.g && color.r >= color.b && color.r >= color.a && color.r >= color2.r && color.r >= color2.g && color.r >= color2.b && color.r >= color2.a && color.r >= color3.r && color.r >= color3.g && color.r >= color3.b && color.r >= color3.a)
			{
				num2 = 0;
			}
			else if (color.g >= color.r && color.g >= color.b && color.g >= color.a && color.g >= color2.r && color.g >= color2.g && color.g >= color2.b && color.g >= color2.a && color.g >= color3.r && color.g >= color3.g && color.g >= color3.b && color.g >= color3.a)
			{
				num2 = 1;
			}
			else if (color.b >= color.r && color.b >= color.g && color.b >= color.a && color.b >= color2.r && color.b >= color2.g && color.b >= color2.b && color.b >= color2.a && color.b >= color3.r && color.b >= color3.g && color.b >= color3.b && color.b >= color3.a)
			{
				num2 = 2;
			}
			else if (color.a >= color.r && color.a >= color.g && color.a >= color.b && color.a >= color2.r && color.a >= color2.g && color.a >= color2.b && color.a >= color2.a && color.a >= color3.r && color.a >= color3.g && color.a >= color3.b && color.a >= color3.a)
			{
				num2 = 3;
			}
			else if (color2.r >= color2.g && color2.r >= color2.b && color2.r >= color2.a && color2.r >= color.r && color2.r >= color.g && color2.r >= color.b && color2.r >= color.a && color2.r >= color3.r && color2.r >= color3.g && color2.r >= color3.b && color2.r >= color3.a)
			{
				num2 = 4;
			}
			else if (color2.g >= color2.r && color2.g >= color2.b && color2.g >= color2.a && color2.g >= color.r && color2.g >= color.g && color2.g >= color.b && color2.g >= color.a && color2.g >= color3.r && color2.g >= color3.g && color2.g >= color3.b && color2.g >= color3.a)
			{
				num2 = 5;
			}
			else if (color2.b >= color2.r && color2.b >= color2.g && color2.b >= color2.a && color2.b >= color.r && color2.b >= color.g && color2.b >= color.b && color2.b >= color.a && color2.b >= color3.r && color2.b >= color3.g && color2.b >= color3.b && color2.b >= color3.a)
			{
				num2 = 6;
			}
			else if (color2.a >= color2.r && color2.a >= color2.g && color2.a >= color2.b && color2.a >= color.r && color2.a >= color.g && color2.a >= color.b && color2.a >= color.a && color2.a >= color3.r && color2.a >= color3.g && color2.a >= color3.b && color2.a >= color3.a)
			{
				num2 = 7;
			}
			else if (color3.r >= color3.g && color3.r >= color3.b && color3.r >= color3.a && color3.r >= color.r && color3.r >= color.g && color3.r >= color.b && color3.r >= color.a && color3.r >= color2.r && color3.r >= color2.g && color3.r >= color2.b && color3.r >= color2.a)
			{
				num2 = 8;
			}
			else if (color3.g >= color3.r && color3.g >= color3.b && color3.g >= color3.a && color3.g >= color.r && color3.g >= color.g && color3.g >= color.b && color3.g >= color.a && color3.g >= color2.r && color3.g >= color2.g && color3.g >= color2.b && color3.g >= color2.a)
			{
				num2 = 9;
			}
			else if (color3.b >= color3.r && color3.b >= color3.g && color3.b >= color3.a && color3.b >= color.r && color3.b >= color.g && color3.b >= color.b && color3.b >= color.a && color3.b >= color2.r && color3.b >= color2.g && color3.b >= color2.b && color3.b >= color3.a)
			{
				num2 = 10;
			}
			else if (color3.a >= color3.r && color3.a >= color3.g && color3.a >= color3.b && color3.a >= color.r && color3.a >= color.g && color3.a >= color.b && color3.a >= color.a && color3.a >= color2.r && color3.a >= color2.g && color3.a >= color2.b && color3.a >= color2.a)
			{
				num2 = 11;
			}
			splatMapMaxValue[num] = (byte)num2;
		}
	}

	public string GetWorldName()
	{
		return worldName;
	}

	public void Init(int _seed, string _worldName, WorldBiomes _biomes, string _params1, string _params2)
	{
	}

	public BiomeDefinition GetBiomeAt(int x, int z, out float _intensity)
	{
		_intensity = 1f;
		return GetBiomeAt(x, z);
	}

	public BiomeDefinition GetBiomeAt(int x, int z)
	{
		if (biomesScaleDiv == 0)
		{
			return null;
		}
		int num = x / biomesScaleDiv + biomesMapWidthHalf;
		if (num < 0 || num >= m_BiomeMap.width)
		{
			return null;
		}
		int num2 = z / biomesScaleDiv + biomesMapHeightHalf;
		if (num2 < 0 || num2 >= m_BiomeMap.height)
		{
			return null;
		}
		byte value = m_BiomeMap.GetValue(num, num2);
		if (value == byte.MaxValue)
		{
			return null;
		}
		return m_Biomes.GetBiome(value);
	}

	public BiomeDefinition GetBiomeOrSubAt(int x, int z)
	{
		BiomeDefinition biomeDefinition = GetBiomeAt(x, z);
		if (biomeDefinition != null)
		{
			int subBiomeIdxAt = GetSubBiomeIdxAt(biomeDefinition, x, 0, z);
			if (subBiomeIdxAt >= 0)
			{
				biomeDefinition = biomeDefinition.subbiomes[subBiomeIdxAt];
			}
		}
		return biomeDefinition;
	}

	public int GetSubBiomeIdxAt(BiomeDefinition bd, int _x, int _y, int _z)
	{
		double num = -3.4028234663852886E+38;
		double num2 = 0.0;
		Vector2 vector = Vector2.zero;
		for (int i = 0; i < bd.subbiomes.Count; i++)
		{
			BiomeDefinition biomeDefinition = bd.subbiomes[i];
			if (num != (double)biomeDefinition.noiseFreq || vector != biomeDefinition.noiseOffset)
			{
				num = biomeDefinition.noiseFreq;
				vector = biomeDefinition.noiseOffset;
				num2 = (float)noiseGen.FBM((float)_x + vector.x, (float)_z + vector.y, num);
				num2 = num2 * 0.5 + 0.5;
			}
			if (num2 >= (double)biomeDefinition.noiseMin && num2 < (double)biomeDefinition.noiseMax)
			{
				return i;
			}
		}
		return -1;
	}

	public Vector2i GetSize()
	{
		return new Vector2i(biomeMapWidth, biomeMapHeight);
	}

	public float GetHumidityAt(int x, int z)
	{
		return 0f;
	}

	public float GetTemperatureAt(int x, int z)
	{
		return 0f;
	}

	public float GetRadiationAt(int x, int z)
	{
		if (radiationMapSmall != null)
		{
			int num = (x + worldSizeHalf) / radiationMapScale + (z + worldSizeHalf) / radiationMapScale * radiationMapSize;
			if (num >= 0 && num < radiationMapSmall.Length)
			{
				return (int)radiationMapSmall[num];
			}
			return 0f;
		}
		return 0f;
	}

	public BlockValue GetTopmostBlockValue(int xWorld, int zWorld)
	{
		bvReturn.type = 0;
		if (xWorld < -worldSizeHalf || xWorld >= worldSizeHalf || zWorld < -worldSizeHalf || zWorld >= worldSizeHalf)
		{
			return bvReturn;
		}
		if (cntSplatChannels > 0)
		{
			switch (splatMapMaxValue[(xWorld + worldSizeHalf) / splatScaleDiv + (zWorld + worldSizeHalf) / splatScaleDiv * splatW])
			{
			case 0:
				bvReturn.type = 14;
				break;
			case 1:
				bvReturn.type = 1;
				break;
			case 2:
				bvReturn.type = 9;
				break;
			case 3:
				bvReturn.type = 8;
				break;
			case 4:
				bvReturn.type = 12;
				break;
			case 5:
				bvReturn.type = 13;
				break;
			case 6:
				bvReturn.type = 16;
				break;
			case 7:
				bvReturn.type = 11;
				break;
			case 8:
				bvReturn.type = 3;
				break;
			case 9:
				bvReturn.type = 29;
				break;
			case 10:
				bvReturn.type = 28;
				break;
			case 11:
				bvReturn.type = 2;
				break;
			}
		}
		return bvReturn;
	}

	public void Cleanup()
	{
	}
}

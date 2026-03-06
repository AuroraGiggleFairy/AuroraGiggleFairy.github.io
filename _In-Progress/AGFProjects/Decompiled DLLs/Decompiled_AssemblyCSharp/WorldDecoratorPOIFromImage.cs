using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using UnityEngine;

public class WorldDecoratorPOIFromImage : IWorldDecorator
{
	public WorldGridCompressedData<byte> m_Poi;

	public int worldScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldSizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldSizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public HeightMap heightMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicPrefabDecorator m_PrefabDecorator;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bChangeWaterDensity;

	[PublicizedFrom(EAccessModifier.Private)]
	public PathAbstractions.AbstractedLocation worldLocation;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D splat3Tex;

	public int splat3Width;

	public int splat3Height;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D splat4Tex;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] water16x16Chunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int water16x16ChunksW;

	public WorldDecoratorPOIFromImage(string _levelName, DynamicPrefabDecorator _prefabDecorator, int _worldX, int _worldZ, Texture2D _splat3Tex = null, bool _bChangeWaterDensity = true, int _worldScale = 1, HeightMap _heightMap = null, Texture2D _splat4Tex = null)
	{
		m_PrefabDecorator = _prefabDecorator;
		bChangeWaterDensity = _bChangeWaterDensity;
		worldScale = _worldScale;
		worldSizeX = _worldX;
		worldSizeZ = _worldZ;
		heightMap = _heightMap;
		worldLocation = PathAbstractions.WorldsSearchPaths.GetLocation(_levelName);
		splat3Tex = _splat3Tex;
		splat4Tex = _splat4Tex;
	}

	public IEnumerator InitData()
	{
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		int num = worldSizeX * worldScale;
		int num2 = worldSizeZ * worldScale;
		GridCompressedData<byte> poiCols = new GridCompressedData<byte>(num, num2, 16, 16);
		water16x16ChunksW = num / 16;
		water16x16Chunks = new byte[num / 16 * num2 / 16];
		yield return null;
		List<WaterInfo> waterSources = LoadWaterInfo(worldLocation.FullPath + "/water_info.xml");
		BlockValue dirtBV = new BlockValue((uint)Block.GetBlockByName("terrDirt").blockID);
		byte b = 5;
		WorldBiomes.Instance.AddPoiMapElement(new PoiMapElement(b, null, dirtBV, dirtBV, 0, -1, 0, 0));
		byte idSand = b;
		yield return null;
		if (splat4Tex != null)
		{
			if (splat4Tex.format != TextureFormat.ARGB32)
			{
				throw new Exception($"splat4Tex was not in the correct format. Expected: {TextureFormat.ARGB32}, Actual: {splat4Tex.format}");
			}
			NativeArray<TextureUtils.ColorARGB32> waterColors = splat4Tex.GetPixelData<TextureUtils.ColorARGB32>(0);
			int w = splat4Tex.width;
			int h = splat4Tex.height;
			for (int z = 0; z < h; z++)
			{
				int num3 = z * w;
				for (int i = 0; i < w; i++)
				{
					TextureUtils.ColorARGB32 colorARGB = waterColors[i + num3];
					if (colorARGB.b > 0)
					{
						b = (byte)Mathf.Clamp(5 + colorARGB.b, 0, 255);
						if (WorldBiomes.Instance.getPoiForColor(b) == null)
						{
							WorldBiomes.Instance.AddPoiMapElement(new PoiMapElement(b, null, new BlockValue((uint)Block.GetBlockByName("water").blockID), new BlockValue((uint)Block.GetBlockByName("terrDirt").blockID), 0, -1, colorARGB.b + 1, 0));
						}
						poiCols.SetValue(i, z, b);
						water16x16Chunks[i / 16 + z / 16 * w / 16] = (byte)(b - 5);
					}
				}
				if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
			}
			waterColors.Dispose();
		}
		else if (waterSources != null && waterSources.Count > 0)
		{
			BlockValue waterBV = new BlockValue(240u);
			List<Vector2i> floodList = new List<Vector2i>(100000);
			for (int h = 0; h < waterSources.Count; h++)
			{
				WaterInfo waterInfo = waterSources[h];
				b = (byte)Mathf.Clamp(5 + waterInfo.pos.y, 0, 255);
				WorldBiomes.Instance.AddPoiMapElement(new PoiMapElement(b, null, waterBV, dirtBV, 0, -1, waterInfo.pos.y + 1, 0));
				GameUtils.WaterFloodFill(poiCols, water16x16Chunks, worldSizeX * worldScale, heightMap, waterInfo.pos.x, waterInfo.pos.y, waterInfo.pos.z, b, idSand, floodList, waterInfo.minX, waterInfo.maxX, waterInfo.minZ, waterInfo.maxZ, worldScale);
				floodList.Clear();
				if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
			}
		}
		yield return null;
		heightMap = null;
		int zE = worldSizeZ * worldScale;
		int xE = worldSizeX * worldScale;
		Texture2D localSplat3 = null;
		if (splat3Tex == null)
		{
			localSplat3 = (SdFile.Exists(worldLocation.FullPath + "/splat3.png") ? TextureUtils.LoadTexture(worldLocation.FullPath + "/splat3.png") : TextureUtils.LoadTexture(worldLocation.FullPath + " / splat3.tga"));
			if (localSplat3.width != xE || localSplat3.height != zE)
			{
				localSplat3.BilinearScale(xE, zE);
			}
			splat3Tex = localSplat3;
		}
		bool flag = splat3Tex.format == TextureFormat.ARGB32;
		if (!flag && splat3Tex.format != TextureFormat.RGBA32)
		{
			Log.Error("World's splat3 file is not in the correct format (needs to be either RGBA32 or ARGB32)!");
			yield break;
		}
		int splatMapWidth = splat3Tex.width;
		splat3Width = splat3Tex.width;
		splat3Height = splat3Tex.height;
		if (splat3Tex == null)
		{
			yield break;
		}
		if (flag)
		{
			NativeArray<TextureUtils.ColorARGB32> waterColors = splat3Tex.GetRawTextureData<TextureUtils.ColorARGB32>();
			for (int h = 0; h < zE; h++)
			{
				for (int j = 0; j < xE; j++)
				{
					TextureUtils.ColorARGB32 colorARGB2 = waterColors[j / worldScale + h / worldScale * splatMapWidth];
					if (colorARGB2.r > 127)
					{
						poiCols.SetValue(j, h, 2);
					}
					else if (colorARGB2.g > 127)
					{
						poiCols.SetValue(j, h, 3);
					}
					else if (colorARGB2.b > 127)
					{
						poiCols.SetValue(j, h, 4);
					}
				}
				if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
			}
		}
		else
		{
			NativeArray<Color32> splat3Cols = splat3Tex.GetRawTextureData<Color32>();
			for (int h = 0; h < zE; h++)
			{
				for (int k = 0; k < xE; k++)
				{
					Color32 color = splat3Cols[k / worldScale + h / worldScale * splatMapWidth];
					if (color.r > 127)
					{
						poiCols.SetValue(k, h, 2);
					}
					else if (color.g > 127)
					{
						poiCols.SetValue(k, h, 3);
					}
					else if (color.b > 127)
					{
						poiCols.SetValue(k, h, 4);
					}
				}
				if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
			}
		}
		poiCols.CheckSameValues();
		m_Poi = new WorldGridCompressedData<byte>(poiCols);
		splat3Tex = null;
		if (localSplat3 != null)
		{
			UnityEngine.Object.Destroy(localSplat3);
		}
	}

	public void GetWaterChunks16x16(out int _water16x16ChunksW, out byte[] _water16x16Chunks)
	{
		_water16x16ChunksW = water16x16ChunksW;
		_water16x16Chunks = water16x16Chunks;
	}

	public static List<WaterInfo> LoadWaterInfo(string _filename)
	{
		if (!SdFile.Exists(_filename))
		{
			return null;
		}
		List<WaterInfo> list = new List<WaterInfo>();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.SdLoad(_filename);
		foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			if (childNode.NodeType != XmlNodeType.Element || !childNode.Name.EqualsCaseInsensitive("Water"))
			{
				continue;
			}
			XmlElement xmlElement = (XmlElement)childNode;
			string attribute = xmlElement.GetAttribute("pos");
			if (attribute.Length != 0)
			{
				WaterInfo item = new WaterInfo
				{
					pos = StringParsers.ParseVector3i(attribute),
					minX = int.MinValue
				};
				string attribute2 = xmlElement.GetAttribute("minx");
				if (attribute2.Length > 0)
				{
					item.minX = int.Parse(attribute2);
				}
				item.maxX = int.MaxValue;
				attribute2 = xmlElement.GetAttribute("maxx");
				if (attribute2.Length > 0)
				{
					item.maxX = int.Parse(attribute2);
				}
				item.minZ = int.MinValue;
				attribute2 = xmlElement.GetAttribute("minz");
				if (attribute2.Length > 0)
				{
					item.minZ = int.Parse(attribute2);
				}
				item.maxZ = int.MaxValue;
				attribute2 = xmlElement.GetAttribute("maxz");
				if (attribute2.Length > 0)
				{
					item.maxZ = int.Parse(attribute2);
				}
				list.Add(item);
			}
		}
		return list;
	}

	public void DecorateChunkOverlapping(World _world, Chunk _chunk, Chunk _c10, Chunk _c01, Chunk _c11, int seed)
	{
		if (m_Poi == null)
		{
			return;
		}
		if (_c10 == null || _c01 == null || _c11 == null)
		{
			Log.Warning("Adjacent chunk missing on decoration " + _chunk);
			return;
		}
		GameRandom gameRandom = Utils.RandomFromSeedOnPos(_chunk.X, _chunk.Z, seed);
		GameManager.Instance.GetSpawnPointList();
		Vector3i worldPos = _chunk.GetWorldPos();
		bool flag = m_PrefabDecorator.IsWithinTraderArea(_chunk.worldPosIMin, _chunk.worldPosIMax);
		for (int i = 0; i < 16; i++)
		{
			int blockWorldPosZ = _chunk.GetBlockWorldPosZ(i);
			for (int j = 0; j < 16; j++)
			{
				int blockWorldPosX = _chunk.GetBlockWorldPosX(j);
				if ((flag && m_PrefabDecorator.GetTraderAtPosition(new Vector3i(blockWorldPosX, 0, blockWorldPosZ), 0) != null) || !m_Poi.Contains(blockWorldPosX, blockWorldPosZ))
				{
					continue;
				}
				byte data = m_Poi.GetData(blockWorldPosX, blockWorldPosZ);
				if (data == 0 || data == byte.MaxValue)
				{
					continue;
				}
				PoiMapElement poiForColor = _world.Biomes.getPoiForColor(data);
				if (poiForColor == null)
				{
					continue;
				}
				EnumDecoAllowed decoAllowedAt = _chunk.GetDecoAllowedAt(j, i);
				if (!poiForColor.m_BlockValue.isWater)
				{
					if (decoAllowedAt.IsNothing())
					{
						continue;
					}
					_chunk.SetDecoAllowedStreetOnlyAt(j, i, _newVal: true);
				}
				int num = poiForColor.m_YPos;
				if (num < 0)
				{
					num = _chunk.GetTerrainHeight(j, i);
				}
				if (!poiForColor.m_BlockValue.isair)
				{
					BlockValue blockValue = poiForColor.m_BlockValue;
					PoiMapDecal randomDecal = poiForColor.GetRandomDecal(gameRandom);
					if (randomDecal != null && _chunk.GetTerrainNormalY(j, i) > 0.98f && _world.GetBlock(new Vector3i(blockWorldPosX, num, blockWorldPosZ) + new Vector3i(Utils.BlockFaceToVector(randomDecal.face))).isair)
					{
						blockValue.hasdecal = true;
						blockValue.decalface = randomDecal.face;
						blockValue.decaltex = (byte)randomDecal.textureIndex;
					}
					BlockValue block = _chunk.GetBlock(j, num, i);
					if (block.isair || (block.Block.shape.IsTerrain() && blockValue.Block.shape.IsTerrain()))
					{
						_chunk.SetBlockRaw(j, num, i, blockValue);
					}
					if (poiForColor.m_BlockValue.isWater)
					{
						for (int k = num; k <= poiForColor.m_YPosFill; k++)
						{
							if (WaterUtils.CanWaterFlowThrough(_chunk.GetBlock(j, k, i)))
							{
								_chunk.SetWater(j, k, i, WaterValue.Full);
							}
						}
					}
					else
					{
						for (int l = num; l <= poiForColor.m_YPosFill; l++)
						{
							if (_chunk.GetBlock(j, l, i).isair)
							{
								_chunk.SetBlockRaw(j, l, i, blockValue);
							}
						}
					}
					if (block.Block.shape.IsTerrain() && !poiForColor.m_BlockBelow.isair)
					{
						_chunk.SetBlockRaw(j, num, i, poiForColor.m_BlockBelow);
					}
					if (poiForColor.m_YPosFill > 0 && bChangeWaterDensity)
					{
						if (!blockValue.Block.shape.IsTerrain())
						{
							_chunk.SetDensity(j, num, i, MarchingCubes.DensityAir);
						}
						for (int m = num; m <= poiForColor.m_YPosFill; m++)
						{
							_chunk.SetBlockRaw(j, m, i, blockValue);
							if (!blockValue.Block.shape.IsTerrain())
							{
								_chunk.SetDensity(j, m, i, MarchingCubes.DensityAir);
							}
						}
					}
					PoiMapBlock randomBlockOnTop = poiForColor.GetRandomBlockOnTop(gameRandom);
					if (randomBlockOnTop == null || (randomBlockOnTop.offset == 0 && !_world.GetBlock(new Vector3i(blockWorldPosX, num + 1, blockWorldPosZ)).isair))
					{
						continue;
					}
					blockValue = (_world.IsEditor() ? randomBlockOnTop.blockValue : BlockPlaceholderMap.Instance.Replace(randomBlockOnTop.blockValue, gameRandom, _chunk, blockWorldPosX, 0, blockWorldPosZ, FastTags<TagGroup.Global>.none));
					int num2 = num + 1 + randomBlockOnTop.offset;
					Vector3i blockPos = new Vector3i(worldPos.x + j, worldPos.y + num2, worldPos.z + i);
					if (DecoUtils.CanPlaceDeco(_chunk, _c10, _c01, _c11, blockPos, blockValue))
					{
						DecoUtils.ApplyDecoAllowed(_chunk, _c10, _c01, _c11, blockPos, blockValue);
						Block block2 = blockValue.Block;
						blockValue = block2.OnBlockPlaced(_world, 0, blockPos, blockValue, gameRandom);
						if (!block2.HasTileEntity)
						{
							_chunk.SetBlockRaw(j, num2, i, blockValue);
						}
						else
						{
							_chunk.SetBlock(_world, j, num2, i, blockValue);
						}
					}
				}
				else if (poiForColor.m_sModelName != null && poiForColor.m_sModelName.Length > 0 && m_PrefabDecorator != null)
				{
					m_PrefabDecorator.GetPrefab(poiForColor.m_sModelName).CopyIntoLocal(_world.ChunkClusters[0], new Vector3i(blockWorldPosX, num, blockWorldPosZ), _bOverwriteExistingBlocks: false, _bSetChunkToRegenerate: false, FastTags<TagGroup.Global>.none);
				}
			}
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}
}

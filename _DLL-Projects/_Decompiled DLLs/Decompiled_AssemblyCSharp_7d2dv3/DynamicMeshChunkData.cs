using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

public class DynamicMeshChunkData
{
	public class ChunkNeighbourData
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public List<uint> BlockRaw { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public List<sbyte> Densities { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public List<long> Textures { get; set; }

		public void Clear()
		{
			BlockRaw.Clear();
			Densities.Clear();
			Textures.Clear();
		}

		public void SetData(uint blockraw, sbyte density, long texture)
		{
			BlockRaw.Add(blockraw);
			if (blockraw != 0)
			{
				Densities.Add(density);
				Textures.Add(texture);
			}
		}

		public void Copy(ChunkNeighbourData other)
		{
			BlockRaw.Clear();
			Densities.Clear();
			Textures.Clear();
			BlockRaw.AddRange(other.BlockRaw);
			Densities.AddRange(other.Densities);
			Textures.AddRange(other.Textures);
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(BlockRaw.Count);
			foreach (uint item in BlockRaw)
			{
				writer.Write(item);
			}
			writer.Write(Densities.Count);
			foreach (sbyte density in Densities)
			{
				writer.Write(density);
			}
			writer.Write(Textures.Count);
			foreach (long texture in Textures)
			{
				writer.Write(texture);
			}
		}

		public void Read(PooledBinaryReader reader)
		{
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				BlockRaw.Add(reader.ReadUInt32());
			}
			num = reader.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				Densities.Add(reader.ReadSByte());
			}
			num = reader.ReadInt32();
			for (int k = 0; k < num; k++)
			{
				Textures.Add(reader.ReadInt64());
			}
		}

		public static ChunkNeighbourData Create()
		{
			return new ChunkNeighbourData
			{
				BlockRaw = new List<uint>(),
				Densities = new List<sbyte>(),
				Textures = new List<long>()
			};
		}

		public static ChunkNeighbourData CreateMax()
		{
			return new ChunkNeighbourData
			{
				BlockRaw = new List<uint>(65280),
				Densities = new List<sbyte>(65280),
				Textures = new List<long>(65280)
			};
		}

		public static ChunkNeighbourData CreateCorner()
		{
			return new ChunkNeighbourData
			{
				BlockRaw = new List<uint>(255),
				Densities = new List<sbyte>(255),
				Textures = new List<long>(255)
			};
		}
	}

	public int MinTerrainHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkNeighbourData[,] _neighbours = new ChunkNeighbourData[3, 3];

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastRaw;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastDen;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastTex;

	public static int ActiveDataItems = 0;

	public static ConcurrentQueue<DynamicMeshChunkData> Cache = new ConcurrentQueue<DynamicMeshChunkData>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int X { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int OffsetY { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Z { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int UpdateTime { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte MainBiome { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int EndY { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<byte> TerrainHeight { get; set; } = new List<byte>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<byte> Height { get; set; } = new List<byte>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<byte> TopSoil { get; set; } = new List<byte>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<uint> BlockRaw { get; set; } = new List<uint>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<sbyte> Densities { get; set; } = new List<sbyte>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<long> Textures { get; set; } = new List<long>();

	public int TotalBlocks => Math.Min((EndY - OffsetY + 1) * 256, BlockRaw.Count);

	public ChunkNeighbourData GetNeighbourData(int x, int z)
	{
		return _neighbours[x + 1, z + 1];
	}

	public void SetNeighbourData(int x, int z, ChunkNeighbourData data)
	{
		_neighbours[x + 1, z + 1] = data;
	}

	public void Copy(DynamicMeshChunkData other)
	{
		if (other == null)
		{
			return;
		}
		Reset();
		X = other.X;
		OffsetY = other.OffsetY;
		MinTerrainHeight = other.MinTerrainHeight;
		Z = other.Z;
		UpdateTime = other.UpdateTime;
		EndY = other.EndY;
		MainBiome = other.MainBiome;
		TerrainHeight.AddRange(other.TerrainHeight);
		Height.AddRange(other.Height);
		TopSoil.AddRange(other.TopSoil);
		int totalBlocks = other.TotalBlocks;
		for (int i = 0; i < totalBlocks; i++)
		{
			BlockRaw.Add(other.BlockRaw[i]);
		}
		Densities.AddRange(other.Densities);
		Textures.AddRange(other.Textures);
		for (int j = -1; j < 2; j++)
		{
			for (int k = -1; k < 2; k++)
			{
				if (j != 0 || k != 0)
				{
					GetNeighbourData(j, k).Copy(other.GetNeighbourData(j, k));
				}
			}
		}
	}

	public static DynamicMeshChunkData LoadFromStream(MemoryStream stream)
	{
		stream.Position = 0L;
		DynamicMeshChunkData fromCache = GetFromCache("_LoadStream_");
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		pooledBinaryReader.SetBaseStream(stream);
		fromCache.Read(pooledBinaryReader);
		return fromCache;
	}

	public void RecordCounts()
	{
		lastRaw = BlockRaw.Count;
		lastDen = Densities.Count;
		lastTex = Textures.Count;
	}

	public void ClearPreviousLayers()
	{
		if (lastRaw > 0)
		{
			BlockRaw.RemoveRange(0, lastRaw);
		}
		if (lastDen > 0)
		{
			Densities.RemoveRange(0, lastDen);
		}
		if (lastTex > 0)
		{
			Textures.RemoveRange(0, lastTex);
		}
	}

	public void Reset()
	{
		X = 0;
		OffsetY = 0;
		Z = 0;
		UpdateTime = 0;
		MainBiome = 0;
		EndY = 0;
		TerrainHeight.Clear();
		Height.Clear();
		TopSoil.Clear();
		BlockRaw.Clear();
		Densities.Clear();
		Textures.Clear();
		MinTerrainHeight = 500;
		GetNeighbourData(-1, -1).Clear();
		GetNeighbourData(-1, 0).Clear();
		GetNeighbourData(-1, 1).Clear();
		GetNeighbourData(1, 1).Clear();
		GetNeighbourData(1, 0).Clear();
		GetNeighbourData(1, -1).Clear();
		GetNeighbourData(0, -1).Clear();
		GetNeighbourData(0, 1).Clear();
	}

	public void SetTopSoil(byte[] soil)
	{
		TopSoil.AddRange(soil);
	}

	public int GetStreamSize()
	{
		int num = 81 + TerrainHeight.Count + Height.Count + TopSoil.Count + 4 * BlockRaw.Count + Densities.Count + 8 * Textures.Count;
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				if (i != 0 || j != 0)
				{
					ChunkNeighbourData neighbourData = GetNeighbourData(i, j);
					num += 4 * neighbourData.BlockRaw.Count + neighbourData.Densities.Count + 8 * neighbourData.Textures.Count;
				}
			}
		}
		return num;
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(X);
		writer.Write(OffsetY);
		writer.Write(Z);
		writer.Write(EndY);
		writer.Write(MinTerrainHeight);
		writer.Write(UpdateTime);
		writer.Write(MainBiome);
		writer.Write(TerrainHeight.Count);
		foreach (byte item in TerrainHeight)
		{
			writer.Write(item);
		}
		writer.Write(Height.Count);
		foreach (byte item2 in Height)
		{
			writer.Write(item2);
		}
		writer.Write(TopSoil.Count);
		foreach (byte item3 in TopSoil)
		{
			writer.Write(item3);
		}
		int totalBlocks = TotalBlocks;
		writer.Write(totalBlocks);
		for (int i = 0; i < totalBlocks; i++)
		{
			writer.Write(BlockRaw[i]);
		}
		writer.Write(Densities.Count);
		foreach (sbyte density in Densities)
		{
			writer.Write(density);
		}
		writer.Write(Textures.Count);
		foreach (long texture in Textures)
		{
			writer.Write(texture);
		}
		for (int j = -1; j < 2; j++)
		{
			for (int k = -1; k < 2; k++)
			{
				if (j != 0 || k != 0)
				{
					GetNeighbourData(j, k).Write(writer);
				}
			}
		}
	}

	public void Read(PooledBinaryReader reader)
	{
		X = reader.ReadInt32();
		OffsetY = reader.ReadInt32();
		Z = reader.ReadInt32();
		EndY = reader.ReadInt32();
		MinTerrainHeight = reader.ReadInt32();
		UpdateTime = reader.ReadInt32();
		MainBiome = reader.ReadByte();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			TerrainHeight.Add(reader.ReadByte());
		}
		num = reader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			Height.Add(reader.ReadByte());
		}
		num = reader.ReadInt32();
		for (int k = 0; k < num; k++)
		{
			TopSoil.Add(reader.ReadByte());
		}
		num = reader.ReadInt32();
		for (int l = 0; l < num; l++)
		{
			BlockRaw.Add(reader.ReadUInt32());
		}
		num = reader.ReadInt32();
		for (int m = 0; m < num; m++)
		{
			Densities.Add(reader.ReadSByte());
		}
		num = reader.ReadInt32();
		for (int n = 0; n < num; n++)
		{
			Textures.Add(reader.ReadInt64());
		}
		for (int num2 = -1; num2 < 2; num2++)
		{
			for (int num3 = -1; num3 < 2; num3++)
			{
				if (num2 != 0 || num3 != 0)
				{
					GetNeighbourData(num2, num3).Read(reader);
				}
			}
		}
	}

	public void ApplyToChunk(Chunk chunk, ChunkCacheNeighborChunks cacheNeighbourChunks)
	{
		int index = 0;
		chunk.X = X;
		chunk.Z = Z;
		chunk.SetTopSoil(TopSoil);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				chunk.SetHeight(i, j, Height[index]);
				chunk.SetTerrainHeight(i, j, TerrainHeight[index++]);
			}
		}
		int num = 0;
		int num2 = 0;
		int value = 0;
		BlockValue blockValue = default(BlockValue);
		for (int k = 0; k < 256; k++)
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					chunk.SetLight(i, k, j, 15, Chunk.LIGHT_TYPE.SUN);
					if (k < OffsetY - 1 || k >= EndY)
					{
						chunk.SetBlockRaw(i, k, j, BlockValue.Air);
						chunk.SetDensity(i, k, j, MarchingCubes.DensityAir);
						chunk.SetTextureFull(i, k, j, 0L);
						continue;
					}
					blockValue.rawData = BlockRaw[num];
					bool flag;
					if (flag = DynamicMeshSettings.UseImposterValues && DynamicMeshBlockSwap.BlockSwaps.TryGetValue(blockValue.type, out value))
					{
						if (value == 0)
						{
							blockValue.rawData = 0u;
							num2++;
						}
						else
						{
							blockValue.type = value;
						}
					}
					if (blockValue.rawData == 0)
					{
						chunk.SetBlockRaw(i, k, j, BlockValue.Air);
						chunk.SetDensity(i, k, j, MarchingCubes.DensityAir);
						chunk.SetTextureFull(i, k, j, 0L);
					}
					else
					{
						long num3 = (Block.list[blockValue.type].shape.IsTerrain() ? 0 : Textures[num2]);
						if (flag)
						{
							DynamicMeshBlockSwap.TextureSwaps.TryGetValue(value, out var value2);
							if (num3 == 0L && value2 != 0L)
							{
								num3 = value2 | (value2 << 8) | (value2 << 16) | (value2 << 24) | (value2 << 32) | (value2 << 40);
							}
						}
						chunk.SetBlockRaw(i, k, j, blockValue);
						chunk.SetDensity(i, k, j, Densities[num2]);
						chunk.SetTextureFull(i, k, j, num3);
						num2++;
					}
					num++;
				}
			}
		}
		SetXNeighbour(15, (Chunk)cacheNeighbourChunks[-1, 0], GetNeighbourData(-1, 0));
		SetXNeighbour(0, (Chunk)cacheNeighbourChunks[1, 0], GetNeighbourData(1, 0));
		SetZNeighbour(15, (Chunk)cacheNeighbourChunks[0, -1], GetNeighbourData(0, -1));
		SetZNeighbour(0, (Chunk)cacheNeighbourChunks[0, 1], GetNeighbourData(0, 1));
		SetNeighbourCorner(15, 15, (Chunk)cacheNeighbourChunks[-1, -1], GetNeighbourData(-1, -1));
		SetNeighbourCorner(0, 15, (Chunk)cacheNeighbourChunks[1, -1], GetNeighbourData(1, -1));
		SetNeighbourCorner(15, 0, (Chunk)cacheNeighbourChunks[-1, 1], GetNeighbourData(-1, 1));
		SetNeighbourCorner(0, 0, (Chunk)cacheNeighbourChunks[1, 1], GetNeighbourData(1, 1));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetNeighbourCorner(int x, int z, Chunk chunk, ChunkNeighbourData data)
	{
		BlockValue blockValue = new BlockValue(0u);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 256; i++)
		{
			blockValue.rawData = data.BlockRaw[num];
			num++;
			chunk.SetBlockRaw(x, i, z, blockValue);
			if (blockValue.rawData != 0)
			{
				chunk.SetDensity(x, i, z, data.Densities[num2]);
				chunk.SetTextureFull(x, i, z, data.Densities[num2]);
				num2++;
			}
			else
			{
				chunk.SetDensity(x, i, z, MarchingCubes.DensityAir);
				chunk.SetTextureFull(x, i, z, 0L);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetXNeighbour(int x, Chunk chunk, ChunkNeighbourData data)
	{
		BlockValue blockValue = new BlockValue(0u);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				blockValue.rawData = data.BlockRaw[num];
				num++;
				chunk.SetBlockRaw(x, i, j, blockValue);
				if (blockValue.rawData != 0)
				{
					chunk.SetDensity(x, i, j, data.Densities[num2]);
					chunk.SetTextureFull(x, i, j, data.Densities[num2]);
					num2++;
				}
				else
				{
					chunk.SetDensity(x, i, j, MarchingCubes.DensityAir);
					chunk.SetTextureFull(x, i, j, 0L);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetZNeighbour(int z, Chunk chunk, ChunkNeighbourData data)
	{
		BlockValue blockValue = new BlockValue(0u);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				blockValue.rawData = data.BlockRaw[num];
				num++;
				chunk.SetBlockRaw(j, i, z, blockValue);
				if (blockValue.rawData != 0)
				{
					chunk.SetDensity(j, i, z, data.Densities[num2]);
					chunk.SetTextureFull(j, i, z, data.Densities[num2]);
					num2++;
				}
				else
				{
					chunk.SetDensity(j, i, z, MarchingCubes.DensityAir);
					chunk.SetTextureFull(j, i, z, 0L);
				}
			}
		}
	}

	public static DynamicMeshChunkData GetFromCache(string debug)
	{
		if (!Cache.TryDequeue(out var result))
		{
			result = Creates();
		}
		ActiveDataItems++;
		return result;
	}

	public static void AddToCache(DynamicMeshChunkData data, string debug)
	{
		data.Reset();
		Cache.Enqueue(data);
		ActiveDataItems--;
	}

	public static DynamicMeshChunkData Creates()
	{
		DynamicMeshChunkData dynamicMeshChunkData = new DynamicMeshChunkData
		{
			BlockRaw = new List<uint>(),
			Densities = new List<sbyte>(),
			Textures = new List<long>(),
			TopSoil = new List<byte>(32),
			Height = new List<byte>(256),
			TerrainHeight = new List<byte>(256)
		};
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				if (i != 0 || j != 0)
				{
					dynamicMeshChunkData.SetNeighbourData(i, j, ChunkNeighbourData.Create());
				}
			}
		}
		return dynamicMeshChunkData;
	}
}

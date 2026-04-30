using System;
using UnityEngine;

public class MeshGeneratorMC2 : MeshGenerator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCacheNeighborChunks nChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[] deck1 = new ushort[1024];

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[] deck2 = new ushort[1024];

	[PublicizedFrom(EAccessModifier.Private)]
	public Transvoxel.BuildVertex[] vertexStorage = new Transvoxel.BuildVertex[8192];

	[PublicizedFrom(EAccessModifier.Private)]
	public Transvoxel.BuildTriangle[] triangleStorage = new Transvoxel.BuildTriangle[8192];

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[] density = new sbyte[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] globalVertexIndex = new int[12];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] allHeights = new int[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int chunksCacheDim = 19;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunk[] chunksCache = new IChunk[361];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 cVectorAdd = new Vector3(0.5f, 0.5f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue[] blockValues = new BlockValue[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] bTopSoil = new bool[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] terrainHeightsCache = new int[1600];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] topSoilCache = new bool[1600];

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector2i[] adjLightPos = new Vector2i[4]
	{
		Vector2i.zero,
		new Vector2i(1, 0),
		new Vector2i(0, 1),
		new Vector2i(1, 1)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public const int voxelBoundsMin = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int voxelBoundsMax = 17;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startY;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[,] voxelDelta = new byte[6, 4]
	{
		{ 0, 1, 0, 1 },
		{ 0, 1, 0, 1 },
		{ 1, 0, 0, 1 },
		{ 1, 0, 0, 1 },
		{ 1, 0, 1, 0 },
		{ 1, 0, 1, 0 }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[,] voxelStart = new byte[6, 3]
	{
		{ 32, 0, 0 },
		{ 0, 0, 0 },
		{ 0, 32, 0 },
		{ 0, 0, 0 },
		{ 0, 0, 32 },
		{ 0, 0, 0 }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] faceFlip = new byte[6] { 0, 128, 128, 0, 0, 128 };

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i[] mipmapCoord = new Vector3i[13];

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[] mipmapDensity = new sbyte[13];

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[] mipmapGlobalVertexIndex = new ushort[12];

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[][,] rowStorage = new ushort[2][,]
	{
		new ushort[10, 16],
		new ushort[10, 16]
	};

	public MeshGeneratorMC2(INeighborBlockCache _nBlocks, ChunkCacheNeighborChunks _nChunks)
		: base(_nBlocks)
	{
		nChunks = _nChunks;
	}

	public override bool IsLayerEmpty(int _startLayerIdx, int _endLayerIdx)
	{
		if (!base.IsLayerEmpty(_startLayerIdx, _endLayerIdx))
		{
			return false;
		}
		return mc2LayerIsEmpty(_startLayerIdx, _endLayerIdx);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool mc2LayerIsEmpty(int _startLayerIdx, int _endLayerIdx)
	{
		Vector3i vector3i = new Vector3i(0, Utils.FastMax(1, _startLayerIdx * 16), 0);
		Vector3i vector3i2 = new Vector3i(15, (_endLayerIdx + 1) * 16 - 1, 15);
		int y = vector3i.y;
		IChunk chunk = nChunks[0, 0];
		if (chunk.HasSameDensityValue(y))
		{
			int sameDensityValue = chunk.GetSameDensityValue(y);
			int num = Utils.FastMin(vector3i2.y + 1, 255);
			for (int i = y - 1; i <= num; i++)
			{
				if (((chunk = nChunks[0, 0]) != null && (!chunk.HasSameDensityValue(i) || chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = nChunks[1, 0]) != null && (!chunk.HasSameDensityValue(i) || chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = nChunks[-1, 0]) != null && (!chunk.HasSameDensityValue(i) || chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = nChunks[0, 1]) != null && (!chunk.HasSameDensityValue(i) || chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = nChunks[0, -1]) != null && (!chunk.HasSameDensityValue(i) || chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = nChunks[1, 1]) != null && (!chunk.HasSameDensityValue(i) || chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = nChunks[-1, -1]) != null && (!chunk.HasSameDensityValue(i) || chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = nChunks[1, -1]) != null && (!chunk.HasSameDensityValue(i) || chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = nChunks[-1, 1]) != null && (!chunk.HasSameDensityValue(i) || chunk.GetSameDensityValue(i) != sameDensityValue)))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateMesh(Vector3i _worldPos, Vector3 _drawPosOffset, Vector3i _start, Vector3i _end, VoxelMesh[] _meshes, bool _bCalcAmbientLight, bool _bOnlyDistortVertices)
	{
		base.CreateMesh(_worldPos, _drawPosOffset, _start, _end, _meshes, _bCalcAmbientLight, _bOnlyDistortVertices);
		if (mc2LayerIsEmpty(_start.y / 16, _end.y / 16))
		{
			return;
		}
		int y = _start.y;
		IChunk chunk = nChunks[0, 0];
		for (int i = _start.x - 1; i <= _end.x + 2; i++)
		{
			for (int j = _start.z - 1; j <= _end.z + 2; j++)
			{
				if (i >= 0 && i < 16)
				{
					if (j >= 0 && j < 16)
					{
						chunksCache[i + 1 + (j + 1) * 19] = chunk;
					}
					else if (j >= 0)
					{
						chunksCache[i + 1 + (j + 1) * 19] = nChunks[0, 1];
					}
					else
					{
						chunksCache[i + 1 + (j + 1) * 19] = nChunks[0, -1];
					}
				}
				else if (i >= 0)
				{
					if (j >= 0 && j < 16)
					{
						chunksCache[i + 1 + (j + 1) * 19] = nChunks[1, 0];
					}
					else if (j >= 0)
					{
						chunksCache[i + 1 + (j + 1) * 19] = nChunks[1, 1];
					}
					else
					{
						chunksCache[i + 1 + (j + 1) * 19] = nChunks[1, -1];
					}
				}
				else if (j >= 0 && j < 16)
				{
					chunksCache[i + 1 + (j + 1) * 19] = nChunks[-1, 0];
				}
				else if (j >= 0)
				{
					chunksCache[i + 1 + (j + 1) * 19] = nChunks[-1, 1];
				}
				else
				{
					chunksCache[i + 1 + (j + 1) * 19] = nChunks[-1, -1];
				}
			}
		}
		int num = 0;
		for (int k = _start.x; k <= _end.x; k++)
		{
			for (int l = _start.z; l <= _end.z; l++)
			{
				heights[0] = chunksCache[k + 1 + (l + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k, l));
				heights[1] = chunksCache[k + 1 + 1 + (l + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k + 1, l));
				heights[2] = chunksCache[k + 1 + (l + 1 - 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k, l - 1));
				heights[3] = chunksCache[k + 1 - 1 + (l + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k - 1, l));
				heights[4] = chunksCache[k + 1 + (l + 1 + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k, l + 1));
				heights[5] = chunksCache[k + 1 + 1 + (l + 1 + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k + 1, l + 1));
				heights[6] = chunksCache[k + 1 + 1 + (l + 1 - 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k + 1, l - 1));
				heights[7] = chunksCache[k + 1 - 1 + (l + 1 + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k - 1, l + 1));
				heights[8] = chunksCache[k + 1 - 1 + (l + 1 - 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k - 1, l - 1));
				int num2 = heights[0];
				for (int m = 1; m < heights.Length; m++)
				{
					if (num2 < heights[m])
					{
						num2 = heights[m];
					}
				}
				num2 = Utils.FastMin(_end.y, num2);
				num2++;
				num2 = Utils.FastMin(255, num2);
				allHeights[k + l * 16] = num2;
				num = Utils.FastMax(num, num2);
			}
		}
		if (y < num)
		{
			int num3 = y + 15;
			if (num3 >= 255)
			{
				num3 = 254;
			}
			build(cVectorAdd + _drawPosOffset, new Vector3i(_start.x, y, _start.z), new Vector3i(_end.x, num3, _end.z), _meshes[5], _worldPos);
		}
		Array.Clear(chunksCache, 0, chunksCache.Length);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte getDensity(int _x, int _y, int _z, int _idxInBlockValues = -1)
	{
		if (_y < 0)
		{
			return MarchingCubes.DensityTerrain;
		}
		if (_y >= 256)
		{
			return MarchingCubes.DensityAir;
		}
		sbyte b = MarchingCubes.DensityAir;
		Chunk chunk = (Chunk)chunksCache[_x + 1 + (_z + 1) * 19];
		if (chunk != null)
		{
			b = chunk.GetDensity(_x & 0xF, _y, _z & 0xF);
			if (b == 0)
			{
				b = 1;
			}
		}
		return b;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue getBlockValue(int _x, int _y, int _z)
	{
		return ((Chunk)chunksCache[_x + 1 + (_z + 1) * 19]).GetBlockNoDamage(_x & 0xF, _y, _z & 0xF);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTopSoil(int _x, int _z)
	{
		return chunksCache[_x + 1 + (_z + 1) * 19]?.IsTopSoil(_x & 0xF, _z & 0xF) ?? false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetTerrainHeight(int _x, int _z)
	{
		return chunksCache[_x + 1 + (_z + 1) * 19]?.GetTerrainHeight(_x & 0xF, _z & 0xF) ?? 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool calcTopSoil(int _x, int _y, int _z)
	{
		_x++;
		_z++;
		int num = terrainHeightsCache[_x + _z * sizeX];
		if (_y >= num && topSoilCache[_x + _z * sizeX])
		{
			return true;
		}
		num = terrainHeightsCache[_x + 1 + _z * sizeX];
		if (_y > num && topSoilCache[_x + 1 + _z * sizeX])
		{
			return true;
		}
		num = terrainHeightsCache[_x + (_z + 1) * sizeX];
		if (_y > num && topSoilCache[_x + (_z + 1) * sizeX])
		{
			return true;
		}
		num = terrainHeightsCache[_x + (_z - 1) * sizeX];
		if (_y > num && topSoilCache[_x + (_z - 1) * sizeX])
		{
			return true;
		}
		num = terrainHeightsCache[_x - 1 + _z * sizeX];
		if (_y > num && topSoilCache[_x - 1 + _z * sizeX])
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetTextureFor(int _x, int _y, int _z, int idx)
	{
		BlockValue blockValue = blockValues[idx];
		Block block = blockValue.Block;
		if (!block.shape.IsTerrain())
		{
			if (_y > 0)
			{
				_y--;
				blockValue = chunksCache[_x + 1 + (_z + 1) * 19].GetBlock(_x & 0xF, _y, _z & 0xF);
			}
			if (!blockValue.Block.shape.IsTerrain())
			{
				blockValue = new BlockValue(1u);
			}
			block = blockValue.Block;
		}
		int sideTextureId = block.GetSideTextureId(blockValue, BlockFace.Top, 0);
		int sideTextureId2 = block.GetSideTextureId(blockValue, BlockFace.South, 0);
		return VoxelMeshTerrain.EncodeTexIds(sideTextureId, sideTextureId2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calcLights(int _x, int _y, int _z, out byte _sun, out byte _block)
	{
		int num = _x + adjLightPos[0].x;
		int num2 = _z + adjLightPos[0].y;
		int num3 = 1 + num + (1 + num2) * 19;
		int num4 = _x + adjLightPos[1].x;
		int num5 = _z + adjLightPos[1].y;
		int num6 = 1 + num4 + (1 + num5) * 19;
		int num7 = _x + adjLightPos[2].x;
		int num8 = _z + adjLightPos[2].y;
		int num9 = 1 + num7 + (1 + num8) * 19;
		int num10 = _x + adjLightPos[3].x;
		int num11 = _z + adjLightPos[3].y;
		int num12 = 1 + num10 + (1 + num11) * 19;
		byte v = (byte)Utils.FastMax(chunksCache[num3].GetLight(num, _y, num2, Chunk.LIGHT_TYPE.SUN), chunksCache[num6].GetLight(num4, _y, num5, Chunk.LIGHT_TYPE.SUN), chunksCache[num9].GetLight(num7, _y, num8, Chunk.LIGHT_TYPE.SUN), chunksCache[num12].GetLight(num10, _y, num11, Chunk.LIGHT_TYPE.SUN));
		byte v2 = (byte)((_y < 255) ? ((byte)Utils.FastMax(chunksCache[num3].GetLight(num, _y + 1, num2, Chunk.LIGHT_TYPE.SUN), chunksCache[num6].GetLight(num4, _y + 1, num5, Chunk.LIGHT_TYPE.SUN), chunksCache[num9].GetLight(num7, _y + 1, num8, Chunk.LIGHT_TYPE.SUN), chunksCache[num12].GetLight(num10, _y + 1, num11, Chunk.LIGHT_TYPE.SUN))) : 15);
		byte v3 = (byte)Utils.FastMax(chunksCache[num3].GetLight(num, _y, num2, Chunk.LIGHT_TYPE.BLOCK), chunksCache[num6].GetLight(num4, _y, num5, Chunk.LIGHT_TYPE.BLOCK), chunksCache[num9].GetLight(num7, _y, num8, Chunk.LIGHT_TYPE.BLOCK), chunksCache[num12].GetLight(num10, _y, num11, Chunk.LIGHT_TYPE.BLOCK));
		byte v4 = (byte)((_y < 255) ? ((byte)Utils.FastMax(chunksCache[num3].GetLight(num, _y + 1, num2, Chunk.LIGHT_TYPE.BLOCK), chunksCache[num6].GetLight(num4, _y + 1, num5, Chunk.LIGHT_TYPE.BLOCK), chunksCache[num9].GetLight(num7, _y + 1, num8, Chunk.LIGHT_TYPE.BLOCK), chunksCache[num12].GetLight(num10, _y + 1, num11, Chunk.LIGHT_TYPE.BLOCK))) : 15);
		_sun = Utils.FastMax(v, v2);
		_block = Utils.FastMax(v3, v4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetDeckIndex(int x, int y, int z)
	{
		return x * 16 * 4 + y * 4 + z;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void build(Vector3 _drawPosOffset, Vector3i _start, Vector3i _end, VoxelMesh _mesh, Vector3i _worldPos)
	{
		int x = _start.x;
		int z = _start.z;
		sizeX = _end.x - _start.x + 1 + 3;
		sizeZ = _end.z - _start.z + 1 + 3;
		startY = _start.y;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeZ; j++)
			{
				terrainHeightsCache[i + j * sizeX] = GetTerrainHeight(i - 1 + x, j - 1 + z);
			}
		}
		for (int k = 0; k < sizeX; k++)
		{
			for (int l = 0; l < sizeZ; l++)
			{
				topSoilCache[k + l * sizeX] = isTopSoil(k - 1 + x, l - 1 + z);
			}
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		ushort[] array = deck1;
		ushort[] array2 = deck2;
		for (int m = _start.z; m <= _end.z; m++)
		{
			for (int n = _start.y; n <= _end.y; n++)
			{
				int num4 = n - _start.y;
				for (int num5 = _start.x; num5 <= _end.x; num5++)
				{
					byte b = 0;
					blockValues[0] = getBlockValue(num5, n, m);
					blockValues[1] = getBlockValue(num5 + 1, n, m);
					blockValues[2] = getBlockValue(num5, n + 1, m);
					blockValues[3] = getBlockValue(num5 + 1, n + 1, m);
					blockValues[4] = getBlockValue(num5, n, m + 1);
					blockValues[5] = getBlockValue(num5 + 1, n, m + 1);
					blockValues[6] = getBlockValue(num5, n + 1, m + 1);
					blockValues[7] = getBlockValue(num5 + 1, n + 1, m + 1);
					density[0] = getDensity(num5, n, m, 0);
					density[1] = getDensity(num5 + 1, n, m, 1);
					density[2] = getDensity(num5, n + 1, m, 2);
					density[3] = getDensity(num5 + 1, n + 1, m, 3);
					density[4] = getDensity(num5, n, m + 1, 4);
					density[5] = getDensity(num5 + 1, n, m + 1, 5);
					density[6] = getDensity(num5, n + 1, m + 1, 6);
					density[7] = getDensity(num5 + 1, n + 1, m + 1, 7);
					bTopSoil[0] = calcTopSoil(num5, n, m);
					bTopSoil[1] = calcTopSoil(num5 + 1, n, m);
					bTopSoil[2] = calcTopSoil(num5, n + 1, m);
					bTopSoil[3] = calcTopSoil(num5 + 1, n + 1, m);
					bTopSoil[4] = calcTopSoil(num5, n, m + 1);
					bTopSoil[5] = calcTopSoil(num5 + 1, n, m + 1);
					bTopSoil[6] = calcTopSoil(num5, n + 1, m + 1);
					bTopSoil[7] = calcTopSoil(num5 + 1, n + 1, m + 1);
					sbyte b2 = density[7];
					int num6 = ((density[0] >> 7) & 1) | ((density[1] >> 6) & 2) | ((density[2] >> 5) & 4) | ((density[3] >> 4) & 8) | ((density[4] >> 3) & 0x10) | ((density[5] >> 2) & 0x20) | ((density[6] >> 1) & 0x40) | (b2 & 0x80);
					if (b2 == 0)
					{
						int num7 = num++;
						array[GetDeckIndex(num4, num5, 0)] = (ushort)num7;
						vertexStorage[num7].position0 = new Vector3i(num5 + 1 << 8, num4 + 1 << 8, m + 1 << 8);
						vertexStorage[num7].normal = CalculateNormal(new Vector3i(num5 + 1, num4 + 1, m + 1));
						vertexStorage[num7].material = b;
						vertexStorage[num7].texture = GetTextureFor(num5, n, m, 7);
						vertexStorage[num7].bTopSoil = bTopSoil[7];
					}
					if ((num6 ^ ((b2 >> 7) & 0xFF)) != 0)
					{
						byte b3 = Transvoxel.regularCellClass[num6];
						Transvoxel.RegularCellData regularCellData = Transvoxel.regularCellData[0, b3];
						int triangleCount = regularCellData.GetTriangleCount();
						if (num2 + triangleCount > 8192)
						{
							goto end_IL_0a1e;
						}
						int vertexCount = regularCellData.GetVertexCount();
						Transvoxel.RegularVertexData.Row row = Transvoxel.regularVertexData[num6];
						for (int num8 = 0; num8 < vertexCount; num8++)
						{
							int num9 = 0;
							ushort num10 = row.data[num8];
							int num11 = (num10 >> 4) & 0xF;
							int num12 = num10 & 0xF;
							int num13 = density[num11];
							int num14 = density[num12];
							int num15 = ((num14 << 8) + 128) / (num14 - num13);
							Vector3i vector3i = new Vector3i(num5 + (num11 & 1), num4 + ((num11 >> 1) & 1), m + ((num11 >> 2) & 1));
							Vector3i vector3i2 = new Vector3i(num5 + (num12 & 1), num4 + ((num12 >> 1) & 1), m + ((num12 >> 2) & 1));
							if ((num15 & 0xFF) != 0)
							{
								int z2 = (num10 >> 8) & 0xF;
								int num16 = (num10 >> 12) & 0xF;
								if ((num16 & num3) != num16)
								{
									num9 = num++;
									int num17 = 256 - num15;
									vertexStorage[num9].position0 = vector3i * num15 + vector3i2 * num17;
									vertexStorage[num9].normal = CalculateNormal(vector3i, vector3i2, num15, num17, num13 < num14);
									vertexStorage[num9].material = b;
									int num18 = ((num13 < num14) ? num11 : num12);
									vertexStorage[num9].texture = GetTextureFor(num5, n, m, num18);
									vertexStorage[num9].bTopSoil = bTopSoil[num18];
									if (num12 == 7)
									{
										int deckIndex = GetDeckIndex(num4, num5, z2);
										array[deckIndex] = (ushort)num9;
									}
									globalVertexIndex[num8] = num9;
									continue;
								}
								if ((num16 & 4) != 0)
								{
									int deckIndex2 = GetDeckIndex(num4 - ((num16 >> 1) & 1), num5 - (num16 & 1), z2);
									num9 = array2[deckIndex2];
								}
								else
								{
									int deckIndex3 = GetDeckIndex(num4 - (num16 >> 1), num5 - (num16 & 1), z2);
									num9 = array[deckIndex3];
								}
							}
							else if (num15 == 0)
							{
								if (num12 == 7)
								{
									num9 = array[GetDeckIndex(num4, num5, 0)];
								}
								else
								{
									int num19 = num12 ^ 7;
									if ((num19 & num3) != num19)
									{
										num9 = num++;
										vertexStorage[num9].position0 = new Vector3i(vector3i2.x << 8, vector3i2.y << 8, vector3i2.z << 8);
										vertexStorage[num9].normal = CalculateNormal(vector3i2);
										vertexStorage[num9].material = b;
										vertexStorage[num9].texture = GetTextureFor(num5, n, m, num12);
										vertexStorage[num9].bTopSoil = bTopSoil[num12];
										globalVertexIndex[num8] = num9;
										continue;
									}
									num9 = (((num19 & 4) == 0) ? array[GetDeckIndex(num4 - (num19 >> 1), num5 - (num19 & 1), 0)] : array2[GetDeckIndex(num4 - ((num19 >> 1) & 1), num5 - (num19 & 1), 0)]);
								}
							}
							else
							{
								int num20 = num11 ^ 7;
								if ((num20 & num3) != num20)
								{
									num9 = num++;
									vertexStorage[num9].position0 = new Vector3i(vector3i.x << 8, vector3i.y << 8, vector3i.z << 8);
									vertexStorage[num9].normal = CalculateNormal(vector3i);
									vertexStorage[num9].material = b;
									vertexStorage[num9].texture = GetTextureFor(num5, n, m, num11);
									vertexStorage[num9].bTopSoil = bTopSoil[num11];
									globalVertexIndex[num8] = num9;
									continue;
								}
								num9 = (((num20 & 4) == 0) ? array[GetDeckIndex(num4 - (num20 >> 1), num5 - (num20 & 1), 0)] : array2[GetDeckIndex(num4 - ((num20 >> 1) & 1), num5 - (num20 & 1), 0)]);
							}
							globalVertexIndex[num8] = num9;
						}
						if (b != byte.MaxValue)
						{
							byte[] vertexIndex = regularCellData.vertexIndex;
							int num21 = 0;
							for (int num22 = 0; num22 < triangleCount; num22++)
							{
								int num23 = num2 + num22;
								triangleStorage[num23].index0 = globalVertexIndex[vertexIndex[num21]];
								triangleStorage[num23].index1 = globalVertexIndex[vertexIndex[1 + num21]];
								triangleStorage[num23].index2 = globalVertexIndex[vertexIndex[2 + num21]];
								num21 += 3;
							}
							num2 += triangleCount;
						}
					}
					num3 |= 1;
				}
				num3 = (num3 | 2) & 6;
			}
			num3 = 4;
			ushort[] array3 = array;
			array = array2;
			array2 = array3;
			continue;
			end_IL_0a1e:
			break;
		}
		int num24 = 0;
		for (int num25 = 0; num25 < num; num25++)
		{
			if (vertexStorage[num25].material != byte.MaxValue)
			{
				Vector3i position = vertexStorage[num25].position0;
				if ((position.x | position.y | position.z) >= 0 && Utils.FastMax(Utils.FastMax(position.x, position.y), position.z) <= 4096)
				{
					int num26 = 1;
					num26 |= (position.x >> 11) & 2;
					num26 |= (position.y >> 10) & 4;
					num26 |= (position.z >> 9) & 8;
					vertexStorage[num25].statusFlags = (byte)num26;
					vertexStorage[num25].remapIndex = num24;
					num24++;
					continue;
				}
			}
			vertexStorage[num25].statusFlags = 0;
		}
		int num27 = 0;
		for (int num28 = 0; num28 < num2; num28++)
		{
			int index = triangleStorage[num28].index0;
			int index2 = triangleStorage[num28].index1;
			int index3 = triangleStorage[num28].index2;
			Vector3i vector3i3 = Vector3i.Cross(vertexStorage[index2].position0 - vertexStorage[index].position0, vertexStorage[index3].position0 - vertexStorage[index].position0);
			bool flag = false;
			if ((vector3i3.x | vector3i3.y | vector3i3.z) != 0)
			{
				int num29 = ((vector3i3.x >> 31) & 2) | ((vector3i3.y >> 31) & 4) | ((vector3i3.z >> 31) & 8) | 1;
				flag = (vertexStorage[index].statusFlags & vertexStorage[index2].statusFlags & vertexStorage[index3].statusFlags & num29) == 1;
			}
			triangleStorage[num28].inclusionFlag = flag;
			if (flag)
			{
				num27++;
				int num30 = _mesh.FindOrCreateSubMesh(vertexStorage[index].texture, vertexStorage[index2].texture, vertexStorage[index3].texture);
				triangleStorage[num28].submeshIdx = num30;
				_mesh.GetColorForTextureId(num30, ref vertexStorage[index]);
				_mesh.GetColorForTextureId(num30, ref vertexStorage[index2]);
				_mesh.GetColorForTextureId(num30, ref vertexStorage[index3]);
			}
		}
		if (num27 != num2)
		{
			Log.Warning("MG build tris {0} != {1}", num27, num2);
		}
		int count = _mesh.m_Vertices.Count;
		Vector3 vector = new Vector3(0f, _start.y, 0f) + _drawPosOffset;
		for (int num31 = 0; num31 < num; num31++)
		{
			if ((vertexStorage[num31].statusFlags & 1) != 0)
			{
				_mesh.m_Vertices.Add(vertexStorage[num31].position0.ToVector3() / 256f + vector);
				_mesh.m_Normals.Add(vertexStorage[num31].normal);
				_mesh.m_ColorVertices.Add(vertexStorage[num31].color);
				_mesh.m_Uvs.Add(vertexStorage[num31].uv);
				_mesh.UvsCrack.Add(vertexStorage[num31].uv2);
				_mesh.m_Uvs3.Add(vertexStorage[num31].uv3);
				_mesh.m_Uvs4.Add(vertexStorage[num31].uv4);
			}
		}
		for (int num32 = 0; num32 < num2; num32++)
		{
			if (triangleStorage[num32].inclusionFlag)
			{
				int index4 = triangleStorage[num32].index0;
				int index5 = triangleStorage[num32].index1;
				int index6 = triangleStorage[num32].index2;
				_mesh.AddIndices(count + vertexStorage[index4].remapIndex, count + vertexStorage[index5].remapIndex, count + vertexStorage[index6].remapIndex, triangleStorage[num32].submeshIdx);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculateSecondaryPosition(int level, int buildVertex)
	{
		int num = 256 << level;
		int num2 = 15 << 8 + level;
		int num3 = num >> 2;
		Vector3 normal = vertexStorage[buildVertex].normal;
		int x = vertexStorage[buildVertex].position0.x;
		Vector3i zero = default(Vector3i);
		if (x < num)
		{
			int num4 = num3 * (num - x) >> 8 + level;
			Vector3 vector = new Vector3(1f - normal.x * normal.x, (0f - normal.x) * normal.y, (0f - normal.x) * normal.z);
			zero.x = (int)(vector.x * 256f) * num4;
			zero.y = (int)(vector.y * 256f) * num4;
			zero.z = (int)(vector.z * 256f) * num4;
		}
		else if (x > num2)
		{
			int num5 = num3 * (num2 + num - 256 - x) >> 8;
			Vector3 vector2 = new Vector3(1f - normal.x * normal.x, (0f - normal.x) * normal.y, (0f - normal.x) * normal.z);
			zero.x = (int)(vector2.x * 256f) * num5;
			zero.y = (int)(vector2.y * 256f) * num5;
			zero.z = (int)(vector2.z * 256f) * num5;
		}
		else
		{
			zero = Vector3i.zero;
		}
		int y = vertexStorage[buildVertex].position0.y;
		if (y < num)
		{
			int num6 = num3 * (num - y) >> 8 + level;
			Vector3 vector3 = new Vector3((0f - normal.x) * normal.y, 1f - normal.y * normal.y, (0f - normal.y) * normal.z);
			zero.x += (int)(vector3.x * 256f) * num6;
			zero.y += (int)(vector3.y * 256f) * num6;
			zero.z += (int)(vector3.z * 256f) * num6;
		}
		else if (y > num2)
		{
			int num7 = num3 * (num2 + num - 256 - y) >> 8;
			Vector3 vector4 = new Vector3((0f - normal.x) * normal.y, 1f - normal.y * normal.y, (0f - normal.y) * normal.z);
			zero.x += (int)(vector4.x * 256f) * num7;
			zero.y += (int)(vector4.y * 256f) * num7;
			zero.z += (int)(vector4.z * 256f) * num7;
		}
		int z = vertexStorage[buildVertex].position0.z;
		if (z < num)
		{
			int num8 = num3 * (num - z) >> 8 + level;
			Vector3 vector5 = new Vector3((0f - normal.x) * normal.z, (0f - normal.y) * normal.z, 1f - normal.z * normal.z);
			zero.x += (int)(vector5.x * 256f) * num8;
			zero.y += (int)(vector5.y * 256f) * num8;
			zero.z += (int)(vector5.z * 256f) * num8;
		}
		else if (z > num2)
		{
			int num9 = num3 * (num2 + num - 256 - z) >> 8;
			Vector3 vector6 = new Vector3((0f - normal.x) * normal.z, (0f - normal.y) * normal.z, 1f - normal.z * normal.z);
			zero.x += (int)(vector6.x * 256f) * num9;
			zero.y += (int)(vector6.y * 256f) * num9;
			zero.z += (int)(vector6.z * 256f) * num9;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalculateNormal(Vector3i coord)
	{
		int x = coord.x;
		int num = coord.y + startY;
		int z = coord.z;
		int x2 = x - 1;
		int x3 = x + 1;
		int y = num - 1;
		int y2 = num + 1;
		int z2 = z - 1;
		int z3 = z + 1;
		int num2 = getDensity(x2, num, z);
		int num3 = getDensity(x3, num, z);
		int num4 = getDensity(x, y, z);
		int num5 = getDensity(x, y2, z);
		int num6 = getDensity(x, num, z2);
		int num7 = getDensity(x, num, z3);
		Vector3 vector = default(Vector3);
		vector.x = num3 - num2;
		vector.y = num5 - num4;
		vector.z = num7 - num6;
		float magnitude = vector.magnitude;
		if (magnitude > 1E-05f)
		{
			return vector * (1f / magnitude);
		}
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalculateNormalUnscaled(Vector3i coord)
	{
		int x = coord.x;
		int num = coord.y + startY;
		int z = coord.z;
		int x2 = x - 1;
		int x3 = x + 1;
		int y = num - 1;
		int y2 = num + 1;
		int z2 = z - 1;
		int z3 = z + 1;
		int num2 = getDensity(x2, num, z);
		int num3 = getDensity(x3, num, z);
		int num4 = getDensity(x, y, z);
		int num5 = getDensity(x, y2, z);
		int num6 = getDensity(x, num, z2);
		int num7 = getDensity(x, num, z3);
		Vector3 result = default(Vector3);
		result.x = num3 - num2;
		result.y = num5 - num4;
		result.z = num7 - num6;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalculateNormal(Vector3i coord0, Vector3i coord1, int t, int u, bool is0Main)
	{
		Vector3 vector = CalculateNormalUnscaled(coord0);
		Vector3 vector2 = CalculateNormalUnscaled(coord1);
		Vector3 vector3 = vector * t + vector2 * u;
		Vector3 vector4 = (is0Main ? (coord1 - coord0) : (coord0 - coord1)).ToVector3();
		float magnitude = vector3.magnitude;
		if (magnitude > 1E-05f)
		{
			vector3 *= 1f / magnitude;
			float num = Vector3.Dot(vector3, vector4);
			if (num < 0.2f)
			{
				if (num < -0.5f)
				{
					vector3 = vector4;
				}
				else
				{
					float t2 = 0.8f + Utils.FastAbs(t - u) * 0.00078124995f;
					vector3 = Vector3.LerpUnclamped(vector4, vector3, t2);
					magnitude = vector3.magnitude;
					vector3 *= 1f / magnitude;
				}
			}
		}
		else
		{
			vector3 = vector4;
		}
		return vector3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FindSurfaceCrossingEdge(int level, Vector3i coord0, Vector3i coord1, ref int d0, ref int d1)
	{
		Vector3i vector3i = coord0 + coord1;
		vector3i.x >>= 1;
		vector3i.y >>= 1;
		vector3i.z >>= 1;
		int num = getDensity(vector3i.x, vector3i.y, vector3i.z);
		if (((d0 ^ num) & 0x80) != 0)
		{
			coord1 = vector3i;
			d1 = num;
		}
		else
		{
			coord0 = vector3i;
			d0 = num;
		}
		if (level > 1)
		{
			FindSurfaceCrossingEdge(level - 1, coord0, coord1, ref d0, ref d1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ChooseTriangulation(int cellClass, int x, int y, int z, int dv, ushort[] _globalVertexIndex)
	{
		dv >>= 1;
		x += dv;
		y += dv;
		z += dv;
		float num = (float)getDensity(x, y, z) * (1f / 127f);
		Vector3i vector3i = new Vector3i(x << 8, y << 8, z << 8);
		Transvoxel.InternalEdgeData internalEdgeData = Transvoxel.regularInternalEdgeData[0, cellClass];
		int edgeCount = internalEdgeData.edgeCount;
		float num2 = 2f;
		for (int i = 0; i < edgeCount; i++)
		{
			Vector3i vector3i2 = vertexStorage[_globalVertexIndex[internalEdgeData.vertexIndex[i, 0]]].position0 - vector3i;
			Vector3i vector3i3 = vertexStorage[_globalVertexIndex[internalEdgeData.vertexIndex[i, 1]]].position0 - vector3i;
			Vector3i vector3i4 = vertexStorage[_globalVertexIndex[internalEdgeData.vertexIndex[i, 2]]].position0 - vector3i;
			Vector3i vector3i5 = vertexStorage[_globalVertexIndex[internalEdgeData.vertexIndex[i, 3]]].position0 - vector3i;
			Vector3 vector = new Vector3((float)vector3i2.x * 0.00390625f, (float)vector3i2.y * 0.00390625f, (float)vector3i2.z * 0.00390625f);
			Vector3 vector2 = new Vector3((float)vector3i3.x * 0.00390625f, (float)vector3i3.y * 0.00390625f, (float)vector3i3.z * 0.00390625f);
			Vector3 vector3 = new Vector3((float)vector3i4.x * 0.00390625f, (float)vector3i4.y * 0.00390625f, (float)vector3i4.z * 0.00390625f);
			Vector3 vector4 = new Vector3((float)vector3i5.x * 0.00390625f, (float)vector3i5.y * 0.00390625f, (float)vector3i5.z * 0.00390625f);
			Vector3 vector5 = vector2 - vector;
			float num3 = Vector3.Dot(vector, vector5);
			float num4 = Mathf.Sqrt(Vector3.Dot(vector, vector) - num3 * num3 / Vector3.Dot(vector5, vector5));
			if (Vector3.Dot(Vector3.Cross(vector5, vector4 - vector3), vector) > 0f)
			{
				num4 = 0f - num4;
			}
			num4 = Mathf.Abs(num4 - num);
			if (num4 < num2)
			{
				num2 = num4;
			}
		}
		internalEdgeData = Transvoxel.regularInternalEdgeData[1, cellClass];
		edgeCount = internalEdgeData.edgeCount;
		float num5 = 2f;
		for (int j = 0; j < edgeCount; j++)
		{
			Vector3i vector3i6 = vertexStorage[_globalVertexIndex[internalEdgeData.vertexIndex[j, 0]]].position0 - vector3i;
			Vector3i vector3i7 = vertexStorage[_globalVertexIndex[internalEdgeData.vertexIndex[j, 1]]].position0 - vector3i;
			Vector3i vector3i8 = vertexStorage[_globalVertexIndex[internalEdgeData.vertexIndex[j, 2]]].position0 - vector3i;
			Vector3i vector3i9 = vertexStorage[_globalVertexIndex[internalEdgeData.vertexIndex[j, 3]]].position0 - vector3i;
			Vector3 vector6 = new Vector3((float)vector3i6.x * 0.00390625f, (float)vector3i6.y * 0.00390625f, (float)vector3i6.z * 0.00390625f);
			Vector3 vector7 = new Vector3((float)vector3i7.x * 0.00390625f, (float)vector3i7.y * 0.00390625f, (float)vector3i7.z * 0.00390625f);
			Vector3 vector8 = new Vector3((float)vector3i8.x * 0.00390625f, (float)vector3i8.y * 0.00390625f, (float)vector3i8.z * 0.00390625f);
			Vector3 vector9 = new Vector3((float)vector3i9.x * 0.00390625f, (float)vector3i9.y * 0.00390625f, (float)vector3i9.z * 0.00390625f);
			Vector3 vector10 = vector7 - vector6;
			float num6 = Vector3.Dot(vector6, vector10);
			float num7 = Mathf.Sqrt(Vector3.Dot(vector6, vector6) - num6 * num6 / Vector3.Dot(vector10, vector10));
			if (Vector3.Dot(Vector3.Cross(vector10, vector9 - vector8), vector6) > 0f)
			{
				num7 = 0f - num7;
			}
			num7 = Mathf.Abs(num7 - num);
			if (num7 < num5)
			{
				num5 = num7;
			}
		}
		Transvoxel.RegularCellData regularCellData = Transvoxel.regularCellData[0, cellClass];
		edgeCount = regularCellData.GetTriangleCount();
		int num8 = 0;
		for (int k = 0; k < edgeCount; k++)
		{
			Vector3i vector3i10 = vertexStorage[_globalVertexIndex[regularCellData.vertexIndex[num8]]].position0 - vector3i;
			Vector3i vector3i11 = vertexStorage[_globalVertexIndex[regularCellData.vertexIndex[1 + num8]]].position0 - vector3i;
			Vector3i vector3i12 = vertexStorage[_globalVertexIndex[regularCellData.vertexIndex[2 + num8]]].position0 - vector3i;
			Vector3 vector11 = new Vector3((float)vector3i10.x * 0.00390625f, (float)vector3i10.y * 0.00390625f, (float)vector3i10.z * 0.00390625f);
			Vector3 vector12 = new Vector3((float)vector3i11.x * 0.00390625f, (float)vector3i11.y * 0.00390625f, (float)vector3i11.z * 0.00390625f);
			Vector3 vector13 = new Vector3((float)vector3i12.x * 0.00390625f, (float)vector3i12.y * 0.00390625f, (float)vector3i12.z * 0.00390625f);
			Vector3 vector14 = vector12 - vector11;
			Vector3 vector15 = vector13 - vector11;
			Vector3 vector16 = Vector3.Cross(vector14, vector15);
			if (Vector3.Dot(Vector3.Cross(vector16, vector14), vector11) < 0f && Vector3.Dot(Vector3.Cross(vector16, vector13 - vector12), vector12) < 0f && Vector3.Dot(Vector3.Cross(vector15, vector16), vector13) < 0f)
			{
				float num9 = 0f - Vector3.Dot(vector16, vector11);
				num9 = Mathf.Abs(num9 - num);
				if (num9 < num2)
				{
					num2 = num9;
				}
			}
			num8 += 3;
		}
		regularCellData = Transvoxel.regularCellData[1, cellClass];
		num8 = 0;
		edgeCount = regularCellData.GetTriangleCount();
		for (int l = 0; l < edgeCount; l++)
		{
			Vector3i vector3i13 = vertexStorage[_globalVertexIndex[regularCellData.vertexIndex[num8]]].position0 - vector3i;
			Vector3i vector3i14 = vertexStorage[_globalVertexIndex[regularCellData.vertexIndex[1 + num8]]].position0 - vector3i;
			Vector3i vector3i15 = vertexStorage[_globalVertexIndex[regularCellData.vertexIndex[2 + num8]]].position0 - vector3i;
			Vector3 vector17 = new Vector3((float)vector3i13.x * 0.00390625f, (float)vector3i13.y * 0.00390625f, (float)vector3i13.z * 0.00390625f);
			Vector3 vector18 = new Vector3((float)vector3i14.x * 0.00390625f, (float)vector3i14.y * 0.00390625f, (float)vector3i14.z * 0.00390625f);
			Vector3 vector19 = new Vector3((float)vector3i15.x * 0.00390625f, (float)vector3i15.y * 0.00390625f, (float)vector3i15.z * 0.00390625f);
			Vector3 vector20 = vector18 - vector17;
			Vector3 vector21 = vector19 - vector17;
			Vector3 vector22 = Vector3.Cross(vector20, vector21);
			if (Vector3.Dot(Vector3.Cross(vector22, vector20), vector17) < 0f && Vector3.Dot(Vector3.Cross(vector22, vector19 - vector18), vector18) < 0f && Vector3.Dot(Vector3.Cross(vector21, vector22), vector19) < 0f)
			{
				float num10 = 0f - Vector3.Dot(vector22, vector17);
				num10 = Mathf.Abs(num10 - num);
				if (num10 < num5)
				{
					num5 = num10;
				}
			}
			num8 += 3;
		}
		return num5 < num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildMipBorder(int detailLevel, int face, ref int globalVertexCount, ref int globalTriangleCount, VoxelMesh _mesh)
	{
		byte b = 0;
		ushort num = 0;
		int num2 = 1 << detailLevel - 1;
		int num3 = voxelDelta[face, 0] * num2;
		int num4 = voxelDelta[face, 1] * num2;
		int num5 = voxelDelta[face, 2] * num2;
		int num6 = voxelDelta[face, 3] * num2;
		for (int i = 0; i < 16; i++)
		{
			int num7 = voxelStart[face, 0] * num2;
			int num8 = voxelStart[face, 1] * num2 + i * num5 * 2;
			int num9 = voxelStart[face, 2] * num2 + i * num6 * 2;
			ushort[,] array = rowStorage[b];
			ushort[,] array2 = rowStorage[b ^ 1];
			for (int j = 0; j < 16; j++)
			{
				mipmapCoord[0] = new Vector3i(num7, num8, num9);
				mipmapCoord[1] = new Vector3i(num7 + num3, num8 + num4, num9);
				mipmapCoord[2] = new Vector3i(num7 + num3 * 2, num8 + num4 * 2, num9);
				mipmapCoord[3] = new Vector3i(num7, num8 + num5, num9 + num6);
				mipmapCoord[4] = new Vector3i(num7 + num3, num8 + num4 + num5, num9 + num6);
				mipmapCoord[5] = new Vector3i(num7 + num3 * 2, num8 + num4 * 2 + num5, num9 + num6);
				mipmapCoord[6] = new Vector3i(num7, num8 + num5 * 2, num9 + num6 * 2);
				mipmapCoord[7] = new Vector3i(num7 + num3, num8 + num4 + num5 * 2, num9 + num6 * 2);
				mipmapCoord[8] = new Vector3i(num7 + num3 * 2, num8 + num4 * 2 + num5 * 2, num9 + num6 * 2);
				for (int k = 0; k < 9; k++)
				{
					mipmapDensity[k] = getDensity(mipmapCoord[k].x, mipmapCoord[k].y, mipmapCoord[k].z);
				}
				byte b2 = 0;
				mipmapCoord[9] = mipmapCoord[0];
				mipmapCoord[10] = mipmapCoord[2];
				mipmapCoord[11] = mipmapCoord[6];
				mipmapCoord[12] = mipmapCoord[8];
				mipmapDensity[9] = mipmapDensity[0];
				mipmapDensity[10] = mipmapDensity[2];
				mipmapDensity[11] = mipmapDensity[6];
				mipmapDensity[12] = mipmapDensity[8];
				if (mipmapDensity[8] == 0)
				{
					ushort num10 = (array[j, 0] = (ushort)globalVertexCount++);
					vertexStorage[num10].position0 = new Vector3i(mipmapCoord[8].x << 8, mipmapCoord[8].y << 8, mipmapCoord[8].z << 8);
					vertexStorage[num10].normal = CalculateNormal(mipmapCoord[8]);
				}
				if (mipmapDensity[7] == 0)
				{
					ushort num11 = (array[j, 1] = (ushort)globalVertexCount++);
					vertexStorage[num11].position0 = new Vector3i(mipmapCoord[7].x << 8, mipmapCoord[7].y << 8, mipmapCoord[7].z << 8);
					vertexStorage[num11].normal = CalculateNormal(mipmapCoord[7]);
				}
				if (mipmapDensity[5] == 0)
				{
					ushort num12 = (array[j, 2] = (ushort)globalVertexCount++);
					vertexStorage[num12].position0 = new Vector3i(mipmapCoord[5].x << 8, mipmapCoord[5].y << 8, mipmapCoord[5].z << 8);
					vertexStorage[num12].normal = CalculateNormal(mipmapCoord[5]);
				}
				if (mipmapDensity[12] == 0)
				{
					ushort num13 = (array[j, 7] = (ushort)globalVertexCount++);
					vertexStorage[num13].position0 = new Vector3i(mipmapCoord[12].x << 8, mipmapCoord[12].y << 8, mipmapCoord[12].z << 8);
					vertexStorage[num13].normal = CalculateNormal(mipmapCoord[12]);
					CalculateSecondaryPosition(detailLevel, num13);
				}
				int num14 = ((mipmapDensity[0] >> 7) & 1) | ((mipmapDensity[1] >> 6) & 2) | ((mipmapDensity[2] >> 5) & 4) | ((mipmapDensity[5] >> 4) & 8) | ((mipmapDensity[8] >> 3) & 0x10) | ((mipmapDensity[7] >> 2) & 0x20) | ((mipmapDensity[6] >> 1) & 0x40) | (mipmapDensity[3] & 0x80) | ((mipmapDensity[4] << 1) & 0x100);
				if ((num14 ^ ((mipmapDensity[8] >> 7) & 0x1FF)) != 0)
				{
					byte b3 = Transvoxel.transitionCellClass[num14];
					Transvoxel.TransitionCellData transitionCellData = Transvoxel.transitionCellData[b3 & 0x7F];
					int triangleCount = transitionCellData.GetTriangleCount();
					if (globalTriangleCount + triangleCount > 8192)
					{
						return;
					}
					int vertexCount = transitionCellData.GetVertexCount();
					Transvoxel.TransitionVertexData.Row row = Transvoxel.transitionVertexData[num14];
					for (int l = 0; l < vertexCount; l++)
					{
						ushort num15 = row[l];
						ushort num16 = (ushort)((num15 >> 4) & 0xF);
						ushort num17 = (ushort)(num15 & 0xF);
						int d = mipmapDensity[num16];
						int d2 = mipmapDensity[num17];
						int num18 = (d2 << 8) / (d2 - d);
						ushort num21;
						if ((num18 & 0xFF) != 0)
						{
							ushort num19 = (ushort)((num15 >> 8) & 0xF);
							ushort num20 = (ushort)((num15 >> 12) & 0xF);
							if ((num20 & num) != num20)
							{
								Vector3i vector3i = mipmapCoord[num16];
								Vector3i vector3i2 = mipmapCoord[num17];
								if (num19 > 7)
								{
									FindSurfaceCrossingEdge(detailLevel, vector3i, vector3i2, ref d, ref d2);
									num18 = (d2 << 8) / (d2 - d);
								}
								else if (detailLevel > 1)
								{
									FindSurfaceCrossingEdge(detailLevel - 1, vector3i, vector3i2, ref d, ref d2);
									num18 = (d2 << 8) / (d2 - d);
								}
								num21 = (ushort)globalVertexCount++;
								int num22 = 256 - num18;
								vertexStorage[num21].position0 = vector3i * num18 + vector3i2 * num22;
								vertexStorage[num21].normal = CalculateNormal(vector3i, vector3i2, num18, num22, d < d2);
								if (num19 > 7)
								{
									CalculateSecondaryPosition(detailLevel, num21);
								}
								if (num20 == 8)
								{
									array[j, num19] = num21;
								}
								mipmapGlobalVertexIndex[l] = num21;
								continue;
							}
							num21 = (((num20 & 2) == 0) ? array[j - (num20 & 1), num19] : array2[j - (num20 & 1), num19]);
						}
						else if (num18 == 0)
						{
							byte b4 = Transvoxel.transitionCornerData[num17];
							ushort num23 = (ushort)(b4 >> 4);
							if (num23 == 8)
							{
								num21 = array[j, b4 & 0xF];
							}
							else
							{
								if ((num23 & num) != num23)
								{
									num21 = (ushort)globalVertexCount++;
									vertexStorage[num21].position0 = new Vector3i(mipmapCoord[num17].x << 8, mipmapCoord[num17].y << 8, mipmapCoord[num17].z << 8);
									if ((ushort)((num15 >> 8) & 0xF) > 7)
									{
										vertexStorage[num21].normal = CalculateNormal(mipmapCoord[num17]);
										CalculateSecondaryPosition(detailLevel, num21);
									}
									else
									{
										vertexStorage[num21].normal = CalculateNormal(mipmapCoord[num17]);
									}
									mipmapGlobalVertexIndex[l] = num21;
									continue;
								}
								num21 = (((num23 & 2) == 0) ? array[j - (num23 & 1), b4 & 0xF] : array2[j - (num23 & 1), b4 & 0xF]);
							}
						}
						else
						{
							byte b5 = Transvoxel.transitionCornerData[num16];
							ushort num24 = (ushort)(b5 >> 4);
							if (num24 == 8)
							{
								num21 = array[j, b5 & 0xF];
							}
							else
							{
								if ((num24 & num) != num24)
								{
									num21 = (ushort)globalVertexCount++;
									vertexStorage[num21].position0 = new Vector3i(mipmapCoord[num16].x << 8, mipmapCoord[num16].y << 8, mipmapCoord[num16].z << 8);
									if ((ushort)((num15 >> 8) & 0xF) > 7)
									{
										vertexStorage[num21].normal = CalculateNormal(mipmapCoord[num16]);
										CalculateSecondaryPosition(detailLevel, num21);
									}
									else
									{
										vertexStorage[num21].normal = CalculateNormal(mipmapCoord[num16]);
									}
									mipmapGlobalVertexIndex[l] = num21;
									continue;
								}
								num21 = (((num24 & 2) == 0) ? array[j - (num24 & 1), b5 & 0xF] : array2[j - (num24 & 1), b5 & 0xF]);
							}
						}
						mipmapGlobalVertexIndex[l] = num21;
					}
					if (b2 != byte.MaxValue)
					{
						int num25 = 0;
						int num26 = 0;
						if (((b3 & 0x80) ^ faceFlip[face]) != 0)
						{
							for (int m = 0; m < triangleCount; m++)
							{
								triangleStorage[globalTriangleCount + num26].index0 = mipmapGlobalVertexIndex[transitionCellData.vertexIndex[num25]];
								triangleStorage[globalTriangleCount + num26].index1 = mipmapGlobalVertexIndex[transitionCellData.vertexIndex[1 + num25]];
								triangleStorage[globalTriangleCount + num26].index2 = mipmapGlobalVertexIndex[transitionCellData.vertexIndex[2 + num25]];
								num25 += 3;
								num26++;
							}
						}
						else
						{
							for (int n = 0; n < triangleCount; n++)
							{
								triangleStorage[globalTriangleCount + num26].index0 = mipmapGlobalVertexIndex[transitionCellData.vertexIndex[num25]];
								triangleStorage[globalTriangleCount + num26].index1 = mipmapGlobalVertexIndex[transitionCellData.vertexIndex[2 + num25]];
								triangleStorage[globalTriangleCount + num26].index2 = mipmapGlobalVertexIndex[transitionCellData.vertexIndex[1 + num25]];
								num25 += 3;
								num26++;
							}
						}
						globalTriangleCount += triangleCount;
					}
				}
				num |= 1;
				num7 += num3 * 2;
				num8 += num4 * 2;
			}
			num = 2;
			b ^= 1;
		}
	}
}

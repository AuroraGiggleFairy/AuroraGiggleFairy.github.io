using UnityEngine;

public class MeshGeneratorPrefab : MeshGenerator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab prefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[,,] deck1 = new ushort[17, 17, 4];

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[,,] deck2 = new ushort[17, 17, 4];

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[][,,] decks = new ushort[2][,,];

	[PublicizedFrom(EAccessModifier.Private)]
	public Transvoxel.BuildVertex[] vertexStorage = new Transvoxel.BuildVertex[8192];

	[PublicizedFrom(EAccessModifier.Private)]
	public Transvoxel.BuildTriangle[] triangleStorage = new Transvoxel.BuildTriangle[8192];

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[] density = new sbyte[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] globalVertexIndex = new int[12];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 cVectorAdd = new Vector3(0.5f, 0.5f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue[] blockValues = new BlockValue[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] bTopSoil = new bool[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public int startX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startZ;

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

	public MeshGeneratorPrefab(Prefab _prefab)
		: base(_prefab)
	{
		prefab = _prefab;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateMesh(Vector3i _worldPos, Vector3 _drawPosOffset, Vector3i _start, Vector3i _end, VoxelMesh[] _meshes, bool _bCalcAmbientLight, bool _bOnlyDistortVertices)
	{
		base.CreateMesh(_worldPos, _drawPosOffset, _start, _end, _meshes, _bCalcAmbientLight, _bOnlyDistortVertices);
		build(cVectorAdd + _drawPosOffset, new Vector3i(_start.x, _start.y, _start.z), new Vector3i(_end.x, _end.y, _end.z), _meshes[5]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte getDensity(int _x, int _y, int _z, int _idxInBlockValues = -1)
	{
		_x--;
		_z--;
		if (_y < 0)
		{
			return MarchingCubes.DensityTerrain;
		}
		if (_y >= 256)
		{
			return MarchingCubes.DensityAir;
		}
		if (!checkCoordinates(_x, _y, _z))
		{
			return MarchingCubes.DensityAir;
		}
		return prefab.GetDensity(_x, _y, _z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue getBlockValue(int _x, int _y, int _z)
	{
		_x--;
		_z--;
		if (!checkCoordinates(_x, _y, _z))
		{
			return BlockValue.Air;
		}
		return prefab.GetBlockNoDamage(prefab.GetLocalRotation(), _x, _y, _z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkCoordinates(int _x, int _y, int _z)
	{
		if (_x >= 0 && _x < prefab.size.x && _y >= 0 && _y < prefab.size.y && _z >= 0 && _z < prefab.size.z)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTopSoil(int _x, int _z)
	{
		return prefab.GetChunkFromWorldPos(new Vector3i(_x, 0, _z))?.IsTopSoil(_x & 0xF, _z & 0xF) ?? false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetTerrainHeight(int _x, int _z)
	{
		return prefab.GetChunkFromWorldPos(new Vector3i(_x, 0, _z))?.GetTerrainHeight(_x & 0xF, _z & 0xF) ?? 0;
	}

	public void GenerateMeshOffset(Vector3i _worldStartPos, Vector3i _worldEndPos, VoxelMesh[] _meshes)
	{
		base.CreateMesh(Vector3i.zero, Vector3.zero, _worldStartPos - new Vector3i(1, 0, 1), _worldEndPos, _meshes, _bCalcAmbientLight: true, _bOnlyDistortVertices: false);
		build(Vector3.zero, new Vector3i(_worldStartPos.x, _worldStartPos.y, _worldStartPos.z), new Vector3i(_worldEndPos.x, _worldEndPos.y, _worldEndPos.z), _meshes[5]);
		for (int i = 0; i < _meshes.Length; i++)
		{
			_meshes[i].Finished();
		}
	}

	public void GenerateMeshNoTerrain(Vector3i _worldStartPos, Vector3i _worldEndPos, VoxelMesh[] _meshes)
	{
		base.CreateMesh(Vector3i.zero, Vector3.zero, _worldStartPos, _worldEndPos, _meshes, _bCalcAmbientLight: true, _bOnlyDistortVertices: false);
		for (int i = 0; i < _meshes.Length; i++)
		{
			_meshes[i].Finished();
		}
	}

	public void GenerateMeshTerrainOnly(Vector3i _worldStartPos, Vector3i _worldEndPos, VoxelMesh[] _meshes)
	{
		build(Vector3.zero, new Vector3i(_worldStartPos.x, _worldStartPos.y, _worldStartPos.z), new Vector3i(_worldEndPos.x, _worldEndPos.y, _worldEndPos.z), _meshes[5]);
		for (int i = 0; i < _meshes.Length; i++)
		{
			_meshes[i].Finished();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool calcTopSoil(int _x, int _y, int _z)
	{
		_x++;
		_z++;
		int terrainHeight = GetTerrainHeight(_x, _z);
		if (_y >= terrainHeight && isTopSoil(_x, _z))
		{
			return true;
		}
		terrainHeight = GetTerrainHeight(_x + 1, _z);
		if (_y > terrainHeight && isTopSoil(_x + 1, _z))
		{
			return true;
		}
		terrainHeight = GetTerrainHeight(_x, _z + 1);
		if (_y > terrainHeight && isTopSoil(_x, _z + 1))
		{
			return true;
		}
		terrainHeight = GetTerrainHeight(_x, _z - 1);
		if (_y > terrainHeight && isTopSoil(_x, _z - 1))
		{
			return true;
		}
		terrainHeight = GetTerrainHeight(_x - 1, _z);
		if (_y > terrainHeight && isTopSoil(_x - 1, _z))
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
				blockValue = prefab.GetBlock(_x, _y, _z);
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
		int x = _x + adjLightPos[0].x;
		int z = _z + adjLightPos[0].y;
		IChunk chunkFromWorldPos = prefab.GetChunkFromWorldPos(new Vector3i(x, _y, z));
		int x2 = _x + adjLightPos[1].x;
		int z2 = _z + adjLightPos[1].y;
		IChunk chunkFromWorldPos2 = prefab.GetChunkFromWorldPos(new Vector3i(x2, _y, z2));
		int x3 = _x + adjLightPos[2].x;
		int z3 = _z + adjLightPos[2].y;
		IChunk chunkFromWorldPos3 = prefab.GetChunkFromWorldPos(new Vector3i(x3, _y, z3));
		int x4 = _x + adjLightPos[3].x;
		int z4 = _z + adjLightPos[3].y;
		IChunk chunkFromWorldPos4 = prefab.GetChunkFromWorldPos(new Vector3i(x4, _y, z4));
		byte v = (byte)Utils.FastMax(chunkFromWorldPos.GetLight(x, _y, z, Chunk.LIGHT_TYPE.SUN), chunkFromWorldPos2.GetLight(x2, _y, z2, Chunk.LIGHT_TYPE.SUN), chunkFromWorldPos3.GetLight(x3, _y, z3, Chunk.LIGHT_TYPE.SUN), chunkFromWorldPos4.GetLight(x4, _y, z4, Chunk.LIGHT_TYPE.SUN));
		byte v2 = (byte)((_y < 255) ? ((byte)Utils.FastMax(chunkFromWorldPos.GetLight(x, _y + 1, z, Chunk.LIGHT_TYPE.SUN), chunkFromWorldPos2.GetLight(x2, _y + 1, z2, Chunk.LIGHT_TYPE.SUN), chunkFromWorldPos3.GetLight(x3, _y + 1, z3, Chunk.LIGHT_TYPE.SUN), chunkFromWorldPos4.GetLight(x4, _y + 1, z4, Chunk.LIGHT_TYPE.SUN))) : 15);
		byte v3 = (byte)Utils.FastMax(chunkFromWorldPos.GetLight(x, _y, z, Chunk.LIGHT_TYPE.BLOCK), chunkFromWorldPos2.GetLight(x2, _y, z2, Chunk.LIGHT_TYPE.BLOCK), chunkFromWorldPos3.GetLight(x3, _y, z3, Chunk.LIGHT_TYPE.BLOCK), chunkFromWorldPos4.GetLight(x4, _y, z4, Chunk.LIGHT_TYPE.BLOCK));
		byte v4 = (byte)((_y < 255) ? ((byte)Utils.FastMax(chunkFromWorldPos.GetLight(x, _y + 1, z, Chunk.LIGHT_TYPE.BLOCK), chunkFromWorldPos2.GetLight(x2, _y + 1, z2, Chunk.LIGHT_TYPE.BLOCK), chunkFromWorldPos3.GetLight(x3, _y + 1, z3, Chunk.LIGHT_TYPE.BLOCK), chunkFromWorldPos4.GetLight(x4, _y + 1, z4, Chunk.LIGHT_TYPE.BLOCK))) : 15);
		_sun = Utils.FastMax(v, v2);
		_block = Utils.FastMax(v3, v4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetDeckIndex(int x, int y, int z)
	{
		return x * 16 * 4 + y * 4 + z;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void build(Vector3 _drawPosOffset, Vector3i _start, Vector3i _end, VoxelMesh _mesh)
	{
		if (IsLayerEmpty(_start.y / 16, _end.y / 16))
		{
			return;
		}
		bool flag = false;
		for (int i = _start.z; i <= _end.z; i++)
		{
			for (int j = _start.y; j <= _end.y; j++)
			{
				int num = _start.x;
				while (num <= _end.x)
				{
					if (!getBlockValue(num, j, i).Block.shape.IsTerrain())
					{
						num++;
						continue;
					}
					goto IL_005c;
				}
			}
			continue;
			IL_005c:
			flag = true;
			break;
		}
		if (!flag)
		{
			return;
		}
		decks[0] = deck1;
		decks[1] = deck2;
		int num2 = 0;
		int num3 = 0;
		byte b = 0;
		int num4 = 0;
		for (int k = _start.z; k <= _end.z; k++)
		{
			ushort[,,] array = decks[b];
			ushort[,,] array2 = decks[b ^ 1];
			for (int l = _start.y; l <= _end.y; l++)
			{
				int num5 = l - _start.y;
				for (int m = _start.x; m <= _end.x; m++)
				{
					int num6 = m - _start.x;
					byte b2 = 0;
					blockValues[0] = getBlockValue(m, l, k);
					blockValues[1] = getBlockValue(m + 1, l, k);
					blockValues[2] = getBlockValue(m, l + 1, k);
					blockValues[3] = getBlockValue(m + 1, l + 1, k);
					blockValues[4] = getBlockValue(m, l, k + 1);
					blockValues[5] = getBlockValue(m + 1, l, k + 1);
					blockValues[6] = getBlockValue(m, l + 1, k + 1);
					blockValues[7] = getBlockValue(m + 1, l + 1, k + 1);
					density[0] = getDensity(m, l, k, 0);
					density[1] = getDensity(m + 1, l, k, 1);
					density[2] = getDensity(m, l + 1, k, 2);
					density[3] = getDensity(m + 1, l + 1, k, 3);
					density[4] = getDensity(m, l, k + 1, 4);
					density[5] = getDensity(m + 1, l, k + 1, 5);
					density[6] = getDensity(m, l + 1, k + 1, 6);
					density[7] = getDensity(m + 1, l + 1, k + 1, 7);
					bTopSoil[0] = true;
					bTopSoil[1] = true;
					bTopSoil[2] = true;
					bTopSoil[3] = true;
					bTopSoil[4] = true;
					bTopSoil[5] = true;
					bTopSoil[6] = true;
					bTopSoil[7] = true;
					sbyte b3 = density[7];
					int num7 = ((density[0] >> 7) & 1) | ((density[1] >> 6) & 2) | ((density[2] >> 5) & 4) | ((density[3] >> 4) & 8) | ((density[4] >> 3) & 0x10) | ((density[5] >> 2) & 0x20) | ((density[6] >> 1) & 0x40) | (b3 & 0x80);
					if (b3 == 0)
					{
						int num8 = num2++;
						array[num5, num6, 0] = (ushort)num8;
						vertexStorage[num8].position0 = new Vector3i(m + 1 << 8, num5 + 1 << 8, k + 1 << 8);
						vertexStorage[num8].normal = CalculateNormal(new Vector3i(m + 1, num5 + 1, k + 1));
						vertexStorage[num8].material = b2;
						vertexStorage[num8].texture = GetTextureFor(m, l, k, 7);
						vertexStorage[num8].bTopSoil = bTopSoil[7];
					}
					if ((num7 ^ ((b3 >> 7) & 0xFF)) != 0)
					{
						byte b4 = Transvoxel.regularCellClass[num7];
						Transvoxel.RegularCellData regularCellData = Transvoxel.regularCellData[0, b4];
						int triangleCount = regularCellData.GetTriangleCount();
						if (num3 + triangleCount > 8192)
						{
							goto end_IL_09d9;
						}
						int vertexCount = regularCellData.GetVertexCount();
						Transvoxel.RegularVertexData.Row row = Transvoxel.regularVertexData[num7];
						for (int n = 0; n < vertexCount; n++)
						{
							int num9 = 0;
							ushort num10 = row.data[n];
							int num11 = (num10 >> 4) & 0xF;
							int num12 = num10 & 0xF;
							int num13 = density[num11];
							int num14 = density[num12];
							int num15 = (num14 << 8) / (num14 - num13);
							Vector3i vector3i = new Vector3i(m + (num11 & 1), num5 + ((num11 >> 1) & 1), k + ((num11 >> 2) & 1));
							Vector3i vector3i2 = new Vector3i(m + (num12 & 1), num5 + ((num12 >> 1) & 1), k + ((num12 >> 2) & 1));
							if ((num15 & 0xFF) != 0)
							{
								int num16 = (num10 >> 8) & 0xF;
								int num17 = (num10 >> 12) & 0xF;
								if ((num17 & num4) != num17)
								{
									num9 = num2++;
									int num18 = 256 - num15;
									vertexStorage[num9].position0 = vector3i * num15 + vector3i2 * num18;
									vertexStorage[num9].normal = CalculateNormal(vector3i, vector3i2, num15, num18);
									vertexStorage[num9].material = b2;
									vertexStorage[num9].texture = GetTextureFor(m, l, k, (num13 < num14) ? num11 : num12);
									vertexStorage[num9].bTopSoil = bTopSoil[(num13 < num14) ? num11 : num12];
									if (num12 == 7)
									{
										try
										{
											array[num5, num6, num16] = (ushort)num9;
										}
										catch
										{
											Log.Error("Out of bounds! dY='{0}' x='{1}' edgeIndex='{2}'", num5, num6, num16);
										}
									}
									globalVertexIndex[n] = num9;
									continue;
								}
								if ((num17 & 4) != 0)
								{
									num9 = array2[num5 - ((num17 >> 1) & 1), num6 - (num17 & 1), num16];
								}
								else
								{
									try
									{
										num9 = array[num5 - (num17 >> 1), num6 - (num17 & 1), num16];
									}
									catch
									{
										Log.Error("Out of bounds! dY='{0}' x='{1}' edgeIndex='{2}'", num5 - (num17 >> 1), num6 - (num17 & 1), num16);
									}
								}
							}
							else if (num15 == 0)
							{
								if (num12 == 7)
								{
									num9 = array[num5, num6, 0];
								}
								else
								{
									int num19 = num12 ^ 7;
									if ((num19 & num4) != num19)
									{
										num9 = num2++;
										vertexStorage[num9].position0 = new Vector3i(vector3i2.x << 8, vector3i2.y << 8, vector3i2.z << 8);
										vertexStorage[num9].normal = CalculateNormal(vector3i2);
										vertexStorage[num9].material = b2;
										vertexStorage[num9].texture = GetTextureFor(m, l, k, num12);
										vertexStorage[num9].bTopSoil = bTopSoil[num12];
										globalVertexIndex[n] = num9;
										continue;
									}
									num9 = (((num19 & 4) == 0) ? array[num5 - (num19 >> 1), num6 - (num19 & 1), 0] : array2[num5 - ((num19 >> 1) & 1), num6 - (num19 & 1), 0]);
								}
							}
							else
							{
								int num20 = num11 ^ 7;
								if ((num20 & num4) != num20)
								{
									num9 = num2++;
									vertexStorage[num9].position0 = new Vector3i(vector3i.x << 8, vector3i.y << 8, vector3i.z << 8);
									vertexStorage[num9].normal = CalculateNormal(vector3i);
									vertexStorage[num9].material = b2;
									vertexStorage[num9].texture = GetTextureFor(m, l, k, num11);
									vertexStorage[num9].bTopSoil = bTopSoil[num11];
									globalVertexIndex[n] = num9;
									continue;
								}
								num9 = (((num20 & 4) == 0) ? array[num5 - (num20 >> 1), num6 - (num20 & 1), 0] : array2[num5 - ((num20 >> 1) & 1), num6 - (num20 & 1), 0]);
							}
							globalVertexIndex[n] = num9;
						}
						if (b2 != byte.MaxValue)
						{
							byte[] vertexIndex = regularCellData.vertexIndex;
							int num21 = 0;
							for (int num22 = 0; num22 < triangleCount; num22++)
							{
								int num23 = num3 + num22;
								triangleStorage[num23].index0 = globalVertexIndex[vertexIndex[num21]];
								triangleStorage[num23].index1 = globalVertexIndex[vertexIndex[1 + num21]];
								triangleStorage[num23].index2 = globalVertexIndex[vertexIndex[2 + num21]];
								num21 += 3;
							}
							num3 += triangleCount;
						}
					}
					num4 |= 1;
				}
				num4 = (num4 | 2) & 6;
			}
			num4 = 4;
			b ^= 1;
			continue;
			end_IL_09d9:
			break;
		}
		int num24 = 0;
		for (int num25 = 0; num25 < num2; num25++)
		{
			if (vertexStorage[num25].material != byte.MaxValue)
			{
				Vector3i position = vertexStorage[num25].position0;
				if ((position.x | position.y | position.z) >= 0)
				{
					int num26 = 1;
					num26 |= (position.x >> 11) & 2;
					num26 |= (position.y >> 10) & 4;
					num26 |= (position.z >> 9) & 8;
					vertexStorage[num25].statusFlags = (byte)num26;
					vertexStorage[num25].remapIndex = (ushort)num24;
					num24++;
				}
			}
			else
			{
				vertexStorage[num25].statusFlags = 0;
			}
		}
		int num27 = 0;
		for (int num28 = 0; num28 < num3; num28++)
		{
			int index = triangleStorage[num28].index0;
			int index2 = triangleStorage[num28].index1;
			int index3 = triangleStorage[num28].index2;
			Vector3i vector3i3 = Vector3i.Cross(vertexStorage[index2].position0 - vertexStorage[index].position0, vertexStorage[index3].position0 - vertexStorage[index].position0);
			bool flag2 = false;
			if ((vector3i3.x | vector3i3.y | vector3i3.z) != 0)
			{
				int num29 = ((vector3i3.x >> 31) & 2) | ((vector3i3.y >> 31) & 4) | ((vector3i3.z >> 31) & 8) | 1;
				flag2 = (vertexStorage[index].statusFlags & vertexStorage[index2].statusFlags & vertexStorage[index3].statusFlags & num29) == 1;
			}
			flag2 = true;
			triangleStorage[num28].inclusionFlag = flag2;
			if (flag2)
			{
				num27++;
				int num30 = _mesh.FindOrCreateSubMesh(vertexStorage[index].texture, vertexStorage[index2].texture, vertexStorage[index3].texture);
				triangleStorage[num28].submeshIdx = num30;
				_mesh.GetColorForTextureId(num30, ref vertexStorage[index]);
				_mesh.GetColorForTextureId(num30, ref vertexStorage[index2]);
				_mesh.GetColorForTextureId(num30, ref vertexStorage[index3]);
			}
			else
			{
				Debug.LogError("excluding triangle!");
			}
		}
		if (num27 != num3)
		{
			Log.Warning("MG build {0}, {1}", num27, num3);
		}
		int count = _mesh.m_Vertices.Count;
		Vector3 vector = new Vector3(-0.5f, (float)_start.y + 0.5f, -0.5f) + _drawPosOffset;
		for (int num31 = 0; num31 < num2; num31++)
		{
			if ((vertexStorage[num31].statusFlags & 1) != 0)
			{
				_mesh.m_Vertices.Add(vertexStorage[num31].position0.ToVector3() / 256f + vector);
				_mesh.m_ColorVertices.Add(vertexStorage[num31].color);
				_mesh.m_Uvs.Add(vertexStorage[num31].uv);
				_mesh.UvsCrack.Add(vertexStorage[num31].uv2);
				if (_mesh.m_Uvs3 == null)
				{
					_mesh.m_Uvs3 = new ArrayListMP<Vector2>(MemoryPools.poolVector2, 100000);
				}
				if (_mesh.m_Uvs4 == null)
				{
					_mesh.m_Uvs4 = new ArrayListMP<Vector2>(MemoryPools.poolVector2, 100000);
				}
				_mesh.m_Uvs3.Add(vertexStorage[num31].uv3);
				_mesh.m_Uvs4.Add(vertexStorage[num31].uv4);
			}
		}
		for (int num32 = 0; num32 < num3; num32++)
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
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalculateNormal(Vector3i coord0, Vector3i coord1, int t, int u)
	{
		Vector3 vector = CalculateNormal(coord0);
		Vector3 vector2 = CalculateNormal(coord1);
		Vector3 result = vector * t + vector2 * u;
		result.Normalize();
		return result;
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
}

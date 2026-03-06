using System.Diagnostics;
using Unity.Profiling;
using UnityEngine;

public class MeshGenerator : IMeshGenerator
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public INeighborBlockCache nBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public LightingAround lightingAround = new LightingAround(0, 0, 0);

	[PublicizedFrom(EAccessModifier.Private)]
	public Lighting3DArray lightCube = new Lighting3DArray();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] verticesCube = new Vector3[8];

	[PublicizedFrom(EAccessModifier.Protected)]
	public int[] heights = new int[9];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWaterDensity = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWaterSize = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWaterFloatHeight = -1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] cpyVerts = new Vector3[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ringHeight1 = 0.9f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ringHeight1c = 0.796f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ringHeight2 = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] raiseHeight = new float[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] dropY = new float[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[,,] neighborType = new int[2, 3, 3];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[,,] neighborIsWater = new bool[2, 3, 3];

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterClippingVolume waterClippingVolume = new WaterClippingVolume();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker renderTopWaterOldMarker = new ProfilerMarker("MeshGenerator.RenderTopWaterOld");

	public MeshGenerator(INeighborBlockCache _nBlocks)
	{
		nBlocks = _nBlocks;
	}

	public void GenerateMesh(Vector3i _chunkPos, int _layerIdx, VoxelMesh[] _meshes)
	{
		CreateMesh(_start: new Vector3i(0, Utils.FastMax(0, _layerIdx * 16), 0), _end: new Vector3i(15, _layerIdx * 16 + 16 - 1, 15), _worldPos: _chunkPos, _drawPosOffset: Vector3.zero, _meshes: _meshes, _bCalcAmbientLight: true, _bOnlyDistortVertices: false);
		for (int i = 0; i < _meshes.Length; i++)
		{
			VoxelMesh voxelMesh = _meshes[i];
			voxelMesh.Finished();
			if (voxelMesh.m_Uvs.Count > 0 && voxelMesh.m_Normals.Count > 0 && voxelMesh.m_Tangents.Count == 0)
			{
				Utils.CalculateMeshTangents(voxelMesh, i == 5);
			}
		}
	}

	public virtual bool IsLayerEmpty(int _layerIdx)
	{
		return IsLayerEmpty(_layerIdx, _layerIdx);
	}

	public virtual bool IsLayerEmpty(int _startLayerIdx, int _endLayerIdx)
	{
		return mcLayerIsEmpty(_startLayerIdx, _endLayerIdx);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool mcLayerIsEmpty(int _startLayerIdx, int _endLayerIdx)
	{
		int num = _startLayerIdx * 4;
		int num2 = (_endLayerIdx + 1) * 4;
		nBlocks.Init(0, 0);
		IChunk neighborChunk = nBlocks.GetNeighborChunk(0, 0);
		if (neighborChunk.IsOnlyTerrainLayer(num) || neighborChunk.IsEmptyLayer(num))
		{
			IChunk neighborChunk2 = nBlocks.GetNeighborChunk(1, 0);
			IChunk neighborChunk3 = nBlocks.GetNeighborChunk(-1, 0);
			IChunk neighborChunk4 = nBlocks.GetNeighborChunk(0, 1);
			IChunk neighborChunk5 = nBlocks.GetNeighborChunk(0, -1);
			for (int i = num - 1; i <= num2 + 1; i++)
			{
				if ((!neighborChunk.IsOnlyTerrainLayer(i) && !neighborChunk.IsEmptyLayer(i)) || (!neighborChunk2.IsOnlyTerrainLayer(i) && !neighborChunk2.IsEmptyLayer(i)) || (!neighborChunk3.IsOnlyTerrainLayer(i) && !neighborChunk3.IsEmptyLayer(i)) || (!neighborChunk4.IsOnlyTerrainLayer(i) && !neighborChunk4.IsEmptyLayer(i)) || (!neighborChunk5.IsOnlyTerrainLayer(i) && !neighborChunk5.IsEmptyLayer(i)))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public void GenerateMesh(Vector3i _worldStartPos, Vector3i _worldEndPos, VoxelMesh[] _meshes)
	{
		CreateMesh(Vector3i.zero, Vector3.zero, _worldStartPos, _worldEndPos, _meshes, _bCalcAmbientLight: true, _bOnlyDistortVertices: false);
		for (int i = 0; i < _meshes.Length; i++)
		{
			_meshes[i].Finished();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Lighting maxLight(Lighting _lMiddle, Lighting _lL1N1, Lighting _lL1N2, Lighting _lL1C, Lighting _lL2, Lighting _lL2N1, Lighting _lL2N2, Lighting _lL2C)
	{
		int num = 0;
		int num2 = 0;
		if (_lL1N1.sun != 0)
		{
			num += _lL1N1.sun;
			num2++;
		}
		if (_lL1N2.sun != 0)
		{
			num += _lL1N2.sun;
			num2++;
		}
		if (_lL1C.sun != 0)
		{
			num += _lL1C.sun;
			num2++;
		}
		if (_lMiddle.sun != 0)
		{
			num += _lMiddle.sun;
			num2++;
		}
		if (_lL2.sun != 0)
		{
			num += _lL2.sun;
			num2++;
		}
		if (_lL2N1.sun != 0)
		{
			num += _lL2N1.sun;
			num2++;
		}
		if (_lL2N2.sun != 0)
		{
			num += _lL2N2.sun;
			num2++;
		}
		if (_lL2C.sun != 0)
		{
			num += _lL2C.sun;
			num2++;
		}
		Lighting result = default(Lighting);
		result.sun = (byte)((float)num / (float)num2);
		result.block = 0;
		result.stability = 0;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwizzleCopy(ref Vector3[] to, int index0, int index1, int index2, int index3)
	{
		to[0] = verticesCube[index0];
		to[1] = verticesCube[index1];
		to[2] = verticesCube[index2];
		to[3] = verticesCube[index3];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidTriNorth(Vector3[] v4Arr, VoxelMesh[] _meshes, int y)
	{
		Vector2 uVdata = new Vector2(1f, 0f);
		bool num = nBlocks.IsWater(1, y + 1, -1);
		bool flag = nBlocks.IsWater(-1, y + 1, -1);
		if (num)
		{
			cpyVerts[0] = v4Arr[0];
			cpyVerts[1] = v4Arr[1];
			cpyVerts[2] = v4Arr[3];
			cpyVerts[3] = v4Arr[0];
			cpyVerts[1].x += 0.75f;
			for (int i = 0; i < 4; i++)
			{
				cpyVerts[i].y += -0.5f;
			}
			WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
		}
		if (flag)
		{
			cpyVerts[0] = v4Arr[0];
			cpyVerts[1] = v4Arr[1];
			cpyVerts[2] = v4Arr[2];
			cpyVerts[0].x -= 0.75f;
			cpyVerts[3] = cpyVerts[0];
			for (int j = 0; j < 4; j++)
			{
				cpyVerts[j].y += -0.5f;
			}
			WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidTriSouth(Vector3[] v4Arr, VoxelMesh[] _meshes, int y)
	{
		Vector2 uVdata = new Vector2(1f, 0f);
		bool num = nBlocks.IsWater(1, y + 1, 1);
		bool flag = nBlocks.IsWater(-1, y + 1, 1);
		if (num)
		{
			cpyVerts[0] = v4Arr[0];
			cpyVerts[1] = v4Arr[1];
			cpyVerts[2] = v4Arr[2];
			cpyVerts[0].x += 0.75f;
			cpyVerts[3] = cpyVerts[0];
			for (int i = 0; i < 4; i++)
			{
				cpyVerts[i].y += -0.5f;
			}
			WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
		}
		if (flag)
		{
			cpyVerts[0] = v4Arr[0];
			cpyVerts[1] = v4Arr[1];
			cpyVerts[2] = v4Arr[3];
			cpyVerts[1].x -= 0.75f;
			cpyVerts[3] = v4Arr[0];
			for (int j = 0; j < 4; j++)
			{
				cpyVerts[j].y += -0.5f;
			}
			WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidTriEast(Vector3[] v4Arr, VoxelMesh[] _meshes, int y)
	{
		Vector2 uVdata = new Vector2(0f, 1f);
		bool num = nBlocks.IsWater(-1, y + 1, 1);
		bool flag = nBlocks.IsWater(-1, y + 1, -1);
		if (num)
		{
			cpyVerts[0] = v4Arr[0];
			cpyVerts[1] = v4Arr[1];
			cpyVerts[2] = v4Arr[2];
			cpyVerts[0].z += 0.75f;
			cpyVerts[3] = cpyVerts[0];
			for (int i = 0; i < 4; i++)
			{
				cpyVerts[i].y += -0.5f;
			}
			WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
		}
		if (flag)
		{
			cpyVerts[0] = v4Arr[0];
			cpyVerts[1] = v4Arr[1];
			cpyVerts[2] = v4Arr[3];
			cpyVerts[1].z -= 0.75f;
			cpyVerts[3] = v4Arr[0];
			for (int j = 0; j < 4; j++)
			{
				cpyVerts[j].y += -0.5f;
			}
			WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidTriWest(Vector3[] v4Arr, VoxelMesh[] _meshes, int y)
	{
		Vector2 uVdata = new Vector2(0f, 1f);
		bool num = nBlocks.IsWater(1, y + 1, 1);
		bool flag = nBlocks.IsWater(1, y + 1, -1);
		if (num)
		{
			cpyVerts[0] = v4Arr[0];
			cpyVerts[1] = v4Arr[1];
			cpyVerts[2] = v4Arr[3];
			cpyVerts[3] = v4Arr[0];
			cpyVerts[1].z += 0.75f;
			for (int i = 0; i < 4; i++)
			{
				cpyVerts[i].y += -0.5f;
			}
			WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
		}
		if (flag)
		{
			cpyVerts[0] = v4Arr[0];
			cpyVerts[1] = v4Arr[1];
			cpyVerts[2] = v4Arr[2];
			cpyVerts[0].z -= 0.75f;
			cpyVerts[3] = cpyVerts[0];
			for (int j = 0; j < 4; j++)
			{
				cpyVerts[j].y += -0.5f;
			}
			WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidFaceNorth(Vector3[] v4Arr, VoxelMesh[] _meshes, int cpyX, int cpyZ)
	{
		Vector2 uVdata = new Vector2(1f, 0f);
		cpyVerts[0].z = v4Arr[0].z;
		cpyVerts[1].z = v4Arr[1].z;
		cpyVerts[2].z = v4Arr[2].z;
		cpyVerts[3].z = v4Arr[3].z;
		cpyVerts[0].x = v4Arr[0].x - (float)cpyZ * 0.25f;
		cpyVerts[1].x = cpyVerts[0].x - 0.25f;
		cpyVerts[2].x = cpyVerts[0].x - 0.25f;
		cpyVerts[3].x = v4Arr[0].x - (float)cpyZ * 0.25f;
		cpyVerts[0].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		cpyVerts[1].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		cpyVerts[2].y = cpyVerts[0].y + 0.25f;
		cpyVerts[3].y = cpyVerts[0].y + 0.25f;
		WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidFaceSouth(Vector3[] v4Arr, VoxelMesh[] _meshes, int cpyX, int cpyZ)
	{
		Vector2 uVdata = new Vector2(1f, 0f);
		cpyVerts[0].z = v4Arr[0].z;
		cpyVerts[1].z = v4Arr[1].z;
		cpyVerts[2].z = v4Arr[2].z;
		cpyVerts[3].z = v4Arr[3].z;
		cpyVerts[0].x = v4Arr[0].x + (float)cpyZ * 0.25f;
		cpyVerts[1].x = cpyVerts[0].x + 0.25f;
		cpyVerts[2].x = cpyVerts[0].x + 0.25f;
		cpyVerts[3].x = v4Arr[0].x + (float)cpyZ * 0.25f;
		cpyVerts[0].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		cpyVerts[1].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		cpyVerts[2].y = cpyVerts[0].y + 0.25f;
		cpyVerts[3].y = cpyVerts[0].y + 0.25f;
		WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidFaceEast(Vector3[] v4Arr, VoxelMesh[] _meshes, int cpyX, int cpyZ)
	{
		Vector2 uVdata = new Vector2(0f, 1f);
		cpyVerts[0].x = v4Arr[0].x;
		cpyVerts[1].x = v4Arr[1].x;
		cpyVerts[2].x = v4Arr[2].x;
		cpyVerts[3].x = v4Arr[3].x;
		cpyVerts[0].z = v4Arr[0].z + (float)cpyZ * 0.25f;
		cpyVerts[1].z = cpyVerts[0].z + 0.25f;
		cpyVerts[2].z = cpyVerts[0].z + 0.25f;
		cpyVerts[3].z = v4Arr[0].z + (float)cpyZ * 0.25f;
		cpyVerts[0].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		cpyVerts[1].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		cpyVerts[2].y = cpyVerts[0].y + 0.25f;
		cpyVerts[3].y = cpyVerts[0].y + 0.25f;
		WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidFaceWest(Vector3[] v4Arr, VoxelMesh[] _meshes, int cpyX, int cpyZ)
	{
		Vector2 uVdata = new Vector2(0f, 1f);
		cpyVerts[0].x = v4Arr[0].x;
		cpyVerts[1].x = v4Arr[1].x;
		cpyVerts[2].x = v4Arr[2].x;
		cpyVerts[3].x = v4Arr[3].x;
		cpyVerts[0].z = v4Arr[0].z - (float)cpyZ * 0.25f;
		cpyVerts[1].z = cpyVerts[0].z - 0.25f;
		cpyVerts[2].z = cpyVerts[0].z - 0.25f;
		cpyVerts[3].z = v4Arr[0].z - (float)cpyZ * 0.25f;
		cpyVerts[0].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		cpyVerts[1].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		cpyVerts[2].y = cpyVerts[0].y + 0.25f;
		cpyVerts[3].y = cpyVerts[0].y + 0.25f;
		WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, uVdata);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAir(int type)
	{
		return type == 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPlant(int type)
	{
		Block block = Block.list[type];
		if (block == null)
		{
			return false;
		}
		if (block.blockMaterial == null)
		{
			return false;
		}
		return block.blockMaterial.IsPlant;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsTerrain(int type)
	{
		Block block = Block.list[type];
		if (block == null)
		{
			return false;
		}
		if (block.shape == null)
		{
			return false;
		}
		return block.shape.IsTerrain();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsSolidCube(int type)
	{
		Block block = Block.list[type];
		if (block == null)
		{
			return false;
		}
		if (block.shape == null)
		{
			return false;
		}
		return block.shape.IsSolidCube;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawSide(BlockFace _face, BlockValue midBV, INeighborBlockCache nBlocks, Vector3[] v4Arr, float biomeTemperature, ref bool isWaterTopDrawn, ref bool isWaterLayerStarted, Vector3 drawPos, VoxelMesh[] _meshes, Vector3i posI, Vector3i _worldPos)
	{
		int x = posI.x;
		int y = posI.y;
		int z = posI.z;
		int num = 0;
		int num2 = 0;
		switch (_face)
		{
		case BlockFace.North:
			num2 = -1;
			break;
		case BlockFace.South:
			num2 = 1;
			break;
		case BlockFace.East:
			num = -1;
			break;
		case BlockFace.West:
			num = 1;
			break;
		}
		BlockValue blockValue = nBlocks.Get(num, y, num2);
		Block block = blockValue.Block;
		if (block == null)
		{
			return;
		}
		byte stab = nBlocks.GetStab(num, y, num2);
		lightingAround.SetStab(stab);
		bool flag = nBlocks.IsWater(num, y, num2) && FacePermitsFlow(blockValue, _face);
		if (flag && (IsTerrain(midBV.type) || (!IsSolidCube(midBV.type) && !IsPlant(midBV.type) && !midBV.isair)))
		{
			bool num3 = isWaterLayerStarted;
			isWaterTopDrawn = true;
			isWaterLayerStarted = true;
			if (!num3 && !nBlocks.IsWater(num, y + 1, num2))
			{
				SwizzleCopy(ref v4Arr, 1, 2, 6, 5);
				RenderTopWater(midBV, v4Arr, _meshes, posI, _worldPos, isInsideTerrain: true);
			}
		}
		switch (_face)
		{
		case BlockFace.North:
			SwizzleCopy(ref v4Arr, 3, 0, 1, 2);
			break;
		case BlockFace.South:
			SwizzleCopy(ref v4Arr, 4, 7, 6, 5);
			break;
		case BlockFace.East:
			SwizzleCopy(ref v4Arr, 0, 4, 5, 1);
			break;
		case BlockFace.West:
			SwizzleCopy(ref v4Arr, 7, 3, 2, 6);
			break;
		}
		if (block.shape.isRenderFace(blockValue, _face, midBV))
		{
			Vector3 drawPos2 = drawPos + Vector3.forward;
			switch (_face)
			{
			case BlockFace.North:
				drawPos2 = drawPos - Vector3.forward;
				break;
			case BlockFace.South:
				drawPos2 = drawPos + Vector3.forward;
				break;
			case BlockFace.East:
				drawPos2 = drawPos - Vector3.right;
				break;
			case BlockFace.West:
				drawPos2 = drawPos + Vector3.right;
				break;
			}
			IChunk chunk = nBlocks.GetChunk(num, num2);
			int x2 = World.toBlockXZ(x + num);
			int z2 = World.toBlockXZ(z + num2);
			TextureFullArray textureFullArray = chunk.GetTextureFullArray(x2, y, z2);
			block.shape.renderFace(_worldPos, blockValue, drawPos2, _face, v4Arr, lightingAround, textureFullArray, _meshes);
		}
		if (!flag || isWaterLayerStarted)
		{
			return;
		}
		bool num4 = nBlocks.IsWater(num, y + 1, num2);
		Block block2 = nBlocks.Get(num, y + 1, num2).Block;
		if (num4 || block2.shape.IsTerrain())
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					switch (_face)
					{
					case BlockFace.North:
						RenderLiquidFaceNorth(v4Arr, _meshes, i, j);
						break;
					case BlockFace.South:
						RenderLiquidFaceSouth(v4Arr, _meshes, i, j);
						break;
					case BlockFace.East:
						RenderLiquidFaceEast(v4Arr, _meshes, i, j);
						break;
					case BlockFace.West:
						RenderLiquidFaceWest(v4Arr, _meshes, i, j);
						break;
					}
				}
			}
		}
		else
		{
			switch (_face)
			{
			case BlockFace.North:
				RenderLiquidTriNorth(v4Arr, _meshes, y);
				break;
			case BlockFace.South:
				RenderLiquidTriSouth(v4Arr, _meshes, y);
				break;
			case BlockFace.East:
				RenderLiquidTriEast(v4Arr, _meshes, y);
				break;
			case BlockFace.West:
				RenderLiquidTriWest(v4Arr, _meshes, y);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool FacePermitsFlow(BlockValue bv, BlockFace worldspaceFace)
	{
		return (bv.rotatedWaterFlowMask & BlockFaceFlags.FromBlockFace(worldspaceFace)) == 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool FacePermitsFlow(BlockFaceFlag flags, BlockFace worldspaceFace)
	{
		return (flags & BlockFaceFlags.FromBlockFace(worldspaceFace)) == 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void CreateMesh(Vector3i _worldPos, Vector3 _drawPosOffset, Vector3i _start, Vector3i _end, VoxelMesh[] _meshes, bool _bCalcAmbientLight, bool _bOnlyDistortVertices)
	{
		if (mcLayerIsEmpty(_start.y / 16, _end.y / 16))
		{
			return;
		}
		World world = GameManager.Instance.World;
		Vector3 zero = Vector3.zero;
		Vector3i zero2 = Vector3i.zero;
		Vector3[] to = new Vector3[4];
		lightCube.SetBlockCache(nBlocks);
		for (int i = _start.x; i <= _end.x; i++)
		{
			for (int j = _start.z; j <= _end.z; j++)
			{
				float temperature = VoxelMesh.GetTemperature(world.GetBiome(_worldPos.x + i, _worldPos.z + j));
				nBlocks.Init(i, j);
				int y = _start.y;
				heights[0] = nBlocks.GetChunk(0, 0).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i, j));
				heights[1] = nBlocks.GetChunk(1, 0).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i + 1, j));
				heights[2] = nBlocks.GetChunk(0, -1).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i, j - 1));
				heights[3] = nBlocks.GetChunk(-1, 0).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i - 1, j));
				heights[4] = nBlocks.GetChunk(0, 1).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i, j + 1));
				heights[5] = (heights[6] = (heights[7] = (heights[8] = 0)));
				int num = heights[0];
				for (int k = 1; k < heights.Length; k++)
				{
					if (num < heights[k])
					{
						num = heights[k];
					}
				}
				num++;
				int v = Utils.FastMin(_end.y, num + 1);
				v = Utils.FastMin(254, v);
				bool isWaterLayerStarted = false;
				for (int num2 = v; num2 >= y; num2--)
				{
					BlockValue blockValue = nBlocks.Get(0, num2, 0);
					Block block = blockValue.Block;
					if (block != null && (blockValue.ischild || !block.shape.IsSolidCube || block.shape.IsTerrain() || (num2 > 0 && nBlocks.IsWater(0, num2 - 1, 0) && !nBlocks.IsWater(-1, num2, 0) && !nBlocks.IsWater(1, num2, 0) && !nBlocks.IsWater(0, num2, -1) && !nBlocks.IsWater(0, num2, 1))))
					{
						bool flag = nBlocks.IsWater(0, num2, 0);
						if (!isWaterLayerStarted && flag)
						{
							isWaterLayerStarted = true;
						}
						zero.x = _drawPosOffset.x + (float)i;
						zero.y = _drawPosOffset.y + (float)num2;
						zero.z = _drawPosOffset.z + (float)j;
						zero2.x = i;
						zero2.y = num2;
						zero2.z = j;
						for (int l = 0; l < 8; l++)
						{
							verticesCube[l].x = BlockShapeCube.Cube[l].x + zero.x;
							verticesCube[l].y = BlockShapeCube.Cube[l].y + zero.y;
							verticesCube[l].z = BlockShapeCube.Cube[l].z + zero.z;
						}
						if (!_bOnlyDistortVertices)
						{
							lightCube.SetPosition(zero2);
							Lighting lighting = lightCube[0, 0, 0];
							lightingAround[LightingAround.Pos.Middle] = lighting;
							lightingAround[LightingAround.Pos.X0Y0Z0] = maxLight(lighting, lightCube[0, 0, -1], lightCube[-1, 0, 0], lightCube[-1, 0, -1], lightCube[0, -1, 0], lightCube[0, -1, -1], lightCube[-1, -1, 0], lightCube[-1, -1, -1]);
							lightingAround[LightingAround.Pos.X1Y0Z0] = maxLight(lighting, lightCube[0, 0, -1], lightCube[1, 0, 0], lightCube[1, 0, -1], lightCube[0, -1, 0], lightCube[0, -1, -1], lightCube[1, -1, 0], lightCube[1, -1, -1]);
							lightingAround[LightingAround.Pos.X1Y0Z1] = maxLight(lighting, lightCube[0, 0, 1], lightCube[1, 0, 0], lightCube[1, 0, 1], lightCube[0, -1, 0], lightCube[0, -1, 1], lightCube[1, -1, 0], lightCube[1, -1, 1]);
							lightingAround[LightingAround.Pos.X0Y0Z1] = maxLight(lighting, lightCube[0, 0, 1], lightCube[-1, 0, 0], lightCube[-1, 0, 1], lightCube[0, -1, 0], lightCube[0, -1, 1], lightCube[-1, -1, 0], lightCube[-1, -1, 1]);
							lightingAround[LightingAround.Pos.X0Y1Z0] = maxLight(lighting, lightCube[0, 0, -1], lightCube[-1, 0, 0], lightCube[-1, 0, -1], lightCube[0, 1, 0], lightCube[0, 1, -1], lightCube[-1, 1, 0], lightCube[-1, 1, -1]);
							lightingAround[LightingAround.Pos.X1Y1Z0] = maxLight(lighting, lightCube[0, 0, -1], lightCube[1, 0, 0], lightCube[1, 0, -1], lightCube[0, 1, 0], lightCube[0, 1, -1], lightCube[1, 1, 0], lightCube[1, 1, -1]);
							lightingAround[LightingAround.Pos.X1Y1Z1] = maxLight(lighting, lightCube[0, 0, 1], lightCube[1, 0, 0], lightCube[1, 0, 1], lightCube[0, 1, 0], lightCube[0, 1, 1], lightCube[1, 1, 0], lightCube[1, 1, 1]);
							lightingAround[LightingAround.Pos.X0Y1Z1] = maxLight(lighting, lightCube[0, 0, 1], lightCube[-1, 0, 0], lightCube[-1, 0, 1], lightCube[0, 1, 0], lightCube[0, 1, 1], lightCube[-1, 1, 0], lightCube[-1, 1, 1]);
							int facesDrawnFullBitfield = block.shape.getFacesDrawnFullBitfield(blockValue);
							bool isWaterTopDrawn = false;
							Vector3i posI = new Vector3i(i, num2, j);
							if ((facesDrawnFullBitfield & 0x10) == 0)
							{
								DrawSide(BlockFace.North, blockValue, nBlocks, to, temperature, ref isWaterTopDrawn, ref isWaterLayerStarted, zero, _meshes, posI, _worldPos);
							}
							if ((facesDrawnFullBitfield & 4) == 0)
							{
								DrawSide(BlockFace.South, blockValue, nBlocks, to, temperature, ref isWaterTopDrawn, ref isWaterLayerStarted, zero, _meshes, posI, _worldPos);
							}
							if ((facesDrawnFullBitfield & 8) == 0)
							{
								DrawSide(BlockFace.East, blockValue, nBlocks, to, temperature, ref isWaterTopDrawn, ref isWaterLayerStarted, zero, _meshes, posI, _worldPos);
							}
							if ((facesDrawnFullBitfield & 0x20) == 0)
							{
								DrawSide(BlockFace.West, blockValue, nBlocks, to, temperature, ref isWaterTopDrawn, ref isWaterLayerStarted, zero, _meshes, posI, _worldPos);
							}
							if ((facesDrawnFullBitfield & 1) == 0)
							{
								BlockValue blockValue2 = nBlocks.Get(0, num2 + 1, 0);
								Block block2 = blockValue2.Block;
								if (block2 != null && block2.shape.isRenderFace(blockValue2, BlockFace.Bottom, blockValue))
								{
									if (block2.GetLightValue(blockValue2) <= 0)
									{
										_ = VoxelMesh.COLOR_BOTTOM;
									}
									else
									{
										_ = VoxelMesh.COLOR_TOP;
									}
									IChunk chunk = nBlocks.GetChunk(0, 0);
									TextureFullArray textureFullArray = chunk.GetTextureFullArray(i, num2 + 1, j);
									SwizzleCopy(ref to, 2, 1, 5, 6);
									byte stability = chunk.GetStability(i, num2 + 1, j);
									lightingAround.SetStab(stability);
									block2.shape.renderFace(_worldPos, blockValue2, zero + Vector3.up, BlockFace.Bottom, to, lightingAround, textureFullArray, _meshes);
								}
							}
							BlockValue blockValue3 = nBlocks.Get(0, num2 - 1, 0);
							Block block3 = blockValue3.Block;
							if (block3 != null && (facesDrawnFullBitfield & 2) == 0 && block3.shape.isRenderFace(blockValue3, BlockFace.Top, blockValue))
							{
								IChunk chunk2 = nBlocks.GetChunk(0, 0);
								TextureFullArray textureFullArray2 = chunk2.GetTextureFullArray(i, num2 - 1, j);
								SwizzleCopy(ref to, 0, 3, 7, 4);
								byte stability2 = chunk2.GetStability(i, num2 - 1, j);
								lightingAround.SetStab(stability2);
								float num3 = 0f;
								if (block3.shape.IsTerrain() && num2 > 0)
								{
									sbyte density = nBlocks.GetChunk(0, 0).GetDensity(i, num2, j);
									sbyte density2 = nBlocks.GetChunk(0, 0).GetDensity(i, num2 - 1, j);
									num3 = MarchingCubes.GetDecorationOffsetY(density, density2);
								}
								block3.shape.renderFace(_worldPos, blockValue3, zero + new Vector3(0f, num3 - 1f, 0f), BlockFace.Top, to, lightingAround, textureFullArray2, _meshes);
							}
							if (nBlocks.IsWater(0, num2 - 1, 0) && !flag && !isWaterTopDrawn)
							{
								IChunk chunk3 = nBlocks.GetChunk(0, 0);
								SwizzleCopy(ref to, 0, 3, 7, 4);
								byte stability3 = chunk3.GetStability(i, num2 - 1, j);
								lightingAround.SetStab(stability3);
								RenderTopWater(blockValue, to, _meshes, zero2, _worldPos);
							}
							if (block != null && !blockValue.ischild && block.shape.IsRenderDecoration())
							{
								IChunk chunk4 = nBlocks.GetChunk(0, 0);
								float y2 = 0f;
								if (block.IsTerrainDecoration && block3.shape.IsTerrain() && num2 > 0)
								{
									sbyte density3 = chunk4.GetDensity(i, num2, j);
									sbyte density4 = chunk4.GetDensity(i, num2 - 1, j);
									y2 = MarchingCubes.GetDecorationOffsetY(density3, density4);
								}
								for (int m = 0; m < _meshes.Length; m++)
								{
									_meshes[m].SetTemperature(temperature);
								}
								byte stability4 = chunk4.GetStability(i, num2, j);
								lightingAround.SetStab(stability4);
								TextureFullArray textureFullArray3 = chunk4.GetTextureFullArray(i, num2, j);
								block.RenderDecorations(_worldPos, blockValue, zero + new Vector3(0f, y2, 0f), verticesCube, lightingAround, textureFullArray3, _meshes, nBlocks);
							}
						}
					}
				}
			}
		}
	}

	public void Test()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsRaisable(int type)
	{
		if (type != 0)
		{
			if (!IsSolidCube(type))
			{
				return !IsPlant(type);
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsRaisableNeighbor(int _y, int _x, int _z)
	{
		if (!neighborIsWater[_y, _x, _z])
		{
			return IsRaisable(neighborType[_y, _x, _z]);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CanWaterTaper(int type)
	{
		if (type != 0)
		{
			return IsPlant(type);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsSolid(int type)
	{
		if (!IsTerrain(type))
		{
			if (IsSolidCube(type))
			{
				return type != 0;
			}
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAirNeighbor(int _y, int _x, int _z)
	{
		if (IsAir(neighborType[_y, _x, _z]))
		{
			return !neighborIsWater[_y, _x, _z];
		}
		return false;
	}

	[Conditional("NEW_WATER_MESH_DEBUG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WatermeshDebugLog(string message)
	{
		UnityEngine.Debug.Log("[watermesh] " + message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderTopWater(BlockValue _middleBV, Vector3[] v4Arr, VoxelMesh[] _meshes, Vector3i _blockPos, Vector3i _worldPos, bool isInsideTerrain = false)
	{
		_ = _worldPos + _blockPos;
		int y = _blockPos.y;
		int num = (isInsideTerrain ? y : (y - 1));
		Vector3i vector3i = new Vector3i(_blockPos.x, num, _blockPos.z);
		neighborType[0, 1, 1] = 0;
		neighborType[1, 1, 1] = _middleBV.type;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					neighborIsWater[i, k, j] = nBlocks.IsWater(k - 1, num + i, j - 1);
					if ((!isInsideTerrain || j != 1 || k != 1) && (isInsideTerrain || i != 1 || j != 1 || k != 1))
					{
						BlockValue blockValue = nBlocks.Get(k - 1, num + i, j - 1);
						neighborType[i, k, j] = blockValue.type;
					}
				}
			}
		}
		int type = _middleBV.type;
		if (!IsSolidCube(type) && !nBlocks.IsAir(0, y, 0) && !neighborIsWater[1, 1, 1] && (neighborIsWater[1, 0, 1] || neighborIsWater[1, 1, 2] || neighborIsWater[1, 2, 1] || neighborIsWater[1, 1, 0]))
		{
			return;
		}
		bool flag = IsSolid(neighborType[0, 0, 0]) && IsSolid(neighborType[0, 1, 0]) && IsSolid(neighborType[0, 2, 0]) && IsSolid(neighborType[0, 2, 1]) && IsSolid(neighborType[0, 2, 2]) && IsSolid(neighborType[0, 1, 2]) && IsSolid(neighborType[0, 0, 2]) && IsSolid(neighborType[0, 0, 1]);
		BlockValue waterClippingBV = (isInsideTerrain ? _middleBV : nBlocks.Get(0, num, 0));
		bool flag2 = TryPrepareWaterClippingVolume(waterClippingBV);
		using (renderTopWaterOldMarker.Auto())
		{
			for (int l = 0; l < 4; l++)
			{
				for (int m = 0; m < 4; m++)
				{
					cpyVerts[0].x = v4Arr[0].x + (float)l * 0.25f;
					cpyVerts[1].x = cpyVerts[0].x + 0.25f;
					cpyVerts[2].x = cpyVerts[0].x + 0.25f;
					cpyVerts[3].x = v4Arr[0].x + (float)l * 0.25f;
					cpyVerts[0].z = v4Arr[0].z + (float)m * 0.25f;
					cpyVerts[1].z = v4Arr[0].z + (float)m * 0.25f;
					cpyVerts[2].z = cpyVerts[0].z + 0.25f;
					cpyVerts[3].z = cpyVerts[0].z + 0.25f;
					cpyVerts[0].y = v4Arr[0].y + -1.5f;
					cpyVerts[1].y = v4Arr[1].y + -1.5f;
					cpyVerts[2].y = v4Arr[2].y + -1.5f;
					cpyVerts[3].y = v4Arr[3].y + -1.5f;
					bool flag3 = false;
					if (isInsideTerrain && !nBlocks.IsAir(0, y + 1, 0) && IsSolidCube(nBlocks.Get(0, y + 1, 0).type))
					{
						flag3 = true;
						bool flag4 = false;
						bool flag5 = false;
						bool flag6 = false;
						bool flag7 = false;
						switch (l)
						{
						case 0:
							if (m <= 2)
							{
								flag6 = true;
							}
							if (m >= 1)
							{
								flag5 = true;
							}
							switch (m)
							{
							case 3:
								if (!IsAirNeighbor(0, 1, 2) && !IsAirNeighbor(1, 1, 2))
								{
									flag6 = true;
								}
								if ((neighborIsWater[0, 0, 2] && !IsAirNeighbor(1, 0, 2)) || (neighborIsWater[0, 1, 2] && !IsAirNeighbor(1, 1, 2)) || (neighborIsWater[0, 0, 1] && !IsAirNeighbor(1, 0, 1)))
								{
									flag7 = true;
								}
								if (!IsAirNeighbor(0, 0, 1) && !IsAirNeighbor(1, 0, 1))
								{
									flag4 = true;
								}
								break;
							case 0:
								if ((neighborIsWater[0, 0, 0] && !IsAirNeighbor(1, 0, 0)) || (neighborIsWater[0, 1, 0] && !IsAirNeighbor(1, 1, 0)) || (neighborIsWater[0, 0, 1] && !IsAirNeighbor(1, 0, 1)))
								{
									flag4 = true;
								}
								if (!IsAirNeighbor(0, 1, 0) && !IsAirNeighbor(1, 1, 0))
								{
									flag5 = true;
								}
								if (!IsAirNeighbor(0, 0, 1) && !IsAirNeighbor(1, 0, 1))
								{
									flag7 = true;
								}
								break;
							default:
								if (!IsAirNeighbor(0, 0, 1) && !IsAirNeighbor(1, 0, 1))
								{
									flag4 = true;
									flag7 = true;
								}
								if (!IsAirNeighbor(0, 2, 1) && !IsAirNeighbor(1, 2, 1))
								{
									flag5 = true;
									flag6 = true;
								}
								break;
							}
							break;
						case 3:
							if (m <= 2)
							{
								flag7 = true;
							}
							if (m >= 1)
							{
								flag4 = true;
							}
							switch (m)
							{
							case 3:
								if (!IsAirNeighbor(0, 2, 1) && !IsAirNeighbor(1, 2, 1))
								{
									flag5 = true;
								}
								if ((neighborIsWater[0, 2, 2] && !IsAirNeighbor(1, 2, 2)) || (neighborIsWater[0, 1, 2] && !IsAirNeighbor(1, 1, 2)) || (neighborIsWater[0, 2, 1] && !IsAirNeighbor(1, 2, 1)))
								{
									flag6 = true;
								}
								if (!IsAirNeighbor(0, 1, 2) && !IsAirNeighbor(1, 1, 2))
								{
									flag7 = true;
								}
								break;
							case 0:
								if ((neighborIsWater[0, 2, 0] && !IsAirNeighbor(1, 2, 0)) || (neighborIsWater[0, 1, 0] && !IsAirNeighbor(1, 1, 0)) || (neighborIsWater[0, 2, 1] && !IsAirNeighbor(1, 2, 1)))
								{
									flag5 = true;
								}
								if (!IsAirNeighbor(0, 1, 0) && !IsAirNeighbor(1, 1, 0))
								{
									flag4 = true;
								}
								if (!IsAirNeighbor(0, 2, 1) && !IsAirNeighbor(1, 2, 1))
								{
									flag6 = true;
								}
								break;
							default:
								if (!IsAirNeighbor(0, 0, 1) && !IsAirNeighbor(1, 0, 1))
								{
									flag4 = true;
									flag7 = true;
								}
								if (!IsAirNeighbor(0, 2, 1) && !IsAirNeighbor(1, 2, 1))
								{
									flag5 = true;
									flag6 = true;
								}
								break;
							}
							break;
						default:
							if (m <= 2)
							{
								flag6 = true;
								flag7 = true;
							}
							else if (!IsAirNeighbor(0, 1, 2) && !IsAirNeighbor(1, 1, 2))
							{
								flag6 = true;
								flag7 = true;
							}
							if (m >= 1)
							{
								flag4 = true;
								flag5 = true;
							}
							else if (!IsAirNeighbor(0, 1, 0) && !IsAirNeighbor(1, 1, 0))
							{
								flag4 = true;
								flag5 = true;
							}
							break;
						}
						if (flag4)
						{
							cpyVerts[0].y += 0.5f;
						}
						if (flag5)
						{
							cpyVerts[1].y += 0.5f;
						}
						if (flag6)
						{
							cpyVerts[2].y += 0.5f;
						}
						if (flag7)
						{
							cpyVerts[3].y += 0.5f;
						}
					}
					if (!isInsideTerrain && !IsAirNeighbor(1, 1, 1) && IsSolidCube(neighborType[1, 1, 1]))
					{
						flag3 = true;
						if (m == 0)
						{
							if (l == 0)
							{
								if (!IsAirNeighbor(0, 0, 1) && !IsAirNeighbor(0, 0, 0) && !IsAirNeighbor(0, 1, 0))
								{
									cpyVerts[0].y += 0.5f;
								}
								if (!IsAirNeighbor(0, 1, 0))
								{
									cpyVerts[1].y += 0.5f;
								}
								if (!IsAirNeighbor(0, 0, 1))
								{
									cpyVerts[3].y += 0.5f;
								}
								cpyVerts[2].y += 0.5f;
							}
							else if (l <= 2)
							{
								if (!IsAirNeighbor(0, 1, 0))
								{
									cpyVerts[0].y += 0.5f;
									cpyVerts[1].y += 0.5f;
								}
								cpyVerts[2].y += 0.5f;
								cpyVerts[3].y += 0.5f;
							}
							else if (l == 3)
							{
								if (!IsAirNeighbor(0, 1, 0) && !IsAirNeighbor(0, 2, 0) && !IsAirNeighbor(0, 2, 1))
								{
									cpyVerts[1].y += 0.5f;
								}
								if (!IsAirNeighbor(0, 1, 0))
								{
									cpyVerts[0].y += 0.5f;
								}
								if (!IsAirNeighbor(0, 2, 1))
								{
									cpyVerts[2].y += 0.5f;
								}
								cpyVerts[3].y += 0.5f;
							}
						}
						else if (m <= 2)
						{
							if (l == 0)
							{
								if (!IsAirNeighbor(0, 0, 1))
								{
									cpyVerts[0].y += 0.5f;
									cpyVerts[3].y += 0.5f;
								}
								cpyVerts[1].y += 0.5f;
								cpyVerts[2].y += 0.5f;
							}
							else if (l <= 2)
							{
								cpyVerts[0].y += 0.5f;
								cpyVerts[1].y += 0.5f;
								cpyVerts[2].y += 0.5f;
								cpyVerts[3].y += 0.5f;
							}
							else if (l == 3)
							{
								if (!IsAirNeighbor(0, 2, 1))
								{
									cpyVerts[1].y += 0.5f;
									cpyVerts[2].y += 0.5f;
								}
								cpyVerts[0].y += 0.5f;
								cpyVerts[3].y += 0.5f;
							}
						}
						else if (m == 3)
						{
							if (l == 0)
							{
								if (!IsAirNeighbor(0, 0, 1) && !IsAirNeighbor(0, 0, 2) && !IsAirNeighbor(0, 1, 2))
								{
									cpyVerts[3].y += 0.5f;
								}
								if (!IsAirNeighbor(0, 0, 1))
								{
									cpyVerts[0].y += 0.5f;
								}
								if (!IsAirNeighbor(0, 1, 2))
								{
									cpyVerts[2].y += 0.5f;
								}
								cpyVerts[1].y += 0.5f;
							}
							else if (l <= 2)
							{
								if (!IsAirNeighbor(0, 1, 2))
								{
									cpyVerts[2].y += 0.5f;
									cpyVerts[3].y += 0.5f;
								}
								cpyVerts[0].y += 0.5f;
								cpyVerts[1].y += 0.5f;
							}
							else if (l == 3)
							{
								if (!IsAirNeighbor(0, 1, 2) && !IsAirNeighbor(0, 2, 2) && !IsAirNeighbor(0, 2, 1))
								{
									cpyVerts[2].y += 0.5f;
								}
								if (!IsAirNeighbor(0, 1, 2))
								{
									cpyVerts[3].y += 0.5f;
								}
								if (!IsAirNeighbor(0, 2, 1))
								{
									cpyVerts[1].y += 0.5f;
								}
								cpyVerts[0].y += 0.5f;
							}
						}
					}
					if (flag && !isInsideTerrain)
					{
						for (int n = 0; n < 4; n++)
						{
							cpyVerts[n].y -= -1.5f;
						}
						if (l == 0)
						{
							cpyVerts[0].x -= 0.125f;
							cpyVerts[3].x -= 0.125f;
							cpyVerts[0].y -= 0.125f;
							cpyVerts[3].y -= 0.125f;
						}
						if (l == 3)
						{
							cpyVerts[1].x += 0.125f;
							cpyVerts[2].x += 0.125f;
							cpyVerts[1].y -= 0.125f;
							cpyVerts[2].y -= 0.125f;
						}
						if (m == 0)
						{
							cpyVerts[0].z -= 0.125f;
							cpyVerts[1].z -= 0.125f;
							cpyVerts[0].y -= 0.125f;
							cpyVerts[1].y -= 0.125f;
						}
						if (m == 3)
						{
							cpyVerts[2].z += 0.125f;
							cpyVerts[3].z += 0.125f;
							cpyVerts[2].y -= 0.125f;
							cpyVerts[3].y -= 0.125f;
						}
					}
					else
					{
						for (int num2 = 0; num2 < 4; num2++)
						{
							raiseHeight[num2] = 0f;
						}
						IsRaisableNeighbor(0, 0, 0);
						switch (m)
						{
						case 0:
							switch (l)
							{
							case 0:
								if (!flag3)
								{
									if ((neighborIsWater[0, 0, 0] && !IsAirNeighbor(1, 0, 0) && IsSolidCube(neighborType[1, 0, 0])) || (neighborIsWater[0, 0, 1] && !IsAirNeighbor(1, 0, 1) && IsSolidCube(neighborType[1, 0, 1])) || (neighborIsWater[0, 1, 0] && !IsAirNeighbor(1, 1, 0) && IsSolidCube(neighborType[1, 1, 0])))
									{
										cpyVerts[0].y += 0.5f;
									}
									if (neighborIsWater[0, 0, 1] && !IsAirNeighbor(1, 0, 1) && IsSolidCube(neighborType[1, 0, 1]))
									{
										cpyVerts[3].y += 0.5f;
									}
									if (neighborIsWater[0, 1, 0] && !IsAirNeighbor(1, 1, 0) && IsSolidCube(neighborType[1, 1, 0]))
									{
										cpyVerts[1].y += 0.5f;
									}
								}
								raiseHeight[0] += ((IsRaisableNeighbor(0, 0, 0) && IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								raiseHeight[1] += ((IsRaisableNeighbor(0, 0, 0) && IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								raiseHeight[2] += ((IsRaisableNeighbor(0, 0, 0) && IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								raiseHeight[3] += ((IsRaisableNeighbor(0, 0, 0) && IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								break;
							case 1:
								if (!flag3 && neighborIsWater[0, 1, 0] && !IsAirNeighbor(1, 1, 0) && IsSolidCube(neighborType[1, 1, 0]))
								{
									cpyVerts[0].y += 0.5f;
									cpyVerts[1].y += 0.5f;
								}
								raiseHeight[0] += ((IsRaisableNeighbor(0, 0, 0) && IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								raiseHeight[1] += (IsRaisableNeighbor(0, 1, 0) ? 1f : 0f);
								raiseHeight[2] += (IsRaisableNeighbor(0, 1, 0) ? 1f : 0.9f);
								raiseHeight[3] += ((IsRaisableNeighbor(0, 0, 0) && IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								break;
							case 2:
								if (!flag3 && neighborIsWater[0, 1, 0] && !IsAirNeighbor(1, 1, 0) && IsSolidCube(neighborType[1, 1, 0]))
								{
									cpyVerts[0].y += 0.5f;
									cpyVerts[1].y += 0.5f;
								}
								raiseHeight[0] += (IsRaisableNeighbor(0, 1, 0) ? 1f : 0f);
								raiseHeight[1] += ((IsRaisableNeighbor(0, 2, 0) && IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								raiseHeight[2] += ((IsRaisableNeighbor(0, 2, 0) && IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								raiseHeight[3] += (IsRaisableNeighbor(0, 1, 0) ? 1f : 0.9f);
								break;
							case 3:
								if (!flag3)
								{
									if ((neighborIsWater[0, 2, 0] && !IsAirNeighbor(1, 2, 0) && IsSolidCube(neighborType[1, 2, 0])) || (neighborIsWater[0, 2, 1] && !IsAirNeighbor(1, 2, 1) && IsSolidCube(neighborType[1, 2, 1])) || (neighborIsWater[0, 1, 0] && !IsAirNeighbor(1, 1, 0) && IsSolidCube(neighborType[1, 1, 0])))
									{
										cpyVerts[1].y += 0.5f;
									}
									if (neighborIsWater[0, 2, 1] && !IsAirNeighbor(1, 2, 1) && IsSolidCube(neighborType[1, 2, 1]))
									{
										cpyVerts[2].y += 0.5f;
									}
									if (neighborIsWater[0, 1, 0] && !IsAirNeighbor(1, 1, 0) && IsSolidCube(neighborType[1, 1, 0]))
									{
										cpyVerts[0].y += 0.5f;
									}
								}
								raiseHeight[0] += ((IsRaisableNeighbor(0, 2, 0) && IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								raiseHeight[1] += ((IsRaisableNeighbor(0, 2, 0) && IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								raiseHeight[2] += ((IsRaisableNeighbor(0, 2, 0) && IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								raiseHeight[3] += ((IsRaisableNeighbor(0, 2, 0) && IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								break;
							}
							break;
						case 1:
							switch (l)
							{
							case 0:
								if (!flag3 && neighborIsWater[0, 0, 1] && !IsAirNeighbor(1, 0, 1) && IsSolidCube(neighborType[1, 0, 1]))
								{
									cpyVerts[0].y += 0.5f;
									cpyVerts[3].y += 0.5f;
								}
								raiseHeight[0] += ((IsRaisableNeighbor(0, 0, 0) && IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								raiseHeight[1] += ((IsRaisableNeighbor(0, 0, 0) && IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								raiseHeight[2] += (IsRaisableNeighbor(0, 0, 1) ? 1f : 0.9f);
								raiseHeight[3] += (IsRaisableNeighbor(0, 0, 1) ? 1f : 0f);
								break;
							case 1:
								raiseHeight[0] += ((IsRaisableNeighbor(0, 0, 0) && IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								raiseHeight[1] += (IsRaisableNeighbor(0, 1, 0) ? 1f : 0.9f);
								raiseHeight[2] += 1f;
								raiseHeight[3] += (IsRaisableNeighbor(0, 0, 1) ? 1f : 0.9f);
								break;
							case 2:
								raiseHeight[0] += (IsRaisableNeighbor(0, 1, 0) ? 1f : 0.9f);
								raiseHeight[1] += ((IsRaisableNeighbor(0, 2, 0) && IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								raiseHeight[2] += (IsRaisableNeighbor(0, 2, 1) ? 1f : 0.9f);
								raiseHeight[3] += 1f;
								break;
							case 3:
								if (!flag3 && neighborIsWater[0, 2, 1] && !IsAirNeighbor(1, 2, 1) && IsSolidCube(neighborType[1, 2, 1]))
								{
									cpyVerts[1].y += 0.5f;
									cpyVerts[2].y += 0.5f;
								}
								raiseHeight[0] += ((IsRaisableNeighbor(0, 2, 0) && IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								raiseHeight[1] += ((IsRaisableNeighbor(0, 2, 0) && IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								raiseHeight[2] += (IsRaisableNeighbor(0, 2, 1) ? 1f : 0f);
								raiseHeight[3] += (IsRaisableNeighbor(0, 2, 1) ? 1f : 0.9f);
								break;
							}
							break;
						case 2:
							switch (l)
							{
							case 0:
								if (!flag3 && neighborIsWater[0, 0, 1] && !IsAirNeighbor(1, 0, 1) && IsSolidCube(neighborType[1, 0, 1]))
								{
									cpyVerts[0].y += 0.5f;
									cpyVerts[3].y += 0.5f;
								}
								raiseHeight[0] += (IsRaisableNeighbor(0, 0, 1) ? 1f : 0f);
								raiseHeight[1] += (IsRaisableNeighbor(0, 0, 1) ? 1f : 0.9f);
								raiseHeight[2] += ((IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 0, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
								raiseHeight[3] += ((IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 0, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
								break;
							case 1:
								raiseHeight[0] += (IsRaisableNeighbor(0, 0, 1) ? 1f : 0.9f);
								raiseHeight[1] += 1f;
								raiseHeight[2] += (IsRaisableNeighbor(0, 1, 2) ? 1f : 0.9f);
								raiseHeight[3] += ((IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 0, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
								break;
							case 2:
								raiseHeight[0] += 1f;
								raiseHeight[1] += (IsRaisableNeighbor(0, 2, 1) ? 1f : 0.9f);
								raiseHeight[2] += ((IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 2, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
								raiseHeight[3] += (IsRaisableNeighbor(0, 1, 2) ? 1f : 0.9f);
								break;
							case 3:
								if (!flag3 && neighborIsWater[0, 2, 1] && !IsAirNeighbor(1, 2, 1) && IsSolidCube(neighborType[1, 2, 1]))
								{
									cpyVerts[1].y += 0.5f;
									cpyVerts[2].y += 0.5f;
								}
								raiseHeight[0] += (IsRaisableNeighbor(0, 2, 1) ? 1f : 0.9f);
								raiseHeight[1] += (IsRaisableNeighbor(0, 2, 1) ? 1f : 0f);
								raiseHeight[2] += ((IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 2, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
								raiseHeight[3] += ((IsRaisableNeighbor(0, 2, 1) && IsRaisableNeighbor(0, 2, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
								break;
							}
							break;
						case 3:
							switch (l)
							{
							case 0:
								if (!flag3)
								{
									if ((neighborIsWater[0, 0, 2] && !IsAirNeighbor(1, 0, 2) && IsSolidCube(neighborType[1, 0, 2])) || (neighborIsWater[0, 1, 2] && !IsAirNeighbor(1, 1, 2) && IsSolidCube(neighborType[1, 1, 2])) || (neighborIsWater[0, 0, 1] && !IsAirNeighbor(1, 0, 1) && IsSolidCube(neighborType[1, 0, 1])))
									{
										cpyVerts[3].y += 0.5f;
									}
									if (neighborIsWater[0, 1, 2] && !IsAirNeighbor(1, 1, 2) && IsSolidCube(neighborType[1, 1, 2]))
									{
										cpyVerts[2].y += 0.5f;
									}
									if (neighborIsWater[0, 0, 1] && !IsAirNeighbor(1, 0, 1) && IsSolidCube(neighborType[1, 0, 1]))
									{
										cpyVerts[0].y += 0.5f;
									}
								}
								raiseHeight[0] += ((IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 0, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
								raiseHeight[1] += ((IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 0, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
								raiseHeight[2] += ((IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 0, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
								raiseHeight[3] += ((IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 0, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
								break;
							case 1:
								if (!flag3 && neighborIsWater[0, 1, 2] && !IsAirNeighbor(1, 1, 2) && IsSolidCube(neighborType[1, 1, 2]))
								{
									cpyVerts[2].y += 0.5f;
									cpyVerts[3].y += 0.5f;
								}
								raiseHeight[0] += ((IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 0, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
								raiseHeight[1] += (IsRaisableNeighbor(0, 1, 2) ? 1f : 0.9f);
								raiseHeight[2] += (IsRaisableNeighbor(0, 1, 2) ? 1f : 0f);
								raiseHeight[3] += ((IsRaisableNeighbor(0, 0, 1) && IsRaisableNeighbor(0, 0, 2) && IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
								break;
							case 2:
								if (!flag3 && neighborIsWater[0, 1, 2] && !IsAirNeighbor(1, 1, 2) && IsSolidCube(neighborType[1, 1, 2]))
								{
									cpyVerts[2].y += 0.5f;
									cpyVerts[3].y += 0.5f;
								}
								raiseHeight[0] += (IsRaisableNeighbor(0, 1, 2) ? 1f : 0.9f);
								raiseHeight[1] += ((IsRaisableNeighbor(0, 1, 2) && IsRaisableNeighbor(0, 2, 2) && IsRaisableNeighbor(0, 2, 1)) ? 1f : 0.796f);
								raiseHeight[2] += ((IsRaisableNeighbor(0, 1, 2) && IsRaisableNeighbor(0, 2, 2) && IsRaisableNeighbor(0, 2, 1)) ? 1f : 0f);
								raiseHeight[3] += (IsRaisableNeighbor(0, 1, 2) ? 1f : 0f);
								break;
							case 3:
								if (!flag3)
								{
									if ((neighborIsWater[0, 2, 2] && !IsAirNeighbor(1, 2, 2) && IsSolidCube(neighborType[1, 2, 2])) || (neighborIsWater[0, 1, 2] && !IsAirNeighbor(1, 1, 2) && IsSolidCube(neighborType[1, 1, 2])) || (neighborIsWater[0, 2, 1] && !IsAirNeighbor(1, 2, 1) && IsSolidCube(neighborType[1, 2, 1])))
									{
										cpyVerts[2].y += 0.5f;
									}
									if (neighborIsWater[0, 1, 2] && !IsAirNeighbor(1, 1, 2) && IsSolidCube(neighborType[1, 1, 2]))
									{
										cpyVerts[3].y += 0.5f;
									}
									if (neighborIsWater[0, 2, 1] && !IsAirNeighbor(1, 2, 1) && IsSolidCube(neighborType[1, 2, 1]))
									{
										cpyVerts[1].y += 0.5f;
									}
								}
								raiseHeight[0] += ((IsRaisableNeighbor(0, 1, 2) && IsRaisableNeighbor(0, 2, 2) && IsRaisableNeighbor(0, 2, 1)) ? 1f : 0.796f);
								raiseHeight[1] += ((IsRaisableNeighbor(0, 1, 2) && IsRaisableNeighbor(0, 2, 2) && IsRaisableNeighbor(0, 2, 1)) ? 1f : 0f);
								raiseHeight[2] += ((IsRaisableNeighbor(0, 1, 2) && IsRaisableNeighbor(0, 2, 2) && IsRaisableNeighbor(0, 2, 1)) ? 1f : 0f);
								raiseHeight[3] += ((IsRaisableNeighbor(0, 1, 2) && IsRaisableNeighbor(0, 2, 2) && IsRaisableNeighbor(0, 2, 1)) ? 1f : 0f);
								break;
							}
							break;
						}
						switch (l)
						{
						case 0:
							if (neighborIsWater[1, 0, 1])
							{
								raiseHeight[0] = (raiseHeight[3] = 1f);
							}
							if (m == 0)
							{
								if (neighborIsWater[1, 0, 0])
								{
									raiseHeight[0] = 1f;
								}
								if (neighborIsWater[1, 1, 0])
								{
									raiseHeight[0] = (raiseHeight[1] = 1f);
								}
							}
							if (m == 3)
							{
								if (neighborIsWater[1, 0, 2])
								{
									raiseHeight[3] = 1f;
								}
								if (neighborIsWater[1, 1, 2])
								{
									raiseHeight[2] = (raiseHeight[3] = 1f);
								}
							}
							break;
						case 3:
							if (neighborIsWater[1, 2, 1])
							{
								raiseHeight[1] = (raiseHeight[2] = 1f);
							}
							if (m == 0)
							{
								if (neighborIsWater[1, 2, 0])
								{
									raiseHeight[1] = 1f;
								}
								if (neighborIsWater[1, 1, 0])
								{
									raiseHeight[0] = (raiseHeight[1] = 1f);
								}
							}
							if (m == 3)
							{
								if (neighborIsWater[1, 2, 2])
								{
									raiseHeight[2] = 1f;
								}
								if (neighborIsWater[1, 1, 2])
								{
									raiseHeight[2] = (raiseHeight[3] = 1f);
								}
							}
							break;
						default:
							switch (m)
							{
							case 0:
								if (neighborIsWater[1, 1, 0])
								{
									raiseHeight[0] = (raiseHeight[1] = 1f);
								}
								break;
							case 3:
								if (neighborIsWater[1, 1, 2])
								{
									raiseHeight[2] = (raiseHeight[3] = 1f);
								}
								break;
							}
							break;
						}
						dropY[0] = (dropY[1] = (dropY[2] = (dropY[3] = 0f)));
						for (int num3 = 0; num3 < 4; num3++)
						{
							cpyVerts[num3].y += raiseHeight[num3] - dropY[num3] * raiseHeight[num3];
						}
					}
					if (flag2)
					{
						for (int num4 = 0; num4 < 4; num4++)
						{
							Vector3 vertLocalPos = cpyVerts[num4] - vector3i;
							waterClippingVolume.ApplyClipping(ref vertLocalPos);
							cpyVerts[num4] = vertLocalPos + vector3i;
						}
					}
					WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, new Vector2(0f, 0f));
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool TryPrepareWaterClippingVolume(BlockValue _waterClippingBV)
		{
			if (!_waterClippingBV.Block.WaterClipEnabled)
			{
				return false;
			}
			Plane plane = _waterClippingBV.Block.WaterClipPlane;
			Quaternion rotationStatic = BlockShapeNew.GetRotationStatic(_waterClippingBV.rotation);
			GeometryUtils.RotatePlaneAroundPoint(ref plane, WaterClippingUtils.CubeBounds.center, rotationStatic);
			bool flag8 = neighborIsWater[0, 1, 2];
			bool flag9 = neighborIsWater[0, 2, 1];
			bool flag10 = neighborIsWater[0, 1, 0];
			bool flag11 = neighborIsWater[0, 0, 1];
			if (flag8 && flag9 && flag10 && flag11)
			{
				return false;
			}
			bool num5 = neighborIsWater[0, 1, 1];
			bool flag12 = false;
			bool flag13 = false;
			if (num5)
			{
				if (plane.GetDistanceToPoint(new Vector3(0.5f, 0.5f, 0.5f)) > 0f)
				{
					flag12 = true;
				}
				else
				{
					flag13 = true;
				}
			}
			if (flag8)
			{
				if (plane.GetDistanceToPoint(new Vector3(0.5f, 0.5f, 1f)) > 0f)
				{
					flag12 = true;
				}
				else
				{
					flag13 = true;
				}
			}
			if (flag9)
			{
				if (plane.GetDistanceToPoint(new Vector3(1f, 0.5f, 0.5f)) > 0f)
				{
					flag12 = true;
				}
				else
				{
					flag13 = true;
				}
			}
			if (flag10)
			{
				if (plane.GetDistanceToPoint(new Vector3(0.5f, 0.5f, 0f)) > 0f)
				{
					flag12 = true;
				}
				else
				{
					flag13 = true;
				}
			}
			if (flag11)
			{
				if (plane.GetDistanceToPoint(new Vector3(0f, 0.5f, 0.5f)) > 0f)
				{
					flag12 = true;
				}
				else
				{
					flag13 = true;
				}
			}
			if (flag12 == flag13)
			{
				return false;
			}
			if (flag12)
			{
				plane.Flip();
			}
			waterClippingVolume.Prepare(plane);
			return true;
		}
	}
}

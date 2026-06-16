using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine;

public class MeshGenerator : IMeshGenerator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct DrawSideParams
	{
		public BlockFace face;

		public BlockValue midBV;

		public INeighborBlockCache nBlocks;

		public Vector3[] v4Arr;

		public float biomeTemperature;

		public bool isWaterTopDrawn;

		public bool isWaterLayerStarted;

		public Vector3 drawPos;

		public VoxelMesh[] _meshes;

		public Vector3i posI;

		public Vector3i _worldPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
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
	public static readonly ProfilerMarker drawSideMarker = new ProfilerMarker("DrawSide");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker sboMarker = new ProfilerMarker("sbo");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker blockGetMarker = new ProfilerMarker("blockGet");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker stabMarker = new ProfilerMarker("Stab");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker isWaterMarker = new ProfilerMarker("IsWater");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker swizzleCopyMarker = new ProfilerMarker("SwizzleCopy");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker isRenderFaceMarker = new ProfilerMarker("isRenderFace");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker getChunkTexMarker = new ProfilerMarker("GetChunkTex");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker renderFaceMarker = new ProfilerMarker("renderFace");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker waterMarker = new ProfilerMarker("water");

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ringHeight1 = 0.9f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ringHeight1c = 0.796f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ringHeight2 = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[,,] neighborType = new int[2, 3, 3];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[,,] neighborIsWater = new bool[2, 3, 3];

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterClippingVolume waterClippingVolume = new WaterClippingVolume();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVertRes = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector3[] vertexPositions = new Vector3[25];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] cRingHeights = new float[25]
	{
		0f, 0f, 0f, 0f, 0f, 0f, 0.796f, 0.9f, 0.796f, 0f,
		0f, 0.9f, 1f, 0.9f, 0f, 0f, 0.796f, 0.9f, 0.796f, 0f,
		0f, 0f, 0f, 0f, 0f
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker renderTopWaterNewMarker = new ProfilerMarker("MeshGenerator.RenderTopWaterNew");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker raisableSidesAndCornersMarker = new ProfilerMarker("MeshGenerator.RaisableSidesAndCorners");

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockFaceFlag[,] neighborFlags = new BlockFaceFlag[3, 3];

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

	public void GenerateMesh(Vector3i _worldStartPos, Vector3i _worldEndPos, VoxelMesh[] _meshes)
	{
		CreateMesh(Vector3i.zero, Vector3.zero, _worldStartPos, _worldEndPos, _meshes, _bCalcAmbientLight: true, _bOnlyDistortVertices: false);
		for (int i = 0; i < _meshes.Length; i++)
		{
			_meshes[i].Finished();
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	public void DrawSide(ref DrawSideParams p)
	{
		int x = p.posI.x;
		int y = p.posI.y;
		int z = p.posI.z;
		int num = 0;
		int num2 = 0;
		switch (p.face)
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
		bool flag = nBlocks.IsWater(num, y, num2) && FacePermitsFlow(blockValue, p.face);
		if (flag && (IsTerrain(p.midBV.type) || (!IsSolidCube(p.midBV.type) && !IsPlant(p.midBV.type) && !p.midBV.isair)))
		{
			bool isWaterLayerStarted = p.isWaterLayerStarted;
			p.isWaterTopDrawn = true;
			p.isWaterLayerStarted = true;
			if (!isWaterLayerStarted && !nBlocks.IsWater(num, y + 1, num2))
			{
				SwizzleCopy(ref p.v4Arr, 1, 2, 6, 5);
				RenderTopWater(p.midBV, p.v4Arr, p._meshes, p.posI, p._worldPos, isInsideTerrain: true);
			}
		}
		switch (p.face)
		{
		case BlockFace.North:
			SwizzleCopy(ref p.v4Arr, 3, 0, 1, 2);
			break;
		case BlockFace.South:
			SwizzleCopy(ref p.v4Arr, 4, 7, 6, 5);
			break;
		case BlockFace.East:
			SwizzleCopy(ref p.v4Arr, 0, 4, 5, 1);
			break;
		case BlockFace.West:
			SwizzleCopy(ref p.v4Arr, 7, 3, 2, 6);
			break;
		}
		if (block.shape.isRenderFace(blockValue, p.face, p.midBV))
		{
			Vector3 drawPos = p.drawPos + Vector3.forward;
			switch (p.face)
			{
			case BlockFace.North:
				drawPos = p.drawPos - Vector3.forward;
				break;
			case BlockFace.South:
				drawPos = p.drawPos + Vector3.forward;
				break;
			case BlockFace.East:
				drawPos = p.drawPos - Vector3.right;
				break;
			case BlockFace.West:
				drawPos = p.drawPos + Vector3.right;
				break;
			}
			IChunk chunk = nBlocks.GetChunk(num, num2);
			int x2 = World.toBlockXZ(x + num);
			int z2 = World.toBlockXZ(z + num2);
			TextureFullArray textureFullArray = chunk.GetTextureFullArray(x2, y, z2);
			block.shape.renderFace(p._worldPos, blockValue, drawPos, p.face, p.v4Arr, lightingAround, textureFullArray, p._meshes);
		}
		if (!flag || p.isWaterLayerStarted)
		{
			return;
		}
		bool num3 = nBlocks.IsWater(num, y + 1, num2);
		Block block2 = nBlocks.Get(num, y + 1, num2).Block;
		if (num3 || block2.shape.IsTerrain())
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					switch (p.face)
					{
					case BlockFace.North:
						RenderLiquidFaceNorth(p.v4Arr, p._meshes, i, j);
						break;
					case BlockFace.South:
						RenderLiquidFaceSouth(p.v4Arr, p._meshes, i, j);
						break;
					case BlockFace.East:
						RenderLiquidFaceEast(p.v4Arr, p._meshes, i, j);
						break;
					case BlockFace.West:
						RenderLiquidFaceWest(p.v4Arr, p._meshes, i, j);
						break;
					}
				}
			}
		}
		else
		{
			switch (p.face)
			{
			case BlockFace.North:
				RenderLiquidTriNorth(p.v4Arr, p._meshes, y);
				break;
			case BlockFace.South:
				RenderLiquidTriSouth(p.v4Arr, p._meshes, y);
				break;
			case BlockFace.East:
				RenderLiquidTriEast(p.v4Arr, p._meshes, y);
				break;
			case BlockFace.West:
				RenderLiquidTriWest(p.v4Arr, p._meshes, y);
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool FacePermitsFlow(BlockValue bv, BlockFace worldspaceFace)
	{
		return (bv.rotatedWaterFlowMask & BlockFaceFlags.FromBlockFace(worldspaceFace)) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		DrawSideParams p = default(DrawSideParams);
		p._meshes = _meshes;
		p.nBlocks = nBlocks;
		p.v4Arr = to;
		p._worldPos = _worldPos;
		for (int i = _start.x; i <= _end.x; i++)
		{
			p.posI.x = i;
			for (int j = _start.z; j <= _end.z; j++)
			{
				p.posI.z = j;
				float temperature = VoxelMesh.GetTemperature(world.GetBiome(_worldPos.x + i, _worldPos.z + j));
				nBlocks.Init(i, j);
				int num = nBlocks.GetChunk(0, 0).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i, j));
				heights[1] = nBlocks.GetChunk(1, 0).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i + 1, j));
				heights[2] = nBlocks.GetChunk(0, -1).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i, j - 1));
				heights[3] = nBlocks.GetChunk(-1, 0).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i - 1, j));
				heights[4] = nBlocks.GetChunk(0, 1).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i, j + 1));
				for (int k = 1; k < 5; k++)
				{
					if (num < heights[k])
					{
						num = heights[k];
					}
				}
				num++;
				int v = Utils.FastMin(_end.y, num + 1);
				v = Utils.FastMin(254, v);
				p.biomeTemperature = temperature;
				p.isWaterLayerStarted = false;
				int y = _start.y;
				for (int num2 = v; num2 >= y; num2--)
				{
					BlockValue blockValue = nBlocks.Get(num2);
					Block block = blockValue.Block;
					if (block != null && (blockValue.ischild || !block.shape.IsSolidCube || block.shape.IsTerrain() || (num2 > 0 && nBlocks.IsWater(0, num2 - 1, 0) && !nBlocks.IsWater(-1, num2, 0) && !nBlocks.IsWater(1, num2, 0) && !nBlocks.IsWater(0, num2, -1) && !nBlocks.IsWater(0, num2, 1))))
					{
						bool flag = nBlocks.IsWater(0, num2, 0);
						if (!p.isWaterLayerStarted && flag)
						{
							p.isWaterLayerStarted = true;
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
							Lighting lL1N = lightCube[-1, 0, 0];
							Lighting lL1N2 = lightCube[1, 0, 0];
							Lighting lL = lightCube[0, -1, 0];
							Lighting lL2 = lightCube[0, 1, 0];
							Lighting lL1N3 = lightCube[0, 0, -1];
							Lighting lL1N4 = lightCube[0, 0, 1];
							Lighting lL1C = lightCube[-1, 0, -1];
							Lighting lL1C2 = lightCube[1, 0, -1];
							Lighting lL1C3 = lightCube[-1, 0, 1];
							Lighting lL1C4 = lightCube[1, 0, 1];
							lightingAround[LightingAround.Pos.Middle] = lighting;
							lightingAround[LightingAround.Pos.X0Y0Z0] = maxLight(lighting, lL1N3, lL1N, lL1C, lL, lightCube[0, -1, -1], lightCube[-1, -1, 0], lightCube[-1, -1, -1]);
							lightingAround[LightingAround.Pos.X1Y0Z0] = maxLight(lighting, lL1N3, lL1N2, lL1C2, lL, lightCube[0, -1, -1], lightCube[1, -1, 0], lightCube[1, -1, -1]);
							lightingAround[LightingAround.Pos.X1Y0Z1] = maxLight(lighting, lL1N4, lL1N2, lL1C4, lL, lightCube[0, -1, 1], lightCube[1, -1, 0], lightCube[1, -1, 1]);
							lightingAround[LightingAround.Pos.X0Y0Z1] = maxLight(lighting, lL1N4, lL1N, lL1C3, lL, lightCube[0, -1, 1], lightCube[-1, -1, 0], lightCube[-1, -1, 1]);
							lightingAround[LightingAround.Pos.X0Y1Z0] = maxLight(lighting, lL1N3, lL1N, lL1C, lL2, lightCube[0, 1, -1], lightCube[-1, 1, 0], lightCube[-1, 1, -1]);
							lightingAround[LightingAround.Pos.X1Y1Z0] = maxLight(lighting, lL1N3, lL1N2, lL1C2, lL2, lightCube[0, 1, -1], lightCube[1, 1, 0], lightCube[1, 1, -1]);
							lightingAround[LightingAround.Pos.X1Y1Z1] = maxLight(lighting, lL1N4, lL1N2, lL1C4, lL2, lightCube[0, 1, 1], lightCube[1, 1, 0], lightCube[1, 1, 1]);
							lightingAround[LightingAround.Pos.X0Y1Z1] = maxLight(lighting, lL1N4, lL1N, lL1C3, lL2, lightCube[0, 1, 1], lightCube[-1, 1, 0], lightCube[-1, 1, 1]);
							int facesDrawnFullBitfield = block.shape.getFacesDrawnFullBitfield(blockValue);
							p.midBV = blockValue;
							p.isWaterTopDrawn = false;
							p.drawPos = zero;
							p.posI.y = num2;
							if ((facesDrawnFullBitfield & 0x10) == 0)
							{
								p.face = BlockFace.North;
								DrawSide(ref p);
							}
							if ((facesDrawnFullBitfield & 4) == 0)
							{
								p.face = BlockFace.South;
								DrawSide(ref p);
							}
							if ((facesDrawnFullBitfield & 8) == 0)
							{
								p.face = BlockFace.East;
								DrawSide(ref p);
							}
							if ((facesDrawnFullBitfield & 0x20) == 0)
							{
								p.face = BlockFace.West;
								DrawSide(ref p);
							}
							if ((facesDrawnFullBitfield & 1) == 0)
							{
								BlockValue blockValue2 = nBlocks.Get(num2 + 1);
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
							BlockValue blockValue3 = nBlocks.Get(num2 - 1);
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
							if (nBlocks.IsWater(0, num2 - 1, 0) && !flag && !p.isWaterTopDrawn)
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
					BlockValue blockValue = nBlocks.Get(k - 1, num + i, j - 1);
					neighborType[i, k, j] = blockValue.type;
					if (i == 0)
					{
						neighborFlags[k, j] = blockValue.rotatedWaterFlowMask;
					}
				}
			}
		}
		int type = _middleBV.type;
		if (!IsSolidCube(type) && !nBlocks.IsAir(0, y, 0) && !neighborIsWater[1, 1, 1] && (neighborIsWater[1, 0, 1] || neighborIsWater[1, 1, 2] || neighborIsWater[1, 2, 1] || neighborIsWater[1, 1, 0]))
		{
			return;
		}
		if (IsSolid(neighborType[0, 0, 0]) && IsSolid(neighborType[0, 1, 0]) && IsSolid(neighborType[0, 2, 0]) && IsSolid(neighborType[0, 2, 1]) && IsSolid(neighborType[0, 2, 2]) && IsSolid(neighborType[0, 1, 2]) && IsSolid(neighborType[0, 0, 2]))
		{
			IsSolid(neighborType[0, 0, 1]);
		}
		else
			_ = 0;
		BlockValue waterClippingBV = (isInsideTerrain ? _middleBV : nBlocks.Get(0, num, 0));
		bool flag = TryPrepareWaterClippingVolume(waterClippingBV);
		using (renderTopWaterNewMarker.Auto())
		{
			for (int l = 0; l < vertexPositions.Length; l++)
			{
				int num2 = l % 5;
				int num3 = l / 5;
				vertexPositions[l] = v4Arr[0] + new Vector3((float)num2 * 0.25f, -1.5f, (float)num3 * 0.25f);
			}
			bool flag2 = neighborIsWater[1, 1, 0];
			bool flag3 = neighborIsWater[1, 2, 1];
			bool flag4 = neighborIsWater[1, 1, 2];
			bool flag5 = neighborIsWater[1, 0, 1];
			bool flag10;
			bool flag11;
			bool flag12;
			bool flag13;
			bool flag14;
			bool flag15;
			bool flag16;
			bool flag17;
			using (raisableSidesAndCornersMarker.Auto())
			{
				if ((waterClippingBV.rotatedWaterFlowMask & BlockFaceFlag.Axials) == BlockFaceFlag.Axials)
				{
					bool flag6 = NeighborIsVisualWater(1, 0, -1, 0);
					bool flag7 = NeighborIsVisualWater(2, 1, 0, -1);
					bool flag8 = NeighborIsVisualWater(1, 2, 1, 0);
					bool flag9 = NeighborIsVisualWater(0, 1, 0, 1);
					flag10 = (flag6 && CheckNeighborHasRaisableCorner(1, 0, BlockFace.West, BlockFace.North)) || (flag9 && CheckNeighborHasRaisableCorner(0, 1, BlockFace.East, BlockFace.South));
					flag11 = (flag6 && CheckNeighborHasRaisableCorner(1, 0, BlockFace.East, BlockFace.North)) || (flag7 && CheckNeighborHasRaisableCorner(2, 1, BlockFace.West, BlockFace.South));
					flag12 = (flag8 && CheckNeighborHasRaisableCorner(1, 2, BlockFace.West, BlockFace.South)) || (flag9 && CheckNeighborHasRaisableCorner(0, 1, BlockFace.East, BlockFace.North));
					flag13 = (flag8 && CheckNeighborHasRaisableCorner(1, 2, BlockFace.East, BlockFace.South)) || (flag7 && CheckNeighborHasRaisableCorner(2, 1, BlockFace.West, BlockFace.North));
					flag14 = flag6 || (flag10 && flag11);
					flag15 = flag7 || (flag11 && flag13);
					flag16 = flag8 || (flag13 && flag12);
					flag17 = flag9 || (flag12 && flag10);
				}
				else
				{
					bool isTerrain = IsTerrain(waterClippingBV.type);
					bool flag18 = CheckSolidWall(1, 1, isTerrain, BlockFace.South);
					bool flag19 = CheckSolidWall(1, 1, isTerrain, BlockFace.East);
					bool flag20 = CheckSolidWall(1, 1, isTerrain, BlockFace.North);
					bool flag21 = CheckSolidWall(1, 1, isTerrain, BlockFace.West);
					bool centerIsWater = neighborIsWater[0, 1, 1];
					flag14 = flag18 || CheckRaisableSide(centerIsWater, 1, 0, neighborIsWater[0, 0, 0] || neighborIsWater[0, 2, 0]);
					flag15 = flag19 || CheckRaisableSide(centerIsWater, 2, 1, neighborIsWater[0, 2, 0] || neighborIsWater[0, 2, 2]);
					flag16 = flag20 || CheckRaisableSide(centerIsWater, 1, 2, neighborIsWater[0, 0, 2] || neighborIsWater[0, 2, 2]);
					flag17 = flag21 || CheckRaisableSide(centerIsWater, 0, 1, neighborIsWater[0, 0, 0] || neighborIsWater[0, 0, 2]);
					flag10 = flag14 && flag17 && CheckRaisableCorner(1, 1, BlockFace.West, BlockFace.South, flag21, flag18);
					flag11 = flag14 && flag15 && CheckRaisableCorner(1, 1, BlockFace.East, BlockFace.South, flag19, flag18);
					flag12 = flag16 && flag17 && CheckRaisableCorner(1, 1, BlockFace.West, BlockFace.North, flag21, flag20);
					flag13 = flag16 && flag15 && CheckRaisableCorner(1, 1, BlockFace.East, BlockFace.North, flag19, flag20);
				}
			}
			bool flag22 = true;
			for (int m = 0; m < vertexPositions.Length; m++)
			{
				int num4 = m % 5;
				int num5 = m / 5;
				bool flag23 = num5 == 0;
				bool flag24 = num5 == 4;
				bool flag25 = num4 == 0;
				bool flag26 = num4 == 4;
				if ((flag23 && flag2) || (flag26 && flag3) || (flag24 && flag4) || (flag25 && flag5))
				{
					vertexPositions[m].y += 1f;
					continue;
				}
				bool flag27 = num5 < 2;
				bool flag28 = num5 >= 3;
				bool flag29 = num4 < 2;
				bool flag30 = num4 >= 3;
				bool num6 = !(flag27 || flag28 || flag29 || flag30);
				bool flag31 = flag27 && !(flag29 || flag30);
				bool flag32 = flag28 && !(flag29 || flag30);
				bool flag33 = flag29 && !(flag27 || flag28);
				bool flag34 = flag30 && !(flag27 || flag28);
				bool flag35 = (num6 && flag22) || (flag31 && flag14) || (flag34 && flag15) || (flag32 && flag16) || (flag33 && flag17) || (flag27 && flag29 && flag10) || (flag27 && flag30 && flag11) || (flag28 && flag29 && flag12) || (flag28 && flag30 && flag13);
				vertexPositions[m].y += (flag35 ? 1f : cRingHeights[m]);
			}
			if (flag)
			{
				for (int n = 0; n < vertexPositions.Length; n++)
				{
					Vector3 vertLocalPos = vertexPositions[n] - vector3i;
					waterClippingVolume.ApplyClipping(ref vertLocalPos);
					vertexPositions[n] = vertLocalPos + vector3i;
				}
			}
			for (int num7 = 0; num7 < 4; num7++)
			{
				for (int num8 = 0; num8 < 4; num8++)
				{
					int num9 = num7 + num8 * 5;
					cpyVerts[0] = vertexPositions[num9];
					cpyVerts[1] = vertexPositions[num9 + 1];
					cpyVerts[2] = vertexPositions[num9 + 1 + 5];
					cpyVerts[3] = vertexPositions[num9 + 5];
					bool alternateWinding = num7 < 2 == num8 < 2;
					WaterMeshUtils.RenderFace(cpyVerts, lightingAround, 0L, _meshes, new Vector2(0f, 0f), alternateWinding);
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool CheckNeighborHasRaisableCorner(int x, int z, BlockFace xFace, BlockFace zFace)
		{
			int num10 = ((xFace == BlockFace.East) ? 1 : (-1));
			int num11 = ((zFace == BlockFace.North) ? 1 : (-1));
			int num12 = x + num10;
			int num13 = z + num11;
			BlockFace face = BlockFaceFlags.OppositeFace(xFace);
			BlockFace face2 = BlockFaceFlags.OppositeFace(zFace);
			if (neighborIsWater[0, num12, num13])
			{
				if ((IsRaisableNeighbor(0, x, num13) || NeighborBlocksFlow(x, num13, xFace) || NeighborBlocksFlow(num12, num13, face)) && (IsRaisableNeighbor(0, num12, z) || NeighborBlocksFlow(num12, z, zFace) || NeighborBlocksFlow(num12, num13, face2)))
				{
					return true;
				}
				return false;
			}
			if (IsRaisable(neighborType[0, num12, num13]))
			{
				return true;
			}
			bool isTerrain2 = IsTerrain(neighborType[0, x, z]);
			if ((CheckSolidWall(x, z, isTerrain2, zFace) || NeighborBlocksFlow(x, num13, xFace) || NeighborBlocksFlow(num12, num13, face)) && (CheckSolidWall(x, z, isTerrain2, xFace) || NeighborBlocksFlow(num12, z, zFace) || NeighborBlocksFlow(num12, num13, face2)))
			{
				return true;
			}
			return false;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool CheckRaisableCorner(int x, int z, BlockFace xFace, BlockFace zFace, bool solidWallX, bool solidWallZ)
		{
			int num10 = ((xFace == BlockFace.East) ? 1 : (-1));
			int num11 = ((zFace == BlockFace.North) ? 1 : (-1));
			int num12 = x + num10;
			int num13 = z + num11;
			BlockFace face = BlockFaceFlags.OppositeFace(xFace);
			BlockFace face2 = BlockFaceFlags.OppositeFace(zFace);
			if (neighborIsWater[0, num12, num13])
			{
				if ((IsRaisableNeighbor(0, x, num13) || NeighborBlocksFlow(x, num13, xFace) || NeighborBlocksFlow(num12, num13, face)) && (IsRaisableNeighbor(0, num12, z) || NeighborBlocksFlow(num12, z, zFace) || NeighborBlocksFlow(num12, num13, face2)))
				{
					return true;
				}
			}
			else
			{
				if (IsRaisable(neighborType[0, num12, num13]))
				{
					return true;
				}
				if ((solidWallZ || NeighborBlocksFlow(x, num13, xFace) || NeighborBlocksFlow(num12, num13, face)) && (solidWallX || NeighborBlocksFlow(num12, z, zFace) || NeighborBlocksFlow(num12, num13, face2)))
				{
					return true;
				}
			}
			return false;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool CheckRaisableSide(bool flag36, int x, int z, bool otherNeighborIsWater)
		{
			if (flag36)
			{
				return IsRaisableNeighbor(0, x, z);
			}
			if (neighborIsWater[0, x, z])
			{
				return true;
			}
			return IsRaisable(neighborType[0, x, z]) && otherNeighborIsWater;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool CheckSolidWall(int x, int z, bool flag36, BlockFace blockFace)
		{
			if (!flag36 && !FacePermitsFlow(neighborFlags[x, z], blockFace))
			{
				return true;
			}
			Vector3i vector3i2 = BlockFaceFlags.OffsetIForFace(blockFace);
			BlockFace face = BlockFaceFlags.OppositeFace(blockFace);
			if (NeighborBlocksFlow(x + vector3i2.x, z + vector3i2.z, face))
			{
				return true;
			}
			return false;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool NeighborBlocksFlow(int x, int z, BlockFace face)
		{
			if (!IsTerrain(neighborType[0, x, z]))
			{
				return !FacePermitsFlow(neighborFlags[x, z], face);
			}
			return false;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool NeighborIsVisualWater(int x, int z, int offsetX, int offsetZ)
		{
			if (neighborIsWater[0, x, z])
			{
				return true;
			}
			if (IsRaisable(neighborType[0, x, z]) && (neighborIsWater[0, x + offsetX, z + offsetZ] || neighborIsWater[0, x - offsetX, z - offsetZ] || neighborIsWater[0, 1, 1]))
			{
				return true;
			}
			return false;
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
			bool flag36 = neighborIsWater[0, 1, 2];
			bool flag37 = neighborIsWater[0, 2, 1];
			bool flag38 = neighborIsWater[0, 1, 0];
			bool flag39 = neighborIsWater[0, 0, 1];
			if (flag36 && flag37 && flag38 && flag39)
			{
				return false;
			}
			bool num10 = neighborIsWater[0, 1, 1];
			bool flag40 = false;
			bool flag41 = false;
			if (num10)
			{
				if (plane.GetDistanceToPoint(new Vector3(0.5f, 0.5f, 0.5f)) > 0f)
				{
					flag40 = true;
				}
				else
				{
					flag41 = true;
				}
			}
			if (flag36)
			{
				if (plane.GetDistanceToPoint(new Vector3(0.5f, 0.5f, 1f)) > 0f)
				{
					flag40 = true;
				}
				else
				{
					flag41 = true;
				}
			}
			if (flag37)
			{
				if (plane.GetDistanceToPoint(new Vector3(1f, 0.5f, 0.5f)) > 0f)
				{
					flag40 = true;
				}
				else
				{
					flag41 = true;
				}
			}
			if (flag38)
			{
				if (plane.GetDistanceToPoint(new Vector3(0.5f, 0.5f, 0f)) > 0f)
				{
					flag40 = true;
				}
				else
				{
					flag41 = true;
				}
			}
			if (flag39)
			{
				if (plane.GetDistanceToPoint(new Vector3(0f, 0.5f, 0.5f)) > 0f)
				{
					flag40 = true;
				}
				else
				{
					flag41 = true;
				}
			}
			if (flag40 == flag41)
			{
				return false;
			}
			if (flag40)
			{
				plane.Flip();
			}
			waterClippingVolume.Prepare(plane);
			return true;
		}
	}
}

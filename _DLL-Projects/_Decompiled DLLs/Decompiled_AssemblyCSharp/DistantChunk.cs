using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DistantChunk
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class WaterMesh
	{
		public Vector3[] Vertices;

		public Vector3[] Normals;

		public Vector4[] Tangents;

		public Vector2[] UVVectors;

		public int[] Triangles;

		public Color[] Colors;

		public Bounds Bound;

		public WaterMesh(int NbVertices, int NbTriangles, float Size)
		{
			Init(NbVertices, NbTriangles, Size);
		}

		public void Init(int NbVertices, int NbTriangles, float Size)
		{
			Vertices = new Vector3[NbVertices];
			Normals = new Vector3[NbVertices];
			Tangents = new Vector4[NbVertices];
			UVVectors = new Vector2[NbVertices];
			Triangles = new int[NbTriangles * 3];
			Colors = new Color[NbVertices];
			Bound = new Bounds(new Vector3(Size / 2f, 0f, Size / 2f), new Vector3(Size, 0f, Size));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] EdgeResFactor;

	[PublicizedFrom(EAccessModifier.Private)]
	public float NextResLevelEdgeFactor;

	[PublicizedFrom(EAccessModifier.Private)]
	public float Size;

	[PublicizedFrom(EAccessModifier.Private)]
	public int Resolution;

	public long CellKey;

	public bool WasReset;

	public int ResLevel;

	public bool IsMeshUpdated;

	public bool IsFreeToUse;

	public Vector3 LeftDownCoor;

	public Vector3 TerrainOriginCoor;

	public Vector2i CellIdVector;

	public DChunkSquareMesh CellMeshData;

	public static DChunkSquareMeshPool SMPool;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[][] ActivateObject_NTabArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] ActivateObject_NTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[][] ActivateObject_ENTabArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] ActivateObject_ENTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[][] CalcMeshTg_tan1Array;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] CalcMeshTg_tan1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[][] CalcMeshTg_tan2Array;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] CalcMeshTg_tan2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WaterMesh[] StaticWaterSMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh AcUMeshGO_CellMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh AcUMeshGO_ColMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[][] TmpMeshV;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[][] TmpMeshN;

	public float[] CMDataVerticesHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] CMDataEdgeCorHeight;

	public bool ChunkObjExist;

	public bool IsChunkActivated;

	public bool IsOnActivationProcess;

	public bool IsOnSeamCorrectionProcess;

	public GameObject CellObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float FoldHeight = 35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject WaterPlaneObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public float WaterPlaneHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool WaterPlaneIsActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool WaterPlaneObjExist;

	[PublicizedFrom(EAccessModifier.Private)]
	public float WaterPlaneUnitStep;

	[PublicizedFrom(EAccessModifier.Private)]
	public float WaterPlaneOverlapSize = 0.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsWaterActivated;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] CurTri;

	public DistantChunkMapInfo ChunkMapInfo;

	public DistantChunkMap BaseChunkMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stack<GameObject> WaterPlaneGameObjStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 ScaleTexVec = new Vector2(3.75f, 3.75f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 FlPosVec = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] TilingFacFromResLevel = new float[4] { 1f, 1f, 1f, 32f };

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 v1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 v2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 v3;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 w1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 w2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 w3;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 sdir;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 tdir;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 n;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 t;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MAX_RESOLUTION = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] SouthTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] EastTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] NorthTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] WestTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] NSouthTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] NEastTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] NNorthTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] NWestTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] DeltaNormal = new float[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] NewNormal = new float[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] TmpVec = new float[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] EdgeFactorTab = new float[4];

	public DistantChunk(DistantChunkMap _BaseChunkMap, int _ResLevel, Vector2i _CellIdVector, Vector2 _TerrainOriginCoor, float[] _EdgeResFactor, Stack<GameObject>[] _waterPlaneGameObjStack)
	{
		ResLevel = _ResLevel;
		BaseChunkMap = _BaseChunkMap;
		ChunkMapInfo = _BaseChunkMap.ChunkMapInfoArray[_ResLevel];
		if (SMPool == null)
		{
			SMPool = new DChunkSquareMeshPool(128, BaseChunkMap.NbResLevel);
		}
		ChunkObjExist = false;
		IsChunkActivated = true;
		IsOnActivationProcess = false;
		IsFreeToUse = true;
		IsOnSeamCorrectionProcess = false;
		WasReset = false;
		IsMeshUpdated = false;
		CellIdVector = _CellIdVector;
		TerrainOriginCoor = new Vector3(_TerrainOriginCoor.x, 0f, _TerrainOriginCoor.y);
		CellKey = GetCellKeyIdFromIdVector(_CellIdVector);
		LeftDownCoor.Set((float)_CellIdVector.x * ChunkMapInfo.ChunkWidth, 0f, (float)_CellIdVector.y * ChunkMapInfo.ChunkWidth);
		EdgeResFactor = (float[])_EdgeResFactor.Clone();
		Size = ChunkMapInfo.ChunkWidth;
		Resolution = ChunkMapInfo.ChunkResolution;
		NextResLevelEdgeFactor = ChunkMapInfo.NextResLevelEdgeFactor;
		WaterPlaneGameObjStack = _waterPlaneGameObjStack[ResLevel];
		if (ResLevel == 0)
		{
			WaterPlaneUnitStep = 16f;
		}
		else if (ResLevel == 1)
		{
			WaterPlaneUnitStep = 64f;
		}
		WaterPlaneHeight = 0f;
		WaterPlaneObjExist = false;
		WaterPlaneIsActive = false;
		int num = ChunkMapInfo.BaseMesh.Vertices.Length;
		CMDataVerticesHeight = new float[num];
		CMDataEdgeCorHeight = new float[Resolution * 4];
		if (ActivateObject_NTabArray == null)
		{
			ActivateObject_NTabArray = new float[BaseChunkMap.NbResLevel][];
		}
		if (ActivateObject_NTabArray[ChunkMapInfo.ResLevel] == null)
		{
			ActivateObject_NTabArray[ChunkMapInfo.ResLevel] = new float[Resolution * Resolution * 3];
		}
		ActivateObject_NTab = ActivateObject_NTabArray[ChunkMapInfo.ResLevel];
		if (ActivateObject_ENTabArray == null)
		{
			ActivateObject_ENTabArray = new float[BaseChunkMap.NbResLevel][];
		}
		if (ActivateObject_ENTabArray[ChunkMapInfo.ResLevel] == null)
		{
			ActivateObject_ENTabArray[ChunkMapInfo.ResLevel] = new float[Resolution * 4 * 3];
		}
		ActivateObject_ENTab = ActivateObject_ENTabArray[ChunkMapInfo.ResLevel];
		if (CalcMeshTg_tan1Array == null)
		{
			CalcMeshTg_tan1Array = new Vector3[BaseChunkMap.NbResLevel][];
		}
		if (CalcMeshTg_tan1Array[ChunkMapInfo.ResLevel] == null)
		{
			CalcMeshTg_tan1Array[ChunkMapInfo.ResLevel] = new Vector3[num];
		}
		CalcMeshTg_tan1 = CalcMeshTg_tan1Array[ChunkMapInfo.ResLevel];
		if (CalcMeshTg_tan2Array == null)
		{
			CalcMeshTg_tan2Array = new Vector3[BaseChunkMap.NbResLevel][];
		}
		if (CalcMeshTg_tan2Array[ChunkMapInfo.ResLevel] == null)
		{
			CalcMeshTg_tan2Array[ChunkMapInfo.ResLevel] = new Vector3[num];
		}
		CalcMeshTg_tan2 = CalcMeshTg_tan2Array[ChunkMapInfo.ResLevel];
		if (TmpMeshV == null)
		{
			TmpMeshV = new Vector3[BaseChunkMap.NbResLevel][];
		}
		if (TmpMeshV[ResLevel] == null)
		{
			TmpMeshV[ResLevel] = new Vector3[num];
		}
		if (TmpMeshN == null)
		{
			TmpMeshN = new Vector3[BaseChunkMap.NbResLevel][];
		}
		if (TmpMeshN[ResLevel] == null)
		{
			TmpMeshN[ResLevel] = new Vector3[num];
		}
		if (StaticWaterSMesh == null)
		{
			StaticWaterSMesh = new WaterMesh[BaseChunkMap.NbResLevel];
		}
		if (StaticWaterSMesh[ResLevel] == null)
		{
			int nbTriangles;
			if (ResLevel < 2)
			{
				int num2 = (int)((double)(Size / WaterPlaneUnitStep) + 0.0001) + 1;
				num = num2 * num2;
				nbTriangles = (num2 - 1) * (num2 - 1) * 2;
			}
			else
			{
				num = 64;
				nbTriangles = 32;
			}
			StaticWaterSMesh[ResLevel] = new WaterMesh(num, nbTriangles, Size);
		}
	}

	public void ResetDistantChunkSameResLevel(DistantChunkMap _BaseChunkMap, Vector2i _CellIdVector, Vector2 _TerrainOriginCoor, float[] _EdgeResFactor)
	{
		BaseChunkMap = _BaseChunkMap;
		ChunkMapInfo = _BaseChunkMap.ChunkMapInfoArray[ResLevel];
		if (CellMeshData == null)
		{
			CellMeshData = SMPool.GetObject(_BaseChunkMap, ResLevel);
		}
		CellIdVector = _CellIdVector;
		TerrainOriginCoor = new Vector3(_TerrainOriginCoor.x, 0f, _TerrainOriginCoor.y);
		CellKey = GetCellKeyIdFromIdVector(_CellIdVector);
		LeftDownCoor.Set((float)_CellIdVector.x * ChunkMapInfo.ChunkWidth, 0f, (float)_CellIdVector.y * ChunkMapInfo.ChunkWidth);
		EdgeResFactor = (float[])_EdgeResFactor.Clone();
		IsMeshUpdated = false;
		IsOnActivationProcess = false;
		IsOnSeamCorrectionProcess = false;
		IsChunkActivated = true;
		WaterPlaneObjExist = false;
		WaterPlaneIsActive = false;
	}

	public void Cleanup()
	{
		SMPool.ReturnObject(CellMeshData, ResLevel);
		CellMeshData = null;
	}

	public void ResetEdgeToOwnResLevel(int _edgeId)
	{
		if (!(CellObj == null))
		{
			Vector3[] vertices = ChunkMapInfo.BaseMesh.Vertices;
			int[][] edgeMap = ChunkMapInfo.EdgeMap;
			float[] cMDataVerticesHeight = CMDataVerticesHeight;
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i].y = CMDataVerticesHeight[i];
			}
			for (int j = 0; j < Resolution; j++)
			{
				vertices[edgeMap[_edgeId][j]].y = cMDataVerticesHeight[edgeMap[_edgeId][j]];
			}
			CellObj.GetComponent<MeshFilter>().mesh.vertices = vertices;
		}
	}

	public void ResetEdgeToNextResLevel(int _edgeId)
	{
		if (!(CellObj == null))
		{
			int num = _edgeId * Resolution;
			Vector3[] vertices = ChunkMapInfo.BaseMesh.Vertices;
			int[][] edgeMap = ChunkMapInfo.EdgeMap;
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i].y = CMDataVerticesHeight[i];
			}
			for (int j = 0; j < Resolution; j++)
			{
				vertices[edgeMap[_edgeId][j]].y = CMDataEdgeCorHeight[num + j];
			}
			CellObj.GetComponent<MeshFilter>().mesh.vertices = vertices;
		}
	}

	public void ActivateObject(bool _bHeightAndNormalOnly)
	{
		int num = ChunkMapInfo.BaseMesh.Vertices.Length;
		int num2 = Resolution * Resolution;
		WasReset = true;
		float[] cMDataVerticesHeight = CMDataVerticesHeight;
		calculateEdgeInformation(LeftDownCoor.x + TerrainOriginCoor.x, LeftDownCoor.z + TerrainOriginCoor.z, Size, Resolution, 1f, 1f, NextResLevelEdgeFactor, NextResLevelEdgeFactor, NextResLevelEdgeFactor, NextResLevelEdgeFactor, cMDataVerticesHeight);
		int num3 = 0;
		int num4 = 0;
		while (num3 < Resolution * 4)
		{
			CellMeshData.EdgeCorNormals[num3].x = 0f - ActivateObject_ENTab[num4];
			CellMeshData.EdgeCorNormals[num3].y = 0f - ActivateObject_ENTab[num4 + 1];
			CellMeshData.EdgeCorNormals[num3].z = 0f - ActivateObject_ENTab[num4 + 2];
			num3++;
			num4 += 3;
		}
		float num5 = float.MaxValue;
		float num6 = float.MinValue;
		int num7 = 0;
		int num8 = 0;
		while (num7 < num2)
		{
			if (cMDataVerticesHeight[num7] > num6)
			{
				num6 = cMDataVerticesHeight[num7];
			}
			if (cMDataVerticesHeight[num7] < num5)
			{
				num5 = cMDataVerticesHeight[num7];
			}
			CellMeshData.Normals[num7].Set(0f - ActivateObject_NTab[num8], 0f - ActivateObject_NTab[num8 + 1], 0f - ActivateObject_NTab[num8 + 2]);
			num7++;
			num8 += 3;
		}
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < Resolution; j++)
			{
				int num9 = i * Resolution + j;
				CellMeshData.Normals[ChunkMapInfo.EdgeMap[i][j]].Set(0f - ActivateObject_ENTab[num9 * 3], 0f - ActivateObject_ENTab[num9 * 3 + 1], 0f - ActivateObject_ENTab[num9 * 3 + 2]);
			}
		}
		bool flag = num > num2;
		if (flag)
		{
			int num10 = num2;
			for (int k = 0; k < 4; k++)
			{
				for (int l = 0; l < Resolution; l++)
				{
					CMDataVerticesHeight[num10] = CMDataVerticesHeight[ChunkMapInfo.EdgeMap[k][l]] - 35f;
					CellMeshData.Normals[num10].Set(CellMeshData.Normals[ChunkMapInfo.EdgeMap[k][l]].x, CellMeshData.Normals[ChunkMapInfo.EdgeMap[k][l]].y, CellMeshData.Normals[ChunkMapInfo.EdgeMap[k][l]].z);
					num10++;
				}
			}
		}
		CellMeshData.ChunkBound.SetMinMax(new Vector3(0f, num5, 0f), new Vector3(Size, num6, Size));
		calculateMeshTangents(CellMeshData);
		IsWaterActivated = false;
		cMDataVerticesHeight = CMDataVerticesHeight;
		Vector3[] vertices = ChunkMapInfo.BaseMesh.Vertices;
		int num11 = 0;
		int num12 = 0;
		while (num11 < cMDataVerticesHeight.Length)
		{
			int x = (int)(vertices[num11].x + LeftDownCoor.x + TerrainOriginCoor.x);
			int num13 = ((!flag || num11 < num2) ? ((int)(cMDataVerticesHeight[num11] + LeftDownCoor.y + TerrainOriginCoor.y)) : ((int)(cMDataVerticesHeight[num11] + LeftDownCoor.y + TerrainOriginCoor.y + 35f)));
			int z = (int)(vertices[num11].z + LeftDownCoor.z + TerrainOriginCoor.z);
			if (BaseChunkMap != null && BaseChunkMap.wcd != null)
			{
				int waterPlaneBlockId;
				if (DistantTerrainConstants.SeaLevel > 0f && (float)num13 <= DistantTerrainConstants.SeaLevel)
				{
					waterPlaneBlockId = 840;
					bool flag2 = !WaterPlaneIsActive && DistantTerrainConstants.SeaLevel >= (float)num13;
					IsWaterActivated |= flag2;
					if (flag2)
					{
						CellMeshData.WaterPlaneBlockId = waterPlaneBlockId;
					}
				}
				waterPlaneBlockId = GameManager.Instance.World.m_WorldEnvironment.DistantTerrain_GetBlockIdAt(x, num13, z);
				Block block = Block.list[waterPlaneBlockId];
				CellMeshData.IsWater[num11] = block.blockMaterial.IsLiquid;
				int sideTextureId = block.GetSideTextureId(new BlockValue((uint)waterPlaneBlockId), BlockFace.Top, 0);
				int sideTextureId2 = block.GetSideTextureId(new BlockValue((uint)waterPlaneBlockId), BlockFace.South, 0);
				if (sideTextureId != -1 && sideTextureId2 != -1)
				{
					CellMeshData.TextureId[num11] = VoxelMeshTerrain.EncodeTexIds(sideTextureId, sideTextureId2);
				}
				else
				{
					CellMeshData.TextureId[num11] = -1;
				}
			}
			else
			{
				CellMeshData.TextureId[num11] = -1;
			}
			num11++;
			num12 += 3;
		}
		Transvoxel.BuildVertex _data = new Transvoxel.BuildVertex
		{
			bTopSoil = true
		};
		CellMeshData.VoxelMesh.ClearMesh();
		CurTri = ChunkMapInfo.BaseMesh.Triangles;
		for (int m = 0; m < CurTri.Length; m += 3)
		{
			int num14 = CurTri[m];
			int num15 = CurTri[m + 1];
			int num16 = CurTri[m + 2];
			int num17;
			if (CellMeshData.IsWater[num14] && CellMeshData.IsWater[num15] && CellMeshData.IsWater[num16])
			{
				num17 = CellMeshData.VoxelMesh.FindOrCreateSubMesh(VoxelMeshTerrain.EncodeTexIds(5000, 5000), VoxelMeshTerrain.EncodeTexIds(5001, 5001), VoxelMeshTerrain.EncodeTexIds(5002, 5002));
			}
			else
			{
				int t = CellMeshData.TextureId[num14];
				int t2 = CellMeshData.TextureId[num15];
				int t3 = CellMeshData.TextureId[num16];
				if (CellMeshData.IsWater[num14])
				{
					t = VoxelMeshTerrain.EncodeTexIds(178, 178);
				}
				if (CellMeshData.IsWater[num15])
				{
					t2 = VoxelMeshTerrain.EncodeTexIds(178, 178);
				}
				if (CellMeshData.IsWater[num16])
				{
					t3 = VoxelMeshTerrain.EncodeTexIds(178, 178);
				}
				num17 = CellMeshData.VoxelMesh.FindOrCreateSubMesh(t, t2, t3);
			}
			CellMeshData.VoxelMesh.AddIndices(num14, num15, num16, num17);
			_data.texture = CellMeshData.TextureId[num14];
			CellMeshData.VoxelMesh.GetColorForTextureId(num17, ref _data);
			CellMeshData.Colors[num14] = _data.color;
			_data.texture = CellMeshData.TextureId[num15];
			CellMeshData.VoxelMesh.GetColorForTextureId(num17, ref _data);
			CellMeshData.Colors[num15] = _data.color;
			_data.texture = CellMeshData.TextureId[num16];
			CellMeshData.VoxelMesh.GetColorForTextureId(num17, ref _data);
			CellMeshData.Colors[num16] = _data.color;
		}
	}

	public void ActivateUnityMeshGameObject(DistantChunkBasicMesh _BasicMesh)
	{
		Vector3[] array = TmpMeshV[ResLevel];
		Vector3[] array2 = TmpMeshN[ResLevel];
		float[] cMDataVerticesHeight = CMDataVerticesHeight;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Set(ChunkMapInfo.BaseMesh.Vertices[i].x, cMDataVerticesHeight[i], ChunkMapInfo.BaseMesh.Vertices[i].z);
			array2[i].Set(CellMeshData.Normals[i].x, CellMeshData.Normals[i].y, CellMeshData.Normals[i].z);
		}
		for (int j = 0; j < 4; j++)
		{
			if (!(EdgeResFactor[j] <= 1f))
			{
				int num = j * Resolution;
				for (int k = 0; k < Resolution; k++)
				{
					array[ChunkMapInfo.EdgeMap[j][k]].y = CMDataEdgeCorHeight[num + k];
					array2[ChunkMapInfo.EdgeMap[j][k]] = CellMeshData.EdgeCorNormals[num + k];
				}
			}
		}
		if (!ChunkObjExist)
		{
			CellObj = new GameObject("DC", typeof(MeshRenderer), typeof(MeshFilter));
			CellObj.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
			CellObj.GetComponent<Renderer>().receiveShadows = false;
			CellObj.transform.parent = BaseChunkMap.ParentGameObject.transform;
			if (OcclusionManager.Instance.cullDistantChunks)
			{
				Occludee.Add(CellObj);
			}
		}
		AcUMeshGO_CellMesh = CellObj.GetComponent<MeshFilter>().mesh;
		AcUMeshGO_CellMesh.vertices = TmpMeshV[ResLevel];
		AcUMeshGO_CellMesh.normals = TmpMeshN[ResLevel];
		AcUMeshGO_CellMesh.tangents = CellMeshData.Tangents;
		AcUMeshGO_CellMesh.bounds = CellMeshData.ChunkBound;
		AcUMeshGO_CellMesh.RecalculateBounds();
		if (!ChunkObjExist)
		{
			AcUMeshGO_CellMesh.triangles = ChunkMapInfo.BaseMesh.Triangles;
			AcUMeshGO_CellMesh.subMeshCount = 1;
		}
		CellObj.layer = ChunkMapInfo.LayerId;
		CellObj.SetActive(IsChunkActivated);
		ActivateWaterPlane(0f, IsActive: false);
		if (IsWaterActivated)
		{
			ActivateWaterPlane(DistantTerrainConstants.SeaLevel, IsActive: true);
		}
		TextureAtlasTerrain ta = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
		MeshRenderer component = CellObj.GetComponent<MeshRenderer>();
		CellMeshData.VoxelMesh.ApplyMaterials(component, ta, TilingFacFromResLevel[ResLevel], _bDistant: true);
		if (WaterPlaneIsActive)
		{
			MeshDescription obj = MeshDescription.meshes[DistantTerrainConstants.MeshIndexWater];
			component = WaterPlaneObj.GetComponent<MeshRenderer>();
			Object.Destroy(component.sharedMaterial);
			Material material = new Material(obj.materialDistant);
			material.SetTextureScale("_MainTex", ScaleTexVec);
			material.SetTextureScale("_BumpMap", ScaleTexVec);
			component.sharedMaterial = material;
			FlPosVec.Set(-0.5f, WaterPlaneHeight, -0.5f);
			WaterPlaneObj.transform.localPosition = FlPosVec;
		}
		AcUMeshGO_CellMesh.colors = CellMeshData.Colors;
		AcUMeshGO_CellMesh.subMeshCount = CellMeshData.VoxelMesh.submeshes.Count;
		for (int l = 0; l < CellMeshData.VoxelMesh.submeshes.Count; l++)
		{
			MeshUnsafeCopyHelper.CopyTriangles(CellMeshData.VoxelMesh.submeshes[l].triangles, AcUMeshGO_CellMesh, l);
		}
		CellObj.transform.localPosition = LeftDownCoor + TerrainOriginCoor + DistantChunkMap.cShiftTerrainVector + ChunkMapInfo.ChunkExtraShiftVector - Origin.position;
		ChunkObjExist = true;
		Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateWaterPlaneMesh(float MeshWidth, GameObject _WaterPlaneObj)
	{
		if (ResLevel == 2)
		{
			createWaterPlaneMeshLowRes(_WaterPlaneObj);
			return;
		}
		WaterMesh waterMesh = StaticWaterSMesh[ResLevel];
		if (waterMesh.Normals[0].sqrMagnitude == 0f)
		{
			int num = 4;
			int num2 = (int)((double)(Size / MeshWidth) + 0.0001) + 1;
			Color color = Lighting.ToColor(15, 0, 1f);
			float num3 = (Size + WaterPlaneOverlapSize * 2f) / (float)(num2 - 1);
			float num4 = 1f / (float)(num2 - 1);
			float num5 = (float)num * BaseChunkMap.ChunkMapInfoArray[0].ChunkWidth;
			int num6 = 0;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					waterMesh.Vertices[num6].Set((float)i * num3, 0f, (float)j * num3);
					waterMesh.Normals[num6].Set(0f, 1f, 0f);
					waterMesh.Tangents[num6].Set(1f, 0f, 0f, -1f);
					if (ResLevel == 0)
					{
						int num7 = ((CellIdVector.x > 0) ? (CellIdVector.x % num) : ((num + CellIdVector.x % num) % num));
						int num8 = ((CellIdVector.y > 0) ? (CellIdVector.y % num) : ((num + CellIdVector.y % num) % num));
						waterMesh.UVVectors[num6].Set(((float)num7 + (float)i * num4) * 0.25f, ((float)num8 + (float)j * num4) * 0.25f);
					}
					else
					{
						waterMesh.UVVectors[num6].Set((float)i * MeshWidth / num5 % 1.000001f, (float)j * MeshWidth / num5 % 1.000001f);
					}
					waterMesh.Colors[num6].r = color.r;
					waterMesh.Colors[num6].g = color.g;
					waterMesh.Colors[num6].b = color.b;
					waterMesh.Colors[num6].a = color.a;
					num6++;
				}
			}
			num6 = 0;
			for (int k = 0; k < num2 - 1; k++)
			{
				for (int l = 0; l < num2 - 1; l++)
				{
					int num9 = k * num2 + l;
					waterMesh.Triangles[num6++] = num9 + 1;
					waterMesh.Triangles[num6++] = num9 + num2;
					waterMesh.Triangles[num6++] = num9;
					waterMesh.Triangles[num6++] = num9 + num2 + 1;
					waterMesh.Triangles[num6++] = num9 + num2;
					waterMesh.Triangles[num6++] = num9 + 1;
				}
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = waterMesh.Vertices;
		mesh.normals = waterMesh.Normals;
		mesh.uv = waterMesh.UVVectors;
		mesh.triangles = waterMesh.Triangles;
		mesh.colors = waterMesh.Colors;
		mesh.subMeshCount = 1;
		mesh.tangents = waterMesh.Tangents;
		mesh.bounds = waterMesh.Bound;
		mesh.name = "WP";
		MeshFilter component = _WaterPlaneObj.GetComponent<MeshFilter>();
		if (component.sharedMesh != null)
		{
			Object.Destroy(component.sharedMesh);
		}
		component.mesh = mesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createWaterPlaneMeshLowRes(GameObject _WaterPlaneObj)
	{
		WaterMesh waterMesh = StaticWaterSMesh[ResLevel];
		if (waterMesh.Normals[0].sqrMagnitude == 0f)
		{
			Color color = Lighting.ToColor(15, 0, 1f);
			float num = Size / 4f;
			int num2 = 0;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					waterMesh.UVVectors[num2].Set(0f, 0f);
					waterMesh.Vertices[num2++].Set((float)i * num, 0f, (float)j * num);
					waterMesh.UVVectors[num2].Set(1f, 0f);
					waterMesh.Vertices[num2++].Set((float)(i + 1) * num, 0f, (float)j * num);
					waterMesh.UVVectors[num2].Set(1f, 1f);
					waterMesh.Vertices[num2++].Set((float)(i + 1) * num, 0f, (float)(j + 1) * num);
					waterMesh.UVVectors[num2].Set(0f, 1f);
					waterMesh.Vertices[num2++].Set((float)i * num, 0f, (float)(j + 1) * num);
				}
			}
			for (int k = 0; k < 64; k++)
			{
				waterMesh.Normals[k].Set(0f, 1f, 0f);
				waterMesh.Tangents[k].Set(1f, 0f, 0f, -1f);
				waterMesh.Colors[k].r = color.r;
				waterMesh.Colors[k].g = color.g;
				waterMesh.Colors[k].b = color.b;
				waterMesh.Colors[k].a = color.a;
			}
			num2 = 0;
			for (int l = 0; l < 4; l++)
			{
				for (int m = 0; m < 4; m++)
				{
					int num3 = (l * 4 + m) * 4;
					waterMesh.Triangles[num2++] = num3 + 3;
					waterMesh.Triangles[num2++] = num3 + 1;
					waterMesh.Triangles[num2++] = num3;
					waterMesh.Triangles[num2++] = num3 + 2;
					waterMesh.Triangles[num2++] = num3 + 1;
					waterMesh.Triangles[num2++] = num3 + 3;
				}
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = waterMesh.Vertices;
		mesh.normals = waterMesh.Normals;
		mesh.uv = waterMesh.UVVectors;
		mesh.triangles = waterMesh.Triangles;
		mesh.colors = waterMesh.Colors;
		mesh.subMeshCount = 1;
		mesh.tangents = waterMesh.Tangents;
		mesh.bounds = waterMesh.Bound;
		mesh.name = "WP";
		MeshFilter component = _WaterPlaneObj.GetComponent<MeshFilter>();
		if (component.sharedMesh != null)
		{
			Object.Destroy(component.sharedMesh);
		}
		component.mesh = mesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createWaterPlaneObject()
	{
		if (!WaterPlaneObjExist)
		{
			if (WaterPlaneGameObjStack.Count > 0)
			{
				WaterPlaneObj = WaterPlaneGameObjStack.Pop();
				WaterPlaneObj.name = "WP";
			}
			else
			{
				WaterPlaneObj = new GameObject("WP", typeof(MeshRenderer), typeof(MeshFilter));
				WaterPlaneObj.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
				CreateWaterPlaneMesh(WaterPlaneUnitStep, WaterPlaneObj);
				WaterPlaneObj.layer = ChunkMapInfo.LayerId;
			}
			WaterPlaneObj.transform.parent = CellObj.transform;
			WaterPlaneObj.transform.localPosition = new Vector3(0f - WaterPlaneOverlapSize, WaterPlaneHeight, 0f - WaterPlaneOverlapSize);
			TextureAtlasTerrain ta = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
			MeshRenderer component = CellObj.GetComponent<MeshRenderer>();
			CellMeshData.VoxelMesh.ApplyMaterials(component, ta, TilingFacFromResLevel[ResLevel], _bDistant: true);
			if (WaterPlaneIsActive)
			{
				MeshDescription obj = MeshDescription.meshes[DistantTerrainConstants.MeshIndexWater];
				component = WaterPlaneObj.GetComponent<MeshRenderer>();
				Object.Destroy(component.sharedMaterial);
				Material material = new Material(obj.materialDistant);
				material.SetTextureScale("_MainTex", ScaleTexVec);
				material.SetTextureScale("_BumpMap", ScaleTexVec);
				component.sharedMaterial = material;
				FlPosVec.Set(-0.5f, WaterPlaneHeight, -0.5f);
				WaterPlaneObj.transform.localPosition = FlPosVec;
			}
			WaterPlaneObj.SetActive(value: true);
			WaterPlaneObjExist = true;
		}
	}

	public void ActivateWaterPlane(float PlaneHeight, bool IsActive)
	{
		WaterPlaneHeight = PlaneHeight;
		WaterPlaneIsActive = IsActive;
		if (!IsActive)
		{
			if (WaterPlaneObjExist)
			{
				WaterPlaneObj.SetActive(value: false);
				WaterPlaneGameObjStack.Push(WaterPlaneObj);
				WaterPlaneObj = null;
				WaterPlaneObjExist = false;
			}
		}
		else if (!WaterPlaneObjExist)
		{
			createWaterPlaneObject();
		}
		else
		{
			if (WaterPlaneHeight != PlaneHeight)
			{
				WaterPlaneObj.transform.localPosition = new Vector3(0f - WaterPlaneOverlapSize, WaterPlaneHeight, 0f - WaterPlaneOverlapSize);
			}
			WaterPlaneObj.SetActive(value: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calculateMeshTangents(DChunkSquareMesh DataMesh)
	{
		CurTri = ChunkMapInfo.BaseMesh.Triangles;
		Vector3[] vertices = ChunkMapInfo.BaseMesh.Vertices;
		float[] cMDataVerticesHeight = CMDataVerticesHeight;
		int num = CurTri.Length;
		int num2 = vertices.Length;
		for (int i = 0; i < num2; i++)
		{
			CalcMeshTg_tan1[i].Set(0f, 0f, 0f);
			CalcMeshTg_tan2[i].Set(0f, 0f, 0f);
		}
		for (long num3 = 0L; num3 < num; num3 += 3)
		{
			long num4 = CurTri[num3];
			long num5 = CurTri[num3 + 1];
			long num6 = CurTri[num3 + 2];
			v1 = vertices[num4];
			v1.y = cMDataVerticesHeight[num4];
			v2 = vertices[num5];
			v2.y = cMDataVerticesHeight[num5];
			v3 = vertices[num6];
			v3.y = cMDataVerticesHeight[num6];
			w1.Set(0f, 0f);
			w2.Set(0f, 0f);
			w3.Set(0f, 0f);
			float num7 = v2.x - v1.x;
			float num8 = v3.x - v1.x;
			float num9 = v2.y - v1.y;
			float num10 = v3.y - v1.y;
			float num11 = v2.z - v1.z;
			float num12 = v3.z - v1.z;
			float num13 = w2.x - w1.x;
			float num14 = w3.x - w1.x;
			float num15 = w2.y - w1.y;
			float num16 = w3.y - w1.y;
			float num17 = num13 * num16 - num14 * num15;
			float num18 = ((num17 == 0f) ? 0f : (1f / num17));
			sdir.Set((num16 * num7 - num15 * num8) * num18, (num16 * num9 - num15 * num10) * num18, (num16 * num11 - num15 * num12) * num18);
			tdir.Set((num13 * num8 - num14 * num7) * num18, (num13 * num10 - num14 * num9) * num18, (num13 * num12 - num14 * num11) * num18);
			CalcMeshTg_tan1[num4].Set(CalcMeshTg_tan1[num4].x + sdir.x, CalcMeshTg_tan1[num4].y + sdir.y, CalcMeshTg_tan1[num4].z + sdir.z);
			CalcMeshTg_tan1[num5].Set(CalcMeshTg_tan1[num5].x + sdir.x, CalcMeshTg_tan1[num5].y + sdir.y, CalcMeshTg_tan1[num5].z + sdir.z);
			CalcMeshTg_tan1[num6].Set(CalcMeshTg_tan1[num6].x + sdir.x, CalcMeshTg_tan1[num6].y + sdir.y, CalcMeshTg_tan1[num6].z + sdir.z);
			CalcMeshTg_tan2[num4].Set(CalcMeshTg_tan2[num4].x + tdir.x, CalcMeshTg_tan2[num4].y + tdir.y, CalcMeshTg_tan2[num4].z + tdir.z);
			CalcMeshTg_tan2[num5].Set(CalcMeshTg_tan2[num5].x + tdir.x, CalcMeshTg_tan2[num5].y + tdir.y, CalcMeshTg_tan2[num5].z + tdir.z);
			CalcMeshTg_tan2[num6].Set(CalcMeshTg_tan2[num6].x + tdir.x, CalcMeshTg_tan2[num6].y + tdir.y, CalcMeshTg_tan2[num6].z + tdir.z);
		}
		for (long num19 = 0L; num19 < num2; num19++)
		{
			n = DataMesh.Normals[num19];
			t = CalcMeshTg_tan1[num19];
			Vector3.OrthoNormalize(ref n, ref t);
			DataMesh.Tangents[num19].x = t.x;
			DataMesh.Tangents[num19].y = t.y;
			DataMesh.Tangents[num19].z = t.z;
			DataMesh.Tangents[num19].w = (n.y * t.z - n.z * t.y) * CalcMeshTg_tan2[num19].x + (n.z * t.x - n.x * t.z) * CalcMeshTg_tan2[num19].y + (n.x * t.y - n.y * t.x) * CalcMeshTg_tan2[num19].z;
			DataMesh.Tangents[num19].w = ((DataMesh.Tangents[num19].w < 0f) ? (-1f) : 1f);
		}
	}

	public static long GetCellKeyIdFromIdVector(int[] CellIdVector)
	{
		return (long)((((ulong)CellIdVector[1] & 0xFFFFFFuL) << 24) | ((ulong)CellIdVector[0] & 0xFFFFFuL));
	}

	public static long GetCellKeyIdFromIdVector(Vector2i CellIdVector)
	{
		return (long)((((ulong)CellIdVector.y & 0xFFFFFFuL) << 24) | ((ulong)CellIdVector.x & 0xFFFFFuL));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calculateEdgeInformation(float _LowLeftCornerX, float _LowLeftCornerZ, float _Width, int _Resolution, float _XZScale, float _YScale, float _NeighbFactorSouth, float _NeighbFactorEast, float _NeighbFactorNorth, float _NeighbFactorWest, float[] _CurVHeights)
	{
		float[] array = null;
		float[] array2 = null;
		float[] array3 = null;
		float[] array4 = null;
		int[] array5 = null;
		int[] array6 = null;
		EdgeFactorTab[0] = _NeighbFactorSouth;
		EdgeFactorTab[1] = _NeighbFactorEast;
		EdgeFactorTab[2] = _NeighbFactorNorth;
		EdgeFactorTab[3] = _NeighbFactorWest;
		_LowLeftCornerX *= _XZScale;
		_LowLeftCornerZ *= _XZScale;
		_Width *= _XZScale;
		float num = _Width / (float)(_Resolution - 1);
		int num2 = _Resolution * _Resolution;
		int num3 = 3 * _Resolution;
		int num4 = 0;
		int num5 = (_Resolution - 1) * _Resolution;
		int num6 = 0;
		while (num6 < _Resolution)
		{
			SouthTab[num6] = num4;
			EastTab[num6] = num5;
			NorthTab[num6] = num2 - 1 - num4;
			WestTab[num6] = _Resolution - 1 - num6;
			NSouthTab[num6] = num4 * 3;
			NEastTab[num6] = num5 * 3;
			NNorthTab[num6] = (num2 - 1 - num4) * 3;
			NWestTab[num6] = (_Resolution - 1 - num6) * 3;
			num6++;
			num4 += _Resolution;
			num5++;
		}
		array = _CurVHeights;
		array2 = ActivateObject_NTab;
		array3 = CMDataEdgeCorHeight;
		array4 = ActivateObject_ENTab;
		float num7 = ((_NeighbFactorSouth < _NeighbFactorEast && _NeighbFactorSouth < _NeighbFactorNorth && _NeighbFactorSouth < _NeighbFactorWest) ? _NeighbFactorSouth : ((_NeighbFactorEast < _NeighbFactorNorth && _NeighbFactorEast < _NeighbFactorWest) ? _NeighbFactorEast : ((!(_NeighbFactorNorth < _NeighbFactorWest)) ? _NeighbFactorWest : _NeighbFactorNorth)));
		int num8 = ((!(num7 < 1f)) ? _Resolution : ((int)((float)_Resolution / num7 + 1f)));
		float[] array7 = new float[num8 * 2];
		float[] array8 = new float[num8 * 6];
		for (num6 = 0; num6 < _Resolution; num6++)
		{
			for (num4 = 0; num4 < _Resolution; num4++)
			{
				array[num4 + num6 * _Resolution] = DistantChunkMap.TGHeightFunc((float)num6 * num + _LowLeftCornerX, 0f, (float)num4 * num + _LowLeftCornerZ) * _YScale;
			}
		}
		for (num6 = 0; num6 < num2 * 3; num6++)
		{
			array2[num6] = 0f;
		}
		float num9 = -1f;
		int num10 = 0;
		for (num6 = 0; num6 < _Resolution - 1; num6++)
		{
			for (num4 = 0; num4 < _Resolution - 1; num4++)
			{
				int num11 = num4 + num10;
				int num12 = num11 * 3;
				float num13 = (array[num11 + _Resolution] - array[num11]) / num;
				float num14 = (array[num11 + 1 + _Resolution] - array[num11 + _Resolution]) / num;
				array2[num12] += num13;
				array2[num12 + 1] += num9;
				array2[num12 + 2] += num14;
				array2[num12 + num3] += num13;
				array2[num12 + num3 + 1] += num9;
				array2[num12 + num3 + 2] += num14;
				array2[num12 + 3 + num3] += num13;
				array2[num12 + 3 + num3 + 1] += num9;
				array2[num12 + 3 + num3 + 2] += num14;
				num13 = (array[num11 + 1 + _Resolution] - array[num11 + 1]) / num;
				num14 = (array[num11 + 1] - array[num11]) / num;
				array2[num12] += num13;
				array2[num12 + 1] += num9;
				array2[num12 + 2] += num14;
				array2[num12 + 3] += num13;
				array2[num12 + 3 + 1] += num9;
				array2[num12 + 3 + 2] += num14;
				array2[num12 + 3 + num3] += num13;
				array2[num12 + 3 + num3 + 1] += num9;
				array2[num12 + 3 + num3 + 2] += num14;
			}
			num10 += _Resolution;
		}
		num10 = 0;
		for (num6 = 0; num6 < _Resolution; num6++)
		{
			for (num4 = 0; num4 < _Resolution; num4++)
			{
				int num12 = (num4 + num10) * 3;
				float num15 = 1f / Mathf.Sqrt(array2[num12] * array2[num12] + array2[num12 + 1] * array2[num12 + 1] + array2[num12 + 2] * array2[num12 + 2]);
				array2[num12] *= num15;
				array2[num12 + 1] *= num15;
				array2[num12 + 2] *= num15;
			}
			num10 += _Resolution;
		}
		for (int i = 0; i < 4; i++)
		{
			switch (i)
			{
			case 0:
				array5 = SouthTab;
				array6 = NSouthTab;
				break;
			case 1:
				array5 = EastTab;
				array6 = NEastTab;
				break;
			case 2:
				array5 = NorthTab;
				array6 = NNorthTab;
				break;
			case 3:
				array5 = WestTab;
				array6 = NWestTab;
				break;
			}
			float num16 = EdgeFactorTab[i];
			int num17 = ((num16 > 1f) ? ((int)((double)num16 + 1E-05)) : ((int)(1.0 / (double)num16 + 1E-05)));
			int num18 = i * _Resolution;
			int num19 = i * 3 * _Resolution;
			float num20;
			int num21;
			int num11;
			if (num16 > 1f)
			{
				num20 = num * (float)num17;
				num21 = (_Resolution - 1) / num17 + 1;
				switch (i)
				{
				case 0:
				{
					float x = _LowLeftCornerX;
					float num22 = _LowLeftCornerZ - num20;
					num6 = 0;
					num5 = 0;
					int num23 = num21;
					while (num6 < num21)
					{
						array7[num6] = DistantChunkMap.TGHeightFunc(x, 0f, num22) * _YScale;
						array7[num23] = array[array5[num5]];
						num6++;
						num5 += num17;
						num23++;
						x += num20;
					}
					break;
				}
				case 1:
				{
					float x = _LowLeftCornerX + _Width + num20;
					float num22 = _LowLeftCornerZ;
					num6 = 0;
					num5 = 0;
					int num23 = num21;
					while (num6 < num21)
					{
						array7[num6] = DistantChunkMap.TGHeightFunc(x, 0f, num22) * _YScale;
						array7[num23] = array[array5[num5]];
						num6++;
						num5 += num17;
						num23++;
						num22 += num20;
					}
					break;
				}
				case 2:
				{
					float x = _LowLeftCornerX + _Width;
					float num22 = _LowLeftCornerZ + _Width + num20;
					num6 = 0;
					num5 = 0;
					int num23 = num21;
					while (num6 < num21)
					{
						array7[num6] = DistantChunkMap.TGHeightFunc(x, 0f, num22) * _YScale;
						array7[num23] = array[array5[num5]];
						num6++;
						num5 += num17;
						num23++;
						x -= num20;
					}
					break;
				}
				case 3:
				{
					float x = _LowLeftCornerX - num20;
					float num22 = _LowLeftCornerZ + _Width;
					num6 = 0;
					num5 = 0;
					int num23 = num21;
					while (num6 < num21)
					{
						array7[num6] = DistantChunkMap.TGHeightFunc(x, 0f, num22) * _YScale;
						array7[num23] = array[array5[num5]];
						num6++;
						num5 += num17;
						num23++;
						num22 -= num20;
					}
					break;
				}
				}
				num6 = 0;
				num4 = 0;
				while (num6 < num21)
				{
					array8[num4] = 0f;
					array8[num4 + 1] = 0f;
					array8[num4 + 2] = 0f;
					num6++;
					num4 += 3;
				}
				computeEdgeNormals(num21, i, _Width, array7, array8);
				num11 = 0;
				num6 = 0;
				float num15;
				while (num6 < _Resolution - 1)
				{
					float num24 = array[array5[num6]];
					float num25 = (array[array5[num6 + num17]] - num24) / (float)num17;
					DeltaNormal[0] = (array8[num11 + 3] - array8[num11]) / (float)num17;
					DeltaNormal[1] = (array8[num11 + 4] - array8[num11 + 1]) / (float)num17;
					DeltaNormal[2] = (array8[num11 + 5] - array8[num11 + 2]) / (float)num17;
					NewNormal[0] = array8[num11];
					NewNormal[1] = array8[num11 + 1];
					NewNormal[2] = array8[num11 + 2];
					array3[num6 + num18] = array[array5[num6]];
					num5 = 3 * num6;
					TmpVec[0] = array2[array6[num6]] + NewNormal[0];
					TmpVec[1] = array2[array6[num6] + 1] + NewNormal[1];
					TmpVec[2] = array2[array6[num6] + 2] + NewNormal[2];
					num15 = 1f / Mathf.Sqrt(TmpVec[0] * TmpVec[0] + TmpVec[1] * TmpVec[1] + TmpVec[2] * TmpVec[2]);
					TmpVec[0] *= num15;
					TmpVec[1] *= num15;
					TmpVec[2] *= num15;
					array4[num5 + num19] = TmpVec[0];
					array4[num5 + num19 + 1] = TmpVec[1];
					array4[num5 + num19 + 2] = TmpVec[2];
					num4 = num6 + 1;
					num5 += 3;
					num24 += num25;
					while (num4 < num6 + num17)
					{
						array3[num4 + num18] = num24;
						NewNormal[0] += DeltaNormal[0];
						NewNormal[1] += DeltaNormal[1];
						NewNormal[2] += DeltaNormal[2];
						TmpVec[0] = array2[array6[num4]] + NewNormal[0];
						TmpVec[1] = array2[array6[num4] + 1] + NewNormal[1];
						TmpVec[2] = array2[array6[num4] + 2] + NewNormal[2];
						num15 = 1f / Mathf.Sqrt(TmpVec[0] * TmpVec[0] + TmpVec[1] * TmpVec[1] + TmpVec[2] * TmpVec[2]);
						TmpVec[0] *= num15;
						TmpVec[1] *= num15;
						TmpVec[2] *= num15;
						array4[num5 + num19] = TmpVec[0];
						array4[num5 + num19 + 1] = TmpVec[1];
						array4[num5 + num19 + 2] = TmpVec[2];
						num4++;
						num5 += 3;
						num24 += num25;
					}
					num6 += num17;
					num11 += 3;
				}
				array3[num18 + _Resolution - 1] = array[array5[_Resolution - 1]];
				num4 = (num21 - 1) * 3;
				num5 = (_Resolution - 1) * 3;
				TmpVec[0] = array2[array6[_Resolution - 1]] + array8[num4];
				TmpVec[1] = array2[array6[_Resolution - 1] + 1] + array8[num4 + 1];
				TmpVec[2] = array2[array6[_Resolution - 1] + 2] + array8[num4 + 2];
				num15 = 1f / Mathf.Sqrt(TmpVec[0] * TmpVec[0] + TmpVec[1] * TmpVec[1] + TmpVec[2] * TmpVec[2]);
				TmpVec[0] *= num15;
				TmpVec[1] *= num15;
				TmpVec[2] *= num15;
				array4[num5 + num19] = TmpVec[0];
				array4[num5 + num19 + 1] = TmpVec[1];
				array4[num5 + num19 + 2] = TmpVec[2];
				continue;
			}
			for (num6 = 0; num6 < _Resolution; num6++)
			{
				array3[num6 + num18] = array[array5[num6]];
			}
			num20 = num / (float)num17;
			num21 = (_Resolution - 1) * num17 + 1;
			switch (i)
			{
			case 0:
			{
				float x = _LowLeftCornerX;
				float num22 = _LowLeftCornerZ - num20;
				num6 = 0;
				num5 = 0;
				int num23 = num21;
				while (num6 < num21)
				{
					array7[num6] = DistantChunkMap.TGHeightFunc(x, 0f, num22) * _YScale;
					if (num6 % num17 == 0)
					{
						array7[num23] = array[array5[num5]];
						num5++;
					}
					else
					{
						array7[num23] = DistantChunkMap.TGHeightFunc(x, 0f, num22 + num20) * _YScale;
					}
					num6++;
					num23++;
					x += num20;
				}
				break;
			}
			case 1:
			{
				float x = _LowLeftCornerX + _Width + num20;
				float num22 = _LowLeftCornerZ;
				num6 = 0;
				num5 = 0;
				int num23 = num21;
				while (num6 < num21)
				{
					array7[num6] = DistantChunkMap.TGHeightFunc(x, 0f, num22) * _YScale;
					if (num6 % num17 == 0)
					{
						array7[num23] = array[array5[num5]];
						num5++;
					}
					else
					{
						array7[num23] = DistantChunkMap.TGHeightFunc(x - num20, 0f, num22) * _YScale;
					}
					num6++;
					num23++;
					num22 += num20;
				}
				break;
			}
			case 2:
			{
				float x = _LowLeftCornerX + _Width;
				float num22 = _LowLeftCornerZ + _Width + num20;
				num6 = 0;
				num5 = 0;
				int num23 = num21;
				while (num6 < num21)
				{
					array7[num6] = DistantChunkMap.TGHeightFunc(x, 0f, num22) * _YScale;
					if (num6 % num17 == 0)
					{
						array7[num23] = array[array5[num5]];
						num5++;
					}
					else
					{
						array7[num23] = DistantChunkMap.TGHeightFunc(x, 0f, num22 - num20) * _YScale;
					}
					num6++;
					num23++;
					x -= num20;
				}
				break;
			}
			case 3:
			{
				float x = _LowLeftCornerX - num20;
				float num22 = _LowLeftCornerZ + _Width;
				num6 = 0;
				num5 = 0;
				int num23 = num21;
				while (num6 < num21)
				{
					array7[num6] = DistantChunkMap.TGHeightFunc(x, 0f, num22) * _YScale;
					if (num6 % num17 == 0)
					{
						array7[num23] = array[array5[num5]];
						num5++;
					}
					else
					{
						array7[num23] = DistantChunkMap.TGHeightFunc(x + num20, 0f, num22) * _YScale;
					}
					num6++;
					num23++;
					num22 -= num20;
				}
				break;
			}
			}
			num6 = 0;
			num4 = 0;
			while (num6 < num21)
			{
				array8[num4] = 0f;
				array8[num4 + 1] = 0f;
				array8[num4 + 2] = 0f;
				num6++;
				num4 += 3;
			}
			computeEdgeNormals(num21, i, _Width, array7, array8);
			num11 = 0;
			num5 = 3 * num17;
			num6 = 0;
			num4 = 0;
			while (num6 < _Resolution)
			{
				TmpVec[0] = array2[array6[num6]] + array8[num4];
				TmpVec[1] = array2[array6[num6] + 1] + array8[num4 + 1];
				TmpVec[2] = array2[array6[num6] + 2] + array8[num4 + 2];
				array4[num11 + num19] = TmpVec[0] / 2f;
				array4[num11 + num19 + 1] = TmpVec[1] / 2f;
				array4[num11 + num19 + 2] = TmpVec[2] / 2f;
				num6++;
				num4 += num5;
				num11 += 3;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void computeEdgeNormals(int _Resolution, int _EdgeId, float _ChunkWidth, float[] _HeightTab, float[] _NormalTab)
	{
		float num = _ChunkWidth / (float)(_Resolution - 1);
		int num2 = _Resolution;
		int num3 = 0;
		int num4 = 0;
		while (num4 < _Resolution - 1)
		{
			float num5 = (_HeightTab[num4 + 1] - _HeightTab[num4]) / num;
			float num6 = (_HeightTab[num2 + 1] - _HeightTab[num4 + 1]) / num;
			_NormalTab[num3 + 3] += num5;
			_NormalTab[num3 + 4] += -1f;
			_NormalTab[num3 + 5] += num6;
			num5 = (_HeightTab[num2 + 1] - _HeightTab[num2]) / num;
			num6 = (_HeightTab[num2] - _HeightTab[num4]) / num;
			_NormalTab[num3] += num5;
			_NormalTab[num3 + 1] += -1f;
			_NormalTab[num3 + 2] += num6;
			_NormalTab[num3 + 3] += num5;
			_NormalTab[num3 + 4] += -1f;
			_NormalTab[num3 + 5] += num6;
			num4++;
			num2++;
			num3 += 3;
		}
		for (int i = 0; i < _Resolution * 3; i += 3)
		{
			float num7 = 1f / Mathf.Sqrt(_NormalTab[i] * _NormalTab[i] + _NormalTab[i + 1] * _NormalTab[i + 1] + _NormalTab[i + 2] * _NormalTab[i + 2]);
			switch (_EdgeId)
			{
			case 0:
				_NormalTab[i] *= num7;
				_NormalTab[i + 1] *= num7;
				_NormalTab[i + 2] *= num7;
				break;
			case 1:
			{
				float num8 = _NormalTab[i + 2] * num7;
				_NormalTab[i + 1] *= num7;
				_NormalTab[i + 2] = _NormalTab[i] * num7;
				_NormalTab[i] = 0f - num8;
				break;
			}
			case 2:
				_NormalTab[i] *= 0f - num7;
				_NormalTab[i + 1] *= num7;
				_NormalTab[i + 2] *= 0f - num7;
				break;
			case 3:
			{
				float num8 = _NormalTab[i + 2] * num7;
				_NormalTab[i + 1] *= num7;
				_NormalTab[i + 2] = (0f - _NormalTab[i]) * num7;
				_NormalTab[i] = num8;
				break;
			}
			}
		}
	}
}

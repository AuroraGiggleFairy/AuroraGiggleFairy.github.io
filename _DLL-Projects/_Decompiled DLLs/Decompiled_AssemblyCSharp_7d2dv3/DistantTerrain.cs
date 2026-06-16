using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class DistantTerrain
{
	public class PlayerPosHelper
	{
		public Vector3 PlayerPos;

		public PlayerPosHelper(Vector3 _PlayerPos)
		{
			PlayerPos = _PlayerPos;
		}
	}

	public class ChunkStateHelper
	{
		public bool IsActive;

		public int PosX;

		public int PosZ;

		public ChunkStateHelper(int _PosX, int _PosZ, bool _IsActive)
		{
			PosX = _PosX;
			PosZ = _PosZ;
			IsActive = _IsActive;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int NbChunkToBeUpdated = 15;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int DT_ViewDistance = 2000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxNbDChunkOnAsyncUpdate = 50;

	public OptimizedList<DistantChunk>[] ChunkDataList;

	public List<OptimizedList<DistantChunk>[]> ChunkDataListBackGround;

	[PublicizedFrom(EAccessModifier.Private)]
	public DistantChunk[] ChunkDataProxyArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public UtilList<PlayerPosHelper> PlPosCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, ChunkStateHelper> ChunkActivationCacheDic;

	[PublicizedFrom(EAccessModifier.Private)]
	public OptimizedList<ChunkStateHelper> ChunkToActivateList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessCacheCnt;

	public List<ThreadInfoParam> MetaThreadContList;

	public List<ThreadInfoParam> MetaCoroutineContList;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadProcessing ThProcessing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsOnMapSyncProcess;

	public bool IsTerrainReady;

	[PublicizedFrom(EAccessModifier.Private)]
	public DistantChunkBasicMesh[] BaseMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public DistantChunkMap[] MainChunkMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public int CurMapId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int ChunkBackGroundMapId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int NbResLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int MaxNbResLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWorldSizeX = 20000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWorldSizeZ = 20000;

	public Vector2 CurrentPlayerPosVec;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 CurrentPlayerExtendedPosVec;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i TerrainOriginIntCoor;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] EdgeResFactor;

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator YieldCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public OptimizedList<DistantChunk>[] DistantChunkStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stack<GameObject>[] WaterPlaneGameObjStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadInfoParamPool ThInfoParamPool;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadProcessingPool ThProcessingPool;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadContainerPool ThContainerPool;

	public int DebugYieldResLevel = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public DistantTerrainQuadTree TerExQuadTree;

	public static Vector3 cShiftHiResChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 cShiftMidResChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 cShiftLowResChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject goDistantTerrain;

	public static DistantTerrain Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] IdToRemove = new int[20];

	public void Init()
	{
		cShiftMidResChunks = cShiftHiResChunks + new Vector3(0f, -0.2f, 0f);
		cShiftLowResChunks = cShiftMidResChunks + new Vector3(0f, -0.2f, 0f);
		Instance = this;
		goDistantTerrain = new GameObject("DistantChunks");
		Origin.Add(goDistantTerrain.transform, 0);
		float[][] array = new float[2][];
		float[][] array2 = new float[2][];
		int[][] array3 = new int[2][];
		int[][] array4 = new int[2][];
		int[][] array5 = new int[2][];
		Vector3[][] array6 = new Vector3[2][];
		TerExQuadTree = new DistantTerrainQuadTree(20000, 20000, 16, 16);
		array[0] = new float[1] { 1024f };
		array2[0] = new float[1] { 256f };
		array3[0] = new int[1] { 33 };
		array4[0] = new int[1] { 9 };
		array5[0] = new int[1] { 2 };
		array6[0] = new Vector3[1] { Vector3.zero };
		array[1] = new float[3] { 192f, 512f, 2560f };
		array2[1] = new float[3] { 16f, 64f, 256f };
		array3[1] = new int[3] { 17, 17, 17 };
		array4[1] = new int[3] { 3, 3, 5 };
		array6[1] = new Vector3[3] { cShiftHiResChunks, cShiftMidResChunks, cShiftLowResChunks };
		int num = Utils.FastMin(12, GameUtils.GetViewDistance()) * 2 * 16;
		num = num / 128 + ((num % 128 != 0) ? 1 : 0);
		int num2 = (num * 128 + 128) / 2;
		int num3 = (int)((float)(int)(2000f / array2[1][2]) * array2[1][2]);
		array[1][0] = num2;
		array[1][2] = num3;
		array5[1] = new int[3] { 0, 1, 2 };
		float[] edgeResFactor = new float[4] { 1f, 1f, 1f, 1f };
		CurMapId = 1;
		ChunkBackGroundMapId = 1;
		MainChunkMap = new DistantChunkMap[2];
		MainChunkMap[0] = new DistantChunkMap(new Vector2(20000f, 20000f), array[0], array2[0], array3[0], array4[0], array5[0], 28, null, null, goDistantTerrain, array6[0]);
		MainChunkMap[1] = new DistantChunkMap(new Vector2(20000f, 20000f), array[1], array2[1], array3[1], array4[1], array5[1], 28, null, null, goDistantTerrain, array6[1]);
		IsOnMapSyncProcess = false;
		IsTerrainReady = false;
		CurrentPlayerPosVec = new Vector2(float.MaxValue, float.MaxValue);
		CurrentPlayerExtendedPosVec = new Vector2(float.MaxValue, float.MaxValue);
		MainChunkMap[CurMapId].TerrainOrigin = Vector2.zero;
		TerrainOriginIntCoor = new Vector2i(0, 0);
		NbResLevel = MainChunkMap[CurMapId].NbResLevel;
		MaxNbResLevel = 0;
		for (int i = 0; i < MainChunkMap.Length; i++)
		{
			if (MaxNbResLevel < MainChunkMap[i].NbResLevel)
			{
				MaxNbResLevel = MainChunkMap[i].NbResLevel;
			}
		}
		DistantChunkStack = new OptimizedList<DistantChunk>[NbResLevel];
		MetaThreadContList = new List<ThreadInfoParam>(NbResLevel * 3);
		MetaCoroutineContList = new List<ThreadInfoParam>(NbResLevel * 3);
		PlPosCache = new UtilList<PlayerPosHelper>(100000, null);
		ChunkActivationCacheDic = new Dictionary<int, ChunkStateHelper>();
		ChunkToActivateList = new OptimizedList<ChunkStateHelper>();
		WaterPlaneGameObjStack = new Stack<GameObject>[NbResLevel];
		for (int j = 0; j < NbResLevel; j++)
		{
			WaterPlaneGameObjStack[j] = new Stack<GameObject>();
		}
		BaseMesh = new DistantChunkBasicMesh[MainChunkMap[CurMapId].NbResLevel];
		for (int k = 0; k < MainChunkMap[CurMapId].NbResLevel; k++)
		{
			BaseMesh[k] = MainChunkMap[CurMapId].ChunkMapInfoArray[k].BaseMesh;
		}
		ChunkDataList = new OptimizedList<DistantChunk>[MainChunkMap[CurMapId].NbResLevel];
		ChunkDataProxyArray = new DistantChunk[MainChunkMap[CurMapId].NbResLevel];
		for (int l = 0; l < MainChunkMap[CurMapId].NbResLevel; l++)
		{
			ChunkDataList[l] = new OptimizedList<DistantChunk>();
			ChunkDataProxyArray[l] = new DistantChunk(MainChunkMap[CurMapId], l, Vector2i.zero, Vector2.zero, edgeResFactor, WaterPlaneGameObjStack);
		}
		for (int m = 0; m < MaxNbResLevel; m++)
		{
			DistantChunkStack[m] = new OptimizedList<DistantChunk>();
		}
		ChunkDataListBackGround = new List<OptimizedList<DistantChunk>[]>();
		ChunkDataListBackGround.Add(new OptimizedList<DistantChunk>[MainChunkMap[ChunkBackGroundMapId].NbResLevel]);
		for (int n = 0; n < MainChunkMap[ChunkBackGroundMapId].NbResLevel; n++)
		{
			ChunkDataListBackGround.Last()[n] = new OptimizedList<DistantChunk>();
		}
		ThInfoParamPool = new ThreadInfoParamPool(MainChunkMap[CurMapId].NbResLevel * 4, 2, 0);
		ThProcessingPool = new ThreadProcessingPool(4, 0);
		ThContainerPool = new ThreadContainerPool(2000, 0);
		EdgeResFactor = new float[4] { 1f, 1f, 1f, 1f };
	}

	public void Cleanup()
	{
		MainChunkMap[CurMapId].wcd = null;
		clearThreadProcess();
		if (!GameManager.IsSplatMapAvailable())
		{
			Renderer[] componentsInChildren = goDistantTerrain.GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					UnityEngine.Object.Destroy(sharedMaterials[j]);
				}
			}
		}
		MeshFilter[] componentsInChildren2 = goDistantTerrain.GetComponentsInChildren<MeshFilter>(includeInactive: true);
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[k].sharedMesh);
			UnityEngine.Object.Destroy(componentsInChildren2[k].mesh);
		}
		UnityEngine.Object.Destroy(goDistantTerrain);
		PlPosCache.Clear();
		ChunkActivationCacheDic.Clear();
		ChunkToActivateList.Clear();
		Instance = null;
	}

	public void Configure(DelegateGetTerrainHeight _delegate, WorldCreationData _wcd, float _size = 0f)
	{
		DistantChunkMap.TGHeightFunc = _delegate;
		MainChunkMap[CurMapId].wcd = _wcd;
		PlPosCache.Clear();
		ChunkActivationCacheDic.Clear();
		ChunkToActivateList.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setTerrainGOState(bool IsTerrainActive)
	{
		MainChunkMap[CurMapId].ParentGameObject.SetActive(IsTerrainActive);
	}

	public void UpdateTerrain(Vector3 InCenterPosXZ)
	{
		if (PlPosCache.Count > PlPosCache.Capacity - 2)
		{
			RedrawTerrain(InCenterPosXZ, ChunkBackGroundMapId, ChunkDataListBackGround[0]);
		}
		if (PlPosCache.Count == 0 || PlPosCache[PlPosCache.Count - 1].PlayerPos.GetHashCode() != InCenterPosXZ.GetHashCode())
		{
			PlPosCache.Add(new PlayerPosHelper(InCenterPosXZ));
		}
		bool flag = IsSpwaningProcessOngoing();
		if (flag)
		{
			threadManagementUpdate();
			return;
		}
		Vector3 playerPos = PlPosCache.Dequeue().PlayerPos;
		int num = 0;
		if (IsOnMapSyncProcess)
		{
			if (!flag)
			{
				ManagePlayerPos(playerPos);
				UpdateCurrentPosOnMapAsync(ChunkBackGroundMapId, 0);
				ThreadInfoParam objectBig = ThInfoParamPool.GetObjectBig(MainChunkMap[CurMapId], 0, 0);
				objectBig.CntThreadContList = 0;
				objectBig.LengthThreadContList = 0;
				objectBig.IsAsynchronous = true;
				for (int i = 0; i < ChunkDataListBackGround[0].Length; i++)
				{
					for (int j = 0; j < ChunkDataListBackGround[0][i].Count; j++)
					{
						if (objectBig.LengthThreadContList >= 50)
						{
							break;
						}
						if (!ChunkDataListBackGround[0][i].array[j].IsMeshUpdated && !ChunkDataListBackGround[0][i].array[j].IsOnActivationProcess)
						{
							ChunkDataListBackGround[0][i].array[j].IsOnActivationProcess = true;
							ChunkDataListBackGround[0][i].array[j].IsFreeToUse = false;
							objectBig.ThreadContListA[objectBig.LengthThreadContList] = ThContainerPool.GetObject(this, ChunkDataListBackGround[0][i].array[j], BaseMesh[ChunkDataListBackGround[0][i].array[j].ChunkMapInfo.ChunkDataListResLevel], ChunkDataListBackGround[0][i].array[j].WasReset);
							objectBig.LengthThreadContList++;
						}
						if (!ChunkDataListBackGround[0][i].array[j].IsMeshUpdated)
						{
							num++;
						}
					}
				}
				if (objectBig.LengthThreadContList - objectBig.CntThreadContList != 0)
				{
					MetaThreadContList.Add(objectBig);
				}
				else if (objectBig != null)
				{
					ThInfoParamPool.ReturnObject(objectBig, ThContainerPool);
				}
			}
			if (num == 0 && !flag)
			{
				IsOnMapSyncProcess = false;
				ResetChunkDataList(ChunkDataListBackGround[0]);
				IsTerrainReady = true;
			}
			else
			{
				threadManagementUpdate();
			}
			return;
		}
		do
		{
			ManagePlayerPos(playerPos);
			UpdateCurrentPosOnMap();
			if (PlPosCache.Count > 0)
			{
				playerPos = PlPosCache.Dequeue().PlayerPos;
			}
		}
		while (MetaThreadContList.Count == 0 && PlPosCache.Count > 0);
		if (ProcessCacheCnt > 10)
		{
			ProcessChunkActivationCache(InCenterPosXZ);
			ProcessCacheCnt = 0;
		}
		ProcessCacheCnt++;
		threadManagementUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ManagePlayerPos(Vector3 InCenterPosXZ)
	{
		float num = ((MainChunkMap[ChunkBackGroundMapId].NbResLevel > 1) ? MainChunkMap[ChunkBackGroundMapId].ChunkMapInfoArray[1].ChunkWidth : MainChunkMap[ChunkBackGroundMapId].ChunkMapInfoArray[0].ChunkWidth);
		float num2 = 0f;
		Mathf.Abs(InCenterPosXZ.x - CurrentPlayerPosVec.x);
		float num3 = InCenterPosXZ.x - CurrentPlayerPosVec.x;
		num2 = InCenterPosXZ.z - CurrentPlayerPosVec.y;
		if (Mathf.Sqrt(num3 * num3 + num2 * num2) >= num)
		{
			RedrawTerrain(InCenterPosXZ, ChunkBackGroundMapId, ChunkDataListBackGround[0]);
		}
		setPlayerCurrentPosition(InCenterPosXZ.x, InCenterPosXZ.z);
	}

	public void RedrawTerrain()
	{
		RedrawTerrain(CurrentPlayerPosVec, ChunkBackGroundMapId, ChunkDataListBackGround[0]);
	}

	public void RedrawTerrain(Vector3 PlayerPos, int _CurMapId, OptimizedList<DistantChunk>[] CurChunkDataList)
	{
		int nbResLevel = MainChunkMap[_CurMapId].NbResLevel;
		PlPosCache.Clear();
		ChunkActivationCacheDic.Clear();
		ChunkToActivateList.Clear();
		CurrentPlayerPosVec.Set(PlayerPos.x, PlayerPos.z);
		CurrentPlayerExtendedPosVec.Set(PlayerPos.x, PlayerPos.z);
		float chunkWidth = MainChunkMap[_CurMapId].ChunkMapInfoArray[NbResLevel - 1].ChunkWidth;
		int num = (int)(CurrentPlayerPosVec.x / chunkWidth);
		int num2 = (int)(CurrentPlayerPosVec.y / chunkWidth);
		if (PlayerPos.x < 0f)
		{
			num--;
		}
		if (PlayerPos.y < 0f)
		{
			num2--;
		}
		num = 0;
		num2 = 0;
		TerrainOriginIntCoor.Set(num, num2);
		MainChunkMap[_CurMapId].TerrainOrigin.Set((float)num * chunkWidth, (float)num2 * chunkWidth);
		IsTerrainReady = false;
		IsOnMapSyncProcess = true;
		clearThreadProcess();
		for (int i = 0; i < nbResLevel; i++)
		{
			if (CurChunkDataList[i].Count != 0)
			{
				for (int num3 = CurChunkDataList[i].Count - 1; num3 >= 0; num3--)
				{
					CurChunkDataList[i].array[num3].IsFreeToUse = true;
					putDistantChunkOnStack(i, num3, CurChunkDataList);
				}
			}
			if (ChunkDataList[i].Count != 0)
			{
				for (int num3 = ChunkDataList[i].Count - 1; num3 >= 0; num3--)
				{
					ChunkDataList[i].array[num3].IsFreeToUse = true;
					putDistantChunkOnStack(i, num3, ChunkDataList);
				}
			}
			int num4 = ((i + 1 < NbResLevel) ? (i + 1) : (NbResLevel - 1));
			float newX = Mathf.Floor(CurrentPlayerPosVec.x / MainChunkMap[_CurMapId].ChunkMapInfoArray[num4].ChunkWidth + 1E-05f) * MainChunkMap[_CurMapId].ChunkMapInfoArray[num4].ChunkWidth;
			float newY = Mathf.Floor(CurrentPlayerPosVec.y / MainChunkMap[_CurMapId].ChunkMapInfoArray[num4].ChunkWidth + 1E-05f) * MainChunkMap[_CurMapId].ChunkMapInfoArray[num4].ChunkWidth;
			MainChunkMap[_CurMapId].ChunkMapInfoArray[i].ShiftVec.Set(newX, newY);
			DistantChunkPosData distantChunkPosData = MainChunkMap[_CurMapId].ComputeChunkPos((int)CurrentPlayerPosVec.x, (int)CurrentPlayerPosVec.y, i);
			for (int num3 = 0; num3 < distantChunkPosData.ChunkPos.Length; num3++)
			{
				GetDistantChunkFromStack(MainChunkMap[_CurMapId], i, distantChunkPosData.ChunkIntPos[num3], MainChunkMap[_CurMapId].TerrainOrigin, distantChunkPosData.EdgeResFactor[num3], WaterPlaneGameObjStack, CurChunkDataList);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearThreadProcess()
	{
		if (ThProcessing != null)
		{
			if (!ThProcessing.IsThreadFinished())
			{
				ThProcessing.CancelThread();
				ThProcessing.TaskInfo.WaitForEnd();
			}
			ThProcessingPool.ReturnObject(ThProcessing);
			ThProcessing = null;
		}
		for (int num = MetaThreadContList.Count - 1; num >= 0; num--)
		{
			while (MetaThreadContList[num].CntThreadContList < MetaThreadContList[num].LengthThreadContList)
			{
				MetaThreadContList[num].ThreadContListA[MetaThreadContList[num].CntThreadContList].DChunk.IsFreeToUse = true;
				ThContainerPool.ReturnObject(MetaThreadContList[num].ThreadContListA[MetaThreadContList[num].CntThreadContList], IsClearItem: true);
				MetaThreadContList[num].CntThreadContList++;
			}
			ThInfoParamPool.ReturnObject(MetaThreadContList[num], ThContainerPool);
			MetaThreadContList.RemoveAt(num);
		}
		MetaThreadContList.Clear();
		for (int num2 = MetaCoroutineContList.Count - 1; num2 >= 0; num2--)
		{
			while (MetaCoroutineContList[num2].CntThreadContList < MetaCoroutineContList[num2].LengthThreadContList)
			{
				MetaCoroutineContList[num2].ThreadContListA[MetaCoroutineContList[num2].CntThreadContList].DChunk.IsFreeToUse = true;
				ThContainerPool.ReturnObject(MetaCoroutineContList[num2].ThreadContListA[MetaCoroutineContList[num2].CntThreadContList], IsClearItem: true);
				MetaCoroutineContList[num2].CntThreadContList++;
			}
			ThInfoParamPool.ReturnObject(MetaCoroutineContList[num2], ThContainerPool);
			MetaCoroutineContList.RemoveAt(num2);
		}
		MetaCoroutineContList.Clear();
		if (YieldCoroutine != null)
		{
			ThreadManager.StopCoroutine(YieldCoroutine);
		}
		DebugYieldResLevel = -1;
	}

	public void SetTerrainVisible(bool IsTerrainVisible)
	{
		if (Camera.main != null)
		{
			Transform transform = Camera.main.transform;
			Transform transform2;
			if ((transform2 = transform.Find("NearCamera")) != null)
			{
				if (IsTerrainVisible)
				{
					transform2.GetComponent<Camera>().cullingMask = transform2.GetComponent<Camera>().cullingMask | 0x10000000;
				}
				else
				{
					transform2.GetComponent<Camera>().cullingMask = transform2.GetComponent<Camera>().cullingMask & -268435457;
				}
			}
			transform2 = transform;
			if (transform != null)
			{
				if (IsTerrainVisible)
				{
					transform2.GetComponent<Camera>().cullingMask = transform2.GetComponent<Camera>().cullingMask | 0x10000000;
				}
				else
				{
					transform2.GetComponent<Camera>().cullingMask = transform2.GetComponent<Camera>().cullingMask & -268435457;
				}
			}
		}
		setTerrainGOState(IsTerrainVisible);
		setAllCollidersOnOff(IsTerrainVisible);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPlayerCurrentPosition(float _posX, float _posY)
	{
		CurrentPlayerPosVec.Set(_posX, _posY);
		CurrentPlayerExtendedPosVec.Set(_posX, _posY);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setAllCollidersOnOff(bool _bColliderEnabled)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setResLevelIdCollidersOnOff(int ResLevelId, bool IsColliderEnabled)
	{
		MainChunkMap[CurMapId].ChunkMapInfoArray[ResLevelId].IsColliderEnabled = IsColliderEnabled;
		for (int i = 0; i < ChunkDataList[ResLevelId].Count; i++)
		{
			ChunkDataList[ResLevelId].array[i].CellObj.GetComponent<Collider>().enabled = IsColliderEnabled;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetDistantChunkFromStack(DistantChunkMap _BaseChunkMap, int _ResLevel, Vector2i _CellIdVector, Vector2 _TerrainOriginCoor, float[] _EdgeResFactor, Stack<GameObject>[] _WaterPlaneGameObjStack)
	{
		DistantChunk distantChunk;
		if (DistantChunkStack[_ResLevel].Count > 0)
		{
			distantChunk = DistantChunkStack[_ResLevel].array[0];
			DistantChunkStack[_ResLevel].RemoveAt(0);
			distantChunk.ResetDistantChunkSameResLevel(_BaseChunkMap, _CellIdVector, _TerrainOriginCoor, _EdgeResFactor);
		}
		else
		{
			distantChunk = new DistantChunk(_BaseChunkMap, _ResLevel, _CellIdVector, _TerrainOriginCoor, _EdgeResFactor, _WaterPlaneGameObjStack);
		}
		ChunkDataList[_ResLevel].Add(distantChunk);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DistantChunk GetDistantChunkFromStack(DistantChunkMap _BaseChunkMap, int _ResLevel, Vector2i _CellIdVector, Vector2 _TerrainOriginCoor, float[] _EdgeResFactor, Stack<GameObject>[] _WaterPlaneGameObjStack, OptimizedList<DistantChunk>[] CurChunkDataList)
	{
		DistantChunkMapInfo distantChunkMapInfo = _BaseChunkMap.ChunkMapInfoArray[_ResLevel];
		DistantChunk distantChunk;
		if (DistantChunkStack[distantChunkMapInfo.ChunkDataListResLevel].Count > 0)
		{
			if (DistantChunkStack[distantChunkMapInfo.ChunkDataListResLevel].array[0].IsFreeToUse && !DistantChunkStack[distantChunkMapInfo.ChunkDataListResLevel].array[0].IsOnActivationProcess)
			{
				distantChunk = DistantChunkStack[distantChunkMapInfo.ChunkDataListResLevel].array[0];
				DistantChunkStack[distantChunkMapInfo.ChunkDataListResLevel].RemoveAt(0);
				distantChunk.ResetDistantChunkSameResLevel(_BaseChunkMap, _CellIdVector, _TerrainOriginCoor, _EdgeResFactor);
			}
			else
			{
				distantChunk = new DistantChunk(_BaseChunkMap, _ResLevel, _CellIdVector, _TerrainOriginCoor, _EdgeResFactor, _WaterPlaneGameObjStack);
				SetDefaultNewGameObj(distantChunk);
			}
		}
		else
		{
			distantChunk = new DistantChunk(_BaseChunkMap, _ResLevel, _CellIdVector, _TerrainOriginCoor, _EdgeResFactor, _WaterPlaneGameObjStack);
			SetDefaultNewGameObj(distantChunk);
		}
		CurChunkDataList[_ResLevel].Add(distantChunk);
		return distantChunk;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDefaultNewGameObj(DistantChunk CurChunk)
	{
		_ = CurChunk.ChunkMapInfo.ChunkDataListResLevel;
		CurChunk.CellObj = new GameObject("DC", typeof(MeshRenderer), typeof(MeshFilter));
		CurChunk.CellObj.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
		CurChunk.CellObj.GetComponent<Renderer>().receiveShadows = false;
		CurChunk.CellObj.transform.parent = MainChunkMap[CurMapId].ParentGameObject.transform;
		MeshFilter component = CurChunk.CellObj.GetComponent<MeshFilter>();
		if (component.mesh != null)
		{
			UnityEngine.Object.Destroy(component.mesh);
		}
		component.mesh = new Mesh();
		Mesh mesh = component.mesh;
		mesh.name = "DC";
		if (OcclusionManager.Instance.cullDistantTerrain)
		{
			Occludee.Add(CurChunk.CellObj);
		}
		mesh.subMeshCount = 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void putDistantChunkOnStack(DistantChunk _DistantChunk, bool _AlsoRemoveInChunkDataList)
	{
		if (_DistantChunk != null && _DistantChunk.CellObj != null)
		{
			_DistantChunk.CellObj.SetActive(value: false);
			_DistantChunk.ActivateWaterPlane(0f, IsActive: false);
			_DistantChunk.IsOnActivationProcess = false;
			_DistantChunk.IsMeshUpdated = false;
			DistantChunkStack[_DistantChunk.ResLevel].Add(_DistantChunk);
			if (_AlsoRemoveInChunkDataList)
			{
				ChunkDataList[_DistantChunk.ResLevel].Remove(_DistantChunk);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void putDistantChunkOnStack(int _ResLevel, int _DistantChunkListId)
	{
		if (ChunkDataList[_ResLevel].array[_DistantChunkListId] != null)
		{
			ChunkDataList[_ResLevel].array[_DistantChunkListId].CellObj.SetActive(value: false);
			ChunkDataList[_ResLevel].array[_DistantChunkListId].ActivateWaterPlane(0f, IsActive: false);
			ChunkDataList[_ResLevel].array[_DistantChunkListId].IsOnActivationProcess = false;
			ChunkDataList[_ResLevel].array[_DistantChunkListId].IsMeshUpdated = false;
			DistantChunkStack[_ResLevel].Add(ChunkDataList[_ResLevel].array[_DistantChunkListId]);
			ChunkDataList[_ResLevel].RemoveAt(_DistantChunkListId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void putDistantChunkOnStack(int _ResLevel, int _DistantChunkListId, OptimizedList<DistantChunk>[] CurChunkDataList)
	{
		if (CurChunkDataList[_ResLevel].array[_DistantChunkListId] != null)
		{
			if (CurChunkDataList[_ResLevel].array[_DistantChunkListId].CellObj != null)
			{
				CurChunkDataList[_ResLevel].array[_DistantChunkListId].CellObj.SetActive(value: false);
			}
			CurChunkDataList[_ResLevel].array[_DistantChunkListId].IsMeshUpdated = false;
			CurChunkDataList[_ResLevel].array[_DistantChunkListId].ActivateWaterPlane(0f, IsActive: false);
			CurChunkDataList[_ResLevel].array[_DistantChunkListId].IsOnActivationProcess = false;
			DistantChunkStack[CurChunkDataList[_ResLevel].array[_DistantChunkListId].ChunkMapInfo.ChunkDataListResLevel].Add(CurChunkDataList[_ResLevel].array[_DistantChunkListId]);
			CurChunkDataList[_ResLevel].RemoveAt(_DistantChunkListId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void putDistantChunkOnStack(DistantChunk _DistantChunk, bool _AlsoRemoveInChunkDataList, OptimizedList<DistantChunk>[] CurChunkDataList)
	{
		if (_DistantChunk != null)
		{
			if (_DistantChunk.CellObj != null)
			{
				_DistantChunk.CellObj.SetActive(value: false);
			}
			_DistantChunk.ActivateWaterPlane(0f, IsActive: false);
			_DistantChunk.IsOnActivationProcess = false;
			_DistantChunk.IsMeshUpdated = false;
			DistantChunkStack[_DistantChunk.ChunkMapInfo.ChunkDataListResLevel].Add(_DistantChunk);
			if (_AlsoRemoveInChunkDataList)
			{
				CurChunkDataList[_DistantChunk.ResLevel].Remove(_DistantChunk);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetChunkDataList(int _MapId)
	{
		for (int i = 0; i < ChunkDataList.Length; i++)
		{
			for (int j = 0; j < ChunkDataList[i].Count; j++)
			{
				putDistantChunkOnStack(i, j);
			}
		}
		ChunkDataList = new OptimizedList<DistantChunk>[MainChunkMap[_MapId].NbResLevel];
		for (int k = 0; k < MainChunkMap[_MapId].NbResLevel; k++)
		{
			ChunkDataList[k] = new OptimizedList<DistantChunk>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetChunkDataList(OptimizedList<DistantChunk>[] ChunkDataListBackground)
	{
		for (int i = 0; i < ChunkDataList.Length; i++)
		{
			for (int j = 0; j < ChunkDataList[i].Count; j++)
			{
				putDistantChunkOnStack(i, j);
			}
		}
		ChunkDataList = new OptimizedList<DistantChunk>[ChunkDataListBackground.Length];
		for (int k = 0; k < ChunkDataListBackground.Length; k++)
		{
			ChunkDataList[k] = ChunkDataListBackground[k];
			ChunkDataListBackground[k] = new OptimizedList<DistantChunk>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCurrentPosOnMap()
	{
		Vector2i vector2i = default(Vector2i);
		Vector2i vector2i2 = default(Vector2i);
		Vector2 vector = default(Vector2);
		vector = CurrentPlayerExtendedPosVec - MainChunkMap[CurMapId].TerrainOrigin;
		for (int i = 0; i < NbResLevel; i++)
		{
			int num = MainChunkMap[CurMapId].FindOutsideFromChunk(vector, i);
			if (num < 0)
			{
				continue;
			}
			MetaThreadContList.Add(ThInfoParamPool.GetObject(MainChunkMap[CurMapId], i, num));
			MetaThreadContList.Last().IsAsynchronous = false;
			DistantChunkMapInfo distantChunkMapInfo = MainChunkMap[CurMapId].ChunkMapInfoArray[i];
			int resLevel = i;
			DistantChunkMapInfo distantChunkMapInfo2;
			int resLevel2;
			if (i < NbResLevel - 1)
			{
				distantChunkMapInfo2 = MainChunkMap[CurMapId].ChunkMapInfoArray[i + 1];
				resLevel2 = i + 1;
			}
			else
			{
				distantChunkMapInfo2 = MainChunkMap[CurMapId].ChunkMapInfoArray[i];
				resLevel2 = i;
			}
			vector2i.Set(Convert.ToInt32(distantChunkMapInfo.ShiftVec.x / distantChunkMapInfo2.ChunkWidth), Convert.ToInt32(distantChunkMapInfo.ShiftVec.y / distantChunkMapInfo2.ChunkWidth));
			vector2i2.Set(Convert.ToInt32(distantChunkMapInfo.ShiftVec.x / distantChunkMapInfo.ChunkWidth), Convert.ToInt32(distantChunkMapInfo.ShiftVec.y / distantChunkMapInfo.ChunkWidth));
			if (i < NbResLevel - 1)
			{
				MetaThreadContList.Last().CntForwardChunkToDeleteId = 0;
				MetaThreadContList.Last().LengthForwardChunkToDeleteId = 0;
				for (int j = 0; j < distantChunkMapInfo.ChunkToDelete[num].Length; j++)
				{
					int num2 = findDistantChunkInList(distantChunkMapInfo.ChunkToDelete[num][j] + vector2i, i + 1);
					if (num2 >= 0)
					{
						MetaThreadContList.Last().ForwardChunkToDeleteIdA[j] = ChunkDataList[i + 1].array[num2];
						MetaThreadContList.Last().LengthForwardChunkToDeleteId++;
						ChunkDataList[i + 1].array[num2].IsOnActivationProcess = true;
						ChunkDataList[i + 1].array[num2].IsOnSeamCorrectionProcess = false;
					}
				}
			}
			MetaThreadContList.Last().CntBackwardChunkToDeleteId = 0;
			MetaThreadContList.Last().LengthBackwardChunkToDeleteId = 0;
			for (int j = 0; j < distantChunkMapInfo.ChunkToConvDel[num].Length; j++)
			{
				int num2 = findDistantChunkInList(distantChunkMapInfo.ChunkToConvDel[num][j] + vector2i2, i);
				if (num2 >= 0)
				{
					MetaThreadContList.Last().BackwardChunkToDeleteIdA[j] = ChunkDataList[i].array[num2];
					MetaThreadContList.Last().LengthBackwardChunkToDeleteId++;
					ChunkDataList[i].array[num2].IsOnActivationProcess = true;
					ChunkDataList[i].array[num2].IsOnSeamCorrectionProcess = false;
				}
			}
			MetaThreadContList.Last().LengthThreadContList = 0;
			for (int j = 0; j < distantChunkMapInfo.ChunkToAdd[num].Length; j++)
			{
				EdgeResFactor[0] = distantChunkMapInfo.ChunkToAddEdgeFactor[num][j].x;
				EdgeResFactor[1] = distantChunkMapInfo.ChunkToAddEdgeFactor[num][j].y;
				EdgeResFactor[2] = distantChunkMapInfo.ChunkToAddEdgeFactor[num][j].z;
				EdgeResFactor[3] = distantChunkMapInfo.ChunkToAddEdgeFactor[num][j].w;
				GetDistantChunkFromStack(MainChunkMap[CurMapId], resLevel, distantChunkMapInfo.ChunkToAdd[num][j] + vector2i2, MainChunkMap[CurMapId].TerrainOrigin, EdgeResFactor, WaterPlaneGameObjStack);
				MetaThreadContList.Last().ThreadContListA[MetaThreadContList.Last().LengthThreadContList] = ThContainerPool.GetObject(this, ChunkDataList[i].Last(), BaseMesh[i], ChunkDataList[i].Last().WasReset);
				MetaThreadContList.Last().LengthThreadContList++;
				ChunkDataList[i].Last().IsOnActivationProcess = true;
			}
			MetaThreadContList.Last().CntThreadContList = 0;
			if (i < NbResLevel - 1)
			{
				for (int j = 0; j < distantChunkMapInfo.ChunkToConvAdd[num].Length; j++)
				{
					EdgeResFactor[0] = distantChunkMapInfo.ChunkToConvAddEdgeFactor[num][j].x;
					EdgeResFactor[1] = distantChunkMapInfo.ChunkToConvAddEdgeFactor[num][j].y;
					EdgeResFactor[2] = distantChunkMapInfo.ChunkToConvAddEdgeFactor[num][j].z;
					EdgeResFactor[3] = distantChunkMapInfo.ChunkToConvAddEdgeFactor[num][j].w;
					GetDistantChunkFromStack(MainChunkMap[CurMapId], resLevel2, distantChunkMapInfo.ChunkToConvAdd[num][j] + vector2i, MainChunkMap[CurMapId].TerrainOrigin, EdgeResFactor, WaterPlaneGameObjStack);
					MetaThreadContList.Last().ThreadContListA[MetaThreadContList.Last().LengthThreadContList] = ThContainerPool.GetObject(this, ChunkDataList[i + 1].Last(), BaseMesh[i + 1], ChunkDataList[i + 1].Last().WasReset);
					MetaThreadContList.Last().LengthThreadContList++;
					ChunkDataList[i + 1].Last().IsOnActivationProcess = true;
				}
			}
			if (i < NbResLevel - 1)
			{
				for (int j = 0; j < distantChunkMapInfo.ChunkEdgeToOwnResLevel[num].Length; j++)
				{
					int num3 = 0;
					for (int k = 0; k < distantChunkMapInfo.ChunkEdgeToOwnResLevel[num][j].Length; k++)
					{
						int num2 = findDistantChunkInList(distantChunkMapInfo.ChunkEdgeToOwnResLevel[num][j][k] + vector2i2, i);
						int num4 = distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[num][j][k];
						if (num2 > 0)
						{
							MetaThreadContList.Last().ForwardChunkSeamToAdjust[j][num3] = ChunkDataList[i].array[num2];
							MetaThreadContList.Last().ForwardEdgeId[j][num3] = num4;
							num3++;
							ChunkDataList[i].array[num2].IsOnSeamCorrectionProcess = true;
						}
					}
					MetaThreadContList.Last().SDLengthForwardChunkSeamToAdjust[j] = num3;
				}
				MetaThreadContList.Last().FDLengthForwardChunkSeamToAdjust = distantChunkMapInfo.ChunkEdgeToOwnResLevel[num].Length;
			}
			if (i < NbResLevel - 1)
			{
				for (int j = 0; j < distantChunkMapInfo.ChunkEdgeToNextResLevel[num].Length; j++)
				{
					int k = 0;
					int num3 = 0;
					for (; k < distantChunkMapInfo.ChunkEdgeToNextResLevel[num][j].Length; k++)
					{
						int num2 = findDistantChunkInList(distantChunkMapInfo.ChunkEdgeToNextResLevel[num][j][k] + vector2i2, i);
						int num4 = distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[num][j][k];
						if (num2 > 0)
						{
							MetaThreadContList.Last().BackwardChunkSeamToAdjust[j][num3] = ChunkDataList[i].array[num2];
							MetaThreadContList.Last().BackwardEdgeId[j][num3] = num4;
							num3++;
							ChunkDataList[i].array[num2].IsOnSeamCorrectionProcess = true;
						}
					}
					MetaThreadContList.Last().SDLengthBackwardChunkSeamToAdjust[j] = num3;
				}
				MetaThreadContList.Last().FDLengthBackwardChunkSeamToAdjust = distantChunkMapInfo.ChunkEdgeToNextResLevel[num].Length;
			}
			MainChunkMap[CurMapId].UpdatePlayerPos(i, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void threadManagementUpdate()
	{
		int num = 0;
		for (int i = 0; i < MetaThreadContList.Count; i++)
		{
			if (MetaThreadContList[i].IsThreadDone)
			{
				MetaCoroutineContList.Add(MetaThreadContList[i]);
				IdToRemove[num] = i;
				num++;
			}
		}
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			MetaThreadContList.RemoveAt(IdToRemove[num2]);
		}
		if (MetaCoroutineContList.Count > 0 && MetaCoroutineContList[0].IsCoroutineDone)
		{
			ThInfoParamPool.ReturnObject(MetaCoroutineContList[0], ThContainerPool);
			MetaCoroutineContList.RemoveAt(0);
		}
		if (MetaCoroutineContList.Count > 0 && DebugYieldResLevel < 0 && !MetaCoroutineContList[0].IsCoroutineDone)
		{
			YieldCoroutine = UpdateMetaListCoroutine(MetaCoroutineContList[0]);
			ThreadManager.StartCoroutine(YieldCoroutine);
		}
		if (ThProcessing != null && ThProcessing.IsThreadFinished())
		{
			ThProcessingPool.ReturnObject(ThProcessing);
			ThProcessing = null;
		}
		if (ThProcessing == null && MetaThreadContList.Count != 0)
		{
			ThProcessing = ThProcessingPool.GetObject(MetaThreadContList);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsSpwaningProcessOngoing()
	{
		if (MetaThreadContList.Count == 0 && MetaCoroutineContList.Count == 0)
		{
			return ThProcessing != null;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCurrentPosOnMapAsync(int _mapId, int _chunkDataListBGId)
	{
		Vector2i vector2i = default(Vector2i);
		Vector2i vector2i2 = default(Vector2i);
		Vector2 vector = default(Vector2);
		DistantChunkMap distantChunkMap = MainChunkMap[_mapId];
		vector = CurrentPlayerPosVec - distantChunkMap.TerrainOrigin;
		for (int i = 0; i < distantChunkMap.NbResLevel; i++)
		{
			int num = distantChunkMap.FindOutsideFromChunk(vector, i);
			if (num < 0)
			{
				continue;
			}
			DistantChunkMapInfo distantChunkMapInfo = distantChunkMap.ChunkMapInfoArray[i];
			int resLevel = i;
			DistantChunkMapInfo distantChunkMapInfo2;
			int resLevel2;
			if (i < distantChunkMap.NbResLevel - 1)
			{
				distantChunkMapInfo2 = distantChunkMap.ChunkMapInfoArray[i + 1];
				resLevel2 = i + 1;
			}
			else
			{
				distantChunkMapInfo2 = distantChunkMap.ChunkMapInfoArray[i];
				resLevel2 = i;
			}
			vector2i.Set(Convert.ToInt32(distantChunkMapInfo.ShiftVec.x / distantChunkMapInfo2.ChunkWidth), Convert.ToInt32(distantChunkMapInfo.ShiftVec.y / distantChunkMapInfo2.ChunkWidth));
			vector2i2.Set(Convert.ToInt32(distantChunkMapInfo.ShiftVec.x / distantChunkMapInfo.ChunkWidth), Convert.ToInt32(distantChunkMapInfo.ShiftVec.y / distantChunkMapInfo.ChunkWidth));
			if (i < distantChunkMap.NbResLevel - 1)
			{
				for (int j = 0; j < distantChunkMapInfo.ChunkToDelete[num].Length; j++)
				{
					int num2 = findDistantChunkInList(distantChunkMapInfo.ChunkToDelete[num][j] + vector2i, i + 1, ChunkDataListBackGround[_chunkDataListBGId]);
					if (num2 > 0)
					{
						putDistantChunkOnStack(i + 1, num2, ChunkDataListBackGround[_chunkDataListBGId]);
					}
				}
			}
			for (int k = 0; k < distantChunkMapInfo.ChunkToConvDel[num].Length; k++)
			{
				int num2 = findDistantChunkInList(distantChunkMapInfo.ChunkToConvDel[num][k] + vector2i2, i, ChunkDataListBackGround[_chunkDataListBGId]);
				if (num2 > 0)
				{
					putDistantChunkOnStack(i, num2, ChunkDataListBackGround[_chunkDataListBGId]);
				}
			}
			for (int l = 0; l < distantChunkMapInfo.ChunkToAdd[num].Length; l++)
			{
				EdgeResFactor[0] = distantChunkMapInfo.ChunkToAddEdgeFactor[num][l].x;
				EdgeResFactor[1] = distantChunkMapInfo.ChunkToAddEdgeFactor[num][l].y;
				EdgeResFactor[2] = distantChunkMapInfo.ChunkToAddEdgeFactor[num][l].z;
				EdgeResFactor[3] = distantChunkMapInfo.ChunkToAddEdgeFactor[num][l].w;
				GetDistantChunkFromStack(distantChunkMap, resLevel, distantChunkMapInfo.ChunkToAdd[num][l] + vector2i2, distantChunkMap.TerrainOrigin, EdgeResFactor, WaterPlaneGameObjStack, ChunkDataListBackGround[_chunkDataListBGId]);
			}
			if (i < distantChunkMap.NbResLevel - 1)
			{
				for (int m = 0; m < distantChunkMapInfo.ChunkToConvAdd[num].Length; m++)
				{
					EdgeResFactor[0] = distantChunkMapInfo.ChunkToConvAddEdgeFactor[num][m].x;
					EdgeResFactor[1] = distantChunkMapInfo.ChunkToConvAddEdgeFactor[num][m].y;
					EdgeResFactor[2] = distantChunkMapInfo.ChunkToConvAddEdgeFactor[num][m].z;
					EdgeResFactor[3] = distantChunkMapInfo.ChunkToConvAddEdgeFactor[num][m].w;
					GetDistantChunkFromStack(distantChunkMap, resLevel2, distantChunkMapInfo.ChunkToConvAdd[num][m] + vector2i, distantChunkMap.TerrainOrigin, EdgeResFactor, WaterPlaneGameObjStack, ChunkDataListBackGround[_chunkDataListBGId]);
				}
			}
			distantChunkMap.UpdatePlayerPos(i, num);
		}
	}

	public void ThreadExtraWork(DistantChunk DChunk, DistantChunkBasicMesh BMesh, bool WasReset)
	{
		if (DChunk.IsOnActivationProcess)
		{
			DChunk.ActivateObject(WasReset);
		}
	}

	public void MainExtraWork(DistantChunk DChunk, DistantChunkBasicMesh BMesh)
	{
		if (DChunk.IsOnActivationProcess)
		{
			DChunk.ActivateUnityMeshGameObject(BMesh);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UpdateMetaListCoroutine(ThreadInfoParam threadInfoParam)
	{
		DebugYieldResLevel = threadInfoParam.ResLevel;
		DistantChunkMapInfo distantChunkMapInfo = MainChunkMap[CurMapId].ChunkMapInfoArray[threadInfoParam.ResLevel];
		int NbChunkAddBackward = MainChunkMap[CurMapId].ChunkMapInfoArray[threadInfoParam.ResLevel].ChunkToConvAdd[threadInfoParam.OutId].Length;
		int NbChunkIONLC = distantChunkMapInfo.NbCurChunkInOneNextLevelChunk;
		int CorrectedNbChunkToBeUpdated = 15 + NbChunkIONLC;
		CorrectedNbChunkToBeUpdated -= CorrectedNbChunkToBeUpdated % NbChunkIONLC;
		while (threadInfoParam.LengthThreadContList - threadInfoParam.CntThreadContList > 0 && threadInfoParam != null)
		{
			int num = threadInfoParam.LengthThreadContList - threadInfoParam.CntThreadContList;
			int num2 = ((num > CorrectedNbChunkToBeUpdated) ? CorrectedNbChunkToBeUpdated : num);
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				if (threadInfoParam.ThreadContListA[threadInfoParam.CntThreadContList].DChunk == null || threadInfoParam.CntThreadContList >= threadInfoParam.LengthThreadContList)
				{
					continue;
				}
				threadInfoParam.ThreadContListA[threadInfoParam.CntThreadContList].MainExtraWork();
				if (threadInfoParam.ThreadContListA[threadInfoParam.CntThreadContList].DChunk.IsOnActivationProcess)
				{
					threadInfoParam.ThreadContListA[threadInfoParam.CntThreadContList].DChunk.IsMeshUpdated = true;
					threadInfoParam.ThreadContListA[threadInfoParam.CntThreadContList].DChunk.IsOnActivationProcess = false;
				}
				else
				{
					threadInfoParam.ThreadContListA[threadInfoParam.CntThreadContList].DChunk.IsMeshUpdated = false;
					if (threadInfoParam.ThreadContListA[threadInfoParam.CntThreadContList].DChunk.CellObj != null)
					{
						threadInfoParam.ThreadContListA[threadInfoParam.CntThreadContList].DChunk.CellObj.SetActive(value: false);
					}
				}
				threadInfoParam.ThreadContListA[threadInfoParam.CntThreadContList].DChunk.IsFreeToUse = true;
				ThContainerPool.ReturnObject(threadInfoParam.ThreadContListA[threadInfoParam.CntThreadContList], !threadInfoParam.IsAsynchronous);
				threadInfoParam.CntThreadContList++;
				num3++;
			}
			int num4 = threadInfoParam.LengthThreadContList - threadInfoParam.CntThreadContList;
			if (!threadInfoParam.IsAsynchronous)
			{
				if (num4 >= NbChunkAddBackward)
				{
					int num5 = num2 / NbChunkIONLC;
					num5 = ((num5 > threadInfoParam.LengthForwardChunkToDeleteId - threadInfoParam.CntForwardChunkToDeleteId) ? (threadInfoParam.LengthForwardChunkToDeleteId - threadInfoParam.CntForwardChunkToDeleteId) : num5);
					for (int j = 0; j < num5; j++)
					{
						putDistantChunkOnStack(threadInfoParam.ForwardChunkToDeleteIdA[threadInfoParam.CntForwardChunkToDeleteId], _AlsoRemoveInChunkDataList: true);
						threadInfoParam.CntForwardChunkToDeleteId++;
					}
				}
				else if (num4 + num3 <= NbChunkAddBackward)
				{
					int num6 = Mathf.Min(num2 * NbChunkIONLC, threadInfoParam.LengthBackwardChunkToDeleteId - threadInfoParam.CntBackwardChunkToDeleteId);
					for (int k = 0; k < num6; k++)
					{
						putDistantChunkOnStack(threadInfoParam.BackwardChunkToDeleteIdA[threadInfoParam.CntBackwardChunkToDeleteId], _AlsoRemoveInChunkDataList: true);
						threadInfoParam.CntBackwardChunkToDeleteId++;
					}
				}
				else
				{
					int num7 = (num4 + num2 - NbChunkAddBackward) / NbChunkIONLC;
					for (int l = 0; l < num7; l++)
					{
						putDistantChunkOnStack(threadInfoParam.ForwardChunkToDeleteIdA[threadInfoParam.CntForwardChunkToDeleteId], _AlsoRemoveInChunkDataList: true);
						threadInfoParam.CntForwardChunkToDeleteId++;
					}
					num7 = (num2 - (num4 + num2 - NbChunkAddBackward)) * NbChunkIONLC;
					if (num7 > threadInfoParam.LengthBackwardChunkToDeleteId - threadInfoParam.CntBackwardChunkToDeleteId)
					{
						num7 = threadInfoParam.LengthBackwardChunkToDeleteId - threadInfoParam.CntBackwardChunkToDeleteId;
					}
					for (int m = 0; m < num7; m++)
					{
						putDistantChunkOnStack(threadInfoParam.BackwardChunkToDeleteIdA[threadInfoParam.CntBackwardChunkToDeleteId], _AlsoRemoveInChunkDataList: true);
						threadInfoParam.CntBackwardChunkToDeleteId++;
					}
				}
			}
			if (!threadInfoParam.IsAsynchronous && threadInfoParam.FDLengthForwardChunkSeamToAdjust - threadInfoParam.CntForwardChunkSeamToAdjust > 0 && threadInfoParam.ResLevel < NbResLevel - 1)
			{
				int num8 = ((num4 >= NbChunkAddBackward) ? (CorrectedNbChunkToBeUpdated / NbChunkIONLC) : ((num4 + num2 > NbChunkAddBackward) ? ((num4 + num2 - NbChunkAddBackward) / NbChunkIONLC) : 0));
				for (int n = 0; n < num8; n++)
				{
					if (threadInfoParam.CntForwardChunkSeamToAdjust >= threadInfoParam.FDLengthForwardChunkSeamToAdjust)
					{
						continue;
					}
					for (int num9 = 0; num9 < threadInfoParam.SDLengthForwardChunkSeamToAdjust[threadInfoParam.CntForwardChunkSeamToAdjust]; num9++)
					{
						if (threadInfoParam.ForwardChunkSeamToAdjust[threadInfoParam.CntForwardChunkSeamToAdjust][num9] != null)
						{
							if (threadInfoParam.ForwardEdgeId[threadInfoParam.CntForwardChunkSeamToAdjust][num9] > 3)
							{
								threadInfoParam.ForwardChunkSeamToAdjust[threadInfoParam.CntForwardChunkSeamToAdjust][num9].ResetEdgeToOwnResLevel(threadInfoParam.ForwardEdgeId[threadInfoParam.CntForwardChunkSeamToAdjust][num9] / 10);
								threadInfoParam.ForwardChunkSeamToAdjust[threadInfoParam.CntForwardChunkSeamToAdjust][num9].ResetEdgeToOwnResLevel(threadInfoParam.ForwardEdgeId[threadInfoParam.CntForwardChunkSeamToAdjust][num9] % 10);
							}
							else
							{
								threadInfoParam.ForwardChunkSeamToAdjust[threadInfoParam.CntForwardChunkSeamToAdjust][num9].ResetEdgeToOwnResLevel(threadInfoParam.ForwardEdgeId[threadInfoParam.CntForwardChunkSeamToAdjust][num9]);
							}
							threadInfoParam.ForwardChunkSeamToAdjust[threadInfoParam.CntForwardChunkSeamToAdjust][num9].IsOnSeamCorrectionProcess = false;
						}
					}
					threadInfoParam.CntForwardChunkSeamToAdjust++;
				}
			}
			if (!threadInfoParam.IsAsynchronous && threadInfoParam.FDLengthBackwardChunkSeamToAdjust > 0 && threadInfoParam.ResLevel < NbResLevel - 1)
			{
				int num10 = ((num4 < NbChunkAddBackward) ? ((num4 + num2 > NbChunkAddBackward) ? (NbChunkAddBackward - num4) : num2) : 0);
				num10 = ((num10 > threadInfoParam.FDLengthBackwardChunkSeamToAdjust - threadInfoParam.CntBackwardChunkSeamToAdjust) ? (threadInfoParam.FDLengthBackwardChunkSeamToAdjust - threadInfoParam.CntBackwardChunkSeamToAdjust) : num10);
				for (int num11 = 0; num11 < num10; num11++)
				{
					if (threadInfoParam.CntBackwardChunkSeamToAdjust >= threadInfoParam.FDLengthBackwardChunkSeamToAdjust)
					{
						continue;
					}
					for (int num12 = 0; num12 < threadInfoParam.SDLengthBackwardChunkSeamToAdjust[num11]; num12++)
					{
						if (threadInfoParam.BackwardChunkSeamToAdjust[threadInfoParam.CntBackwardChunkSeamToAdjust][num12] != null)
						{
							if (threadInfoParam.BackwardEdgeId[threadInfoParam.CntBackwardChunkSeamToAdjust][num12] > 3)
							{
								threadInfoParam.BackwardChunkSeamToAdjust[threadInfoParam.CntBackwardChunkSeamToAdjust][num12].ResetEdgeToNextResLevel(threadInfoParam.BackwardEdgeId[threadInfoParam.CntBackwardChunkSeamToAdjust][num12] / 10);
								threadInfoParam.BackwardChunkSeamToAdjust[threadInfoParam.CntBackwardChunkSeamToAdjust][num12].ResetEdgeToNextResLevel(threadInfoParam.BackwardEdgeId[threadInfoParam.CntBackwardChunkSeamToAdjust][num12] % 10);
							}
							else
							{
								threadInfoParam.BackwardChunkSeamToAdjust[threadInfoParam.CntBackwardChunkSeamToAdjust][num12].ResetEdgeToNextResLevel(threadInfoParam.BackwardEdgeId[threadInfoParam.CntBackwardChunkSeamToAdjust][num12]);
							}
							threadInfoParam.BackwardChunkSeamToAdjust[threadInfoParam.CntBackwardChunkSeamToAdjust][num12].IsOnSeamCorrectionProcess = false;
						}
					}
					threadInfoParam.CntBackwardChunkSeamToAdjust++;
				}
			}
			yield return null;
		}
		DebugYieldResLevel = -1;
		threadInfoParam.IsCoroutineDone = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int findDistantChunkInList(long _CellKey, int _ResLevel)
	{
		int i;
		for (i = 0; i < ChunkDataList[_ResLevel].Count && _CellKey != ChunkDataList[_ResLevel].array[i].CellKey; i++)
		{
		}
		if (i < ChunkDataList[_ResLevel].Count)
		{
			return i;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int findDistantChunkInList(Vector2i _IntPosVec, int _ResLevel)
	{
		long cellKeyIdFromIdVector = DistantChunk.GetCellKeyIdFromIdVector(_IntPosVec);
		int i;
		for (i = 0; i < ChunkDataList[_ResLevel].Count && cellKeyIdFromIdVector != ChunkDataList[_ResLevel].array[i].CellKey; i++)
		{
		}
		if (i < ChunkDataList[_ResLevel].Count)
		{
			return i;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int findDistantChunkInList(Vector2i _IntPosVec, int _ResLevel, OptimizedList<DistantChunk>[] CurChunkDataList)
	{
		long cellKeyIdFromIdVector = DistantChunk.GetCellKeyIdFromIdVector(_IntPosVec);
		int i;
		for (i = 0; i < CurChunkDataList[_ResLevel].Count && cellKeyIdFromIdVector != CurChunkDataList[_ResLevel].array[i].CellKey; i++)
		{
		}
		if (i < CurChunkDataList[_ResLevel].Count)
		{
			return i;
		}
		return -1;
	}

	public bool ActivateChunk(int _chunkX, int _chunkZ, bool IsActive)
	{
		int x = _chunkX - TerrainOriginIntCoor.x;
		int y = _chunkZ - TerrainOriginIntCoor.y;
		Vector2i intPosVec = new Vector2i(x, y);
		Vector2i locPosId = new Vector2i(_chunkX, _chunkZ);
		DistantChunk distantChunk;
		if (IsOnMapSyncProcess)
		{
			int num;
			if ((num = findDistantChunkInList(intPosVec, 0, ChunkDataListBackGround[0])) < 0)
			{
				UpdateChunkActivationCache(locPosId, IsActive, IsChunkFound: false);
				return false;
			}
			distantChunk = ChunkDataListBackGround[0][0].array[num];
		}
		else
		{
			int num;
			if ((num = findDistantChunkInList(intPosVec, 0)) < 0)
			{
				UpdateChunkActivationCache(locPosId, IsActive, IsChunkFound: false);
				return false;
			}
			distantChunk = ChunkDataList[0].array[num];
		}
		UpdateChunkActivationCache(locPosId, IsActive, IsChunkFound: true);
		distantChunk.IsChunkActivated = IsActive;
		if (distantChunk.CellObj != null)
		{
			distantChunk.CellObj.SetActive(IsActive);
			return true;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateChunkActivationCache(Vector2i LocPosId, bool IsActive, bool IsChunkFound)
	{
		int hashCode = LocPosId.GetHashCode();
		if (ChunkActivationCacheDic.TryGetValue(hashCode, out var value))
		{
			value.IsActive = IsActive;
		}
		else
		{
			ChunkActivationCacheDic.Add(hashCode, new ChunkStateHelper(LocPosId.x, LocPosId.y, IsActive));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessChunkActivationCache(Vector3 PlayerPos)
	{
		DistantChunkMapInfo distantChunkMapInfo = MainChunkMap[CurMapId].ChunkMapInfoArray[0];
		int num = -distantChunkMapInfo.LLIntArea.x;
		_ = new Vector2(distantChunkMapInfo.ShiftVec.x + (float)(distantChunkMapInfo.LLIntArea.x + TerrainOriginIntCoor.x) * distantChunkMapInfo.ChunkWidth, distantChunkMapInfo.ShiftVec.y + (float)(distantChunkMapInfo.LLIntArea.y + TerrainOriginIntCoor.y) * distantChunkMapInfo.ChunkWidth) - Vector2.one * distantChunkMapInfo.LLIntArea.x * 2f * distantChunkMapInfo.ChunkWidth;
		int num2 = (int)distantChunkMapInfo.ChunkWidth / 16;
		Vector2 vector = MainChunkMap[CurMapId].TerrainOrigin * num2;
		_ = Vector2i.zero;
		int num3 = ((int)distantChunkMapInfo.ShiftVec.x >> 4) + distantChunkMapInfo.LLIntArea.x * num2 + (int)vector.x;
		int num4 = ((int)distantChunkMapInfo.ShiftVec.y >> 4) + distantChunkMapInfo.LLIntArea.y * num2 + (int)vector.y;
		int num5 = num * 2 * num2;
		int num6 = ((int)PlayerPos.x >> 4) - num - 1;
		int num7 = ((int)PlayerPos.z >> 4) - num - 1;
		ChunkToActivateList.Clear();
		OptimizedList<int> optimizedList = new OptimizedList<int>();
		foreach (KeyValuePair<int, ChunkStateHelper> item in ChunkActivationCacheDic)
		{
			int num8 = item.Value.PosX - num3;
			int num9 = item.Value.PosZ - num4;
			int num10 = item.Value.PosX - num6;
			int num11 = item.Value.PosZ - num7;
			bool flag = false;
			if (num10 < 0 || num10 > num5 + 2 || (num11 < 0 && num11 > num5 + 2))
			{
				optimizedList.Add(item.Key);
				flag = true;
			}
			if (!flag && num8 >= 0 && num8 < num5 && num9 >= 0 && num9 < num5)
			{
				ChunkToActivateList.Add(item.Value);
			}
		}
		for (int i = 0; i < optimizedList.Count; i++)
		{
			ChunkActivationCacheDic.Remove(optimizedList.array[i]);
		}
		for (int j = 0; j < ChunkToActivateList.Count; j++)
		{
			ChunkStateHelper chunkStateHelper = ChunkToActivateList.array[j];
			ActivateChunk(chunkStateHelper.PosX, chunkStateHelper.PosZ, chunkStateHelper.IsActive);
		}
		if (!IsSpwaningProcessOngoing() && IsPlayerPosCacheEmpty())
		{
			ChunkActivationCacheDic.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPlayerPosCacheEmpty()
	{
		return PlPosCache.Count == 0;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class DistantChunkMap
{
	public static Vector3 cShiftTerrainVector = new Vector3(0.5f, 0.5f, 0.5f);

	public const float Epsilon = 1E-05f;

	public Vector2 WorldSize;

	public Vector2 TerrainOrigin;

	public int NbResLevel;

	public int TotNbOfChunk;

	public int LayerId;

	public static DelegateGetTerrainHeight TGHeightFunc;

	public WorldCreationData wcd;

	public GameObject ParentGameObject;

	public int MaxNbChunkToDelete;

	public int MaxNbChunkToAdd;

	public int MaxNbChunkToConvDel;

	public int MaxNbChunkToConvAdd;

	public int MaxNbChunkEdgeToOwnResLevel;

	public int MaxNbChunkEdgeToNextResLevel;

	public DistantChunkMapInfo[] ChunkMapInfoArray;

	public DistantChunkMap(Vector2 _worldSize, float[] _ResRadius, float[] _ChunkWidth, int[] _ChunkResolution, int[] _ColliderResolution, int[] _ChunkDataListResLevel, int _LayerId, DelegateGetTerrainHeight _TGHeightFunc, WorldCreationData _wcd, GameObject _ParentGameObject, Vector3[] ChunkExtraShiftVector)
	{
		WorldSize = _worldSize;
		NbResLevel = _ResRadius.Length;
		LayerId = _LayerId;
		TGHeightFunc = _TGHeightFunc;
		wcd = _wcd;
		ParentGameObject = _ParentGameObject;
		TerrainOrigin = Vector2.zero;
		ChunkMapInfoArray = new DistantChunkMapInfo[NbResLevel];
		int[] array = new int[NbResLevel];
		float[] array2 = new float[NbResLevel];
		for (int i = 0; i < NbResLevel - 1; i++)
		{
			array[i] = (int)(Math.Truncate(_ResRadius[i] / _ChunkWidth[i + 1] - 1E-05f) + 1.0) * (int)Math.Truncate(_ChunkWidth[i + 1] / _ChunkWidth[i] + 1E-05f);
			array2[i] = (float)array[i] * _ChunkWidth[i];
		}
		array[NbResLevel - 1] = (int)Math.Truncate(_ResRadius[NbResLevel - 1] / _ChunkWidth[NbResLevel - 1] - 1E-05f) + 1;
		array2[NbResLevel - 1] = (float)array[NbResLevel - 1] * _ChunkWidth[NbResLevel - 1];
		for (int i = 0; i < NbResLevel; i++)
		{
			ChunkMapInfoArray[i] = new DistantChunkMapInfo();
			ChunkMapInfoArray[i].ChunkResolution = _ChunkResolution[i];
			ChunkMapInfoArray[i].ColliderResolution = _ColliderResolution[i];
			ChunkMapInfoArray[i].IsColliderEnabled = false;
			ChunkMapInfoArray[i].ResRadius = array2[i];
			ChunkMapInfoArray[i].IntResRadius = array[i];
			ChunkMapInfoArray[i].ChunkWidth = _ChunkWidth[i];
			ChunkMapInfoArray[i].UnitStep = _ChunkWidth[i] / (float)(_ChunkResolution[i] - 1);
			ChunkMapInfoArray[i].ResLevel = i;
			ChunkMapInfoArray[i].LayerId = LayerId;
			ChunkMapInfoArray[i].ChunkDataListResLevel = _ChunkDataListResLevel[i];
			ChunkMapInfoArray[i].ShiftVec = new Vector2(0f, 0f);
			ChunkMapInfoArray[i].ChunkExtraShiftVector = ChunkExtraShiftVector[i];
			int num = (int)(ChunkMapInfoArray[i].ResRadius / ChunkMapInfoArray[i].ChunkWidth + 1E-05f);
			ChunkMapInfoArray[i].LLIntArea = new Vector2i(-num, -num);
			int chunkResolution = ChunkMapInfoArray[i].ChunkResolution;
			ChunkMapInfoArray[i].SouthMap = new int[chunkResolution];
			ChunkMapInfoArray[i].WestMap = new int[chunkResolution];
			ChunkMapInfoArray[i].NorthMap = new int[chunkResolution];
			ChunkMapInfoArray[i].EastMap = new int[chunkResolution];
			int num2 = 0;
			int num3 = (chunkResolution - 1) * chunkResolution;
			int num4 = 0;
			while (num4 < chunkResolution)
			{
				ChunkMapInfoArray[i].SouthMap[num4] = num2;
				ChunkMapInfoArray[i].EastMap[num4] = num3;
				ChunkMapInfoArray[i].NorthMap[num4] = chunkResolution * chunkResolution - 1 - num2;
				ChunkMapInfoArray[i].WestMap[num4] = chunkResolution - 1 - num4;
				num4++;
				num2 += chunkResolution;
				num3++;
			}
			ChunkMapInfoArray[i].EdgeMap = new int[4][];
			ChunkMapInfoArray[i].EdgeMap[0] = ChunkMapInfoArray[i].SouthMap;
			ChunkMapInfoArray[i].EdgeMap[1] = ChunkMapInfoArray[i].EastMap;
			ChunkMapInfoArray[i].EdgeMap[2] = ChunkMapInfoArray[i].NorthMap;
			ChunkMapInfoArray[i].EdgeMap[3] = ChunkMapInfoArray[i].WestMap;
			ChunkMapInfoArray[i].BaseMesh = createBaseDataMeshFold(ChunkMapInfoArray[i]);
		}
		for (int i = 0; i < NbResLevel; i++)
		{
			ChunkMapInfoArray[i].ChunkTriggerArea = new Vector4[8];
			ChunkMapInfoArray[i].ChunkToDelete = new Vector2i[8][];
			ChunkMapInfoArray[i].ChunkToAdd = new Vector2i[8][];
			ChunkMapInfoArray[i].ChunkToConvDel = new Vector2i[8][];
			ChunkMapInfoArray[i].ChunkToConvAdd = new Vector2i[8][];
			ChunkMapInfoArray[i].ChunkToAddEdgeFactor = new Vector4[8][];
			ChunkMapInfoArray[i].ChunkToConvAddEdgeFactor = new Vector4[8][];
			ChunkMapInfoArray[i].ChunkEdgeToOwnResLevel = new Vector2i[8][][];
			ChunkMapInfoArray[i].ChunkEdgeToOwnRLEdgeId = new int[8][][];
			ChunkMapInfoArray[i].ChunkEdgeToNextResLevel = new Vector2i[8][][];
			ChunkMapInfoArray[i].ChunkEdgeToNextRLEdgeId = new int[8][][];
			SetChunkTrigger(i);
		}
		DistantChunkPosData distantChunkPosData = ComputeChunkPos(0, NbResLevel, 0f, array2[0], _ChunkWidth[0]);
		ChunkMapInfoArray[0].ChunkLLPos = distantChunkPosData.ChunkPos;
		ChunkMapInfoArray[0].ChunkLLIntPos = distantChunkPosData.ChunkIntPos;
		ChunkMapInfoArray[0].NeighbResLevel = distantChunkPosData.NeighbResLevel;
		ChunkMapInfoArray[0].NbChunk = ChunkMapInfoArray[0].ChunkLLIntPos.Length;
		for (int i = 1; i < NbResLevel; i++)
		{
			distantChunkPosData = ComputeChunkPos(i, NbResLevel, array2[i - 1], array2[i], _ChunkWidth[i]);
			ChunkMapInfoArray[i].ChunkLLPos = distantChunkPosData.ChunkPos;
			ChunkMapInfoArray[i].ChunkLLIntPos = distantChunkPosData.ChunkIntPos;
			ChunkMapInfoArray[i].NeighbResLevel = distantChunkPosData.NeighbResLevel;
			ChunkMapInfoArray[i].NbChunk = ChunkMapInfoArray[i].ChunkLLIntPos.Length;
		}
		for (int i = 0; i < NbResLevel; i++)
		{
			TotNbOfChunk += ChunkMapInfoArray[i].NbChunk;
			ChunkMapInfoArray[i].EdgeResFactor = new float[ChunkMapInfoArray[i].NbChunk][];
			ChunkMapInfoArray[i].NextResLevelEdgeFactor = ((i < NbResLevel - 1) ? (ChunkMapInfoArray[i + 1].UnitStep / ChunkMapInfoArray[i].UnitStep) : 1f);
			for (int num2 = 0; num2 < ChunkMapInfoArray[i].NbChunk; num2++)
			{
				ChunkMapInfoArray[i].EdgeResFactor[num2] = new float[4];
				for (int num3 = 0; num3 < 4; num3++)
				{
					int num5 = ChunkMapInfoArray[i].NeighbResLevel[num2][num3];
					ChunkMapInfoArray[i].EdgeResFactor[num2][num3] = ChunkMapInfoArray[num5].UnitStep / ChunkMapInfoArray[i].UnitStep;
				}
			}
		}
		setArrayMaxSize();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setArrayMaxSize()
	{
		int[] array = new int[6];
		for (int i = 0; i < NbResLevel; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				if (array[0] < ChunkMapInfoArray[i].ChunkToDelete[j].Length)
				{
					array[0] = ChunkMapInfoArray[i].ChunkToDelete[j].Length;
				}
				if (array[1] < ChunkMapInfoArray[i].ChunkToAdd[j].Length)
				{
					array[1] = ChunkMapInfoArray[i].ChunkToAdd[j].Length;
				}
				if (array[2] < ChunkMapInfoArray[i].ChunkToConvDel[j].Length)
				{
					array[2] = ChunkMapInfoArray[i].ChunkToConvDel[j].Length;
				}
				if (array[3] < ChunkMapInfoArray[i].ChunkToConvAdd[j].Length)
				{
					array[3] = ChunkMapInfoArray[i].ChunkToConvAdd[j].Length;
				}
				if (array[4] < ChunkMapInfoArray[i].ChunkEdgeToOwnResLevel[j].Length)
				{
					array[4] = ChunkMapInfoArray[i].ChunkEdgeToOwnResLevel[j].Length;
				}
				if (array[5] < ChunkMapInfoArray[i].ChunkEdgeToNextResLevel[j].Length)
				{
					array[5] = ChunkMapInfoArray[i].ChunkEdgeToNextResLevel[j].Length;
				}
			}
		}
		MaxNbChunkToDelete = array[0];
		MaxNbChunkToAdd = array[1];
		MaxNbChunkToConvDel = array[2];
		MaxNbChunkToConvAdd = array[3];
		MaxNbChunkEdgeToOwnResLevel = array[4];
		MaxNbChunkEdgeToNextResLevel = array[5];
	}

	public void SetChunkTrigger(int _ResLevel)
	{
		DistantChunkMapInfo distantChunkMapInfo = ChunkMapInfoArray[_ResLevel];
		DistantChunkMapInfo distantChunkMapInfo2 = ((_ResLevel < NbResLevel - 1) ? ChunkMapInfoArray[_ResLevel + 1] : ChunkMapInfoArray[_ResLevel]);
		int num = ((_ResLevel < NbResLevel - 1) ? (_ResLevel + 1) : _ResLevel);
		float num2 = distantChunkMapInfo2.UnitStep / distantChunkMapInfo.UnitStep;
		distantChunkMapInfo.ChunkTriggerArea[0].Set(-1f, -2f, 2f, 1f);
		distantChunkMapInfo.ChunkTriggerArea[1].Set(1f, -1f, 1f, 2f);
		distantChunkMapInfo.ChunkTriggerArea[2].Set(-1f, 1f, 2f, 1f);
		distantChunkMapInfo.ChunkTriggerArea[3].Set(-2f, -1f, 1f, 2f);
		distantChunkMapInfo.ChunkTriggerArea[4].Set(-2f, -2f, 1f, 1f);
		distantChunkMapInfo.ChunkTriggerArea[5].Set(1f, -2f, 1f, 1f);
		distantChunkMapInfo.ChunkTriggerArea[6].Set(1f, 1f, 1f, 1f);
		distantChunkMapInfo.ChunkTriggerArea[7].Set(-2f, 1f, 1f, 1f);
		int num3 = (int)(distantChunkMapInfo.ResRadius / distantChunkMapInfo2.ChunkWidth + 1E-05f);
		int num4 = num3 * 2;
		for (int i = 0; i < 4; i++)
		{
			distantChunkMapInfo.ChunkToDelete[i] = new Vector2i[num4];
			distantChunkMapInfo.ChunkToDelete[i + 4] = new Vector2i[num4 * 2 - 1];
		}
		for (int j = 0; j < num4; j++)
		{
			distantChunkMapInfo.ChunkToDelete[0][j] = new Vector2i(-num3 + j, -num3 - 1);
			distantChunkMapInfo.ChunkToDelete[1][j] = new Vector2i(num3, -num3 + j);
			distantChunkMapInfo.ChunkToDelete[2][j] = new Vector2i(num3 - 1 - j, num3);
			distantChunkMapInfo.ChunkToDelete[3][j] = new Vector2i(-num3 - 1, num3 - 1 - j);
		}
		distantChunkMapInfo.ChunkToDelete[4][num4 - 1] = new Vector2i(-num3 - 1, -num3 - 1);
		distantChunkMapInfo.ChunkToDelete[5][num4 - 1] = new Vector2i(num3, -num3 - 1);
		distantChunkMapInfo.ChunkToDelete[6][num4 - 1] = new Vector2i(num3, num3);
		distantChunkMapInfo.ChunkToDelete[7][num4 - 1] = new Vector2i(-num3 - 1, num3);
		for (int k = 0; k < num4 - 1; k++)
		{
			distantChunkMapInfo.ChunkToDelete[4][k] = new Vector2i(-num3 - 1, num3 - 2 - k);
			distantChunkMapInfo.ChunkToDelete[4][num4 + k] = new Vector2i(-num3 + k, -num3 - 1);
			distantChunkMapInfo.ChunkToDelete[5][k] = new Vector2i(-num3 + 1 + k, -num3 - 1);
			distantChunkMapInfo.ChunkToDelete[5][num4 + k] = new Vector2i(num3, -num3 + k);
			distantChunkMapInfo.ChunkToDelete[6][k] = new Vector2i(num3, -num3 + 1 + k);
			distantChunkMapInfo.ChunkToDelete[6][num4 + k] = new Vector2i(num3 - 1 - k, num3);
			distantChunkMapInfo.ChunkToDelete[7][k] = new Vector2i(num3 - 2 - k, num3);
			distantChunkMapInfo.ChunkToDelete[7][num4 + k] = new Vector2i(-num3 - 1, num3 - 1 - k);
		}
		for (int l = 0; l < 4; l++)
		{
			distantChunkMapInfo.ChunkToConvAdd[l] = new Vector2i[num4];
			distantChunkMapInfo.ChunkToConvAdd[l + 4] = new Vector2i[num4 * 2 - 1];
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[l] = new Vector4[num4];
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[l + 4] = new Vector4[num4 * 2 - 1];
		}
		for (int m = 0; m < num4; m++)
		{
			distantChunkMapInfo.ChunkToConvAdd[0][m] = new Vector2i(distantChunkMapInfo.ChunkToDelete[2][m].x, distantChunkMapInfo.ChunkToDelete[2][m].y - 1);
			distantChunkMapInfo.ChunkToConvAdd[1][m] = new Vector2i(distantChunkMapInfo.ChunkToDelete[3][m].x + 1, distantChunkMapInfo.ChunkToDelete[3][m].y);
			distantChunkMapInfo.ChunkToConvAdd[2][m] = new Vector2i(distantChunkMapInfo.ChunkToDelete[0][m].x, distantChunkMapInfo.ChunkToDelete[0][m].y + 1);
			distantChunkMapInfo.ChunkToConvAdd[3][m] = new Vector2i(distantChunkMapInfo.ChunkToDelete[1][m].x - 1, distantChunkMapInfo.ChunkToDelete[1][m].y);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[0][m] = new Vector4(1f / num2, 1f, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[1][m] = new Vector4(1f, 1f / num2, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[2][m] = new Vector4(1f, 1f, 1f / num2, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[3][m] = new Vector4(1f, 1f, 1f, 1f / num2);
		}
		for (int n = 0; n < num4 * 2 - 1; n++)
		{
			distantChunkMapInfo.ChunkToConvAdd[4][n] = new Vector2i(distantChunkMapInfo.ChunkToDelete[6][n].x - 1, distantChunkMapInfo.ChunkToDelete[6][n].y - 1);
			distantChunkMapInfo.ChunkToConvAdd[5][n] = new Vector2i(distantChunkMapInfo.ChunkToDelete[7][n].x + 1, distantChunkMapInfo.ChunkToDelete[7][n].y - 1);
			distantChunkMapInfo.ChunkToConvAdd[6][n] = new Vector2i(distantChunkMapInfo.ChunkToDelete[4][n].x + 1, distantChunkMapInfo.ChunkToDelete[4][n].y + 1);
			distantChunkMapInfo.ChunkToConvAdd[7][n] = new Vector2i(distantChunkMapInfo.ChunkToDelete[5][n].x - 1, distantChunkMapInfo.ChunkToDelete[5][n].y + 1);
		}
		for (int num5 = 0; num5 < num4 - 1; num5++)
		{
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[4][num5] = new Vector4(1f, 1f, 1f, 1f / num2);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[4][num5 + num4 - 1] = new Vector4(1f / num2, 1f, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[4][num4 * 2 - 2] = new Vector4(1f, 1f, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[5][num5] = new Vector4(1f / num2, 1f, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[5][num5 + num4 - 1] = new Vector4(1f, 1f / num2, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[5][num4 * 2 - 2] = new Vector4(1f, 1f, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[6][num5] = new Vector4(1f, 1f / num2, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[6][num5 + num4 - 1] = new Vector4(1f, 1f, 1f / num2, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[6][num4 * 2 - 2] = new Vector4(1f, 1f, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[7][num5] = new Vector4(1f, 1f / num2, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[7][num5 + num4 - 1] = new Vector4(1f / num2, 1f, 1f, 1f);
			distantChunkMapInfo.ChunkToConvAddEdgeFactor[7][num4 * 2 - 2] = new Vector4(1f, 1f, 1f, 1f);
		}
		int num6 = (int)(distantChunkMapInfo2.ChunkWidth / distantChunkMapInfo.ChunkWidth + 1E-05f);
		int num7 = num6 * num6;
		int num8 = distantChunkMapInfo.ChunkToDelete[0].Length;
		num4 = num8 * num7;
		distantChunkMapInfo.NbCurChunkInOneNextLevelChunk = num7;
		Vector2i[][] array = new Vector2i[4][]
		{
			new Vector2i[4]
			{
				new Vector2i(-distantChunkMapInfo.IntResRadius - num6, num),
				new Vector2i(distantChunkMapInfo.IntResRadius - 1, num),
				new Vector2i(-distantChunkMapInfo.IntResRadius - 1, _ResLevel),
				new Vector2i(-distantChunkMapInfo.IntResRadius, num)
			},
			new Vector2i[4]
			{
				new Vector2i(-distantChunkMapInfo.IntResRadius, num),
				new Vector2i(distantChunkMapInfo.IntResRadius + num6 - 1, num),
				new Vector2i(distantChunkMapInfo.IntResRadius - 1, num),
				new Vector2i(distantChunkMapInfo.IntResRadius, _ResLevel)
			},
			new Vector2i[4]
			{
				new Vector2i(distantChunkMapInfo.IntResRadius, _ResLevel),
				new Vector2i(distantChunkMapInfo.IntResRadius - 1, num),
				new Vector2i(distantChunkMapInfo.IntResRadius + num6 - 1, num),
				new Vector2i(-distantChunkMapInfo.IntResRadius, num)
			},
			new Vector2i[4]
			{
				new Vector2i(-distantChunkMapInfo.IntResRadius, num),
				new Vector2i(-distantChunkMapInfo.IntResRadius - 1, _ResLevel),
				new Vector2i(distantChunkMapInfo.IntResRadius - 1, num),
				new Vector2i(-distantChunkMapInfo.IntResRadius - num6, num)
			}
		};
		for (int num9 = 0; num9 < 4; num9++)
		{
			distantChunkMapInfo.ChunkToAdd[num9] = new Vector2i[num4];
			distantChunkMapInfo.ChunkToAdd[num9 + 4] = new Vector2i[num4 * 2 - num7];
			distantChunkMapInfo.ChunkToAddEdgeFactor[num9] = new Vector4[num4];
			distantChunkMapInfo.ChunkToAddEdgeFactor[num9 + 4] = new Vector4[num4 * 2 - num7];
		}
		for (int num10 = 0; num10 < num8; num10++)
		{
			for (int num11 = 0; num11 < num7; num11++)
			{
				for (int num12 = 0; num12 < 4; num12++)
				{
					distantChunkMapInfo.ChunkToAdd[num12][num10 * num7 + num11] = new Vector2i(distantChunkMapInfo.ChunkToDelete[num12][num10].x * num6 + num11 / num6, distantChunkMapInfo.ChunkToDelete[num12][num10].y * num6 + num11 % num6);
					distantChunkMapInfo.ChunkToAddEdgeFactor[num12][num10 * num7 + num11].Set(1f, 1f, 1f, 1f);
					if (distantChunkMapInfo.ChunkToAdd[num12][num10 * num7 + num11].y == array[num12][0].x)
					{
						distantChunkMapInfo.ChunkToAddEdgeFactor[num12][num10 * num7 + num11].x = ((array[num12][0].y == num) ? num2 : 1f);
					}
					if (distantChunkMapInfo.ChunkToAdd[num12][num10 * num7 + num11].x == array[num12][1].x)
					{
						distantChunkMapInfo.ChunkToAddEdgeFactor[num12][num10 * num7 + num11].y = ((array[num12][1].y == num) ? num2 : 1f);
					}
					if (distantChunkMapInfo.ChunkToAdd[num12][num10 * num7 + num11].y == array[num12][2].x)
					{
						distantChunkMapInfo.ChunkToAddEdgeFactor[num12][num10 * num7 + num11].z = ((array[num12][2].y == num) ? num2 : 1f);
					}
					if (distantChunkMapInfo.ChunkToAdd[num12][num10 * num7 + num11].x == array[num12][3].x)
					{
						distantChunkMapInfo.ChunkToAddEdgeFactor[num12][num10 * num7 + num11].w = ((array[num12][3].y == num) ? num2 : 1f);
					}
				}
			}
		}
		array[0] = new Vector2i[4]
		{
			new Vector2i(-distantChunkMapInfo.IntResRadius - num6, num),
			new Vector2i(distantChunkMapInfo.IntResRadius - num6 - 1, num),
			new Vector2i(distantChunkMapInfo.IntResRadius - num6 - 1, num),
			new Vector2i(-distantChunkMapInfo.IntResRadius - num6, num)
		};
		array[1] = new Vector2i[4]
		{
			new Vector2i(-distantChunkMapInfo.IntResRadius - num6, num),
			new Vector2i(distantChunkMapInfo.IntResRadius + num6 - 1, num),
			new Vector2i(distantChunkMapInfo.IntResRadius - num6 - 1, num),
			new Vector2i(-distantChunkMapInfo.IntResRadius + num6, num)
		};
		array[2] = new Vector2i[4]
		{
			new Vector2i(-distantChunkMapInfo.IntResRadius + num6, num),
			new Vector2i(distantChunkMapInfo.IntResRadius + num6 - 1, num),
			new Vector2i(distantChunkMapInfo.IntResRadius + num6 - 1, num),
			new Vector2i(-distantChunkMapInfo.IntResRadius + num6, num)
		};
		array[3] = new Vector2i[4]
		{
			new Vector2i(-distantChunkMapInfo.IntResRadius + num6, num),
			new Vector2i(distantChunkMapInfo.IntResRadius - num6 - 1, num),
			new Vector2i(distantChunkMapInfo.IntResRadius + num6 - 1, num),
			new Vector2i(-distantChunkMapInfo.IntResRadius - num6, num)
		};
		num8 = distantChunkMapInfo.ChunkToDelete[4].Length;
		for (int num13 = 0; num13 < num8; num13++)
		{
			for (int num14 = 0; num14 < num7; num14++)
			{
				for (int num15 = 4; num15 < 8; num15++)
				{
					distantChunkMapInfo.ChunkToAdd[num15][num13 * num7 + num14] = new Vector2i(distantChunkMapInfo.ChunkToDelete[num15][num13].x * num6 + num14 / num6, distantChunkMapInfo.ChunkToDelete[num15][num13].y * num6 + num14 % num6);
					distantChunkMapInfo.ChunkToAddEdgeFactor[num15][num13 * num7 + num14].Set(1f, 1f, 1f, 1f);
					if (distantChunkMapInfo.ChunkToAdd[num15][num13 * num7 + num14].y == array[num15 - 4][0].x)
					{
						distantChunkMapInfo.ChunkToAddEdgeFactor[num15][num13 * num7 + num14].x = num2;
					}
					if (distantChunkMapInfo.ChunkToAdd[num15][num13 * num7 + num14].x == array[num15 - 4][1].x)
					{
						distantChunkMapInfo.ChunkToAddEdgeFactor[num15][num13 * num7 + num14].y = num2;
					}
					if (distantChunkMapInfo.ChunkToAdd[num15][num13 * num7 + num14].y == array[num15 - 4][2].x)
					{
						distantChunkMapInfo.ChunkToAddEdgeFactor[num15][num13 * num7 + num14].z = num2;
					}
					if (distantChunkMapInfo.ChunkToAdd[num15][num13 * num7 + num14].x == array[num15 - 4][3].x)
					{
						distantChunkMapInfo.ChunkToAddEdgeFactor[num15][num13 * num7 + num14].w = num2;
					}
				}
			}
		}
		for (int num16 = 0; num16 < 4; num16++)
		{
			distantChunkMapInfo.ChunkToConvDel[num16] = new Vector2i[num4];
			distantChunkMapInfo.ChunkToConvDel[num16 + 4] = new Vector2i[num4 * 2 - num7];
		}
		num8 = distantChunkMapInfo.ChunkToConvAdd[0].Length;
		for (int num17 = 0; num17 < num8; num17++)
		{
			for (int num18 = 0; num18 < num7; num18++)
			{
				for (int num19 = 0; num19 < 4; num19++)
				{
					distantChunkMapInfo.ChunkToConvDel[num19][num17 * num7 + num18] = new Vector2i(distantChunkMapInfo.ChunkToConvAdd[num19][num17].x * num6 + num18 / num6, distantChunkMapInfo.ChunkToConvAdd[num19][num17].y * num6 + num18 % num6);
				}
			}
		}
		num8 = distantChunkMapInfo.ChunkToConvAdd[4].Length;
		for (int num20 = 0; num20 < num8; num20++)
		{
			for (int num21 = 0; num21 < num7; num21++)
			{
				for (int num22 = 4; num22 < 8; num22++)
				{
					distantChunkMapInfo.ChunkToConvDel[num22][num20 * num7 + num21] = new Vector2i(distantChunkMapInfo.ChunkToConvAdd[num22][num20].x * num6 + num21 / num6, distantChunkMapInfo.ChunkToConvAdd[num22][num20].y * num6 + num21 % num6);
				}
			}
		}
		num6 = (int)(distantChunkMapInfo2.ChunkWidth / distantChunkMapInfo.ChunkWidth + 1E-05f);
		num3 = (int)(distantChunkMapInfo.ResRadius / distantChunkMapInfo.ChunkWidth + 1E-05f);
		num4 = num3 * 2;
		int num23 = (int)(distantChunkMapInfo.ResRadius / distantChunkMapInfo2.ChunkWidth + 1E-05f) * 2;
		for (int num24 = 0; num24 < 4; num24++)
		{
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[num24] = new Vector2i[num23][];
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[num24] = new int[num23][];
			for (int num25 = 0; num25 < num23; num25++)
			{
				distantChunkMapInfo.ChunkEdgeToOwnResLevel[num24][num25] = new Vector2i[num6];
				distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[num24][num25] = new int[num6];
			}
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[num24 + 4] = new Vector2i[num23 * 2 - 1][];
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[num24 + 4] = new int[num23 * 2 - 1][];
			for (int num26 = 0; num26 < num23 * 2 - 1; num26++)
			{
				if (num26 != num23 - 1)
				{
					distantChunkMapInfo.ChunkEdgeToOwnResLevel[num24 + 4][num26] = new Vector2i[num6];
					distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[num24 + 4][num26] = new int[num6];
				}
			}
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[num24 + 4][num23 - 1] = new Vector2i[num6 * 2 - 1];
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[num24 + 4][num23 - 1] = new int[num6 * 2 - 1];
		}
		for (int num27 = 0; num27 < num23; num27++)
		{
			for (int num28 = 0; num28 < num6; num28++)
			{
				distantChunkMapInfo.ChunkEdgeToOwnResLevel[0][num27][num28] = new Vector2i(-num3 + num27 * num6 + num28, -num3);
				distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[0][num27][num28] = 0;
				distantChunkMapInfo.ChunkEdgeToOwnResLevel[1][num27][num28] = new Vector2i(num3 - 1, -num3 + num27 * num6 + num28);
				distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[1][num27][num28] = 1;
				distantChunkMapInfo.ChunkEdgeToOwnResLevel[2][num27][num28] = new Vector2i(num3 - 1 - num27 * num6 - num28, num3 - 1);
				distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[2][num27][num28] = 2;
				distantChunkMapInfo.ChunkEdgeToOwnResLevel[3][num27][num28] = new Vector2i(-num3, num3 - 1 - num27 * num6 - num28);
				distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[3][num27][num28] = 3;
			}
		}
		int num29 = num23 - 1;
		distantChunkMapInfo.ChunkEdgeToOwnResLevel[4][num29][num6 - 1] = new Vector2i(-num3, -num3);
		distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[4][num29][num6 - 1] = 30;
		distantChunkMapInfo.ChunkEdgeToOwnResLevel[5][num29][num6 - 1] = new Vector2i(num3 - 1, -num3);
		distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[5][num29][num6 - 1] = 10;
		distantChunkMapInfo.ChunkEdgeToOwnResLevel[6][num29][num6 - 1] = new Vector2i(num3 - 1, num3 - 1);
		distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[6][num29][num6 - 1] = 12;
		distantChunkMapInfo.ChunkEdgeToOwnResLevel[7][num29][num6 - 1] = new Vector2i(-num3, num3 - 1);
		distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[7][num29][num6 - 1] = 23;
		for (int num30 = 0; num30 < num6 - 1; num30++)
		{
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[4][num29][num30] = new Vector2i(-num3, num3 - 1 - num6 - num23 * num6 - num30);
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[4][num29][num30 + num6] = new Vector2i(-num3 + 1 + num23 * num6 + num30, -num3);
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[4][num29][num30] = 3;
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[4][num29][num30 + num6] = 0;
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[5][num29][num30] = new Vector2i(-num3 + num6 + num23 * num6 + num30, -num3);
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[5][num29][num30 + num6] = new Vector2i(num3 - 1, -num3 + 1 + num23 * num6 + num30);
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[5][num29][num30] = 0;
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[5][num29][num30 + num6] = 1;
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[6][num29][num30] = new Vector2i(num3 - 1, -num3 + num6 + num23 * num6 + num30);
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[6][num29][num30 + num6] = new Vector2i(num3 - 1 - 1 - num23 * num6 - num30, num3 - 1);
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[6][num29][num30] = 1;
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[6][num29][num30 + num6] = 2;
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[7][num29][num30] = new Vector2i(num3 - 1 - num6 - num23 * num6 - num30, num3 - 1);
			distantChunkMapInfo.ChunkEdgeToOwnResLevel[7][num29][num30 + num6] = new Vector2i(-num3, num3 - 1 - 1 - num23 * num6 - num30);
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[7][num29][num30] = 2;
			distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[7][num29][num30 + num6] = 3;
		}
		for (int num31 = 0; num31 < num29; num31++)
		{
			if (num31 != num29)
			{
				for (int num32 = 0; num32 < num6; num32++)
				{
					distantChunkMapInfo.ChunkEdgeToOwnResLevel[4][num31][num32] = new Vector2i(-num3, num3 - 1 - num6 - num31 * num6 - num32);
					distantChunkMapInfo.ChunkEdgeToOwnResLevel[4][num29 + num31 + 1][num32] = new Vector2i(-num3 + 1 + num31 * num6 + num32, -num3);
					distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[4][num31][num32] = 3;
					distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[4][num29 + num31 + 1][num32] = 0;
					distantChunkMapInfo.ChunkEdgeToOwnResLevel[5][num31][num32] = new Vector2i(-num3 + num6 + num31 * num6 + num32, -num3);
					distantChunkMapInfo.ChunkEdgeToOwnResLevel[5][num29 + num31 + 1][num32] = new Vector2i(num3 - 1, -num3 + 1 + num31 * num6 + num32);
					distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[5][num31][num32] = 0;
					distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[5][num29 + num31 + 1][num32] = 1;
					distantChunkMapInfo.ChunkEdgeToOwnResLevel[6][num31][num32] = new Vector2i(num3 - 1, -num3 + num6 + num31 * num6 + num32);
					distantChunkMapInfo.ChunkEdgeToOwnResLevel[6][num29 + num31 + 1][num32] = new Vector2i(num3 - 1 - 1 - num31 * num6 - num32, num3 - 1);
					distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[6][num31][num32] = 1;
					distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[6][num29 + num31 + 1][num32] = 2;
					distantChunkMapInfo.ChunkEdgeToOwnResLevel[7][num31][num32] = new Vector2i(num3 - 1 - num6 - num31 * num6 - num32, num3 - 1);
					distantChunkMapInfo.ChunkEdgeToOwnResLevel[7][num29 + num31 + 1][num32] = new Vector2i(-num3, num3 - 1 - 1 - num31 * num6 - num32);
					distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[7][num31][num32] = 2;
					distantChunkMapInfo.ChunkEdgeToOwnRLEdgeId[7][num29 + num31 + 1][num32] = 3;
				}
			}
		}
		num6 = (int)(distantChunkMapInfo2.ChunkWidth / distantChunkMapInfo.ChunkWidth + 1E-05f);
		num3 = (int)(distantChunkMapInfo.ResRadius / distantChunkMapInfo.ChunkWidth + 1E-05f);
		num4 = num3 * 2;
		num23 = (int)(distantChunkMapInfo.ResRadius / distantChunkMapInfo2.ChunkWidth + 1E-05f) * 2;
		for (int num33 = 0; num33 < 4; num33++)
		{
			distantChunkMapInfo.ChunkEdgeToNextResLevel[num33] = new Vector2i[num23][];
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[num33] = new int[num23][];
			for (int num34 = 0; num34 < num23; num34++)
			{
				distantChunkMapInfo.ChunkEdgeToNextResLevel[num33][num34] = new Vector2i[num6];
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[num33][num34] = new int[num6];
			}
			distantChunkMapInfo.ChunkEdgeToNextResLevel[num33 + 4] = new Vector2i[num23 * 2 - 1][];
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[num33 + 4] = new int[num23 * 2 - 1][];
			distantChunkMapInfo.ChunkEdgeToNextResLevel[num33 + 4][num23 - 2] = new Vector2i[0];
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[num33 + 4][num23 - 2] = new int[0];
			distantChunkMapInfo.ChunkEdgeToNextResLevel[num33 + 4][num23] = new Vector2i[0];
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[num33 + 4][num23] = new int[0];
			for (int num35 = 0; num35 < num23 - 2; num35++)
			{
				distantChunkMapInfo.ChunkEdgeToNextResLevel[num33 + 4][num35] = new Vector2i[num6];
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[num33 + 4][num35] = new int[num6];
				distantChunkMapInfo.ChunkEdgeToNextResLevel[num33 + 4][num23 + 1 + num35] = new Vector2i[num6];
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[num33 + 4][num23 + 1 + num35] = new int[num6];
			}
			distantChunkMapInfo.ChunkEdgeToNextResLevel[num33 + 4][num23 - 1] = new Vector2i[num6 * 2 - 1];
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[num33 + 4][num23 - 1] = new int[num6 * 2 - 1];
		}
		for (int num36 = 0; num36 < num23; num36++)
		{
			for (int num37 = 0; num37 < num6; num37++)
			{
				distantChunkMapInfo.ChunkEdgeToNextResLevel[0][num36][num37] = new Vector2i(num3 - 1 - num36 * num6 - num37, num3 - 1 - num6);
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[0][num36][num37] = 2;
				distantChunkMapInfo.ChunkEdgeToNextResLevel[1][num36][num37] = new Vector2i(-num3 + num6, num3 - 1 - num36 * num6 - num37);
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[1][num36][num37] = 3;
				distantChunkMapInfo.ChunkEdgeToNextResLevel[2][num36][num37] = new Vector2i(-num3 + num36 * num6 + num37, -num3 + num6);
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[2][num36][num37] = 0;
				distantChunkMapInfo.ChunkEdgeToNextResLevel[3][num36][num37] = new Vector2i(num3 - 1 - num6, -num3 + num36 * num6 + num37);
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[3][num36][num37] = 1;
			}
		}
		num29 = num23 - 1;
		distantChunkMapInfo.ChunkEdgeToNextResLevel[4][num29][num6 - 1] = new Vector2i(num3 - 1 - num6, num3 - 1 - num6);
		distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[4][num29][num6 - 1] = 12;
		distantChunkMapInfo.ChunkEdgeToNextResLevel[5][num29][num6 - 1] = new Vector2i(-num3 + num6, num3 - 1 - num6);
		distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[5][num29][num6 - 1] = 23;
		distantChunkMapInfo.ChunkEdgeToNextResLevel[6][num29][num6 - 1] = new Vector2i(-num3 + num6, -num3 + num6);
		distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[6][num29][num6 - 1] = 30;
		distantChunkMapInfo.ChunkEdgeToNextResLevel[7][num29][num6 - 1] = new Vector2i(num3 - 1 - num6, -num3 + num6);
		distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[7][num29][num6 - 1] = 10;
		for (int num38 = 0; num38 < num6 - 1; num38++)
		{
			distantChunkMapInfo.ChunkEdgeToNextResLevel[4][num29][num38] = new Vector2i(num3 - 1 - num6, num3 - 2 * num6 + num38);
			distantChunkMapInfo.ChunkEdgeToNextResLevel[4][num29][num38 + num6] = new Vector2i(num3 - 2 - num6 - num38, num3 - 1 - num6);
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[4][num29][num38] = 1;
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[4][num29][num38 + num6] = 2;
			distantChunkMapInfo.ChunkEdgeToNextResLevel[5][num29][num38] = new Vector2i(-num3 - 1 + 2 * num6 - num38, num3 - 1 - num6);
			distantChunkMapInfo.ChunkEdgeToNextResLevel[5][num29][num38 + num6] = new Vector2i(-num3 + num6, num3 - 2 - num6 - num38);
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[5][num29][num38] = 2;
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[5][num29][num38 + num6] = 3;
			distantChunkMapInfo.ChunkEdgeToNextResLevel[6][num29][num38] = new Vector2i(-num3 + num6, -num3 - 1 + 2 * num6 - num38);
			distantChunkMapInfo.ChunkEdgeToNextResLevel[6][num29][num38 + num6] = new Vector2i(-num3 + 1 + num6 + num38, -num3 + num6);
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[6][num29][num38] = 3;
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[6][num29][num38 + num6] = 0;
			distantChunkMapInfo.ChunkEdgeToNextResLevel[7][num29][num38] = new Vector2i(num3 - 2 * num6 + num38, -num3 + num6);
			distantChunkMapInfo.ChunkEdgeToNextResLevel[7][num29][num38 + num6] = new Vector2i(num3 - 1 - num6, -num3 + 1 + num6 + num38);
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[7][num29][num38] = 0;
			distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[7][num29][num38 + num6] = 1;
		}
		for (int num39 = 0; num39 < num29 - 1; num39++)
		{
			for (int num40 = 0; num40 < num6; num40++)
			{
				distantChunkMapInfo.ChunkEdgeToNextResLevel[4][num39][num40] = new Vector2i(num3 - 1 - num6, -num3 + num6 * num39 + num40);
				distantChunkMapInfo.ChunkEdgeToNextResLevel[4][num29 + num39 + 2][num40] = new Vector2i(num3 - 1 - 2 * num6 - num6 * num39 - num40, num3 - 1 - num6);
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[4][num39][num40] = 1;
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[4][num29 + num39 + 2][num40] = 2;
				distantChunkMapInfo.ChunkEdgeToNextResLevel[5][num39][num40] = new Vector2i(num3 - 1 - num6 * num39 - num40, num3 - 1 - num6);
				distantChunkMapInfo.ChunkEdgeToNextResLevel[5][num29 + num39 + 2][num40] = new Vector2i(-num3 + num6, num3 - 1 - 2 * num6 - num6 * num39 - num40);
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[5][num39][num40] = 2;
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[5][num29 + num39 + 2][num40] = 3;
				distantChunkMapInfo.ChunkEdgeToNextResLevel[6][num39][num40] = new Vector2i(-num3 + num6, num3 - 1 - num6 * num39 - num40);
				distantChunkMapInfo.ChunkEdgeToNextResLevel[6][num29 + num39 + 2][num40] = new Vector2i(-num3 + 2 * num6 + num6 * num39 + num40, -num3 + num6);
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[6][num39][num40] = 3;
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[6][num29 + num39 + 2][num40] = 0;
				distantChunkMapInfo.ChunkEdgeToNextResLevel[7][num39][num40] = new Vector2i(-num3 + num6 + num6 * num39 + num40, -num3 + num6);
				distantChunkMapInfo.ChunkEdgeToNextResLevel[7][num29 + num39 + 2][num40] = new Vector2i(num3 - 1 - num6, -num3 + 2 * num6 + num6 * num39 + num40);
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[7][num39][num40] = 0;
				distantChunkMapInfo.ChunkEdgeToNextRLEdgeId[7][num29 + num39 + 2][num40] = 1;
			}
		}
	}

	public int FindOutsideFromChunk(Vector2 _CurrentPosVec, int _Reslevel)
	{
		Vector2 vector = default(Vector2);
		DistantChunkMapInfo distantChunkMapInfo = ChunkMapInfoArray[_Reslevel];
		DistantChunkMapInfo distantChunkMapInfo2 = ((_Reslevel >= NbResLevel - 1) ? ChunkMapInfoArray[_Reslevel] : ChunkMapInfoArray[_Reslevel + 1]);
		for (int i = 0; i < 8; i++)
		{
			vector.Set((_CurrentPosVec.x - distantChunkMapInfo.ShiftVec.x) / distantChunkMapInfo2.ChunkWidth - distantChunkMapInfo.ChunkTriggerArea[i].x, (_CurrentPosVec.y - distantChunkMapInfo.ShiftVec.y) / distantChunkMapInfo2.ChunkWidth - ChunkMapInfoArray[_Reslevel].ChunkTriggerArea[i].y);
			if (vector.x >= 0f && vector.y >= 0f && vector.x < distantChunkMapInfo.ChunkTriggerArea[i].z && vector.y < distantChunkMapInfo.ChunkTriggerArea[i].w)
			{
				return i;
			}
		}
		return -1;
	}

	public void UpdatePlayerPos(int _ResLevel, int _AreaId)
	{
		DistantChunkMapInfo distantChunkMapInfo = ChunkMapInfoArray[_ResLevel];
		DistantChunkMapInfo distantChunkMapInfo2 = ((_ResLevel >= NbResLevel - 1) ? ChunkMapInfoArray[_ResLevel] : ChunkMapInfoArray[_ResLevel + 1]);
		switch (_AreaId)
		{
		case 0:
			distantChunkMapInfo.ShiftVec.Set(distantChunkMapInfo.ShiftVec.x, (float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.y / distantChunkMapInfo2.ChunkWidth) - 1) * distantChunkMapInfo2.ChunkWidth);
			break;
		case 1:
			distantChunkMapInfo.ShiftVec.Set((float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.x / distantChunkMapInfo2.ChunkWidth) + 1) * distantChunkMapInfo2.ChunkWidth, distantChunkMapInfo.ShiftVec.y);
			break;
		case 2:
			distantChunkMapInfo.ShiftVec.Set(distantChunkMapInfo.ShiftVec.x, (float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.y / distantChunkMapInfo2.ChunkWidth) + 1) * distantChunkMapInfo2.ChunkWidth);
			break;
		case 3:
			distantChunkMapInfo.ShiftVec.Set((float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.x / distantChunkMapInfo2.ChunkWidth) - 1) * distantChunkMapInfo2.ChunkWidth, distantChunkMapInfo.ShiftVec.y);
			break;
		case 4:
			distantChunkMapInfo.ShiftVec.Set((float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.x / distantChunkMapInfo2.ChunkWidth) - 1) * distantChunkMapInfo2.ChunkWidth, (float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.y / distantChunkMapInfo2.ChunkWidth) - 1) * distantChunkMapInfo2.ChunkWidth);
			break;
		case 5:
			distantChunkMapInfo.ShiftVec.Set((float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.x / distantChunkMapInfo2.ChunkWidth) + 1) * distantChunkMapInfo2.ChunkWidth, (float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.y / distantChunkMapInfo2.ChunkWidth) - 1) * distantChunkMapInfo2.ChunkWidth);
			break;
		case 6:
			distantChunkMapInfo.ShiftVec.Set((float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.x / distantChunkMapInfo2.ChunkWidth) + 1) * distantChunkMapInfo2.ChunkWidth, (float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.y / distantChunkMapInfo2.ChunkWidth) + 1) * distantChunkMapInfo2.ChunkWidth);
			break;
		case 7:
			distantChunkMapInfo.ShiftVec.Set((float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.x / distantChunkMapInfo2.ChunkWidth) - 1) * distantChunkMapInfo2.ChunkWidth, (float)(Convert.ToInt32(distantChunkMapInfo.ShiftVec.y / distantChunkMapInfo2.ChunkWidth) + 1) * distantChunkMapInfo2.ChunkWidth);
			break;
		}
	}

	public DistantChunkPosData ComputeChunkPos(int _ResLevel, int _NbResLevel, float _LowerResRadius, float _UpperResRadius, float _ChunkWidth)
	{
		List<Vector2i> list = new List<Vector2i>();
		List<Vector2> list2 = new List<Vector2>();
		List<int[]> list3 = new List<int[]>();
		DistantChunkPosData distantChunkPosData = new DistantChunkPosData();
		int num = (int)(_LowerResRadius / _ChunkWidth + 1E-05f);
		int num2 = (int)(_UpperResRadius / _ChunkWidth + 1E-05f);
		for (int i = -num2; i < num2; i++)
		{
			for (int j = -num2; j < num2; j++)
			{
				if (i < -num || i >= num || j < -num || j >= num)
				{
					list.Add(new Vector2i(i, j));
					list2.Add(new Vector2((float)i * _ChunkWidth, (float)j * _ChunkWidth));
					int[] array = new int[4] { _ResLevel, _ResLevel, _ResLevel, _ResLevel };
					if (i == -num - 1 && j >= -num && j < num)
					{
						array[1] = ((_ResLevel - 1 >= 0) ? (_ResLevel - 1) : 0);
					}
					else if (i == num && j >= -num && j < num)
					{
						array[3] = ((_ResLevel - 1 >= 0) ? (_ResLevel - 1) : 0);
					}
					if (j == -num - 1 && i >= -num && i < num)
					{
						array[2] = ((_ResLevel - 1 >= 0) ? (_ResLevel - 1) : 0);
					}
					else if (j == num && i >= -num && i < num)
					{
						array[0] = ((_ResLevel - 1 >= 0) ? (_ResLevel - 1) : 0);
					}
					if (i == -num2)
					{
						array[3] = ((_ResLevel + 1 < _NbResLevel) ? (_ResLevel + 1) : (_NbResLevel - 1));
					}
					else if (i == num2 - 1)
					{
						array[1] = ((_ResLevel + 1 < _NbResLevel) ? (_ResLevel + 1) : (_NbResLevel - 1));
					}
					if (j == -num2)
					{
						array[0] = ((_ResLevel + 1 < _NbResLevel) ? (_ResLevel + 1) : (_NbResLevel - 1));
					}
					else if (j == num2 - 1)
					{
						array[2] = ((_ResLevel + 1 < _NbResLevel) ? (_ResLevel + 1) : (_NbResLevel - 1));
					}
					list3.Add(array);
				}
			}
		}
		distantChunkPosData.ChunkPos = list2.ToArray();
		distantChunkPosData.ChunkIntPos = list.ToArray();
		distantChunkPosData.NeighbResLevel = list3.ToArray();
		return distantChunkPosData;
	}

	public DistantChunkPosData ComputeChunkPos(int _PosX, int _PosZ, int _ResLevel)
	{
		List<Vector2i> list = new List<Vector2i>();
		List<Vector2> list2 = new List<Vector2>();
		List<int[]> list3 = new List<int[]>();
		List<float[]> list4 = new List<float[]>();
		DistantChunkPosData distantChunkPosData = new DistantChunkPosData();
		int num = ((_ResLevel - 1 >= 0) ? (_ResLevel - 1) : 0);
		int num2 = ((_ResLevel + 1 < NbResLevel) ? (_ResLevel + 1) : (NbResLevel - 1));
		int num3 = ((_ResLevel != 0) ? Mathf.FloorToInt(ChunkMapInfoArray[num].ResRadius / ChunkMapInfoArray[_ResLevel].ChunkWidth + 1E-05f) : 0);
		int num4 = Mathf.FloorToInt(ChunkMapInfoArray[_ResLevel].ResRadius / ChunkMapInfoArray[_ResLevel].ChunkWidth + 1E-05f);
		int num5 = Mathf.FloorToInt((float)_PosX / ChunkMapInfoArray[_ResLevel].ChunkWidth + 1E-05f);
		int num6 = Mathf.FloorToInt((float)_PosZ / ChunkMapInfoArray[_ResLevel].ChunkWidth + 1E-05f);
		int num7 = Mathf.FloorToInt((float)_PosX / ChunkMapInfoArray[num2].ChunkWidth + 1E-05f);
		num7 = Mathf.RoundToInt((float)num7 * ChunkMapInfoArray[num2].ChunkWidth / ChunkMapInfoArray[_ResLevel].ChunkWidth);
		int num8 = Mathf.FloorToInt((float)_PosZ / ChunkMapInfoArray[num2].ChunkWidth + 1E-05f);
		num8 = Mathf.FloorToInt((float)num8 * ChunkMapInfoArray[num2].ChunkWidth / ChunkMapInfoArray[_ResLevel].ChunkWidth);
		for (int i = -num4 + num7; i < num4 + num7; i++)
		{
			for (int j = -num4 + num8; j < num4 + num8; j++)
			{
				if (i < -num3 + num5 || i >= num3 + num5 || j < -num3 + num6 || j >= num3 + num6)
				{
					list.Add(new Vector2i(i, j));
					list2.Add(new Vector2((float)i * ChunkMapInfoArray[_ResLevel].ChunkWidth, (float)j * ChunkMapInfoArray[_ResLevel].ChunkWidth));
					int[] array = new int[4] { _ResLevel, _ResLevel, _ResLevel, _ResLevel };
					float[] array2 = new float[4] { 1f, 1f, 1f, 1f };
					if (i == -num3 - 1 + num5 && j >= -num3 + num6 && j < num3 + num6)
					{
						array[1] = ((_ResLevel - 1 >= 0) ? (_ResLevel - 1) : 0);
						array2[1] = Mathf.Round(ChunkMapInfoArray[array[1]].UnitStep / ChunkMapInfoArray[_ResLevel].UnitStep);
					}
					else if (i == num3 + num5 && j >= -num3 + num6 && j < num3 + num6)
					{
						array[3] = ((_ResLevel - 1 >= 0) ? (_ResLevel - 1) : 0);
						array2[3] = Mathf.Round(ChunkMapInfoArray[array[3]].UnitStep / ChunkMapInfoArray[_ResLevel].UnitStep);
					}
					if (j == -num3 - 1 + num6 && i >= -num3 && i < num3 + num5)
					{
						array[2] = ((_ResLevel - 1 >= 0) ? (_ResLevel - 1) : 0);
						array2[2] = Mathf.Round(ChunkMapInfoArray[array[2]].UnitStep / ChunkMapInfoArray[_ResLevel].UnitStep);
					}
					else if (j == num3 + num6 && i >= -num3 && i < num3 + num5)
					{
						array[0] = ((_ResLevel - 1 >= 0) ? (_ResLevel - 1) : 0);
						array2[0] = Mathf.Round(ChunkMapInfoArray[array[0]].UnitStep / ChunkMapInfoArray[_ResLevel].UnitStep);
					}
					if (i == -num4 + num7)
					{
						array[3] = ((_ResLevel + 1 < NbResLevel) ? (_ResLevel + 1) : (NbResLevel - 1));
						array2[3] = Mathf.Round(ChunkMapInfoArray[array[3]].UnitStep / ChunkMapInfoArray[_ResLevel].UnitStep);
					}
					else if (i == num4 - 1 + num7)
					{
						array[1] = ((_ResLevel + 1 < NbResLevel) ? (_ResLevel + 1) : (NbResLevel - 1));
						array2[1] = Mathf.Round(ChunkMapInfoArray[array[1]].UnitStep / ChunkMapInfoArray[_ResLevel].UnitStep);
					}
					if (j == -num4 + num8)
					{
						array[0] = ((_ResLevel + 1 < NbResLevel) ? (_ResLevel + 1) : (NbResLevel - 1));
						array2[0] = Mathf.Round(ChunkMapInfoArray[array[0]].UnitStep / ChunkMapInfoArray[_ResLevel].UnitStep);
					}
					else if (j == num4 - 1 + num8)
					{
						array[2] = ((_ResLevel + 1 < NbResLevel) ? (_ResLevel + 1) : (NbResLevel - 1));
						array2[2] = Mathf.Round(ChunkMapInfoArray[array[2]].UnitStep / ChunkMapInfoArray[_ResLevel].UnitStep);
					}
					list3.Add(array);
					list4.Add(array2);
				}
			}
		}
		distantChunkPosData.ChunkPos = list2.ToArray();
		distantChunkPosData.ChunkIntPos = list.ToArray();
		distantChunkPosData.NeighbResLevel = list3.ToArray();
		distantChunkPosData.EdgeResFactor = list4.ToArray();
		return distantChunkPosData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DistantChunkBasicMesh CreateBaseDataMesh(DistantChunkMapInfo _ChunkMapInfo)
	{
		int chunkResolution = _ChunkMapInfo.ChunkResolution;
		_ = _ChunkMapInfo.ColliderResolution;
		DistantChunkBasicMesh distantChunkBasicMesh = new DistantChunkBasicMesh();
		distantChunkBasicMesh.NbVertices = chunkResolution * chunkResolution;
		float num = _ChunkMapInfo.ChunkWidth / (float)(chunkResolution - 1);
		_ = 1f / (float)(chunkResolution - 1);
		distantChunkBasicMesh.Vertices = new Vector3[distantChunkBasicMesh.NbVertices];
		distantChunkBasicMesh.Normals = new Vector3[distantChunkBasicMesh.NbVertices];
		distantChunkBasicMesh.Triangles = new int[(chunkResolution - 1) * (chunkResolution - 1) * 6];
		distantChunkBasicMesh.Colors = new Color[distantChunkBasicMesh.NbVertices];
		distantChunkBasicMesh.TextureId = new int[distantChunkBasicMesh.NbVertices];
		int num2 = 0;
		for (int i = 0; i < chunkResolution; i++)
		{
			for (int j = 0; j < chunkResolution; j++)
			{
				distantChunkBasicMesh.Vertices[num2].Set((float)i * num, 0f, (float)j * num);
				num2++;
			}
		}
		num2 = 0;
		for (int k = 0; k < chunkResolution - 1; k++)
		{
			for (int l = 0; l < chunkResolution - 1; l++)
			{
				int num3 = k * chunkResolution + l;
				distantChunkBasicMesh.Triangles[num2++] = num3 + 1;
				distantChunkBasicMesh.Triangles[num2++] = num3 + chunkResolution;
				distantChunkBasicMesh.Triangles[num2++] = num3;
				distantChunkBasicMesh.Triangles[num2++] = num3 + chunkResolution + 1;
				distantChunkBasicMesh.Triangles[num2++] = num3 + chunkResolution;
				distantChunkBasicMesh.Triangles[num2++] = num3 + 1;
			}
		}
		return distantChunkBasicMesh;
	}

	public static DistantChunkBasicMesh CreateBaseDataCollider(float _ChunkWidth, int _ChunkResolution)
	{
		DistantChunkBasicMesh distantChunkBasicMesh = new DistantChunkBasicMesh();
		distantChunkBasicMesh.NbVertices = _ChunkResolution * _ChunkResolution;
		float num = _ChunkWidth / (float)(_ChunkResolution - 1);
		distantChunkBasicMesh.Vertices = new Vector3[distantChunkBasicMesh.NbVertices];
		distantChunkBasicMesh.Triangles = new int[(_ChunkResolution - 1) * (_ChunkResolution - 1) * 6];
		int num2 = 0;
		for (int i = 0; i < _ChunkResolution; i++)
		{
			for (int j = 0; j < _ChunkResolution; j++)
			{
				distantChunkBasicMesh.Vertices[num2].Set((float)i * num, 0f, (float)j * num);
				num2++;
			}
		}
		num2 = 0;
		for (int k = 0; k < _ChunkResolution - 1; k++)
		{
			for (int l = 0; l < _ChunkResolution - 1; l++)
			{
				int num3 = k * _ChunkResolution + l;
				distantChunkBasicMesh.Triangles[num2++] = num3 + 1;
				distantChunkBasicMesh.Triangles[num2++] = num3 + _ChunkResolution;
				distantChunkBasicMesh.Triangles[num2++] = num3;
				distantChunkBasicMesh.Triangles[num2++] = num3 + _ChunkResolution + 1;
				distantChunkBasicMesh.Triangles[num2++] = num3 + _ChunkResolution;
				distantChunkBasicMesh.Triangles[num2++] = num3 + 1;
			}
		}
		return distantChunkBasicMesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DistantChunkBasicMesh createBaseDataMeshFold(DistantChunkMapInfo _ChunkMapInfo)
	{
		int chunkResolution = _ChunkMapInfo.ChunkResolution;
		_ = _ChunkMapInfo.ColliderResolution;
		DistantChunkBasicMesh distantChunkBasicMesh = new DistantChunkBasicMesh();
		int num = chunkResolution * chunkResolution;
		distantChunkBasicMesh.NbVertices = chunkResolution * chunkResolution + 4 * chunkResolution;
		float num2 = _ChunkMapInfo.ChunkWidth / (float)(chunkResolution - 1);
		_ = 1f / (float)(chunkResolution - 1);
		distantChunkBasicMesh.Vertices = new Vector3[distantChunkBasicMesh.NbVertices];
		distantChunkBasicMesh.Normals = new Vector3[distantChunkBasicMesh.NbVertices];
		distantChunkBasicMesh.Triangles = new int[(chunkResolution - 1) * (chunkResolution - 1) * 6 + (chunkResolution - 1) * 24];
		distantChunkBasicMesh.Colors = new Color[distantChunkBasicMesh.NbVertices];
		distantChunkBasicMesh.TextureId = new int[distantChunkBasicMesh.NbVertices];
		int num3 = 0;
		for (int i = 0; i < chunkResolution; i++)
		{
			for (int j = 0; j < chunkResolution; j++)
			{
				distantChunkBasicMesh.Vertices[num3].Set((float)i * num2, 0f, (float)j * num2);
				num3++;
			}
		}
		for (int k = 0; k < 4; k++)
		{
			for (int l = 0; l < chunkResolution; l++)
			{
				distantChunkBasicMesh.Vertices[num3] = distantChunkBasicMesh.Vertices[_ChunkMapInfo.EdgeMap[k][l]] * 1f;
				num3++;
			}
		}
		num3 = 0;
		for (int m = 0; m < chunkResolution - 1; m++)
		{
			for (int n = 0; n < chunkResolution - 1; n++)
			{
				int num4 = m * chunkResolution + n;
				distantChunkBasicMesh.Triangles[num3++] = num4 + 1;
				distantChunkBasicMesh.Triangles[num3++] = num4 + chunkResolution;
				distantChunkBasicMesh.Triangles[num3++] = num4;
				distantChunkBasicMesh.Triangles[num3++] = num4 + chunkResolution + 1;
				distantChunkBasicMesh.Triangles[num3++] = num4 + chunkResolution;
				distantChunkBasicMesh.Triangles[num3++] = num4 + 1;
			}
		}
		for (int num5 = 0; num5 < chunkResolution - 1; num5++)
		{
			int num6 = num;
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[0][num5];
			distantChunkBasicMesh.Triangles[num3++] = num6 + 1 + num5;
			distantChunkBasicMesh.Triangles[num3++] = num6 + num5;
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[0][num5 + 1];
			distantChunkBasicMesh.Triangles[num3++] = num6 + 1 + num5;
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[0][num5];
			num6 = num + chunkResolution;
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[1][num5 + 1];
			distantChunkBasicMesh.Triangles[num3++] = num6 + num5;
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[1][num5];
			distantChunkBasicMesh.Triangles[num3++] = num6 + 1 + num5;
			distantChunkBasicMesh.Triangles[num3++] = num6 + num5;
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[1][num5 + 1];
			num6 = num + chunkResolution * 2;
			distantChunkBasicMesh.Triangles[num3++] = num6 + 1 + num5;
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[2][num5];
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[2][num5 + 1];
			distantChunkBasicMesh.Triangles[num3++] = num6 + num5;
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[2][num5];
			distantChunkBasicMesh.Triangles[num3++] = num6 + 1 + num5;
			num6 = num + chunkResolution * 3;
			distantChunkBasicMesh.Triangles[num3++] = num6 + num5;
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[3][num5 + 1];
			distantChunkBasicMesh.Triangles[num3++] = num6 + 1 + num5;
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[3][num5];
			distantChunkBasicMesh.Triangles[num3++] = _ChunkMapInfo.EdgeMap[3][num5 + 1];
			distantChunkBasicMesh.Triangles[num3++] = num6 + num5;
		}
		return distantChunkBasicMesh;
	}
}

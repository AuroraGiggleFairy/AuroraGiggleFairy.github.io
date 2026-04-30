using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistantTerrainQuadTree
{
	public object[] Root;

	public int[] AreaSize;

	public int[] ElementSize;

	public int ExtendedElementSizeX;

	public int ExtendedElementSizeY;

	public int DataSize;

	public int NbTreeLevel;

	public int NbPosBit;

	public int NbTreeElement;

	[PublicizedFrom(EAccessModifier.Private)]
	public object[] CurNode;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<object[]> ObjList;

	[PublicizedFrom(EAccessModifier.Private)]
	public DatabaseWithFixedDS<long, byte[]> DataBase;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> NodeSeqList = new List<int>();

	public DistantTerrainQuadTree(int AreaXSize, int AreaYSize, int ElementXSize, int ElementYSize)
	{
		AreaSize = new int[2] { AreaXSize, AreaYSize };
		ElementSize = new int[2] { ElementXSize, ElementYSize };
		NbTreeLevel = Mathf.Max(Mathf.CeilToInt((float)AreaXSize / (float)ElementXSize), Mathf.CeilToInt((float)AreaYSize / (float)ElementYSize));
		NbTreeLevel = Mathf.CeilToInt(Mathf.Log(NbTreeLevel, 2f));
		ExtendedElementSizeX = (int)(Mathf.Pow(2f, NbTreeLevel) * (float)ElementXSize);
		ExtendedElementSizeY = (int)(Mathf.Pow(2f, NbTreeLevel) * (float)ElementYSize);
		NbPosBit = Mathf.CeilToInt(Mathf.Log(ExtendedElementSizeX, 2f)) - 1;
		DataSize = ElementXSize * ElementYSize;
		ObjList = new List<object[]>();
		Root = new object[4];
	}

	public void AddChunk(int PosX, int PosY, byte[] Data)
	{
		AddElement(PosX * ElementSize[0] + AreaSize[0] / 2, PosY * ElementSize[1] + AreaSize[1] / 2, Data);
	}

	public void AddElement(int PosX, int PosY, byte[] Data)
	{
		int num = ExtendedElementSizeX;
		int num2 = ExtendedElementSizeY;
		int num3 = 0;
		int num4 = 0;
		CurNode = Root;
		NodeSeqList.Clear();
		int num5 = 0;
		for (int i = 0; i < NbTreeLevel - 1; i++)
		{
			num5 = 0;
			num >>= 1;
			if (PosX >= num3 + num)
			{
				num3 += num;
				num5++;
			}
			num2 >>= 1;
			if (PosY >= num4 + num2)
			{
				num4 += num2;
				num5 += 2;
			}
			if (CurNode[num5] == null)
			{
				CurNode[num5] = new object[4];
			}
			CurNode = (object[])CurNode[num5];
			NodeSeqList.Add(num5);
		}
		num5 = 0;
		num >>= 1;
		if (PosX >= num3 + num)
		{
			num5++;
		}
		num2 >>= 1;
		if (PosY >= num4 + num2)
		{
			num5 += 2;
		}
		NodeSeqList.Add(num5);
		if (CurNode[num5] == null)
		{
			CurNode[num5] = new QTDataElement(PosX, PosY, new byte[DataSize]);
			NbTreeElement++;
		}
		((QTDataElement)CurNode[num5]).Data = Data;
	}

	public QTDataElement GetElement(uint PosX, uint PosY)
	{
		int num = ExtendedElementSizeX;
		int num2 = ExtendedElementSizeY;
		int num3 = 0;
		int num4 = 0;
		CurNode = Root;
		int num5 = 0;
		for (int i = 0; i < NbTreeLevel - 1; i++)
		{
			num5 = 0;
			num >>= 1;
			if (PosX >= num3 + num)
			{
				num3 += num;
				num5++;
			}
			num2 >>= 1;
			if (PosY >= num4 + num2)
			{
				num4 += num2;
				num5 += 2;
			}
			if (CurNode[num5] == null)
			{
				return null;
			}
			CurNode = (object[])CurNode[num5];
		}
		num5 = 0;
		num >>= 1;
		if (PosX >= num3 + num)
		{
			num5++;
		}
		num2 >>= 1;
		if (PosY >= num4 + num2)
		{
			num5 += 2;
		}
		if (CurNode[num5] == null)
		{
			return null;
		}
		return (QTDataElement)CurNode[num5];
	}

	public List<QTDataElement> GetAllElementFromLevelId(uint PosX, uint PosY, int LevelIdFromLeaf)
	{
		object nodeFromLevelId = GetNodeFromLevelId(PosX, PosY, LevelIdFromLeaf);
		if (nodeFromLevelId == null)
		{
			return null;
		}
		List<QTDataElement> list = new List<QTDataElement>();
		if (LevelIdFromLeaf == 0)
		{
			list.Add((QTDataElement)nodeFromLevelId);
			return list;
		}
		ObjList.Clear();
		ObjList.Add((object[])nodeFromLevelId);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < LevelIdFromLeaf - 1; i++)
		{
			num2 = ObjList.Count;
			for (int j = num; j < num2; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					if (ObjList[j][k] != null)
					{
						ObjList.Add((object[])ObjList[j][k]);
					}
				}
			}
			num = num2;
		}
		num2 = ObjList.Count;
		for (int l = num; l < num2; l++)
		{
			for (int m = 0; m < 4; m++)
			{
				if (ObjList[l][m] != null)
				{
					list.Add((QTDataElement)ObjList[l][m]);
				}
			}
		}
		return list;
	}

	public object GetNodeFromLevelId(uint PosX, uint PosY, int LevelIdFromLeaf)
	{
		int num = ExtendedElementSizeX;
		int num2 = ExtendedElementSizeY;
		int num3 = 0;
		int num4 = 0;
		object obj = Root;
		NodeSeqList.Clear();
		int num5 = 0;
		for (int i = 0; i < NbTreeLevel - LevelIdFromLeaf; i++)
		{
			num5 = 0;
			num >>= 1;
			if (PosX >= num3 + num)
			{
				num3 += num;
				num5++;
			}
			num2 >>= 1;
			if (PosY >= num4 + num2)
			{
				num4 += num2;
				num5 += 2;
			}
			NodeSeqList.Add(num5);
			if (((object[])obj)[num5] == null)
			{
				return null;
			}
			obj = ((object[])obj)[num5];
		}
		return obj;
	}

	public byte[] GetAllElementFromLevelId(uint PosX, uint PosY)
	{
		PosX <<= 32 - NbPosBit;
		PosY <<= 32 - NbPosBit;
		CurNode = Root;
		for (int i = 0; i < NbTreeLevel - 1; i++)
		{
			uint num = ((PosX & 0x80000000u) >> 31) + ((PosY & 0x80000000u) >> 30);
			PosX <<= 1;
			PosY <<= 1;
			if (CurNode[num] == null)
			{
				return null;
			}
			CurNode = (object[])CurNode[num];
		}
		return (byte[])CurNode[((PosX & 0x80000000u) >> 31) + ((PosY & 0x80000000u) >> 30)];
	}

	public List<QTDataElement> SetRandomData(byte DefaultHeight, int NbChunk)
	{
		int num = 50;
		int num2 = AreaSize[0] / ElementSize[0];
		int num3 = AreaSize[1] / ElementSize[1];
		List<QTDataElement> list = new List<QTDataElement>();
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(0);
		int[] array = new int[2];
		List<int> list2 = new List<int>();
		while (list.Count < NbChunk)
		{
			int num4 = Mathf.Min(gameRandom.RandomRange(1, num + 1), NbChunk - list.Count);
			array[0] = gameRandom.RandomRange(0, num2);
			array[1] = gameRandom.RandomRange(0, num3);
			list2.Clear();
			list2.Add(gameRandom.RandomRange(0, num2) + gameRandom.RandomRange(0, num3) * 1000);
			while (list2.Count < num4)
			{
				int index = gameRandom.RandomRange(0, list2.Count);
				int num5 = gameRandom.RandomRange(-1, 2);
				int num6 = gameRandom.RandomRange(-1, 2);
				if (list2[index] % 1000 + num5 >= 0 && list2[index] % 1000 + num5 < num2 && list2[index] / 1000 + num6 >= 0 && list2[index] / 1000 + num6 < num3)
				{
					list2.Add(list2[index] % 1000 + num5 + (list2[index] / 1000 + num6) * 1000);
					list2 = list2.Distinct().ToList();
				}
			}
			for (int i = 0; i < list2.Count; i++)
			{
				QTDataElement qTDataElement = new QTDataElement();
				qTDataElement.Data = new byte[256];
				for (int j = 0; j < 256; j++)
				{
					qTDataElement.Data[j] = DefaultHeight;
				}
				qTDataElement.Key = WorldChunkCache.MakeChunkKey(list2[i] % 1000 * ElementSize[0], list2[i] / 1000 * ElementSize[1]);
				list.Add(qTDataElement);
			}
		}
		return list;
	}

	public void CreateFile(List<QTDataElement> NewData, string DirName, string FileName)
	{
		for (int i = 0; i < NewData.Count; i++)
		{
			DataBase.SetDS(NewData[i].Key, NewData[i].Data);
		}
		DataBase.Save(DirName, FileName);
	}

	public void SetQuadtreeFromDatabase(string DirName, string FileName)
	{
		DataBase = new ExtensionChunkDatabase(4660, AreaSize[0], AreaSize[1], ElementSize[0]);
		DataBase.Load(DirName, FileName);
		List<long> allKeys = DataBase.GetAllKeys();
		for (int i = 0; i < allKeys.Count; i++)
		{
			byte[] dS = DataBase.GetDS(allKeys[i]);
			int posX = WorldChunkCache.extractX(allKeys[i]);
			int posY = WorldChunkCache.extractZ(allKeys[i]);
			AddElement(posX, posY, dS);
		}
	}
}

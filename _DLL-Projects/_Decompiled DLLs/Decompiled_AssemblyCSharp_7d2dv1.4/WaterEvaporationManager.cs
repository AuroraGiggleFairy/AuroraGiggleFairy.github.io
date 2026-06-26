using System;
using System.Collections.Generic;
using UnityEngine;

public class WaterEvaporationManager : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class BlockData
	{
		public ulong time;

		public ulong ID;

		public Vector3i pos;

		public BlockData(Vector3i _pos)
		{
			time = GameManager.Instance.World.GetWorldTime();
			ID = uniqueIndex++;
			pos = _pos;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int evapWalkIndex = 0;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static ulong uniqueIndex = 0uL;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int restWalkIndex = 0;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryList<ulong, BlockData> evapWalkList = new DictionaryList<ulong, BlockData>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryList<int, DictionaryList<int, DictionaryList<int, BlockData>>> evaporationList = new DictionaryList<int, DictionaryList<int, DictionaryList<int, BlockData>>>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryList<ulong, BlockData> restWalkList = new DictionaryList<ulong, BlockData>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryList<int, DictionaryList<int, DictionaryList<int, BlockData>>> restingList = new DictionaryList<int, DictionaryList<int, DictionaryList<int, BlockData>>>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int waterBlockID = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3i> vRemovalList = new List<Vector3i>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<ulong> uRemovalList = new List<ulong>();

	public void Start()
	{
	}

	public void Update()
	{
	}

	public static void UpdateEvaporation()
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		vRemovalList.Clear();
		uRemovalList.Clear();
		lock (evapWalkList)
		{
			int num = 0;
			for (num = evapWalkIndex; num < evapWalkList.Count && num < evapWalkIndex + 15; num++)
			{
				if (GameManager.Instance == null || GameManager.Instance.World == null)
				{
					return;
				}
				if (GameManager.Instance.World.GetWorldTime() > evapWalkList.list[num].time + 1000)
				{
					vRemovalList.Add(evapWalkList.list[num].pos);
					uRemovalList.Add(evapWalkList.list[num].ID);
					GameManager.Instance.World.SetBlockRPC(evapWalkList.list[num].pos, BlockValue.Air);
					if (waterBlockID < 0)
					{
						waterBlockID = ItemClass.GetItem("Water").ToBlockValue().type;
					}
					GameManager.Instance.World.GetWBT().AddScheduledBlockUpdate(0, evapWalkList.list[num].pos, waterBlockID, 1uL);
				}
			}
			num = 0;
			for (int i = 0; i < vRemovalList.Count; i++)
			{
				Vector3i blockPos = vRemovalList[i];
				evapWalkList.Remove(uRemovalList[num++]);
				RemoveFromEvapList(blockPos);
			}
			evapWalkIndex += 15;
			if (evapWalkIndex >= evapWalkList.Count)
			{
				evapWalkIndex = 0;
			}
		}
		vRemovalList.Clear();
		uRemovalList.Clear();
		lock (restWalkList)
		{
			int num2 = 0;
			for (num2 = restWalkIndex; num2 < restWalkList.Count && num2 < restWalkIndex + 15; num2++)
			{
				if (GameManager.Instance == null || GameManager.Instance.World == null)
				{
					return;
				}
				if (GameManager.Instance.World.GetWorldTime() > restWalkList.list[num2].time + 1000)
				{
					vRemovalList.Add(restWalkList.list[num2].pos);
					uRemovalList.Add(restWalkList.list[num2].ID);
					BlockValue _blockValue = GameManager.Instance.World.GetBlock(restWalkList.list[num2].pos.x, restWalkList.list[num2].pos.y, restWalkList.list[num2].pos.z);
					BlockLiquidv2.SetBlockState(ref _blockValue, BlockLiquidv2.UpdateID.Evaporate);
					GameManager.Instance.World.SetBlockRPC(restWalkList.list[num2].pos, _blockValue);
					if (waterBlockID < 0)
					{
						waterBlockID = ItemClass.GetItem("Water").ToBlockValue().type;
					}
					GameManager.Instance.World.GetWBT().AddScheduledBlockUpdate(0, restWalkList.list[num2].pos, waterBlockID, 1uL);
				}
			}
			num2 = 0;
			for (int j = 0; j < vRemovalList.Count; j++)
			{
				Vector3i blockPos2 = vRemovalList[j];
				restWalkList.Remove(uRemovalList[num2++]);
				RemoveFromRestList(blockPos2);
			}
			restWalkIndex += 15;
			if (restWalkIndex >= restWalkList.Count)
			{
				restWalkIndex = 0;
			}
		}
	}

	public static void ClearAll()
	{
		lock (restingList)
		{
			restingList.Clear();
		}
		lock (evaporationList)
		{
			evaporationList.Clear();
		}
		lock (evapWalkList)
		{
			evapWalkList.Clear();
		}
		lock (restWalkList)
		{
			restWalkList.Clear();
		}
	}

	public static void AddToRestList(Vector3i _blockPos)
	{
		lock (restingList)
		{
			lock (restWalkList)
			{
				if (restingList.dict.ContainsKey(_blockPos.x))
				{
					if (restingList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y))
					{
						if (restingList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
						{
							BlockData blockData = new BlockData(_blockPos);
							restingList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z] = blockData;
							restWalkList.Add(blockData.ID, blockData);
						}
						else
						{
							BlockData blockData2 = new BlockData(_blockPos);
							restingList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData2);
							restWalkList.Add(blockData2.ID, blockData2);
						}
					}
					else
					{
						BlockData blockData3 = new BlockData(_blockPos);
						restingList.dict[_blockPos.x].Add(_blockPos.y, new DictionaryList<int, BlockData>());
						restingList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData3);
						restWalkList.Add(blockData3.ID, blockData3);
					}
				}
				else
				{
					BlockData blockData4 = new BlockData(_blockPos);
					restingList.Add(_blockPos.x, new DictionaryList<int, DictionaryList<int, BlockData>>());
					restingList.dict[_blockPos.x].Add(_blockPos.y, new DictionaryList<int, BlockData>());
					restingList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData4);
					restWalkList.Add(blockData4.ID, blockData4);
				}
			}
		}
	}

	public static void RemoveFromRestList(Vector3i _blockPos)
	{
		lock (restingList)
		{
			if (restingList.dict.ContainsKey(_blockPos.x) && restingList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y) && restingList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
			{
				lock (restWalkList)
				{
					restWalkList.Remove(restingList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z].ID);
				}
				restingList.dict[_blockPos.x].dict[_blockPos.y].Remove(_blockPos.z);
			}
		}
	}

	public static ulong GetRestTime(Vector3i _blockPos)
	{
		lock (restingList)
		{
			if (restingList.dict.ContainsKey(_blockPos.x) && restingList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y) && restingList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
			{
				return restingList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z].time;
			}
		}
		return 0uL;
	}

	public static void AddToEvapList(Vector3i _blockPos)
	{
		lock (evaporationList)
		{
			lock (evapWalkList)
			{
				if (evaporationList.dict.ContainsKey(_blockPos.x))
				{
					if (evaporationList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y))
					{
						if (evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
						{
							BlockData blockData = new BlockData(_blockPos);
							evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z] = blockData;
							evapWalkList.Add(blockData.ID, blockData);
						}
						else
						{
							BlockData blockData2 = new BlockData(_blockPos);
							evaporationList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData2);
							evapWalkList.Add(blockData2.ID, blockData2);
						}
					}
					else
					{
						BlockData blockData3 = new BlockData(_blockPos);
						evaporationList.dict[_blockPos.x].Add(_blockPos.y, new DictionaryList<int, BlockData>());
						evaporationList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData3);
						evapWalkList.Add(blockData3.ID, blockData3);
					}
				}
				else
				{
					BlockData blockData4 = new BlockData(_blockPos);
					evaporationList.Add(_blockPos.x, new DictionaryList<int, DictionaryList<int, BlockData>>());
					evaporationList.dict[_blockPos.x].Add(_blockPos.y, new DictionaryList<int, BlockData>());
					evaporationList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData4);
					evapWalkList.Add(blockData4.ID, blockData4);
				}
			}
		}
	}

	public static void RemoveFromEvapList(Vector3i _blockPos)
	{
		lock (evaporationList)
		{
			if (evaporationList.dict.ContainsKey(_blockPos.x) && evaporationList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y) && evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
			{
				lock (evapWalkList)
				{
					evapWalkList.Remove(evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z].ID);
				}
				evaporationList.dict[_blockPos.x].dict[_blockPos.y].Remove(_blockPos.z);
			}
		}
	}

	public static ulong GetEvapTime(Vector3i _blockPos)
	{
		lock (evaporationList)
		{
			if (evaporationList.dict.ContainsKey(_blockPos.x) && evaporationList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y) && evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
			{
				return evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z].time;
			}
		}
		return 0uL;
	}
}

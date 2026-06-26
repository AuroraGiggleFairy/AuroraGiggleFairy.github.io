using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ConcurrentCollections;

public class DynamicMeshBlockSwap
{
	public static BlockValue DoorReplacement;

	public static ConcurrentHashSet<int> DoorBlocks = new ConcurrentHashSet<int>();

	public static ConcurrentHashSet<int> OpaqueBlocks = new ConcurrentHashSet<int>();

	public static ConcurrentHashSet<int> TerrainBlocks = new ConcurrentHashSet<int>();

	public static ConcurrentDictionary<int, int> BlockSwaps = new ConcurrentDictionary<int, int>();

	public static ConcurrentDictionary<int, long> TextureSwaps = new ConcurrentDictionary<int, long>();

	public static HashSet<int> InvalidPaintIds = new HashSet<int>();

	public static bool IsValidBlock(int type)
	{
		if (!OpaqueBlocks.Contains(type))
		{
			return TerrainBlocks.Contains(type);
		}
		return true;
	}

	public static void Init()
	{
		BlockSwaps.Clear();
		TextureSwaps.Clear();
		OpaqueBlocks.Clear();
		DoorBlocks.Clear();
		TerrainBlocks.Clear();
		DoorReplacement = Block.GetBlockValue("imposterBlock", _caseInsensitive: true);
		if (DoorReplacement.isair)
		{
			Log.Warning("Dynamic mesh door replacement block not found");
		}
		else
		{
			Log.Out("Dymesh door replacement: " + DoorReplacement.Block.GetBlockName());
		}
		Type typeFromHandle = typeof(BlockDoorSecure);
		Type typeFromHandle2 = typeof(BlockDoor);
		Block[] list = Block.list;
		foreach (Block block in list)
		{
			if (block == null || block.blockID == 0)
			{
				continue;
			}
			bool flag = typeFromHandle2.IsAssignableFrom(block.GetType()) || typeFromHandle.IsAssignableFrom(block.GetType());
			if (block.MeshIndex == 0 || flag)
			{
				int type = Block.GetBlockValue(block.GetBlockName()).type;
				bool num = block is BlockModelTree;
				bool flag2 = block.shape is BlockShapeModelEntity;
				if (!num && !block.IsPlant() && !flag2)
				{
					OpaqueBlocks.Add(type);
				}
				if (flag)
				{
					DoorBlocks.Add(type);
				}
				if (block.bImposterExcludeAndStop || block.bImposterExclude || (block.IsTerrainDecoration && block.ImposterExchange == 0))
				{
					BlockSwaps.TryAdd(block.blockID, 0);
				}
				else if (block.ImposterExchange != 0)
				{
					BlockSwaps.TryAdd(block.blockID, block.ImposterExchange);
					TextureSwaps.TryAdd(block.blockID, block.ImposterExchangeTexIdx);
				}
			}
			else if (block.MeshIndex == 5)
			{
				int type2 = Block.GetBlockValue(block.GetBlockName()).type;
				TerrainBlocks.Add(type2);
			}
		}
	}
}

using System;
using System.Collections.Generic;

public class StabilityInitializer
{
	public const byte MaxStability = 15;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBase world;

	public StabilityInitializer(WorldBase _world)
	{
		world = _world;
	}

	public void DistributeStability(Chunk _chunk)
	{
		int maxHeight = _chunk.GetMaxHeight();
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k <= maxHeight; k++)
				{
					int blockId = _chunk.GetBlockId(j, k, i);
					if (blockId == 0)
					{
						continue;
					}
					Block block = Block.list[blockId];
					if (!block.blockMaterial.IsLiquid && !block.StabilityIgnore)
					{
						byte stability = _chunk.GetStability(j, k, i);
						if (stability > 1)
						{
							spreadHorizontal(_chunk, j, k, i, stability);
						}
					}
				}
			}
		}
		_chunk.StopStabilityCalculation = false;
	}

	public void BlockRemovedAt(int _worldX, int _worldY, int _worldZ)
	{
		Chunk chunk = (Chunk)world.GetChunkFromWorldPos(_worldX, _worldY, _worldZ);
		if (chunk == null)
		{
			return;
		}
		int num = World.toBlockXZ(_worldX);
		int num2 = World.toBlockXZ(_worldZ);
		byte stability = chunk.GetStability(num, _worldY, num2);
		chunk.SetStability(num, _worldY, num2, 0);
		HashSet<Vector3i> hashSet = new HashSet<Vector3i>();
		unspreadHorizontal(chunk, num, _worldY, num2, stability, hashSet);
		unspreadVertical(chunk, num, _worldY, num2, stability, hashSet);
		foreach (Vector3i item in hashSet)
		{
			chunk = (Chunk)world.GetChunkFromWorldPos(item.x, item.y, item.z);
			if (chunk != null)
			{
				num = World.toBlockXZ(item.x);
				num2 = World.toBlockXZ(item.z);
				stability = chunk.GetStability(num, item.y, num2);
				spreadHorizontal(chunk, num, item.y, num2, stability);
				spreadVertical(chunk, num, item.y, num2, stability);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unspreadVertical(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stab, HashSet<Vector3i> _list)
	{
		for (int i = _y + 1; i < 256; i++)
		{
			BlockValue blockNoDamage = _chunk.GetBlockNoDamage(_blockX, i, _blockZ);
			Block block = blockNoDamage.Block;
			if (blockNoDamage.isair || !block.StabilitySupport || block.blockMaterial.IsLiquid || block.StabilityIgnore)
			{
				break;
			}
			byte stability = _chunk.GetStability(_blockX, i, _blockZ);
			if (stability == 0)
			{
				break;
			}
			if (stability > _stab)
			{
				_list.Add(new Vector3i(_chunk.GetBlockWorldPosX(_blockX), i, _chunk.GetBlockWorldPosZ(_blockZ)));
				break;
			}
			_chunk.SetStability(_blockX, i, _blockZ, 0);
			unspreadHorizontal(_chunk, _blockX, i, _blockZ, _stab, _list);
		}
		int num = _stab - 1;
		int num2 = _y - 1;
		while (num2 > 0 && num > 0)
		{
			BlockValue blockNoDamage2 = _chunk.GetBlockNoDamage(_blockX, num2, _blockZ);
			Block block2 = blockNoDamage2.Block;
			if (!blockNoDamage2.isair && block2.StabilitySupport && !block2.blockMaterial.IsLiquid && !block2.StabilityIgnore)
			{
				byte stability2 = _chunk.GetStability(_blockX, num2, _blockZ);
				if (stability2 != 0)
				{
					if (stability2 > num)
					{
						_list.Add(new Vector3i(_chunk.GetBlockWorldPosX(_blockX), num2, _chunk.GetBlockWorldPosZ(_blockZ)));
						break;
					}
					_chunk.SetStability(_blockX, num2, _blockZ, 0);
					unspreadHorizontal(_chunk, _blockX, num2, _blockZ, num, _list);
					num--;
					num2--;
					continue;
				}
				break;
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unspreadHorizontal(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stab, HashSet<Vector3i> _list)
	{
		if (_stab <= 0)
		{
			return;
		}
		_stab--;
		for (int i = 0; i < Vector3i.HORIZONTAL_DIRECTIONS.Length; i++)
		{
			int num = _blockX + Vector3i.HORIZONTAL_DIRECTIONS[i].x;
			int num2 = _blockZ + Vector3i.HORIZONTAL_DIRECTIONS[i].z;
			Chunk chunk = _chunk;
			if (num < 0 || num >= 16 || num2 < 0 || num2 >= 16)
			{
				chunk = (Chunk)world.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num), _y, _chunk.GetBlockWorldPosZ(num2));
				num = World.toBlockXZ(num);
				num2 = World.toBlockXZ(num2);
			}
			if (chunk == null)
			{
				continue;
			}
			BlockValue blockNoDamage = chunk.GetBlockNoDamage(num, _y, num2);
			Block block = blockNoDamage.Block;
			if (blockNoDamage.isair || !block.StabilitySupport || block.blockMaterial.IsLiquid || block.StabilityIgnore)
			{
				continue;
			}
			byte stability = chunk.GetStability(num, _y, num2);
			if (stability != 0)
			{
				if (stability > _stab)
				{
					_list.Add(new Vector3i(chunk.GetBlockWorldPosX(num), _y, chunk.GetBlockWorldPosZ(num2)));
					continue;
				}
				chunk.SetStability(num, _y, num2, 0);
				unspreadHorizontal(chunk, num, _y, num2, _stab, _list);
				unspreadVertical(chunk, num, _y, num2, _stab, _list);
			}
		}
	}

	public void BlockPlacedAt(int _worldX, int _worldY, int _worldZ, BlockValue _blockValue)
	{
		int num = 0;
		for (int i = 0; i < Vector3i.HORIZONTAL_DIRECTIONS.Length; i++)
		{
			num = Math.Max(world.GetStability(_worldX + Vector3i.HORIZONTAL_DIRECTIONS[i].x, _worldY, _worldZ + Vector3i.HORIZONTAL_DIRECTIONS[i].z), num);
		}
		num--;
		int stability = world.GetStability(_worldX, _worldY - 1, _worldZ);
		int stability2 = world.GetStability(_worldX, _worldY + 1, _worldZ);
		Chunk chunk = (Chunk)world.GetChunkFromWorldPos(_worldX, _worldY, _worldZ);
		if (chunk != null)
		{
			int num2 = World.toBlockXZ(_worldX);
			int num3 = World.toBlockXZ(_worldZ);
			stability = ((15 != stability) ? (stability - 1) : 15);
			stability2--;
			int val = Math.Max(stability2, stability);
			int num4 = Math.Max(Math.Min(Math.Max(num, val), 15), 0);
			chunk.SetStability(num2, _worldY, num3, (byte)num4);
			if (num4 > 0)
			{
				spreadHorizontal(chunk, num2, _worldY, num3, num4);
				spreadVertical(chunk, num2, _worldY, num3, num4);
			}
			else
			{
				Log.Out("Unbalanced");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void spreadVertical(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stab)
	{
		for (int i = _y + 1; i < 256; i++)
		{
			int blockId = _chunk.GetBlockId(_blockX, i, _blockZ);
			if (blockId == 0)
			{
				break;
			}
			Block block = Block.list[blockId];
			if (block.blockMaterial.IsLiquid || block.StabilityIgnore)
			{
				break;
			}
			int num = Math.Min(_stab, 15);
			if (num > 1 && !block.StabilitySupport)
			{
				num = 1;
			}
			byte stability = _chunk.GetStability(_blockX, i, _blockZ);
			if (num <= stability)
			{
				break;
			}
			_chunk.SetStability(_blockX, i, _blockZ, (byte)num);
			if (block.StabilitySupport)
			{
				spreadHorizontal(_chunk, _blockX, i, _blockZ, num);
			}
		}
		int num2 = _stab - 1;
		int num3 = _y - 1;
		while (num3 > 0 && num2 > 0)
		{
			int blockId2 = _chunk.GetBlockId(_blockX, num3, _blockZ);
			if (blockId2 != 0)
			{
				Block block2 = Block.list[blockId2];
				if (!block2.blockMaterial.IsLiquid && !block2.StabilityIgnore)
				{
					int num4 = Math.Min(num2, 15);
					if (num4 > 1 && !block2.StabilitySupport)
					{
						num4 = 1;
					}
					byte stability2 = _chunk.GetStability(_blockX, num3, _blockZ);
					if (num4 > stability2)
					{
						_chunk.SetStability(_blockX, num3, _blockZ, (byte)num4);
						if (block2.StabilitySupport)
						{
							spreadHorizontal(_chunk, _blockX, num3, _blockZ, num4);
						}
						num2--;
						num3--;
						continue;
					}
					break;
				}
				break;
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void spreadHorizontal(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stab)
	{
		if (_stab <= 1)
		{
			return;
		}
		_stab--;
		for (int num = Vector3i.HORIZONTAL_DIRECTIONS.Length - 1; num >= 0; num--)
		{
			int num2 = _blockX + Vector3i.HORIZONTAL_DIRECTIONS[num].x;
			int num3 = _blockZ + Vector3i.HORIZONTAL_DIRECTIONS[num].z;
			Chunk chunk = _chunk;
			if ((uint)num2 >= 16u || (uint)num3 >= 16u)
			{
				chunk = (Chunk)world.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num2), _y, _chunk.GetBlockWorldPosZ(num3));
				if (chunk == null)
				{
					continue;
				}
				num2 = World.toBlockXZ(num2);
				num3 = World.toBlockXZ(num3);
			}
			int blockId = chunk.GetBlockId(num2, _y, num3);
			if (blockId == 0)
			{
				continue;
			}
			Block block = Block.list[blockId];
			if (block.blockMaterial.IsLiquid || block.StabilityIgnore)
			{
				continue;
			}
			int num4 = _stab;
			if (num4 > 1 && !block.StabilitySupport)
			{
				num4 = 1;
			}
			byte stability = chunk.GetStability(num2, _y, num3);
			if (num4 > stability)
			{
				chunk.SetStability(num2, _y, num3, (byte)num4);
				if (block.StabilitySupport)
				{
					spreadHorizontal(chunk, num2, _y, num3, num4);
					spreadVertical(chunk, num2, _y, num3, num4);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearDown(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stabStop)
	{
		int num = _y - 1;
		while (num > 0)
		{
			BlockValue blockNoDamage = _chunk.GetBlockNoDamage(_blockX, num, _blockZ);
			Block block = blockNoDamage.Block;
			if (!blockNoDamage.isair && block.StabilitySupport && !block.blockMaterial.IsLiquid && !block.StabilityIgnore)
			{
				byte stability = _chunk.GetStability(_blockX, num, _blockZ);
				if (stability != 0 && stability < _stabStop)
				{
					_chunk.SetStability(_blockX, num, _blockZ, 0);
					clearHorizontal(_chunk, _blockX, num, _blockZ, _stabStop);
					num--;
					continue;
				}
				break;
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearHorizontal(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stabStop)
	{
		for (int i = 0; i < Vector3i.HORIZONTAL_DIRECTIONS.Length; i++)
		{
			int num = _blockX + Vector3i.HORIZONTAL_DIRECTIONS[i].x;
			int num2 = _blockZ + Vector3i.HORIZONTAL_DIRECTIONS[i].z;
			Chunk chunk = _chunk;
			if (num < 0 || num >= 16 || num2 < 0 || num2 >= 16)
			{
				chunk = (Chunk)world.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num), _y, _chunk.GetBlockWorldPosZ(num2));
				num = World.toBlockXZ(num);
				num2 = World.toBlockXZ(num2);
			}
			if (chunk == null)
			{
				continue;
			}
			BlockValue blockNoDamage = chunk.GetBlockNoDamage(num, _y, num2);
			Block block = blockNoDamage.Block;
			if (!blockNoDamage.isair && block.StabilitySupport && !block.blockMaterial.IsLiquid && !block.StabilityIgnore)
			{
				byte stability = chunk.GetStability(num, _y, num2);
				if (stability != 0 && stability < _stabStop)
				{
					chunk.SetStability(num, _y, num2, 0);
					clearHorizontal(chunk, num, _y, num2, _stabStop);
					clearDown(chunk, num, _y, num2, _stabStop);
				}
			}
		}
	}
}

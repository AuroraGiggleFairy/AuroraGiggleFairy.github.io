using UnityEngine.Scripting;

[Preserve]
public class BlockLiquidSource : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue waterBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i[,] fallDirsSet = new Vector3i[8, 4]
	{
		{
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 0, -1),
			new Vector3i(1, 0, 0),
			new Vector3i(0, 0, 1)
		},
		{
			new Vector3i(1, 0, 0),
			new Vector3i(0, 0, -1),
			new Vector3i(0, 0, 1),
			new Vector3i(-1, 0, 0)
		},
		{
			new Vector3i(0, 0, 1),
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 0, -1),
			new Vector3i(1, 0, 0)
		},
		{
			new Vector3i(0, 0, -1),
			new Vector3i(0, 0, 1),
			new Vector3i(1, 0, 0),
			new Vector3i(-1, 0, 0)
		},
		{
			new Vector3i(-1, 0, 0),
			new Vector3i(1, 0, 0),
			new Vector3i(0, 0, -1),
			new Vector3i(0, 0, 1)
		},
		{
			new Vector3i(1, 0, 0),
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 0, 1),
			new Vector3i(0, 0, -1)
		},
		{
			new Vector3i(0, 0, 1),
			new Vector3i(-1, 0, 0),
			new Vector3i(1, 0, 0),
			new Vector3i(0, 0, -1)
		},
		{
			new Vector3i(1, 0, 0),
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 0, 1),
			new Vector3i(0, 0, -1)
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static int fallSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i emitionPos;

	public BlockLiquidSource()
	{
		IsRandomlyTick = false;
	}

	public override void LateInit()
	{
		base.LateInit();
		ItemValue item = ItemClass.GetItem("water");
		if (item != null)
		{
			waterBlock = item.ToBlockValue();
		}
		else
		{
			waterBlock = BlockValue.Air;
		}
	}

	public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
	{
		base.OnNeighborBlockChange(world, _clrIdx, _myBlockPos, _myBlockValue, _blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);
		if (_newNeighborBlockValue.isair)
		{
			_myBlockValue.meta = (byte)(1 - _myBlockValue.meta);
			world.SetBlockRPC(_clrIdx, _myBlockPos, _myBlockValue);
			world.GetWBT().AddScheduledBlockUpdate(_clrIdx, _myBlockPos, blockID, 1uL);
		}
	}

	public override bool IsMovementBlocked(IBlockAccess world, Vector3i _blockPos, BlockValue _blockValue, BlockFace crossingFace)
	{
		return false;
	}

	public override ulong GetTickRate()
	{
		return 20uL;
	}

	public override bool UpdateTick(WorldBase world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		BlockValue blockValue;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (i != 0 || j != 0)
				{
					emitionPos = _blockPos;
					emitionPos.x += j;
					emitionPos.z += i;
					BlockValue block = world.GetBlock(_clrIdx, emitionPos.x, emitionPos.y, emitionPos.z);
					if (block.isair || block.Block.blockMaterial.IsPlant)
					{
						blockValue = new BlockValue((uint)waterBlock.type);
						blockValue.meta = 14;
						blockValue.meta2 = 8;
						world.SetBlockRPC(emitionPos, blockValue);
						world.GetWBT().AddScheduledBlockUpdate(_clrIdx, emitionPos, BlockValue.Air.type, 1uL);
					}
				}
			}
		}
		blockValue = new BlockValue((uint)waterBlock.type);
		blockValue.meta = 14;
		blockValue.meta2 = 0;
		world.SetBlockRPC(_blockPos, blockValue);
		world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, 1uL);
		return true;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!world.IsRemote())
		{
			_blockValue.damage = Count;
			world.SetBlockRPC(_blockPos, _blockValue);
			world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, 1uL);
		}
	}
}

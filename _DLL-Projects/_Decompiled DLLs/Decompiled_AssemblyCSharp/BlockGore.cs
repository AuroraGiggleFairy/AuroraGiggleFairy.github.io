using UnityEngine.Scripting;

[Preserve]
public class BlockGore : BlockLoot
{
	public const int cDefaultTimeMinutes = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	public int timeInMinutes;

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("GoreTime"))
		{
			int.TryParse(base.Properties.Values["GoreTime"], out timeInMinutes);
		}
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void addTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		TileEntityGoreBlock tileEntityGoreBlock = new TileEntityGoreBlock(_chunk);
		tileEntityGoreBlock.localChunkPos = World.toBlock(_blockPos);
		tileEntityGoreBlock.lootListName = lootList;
		tileEntityGoreBlock.SetContainerSize(LootContainer.GetLootContainer(lootList).size);
		if (timeInMinutes > 0)
		{
			tileEntityGoreBlock.tickTimeToRemove = GameTimer.Instance.ticks + (ulong)(1200 * timeInMinutes);
		}
		_chunk.AddTileEntity(tileEntityGoreBlock);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void removeTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		_chunk.RemoveTileEntityAt<TileEntityGoreBlock>((World)world, World.toBlock(_blockPos));
	}
}

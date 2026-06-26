using UnityEngine.Scripting;

[Preserve]
public class BlockBackpack : BlockLoot
{
	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
		if (primaryPlayer != null && primaryPlayer.EqualsDroppedBackpackPositions(_blockPos))
		{
			primaryPlayer.SetDroppedBackpackPositions(primaryPlayer.persistentPlayerData.GetDroppedBackpackPositions());
		}
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockDef, BlockFaceFlag _sides)
	{
		return true;
	}
}

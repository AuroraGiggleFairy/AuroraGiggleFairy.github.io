using Audio;
using UnityEngine.Scripting;

[Preserve]
public class BlockSolarPanel : BlockPowerSource
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string runningSound = "solarpanel_idle";

	public override TileEntityPowerSource CreateTileEntity(Chunk chunk)
	{
		if (slotItem == null)
		{
			slotItem = ItemClass.GetItemClass(SlotItemName);
		}
		return new TileEntityPowerSource(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.SolarPanel,
			SlotItem = slotItem
		};
	}

	public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (!base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck))
		{
			return false;
		}
		Vector3i blockPos = _blockPos + Vector3i.up;
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster != null && chunkCluster.GetLight(blockPos, Chunk.LIGHT_TYPE.SUN) < 15)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetPowerSourceIcon()
	{
		return "electric_solar";
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		Manager.BroadcastStop(_blockPos.ToVector3(), runningSound);
	}

	public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
		Manager.Stop(_blockPos.ToVector3(), runningSound);
	}
}

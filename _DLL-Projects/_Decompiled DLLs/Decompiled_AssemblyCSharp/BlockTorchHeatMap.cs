using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockTorchHeatMap : BlockTorch
{
	public BlockTorchHeatMap()
	{
		IsRandomlyTick = true;
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
		if (HeatMapStrength > 0f)
		{
			AIDirector aIDirector = _world.GetAIDirector();
			if (aIDirector != null)
			{
				float num = 1f;
				num *= 0.4f;
				aIDirector.NotifyActivity(EnumAIDirectorChunkEvent.Torch, _blockPos, HeatMapStrength * num);
			}
		}
		return true;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null)
		{
			return;
		}
		IChunk chunkSync = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
		if (chunkSync == null)
		{
			return;
		}
		BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return;
		}
		Transform transform = blockEntity.transform.FindInChildren("MainLight");
		if ((bool)transform)
		{
			LightLOD component = transform.GetComponent<LightLOD>();
			if ((bool)component)
			{
				component.SetBlockEntityData(blockEntity);
			}
		}
	}
}

using Audio;
using UnityEngine.Scripting;

[Preserve]
public class BlockBladeTrap : BlockPoweredTrap
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string runningSound = "Electricity/BladeTrap/bladetrap_fire_lp";

	[PublicizedFrom(EAccessModifier.Private)]
	public string runningSoundPartlyBroken = "Electricity/BladeTrap/bladetrap_dm1_lp";

	[PublicizedFrom(EAccessModifier.Private)]
	public string runningSoundBroken = "Electricity/BladeTrap/bladetrap_dm2_lp";

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("RunningSound"))
		{
			runningSound = base.Properties.Values["RunningSound"];
		}
		if (base.Properties.Values.ContainsKey("RunningSoundBreaking"))
		{
			runningSoundPartlyBroken = base.Properties.Values["RunningSoundBreaking"];
		}
		if (base.Properties.Values.ContainsKey("RunningSoundBroken"))
		{
			runningSoundBroken = base.Properties.Values["RunningSoundBroken"];
		}
	}

	public override bool ActivateTrap(BlockEntityData blockEntity, bool isOn)
	{
		SpinningBladeTrapController component = blockEntity.transform.gameObject.GetComponent<SpinningBladeTrapController>();
		if (component == null)
		{
			return false;
		}
		component.Init(base.Properties, this);
		component.BlockPosition = blockEntity.pos;
		component.HealthRatio = 1f - (float)blockEntity.blockValue.damage / (float)blockEntity.blockValue.Block.MaxDamage;
		component.IsOn = isOn;
		return true;
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			IChunk chunkSync = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
			if (chunkSync != null)
			{
				BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
				if (blockEntity != null && blockEntity.bHasTransform)
				{
					SpinningBladeTrapController component = blockEntity.transform.gameObject.GetComponent<SpinningBladeTrapController>();
					if (component != null)
					{
						component.HealthRatio = 1f - (float)blockEntity.blockValue.damage / (float)blockEntity.blockValue.Block.MaxDamage;
					}
				}
			}
		}
		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		Manager.BroadcastStop(_blockPos.ToVector3(), runningSound);
		Manager.BroadcastStop(_blockPos.ToVector3(), runningSoundPartlyBroken);
		Manager.BroadcastStop(_blockPos.ToVector3(), runningSoundBroken);
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
	}

	public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		Manager.Stop(_blockPos.ToVector3(), runningSound);
		Manager.Stop(_blockPos.ToVector3(), runningSoundPartlyBroken);
		Manager.Stop(_blockPos.ToVector3(), runningSoundBroken);
		base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
	}
}

using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockTNT : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ExplosionData explosion;

	public override void Init()
	{
		base.Init();
		explosion = new ExplosionData(base.Properties);
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityId, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (_world.GetGameRandom().RandomFloat <= (float)_damagePoints / (float)_blockValue.Block.MaxDamage)
		{
			explode(_world, _clrIdx, _blockPos, _entityId, 0.1f);
		}
		_world.ChunkClusters[_clrIdx]?.InvokeOnBlockDamagedDelegates(_blockPos, _blockValue, _damagePoints, _entityId);
		return _blockValue.damage;
	}

	public override DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, int _clrIdx, Vector3i _pos, BlockValue _blockValue, int _playerIdx)
	{
		base.OnBlockDestroyedByExplosion(_world, _clrIdx, _pos, _blockValue, _playerIdx);
		explode(_world, _clrIdx, _pos, _playerIdx, _world.GetGameRandom().RandomFloat * 0.5f + 0.3f);
		return DestroyedResult.Remove;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void explode(WorldBase _world, int _clrIdx, Vector3i _expBlockPos, int _entityId, float _delay)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		Vector3 worldPos = _expBlockPos.ToVector3();
		if (chunkCluster != null)
		{
			worldPos = chunkCluster.ToWorldPosition(_expBlockPos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f));
		}
		_world.GetGameManager().ExplosionServer(_clrIdx, worldPos, _expBlockPos, Quaternion.identity, explosion, _entityId, _delay, _bRemoveBlockAtExplPosition: true);
	}
}

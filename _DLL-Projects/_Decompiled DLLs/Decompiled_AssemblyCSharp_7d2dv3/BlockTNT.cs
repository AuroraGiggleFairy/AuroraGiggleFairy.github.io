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

	public override int OnBlockDamaged(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _damagePoints, int _entityId, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (_world.GetGameRandom().RandomFloat <= (float)_damagePoints / (float)_blockValue.Block.MaxDamage)
		{
			explode(_world, _bvRef, _entityId, 0.1f);
		}
		_world.ChunkCache?.InvokeOnBlockDamagedDelegates(_bvRef, _blockValue, _damagePoints, _entityId);
		return _blockValue.damage;
	}

	public override DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, BlockValueRef _pos, BlockValue _blockValue, int _playerIdx)
	{
		base.OnBlockDestroyedByExplosion(_world, _pos, _blockValue, _playerIdx);
		explode(_world, _pos, _playerIdx, _world.GetGameRandom().RandomFloat * 0.5f + 0.3f);
		return DestroyedResult.Remove;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void explode(WorldBase _world, BlockValueRef _bvRef, int _entityId, float _delay)
	{
		Vector3 worldPos = _bvRef.ToVector3Center(_world);
		_world.GetGameManager().ExplosionServer(worldPos, _bvRef, Quaternion.identity, explosion, _entityId, _delay, _bRemoveBlockAtExplPosition: true);
	}
}
